namespace ERPCore2.Models
{
    public class UpdateRecord
    {
        public string Version { get; set; } = string.Empty;
        public DateTime ReleaseDate { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // major, minor, patch
        public List<UpdateItem> Items { get; set; } = new();
    }

    public class UpdateItem
    {
        public string Category { get; set; } = string.Empty; // 新功能, 改進, 修復, 安全
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty; // FontAwesome icon class
    }
}
