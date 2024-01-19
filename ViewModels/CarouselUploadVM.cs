using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SMSS.ViewModels
{
  public class CarouselUploadVM
  {

    //[Required]
    [Display(Name = "Carousel Slider Image")]
    public IFormFile CImage { get; set; }
  }
}
