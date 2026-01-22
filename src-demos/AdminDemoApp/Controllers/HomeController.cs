using AdminDemoApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AdminDemoApp.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return Content("index");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
