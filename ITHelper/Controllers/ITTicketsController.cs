﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ITHelper.Data;
using ITHelper.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace ITHelper.Controllers
{
    public class ITTicketsController : MyController
    {
        /// <summary>
        /// Constructor with dependency injection for Db context
        /// </summary>
        public ITTicketsController(ITHelperContext context) : base(context) { }

        // GET: Tickets
        [AllowAnonymous]
        [Route("~/ITTickets/Index/{ticketStatus?}/{userName?}/{pageNo?}")]
        public async Task<IActionResult> Index(int ticketStatus = 1, string userName = "-- All Users --", int pageNo = 0)
        {
            var ticketQuery = GetTicketsByType(ticketStatus);
            var ticketList = new List<ITTicket>();
            if (User.IsInRole("Domain Admins"))
            {
                ticketList = await ticketQuery.ToListAsync();
            }
            else
            {
                ticketList = await ticketQuery
                    .Where(x => x.Username == User.Identity.Name)
                    .ToListAsync();
            }

            if (userName != "-- All Users --")
            {
                ticketList = ticketList.Where(x => x.Username.EndsWith(userName)).ToList();
            }

            var itemsPerPage = 15;
            var totalItems = ticketList.Count();
            pageNo = SetPageInformation(pageNo, totalItems, itemsPerPage);
            ViewBag.baseURL = "/Tickets/Index";
            ViewBag.DetailsMethod = "Details";
            ViewBag.TicketStatusList = GetTicketStatusSelectList(ticketStatus);
            ViewBag.UserName = userName;

            return View(ticketList);
        }

        // GET: Tickets/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.ITTickets
                .Where(m => m.Id == id)
                .Include(a => a.Updates)
                .FirstOrDefaultAsync();
            if (ticket == null)
            {
                return NotFound();
            }

            if ((ticket.Username != User.Identity.Name)
                && (!User.IsInRole("Domain Admins")))
            {
                throw (new ArgumentException());
            }

            ViewBag.Id = ticket.Id;

            return View(ticket);
        }

        // GET: Tickets/Create
        public IActionResult Create()
        {
            return View(new ITTicket() { Type = Ticket.TicketType.ITSupport });
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Username,FName,LName,EMail,Phone,Category,Type,Description,Status,Severity,AssignedTo,Notes,Resolution,DateSubmitted,LastUpdated")] ITTicket ticket)
        {
            if (ModelState.IsValid)
            {
                ticket.Id = Guid.NewGuid();
                ticket.Type = Ticket.TicketType.ITSupport;
                ticket.DateSubmitted = DateTimeOffset.Now;
                ticket.LastUpdated = DateTimeOffset.Now;
                _context.Add(ticket);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(ticket);
        }

        /// <summary>
        /// Provides the user with a form that they can add an update to the ticket status
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> AddUpdate(Guid id)
        {
            var u = new Update()
            {
                Id = new Guid(),
                Ticket = await _context.ITTickets.FindAsync(id),
                Username = User.Identity.Name,
                DateCreated = DateTimeOffset.Now
            };
            return View(u);
        }

        /// <summary>
        /// Post method for storing the update information to the Db
        /// </summary>
        /// <param name="id"></param>
        /// <param name="update"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> AddUpdate(Guid id, [Bind("Id,Username,Notes,IsResolved")] Update update, IFormCollection collection)
        {
            update.Ticket = await _context.ITTickets.FindAsync(Guid.Parse(collection["ticketId"]));
            ModelState.Remove("Ticket");

            update.Username = User.Identity.Name;
            ModelState.Remove("Username");

            if (ModelState.IsValid)
            {
                update.DateCreated = DateTimeOffset.Now;
                await _context.AddAsync(update);

                update.Ticket.LastUpdated = DateTimeOffset.Now;
                update.Ticket.Status = update.IsResolved ? Ticket.TicketStatus.Closed : update.Ticket.Status;
                _context.Update(update.Ticket);

                await _context.SaveChangesAsync();
                await _context.SaveChangesAsync();
            }
            else
            {
                return View(update);
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.ITTickets
               .Where(m => m.Id == id)
               .Include(a => a.Updates)
               .FirstOrDefaultAsync();
            if (ticket == null)
            {
                return NotFound();
            }

            ViewBag.Id = ticket.Id;
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Username,FName,LName,EMail,Phone,Category,Type,Description,Status,Severity,AssignedTo,Notes,Resolution,DateSubmitted,LastUpdated")] ITTicket ticket)
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

            var ticket = await _context.ITTickets
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            ticket.Updates = await _context.Updates
                .Where(x => x.Ticket.Id == ticket.Id)
                .ToListAsync();

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var ticket = await _context.ITTickets.FindAsync(id);
            _context.ITTickets.Remove(ticket);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        #region Internal Methods

        /// <summary>
        /// Indicates if the specified ticket exists in the system or not
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private bool TicketExists(Guid id)
        {
            return _context.ITTickets.Any(e => e.Id == id);
        }

        /// <summary>
        /// Retrieves the query for the associated ticket types based on the selected request
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        private IQueryable<ITTicket> GetTicketsByType(int status)
        {
            IOrderedQueryable<ITTicket> ticketQuery = null;
            switch (status)
            {
                case 1:
                    ticketQuery = _context.ITTickets          // Open Tickets
                        .Where(x => (x.Status >= Ticket.TicketStatus.Submitted) && (x.Status < Ticket.TicketStatus.Closed))
                        .OrderByDescending(y => y.LastUpdated);
                    break;

                case 2:
                    ticketQuery = _context.ITTickets          // Open & Closed Tickets
                        .Where(x => x.Status >= Ticket.TicketStatus.Submitted)
                        .OrderByDescending(y => y.LastUpdated);
                    break;

                case 3:
                    ticketQuery = _context.ITTickets          // Tickets which have been assigned to someone
                        .Where(x => (x.Status >= Ticket.TicketStatus.Reviewed) && (x.Status < Ticket.TicketStatus.Closed))
                        .OrderByDescending(y => y.LastUpdated);
                    break;

                case 4:
                    ticketQuery = _context.ITTickets          // Closed tickets
                        .Where(x => x.Status >= Ticket.TicketStatus.Closed)
                        .OrderByDescending(y => y.LastUpdated);
                    break;

                default:
                    ticketQuery = _context.ITTickets          // All tickets
                        .OrderByDescending(y => y.LastUpdated);
                    break;
            }

            return ticketQuery;
        }

        #endregion
    }
}