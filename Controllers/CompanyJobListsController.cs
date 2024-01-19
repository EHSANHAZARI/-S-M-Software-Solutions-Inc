using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DNTCaptcha.Core;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SMSS.Data;
using SMSS.Models;
using SMSS.Services;
using SMSS.ViewModels;
using SMSS.WebSecurity;
using System.Diagnostics;
using System.IO;
using System.Net.Mail;
using System.Net;
using System.Xml;
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





namespace SMSS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyJobListsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private const int pageSize = 3;
        private readonly ILogger<HomeController> _logger;

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

        public CompanyJobListsController(
            ILogger<HomeController> logger,
            ApplicationDbContext context,
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
            IWebHostEnvironment hostEnvironment
            )
        {
            _logger = logger;
            _context = context;
            _manager = manager;
            _signInManager = signInManager;
            _iweb = iweb;
            _protector = dataProtectionProvider.CreateProtector(jobIdProtectionSettings.ProtectJobIdURL);
            _validatorService = validatorService;
            _captchaOptions = captchaOptions == null ? throw new ArgumentException(nameof(captchaOptions)) : captchaOptions.Value;
            _coProtector = dataProtectionProvider.CreateProtector(CompanyProfileIdProtectionSettings.ProtectCompanyIdURL);
            _options = options;
            _viewEngine = viewEngine;
            _hostingEnvironment = hostEnvironment;
        }

        // GET: api/CompanyJobLists
        [HttpGet]
        public ActionResult<IEnumerable<CompanyJob>> GetCompanyJobs()
        {
            // return await _context.CompanyJobs.ToListAsync();

            CompanyTestimonialVM ctVM = new CompanyTestimonialVM();


            List<CompanyJob> companyJobs = _context.CompanyJobs
                .Where(cj => cj.ExpireDate >= DateTime.Now && cj.IsInactive == false)
                .OrderByDescending(cj => cj.PostingDate)
                .Include(cp => cp.Country)
                .Include(cp => cp.Province)
                .Include(cp => cp.City)
                .Include(apj => apj.ApplicantJobApplications)
                .Include(cmpy => cmpy.CompanyProfile)
                .ThenInclude(rgusr => rgusr.RegisteredUser)
                .Include(cp => cp.CompanyProfile.RegisteredUser).Take(6)
                .ToList();

            List<ApplicantTestimonial> atVM = _context.ApplicantTestimonials
                                  .Where(cj => cj.IsApprove == true)
                              .Include(ap => ap.ApplicantProfile.RegisteredUser).ToList();

            ctVM.CompanyJobs = companyJobs;



            if (_signInManager.IsSignedIn(User))
            {
                int userId = int.Parse(_manager.GetUserId(HttpContext.User));
                ApplicantProfile profile = _context.ApplicantProfiles.FirstOrDefault(ap => ap.RegisteredUserId == userId);

                if (profile != null)
                {

                    //ViewData["CurrentProfileId"] = profile.Id;
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

            return new JsonResult(ctVM);
        }

        // GET: api/CompanyJobLists/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CompanyJob>> GetCompanyJob(int id)
        {
            var companyJob = await _context.CompanyJobs.FindAsync(id);

            if (companyJob == null)
            {
                return NotFound();
            }

            return companyJob;
        }

        // PUT: api/CompanyJobLists/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCompanyJob(int id, CompanyJob companyJob)
        {
            if (id != companyJob.Id)
            {
                return BadRequest();
            }

            _context.Entry(companyJob).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CompanyJobExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/CompanyJobLists
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<CompanyJob>> PostCompanyJob(CompanyJob companyJob)
        {
            _context.CompanyJobs.Add(companyJob);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCompanyJob", new { id = companyJob.Id }, companyJob);
        }

        // DELETE: api/CompanyJobLists/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCompanyJob(int id)
        {
            var companyJob = await _context.CompanyJobs.FindAsync(id);
            if (companyJob == null)
            {
                return NotFound();
            }

            _context.CompanyJobs.Remove(companyJob);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CompanyJobExists(int id)
        {
            return _context.CompanyJobs.Any(e => e.Id == id);
        }
    }
}
