using Gumani_Moila_ST10229429_CLDV7111w.Data;
using Gumani_Moila_ST10229429_CLDV7111w.Helpers;
using Gumani_Moila_ST10229429_CLDV7111w.Models;
using Humanizer.Localisation;
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
    public class EventsController : Controller
    {
        private readonly EventEaseContext _context;

        public EventsController(EventEaseContext context)
        {
            _context = context;
        }

        // GET: Events
        public async Task<IActionResult> Index(string searchInput,string sortOrder,int? pageNumber)
        {
            const int pageSize = 9; // adjust page size as needed

            var query = _context.Event
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
                .AsNoTracking()
                .OrderByDescending(e => e.EventDate)
                .AsQueryable();

            // handle saerch input 
            if(!string.IsNullOrEmpty(searchInput))
            {
                if (DateTime.TryParse(searchInput, out DateTime parsedDate))
                {
                    query = query.Where(e => e.EventDate.Date == parsedDate.Date);
                }
                else
                {
                    query = query.Where(e =>  e.EventId.ToString().Contains(searchInput) 
                    || e.EventName.ToLower().Contains(searchInput.ToLower()));
                }
            }
            // ✅ Handle sort options
            query = sortOrder switch
            {
                "Oldest" => query.OrderBy(e => e.EventDate),
                "Most Recent" => query.OrderByDescending(e => e.EventDate),
                _ => query.OrderByDescending(e => e.EventDate)
            };


            var model = await PaginatedList<Event>.CreateAsync(query, pageNumber ?? 1, pageSize);
            ViewData["CurrentSearch"] = searchInput;
            ViewData["CurrentSort"] = sortOrder;
            return View(model);
        }

        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Event
                .Include(v => v.Venue)
                .FirstOrDefaultAsync(m => m.EventId == id);
            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

        // GET: Events/Create
        public IActionResult Create()
        {
  
            ViewData["VenueId"] = new SelectList(
                _context.Venue.Select(v => new {
                    v.VenueId,
                    DisplayText = v.VenueName + " (" + v.VenueLocation + ") - Capacity: " + v.VenueCapacity
                }),
                "VenueId",
                "DisplayText"
            );
   
            return View();
        }

        // POST: Events/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventId,EventName,EventDescription,EventDate,VenueId,CreatedAt,UserId")] Event @event)
        {
            @event.UserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            if (ModelState.IsValid)
            {
                _context.Add(@event);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["VenueId"] = new SelectList(_context.Venue, "VenueId", "VenueImageUrl", @event.VenueId);
            return View(@event);
        }

        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Event.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }
            ViewData["VenueId"] = new SelectList(_context.Venue.Select( v => new
                {
                v.VenueId,
                DisplayText = v.VenueName + " (" + v.VenueLocation + ") - Capacity: " + v.VenueCapacity


            }), "VenueId","DisplayText");
            return View(@event);
        }

        // POST: Events/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EventId,EventName,EventDescription,EventDate,VenueId,CreatedAt")] Event @event)
        {
            if (id != @event.EventId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(@event);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(@event.EventId))
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
            ViewData["VenueId"] = new SelectList(_context.Venue, "VenueId", "VenueImageUrl", @event.VenueId);
            return View(@event);
        }

        // GET: Events/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Event
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(m => m.EventId == id);
            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @event = await _context.Event.FindAsync(id);
            bool hasExististingBookings = await _context.Booking.AnyAsync(b => b.EventId == id);

            if (hasExististingBookings)
            {
                //  Prevent deletion and show an error message
                ModelState.AddModelError(string.Empty, "Cannot delete event that has existing bookings.");

                var eventWithVenue = await _context.Event
                    .Include(e => e.Venue)
                    .FirstOrDefaultAsync(m => m.EventId == id);

                return View(eventWithVenue ?? @event);
            }
            if (@event != null)
            {
                _context.Event.Remove(@event);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id)
        {
            return _context.Event.Any(e => e.EventId == id);
        }
    }
}
