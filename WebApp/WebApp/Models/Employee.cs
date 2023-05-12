using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Department { get; set; }
        public string Email { get; set; }

        public Employee() { }
        public Employee(int id, string name, string department, string email)
        {
            Id = id;
            Name = name;
            Department = department;
            Email = email;
        }
    }
}
