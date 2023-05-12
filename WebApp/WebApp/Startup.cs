using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Repository;

namespace WebApp
{
    public class Startup
    {
        private IConfiguration _confi;
        public Startup(IConfiguration confi)
        {
            _confi = confi ;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(option => option.EnableEndpointRouting = false);
            services.AddSingleton<IEmployeeRepo, EmployeeRepo>();
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
            app.UseMvcWithDefaultRoute();

            app.Run( async context =>
            {
                //throw new Exception("Some error while processing the request");
               
                await context.Response.WriteAsync("Hello World!");
            });

        }
    }
}
