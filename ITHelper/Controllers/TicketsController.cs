using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ITHelper.Data;
using ITHelper.Models;

namespace ITHelper.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ITHelperContext _context;

        public TicketsController(ITHelperContext context)
        {
            _context = context;
        }

        // GET: Tickets
        public async Task<IActionResult> Index(int ticketStatus = 0, int pageNo = 0)
        {
            var myStatus = (Ticket.TicketStatus)ticketStatus;
            var ticketList = new List<Ticket>();
            switch(ticketStatus)
            {
                case 0:
                    ticketList = await _context.Ticket
                        .Where(x => x.Status != Ticket.TicketStatus.Closed)
                        .OrderByDescending(y => y.LastUpdated)
                        .ToListAsync();
                    break;

                case 1 - 5:
                    ticketList = await _context.Ticket
                        .Where(x => (x.Status == myStatus) && (x.Status != Ticket.TicketStatus.Closed))
                        .OrderByDescending(y => y.LastUpdated)
                        .ToListAsync();
                    break;

                case 6:
                    ticketList = await _context.Ticket
                        .Where(x => x.Status == myStatus)
                        .OrderByDescending(y => y.LastUpdated)
                        .ToListAsync();
                    break;
            }
              
            return View(ticketList);
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Ticket
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // GET: Tickets/Create
        public IActionResult Create()
        {
            return View(new Ticket());
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Username,FName,LName,EMail,Phone,Category,Description,Status,Severity,AssignedTo,Notes,Resolution,DateSubmitted,LastUpdated")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                ticket.Id = Guid.NewGuid();
                ticket.DateSubmitted = DateTimeOffset.Now;
                ticket.LastUpdated = DateTimeOffset.Now;
                _context.Add(ticket);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Ticket.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Username,FName,LName,EMail,Phone,Category,Description,Status,Severity,AssignedTo,Notes,Resolution,DateSubmitted,LastUpdated")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    ticket.LastUpdated = DateTimeOffset.Now;
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.Id))
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
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Ticket
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var ticket = await _context.Ticket.FindAsync(id);
            _context.Ticket.Remove(ticket);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketExists(Guid id)
        {
            return _context.Ticket.Any(e => e.Id == id);
        }
    }
}
