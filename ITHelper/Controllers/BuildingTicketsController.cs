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
            {
                return NotFound();
            }

            var buildingTicket = await _context.BuildingTickets
                .Where(m => m.Id == id)
                .Include(a => a.Updates)
                .FirstOrDefaultAsync();
            if (buildingTicket == null)
            {
                return NotFound();
            }

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
        public async Task<IActionResult> Create([Bind("Location,Id,Username,FName,LName,EMail,Phone,Category,Type,Description,Severity,Status,AssignedTo,Notes,Resolution,DateSubmitted,LastUpdated")] BuildingTicket buildingTicket)
        {
            if (ModelState.IsValid)
            {
                buildingTicket.Id = Guid.NewGuid();
                buildingTicket.Type = Ticket.TicketType.BuildingsAndGrounds;
                _context.Add(buildingTicket);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(buildingTicket);
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
                await _context.SaveChangesAsync();
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
            {
                return NotFound();
            }

            var buildingTicket = await _context.BuildingTickets
               .Where(m => m.Id == id)
               .Include(a => a.Updates)
               .FirstOrDefaultAsync();
            if (buildingTicket == null)
            {
                return NotFound();
            }

            ViewBag.Id = buildingTicket.Id;
            return View(buildingTicket);
        }

        // POST: BuildingTickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Location,Id,Username,FName,LName,EMail,Phone,Category,Type,Description,Severity,Status,AssignedTo,Notes,Resolution,DateSubmitted,LastUpdated")] BuildingTicket buildingTicket)
        {
            if (id != buildingTicket.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(buildingTicket);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BuildingTicketExists(buildingTicket.Id))
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
            return View(buildingTicket);
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
            var buildingTicket = await _context.BuildingTickets.FindAsync(id);
            _context.BuildingTickets.Remove(buildingTicket);
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
                        .Where(x => (x.Status >= Ticket.TicketStatus.Reviewed) && (x.Status < Ticket.TicketStatus.Closed))
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
