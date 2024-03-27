using System;
using Aspose.Pdf;
using Aspose.Pdf.Devices;
using ImageMagick;

namespace RITES_Bill_Saathi.Converters
{
    public static class AsposePdfConverter
    {
        public static async Task<List<string>> ConvertPdfToImages(Stream pdfStream, string pdfFileName, string workingDirectory)
        {
            try
            {
                // Initialize Aspose.Pdf license here if you have one
                // var license = new Aspose.Pdf.License();
                // license.SetLicense("Aspose.Total.lic");

                List<string> imagePaths = new List<string>();
                string baseFileName = Path.GetFileNameWithoutExtension(pdfFileName);
                string outputDirectory = Path.Combine(workingDirectory, baseFileName);
                Directory.CreateDirectory(outputDirectory);

                Document pdfDocument = new Document(pdfStream);

                for (int pageCount = 1; pageCount <= pdfDocument.Pages.Count; pageCount++)
                {
                    using (FileStream imageStream = new FileStream(Path.Combine(outputDirectory, $"page_{pageCount}.png"), FileMode.Create))
                    {
                        // Create PNG device with specified attributes
                        // Quality [0-100], 100 is Maximum
                        // Resolution is in dots per inch
                        PngDevice pngDevice = new PngDevice(new Resolution(150));

                        // Convert a particular page and save the image to stream
                        pngDevice.Process(pdfDocument.Pages[pageCount], imageStream);

                        // Close stream
                        imageStream.Close();

                        imagePaths.Add(imageStream.Name);
                    }
                }

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
