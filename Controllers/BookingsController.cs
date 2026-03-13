using Gumani_Moila_ST10229429_CLDV7111w.Data;
using Gumani_Moila_ST10229429_CLDV7111w.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Gumani_Moila_ST10229429_CLDV7111w.Controllers
{
    [Authorize]
    public class BookingsController : Controller
    {
        private readonly EventEaseContext _context;

        public BookingsController(EventEaseContext context)
        {
            _context = context;
        }

        // GET: Bookings
        public async Task<IActionResult> Index()
        {
            var eventEaseContext = _context.Booking.Include(b => b.CustomerDetail).Include(b => b.Event).Include(b => b.User).Include(b => b.Venue);
            return View(await eventEaseContext.ToListAsync());
        }

        // GET: Bookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Booking
                .Include(b => b.CustomerDetail)
                .Include(b => b.Event)
                .Include(b => b.User)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // GET: Bookings/Create
        // GET: Bookings/Create
        public IActionResult Create(int? eventId, int? venueId)
        {
            ViewData["CustomerId"] = new SelectList(
                _context.CustomerDetail
                 .OrderByDescending(c => c.CreatedAt) // sort by most recent added 
.Select(v => new {
                    v.CustomerId,
                    DisplayName = v.CustomerName + " " + v.CustomerLastName + " - " + v.CustomerPhone
                }),
                "CustomerId",
                "DisplayName",
                null // as a default
            );

            ViewData["EventId"] = new SelectList(
                _context.Event,
                "EventId",
                "EventName",
                eventId // pre-select if provided
            );

            ViewData["UserId"] = new SelectList(_context.User, "UserId", "UserEmail");

            ViewData["VenueId"] = new SelectList(
                _context.Venue.Select(v => new {
                    v.VenueId,
                    DisplayName = v.VenueName + " - " + v.VenueLocation
                }),
                "VenueId",
                "DisplayName",
                venueId // pre-select if provided
            );

            return View();
        }
        // POST: Bookings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookingId,EventId,VenueId,CustomerId,BookingDate")] Booking booking)
        {
            // ✅ Middleware ensures only authenticated users reach here
            // Grab the logged-in user's ID from session
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            booking.UserId = userId; 
            if (ModelState.IsValid)
            {
               
                _context.Add(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomerId"] = new SelectList(_context.CustomerDetail.Select(v => new
            {
                v.CustomerId,
                DisplayName = v.CustomerName + " " + v.CustomerLastName + "- " + v.CustomerPhone

            }), "CustomerId", "DisplayName",booking.CustomerId);
            ViewData["EventId"] = new SelectList(_context.Event, "EventId", "EventName", booking.EventId);
            ViewData["UserId"] = new SelectList(_context.User, "UserId", "UserEmail", booking.UserId);
            ViewData["VenueId"] = new SelectList(
                _context.Venue
                    .Select(v => new {
                        v.VenueId,
                        DisplayName = v.VenueName + ", " + v.VenueLocation
                    }),
                "VenueId",
                "DisplayName",
                booking.VenueId
            );

            return View(booking);
        }

        // GET: Bookings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Booking.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }
            ViewData["CustomerId"] = new SelectList(_context.CustomerDetail.Select(v=> new
            {
                v.CustomerId,
                DisplayName= v.CustomerName + " " + v.CustomerLastName + "- " + v.CustomerPhone
            }),"CustomerId", "DisplayName", booking.CustomerId);
            ViewData["EventId"] = new SelectList(_context.Event, "EventId", "EventName", booking.EventId);
  
            ViewData["VenueId"] = new SelectList(_context.Venue.Select(v => new { 
             v.VenueId,
             DisplayName = v.VenueName + ", " + v.VenueLocation
            }), "VenueId", "DisplayName", booking.VenueId);
            return View(booking);
        }

        // POST: Bookings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingId,EventId,VenueId,CustomerId")] Booking posted)
        {
            if (id != posted.BookingId)
            {
                return NotFound();
            }

            var booking = await _context.Booking.FindAsync(id);
            if(booking == null)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    // only update the properties that are allowed to be edited
                    booking.EventId = posted.EventId;
                    booking.VenueId = posted.VenueId;
                    booking.CustomerId = posted.CustomerId;
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.BookingId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomerId"] = new SelectList(_context.CustomerDetail.Select(v => new
            {
                v.CustomerId,
                DisplayName = v.CustomerName + " " + v.CustomerLastName + "- " + v.CustomerPhone
            }),"CustomerId", "DisplayName", booking.CustomerId);
            ViewData["EventId"] = new SelectList(_context.Event, "EventId", "EventName", booking.EventId);

            ViewData["VenueId"] = new SelectList(_context.Venue.Select(v => new {
                v.VenueId,
                DisplayName = v.VenueName + ", " + v.VenueLocation
            }),"VenueId","DisplayName", booking.VenueId);
            return View(booking);
        }

        // GET: Bookings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Booking
                .Include(b => b.CustomerDetail)
                .Include(b => b.Event)
                .Include(b => b.User)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Booking.FindAsync(id);
            if (booking != null)
            {
                _context.Booking.Remove(booking);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
            return _context.Booking.Any(e => e.BookingId == id);
        }
    }
}
