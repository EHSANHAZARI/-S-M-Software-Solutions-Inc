using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMSS.ViewModels
{
    public class EnrolledCandidatesCountVM
    {
        public int Id { get; set; }
        [Required]
        [Display(Name = "Current No. Of Candidates Enrolled")]
        public int CurrentNumOfEnrolledCandidates { get; set; }

        [Display(Name = "Date Updated")]
        public DateTime DateUpdated { get; set; }
    }
}
