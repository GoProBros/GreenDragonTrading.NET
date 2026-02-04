using System.Text.Json;

namespace GreenDragonTrading.DataImporter.Models;

public class ImportProgress
{
    public string JobId { get; set; } = Guid.NewGuid().ToString();
    public DateTime StartTime { get; set; } = DateTime.Now;
    public DateTime? LastUpdateTime { get; set; }
    public DateTime? CompletedTime { get; set; }
    public string Status { get; set; } = "InProgress"; // InProgress, Completed, Cancelled
    
    public int TotalSymbols { get; set; }
    public int ProcessedSymbols { get; set; }
    public int SuccessSymbols { get; set; }
    public int FailedSymbols { get; set; }
    public int NoDataSymbols { get; set; }        // Số mã xử lý xong nhưng không có data mới
    
    public List<string> CompletedTickers { get; set; } = new();
    public List<string> FailedTickers { get; set; } = new();
    public List<string> NoDataTickers { get; set; } = new();    // Danh sách mã không có data
    public List<string> RemainingTickers { get; set; } = new();
    
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string Timeframe { get; set; } = "D1"; // D1 or M1
    
    private static readonly string ProgressDirectory = "Progress";
    private static readonly string CurrentProgressFile = Path.Combine(ProgressDirectory, "current_import.json");
    
    public static ImportProgress? LoadCurrent()
    {
        try
        {
            if (!File.Exists(CurrentProgressFile))
                return null;
                
            var json = File.ReadAllText(CurrentProgressFile);
            return JsonSerializer.Deserialize<ImportProgress>(json);
        }
        catch
        {
            return null;
        }
    }
    
    public void Save()
    {
        try
        {
            Directory.CreateDirectory(ProgressDirectory);
            LastUpdateTime = DateTime.Now;
            
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            
            File.WriteAllText(CurrentProgressFile, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Failed to save progress: {ex.Message}");
        }
    }
    
    public void Complete()
    {
        Status = "Completed";
        CompletedTime = DateTime.Now;
        Save();
        
        // Archive completed job
        try
        {
            var archiveFile = Path.Combine(ProgressDirectory, $"completed_{JobId}.json");
            File.Move(CurrentProgressFile, archiveFile, overwrite: true);
        }
        catch { }
    }
    
    public void Cancel()
    {
        // Không đổi Status để có thể resume lại
        // Status vẫn là "InProgress"
        LastUpdateTime = DateTime.Now;
        Save();
    }
    
    public static void ClearCurrent()
    {
        try
        {
            if (File.Exists(CurrentProgressFile))
                File.Delete(CurrentProgressFile);
        }
        catch { }
    }
}
