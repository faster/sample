namespace Services.Interfaces;

public interface IFileStorage
{
    void Upload(string path, string fileContent);
    void Delete(string fileExistingBlobPath);
}
