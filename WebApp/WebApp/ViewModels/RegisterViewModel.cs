using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Utility;

namespace WebApp.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [ValidEmailDomainAttribute(allowDomain: "ab.com", ErrorMessage ="Email domain must be ab.com.")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name ="Confirm Password")]
        [Compare("Password",ErrorMessage ="Password and Confirm Password do not match.")]
        public string ConfirmPassword { get; set; }

        public string City { get; set; }
    }
}
