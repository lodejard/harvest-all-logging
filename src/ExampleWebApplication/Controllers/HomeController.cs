using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ExampleWebApplication.Models;
using Microsoft.AspNetCore.DataProtection;

namespace ExampleWebApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IDataProtectionProvider _protectionProvider;

        public HomeController(ILogger<HomeController> logger, IDataProtectionProvider protectionProvider)
        {
            _logger = logger;
            _protectionProvider = protectionProvider;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            try
            {
                System.IO.File.ReadAllText("c:\\autoexec.bat");
                return View();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Unable to show privacy", ex);
            }
        }

        public string ActionThatLogs(string flavor, string color, string text)
        {
            using (_logger.BeginScope("Flavor:{Flavor}", flavor))
            {
                _logger.LogInformation(new EventId(1, "EncodingData"), "Encoding user-provided value '{Text}' with '{Color}'", text, color);
                
                var protector = _protectionProvider.CreateProtector("MyExample", color);
                var ciphertext = protector.Protect(text);
                
                _logger.LogInformation(new EventId(2, "ResultingSize"), "Resulting data is {ProtectedLength} characters long", ciphertext.Length);

                return ciphertext;
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
