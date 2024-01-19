using Microsoft.AspNetCore.Http;
using SMSS.Models;
using System.Collections.Generic;

namespace SMSS.ViewModels
{
    public class UnsubscribeUserVM
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        public List<UnsubscribeUser> UnsubscribeUsers { get; set; }
    }
}
