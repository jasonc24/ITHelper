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
using System.Net.Mail;
using System.Net.Mime;
using Utilities.SystemHelpers;

namespace ITHelper.Controllers
{
    public class AVTicketsController : MyController
    {
       
        public AVTicketsController(ITHelperContext context) : base(context)
        { }

        // GET: AVTickets
        [AllowAnonymous]
        [Route("~/AVTickets/Index/{pageNo?}")]
        public async Task<IActionResult> Index(int pageNo = 0)
        {
            var ticketList = await _context.AVTickets
                .Where(x => (x.Status != Enumerations.TicketStatus.Closed) || (x.Status != Enumerations.TicketStatus.Rejected))
                .Include(a => a.Category)
                .Include(a => a.Location)
                .ToListAsync();
            var itemsPerPage = 15;
            var totalItems = ticketList.Count();

            pageNo = SetPageInformation(pageNo, totalItems, itemsPerPage);
            ViewBag.baseURL = $"/ITHelper/AVTickets/Index";
            ViewBag.DetailsMethod = "Details";

            var returnList = ticketList.Skip(itemsPerPage * pageNo).Take(itemsPerPage);
            return View(returnList);
        }

        // GET: AVTickets/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            { return NotFound(); }

            var ticket = await GetTicketAsync(id.Value);
            if (ticket == null)
            { return NotFound(); }

            return View(ticket);
        }

        // GET: AVTickets/Create
        public async Task<IActionResult> Create()
        {
            var ticket = new AVTicket();
            ticket.Categories = await GetCategoriesAsync(null);
            ticket.Locations = await GetLocationsAsync(null);
            return View(ticket);
        }

        // POST: AVTickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AVTicket ticket)
        {
            if (ModelState.IsValid)
            {
                ticket.Status = (ticket.Status == Enumerations.TicketStatus.New) ? Enumerations.TicketStatus.Submitted : ticket.Status;
                ticket.Status = Enumerations.TicketStatus.Submitted;
                ticket.DateSubmitted = DateTimeOffset.Now;
                ticket.LastUpdated = DateTimeOffset.Now;
                _context.Add(ticket);
                await _context.SaveChangesAsync();

                var message = await GenerateMessage("Create", ticket.Id, false);
                await SendNotification(message);
                return RedirectToAction(nameof(Index));
            }
            
            ticket.Categories = await GetCategoriesAsync(ticket.CategoryId);
            ticket.Locations = await GetLocationsAsync(ticket.LocationId);
            return View(ticket);
        }

        // GET: AVTickets/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            { return NotFound(); }

            var ticket = await GetTicketAsync(id.Value);
            if (ticket == null)
            { return NotFound(); }

            ticket.Categories = await GetCategoriesAsync(ticket.CategoryId);
            ticket.Locations = await GetLocationsAsync(ticket.LocationId);

            return View(ticket);
        }

        // POST: AVTickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, AVTicket ticket)
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

                    var message = await GenerateMessage("Edit", ticket.Id, ticket.Status == Enumerations.TicketStatus.Closed);
                    await SendNotification(message);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AVTicketExists(ticket.Id))
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

            ticket.Categories = await GetCategoriesAsync(ticket.CategoryId);
            ticket.Locations = await GetLocationsAsync(ticket.LocationId);

            return View(ticket);
        }

        // GET: AVTickets/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aVTicket = await _context.AVTickets
                .Include(a => a.Category)
                .Include(a => a.Location)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (aVTicket == null)
            {
                return NotFound();
            }

            return View(aVTicket);
        }

        // POST: AVTickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var aVTicket = await _context.AVTickets.FindAsync(id);
            _context.AVTickets.Remove(aVTicket);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        #region Private & Protected Methods

        private bool AVTicketExists(Guid id)
        {
            return _context.AVTickets.Any(e => e.Id == id);
        }

        /// <summary>
        /// Returns the ticket specified by the Guid provided
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected async Task<AVTicket> GetTicketAsync(Guid id)
        {
            var ticket = await _context.AVTickets
                .Where(m => m.Id == id)
                .Include(b => b.Location)
                .Include(c => c.Category)
                .ThenInclude(d => d.ParentCategory)
                .FirstOrDefaultAsync();

            if (ticket == null)
                throw new ArgumentException("Invalid Ticket Id", "Id");

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
            var nameStub = ticket.GeneralIdea.Length > 9 ? $"{ticket.GeneralIdea.Substring(0, 9).Trim()}..." : ticket.GeneralIdea.Trim();
            nameStub = nameStub.Replace('\r', ' ').Replace('\n', ' ');

            var subject = string.Empty;
            var body = string.Empty;
            switch (callingMethod)
            {
                case "Create":
                    subject = $"New AV Ticket Created - {nameStub}";
                    body = await this.RenderViewAsync("~/Views/EMail/AVTicketCreated.cshtml", ticket, false);
                    break;

                case "Edit":
                    subject = resolved ? $"AV Ticket Resolved - {nameStub}" : $"AV Ticket Edited - {nameStub}";
                    body = await this.RenderViewAsync("/Views/EMail/AVTicketEdited.cshtml", ticket, false);
                    break;

                case "Update":
                    subject = resolved ? $"AV Ticket Resolved - {nameStub}" : $"AV Ticket Updated - {nameStub}";
                    body = await this.RenderViewAsync("~/Views/EMail/AVTicketUpdated.cshtml", ticket, false);
                    break;

                case "Delete":
                    subject = $"AV Ticket Deleted - {nameStub}";
                    body = await this.RenderViewAsync("~/Views/EMail/AVTicketDeleted.cshtml", ticket, false);
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

            //message.To.Add(ticket.EMail);
            //try { message.To.Add(new MailAddress(ticket.AssignedTo)); } catch { }

            AlternateView av = AlternateView.CreateAlternateViewFromString(body, null, MediaTypeNames.Text.Html);
            message.AlternateViews.Add(av);

            return message;
        }

        /// <summary>
        /// Returns the category list specific to the AV Tickets
        /// </summary>
        /// <param name="parentItem"></param>
        /// <param name="selectedItem"></param>
        /// <returns></returns>
        //protected List<SelectListItem> GetCategories(Guid? selectedItem)
        //{
        //    var categories = new List<SelectListItem>();
        //    categories.Add(new SelectListItem() { Text = " Please Select...", Value = "", Selected = selectedItem == null });
        //    categories.Add(new SelectListItem() { Text = "Wedding", Value = "1", Selected = selectedItem == null });
        //    categories.Add(new SelectListItem() { Text = "Funeral", Value = "2", Selected = selectedItem == null });
        //    categories.Add(new SelectListItem() { Text = "Holiday Service", Value = "3", Selected = selectedItem == null });
        //    categories.Add(new SelectListItem() { Text = "Bar Mitzvah", Value = "4", Selected = selectedItem == null });

        //    return categories.OrderBy(x => x.Text).ToList();
        //}

        /// <summary>
        /// Returns a list of matching the subcategory requested
        /// </summary>
        /// <param name="parentItem"></param>
        /// <param name="excludedItem"></param>
        /// <returns></returns>
        protected async Task<List<SelectListItem>> GetCategoriesAsync(Guid? selectedItem)
        {
            var categories = new List<SelectListItem>();
            categories.Add(new SelectListItem() { Text = " Please Select...", Value = "", Selected = selectedItem == null });
            categories.AddRange(await _context.Categories
                .Where(w => !w.Deleted && w.ParentCategory.Name.Contains("AV"))
                .OrderBy(x => x.Name)
                .Distinct()
                .Select(y => new SelectListItem()
                {
                    Text = y.ParentCategory == null ? $"{y.Name} - General" : $"{y.ParentCategory.Name} - {y.Name}",
                    Value = y.Id.ToString(),
                    Selected = y.Id.Equals(selectedItem)
                })
                .ToListAsync());

            return categories.OrderBy(x => x.Text).ToList();
        }

        #endregion
    }
}
