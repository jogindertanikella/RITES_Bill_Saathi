using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace RITES_Bill_Saathi.Helpers
{
    public static class ImageConverter
    {
        public static async Task<string> ConvertImageToBase64Async(string imagePathOrUrl)
        {
            try
            {
                // Check if the imagePathOrUrl is a valid HTTP/HTTPS URL
                if (Uri.TryCreate(imagePathOrUrl, UriKind.Absolute, out Uri uriResult)
                    && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
                {
                    using (HttpClient httpClient = new HttpClient())
                    {
                        // Download the image data
                        HttpResponseMessage response = await httpClient.GetAsync(uriResult);
                        response.EnsureSuccessStatusCode();
                        using (Stream imageStream = await response.Content.ReadAsStreamAsync())
                        {
                            return await ConvertStreamToBase64Async(imageStream);
                        }
                    }
                }
                else
                    return "Error";
            }
            catch (Exception ex)
            {
                // Handle exceptions
                return $"Error converting image to Base64: {ex.Message}";
            }
        }

        private static async Task<string> ConvertStreamToBase64Async(Stream stream)
        {
            using (Image image = await Image.LoadAsync(stream))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    image.SaveAsJpeg(ms); // Or use SaveAsPng(ms) depending on your requirements
                    byte[] imageBytes = ms.ToArray();
                    return Convert.ToBase64String(imageBytes);
                }
            }
        }
    }
}
