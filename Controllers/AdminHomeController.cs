using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Private_Note.Areas.Identity.Data;
using Private_Note.EncryptAndDecrypt;

namespace Private_Note.Controllers
{
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


    }
}
