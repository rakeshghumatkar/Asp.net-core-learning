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
                new Employee() { Id=1, Name="Rakesh", Department=Dept.Engineer, Email="rakesh@"},
                new Employee() { Id=2, Name="Sneha", Department=Dept.HR, Email="Sneha@"},
                new Employee() { Id=3, Name="Ram", Department=Dept.Payroll, Email="ram@"},
                new Employee() { Id=4, Name="Vishal", Department=Dept.Sales, Email="vishal@"}
            };
        }
        public Employee GetEmployeeById(int id)
        {
            return _employeeList.FirstOrDefault(x => x.Id == id);
        }

        public IEnumerable<Employee> GetEmployees()
        {
            return _employeeList;
        }

        public Employee AddEmployee(Employee employee)
        {
            employee.Id = _employeeList.Max(e=>e.Id) + 1;
             _employeeList.Add(employee);
            return employee;
        }

        public Employee Update(Employee employeeChanges)
        {
            Employee employee = _employeeList.FirstOrDefault(x=> x.Id== employeeChanges.Id);
            if(employee!=null)
            {
                employee.Name = employeeChanges.Name;
                employee.Email = employeeChanges.Email;
                employee.Department = employeeChanges.Department;
                
            }
            return employee;

        }

        public Employee Delete(int id)
        {
            Employee employee = _employeeList.FirstOrDefault(x => x.Id == id);
            if(employee!=null)
            {
                _employeeList.Remove(employee);
            }
            return employee;
        }
    }
}
