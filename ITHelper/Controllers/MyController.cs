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
using static ITHelper.Models.Enumerations;

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
            { int.TryParse(Utilities.SystemHelpers.SystemHelper.GetConfigValue("AppSettings:ItemsPerPage"), out itemsPerPage); }
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
        /// <param name="selectedItems"></param>
        /// <returns></returns>
        protected List<SelectListItem> GetTicketStatusSelectList(List<string> selectedItems)
        {
            var list = new List<SelectListItem>();
            list.Add(new SelectListItem() { Text = " All Statuses...", Value = "All", Selected = selectedItems == null || (selectedItems.Contains("All")) });

            var selectedItemsList = Utilities.SystemHelpers.EnumHelper<TicketStatus>.TransformEnumString(TicketStatus.Submitted, 
                string.Join(",", selectedItems));
            list.AddRange(Utilities.SystemHelpers.EnumHelper<TicketStatus>.GetEnumSelectList(TicketStatus.Submitted, 
                selectedItemsList, true));

            if (selectedItems.Contains("Default"))
            {
                foreach(var item in list)
                {
                    if (!item.Value.Equals("All") || !item.Value.Equals("6"))
                        item.Selected = true;
                    else
                        item.Selected = false;
                }                           
            }

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
                .Where(w => !w.Deleted && (w.ParentCategory == null))
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
        protected virtual async Task<List<SelectListItem>> GetSubCategoriesAsync(Guid? parentItem, Guid? selectedItem)
        {
            var categories = new List<SelectListItem>();
            categories.Add(new SelectListItem() { Text = " Please Select...", Value = "", Selected = selectedItem == null });
            categories.AddRange(await _context.Categories
                .Where(w => !w.Deleted && ((w.ParentCategory.Id == parentItem) || w.Id.Equals(parentItem)))
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
        /// <param name="selectedItems"></param>
        /// <param name="excludedItem"></param>
        /// <returns></returns>
        protected async Task<IOrderedEnumerable<SelectListItem>> GetCategoriesAsync(List<string> selectedItems)
        {
            var categories = await _context.Categories.OrderBy(x => x.Name).ToArrayAsync();
            var catList = new List<SelectListItem>();
            catList.Add(new SelectListItem() { Text = " All Categories...", Value = "All", Selected = selectedItems == null || (selectedItems.Contains("All")) });

            var i = 0;
            foreach (var category in categories)
            {
                catList.Add(new SelectListItem()
                {
                    Text = category.DisplayName,
                    Value = i.ToString(),
                    Selected = selectedItems.Contains(i.ToString())
                });
                i++;
            }

            return catList.OrderBy(y => y.Text);
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
        /// Transforms the user selected indexes to Guid for use in the Linq queries
        /// </summary>
        /// <param name="indexes"></param>
        /// <returns></returns>
        protected async Task<List<Guid>> GetCategoriesFromIndexAsync(List<string> indexes)
        {
            var catList = await _context.Categories
                .OrderBy(x => x.Name)
                .ToArrayAsync();

            var returnList = new List<Guid>();
            if (!indexes.Contains("All"))
            {
                foreach (var item in indexes)
                {
                    var index = int.Parse(item);
                    returnList.Add(catList[index].Id);
                }
            }
            else
            {
                foreach (var item in catList)
                { returnList.Add(item.Id); }
            }

            return returnList;
        }

        /// <summary>
        /// Returns the status list for the selected statuses based on the user request
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        protected List<string> GetStatusFromIndex(string status)
        {
            var statusList = new List<string>();
            var values = Utilities.SystemHelpers.EnumHelper<TicketStatus>.GetValues(TicketStatus.Submitted).ToList();

            // Look for special cases...
            switch (status)
            {
                case "All":
                    statusList = values.Select(x => x.ToString()).ToList();
                    break;

                case "Default":
                    statusList = values.Select(x => x.ToString()).ToList();
                    statusList.Remove("Closed");
                    break;

                default:
                    statusList = status.Split(",").ToList();
                    break;
            }

            return statusList;
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

        /// <summary>
        /// Retrieves the query for the associated ticket types based on the selected request
        /// </summary>
        /// <param name="statuses"></param>
        /// <returns></returns>
        protected async Task<IQueryable<Ticket>> GetTicketsQueryAsync(IEnumerable<Guid> catList, IEnumerable<TicketStatus> statuses, IEnumerable<TicketSeverity> severities)
        {
            catList = catList.Count() == 0 ? (await _context.Categories.Select(x => x.Id).ToListAsync()) : catList;
            IOrderedQueryable<Ticket> ticketQuery = null;
            ticketQuery = _context.Tickets
                .Where(x => catList.Contains(x.CategoryId)
                    && statuses.Contains(x.Status)
                    && severities.Contains(x.Severity))
                .Include(a => a.Category)
                .ThenInclude(b => b.ParentCategory)
                .OrderByDescending(y => y.LastUpdated);

            return ticketQuery;
        }

        /// <summary>
        /// This method sets the filtering criteria to be used by multiple operations for filtering specific tickets
        /// </summary>
        /// <param name="categories"></param>
        /// <param name="ticketStatuses"></param>
        /// <param name="severity"></param>
        /// <returns></returns>
        protected async Task<IEnumerable<Category>> SetCategoryFilterAsync(string categories = "All")
        {
            var categoryList = await _context.Categories.Where(x => !x.Deleted).ToListAsync();
            Func<IEnumerable<Category>, IOrderedEnumerable<Category>> categorySortFunction = (x) => x.OrderBy(y => y.DisplayName);
            var catList = Utilities.SystemHelpers.SelectListHelper<Category>.DecodeSelection(categories, categoryList, categorySortFunction);
            Func<IEnumerable<Category>, IOrderedEnumerable<string>> catDisplayFunction = (x) => x.Select(y => y.DisplayName).OrderBy(z => z);
            ViewBag.CategoryList = Utilities.SystemHelpers.SelectListHelper<Category>.EncodeSelectList(categoryList, catDisplayFunction, categories);
            return catList;           
        }

        /// <summary>
        /// This method sets the filtering criteria to be used by multiple operations for filtering specific tickets
        /// </summary>
        /// <param name="categories"></param>
        /// <param name="ticketStatuses"></param>
        /// <param name="severity"></param>
        /// <returns></returns>
        protected IEnumerable<TicketStatus> SetStatusFilter(string ticketStatuses = "All")
        {
            var statuses = Utilities.SystemHelpers.EnumHelper<TicketStatus>.GetValues(TicketStatus.Submitted);
            Func<IEnumerable<TicketStatus>, IOrderedEnumerable<TicketStatus>> statusSortFunction = (x) => x.OrderBy(y => (int)y);
            var statusList = Utilities.SystemHelpers.SelectListHelper<TicketStatus>.DecodeSelection(ticketStatuses, statuses, statusSortFunction);
            Func<IEnumerable<TicketStatus>, IOrderedEnumerable<string>> statusDisplayFunction = (x) => Utilities.SystemHelpers.EnumHelper<TicketStatus>.GetDisplayNames(TicketStatus.Submitted).OrderBy(y => 0);
            ViewBag.TicketStatusList = Utilities.SystemHelpers.SelectListHelper<TicketStatus>.EncodeSelectList(statuses, statusDisplayFunction, ticketStatuses);
            return statusList;            
        }

        /// <summary>
        /// This method sets the filtering criteria to be used by multiple operations for filtering specific tickets
        /// </summary>
        /// <param name="categories"></param>
        /// <param name="ticketStatuses"></param>
        /// <param name="severity"></param>
        /// <returns></returns>
        protected IEnumerable<TicketSeverity> SetSeverityFilter(string severity = "All")
        {
            var severities = Utilities.SystemHelpers.EnumHelper<TicketSeverity>.GetValues(TicketSeverity.Low);
            Func<IEnumerable<TicketSeverity>, IOrderedEnumerable<TicketSeverity>> severitySortFunction = (x) => x.OrderBy(y => (int)y);
            var severityList = Utilities.SystemHelpers.SelectListHelper<TicketSeverity>.DecodeSelection(severity, severities, severitySortFunction);
            Func<IEnumerable<TicketSeverity>, IOrderedEnumerable<string>> severityDisplayFunction = (x) => Utilities.SystemHelpers.EnumHelper<TicketSeverity>.GetNames(TicketSeverity.Low).OrderBy(y => 0);
            ViewBag.SeverityList = Utilities.SystemHelpers.SelectListHelper<TicketSeverity>.EncodeSelectList(severities, severityDisplayFunction, severity);
            return severityList;
        }

        #endregion
    }
}