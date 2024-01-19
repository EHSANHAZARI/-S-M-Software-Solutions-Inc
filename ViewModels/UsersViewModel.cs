

using Microsoft.AspNetCore.Identity;
using SMSS.Models;
using System.Collections.Generic;

namespace SMSS.ViewModels
{
    public class UsersViewModel
    {
        public virtual ICollection<IdentityUser> IdentityUsers { get; set; }
        public virtual ICollection<ApplicantProfile> ApplicantProfiles { get; set; }

        public virtual ICollection<ApplicantJobApplication> ApplicantJobApplications { get; set; }

        public virtual ICollection<CompanyProfile> CompanyProfiles { get; set; }

        public virtual ICollection<CompanyJob> CompanyJobs { get; set; }
    }
}
