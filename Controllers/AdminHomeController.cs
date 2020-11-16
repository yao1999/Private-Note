using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public AdminHomeController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
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


    }
}
