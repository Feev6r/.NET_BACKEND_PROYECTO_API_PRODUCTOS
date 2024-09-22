namespace NET_PRACTICA_MINIPROYECTO_5.Interfaces
{
    public interface ICloudinaryImgRepository
    {
        public string GetImage(); //id parameter
        public string UploadImage(IFormFile file); 

        public void DeleteImage(string publicId);

    }
}
