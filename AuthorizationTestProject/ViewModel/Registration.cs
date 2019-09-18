using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.V3.Pages.Account.Internal;
using Microsoft.AspNetCore.Mvc;

namespace AuthorizationTestProject.ViewModel
{
    public class Registration
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        [Remote(action: "IsEmailInUse",controller:"Account")]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string  Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password",ErrorMessage = "Password and confirm does not match!")]
        [Display(Name ="Confirm Password")]
        public string ConfirmPassword { get; set; }

        public string City { get; set; }
    }
}
