using Gumani_Moila_ST10229429_CLDV7111w.Data;
using Gumani_Moila_ST10229429_CLDV7111w.Helpers;
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
    public class VenuesController : Controller
    {
        private readonly EventEaseContext _context;

        public VenuesController(EventEaseContext context)
        {
            _context = context;
        }

        // GET: Venues
            public async Task<IActionResult> Index(int? pageNumber)
            {
                const int pageSize = 9; // adjust as needed

                var query = _context.Venue
                    // Use the actual navigation property name on your Venue model:
                    // if the property is 'User' use Include(v => v.User)
                    // if it really is 'user' use Include(v => v.user)
                    .Include(v => v.user)
                    .AsNoTracking()
                    .OrderBy(v => v.VenueName)
                    .AsQueryable();

                var model = await PaginatedList<Gumani_Moila_ST10229429_CLDV7111w.Models.Venue>
                    .CreateAsync(query, pageNumber ?? 1, pageSize);

                return View(model);
            }

        // GET: Venues/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venue
                .Include(v => v.user)
                .FirstOrDefaultAsync(m => m.VenueId == id);
            if (venue == null)
            {
                return NotFound();
            }

            return View(venue);
        }

        // GET: Venues/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.User, "UserId", "UserEmail");
            return View();
        }

        // POST: Venues/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VenueId,VenueName,VenueLocation,VenueCapacity,VenueImageUrl,UserId")] Venue venue)
        {
            // ✅ Middleware ensures only authenticated users reach here
            // Grab the logged-in user's ID from session
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            venue.UserId = userId;


            if (ModelState.IsValid)
            {
                _context.Add(venue);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData ["UserId"] = new SelectList(_context.User, "UserId", "UserEmail", venue.UserId);
            return View(venue);
        }


        // GET: Venues/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venue.FindAsync(id);
            if (venue == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.User, "UserId", "UserEmail", venue.UserId);
            return View(venue);
        }

        // POST: Venues/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("VenueId,VenueName,VenueLocation,VenueCapacity,VenueImageUrl")] Venue posted)
        {
            if (id != posted.VenueId)
            {
                return NotFound();
            }

            // load tracked entity so we don't have to worry about updating the UserId
            var venue = await _context.Venue.FindAsync(id);
            if (venue == null) {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                // update only allowed properties
                venue.VenueName = posted.VenueName;
                venue.VenueLocation = posted.VenueLocation;
                venue.VenueCapacity = posted.VenueCapacity;
                venue.VenueImageUrl = posted.VenueImageUrl;
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VenueExists(venue.VenueId))
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
            ViewData["UserId"] = new SelectList(_context.User, "UserId", "UserEmail", venue.UserId);
            return View(venue);
        }

        // GET: Venues/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venue
                .Include(v => v.user)
                .FirstOrDefaultAsync(m => m.VenueId == id);
            if (venue == null)
            {
                return NotFound();
            }

            return View(venue);
        }

        // POST: Venues/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venue = await _context.Venue.FindAsync(id);
           bool  hasExististingBookings = await _context.Booking.AnyAsync(b => b.VenueId == id);

            if (hasExististingBookings)
            {
                // Prevent deletion and show an error message
                ModelState.AddModelError(string.Empty, "Cannot delete venue that has existing bookings.");

             
                var venueWithUser = await _context.Venue
                    .Include(v => v.user)   
                    .FirstOrDefaultAsync(v => v.VenueId == id);

                return View(venueWithUser ?? venue); 
            }
            if (venue != null)
            {
                _context.Venue.Remove(venue);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VenueExists(int id)
        {
            return _context.Venue.Any(e => e.VenueId == id);
        }
    }
}
