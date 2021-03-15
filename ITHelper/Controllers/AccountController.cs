using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.DirectoryServices.AccountManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ITHelper.Models;
using System.Security.Cryptography;
using ITHelper.Helpers;
using System.Net.Mail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace ITHelper.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private static readonly char[] Punctuations = "!@#$%^&*()_-+=[{]};:>|./?".ToCharArray();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IConfiguration _configuration;

        public AccountController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult Index()
        { return View(); }


        [HttpGet]
        public IActionResult ReviewAccount()
        { return View(); }

        [HttpPost]
        [ActionName("ReviewAccount")]
        public IActionResult ReviewAccountResults([Bind("UserName")] UserNameViewModel model)
        {
            UserPrincipal user = null;
            try { user = GetUser(model.UserName); } catch (Exception e) { return RedirectToAction("Error"); }
            return View("ReviewAccountResults", user);
        }

        [HttpGet]
        public IActionResult ChangePassword()
        { return View(new ChangePasswordViewModel()); }

        [HttpPost]
        public async Task<IActionResult> ChangePassword([Bind("UserName,Password,NewPassword,ConfirmPassword")] ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                UserPrincipal user = null;
                try { user = GetUser(model.UserName); } catch (Exception e) { return RedirectToAction("Error"); }
                try { ValidateUserCreds(model.UserName, model.Password); } catch (Exception e) { return RedirectToAction("Error"); }
                user.ChangePassword(model.Password, model.NewPassword);
                user.Save();

                // Notify the user
                var message = new MailMessage();
                message.To.Add(user.EmailAddress);
                message.From = new MailAddress(_configuration["MailSettings:EMailAddress"]);
                message.Subject = "Your B313 Password Has Been Reset";
                message.Body = await this.RenderViewAsync("PasswordChangeSuccessfulEMail", model.UserName, true);
                message.IsBodyHtml = true;

                //var mailClient = MessageHelper.GetSmtpClient(_configuration);
                //mailClient.SendAsync(message, DateTimeOffset.Now.Second);

                log.Info(string.Format("Password successfully changed for: {0}", model.UserName));

                return View("PasswordChangeSuccessful");
            }

            return View(model);
        }



        [HttpGet]
        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword([Bind("UserName")] UserNameViewModel model)
        {
            UserPrincipal user = null;
            try { user = GetUser(model.UserName); } catch (Exception e) { return RedirectToAction("Error"); }
            if (IsValidEmail(user.EmailAddress))
            {
                // Set the user's password and make so they have to reset it at next logon
                var password = GeneratePassword(8, 1);
                user.SetPassword(password);
                user.ExpirePasswordNow();

                // Notify the user
                var message = new MailMessage();
                message.To.Add(user.EmailAddress);
                message.From = new MailAddress(_configuration["MailSettings:EMailAddress"]);
                message.Subject = "Your Password Has Been Reset";
                message.Body = await this.RenderViewAsync("PasswordResetSuccessfulEmail", password, true);
                message.IsBodyHtml = true;

                //var mailClient = MessageHelper.GetSmtpClient(_configuration);
                //mailClient.SendAsync(message, DateTimeOffset.Now.Second);

                log.Info(string.Format("Password successfully reset for: {0}", model.UserName));

                return View("PasswordResetSuccessful", user.EmailAddress);
            }
            else
            {
                var errorMessage = "Password reset error!";
                errorMessage = string.Format("Unable to reset password for user: {0}.  No email recorded for account.", model.UserName);
                log.Warn(errorMessage);

                // Notify the user
                var message = new MailMessage();
                message.To.Add(new MailAddress(_configuration["MailSettings:EMailAddress"]));
                message.From = new MailAddress(_configuration["MailSettings:EMailAddress"]);
                message.Subject = "Failed B313 Password Reset Attempt";
                message.Body = await this.RenderViewAsync("PasswordResetFailedEMail", errorMessage, true);
                message.IsBodyHtml = true;

                //var mailClient = MessageHelper.GetSmtpClient(_configuration);
                //mailClient.SendAsync(message, DateTimeOffset.Now.Second);

                return View("PasswordResetFailed");
            }

        }


        [HttpGet]
        public IActionResult UpdateAccount()
        {
            return View(new UserCredentialsViewModel());
        }

        [HttpPost]
        public IActionResult UpdateAccount([Bind("UserName,Password")] UserCredentialsViewModel model)
        {
            UserPrincipal user = null;
            try { user = GetUser(model.UserName); } catch (Exception e) { return RedirectToAction("Error"); }
            try { ValidateUserCreds(model.UserName, model.Password); } catch (Exception e) { return RedirectToAction("Error"); }

            var viewModel = new UserUpdateViewModel()
            {
                SamAccountName = user.SamAccountName,
                GivenName = user.GivenName,
                MiddleName = user.MiddleName,
                Surname = user.Surname,
                EMailAddress = user.EmailAddress,
                IsAccountLockedOut = user.IsAccountLockedOut(),
                Groups = user.GetAuthorizationGroups().Select(x => x.Name).ToList()
            };

            return View("UpdateAccountResults", viewModel);
        }

        [HttpPost]
        public IActionResult UpdateAccountResults([Bind("SamAccountName,GivenName,MiddleName,Surname,EMailAddress")] UserUpdateViewModel model)
        {
            UserPrincipal user = null;
            try { user = GetUser(model.SamAccountName); } catch (Exception e) { return RedirectToAction("Error"); }

            if (ModelState.IsValid)
            {
                user.GivenName = model.GivenName;
                user.MiddleName = model.MiddleName;
                user.Surname = model.Surname;
                user.EmailAddress = model.EMailAddress;
                user.Save();

                return View("UserUpdateSuccessful", model.SamAccountName);
            }

            model.Groups = user.GetAuthorizationGroups().Select(x => x.Name).ToList();
            return View("UserEditResults", model);
        }



        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        #region Internal Methods

        protected UserPrincipal GetUser(string userName)
        {
            if (userName.Contains("\\"))
            {
                var locationOfSlash = userName.IndexOf("\\") + 1;
                userName = userName.Substring(locationOfSlash);
            }

            var principalContext = new PrincipalContext(ContextType.Domain);
            var user = UserPrincipal.FindByIdentity(principalContext, userName);

            if (user == null)
            {
                var error = $"Username \"{userName}\" does not exist in the system.  Please check and try again.";
                log.Error(error);
                throw new ArgumentException(error);
            }

            return user;
        }

        protected bool ValidateUserCreds(string userName, string password)
        {
            var principalContext = new PrincipalContext(ContextType.Domain);
            if (!principalContext.ValidateCredentials(userName, password))
            {
                var error = $"Invalid user credentials for {userName}.";
                log.Error(error);
                throw new AccessViolationException(error);
            }

            return true;
        }

        protected bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// This method generates a grandom password of the specified length using upper, lower, alpha-number characters,
        /// include the specified number of special characters,
        /// </summary>
        /// <param name="length"></param>
        /// <param name="numberOfNonAlphanumericCharacters"></param>
        /// <returns></returns>
        protected static string GeneratePassword(int length, int numberOfNonAlphanumericCharacters)
        {
            if (length < 1 || length > 128)
            {
                throw new ArgumentException(nameof(length));
            }

            if (numberOfNonAlphanumericCharacters > length || numberOfNonAlphanumericCharacters < 0)
            {
                throw new ArgumentException(nameof(numberOfNonAlphanumericCharacters));
            }

            using (var rng = RandomNumberGenerator.Create())
            {
                var byteBuffer = new byte[length];

                rng.GetBytes(byteBuffer);

                var count = 0;
                var characterBuffer = new char[length];

                for (var iter = 0; iter < length; iter++)
                {
                    var i = byteBuffer[iter] % 87;

                    if (i < 10)
                    {
                        characterBuffer[iter] = (char)('0' + i);
                    }
                    else if (i < 36)
                    {
                        characterBuffer[iter] = (char)('A' + i - 10);
                    }
                    else if (i < 62)
                    {
                        characterBuffer[iter] = (char)('a' + i - 36);
                    }
                    else
                    {
                        characterBuffer[iter] = Punctuations[i - 62];
                        count++;
                    }
                }

                if (count >= numberOfNonAlphanumericCharacters)
                {
                    return new string(characterBuffer);
                }

                int j;
                var rand = new Random();

                for (j = 0; j < numberOfNonAlphanumericCharacters - count; j++)
                {
                    int k;
                    do
                    {
                        k = rand.Next(0, length);
                    }
                    while (!char.IsLetterOrDigit(characterBuffer[k]));

                    characterBuffer[k] = Punctuations[rand.Next(0, Punctuations.Length)];
                }

                return new string(characterBuffer);
            }
        }

        #endregion
    }
}