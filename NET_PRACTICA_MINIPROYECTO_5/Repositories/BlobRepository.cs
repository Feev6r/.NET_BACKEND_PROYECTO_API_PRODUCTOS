using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using NET_PRACTICA_MINIPROYECTO_5.Interfaces;
using NET_PRACTICA_MINIPROYECTO_5.Models;


namespace NET_PRACTICA_MINIPROYECTO_5.Repositories
{
    public class BlobRepository : IBlobRepository
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobContainerClient client;
        public static readonly List<string> ImageExtensions = [".JPG", ".JPEG", ".PNG"];

        public BlobRepository(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
            client = _blobServiceClient.GetBlobContainerClient("productsimages");
        }

        public async void DeleteBlob(string Path)
        {
            try
            {
                var FileName = new Uri(Path).Segments.LastOrDefault();
                var blobCalient = client.GetBlobClient(FileName);
                await blobCalient.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        public async Task<BlobObject> GetBlobFile(string url)
        {

            try
            {
                var fileName = new Uri(url).Segments.LastOrDefault();

                //Si existe el archivo con tal nombre
                BlobClient blobClient = client.GetBlobClient(fileName);
                if (await blobClient.ExistsAsync())
                {
                    //Descargamos y lo convertimos en un Stream de Bytes.
                    BlobDownloadResult content = await blobClient.DownloadContentAsync();
                    Stream dowloadedData = content.Content.ToStream();

                    //Verifica si la extencion es de imagen (para que el cliente sepa que esta obteniendo)
                    if (ImageExtensions.Contains(Path.GetExtension(fileName!.ToUpperInvariant())))
                    {
                        //Devolvemos el blobobject (IMAGEN) con sus bytes, agregamos la extencion sin el punto "."
                        string extension = Path.GetExtension(fileName);
                        return new BlobObject { Content = dowloadedData, ContentType = "image/" + extension.Remove(0, 1) };
                    }
                    else
                    {
                        //Devolvemos (ARCHIVO), su content es el default
                        return new BlobObject { Content = dowloadedData, ContentType = content.Details.ContentType };
                    }
                }
                else
                {
                    throw new Exception("Doent exist the name");
                }
            }
            catch
            {
                throw new Exception("Error at getblobfile");
            }
        }

        public async Task<List<string>> ListBlobs()
        {
            List<string> lst = new List<string>();

            await foreach (var blobitem in client.GetBlobsAsync())
            {
                lst.Add(blobitem.Name);
            }

            return lst;
        }

        public async Task<string> UploadBlobFile(string filePath, string filename)
        {
            try
            {
                var blobClient = client.GetBlobClient(filename);

                var status = await blobClient.UploadAsync(filePath);
                return blobClient.Uri.AbsoluteUri;

            }
            catch (Exception ex)
            {
                throw new Exception("Error: " + ex.Message);
            }


        }
    }
}
