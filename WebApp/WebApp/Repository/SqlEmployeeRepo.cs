using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Models;

namespace WebApp.Repository
{
    public class SqlEmployeeRepo : IEmployeeRepo
    {
        private readonly ILogger logger;

        private AppDbContext _dbContext { get; set; }
        public SqlEmployeeRepo(AppDbContext dbContext, ILogger<SqlEmployeeRepo> logger)
        {
            _dbContext = dbContext;
            this.logger = logger;
        }
        public Employee AddEmployee(Employee employee)
        {
            _dbContext.Employees.Add(employee);
            _dbContext.SaveChanges();
            return employee;
        }

        public Employee Delete(int id)
        {
            Employee emp = _dbContext.Employees.Find(id);
            if (emp != null)
            {
                _dbContext.Employees.Remove(emp);
                _dbContext.SaveChanges();
            }
            return emp;
           
        }

        public Employee GetEmployeeById(int id)
        {
            logger.LogTrace("log Trace");
            logger.LogDebug("log debug");
            logger.LogInformation("log information");
            logger.LogWarning("log warning");
            logger.LogError("log error");
            logger.LogCritical("log Critical");

            return _dbContext.Employees.Find(id);
        }

        public IEnumerable<Employee> GetEmployees()
        {
            return _dbContext.Employees;
        }

        public Employee Update(Employee employeeChanges)
        {
            var emp = _dbContext.Employees.Attach(employeeChanges);
            emp.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            _dbContext.SaveChanges();
            return employeeChanges;
            
        }
    }
}
