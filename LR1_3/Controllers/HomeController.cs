using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using LR1_3.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LR1_3.Controllers;

public class HomeController : Controller
{
    public class ListDemo
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    public IActionResult Index()
    {
        ViewData["Text"] = "Laboratory work 2";

        var model = new HomeViewModel
        {
            List = new SelectList(new List<ListDemo>{
                new() {Id = 1, Name = "First"},
                new() {Id = 2, Name = "Second"},
                new() {Id = 3, Name = "Third"},
            },
            "Id",
            "Name")
        };

        return View(model);
    }
}
