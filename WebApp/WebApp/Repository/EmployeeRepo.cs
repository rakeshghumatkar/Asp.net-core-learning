using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Wordprocessing;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Models;

namespace WebApp.Repository
{
    public class EmployeeRepo : IEmployeeRepo
    {
        private List<Employee> _employeeList;
        public EmployeeRepo()
        {
            _employeeList = new List<Employee>()
            {
                new Employee() { Id=1, Name="Rakesh", Department="Developer", Email="rakesh@"},
                new Employee() { Id=2, Name="Ram", Department="Support", Email="ram@"},
                new Employee() { Id=3, Name="Raj", Department="Developer", Email="raj@"},
                new Employee() { Id=4, Name="Vishal", Department="Support", Email="vishal@"}
            };
        }
        public Employee GetEmployeeById(int id)
        {
            return _employeeList.FirstOrDefault(x => x.Id == id);
        }
    }
}
