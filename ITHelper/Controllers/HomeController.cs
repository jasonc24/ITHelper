using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ITHelper.Models;

namespace ITHelper.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            var server = Utilities.SystemHelpers.SystemHelper.GetConfigValue("EMailSettings:SMTPRelay");
            var username = Utilities.SystemHelpers.SystemHelper.GetConfigValue("EMailSettings:Username");
            var password = Utilities.SystemHelpers.SystemHelper.GetConfigValue("EMailSettings:Password");
            var relay = new Utilities.Messaging.MessageHelper(server, username, password);
            relay.SendMessageAsync("Test Message",
                new System.Net.Mail.MailAddress("jchristopher@sharethehope.org", "Jason Christopher"),
                new System.Net.Mail.MailAddress("controlsengineer@gmail.com"),
                null,
                null,
                "Test Message",
                "33333");

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
