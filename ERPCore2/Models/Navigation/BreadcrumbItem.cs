namespace ERPCore2.Models.Navigation
{
    /// <summary>
    /// 麵包屑導航項目模型
    /// </summary>
    public class BreadcrumbItem
    {
        public string Text { get; set; } = string.Empty;
        public string? Href { get; set; }
        public bool IsActive => string.IsNullOrEmpty(Href);
        
        public BreadcrumbItem() { }
        
        public BreadcrumbItem(string text, string? href = null)
        {
            Text = text;
            Href = href;
        }
    }
}
