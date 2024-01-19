using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMSS.ViewModels
{
    public class HiringClientLogoVM : EditHiringClientLogoVM
    {
        [Required(ErrorMessage = "Enter Client's Name")]
        [Display(Name = "Client's Name")]
        [StringLength(255)]
        public string Client_Name { get; set; }

        [Required(ErrorMessage = "Enter Client's Province")]
        [Display(Name = "Client's Province")]
        [StringLength(255)]
        public string Client_Province { get; set; }

        [Display(Name = "Date Added")]
        [Column("Date Added")]
        public DateTime DateAdded { get; set; }
    }
}
