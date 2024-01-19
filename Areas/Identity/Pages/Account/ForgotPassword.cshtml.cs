using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using SMSS.Models;
using Microsoft.Extensions.Logging;
using SMSS.Data;
using SMSS.Controllers;
using Microsoft.AspNetCore.Hosting;
using System.Linq;

namespace SMSS.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<RegisteredUser> _userManager;
        private readonly IEmailSender _emailSender;


        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        private readonly UserManager<RegisteredUser> _manager;
        private readonly SignInManager<RegisteredUser> _signInManager;

        private readonly IWebHostEnvironment _iweb;


        public ForgotPasswordModel(UserManager<RegisteredUser> userManager, IEmailSender emailSender, ILogger<HomeController> logger, ApplicationDbContext dbContext, UserManager<RegisteredUser> manager,
                                          SignInManager<RegisteredUser> signInManager, IWebHostEnvironment iweb)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;
            _context = dbContext;
            _manager = manager;
            _signInManager = signInManager;
            _iweb = iweb;

        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please 
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);



                    RegisteredUser profile = _context.RegisteredUsers.FirstOrDefault(ap => ap.Email == Input.Email);
                   

                await _emailSender.SendEmailAsync(
                   Input.Email,
                    "Reset Password",
                    $"<h1>Welcome to S M Software Solutions!</h1><br/>"+
                    "Hello " + profile.FirstName + ",<br>" +
                   $"We sent this email because you were having some trouble getting into your SMSS account.  <br/>" +
                   $"Here's your username: " + Input.Email + "<br/>"+
                   $"If you forgot your password, just click the link below.<br/>"+
                   $"<a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>Change your password now</a>."+
                   $"<br><br> Thanks, <br> SMSS Support ");

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
    }
}
