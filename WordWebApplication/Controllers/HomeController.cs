using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Syncfusion.EJ2.DocumentEditor;
using WordWebApplication.Models;

namespace WordWebApplication.Controllers
{
    [EnableCors]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHostEnvironment _environment;
        private readonly AppDbContext _dbContext;

        public HomeController(ILogger<HomeController> logger, IHostEnvironment environment, AppDbContext dbContext)
        {
            _logger = logger;
            _environment = environment;
            _dbContext = dbContext;
        }

        [HttpGet]
        [EnableCors]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [EnableCors]
        [Route("Privacy")]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }

        // [AcceptVerbs("Post")]
        // [EnableCors("AllowAllOrigins")]
        [HttpPost]
        [Route("Import")]
        public string Import(IFormCollection data)
        {
            if (data.Files.Count == 0) return null;
            Stream stream = new MemoryStream();
            IFormFile file = data.Files[0];
            int index = file.FileName.LastIndexOf('.');

            string type = (index > -1 && index < file.FileName.Length - 1)
                ? file.FileName.Substring(index)
                : ".docx";
            file.CopyTo(stream);
            stream.Position = 0;

            WordDocument document = WordDocument.Load(stream, GetFormatType(type.ToLower()));
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(document);
            document.Dispose();
            return json;
        }

        [HttpPost]
        [Route("Import2")]
        public IActionResult Import2(IFormCollection data)
        {
            if (data.Files.Count == 0) return null;

            MemoryStream stream = new MemoryStream();
            IFormFile file = data.Files[0];
            file.CopyTo(stream); //file in memory stream
            stream.Position = 0;

            int index = file.FileName.LastIndexOf('.');
            string type = (index > -1 && index < file.FileName.Length - 1)
                ? file.FileName.Substring(index)
                : ".docx";
            var path = Path.Combine(
                @$"{_environment.ContentRootPath}/wwwroot/files/{DateTime.Now.Minute}{type}");

            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                var arr = stream.ToArray();
                fs.Write(arr, 0, arr.Length);
            }

            return Ok("Done");
        }

        internal static FormatType GetFormatType(string format)
        {
            if (string.IsNullOrEmpty(format))
                throw new NotSupportedException("EJ2 DocumentEditor does not support this file format.");
            switch (format.ToLower())
            {
                case ".dotx":
                case ".docx":
                case ".docm":
                case ".dotm":
                    return FormatType.Docx;
                case ".dot":
                case ".doc":
                    return FormatType.Doc;
                case ".rtf":
                    return FormatType.Rtf;
                case ".txt":
                    return FormatType.Txt;
                case ".xml":
                    return FormatType.WordML;
                case ".html":
                    return FormatType.Html;
                default:
                    throw new NotSupportedException("EJ2 DocumentEditor does not support this file format.");
            }
        }
    }
}