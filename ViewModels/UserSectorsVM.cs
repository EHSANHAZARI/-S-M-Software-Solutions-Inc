using Microsoft.AspNetCore.Mvc.Rendering;
using SMSS.Models;
using SMSS.Models.DataAnnotaions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SMSS.ViewModels
{
    public class UserSectorsVM
    {
        public virtual IEnumerable<RegisteredUser> RegisteredUser { get; set; }
        public virtual IEnumerable<ApplicantProfile> ApplicantProfile { get; set; }
        public virtual IEnumerable<Sector> Sectors { get; set; }
        public virtual ICollection<UserSector> UserSectors { get; set; }
    }
}