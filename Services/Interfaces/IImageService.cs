namespace ContactPro_Crucible.Services.Interfaces
{

    //eveything in interface is public by default, class is private by default
    public interface IImageService //interface describes what a class can do; when a class implements this interface, it will get to do whatever is in the interface
     
    {
        public Task<byte[]> ConvertFileToByteArrayAsync(IFormFile? file);

        public string? ConvertByteArrayToFile(byte[]? fileData, string? extension);
    }
}
