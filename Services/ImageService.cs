using ContactPro_Crucible.Services.Interfaces;

namespace ContactPro_Crucible.Services
{
    public class ImageService : IImageService
    {

        private readonly string? defaultImage = "/img/DefaultContactImage.png";

        public string? ConvertByteArrayToFile(byte[]? fileData, string? extension)
        {
            if (fileData == null) //no byte info, return default image 
            { 
                return defaultImage;
            }

            try 
            { 
                string? imageBase64Data = Convert.ToBase64String(fileData);
                imageBase64Data = string.Format($"data:{extension};base64,{imageBase64Data}"); //this makes it an image in the view

                return imageBase64Data;

            }catch (Exception)
            {
                throw;
            }
        }

        public async Task<byte[]> ConvertFileToByteArrayAsync(IFormFile? file)
        {
            try
            {
                using MemoryStream memoryStream = new MemoryStream();
                await file!.CopyToAsync(memoryStream); //copying iformfile into memory stream
                byte[] byteFile = memoryStream.ToArray();
                memoryStream.Close();

                return byteFile;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
