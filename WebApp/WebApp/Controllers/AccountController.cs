using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApp.Models;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly ILogger<AccountController> logger;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<AccountController> logger)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.logger = logger;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if(ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    Email = model.Email,
                    UserName = model.Email,
                    City = model.City
                };

                var result = await userManager.CreateAsync(user, model.Password);

                if(result.Succeeded)
                {
                    //ConfirmEmail password implementing
                    //Genrating token
                    var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    var confirmEmailLink = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token = token }, Request.Scheme);

                    logger.Log(LogLevel.Warning, confirmEmailLink);
                    if (signInManager.IsSignedIn(User) && User.IsInRole("Admin"))
                    {
                        return RedirectToAction("ListUsers", "Administration");
                    }
                    ViewBag.ErrorTitle = "Registration Successful";
                    ViewBag.ErrorMessage = "Before login, Please confirm your email by clicking link that we sent you in your email";
                    return View("Error");
                }
                foreach(var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View();
        }

        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if(userId == null || token == null)
            {
                return RedirectToAction("index", "home");
            }
            var user = await userManager.FindByIdAsync(userId);
            if(user==null)
            {
                ViewBag.ErrorTitle = $"User with user Id : {userId} not found";
                return View("NotFound");
            }

            var result = await userManager.ConfirmEmailAsync(user, token);
            if(result.Succeeded)
            {
                return View();
            }

            ViewBag.ErrorTitle = "Email cannot be confirmed";
            return View("Error");
        }


        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("index","home");
        }


        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl)
        {
            LoginViewModel model = new LoginViewModel
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList()

            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model,string returnUrl)
        {
            model.ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                //checking user has confirmed email or not, if not then showing error and redirect to login page
                if(user!=null && !user.EmailConfirmed && (await userManager.CheckPasswordAsync(user, model.Password)))
                {
                    ModelState.AddModelError(string.Empty, "Email is not confirm yet");
                    return View(model);
                }

                var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, true);

                if(result.Succeeded)
                {
                    //Redirecting to returnURl link when user login
                    if(!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        //return LocalRedirect(returnUrl);
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("index", "home");
                }

                if(result.IsLockedOut)
                {
                    return View("AccountLocked");
                }


                ModelState.AddModelError(string.Empty,"Invalid Username or Password");
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult ExternalLogin(string provider, string returnUrl)
        {
            var redirectUrl = Url.Action("ExternalUrlCallback", "Account", new { ReturnUrl = returnUrl });
            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        [HttpGet]
        public async Task<IActionResult> ExternalUrlCallback(string returnUrl =null, string remoteError=null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            LoginViewModel loginViewModel = new LoginViewModel
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };
            if(remoteError!=null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider : {remoteError}");
                return View("Login",loginViewModel);
            }

            // Get the login information about the user from the external login provider
            var info = await signInManager.GetExternalLoginInfoAsync();
            if(info==null)
            {
                ModelState.AddModelError(string.Empty, "Error while loading External login credential");
                return View("Login", loginViewModel);
            }

            //get email from provide
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            ApplicationUser user = null;

            //checking email has confirm or not, If not then redirect to login page
            if(email!=null)
            {
                user = await userManager.FindByEmailAsync(email);
                if(!user.EmailConfirmed)
                {
                    ModelState.AddModelError(string.Empty, "User Email has not confirm yet, Please confirm!");
                    return View("Login", loginViewModel);
                }
            }

            // If the user already has a login (i.e if there is a record in AspNetUserLogins
            // table) then sign-in the user with this external login provider
            var signInResult = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false,  bypassTwoFactor: true);
            if(signInResult.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }
            // If there is no record in AspNetUserLogins table, the user may not have
            // a local account
            else
            {
                //user is not create yet
                if(email!=null)
                {
                    user = await userManager.FindByEmailAsync(email);
                    if(user == null)
                    {
                        user = new ApplicationUser
                        {
                            Email = info.Principal.FindFirstValue(ClaimTypes.Email),
                            UserName = info.Principal.FindFirstValue(ClaimTypes.Email)
                        };

                        await userManager.CreateAsync(user);
                    }
                    // Add a login (i.e insert a row for the user in AspNetUserLogins table)
                    await userManager.AddLoginAsync(user, info);
                    await signInManager.SignInAsync(user,  isPersistent: false);
                    return LocalRedirect(returnUrl);
                }

                ViewBag.ErrorTitle = $"Email claim cannot recevied from {info.LoginProvider} ";
                ViewBag.ErrorMessage = "for further support contact support.rakesh@gmail.com";
                return View("Error");
            }

        }

        [HttpGet]
        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordViewModel model)
        {
            if(ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user != null && await userManager.IsEmailConfirmedAsync(user))
                {
                    var token = await userManager.GeneratePasswordResetTokenAsync(user);

                    var passwordResetLink = Url.Action("ResetPassword", "Account", new { email = model.Email, token = token }, Request.Scheme);
                    logger.Log(LogLevel.Warning, passwordResetLink);

                    return View("ForgetPasswordConformation");
                }
                return View("ForgetPasswordConformation");
            }


            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            if(email == null || token == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid Password reset token");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if(user!=null)
            {
                var result = await userManager.ResetPasswordAsync(user, model.Token, model.Password);
                if(result.Succeeded)
                {
                    if(await userManager.IsLockedOutAsync(user))
                    {
                        await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);
                    }
                    return View("ResetPasswordConfirmed");
                }

                foreach(var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }
            return View("ResetPasswordConfirmated");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await userManager.GetUserAsync(User);
            var hasPassword = await userManager.HasPasswordAsync(user);
            if(!hasPassword)
            {
                return RedirectToAction("AddPassword");
            }
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var user = await userManager.GetUserAsync(User);
            if(user==null)
            {
                return RedirectToAction("Login");
            }
            var result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if(!result.Succeeded)
            {
                foreach(var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }
            await signInManager.RefreshSignInAsync(user);
            return View("ChangePasswordConfirmation");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> AddPassword()
        {
            var user = await userManager.GetUserAsync(User);
            var userHasPassword = await userManager.HasPasswordAsync(user);
            if (userHasPassword)
            {
                return RedirectToAction("ChangePassword");
            }
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddPassword(AddPasswordViewModel model)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }
            var result = await userManager.AddPasswordAsync(user, model.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }
            await signInManager.RefreshSignInAsync(user);
            return View("ChangePasswordConfirmation");
        }

    }
}
