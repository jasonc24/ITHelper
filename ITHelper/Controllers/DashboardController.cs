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
        public DashboardController(ITHelperContext context) : base(context) { }

        public IActionResult Index()
        { return View(); }


        [Route("~/Dashboard/OpenIssues/{categories?}/{ticketStatuses?}/{severity?}")]
        public async Task<IActionResult> OpenIssues(string categories = "All", string ticketStatuses = "All", string severity = "All")
        {
            var ticketList = await GetTicketsAsync(categories, ticketStatuses, severity);

            var dataSeries = new Dictionary<string, double?>();
            var ticketCategories = ticketList.Select(x => x.Category.DisplayName).Distinct().ToList();
            foreach (var category in ticketCategories)
            {
                var count = ticketList.Where(x => x.Category.DisplayName.Equals(category)).Count();
                dataSeries.Add(category, (double?)count);
            }

            var openIssuesTitle = "Title"; // await GetSysParam(12);
            var scaleLabel = "Scale Label"; // await GetSysParam(13);
            var chart = ChartFactory.GetBarChart(dataSeries, openIssuesTitle, scaleLabel, ChartFactory.SortType.ByValueDescending);
            ViewBag.Chart = chart;

            return View();
        }

        [Route("~/Dashboard/Activity/{startTime:DateTime?}/{endTime:DateTime?}/{categories?}/{ticketStatuses?}/{severity?}")]
        public async Task<IActionResult> Activity(DateTime? startTime, DateTime? endTime, string categories = "All", string ticketStatuses = "All", string severity = "All")
        {
            var ticketList = await GetActivityListAsync(startTime, endTime, categories, ticketStatuses, severity);

            var dataSeries = new Dictionary<string, List<double?>>();
            var ticketCategories = ticketList.Select(x => x.Category.DisplayName).Distinct().ToList();
            foreach (var category in ticketCategories)
            {
                var tickets = ticketList
                    .Where(x => x.Category.DisplayName.Equals(category)
                        && (x.Status == Ticket.TicketStatus.Closed)
                        && (x.LastUpdated >= startTime)
                        && (x.LastUpdated <= endTime))
                    .ToList();

                var counts = new List<double?>();
                var dates = tickets.Select(x => x.LastUpdated.Date).Distinct().ToList();
                foreach (var date in dates)
                {
                    var count = tickets.Where(x => x.LastUpdated.Date.Equals(date)).Count();
                    counts.Add(count);

                }
                dataSeries.Add(category, counts);
            }

            var openIssuesTitle = "Title"; // await GetSysParam(12);
            var scaleLabel = "Scale Label"; // await GetSysParam(13);
            var chart = ChartFactory.GetLineChart(dataSeries, openIssuesTitle, scaleLabel);

            return View(chart);
        }

        [Route("~/Dashboard/Activity/{startTime:DateTime?}/{endTime:DateTime?}/{categories?}/{ticketStatuses?}/{severity?}")]
        public async Task<IActionResult> RequestTypes(DateTime? startTime, DateTime? endTime, string categories = "All", string severity = "All")
        {
            var ticketList = await GetActivityListAsync(startTime, endTime, categories, "All", severity);

            var dataSeries = new Dictionary<string, List<double?>>();
            var ticketCategories = ticketList.Select(x => x.Category.DisplayName).Distinct().ToList();
            foreach (var category in ticketCategories)
            {
                var tickets = ticketList
                    .Where(x => x.Category.DisplayName.Equals(category)
                        && (x.DateSubmitted >= startTime)
                        && (x.DateSubmitted <= endTime))
                    .ToList();

                var counts = new List<double?>();
                var dates = tickets.Select(x => x.LastUpdated.Date).Distinct().ToList();
                foreach (var date in dates)
                {
                    var count = tickets.Where(x => x.LastUpdated.Date.Equals(date)).Count();
                    counts.Add(count);

                }
                dataSeries.Add(category, counts);
            }

            var openIssuesTitle = "Title"; // await GetSysParam(12);
            var scaleLabel = "Scale Label"; // await GetSysParam(13);
            var chart = ChartFactory.GetLineChart(dataSeries, openIssuesTitle, scaleLabel);

            return View(chart);

        }


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

    }
}
