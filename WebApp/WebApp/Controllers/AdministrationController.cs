using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApp.Models;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    //[Authorize(Roles ="Admin")]
    //[AllowAnonymous]
    [Authorize(Policy = "AdminRolePolicy")]
    public class AdministrationController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;

        public AdministrationController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;  
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Policy = "CreateRolePolicy")]
        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [Authorize(Policy = "CreateRolePolicy")]
        public async Task<IActionResult> Create(CreateRoleViewModel model)
        {
            if(ModelState.IsValid)
            {
                IdentityRole role = new IdentityRole
                {
                    Name = model.RoleName
                };

                var result = await roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    return RedirectToAction("ListRoles","Administration");
                }
                foreach(var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View();
        }


        [HttpGet]
        public IActionResult ListRoles()
        {
            var roles = roleManager.Roles;
            return View(roles);
        }

        
        [HttpGet]
        public async Task<IActionResult> EditRole(string id)
        {
            var role = await roleManager.FindByIdAsync(id);
            if(role==null)
            {
                ViewBag.ErrorMessage = $"role Id = {id} not found";
                return View("NotFound");
            }

            var model = new EditRoleViewModel
            {
                Id = role.Id,
                RoleName = role.Name
            };

            foreach(var user in userManager.Users.ToList())
            { 
                if (await userManager.IsInRoleAsync(user, role.Name))
                {
                    model.Users.Add(user.UserName);
                }

            }


            return View(model);
          
        }

        [HttpPost]
        public async Task<IActionResult> EditRole(EditRoleViewModel model)
        {
            var role = await roleManager.FindByIdAsync(model.Id);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"role Id = {model.Id} not found";
                return View("NotFound");
            }
            else
            {
                role.Name = model.RoleName;
                var result = await roleManager.UpdateAsync(role);
                if(result.Succeeded)
                {
                    return RedirectToAction("ListRoles");
                }
               foreach(var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditUserInRole(string roleId)
        {
            ViewBag.roleId = roleId;
            var role = await roleManager.FindByIdAsync(roleId);
            if(role==null)
            {
                ViewBag.ErrorMessage = $"Role with RoleId : {roleId} cannot be found";
                return RedirectToAction("NotFound");
            }

            var model = new List<UserRoleViewModel>();
            //Adding all user to userRoleViewModel formate
            foreach(var user in userManager.Users.ToList())
            {
                var userRoleViewModel = new UserRoleViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName
                };

                if(await userManager.IsInRoleAsync(user, role.Name))
                {
                    userRoleViewModel.IsSelected = true;
                }
                else
                {
                    userRoleViewModel.IsSelected = false;
                }

                model.Add(userRoleViewModel);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUserInRole(List<UserRoleViewModel> model, string roleId)
        {
            var role = await roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with RoleId : {roleId} cannot be found";
                return RedirectToAction("NotFound");
            }

            for(int i =0; i<model.Count; i++)
            {
                IdentityResult result = null;
                var user = await userManager.FindByIdAsync(model[i].UserId);
                if(model[i].IsSelected && !(await userManager.IsInRoleAsync(user, role.Id)))
                {
                    result = await userManager.AddToRoleAsync(user, role.Name);
                }
                else if(!(model[i].IsSelected && !(await userManager.IsInRoleAsync(user, role.Id))))
                {
                    result = await userManager.RemoveFromRoleAsync(user, role.Name);
                }
                else
                {
                    continue;
                }

                /*if (i < (model.Count - 1))
                    continue;
                else
                    return RedirectToAction("EditRole", new { Id = role.Id });*/
            }


            return RedirectToAction("EditRole", new { Id = role.Id });
        }


        [HttpGet]
        [Authorize(Policy = "EditRolePolicy")]
        public async Task<IActionResult> ManageUserRole(string userId)
        {
            ViewBag.userId = userId;
            var user = await userManager.FindByIdAsync(userId);
            if(user==null)
            {
                ViewBag.ErrorMessage = $"User with user Id : {userId} cannot be found";
                return View("NotFound");
            }
            var model = new List<UserRolesViewModel>();

            foreach(var role in roleManager.Roles.ToList())
            {
                UserRolesViewModel userRolesViewModel = new UserRolesViewModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name
                };
                if(await userManager.IsInRoleAsync(user , role.Name))
                {
                    userRolesViewModel.IsSelected = true;
                }
                else
                {
                    userRolesViewModel.IsSelected = false;
                }

                model.Add(userRolesViewModel);
            }
            
            return View(model);
        }


        [HttpPost]
        [Authorize(Policy = "EditRolePolicy")]
        public async Task<IActionResult> ManageUserRole(List<UserRolesViewModel> model, string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if(user == null)
            {
                ViewBag.ErrorMessage = $"user with user Id : {userId} cannot be found";
                return View("NotFound");
            }

            var roles = await userManager.GetRolesAsync(user);
            var result = await userManager.RemoveFromRolesAsync(user, roles);
            if(!result.Succeeded)
            {
                ModelState.AddModelError("", "Can not remove the user existing roles ");
                return View(model);
            }
            result = await userManager.AddToRolesAsync(user, model.Where(x => x.IsSelected).Select(x => x.RoleName));
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Can not add the user existing roles ");
                return View(model);
            }
            return RedirectToAction("EditUser", new { Id = userId });
        }

        [HttpGet]
        public IActionResult ListUsers()
        {
            var users = userManager.Users;
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string Id) 
        {
            var user = await userManager.FindByIdAsync(Id);
            if(user==null)
            {
                ViewBag.ErrorMessage = $"User belong to User Id : {Id} can not be found";
                return View("NotFound");
            }

            var userClaims = await userManager.GetClaimsAsync(user);
            var userRoles = await userManager.GetRolesAsync(user);

            var model = new EditUserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                City = user.City,
                Claims = userClaims.Select(x=>x.Type +" : "+ x.Value).ToList(),
                Roles = userRoles
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            var user = await userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User belong to User Id : {model.Id} can not be found";
                return View("NotFound");
            }
            else
            {
                user.Email = model.Email;
                user.UserName = model.UserName;
                user.City = model.City;
            }

            var result = await userManager.UpdateAsync(user);
            if(result.Succeeded)
            {
                return RedirectToAction("ListUsers");
            }

            foreach(var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
           
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> DeleteUser (string Id)
        {
            var user = await userManager.FindByIdAsync(Id);
            if(user == null)
            {
                ViewBag.ErrorMessage = $"user of user Id : {Id} not found.";
                return View("NotFound");
            }
            var result = await userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction("ListUsers");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return RedirectToAction("ListUsers");
        }

        [HttpPost]
        [Authorize(Policy ="DeleteRolePolicy")]
        public async Task<IActionResult> DeleteRole (string Id)
        {
           
            var role = await roleManager.FindByIdAsync(Id);
            if(role==null)
            {
                ViewBag.ErrorMessage = $" role of role Id : {Id} not found";
                return View("NotFound");
            }
            try
            {
                var result = await roleManager.DeleteAsync(role);
                if (result.Succeeded)
                {
                    return RedirectToAction("ListRoles");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return RedirectToAction("ListRoles");
            }
            catch(Exception ex)
            {
                ViewBag.ErrorTitle = $"Role with Role Id : {role.Id} is assigned to users";
                ViewBag.ErrorMessage = "Please remove the role from the users and then perform delete operation.";
                return View("Exception");
            }
        }


        [HttpGet]
        public async Task<IActionResult> ManageUserClaim(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with User ID : {userId} not found";
                return View("NotFound");
            }
            var existingUserClaims = await userManager.GetClaimsAsync(user);

            UserClaimViewModel model = new UserClaimViewModel
            {
                UserId = userId
            };

            foreach(var claim in ClaimStore.AllClaims)
            {
                UserClaim userClaim = new UserClaim
                {
                    ClaimType = claim.Type
                };

                if(existingUserClaims.Any(c=>c.Type == claim.Type && c.Value =="true"))
                {
                    userClaim.IsSelected = true;
                }

                model.Claims.Add(userClaim);
            }

            return View(model);

        }


        [HttpPost]
        public async Task<IActionResult> ManageUserClaim(UserClaimViewModel model)
        {
            var user = await userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with User ID : {model.UserId} not found";
                return View("NotFound");
            }
            var claims = await userManager.GetClaimsAsync(user);
            var result = await userManager.RemoveClaimsAsync(user, claims);
            if(!result.Succeeded)
            {
                ModelState.AddModelError("", "cannot remove the claims from user");
                return View(model);
            }
            result = await userManager.AddClaimsAsync(user, 
                model.Claims.Select(c => new Claim(c.ClaimType, c.IsSelected ? "true":"false")));

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "cannot Adding the claims from user");
                return View(model);
            }

            return RedirectToAction("EditUser", new {Id = model.UserId});

        }


    }
}
