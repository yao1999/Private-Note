using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using EmailService;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Private_Note.Areas.Identity.Data;
using Private_Note.EncryptAndDecrypt;

namespace Private_Note.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class AdminRegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly EmailService.IEmailSender _emailSender;

        public AdminRegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            EmailService.IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "User Name")]
            public string UserName { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email Address")]
            public string Email { get; set; }

            [Required]
            [StringLength(30, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required]
            [StringLength(16, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Secret Password")]
            public string SecretPassword { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm Secret Password")]
            [Compare("SecretPassword", ErrorMessage = "The Secret password and confirmation Secret password do not match.")]
            public string ConfirmSecretPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            //if (User.Identity.IsAuthenticated)
            //{
            //    Response.Redirect("/AdminHome");
            //}
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            var checkUser = await _userManager.FindByNameAsync(Input.UserName);

            if (checkUser != null)
            {
                ModelState.AddModelError(string.Empty, "User name exist");
                return Page();
            }

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser {
                    UserName = Input.UserName,
                    Email = Input.Email,
                    Password = Input.Password,
                    SecretPassword = Methods.Encrypt(Input.SecretPassword),
                    IsUser = false,
                    IsAdmin = true,
                    AccessFailedCount = 0
                };
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Admin created a new account with password.");

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    SendEmailToUser(user, "Welcome to Private Note Admin Team");
                    return RedirectToAction("Index", "AdminHome");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private void SendEmailToUser(ApplicationUser user, string subject)
        {
            var UserEmails = new string[] { user.Email };
            IEnumerable<ApplicationUser> adminTeam = _userManager.Users.Where(u => u.IsAdmin == true);
            var admins = new List<string>();

            foreach (var admin in adminTeam)
            {
                admins.Add(admin.Email);
            }
            string allAdmins = string.Join(", ", admins);
            string content = "Hi " + user.UserName + ", thank for being part of our admin team." +
                " here is the rest of the team members: " + allAdmins + ".";
            var message = new Message(UserEmails, subject, content);
            _emailSender.SendEmail(message);
        }
    }
}
