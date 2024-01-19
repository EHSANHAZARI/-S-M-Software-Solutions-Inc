using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SMSS.Data;
using SMSS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMSS.Controllers
{
    [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "User")]
    public class ApplicantEducationController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<RegisteredUser> _manager;

        public ApplicantEducationController(ApplicationDbContext context, UserManager<RegisteredUser> manager)
        {
            _context = context;
            _manager = manager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddEducation(IFormCollection educationForm)
        {
            int id = int.Parse(_manager.GetUserId(HttpContext.User));
            if (educationForm != null)
            {

                ApplicantEducation education = new ApplicantEducation()
                {
                    ApplicantProfileId = _context.ApplicantProfiles.FirstOrDefault(u => u.RegisteredUserId == id).Id,
                    Major = educationForm["ApplicantEducation.Major"].ToString(),
                    CertificateDiploma = educationForm["ApplicantEducation.CertificateDiploma"].ToString(),
                    StartYear = Convert.ToInt16(educationForm["StartYear"].ToString()),
                    CompletionYear = (educationForm["CompletionYear"].ToString() == "") ? null : Convert.ToInt16(educationForm["CompletionYear"].ToString()),
                    IsInProgress =  Convert.ToBoolean(educationForm["ApplicantEducation.IsInProgress"].ToString().Split(',')[0])

            };
                _context.ApplicantEducations.Add(education);
            }
            else
            {
                //do somthing

            }

            _context.SaveChanges();
            return RedirectToAction("GetProfile", "ApplicantProfile");
        }

        [HttpPost]
        public IActionResult EditEducation(IFormCollection educationForm)
        {
            int id = int.Parse(_manager.GetUserId(HttpContext.User));
            var education = _context.ApplicantEducations.FirstOrDefault(u =>
            u.Id == Convert.ToInt16(educationForm["ApplicantProfile.ApplicantEducations.edu.Id"].ToString()));
            if (educationForm != null)
            {
                education.Major = educationForm["ApplicantProfile.ApplicantEducations.edu.Major"].ToString();
                education.CertificateDiploma = educationForm["ApplicantProfile.ApplicantEducations.edu.CertificateDiploma"].ToString();
                education.StartYear = Convert.ToInt16(educationForm["StartYear"].ToString());
                education.CompletionYear = (educationForm["CompletionYear"].ToString() == "") ? null : Convert.ToInt16(educationForm["CompletionYear"].ToString());
                education.IsInProgress = Convert.ToBoolean(educationForm["ApplicantProfile.ApplicantEducations.edu.IsInProgress"].ToString().Split(',')[0]);

                _context.Update(education);
            }
            else
            {
                //do somthing

            }
            _context.SaveChanges();

            return RedirectToAction("GetProfile", "ApplicantProfile");
        }

        [HttpPost]
        public IActionResult RemoveEducation(IFormCollection educationForm)
        {
            int id = int.Parse(_manager.GetUserId(HttpContext.User));
            var education = _context.ApplicantEducations.FirstOrDefault(u =>
            u.Id == Convert.ToInt16(educationForm["ApplicantProfile.ApplicantEducations.edu.Id"].ToString()));
            if (education != null)
            {
                _context.ApplicantEducations.Remove(education);
            }

            _context.SaveChanges();

            return RedirectToAction("GetProfile", "ApplicantProfile");
        }

    }
}
