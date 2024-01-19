using Microsoft.AspNetCore.Mvc.Rendering;
using SMSS.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SMSS.ViewModels
{
    public class AllApplicantsVM
    {
        public AllApplicantsVM()
        {
            this.JobsList = new List<SelectListItem>();
            this.JobApplicantsLists = new List<JobApplicantsList>();
        }
        public List<SelectListItem> JobsList { get; set; }
        public int JobId { get; set; }
        [Required]
        [Display(Name = "Subject")]
        [StringLength(100, MinimumLength = 10, ErrorMessage = "{0} Length Must Be between {2} and {1} character")]
        public string Subject { get; set; }
        [Required]
        [Display(Name = "Message")]
        [StringLength(500, MinimumLength = 20 , ErrorMessage = "{0} Length Must Be between {2} and {1} character")]
        public string Message { get; set; }
        public int ApplicantsCount { get; set; }

        public List<JobApplicantsList> JobApplicantsLists { get; set; }

        public virtual ICollection<ApplicantJobApplication> ApplicantJobApplications { get; set; }

        public IEnumerable<CompanyProfileVM> CompanyProfileVMs { get; set; }
    }
}
