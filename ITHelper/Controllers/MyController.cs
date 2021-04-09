using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using ITHelper.Data;
using ITHelper.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Utilities.Messaging;

namespace ITHelper.Controllers
{
    public class MyController : Controller
    {
        #region Instance Objects

        /// <summary>
        /// Db Context for Db interactions
        /// </summary>
        protected readonly ITHelperContext _context;

        #endregion

        #region Initialization

        /// <summary>
        /// Constructor taking Db context
        /// </summary>
        /// <param name="context"></param>
        public MyController(ITHelperContext context)
        { _context = context; }

        #endregion

        #region Methods

        /// <summary>
        /// Informs the user that the page they requested was not found
        /// </summary>
        /// <returns></returns>
        public new IActionResult NotFound()
        { return View("NotFound"); }

        /// <summary>
        /// Returns the user's requested items per page for most table pages
        /// </summary>
        /// <returns></returns>
        protected int GetItemsPerPage()
        {
            var itemsPerPage = 10;  //Default setting
            try
            {  int.TryParse(Utilities.SystemHelpers.SystemHelper.GetConfigValue("AppSettings:ItemsPerPage"), out itemsPerPage); }
            catch (Exception e) { }
            return (itemsPerPage);
        }

        /// <summary>
        /// Sets the paging information for indexes and table pages
        /// </summary>
        /// <param name="pageNo"></param>
        /// <param name="totalItems"></param>
        /// <param name="itemsPerPage"></param>
        /// <returns></returns>
        protected virtual int SetPageInformation(int pageNo, int totalItems, int itemsPerPage)
        {
            var totalPages = totalItems / itemsPerPage;
            totalPages = ((totalItems % itemsPerPage) == 0) ? totalPages : totalPages + 1;
            ViewBag.totalPages = totalPages;
            ViewBag.totalItems = totalItems;

            if (pageNo >= totalPages)
                pageNo = totalPages - 1;

            if (pageNo < 0)
                pageNo = 0;

            ViewBag.pageNo = pageNo;
            ViewBag.totalPages = totalPages;
            ViewBag.itemsPerPage = itemsPerPage;

            return (pageNo);
        }

        /// <summary>
        /// Creates the select list for the ticket status
        /// </summary>
        /// <param name="ticketStatus"></param>
        /// <returns></returns>
        protected List<SelectListItem> GetTicketStatusSelectList(int ticketStatus)
        {
            var list = new List<SelectListItem>();
            list.Add(new SelectListItem() { Text = "Open Items", Value = "1", Selected = (ticketStatus <= 1) });
            list.Add(new SelectListItem() { Text = "Unassigned Tickets", Value = "2", Selected = (ticketStatus == 2) });
            list.Add(new SelectListItem() { Text = "In-Process Tickets", Value = "3", Selected = (ticketStatus == 3) });
            list.Add(new SelectListItem() { Text = "Closed Tickets", Value = "4", Selected = (ticketStatus == 4) });
            list.Add(new SelectListItem() { Text = "All Tickets", Value = "5", Selected = (ticketStatus == 5) });

            return list;
        }

        /// <summary>
        /// Returns a list of categories for the application to use
        /// </summary>
        /// <param name="selectedItem"></param>
        /// <param name="excludedItem"></param>
        /// <returns></returns>
        protected async Task<List<SelectListItem>> GetParentCategoriesAsync(Guid? selectedItem, Guid? excludedItem)
        {
            var categories = new List<SelectListItem>();
            categories.Add(new SelectListItem() { Text = "Please Select...", Value = "", Selected = selectedItem == null });

            categories.AddRange(await _context.Categories
                .Where(w => w.ParentCategory == null)
                .OrderBy(x => x.Name)
                .Select(y => new SelectListItem()
                {
                    Text = y.Name,
                    Value = y.Id.ToString(),
                    Selected = y.Id.Equals(selectedItem),
                    Disabled = y.Id.Equals(excludedItem)
                })
                .ToListAsync());

            return categories;
        }

        /// <summary>
        /// Returns a list of matching the subcategory requested
        /// </summary>
        /// <param name="parentItem"></param>
        /// <param name="excludedItem"></param>
        /// <returns></returns>
        protected async Task<List<SelectListItem>> GetSubCategoriesAsync(Guid? parentItem, Guid? selectedItem)
        {
            var categories = new List<SelectListItem>();
            categories.Add(new SelectListItem() { Text = " Please Select...", Value = "", Selected = selectedItem == null });
            categories.AddRange(await _context.Categories
                .Where(w => (w.ParentCategory.Id == parentItem) || w.Id.Equals(parentItem))
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

        /// <summary>
        /// Returns a list of categories for the application to use
        /// </summary>
        /// <param name="selectedItem"></param>
        /// <param name="excludedItem"></param>
        /// <returns></returns>
        protected async Task<IOrderedEnumerable<SelectListItem>> GetCategoriesAsync(string selectedItem)
        {
            var categories = new List<SelectListItem>();
            categories.Add(new SelectListItem() { Text = " All Categories...", Value = "ALL", Selected = selectedItem == null || selectedItem.Equals("all", StringComparison.OrdinalIgnoreCase) });

            categories.AddRange(await _context.Categories
                .Where(x => x.ParentCategoryId == null)
                .Select(y => new SelectListItem()
                {
                    Text = y.DisplayName,
                    Value = y.Name,
                    Selected = y.Name.Equals(selectedItem)
                })
                .ToListAsync());

            categories.AddRange(await _context.Categories
                .OrderBy(x => x.Name)
                .Select(y => new SelectListItem()
                {
                    Text = y.DisplayName,
                    Value = y.Name.ToString(),
                    Selected = y.Name.Equals(selectedItem)
                })
                .ToListAsync());

            return categories.OrderBy(y => y.Text);
        }

        /// <summary>
        /// Generates a List of SelectListItems for use by the users for selecting configured locations
        /// </summary>
        /// <param name="selectedLocation"></param>
        /// <returns></returns>
        protected async Task<List<SelectListItem>> GetLocationsAsync(Guid? selectedLocation)
        {
            var locations = new List<SelectListItem>();
            locations.Add(new SelectListItem() { Text = "Please Select...", Value = "", Selected = selectedLocation == null });

            locations.AddRange(await _context.Locations
                .OrderBy(x => x.Name)
                .Select(y => new SelectListItem()
                {
                    Text = y.Name,
                    Value = y.Id.ToString(),
                    Selected = y.Id.Equals(selectedLocation)
                })
                .ToListAsync());

            return locations;
        }

        /// <summary>
        /// Retrieves the System Parameter object identified by the id value
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected async Task<SystemParameter> GetSysParam(int id)
        {
            var parameter = await _context.SystemParameters.FindAsync(id);
            if (parameter == null)
            { throw new ArgumentException($"Parameter \"{id}\" does not exist.  Please check and try again."); }

            return parameter;
        }

        /// <summary>
        /// Returns an instance of the Message Helper utility class to send emails to recipients
        /// </summary>
        /// <returns></returns>
        protected async Task<MessageHelper> GetMessageHelperAsync()
        {
            var server = (await GetSysParam(1).ConfigureAwait(false)).Value;
            var username = (await GetSysParam(2).ConfigureAwait(false)).Value;
            var password = (await GetSysParam(3).ConfigureAwait(false)).Value;

            var port = 587;
            int.TryParse((await GetSysParam(4).ConfigureAwait(false)).Value, out port);

            var tsl = true;
            bool.TryParse((await GetSysParam(5).ConfigureAwait(false)).Value, out tsl);

            var timeout = 10000;
            int.TryParse((await GetSysParam(1002).ConfigureAwait(false)).Value, out timeout);

            var domain = (await GetSysParam(11).ConfigureAwait(false)).Value;

            var relay = new MessageHelper(server, domain, username, password, port, tsl, timeout);
            return relay;
        }

        /// <summary>
        /// Send a notification to the user of the specified update
        /// </summary>
        /// <returns></returns>
        protected async Task SendNotification(MailMessage message)
        {
            var mailClient = await GetMessageHelperAsync();
            mailClient.SendMessageAsync(message, DateTimeOffset.Now.Second);
        }

        #endregion
    }
}