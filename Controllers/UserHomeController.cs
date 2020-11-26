using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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

        public UserHomeController(PrivateNoteDBContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            IEnumerable<Files> AllFiles = _context.Files.Where(u => u.UserName == User.Identity.Name);
            return View(AllFiles);
        }

        [HttpPost]
        public JsonResult FileUpload([FromForm] IFormFile file)
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

                            using (var target = new MemoryStream())
                            {
                                file.CopyTo(target);
                                objfiles.File = target.ToArray(); //Getting file data
                            }

                            _context.Files.Add(objfiles);
                            _context.SaveChanges();
                        }
                    }
                    else
                    {
                        JsonResult error = new JsonResult("File Type not match") {StatusCode = (int)(HttpStatusCode.NotFound) };
                        return error;
                    }

                    //return RedirectToAction("Index", "UserHome");
                    JsonResult success = new JsonResult("File Successfully Removed");
                    return success;
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

        public IActionResult FileDownload([FromForm] string FileName, [FromForm] string FileExtension)
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
                    JsonResult error = new JsonResult("File Not Found"){ StatusCode = (int)(HttpStatusCode.NotFound) };
                    return error;
                }
            }
            catch (Exception e)
            {
                JsonResult failed = new JsonResult(e.Message) { StatusCode = (int)(HttpStatusCode.NotFound) };
                return failed;
            }
        }

        [HttpPost]
        public JsonResult FileDelete([FromForm] string FileName, [FromForm] string FileExtension)
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
                    JsonResult error = new JsonResult("User not found") { StatusCode = (int)(HttpStatusCode.NotFound) };
                    return error;
                }
                currentUser.SecretPassword = Methods.Encrypt(secretPassword);
                await _userManager.UpdateAsync(currentUser);

                JsonResult success = new JsonResult("Secret Password Successfully Changed");
                return success;
            }
            catch(Exception e)
            {
                JsonResult failed = new JsonResult(e.Message) { StatusCode = (int)(HttpStatusCode.NotFound) };
                return failed;
            }
            
        }
    }
}
