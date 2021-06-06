using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SMEnterprise.Models
{
    public class ApiModels
    {
    }
    public static class ApiRole
    {
        public const string Principal = "8";
        public const string Teacher = "3";
        public const string Parent = "4";
    }
    public class LoginModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string password { get; set; }

        [Required(ErrorMessage = "Enter User name")]
        public string username { get; set; }

        public string salt { get; set; }
        public string UUID { get; set; }
        public string deviceType { get; set; }
        public string deviceToken { get; set; }
    }
}