using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebApp.Models
{
    public class ClaimStore
    {
        public static List<Claim> AllClaims = new List<Claim>()
        {
             new Claim("Edit Role", "Edit Role"),
             new Claim("Create Role", "Create Role"),
             new Claim("Delete Role", "Delete Role") 

        };
    }
}
