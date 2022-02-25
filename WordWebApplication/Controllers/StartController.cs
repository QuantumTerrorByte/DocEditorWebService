using System;
using System.IO;
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
    public class StartController : Controller
    {
        private readonly ILogger<StartController> _logger;
        private readonly IHostEnvironment _environment;
        private readonly AppDbContext _dbContext;

        public StartController(ILogger<StartController> logger, IHostEnvironment environment, AppDbContext dbContext)
        {
            _logger = logger;
            _environment = environment;
            _dbContext = dbContext;
        }
        
        [EnableCors]
        [Route("")]
        public IActionResult Index()
        {
            return View(_dbContext.DiagramsData);
        }
        
        [HttpPost]
        [Route("ConvertFile")]
        public IActionResult ConvertFile(IFormCollection data)
        {
            if (data?.Files?.Count > 0)
            {
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
                return Ok(json);
            }

            HttpContext.Response.StatusCode = 500;
            return BadRequest("No files in request. Make sure the file is no larger than 25 mb");
        }

        [HttpGet]
        [Route("DownloadFile")]
        public async Task<IActionResult> GetFile(string fileName)
        {
            try
            {
                var resultFile = await _dbContext.Files.FirstAsync(file => file.Name.Contains(fileName));
                var mStream = new MemoryStream(resultFile.Blob);
                WordDocument document = WordDocument.Load(mStream, GetFormatType(".docx")); //sfdt
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(document);
                document.Dispose();
                return Ok(json);
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine(e);
                return BadRequest($"File with name:{fileName} do not exist.");
            }
            catch (Exception e)
            {
                return BadRequest($"Db problems.");
            }
        }

        [HttpPost]
        [Route("SaveFile")]
        public async Task<IActionResult> SaveDbFile(IFormCollection fileForm)
        {
            var data = fileForm;
            if (data.Files.Count > 0 && data.ContainsKey("fileName"))
            {
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
                        Name = string.IsNullOrEmpty("fileName") ? file.Name : data["fileName"],
                        Blob = memoryStream.ToArray(),
                    });
                    await _dbContext.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    return BadRequest($"Add file in to DB error(file name:{file.Name})");
                }

                return Ok("File save successful");
            }

            return BadRequest("Request have no data.");
        }


        private FormatType GetFormatType(string format)
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