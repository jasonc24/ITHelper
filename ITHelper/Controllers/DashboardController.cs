using ITHelper.Data;
using ITHelper.Helpers;
using ITHelper.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITHelper.Controllers
{
    public class DashboardController : MyController
    {
        #region Initialization 

        public DashboardController(ITHelperContext context) : base(context) { }

        #endregion

        #region Action Methods

        public async Task<IActionResult> Index()
        {
            await GenerateTicketStatusChartAsync("All", "0,1,2,3,4,5,6", "All", false);
            await GenerateActivityChartAsync(DateTime.Now.AddMonths(-1), DateTime.Now, "All", "All", false);
            await GenerateRequestTypeChartAsync(DateTime.Now.AddMonths(-1), DateTime.Now, "All", "All", false);
            return View();
        }

        [Route("~/Dashboard/TicketStatus/{categories?}/{ticketStatuses?}/{severity?}")]
        public async Task<IActionResult> TicketStatus(string categories = "All", string ticketStatuses = "All", string severity = "All")
        {
            await GenerateTicketStatusChartAsync(categories, ticketStatuses, severity, true);
            return View();
        }

        [Route("~/Dashboard/Activity/{startTime:DateTime?}/{endTime:DateTime?}/{categories?}/{ticketStatuses?}/{severity?}")]
        public async Task<IActionResult> Activity(DateTime? startTime, DateTime? endTime, string categories = "All", string ticketStatuses = "All", string severity = "All")
        {
            await GenerateActivityChartAsync(startTime, endTime, categories, severity, true);
            return View();
        }

        [Route("~/Dashboard/RequestTypes/{startTime:DateTime?}/{endTime:DateTime?}/{categories?}/{severity?}")]
        public async Task<IActionResult> RequestTypes(DateTime? startTime, DateTime? endTime, string categories = "All", string severity = "All")
        {
            await GenerateRequestTypeChartAsync(startTime, endTime, categories, severity, true);
            return View();
        }

        #endregion

        #region Protected & Internal Methods

        /// <summary>
        /// Retrieves a list of the tickets matching the requested filter criteria
        /// </summary>
        /// <param name="categories"></param>
        /// <param name="ticketStatuses"></param>
        /// <param name="severity"></param>
        /// <returns></returns>
        protected async Task<IEnumerable<Ticket>> GetTicketsAsync(string categories, string ticketStatuses, string severity)
        {
            categories = categories ?? "All";
            ticketStatuses = ticketStatuses ?? "All";
            severity = severity ?? "All";

            var catList = await SetCategoryFilterAsync(categories);
            var statusList = SetStatusFilter(ticketStatuses);
            var severityList = SetSeverityFilter(severity);

            var ticketQuery = await GetTicketsQueryAsync(catList.Select(x => x.Id), statusList, severityList);
            var ticketList = await ticketQuery.ToListAsync();

            return ticketList;
        }

        /// <summary>
        /// Retrieves a list of th tickets matching the requested filter criteria which were closed between the specified dates
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="categories"></param>
        /// <param name="ticketStatuses"></param>
        /// <param name="severity"></param>
        /// <returns></returns>
        protected async Task<IEnumerable<Ticket>> GetActivityListAsync(DateTime? startTime, DateTime? endTime, string categories, string ticketStatuses, string severity)
        {
            startTime = startTime ?? DateTime.Now.AddDays(-7);
            endTime = endTime ?? DateTime.Now;
            categories = categories ?? "All";
            ticketStatuses = ticketStatuses ?? "All";
            severity = severity ?? "All";

            var catList = await SetCategoryFilterAsync(categories);
            var statusList = SetStatusFilter(ticketStatuses);
            var severityList = SetSeverityFilter(severity);

            var ticketQuery = await GetTicketsQueryAsync(catList.Select(x => x.Id), statusList, severityList);
            var ticketList = await ticketQuery
                .Where(x => ((x.DateSubmitted >= startTime) && (x.DateSubmitted <= endTime))
                    || (x.Status.Equals(Ticket.TicketStatus.Closed)) && (x.LastUpdated >= startTime) && (x.LastUpdated <= endTime))
                .ToListAsync();

            return ticketList;
        }

        /// <summary>
        /// Generates a bar chart displaying the status of the tickets matching the specified filter criteria
        /// </summary>
        /// <param name="categories"></param>
        /// <param name="ticketStatuses"></param>
        /// <param name="severity"></param>
        /// <param name="displayLegend"></param>
        /// <returns></returns>
        protected async Task GenerateTicketStatusChartAsync(string categories, string ticketStatuses, string severity, bool displayLegend)
        {
            var ticketList = await GetTicketsAsync(categories, ticketStatuses, severity);

            var title = (await GetSysParam(1003)).Value;
            var scaleLabel = (await GetSysParam(1004)).Value;
            var data = new List<BarChartValue>();

            var ticketCategories = ticketList
                .Select(x => x.Category.Id)
                .Distinct()
                .ToList();
            foreach (var categoryId in ticketCategories)
            {
                var count = ticketList
                    .Where(x => x.Category.Id.Equals(categoryId))
                    .Count();
                var category = await _context.Categories
                  .Where(x => x.Id.Equals(categoryId))
                  .FirstOrDefaultAsync();
                data.Add(new BarChartValue() { Group = scaleLabel, Label = category.DisplayName, Value = count });
            }

            var chart = ChartFactory.GetBarChart(data, title, scaleLabel, ChartFactory.SortType.ByValueDescending, displayLegend);
            ViewBag.StatusChart = chart;
        }

        /// <summary>
        /// Generates a line chart reflecting the activity of the tickets based on the filtering criteria requested
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="categories"></param>
        /// <param name="severity"></param>
        /// <param name="displayLegend"></param>
        /// <returns></returns>
        protected async Task GenerateActivityChartAsync(DateTime? startTime, DateTime? endTime, string categories, string severity, bool displayLegend)
        {
            var ticketList = await GetActivityListAsync(startTime, endTime, categories, null, severity);

            var dataSeries = new List<LineChartValue>();
            var ticketCategories = ticketList.Select(x => x.Category.DisplayName).Distinct().ToList();
            foreach (var category in ticketCategories)
            {
                var tickets = ticketList
                    .Where(x => x.Category.DisplayName.Equals(category)
                        && (x.LastUpdated >= startTime)
                        && (x.LastUpdated <= endTime))
                    .OrderBy(y => y.LastUpdated.Date)
                    .ToList();

                var dates = tickets.Select(x => x.LastUpdated.Date).Distinct().ToList();
                var statuses = tickets.Select(x => x.Status).Distinct().ToList();
                foreach (var date in dates)
                {
                    foreach (var status in statuses)
                    {
                        var count = tickets.Where(x => x.LastUpdated.Date.Equals(date)).Count();
                        dataSeries.Add(new LineChartValue() { Label = Utilities.SystemHelpers.EnumHelper<Ticket.TicketStatus>.GetDisplayName(status), XValue = date.ToString("MM-dd-yyyy"), YValue = count });
                    }
                }
            }

            ViewBag.StartDate = startTime.Value.Date.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endTime.Value.Date.ToString("yyyy-MM-dd"); ;

            var title = (await GetSysParam(1005)).Value;
            var scaleLabel = (await GetSysParam(1006)).Value;
            var chart = ChartFactory.GetLineChart(dataSeries, title, scaleLabel, displayLegend);
            ViewBag.ActivityChart = chart;
        }

        /// <summary>
        /// Generates a pie chart describing the tickets which were created matching the filter criteria provided
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="categories"></param>
        /// <param name="severity"></param>
        /// <param name="displayLegend"></param>
        /// <returns></returns>
        protected async Task GenerateRequestTypeChartAsync(DateTime? startTime, DateTime? endTime, string categories, string severity, bool displayLegend)
        {
            var ticketList = await GetActivityListAsync(startTime, endTime, categories, null, severity);

            var dataSeries = new Dictionary<string, double?>();
            var ticketCategories = ticketList.Select(x => x.Category.DisplayName).Distinct().ToList();
            foreach (var category in ticketCategories)
            {
                var tickets = ticketList
                    .Where(x => x.Category.DisplayName.Equals(category)
                        && (x.DateSubmitted >= startTime)
                        && (x.DateSubmitted <= endTime))
                    .ToList();

                var count = tickets.Count();
                if (count > 0)
                { dataSeries.Add(category, count); }
            }

            ViewBag.StartDate = startTime.Value.Date.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endTime.Value.Date.ToString("yyyy-MM-dd"); ;

            var title = (await GetSysParam(1007)).Value;
            var chart = ChartFactory.GetPieChart(dataSeries, title,  ChartFactory.SortType.ByValueDescending, displayLegend);
            ViewBag.RequestChart = chart;
        }

        #endregion
    }
}
