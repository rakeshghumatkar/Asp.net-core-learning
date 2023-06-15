using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Models;
using WebApp.Repository;
using WebApp.Security;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    //[Authorize]
    public class HomeController : Controller
    {
        private readonly IEmployeeRepo _employeeRepo;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ILogger<HomeController> logger;
        protected readonly IDataProtectionProvider protector;

        public HomeController(IEmployeeRepo employeeRepo, IHostingEnvironment hostingEnvironment, ILogger<HomeController> logger,
            IDataProtectionProvider dataProtectionProvider, DataProtectionPurposeString dataProtectionPurposeString)
        {
            _employeeRepo = employeeRepo;
            _hostingEnvironment = hostingEnvironment;
            this.logger = logger;
            //data encryption
            protector = dataProtectionProvider.CreateProtector(dataProtectionPurposeString.EmployeeIdRouteValue);
        }

        private string ProcessUploadFile(EmployeeCreateViewModel model)
        {
            string uniqueFileName = null;
            if (model.Photo != null)
            {
                string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "images");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Photo.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    model.Photo.CopyTo(fileStream);
                }
                   
            }

            return uniqueFileName;
        }

        [AllowAnonymous]
        public ViewResult Index()
        {
            // Cannot find the Protect method in I DataProtectionProvider
            /*var model = _employeeRepo.GetEmployees().Select(e=> 
            {
                //e.EncryptedId = protector.(e.Id.ToString());
                e.EncryptedId = protector.Protect(e.Id.ToString());
                return e;
                
            });*/
            var model = _employeeRepo.GetEmployees();
            return View(model);
        }
        
        
        public ViewResult Details(int? id)
        {
            logger.LogTrace("log Trace");
            logger.LogDebug("log debug");
            logger.LogInformation("log information");
            logger.LogWarning("log warning");
            logger.LogError("log error");
            logger.LogCritical("log Critical");
            
            Employee model = _employeeRepo.GetEmployeeById(id.Value);
            /*ViewData["Employee"] = model;
            ViewBag.PageTitle = "Set title using viewbag";*/
            if(model==null)
            {
                return View("EmployeeNotFound", id.Value);
            }

            DetailsHomeViewModel detailsHomeViewModel = new DetailsHomeViewModel()
            {
                Employee = model,
                PageTitle = "Home Details Page"
            };
            return View(detailsHomeViewModel);
        }
        
       
        [HttpGet]
        public ViewResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(EmployeeCreateViewModel model)
        {
           if(ModelState.IsValid)
            {
                string uniqueFileName = ProcessUploadFile(model);
                Employee newEmployee = new Employee
                {
                    Name = model.Name,
                    Email = model.Email,
                    Department = model.Department,
                    PhotoPath = uniqueFileName

                };
                _employeeRepo.AddEmployee(newEmployee);
                return RedirectToAction("details", new { id = newEmployee.Id });
            }
            return View();
        }

        [HttpGet]
        public ViewResult Edit(int id)
        {
            Employee employee = _employeeRepo.GetEmployeeById(id);
            EmoloyeeEditViewModel emoloyeeEditViewModel = new EmoloyeeEditViewModel
            {
                Id = employee.Id,
                Name = employee.Name,
                Email = employee.Email,
                Department = employee.Department,
                ExistingPhotoPath = employee.PhotoPath
            };

            return View(emoloyeeEditViewModel);
        }

        [HttpPost]
        public IActionResult Edit(EmoloyeeEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                Employee employee = _employeeRepo.GetEmployeeById(model.Id);
                employee.Name = model.Name;
                employee.Email = model.Email;
                employee.Department = model.Department;
                if(model.Photo!=null)
                {
                    if(model.ExistingPhotoPath!=null)
                    {
                        var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "images", model.ExistingPhotoPath);
                        System.IO.File.Delete(filePath);

                    }
                    employee.PhotoPath = ProcessUploadFile(model);
                }
              
                _employeeRepo.Update(employee);
                return RedirectToAction("index");
            }
            return View();
        }

    }
}
