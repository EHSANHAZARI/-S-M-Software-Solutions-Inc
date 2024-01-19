using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SMSS.ViewModels
{
    public class UlploadHiringClientLogoVM
    {
        [Required]
        [Display(Name = "Hiring Client's Logo")]
        public IFormFile ClientLogo { get; set; }
    }
}
