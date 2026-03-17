using Gumani_Moila_ST10229429_CLDV7111w.Data;
using Gumani_Moila_ST10229429_CLDV7111w.Models;
using Gumani_Moila_ST10229429_CLDV7111w.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gumani_Moila_ST10229429_CLDV7111w.Controllers
{
    [Authorize]
    public class CustomerDetailsController : Controller
    {
        private readonly EventEaseContext _context;

        public CustomerDetailsController(EventEaseContext context)
        {
            _context = context;
        }

        // GET: CustomerDetails
        // Supports pagination via ?pageNumber=1
        public async Task<IActionResult> Index(int? pageNumber)
        {
            const int pageSize = 6; // change page size as required
            var query = _context.CustomerDetail
                                .AsNoTracking()
                                .OrderBy(c => c.CustomerId)
                                .AsQueryable();

            var model = await PaginatedList<CustomerDetail>.CreateAsync(query, pageNumber ?? 1, pageSize);
            return View(model);
        }

        // GET: CustomerDetails/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customerDetail = await _context.CustomerDetail
                .FirstOrDefaultAsync(m => m.CustomerId == id);
            if (customerDetail == null)
            {
                return NotFound();
            }

            return View(customerDetail);
        }

        // GET: CustomerDetails/Create
        public IActionResult Create()
        {
            // Capture previous page
            var referer = Request.Headers["Referer"].ToString();
            ViewBag.ReturnUrl = referer;

            return View();
        }

        // POST: CustomerDetails/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
           [Bind("CustomerId,CustomerName,CustomerLastName,CustomerPhone,CustomerEmail,CreatedAt")] CustomerDetail customerDetail,
           string ReturnUrl)
        {
            if (ModelState.IsValid)
            {
                // Only validate uniqueness when a value was provided
                bool emailExists = false;
                bool phoneExists = false;

                if (!string.IsNullOrWhiteSpace(customerDetail.CustomerEmail))
                {
                    var emailToCheck = customerDetail.CustomerEmail.Trim();
                    emailExists = await _context.CustomerDetail
                        .AnyAsync(c => c.CustomerEmail == emailToCheck);
                    if (emailExists)
                    {
                        ModelState.AddModelError("CustomerEmail", "Email already exists.");
                    }
                    else
                    {
                        customerDetail.CustomerEmail = emailToCheck;
                    }
                }

                if (!string.IsNullOrWhiteSpace(customerDetail.CustomerPhone))
                {
                    var phoneToCheck = customerDetail.CustomerPhone.Trim();
                    phoneExists = await _context.CustomerDetail
                        .AnyAsync(c => c.CustomerPhone == phoneToCheck);
                    if (phoneExists)
                    {
                        ModelState.AddModelError("CustomerPhone", "Phone number already exists.");
                    }
                    else
                    {
                        customerDetail.CustomerPhone = phoneToCheck;
                    }
                }

                if (!emailExists && !phoneExists)
                {
                    _context.Add(customerDetail);
                    await _context.SaveChangesAsync();

                    if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                    {
                        return Redirect(ReturnUrl);
                    }

                    return RedirectToAction(nameof(Index));
                }
            }

            // preserve return URL so the view can re-populate the hidden field
            ViewBag.ReturnUrl = ReturnUrl;
            return View(customerDetail);
        }



        // GET: CustomerDetails/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customerDetail = await _context.CustomerDetail.FindAsync(id);
            if (customerDetail == null)
            {
                return NotFound();
            }
            return View(customerDetail);
        }

        // POST: CustomerDetails/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CustomerId,CustomerName,CustomerLastName,CustomerPhone,CustomerEmail")] CustomerDetail posted)
        {
            if (id != posted.CustomerId)
            {
                return NotFound();
            }

            var customerDetail = await _context.CustomerDetail.FindAsync(id);
            if (customerDetail == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Trim incoming values
                posted.CustomerName = posted.CustomerName?.Trim();
                posted.CustomerLastName = posted.CustomerLastName?.Trim();
                posted.CustomerEmail = string.IsNullOrWhiteSpace(posted.CustomerEmail) ? null : posted.CustomerEmail.Trim();
                posted.CustomerPhone = string.IsNullOrWhiteSpace(posted.CustomerPhone) ? null : posted.CustomerPhone.Trim();

                // Validate uniqueness only when a value is provided, excluding current record
                bool emailExists = false;
                bool phoneExists = false;

                if (!string.IsNullOrWhiteSpace(posted.CustomerEmail))
                {
                    emailExists = await _context.CustomerDetail
                        .AnyAsync(c => c.CustomerEmail == posted.CustomerEmail && c.CustomerId != posted.CustomerId);

                    if (emailExists)
                    {
                        ModelState.AddModelError(nameof(posted.CustomerEmail), "Email already exists.");
                    }
                }

                if (!string.IsNullOrWhiteSpace(posted.CustomerPhone))
                {
                    phoneExists = await _context.CustomerDetail
                        .AnyAsync(c => c.CustomerPhone == posted.CustomerPhone && c.CustomerId != posted.CustomerId);

                    if (phoneExists)
                    {
                        ModelState.AddModelError(nameof(posted.CustomerPhone), "Phone number already exists.");
                    }
                }

                if (!emailExists && !phoneExists)
                {
                    // Apply allowed updates
                    customerDetail.CustomerName = posted.CustomerName;
                    customerDetail.CustomerLastName = posted.CustomerLastName;
                    customerDetail.CustomerPhone = posted.CustomerPhone;
                    customerDetail.CustomerEmail = posted.CustomerEmail;

                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!CustomerDetailExists(customerDetail.CustomerId))
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
            }

            // Ensure view shows the attempted changes so user doesn't lose input
            customerDetail.CustomerName = posted.CustomerName;
            customerDetail.CustomerLastName = posted.CustomerLastName;
            customerDetail.CustomerPhone = posted.CustomerPhone;
            customerDetail.CustomerEmail = posted.CustomerEmail;

            return View(customerDetail);
        }

        // GET: CustomerDetails/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customerDetail = await _context.CustomerDetail
                .FirstOrDefaultAsync(m => m.CustomerId == id);
            if (customerDetail == null)
            {
                return NotFound();
            }

            return View(customerDetail);
        }

        // POST: CustomerDetails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customerDetail = await _context.CustomerDetail.FindAsync(id);
            if (customerDetail != null)
            {
                _context.CustomerDetail.Remove(customerDetail);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CustomerDetailExists(int id)
        {
            return _context.CustomerDetail.Any(e => e.CustomerId == id);
        }
    }
}
