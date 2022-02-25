namespace WordWebApplication.Models
{
    public partial class DocumentEditorController
    {
        public class SaveParameter
        {
            public string Content { get; set; }
            public string FileName { get; set; }
        }
    }
}