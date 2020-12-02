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
            var currentAdmin = await _userManager.FindByNameAsync(User.Identity.Name);
            if(currentAdmin.IsAdmin==false)
            {
                return RedirectToAction("Index", "UserHome");
            }
            IEnumerable<ApplicationUser> Users = _userManager.Users.Where(u => u.IsAdmin == false);
            foreach (var user in Users)
            {
                var SecretPassword = Methods.Decrypt(user.SecretPassword);
                user.SecretPassword = SecretPassword;
            }
            return View(Users);
        }

        [HttpPost]
        public async Task<JsonResult> GetSecretPassword([FromForm] string userName)
        {
            try
            {
                var currentUser = await _userManager.FindByNameAsync(userName);
                if (currentUser == null || currentUser.IsAdmin == true)
                {
                    JsonResult error = new JsonResult("User not found Or User is Admin") { StatusCode = (int)(HttpStatusCode.NotFound) };
                    return error;
                }
                var secretPassword = Methods.Decrypt(currentUser.SecretPassword);
                JsonResult success = new JsonResult(secretPassword);
                return success;
            }
            catch (Exception e)
            {
                JsonResult failed = new JsonResult(e.Message) { StatusCode = (int)(HttpStatusCode.NotFound) };
                return failed;
            }
        }

        [HttpPost]
        public async Task<JsonResult> DeleteUser([FromForm] string UserName)
        {
            try
            {
                var currentUser = await _userManager.FindByNameAsync(UserName);
                var currentAdmin = await _userManager.FindByNameAsync(User.Identity.Name);
                if (currentUser == null || currentUser.IsAdmin == true)
                {
                    JsonResult error = new JsonResult("User not found Or User is Admin") { StatusCode = (int)(HttpStatusCode.NotFound) };
                    return error;
                }
                await _userManager.DeleteAsync(currentUser);
                UserDeletedEmail(currentUser, currentAdmin, "Account Deleted By Admin");
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
        public async Task<JsonResult> ChangeSecretPassword([FromForm] string Username, [FromForm] string SecretPassword)
        {
            try
            {
                var currentAdmin = await _userManager.FindByNameAsync(User.Identity.Name);
                var currentUser = await _userManager.FindByNameAsync(Username);
                if (currentUser == null)
                {
                    JsonResult error = new JsonResult("User not found") { StatusCode = (int)(HttpStatusCode.NotFound) };
                    return error;
                }
                var oldSecretPassword = Methods.Decrypt(currentUser.SecretPassword);
                var newSecretPassword = SecretPassword;
                currentUser.SecretPassword = Methods.Encrypt(SecretPassword);
                await _userManager.UpdateAsync(currentUser);

                SecretPasswordChangedEmail(currentUser, currentAdmin, "Secret Password Changed", oldSecretPassword, newSecretPassword);

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
            [FromForm] string username, 
            [FromForm] string subject, 
            [FromForm] string content,
            [FromForm] string userType)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(username);
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

        private void SecretPasswordChangedEmail(
            ApplicationUser user,
            ApplicationUser currentAdmin,
            string subject,
            string oldSecretPassword,
            string newSecretPassword)
        {
            IEnumerable<ApplicationUser> adminTeam = _userManager.Users.Where(u => u.IsAdmin == true);
            var admins = new List<string>();

            foreach (var admin in adminTeam)
            {
                admins.Add(admin.Email);
            }
            string allAdmins = string.Join(", ", admins);
            string content = "Hi " + user.UserName + ", admin " + currentAdmin.UserName + " changed your Secret Password at " + DateTime.Now + ", " +
                "your old password was " + oldSecretPassword + ", now the new one is " + newSecretPassword + "." +
                "If you have any question, please contact one of our admins: " + allAdmins + ".";

            SendEmailToUser(user, subject, content);
        }

        private void UserDeletedEmail(ApplicationUser user, ApplicationUser currentAdmin, string subject)
        {
            IEnumerable<ApplicationUser> adminTeam = _userManager.Users.Where(u => u.IsAdmin == true);
            var admins = new List<string>();

            foreach (var admin in adminTeam)
            {
                admins.Add(admin.Email);
            }
            string allAdmins = string.Join(", ", admins);
            string content = "Hi " + user.UserName + ", admin " + currentAdmin.UserName + " delete your account at " + DateTime.Now + ", " +
                "If you have any question, please contact one of our admins: " + allAdmins + ".";

            SendEmailToUser(user, subject, content);
        }
        private void SendEmailToUser(ApplicationUser user, string subject, string content)
        {
            var UserEmails = new string[] { user.Email };
            var message = new Message(UserEmails, subject, content);
            _emailSender.SendEmail(message);
        }
    }
}
