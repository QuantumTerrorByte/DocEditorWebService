using Microsoft.AspNetCore.Http;

namespace WordWebApplication.Models
{
    public class SaveFileRequestModel
    {
        public string Name { get; set; }
        public IFormCollection Data { get; set; }
    }
}