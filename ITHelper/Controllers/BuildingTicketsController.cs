using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ITHelper.Data;
using ITHelper.Models;
using Microsoft.AspNetCore.Http;
using ITHelper.Helpers;
using System.Net.Mail;
using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;

namespace ITHelper.Controllers
{
    [Authorize(Roles = "Domain Users")]
    public class BuildingTicketsController : MyController
    {
        /// <summary>
        /// Constructor with dependency injection for Db context
        /// </summary>
        /// <param name="context"></param>
        public BuildingTicketsController(ITHelperContext context) : base(context) { }

        // GET: BuildingTickets
        [AllowAnonymous]
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
        [AllowAnonymous]
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

                var message = await GenerateMessage("Create", ticket.Id, false);
                await SendNotification(message);

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
            var ticket = await _context.BuildingTickets.FindAsync(id);
            var u = new Update()
            {
                Id = new Guid(),
                Ticket = ticket,
                Status = ticket.Status,
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
        public async Task<IActionResult> AddUpdate(Guid id, [Bind("Id,Username,Notes,Status,IsResolved")] Update update, IFormCollection collection)
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
                update.Ticket.Status = update.IsResolved ? Ticket.TicketStatus.Closed : update.Status;
                _context.Update(update.Ticket);

                await _context.SaveChangesAsync();

                var message = await GenerateMessage("Update", update.Ticket.Id, update.IsResolved);
                await SendNotification(message);
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

                var message = await GenerateMessage("Edit", ticket.Id, ticket.Status == Ticket.TicketStatus.Closed);
                await SendNotification(message);

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

            if ((ticket.Status != Ticket.TicketStatus.Closed) && User.IsInRole("Domain Admins"))     // Don't send delete notices for resolved items
            {
                var message = await GenerateMessage("Delete", ticket.Id, false);
                await SendNotification(message);
            }

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
                        .Where(x => string.IsNullOrEmpty(x.AssignedTo))
                        .OrderByDescending(y => y.LastUpdated);
                    break;

                case 3:
                    ticketQuery = _context.BuildingTickets          // Tickets which have been assigned to someone
                        .Where(x => (x.Status >= Ticket.TicketStatus.AssignedInternally) && (x.Status < Ticket.TicketStatus.Closed))
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

        /// <summary>
        /// Generates the mail message content used for distributing messages to users
        /// </summary>
        /// <returns></returns>
        private async Task<MailMessage> GenerateMessage(string callingMethod, Guid id, bool resolved = false)
        {
            var sender = (await GetSysParam(10).ConfigureAwait(false)).Value;
            var admin = (await GetSysParam(6).ConfigureAwait(false)).Value;
            var adminReceives = true;
            bool.TryParse((await GetSysParam(7).ConfigureAwait(false)).Value, out adminReceives);
            var bgAdmin = (await GetSysParam(9).ConfigureAwait(false)).Value;

            var ticket = await GetTicketAsync(id);
            var nameStub = ticket.Description.Length > 9 ? $"{ticket.Description.Substring(9)}..." : ticket.Description;
            var subject = string.Empty;
            var body = string.Empty;
            //var picURL = Environment.CurrentDirectory + @"\wwwroot";
            switch (callingMethod)
            {
                case "Create":
                    subject = $"New Buildings & Grounds Ticket Created - {nameStub}";
                    body = await this.RenderViewAsync("~/Views/EMail/BuildingTicketCreated.cshtml", ticket, false);
                    //picURL += "/images/brokenBuilding.jpg";
                    break;

                case "Edit":
                    subject = resolved ? $"Buildings & Grounds Ticket Resolved - {nameStub}" : $"Buildings & Grounds Ticket Edited - {nameStub}";
                    body = await this.RenderViewAsync("~/Views/EMail/BuildingTicketEdited.cshtml", ticket, false);
                    //picURL += resolved ? "/images/HopeChurch.jpg" : "/images/buildingRepair.jpg";
                    break;

                case "Update":
                    subject = resolved ? $"Buildings & Grounds Ticket Resolved - {nameStub}" : $"Buildings & Grounds Ticket Updated - {nameStub}";
                    body = await this.RenderViewAsync("~/Views/EMail/BuildingTicketUpdated.cshtml", ticket, false);
                    //picURL += resolved ? "/images/HopeChurch.jpg" : "/images/buildingRepair.jpg";
                    break;

                case "Delete":
                    subject = $"Buildings & Grounds Ticket Deleted - {nameStub}";
                    body = await this.RenderViewAsync("~/Views/EMail/BuildingTicketDeleted.cshtml", ticket, false);
                    //picURL += "/images/deleteJob.jpg";
                    break;
            }

            var message = new MailMessage()
            {
                From = new MailAddress(sender),
                Sender = new MailAddress(sender),
                Subject = subject,
                IsBodyHtml = true
            };
            message.To.Add(bgAdmin);
            message.To.Add(ticket.EMail);
            if (adminReceives) message.CC.Add(admin);
            message.ReplyToList.Add(bgAdmin);

            AlternateView av = AlternateView.CreateAlternateViewFromString(body, null, MediaTypeNames.Text.Html);
            //LinkedResource pic1 = new LinkedResource(picURL, MediaTypeNames.Image.Jpeg);
            //pic1.ContentId = "Pic1";
            //av.LinkedResources.Add(pic1);
            message.AlternateViews.Add(av);

            return message;
        }

        #endregion
    }
}
