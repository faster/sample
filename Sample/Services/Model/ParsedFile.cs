namespace Services.Model;

public class ParsedFile
{
    public Guid Id { get; set; }
    public int BranchId { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime Date { get; set; }
    public string FileName { get; set; }
    public int ParserResultCode { get; set; }
    public string ParsedCheckNumber { get; set; }
    public string ParsedRevenueCenter { get; set; }
    public List<ParseItem> Items { get; set; }
    public string? ParserMessage { get; set; }
}
