using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Repository;

namespace WebApp.Controllers
{
    public class HomeController : Controller
    {
        private IEmployeeRepo _employeeRepo;
        public HomeController(IEmployeeRepo employeeRepo)
        {
            _employeeRepo = employeeRepo;
        }
        public string Index()
        {
            return _employeeRepo.GetEmployeeById(1).Name;
        }
    }
}
