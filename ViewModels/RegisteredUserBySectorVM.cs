using SMSS.Models;
using System;
using System.Collections.Generic;


namespace SMSS.ViewModels
{
    public class RegisteredUserBySectorVM
    {
        public int SectorId { get; set; }
        public string SectorName { get; set; }
        public int UserId { get; set; }
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public string Email { get; set; }
        public string UserPhone { get; set; }
        public EnumResidencyStatus ResidencyStatus { get; set; }
        public DateTime RegistrationDate { get; set; }
    }
}
