using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SMSS.ViewModels
{
  public class CarouselEditVM : CarouselUploadVM
  {
    public int Id { get; set; }
    public string  ExistingImage { get; set; }
  }
}
