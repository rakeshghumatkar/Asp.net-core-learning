using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Models;

namespace WebApp.Repository
{
    public interface IEmployeeRepo
    {
        Employee GetEmployeeById(int id);
    }
}
