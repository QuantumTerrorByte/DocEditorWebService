using System.ComponentModel.DataAnnotations;

namespace WordWebApplication.Models
{
    public class FileModel
    {
        public long Id { get; set; }

        [Required] [MinLength(2)] [MaxLength(50)]
        public string Name { get; set; }

        [Required]
        public byte[] Blob { get; set; }
    }
}