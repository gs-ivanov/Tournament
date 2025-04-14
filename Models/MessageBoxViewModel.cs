namespace Tournament.Models
{
    public class MessageBoxViewModel
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; } // "info", "success", "error", "warning"
    }
}
