namespace Services.Model;

public class ParseItem
{
    public Guid Id { get; set; }
    public string ParsedItemName { get; set; }
    public DateTime ParsedItemDate { get; set; }
    public int ParsedItemQuantity { get; set; }
    public Guid ParentReceipt { get; set; }
}
