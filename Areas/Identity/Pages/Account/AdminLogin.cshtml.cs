using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Private_Note.Areas.Identity.Data;
using Private_Note.EncryptAndDecrypt;
using EmailService;

namespace Private_Note.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class AdminLoginModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly EmailService.IEmailSender _emailSender;

        public AdminLoginModel(SignInManager<ApplicationUser> signInManager, 
            ILogger<LoginModel> logger,
            UserManager<ApplicationUser> userManager,
            EmailService.IEmailSender EmailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = EmailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

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

            [Required]
            [StringLength(16, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Secret Password")]
            public string SecretPassword { get; set; }
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if(User.Identity.IsAuthenticated)
            {
                Response.Redirect("/AdminHome");
            }
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl = returnUrl ?? Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(Input.UserName);
                Input.SecretPassword = Methods.Encrypt(Input.SecretPassword);

                if (user.SecretPassword.CompareTo(Input.SecretPassword) != 0 ||
                    user.Email.CompareTo(Input.Email) != 0)
                {
                    ModelState.AddModelError(string.Empty, "Incorrect information.");
                    await _userManager.AccessFailedAsync(user);
                    return Page();
                }

                var result = await _signInManager.PasswordSignInAsync(
                    user,
                    Input.Password,
                    isPersistent: false,
                    lockoutOnFailure: false
                    );

                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    await _userManager.ResetAccessFailedCountAsync(user);
                    return RedirectToAction("Index", "AdminHome");
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut && user.LockoutEnd != null)
                {
                    _logger.LogWarning("User account locked out.");
                    SendEmailToUser(user, "Account LockedOut");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    await _userManager.AccessFailedAsync(user);
                    return Page();
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
            string content = "Hi " + user.UserName + ", Someone trying to access your account at " + DateTime.Now + ", " +
                "the account is locked right now, the end is " + user.LockoutEnd + ". " +
                "Please contact one of our admins: " + allAdmins + ".";
            var message = new Message(UserEmails, subject, content);
            _emailSender.SendEmail(message);
        }
    }
}
