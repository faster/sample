using Services.Model;

namespace Services.Interfaces;

public interface IRepository
{
    Task<object> GetConfiguration(int areaId);
    Task SaveReceipts(List<ParsedFile> receipts);
}
