using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        [HttpPost]
        [Route("GetFile")]
        public async Task<string> GetFile(string fileName, string format) //todo ActionResult
        {
            var resultFile = await _dbContext.Files.FirstOrDefaultAsync(file => file.Name.Contains(fileName));
            if (resultFile != null)
            {
                var mStream = new MemoryStream(resultFile.Blob);
                WordDocument document = WordDocument.Load(mStream, GetFormatType(".docx")); //sfdt
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(document);
                document.Dispose();
                return json;
            }
            
            return null;
        }

        [HttpPost]
        [Route("SaveFile")]
        public async Task<IActionResult> SaveFile(SaveFileRequestModel saveFileRequestModel)
        {
            var data = saveFileRequestModel.Data;
            if (data.Files.Count == 0) return null;

            MemoryStream memoryStream = new MemoryStream();
            IFormFile file = data.Files[0];
            await file.CopyToAsync(memoryStream); //file in memory stream
            memoryStream.Position = 0;

            int index = file.FileName.LastIndexOf('.');
            string type = (index > -1 && index < file.FileName.Length - 1) //todo docx only
                ? file.FileName.Substring(index)
                : ".docx";

            try
            {
                _dbContext.Files.Add(new FileModel()
                {
                    Name = string.IsNullOrEmpty(saveFileRequestModel.Name)
                        ? file.Name
                        : saveFileRequestModel.Name, //todo check file name
                    Blob = memoryStream.ToArray(),
                });
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                return BadRequest($"Add file in to DB error(file name:{file.Name})");
            }

            return Ok("Done");
        }

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

        private static FormatType GetFormatType(string format)
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

        [HttpGet]
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
    }
}

/*
[HttpPost]
[Route("Import2")]
public async Task<string> Import2(IFormCollection data)
{
    if (data.Files.Count == 0) return null;

    MemoryStream memoryStream = new MemoryStream();
    IFormFile file = data.Files[0];
    file.CopyTo(memoryStream); //file in memory stream
    memoryStream.Position = 0;

    int index = file.FileName.LastIndexOf('.');
    string type = (index > -1 && index < file.FileName.Length - 1)
        ? file.FileName.Substring(index)
        : ".docx";
    // var path = Path.Combine(
    //     @$"{_environment.ContentRootPath}/wwwroot/files/{DateTime.Now.ToFileTimeUtc()}{type}");
    // using (FileStream fs = new FileStream(path, FileMode.Create)) {
    //     var arr = stream.ToArray();
    //     fs.Write(arr, 0, arr.Length);
    // }

    var result = _dbContext.Files.Add(new FileModel()
    {
        Name = file.Name,
        Blob = memoryStream.ToArray(),
    });
    await _dbContext.SaveChangesAsync();

    var resultFile = await _dbContext.Files.FirstOrDefaultAsync();
    if (resultFile != null)
    {
        var mStream = new MemoryStream(resultFile.Blob);
        WordDocument document = WordDocument.Load(mStream, GetFormatType(type.ToLower())); //sfdt
        string json = Newtonsoft.Json.JsonConvert.SerializeObject(document);
        document.Dispose();
        return json;
    }

    return null;
    // return Ok("Done");
}*/
