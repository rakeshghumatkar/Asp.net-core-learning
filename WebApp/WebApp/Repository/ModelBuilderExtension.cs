using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Models;

namespace WebApp.Repository
{
    public static class ModelBuilderExtension
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>().HasData(
                new Employee
                {
                    Id = 1,
                    Name = "Ram",
                    Department = Dept.HR,
                    Email = "ram@god.com"
                },
                new Employee
                {
                    Id = 2,
                    Name = "Siya",
                    Department = Dept.HR,
                    Email = "siya@god.com"
                });
        }
    }
}
