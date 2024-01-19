using DNTCaptcha.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SMSS.Data;
using SMSS.Models;
using SMSS.ViewModels;
using SMSS.WebSecurity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using SMSS.Services;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;
using System.Linq.Dynamic.Core;
using SMSS.Migrations;
using MailKit.Net.Smtp;
using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;
using System.Web.Helpers;
using MailKit.Security;
using MimeKit.Text;
using com.sun.xml.@internal.org.jvnet.mimepull;
using System.Text.Encodings.Web;
using X.PagedList;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Configuration;
using Org.BouncyCastle.Ocsp;
using jdk.@internal.util.xml.impl;
using Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Internal;
using com.sun.xml.@internal.bind.v2.schemagen.xmlschema;

namespace SMSS.Controllers
{
    [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Recruiter , Admin")]
    [AllowAnonymous]

    public class HomeController : BaseController
    {
        private const int pageSize = 3;
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        private readonly UserManager<RegisteredUser> _manager;
        private readonly SignInManager<RegisteredUser> _signInManager;

        private readonly IWebHostEnvironment _iweb;

        private readonly IDNTCaptchaValidatorService _validatorService;
        private readonly DNTCaptchaOptions _captchaOptions;

        private readonly IDataProtector _protector;
        private readonly IDataProtector _coProtector;

        private readonly IOptions<SMSSSmtpClientSettings> _options;

        private readonly ICompositeViewEngine _viewEngine;

        private readonly IWebHostEnvironment _hostingEnvironment;

        private IConfiguration Configuration;

        private readonly UserManager<RegisteredUser> _userManager;


        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext dbContext,
            UserManager<RegisteredUser> manager,
            SignInManager<RegisteredUser> signInManager,
            IWebHostEnvironment iweb,
            IDNTCaptchaValidatorService validatorService,
            IOptions<DNTCaptchaOptions> captchaOptions,
            IDataProtectionProvider dataProtectionProvider,
            JobIdProtectionSettings jobIdProtectionSettings,
            CompanyProfileIdProtectionSettings CompanyProfileIdProtectionSettings,
            IOptions<SMSSSmtpClientSettings> options,
            ICompositeViewEngine viewEngine,
            IWebHostEnvironment hostEnvironment,
            IConfiguration _configuration,
            UserManager<RegisteredUser> userManager
            )
        {
            _logger = logger;
            _context = dbContext;
            _manager = manager;
            _signInManager = signInManager;
            _iweb = iweb;
            _protector = dataProtectionProvider.CreateProtector(jobIdProtectionSettings.ProtectJobIdURL);
            _validatorService = validatorService;
            _captchaOptions = captchaOptions == null ? throw new ArgumentException(nameof(captchaOptions)) : captchaOptions.Value;
            //_protector = dataProtectionProvider.CreateProtector(jobIdProtectionSettings.ProtectJobIdURL);
            _coProtector = dataProtectionProvider.CreateProtector(CompanyProfileIdProtectionSettings.ProtectCompanyIdURL);
            _options = options;
            _viewEngine = viewEngine;
            _hostingEnvironment = hostEnvironment;
            Configuration = _configuration;
            _userManager = userManager;
        }



        [Route("")]

        [AllowAnonymous]
        public IActionResult Index()
        {

            ViewBag.companyName = _options.Value.AppName;


            ViewBag.Description = "Looking for IT/Software related solutions? S M software solutions is the best platform for Job-Seekers & Employers to search from millions of Jobs & Resumes.";


            CompanyTestimonialVM ctVM = new CompanyTestimonialVM();

            //DateTimeOffset datetimezonediff;
            //datetimezonediff = DateTimeOffset.UtcNow;

            List<CompanyJob> companyJobs = _context.CompanyJobs
                .Where(cj => cj.ExpireDate >= DateTime.Now && cj.IsInactive == false)
                .OrderByDescending(cj => cj.PostingDate)
                .Include(cp => cp.Country)
                .Include(cp => cp.Province)
                .Include(cp => cp.City)
                .Include(jm => jm.JobMode)
                .Include(apj => apj.ApplicantJobApplications)
                .Include(cmpy => cmpy.CompanyProfile)
                .ThenInclude(rgusr => rgusr.RegisteredUser)
                .Include(cp => cp.CompanyProfile.RegisteredUser).Skip(3).Take(6).ToList();

            List<ApplicantTestimonial> atVM = _context.ApplicantTestimonials
                                  .Where(cj => cj.IsApprove == true)
                              .Include(ap => ap.ApplicantProfile.RegisteredUser).ToList();


            string csFolder = Path.Combine(_iweb.WebRootPath, "CarouselImages");

            string hcmlogos = Path.Combine(_iweb.WebRootPath, "Hiring-Client-Logos");

            List<CarouselSliderImage> carousels = _context.CarouselSliderImages.OrderByDescending(cs => cs.Id).Take(6).ToList();

            List<HiringClientLogo> hiringClients = _context.HiringClientLogos.ToList();

            List<RegisteredUser> activeusers = _context.RegisteredUsers.ToList();

            var isdatacount = _context.CompanyJobs.Where(cj => cj.ExpireDate >= DateTime.Now && cj.IsInactive == false).ToList();

            if (isdatacount != null)
            {
                ViewBag.ActiveJobCount = isdatacount.Count;
            }
            else
            {
                //
            }

            EnrolledCandidatesCount lastupdatednumber = _context.EnrolledCandidatesCounts.OrderBy(la => la.Id).LastOrDefault<EnrolledCandidatesCount>();

            if (lastupdatednumber != null)
            {
                lastupdatednumber.CurrentNumOfEnrolledCandidates = lastupdatednumber.CurrentNumOfEnrolledCandidates;
            }
            else
            {
                //
            }

            //EnrolledCandidatesCount lastupdatenumber = _context.EnrolledCandidatesCounts.OrderBy(la => la.Id).LastOrDefault<EnrolledCandidatesCount>();
             


            // ctVM.CompanyJobs = companyJobs;

            //foreach (var job in companyJobs)
            //{
            //   var jobId = job.Id.ToString();
            //   job.EncryptedId = _protector.Protect(jobId);
            //}

            ctVM.CompanyJobs = companyJobs;




            foreach (var job in companyJobs)
            {
                var jobId = job.Id.ToString();
                job.EncryptedId = _protector.Protect(jobId);
                var comProfid = job.CompanyProfileId.ToString();
                job.CompanyProfile.RegisteredUser.EncryptCoId = _coProtector.Protect(comProfid);
            }

            if (_signInManager.IsSignedIn(User))
            {
                int userId = int.Parse(_manager.GetUserId(HttpContext.User));
                ApplicantProfile profile = _context.ApplicantProfiles.FirstOrDefault(ap => ap.RegisteredUserId == userId);

                if (profile != null)
                {

                    ViewData["CurrentProfileId"] = profile.Id;
                    foreach (var job in companyJobs)
                    {
                        var jobId = job.Id.ToString();
                        job.EncryptedId = _protector.Protect(jobId);
                        var comProfid = job.CompanyProfileId.ToString();
                        job.CompanyProfile.RegisteredUser.EncryptCoId = _coProtector.Protect(comProfid);
                    }
                }
                else
                {

                }

            }
            ctVM.ApplicantTestimonial = atVM;
            ctVM.CarouselSliderImages = carousels;
            ctVM.HiringClientLogos = hiringClients;
            ctVM.EnrolledCandidatesCount = lastupdatednumber;
            ctVM.RegisteredUsers = activeusers;
            return View(ctVM);

        }



        [HttpPost]
        public IActionResult LazyMethod(int? pageNumber)
        {

            var numberOfRecordToskip = pageNumber * pageSize;
            var Companyjobs = _context.CompanyJobs
                .Where(cj => cj.ExpireDate >= DateTime.Now && cj.IsInactive == false)
                .OrderByDescending(cj => cj.PostingDate)
                .Include(cp => cp.Country)
                .Include(cp => cp.Province)
                .Include(cp => cp.City)
                .Include(jm => jm.JobMode)
                .Include(apj => apj.ApplicantJobApplications)
                .Include(cmpy => cmpy.CompanyProfile)
                .ThenInclude(rgusr => rgusr.RegisteredUser)
                .Skip(Convert.ToInt32(numberOfRecordToskip)).Skip(3).Take(3).ToList<CompanyJob>();

            ViewBag.data = Companyjobs.Count();
            Thread.Sleep(500);

            foreach (var job in Companyjobs)
            {
                var jobId = job.Id.ToString();
                job.EncryptedId = _protector.Protect(jobId);
                var comProfid = job.CompanyProfileId.ToString();
                job.CompanyProfile.RegisteredUser.EncryptCoId = _coProtector.Protect(comProfid);
            }

            if (_signInManager.IsSignedIn(User))
            {
                int userId = int.Parse(_manager.GetUserId(HttpContext.User));
                ApplicantProfile profile = _context.ApplicantProfiles.FirstOrDefault(ap => ap.RegisteredUserId == userId);
                if (profile != null)
                {
                    ViewData["CurrentProfileId"] = profile.Id;
                    foreach (var job in Companyjobs)
                    {
                        var jobId = job.Id.ToString();
                        job.EncryptedId = _protector.Protect(jobId);
                        var comProfid = job.CompanyProfileId.ToString();
                        job.CompanyProfile.RegisteredUser.EncryptCoId = _coProtector.Protect(comProfid);
                    }
                }
                else
                {

                }
            }

            return PartialView("_LazyJobs", Companyjobs);
        }


        // Trying new Pagination Method

        //public IActionResult BrowseJobsPagination(string sortOrder, int pageNum = 1, int pageSize = 1)
        //{
        //    ViewBag.pageSize = pageSize; // this will be the key point in the pagination
        //    ViewBag.CurrentSortOrder = sortOrder;
        //    ViewBag.nameSortParam = String.IsNullOrEmpty(sortOrder) ? "job_page" : "";
        //}


        [HttpPost]
        public IActionResult BrowseJobsLazyMethod(int? pageNumber)
        {

            //DateTimeOffset datetimezonediff;

            //datetimezonediff = DateTimeOffset.UtcNow;

            var numberOfRecordToskip = pageNumber * pageSize;
            var Companyjobs = _context.CompanyJobs
                .Where(cj => cj.ExpireDate >= DateTime.Now && cj.IsInactive == false)
                .OrderByDescending(cj => cj.PostingDate)
                .Include(cp => cp.Country)
                .Include(cp => cp.Province)
                .Include(cp => cp.City)
                .Include(jm => jm.JobMode)
                .Include(apj => apj.ApplicantJobApplications)
                .Include(cmpy => cmpy.CompanyProfile)
                .ThenInclude(rgusr => rgusr.RegisteredUser)
                .Include(cp => cp.CompanyProfile.RegisteredUser)
                .Skip(Convert.ToInt32(numberOfRecordToskip)).Take(3).ToList<CompanyJob>();

            ViewBag.data = Companyjobs.Count();
            Thread.Sleep(800);

            foreach (var job in Companyjobs)
            {
                var jobId = job.Id.ToString();
                job.EncryptedId = _protector.Protect(jobId);
                var comProfid = job.CompanyProfileId.ToString();
                job.CompanyProfile.RegisteredUser.EncryptCoId = _coProtector.Protect(comProfid);
            }

            if (_signInManager.IsSignedIn(User))
            {
                int userId = int.Parse(_manager.GetUserId(HttpContext.User));
                ApplicantProfile profile = _context.ApplicantProfiles.FirstOrDefault(ap => ap.RegisteredUserId == userId);
                if (profile != null)
                {
                    ViewData["CurrentProfileId"] = profile.Id;
                    foreach (var job in Companyjobs)
                    {
                        var jobId = job.Id.ToString();
                        job.EncryptedId = _protector.Protect(jobId);
                        var comProfid = job.CompanyProfileId.ToString();
                        job.CompanyProfile.RegisteredUser.EncryptCoId = _coProtector.Protect(comProfid);
                    }
                }
                else
                {

                }
            }

            return PartialView("_AllJobs", Companyjobs);
        }

        [HttpGet]
        public PartialViewResult LoadMoreJobsPartial()
        {

            //DateTimeOffset datetimezonediff;

            //datetimezonediff = DateTimeOffset.UtcNow;

            CompanyTestimonialVM ctVM = new CompanyTestimonialVM();


            List<CompanyJob> companyJobs = _context.CompanyJobs
                .Where(cj => cj.ExpireDate >= DateTime.Now && cj.IsInactive == false)
                .OrderByDescending(cj => cj.PostingDate)
                .Include(cp => cp.Country)
                .Include(cp => cp.Province)
                .Include(cp => cp.City)
                .Include(jm => jm.JobMode)
                .Include(apj => apj.ApplicantJobApplications)
                .Include(cmpy => cmpy.CompanyProfile)
                .ThenInclude(rgusr => rgusr.RegisteredUser)
                .Include(cp => cp.CompanyProfile.RegisteredUser).Skip(3).Take(6)
                .ToList();

            List<ApplicantTestimonial> atVM = _context.ApplicantTestimonials
                                  .Where(cj => cj.IsApprove == true)
                              .Include(ap => ap.ApplicantProfile.RegisteredUser).ToList();


            string csFolder = Path.Combine(_iweb.WebRootPath, "CarouselImages");

            List<CarouselSliderImage> carousels = _context.CarouselSliderImages.OrderByDescending(cs => cs.Id).Take(6).ToList();

            ctVM.CompanyJobs = companyJobs;

            foreach (var job in companyJobs)
            {
                var jobId = job.Id.ToString();
                job.EncryptedId = _protector.Protect(jobId);
                var comProfid = job.CompanyProfileId.ToString();
                job.CompanyProfile.RegisteredUser.EncryptCoId = _coProtector.Protect(comProfid);
            }

            if (_signInManager.IsSignedIn(User))
            {
                int userId = int.Parse(_manager.GetUserId(HttpContext.User));
                ApplicantProfile profile = _context.ApplicantProfiles.FirstOrDefault(ap => ap.RegisteredUserId == userId);

                if (profile != null)
                {
                    ViewData["CurrentProfileId"] = profile.Id;
                    foreach (var job in companyJobs)
                    {
                        var jobId = job.Id.ToString();
                        job.EncryptedId = _protector.Protect(jobId);
                        var comProfid = job.CompanyProfileId.ToString();
                        job.CompanyProfile.RegisteredUser.EncryptCoId = _coProtector.Protect(comProfid);
                    }
                }
                else
                {

                }

            }
            return PartialView("_LoadMoreJobsPartial", ctVM);
        }




        [Route("/Privacy")]
        public IActionResult Privacy()
        {
            ViewBag.companyName = _options.Value.AppName;

            ViewBag.Keywords = "Privacy policy SMSS, Our Privacy Policy";
            ViewBag.Description = "We only collect information about you which will help us determine your needs better. Your information is safe with us. ";

            return View();
        }

        [Route("/About-Us")]
        public IActionResult Aboutus()
        {
            ViewBag.companyName = _options.Value.AppName;

            ViewBag.Keywords = "S M Soft Consulting, S M Soft Solutions";
            ViewBag.Description = "S M Soft Solutions is the right place for all your IT Related Challenges. SMSS is one of the top Job listing website suitable for your Resume/CV.";

            return View();
        }

        [Route("/Talent-Acquisition")]
        public IActionResult Talent()
        {
            ViewBag.companyName = _options.Value.AppName;

            ViewBag.Keywords = "Staffing Company, Staffing Companies";
            ViewBag.Description = "At SMSS, We Monitor the Job Market, Understand your needs and help find Companies Qualified Candidates from our Global Talent Pool.";

            return View();
        }


        [HttpGet]
        [Route("/Contact-SMSS")]
        public IActionResult Contactus()
        {
            ViewBag.companyName = _options.Value.AppName;
            ViewBag.Keywords = "S M Software Solutions Inc, Staffing Company Near Me";
            ViewBag.Description = "Send us a message anytime by filling the short form or you can get in touch with us via Facebook, LinkedIn, Twitter or Instagram.";

            return View();
        }



        //[HttpPost]
        //public async Task<IActionResult> Contactus(ContactusModel vm)
        //{
        //    ViewBag.companyName = _options.Value.AppName;

        //    ViewBag.Keywords = "S M Software Solutions Inc, Staffing Company Near Me";
        //    ViewBag.Description = "Send us a message anytime by filling the short form or you can get in touch with us via Facebook, LinkedIn, Twitter or Instagram.";

        //    if (!_validatorService.HasRequestValidCaptchaEntry(Language.English, DisplayMode.ShowDigits))
        //    {
        //        this.ModelState.AddModelError(_captchaOptions.CaptchaComponent.CaptchaInputName, "Please Enter Captcha Correctly.");
        //        // return View("Index");
        //        return View();

        //    }
        //    else
        //    {

        //        using (MailMessage mailMessage = new MailMessage())
        //        {
        //            mailMessage.From = new MailAddress(vm.EmailId);
        //            mailMessage.Subject = vm.Subject;
        //            mailMessage.Body = String.Format(@"Hello Admin,
        //            {0} has a message for you. If you can answer it.
        //            “", vm.Name) + vm.Message + "”" + System.Environment.NewLine + String.Format(@"
        //            The detail of the client is
        //            User name: {0}
        //            Email:{1}
        //            Phone:{2}
        //            Thanks
        //            SMSS Team", vm.Name, vm.EmailId, vm.PhoneNo);

        //            mailMessage.IsBodyHtml = false;
        //            mailMessage.To.Add(new MailAddress(_options.Value.FromEmail));
        //            System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient();
        //            smtp.UseDefaultCredentials = false;

        //            smtp.Host = _options.Value.Host;
        //            smtp.EnableSsl = true;

        //            System.Net.NetworkCredential networkcred = new System.Net.NetworkCredential();
        //            networkcred.UserName = _options.Value.Username;
        //            networkcred.Password = _options.Value.Password;
        //            smtp.Credentials = networkcred;

        //            smtp.Port = _options.Value.Port;
        //            await smtp.SendMailAsync(mailMessage);

        //            mailMessage.From = new MailAddress(_options.Value.FromEmail);
        //            mailMessage.Subject = "SMSS Auto Reply";
        //            mailMessage.Body = String.Format(@"Dear {0},
        //            We have received your request on the {1}. Our team will get back to you with answers as soon as possible. 
        //            We understand that this is an important query, and any delay would affect you.
        //            Hope you understand, and we will continue our collaboration smoothly.
        //            Regards,
        //            SMSS TEAM",
        //            vm.Name, DateTime.Now.Date.ToString("d"));
        //            mailMessage.To.Clear();
        //            mailMessage.To.Add(new MailAddress(vm.EmailId));
        //            await smtp.SendMailAsync(mailMessage);
        //        }
        //        ModelState.Clear();

        //        return View();

        //        //  ViewBag.Message = "Thank you for contacting us.";
        //    }
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/Contact-SMSS")]
        public IActionResult Contactus([Bind("Id, Name, EmailId, PhoneNo, Subject, Message")] ContactusModel model)
        {
            if (ModelState.IsValid)
            {
                InternetAddressList list = new InternetAddressList();
                list.Add(new MailboxAddress("S M Software Solutions Inc.", "info@smsoftconsulting.com"));
                list.Add(new MailboxAddress("S M Software Solutions Inc.", "hr@smsoftconsulting.com"));

                var toemails = new MimeMessage();
                toemails.From.Add(new MailboxAddress("S M Software Solutions Inc.", "info@smsoftconsulting.com"));
                toemails.To.AddRange(list);
                toemails.Subject = "Contact Us Form Submission!!";

                BodyBuilder mailbody = new BodyBuilder();

                mailbody.HtmlBody = "New User by Name: <b>" + model.Name + ", </b> <br>" +
                $" " + "Submitted an inquiry on our Contact Us Form Page" + "<br>" +
                $" " + "Below are his/her details:" + " " + "<br/>" +

                $" " + "Name: " + model.Name + "<br>" +
                $" " + "Email: " + model.EmailId + "<br>" +
                $" " + "Phone Number: " + model.PhoneNo + "<br>" +
                $" " + "Subject: <b>" + model.Subject + "</b><br>" +
                $" " + "Message: <br></br><i>" + model.Message + "</i><br>" +
                "</b> <br>";

                mailbody.Attachments.Add(_iweb.WebRootPath + "\\images\\" + "\\logo\\smsslogo.PNG");

                toemails.Body = mailbody.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    client.Connect("smtp.gmail.com", 465, true);
                    client.Authenticate("info@smsoftconsulting.com", "smsoftconsulting@123");
                    client.Send(toemails);
                    client.Disconnect(true);
                    client.Dispose();
                }
                AlertMessage("Hi " + model.Name + ", Your Message has been Sent Successfully!.. <br> We will respond to you within the next 24 hours.", NotificationType.success);
                return Redirect("/Contact-SMSS");

            }
            return View(model);
        }


        [Route("/Services")]
        public IActionResult Services()
        {
            ViewBag.companyName = _options.Value.AppName;
            ViewBag.Keywords = "Networking Services, Recruitment Services";
            ViewBag.Description = "SMSS provides a variety of IT solutions & consulting services including data analytics, networking, software development, Cloud & backup management services.";

            return View();
        }


        [Route("/Data-Analytics")]
        public IActionResult Data_Analytics()
        {
            ViewBag.companyName = _options.Value.AppName;
            ViewBag.Keywords = "Data and Analytics Services, SMSS Data Analytics";
            ViewBag.Description = "Data analytics services provided by us can help you to unlock powerful analytic insights and understand data mining goals and enhance productivity.";

            return View();
        }


        [Route("/Networking-Solutions")]
        public IActionResult Networking_Solutions()
        {
            ViewBag.companyName = _options.Value.AppName;
            ViewBag.Keyword = "Networking Services, Network Marketing Company";
            ViewBag.Description = "SMSS is one of the top  network marketing companies. Our professional team would take care of your networking needs to manage and exchange data securely.";

            return View();
        }

        [Route("/Recruitment-Process")]
        public IActionResult Recruitment_Process()
        {
            ViewBag.companyName = _options.Value.AppName;
            ViewBag.Keywords = "Recruitment Agencies, Job Agencies Near Me";
            ViewBag.Description = "We are amongst the top recruiting firms in Canada & USA. Looking for permanent/contractual employees? You will find your next potential candidate here.";

            return View();
        }


        [Route("/IT-Infrastructure-And-Cloud-Services")]
        public IActionResult IT_Infrastructure_Cloud()
        {
            ViewBag.companyName = _options.Value.AppName;

            ViewBag.Keywords = "Secure Cloud Data Environment, Cloud Migration Services";
            ViewBag.Description = "Searching for affordable cloud services? IT infrastructure and secured cloud services by SMSS will give your workplace a complete transformation.";

            return View();
        }


        [Route("/Backup-Management")]
        public IActionResult Backup_Management()
        {
            ViewBag.companyName = _options.Value.AppName;

            ViewBag.Keywords = "Backup Setup, Backup Software";
            ViewBag.Description = "Searching for customized data protection solutions? At SMSS, we will provide you summary, reports & updates regarding backup to minimize the risk of data loss.";

            return View();
        }


        [Route("/Software-Development")]
        public IActionResult Software_Development()
        {

            ViewBag.Keywords = "AWS DevOps team SMSS, Customized Web and Mobile Application Services";
            ViewBag.Description = "Looking for outstanding software solutions? At SMSS, we provide customized web & mobile application services across the Globe at prices that are in your budget.";

            return View();
        }


        [Route("/Jobs")]
        public async Task<IActionResult> Jobs(int? pageNumber, int pageSize = 10, string SearchString = "", int ProvinceId = 0, int CountryId = 0)
        {
            ViewBag.companyName = _options.Value.AppName;


            ViewBag.Keywords = "IT Job Search, Job Listings";
            ViewBag.Description = "Looking forward to finding your first IT job or searching for a job that matches your experience and skill level? S M Software Solutions is the right place for you.";

            var numberOfRecordToskip = pageNumber * pageSize;

            if ((SearchString == null || SearchString == "") && CountryId == 0 && ProvinceId == 0)
            {
                var alljobs = _context.CompanyJobs
                    .Where(cj => cj.ExpireDate >= DateTime.Now && cj.IsInactive == false)
                    .OrderByDescending(cj => cj.PostingDate)
                    .Include(cj => cj.Country)
                    .Include(cj => cj.Province)
                    .Include(cj => cj.City)
                    .Include(jm => jm.JobMode)
                    .Include(apj => apj.ApplicantJobApplications)
                    .Include(cj => cj.CompanyProfile)
                    .ThenInclude(cp => cp.RegisteredUser);

                List<SectorJobCount> sectorJobCounts = _context.SectorJobCounts.OrderByDescending(sc => sc.JobCount).ToList();
                ViewData["Sectors"] = sectorJobCounts;

                List<SelectListItem> countries = GetCountryList();
                ViewBag.Countrylist = countries;

                foreach (var job in alljobs)
                {
                    var jobId = job.Id.ToString();
                    job.EncryptedId = _protector.Protect(jobId);
                    var comProfid = job.CompanyProfileId.ToString();
                    job.CompanyProfile.RegisteredUser.EncryptCoId = _coProtector.Protect(comProfid);
                }

                if (_signInManager.IsSignedIn(User))
                {
                    int userId = int.Parse(_manager.GetUserId(HttpContext.User));
                    ApplicantProfile profile = _context.ApplicantProfiles.FirstOrDefault(ap => ap.RegisteredUserId == userId);

                    if (profile != null)
                    {
                        ViewData["CurrentProfileId"] = profile.Id;
                        foreach (var job in alljobs)
                        {
                            var jobId = job.Id.ToString();
                            job.EncryptedId = _protector.Protect(jobId);
                            var comProfid = job.CompanyProfileId.ToString();
                            job.CompanyProfile.RegisteredUser.EncryptCoId = _coProtector.Protect(comProfid);
                        }
                    }
                    else
                    {

                    }

                }
                return View(await PaginatedList<CompanyJob>.CreateAsync(alljobs, pageNumber ?? 1, pageSize));

            }
            else
            {
                    JobListVM jobList = new JobListVM();
                    var alljobs = _context.CompanyJobs
                    .Where(cj => cj.ExpireDate >= DateTime.Now && cj.IsInactive == false).Where(c => c.JobTitle == SearchString)
                    .Where(cn => cn.CountryId == CountryId).Where(pv => pv.ProvinceId == ProvinceId)
                    .OrderByDescending(cj => cj.PostingDate)
                    .Include(cj => cj.Country)
                    .Include(cj => cj.Province)
                    .Include(cj => cj.City)
                    .Include(jm => jm.JobMode)
                    .Include(cj => cj.CompanyProfile)
                    .ThenInclude(cp => cp.RegisteredUser);

                List<SectorJobCount> sectorJobCounts = _context.SectorJobCounts.OrderByDescending(sc => sc.JobCount).ToList();
                ViewData["Sectors"] = sectorJobCounts;

                List<SelectListItem> countries = GetCountryList();
                ViewBag.Countrylist = countries;

                foreach (var job in alljobs)
                {
                    var jobId = job.Id.ToString();
                    job.EncryptedId = _protector.Protect(jobId);
                    var comProfid = job.CompanyProfileId.ToString();
                    job.CompanyProfile.RegisteredUser.EncryptCoId = _coProtector.Protect(comProfid);
                }

                if (_signInManager.IsSignedIn(User))
                {
                    int userId = int.Parse(_manager.GetUserId(HttpContext.User));
                    ApplicantProfile profile = _context.ApplicantProfiles.FirstOrDefault(ap => ap.RegisteredUserId == userId);

                    if (profile != null)
                    {
                        ViewData["CurrentProfileId"] = profile.Id;
                        foreach (var job in alljobs)
                        {
                            var jobId = job.Id.ToString();
                            job.EncryptedId = _protector.Protect(jobId);
                            var comProfid = job.CompanyProfileId.ToString();
                            job.CompanyProfile.RegisteredUser.EncryptCoId = _coProtector.Protect(comProfid);
                        }
                    }
                    else
                    {

                    }

                }


                return View(await PaginatedList<CompanyJob>.CreateAsync(alljobs, pageNumber ?? 1, pageSize));


            }

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
            provinces.Add(new SelectListItem() { Text = "Select Province", Value = "0" });

            foreach (Province province in dbProvinces)
            {
                provinces.Add(new SelectListItem { Value = province.Id.ToString(), Text = province.ProvinceName });
            }


            return provinces;
        }



        [Route("/Terms-And-Conditions")]
        public IActionResult TermsAndConditions()
        {
            ViewBag.companyName = _options.Value.AppName;
            ViewBag.Keywords = "SMSS Terms and Conditions, Our Terms and Conditions";
            ViewBag.Description = "Welcome to S M Software Solutions. Please read our terms and conditions carefully.";

            return View();
        }

        [Route("/Why-Us")]
        public IActionResult WhyUs()
        {
            ViewBag.Keywords = "Recruitment Services, Recruitment Services Near Me";
            ViewBag.Description = "We are a Diverse IT Solutions Provider That Assures to Meet the Needs of Job-Seekers and Employers, a well as Provide Custom Colutions to help you Achieve Your Goals.";


            return View();
        }


        [Route("/Our-Vision")]
        public IActionResult Vision()
        {
            ViewBag.companyName = _options.Value.AppName;
            ViewBag.Keywords = "Recruitment Firm, Recruitment Firms Near Me";
            ViewBag.Description = "Looking For Unbiased Advice or Cost-Effective Delivery? SMSS is a Recruitment Firm DDedicated to help you find a Better Career.";


            return View();
        }

        [Route("/SMSS-Employer")]
        public IActionResult Employer()
        {
            ViewBag.companyName = _options.Value.AppName;
            ViewBag.Keywords = "";
            ViewBag.Description = "";

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Employer(ContactusModel vm)
        {
            ViewBag.companyName = _options.Value.AppName;

            ViewBag.Keywords = "";
            ViewBag.Description = "";

            using (MailMessage mailMessage = new MailMessage())
            {
                mailMessage.From = new MailAddress(vm.EmailId);
                mailMessage.Subject = vm.Subject;
                mailMessage.Body = vm.Message;
                mailMessage.IsBodyHtml = true;
                mailMessage.To.Add(new MailAddress(_options.Value.FromEmail));
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
            ModelState.Clear();
            return View();
        }


        public async Task<PartialViewResult> PartialJobs(int? pageNumber, int pageSize = 10, int[] sector = null, int[] jobExperience = null, int[] jobQualification = null)
        {
            ViewBag.companyName = _options.Value.AppName;

            var numberOfRecordToskip = pageNumber * pageSize;

            JobListVM jobList = new JobListVM();
            IQueryable<CompanyJob> alljobs = _context.CompanyJobs
                .Where(cj => cj.ExpireDate >= DateTime.Now && cj.IsInactive == false)
                .OrderByDescending(cj => cj.PostingDate)
                .Include(cj => cj.Country)
                .Include(cj => cj.Province)
                .Include(cj => cj.City)
                .Include(jm => jm.JobMode)
                .Include(cj => cj.CompanyProfile)
                .ThenInclude(cp => cp.RegisteredUser)
                .Include(cj => cj.CompanyJobSectors);

            if (sector.Count() > 0)
            {

                alljobs = alljobs.Where(cj => cj.CompanyJobSectors.Any(js => sector.Contains(js.SectorId)));

            }
            if (jobExperience.Count() > 0)
            {
                alljobs = alljobs.Where(cj => jobExperience.Contains((int)cj.JobExperience));
            }
            if (jobQualification.Count() > 0)
            {
                alljobs = alljobs.Where(cj => jobQualification.Contains((int)cj.JobQualification));
            }

            foreach (var job in alljobs)
            {
                var jobId = job.Id.ToString();
                job.EncryptedId = _protector.Protect(jobId);
                var comProfid = job.CompanyProfileId.ToString();
                job.CompanyProfile.RegisteredUser.EncryptCoId = _coProtector.Protect(comProfid);
            }


            return PartialView("_Jobs", await PaginatedList<CompanyJob>.CreateAsync(alljobs, pageNumber ?? 1, pageSize));

        }



        [Route("/Job_Detail/{slug?}")]
        public IActionResult JobDetails(string id)
        {
            ViewBag.companyName = _options.Value.AppName;

            //  id = 1;
            int jbid = Convert.ToInt32(_protector.Unprotect(id));
            var CompanyJobs = _context.CompanyJobs.Include(cj => cj.Country)
                                                  .Include(cj => cj.Province)
                                                  .Include(cj => cj.City)
                                                  .Include(cj => cj.CompanyProfile)
                                                  .ThenInclude(cp => cp.RegisteredUser)
                                                  .Include(cj => cj.CompanyJobSectors)
                                                  .ThenInclude(cs => cs.Sector)
                                                  .Include(cj => cj.ApplicantJobApplications)
                                                  .Include(jm => jm.JobMode)
                                                  .Include(pv => pv.ProvinceDemoFileAttachment)
                                                  .Where(cp => cp.Id == jbid).FirstOrDefault();

            var jobId = CompanyJobs.Id.ToString();

            CompanyJobs.EncryptedId = _protector.Protect(jobId);

            if (_signInManager.IsSignedIn(User))
            {
                int userId = int.Parse(_manager.GetUserId(HttpContext.User));

                ApplicantProfile profile = _context.ApplicantProfiles.FirstOrDefault(ap => ap.RegisteredUserId == userId);
                if (profile != null)
                {
                    ViewData["CurrProfileId"] = profile.Id;
                    var resumeFile = Path.GetFileName(profile.ResumeLocation);
                    TempData["ResumeFileName"] = resumeFile;
                    var comProfid = CompanyJobs.CompanyProfileId.ToString(); //Added
                    CompanyJobs.CompanyProfile.RegisteredUser.EncryptCoId = _coProtector.Protect(comProfid);//Added
                }
                else
                {
                    var comProfid = CompanyJobs.CompanyProfileId.ToString(); //Added
                    CompanyJobs.CompanyProfile.RegisteredUser.EncryptCoId = _coProtector.Protect(comProfid);//Added
                }

            }
            // Added for non loggedIn user
            else
            {
                var comProfid = CompanyJobs.CompanyProfileId.ToString(); //Added
                CompanyJobs.CompanyProfile.RegisteredUser.EncryptCoId = _coProtector.Protect(comProfid);//Added
            }

            return View(CompanyJobs);
        }


        public string UploadedResusmeFile(IFormFile ResumeLocation)
        {

            //DateTimeOffset datetimezonediff;

            //datetimezonediff = DateTimeOffset.UtcNow;


            string resumeFileName = null;
            string nameResume = "";
            if (ResumeLocation != null)
            {
                nameResume = Path.GetExtension(ResumeLocation.FileName);
                nameResume = Path.GetFileNameWithoutExtension(ResumeLocation.FileName);
                string resumefileUploadsFolder = Path.Combine(_hostingEnvironment.ContentRootPath, "wwwroot\\Resumes");
                string newresfileUploadsFolder = Path.Combine(_hostingEnvironment.ContentRootPath, resumefileUploadsFolder);
                if (!Directory.Exists(newresfileUploadsFolder))
                    Directory.CreateDirectory(newresfileUploadsFolder);
                else
                    resumeFileName = (nameResume + "_" + DateTime.Now.ToString("dd_MMM_yyyy_hhmmss") + Path.GetExtension(ResumeLocation.FileName)).Replace(" ", "_");
                string resFilePath = Path.Combine(resumefileUploadsFolder, resumeFileName);
                using (var fileStream = new FileStream(resFilePath, FileMode.Create))
                {
                    ResumeLocation.CopyTo(fileStream);
                }
                return resumeFileName;
            }
            return null;
        }

        public string UploadedSkillMatrixeFile(IFormFile SkillMatrixfile)
        {

            //DateTimeOffset datetimezonediff;

            //datetimezonediff = DateTimeOffset.UtcNow;

            string skillMatrixFileName = null;
            string nameSkillMatrix = "";
            if (SkillMatrixfile != null)
            {
                nameSkillMatrix = Path.GetExtension(SkillMatrixfile.FileName);
                nameSkillMatrix = Path.GetFileNameWithoutExtension(SkillMatrixfile.FileName);
                string skmfileUploadsFolder = Path.Combine(_hostingEnvironment.ContentRootPath, "wwwroot\\SkillMatrix");
                string newskmfileUploadsFolder = Path.Combine(_hostingEnvironment.ContentRootPath, skmfileUploadsFolder);
                if (!Directory.Exists(newskmfileUploadsFolder))
                    Directory.CreateDirectory(newskmfileUploadsFolder);
                else
                    skillMatrixFileName = (nameSkillMatrix + "_" + DateTime.Now.ToString("dd_MMM_yyyy_hhmmss") + Path.GetExtension(SkillMatrixfile.FileName)).Replace(" ", "_");
                string skmFilePath = Path.Combine(skmfileUploadsFolder, skillMatrixFileName);
                using (var fileStream = new FileStream(skmFilePath, FileMode.Create))
                {
                    SkillMatrixfile.CopyTo(fileStream);
                }
                return skillMatrixFileName;
            }
            return null;
        }



        //[Route("/Job_Detail/{slug?}")]

        //[HttpPost]
        //[RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        //public async Task<IActionResult> SubmitJobApplicationForm(IFormFile Resumefile, IFormFile SkillMatrixfile, string jobid, string JobTitle, string salexp, string ResumeFileIfAlreadyExist)
        //{
        //    ViewBag.companyName = _options.Value.AppName;

        //    int id = Convert.ToInt32(_protector.Unprotect(jobid));

        //    try
        //    {
        //        string appResumeFile = UploadedResusmeFile(Resumefile);
        //        string appSkillMatrixFile = UploadedSkillMatrixeFile(SkillMatrixfile);

        //        ApplicantJobApplication applicantJob = new ApplicantJobApplication();
        //        int userId = int.Parse(_manager.GetUserId(HttpContext.User));
        //        ApplicantProfile applicant = _context.ApplicantProfiles.FirstOrDefault(app => app.RegisteredUserId == userId);



        //        applicantJob.ApplicantProfileId = applicant.Id;
        //        applicantJob.CompanyJobId = id;
        //        applicantJob.ApplicationDate = DateTime.Now;
        //        applicantJob.ResumeLocation = ("\\wwwroot\\" + "\\ApplicantJobApplicationResumes\\" + appResumeFile);
        //        applicantJob.SkillMatrixLocation = ("\\wwwroot" + "\\ApplicantJobApplicationSkillMatrix\\" + appSkillMatrixFile);
        //        applicantJob.ExpectedSalary = salexp;


        //        _context.ApplicantJobApplications.Add(applicantJob);
        //        await _context.SaveChangesAsync();


        //        var Toemailaddress = "";
        //        var regusername = "";
        //        if (_signInManager.IsSignedIn(User))
        //        {
        //            int reguserId = int.Parse(_manager.GetUserId(HttpContext.User));
        //            RegisteredUser reguser = _context.RegisteredUsers.FirstOrDefault(ap => ap.Id == reguserId);
        //            if (reguser != null)
        //            {
        //                Toemailaddress = reguser.Email.ToString();
        //                regusername = reguser.FirstName.ToString() + " " + reguser.LastName.ToString();
        //            }

        //        }

        //        // Send an Email Notification to Applicant after Job Application Submission Success.

        //        MimeMessage mimeMessage = new MimeMessage();
        //        MailboxAddress mailbox = MailboxAddress.Parse(_options.Value.FromEmail);
        //        mimeMessage.From.Add(mailbox);

        //        MailboxAddress to = new MailboxAddress(regusername, Toemailaddress);
        //        mimeMessage.To.Add(to);

        //        mimeMessage.Subject = "SMSS Submitted Job Application : " + JobTitle;

        //        BodyBuilder bodyBuilder = new BodyBuilder();
        //        bodyBuilder.HtmlBody = "Dear <b>" + regusername + ", </b> <br>" +
        //           $"Thank you for submitting your resume and qualifications to SMSS. <br/>" +
        //           $"You submitted an application for: " + JobTitle +
        //           $"<br><br> Thanks, <br> SMSS Support ";
        //        bodyBuilder.Attachments.Add(_iweb.WebRootPath + "\\images\\" + "\\logo\\smsslogo.PNG");

        //        mimeMessage.Body = bodyBuilder.ToMessageBody();

        //        SmtpClient smtpClient = new SmtpClient();
        //        smtpClient.Connect(_options.Value.Host, _options.Value.Port, true);
        //        smtpClient.Authenticate(_options.Value.Username, _options.Value.Password);

        //        smtpClient.SendAsync(mimeMessage).Wait();
        //        smtpClient.Disconnect(true);
        //        smtpClient.Dispose();


        //        //send Email Notification To Employer only after successfuly Applicant Job Application Form Submission
        //        var ToCompanyEmail = "";
        //        CompanyJob companyJob = _context.CompanyJobs
        //            .Include(cj => cj.CompanyProfile)
        //            .ThenInclude(cp => cp.RegisteredUser)
        //            .FirstOrDefault(cj => cj.Id == id);

        //        ToCompanyEmail = companyJob.CompanyProfile.RegisteredUser.Email;

        //        string companyName = companyJob.CompanyProfile.RegisteredUser.OrganizationName;

        //        MimeMessage mimeMsgEmployer = new MimeMessage();
        //        MailboxAddress mailEmployer = MailboxAddress.Parse("hr@smsoftconsulting.com");
        //        mimeMsgEmployer.From.Add(mailEmployer);

        //        MailboxAddress toEmployer = new MailboxAddress(companyName, ToCompanyEmail);
        //        mimeMsgEmployer.To.Add(toEmployer);

        //        mimeMsgEmployer.Subject = "SMSS Submitted Job Application : " + JobTitle;

        //        BodyBuilder bodyBuilderEmployer = new BodyBuilder();
        //        bodyBuilderEmployer.HtmlBody = "Dear <b>" + companyName + ", </b> <br>" +
        //                   $"Thank You for Using SMSS Job Portal. <br/>" +
        //                   "<b> " + regusername + "</b>" + " Submitted an Application for: " + JobTitle +
        //                   $"<br><br> Thanks, <br> SMSS Support ";
        //        bodyBuilderEmployer.Attachments.Add(_iweb.WebRootPath + "\\images\\" + "\\logo\\smsslogo.PNG");

        //        mimeMsgEmployer.Body = bodyBuilderEmployer.ToMessageBody();

        //        SmtpClient smtpEmployer = new SmtpClient();
        //        smtpEmployer.Connect("smtp.gmail.com", 465, true);
        //        smtpEmployer.Authenticate("info@smsoftconsulting.com", "smsoftconsulting@123");

        //        smtpEmployer.SendAsync(mimeMsgEmployer).Wait();
        //        smtpEmployer.Disconnect(true);
        //        smtpEmployer.Dispose();

        //        //Sending Notification Email to the Employer's Additional Email1

        //        var ToEmailOne = "";
        //        var ToEmailTwo = "";

        //        ToEmailOne = companyJob.CompanyProfile.Email1;
        //        ToEmailTwo = companyJob.CompanyProfile.Email2;

        //        MimeMessage mimeMsgOptional = new MimeMessage();
        //        MailboxAddress mailOptional = MailboxAddress.Parse("hr@smsoftconsulting.com");
        //        mimeMsgOptional.From.Add(mailOptional);

        //        MailboxAddress toOptional = new MailboxAddress(companyName, ToEmailOne);
        //        mimeMsgOptional.To.Add(toOptional);

        //        mimeMsgOptional.Subject = "SMSS Submitted Job Application : " + JobTitle;

        //        BodyBuilder bodyBuilderOptional = new BodyBuilder();
        //        bodyBuilderOptional.HtmlBody = "Dear <b>" + companyName + ", </b> <br>" +
        //                   $"Thank You for Using SMSS Job Portal. <br/>" +
        //                   "<b> " + regusername + "</b>" + " Submitted an Application for: " + JobTitle +
        //                   $"<br><br> Thanks, <br> SMSS Support ";
        //        bodyBuilderOptional.Attachments.Add(_iweb.WebRootPath + "\\images\\" + "\\logo\\smsslogo.PNG");

        //        mimeMsgOptional.Body = bodyBuilderOptional.ToMessageBody();

        //        SmtpClient smtpOptional = new SmtpClient();
        //        smtpOptional.Connect("smtp.gmail.com", 465, true);
        //        smtpOptional.Authenticate("info@smsoftconsulting.com", "smsoftconsulting@123");

        //        smtpOptional.SendAsync(mimeMsgOptional).Wait();
        //        smtpOptional.Disconnect(true);
        //        smtpOptional.Dispose();


        //        //Sending Notification Email to the Employer's Additional Email1
        //        MimeMessage mimeMsgOptional1 = new MimeMessage();
        //        MailboxAddress mailOptional1 = MailboxAddress.Parse("hr@smsoftconsulting.com");
        //        mimeMsgOptional1.From.Add(mailOptional1);

        //        MailboxAddress toOptional1 = new MailboxAddress(companyName, ToEmailTwo);
        //        mimeMsgOptional1.To.Add(toOptional1);

        //        mimeMsgOptional1.Subject = "SMSS Submitted Job Application : " + JobTitle;

        //        BodyBuilder bodyBuilderOptional1 = new BodyBuilder();
        //        bodyBuilderOptional1.HtmlBody = "Dear <b>" + companyName + ", </b> <br>" +
        //                   $"Thank You for Using SMSS Job Portal. <br/>" +
        //                   "<b> " + regusername + "</b>" + " Submitted an Application for: " + JobTitle +
        //                   $"<br><br> Thanks, <br> SMSS Support ";
        //        bodyBuilderOptional1.Attachments.Add(_iweb.WebRootPath + "\\images\\" + "\\logo\\smsslogo.PNG");

        //        mimeMsgOptional1.Body = bodyBuilderOptional1.ToMessageBody();

        //        SmtpClient smtpOptional1 = new SmtpClient();
        //        smtpOptional1.Connect("smtp.gmail.com", 465, true);
        //        smtpOptional1.Authenticate("info@smsoftconsulting.com", "smsoftconsulting@123");

        //        smtpOptional1.SendAsync(mimeMsgOptional1).Wait();
        //        smtpOptional1.Disconnect(true);
        //        smtpOptional1.Dispose();


        //        AlertMessage("Job Application Submitted Successfully", NotificationType.success);

        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex.Message);
        //        AlertMessage("Oppss! Something Went Wrong!!! Try Again Later From Try Block ", NotificationType.error);
        //        return RedirectToAction("JobDetails", new { id = jobid });
        //    }

        //    return RedirectToAction("JobDetails", new { id = jobid });
        //}

        [Route("/Job_Detail/{slug?}")]

        [HttpPost]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]

        public async Task<IActionResult> SubmitJobApplicationForm(IFormFile Resumefile, IFormFile SkillMatrixfile, string jobid, string JobTitle, string salexp, string ResumeFileIfAlreadyExist)
        {
            ViewBag.companyName = _options.Value.AppName;

            string extR = "";
            string nameResume = "";
            if (Resumefile != null)
            {
                extR = Path.GetExtension(Resumefile.FileName);
                nameResume = Path.GetFileNameWithoutExtension(Resumefile.FileName);

            }
            else if (Resumefile == null)
            {
                if (ResumeFileIfAlreadyExist != null)
                {
                    nameResume = ResumeFileIfAlreadyExist;
                    extR = Path.GetExtension(nameResume);
                }
                else
                {
                    AlertMessage("Oppss! Something Went Wrong!!! Kindly Upload Your Resume To Apply for This Role", NotificationType.error);
                    return RedirectToAction("JobDetails", new { id = jobid });
                }
            }
            else
            {
                // Check Later
                extR = Path.GetExtension(Resumefile.FileName);
                nameResume = Path.GetFileNameWithoutExtension(Resumefile.FileName);
            }


            string extSkillmatrix = "";
            string nameJobMatrix = "";
            if (SkillMatrixfile != null)
            {
                extSkillmatrix = Path.GetExtension(SkillMatrixfile.FileName);
                nameJobMatrix = Path.GetFileNameWithoutExtension(SkillMatrixfile.FileName);

            }
            else if (SkillMatrixfile == null)
            {
                AlertMessage("Oppss! Something Went Wrong!!! Kindly Provide a Valid JobMatrix File To Apply for This Role", NotificationType.error);

                return RedirectToAction("JobDetails", new { id = jobid });
            }
            else
            {
                extSkillmatrix = Path.GetExtension("");
                nameJobMatrix = Path.GetFileNameWithoutExtension("");

            }


            //int id = jobid;
            int id = Convert.ToInt32(_protector.Unprotect(jobid));
            string pathroot = _iweb.WebRootPath;

            try
            {
                //var filepath = "";
                var filepathResume = "";
                var filepathSkillMatrix = "";
                if (ModelState.IsValid)
                {
                    foreach (var file in Request.Form.Files)
                    {
                        if (file.Length == 0)

                            ModelState.AddModelError("ModelError", "Please Provide a Valid File Format");

                        var fileName = "";
                        if (file.FileName == (Resumefile != null ? Resumefile.FileName : ""))
                        {
                            fileName = (nameResume + "_" + DateTime.Now.ToString("dd_MMM_yyyy_hhmmss") + Path.GetExtension(file.FileName)).Replace(" ", "_");

                        }
                        else if (file.FileName == (SkillMatrixfile != null ? SkillMatrixfile.FileName : ""))
                        {
                            fileName = (nameJobMatrix + "_" + DateTime.Now.ToString("dd_MMM_yyyy_hhmmss") + Path.GetExtension(file.FileName)).Replace(" ", "_");

                        }
                        if (Resumefile != null && file.FileName == Resumefile.FileName)
                        {
                            filepathResume = pathroot + "\\Resumes\\" + fileName;

                        }
                        if (SkillMatrixfile != null && file.FileName == SkillMatrixfile.FileName)
                        {
                            filepathSkillMatrix = pathroot + "\\JobMatrix\\" + fileName;

                        }
                        if (Resumefile != null && file.FileName == Resumefile.FileName)
                        {
                            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Resumes", fileName);
                            using (var stream = new FileStream(path, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }
                        }
                        else if (SkillMatrixfile != null && file.FileName == SkillMatrixfile.FileName)
                        {
                            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "JobMatrix", fileName);
                            using (var stream = new FileStream(path, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }
                        }
                        else
                        {
                            AlertMessage("Oppss! Something Went Wrong!!! There is an Issue with Either Resume or JobMatrix File Upload. Try Again Later...", NotificationType.error);
                        }

                    }

                    // 3) Save path to Database

                    ApplicantJobApplication app = new ApplicantJobApplication();


                    int userId = int.Parse(_manager.GetUserId(HttpContext.User));
                    ApplicantProfile AppProfile = _context.ApplicantProfiles.FirstOrDefault(ap => ap.RegisteredUserId == userId);

                    string appResumeFile = UploadedResusmeFile(Resumefile);
                    string appSkillMatrixFile = UploadedSkillMatrixeFile(SkillMatrixfile);


                    app.ApplicantProfileId = AppProfile.Id;
                    app.CompanyJobId = id;
                    app.ApplicationDate = DateTime.Now;
                    app.SkillMatrixLocation = filepathSkillMatrix;
                    app.ExpectedSalary = salexp;
                    app.CompanyJobId = id;
                    app.ApplicationDate = DateTime.Now;
                    app.ResumeLocation = ("\\wwwroot\\" + "\\ApplicantJobApplicationResumes\\" + appResumeFile);
                    app.SkillMatrixLocation = ("\\wwwroot\\" + "\\ApplicantJobApplicationSkillMatrix\\" + appSkillMatrixFile);

                    if (filepathResume != "")
                    {
                        app.ResumeLocation = filepathResume;

                    }

                    _context.ApplicantJobApplications.Add(app);
                    AlertMessage("Job Application Submitted Successfully", NotificationType.success);
                    await _context.SaveChangesAsync();

                    // Send an Email Notification to Applicant after Job Application Submission Success.

                    var Toemailaddress = "";
                    var regusername = "";
                    var ToCompanyEmail = "";
                    var ToEmailOne = "";
                    var ToEmailTwo = "";


                    CompanyJob companyJob = _context.CompanyJobs
                        .Include(cj => cj.CompanyProfile)
                        .ThenInclude(cp => cp.RegisteredUser)
                        .FirstOrDefault(cj => cj.Id == id);


                    if (_signInManager.IsSignedIn(User))
                    {
                        int reguserId = int.Parse(_manager.GetUserId(HttpContext.User));
                        RegisteredUser reguser = _context.RegisteredUsers.FirstOrDefault(ap => ap.Id == reguserId);
                        if (reguser != null)
                        {
                            Toemailaddress = reguser.Email.ToString();
                            regusername = reguser.FirstName.ToString() + " " + reguser.LastName.ToString();
                        }

                    }


                    ToCompanyEmail = companyJob.CompanyProfile.RegisteredUser.Email;

                    string companyName = companyJob.CompanyProfile.RegisteredUser.OrganizationName;

                    ToEmailOne = companyJob.CompanyProfile.Email1;
                    ToEmailTwo = companyJob.CompanyProfile.Email2;

                    InternetAddressList list = new InternetAddressList();
                    list.Add(new MailboxAddress(companyName, ToCompanyEmail));
                    list.Add(new MailboxAddress(companyName, ToEmailOne));
                    list.Add(new MailboxAddress(companyName, ToEmailTwo));

                    var mail = new MimeMessage();
                    mail.From.Add(new MailboxAddress("S M Software Solutions", "donotreply@smsoftconsulting.com"));
                    mail.To.AddRange(list);
                    mail.Subject = "SMSS Submitted Job Application : " + JobTitle;

                    BodyBuilder mailBodyBuilder = new BodyBuilder();

                    mailBodyBuilder.HtmlBody = "Dear <b>" + companyName + ", </b> <br>" +
                       "<b> " + regusername + "</b>" + " Submitted an Application for: " + JobTitle + "<br><br>" +
                        $" " + regusername + " " + "Email: " + Toemailaddress + " " + "<br>" +
                        $" " + regusername + " " + "Salary Expectation: " + "$" + salexp + " " + "<br>" +
                        $" " + "Job Application Date: " + DateTime.Now + " " + "<br>" +
                       $"<br><br> Thanks, <br> SMSS Support" +
                       $"<br><br>" +
                       $" " + regusername + " " + "Attached Documents are Listed Below:";

                    mailBodyBuilder.Attachments.Add(filepathResume);
                    mailBodyBuilder.Attachments.Add(filepathSkillMatrix);

                    mail.Body = mailBodyBuilder.ToMessageBody();

                    var email = new MimeMessage();
                    email.From.Add(new MailboxAddress("S M Software Solutions", "donotreply@smsoftconsulting.com"));
                    email.To.Add(new MailboxAddress(regusername, Toemailaddress));
                    email.Subject = "SMSS Submitted Job Application : " + JobTitle;

                    BodyBuilder bodyBuilder = new BodyBuilder();

                    bodyBuilder.HtmlBody = "Dear <b>" + regusername + ", </b> <br>" +
                       $"Thank you for submitting your resume and qualifications to SMSS. <br/>" +
                       $"You submitted an application for: " + JobTitle +
                    $"<br><br> Thanks, <br> SMSS Support ";

                    bodyBuilder.Attachments.Add(_iweb.WebRootPath + "\\images\\" + "\\logo\\smsslogo.PNG");

                    email.Body = bodyBuilder.ToMessageBody();


                    using (var client = new SmtpClient())
                    {
                        client.Connect("smtp.gmail.com", 465, true);
                        client.Authenticate("info@smsoftconsulting.com", "smsoftconsulting@123");
                        client.Send(mail);
                        client.Send(email);
                        client.Disconnect(true);
                        client.Dispose();
                    }

                }
                else
                {
                    //ModelState.AddModelError("ModelError", ModelState.FirstOrDefault().Value.Errors.FirstOrDefault().ErrorMessage);
                    AlertMessage("Oppss! Something Went Wrong!!! Try Again Later from Model", NotificationType.error);
                }
            }
            catch (Exception e)
            {
                //throw e;
                AlertMessage("Oppss! Something Went Wrong!!! Try Again Later from Model", NotificationType.error);
            }
            return RedirectToAction("JobDetails", new { id = jobid });
        }


        //[Route("/Error")]
        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        //public IActionResult Error()
        //{
        //    ViewBag.companyName = _options.Value.AppName;
        //    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        //}


        //[Route("/Company-Details")]

        ////public IActionResult CompanyDetails(int id)
        //public IActionResult CompanyDetails(string id)
        //{
        //    ViewBag.companyName = _options.Value.AppName;
        //    int coProfId = Convert.ToInt32(_coProtector.Unprotect(id));

        //    var CompanyProfile = _context.CompanyProfiles.Include(cp => cp.RegisteredUser)
        //        .Include(cp => cp.CompanyLocations)
        //        .ThenInclude(cl => cl.Country)
        //        .Include(cp => cp.CompanyLocations)
        //        .ThenInclude(cl => cl.Province)
        //        .Include(cp => cp.CompanyLocations)
        //        .ThenInclude(cl => cl.City)
        //        .Include(cp => cp.CompanyJobs)
        //        .Where(cp => cp.Id == coProfId).FirstOrDefault();

        //    RegisteredUser currUser = _context.RegisteredUsers.Include(ru => ru.CompanyProfile).FirstOrDefault(ru => ru.Id == coProfId);
        //    int compamyProfileId = currUser.CompanyProfile.Id;

        //    var CompanyJobs = _context.CompanyJobs.Where(cp => cp.CompanyProfileId == compamyProfileId).Include(cj => cj.Country).Include(cj => cj.Province).OrderByDescending(cp => cp.PostingDate);


        //    foreach (var job in CompanyJobs)
        //    {
        //        var jobId = job.Id.ToString();
        //        job.EncryptedId = _protector.Protect(jobId);
        //    }

        //    CompanyProfile.CompanyJobs = CompanyProfile.CompanyJobs;


        //    return View(CompanyProfile);

        //}


        //[Route("/Access-Denied")]
        //public IActionResult AccessDenied()
        //{
        //    ViewBag.companyName = _options.Value.AppName;
        //    return View();
        //}



        public async Task<IActionResult> SearchJobs(int? pageNumber, int pageSize = 25, string SearchString = "", int ProvinceId = 0, int CountryId = 0)
        {
            ViewBag.companyName = _options.Value.AppName;

            //DateTimeOffset datetimezonediff;

            //datetimezonediff = DateTimeOffset.UtcNow;

            if ((SearchString == null || SearchString == "") || CountryId == 0 || ProvinceId == 0)
            {
                var alljobs = _context.CompanyJobs
                    .Where(cj => cj.ExpireDate >= DateTime.Now && cj.IsInactive == false).Where(c => c.JobTitle.Contains(SearchString))
                    .OrderByDescending(cj => cj.PostingDate)
                    .Include(cj => cj.Country)
                    .Include(cj => cj.Province)
                    .Include(apj => apj.ApplicantJobApplications)
                    .Include(cj => cj.City)
                    .Include(jm => jm.JobMode)
                    .Include(cj => cj.CompanyProfile)
                    .ThenInclude(cp => cp.RegisteredUser);

                List<SectorJobCount> sectorJobCounts = _context.SectorJobCounts.OrderByDescending(sc => sc.JobCount).ToList();
                ViewData["Sectors"] = sectorJobCounts;

                List<SelectListItem> countries = GetCountryList();
                ViewBag.Countrylist = countries;

                foreach (var job in alljobs)
                {
                    var jobId = job.Id.ToString();
                    job.EncryptedId = _protector.Protect(jobId);
                    var comProfid = job.CompanyProfileId.ToString();
                    job.CompanyProfile.RegisteredUser.EncryptCoId = _coProtector.Protect(comProfid);
                }

                if (_signInManager.IsSignedIn(User))
                {
                    int userId = int.Parse(_manager.GetUserId(HttpContext.User));
                    ApplicantProfile profile = _context.ApplicantProfiles.FirstOrDefault(ap => ap.RegisteredUserId == userId);

                    if (profile != null)
                    {
                        ViewData["CurrentProfileId"] = profile.Id;
                        foreach (var job in alljobs)
                        {
                            var jobId = job.Id.ToString();
                            job.EncryptedId = _protector.Protect(jobId);
                            var comProfid = job.CompanyProfileId.ToString();
                            job.CompanyProfile.RegisteredUser.EncryptCoId = _coProtector.Protect(comProfid);
                        }
                    }
                    else
                    {

                    }

                }

                return View(await PaginatedList<CompanyJob>.CreateAsync(alljobs, pageNumber ?? 1, pageSize));

            }
            else
            {

                JobListVM jobList = new JobListVM();
                var alljobs = _context.CompanyJobs
                    .Where(cj => cj.ExpireDate >= DateTime.Now && cj.IsInactive == false).Where(c => c.JobTitle.Contains(SearchString))
                    .Where(cn => cn.CountryId == CountryId).Where(pv => pv.ProvinceId == ProvinceId)
                    .OrderByDescending(cj => cj.PostingDate)
                    .Include(cj => cj.Country)
                    .Include(cj => cj.Province)
                    .Include(cj => cj.City)
                    .Include(jm => jm.JobMode)
                    .Include(cj => cj.CompanyProfile)
                    .ThenInclude(cp => cp.RegisteredUser);

                List<SectorJobCount> sectorJobCounts = _context.SectorJobCounts.OrderByDescending(sc => sc.JobCount).ToList();
                ViewData["Sectors"] = sectorJobCounts;

                List<SelectListItem> countries = GetCountryList();
                ViewBag.Countrylist = countries;

                foreach (var job in alljobs)
                {
                    var jobId = job.Id.ToString();
                    job.EncryptedId = _protector.Protect(jobId);
                    var comProfid = job.CompanyProfileId.ToString();
                    job.CompanyProfile.RegisteredUser.EncryptCoId = _coProtector.Protect(comProfid);
                }


                return View(await PaginatedList<CompanyJob>.CreateAsync(alljobs, pageNumber ?? 1, pageSize));
            }

        }

        [HttpGet]
        [Route("/getLatestJobs")]
        public async Task<ActionResult<IEnumerable<CompanyJob>>> GetCompanyJobs()
        {

            //DateTimeOffset datetimezonediff;

            //datetimezonediff = DateTimeOffset.UtcNow;

            List<CompanyJob> companyJobs = await _context.CompanyJobs
                .Where(cj => cj.ExpireDate >= DateTime.Now && cj.IsInactive == false)
                .OrderByDescending(cj => cj.PostingDate)
                .Include(apj => apj.ApplicantJobApplications)
                .Include(cmpy => cmpy.CompanyProfile)
                .ThenInclude(rgusr => rgusr.RegisteredUser)
                .Include(cp => cp.CompanyProfile.RegisteredUser).Take(15)
                .ToListAsync();

            foreach (var job in companyJobs)
            {
                var jobId = job.Id.ToString();
                job.EncryptedId = _protector.Protect(jobId);
                var comProfid = job.CompanyProfileId.ToString();
                job.CompanyProfile.RegisteredUser.EncryptCoId = _coProtector.Protect(comProfid);
            }
            return Json(companyJobs);
        }

        [HttpGet]
        [Route("/achievement")]
        public IActionResult GetAchievement()
        {
            var companyCount = _context.CompanyJobs.Count();
            var registeredCandidates = _context.RegisteredUsers.Count();
            var SectorsCount = _context.Sectors.Count();

            return Json(new
            {
                companyCount,
                registeredCandidates,
                SectorsCount
            });
        }

        [HttpGet]
        [Route("/testimonials")]
        public async Task<ActionResult<IEnumerable<ApplicantTestimonial>>> GetTestimonials()
        {
            List<ApplicantTestimonial> testimonials = await _context.ApplicantTestimonials
                                      .Where(cj => cj.IsApprove == true)
                                  .Include(ap => ap.ApplicantProfile.RegisteredUser).ToListAsync();

            return Json(testimonials);
        }


        [Route("/BrowseJobs")]
        public IActionResult BrowseJobs(int? page, string SearchString = "", int ProvinceId = 0, int CountryId = 0)
        {

            ViewBag.companyName = _options.Value.AppName;

            //DateTimeOffset datetimezonediff;

            //datetimezonediff = DateTimeOffset.UtcNow;


            ViewBag.Keywords = "IT Job Search, Job Listings";
            ViewBag.Description = "Looking forward to finding your first IT job or searching for a job that matches your experience and skill level? S M Software Solutions is the right place for you.";


            if ((SearchString == null || SearchString == "") && CountryId == 0 && ProvinceId == 0)
            {
                JobListVM jobList = new JobListVM();
                var alljobs = _context.CompanyJobs
                    .Where(cj => cj.ExpireDate >= DateTime.Now && cj.IsInactive == false)
                    .OrderByDescending(cj => cj.PostingDate)
                    .Include(cj => cj.Country)
                    .Include(cj => cj.Province)
                    .Include(cj => cj.City)
                    .Include(jm => jm.JobMode)
                    .Include(apj => apj.ApplicantJobApplications)
                    .Include(cj => cj.CompanyProfile)
                    .ThenInclude(cp => cp.RegisteredUser);

                List<SectorJobCount> sectorJobCounts = _context.SectorJobCounts.OrderByDescending(sc => sc.JobCount).ToList();
                ViewData["Sectors"] = sectorJobCounts;

                List<SelectListItem> countries = GetCountryList();
                ViewBag.Countrylist = countries;

                foreach (var job in alljobs)
                {
                    var jobId = job.Id.ToString();
                    job.EncryptedId = _protector.Protect(jobId);
                    var comProfid = job.CompanyProfileId.ToString();
                    job.CompanyProfile.RegisteredUser.EncryptCoId = _coProtector.Protect(comProfid);
                }

                if (_signInManager.IsSignedIn(User))
                {
                    int userId = int.Parse(_manager.GetUserId(HttpContext.User));
                    ApplicantProfile profile = _context.ApplicantProfiles.FirstOrDefault(ap => ap.RegisteredUserId == userId);

                    if (profile != null)
                    {
                        ViewData["CurrentProfileId"] = profile.Id;
                        foreach (var job in alljobs)
                        {
                            var jobId = job.Id.ToString();
                            job.EncryptedId = _protector.Protect(jobId);
                            var comProfid = job.CompanyProfileId.ToString();
                            job.CompanyProfile.RegisteredUser.EncryptCoId = _coProtector.Protect(comProfid);
                        }
                    }
                    else
                    {

                    }

                }

                //if (page < 1)
                //{
                //    page = 1;
                //}


                //int jobCount = alljobs.Count();
                //var pagination = new Pagination(jobCount, page, pageSize);
                //int jobSkip = (page - 1) * pageSize;
                //var jobData = alljobs.Skip(jobSkip).Take(pagination.PageSize).ToList();
                //this.ViewBag.Pagination = pagination;

                var pageSize = 10;


                //return View(alljobs.ToPagedList(page ?? 1, pageSize).ToList());
                return (View(alljobs.ToPagedList(pageNumber: page ?? 1, pageSize)));

            }
            else
            {

                JobListVM jobList = new JobListVM();
                var alljobs = _context.CompanyJobs
                    .Where(cj => cj.ExpireDate >= DateTime.Now && cj.IsInactive == false).Where(c => c.JobTitle == SearchString)
                    .Where(cn => cn.CountryId == CountryId).Where(pv => pv.ProvinceId == ProvinceId)
                    .OrderByDescending(cj => cj.PostingDate)
                    .Include(cj => cj.Country)
                    .Include(cj => cj.Province)
                    .Include(cj => cj.City)
                    .Include(jm => jm.JobMode)
                    .Include(cj => cj.CompanyProfile)
                    .ThenInclude(cp => cp.RegisteredUser);

                List<SectorJobCount> sectorJobCounts = _context.SectorJobCounts.OrderByDescending(sc => sc.JobCount).ToList();
                ViewData["Sectors"] = sectorJobCounts;

                List<SelectListItem> countries = GetCountryList();
                ViewBag.Countrylist = countries;

                foreach (var job in alljobs)
                {
                    var jobId = job.Id.ToString();
                    job.EncryptedId = _protector.Protect(jobId);
                    var comProfid = job.CompanyProfileId.ToString();
                    job.CompanyProfile.RegisteredUser.EncryptCoId = _coProtector.Protect(comProfid);
                }

                if (_signInManager.IsSignedIn(User))
                {
                    int userId = int.Parse(_manager.GetUserId(HttpContext.User));
                    ApplicantProfile profile = _context.ApplicantProfiles.FirstOrDefault(ap => ap.RegisteredUserId == userId);

                    if (profile != null)
                    {
                        ViewData["CurrentProfileId"] = profile.Id;
                        foreach (var job in alljobs)
                        {
                            var jobId = job.Id.ToString();
                            job.EncryptedId = _protector.Protect(jobId);
                            var comProfid = job.CompanyProfileId.ToString();
                            job.CompanyProfile.RegisteredUser.EncryptCoId = _coProtector.Protect(comProfid);
                        }
                    }
                    else
                    {

                    }

                }

                //int jobCount = alljobs.Count();
                //var pagination = new Pagination(jobCount, page, pageSize);
                //int jobSkip = (page - 1) * pageSize;
                //var jobData = alljobs.Skip(jobSkip).Take(pagination.PageSize).ToList();
                //this.ViewBag.Pagination = pagination;


                //return View(jobData);


                int pageSize = 10;
                int pageNumber = (page ?? 1);

                //return View(alljobs.ToPagedList(page ?? 1, pageSize).ToList());
                return (View(alljobs.ToPagedList(pageNumber, pageSize)));
            }

        }


        public IActionResult PartialAllJobs(int? page, int pageSize = 25, int[] sector = null, int[] jobExperience = null, int[] jobQualification = null)
        {
            ViewBag.companyName = _options.Value.AppName;

            //DateTimeOffset datetimezonediff;

            //datetimezonediff = DateTimeOffset.UtcNow;

            JobListVM jobList = new JobListVM();
            IQueryable<CompanyJob> alljobs = _context.CompanyJobs
                .Where(cj => cj.ExpireDate >= DateTime.Now && cj.IsInactive == false)
                .OrderByDescending(cj => cj.PostingDate)
                .Include(cj => cj.Country)
                .Include(cj => cj.Province)
                .Include(cj => cj.City)
                .Include(jm => jm.JobMode)
                .Include(cj => cj.CompanyProfile)
                .ThenInclude(cp => cp.RegisteredUser)
                .Include(cj => cj.CompanyJobSectors);

            if (sector.Count() > 0)
            {

                alljobs = alljobs.Where(cj => cj.CompanyJobSectors.Any(js => sector.Contains(js.SectorId)));

            }
            if (jobExperience.Count() > 0)
            {
                alljobs = alljobs.Where(cj => jobExperience.Contains((int)cj.JobExperience));
            }
            if (jobQualification.Count() > 0)
            {
                alljobs = alljobs.Where(cj => jobQualification.Contains((int)cj.JobQualification));
            }

            foreach (var job in alljobs)
            {
                var jobId = job.Id.ToString();
                job.EncryptedId = _protector.Protect(jobId);
                var comProfid = job.CompanyProfileId.ToString();
                job.CompanyProfile.RegisteredUser.EncryptCoId = _coProtector.Protect(comProfid);
            }

            //return PartialView("_Jobs", await PaginatedList<CompanyJob>.CreateAsync(alljobs, page ?? 1, pageSize));
            return PartialView("_AllJobs");
        }

        //[HttpGet]
        //[Route("/UnsubscribeUserEmailRequest")]
        //public IActionResult UnsubscribeUserEmailRequest()
        //{
        //    return View();
        //}


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[Route("/UnsubscribeUserRequestSubmission")]
        //public async Task<IActionResult> UnsubscribeUserRequestForm(UnsubscribeUserEmailRequest req)
        //{
        //    if (req != null)
        //    {
        //        await _context.UnsubscribeUserEmailRequests.AddAsync(req);
        //        AlertMessage("Hi " + req.FullName + ", Your Unsubscribe Request For " + req.Email + " has been Submitted Successfully!!!..", NotificationType.success);
        //    }
        //    else
        //    {
        //        AlertMessage("Opps!!!.. Feilds cannot be empty!", NotificationType.error);
        //    }
        //    await _context.SaveChangesAsync();

        //    // Send email to hr@smsoftconsulting.com              

        //    var Toinfoemail = "info@smsoftconsulting.com";
        //    var Tohremail = "info@smsoftconsulting.com";
        //    string companyName = "S M Software Solutions Inc.";

        //    InternetAddressList list = new InternetAddressList();
        //    list.Add(new MailboxAddress(companyName, Tohremail));
        //    list.Add(new MailboxAddress(companyName, Toinfoemail));

        //    var toemails = new MimeMessage();
        //    toemails.From.Add(new MailboxAddress(companyName, Toinfoemail));
        //    toemails.To.AddRange(list);
        //    toemails.Subject = "To Unsubscribe Email Request Submitted";

        //    BodyBuilder mailbdyhr = new BodyBuilder();

        //    mailbdyhr.HtmlBody = "<b>" + req.FullName + ", </b> <br>" +
        //    $" " + "Has requested to unsubscribe to our mass mailing service" + "<br>" +
        //    $" " + "Below are his/her details:" + " " + "<br/><br/>" +

        //     $" " + "First Name: " + req.FirstName + "<br>" +
        //    $" " + "Last Name: " + req.LastName + " " + "<br>" +
        //    $" " + "Email: " + "<b>" + req.Email + " " + "</b> <br>";

        //    mailbdyhr.Attachments.Add(_iweb.WebRootPath + "\\images\\" + "\\logo\\smsslogo.PNG");

        //    toemails.Body = mailbdyhr.ToMessageBody();

        //    using (var client = new SmtpClient())
        //    {
        //        client.Connect("smtp.gmail.com", 465, true);
        //        client.Authenticate("info@smsoftconsulting.com", "smsoftconsulting@123");
        //        client.Send(toemails);
        //        client.Disconnect(true);
        //        client.Dispose();
        //    }
        //    //return RedirectToAction(req);
        //    return RedirectToAction("UnsubscribeUserRequestSubmission");
        //}


        //[HttpGet]
        //[Route("/UnsubscribeUserRequestSubmission")]
        //public IActionResult UnsubscribeUserRequestSubmission()
        //{
        //    ViewBag.companyName = _options.Value.AppName;

        //    return View();
        //}

        [HttpGet]
        //[Route("/UnsubscribeUserConfirmation")]
        public IActionResult UnsubscribeUserConfirmation()
        {

            ViewBag.companyName = _options.Value.AppName;

            UnsubscribeUserVM unsubscribe = new UnsubscribeUserVM();

            return View(unsubscribe);
        }

        [HttpGet]
        [Route("UnsubscribeUser")]
        public IActionResult UnsubscribeUser()
        {
            ViewBag.companyName = _options.Value.AppName;
            //Create an object for the CheckBoxList model class
            return View();
        }

       

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitUnsubscribeRequestForm([Bind("Id, FirstName, LastName, Email")] UnsubscribeUser unsubscribe, List<string> Unsubscibereasonslist)
        {
            if (ModelState.IsValid)
            {

                string UnsubscribeReasons = string.Join("<br/>", Unsubscibereasonslist);
                unsubscribe.UnsubscribeReasons = UnsubscribeReasons;

                InternetAddressList list = new InternetAddressList();
                list.Add(new MailboxAddress("S M Software Solutions Inc.", "info@smsoftconsulting.com"));
                list.Add(new MailboxAddress("S M Software Solutions Inc.", "hr@smsoftconsulting.com"));

                var toemails = new MimeMessage();
                toemails.From.Add(new MailboxAddress("S M Software Solutions Inc.", "info@smsoftconsulting.com"));
                toemails.To.AddRange(list);
                toemails.Subject = "To Unsubscribe Email Request Submitted";

                BodyBuilder mailbdyhr = new BodyBuilder();

                mailbdyhr.HtmlBody = "<b>" + unsubscribe.FullName + ", </b> <br>" +
                $" " + "Has requested to unsubscribe to our mass mailing service" + "<br>" +
                $" " + "Below are his/her details:" + " " + "<br/><br/>" +

                 $" " + "First Name: " + unsubscribe.FirstName + "<br>" +
                $" " + "Last Name: " + unsubscribe.LastName + " " + "<br>" +
                $" " + "Email: " + unsubscribe.Email + " " + "<br>" +
                $" " + "Unsubscribe Reason: " + unsubscribe.UnsubscribeReasons + " " + "<br>" +
                "</b> <br>";

                mailbdyhr.Attachments.Add(_iweb.WebRootPath + "\\images\\" + "\\logo\\smsslogo.PNG");

                toemails.Body = mailbdyhr.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    client.Connect("smtp.gmail.com", 465, true);
                    client.Authenticate("info@smsoftconsulting.com", "smsoftconsulting@123");
                    client.Send(toemails);
                    client.Disconnect(true);
                    client.Dispose();
                }
                AlertMessage("Hi " + unsubscribe.FullName + ", Your Unsubscribe Request For " + unsubscribe.Email + " has been Submitted Successfully!..", NotificationType.success);
                return RedirectToAction("UnsubscribeUserConfirmation", "Home");
            }
            return View(unsubscribe);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult VerifySubcribeEmailExists(RegisteredUserBySectorVM model)
        {
            //var users = _context.RegisteredUsers.Where(ru => ru.Email == user.Email).SingleOrDefault();
            if (ModelState.IsValid)
            {
                var user = new RegisteredUser { Email = model.Email };

                if (user.Email == null)
                {
                    AlertMessage("Success! Continue by clicking OK!", NotificationType.success);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    AlertMessage("Email cannot be empty", NotificationType.error);
                    return RedirectToAction("Index", "Home");
                }

                //if (emailExist. != null)
                //{
                //    AlertMessage("Email cannot be empty", NotificationType.error);
                //    return RedirectToAction("Index", "Home");
                //}
                //else
                //{
                //    AlertMessage("Success! Continue by clicking OK!", NotificationType.success);
                //    return RedirectToAction("Index", "Home");
                //}
            }
            //else
            //{
            //    AlertMessage("Something Went Wrong", NotificationType.error);
            //} 
            return RedirectToAction("Index", "Home");
        }
    }
}