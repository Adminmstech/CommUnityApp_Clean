using Microsoft.AspNetCore.Mvc;

namespace CommUnityApp.Areas.TalentShow.Controllers
{
    [Area("TalentShow")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction(nameof(Gallery));
        }

        public IActionResult Gallery()
        {
            return View();
        }

        public IActionResult Campaign(int? id)
        {
            ViewBag.TalentShowCampaignId = id ?? 0;
            return View();
        }

        public IActionResult Upload()
        {
            return View();
        }

        public IActionResult Rankings()
        {
            return View();
        }
    }
}
