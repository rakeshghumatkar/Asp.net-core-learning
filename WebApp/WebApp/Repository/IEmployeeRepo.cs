using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Models;

namespace WebApp.Repository
{
    public interface IEmployeeRepo
    {
        public Employee GetEmployeeById(int id);
        public IEnumerable<Employee> GetEmployees();
        public Employee AddEmployee(Employee employee);

        public Employee Update(Employee employeeChanges);

        public Employee Delete(int id);

    }
}
