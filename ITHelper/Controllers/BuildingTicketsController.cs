using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ITHelper.Data;
using ITHelper.Models;
using Microsoft.AspNetCore.Http;
using ITHelper.Helpers;

namespace ITHelper.Controllers
{
    public class BuildingTicketsController : MyController
    {
        /// <summary>
        /// Constructor with dependency injection for Db context
        /// </summary>
        /// <param name="context"></param>
        public BuildingTicketsController(ITHelperContext context) : base(context) { }

        // GET: BuildingTickets
        [Route("~/BuildingTickets/Index/{ticketStatus?}/{userName?}/{pageNo?}")]
        public async Task<IActionResult> Index(int ticketStatus = 1, string userName = "-- All Users --", int pageNo = 0)
        {
            var ticketQuery = GetTicketsByType(ticketStatus);
            var ticketList = new List<BuildingTicket>();
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

        // GET: BuildingTickets/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            { return NotFound(); }

            var buildingTicket = await _context.BuildingTickets
                .Where(m => m.Id == id)
                .Include(a => a.Updates)
                .FirstOrDefaultAsync();
            if (buildingTicket == null)
            { return NotFound(); }

            ViewBag.Id = buildingTicket.Id;

            return View(buildingTicket);
        }

        // GET: BuildingTickets/Create
        public IActionResult Create()
        {
            return View(new BuildingTicket() { Type = Ticket.TicketType.BuildingsAndGrounds });
        }

        // POST: BuildingTickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Location,Id,Username,FName,LName,EMail,Phone,Category,Type,Description,Severity,Status,AssignedTo,Notes,Resolution,DateSubmitted,LastUpdated")] BuildingTicket ticket)
        {
            if (ModelState.IsValid)
            {
                ticket.Id = Guid.NewGuid();
                ticket.Type = Ticket.TicketType.BuildingsAndGrounds;
                _context.Add(ticket);
                await _context.SaveChangesAsync();

                var newTicket = await GetTicketAsync(ticket.Id);
                var content = await this.RenderViewAsync("~/Views/EMail/BuildingTicketCreated.cshtml", newTicket, false);
                SendNotification("New Buildings & Grounds Ticket Created", content);

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
                Ticket = await _context.BuildingTickets.FindAsync(id),
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
            update.Ticket = await _context.BuildingTickets.FindAsync(Guid.Parse(collection["ticketId"]));
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

                var ticket = await GetTicketAsync(update.Ticket.Id);
                var content = await this.RenderViewAsync("~/Views/EMail/BuildingTicketUpdated.cshtml", ticket, false);
                var subject = update.IsResolved ? "Buildings & Grounds Ticket Resolved" : "Buildings & Grounds Ticket Updated";
                SendNotification(subject, content);
            }
            else
            {
                return View(update);
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: BuildingTickets/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            { return NotFound(); }

            var buildingTicket = await _context.BuildingTickets
               .Where(m => m.Id == id)
               .Include(a => a.Updates)
               .FirstOrDefaultAsync();
            if (buildingTicket == null)
            { return NotFound(); }

            ViewBag.Id = buildingTicket.Id;
            return View(buildingTicket);
        }

        // POST: BuildingTickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Location,Id,Username,FName,LName,EMail,Phone,Category,Type,Description,Severity,Status,AssignedTo,Notes,Resolution,DateSubmitted,LastUpdated")] BuildingTicket ticket)
        {
            if (id != ticket.Id)
            { return NotFound(); }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BuildingTicketExists(ticket.Id))
                    { return NotFound(); }
                    else
                    { throw; }
                }

                var content = await this.RenderViewAsync("~/Views/EMail/BuildingTicketEdited.cshtml", ticket, false);
                var subject = "Buildings & Grounds Ticket Edited";
                SendNotification(subject, content);

                return RedirectToAction(nameof(Index));
            }
            return View(ticket);
        }

        // GET: BuildingTickets/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var buildingTicket = await _context.BuildingTickets
                .FirstOrDefaultAsync(m => m.Id == id);
            if (buildingTicket == null)
            {
                return NotFound();
            }

            return View(buildingTicket);
        }

        // POST: BuildingTickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var ticket = await _context.BuildingTickets.FindAsync(id);

            var content = await this.RenderViewAsync("/Views/EMail/BuildingTicketDeleted.cshtml", ticket, false);
            var subject = "IT Ticket Deleted";
            if (ticket.Status != Ticket.TicketStatus.Closed)     // Don't send delete notices for resolved items
                SendNotification(subject, content);

            _context.BuildingTickets.Remove(ticket);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        #region Private Methods

        /// <summary>
        /// Indicates if the specified ticket exists in the system or not
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private bool BuildingTicketExists(Guid id)
        {
            return _context.BuildingTickets.Any(e => e.Id == id);
        }

        /// <summary>
        /// Returns the ticket specified by the Guid provided
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected async Task<BuildingTicket> GetTicketAsync(Guid id)
        {
            var ticket = await _context.BuildingTickets
                .Where(m => m.Id == id)
                .Include(a => a.Updates)
                .FirstOrDefaultAsync();

            if (ticket == null)
                throw new ArgumentException("Invalid Ticket Id", "Id");

            return ticket;
        }

        /// <summary>
        /// Retrieves the query for the associated ticket types based on the selected request
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        private IQueryable<BuildingTicket> GetTicketsByType(int status)
        {
            IOrderedQueryable<BuildingTicket> ticketQuery = null;
            switch (status)
            {
                case 1:
                    ticketQuery = _context.BuildingTickets          // Open Tickets
                        .Where(x => (x.Status >= Ticket.TicketStatus.Submitted) && (x.Status < Ticket.TicketStatus.Closed))
                        .OrderByDescending(y => y.LastUpdated);
                    break;

                case 2:
                    ticketQuery = _context.BuildingTickets          // Open & Closed Tickets
                        .Where(x => x.Status >= Ticket.TicketStatus.Submitted)
                        .OrderByDescending(y => y.LastUpdated);
                    break;

                case 3:
                    ticketQuery = _context.BuildingTickets          // Tickets which have been assigned to someone
                        .Where(x => (x.AssignedTo != string.Empty) && (x.Status < Ticket.TicketStatus.Closed))
                        .OrderByDescending(y => y.LastUpdated);
                    break;

                case 4:
                    ticketQuery = _context.BuildingTickets          // Closed tickets
                        .Where(x => x.Status >= Ticket.TicketStatus.Closed)
                        .OrderByDescending(y => y.LastUpdated);
                    break;

                default:
                    ticketQuery = _context.BuildingTickets          // All tickets
                        .OrderByDescending(y => y.LastUpdated);
                    break;
            }

            return ticketQuery;
        }

        #endregion
    }
}
