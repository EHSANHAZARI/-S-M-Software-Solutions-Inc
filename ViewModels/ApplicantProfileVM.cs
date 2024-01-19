using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SMSS.Models;



namespace SMSS.ViewModels
{
    public class ApplicantProfileVM
    {
        public RegisteredUser RegisteredUser { get; set; }
        public ApplicantProfile  ApplicantProfile { get; set; }

        public ApplicantEducation ApplicantEducation { get; set; }

        public ApplicantSkill ApplicantSkill { get; set; }
        public ApplicantWorkHistory ApplicantWorkHistory { get; set; }

        public ApplicantTestimonial ApplicantTestimonial { get; set; }

        public FileInfo[] Resumefile { get; set; }

    }
}
