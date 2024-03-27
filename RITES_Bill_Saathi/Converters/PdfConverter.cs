using System;
using ImageMagick;

namespace RITES_Bill_Saathi.Converters
{
    public static class PdfConverter
    {
        public static async Task<List<string>> ConvertPdfToImages(Stream pdfStream, string pdfFileName, string workingDirectory)
        {
            try
            {
                // Initialize a list to store the paths of the generated images
                List<string> imagePaths = new List<string>();

            // Extract the base file name without the extension to use as the directory name
            string baseFileName = Path.GetFileNameWithoutExtension(pdfFileName);

            // Create a directory for the PDF within the working directory, using the base file name
            string outputDirectory = Path.Combine(workingDirectory, baseFileName);
            Directory.CreateDirectory(outputDirectory);

            // Create settings to specify density. Higher density gives better quality.
            MagickReadSettings settings = new MagickReadSettings();
            settings.Density = new Density(300, 300);

            // Read the PDF from the stream
            using (MagickImageCollection images = new MagickImageCollection())
            {
                images.Read(pdfStream, settings);

                int page = 1;
                foreach (MagickImage image in images)
                {
                    // Define the path for the output image within the newly created directory
                    string outputPath = Path.Combine(outputDirectory, $"page_{page}.jpg");

                    // Save each page as a JPG
                    image.Write(outputPath);

                    // Add the image path to the list
                    imagePaths.Add(outputPath);

                    page++;
                }
            }

            // Return the list of image paths
            return imagePaths;
            }
            catch (Exception ex)
            {
                // Handle exceptions
                return new List<string>();
            }
        }
    }

}
