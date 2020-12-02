using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EmailService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Private_Note.Areas.Identity.Data;
using Private_Note.Data;
using Private_Note.EncryptAndDecrypt;
using Private_Note.Models;

namespace Private_Note.Controllers
{
    [Authorize]
    public class UserHomeController : Controller
    {
        private readonly PrivateNoteDBContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public UserHomeController(
            PrivateNoteDBContext context, 
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
        }
        public IActionResult Index()
        {
            IEnumerable<Files> AllFiles = _context.Files.Where(u => u.UserName == User.Identity.Name);
            return View(AllFiles);
        }

        [HttpPost]
        public IActionResult FileUpload([FromForm] IFormFile file)
        {
            try
            {
                if (file != null)
                {
                    var FileExtension = Path.GetExtension(Path.GetFileName(file.FileName));
                    if (AllFileType().ContainsKey(FileExtension))
                    {
                        if (file.Length > 0)
                        {
                            var objfiles = new Files()
                            {
                                FileName = Path.GetFileNameWithoutExtension(file.FileName), //Getting FileName 
                                FileType = Path.GetExtension(Path.GetFileName(file.FileName)), //Getting Extension
                                CreatedDate = DateTime.Now, //Getting current time
                                UserName = User.Identity.Name //get the current user name
                            };

                            var currentFile = _context.Files.SingleOrDefault(r => 
                                                           r.FileName == objfiles.FileName && 
                                                           r.FileType == objfiles.FileType);
                            if(currentFile != null)
                            {
                                JsonResult error = new JsonResult("Found File in Database with same Name and same Type") { StatusCode = (int)(HttpStatusCode.NotFound) };
                                return error;
                            }

                            using (var target = new MemoryStream())
                            {
                                file.CopyTo(target);
                                objfiles.File = target.ToArray(); //Getting file data
                            }

                            _context.Files.Add(objfiles);
                            _context.SaveChanges();
                            JsonResult success = new JsonResult("File Successfully Uploaded");
                            return success;
                        }
                    }
                    else
                    {
                        JsonResult error = new JsonResult("File Type not match") {StatusCode = (int)(HttpStatusCode.NotFound) };
                        return error;
                    }

                    return RedirectToAction("Index", "UserHome");
                    //JsonResult success = new JsonResult("File Successfully Uploaded");
                    //return success;
                }
                else
                {
                    JsonResult error = new JsonResult("No File Uploaded") {StatusCode = (int)(HttpStatusCode.NotFound) };
                    return error;
                }

            }
            catch (Exception e)
            {
                JsonResult failed = new JsonResult(e.Message) { StatusCode = (int)(HttpStatusCode.NotFound) };
                return failed;
            }
        }

        public JsonResult FileDownloadCheck([FromForm] string FileName, [FromForm] string FileExtension)
        {
            try
            {
                if (FileExtension.Contains('.') == false)
                {
                    FileExtension = FileExtension.Insert(0, ".");
                }
                var currentFile = _context.Files.SingleOrDefault(r =>
                                                r.FileName == FileName &&
                                                r.FileType == FileExtension &&
                                                r.UserName == User.Identity.Name);
                if (currentFile != null)
                {
                    JsonResult success = new JsonResult("File in DataBase");
                    return success;
                }
                else
                {
                    JsonResult error = new JsonResult("No such File") { StatusCode = (int)(HttpStatusCode.NotFound) };
                    return error;
                }
            }
            catch (Exception e)
            {
                JsonResult error = new JsonResult(e.Message) { StatusCode = (int)(HttpStatusCode.NotFound) };
                return error;
            }
        }

        [HttpGet]
        public IActionResult FileDownload(string FileName, string FileExtension)
        {
            try
            {
                if (FileExtension.Contains('.') == false)
                {
                    FileExtension = FileExtension.Insert(0, ".");
                }
                var currentFile = _context.Files.SingleOrDefault(r =>
                                                r.FileName == FileName &&
                                                r.FileType == FileExtension &&
                                                r.UserName == User.Identity.Name);
                if (currentFile != null)
                {
                    var memory = new MemoryStream(currentFile.File);
                    memory.Position = 0;
                    var fileNameAndExtension = string.Concat(currentFile.FileName, currentFile.FileType);
                    return File(memory, AllFileType()[currentFile.FileType], fileNameAndExtension);
                }
                else
                {
                    return RedirectToAction("Index", "UserHome");
                }
            }
            catch (Exception e)
            {
                return RedirectToAction("Index", "UserHome");
            }
        }

        [HttpPost]
        public JsonResult FileDelete([FromForm] string fileName, [FromForm] string fileExtension)
        {
            try
            {
                if (fileExtension.Contains('.') == false)
                {
                    fileExtension = fileExtension.Insert(0, ".");
                }
                var currentFile = _context.Files.SingleOrDefault(r =>
                                                r.FileName == fileName &&
                                                r.FileType == fileExtension &&
                                                r.UserName == User.Identity.Name);
                if(currentFile == null)
                {
                    JsonResult error = new JsonResult("File Not Found") { StatusCode = (int)(HttpStatusCode.NotFound) };
                    return error;
                }
                _context.Files.Remove(currentFile);
                _context.SaveChanges();
                JsonResult success = new JsonResult("File Successfully Removed");
                return success;
            }
            catch (Exception e)
            {
                JsonResult failed = new JsonResult(e.Message) { StatusCode = (int)(HttpStatusCode.NotFound) };
                return failed;
            }
        }

        private Dictionary<string, string> AllFileType()
        {
            var fileTypes = new Dictionary<string, string>
            {
                { ".txt", "text/plain" },
                { ".pdf", "application/pdf" },
                { ".docx", "application/vnd.ms-word" },
                { ".doc", "application/vnd.ms-word" },
                { ".png", "image/png" },
                { ".jpg", "image/jpg" },
                { ".jpeg", "image/jpeg" },
                { ".pptx", "application/vnd.ms-powerpoint" }
            };
            return fileTypes;
        }

        [HttpPost]
        public async Task<JsonResult> ChangeSecretPassword([FromForm] string secretPassword)
        {
            try
            {
                var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
                if (currentUser == null)
                {
                    //JsonResult error = new JsonResult("User not found") { StatusCode = (int)(HttpStatusCode.NotFound) };
                    //return error;
                    JsonResult error = new JsonResult("User not found") { StatusCode = (int)(HttpStatusCode.NotFound) };
                    return error;
                }
                var oldSecretPassword = Methods.Decrypt(currentUser.SecretPassword);
                var newSecretPassword = secretPassword;
                currentUser.SecretPassword = Methods.Encrypt(secretPassword);
                await _userManager.UpdateAsync(currentUser);

                SendEmailToUser(currentUser, "Secret Password Changed", oldSecretPassword, newSecretPassword);

                JsonResult success = new JsonResult("Secret Password Successfully Changed");
                return success;                
            }
            catch(Exception e)
            {
                JsonResult failed = new JsonResult(e.Message) { StatusCode = (int)(HttpStatusCode.NotFound) };
                return failed;
            }
            
        }

        private void SendEmailToUser(ApplicationUser user, string subject, string oldSecretPassword, string newSecretPassword)
        {
            var UserEmails = new string[] { user.Email };
            IEnumerable<ApplicationUser> adminTeam = _userManager.Users.Where(u => u.IsAdmin == true);
            var admins = new List<string>();

            foreach (var admin in adminTeam)
            {
                admins.Add(admin.Email);
            }
            string allAdmins = string.Join(", ", admins);
            string content = "Hi " + user.UserName + ", Someone changed your Secret Password at " + DateTime.Now + ", " +
                "your old password was "+ oldSecretPassword + ", now the new one is " + newSecretPassword +"."+
                "If you have any question, please contact one of our admins: " + allAdmins + ".";
            var message = new Message(UserEmails, subject, content);
            _emailSender.SendEmail(message);
        }
    }
}
