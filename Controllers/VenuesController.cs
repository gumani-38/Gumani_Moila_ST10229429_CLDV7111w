using Azure.Storage.Blobs;
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
        private readonly BlobServiceClient _blobServiceClient;

        public VenuesController(EventEaseContext context, BlobServiceClient blobServiceClient)
        {
            _context = context;
            _blobServiceClient = blobServiceClient;
        }

        // GET: Venues
            public async Task<IActionResult> Index(string searchInput,string sortOrder,int? pageNumber)
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
            // handle saerch input
            if(!string.IsNullOrEmpty(searchInput))
            {
                if (DateTime.TryParse(searchInput, out DateTime parsedDate))
                {
                    query = query.Where(v => v.CreatedAt.Date == parsedDate.Date);
                }
                else
                {
                    query = query.Where(v =>  v.VenueId.ToString().Contains(searchInput) 
                    || v.VenueName.ToLower().Contains(searchInput.ToLower())
                    || v.VenueLocation.ToLower().Contains(searchInput.ToLower()));
                }
            }
             // ✅ Handle sort options
             query = sortOrder switch
             {
              
                 "Oldest" => query.OrderBy(v => v.CreatedAt),
                 "Most Recent" => query.OrderByDescending(v => v.CreatedAt),
                 _ => query.OrderBy(v => v.VenueName)
             };

            var model = await PaginatedList<Gumani_Moila_ST10229429_CLDV7111w.Models.Venue>
                    .CreateAsync(query, pageNumber ?? 1, pageSize);
            ViewData["CurrentSearch"] = searchInput;
            ViewData["CurrentSort"] = sortOrder;

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
        public async Task<IActionResult> Create([Bind("VenueId,VenueName,VenueLocation,VenueCapacity")] Venue venue, IFormFile venueImage)
        {
            // ✅ Ensure only authenticated users reach here
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            venue.UserId = userId;

            if (venueImage == null || venueImage.Length == 0)
            {
                ModelState.AddModelError("VenueImageUrl", "Venue image is required.");
            }

            if (ModelState.IsValid)
            {
                if (venueImage != null && venueImage.Length > 0)
                {
                    // Get container reference
                    var containerClient = _blobServiceClient.GetBlobContainerClient("venueimages");
                    await containerClient.CreateIfNotExistsAsync();

                    // Generate unique blob name
                    string blobName = $"{Guid.NewGuid()}-{venueImage.FileName}";
                    var blobClient = containerClient.GetBlobClient(blobName);

                    // Upload file stream
                    using var stream = venueImage.OpenReadStream();
                    await blobClient.UploadAsync(stream, overwrite: true);

                    // Save blob URL in VenueImageUrl
                    venue.VenueImageUrl = blobClient.Uri.ToString();
                }

                _context.Add(venue);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["UserId"] = new SelectList(_context.User, "UserId", "UserEmail", venue.UserId);
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
        public async Task<IActionResult> Edit(int id, [Bind("VenueId,VenueName,VenueLocation,VenueCapacity")] Venue posted,IFormFile venueImage)
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
                if (venueImage != null && venueImage.Length > 0)
                {
                    // Get container reference
                    var containerClient = _blobServiceClient.GetBlobContainerClient("venueimages");
                    await containerClient.CreateIfNotExistsAsync();
                    // if the venue already has an image, delete the old one
                    if(!string.IsNullOrEmpty(venue.VenueImageUrl))
                    {
                        var oldBlobName = new Uri(venue.VenueImageUrl).Segments.Last();
                        var oldBlobClient = containerClient.GetBlobClient(oldBlobName);
                        await oldBlobClient.DeleteIfExistsAsync();
                    }

                    // Generate unique blob name
                    string blobName = $"{Guid.NewGuid()}-{venueImage.FileName}";
                    var blobClient = containerClient.GetBlobClient(blobName);

                    // Upload file stream
                    using var stream = venueImage.OpenReadStream();
                    await blobClient.UploadAsync(stream, overwrite: true);

                    // Save blob URL in VenueImageUrl
                    venue.VenueImageUrl = blobClient.Uri.ToString();
                }

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
