using ERPCore2.Models;
using System.Text.Json;

namespace ERPCore2.Services
{
    public interface IUpdateService
    {
        Task<List<UpdateRecord>> GetUpdatesAsync();
    }

    public class UpdateService : IUpdateService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<UpdateService> _logger;

        public UpdateService(IWebHostEnvironment webHostEnvironment, ILogger<UpdateService> logger)
        {
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        public async Task<List<UpdateRecord>> GetUpdatesAsync()
        {
            try
            {
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "data", "updates.json");
                
                if (!File.Exists(filePath))
                {
                    _logger.LogWarning("Updates file not found at {FilePath}", filePath);
                    return new List<UpdateRecord>();
                }

                var json = await File.ReadAllTextAsync(filePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var updates = JsonSerializer.Deserialize<List<UpdateRecord>>(json, options) ?? new List<UpdateRecord>();
                
                // 按發布日期降序排列（最新的在前）
                return updates.OrderByDescending(u => u.ReleaseDate).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading updates data");
                return new List<UpdateRecord>();
            }
        }
    }
}

