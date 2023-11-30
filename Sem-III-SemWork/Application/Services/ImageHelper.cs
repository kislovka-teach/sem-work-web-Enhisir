using SixLabors.ImageSharp.Formats.Jpeg;

namespace Application.Services;

public class ImageHelper
{
    public async Task<bool> SaveImageAsync(Guid id, string base64Image)
    {
        try
        {
            var begin = base64Image.IndexOf(',') + 1;
            base64Image = base64Image.Substring(begin); // escaping metadata
            var buffer = Convert.FromBase64String(base64Image);
            const double thumbnailWidth = 250.00;
        
            var imagePath = $"Cache/{id}.jpg";
            var thumbnailPath = $"Cache/{id}_thumb.jpg";
            
            using var stream = new MemoryStream(buffer);
            using var image = await Image.LoadAsync(stream);
            await image.SaveAsync(imagePath, new JpegEncoder());
            
            var scaler = thumbnailWidth / image.Width;
            var thumbnailHeight = (int)Math.Ceiling(image.Height * scaler);
            using var thumb = image.Clone(x => x.Resize((int)thumbnailWidth, thumbnailHeight));
            
            await thumb.SaveAsync(thumbnailPath, new JpegEncoder());
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}