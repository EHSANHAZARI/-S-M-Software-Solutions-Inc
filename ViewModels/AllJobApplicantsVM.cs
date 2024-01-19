using Microsoft.AspNetCore.Mvc.Rendering;
using SMSS.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SMSS.ViewModels
{
    public class AllJobApplicantsVM
    {
        public AllJobApplicantsVM()
        {
            this.applicantJobApplications = new List<ApplicantJobApplication>();
            this.JobsList = new List<SelectListItem>();
            this.ApplicantProfiles = new List<ApplicantProfile>();
        }
        public int ApplicantsCount { get; set; }
        public string JobTitle { get; set; }
        public Byte[] ProfileImg { get; set; }
        public List<SelectListItem> JobsList { get; set; }

        public List<ApplicantJobApplication> applicantJobApplications { get; set; }

        public virtual ICollection<ApplicantProfile> ApplicantProfiles { get; set; }
        public int id { get; internal set; }




    }
}
