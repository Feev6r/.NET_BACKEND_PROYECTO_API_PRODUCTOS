using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using NET_PRACTICA_MINIPROYECTO_5.Interfaces;

namespace NET_PRACTICA_MINIPROYECTO_5.Repositories
{
    public class ClaudinaryImgRepository : ICloudinaryImgRepository
    {

       private readonly Cloudinary _cloudinary;

        public ClaudinaryImgRepository(Cloudinary cloudinary) {
           _cloudinary = cloudinary;
        }



        public string GetImage()
        {
            string url = _cloudinary.Api.UrlImgUp.BuildImageTag("cld-sample-4");
            return url;
        }

        //hacer un cambio, se entrega un stream, osea istreamfile -> stream, toca ver
        public string UploadImage(IFormFile file)
        {
            string UrlImage;
            
            try
            {
                using var fileStream = file.OpenReadStream();


                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(file.FileName, fileStream),
                    /*              UseFilename = true,
                                    UniqueFilename = true,*/
                    //Overwrite = true,
                    AssetFolder = "test"

                };

                var uploadResult = _cloudinary.Upload(uploadParams);


                //var json = uploadResult.JsonObj;
                //json.Root["public_id"]!.ToString();
                UrlImage = uploadResult.FullyQualifiedPublicId.ToString();
                


            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");

            }

            return UrlImage;
           

        }
         
        public void DeleteImage(string publicId)
        {

            try
            {
                var deletionParams = new DeletionParams(publicId);
                DeletionResult deletionResult = _cloudinary.Destroy(deletionParams);

                if (deletionResult.Result == "ok") return;
                else throw new Exception("Error while trying to delete the image");
            }
            catch (Exception ex) 
            { 
                throw new Exception(ex.Message);
            }

        }
    }
}
