using Microsoft.AspNetCore.Identity;
using System;

namespace Task3_5.Models
{
    public class User : IdentityUser
    {
        public string Name { get; set; }
        public DateTime DateRegister { get; set; }
        public DateTime LastLogin { get; set; }

        public bool Status { get; set; }

        public bool Flag { get; set; }
    }
}