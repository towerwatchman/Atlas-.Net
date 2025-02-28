using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using ImageMagick;
class Program
{
    static async Task Main(string[] args)
    {
        // Example URL - replace with your own URL
        string imageUrl = "https://attachments.f95zone.to/2019/02/259073_ch1afterout0a.jpg";

        try
        {
            // Download the image
            byte[] imageBytes = await DownloadImage(imageUrl);

            // Get the original filename from the URL
            string fileName = Path.GetFileName(new Uri(imageUrl).LocalPath);
            string nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            string outFile = nameWithoutExtension + ".webp";

            // Convert to WebP with specified quality
            bool isComplete = await ConvertToWebpAsync(imageBytes, 90, 0, outFile); // Default quality of 75

            // Save the WebP file

        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
    static async Task<byte[]> DownloadImage(string url)
    {
        using (var httpClient = new HttpClient())
        {
            // Download the image bytes
            byte[] imageBytes = await httpClient.GetByteArrayAsync(url);
            return imageBytes;
        }
    }

    static async Task<bool> ConvertToWebpAsync(byte[] imageBytes, uint quality, uint maxDimension, string outFile)
    {
        using (var image = new MagickImage(imageBytes))
        {
            try
            {
                //Check if file type before continueing
                string type = DetectImageFormat(imageBytes);
                Console.Out.WriteLine($"{type} image format detected");
                // Get original dimensions
                int width = (int)image.Width;
                int height = (int)image.Height;

                // Resize if necessary
                if ((width > maxDimension || height > maxDimension) && maxDimension > 0)
                {
                    if (width > height)
                    {
                        // Scale based on width
                        image.Resize(maxDimension, 0); // Height will scale proportionally, 0 maintains aspect ratio
                    }
                    else
                    {
                        // Scale based on height
                        image.Resize(0, maxDimension); // Width will scale proportionally, 0 maintains aspect ratio
                    }
                }

                // Set WebP format and quality
                image.Format = MagickFormat.WebP;
                image.Quality = quality;

                // Optionally configure WebP-specific settings
                image.Settings.SetDefine("webp:lossless", "false"); // Use "true" for lossless
                                                                    // image.Settings.SetDefine("webp:compression-level", "6"); // 0-6, higher = better compression

                // Convert to WebP and return bytes
                byte[] webpBytes = image.ToByteArray();

                await File.WriteAllBytesAsync(outFile, webpBytes);
                Console.WriteLine($"Saved as: {outFile}");
                Console.WriteLine("Image downloaded, converted to WebP, and saved successfully!");
            }
            catch (Exception e)
            {
                return false;
            }
        }
        return true;
    }

    static string DetectImageFormat(byte[] imageBytes)
    {
        // Check if we have enough bytes to analyze (increased to 14 for TGA)
        if (imageBytes.Length < 14)
            return string.Empty;

        // JPEG: FF D8 FF
        if (imageBytes[0] == 0xFF && imageBytes[1] == 0xD8 && imageBytes[2] == 0xFF)
            return "jpg";

        // PNG: 89 50 4E 47
        if (imageBytes[0] == 0x89 && imageBytes[1] == 0x50 &&
            imageBytes[2] == 0x4E && imageBytes[3] == 0x47)
            return "png";

        // GIF: 47 49 46 38
        if (imageBytes[0] == 0x47 && imageBytes[1] == 0x49 &&
            imageBytes[2] == 0x46 && imageBytes[3] == 0x38)
            return "gif";

        // BMP: 42 4D
        if (imageBytes[0] == 0x42 && imageBytes[1] == 0x4D)
            return "bmp";

        // WebP: 52 49 46 46 (RIFF) ... 57 45 42 50 (WEBP)
        if (imageBytes[0] == 0x52 && imageBytes[1] == 0x49 &&
            imageBytes[2] == 0x46 && imageBytes[3] == 0x46 &&
            imageBytes[8] == 0x57 && imageBytes[9] == 0x45 &&
            imageBytes[10] == 0x42 && imageBytes[11] == 0x50)
            return "webp";

        // AVIF: Usually starts with ftypavif in bytes 4-11
        if (imageBytes[4] == 0x66 && imageBytes[5] == 0x74 &&
            imageBytes[6] == 0x79 && imageBytes[7] == 0x70 &&
            imageBytes[8] == 0x61 && imageBytes[9] == 0x76 &&
            imageBytes[10] == 0x69 && imageBytes[11] == 0x66)
            return "avif";

        // PBM (Portable Bitmap, ASCII): 50 31 (P1)
        if (imageBytes[0] == 0x50 && imageBytes[1] == 0x31)
            return "pbm";

        // TIFF: 49 49 2A 00 (little-endian) or 4D 4D 00 2A (big-endian)
        if ((imageBytes[0] == 0x49 && imageBytes[1] == 0x49 && imageBytes[2] == 0x2A && imageBytes[3] == 0x00) ||
            (imageBytes[0] == 0x4D && imageBytes[1] == 0x4D && imageBytes[2] == 0x00 && imageBytes[3] == 0x2A))
            return "tiff";

        // TGA (Truevision TGA): Basic header check
        if (imageBytes[0] <= 0x01 && imageBytes[1] <= 0x0D &&
            (imageBytes[2] == 0x01 || imageBytes[2] == 0x02 || imageBytes[2] == 0x03 ||
             imageBytes[2] == 0x09 || imageBytes[2] == 0x0A || imageBytes[2] == 0x0B))
            return "tga";

        // QOI (Quite OK Image): 71 6F 69 66 (qoif)
        if (imageBytes[0] == 0x71 && imageBytes[1] == 0x6F &&
            imageBytes[2] == 0x69 && imageBytes[3] == 0x66)
            return "qoi";

        // Default to .jpg if unknown
        return "unknown";
    }
}
