namespace Services.Model
{
    public class RawFile
    {
        public string FileName { get; set; }
        public string Content { get; set; }
        public UploadFile UploadFile { get; set; }
        public string BlobPath { get; set; }
        public string ExistingBlobPath { get; set; }
    }
}
