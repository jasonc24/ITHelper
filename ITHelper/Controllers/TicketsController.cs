using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ITHelper.Data;
using ITHelper.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Net.Mail;
using ITHelper.Helpers;
using System.Net.Mime;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITHelper.Controllers
{
    //[Authorize(Roles = "Domain Users")]
    public class TicketsController : MyController
    {
        #region Initialization

        /// <summary>
        /// Constructor with dependency injection for Db context
        /// </summary>
        public TicketsController(ITHelperContext context) : base(context) { }

        #endregion

        #region Action Methods

        // GET: Tickets
        [AllowAnonymous]
        [Route("~/Tickets/Index/{categories?}/{ticketStatuses?}/{severity?}/{pageNo?}")]
        public async Task<IActionResult> Index(string categories = "All", string ticketStatuses = "All", string severity = "All", int pageNo = 0)
        {
            var catList = await SetCategoryFilterAsync(categories);
            var statusList = SetStatusFilter(ticketStatuses);
            var severityList = SetSeverityFilter(severity);
            
            var ticketQuery = await GetTicketsQueryAsync(catList.Select(x => x.Id), statusList, severityList);
            var ticketList = await GetUserTickets(ticketQuery);
                        
            var itemsPerPage = 15;
            var totalItems = ticketList.Count();
            pageNo = SetPageInformation(pageNo, totalItems, itemsPerPage);
            ViewBag.baseURL = $"/ITHelper/Tickets/Index/{categories}/{ticketStatuses}/{severity}";
            ViewBag.DetailsMethod = "Details";

            var returnList = ticketList.Skip(itemsPerPage * pageNo).Take(itemsPerPage);
            return View(returnList);
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

            ViewBag.TicketId = id;

            return View(ticket);
        }

        // GET: Tickets/Create
        public async Task<IActionResult> Create()
        {
            var ticket = new Ticket();
            ticket.ParentCategories = await GetParentCategoriesAsync(null, null);
            ticket.Categories = await GetSubCategoriesAsync(null, null);
            ticket.Locations = await GetLocationsAsync(null);
            return View(ticket);
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                ticket.Status = (ticket.Status == Ticket.TicketStatus.New) ? Ticket.TicketStatus.Submitted : ticket.Status;
                ticket.DateSubmitted = DateTimeOffset.Now;
                ticket.LastUpdated = DateTimeOffset.Now;
                _context.Add(ticket);
                await _context.SaveChangesAsync();

                var message = await GenerateMessage("Create", ticket.Id, false);
                await SendNotification(message);
                return RedirectToAction(nameof(Index));
            }

            ticket.Category = await _context.Categories.FindAsync(ticket.CategoryId);
            ticket.ParentCategories = await GetParentCategoriesAsync(ticket.Category?.ParentCategoryId, null);
            ticket.Categories = await GetSubCategoriesAsync(ticket?.Category?.ParentCategoryId.Value, ticket.CategoryId);
            ticket.Locations = await GetLocationsAsync(ticket.LocationId);

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
            var ticket = await _context.Tickets.FindAsync(id);
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
        public async Task<IActionResult> AddUpdate(Guid id, Update update, IFormCollection collection)
        {
            update.Ticket = await _context.Tickets.FindAsync(Guid.Parse(collection["ticketId"]));
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

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            { return NotFound(); }

            var ticket = await GetTicketAsync(id.Value);
            if (ticket == null)
            { return NotFound(); }

            ViewBag.TicketId = id;

            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Ticket ticket)
        {
            if (id != ticket.Id)
            { return NotFound(); }

            if (ModelState.IsValid)
            {
                try
                {
                    ticket.LastUpdated = DateTimeOffset.Now;
                    if (!string.IsNullOrEmpty(ticket.Resolution))
                    { ticket.Status = Ticket.TicketStatus.Closed; }
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

            ticket.Category = await _context.Categories.FindAsync(ticket.CategoryId);
            ticket.ParentCategories = await GetParentCategoriesAsync(ticket.Category?.ParentCategoryId, null);
            ticket.Categories = await GetSubCategoriesAsync(ticket?.Category?.ParentCategoryId.Value, ticket.CategoryId);
            ticket.Locations = await GetLocationsAsync(ticket.LocationId);
            ViewBag.TicketId = ticket.Id;

            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            { return NotFound(); }

            var ticket = await GetTicketAsync(id.Value);
            if (ticket == null)
            { return NotFound(); }

            ViewBag.TicketId = id;

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var ticket = await _context.Tickets.FindAsync(id);

            if ((ticket.Status != Ticket.TicketStatus.Closed) && User.IsInRole("Domain Admins"))    // Don't send delete notices for resolved items
            {
                var message = await GenerateMessage("Delete", ticket.Id, false);
                await SendNotification(message);
            }

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [AllowAnonymous]
        public JsonResult GetSubCategories(string parentCategory)
        {
            var categories = new List<SelectListItem>();
            try { categories = GetSubCategoriesAsync(Guid.Parse(parentCategory), null).Result; }
            catch { categories.Add(new SelectListItem() { Text = "Please Select...", Value = "" }); }
            return Json(JsonConvert.SerializeObject(categories));
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Indicates if the specified ticket exists in the system or not
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected bool TicketExists(Guid id)
        { return _context.Tickets.Any(e => e.Id == id); }

        /// <summary>
        /// Returns the ticket specified by the Guid provided
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected async Task<Ticket> GetTicketAsync(Guid id)
        {
            var ticket = await _context.Tickets
                .Where(m => m.Id == id)
                .Include(a => a.Updates)
                .Include(b => b.Location)
                .Include(c => c.Category)
                .ThenInclude(d => d.ParentCategory)
                .FirstOrDefaultAsync();

            if (ticket == null)
                throw new ArgumentException("Invalid Ticket Id", "Id");

            ticket.ParentCategories = await GetParentCategoriesAsync(ticket.Category?.ParentCategoryId ?? ticket.CategoryId, null);
            ticket.Categories = await GetSubCategoriesAsync(ticket?.Category?.ParentCategoryId, ticket.CategoryId);
            ticket.Locations = await GetLocationsAsync(ticket.LocationId);

            return ticket;
        }

        /// <summary>
        /// Generates the mail message content used for distributing messages to users
        /// </summary>
        /// <returns></returns>
        protected async Task<MailMessage> GenerateMessage(string callingMethod, Guid id, bool resolved = false)
        {
            var ticket = await GetTicketAsync(id);
            var nameStub = ticket.Description.Length > 9 ? $"{ticket.Description.Substring(0, 9).Trim()}..." : ticket.Description.Trim();
            nameStub = nameStub.Replace('\r', ' ').Replace('\n', ' ');

            var subject = string.Empty;
            var body = string.Empty;
            switch (callingMethod)
            {
                case "Create":
                    subject = $"New Ticket Created - {nameStub}";
                    body = await this.RenderViewAsync("~/Views/EMail/TicketCreated.cshtml", ticket, false);
                    break;

                case "Edit":
                    subject = resolved ? $"Ticket Resolved - {nameStub}" : $"Ticket Edited - {nameStub}";
                    body = await this.RenderViewAsync("~/Views/EMail/TicketEdited.cshtml", ticket, false);
                    break;

                case "Update":
                    subject = resolved ? $"Ticket Resolved - {nameStub}" : $"Ticket Updated - {nameStub}";
                    body = await this.RenderViewAsync("~/Views/EMail/TicketUpdated.cshtml", ticket, false);
                    break;

                case "Delete":
                    subject = $"Ticket Deleted - {nameStub}";
                    body = await this.RenderViewAsync("~/Views/EMail/TicketDeleted.cshtml", ticket, false);
                    break;
            }

            var sender = (await GetSysParam(10).ConfigureAwait(false)).Value;
            var message = new MailMessage()
            {
                From = new MailAddress(sender),
                Sender = new MailAddress(sender),
                Subject = subject,
                IsBodyHtml = true
            };

            message.To.Add(ticket.EMail);

            var primaryCategoryUser = $"{ticket.Category.PrimaryContact} <{ticket.Category.PrimaryEMail}>";
            message.To.Add(primaryCategoryUser);
            message.ReplyToList.Add(primaryCategoryUser);

            if (ticket.Category.ParentCategory != null)
            {
                var superCategroyUser = $"{ticket.Category.ParentCategory.PrimaryContact} <{ticket.Category.ParentCategory.PrimaryEMail}>";
                message.CC.Add(superCategroyUser);
            }

            if (ticket.Location.SendEmail)
            {
                var locationRecipient = $"{ticket.Location.PrimaryContact} <{ticket.Location.PrimaryEMail}>";
                message.CC.Add(locationRecipient);
            }

            var admin = (await GetSysParam(6).ConfigureAwait(false)).Value;
            var adminEmails = admin.Split(';');
            var adminReceives = false;
            bool.TryParse((await GetSysParam(7).ConfigureAwait(false)).Value, out adminReceives);
            if (adminReceives)
            {
                foreach (var address in adminEmails)
                    message.CC.Add(address);
            }

            message.To.Add(ticket.EMail);
            try { message.To.Add(new MailAddress(ticket.AssignedTo)); } catch { }

            AlternateView av = AlternateView.CreateAlternateViewFromString(body, null, MediaTypeNames.Text.Html);
            message.AlternateViews.Add(av);

            return message;
        }

        /// <summary>
        /// Returns the list of applicable tickets for the user logged in
        /// </summary>
        /// <param name="ticketQuery"></param>
        /// <returns></returns>
        protected async Task<List<Ticket>> GetUserTickets(IQueryable<Ticket> ticketQuery)
        {
            if (User.IsInRole("Domain Admins"))
                return await ticketQuery.ToListAsync();

            var userId = User.Identity.Name;

            var ticketList = await ticketQuery
                    .Where(x => x.Username == userId)
                    .ToListAsync();

            var ownedCategories = await _context.Categories
                .Include(a => a.ParentCategory)
                .Where(x => x.UserName.Equals(userId)
                    || x.ParentCategory.UserName.Equals(userId))
                .Select(y => y.Id)
                .ToListAsync();

            ticketList.AddRange(
                await ticketQuery.Where(x => ownedCategories.Contains(x.CategoryId))
                .ToListAsync()
                );

            return ticketList.OrderByDescending(x => x.LastUpdated).Distinct().ToList();
        }

        #endregion
    }
}
