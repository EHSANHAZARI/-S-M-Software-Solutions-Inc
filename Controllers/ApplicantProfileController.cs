using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SMSS.Data;
using SMSS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SMSS.ViewModels;
using System.IO;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.StaticFiles;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using SMSS.Services;
using System.Net;
using Microsoft.AspNetCore.Authentication;

namespace SMSS.Controllers
{
    [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "User")]
    public class ApplicantProfileController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<RegisteredUser> _manager;
        private readonly SignInManager<RegisteredUser> _signInManager;
        private readonly IWebHostEnvironment _iweb;
        private readonly IOptions<SMSSSmtpClientSettings> _options;


        [Obsolete]
        public ApplicantProfileController(ApplicationDbContext context, UserManager<RegisteredUser> manager,
                                          SignInManager<RegisteredUser> signInManager, IWebHostEnvironment iweb, IOptions<SMSSSmtpClientSettings> options)
        {
            _context = context;
            _manager = manager;
            _signInManager = signInManager;
            _iweb = iweb;
            _options = options;
        }


        public IActionResult GetDashboardTest()
        {
            //int id = int.Parse(_manager.GetUserId(HttpContext.User));
            //List<ApplicantJobApplication> jobApplication = _context.ApplicantJobApplications
            //    .Where(ja => ja.ApplicantProfileId == id).ToList();
            //return View(jobApplication);

            ApplicantProfile profile = GetApplicantInfo();
            ViewData["UserFullName"] = profile.RegisteredUser.FirstName + " " + profile.RegisteredUser.LastName;
            ViewBag.ProfilePicture = profile.ProfileImg;

            int id = int.Parse(_manager.GetUserId(HttpContext.User));
            IQueryable<JobsDateGroup> data = from jobs in _context.ApplicantJobApplications
                                             .Where(ja => ja.ApplicantProfileId == id)
                                             group jobs by jobs.ApplicationDate.Date
                                             into dateGroup
                                             select new JobsDateGroup()
                                             {
                                                 applicationDate = dateGroup.Key,
                                                 jobCount = dateGroup.Count()
                                             };
            return View(data);
        }

        public IActionResult GetDashboard()
        {
            ApplicantProfile profile = GetApplicantInfo();
            ViewData["UserFullName"] = profile.RegisteredUser.FirstName + " " + profile.RegisteredUser.LastName;
            ViewBag.ProfilePicture = profile.ProfileImg;

            int id = int.Parse(_manager.GetUserId(HttpContext.User));
            int applicantId = _context.ApplicantProfiles.FirstOrDefault(ap => ap.RegisteredUserId == id).Id;
            IQueryable<JobsDateGroup> data = from jobs in _context.ApplicantJobApplications
                                             .Where(ja => ja.ApplicantProfileId == applicantId)
                                             group jobs by jobs.ApplicationDate.Date
                                             into dateGroup
                                             select new JobsDateGroup()
                                             {
                                                 applicationDate = dateGroup.Key,
                                                 jobCount = dateGroup.Count()
                                             };
            return View(data);
            //List<ApplicantJobApplication> jobApplication = _context.ApplicantJobApplications
            //     .Where(ja => ja.ApplicantProfileId == id).ToList();
            //     return View(jobApplication);

        }
        public IActionResult DashboardViewDetails()
        {
            ViewBag.companyName = _options.Value.AppName;
            int id = int.Parse(_manager.GetUserId(HttpContext.User));
            ApplicantProfile profile = GetApplicantInfo();
            ViewData["UserFullName"] = profile.RegisteredUser.FirstName + " " + profile.RegisteredUser.LastName;
            ViewBag.ProfilePicture = profile.ProfileImg;

            var viewModel = new DashboardViewDetailsVM();
            //List<ApplicantJobApplication> jobApplication = _context.ApplicantJobApplications
            viewModel.applicantJobApplications = _context.ApplicantJobApplications
                .Include(ja => ja.CompanyJob)
                .ThenInclude(cj => cj.Country)
                .Include(ja => ja.CompanyJob)
                .ThenInclude(cj => cj.Province)
                .Include(ja => ja.CompanyJob)
                .ThenInclude(cj => cj.City)
                .Include(ja => ja.ApplicantProfile)
                .ThenInclude(ap => ap.RegisteredUser)
                .Where(ja => ja.ApplicantProfile.RegisteredUser.Id == id).ToList();

            foreach(var application in viewModel.applicantJobApplications)
            {
                if(application.DeleteFlag==true)
                    AlertMessage($"The {application.CompanyJob.JobTitle}  job Has been deleted by the Employer..", NotificationType.info);
            }

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult GetProfile()
        {
            int id = int.Parse(_manager.GetUserId(HttpContext.User));
            ApplicantProfile profile = _context.ApplicantProfiles
                .Include(u => u.ApplicantEducations)
                .Include(u => u.ApplicantWorkHistorys)
                .Include(u => u.ApplicantSkills)
                .Include(u => u.Country)
                .Include(u => u.Province)
                .Include(u => u.City)
                .FirstOrDefault(u => u.RegisteredUserId == id);
            RegisteredUser user = _context.RegisteredUsers.Include(u => u.UserSectors).FirstOrDefault(u => u.Id == id);
            ViewBag.ProfilePicture = profile.ProfileImg;
            ApplicantProfileVM profileVM = new ApplicantProfileVM()
            {
                RegisteredUser = user,
                ApplicantProfile = profile

            };


            LookupVM lookup = new LookupVM();
            lookup.Sectors = _context.Sectors.ToList();
            List<Country> countries = _context.Countries.ToList();
            List<Province> provinces = _context.Provinces.Where(p => p.CountryId == profile.CountryId).ToList();
            List<City> cities = _context.Cities.Where(c => c.ProvinceId == profile.ProvinceId).ToList();


            lookup.Countries.Add(new SelectListItem() { Text = "Please Select", Value = "0" });
            foreach (var country in countries)
            {
                lookup.Countries.Add(new SelectListItem() { Value = country.Id.ToString(), Text = country.Name });
            }


            foreach (var province in provinces)
            {
                lookup.Provinves.Add(new SelectListItem() { Value = province.Id.ToString(), Text = province.ProvinceName });
            }

            foreach (var city in cities)
            {
                lookup.Cities.Add(new SelectListItem() { Value = city.Id.ToString(), Text = city.CityName });
            }

            lookup.Months = GetMonths();

            ViewData["Lookup"] = lookup;
            ViewData["UserFullName"] = profileVM.RegisteredUser.FirstName + " " + profileVM.RegisteredUser.LastName;
            return View(profileVM);
        }


        [HttpGet]
        public IActionResult ChangePassword()
        {

            ApplicantProfile profile = GetApplicantInfo();
            ViewData["UserFullName"] = profile.RegisteredUser.FirstName + " " + profile.RegisteredUser.LastName;
            ViewBag.ProfilePicture = profile.ProfileImg;

            return View();
        }



        [HttpPost]
        public IActionResult UpdateTestimonial(ApplicantTestimonial applicantTestimonial)
        {
            if (ModelState.IsValid)
            {
                int id = int.Parse(_manager.GetUserId(HttpContext.User));

                ApplicantTestimonial atVM = _context.ApplicantTestimonials
                                  .Include(ap => ap.ApplicantProfile.RegisteredUser)
                                  .Where(cj => cj.ApplicantProfile.RegisteredUser.Id == id).FirstOrDefault();


                atVM.Testimonial = applicantTestimonial.Testimonial;
                atVM.Description = applicantTestimonial.Description;

                _context.ApplicantTestimonials.Update(atVM);
                _context.SaveChanges();






            }

            return RedirectToAction("AddTestimonial", "ApplicantProfile");
        }



        [HttpGet]
        public IActionResult AddTestimonial()
        {
            int id = int.Parse(_manager.GetUserId(HttpContext.User));

            ApplicantTestimonial atVM = _context.ApplicantTestimonials
                                              .Include(ap => ap.ApplicantProfile.RegisteredUser)
                                              .Where(cj => cj.ApplicantProfile.RegisteredUser.Id == id).FirstOrDefault();

            return View(atVM);
        }


        [HttpPost]
        public async Task<IActionResult> AddTestimonial(string Testimonial, string Description)
        {
            int id = int.Parse(_manager.GetUserId(HttpContext.User));

            ApplicantProfile profile = GetApplicantInfo();



            ApplicantTestimonial testimonial = new ApplicantTestimonial();
            testimonial.ApplicantProfileId = profile.Id;
            testimonial.Testimonial = Testimonial.ToString();
            testimonial.Description = Description.ToString();

            _context.ApplicantTestimonials.Add(testimonial);
            await _context.SaveChangesAsync();


            return RedirectToAction("AddTestimonial", "ApplicantProfile");


        }



        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePassword model)
        {
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
                ApplicantProfile profile = GetApplicantInfo();




                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        if (error.Description == "Incorrect password.")
                        {
                            //ModelState.AddModelError(string.Empty, "Incorrect current password.");
                            AlertMessage("Incorrect Current Password. ", NotificationType.error);
                        }
                        else
                        {
                            //ModelState.AddModelError(string.Empty, error.Description);
                            AlertMessage("Incorrect Current Password. " + error.Description, NotificationType.error);
                        }

                    }



                    ViewData["UserFullName"] = profile.RegisteredUser.FirstName + " " + profile.RegisteredUser.LastName;
                    ViewBag.ProfilePicture = profile.ProfileImg;

                    return View();
                }

                // Send Password change confirmation Emal
                //using (MailMessage mailMessage = new MailMessage())
                //{
                //    mailMessage.From = new MailAddress("info@smsoftconsulting.com");
                //    mailMessage.Subject = "Your SMSS Account password has been reset.";
                //    mailMessage.Body = "Dear " + profile.RegisteredUser.FirstName + ",<br>" +
                //   $"We’ve reset your password for username : " + profile.RegisteredUser.UserName + "  <br/>" +
                //   $"Your new Password : " + model.NewPassword +
                //   $"<br><br> Thanks, <br> SMSS Support ";
                //    mailMessage.IsBodyHtml = true;
                //    mailMessage.To.Add(new MailAddress(profile.RegisteredUser.Email));
                //    System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient();
                //    smtp.UseDefaultCredentials = false;

                //    smtp.Host = "smtp.gmail.com";
                //    smtp.EnableSsl = true;

                //    System.Net.NetworkCredential networkcred = new System.Net.NetworkCredential();
                //    networkcred.UserName = "info@smsoftconsulting.com";
                //    networkcred.Password = "smsoftconsulting@123";
                //    smtp.Credentials = networkcred;

                //    smtp.Port = 587;
                //    await smtp.SendMailAsync(mailMessage);

                //}

                // Upon successfully changing the password refresh sign-in cookie
                //await _signInManager.RefreshSignInAsync(user);
                AlertMessage("Success!!! Hi " + profile.RegisteredUser.FirstName + " " + profile.RegisteredUser.LastName + " Your Password is Changed Successfully!!!..", NotificationType.success);

                //ViewData["UserFullName"] = profile.RegisteredUser.FirstName + " " + profile.RegisteredUser.LastName;
                //ViewBag.ProfilePicture = profile.ProfileImg;
                //return View("ChangePasswordConfirmation");
                await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
                return Redirect("/Login");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Resume()
        {
            int id = int.Parse(_manager.GetUserId(HttpContext.User));
            ApplicantProfile app = _context.ApplicantProfiles.FirstOrDefault(ap => ap.RegisteredUserId == id);
            string filename = Path.GetFileName(app.ResumeLocation);

            Resume r = new Resume();

            var displayfile = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Resumes");
            DirectoryInfo di = new DirectoryInfo(displayfile);
            FileInfo[] fileInfo = di.GetFiles();

            r.Resumefile = fileInfo.Where(fn => fn.Name == filename).ToArray();
            ApplicantProfile profile = GetApplicantInfo();
            ViewData["UserFullName"] = profile.RegisteredUser.FirstName + " " + profile.RegisteredUser.LastName;
            ViewBag.ProfilePicture = profile.ProfileImg;
            return View(r);
        }


        [HttpPost]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        public async Task<IActionResult> Resume(IFormFile Resumefile)
        {
            string ext = Path.GetExtension(Resumefile.FileName);
            string name = Path.GetFileNameWithoutExtension(Resumefile.FileName);
            int id = int.Parse(_manager.GetUserId(HttpContext.User));
            string pathroot = _iweb.WebRootPath;

            try
            {
                if (ext == ".docx" || ext == ".doc" || ext == ".pdf")
                {
                    if (ModelState.IsValid)
                    {
                        foreach (var file in Request.Form.Files)
                        {
                            if (file.Length == 0)
                                ModelState.AddModelError("ModelError", "please provide valid file");

                            var fileName = (name + "_" + DateTime.Now.ToString("dd_MMM_yyyy_hhmmss") + Path.GetExtension(file.FileName)).Replace(" ", "_");
                            // var filepath = (pathroot + "\\Resumes\\" + fileName);
                            var filepath = ("\\wwwroot\\" + "\\Resumes\\" + fileName);
                            // 1) Upload file to any cloud stoarege or database 
                            using (var fileStream = file.OpenReadStream())
                            using (var ms = new MemoryStream())
                            {
                                await fileStream.CopyToAsync(ms);

                            }
                            // 2) Save file to local path in Resumes folder
                            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Resumes", fileName);
                            using (var stream = new FileStream(path, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }
                            // 3) Save path to Database
                            ApplicantProfile app = _context.ApplicantProfiles.FirstOrDefault(ap => ap.RegisteredUserId == id);
                            app.ResumeLocation = filepath;
                            _context.ApplicantProfiles.Update(app);
                            await _context.SaveChangesAsync();

                            //Resume obj = new Resume();
                            //obj.Resumefile = fileName.ToString();
                            //  var content = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("ModelError", ModelState.FirstOrDefault().Value.Errors.FirstOrDefault().ErrorMessage);
                    }

                }
                else
                {
                    ViewBag.Message = "File extension is not valid. ";
                }

            }
            catch
            {
                //do something
                AlertMessage("Some Error has occured for Resume", NotificationType.error);
            }


            return RedirectToAction("Resume");
        }

        public IActionResult Delete(string filedelete)
        {

            filedelete = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Resumes", filedelete);
            FileInfo fi = new FileInfo(filedelete);

            if (fi != null)
            {
                System.IO.File.Delete(filedelete);
                fi.Delete();
            }
            return RedirectToAction("Resume");
        }


        [HttpGet]
        //[Route("download")]
        public async Task<ActionResult> GetPdf([FromQuery] string filename)
        {
            string fullpath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Resumes", filename);

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

        [HttpPost]
        public JsonResult AjaxMethod(string type, int value)
        {
            LookupVM model = new LookupVM();
            switch (type)
            {
                case "ApplicantProfile_CountryId":
                    List<Province> provinces = _context.Provinces.Where(p => p.CountryId == value).ToList();
                    foreach (var province in provinces)
                    {
                        model.Provinves.Add(new SelectListItem() { Value = province.Id.ToString(), Text = province.ProvinceName });
                    }


                    break;
                case "ApplicantProfile_ProvinceId":
                    List<City> cities = _context.Cities.Where(c => c.ProvinceId == value).ToList();
                    foreach (var city in cities)
                    {
                        model.Cities.Add(new SelectListItem() { Value = city.Id.ToString(), Text = city.CityName });
                    }

                    break;
            }
            return Json(model);
        }



        [HttpPost]
        public IActionResult AddSector(int[] SelectedSectors)
        {
            int userId = int.Parse(_manager.GetUserId(HttpContext.User));

            List<UserSector> newUserSectors = new List<UserSector>();
            List<UserSector> existUserSectors = new List<UserSector>();

            foreach (int sectorId in SelectedSectors)
            {
                newUserSectors.Add(new UserSector { RegisteredUserId = userId, SectorId = sectorId });
            }
            existUserSectors = _context.UserSectors.Where(us => us.RegisteredUserId == userId).ToList();
            _context.UserSectors.RemoveRange(existUserSectors);
            _context.UserSectors.AddRange(newUserSectors);
            _context.SaveChanges();

            return RedirectToAction("GetProfile");


        }

        [HttpPost]
        public IActionResult UpdateUserDetails(IFormCollection userDetails)
        {
            int id = int.Parse(_manager.GetUserId(HttpContext.User));
            RegisteredUser curUser = _context.RegisteredUsers.FirstOrDefault(ru => ru.Id == id);
            curUser.FirstName = userDetails["RegisteredUser.FirstName"].ToString();
            curUser.LastName = userDetails["RegisteredUser.LastName"].ToString();
            curUser.UserPhone = userDetails["RegisteredUser.UserPhone"].ToString();
            curUser.ResidencyStatus = (EnumResidencyStatus)Enum.Parse(typeof(EnumResidencyStatus), userDetails["RegisteredUser.ResidencyStatus"].ToString());

            _context.SaveChanges();
            return RedirectToAction("GetProfile");
        }

        [HttpPost]

        [HttpPost]

        public IActionResult UpdateApplicantInfo(ApplicantProfile applicantProfile)
        {
            if (ModelState.IsValid)
            {
                byte[] uploadLogo = null;
                int id = applicantProfile.Id;
                ApplicantProfile curApplicant = _context.ApplicantProfiles.FirstOrDefault(ap => ap.Id == id);
                curApplicant.JobTitle = applicantProfile.JobTitle;
                curApplicant.AcademicLevel = applicantProfile.AcademicLevel;
                curApplicant.Gender = applicantProfile.Gender;
                curApplicant.LinkedIn = applicantProfile.LinkedIn;
                curApplicant.GitHub = applicantProfile.GitHub;
                curApplicant.CountryId = applicantProfile.CountryId == null ? null : applicantProfile.CountryId;
                curApplicant.ProvinceId = applicantProfile.ProvinceId == 0 ? null : applicantProfile.ProvinceId;
                curApplicant.CityId = applicantProfile.CityId == 0 ? null : applicantProfile.CityId;
                curApplicant.PostalCode = applicantProfile.PostalCode;
                curApplicant.Street = applicantProfile.Street;
                if (applicantProfile.ImageFile != null)
                {
                    using (var rStream = applicantProfile.ImageFile.OpenReadStream())
                    using (var mStream = new MemoryStream())
                    {
                        rStream.CopyTo(mStream);
                        uploadLogo = mStream.ToArray();
                    }
                    curApplicant.ProfileImg = uploadLogo;

                }
                _context.SaveChanges();
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



            return RedirectToAction("GetProfile");
        }

        private List<SelectListItem> GetMonths()
        {
            List<SelectListItem> months = new List<SelectListItem>
            {
                new SelectListItem{Text = "Not Selected" , Value = ""},
                new SelectListItem{Text = "Jan" , Value = "1"},
                new SelectListItem{Text = "Feb" , Value = "2"},
                new SelectListItem{Text = "Mar" , Value = "3"},
                new SelectListItem{Text = "Apr" , Value = "4"},
                new SelectListItem{Text = "May" , Value = "5"},
                new SelectListItem{Text = "June" , Value = "6"},
                new SelectListItem{Text = "July" , Value = "7"},
                new SelectListItem{Text = "Aug" , Value = "8"},
                new SelectListItem{Text = "Sept" , Value = "9"},
                new SelectListItem{Text = "Oct" , Value = "10"},
                new SelectListItem{Text = "Nov" , Value = "11"},
                new SelectListItem{Text = "Dec" , Value = "12"}

            };

            return months;
        }

        private ApplicantProfile GetApplicantInfo()
        {
            int id = int.Parse(_manager.GetUserId(HttpContext.User));
            ApplicantProfile profile = _context.ApplicantProfiles
                .Include(ap => ap.RegisteredUser)
                .AsNoTracking()
                .FirstOrDefault(u => u.RegisteredUserId == id);
            return profile;
        }



        [HttpGet]
        public IActionResult DeleteAccount()
        {
            int id = int.Parse(_manager.GetUserId(HttpContext.User));
            ApplicantProfile profile = _context.ApplicantProfiles
                .Include(u => u.ApplicantEducations)
                .Include(u => u.ApplicantWorkHistorys)
                .Include(apjs => apjs.ApplicantJobApplications)
                .ThenInclude(cj => cj.CompanyJob)
                .ThenInclude(cp => cp.CompanyProfile)
                .Include(apt => apt.ApplicantTestimonials)
                .Include(u => u.ApplicantSkills)
                .Include(u => u.Country)
                .Include(u => u.Province)
                .Include(u => u.City)
                .FirstOrDefault(u => u.RegisteredUserId == id);

            RegisteredUser user = _context.RegisteredUsers.Include(u => u.UserSectors).FirstOrDefault(u => u.Id == id);


            string filename = Path.GetFileName(profile.ResumeLocation);

            Resume r = new Resume();

            var displayfile = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Resumes");
            DirectoryInfo di = new DirectoryInfo(displayfile);
            FileInfo[] fileInfo = di.GetFiles();

            r.Resumefile = fileInfo.Where(fn => fn.Name == filename).ToArray();

            ViewBag.ProfilePicture = profile.ProfileImg;

            ApplicantProfileVM profileVM = new ApplicantProfileVM()
            {
                RegisteredUser = user,
                ApplicantProfile = profile,
                Resumefile = r.Resumefile

            };


            LookupVM lookup = new LookupVM();
            lookup.Sectors = _context.Sectors.ToList();
            List<Country> countries = _context.Countries.ToList();
            List<Province> provinces = _context.Provinces.Where(p => p.CountryId == profile.CountryId).ToList();
            List<City> cities = _context.Cities.Where(c => c.ProvinceId == profile.ProvinceId).ToList();


            lookup.Countries.Add(new SelectListItem() { Text = "Please Select", Value = "0" });
            foreach (var country in countries)
            {
                lookup.Countries.Add(new SelectListItem() { Value = country.Id.ToString(), Text = country.Name });
            }


            foreach (var province in provinces)
            {
                lookup.Provinves.Add(new SelectListItem() { Value = province.Id.ToString(), Text = province.ProvinceName });
            }

            foreach (var city in cities)
            {
                lookup.Cities.Add(new SelectListItem() { Value = city.Id.ToString(), Text = city.CityName });
            }

            lookup.Months = GetMonths();

            ViewData["Lookup"] = lookup;
            ViewData["UserFullName"] = profileVM.RegisteredUser.FirstName + " " + profileVM.RegisteredUser.LastName;

            return View(profileVM);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmAccountDelete(IFormCollection formData)

        {

            var UserId = int.Parse(formData["ApplicantProfile.RegisteredUser.Id"]);
            ApplicantProfile profile = _context.ApplicantProfiles
               .Include(u => u.ApplicantEducations)
               .Include(u => u.ApplicantWorkHistorys)
               .Include(apjs => apjs.ApplicantJobApplications)
               .ThenInclude(cj => cj.CompanyJob)
               .ThenInclude(cp => cp.CompanyProfile)
               .Include(apt => apt.ApplicantTestimonials)
               .Include(u => u.ApplicantSkills)
               .Include(u => u.Country)
               .Include(u => u.Province)
               .Include(u => u.City)
               .FirstOrDefault(u => u.RegisteredUserId == UserId);
            if (profile != null)
            {
                if (profile.ApplicantEducations.Count() > 0)
                {
                    var AppEdu = _context.ApplicantEducations.FirstOrDefault(ae => ae.ApplicantProfileId == profile.Id);
                    _context.ApplicantEducations.RemoveRange(_context.ApplicantEducations.Where(x => x.Id == AppEdu.Id));
                }
                if (profile.ApplicantSkills.Count() > 0)
                {
                    var AppSkill = _context.ApplicantSkills.FirstOrDefault(ask => ask.ApplicantProfileId == profile.Id);
                    _context.ApplicantSkills.RemoveRange(_context.ApplicantSkills.Where(x => x.Id == AppSkill.Id));
                }
                if (profile.ApplicantTestimonials.Count() > 0)
                {
                    var AppTestimonial = _context.ApplicantTestimonials.FirstOrDefault(atst => atst.ApplicantProfileId == profile.Id);
                    _context.ApplicantTestimonials.RemoveRange(_context.ApplicantTestimonials.Where(x => x.Id == AppTestimonial.Id));
                }

                if (profile.ApplicantWorkHistorys.Count() > 0)
                {
                    var AppWorkHistory = _context.ApplicantWorkHistories.FirstOrDefault(atst => atst.ApplicantProfileId == profile.Id);
                    _context.ApplicantWorkHistories.RemoveRange(_context.ApplicantWorkHistories.Where(x => x.Id == AppWorkHistory.Id));
                }
                if (profile.ApplicantJobApplications.Count() > 0)
                {
                    var AppJobapplication = _context.ApplicantJobApplications.FirstOrDefault(atst => atst.ApplicantProfileId == profile.Id);
                    _context.ApplicantJobApplications.RemoveRange(_context.ApplicantJobApplications.Where(x => x.Id == AppJobapplication.Id));
                }

                var applicant = _context.ApplicantProfiles.FirstOrDefault(x => x.Id == profile.Id);
                 _context.ApplicantProfiles.Remove(applicant);

                var User = _context.RegisteredUsers.FirstOrDefault(x => x.Id == UserId);
                var deleted = _context.RegisteredUsers.Remove(User);

                if (deleted != null)
                {
                    //var User = await UserManager.FindByIdAsync(UserId.ToString());

                    
                    AlertMessage("You Have DELETED your Account Successfully!!!..", NotificationType.info);

                   

                    IdentityResult result = await _manager.DeleteAsync(User);
                    //result= await _manager.RemoveClaimAsync(User) 

                    if (result.Succeeded)
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("Login");

                    }
                    else
                    {
                        return View("/Error",result);
                    }
                    //return View("~/Views/Shared/_layout.cshtml","Index");
                }
                else
                {
                    AlertMessage("Opps!!!.. Account Could not be DELETED!", NotificationType.error);
                }
            }
            return RedirectToAction("Index", "Home");

        }

    }
}