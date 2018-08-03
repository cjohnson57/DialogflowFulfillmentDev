using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication3.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            Random rdm = new Random();
            bool twoyears = rdm.Next(0, 2) == 1;
            if(twoyears)
            {
                int year1 = rdm.Next(2005, 2018);
                int year2 = rdm.Next(year1 + 1, 2019);
                string[] firststrings = { "from ", "during ", "between ", " ",  };
                string[] secondstrings = { " and ", " to ", "-", " ", };
                string firststring = firststrings[rdm.Next(0, 4)];
                string secondstring = secondstrings[rdm.Next(0, 4)];
                ViewBag.title = firststring + year1.ToString() + secondstring + year2.ToString();
            }
            else
            {
                int year1 = rdm.Next(2005, 2019);
                string[] firststrings = { "from ", "during ", "in ", "", };
                string firststring = firststrings[rdm.Next(0, 4)];
                ViewBag.title = firststring + year1.ToString();
            }

            return View();
        }
    }
}
