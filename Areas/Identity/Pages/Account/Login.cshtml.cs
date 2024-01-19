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
using SMSS.Models;
using SMSS.Data;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.IO;

namespace SMSS.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly UserManager<RegisteredUser> _userManager;
        private readonly SignInManager<RegisteredUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly ApplicationDbContext _context;

        public LoginModel(SignInManager<RegisteredUser> signInManager, 
            ILogger<LoginModel> logger,
            UserManager<RegisteredUser> userManager ,
            ApplicationDbContext context )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _context = context;
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
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
           

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        
            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {





                    _logger.LogInformation("User logged in.");
                    int userRoll = _context.RegisteredUsers.FirstOrDefault(ru => ru.Email == Input.Email).UserFlag;
                    switch (userRoll)
                    {
                        case 1:
                            returnUrl ??= Url.Content("~/CompanyProfile/GetCompanyProfile");
                            break;
                        case 2:
                            returnUrl ??= Url.Content("~/ApplicantProfile/GetProfile");
                            break;
                        case 3:
                            returnUrl ??= Url.Content("~/CompanyProfile/GetCompanyProfile");
                            break;
                    }
                    if (result.RequiresTwoFactor)
                    {
                        return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                    }
                    if (result.IsLockedOut)
                    {
                        _logger.LogWarning("User account locked out.");
                        return RedirectToPage("./Lockout");
                    }
                    return LocalRedirect(returnUrl);
                }
                else
                {
                    //ModelState.AddModelError(string.Empty,"Invalid user Id or Password");
                    AlertMessage("Opps!!! Invalid/Incorrect Email or Password. Try Again Later.. ", NotificationType.error);
                    return Page();
                }
               
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
        public void Notify(string message, string title = "SMSS Message Alert", NotificationType notificationType = NotificationType.success)
        {
            var msg = new
            {
                message = message,
                title = title,
                icon = notificationType.ToString(),
                type = notificationType.ToString(),
                provider = GetProvider()
            };
            TempData["Message"] = JsonConvert.SerializeObject(msg);
        }

        public void AlertMessage(string message, NotificationType notificationType)
        {
            var msg = "swal.fire('" + notificationType.ToString() + "', '" + message + "', '" + notificationType + "')" + "";
            TempData["notification"] = msg;
        }

        private string GetProvider()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            IConfigurationRoot configuration = builder.Build();

            var value = configuration["NotificationProvider"];

            return value;
        }
    }
}
