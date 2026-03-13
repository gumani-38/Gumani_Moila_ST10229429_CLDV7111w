using Gumani_Moila_ST10229429_CLDV7111w.Data;
using Gumani_Moila_ST10229429_CLDV7111w.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace Gumani_Moila_ST10229429_CLDV7111w.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly EventEaseContext _context;
        public HomeController(ILogger<HomeController> logger,EventEaseContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var events = await _context.Event
      .Include(e => e.Venue)
      .Select(e => new Event
      {
          EventId = e.EventId,
          EventName = e.EventName,
          EventDescription = e.EventDescription,
          EventDate = e.EventDate,
          VenueId = e.VenueId,
          CreatedAt = e.CreatedAt,
          Venue = e.Venue,
          RemainingSeats = e.Venue.VenueCapacity - _context.Booking.Count(b => b.EventId == e.EventId)
      })
      .ToListAsync();


            ViewData["TotalEvents"] = await _context.Event.CountAsync();
            ViewData["TotalVenues"] = await _context.Venue.CountAsync();
            ViewData["TotalBookings"] = await _context.Booking.CountAsync();
            ViewData["TotalCustomers"] = await _context.CustomerDetail.CountAsync();
            return View(events);
        }
        [AllowAnonymous]
        public IActionResult Login()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim != null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
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
}
