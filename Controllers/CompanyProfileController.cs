using ClosedXML.Excel;
using Glimpse.AspNet.Tab;
using jdk.nashorn.tools;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MimeKit;
using Nest;
using SMSS.Data;
using SMSS.Models;
using SMSS.Services;
using SMSS.ViewModels;
using SMSS.WebSecurity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;
using Microsoft.Extensions.Configuration;
using SendGrid;
using System.Text.Encodings.Web;

using SendGrid.Helpers.Mail;
using AutoMapper;
using javax.swing.text.html;
using StackExchange.Redis;
using Microsoft.Extensions.Logging;
using java.awt;

namespace SMSS.Controllers
{
    [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Recruiter , Admin, SuperAdmin")]
    public class CompanyProfileController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<RegisteredUser> _manager;
        private readonly SignInManager<RegisteredUser> _signInManager;
        private readonly IWebHostEnvironment _iweb;
        private readonly IWebHostEnvironment _hostingEnvironment;
        //private readonly ILogger<CompanyProfileController> _logger;




        //private readonly IDataProtector _protector;

        private readonly IOptions<SMSSSmtpClientSettings> _options;


        //private readonly IConfiguration _configuration;
        //private readonly ISendGridClient _sendGridClient;
        //private readonly IEmailSubscriptionRepository _emailRepository;
        //private readonly HtmlEncoder _htmlEncoder;
        //private readonly IEmailRepository _emaiRepo;
        //private readonly IMapper _mapper;

        public CompanyProfileController(
            ApplicationDbContext dbContext,                                        
            UserManager<RegisteredUser> manager,
            SignInManager<RegisteredUser> signInManager,
            IWebHostEnvironment iweb,
            IDataProtectionProvider dataProtectionProvider,
            CompanyProfileIdProtectionSettings companyProfileIdProtectionSettings,
            IOptions<SMSSSmtpClientSettings> options,
            IWebHostEnvironment hostEnvironment
            //IConfiguration configuration,
            //ISendGridClient sendGridClient,
            //IEmailSubscriptionRepository emailRepository,
            //HtmlEncoder htmlEncoder,
            //IEmailRepository emaiRepo,
            //IMapper mapper,
            //ILogger<CompanyProfileController> logger
            )
        {
            _context = dbContext;
            _manager = manager;
            _signInManager = signInManager;
            _iweb = iweb;
            _options = options;
            _hostingEnvironment = hostEnvironment;
            //_configuration = configuration;
            //_sendGridClient = sendGridClient;
            //_emailRepository = emailRepository;
            //_htmlEncoder = htmlEncoder;
            //_emaiRepo = emaiRepo;
            //_mapper = mapper;
            //_logger = logger;
        }

        //[Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Recruiter")]
        public IActionResult GetCompanyDashboard()
        {
            ViewBag.companyName = _options.Value.AppName;

            int id = int.Parse(_manager.GetUserId(HttpContext.User));

            RegisteredUser currUser = _context.RegisteredUsers
               .Include(ru => ru.CompanyProfile)
               .Where(ru => ru.Id == id)
              .FirstOrDefault();

            ViewData["OrganizationName"] = currUser.OrganizationName;
            ViewData["UserFullName"] = currUser.FirstName + " " + currUser.LastName;
            ViewBag.Picture = currUser.CompanyProfile.CompanyLogo;

            int companyId = _context.CompanyProfiles.FirstOrDefault(cp => cp.RegisteredUserId == id).Id;
            IQueryable<CompanyJobPostVM> data = from jobs in _context.CompanyJobs
                                             .Where(ja => ja.CompanyProfileId == companyId)
                                                group jobs by jobs.PostingDate.Date
                                             into dateGroup
                                                select new CompanyJobPostVM()
                                                {
                                                    jobPostDate = dateGroup.Key,
                                                    jobCount = dateGroup.Count()
                                                };
            return View(data);

        }
        public IActionResult CompanyDashboardViewDetails()
        {
            ViewBag.companyName = _options.Value.AppName;

            int id = int.Parse(_manager.GetUserId(HttpContext.User));

            RegisteredUser currUser = _context.RegisteredUsers
              .Include(ru => ru.CompanyProfile)
              .Where(ru => ru.Id == id)
             .FirstOrDefault();



            ViewData["OrganizationName"] = currUser.OrganizationName;
            ViewData["UserFullName"] = currUser.FirstName + " " + currUser.LastName;
            ViewBag.Picture = currUser.CompanyProfile.CompanyLogo;

            int companyId = _context.CompanyProfiles.FirstOrDefault(cp => cp.RegisteredUserId == id).Id;
            List<CompanyJob> companyJob = _context.CompanyJobs
                .Where(cj => cj.CompanyProfileId == companyId)
                .Include(cj => cj.ApplicantJobApplications)
                .OrderByDescending(cj => cj.PostingDate)
                .ToList();
            return View(companyJob);
        }

        //[Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        public IActionResult GetAdminDashboard(int? companyId)
        {
            ViewBag.companyName = _options.Value.AppName;

            int id = int.Parse(_manager.GetUserId(HttpContext.User));
            RegisteredUser currUser = _context.RegisteredUsers
               .Include(ru => ru.CompanyProfile)
               .AsNoTracking()
               .Where(ru => ru.Id == id)
              .FirstOrDefault();



            ViewData["OrganizationName"] = currUser.OrganizationName;
            ViewData["UserFullName"] = currUser.FirstName + " " + currUser.LastName;
            ViewBag.Picture = currUser.CompanyProfile.CompanyLogo;
            if (companyId == null)
                companyId = currUser.CompanyProfile.Id;
            CompanyProfile company = _context.CompanyProfiles
                .Include(cp => cp.RegisteredUser)
                .AsNoTracking()
                .FirstOrDefault(cp => cp.Id == companyId);

            IQueryable<CompanyJobPostVM> data = from jobs in _context.CompanyJobs
                                        .Where(ja => ja.CompanyProfileId == companyId)
                                                group jobs by jobs.PostingDate.Date
                                         into dateGroup
                                                select new CompanyJobPostVM()
                                                {
                                                    jobPostDate = dateGroup.Key,
                                                    jobCount = dateGroup.Count()
                                                };
            AdminDashboard adminDashboard = new AdminDashboard()
            {
                CompanyId = company.Id,
                CompanyLogo = company.CompanyLogo,
                CompanyName = company.RegisteredUser.OrganizationName,
                CompanyPosts = data.ToList(),
                Companies = _context.CompanyProfiles
                    .Include(cp => cp.RegisteredUser)
                    .Select(cp => new SelectListItem { Text = cp.RegisteredUser.OrganizationName, Value = cp.Id.ToString(), Selected = cp.Id == companyId }).ToList(),
                ApplicantJobApplications = _context.ApplicantJobApplications
                    .Where(aja => aja.CompanyJob.CompanyProfileId == companyId)
                    .Include(aja => aja.CompanyJob)
                    .Include(aja => aja.ApplicantProfile)
                    .ThenInclude(ap => ap.RegisteredUser)
                    .Include(aja => aja.ApplicantProfile)
                    .ThenInclude(ap => ap.Province)
                    .Include(aja => aja.ApplicantProfile)
                    .ThenInclude(ap => ap.Country)
                    .OrderByDescending(aja => aja.ApplicationDate)
                    .AsNoTracking()
                    .Take(10).ToList()
            };




            return View(adminDashboard);

        }


        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        [HttpGet]
        public IActionResult AllTestinomials()
        {
            ViewBag.companyName = _options.Value.AppName;

            return View();
        }


        public IActionResult GetCompanyProfile()
        {
            ViewBag.companyName = _options.Value.AppName;

            CompanyProfileVM currCompanyProfile = new CompanyProfileVM();
            int id = int.Parse(_manager.GetUserId(HttpContext.User));
            RegisteredUser currUser = _context.RegisteredUsers
                .Include(ru => ru.CompanyProfile)
                    .ThenInclude(cp => cp.CompanyLocations)
                    .ThenInclude(cl => cl.Country)
                .Include(ru => ru.CompanyProfile)
                    .ThenInclude(cp => cp.CompanyLocations)
                    .ThenInclude(cl => cl.Province)
                .Include(ru => ru.CompanyProfile)
                    .ThenInclude(cp => cp.CompanyLocations)
                    .ThenInclude(cl => cl.City)
                .Where(ru => ru.Id == id)
               .FirstOrDefault();


            ViewData["OrganizationName"] = currUser.OrganizationName;
            currCompanyProfile.Countries = GetCountryList();
            if (currUser.CompanyProfile != null)
            {
                ViewBag.Picture = currUser.CompanyProfile.CompanyLogo;
                currCompanyProfile.CompanyProfile = currUser.CompanyProfile;
                currCompanyProfile.CompanyProfile.OrganizationName = currUser.OrganizationName;
                foreach (var location in currUser.CompanyProfile.CompanyLocations)
                {
                    currCompanyProfile.CompanyLocations.Add(new CompanyLocationVM
                    {
                        CompanyLocation = location,
                        lookupVM = new LookupVM
                        {
                            Countries = GetCountryList(),
                            Provinves = GetProvicesList(location.CountryId),
                            Cities = GetCitiesList(location.ProvinceId)
                        }
                    });
                }
            }



            return View(currCompanyProfile);
        }

        [HttpPost]
        public IActionResult UpdateProfile(CompanyProfile companyProfile)
        {
            ViewBag.companyName = _options.Value.AppName;

            if (ModelState.IsValid)
            {
                int id = int.Parse(_manager.GetUserId(HttpContext.User));
                CompanyProfile currCompanyProfile = _context.CompanyProfiles.FirstOrDefault(cp => cp.RegisteredUserId == id);
                RegisteredUser currRegisteredUser = _context.RegisteredUsers.FirstOrDefault(ru => ru.Id == id);
                byte[] uploadLogo = null;



                if (companyProfile != null)
                {
                    if (companyProfile.LogoFile != null)
                    {
                        using (var rStream = companyProfile.LogoFile.OpenReadStream())
                        using (var mStream = new MemoryStream())
                        {
                            rStream.CopyTo(mStream);
                            uploadLogo = mStream.ToArray();
                        }
                        currCompanyProfile.CompanyLogo = uploadLogo;
                    }

                    if (currRegisteredUser.OrganizationName != companyProfile.OrganizationName)
                    {
                        currRegisteredUser.OrganizationName = companyProfile.OrganizationName;
                        _context.Update(currRegisteredUser);
                    }
                    if (currCompanyProfile != null)
                    {
                        currCompanyProfile.ContactName = companyProfile.ContactName;
                        currCompanyProfile.CompanyWebsite = companyProfile.CompanyWebsite;
                        currCompanyProfile.LinkedIn = companyProfile.LinkedIn;
                        currCompanyProfile.ContactPhone = companyProfile.ContactPhone;
                        currCompanyProfile.RegisteredUser.Email = companyProfile.RegisteredUser.Email;
                        currCompanyProfile.Description = companyProfile.Description;
                        currCompanyProfile.Email1 = companyProfile.Email1;
                        currCompanyProfile.Email2 = companyProfile.Email2;

                        var checkUpdateStatus = _context.Update(currCompanyProfile);

                        if (checkUpdateStatus != null)
                        {
                            AlertMessage("You Have UPDATED Your Profile Successfully!!!..", NotificationType.success);
                        }
                        else
                        {
                            AlertMessage("Oppss.. Profile could not be UPDATED. Try again Later!!!..", NotificationType.error);
                        }
                    }
                    else
                    {
                        CompanyProfile newCompanyProfile = new CompanyProfile
                        {
                            RegisteredUserId = id,
                            ContactName = companyProfile.ContactName,
                            CompanyWebsite = companyProfile.CompanyWebsite,
                            LinkedIn = companyProfile.LinkedIn,
                            ContactPhone = companyProfile.ContactPhone,
                            Description = companyProfile.Description,
                            Email1 = companyProfile.Email1,
                            Email2 = companyProfile.Email2,
                            CompanyLogo = uploadLogo
                        };

                        _context.CompanyProfiles.Add(newCompanyProfile);

                    }

                    _context.SaveChanges();
                }
            }
            else
            {
                string errorMessages = "";


                foreach (var modelStateKey in ViewData.ModelState.Keys)
                {
                    //decide if you want to show it or not...
                    //...

                    var value = ViewData.ModelState[modelStateKey];
                    foreach (var error in value.Errors)
                    {
                        errorMessages += error.ErrorMessage;
                    }
                }

                TempData["ErrorMessage"] = errorMessages;
            }

            return RedirectToAction("GetCompanyProfile");
        }



        [HttpPost]
        public IActionResult UpdateLocation(IFormCollection companyLocationForm)
        {
            ViewBag.companyName = _options.Value.AppName;

            if (companyLocationForm != null)
            {
                int id = int.Parse(companyLocationForm["location.CompanyLocation.Id"].ToString());
                CompanyLocation currCompanyLocation = _context.CompanyLocations.FirstOrDefault(cl => cl.Id == id);
                currCompanyLocation.CountryId = int.Parse(companyLocationForm["location.CompanyLocation.CountryId"].ToString());
                currCompanyLocation.ProvinceId = int.Parse(companyLocationForm["location.CompanyLocation.ProvinceId"].ToString());
                currCompanyLocation.CityId = int.Parse(companyLocationForm["location.CompanyLocation.CityId"].ToString());
                currCompanyLocation.Street = companyLocationForm["location.CompanyLocation.Street"].ToString();
                currCompanyLocation.PostalCode = companyLocationForm["location.CompanyLocation.PostalCode"].ToString();

                _context.CompanyLocations.Update(currCompanyLocation);

                _context.SaveChanges();

            }


            return RedirectToAction("GetCompanyProfile");
        }


        
        [HttpPost]
        public IActionResult AddLocation(IFormCollection companyLocationForm)
        {
            ViewBag.companyName = _options.Value.AppName;

            if (companyLocationForm != null)
            {
                int id = int.Parse(_manager.GetUserId(HttpContext.User));
                int companyProfileId = _context.CompanyProfiles.FirstOrDefault(cp => cp.RegisteredUserId == id).Id;
                CompanyLocation newCompanyLocation = new CompanyLocation();
                newCompanyLocation.CompanyProfileId = companyProfileId;
                newCompanyLocation.CountryId = int.Parse(companyLocationForm["CompanyLocation.CountryId"].ToString());
                newCompanyLocation.ProvinceId = int.Parse(companyLocationForm["CompanyLocation.ProvinceId"].ToString());
                newCompanyLocation.CityId = int.Parse(companyLocationForm["CompanyLocation.CityId"].ToString());
                newCompanyLocation.Street = companyLocationForm["CompanyLocation.Street"].ToString();
                newCompanyLocation.PostalCode = companyLocationForm["CompanyLocation.PostalCode"].ToString();

                _context.CompanyLocations.Add(newCompanyLocation);

                _context.SaveChanges();

            }


            return RedirectToAction("GetCompanyProfile");
        }




        [HttpPost]
        public IActionResult DeleteLocation(IFormCollection companyLocationForm)
        {
            ViewBag.companyName = _options.Value.AppName;

            if (companyLocationForm != null)
            {
                int id = int.Parse(companyLocationForm["location.CompanyLocation.Id"].ToString());
                CompanyLocation delCompanyLocation = _context.CompanyLocations.FirstOrDefault(cl => cl.Id == id);
                _context.CompanyLocations.Remove(delCompanyLocation);
                _context.SaveChanges();

            }


            return RedirectToAction("GetCompanyProfile");
        }

        [HttpPost]
        public JsonResult AjaxMethod(string type, int value)
        {
            LookupVM model = new LookupVM();
            switch (type)
            {
                case "Country":
                    List<Province> provinces = _context.Provinces.Where(p => p.CountryId == value).ToList();
                    foreach (var province in provinces)
                    {
                        model.Provinves.Add(new SelectListItem() { Value = province.Id.ToString(), Text = province.ProvinceName });
                    }


                    break;
                case "Province":
                    List<City> cities = _context.Cities.Where(c => c.ProvinceId == value).ToList();
                    foreach (var city in cities)
                    {
                        model.Cities.Add(new SelectListItem() { Value = city.Id.ToString(), Text = city.CityName });
                    }

                    break;
            }
            return Json(model);
        }


        [HttpGet]
        public IActionResult PostJob()
        {
            ViewBag.companyName = _options.Value.AppName;

            PostJobVM postJobVM = new PostJobVM();

            int id = int.Parse(_manager.GetUserId(HttpContext.User));
            RegisteredUser currUser = _context.RegisteredUsers
                .Include(ru => ru.CompanyProfile)
                .Where(ru => ru.Id == id)
               .FirstOrDefault();

            ViewData["OrganizationName"] = currUser.OrganizationName;
            ViewData["UserFullName"] = currUser.FirstName + " " + currUser.LastName;
            ViewBag.Picture = currUser.CompanyProfile.CompanyLogo;

            if (!currUser.CompanyProfile.IsApproved)
            {
                return View("CompanyConfirm");
            }

            List<ProvinceDemoFileAttachment> prVM = _context.ProvinceDemoFileAttachments.OrderByDescending(pf => pf.Id).ToList();

            postJobVM.CompanyJob.CompanyProfileId = currUser.CompanyProfile.Id;
            //postJobVM.CompanyJob.ProvinceDemoFileAttachmentId = currUser.CompanyProfile.Id;
            postJobVM.Countries = GetCountryList();
            postJobVM.Sectors = GetSectorsList();
            postJobVM.ProvinceDemoFileAttachments = GetProvinceDemoFileAttachmentsList();
            postJobVM.JobModes = GetJobModesList();
            
            //postJobVM.ProvinceDemoFileAttachments = prVM;

            return View(postJobVM);
        }

        

        [HttpPost]
        public async Task<IActionResult> AddJob(IFormCollection formData, IFormFile fileInfo)
        {

            string attchmentPath = "";

            if (ModelState.IsValid == false)
            {
                return NotFound();
            }

            if (fileInfo != null)
            {
                attchmentPath = await SaveFile(fileInfo);
            }

            //if (formData != null)
            //{
                CompanyJob jopPost = new CompanyJob();
                jopPost.CompanyProfileId = int.Parse(formData["CompanyJob.CompanyProfileId"]);
                jopPost.JobTitle = formData["CompanyJob.JobTitle"];
                jopPost.JobDescription = formData["CompanyJob.JobDescription"];
                jopPost.JobExperience = (Experience)Enum.Parse(typeof(Qualifications), formData["CompanyJob.JobExperience"].ToString());
                jopPost.JobQualification = (Qualifications)Enum.Parse(typeof(Qualifications), formData["CompanyJob.JobQualification"].ToString());
                jopPost.ExpireDate = DateTime.Parse(formData["CompanyJob.ExpireDate"].ToString());
                jopPost.PostingDate = DateTime.Now;

                jopPost.FileAttachment = Path.GetFileName(attchmentPath);

                jopPost.IsAttachmentRequired = formData["CompanyJob.IsAttachmentRequired"].Count == 1 ? false : true;

                jopPost.CountryId = int.Parse(formData["CompanyJob.CountryId"]);

                jopPost.ProvinceId = int.Parse(formData["CompanyJob.ProvinceId"]);
                jopPost.CityId = int.Parse(formData["CompanyJob.CityId"]);
                jopPost.Street = formData["CompanyJob.Street"];
                jopPost.CompanyJobSectors = new List<CompanyJobSector>();


                //jopPost.ProvinceDemoFileAttachmentId = int.Parse(formData["CompanyJob.ProvinceDemoFileAttachmentId"]);

                if (!string.IsNullOrEmpty(formData["CompanyJob.ProvinceDemoFileAttachmentId"]))
                {
                    jopPost.ProvinceDemoFileAttachmentId = int.Parse(formData["CompanyJob.ProvinceDemoFileAttachmentId"]);
                }
                else
                {
                   jopPost.ProvinceDemoFileAttachmentId = null; 
                }

            //if (formData["CompanyJob.ProvinceDemoFileAttachmentId"] != "")
            //{
            //    jopPost.ProvinceDemoFileAttachmentId = int.Parse(formData["CompanyJob.ProvinceDemoFileAttachmentId"]);
            //}
            //else
            //{
            //    jopPost.ProvinceDemoFileAttachmentId = null;
            //}

            //jopPost.JobModeId = int.Parse(formData["CompanyJob.JobModeId"]);

            if (!string.IsNullOrEmpty(formData["CompanyJob.JobModeId"]))
            {
                
                jopPost.JobModeId = null;
            }
            else
            {
                jopPost.JobModeId = int.Parse(formData["CompanyJob.JobModeId"]);
            }

            //if (formData["CompanyJob.JobModeId"] != "")
            //{
            //jopPost.JobModeId = int.Parse(formData["CompanyJob.JobModeId"]); 
            //}
            //else
            //{
            //    jopPost.JobModeId = null;
            //}


            foreach (var item in formData["CompanyJob.CompanyJobSectors"])
                {
                    jopPost.CompanyJobSectors.Add(new CompanyJobSector { CompanyJobId = jopPost.Id, SectorId = int.Parse(item) });
                }

                _context.CompanyJobs.Add(jopPost);
                _context.SaveChanges();

                //jopPost.CompanyJobSectors = new List<CompanyJobSector>();
                //foreach (var item in formData["CompanyJob.CompanyJobSectors"])
                //{
                //    jopPost.CompanyJobSectors.Add(new CompanyJobSector { CompanyJobId = jopPost.Id, SectorId = int.Parse(item) });
                //}
                //_context.CompanyJobs.Update(jopPost);
                //_context.SaveChanges();
            //}
            return RedirectToAction("JobList", "CompanyJobs");


        }


        [HttpGet]
        public IActionResult ChangePassword()
        {
            ViewBag.companyName = _options.Value.AppName;

            int id = int.Parse(_manager.GetUserId(HttpContext.User));
            RegisteredUser currUser = _context.RegisteredUsers
                .Include(ru => ru.CompanyProfile)
                .Where(ru => ru.Id == id)
               .FirstOrDefault();


            ViewData["OrganizationName"] = currUser.OrganizationName;
            ViewData["UserFullName"] = currUser.FirstName + " " + currUser.LastName;
            ViewBag.Picture = currUser.CompanyProfile.CompanyLogo;

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePassword model)
        {
            ViewBag.companyName = _options.Value.AppName;

            int id = int.Parse(_manager.GetUserId(HttpContext.User));
            RegisteredUser currUser = _context.RegisteredUsers
                .Include(ru => ru.CompanyProfile)
                .Where(ru => ru.Id == id)
               .FirstOrDefault();


            ViewData["OrganizationName"] = currUser.OrganizationName;
            ViewData["UserFullName"] = currUser.FirstName + " " + currUser.LastName;
            ViewBag.Picture = currUser.CompanyProfile.CompanyLogo;

            if (ModelState.IsValid)
            {
                var user = await _manager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login");
                }

                // ChangePasswordAsync changes the user password
                var result = await _manager.ChangePasswordAsync(user,
                    model.CurrentPassword, model.NewPassword);


                // The new password did not meet the complexity rules or
                // the current password is incorrect. Add these errors to
                // the ModelState and rerender ChangePassword view
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View();
                }

                // Send Password change confirmation Emal
                using (MailMessage mailMessage = new MailMessage())
                {
                    mailMessage.From = new MailAddress(_options.Value.FromEmail);
                    mailMessage.Subject = "Your SMSS Account password has been reset.";
                    mailMessage.Body = "Dear " + currUser.FirstName + ",<br>" +
                   $"We’ve reset your password for username : " + currUser.UserName + "  <br/>" +
                   $"Your new Password : " + model.NewPassword +
                   $"<br><br> Thanks, <br> SMSS Support ";
                    mailMessage.IsBodyHtml = true;
                    mailMessage.To.Add(new MailAddress(currUser.Email));
                    System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient();
                    smtp.UseDefaultCredentials = false;

                    smtp.Host = _options.Value.Host;
                    smtp.EnableSsl = true;

                    System.Net.NetworkCredential networkcred = new System.Net.NetworkCredential();
                    networkcred.UserName = _options.Value.Username;
                    networkcred.Password = _options.Value.Password;
                    smtp.Credentials = networkcred;

                    smtp.Port = _options.Value.Port;
                    await smtp.SendMailAsync(mailMessage);

                }

                // Upon successfully changing the password refresh sign-in cookie
                await _signInManager.RefreshSignInAsync(user);
                return View("ChangePasswordConfirmation");
            }

            return View(model);
        }




        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        public async Task<IActionResult> CompanyApproval(int? pageNumber, int pageSize = 5)
        {
            ViewBag.companyName = _options.Value.AppName;

            var Companyprofile = _context.CompanyProfiles
                                .Include(ru => ru.RegisteredUser);



            return View(await PaginatedList<CompanyProfile>.CreateAsync(Companyprofile, pageNumber ?? 1, pageSize));

        }

        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        [HttpGet]
        public async Task<PartialViewResult> PartialCompanyApproval(int? pageNumber, int pageSize = 2)
        {
            ViewBag.companyName = _options.Value.AppName;

            var Companyprofile = _context.CompanyProfiles
                                .Include(ru => ru.RegisteredUser);

            return PartialView("_CompanyApproval", await PaginatedList<CompanyProfile>.CreateAsync(Companyprofile, pageNumber ?? 1, pageSize));

        }


        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        public async Task<IActionResult> ApproveCompany(int Id)
        {
            ViewBag.companyName = _options.Value.AppName;

            var comapny = await _context.CompanyProfiles.FirstOrDefaultAsync(cj => cj.Id == Id);
            if (comapny != null)
            {
                comapny.IsApproved = true;

                _context.SaveChanges();
            }


            return RedirectToAction("CompanyApproval");


        }

        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        public async Task<IActionResult> DisApproveCompany(int Id)
        {
            ViewBag.companyName = _options.Value.AppName;

            var comapny = await _context.CompanyProfiles.FirstOrDefaultAsync(cj => cj.Id == Id);
            if (comapny != null)
            {
                comapny.IsApproved = false;

                _context.SaveChanges();
            }


            return RedirectToAction("CompanyApproval");


        }



        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        public async Task<IActionResult> TestimonialApproval(int? pageNumber, int pageSize = 5)
        {

            ViewBag.companyName = _options.Value.AppName;

            var TestimonialList = _context.ApplicantTestimonials.OrderByDescending(ru => ru.ApplicantProfile)
                               .Include(ru => ru.ApplicantProfile)
                               .ThenInclude(cp => cp.RegisteredUser);




            return View(await PaginatedList<ApplicantTestimonial>.CreateAsync(TestimonialList, pageNumber ?? 1, pageSize));

        }

        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        [HttpGet]
        public async Task<PartialViewResult> PartialTestimonialApproval(int? pageNumber, int pageSize = 2)
        {
            ViewBag.companyName = _options.Value.AppName;

            var TestimonialList = _context.ApplicantTestimonials
                                .Include(ru => ru.ApplicantProfile)
                                .ThenInclude(cp => cp.RegisteredUser);

            return PartialView("_TestimonialApproval", await PaginatedList<ApplicantTestimonial>.CreateAsync(TestimonialList, pageNumber ?? 1, pageSize));

        }

        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        [Route("ApproveTestimonial/{id:int}")]
        public async Task<IActionResult> ApproveTestimonial(int Id)
        {
            ViewBag.companyName = _options.Value.AppName;

            var testimonial = await _context.ApplicantTestimonials.FirstOrDefaultAsync(cj => cj.Id == Id);
            if (testimonial != null)
            {
                testimonial.IsApprove = true;

                _context.SaveChanges();
            }

            //send email

            var Toemailaddress = "";
            var regusername = "";
            if (_signInManager.IsSignedIn(User))
            {

                ApplicantTestimonial at = _context.ApplicantTestimonials.FirstOrDefault(at => at.Id == Id);
                ApplicantProfile ap = _context.ApplicantProfiles.FirstOrDefault(ap => ap.Id == at.ApplicantProfileId);


                int reguserId = ap.RegisteredUserId;
                RegisteredUser reguser = _context.RegisteredUsers.FirstOrDefault(ap => ap.Id == reguserId);

                if (reguser != null)
                {
                    Toemailaddress = reguser.Email.ToString();
                    regusername = reguser.FirstName.ToString() + " " + reguser.LastName.ToString();
                }

            }
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("S M Software Solutions", "donotreply@smsoftconsulting.com"));
            email.To.Add(new MailboxAddress(regusername, Toemailaddress));
            email.Subject = "Thank you for your Testimonial";

            BodyBuilder bodyBuilder = new BodyBuilder();

            bodyBuilder.HtmlBody = "Dear <b>" + regusername + ", </b> <br><br/>" +
               $"Thank you for submitting your Testimonial for SMSS. <br/><br/>" +
               $"Your Testimonial is successfully approved. You can view your testimonial on the website.<br/>" +
               $"<br><br> Thanks, <br> SMSS Support ";

            bodyBuilder.Attachments.Add(_iweb.WebRootPath + "\\images\\" + "\\logo\\smsslogo.PNG");

            email.Body = bodyBuilder.ToMessageBody();

                    
            using (var client = new SmtpClient())
            {
               client.Connect("smtp.gmail.com", 465, true);
               client.Authenticate("info@smsoftconsulting.com", "smsoftconsulting@123");
               client.Send(email);
               client.Disconnect(true);
               client.Dispose();
            }

           



            return RedirectToAction("TestimonialApproval");


        }

        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]

        [Route("DisApproveTestimonial/{id:int}")]
        public async Task<IActionResult> DisApproveTestimonial(int Id)
        {
            ViewBag.companyName = _options.Value.AppName;

            var testimonial = await _context.ApplicantTestimonials.FirstOrDefaultAsync(cj => cj.Id == Id);
            if (testimonial != null)
            {
                testimonial.IsApprove = false;

                _context.SaveChanges();
            }

            //send email



            var Toemailaddress = "";
            var regusername = "";
            if (_signInManager.IsSignedIn(User))
            {

                ApplicantTestimonial at = _context.ApplicantTestimonials.FirstOrDefault(at => at.Id == Id);
                ApplicantProfile ap = _context.ApplicantProfiles.FirstOrDefault(ap => ap.Id == at.ApplicantProfileId);


                int reguserId = ap.RegisteredUserId;
                RegisteredUser reguser = _context.RegisteredUsers.FirstOrDefault(ap => ap.Id == reguserId);

                if (reguser != null)
                {
                    Toemailaddress = reguser.Email.ToString();
                    regusername = reguser.FirstName.ToString() + " " + reguser.LastName.ToString();
                }

            }
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("S M Software Solutions", "donotreply@smsoftconsulting.com"));
            email.To.Add(new MailboxAddress(regusername, Toemailaddress));
            email.Subject = "Thank you for your Testimonial";

            BodyBuilder bodyBuilder = new BodyBuilder();

            bodyBuilder.HtmlBody = "Dear <b>" + regusername + ", </b> <br/><br/>" +
               $"Thank you for submitting your Testimonial for SMSS. <br/><br/>" +
               $"Unfortunately Your Testimonial is Disapproved. You can edit  your testimonial and resubmit it from the Manage Testimonial Tab.<br/>" +
               $"<br><br> Thanks, <br> SMSS Support ";

            bodyBuilder.Attachments.Add(_iweb.WebRootPath + "\\images\\" + "\\logo\\smsslogo.PNG");

            email.Body = bodyBuilder.ToMessageBody();

                    
            using (var client = new SmtpClient())
            {
               client.Connect("smtp.gmail.com", 465, true);
               client.Authenticate("info@smsoftconsulting.com", "smsoftconsulting@123");
               client.Send(email);
               client.Disconnect(true);
               client.Dispose();
            }



            return RedirectToAction("TestimonialApproval");


        }





        private List<SelectListItem> GetCountryList()
        {
            List<SelectListItem> countries = new List<SelectListItem>();
            List<Country> dbCountry = _context.Countries.ToList();
            countries.Add(new SelectListItem() { Text = "Select Country", Value = "0" });
            foreach (Country country in dbCountry)
            {
                countries.Add(new SelectListItem { Value = country.Id.ToString(), Text = country.Name });
            }


            return countries;
        }



        private List<SelectListItem> GetProvicesList(int countryId)
        {
            List<SelectListItem> provinces = new List<SelectListItem>();
            List<Province> dbProvinces = _context.Provinces.Where(p => p.CountryId == countryId).ToList();
            foreach (Province province in dbProvinces)
            {
                provinces.Add(new SelectListItem { Value = province.Id.ToString(), Text = province.ProvinceName });
            }


            return provinces;
        }


        private List<SelectListItem> GetCitiesList(int provinceId)
        {
            List<SelectListItem> cities = new List<SelectListItem>();
            List<City> dbCites = _context.Cities.Where(p => p.ProvinceId == provinceId).ToList();
            if (dbCites != null)
            {
                cities.Add(new SelectListItem() { Text = "Please Select", Value = "0" });
                foreach (City city in dbCites)
                {
                    cities.Add(new SelectListItem { Value = city.Id.ToString(), Text = city.CityName });
                }
            }



            return cities;
        }

        private List<SelectListItem> GetSectorsList()
        {
            List<SelectListItem> sectors = new List<SelectListItem>();
            List<Sector> dbSectors = _context.Sectors.ToList();
            if (dbSectors != null)
            {

                foreach (Sector sector in dbSectors)
                {
                    sectors.Add(new SelectListItem { Value = sector.Id.ToString(), Text = sector.SectorName });
                }
            }

            return sectors;
        }


        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        [HttpGet]
        public IActionResult ManageJobSectors()
        {
            ViewBag.companyName = _options.Value.AppName;

            JobSectorVM jobSectors = new JobSectorVM();

            //List<Sector> sectors = _context.Sectors.OrderByDescending(sector => sector.Id).ToList();
            List<Sector> jsectors = _context.Sectors.OrderBy(sector => sector.SectorName).ToList();

            int index = jsectors.FindIndex(js => js.SectorName == js.SectorName);
            var previousjs = index > 1 ? jsectors[index - 1] : null;
            var nextjs = index < jsectors.Count() ? jsectors[index + 1] : null;

            //jobSectors.Sectors = sectors;

            jobSectors.Sectors = jsectors;

            return View(jobSectors);
        }


        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]

        [HttpPost]
        [ValidateAntiForgeryToken]
        public  async Task<IActionResult> AddJobSector(Sector sector)
        {

            if (sector != null)
            {
                 await _context.Sectors.AddAsync(sector);
                AlertMessage("You Have ADDED " + sector.SectorName + " Job Sector Successfully!!!..", NotificationType.success);
            }
            else
            {
                // We Direct if there's an error
                AlertMessage("Opps!!!.. Job Sector Could not be ADDED!", NotificationType.error);

            }
            await _context.SaveChangesAsync();
            return RedirectToAction("ManageJobSectors", "CompanyProfile");
        }



        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        [HttpGet]
        [Route("/[controller]/EditJobSector/{id}")]
        public async Task<IActionResult> EditJobSector(int? id)
        {
            if (id == null)
            {
                AlertMessage("Oppss.. Faile", NotificationType.error);
                return NotFound(id);
            }

            var jsector = await _context.Sectors.FindAsync(id);
            var jsectorVM = new JobSectorVM()
            {
                Id = jsector.Id,
                SectorName = jsector.SectorName,
            };
            if (jsector == null)
            {
                AlertMessage("Oppss.. Nothing Found..", NotificationType.error);
                return NotFound(id);
            }
            return View(jsectorVM);
        }




        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditJobSector(int id, JobSectorVM model)
        {
            if (ModelState.IsValid)
            {
                var jsVM = await _context.Sectors.FindAsync(model.Id);
                jsVM.SectorName = model.SectorName;
                await _context.SaveChangesAsync();
                AlertMessage("You Have UPDATED " + model.SectorName + " Job Sector Successfully!!!..", NotificationType.success);
                return RedirectToAction("ManageJobSectors", "CompanyProfile");
            }
            
            return View(model);
        }



        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        [HttpGet]
        [Route("[controller]/DeleteJobSector/{id}")]
        public async Task<IActionResult> DeleteJobSector(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var jsDel = await _context.Sectors.FirstOrDefaultAsync(js => js.Id == id);
            var jsVM = new JobSectorVM()
            {
                Id = jsDel.Id,
                SectorName = jsDel.SectorName
            };
            return View(jsVM);
        }


        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteJobSector(int id)
        {
            var jsector = await _context.Sectors.FindAsync(id);
            _context.Sectors.Remove(jsector);
            await _context.SaveChangesAsync();
            AlertMessage("You Have DELETED " + jsector.SectorName + " Job Sector Successfully!!!..", NotificationType.success);
            return RedirectToAction("ManageJobSectors", "CompanyProfile");
        }


        private List<SelectListItem> GetProvinceDemoFileAttachmentsList()
        {
            List<SelectListItem> provdemoFiles = new List<SelectListItem>();
            List<ProvinceDemoFileAttachment> _dbprovNames = _context.ProvinceDemoFileAttachments.ToList();
            provdemoFiles.Add(new SelectListItem() { Text = "", Value = "" });
            


            foreach (ProvinceDemoFileAttachment provinceDemoFileAttachment in _dbprovNames)
            {
                provdemoFiles.Add(new SelectListItem { Value = provinceDemoFileAttachment.Id.ToString(), Text = provinceDemoFileAttachment.ProvinceName });
            }
            return provdemoFiles;
        }

        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        private List<SelectListItem> GetJobModesList()
        {
            List<SelectListItem> jobModesList = new List<SelectListItem>();
            List<JobMode> _jobmodes = _context.JobModes.ToList();
            jobModesList.Add(new SelectListItem() { Text = " ", Value = " " });

            foreach (JobMode mode in _jobmodes)
            {
                jobModesList.Add(new SelectListItem { Value = mode.Id.ToString(), Text = mode.JobModeName });
            }
            return jobModesList;
        }
        
        private async Task<string> SaveFile(IFormFile file)
        {
            string filePath = "";

            string ext = Path.GetExtension(file.FileName);
            string name = Path.GetFileNameWithoutExtension(file.FileName);
            string pathroot = _iweb.WebRootPath;

            try
            {
                if (ext == ".docx" || ext == ".doc" || ext == ".pdf")
                {

                    if (file.Length == 0)
                        ModelState.AddModelError("ModelError", "please provide valid file");

                    var fileName = (name + "_" + DateTime.Now.ToString("dd_MMM_yyyy_hhmmss") + Path.GetExtension(file.FileName)).Replace(" ", "_");
                    using (var fileStream = file.OpenReadStream())
                    using (var ms = new MemoryStream())
                    {
                        await fileStream.CopyToAsync(ms);

                    }
                    // 2) Save file to local path in Resumes folder
                    filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "JobPosts", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                }

            }
            catch
            {
                //do something
            }



            return filePath;
        }


        //[Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        public async Task<IActionResult> AllCarouselImages()
        {
            var csImages = await _context.CarouselSliderImages.OrderByDescending(cs => cs.Id).ToListAsync();
            return View(csImages);
        }



        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        [HttpGet]
        public IActionResult CreateCarouselImage()
        {
           ViewBag.companyName = _options.Value.AppName;

           return View();
        }


        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCarouselImage(CarouselSliderImageVM csModel)
        {
            ViewBag.companyName = _options.Value.AppName;

            if (ModelState.IsValid)
            {
                string imgName = UploadedCSImage(csModel);

                CarouselSliderImage carousel = new CarouselSliderImage
                {
                    Id = csModel.Id,
                    Heading_Content = csModel.Heading_Content,
                    Content_Caption = csModel.Content_Caption,
                    Carousel_Button_Title = csModel.Carousel_Button_Title,
                    Carousel_Button_URL = csModel.Carousel_Button_URL,
                    CSImage = imgName,
                };
                _context.Add(carousel);
                await _context.SaveChangesAsync();
                return RedirectToAction("AllCarouselImages", "CompanyProfile");
            }
            return View(csModel);
        }


        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        [HttpGet("[controller]/CarouselEdit/{id}")]
        public async Task<IActionResult> CarouselEdit(int? id)
        {
          if (id == null)
          {
            //
            AlertMessage("Oppss.. Carousel Image Slide Does Not Exist..", NotificationType.error);
            return NotFound(id);
            
          }
              
          var carousel = await _context.CarouselSliderImages.FindAsync(id);
          var csVM = new CarouselSliderImageVM()
          {
            Id = carousel.Id,
            Heading_Content = carousel.Heading_Content,
            Content_Caption = carousel.Content_Caption,
            Carousel_Button_Title= carousel.Carousel_Button_Title,
            Carousel_Button_URL= carousel.Carousel_Button_URL,
            ExistingImage = carousel.CSImage
          };

          if (carousel == null)
          {
            AlertMessage("Oppss.. Carousel Image Slide Does Not Exist..", NotificationType.error);
            return NotFound(id);
          }

          return View(csVM);
        }


        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CarouselEdit(int id, CarouselSliderImageVM csVM)
        {
          if (ModelState.IsValid)
          {
            var carousel = await _context.CarouselSliderImages.FindAsync(csVM.Id); 
            carousel.Heading_Content = csVM.Heading_Content;
            carousel.Content_Caption = csVM.Content_Caption;
            carousel.Carousel_Button_Title = csVM.Carousel_Button_Title;
            carousel.Carousel_Button_URL = csVM.Carousel_Button_URL;

            if (csVM.CImage != null)
            {
              if (csVM.ExistingImage != null)
              {
                string filePath = Path.Combine(_iweb.WebRootPath, CarouselImageUploadLocation.CarouselUploadFolder, csVM.ExistingImage);
                System.IO.File.Delete(filePath);
              }
              carousel.CSImage = UploadedCSImage(csVM);
            }
            AlertMessage("You Have UPDATED Carousel Image Slide Successfully!!!..", NotificationType.success);
            await _context.SaveChangesAsync();
            return RedirectToAction("AllCarouselImages", "CompanyProfile");
          }
         return View(csVM);
        }



        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        public async Task<IActionResult> CarouselDetails(int? id)
        {
            if (id == null)
            {
                AlertMessage("Oppss.. Carousel Image Slide Does Not Exist..", NotificationType.error);
                return BadRequest();
            }

            var carousel = await _context.CarouselSliderImages.FirstOrDefaultAsync(cs => cs.Id == id);

            var csVM = new CarouselSliderImageVM()
            {
                Id = carousel.Id,
                Heading_Content = carousel.Heading_Content,
                Content_Caption = carousel.Content_Caption,
                ExistingImage = carousel.CSImage
            };

            if (carousel == null)
            {
                AlertMessage("Oppss.. Carousel Image Slide Does Not Exist..", NotificationType.error);
                return NotFound(id);
            }

            return View(carousel);
        }


        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        private string UploadedCSImage(CarouselSliderImageVM csModel)
        {
            string imgName = null;

            if (csModel.CImage != null)
            {
                string csFolder = Path.Combine(_iweb.WebRootPath, CarouselImageUploadLocation.CarouselUploadFolder);
                imgName = Guid.NewGuid().ToString() + "_" + csModel.CImage.FileName;
                string imgPath = Path.Combine(csFolder, imgName);

                using (var fileStream = new FileStream(imgPath, FileMode.CreateNew))
                {
                    csModel.CImage.CopyTo(fileStream);
                }
            }
            return imgName;
        }


        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        [HttpGet("[controller]/CarouselDelete/{id}")]
        public async Task<IActionResult> CarouselDelete(int? id)
          {
            if (id == null)
            {
              //AlertMessage("Oppss.. Carousel Image Slide Does Not Exist..", NotificationType.error);
              return NotFound(id);
            }
            var csDel = await _context.CarouselSliderImages.FirstOrDefaultAsync(cs => cs.Id == id);
            var csVM = new CarouselSliderImageVM()
            {
              Id = csDel.Id,
              Heading_Content = csDel.Heading_Content,
              Content_Caption = csDel.Content_Caption,
              Carousel_Button_Title = csDel.Carousel_Button_Title,
              Carousel_Button_URL = csDel.Carousel_Button_URL,
              ExistingImage = csDel.CSImage
            };

            if (csVM != null)
            {
              //AlertMessage("Oppss.. Carousel Image Slide Does Not Exist..", NotificationType.error);
              return NotFound(id);
            }
            return View(csVM);
          }



          [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
          [HttpPost]
          [ValidateAntiForgeryToken]
          public async Task<IActionResult> CarouselDeleteConfirmed(int id)
          {
            var carousel = await _context.CarouselSliderImages.FindAsync(id);

            var CurrCSImage = Path.Combine(Directory.GetCurrentDirectory(), CarouselImageUploadLocation.DeleteCarouselImageFromFolder, carousel.CSImage);
            
            _context.CarouselSliderImages.Remove(carousel);

            if (System.IO.File.Exists(CurrCSImage))
            {
                System.IO.File.Delete(CurrCSImage);
            }
            await _context.SaveChangesAsync();

            return RedirectToAction("AllCarouselImages", "CompanyProfile");
          }

          private bool CarouselSliderImageExists(int id)
          {
            return _context.CarouselSliderImages.Any(cs => cs.Id == id);
          }

        //[Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> JobModes()
        {
            ViewBag.companyName = _options.Value.AppName;
            JobeModeVM model = new JobeModeVM();
            model.JobModes = await _context.JobModes.Select(jm => jm).ToListAsync();
            return View(model);
        }



        //[Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        [HttpGet]
        public IActionResult ManageJobModes()
        {
            ViewBag.companyName = _options.Value.AppName;

            JobeModeVM jobmodes = new JobeModeVM();

            List<JobMode> jmodes = _context.JobModes.OrderBy(jm => jm.JobModeName).ToList();

            int index = jmodes.FindIndex(jm => jm.JobModeName == jm.JobModeName);
            var previousjm = index > 1 ? jmodes[index - 1] : null;
            var nextjm = index < jmodes.Count() ? jmodes[index + 1] : null;

            jobmodes.JobModes = jmodes;

            return View(jobmodes);

        }


        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        [HttpPost]
        [Route("AddJobMode")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddJobMode(JobMode jobmode)
        {
            if (jobmode != null && jobmode.JobModeName != null)
            {
                await _context.JobModes.AddAsync(jobmode);
                await _context.SaveChangesAsync();

                AlertMessage("You Have ADDED " + jobmode.JobModeName + " Job Mode for Roles Successfully!!!..", NotificationType.success);                
            }
            else
            {
                // We Direct if there's an error
                AlertMessage("Opps!!!.. Job Mode for Role Could not be ADDED!", NotificationType.error);
            }        
            
            return RedirectToAction("ManageJobModes", "CompanyProfile");
        }


        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        [HttpGet]
        [Route("[controller]/EditJobMode/{id}")]
        public async Task<IActionResult> EditJobMode(int? id)
        {
            if (id == null)
            {
                AlertMessage("Oppss.. Failed, Something Went Wrong", NotificationType.error);
                return NotFound(id);
            }
            var jmode = await _context.JobModes.FindAsync(id);
            var jmodeVM = new JobeModeVM()
            {
                Id = jmode.Id,
                JobModeName = jmode.JobModeName
            };
            if (jmode == null)
            {
                AlertMessage("Oppss.. Job Mode ID Not Found..", NotificationType.error);
                return NotFound(id);
            }
            return View(jmodeVM);
        }


        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditJobMode(int id, JobeModeVM model)
        {
            if (ModelState.IsValid)
            {
                var jmVM = await _context.JobModes.FindAsync(model.Id);
                jmVM.JobModeName = model.JobModeName;
                
                await _context.SaveChangesAsync();

                AlertMessage("You Have UPDATED " + model.JobModeName + " Job Mode for Roles Successfully!!!..", NotificationType.success);
                return RedirectToAction("ManageJobModes", "CompanyProfile");
            }

            return View(model);
        }


        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        [HttpGet]
        [Route("[controller]/DeleteJobMode/{id}")]
        public async Task<IActionResult> DeleteJobMode(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var jmDel = await _context.JobModes.FirstOrDefaultAsync(jm => jm.Id == id);
            var jmVM = new JobeModeVM()
            {
                Id = jmDel.Id,
                JobModeName = jmDel.JobModeName
            };
            return View(jmVM);
        }



        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteJobMode(int id)
        {
            var jmode = await _context.JobModes.FindAsync(id);
            _context.JobModes.Remove(jmode);
            await _context.SaveChangesAsync();

            AlertMessage("You Have DELETED " + jmode.JobModeName + " Job Mode for Roles Successfully!!!..", NotificationType.success);
            return RedirectToAction("ManageJobModes", "CompanyProfile");
        }




        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        [HttpPost]
        //[ValidateAntiForgeryToken]
        //[Route("[controller]/JobModes")]
        public async Task<IActionResult> JobModes(JobeModeVM model)
        {
            if (ModelState.IsValid)
            {
                JobMode jobMode = new JobMode();
                jobMode.Id = model.Id;
                jobMode.JobModeName = model.JobModeName;
                _context.JobModes.Add(jobMode);
                await _context.SaveChangesAsync();
                AlertMessage("Success.. Job Mode For Role Added Successfully !!!", NotificationType.success);
            }
            else
            {
                var message = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

                AlertMessage("Oppss.. Something Went Wrong!!!", NotificationType.error);

                return new StatusCodeResult(0);
            }

            return RedirectToAction("JobModes");
        }


        [HttpGet]
        public async Task<IActionResult> ProvinceDemoFileAttachments()
        {
            ProvinceDemoFileAttachmentVM model = new ProvinceDemoFileAttachmentVM();

            model.ProvinceDemoFileAttachments = await _context.ProvinceDemoFileAttachments.Select(df => df).ToListAsync();

            return View(model);
        }       


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProvinceDemoFileAttachments(ProvinceDemoFileAttachmentVM model)
        {
            if (model.Attachment != null)
            {
                // here i will write atachment file to a physical path
                var uniqueFileName = ProvinceDemoFileAttachmentProcess.CretaeUniqueFileExtension(model.Attachment.FileName);
                var uploadDemoFileAttachment = Path.Combine(_hostingEnvironment.WebRootPath, "JobPosts/ProvinceDemoJobMatrixFile");
                var filePath = Path.Combine(uploadDemoFileAttachment, uniqueFileName);
                model.Attachment.CopyTo(new FileStream(filePath, FileMode.Create));

                // now will save the Demo File Attachment to the Database
                ProvinceDemoFileAttachment provinceDemoFileAttachment = new ProvinceDemoFileAttachment();
                provinceDemoFileAttachment.Id = model.Id;
                provinceDemoFileAttachment.FileName = uniqueFileName;
                provinceDemoFileAttachment.ProvinceName = model.ProvinceName;
                provinceDemoFileAttachment.ContentType = model.ContentType;
                provinceDemoFileAttachment.Attachment = ProvinceDemoFileAttachmentProcess.GetByteArrayFromFile(model.Attachment);

                _context.ProvinceDemoFileAttachments.Add(provinceDemoFileAttachment);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("ProvinceDemoFileAttachments");
        }

        [HttpGet("[controller]/EditProvinceDemoFileAttachment/{id}")]
        public async Task<IActionResult> EditProvinceDemoFileAttachment(int? id)
        {
            if (id == null)
            {
                AlertMessage("Oppss.. Province Demo File Attachment Does Not Exist..", NotificationType.error);
                return NotFound(id);
            }

            var provDemoFileAttach = await _context.ProvinceDemoFileAttachments.FindAsync(id);
            var provDemoFileVM = new ProvinceDemoFileAttachmentVM()
            {
                Id = provDemoFileAttach.Id,
                FileName = provDemoFileAttach.FileName,
                ProvinceName = provDemoFileAttach.ProvinceName,
            };
            if (provDemoFileAttach == null)
            {
                AlertMessage("Oppss.. Province Demo File Attachment Does Not Exist..", NotificationType.error);
                return NotFound(id);
            }
            return View(provDemoFileVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProvinceDemoFileAttachment(int id, ProvinceDemoFileAttachmentVM model)
        {
            if (ModelState.IsValid)
            {
                var pvdfVM = await _context.ProvinceDemoFileAttachments.FindAsync(model.Id);
                pvdfVM.ProvinceName = model.ProvinceName;

                if (model.FileName != null)
                {
                    if (model.FileName != null)
                    {
                        string filePath = Path.Combine(_hostingEnvironment.WebRootPath, "JobPosts/ProvinceDemoJobMatrixFile", model.FileName);
                        System.IO.File.Delete(filePath);

                    }
                    pvdfVM.FileName = ProvinceDemoFileAttachmentProcess.CretaeUniqueFileExtension(model.Attachment.FileName);
                    
                }
                await _context.SaveChangesAsync();
                return RedirectToAction("ProvinceDemoFileAttachments", "CompanyProfile");
            }
            

            return View(model);
        }



        [HttpGet]
        public PhysicalFileResult GetFileStreamProvinceDemoFileAttachment(string filename)
        {
            string path = "/wwwroot/JobPosts/ProvinceDemoJobMatrixFile/" + filename;
            string contentType = ProvinceDemoFileAttachmentProcess.GetContentType(filename);
            return new PhysicalFileResult(_hostingEnvironment.ContentRootPath
                + path, contentType);
        }



        [HttpGet]
        public ActionResult GetProvinceDemoFileAttachment(int Id)
        {
            byte[] fileContent;
            string fileName = string.Empty;
            ProvinceDemoFileAttachment provinceDemoFileAttachment = new ProvinceDemoFileAttachment();
            provinceDemoFileAttachment = _context.ProvinceDemoFileAttachments.Select(fa => fa).Where(fa => fa.Id == Id).FirstOrDefault();

            string contentType = ProvinceDemoFileAttachmentProcess.GetContentType(provinceDemoFileAttachment.FileName);
            fileContent = (byte[])provinceDemoFileAttachment.Attachment;

            return new FileContentResult(fileContent, contentType);
        }


        private async Task<string> SaveProvinceDemoFile(IFormFile provinceDemoFile)
        {
            string filePath = "";

            string ext = Path.GetExtension(provinceDemoFile.FileName);
            string name = Path.GetFileNameWithoutExtension(provinceDemoFile.FileName);
            string pathroot = _iweb.WebRootPath;

            try
            {
                if (ext == ".docx" || ext == ".doc" || ext == ".pdf")
                {

                    if (provinceDemoFile.Length == 0)
                        ModelState.AddModelError("ModelError", "please provide valid file");

                    var fileName = (name + "_" + DateTime.Now.ToString("dd_MMM_yyyy_hhmmss") + Path.GetExtension(provinceDemoFile.FileName)).Replace(" ", "_");
                    using (var fileStream = provinceDemoFile.OpenReadStream())
                    using (var ms = new MemoryStream())
                    {
                        await fileStream.CopyToAsync(ms);

                    }
                    // 2) Save file to local path in Resumes folder
                    filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "JobPosts/ProvinceDemoJobMatrixFile", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await provinceDemoFile.CopyToAsync(stream);
                    }

                }

            }
            catch
            {
                //do something
            }

            return filePath;
        }

        //[HttpGet]
        //public IActionResult ViewUsersBySector()
        //{
        //    ViewBag.companyName = _options.Value.AppName;            
        //    var userSectors = new UserSectorsVM
        //    {
        //        Sectors = _context.Sectors.OrderByDescending(v => v.Id).ToList(),
        //        UserSectors = _context.UserSectors.OrderByDescending(a => a.Id).ToList(),                
        //        RegisteredUser = _context.RegisteredUsers.OrderByDescending(x => x.Id).ToList(),
        //        ApplicantProfile = _context.ApplicantProfiles.OrderByDescending(c => c.Id).ToList()
        //    };

        //    //var sects = _context.Sectors.OrderByDescending(v => v.Id).ToList();
        //    //var usects = _context.UserSectors.OrderByDescending(a => a.Id).ToList();
        //    //var regUsers = _context.RegisteredUsers.OrderByDescending(x => x.Id).ToList();
        //    //var appUsers = _context.ApplicantProfiles.OrderByDescending(c => c.Id).ToList();

        //    //var userSectors = new UserSectorsVM()
        //    //{
        //    //        Sectors = sects,
        //    //        UserSectors = usects,
        //    //        RegisteredUser = regUsers,
        //    //        ApplicantProfile = appUsers
        //    //};

        //    return View(userSectors);
        //}


        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        public IActionResult ViewUsersBySector()
        {
          ViewBag.companyName = _options.Value.AppName;

         var result = _context.RegisteredUsers.Join(_context.UserSectors, user => user.Id, userSec => userSec.registeredUser.Id, (user, userSec) => new { user, userSec }).Join(_context.Sectors, CombinedSec => CombinedSec.userSec.SectorId, sect => sect.Id, (CombinedSec, sect) => new { CombinedSec, sect }).Join(_context.ApplicantProfiles, CombinedSecProfile => CombinedSecProfile.CombinedSec.userSec.RegisteredUserId, app => app.RegisteredUserId,(CombinedSecProfile, app) => new { CombinedSecProfile, app }).OrderByDescending(user => user.app.Id).ToList();

            RegisteredUserBySectorVM userSector;
            List<RegisteredUserBySectorVM> userSectorList = new List<RegisteredUserBySectorVM>();
            foreach(var sec in result)
            {
                userSector = new RegisteredUserBySectorVM();
                userSector.SectorId = sec.CombinedSecProfile.sect.Id;
                userSector.SectorName = sec.CombinedSecProfile.sect.SectorName;
                userSector.UserId = sec.CombinedSecProfile.CombinedSec.user.Id;
                userSector.UserLastName = sec.CombinedSecProfile.CombinedSec.user.LastName;
                userSector.UserFirstName = sec.CombinedSecProfile.CombinedSec.user.FirstName;
                userSector.Email = sec.CombinedSecProfile.CombinedSec.user.Email;
                userSector.UserPhone = sec.CombinedSecProfile.CombinedSec.user.UserPhone;
                userSector.ResidencyStatus = sec.CombinedSecProfile.CombinedSec.user.ResidencyStatus;
                userSector.RegistrationDate = sec.app.RegistrationDate;

                userSectorList.Add(userSector);
            }
    
            return View(userSectorList);
        }

        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        public IActionResult ExportUsersListBySectorsToExcellSheet()
        {
           var query = _context.RegisteredUsers.Join(_context.UserSectors, user => user.Id, userSec => userSec.registeredUser.Id, (user, userSec) => new { user, userSec }).Join(_context.Sectors, CombinedSec => CombinedSec.userSec.SectorId, sect => sect.Id, (CombinedSec, sect) => new { CombinedSec, sect }).Join(_context.ApplicantProfiles, CombinedSecProfile => CombinedSecProfile.CombinedSec.userSec.RegisteredUserId, app => app.RegisteredUserId,(CombinedSecProfile, app) => new { CombinedSecProfile, app }).OrderByDescending(user => user.app.Id).ToList();

      
            RegisteredUserBySectorVM userSector;
            List<RegisteredUserBySectorVM> userSectorList = new List<RegisteredUserBySectorVM>();
            foreach(var sec in query)
            {
                userSector = new RegisteredUserBySectorVM();
                userSector.SectorId = sec.CombinedSecProfile.sect.Id;
                userSector.SectorName = sec.CombinedSecProfile.sect.SectorName;
                userSector.UserId = sec.CombinedSecProfile.CombinedSec.user.Id;
                userSector.UserLastName = sec.CombinedSecProfile.CombinedSec.user.LastName;
                userSector.UserFirstName = sec.CombinedSecProfile.CombinedSec.user.FirstName;
                userSector.Email = sec.CombinedSecProfile.CombinedSec.user.Email;
                userSector.UserPhone = sec.CombinedSecProfile.CombinedSec.user.UserPhone;
                userSector.ResidencyStatus = sec.CombinedSecProfile.CombinedSec.user.ResidencyStatus;
                userSector.RegistrationDate = sec.app.RegistrationDate;

                userSectorList.Add(userSector);
            }
             using (var workbooklists = new XLWorkbook())
            {
                var excelsheet = workbooklists.Worksheets.Add("Users");
                var currRow = 1;
                excelsheet.Cell(currRow, 1).Value = "Id";
                excelsheet.Cell(currRow, 2).Value = "First Name";
                excelsheet.Cell(currRow, 3).Value = "Last Name";
                excelsheet.Cell(currRow, 4).Value = "Name";
                excelsheet.Cell(currRow, 5).Value = "Email";
                excelsheet.Cell(currRow, 6).Value = "Phone Number";
                excelsheet.Cell(currRow, 7).Value = "User Sectors";
                excelsheet.Cell(currRow, 8).Value = "Residency Status";
                excelsheet.Cell(currRow, 9).Value = "Registration Date";

                foreach(var user in query)
                {
                    currRow++;
                    excelsheet.Cell(currRow, 1).Value = user.CombinedSecProfile.CombinedSec.userSec.Id;
                    excelsheet.Cell(currRow, 2).Value = user.app.RegisteredUser.FirstName;
                    excelsheet.Cell(currRow, 3).Value = user.app.RegisteredUser.LastName;
                    excelsheet.Cell(currRow, 4).Value = user.app.RegisteredUser.FirstName + " " + user.app.RegisteredUser.LastName;
                    excelsheet.Cell(currRow, 5).Value = user.app.RegisteredUser.Email;
                    excelsheet.Cell(currRow, 6).Value = user.app.RegisteredUser.UserPhone;
                    excelsheet.Cell(currRow, 7).Value = user.CombinedSecProfile.CombinedSec.userSec.Sector.SectorName;
                              
                    excelsheet.Cell(currRow, 8).Value = user.app.RegisteredUser.ResidencyStatus;
                    excelsheet.Cell(currRow, 9).Value = user.app.RegistrationDate.ToString("yyyy-MM-dd");
                }
                using (var stream = new MemoryStream())
                {
                    workbooklists.SaveAs(stream);
                    var userContent = stream.ToArray();
                    return File(userContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "usersBySectorsExcel.xlsx");
                }
            }
        }


        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        public IActionResult ExportUsersBySectorsToCSVFile()
        {
            var query = _context.RegisteredUsers.Join(_context.UserSectors, user => user.Id, userSec => userSec.registeredUser.Id, (user, userSec) => new { user, userSec }).Join(_context.Sectors, CombinedSec => CombinedSec.userSec.SectorId, sect => sect.Id, (CombinedSec, sect) => new { CombinedSec, sect }).Join(_context.ApplicantProfiles, CombinedSecProfile => CombinedSecProfile.CombinedSec.userSec.RegisteredUserId, app => app.RegisteredUserId,(CombinedSecProfile, app) => new { CombinedSecProfile, app }).OrderByDescending(user => user.app.Id).ToList();

            RegisteredUserBySectorVM userSector;
            List<RegisteredUserBySectorVM> userSectorList = new List<RegisteredUserBySectorVM>();
            foreach(var sec in query)
            {
                userSector = new RegisteredUserBySectorVM();
                userSector.SectorId = sec.app.Id;
                userSector.SectorName = sec.CombinedSecProfile.sect.SectorName;
                userSector.UserId = sec.CombinedSecProfile.CombinedSec.user.Id;
                userSector.UserLastName = sec.CombinedSecProfile.CombinedSec.user.LastName;
                userSector.UserFirstName = sec.CombinedSecProfile.CombinedSec.user.FirstName;
                userSector.Email = sec.CombinedSecProfile.CombinedSec.user.Email;
                userSector.UserPhone = sec.CombinedSecProfile.CombinedSec.user.UserPhone;
                userSector.ResidencyStatus = sec.CombinedSecProfile.CombinedSec.user.ResidencyStatus;
                userSector.RegistrationDate = sec.app.RegistrationDate;

                userSectorList.Add(userSector);
            }

            var builder = new StringBuilder();
            builder.AppendLine("Id, First Name, Last Name, Name, Email, Phone Number, User Sectors, Residency Status, Registration Date");

            foreach(var user in query)
            {
               builder.AppendLine($"" +
                    $"{user.app.Id}, " +
                    $"{user.app.RegisteredUser.FirstName}, " +
                    $"{user.app.RegisteredUser.LastName}, " +
                    $"{user.app.RegisteredUser.FirstName}  {user.app.RegisteredUser.LastName}, " +
                    $"{user.app.RegisteredUser.Email}, " +
                    $"{user.app.RegisteredUser.UserPhone}, " +
                    $"{user.CombinedSecProfile.CombinedSec.userSec.Sector.SectorName}," +
                    $"{user.app.RegisteredUser.ResidencyStatus}, " +
                    $"{user.app.RegistrationDate.ToString("yyyy-MM-dd")}"
                    );
            }
            return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", "usersBySectorsCSV.csv");
        }


        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]

        [HttpGet]
        public IActionResult ViewRegisteredUsersList()
        {
            ViewBag.companyName = _options.Value.AppName;

            var user = _context.RegisteredUsers.Include(ap => ap.ApplicantProfile).Include(us => us.UserSectors).ThenInclude(sc => sc.Sector).Include(cp => cp.CompanyProfile).OrderByDescending(u => u.Id);

            return View(user);

        }


        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        public IActionResult ExportUsersListToCSVFile()
        {
            var users = _context.RegisteredUsers.Include(ap => ap.ApplicantProfile).Include(us => us.UserSectors).ThenInclude(sc => sc.Sector).Include(cp => cp.CompanyProfile).OrderByDescending(u => u.Id);

                                                            var builder = new StringBuilder();
            builder.AppendLine("Id, FirstName, LastName, Name, Email, Phone Number, User Sectors, Residency Status, Registration Date");

            foreach(var user in users.Where(ap => ap.ApplicantProfile != null))
            {
               builder.AppendLine($"" +
                    $"{user.Id}, " +
                    $"{user.FirstName}, " +
                    $"{user.LastName}, " +
                    $"{user.FirstName}  {user.LastName}, " +
                    $"{user.Email}, " +
                    $"{user.UserPhone}, " +
                    $"{user.UserSectors}," +
                    $"{user.ResidencyStatus}, " +
                    $"{user.ApplicantProfile.RegistrationDate.ToString("yyyy-MM-dd")}"
                    );
            }
            return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", "users.csv");
        }


        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        public IActionResult ExportUsersListToEXCELLSheet()
        {
            var users = _context.RegisteredUsers.Include(ap => ap.ApplicantProfile).Include(us => us.UserSectors).ThenInclude(sc => sc.Sector).Include(cp => cp.CompanyProfile).OrderByDescending(u => u.Id);


             using (var workbooklists = new XLWorkbook())
            {
                var excelsheet = workbooklists.Worksheets.Add("Users");
                var currRow = 1;
                excelsheet.Cell(currRow, 1).Value = "Id";
                excelsheet.Cell(currRow, 2).Value = "First Name";
                excelsheet.Cell(currRow, 3).Value = "Last Name";
                excelsheet.Cell(currRow, 4).Value = "Name";
                excelsheet.Cell(currRow, 5).Value = "Email";
                excelsheet.Cell(currRow, 6).Value = "Phone Number";
                excelsheet.Cell(currRow, 7).Value = "User Sectors";
                excelsheet.Cell(currRow, 8).Value = "Residency Status";
                excelsheet.Cell(currRow, 9).Value = "Registration Date";

                foreach(var user in users.Where(ap => ap.ApplicantProfile != null))
                {
                    currRow++;
                    excelsheet.Cell(currRow, 1).Value = user.Id;
                    excelsheet.Cell(currRow, 2).Value = user.FirstName;
                    excelsheet.Cell(currRow, 3).Value = user.LastName;
                    excelsheet.Cell(currRow, 4).Value = user.FirstName + " " + user.LastName;
                    excelsheet.Cell(currRow, 5).Value = user.Email;
                    excelsheet.Cell(currRow, 6).Value = user.UserPhone;

                    if (user.UserSectors != null && user.UserSectors.Count > 0)
                    {
                      for (int i = 0; i < user.UserSectors.Count; i++)
                      { 
                        foreach(var sect in user.UserSectors)
                        {
                           excelsheet.Cell(currRow, 7).Value = sect.Sector.SectorName;
                        }
                      }                      
                    }
                              
                    excelsheet.Cell(currRow, 8).Value = user.ResidencyStatus;
                    excelsheet.Cell(currRow, 9).Value = user.ApplicantProfile.RegistrationDate.ToString("yyyy-MM-dd");
                }
                using (var stream = new MemoryStream())
                {
                    workbooklists.SaveAs(stream);
                    var userContent = stream.ToArray();
                    return File(userContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "users.xlsx");
                }
            }
        }


        // ************************************ Hiring Clients / Ministries Logo Section ******************************** //

        // Get all Hiring Client / Ministry Logos
        public async Task<IActionResult> HiringClientMinistryLogos()
        {
            var hcmlogos = await _context.HiringClientLogos.OrderByDescending(hcm => hcm.Id).ToListAsync();
            return View(hcmlogos);
        }


        // Get Method for Adding new Hiring Client Logo
        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        [HttpGet]
        public IActionResult AddHiringClientLogo()
        {
            ViewBag.companyName = _options.Value.AppName;

            return View();
        }


        // Add Client / Ministry new Logo Post Method
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> AddHiringClientLogo([Bind("Id, Client_Name, Client_Province, CMLogo, DateAdded")] HiringClientLogo model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        // Save Client Logo in the wwwroot/images/Hiring-Clients-Logos ToString() + "_" + model.SpeakerPicture.FileName;
        //        string clientlogopath = _iweb.WebRootPath;
        //        string clientlogoname = Path.GetFileNameWithoutExtension(model.CMLogo.FileName);
        //        string extension = Path.GetExtension(model.CMLogo.FileName);
        //        model.Client_Name = clientlogoname + DateTime.Now.ToString("MMMM_d_yyyy");
        //        string path = Path.Combine(clientlogopath + "/images/Hiring-Client-Logos", clientlogoname);

        //        using (var filestream = new FileStream(path, FileMode.Create))
        //        {
        //            await model.CMLogo.CopyToAsync(filestream);
        //        }
        //        // inserting into database
        //        model.DateAdded = DateTime.UtcNow;
        //        _context.Add(model);
        //        await _context.SaveChangesAsync();
        //        AlertMessage("Success!! You have Added new Hiring Client Logo for " + model.Client_Name + " in " + model.Client_Province, NotificationType.success);

        //        return RedirectToAction("HiringClientMinistryLogos", "CompanyProfile");
        //    }
        //    return View(model);
        //}


        // Post method to add new Hiring Client Logo
        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddHiringClientLogo(HiringClientLogoVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string clientlogoname = ProcessHiringClientLogoFileUpload(model);

                    HiringClientLogo logo = new()
                    {
                        Client_Name = model.Client_Name,
                        Client_Province = model.Client_Province,
                        Hiring_Client_Logo = clientlogoname,
                        DateAdded = DateTime.UtcNow
                    };
                    _context.Add(logo);
                    await _context.SaveChangesAsync();
                    AlertMessage("Success!! You have Added new Hiring Client Logo for " + model.Client_Name + " in " + model.Client_Province, NotificationType.success);
                    return RedirectToAction("HiringClientMinistryLogos", "CompanyProfile");
                }
            }
            catch (Exception)
            {
                throw;
            }
            return View(model);
        }





        // Get method to Edit Hiring Client Logo which includes Client Name & Province
        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        [HttpGet("[controller]/EditHiringClientLogo/{id}")]
        public async Task<IActionResult> EditHiringClientLogo(int? id)
        {
            if (id == null)
            {
                AlertMessage("Error!! Hiring Client Logo Does not Exist ", NotificationType.error);
                return NotFound();
            }

            var clientlogo = await _context.HiringClientLogos.FindAsync(id);

            var clientlogovm = new HiringClientLogoVM()
            {
                Id = clientlogo.Id,
                Client_Name = clientlogo.Client_Name,
                Client_Province = clientlogo.Client_Province,
                ExisitingClientLogo = clientlogo.Hiring_Client_Logo,
                DateAdded = clientlogo.DateAdded
            };

            if (clientlogo == null)
            {
                AlertMessage("Error!! Hiring Client Logo Does not Exist ", NotificationType.error);
                return NotFound();
            }
            return View(clientlogovm);
        }

        // Post method to Edit Hiring Client Logo which includes Client Name & Province
        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditHiringClientLogo(int id, HiringClientLogoVM model)
        {
            if (ModelState.IsValid)
            {
                var clientlogo = await _context.HiringClientLogos.FindAsync(model.Id);

                clientlogo.Client_Name = model.Client_Name;
                clientlogo.Client_Province = model.Client_Province;
                clientlogo.DateAdded = DateTime.UtcNow;

                if (model.ClientLogo != null)
                {
                    if (model.ExisitingClientLogo != null)
                    {
                        string logopath = Path.Combine(_iweb.WebRootPath, "Clients-Hiring-Logos", model.ExisitingClientLogo);
                        System.IO.File.Delete(logopath);
                    }
                    clientlogo.Hiring_Client_Logo = ProcessHiringClientLogoFileUpload(model);
                }
                _context.Update(clientlogo);
                await _context.SaveChangesAsync();
                AlertMessage("Success!! You have Updated Hiring Client Logo for " + model.Client_Name + " in " + model.Client_Province, NotificationType.success);
                return RedirectToAction("HiringClientMinistryLogos", "CompanyProfile");
            }
            return View(model);
        }

        // Method to Delete Hiring Client Logo GET METHOD
        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        [HttpGet("[controller]/DeleteHiringClientLogo/{id}")]
        public async Task<IActionResult> DeleteHiringClientLogo(int? id)
        {
            if (id == null)
            {
                AlertMessage("Error!! Hiring Client Logo Does not Exist ", NotificationType.error);
                return NotFound();
            }

            var clientlogo = await _context.HiringClientLogos.FirstOrDefaultAsync(hc => hc.Id == id);

            var clientlogovm = new HiringClientLogoVM()
            {
                Id = clientlogo.Id,
                Client_Name = clientlogo.Client_Name,
                Client_Province = clientlogo.Client_Province,
                DateAdded = clientlogo.DateAdded,
                ExisitingClientLogo = clientlogo.Hiring_Client_Logo
            };

            if (clientlogo == null)
            {
                AlertMessage("Error!! Hiring Client Logo Does not Exist ", NotificationType.error);
                return NotFound();
            }
            return View(clientlogovm);
        }


        // Method to Confirm Delete Hiring Client Logo POST METHOD
        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmDeleteHiringClientLogo(int id)
        {
            var clientlogo = await _context.HiringClientLogos.FindAsync(id);
            string deletelogofromhiringclientfolder = Path.Combine(_iweb.WebRootPath, "Clients-Hiring-Logos");
            var Currclientlogo = Path.Combine(Directory.GetCurrentDirectory(), deletelogofromhiringclientfolder, clientlogo.Hiring_Client_Logo);
            _context.HiringClientLogos.Remove(clientlogo);

            if (System.IO.File.Exists(Currclientlogo))
            {
                System.IO.File.Delete(Currclientlogo);
            }
            await _context.SaveChangesAsync();
            AlertMessage("Success!! Hiring Client Logo Has been DELETED SUCCESSFULLY!!! ", NotificationType.success);
            return RedirectToAction("HiringClientMinistryLogos", "CompanyProfile");
        }



        // Method to check if Hiring Client Logo Image File already exists
        private bool HiringClientLogoExists(int id)
        {
            return _context.HiringClientLogos.Any(hc => hc.Id == id);
        }


        // Process the Hiring Client Logo Image File
        private string ProcessHiringClientLogoFileUpload(HiringClientLogoVM model)
        {
            string clientlogoname = null;
            string path = Path.Combine(_iweb.WebRootPath, "Clients-Hiring-Logos");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            if (model.ClientLogo != null)
            {
                string clientlogouploadfolder = Path.Combine(_iweb.WebRootPath, "Clients-Hiring-Logos");
                clientlogoname = Guid.NewGuid().ToString() + "_" + DateTime.Now.ToString("MMMM_d_yyyy") + "_" + model.ClientLogo.FileName;
                string filepath = Path.Combine(clientlogouploadfolder, clientlogoname);
                using (var filestream = new FileStream(filepath, FileMode.Create))
                {
                    model.ClientLogo.CopyTo(filestream);
                }
            }
            return clientlogoname;
        }


        // ************************************ Hiring Clients / Ministries Logo Section ******************************** //



        // ************************************ Enrolled Cadiddate Count Section ******************************** //

        [HttpGet]
        [Route("/Enrolled-Candidate-Count")]
        public async Task<IActionResult> CandidatesEnrolledCount()
        {

            var candidatecount = await _context.EnrolledCandidatesCounts.OrderByDescending(cnd => cnd.Id).Take(1).ToListAsync();

            return View(candidatecount);
        }

        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        [HttpGet]
        [Route("[controller]/Add-Enrolled-Candidate-Number")]
        public IActionResult AddCandidatesEnrolledNumber()
        {
            return View();
        }

        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        [HttpPost]
        [Route("[controller]/Add-Enrolled-Candidate-Number")]
        public async Task<IActionResult> AddCandidatesEnrolledNumber(EnrolledCandidatesCount model)
        {
            model.CurrentNumOfEnrolledCandidates = Convert.ToInt32(model.CurrentNumOfEnrolledCandidates);
            model.DateUpdated = DateTime.UtcNow;

            _context.Add(model);
            await _context.SaveChangesAsync();

            AlertMessage("Success!! You have Add New Total Number of Enrolled Candidates ", NotificationType.success);

            return RedirectToAction("CandidatesEnrolledCount", "CompanyProfile");
        }


        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        [HttpGet("[controller]/UpdateEnrolledCandidatesCount/{id}")]
        public async Task<IActionResult> UpdateEnrolledCandidatesCount(int? id)
        {
            if (id == null)
            {
                AlertMessage("Error!! Something Went Wrong. Try Again Later!! ", NotificationType.error);
                return NotFound();
            }

            var currenrolledcandidatecount = await _context.EnrolledCandidatesCounts.FindAsync(id);

            var updatedcount = new EnrolledCandidatesCountVM()
            {
                Id = currenrolledcandidatecount.Id,
                CurrentNumOfEnrolledCandidates = currenrolledcandidatecount.CurrentNumOfEnrolledCandidates,
                DateUpdated = DateTime.UtcNow
            };

            if (updatedcount == null)
            {
                AlertMessage("Error!! Something Went Wrong. Try Again Later!! ", NotificationType.error);
                return NotFound();
            }
            return View(updatedcount);
        }


        [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateEnrolledCandidatesCount(int? id, EnrolledCandidatesCount model)
        {
            if (ModelState.IsValid)
            {
                var updatecount = await _context.EnrolledCandidatesCounts.FindAsync(id);
                _context.Update(model);
                await _context.SaveChangesAsync();
                AlertMessage("Success!! You have Updated Enrolled Candidate Number ", NotificationType.success);
                return RedirectToAction("CandidatesEnrolledCount", "CompanyProfile");
            }


            return View(model);
        }
        // ************************************ Enrolled Cadiddate Count Section ******************************** //

    }
}

