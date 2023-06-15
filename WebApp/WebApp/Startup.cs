using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Models;
using WebApp.Repository;
using WebApp.Security;

namespace WebApp
{
    public class Startup
    {
        private IConfiguration _config;
        public Startup(IConfiguration confi)
        {
            _config = confi ;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(option => {
                option.EnableEndpointRouting = false;
                //Applying authoziation policy globally
                var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                option.Filters.Add(new AuthorizeFilter(policy));
                
                });

            services.AddScoped<IEmployeeRepo,SqlEmployeeRepo>();

            services.AddDbContextPool<AppDbContext>(
                options => options.UseSqlServer(_config.GetConnectionString("EmployeeDBConnection"))
                );

            services.AddIdentity<ApplicationUser, IdentityRole>(
                option =>
                {
                    //overriding built`in password validations
                    option.Password.RequiredLength = 3;
                    option.SignIn.RequireConfirmedEmail = true;
                    option.Tokens.EmailConfirmationTokenProvider = "CustomEmailConfirmation";
                    //Lockout properties
                    option.Lockout.MaxFailedAccessAttempts = 3;
                    option.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);

                }).AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders()
                .AddTokenProvider<CustomEmailConfirmationTokenProvider<ApplicationUser>>("CustomEmailConfirmation");

            //setting the token timespan for all generated tokens
            services.Configure<DataProtectionTokenProviderOptions>(o => o.TokenLifespan = TimeSpan.FromMinutes(2));
            //Setting the custom email token timespan
            services.Configure<CustomEmailConfirmationTokenProviderOptions>(o => o.TokenLifespan = TimeSpan.FromMinutes(1));

            //Registrating Claim handler
            services.AddSingleton<IAuthorizationHandler, CanEditOnlyOtherAdminRolesAndClaimsHandler>();
            services.AddSingleton<IAuthorizationHandler, SuperAdminHandler>();
            services.AddSingleton<DataProtectionPurposeString>();

            //Google and facebook Login Link configuration
            services.AddAuthentication().AddGoogle(option=>
            {
                option.ClientId = "523420889317-609ionqbb7p8rd0omun3r5bvgnga5gq1.apps.googleusercontent.com";
                option.ClientSecret = "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";
            })
            .AddFacebook(option =>
            {
                option.AppId = "791065936029134";
                option.AppSecret = "a6780410fa7dd3e13d81a76074ba7282";
            });

            //Claim Policies configuration
            services.AddAuthorization(option =>
            {
                option.AddPolicy("DeleteRolePolicy",
                    policy => policy.RequireClaim("Delete Role","true"));

                option.AddPolicy("EditRolePolicy", policy => policy.AddRequirements(new ManageAdminRoleAndClaimRequirement()));

                option.AddPolicy("CreateRolePolicy", policy => policy.RequireClaim("Create Role","true"));

                option.AddPolicy("AdminRolePolicy",
                    policy => policy.RequireAssertion(
                        context => context.User.IsInRole("Admin") &&
                        context.User.HasClaim(claim => claim.Type == "Edit Role" && claim.Value == "true") ||
                        context.User.IsInRole("SuperAdmin")
                        ));
            });

            services.ConfigureApplicationCookie(option =>
            {
                option.AccessDeniedPath = new PathString("/Administration/AccessDenied");
            });
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                // Adding extra functionality in developer exception
                /*DeveloperExceptionPageOptions developerExceptionPageOptions = new DeveloperExceptionPageOptions()
                {
                    //display code line before and after exception
                    SourceCodeLineCount = 10
                };
                app.UseDeveloperExceptionPage(developerExceptionPageOptions);*/

                
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseStatusCodePagesWithReExecute("/Error/{0}");
            }

            /* 
            Instead of two UseDefaultFiles and UseStaticFiles middleware you can use UseFileServer()
            Changing default file index to page1

            DefaultFilesOptions defaultFilesOptions = new DefaultFilesOptions();
            defaultFilesOptions.DefaultFileNames.Clear();
            defaultFilesOptions.DefaultFileNames.Add("page1.html");
            */
            //app.UseDefaultFiles();

            //Changing default index.html to page1.html
            /*FileServerOptions fileServerOptions = new FileServerOptions();
            fileServerOptions.DefaultFilesOptions.DefaultFileNames.Clear();
            fileServerOptions.DefaultFilesOptions.DefaultFileNames.Add("page1.html");
*/
            //app.UseFileServer(fileServerOptions);



            app.UseStaticFiles();
            app.UseAuthentication();
            //app.UseMvcWithDefaultRoute();  //Select default mvc setting 
            app.UseMvc(route =>
            {
                route.MapRoute(default, "{controller=home}/{action=index}/{id?}");
            });

            /*app.Run( async context =>
            {
                //throw new Exception("Some error while processing the request");
               
                await context.Response.WriteAsync("Hello World!");
            });*/

        }
    }
}
