using System.Collections.Concurrent;
using System.Text;
using Services.Common;
using Services.Interfaces;
using Services.Model;

namespace Services;

public class ParseService
{
    private IFileStorage _fileStorage;
    private readonly IRepository _repository;

    public ParseService(IFileStorage fileStorage, IRepository repository)
    {
        _fileStorage = fileStorage;
        _repository = repository;
    }


    public async Task ParseFiles(int areaId, string periodId, IEnumerable<RawFile> files)
    {
        var auditDate = DateTime.ParseExact(periodId, "yyyy-MM-dd", null);
        var branchConfig = _repository.GetConfiguration(areaId);

        DateTime now = DateTime.UtcNow;
        List<ParsedFile> receipts = new List<ParsedFile>();
        foreach (var file in files)
        {
            var parser = PointOfSaleFactory.GetParser(branchConfig);
            IEnumerable<ParserResult> parsedReceipts = null;
            try
            {
                parsedReceipts = parser.Parse(file.FileName, file.Content);
            }
            catch(ApplicationException ex)
            {
                var receiptId = Guid.NewGuid();
                var filterDateOfCreation = auditDate;

                receipts.Add(new ParsedFile
                {
                    Id = receiptId,
                    BranchId = areaId,
                    CreatedOn = now,
                    Date = filterDateOfCreation,
                    FileName = file.FileName,
                    ParserResultCode = (int)ParserResultCodes.Failed
                });
                continue;
            }

            if (file.FileName.IsTypeA() && (parsedReceipts.First().Status == ParserResultCodes.Success || parsedReceipts.First().Status == ParserResultCodes.Review))
            {
                if (string.IsNullOrEmpty(parsedReceipts.First().RevenueCenter))
                {
                    file.UploadFile = new UploadFile { BlobPath = file.BlobPath };
                }
            }
            else
            {
                file.UploadFile = new UploadFile { BlobPath = file.BlobPath };
            }
            foreach (var receipt in parsedReceipts)
            {
                int addAreaId = file.UploadFile != null && file.UploadFile.AreaId > 0 ? file.UploadFile.AreaId : areaId;
                if (!file.FileName.IsTypeA() && (receipt.Status == ParserResultCodes.Success || receipt.Status == ParserResultCodes.Review))
                {
                    if (string.IsNullOrEmpty(receipt.RevenueCenter))
                    {
                        receipt.RevenueCenter = "Default";
                    }
                }
                var receiptId = Guid.NewGuid();
                if (receipt.Date == default)
                    receipt.Date = auditDate;

                List<ParseItem> items = new List<ParseItem>();
                foreach (var item in receipt.Items)
                {
                    items.Add(new ParseItem
                    {
                        Id = Guid.NewGuid(),
                        ParsedItemName = item.Name,
                        ParsedItemDate = receipt.Date,
                        ParsedItemQuantity = item.Quantity,
                        ParentReceipt = receiptId
                    });
                }
                string message = null;
                if (receipt.Status == ParserResultCodes.Failed)
                    message = string.Join("\n", receipt.Errors);
                else if (receipt.Status == ParserResultCodes.Review)
                    message = receipt.ReviewMessage;
                receipts.Add(new ParsedFile
                {

                    Id = receiptId,
                    BranchId = addAreaId,
                    CreatedOn = now,
                    ParsedCheckNumber = receipt.CheckNumber,
                    ParsedRevenueCenter = !string.IsNullOrEmpty(receipt.RevenueCenter) ? receipt.RevenueCenter : "Main",
                    Items = items,
                    ParserMessage = message,
                    ParserResultCode = (int)receipt.Status
                });
            }
        }

        Task moveTask = Task.Run(async () => 
        {
            var tasks = files.Select(async file =>
            {
                string path = null;
                if (file.UploadFile != null)
                    path = file.UploadFile.BlobPath;
                else
                    path = file.BlobPath;
                if (file.ExistingBlobPath != null)
                {
                    if (file.ExistingBlobPath != path && file.ExistingBlobPath.IsTypeA())
                    {
                        _fileStorage.Upload(path, file.Content);
                        _fileStorage.Delete(file.ExistingBlobPath);
                    }
                }
                else
                {
                    _fileStorage.Upload(path, file.Content);
                }
            });
            await Task.WhenAll(tasks);
        });

        Task addTask = Task.Run(async () =>
        {
            await _repository.SaveReceipts(receipts);
        });

        await Task.WhenAll(moveTask, addTask);
    }
}
