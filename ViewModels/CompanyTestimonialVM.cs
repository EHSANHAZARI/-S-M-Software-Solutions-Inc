using SMSS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMSS.ViewModels
{
    public class CompanyTestimonialVM
    {

        public List<CompanyJob> CompanyJobs { get; set; }
        public List<ApplicantTestimonial> ApplicantTestimonial { get; set; }

        public List<CarouselSliderImage> CarouselSliderImages { get; set; }

        public List<JobMode> JobModes { get; set; }

        public List<RegisteredUser> RegisteredUsers { get; set; }

        public List<UserSector> UserSectors { get; set; }

        public List<Sector> Sectors { get; set; }

        public List<ApplicantProfile> ApplicantProfiles { get; set; }

        public List<HiringClientLogo> HiringClientLogos { get; set; }

       public EnrolledCandidatesCount EnrolledCandidatesCount { get; set; }
    }
}
