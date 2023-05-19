using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Models
{
    public class Employee
    {
        public int Id { get; set; }
        [MaxLength(50, ErrorMessage ="Name can't be more than 50 characters")]
        [Required]
        public string Name { get; set; }
        [Required]
        [RegularExpression("^[a-z0-9][-a-z0-9._]+@([-a-z0-9]+.)+[a-z]{2,5}$", ErrorMessage ="Invalid Email")]
        [Display(Name ="Office Email")]
        public string Email { get; set; }
        [Required]
        public Dept? Department { get; set; }
        public string? PhotoPath { get; set; }


        public Employee() { }
        public Employee(int id, string name, Dept department, string email)
        {
            Id = id;
            Name = name;
            Department = department;
            Email = email;
        }
    }
}
