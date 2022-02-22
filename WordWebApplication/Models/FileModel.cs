namespace WordWebApplication.Models
{
    public class FileModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public byte[] Blob { get; set; }
    }
}