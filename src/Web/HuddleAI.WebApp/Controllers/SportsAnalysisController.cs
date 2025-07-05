using Microsoft.AspNetCore.Mvc;

namespace HuddleAI.WebApp.Controllers;

public class SportsAnalysisController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}