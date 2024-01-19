using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SMSS.Data;
using SMSS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace SMSS.Controllers
{
    [Authorize(AuthenticationSchemes = "Identity.Application", Roles = "User")]
    public class ApplicantWorkHistoryController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<RegisteredUser> _manager;

        public ApplicantWorkHistoryController(ApplicationDbContext context, UserManager<RegisteredUser> manager)
        {
            _context = context;
            _manager = manager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddWorkHistory(IFormCollection workForm)
        {
            int id = int.Parse(_manager.GetUserId(HttpContext.User));
            if (workForm != null)
            {

                ApplicantWorkHistory work = new ApplicantWorkHistory()
                {
                        ApplicantProfileId = _context.ApplicantProfiles.FirstOrDefault(u=>u.RegisteredUserId == id).Id,
                        CompanyName = workForm["ApplicantWorkHistory.CompanyName"].ToString(),
                        Location = workForm["ApplicantWorkHistory.Location"].ToString(),
                        JobTitle = workForm["ApplicantWorkHistory.JobTitle"].ToString(),
                        JobDescription = workForm["ApplicantWorkHistory.JobDescription"].ToString(),
                        StartMonth = Convert.ToInt16(workForm["StartMonth"].ToString()),
                        StartYear = Convert.ToInt32(workForm["StartYear"].ToString()),
                        EndMonth = (workForm["EndMonth"].ToString()=="") ? null : Convert.ToInt16(workForm["EndMonth"].ToString()),
                        EndYear = (workForm["EndYear"].ToString()=="")? null : Convert.ToInt32(workForm["EndYear"].ToString()),
                        IsPresent = Convert.ToBoolean(workForm["ApplicantWorkHistory.IsPresent"].ToString().Split(',')[0])


                };
                _context.ApplicantWorkHistories.Add(work);
            }
            else
            {
                //do somthing

            }

            _context.SaveChanges();
            return RedirectToAction("GetProfile", "ApplicantProfile");
        }

        [HttpPost]
        public IActionResult EditWorkHistory(IFormCollection workForm)
        {
            int id = int.Parse(_manager.GetUserId(HttpContext.User));
            var work = _context.ApplicantWorkHistories.FirstOrDefault(u =>
            u.Id == Convert.ToInt16(workForm["ApplicantProfile.ApplicantWorkHistorys.work.Id"].ToString()));
            if (workForm != null)
            {
                work.CompanyName = workForm["ApplicantProfile.ApplicantWorkHistorys.work.CompanyName"].ToString();
                work.Location = workForm["ApplicantProfile.ApplicantWorkHistorys.work.Location"].ToString();
                work.JobTitle = workForm["ApplicantProfile.ApplicantWorkHistorys.work.JobTitle"].ToString();
                work.JobDescription = workForm["ApplicantProfile.ApplicantWorkHistorys.work.JobDescription"].ToString();
                work.StartMonth = Convert.ToInt16(workForm["StartMonth"].ToString());
                work.StartYear = Convert.ToInt32(workForm["StartYear"].ToString());
                work.EndMonth = (workForm["EndMonth"].ToString()=="") ? null : Convert.ToInt16(workForm["EndMonth"].ToString());
                work.EndYear = (workForm["EndYear"].ToString() == "") ? null : Convert.ToInt32(workForm["EndYear"].ToString());
                work.IsPresent = Convert.ToBoolean(workForm["ApplicantProfile.ApplicantWorkHistorys.work.IsPresent"].ToString().Split(',')[0]);
                _context.Update(work);
            }
            else
            {
                //do somthing

            }
            _context.SaveChanges();

            return RedirectToAction("GetProfile", "ApplicantProfile");
        }

        [HttpPost]
        public IActionResult RemoveWorkHistory(IFormCollection workForm)
        {
            int id = int.Parse(_manager.GetUserId(HttpContext.User));
            var work = _context.ApplicantWorkHistories.FirstOrDefault(u =>
            u.Id == Convert.ToInt16(workForm["ApplicantProfile.ApplicantWorkHistorys.work.Id"].ToString()));
            if (work != null)
            {
                _context.ApplicantWorkHistories.Remove(work);
            }

            _context.SaveChanges();

            return RedirectToAction("GetProfile", "ApplicantProfile");
        }

    }
}
