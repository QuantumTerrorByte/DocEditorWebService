namespace WordWebApplication.Models
{
    public class SpellCheckJsonData
    {
        public int LanguageId { get; set; }
        public string TexttoCheck { get; set; }
        public bool CheckSpelling { get; set; }
        public bool CheckSuggestion { get; set; }
        public bool AddWord { get; set; }
    }
}