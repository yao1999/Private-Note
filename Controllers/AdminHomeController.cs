using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmailService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Private_Note.Areas.Identity.Data;
using Private_Note.Common;
using Private_Note.EncryptAndDecrypt;

namespace Private_Note.Controllers
{
    //[Authorize("PolicyName")]
    //[IsAdminAuth]
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
        public IActionResult Index()
        {
            IEnumerable<ApplicationUser> Users = _userManager.Users;
            foreach (var user in Users)
            {
                var SecretPassword = Methods.Decrypt(user.SecretPassword);
                user.SecretPassword = SecretPassword;
            }
            return View(Users);
        }

        public async Task<IActionResult> DeleteUser([FromForm] string userName)
        {
            var currentUser = await _userManager.FindByNameAsync(userName);
            if (currentUser == null || currentUser.IsAdmin == true)
            {
                return RedirectToAction("Index", "AdminHome");
            }
            await _userManager.DeleteAsync(currentUser);
            return RedirectToAction("Index", "AdminHome");
        }

        public async Task<IActionResult> ChangeSecretPassword([FromForm] string userName, [FromForm] string secretPassword)
        {
            var currentUser = await _userManager.FindByNameAsync(userName);
            if (currentUser == null || currentUser.IsAdmin == true)
            {
                return RedirectToAction("Index", "AdminHome");
            }
            currentUser.SecretPassword = Methods.Encrypt(secretPassword);
            await _userManager.UpdateAsync(currentUser);
            return RedirectToAction("Index", "AdminHome");
        }


        public async Task<IActionResult> ContactUser(
            [FromForm] string userName, 
            [FromForm] string subject, 
            [FromForm] string content)
        {
            var currentUser = await _userManager.FindByNameAsync(userName);
            var UserEmails = new string[] { currentUser.Email };
            var message = new Message(UserEmails, subject, content);
            _emailSender.SendEmail(message);
            return RedirectToAction("Index", "AdminHome");
        }

        public async Task SendEmailToUserAsync(
            [FromForm] string userName, 
            [FromForm] string subject, 
            [FromForm] string content,
            [FromForm] string userType)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (TypeCheck(user, userType) == true)
            {
                var UserEmails = new string[] { user.Email };
                var message = new Message(UserEmails, subject, content);
                _emailSender.SendEmail(message);
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
