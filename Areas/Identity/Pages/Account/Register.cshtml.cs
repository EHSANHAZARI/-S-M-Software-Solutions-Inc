using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using DNTCaptcha.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SMSS.Data;
using SMSS.Models;

namespace SMSS.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<RegisteredUser> _signInManager;
        private readonly UserManager<RegisteredUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;

        private readonly IDNTCaptchaValidatorService _validatorService; 
        private readonly DNTCaptchaOptions _captchaOptions;

        public RegisterModel(
            UserManager<RegisteredUser> userManager,
            SignInManager<RegisteredUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender ,
            ApplicationDbContext context,
            IDNTCaptchaValidatorService validatorService,
            IOptions<DNTCaptchaOptions> captchaOptions
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
            _validatorService = validatorService;
            _captchaOptions = captchaOptions == null ? throw new ArgumentException(nameof(captchaOptions)) : captchaOptions.Value;

        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public List<SelectListItem> Sectors { get; set; }

        public class InputModel
        {

            [Required]
            [RegularExpression(@"^[a-zA-Z\s]*$", ErrorMessage = "Use letters only")]
            [StringLength(50, MinimumLength = 3, ErrorMessage = "{0} Length must be between {2} and {1} character.")]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required]
            [StringLength(50, MinimumLength = 3, ErrorMessage = "{0} Length must be between {2} and {1} character.")]
            [RegularExpression(@"^[a-zA-Z\s]*$", ErrorMessage = "Use letters only")]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            [RegularExpression(".+@.+\\..+", ErrorMessage = "Please Enter Correct Email Address")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required]
            [StringLength(13, MinimumLength = 10)]
            [DataType(DataType.PhoneNumber, ErrorMessage = "Provided phone number not valid")]
            [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid phone number")]

            [Display(Name = "Phone Number")]
            public string UserPhone { get; set; }

           /// [Required]
            //[RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Use letters only")]
           // [StringLength(100, MinimumLength = 3, ErrorMessage = "{0} Length must be between {1} and {2} character.")]
            [Display(Name = "Organization Name")]
            public string OrganizationName { get; set; }

            public byte UserFlag { get; set; }

            [PersonalData]
           // [Required]
            public EnumResidencyStatus ResidencyStatus { get; set; }

           // [Required]
            public IEnumerable<int> Sectors { get; set; }

        

            [Range(typeof(bool), "true", "true", ErrorMessage = "Please Accept the Terms & Conditions!")]
            public bool TermsAccepted { get; set; }

        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            List<Sector> sectors = _context.Sectors.OrderBy(js => js.SectorName).ToList();

            Sectors = new List<SelectListItem>();
            foreach (var sector in sectors)
            {
                SelectListItem item = new SelectListItem() { Value = sector.Id.ToString(), Text = sector.SectorName };
                Sectors.Add(item);
            }
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {



            List<Sector> sectors = _context.Sectors.OrderBy(js => js.SectorName).ToList();

            Sectors = new List<SelectListItem>();
            foreach (var sector in sectors)
            {
                SelectListItem item = new SelectListItem() { Value = sector.Id.ToString(), Text = sector.SectorName };
                Sectors.Add(item);
            }
            

            string roll = "";
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                if (!_validatorService.HasRequestValidCaptchaEntry(Language.English, DisplayMode.ShowDigits))
                {
                    this.ModelState.AddModelError(_captchaOptions.CaptchaComponent.CaptchaInputName, "Please Enter Security code as number.");
                    // return View("Index");
                    return Page();
                }
                else
                {
                    var user = new RegisteredUser();
                    if(Input.OrganizationName == null)
                    {
                        user.UserName = Input.Email;
                        user.Email = Input.Email;
                        user.FirstName = Input.FirstName;
                        user.LastName = Input.LastName;
                        user.UserPhone = Input.UserPhone;
                        user.ResidencyStatus = Input.ResidencyStatus;
                        user.UserFlag = 2;
                        roll = "User";
                        user.UserSectors = new List<UserSector>();
                        foreach (var item in Input.Sectors)
                        {
                            user.UserSectors.Add(new UserSector { SectorId = item });
                        }
                         
                    }
                    else
                    {
                        user.UserName = Input.Email;
                        user.Email = Input.Email;
                        user.FirstName = Input.FirstName;
                        user.LastName = Input.LastName;
                        user.UserPhone = Input.UserPhone;
                        user.ResidencyStatus = Input.ResidencyStatus;
                        user.OrganizationName = Input.OrganizationName;
                        user.UserFlag = 3;
                        roll = "Recruiter";
                    }

                    var result = await _userManager.CreateAsync(user, Input.Password);

                    //Console.WriteLine(result);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created a new account with password.");
                        var isAdded =  await _userManager.AddToRoleAsync(user, roll);
                        if (isAdded.Succeeded)
                        {
                            int userId = user.Id;
                            if (Input.OrganizationName == null)
                            {
                                ApplicantProfile applicantProfile = new ApplicantProfile() { RegisteredUserId = userId, JobTitle = "" };
                                _context.ApplicantProfiles.Add(applicantProfile);
                                _context.SaveChanges();
                            }
                            else
                            {
                                CompanyProfile companyProfile = new CompanyProfile() { RegisteredUserId = userId , ContactName = Input.FirstName +" "+ Input.LastName , ContactPhone = Input.UserPhone , CompanyWebsite = " "};
                                _context.CompanyProfiles.Add(companyProfile);
                                _context.SaveChanges();
                            }
                           
                            
                        }
                        
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                            protocol: Request.Scheme);

                        //await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        //$"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
                        if (user.OrganizationName == null)
                        {
                            await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                            $"<h1>Welcome to S M Software Solutions!</h1><br>Hello " + Input.FirstName + ",<br><br> Thank you for getting started with SMSS! " +
                            $"We need a little more information to complete your registration, including confirmation of your account. <br>" +
                            $"Please Click on this link to confirm your account: <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' class='theme-btn btn-sm'>Click Here</a>" +
                            $"<br><br><b>New User Register</b><br> Username: " + Input.Email + "<br> Password: " + Input.Password +
                            $"<br><br>If you are facing any trouble to login, please contact our team by filling out the form available at <a asp-controller='Home' asp-action='Contactus'>Contact Page</a> of our website." +
                            $"<br><br> Thanks, <br> S M Software Consulting <br>info@smsoftconsulting.com <br> 25 Wandering Trail, Toronto, ON, Canada, M1X 1K4 <br><br> You are receiving this because you’ve signed up for a new account.");
                        } else {
                            await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                            $"<h1>Welcome to S M Software Solutions!</h1><br>Hello " + Input.OrganizationName + ",<br><br> Thank you for getting started with SMSS! " +
                            $"We need a little more information to complete your registration, including confirmation of your account. <br>" +
                            $"Please Click on this link to confirm your account: <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' class='theme-btn btn-sm'>Click Here</a>" +
                            $"<br><br><b>New User Register</b><br> Username: " + Input.Email + "<br> Password: " + Input.Password +
                            $"<br><br> After Email Confirmation we need 2 days to review Company profile for Approval Kindly fill the company profile to complete the aproval process" +
                            $"<br><br>If you are facing any trouble to login, please contact our team by filling out the form available at <a asp-controller='Home' asp-action='Contactus'>Contact Page</a> of our website." +
                            $"<br><br> Thanks, <br> S M Software Consulting <br>info@smsoftconsulting.com <br> 25 Wandering Trail, Toronto, ON, Canada, M1X 1K4 <br><br> You are receiving this because you’ve signed up for a new account.");

                            await _emailSender.SendEmailAsync("info@smsoftconsulting.com", "Company Approval Request",
                           $"Hello Admin,<br> " + Input.FirstName + " has successfully created an account on our website. " +
                           $"Kindly verify the details of the registered employer from the admin dashboard. <br>" +
                           $"<br><b>New User Register</b><br> Email: " + Input.Email +
                           $"<br> Organization Name: " + Input.OrganizationName +
                           $"<br> Phone Number: " + Input.UserPhone +
                           $"<br><br> Thanks, <br> SMSS Support ");

                        }
                    
                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
                
            }


            // If we got this far, something failed, redisplay form

            return Page();
        }
    }
}
