using ChustaSoft.Common.Helpers;
using DocumentFormat.OpenXml.ExtendedProperties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.Services.Identity;
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
using System.Threading.Tasks;

namespace SMSS.Controllers
{

    [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "Recruiter , Admin")]
    public class CompanyJobsController : BaseController

    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<RegisteredUser> _manager;
        private readonly SignInManager<RegisteredUser> _signInManager;
        private readonly IWebHostEnvironment _iweb;

        private readonly IDataProtector _protector;
        private readonly IConfiguration _config;
        private readonly ILogger<CompanyJobsController> _logger;
        private readonly IOptions<SMSSSmtpClientSettings> _options;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IDataProtector _coProtector;


        public CompanyJobsController(
                                        ApplicationDbContext dbContext,
                                        UserManager<RegisteredUser> manager,
                                        SignInManager<RegisteredUser> signInManager,
                                        IWebHostEnvironment iweb, IDataProtectionProvider dataProtectionProvider,                                
                                        JobIdProtectionSettings jobIdProtectionSettings,
                                        IConfiguration config,
                                        ILogger<CompanyJobsController> logger,
                                        IOptions<SMSSSmtpClientSettings> options,
                                        IWebHostEnvironment hostEnvironment,
                                        CompanyProfileIdProtectionSettings CompanyProfileIdProtectionSettings
            )
        {
            _context = dbContext;
            _manager = manager;
            _signInManager = signInManager;
            _iweb = iweb;
            _protector = dataProtectionProvider.CreateProtector(jobIdProtectionSettings.ProtectJobIdURL);
            _config = config;
            _logger = logger;
            _options = options;
            _hostingEnvironment = hostEnvironment;
            _coProtector = dataProtectionProvider.CreateProtector(CompanyProfileIdProtectionSettings.ProtectCompanyIdURL);
        }

       

        //[HttpGet]
        // [HttpGet("symbols/{symbol}/{slug}/joblist")]
        public async Task<IActionResult> JobList(int? pageNumber, int pageSize = 10)
        {
            ViewBag.companyName = _options.Value.AppName;

            int id = int.Parse(_manager.GetUserId(HttpContext.User));
            RegisteredUser currUser = _context.RegisteredUsers.Include(ru => ru.CompanyProfile).FirstOrDefault(ru => ru.Id == id);
            int compamyProfileId = currUser.CompanyProfile.Id;

            ViewData["OrganizationName"] = currUser.OrganizationName;
            ViewData["UserFullName"] = currUser.FirstName + " " + currUser.LastName;
            ViewBag.Picture = currUser.CompanyProfile.CompanyLogo;

            ///// Check if Company is approved/////
            if (!currUser.CompanyProfile.IsApproved)
            {
                return View("~/Views/CompanyProfile/CompanyConfirm.cshtml");
            }
            /////////////////////////////////////
            ///

            var addedDateTime = DateTime.Now.AddDays(10);

            var companyJobs = _context.CompanyJobs.Where(cp => cp.CompanyProfileId == compamyProfileId && cp.ExpireDate >= DateTime.Now.AddDays(-15) && cp.IsInactive == false).Include(cj => cj.Country).Include(cj => cj.Province).Include(jm => jm.JobMode).OrderByDescending(cp => cp.PostingDate);

            foreach (var job in companyJobs)
            {
                var jobId = job.Id.ToString();
                job.EncryptedId = _protector.Protect(jobId);
            }


            return View(await PaginatedList<CompanyJob>.CreateAsync((IQueryable<CompanyJob>)companyJobs, pageNumber ?? 1, pageSize));

        }

        [HttpGet]
        public async Task<PartialViewResult> PartialJobList(int? pageNumber, int pageSize = 10)
        {
            ViewBag.companyName = _options.Value.AppName;
            int id = int.Parse(_manager.GetUserId(HttpContext.User));
            RegisteredUser currUser = _context.RegisteredUsers.Include(ru => ru.CompanyProfile).FirstOrDefault(ru => ru.Id == id);
            int compamyProfileId = currUser.CompanyProfile.Id;

            var addedDateTime = DateTime.Now.AddDays(55);


            ViewData["OrganizationName"] = currUser.OrganizationName;
            ViewData["UserFullName"] = currUser.FirstName + " " + currUser.LastName;
            ViewBag.Picture = currUser.CompanyProfile.CompanyLogo;

            var companyJobs = _context.CompanyJobs.Where(cp => cp.CompanyProfileId == compamyProfileId && cp.ExpireDate >= DateTime.Now.AddDays(-15) && cp.IsInactive == false).Include(cj => cj.Country).Include(cj => cj.Province).Include(jm => jm.JobMode).OrderByDescending(cp => cp.PostingDate);

            foreach (var job in companyJobs)
            {
                var jobId = job.Id.ToString();
                job.EncryptedId = _protector.Protect(jobId);
            }


            return PartialView("_JobsList", await PaginatedList<CompanyJob>.CreateAsync(companyJobs, pageNumber ?? 1, pageSize));

        }

        [HttpGet("[controller]/JobEdit/{id}")]
        public async Task<IActionResult> JobEdit(int? id)
        {
            ViewBag.companyName = _options.Value.AppName;

            PostJobVM postJobVM = new PostJobVM();

            int userId = int.Parse(_manager.GetUserId(HttpContext.User));
            RegisteredUser currUser = _context.RegisteredUsers
                .Include(ru => ru.CompanyProfile)
                .Where(ru => ru.Id == userId)
               .FirstOrDefault();


            ViewData["OrganizationName"] = currUser.OrganizationName;
            ViewData["UserFullName"] = currUser.FirstName + " " + currUser.LastName;
            ViewBag.Picture = currUser.CompanyProfile.CompanyLogo;
            CompanyJob currJob = await _context.CompanyJobs.FirstOrDefaultAsync(cj => cj.Id == id);
            postJobVM.CompanyJob = currJob;
            postJobVM.Countries = GetCountryList();
            postJobVM.Provices = GetProvicesList(currJob.CountryId);
            postJobVM.Cites = GetCitiesList(currJob.ProvinceId);
            postJobVM.JobModes = GetJobModesList();
            postJobVM.Sectors = GetSectorsListSelect(currJob.Id);
            postJobVM.ProvinceDemoFileAttachments = GetProvinceDemoFileAttachmentsList();
            return View(postJobVM);
        }

        [HttpPost]
        [Route("[controller]/UpdateJob")]
        public async Task<IActionResult> UpdateJob(IFormCollection formData, IFormFile fileInfo)
        {
            ViewBag.companyName = _options.Value.AppName;

            string attchmentPath = "";
            // string attchmentDemoPath = "";

            if (ModelState.IsValid == false)
            {
                return NotFound();
            }

            //if (formData != null)
            //{
                CompanyJob jopPost = await _context.CompanyJobs
                    .Include(cj => cj.CompanyJobSectors)
                    .FirstOrDefaultAsync(cj => cj.Id == int.Parse(formData["CompanyJob.id"]));

                jopPost.JobTitle = formData["CompanyJob.JobTitle"];
                jopPost.JobDescription = formData["CompanyJob.JobDescription"];
                jopPost.JobExperience = (Experience)Enum.Parse(typeof(Qualifications), formData["CompanyJob.JobExperience"].ToString());
                jopPost.JobQualification = (Qualifications)Enum.Parse(typeof(Qualifications), formData["CompanyJob.JobQualification"].ToString());
                jopPost.ExpireDate = DateTime.Parse(formData["CompanyJob.ExpireDate"].ToString());
                jopPost.CountryId = int.Parse(formData["CompanyJob.CountryId"]);
                jopPost.ProvinceId = int.Parse(formData["CompanyJob.ProvinceId"]);
                jopPost.CityId = int.Parse(formData["CompanyJob.CityId"]);
                jopPost.Street = formData["CompanyJob.Street"];
                jopPost.CompanyJobSectors = new List<CompanyJobSector>();

                //jopPost.ProvinceDemoFileAttachmentId = int.Parse(formData["CompanyJob.ProvinceDemoFileAttachmentId"]);




                if ((formData["CompanyJob.ProvinceDemoFileAttachmentId"]) != "")
                {
                    jopPost.ProvinceDemoFileAttachmentId = int.Parse(formData["CompanyJob.ProvinceDemoFileAttachmentId"]);
                }
                else
                {
                    jopPost.ProvinceDemoFileAttachmentId = null;
                }

                foreach (var item in formData["CompanyJob.CompanyJobSectors"])
                {
                    jopPost.CompanyJobSectors.Add(new CompanyJobSector { CompanyJobId = jopPost.Id, SectorId = int.Parse(item) });
                }

                if (fileInfo != null)
                {
                    attchmentPath = await SaveFile(fileInfo, jopPost.FileAttachment);
                    jopPost.FileAttachment = Path.GetFileName(attchmentPath);
                }

                //if (demoFileInfo != null)
                //{
                //    attchmentDemoPath = await SaveDemoFile(demoFileInfo, jopPost.DemoFileAttachment);
                //    jopPost.DemoFileAttachment = Path.GetFileName(attchmentDemoPath);
                //}

                jopPost.IsAttachmentRequired = formData["CompanyJob.IsAttachmentRequired"].Count == 1 ? false : true;

                //jopPost.JobModeId = int.Parse(formData["CompanyJob.JobModeId"]);

                if ((formData["CompanyJob.JobModeId"]) != "")
                {
                    jopPost.JobModeId = int.Parse(formData["CompanyJob.JobModeId"]); 
                }
                else
                {
                    jopPost.JobModeId = null;
                }

                _context.CompanyJobs.Update(jopPost);
                await _context.SaveChangesAsync();

            //}
            return RedirectToAction("JobList", "CompanyJobs");


        }



        [HttpGet("[controller]/DeleteCompanyJob/{id}")]
        public async Task<IActionResult> DeleteCompanyJob(int Id)
        {
            ViewBag.companyName = _options.Value.AppName;

            int currUserId = int.Parse(_manager.GetUserId(HttpContext.User));
            RegisteredUser currUser = _context.RegisteredUsers.Include(ru => ru.CompanyProfile).FirstOrDefault(ru => ru.Id == currUserId);
            int compamyProfileId = currUser.CompanyProfile.Id;


            var applicant = _context.ApplicantJobApplications.Where(aj => aj.CompanyJobId == Id).FirstOrDefault();

            if (applicant != null)
            {
                ViewData["Applicant"] = applicant.Id;
                applicant.DeleteFlag = true;
                _context.ApplicantJobApplications.Update(applicant);
                _context.SaveChanges();
            }



            ViewData["OrganizationName"] = currUser.OrganizationName;
            ViewData["UserFullName"] = currUser.FirstName + " " + currUser.LastName;
            ViewBag.Picture = currUser.CompanyProfile.CompanyLogo;

            var comapnyJob = await _context.CompanyJobs.Where(cp => cp.Id == Id && cp.IsInactive == false).Include(jm => jm.JobMode).Include(cj => cj.Country).Include(cj => cj.Province).AsNoTracking().FirstOrDefaultAsync();

            return View(comapnyJob);

        }

        [HttpPost]
        [Route("[controller]/ConfirmCompanyJobDelete")]
        public async Task<IActionResult> ConfirmCompanyJobDelete(IFormCollection formData)
        {
            ViewBag.companyName = _options.Value.AppName;


            var companyjob = await _context.CompanyJobs.Where(cj => cj.Id == int.Parse(formData["Id"])).FirstOrDefaultAsync();

            if (companyjob != null)
            {
                if (companyjob.FileAttachment != null)
                {
                    DeleteFile(companyjob.FileAttachment);
                }
                else if(companyjob.JobModeId != null)
                {
                    //
                    var jmodeId = await _context.JobModes.Where(jm => jm.Id == int.Parse(formData["JobModeId"])).FirstOrDefaultAsync();
                    _context.Remove(jmodeId);
                    _context.Update(jmodeId);
                }
                else
                {
                    //
                }
                companyjob.IsInactive = true;
                _context.CompanyJobs.Remove(companyjob);
                _context.CompanyJobs.Update(companyjob);

                AlertMessage("You Have DELETED This Job Successfully!!!..", NotificationType.success);

                await _context.SaveChangesAsync();
            }


            return RedirectToAction("JobList", "CompanyJobs");

        }


        [Route("/JobDetail/{id:int}/{slug?}")]
        public async Task<IActionResult> JobDetails(int id)
        {
            ViewBag.companyName = _options.Value.AppName;
            //int jobId = Convert.ToInt32(_protector.Unprotect(id));

            int userId = int.Parse(_manager.GetUserId(HttpContext.User));
            RegisteredUser currUser = _context.RegisteredUsers.Include(ru => ru.CompanyProfile).FirstOrDefault(ru => ru.Id == userId);
            int compamyProfileId = currUser.CompanyProfile.Id;

            ViewData["OrganizationName"] = currUser.OrganizationName;
            ViewData["UserFullName"] = currUser.FirstName + " " + currUser.LastName;
            ViewBag.Picture = currUser.CompanyProfile.CompanyLogo;

            var CompanyJobs = await _context.CompanyJobs.Include(cp => cp.Country)
                            .Include(cp => cp.Province)
                            .Include(cp => cp.City)
                            .Include(cp => cp.CompanyProfile)
                            .ThenInclude(cp => cp.RegisteredUser)
                            .Include(cp => cp.CompanyJobSectors)
                            .ThenInclude(cp => cp.Sector)
                            .Where(cp => cp.Id == id).FirstOrDefaultAsync();




            return View(CompanyJobs);

        }


        [Route("ApplicantDetails/{id:int}")]
        public async Task<IActionResult> ApplicantDetails(int id)
        {
            ViewBag.companyName = _options.Value.AppName;
            int userId = int.Parse(_manager.GetUserId(HttpContext.User));
            RegisteredUser currUser = _context.RegisteredUsers.FirstOrDefault(ru => ru.Id == userId);
            //int compamyProfileId = currUser.CompanyProfile.Id;

            //ViewData["OrganizationName"] = currUser.OrganizationName;
            //ViewData["UserFullName"] = currUser.FirstName + " " + currUser.LastName;
            //ViewBag.Picture = currUser.CompanyProfile.CompanyLogo;

            var registeredUsers = await _context.RegisteredUsers
                            .Include(ad => ad.ApplicantProfile)
                            .ThenInclude(ap => ap.ApplicantWorkHistorys)
                            .Include(ad => ad.ApplicantProfile)
                            .ThenInclude(ap => ap.ApplicantEducations)
                            .Include(ad => ad.ApplicantProfile)
                            .ThenInclude(ap => ap.City)
                            .Include(ad => ad.ApplicantProfile)
                            .ThenInclude(ap => ap.Province)
                            .Include(ad => ad.ApplicantProfile)
                            .ThenInclude(ap => ap.Country)
                            .Include(ad => ad.UserSectors)
                            .ThenInclude(us => us.Sector)
                            .Where(cp => cp.Id == id).FirstOrDefaultAsync();

            return View(registeredUsers);

        }





        [HttpGet]
        //[Route("download")]
        public async Task<ActionResult> GetAttachment([FromQuery] string filename)
        {
            string fullpath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "JobPosts", filename);

            if (!System.IO.File.Exists(fullpath))
                return NotFound();

            var memory = new MemoryStream();
            using (var stream = new FileStream(fullpath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, GetContentType(fullpath), filename);
        }

        [HttpGet]
        public async Task<ActionResult> AllApplicants(int jobId = 0)
        {
            ViewBag.companyName = _options.Value.AppName;

            int id = int.Parse(_manager.GetUserId(HttpContext.User));
            RegisteredUser currUser = _context.RegisteredUsers.Include(ru => ru.CompanyProfile).FirstOrDefault(ru => ru.Id == id);
            int compamyProfileId = currUser.CompanyProfile.Id;

            ViewData["OrganizationName"] = currUser.OrganizationName;
            ViewData["UserFullName"] = currUser.FirstName + " " + currUser.LastName;
            ViewBag.Picture = currUser.CompanyProfile.CompanyLogo;

            ///// Check if Company is approved/////
            if (!currUser.CompanyProfile.IsApproved)
            {
                return View("~/Views/CompanyProfile/CompanyConfirm.cshtml");
            }
            /////////////////////////////////////

            AllApplicantsVM allApplicantsVM = new AllApplicantsVM();

            List<CompanyJob> jobList = await _context.CompanyJobs
                .Where(cj => cj.CompanyProfileId == compamyProfileId)
                .ToListAsync();

            

            List<CompanyJob> companyJobs = await _context.CompanyJobs
                .Where(cj => cj.CompanyProfileId == compamyProfileId && cj.ApplicantJobApplications.Count > 0 && cj.ExpireDate >= DateTime.Now.AddDays(-15) && cj.IsInactive == false)
                .Include(cj => cj.ApplicantJobApplications.Where(cj => cj.ApplicantProfileId != null && cj.ApplicationDate <= DateTime.Now.AddDays(15)))
                .ThenInclude(aja => aja.ApplicantProfile)
                .ThenInclude(ap => ap.RegisteredUser)
                .Include(cj => cj.ApplicantJobApplications)
                .ThenInclude(aja => aja.ApplicantProfile)
                .ThenInclude(ap => ap.Province)
                .Include(cj => cj.ApplicantJobApplications)
                .ThenInclude(aja => aja.ApplicantProfile)
                .ThenInclude(ap => ap.Country)                
                .Where(cj => cj.Id == jobId || jobId == 0)
                .OrderByDescending(cj => cj.PostingDate)
                .ToListAsync();

            allApplicantsVM.JobsList.Add(new SelectListItem() { Value = "0", Text = "All Jobs", Selected = true });

            foreach (CompanyJob item in jobList.OrderByDescending(cp => cp.PostingDate))
            {
                allApplicantsVM.JobsList.Add(new SelectListItem() { Value = item.Id.ToString(), Text = item.JobTitle });
            }

            foreach (CompanyJob job in companyJobs.OrderByDescending(cp => cp.PostingDate))
            {

                allApplicantsVM.ApplicantsCount += job.ApplicantJobApplications.Count;
                               
                allApplicantsVM.JobApplicantsLists.Add(new JobApplicantsList()
                {
                    
                    ApplicantsCount = job.ApplicantJobApplications.Count,
                    JobTitle = job.JobTitle,                    
                    id = job.Id,
                    applicantJobApplications = job.ApplicantJobApplications.ToList()
                });
            }

            return View(allApplicantsVM);
        }

        public async Task<PartialViewResult> ChangeStatus(int jobId, int applicationId, int applicantionStatus)
        {
            ViewBag.companyName = _options.Value.AppName;
            /////Update Status/////
            ApplicantJobApplication application = _context.ApplicantJobApplications.FirstOrDefault(apa => apa.Id == applicationId);
            application.ApplicationStatus = (ApplicationStatus)applicantionStatus;
            _context.SaveChanges();
            ///////////////////////

            int id = int.Parse(_manager.GetUserId(HttpContext.User));
            RegisteredUser currUser = _context.RegisteredUsers.Include(ru => ru.CompanyProfile).FirstOrDefault(ru => ru.Id == id);
            int compamyProfileId = currUser.CompanyProfile.Id;

            ViewData["OrganizationName"] = currUser.OrganizationName;
            ViewData["UserFullName"] = currUser.FirstName + " " + currUser.LastName;
            ViewBag.Picture = currUser.CompanyProfile.CompanyLogo;

            AllApplicantsVM allApplicantsVM = new AllApplicantsVM();

            List<CompanyJob> jobList = await _context.CompanyJobs
                .Where(cj => cj.CompanyProfileId == compamyProfileId)
                .ToListAsync();

            List<CompanyJob> companyJobs = await _context.CompanyJobs
                .Where(cj => cj.CompanyProfileId == compamyProfileId && cj.ApplicantJobApplications.Count > 0)
                .Include(cj => cj.ApplicantJobApplications)
                .ThenInclude(aja => aja.ApplicantProfile)
                .ThenInclude(ap => ap.RegisteredUser)
                .Include(cj => cj.ApplicantJobApplications)
                .ThenInclude(aja => aja.ApplicantProfile)
                .ThenInclude(ap => ap.Province)
                .Include(cj => cj.ApplicantJobApplications)
                .ThenInclude(aja => aja.ApplicantProfile)
                .ThenInclude(ap => ap.Country)
                .Where(cj => cj.Id == jobId || jobId == 0)
                .OrderByDescending(cj => cj.PostingDate)
                .ToListAsync();

            allApplicantsVM.JobsList.Add(new SelectListItem() { Value = "0", Text = "All Jobs", Selected = true });
            foreach (CompanyJob item in jobList)
            {
                allApplicantsVM.JobsList.Add(new SelectListItem() { Value = item.Id.ToString(), Text = item.JobTitle });
            }

            foreach (CompanyJob job in companyJobs)
            {
                allApplicantsVM.ApplicantsCount += job.ApplicantJobApplications.Count;
                allApplicantsVM.JobApplicantsLists.Add(new JobApplicantsList()
                {
                    ApplicantsCount = job.ApplicantJobApplications.Count,
                    JobTitle = job.JobTitle,
                    applicantJobApplications = job.ApplicantJobApplications.ToList()
                });
            }

            return PartialView("_AllApplicant", allApplicantsVM);

        }

        [HttpGet]

        public async Task<IActionResult> AllCandidate(int? pageNumber, int pageSize = 10)
        {
            ViewBag.companyName = _options.Value.AppName;
            int id = int.Parse(_manager.GetUserId(HttpContext.User));
            RegisteredUser currUser = _context.RegisteredUsers.Include(ru => ru.CompanyProfile).FirstOrDefault(ru => ru.Id == id);
            int compamyProfileId = currUser.CompanyProfile.Id;

            ViewData["OrganizationName"] = currUser.OrganizationName;
            ViewData["UserFullName"] = currUser.FirstName + " " + currUser.LastName;
            ViewBag.Picture = currUser.CompanyProfile.CompanyLogo;

            var applicantProfilesQuery = _context.ApplicantProfiles
                .Include(ap => ap.RegisteredUser)
                .Include(ap => ap.City)
                .Include(ap => ap.Province)
                .Include(ap => ap.Country);


            PaginatedList<ApplicantProfile> applicantProfiles = await PaginatedList<ApplicantProfile>.CreateAsync(applicantProfilesQuery, pageNumber ?? 1, pageSize);
            var sectors = _context.Sectors
                 .ToList()
                 .Select(sec => new SelectListItem { Text = sec.SectorName, Value = sec.Id.ToString() }).ToList();
            var counties = _context.Countries.ToList().Select(co => new SelectListItem { Text = co.Name, Value = co.Id.ToString() }).ToList();
            counties.Insert(0, new SelectListItem { Text = "Please Select", Value = "0" });

            AllCandidateVM allCandidateVM = new AllCandidateVM() { ApplicantProfiles = applicantProfiles, SectorsList = sectors, Countries = counties };


            return View(allCandidateVM);

        }
        [HttpGet]
        public async Task<PartialViewResult> PartialAllCandidate(int? pageNumber, int pageSize = 10, int[] sectorsFilter = null, int? country = null, int? province = null, int? city = null, DateTime? fromDate = null, DateTime? toDate = null, string name = null)
        {
            ViewBag.companyName = _options.Value.AppName;

            int id = int.Parse(_manager.GetUserId(HttpContext.User));
            RegisteredUser currUser = _context.RegisteredUsers.Include(ru => ru.CompanyProfile).FirstOrDefault(ru => ru.Id == id);
            int compamyProfileId = currUser.CompanyProfile.Id;
            string queryString = "";

            ViewData["OrganizationName"] = currUser.OrganizationName;
            ViewData["UserFullName"] = currUser.FirstName + " " + currUser.LastName;
            ViewBag.Picture = currUser.CompanyProfile.CompanyLogo;

            IQueryable<ApplicantProfile> applicantProfilesQuery = _context.ApplicantProfiles
                .Include(ap => ap.RegisteredUser)
                .Include(ap => ap.City)
                .Include(ap => ap.Province)
                .Include(ap => ap.Country);

            if (sectorsFilter.Count() > 0)
            {
                applicantProfilesQuery = applicantProfilesQuery.Include(ap => ap.RegisteredUser).ThenInclude(ru => ru.UserSectors).Where(ap => ap.RegisteredUser.UserSectors.Any(us => sectorsFilter.Contains(us.SectorId)));
            }
            else
            {
                applicantProfilesQuery = applicantProfilesQuery.Include(ap => ap.RegisteredUser);
            }

            if (country != null)
            {
                applicantProfilesQuery = applicantProfilesQuery.Where(ap => ap.CountryId == country);
                queryString += "&country=" + country.ToString();
            }

            if (province != null)
            {
                applicantProfilesQuery = applicantProfilesQuery.Where(ap => ap.ProvinceId == province);
                queryString += "&province=" + province.ToString();
            }

            if (city != null)
            {
                applicantProfilesQuery = applicantProfilesQuery.Where(ap => ap.CityId == city);
                queryString += "&city=" + city.ToString();
            }

            if (fromDate != null)
            {
                applicantProfilesQuery = applicantProfilesQuery.Where(ap => ap.RegistrationDate >= fromDate);
                queryString += "&fromDate=" + fromDate.ToString();
            }

            if (toDate != null)
            {
                applicantProfilesQuery = applicantProfilesQuery.Where(ap => ap.RegistrationDate <= ((DateTime)toDate).AddDays(1));
                queryString += "&toDate=" + toDate.ToString();
            }

            if (name != null && name != "")
            {
                applicantProfilesQuery = applicantProfilesQuery.Where(ap => ap.RegisteredUser.FirstName.ToUpper().Contains(name.ToUpper()) || ap.RegisteredUser.LastName.ToUpper().Contains(name.ToUpper()));
                queryString += "&name=" + name;
            }


            PaginatedList<ApplicantProfile> applicantProfiles = await PaginatedList<ApplicantProfile>.CreateAsync(applicantProfilesQuery, pageNumber ?? 1, pageSize);
            var sectors = _context.Sectors
                 .ToList()
                 .Select(sec => new SelectListItem { Text = sec.SectorName, Value = sec.Id.ToString() }).ToList();




            AllCandidateVM allCandidateVM = new AllCandidateVM() { ApplicantProfiles = applicantProfiles, SectorsList = sectors, QueryString = queryString };

            return PartialView("_AllCandidate", allCandidateVM);

        }

        [HttpGet]
        public async Task<ActionResult> DownloadFile(int type, string filename)
        {
            string fullpath = "";
            switch (type)
            {
                case 1:
                    fullpath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "JobPosts", filename);
                    break;
                case 2:
                    fullpath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "JobPosts", filename);
                    break;
                case 3:
                    fullpath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "CoverLetter", filename);
                    break;
                case 4:
                    fullpath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "JobMatrix", filename);
                    break;


            }


            if (!System.IO.File.Exists(fullpath))
                return NotFound();

            var memory = new MemoryStream();
            using (var stream = new FileStream(fullpath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, GetContentType(fullpath), filename);
        }

        public IActionResult SendMailToApplicant(string toaddress, string subject, string message)
        {
            ViewBag.companyName = _options.Value.AppName;

            using (MailMessage mailMessage = new MailMessage())
            {

                mailMessage.From = new MailAddress(_options.Value.FromEmail);
                mailMessage.Subject = subject;
                mailMessage.Body = message;
                mailMessage.IsBodyHtml = true;
                mailMessage.To.Add(new MailAddress(toaddress));
                System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient();
                smtp.UseDefaultCredentials = false;

                smtp.Host = _options.Value.Host;
                smtp.EnableSsl = true;

                System.Net.NetworkCredential networkcred = new System.Net.NetworkCredential();
                networkcred.UserName = _options.Value.Username;
                networkcred.Password = _options.Value.Password;
                smtp.Credentials = networkcred;

                smtp.Port = _options.Value.Port;
                smtp.Send(mailMessage);



            }
            //return RedirectToAction("SendMailToApplicant", "CompanyJobs");
            return View("~/Views/CompanyJobs/SendMailToApplicant.cshtml");
        }


        private string GetContentType(string path)
        {
            var provider = new FileExtensionContentTypeProvider();
            string contentType;
            if (!provider.TryGetContentType(path, out contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }


       

        private List<SelectListItem> GetCountryList()
        {
            List<SelectListItem> countries = new List<SelectListItem>();
            List<Country> dbCountry = _context.Countries.ToList();
            countries.Add(new SelectListItem() { Text = "Please Select", Value = "0" });
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

        private List<SelectListItem> GetSectorsListSelect(int companyJobId)
        {


            

            List<CompanyJobSector> selectedSectors = _context.CompanyJobSectors.Where(js => js.CompanyJobId == companyJobId).ToList();
            List<SelectListItem> sectors = new List<SelectListItem>();
            List<Sector> dbSectors = _context.Sectors.OrderBy(st => st.SectorName).ToList();

            

            if (dbSectors != null)
            {

                foreach (Sector sector in dbSectors)
                {
                    var item = new SelectListItem { Value = sector.Id.ToString(), Text = sector.SectorName };
                    item.Selected = (selectedSectors.FirstOrDefault(sc => sc.SectorId == sector.Id) != null);
                    sectors.Add(item);
                }
            }

            return sectors;
        }


        private List<SelectListItem> GetJobModesList()
        {
            List<SelectListItem> jobModes = new List<SelectListItem>();
            List<JobMode> _dbjobModeName = _context.JobModes.ToList();
            jobModes.Add(new SelectListItem() { Text = "", Value = "" });



            foreach (JobMode jobmode in _dbjobModeName)
            {
                jobModes.Add(new SelectListItem { Value = jobmode.Id.ToString(), Text = jobmode.JobModeName });
            }
            return jobModes;
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

        private async Task<string> SaveFile(IFormFile file, string filedelete = "")
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
                        ModelState.AddModelError("ModelError", "Please Provide a Valid file Format");
                    if (filedelete != "")
                    {
                        filedelete = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "JobPosts", filedelete);
                        FileInfo fi = new FileInfo(filedelete);
                        if (fi != null)
                        {
                            System.IO.File.Delete(filedelete);
                            fi.Delete();
                        }
                    }



                    //var fileName = (name + "_" + DateTime.Now.ToString("ddMMyyhhmmss") + Path.GetExtension(file.FileName)).Replace(" ", "_");
                    var fileName = (name + Path.GetExtension(file.FileName)).Replace(" ", "_");
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
                //demofile



            }
            catch
            {
                //do something
            }



            return filePath;
        }

        private void DeleteFile(string fileName)
        {
            if (fileName != "")
            {
                fileName = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "JobPosts", fileName);
                FileInfo fi = new FileInfo(fileName);
                if (fi != null)
                {
                    System.IO.File.Delete(fileName);
                    fi.Delete();
                }
            }
        }

        private void DeleteResumeFile(string fileName)
        {
            if (fileName != "")
            {
                //fileName = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Resumes", fileName);

                fileName = Path.Combine(_hostingEnvironment.WebRootPath, "JobPosts/Resumes", fileName);
                FileInfo fi = new FileInfo(fileName);
                if (fi != null)
                {
                    System.IO.File.Delete(fileName);
                    fi.Delete();
                }
                else
                {
                    AlertMessage("Resume File Does not Exist!!!..", NotificationType.error);
                }
            }
        }

        private void DeleteSkillMatrixFile(string fileName)
        {
            if (fileName != "")
            {
                fileName = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "JobMatrix", fileName);
                FileInfo fi = new FileInfo(fileName);
                if (fi != null)
                {
                    System.IO.File.Delete(fileName);
                    fi.Delete();
                }
                else
                {
                    AlertMessage("Skill Matrix File Does not Exist!!!..", NotificationType.error);
                }
            }
        }


        [HttpGet]
        
        public async Task<IActionResult> Delete(int Id)
        {
            ViewBag.companyName = _options.Value.AppName;

            int currUserId = int.Parse(_manager.GetUserId(HttpContext.User));
            RegisteredUser currUser = _context.RegisteredUsers.Include(ru => ru.CompanyProfile).FirstOrDefault(ru => ru.Id == currUserId);
            int compamyProfileId = currUser.CompanyProfile.Id;


            var applicant = _context.ApplicantJobApplications.Where(aj => aj.CompanyJobId == Id).FirstOrDefault();

            if (applicant != null)
            {
                ViewData["Applicant"] = applicant.Id;
            }
            ViewData["OrganizationName"] = currUser.OrganizationName;
            ViewData["UserFullName"] = currUser.FirstName + " " + currUser.LastName;
            ViewBag.Picture = currUser.CompanyProfile.CompanyLogo;
            var comapnyJob = await _context.CompanyJobs.Where(cp => cp.Id == Id).Include(cj => cj.Country).Include(cj => cj.Province).AsNoTracking().FirstOrDefaultAsync();

            return View(comapnyJob);

        }



        //[HttpGet]
        //[Route("DeleteApplicant/{id:int}")]
        //public async Task<IActionResult> DeleteApplicant(int id)
        //{
        //    ViewBag.companyName = _options.Value.AppName;

        //    int userId = int.Parse(_manager.GetUserId(HttpContext.User));
        //    RegisteredUser currUser = _context.RegisteredUsers.FirstOrDefault(ru => ru.ApplicantProfile.RegisteredUser.Id == userId);

        //    var applicantJobApp = await _context.RegisteredUsers
        //                    .Include(ad => ad.ApplicantProfile)
        //                    .ThenInclude(ap => ap.ApplicantWorkHistorys)
        //                    .Include(ad => ad.ApplicantProfile)
        //                    .ThenInclude(ap => ap.ApplicantEducations)
        //                    .Include(ad => ad.ApplicantProfile)
        //                    .ThenInclude(ap => ap.City)
        //                    .Include(ad => ad.ApplicantProfile)
        //                    .ThenInclude(ap => ap.Province)
        //                    .Include(ad => ad.ApplicantProfile)
        //                    .ThenInclude(ap => ap.Country)
        //                    .Include(ad => ad.UserSectors)
        //                    .ThenInclude(us => us.Sector)
        //                    .Where(cp => cp.Id == id).FirstOrDefaultAsync();

        //    var applicant = _context.ApplicantJobApplications.Where(aj => aj.CompanyJobId == id).FirstOrDefault();

        //    if (applicant != null)
        //    {
        //        ViewData["Applicant"] = applicant.Id;
        //    }

        //    return View(applicantJobApp);

        //}

        //[HttpPost]
        //public async Task<IActionResult> ConfirmApplicantDelete(IFormCollection formData)

        //{
        //    ViewBag.companyName = _options.Value.AppName;

        //    var deleteApplicant = await _context.ApplicantJobApplications.FirstOrDefaultAsync(dap => dap.ApplicantProfile.RegisteredUser.Id == int.Parse(formData["ApplicantProfile.RegisteredUser.Id"]));
        //    if (deleteApplicant != null)
        //    {
        //      _context.ApplicantJobApplications.Remove(deleteApplicant);
        //      AlertMessage("You Have DELETED Applicant Job Applicantion Successfully!!!..", NotificationType.success);
        //    }
        //    else
        //    {
        //        AlertMessage("Opps!!!.. Applicant Job Applicantion Could not be DELETED!", NotificationType.error);
        //    }
        //    _context.SaveChanges();
        //    return RedirectToAction("AllApplicants", "CompanyJobs");

        //}

        [HttpGet("[controller]/DeleteApplicant/{id}")]
        public async Task<IActionResult> DeleteApplicant(int Id)
        {
            ViewBag.companyName = _options.Value.AppName;

            int userId = int.Parse(_manager.GetUserId(HttpContext.User));
            RegisteredUser currUser = _context.RegisteredUsers.FirstOrDefault(ru => ru.ApplicantProfile.RegisteredUser.Id == userId);

            var applicantJobApp = await _context.RegisteredUsers
                            .Include(ad => ad.ApplicantProfile)
                            .ThenInclude(ap => ap.ApplicantWorkHistorys)
                            .Include(ad => ad.ApplicantProfile)
                            .ThenInclude(ap => ap.ApplicantEducations)
                            .Include(ad => ad.ApplicantProfile)
                            .ThenInclude(ap => ap.City)
                            .Include(ad => ad.ApplicantProfile)
                            .ThenInclude(ap => ap.Province)
                            .Include(ad => ad.ApplicantProfile)
                            .ThenInclude(ap => ap.Country)
                            .Include(ad => ad.UserSectors)
                            .ThenInclude(us => us.Sector)
                            .Where(cp => cp.Id == Id).FirstOrDefaultAsync();

            var applicant = _context.ApplicantJobApplications.Where(aj => aj.CompanyJobId == Id).FirstOrDefault();


            if (applicant != null)
            {
                ViewData["Applicant"] = applicant.Id;
            }
            //else
            //{
            //    AlertMessage("Opps!!!.. Applicant Does not Exist!", NotificationType.error);
            //}

            return View(applicantJobApp);
        }


        [HttpPost]
        public async Task<IActionResult> ConfirmApplicantDelete(IFormCollection formData)

        {
            ViewBag.companyName = _options.Value.AppName;

            var deleteApplicant = await _context.ApplicantJobApplications.FirstOrDefaultAsync(dap => dap.ApplicantProfile.Id == int.Parse(formData["ApplicantProfile.Id"]));

            _context.ApplicantJobApplications.Remove(deleteApplicant);

            AlertMessage("You Have DELETED Applicant Job Applicantion Successfully!!!.. ", NotificationType.success);

            if (deleteApplicant.ResumeLocation != null && deleteApplicant.SkillMatrixLocation != null)
            {
                DeleteResumeFile(deleteApplicant.ResumeLocation);
                DeleteSkillMatrixFile(deleteApplicant.SkillMatrixLocation);
                _context.ApplicantJobApplications.Update(deleteApplicant);
                _context.SaveChanges();
            }

            _context.SaveChanges();

            return RedirectToAction("AllApplicants", "CompanyJobs");

        }

        //[HttpPost]
        //public async Task<IActionResult> ConfirmApplicantDelete(IFormCollection formData)
        //{
        //    ViewBag.companyName = _options.Value.AppName;

        //    if (formData != null)
        //    {
        //        int id = int.Parse(formData["ApplicantProfile.RegisteredUser.Id"].ToString());

        //        ApplicantJobApplication confirmDel = await _context.ApplicantJobApplications.FirstOrDefaultAsync(cd => cd.ApplicantProfile.RegisteredUser.Id == id);
        //        _context.ApplicantJobApplications.Remove(confirmDel);
        //        _context.SaveChanges();
        //        AlertMessage("You Have DELETED Applicant Job Applicantion Successfully!!!..", NotificationType.success);
        //    }
        //    return RedirectToAction("AllApplicants", "CompanyJobs");
        //}


        [HttpGet]
        public async Task<ActionResult> GetResumeFileAttachment([FromQuery] string filename)
        {
            string fullpath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Resumes", filename);

            if (!System.IO.File.Exists(fullpath))
                AlertMessage("Opps!!!.. Resume File Does not Exists or have been  DELETED!", NotificationType.error);

            var memory = new MemoryStream();
            using (var stream = new FileStream(fullpath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, GetContentType(fullpath), filename);
        }





        [HttpGet]
        public async Task<IActionResult> DownloadResumeFileAttachment(int type, string resFileName)
        {
            string fullpath = "";
            switch (type)
            {
                case 1:
                    fullpath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Resumes", resFileName);
                    break;
            }

            if (!System.IO.File.Exists(fullpath))
                return NotFound();

            var memory = new MemoryStream();
            using (var stream = new FileStream(fullpath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, GetContentType(fullpath), resFileName);
        }



        private string GetResumeContentype(string resFilePath)
        {
            var provider = new FileExtensionContentTypeProvider();
            string contentType;
            if (!provider.TryGetContentType(resFilePath, out contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }
    }
}
