using SMSS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMSS.ViewModels
{
    public class DashboardViewDetailsVM
    {
        public IEnumerable<ApplicantJobApplication> applicantJobApplications { get; set; }
     // public ApplicantJobApplication jobApplication { get; set; }
        public IEnumerable<CompanyJob> companyJobs { get; set; }
        public Resume resume { get; set; }
        
    }
}
