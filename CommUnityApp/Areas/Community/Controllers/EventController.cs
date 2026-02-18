using Microsoft.AspNetCore.Mvc;

namespace CommUnityApp.Areas.Community.Controllers
{
    [Area("Community")]

    public class EventController : Controller
    {

        public IActionResult Register(long eventId)
        {
            if (eventId <= 0)
            {
                return BadRequest("Invalid event");
            }

            ViewBag.EventId = eventId;
            return View();
        }

        public IActionResult EventDetails(long eventId)
        {
            if (eventId <= 0)
            {
                return BadRequest("Invalid event");
            }

            ViewBag.EventId = eventId;

            return View();
        }
        public IActionResult RegistrationDetails(long id)
        {
            if (id <= 0)
                return BadRequest("Invalid registration");

            ViewBag.RegistrationId = id;
            return View();
        }



    }

    }

