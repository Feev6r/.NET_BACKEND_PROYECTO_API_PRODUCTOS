using NET_PRACTICA_MINIPROYECTO_5.Models;

namespace NET_PRACTICA_MINIPROYECTO_5.Interfaces
{
    public interface IBlobRepository
    {
        Task<BlobObject> GetBlobFile(string url);
        Task<string> UploadBlobFile(string filePath, string fileName);
        void DeleteBlob(string name);
        Task<List<string>> ListBlobs();

    }
}
