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
    public class ApplicantSkillController : BaseController
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<RegisteredUser> _manager;

        public ApplicantSkillController(ApplicationDbContext context, UserManager<RegisteredUser> manager)
        {
            _context = context;
            _manager = manager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddSkill(IFormCollection skillForm)
        {
            int id = int.Parse(_manager.GetUserId(HttpContext.User));

            if (skillForm != null)
            {
                ApplicantSkill skill = new()
                {
                    ApplicantProfileId = _context.ApplicantProfiles.FirstOrDefault(u => u.RegisteredUserId == id).Id,
                    SkillName = skillForm["ApplicantSkill.SkillName"].ToString()
                };
                _context.ApplicantSkills.Add(skill);
                AlertMessage("You Have ADDED " + skill.SkillName + " Skill Successfully!!!..", NotificationType.success);
            }
            else
            {
                // We Direct if there's an error
                AlertMessage("Opps!!!.. Applicant Skill Could not be ADDED!",  NotificationType.error);

            }
            _context.SaveChanges();
            return RedirectToAction("GetProfile", "ApplicantProfile");
        }

        [HttpPost]
        public IActionResult EditSkill(IFormCollection skillForm)
        {
            int id = int.Parse(_manager.GetUserId(HttpContext.User));

            var skill = _context.ApplicantSkills.FirstOrDefault(u =>
            u.Id == Convert.ToInt16(skillForm["ApplicantProfile.ApplicantSkills.skill.Id"].ToString()));
            if (skillForm != null)
            {
                skill.SkillName = skillForm["ApplicantProfile.ApplicantSkills.skill.SkillName"].ToString();
                _context.Update(skill);
                AlertMessage("You have UPDATED " + skill.SkillName + " Skill Successfully!!!..", NotificationType.success);
            }
            else
            {
                //do somthing
                AlertMessage("Opps!!!.. Applicant Skill Could not be UPDATED!", NotificationType.error);

            }
            _context.SaveChanges();
            return RedirectToAction("GetProfile", "ApplicantProfile");
        }

      

        [HttpPost]
        public IActionResult RemoveSkill(IFormCollection skillForm)
        {
            int id = int.Parse(_manager.GetUserId(HttpContext.User));
            var skill = _context.ApplicantSkills.FirstOrDefault(u =>
            u.Id == Convert.ToInt16(skillForm["ApplicantProfile.ApplicantSkills.skill.Id"].ToString()));
            if (skill != null)
            {
                _context.ApplicantSkills.Remove(skill);
                AlertMessage("Success!!! You Have DELETED " + skill.SkillName + " Skill Successfully!!!",  NotificationType.success);

            }
            else
            {
                AlertMessage("Opps!!!.. Applicant Skill Could not be DELETED!", NotificationType.error);
            }
            _context.SaveChanges();
            return RedirectToAction("GetProfile", "ApplicantProfile");
        }

    }
}
