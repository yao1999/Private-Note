using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EmailService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Private_Note.Areas.Identity.Data;
using Private_Note.EncryptAndDecrypt;

namespace Private_Note.Controllers
{
    [Authorize]
    public class AdminHomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public AdminHomeController(
            UserManager<ApplicationUser> userManager, 
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            if(currentUser.IsAdmin==false)
            {
                return RedirectToAction("Index", "UserHome");
            }
            IEnumerable<ApplicationUser> Users = _userManager.Users;
            foreach (var user in Users)
            {
                var SecretPassword = Methods.Decrypt(user.SecretPassword);
                user.SecretPassword = SecretPassword;
            }
            return View(Users);
        }

        [HttpPost]
        public async Task<JsonResult> DeleteUser([FromForm] string userName)
        {
            try
            {
                var currentUser = await _userManager.FindByNameAsync(userName);
                if (currentUser == null || currentUser.IsAdmin == true)
                {
                    JsonResult error = new JsonResult("User not found Or User is Admin") { StatusCode = (int)(HttpStatusCode.NotFound) };
                    return error;
                }
                await _userManager.DeleteAsync(currentUser);
                JsonResult success = new JsonResult("User Successfully Removed");
                return success;
            }
            catch (Exception e)
            {
                JsonResult failed = new JsonResult(e.Message) { StatusCode = (int)(HttpStatusCode.NotFound) };
                return failed;
            }
        }

        [HttpPost]
        public async Task<JsonResult> ChangeSecretPassword([FromForm] string userName, [FromForm] string secretPassword)
        {
            try
            {
                var currentUser = await _userManager.FindByNameAsync(userName);
                if (currentUser == null)
                {
                    JsonResult error = new JsonResult("User not found") { StatusCode = (int)(HttpStatusCode.NotFound) };
                    return error;
                }
                currentUser.SecretPassword = Methods.Encrypt(secretPassword);
                await _userManager.UpdateAsync(currentUser);

                JsonResult success = new JsonResult("User's Secret Password Successfully Changed");
                return success;
            }
            catch (Exception e)
            {
                JsonResult failed = new JsonResult(e.Message) { StatusCode = (int)(HttpStatusCode.NotFound) };
                return failed;
            }
        }

        [HttpPost]
        public async Task<JsonResult> ContactUser(
            [FromForm] string userName, 
            [FromForm] string subject, 
            [FromForm] string content,
            [FromForm] string userType)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(userName);
                if (TypeCheck(user, userType) == false)
                {
                    JsonResult error = new JsonResult("User Type Not Match") { StatusCode = (int)(HttpStatusCode.NotFound) };
                    return error;
                    
                }
                var UserEmails = new string[] { user.Email };
                var message = new Message(UserEmails, subject, content);
                _emailSender.SendEmail(message);
                JsonResult success = new JsonResult("Email Sent");
                return success;
            }
            catch (Exception e)
            {
                JsonResult failed = new JsonResult(e.Message) { StatusCode = (int)(HttpStatusCode.NotFound) };
                return failed;
            }
        }

        private bool TypeCheck(ApplicationUser user, string userType)
        {
            var type = userType.ToLower();
            if (type == "u" || type == "user")
            {
                return user.IsUser;
            }
            else if (type == "a" || type == "admin")
            {
                return user.IsAdmin;
            }

            return false;
        }
    }
}
