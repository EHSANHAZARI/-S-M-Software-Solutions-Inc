using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.WebPages.Html;
using SMSS.Models;

namespace SMSS.ViewModels
{
    public class JobApplicantsList
    {
        public JobApplicantsList()
        {
            this.applicantJobApplications = new List<ApplicantJobApplication>();
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
