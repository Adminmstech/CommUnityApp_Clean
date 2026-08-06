using Microsoft.AspNetCore.Mvc;

namespace CommUnityApp.Areas.TalentShow.Controllers
{
    [Area("TalentShow")]
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction(nameof(Categories));
        }

        public IActionResult Categories()
        {
            return View();
        }

        public IActionResult Campaigns()
        {
            return View();
        }

        public IActionResult CampaignForm(int? id)
        {
            ViewBag.TalentShowCampaignId = id ?? 0;
            return View();
        }

        public IActionResult CategoryForm(int? id)
        {
            ViewBag.TalentShowCategoryId = id ?? 0;
            return View();
        }

        public IActionResult AddVideo()
        {
            return View();
        }

        public IActionResult Videos()
        {
            return View();
        }

        public IActionResult Rankings()
        {
            return View();
        }
    }
}
