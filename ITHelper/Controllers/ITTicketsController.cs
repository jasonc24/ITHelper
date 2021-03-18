using System;
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
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using ITHelper.Helpers;
using System.Net.Mime;

namespace ITHelper.Controllers
{
    [Authorize(Roles = "Domain Users")]
    public class ITTicketsController : MyController
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Constructor with dependency injection for Db context
        /// </summary>
        public ITTicketsController(ITHelperContext context, IConfiguration configuration) : base(context) { _configuration = configuration; }

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
            { return NotFound(); }

            var ticket = await GetTicketAsync(id.Value);
            if (ticket == null)
            { return NotFound(); }

            if ((ticket.Username != User.Identity.Name)
                && (!User.IsInRole("Domain Admins")))
            { throw new ArgumentException(); }

            ViewBag.Id = ticket.Id;
            return View(ticket);
        }

        // GET: Tickets/Create
        public IActionResult Create()
        { return View(new ITTicket() { Type = Ticket.TicketType.ITSupport }); }

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

                var message = await GenerateMessage("Update", update.Ticket.Id, update.IsResolved);
                await SendNotification(message);
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
            { return NotFound(); }

            var ticket = await _context.ITTickets
               .Where(m => m.Id == id)
               .Include(a => a.Updates)
               .FirstOrDefaultAsync();
            if (ticket == null)
            { return NotFound(); }

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
            { return NotFound(); }

            if (ModelState.IsValid)
            {
                try
                {
                    ticket.LastUpdated = DateTimeOffset.Now;
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();

                    var message = await GenerateMessage("Edit", ticket.Id, ticket.Status == Ticket.TicketStatus.Closed);
                    await SendNotification(message);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.Id))
                    { return NotFound(); }
                    else
                    { throw; }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            { return NotFound(); }

            var ticket = await _context.ITTickets
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            { return NotFound(); }

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

            if ((ticket.Status != Ticket.TicketStatus.Closed) && User.IsInRole("Domain Admins"))    // Don't send delete notices for resolved items
            {
                var message = await GenerateMessage("Delete", ticket.Id, false);
                await SendNotification(message);
            }

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
        { return _context.ITTickets.Any(e => e.Id == id); }

        /// <summary>
        /// Returns the ticket specified by the Guid provided
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected async Task<ITTicket> GetTicketAsync(Guid id)
        {
            var ticket = await _context.ITTickets
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
                        .Where(x => (x.AssignedTo != string.Empty) && (x.Status < Ticket.TicketStatus.Closed))
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
            var itAdmin = (await GetSysParam(8).ConfigureAwait(false)).Value;

            var ticket = await GetTicketAsync(id);
            var nameStub = ticket.Description.Length > 9 ? $"{ticket.Description.Substring(9).Trim()}..." : ticket.Description.Trim();
            var subject = string.Empty;
            var body = string.Empty;
            //var picURL = Environment.CurrentDirectory + @"\wwwroot";
            switch (callingMethod)
            {
                case "Create":
                    subject = $"New IT Ticket Created - {nameStub}";
                    body = await this.RenderViewAsync("~/Views/EMail/TicketCreated.cshtml", ticket, false);
                    //picURL += "/images/brokenPC.jpg";
                    break;

                case "Edit":
                    subject = resolved ? $"IT Ticket Resolved - {nameStub}" : $"IT Ticket Edited - {nameStub}";
                    body = await this.RenderViewAsync("~/Views/EMail/TicketEdited.cshtml", ticket, false);
                    //picURL += resolved ? "/images/happyPC.jpg" : "/images/brokenPCUpdate.jpg";
                    break;

                case "Update":
                    subject = resolved ? $"IT Ticket Resolved - {nameStub}" : $"IT Ticket Updated - {nameStub}";
                    body = await this.RenderViewAsync("~/Views/EMail/TicketUpdated.cshtml", ticket, false);
                    //picURL += resolved ? "/images/happyPC.jpg" : "/images/brokenPCUpdate.jpg";
                    break;

                case "Delete":
                    subject = $"IT Ticket Deleted - {nameStub}";
                    body = await this.RenderViewAsync("~/Views/EMail/TicketDeleted.cshtml", ticket, false);
                    //picURL += "/images/pen-and-pad.jpg";
                    break;
            }

            var message = new MailMessage()
            {
                From = new MailAddress(sender),
                Sender = new MailAddress(sender),
                Subject = subject,
                IsBodyHtml = true
            };
            message.To.Add(itAdmin);
            message.To.Add(ticket.EMail);
            if (adminReceives) message.CC.Add(admin);
            message.ReplyToList.Add(itAdmin);

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
