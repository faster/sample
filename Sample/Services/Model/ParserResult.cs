namespace Services.Model;

public class ParserResult
{
    public ParserResultCodes Status { get; set; }
    public string? RevenueCenter { get; set; }
    public string? ReviewMessage { get; set; }
    public IEnumerable<string?> Errors { get; set; }
    public IEnumerable<ParserItem> Items { get; set; }
    public DateTime Date { get; set; }
    public string CheckNumber { get; set; }
}

public class ParserItem
{
    public string Name { get; set; }
    public int Quantity { get; set; }
}
