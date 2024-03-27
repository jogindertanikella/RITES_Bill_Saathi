using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using RITES_Bill_Saathi.Models;
using Newtonsoft.Json;
using RITES_Bill_Saathi.Helpers;
using Parse;
using System.Net;
using LovePdf.Model.Task;
using LovePdf.Core;
using RestSharp;
using RITES_Bill_Saathi.Converters;
using static RITES_Bill_Saathi.Controllers.HomeController;
using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;

namespace RITES_Bill_Saathi.Controllers;

public class HomeController : Controller
{
    private readonly string endpoint = "https://centralindia.api.cognitive.microsoft.com/";
    private readonly string apiKey = "0817161aa32d40daac59ce64be3fa819";
    private readonly AzureKeyCredential credential;

    public HomeController()
    {
        credential = new AzureKeyCredential(apiKey);
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }


    public class Root_PDFAPI
    {
        public string url { get; set; }
        public DateTime outputLinkValidTill { get; set; }
        public bool error { get; set; }
        public int status { get; set; }
        public string name { get; set; }
        public int credits { get; set; }
        public int remainingCredits { get; set; }
        public int duration { get; set; }
    }

    public class ImageUrlResult
    {
        public string imageUrl { get; set; }
        public string imageName { get; set; }
    }

    public class ConversionResult
    {
        public List<string> Urls { get; set; }
        public string OutputLinkValidTill { get; set; }
        public int PageCount { get; set; }
        public bool Error { get; set; }
        public int Status { get; set; }
        public string Name { get; set; }
        public int Credits { get; set; }
        public int RemainingCredits { get; set; }
        public int Duration { get; set; }
    }

    public async Task<ImageUrlResult> ConvertPdfToJpeg(string fileUrl, int pageNumber)
    {
        string customImageName = UniversalHelper.GenerateFileName("jpg");

        var client = new RestClient("https://api.pdf.co/v1/pdf/convert/to/jpg");

        var request = new RestRequest();
        request.Method = Method.Post;
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("x-api-key", Settings.pdfcoAPI_key);

        // Configure the request to convert only the specified page
        string jsonRequestBody = $"{{\"url\":\"{fileUrl}\", \"pages\":\"{pageNumber - 1}\", \"async\": false, \"name\": \"{customImageName}\"}}";

        request.AddParameter("application/json", jsonRequestBody, ParameterType.RequestBody);

        RestResponse response = await client.ExecuteAsync(request);

        string result = response.Content;

        ConversionResult conversionResult = JsonConvert.DeserializeObject<ConversionResult>(result);

        string imageUrlDefault = "https://accelracer.com/medicalbills/default.jpg";

        string firstUrl = imageUrlDefault;

        if (conversionResult.Urls != null && conversionResult.Urls.Count > 0)
        {
            firstUrl = conversionResult.Urls[0];
        }


        ImageUrlResult imageUrlResult = new ImageUrlResult();


        imageUrlResult.imageUrl = firstUrl;
        imageUrlResult.imageName = customImageName;

        return imageUrlResult;
    }

    public async Task<ImageUrlsResult> ConvertPdfToJpegAllPages(string fileUrl)
    {
        string customImageName = UniversalHelper.GenerateFileName("jpg");

        var client = new RestClient("https://api.pdf.co/v1/pdf/convert/to/jpg");

        var request = new RestRequest();
        request.Method = Method.Post;
        request.AddHeader("Content-Type", "application/json");
        request.AddHeader("x-api-key", Settings.pdfcoAPI_key);

        // Configure the request to convert the entire document
        string jsonRequestBody = $"{{\"url\":\"{fileUrl}\", \"async\": false, \"name\": \"{customImageName}\"}}";

        request.AddParameter("application/json", jsonRequestBody, ParameterType.RequestBody);

        RestResponse response = await client.ExecuteAsync(request);

        string result = response.Content;

        // Assuming ConversionResult can handle multiple URLs
        ConversionResult conversionResult = JsonConvert.DeserializeObject<ConversionResult>(result);

        ImageUrlsResult imageUrlsResult = new ImageUrlsResult();

        // Use default image URL in case conversion fails or returns no URLs
        string imageUrlDefault = "https://accelracer.com/medicalbills/default.jpg";
        List<string> urls = conversionResult.Urls != null && conversionResult.Urls.Count > 0 ? conversionResult.Urls : new List<string> { imageUrlDefault };

        imageUrlsResult.imageUrls = urls;
        imageUrlsResult.imageName = customImageName;

        return imageUrlsResult;
    }

    // Supporting class to hold the result
    public class ImageUrlsResult
    {
        public List<string> imageUrls { get; set; }
        public string imageName { get; set; }
    }




    public async Task<string> UploadFilePDFAPI(string pdfFilePath)
    {
        var client = new RestClient("https://api.pdf.co/v1/file/upload");
        //client.Timeout = -1;
        var request = new RestRequest();
        request.Method = Method.Post;
        request.AddHeader("x-api-key", Settings.pdfcoAPI_key);
        request.AddFile("file", pdfFilePath);
        RestResponse response = await client.ExecuteAsync(request);

        string result = response.Content;

        Root_PDFAPI root_PDFAPI = JsonConvert.DeserializeObject<Root_PDFAPI>(result);

        string uploadedfileurl = root_PDFAPI.url;


        return uploadedfileurl; // This should be replaced with the actual file URL from the response
    }


    [HttpPost]
    public async Task<IActionResult> UploadPDF(IFormFile pdfFile)
    {
        try
        {
            if (pdfFile != null && pdfFile.Length > 0)
            {

                // Save the uploaded PDF temporarily
                var tempPdfPath = Path.Combine(Path.GetTempPath(), pdfFile.FileName);
                using (var stream = new FileStream(tempPdfPath, FileMode.Create))
                {
                    await pdfFile.CopyToAsync(stream);
                }

                string pdfFilePath = tempPdfPath;

                // Upload the PDF file and get the URL
                string uploadedFileUrl = await UploadFilePDFAPI(pdfFilePath);

                ImageUrlResult imageUrlResult = new ImageUrlResult();

                // Convert the uploaded PDF to JPEG
                if (!string.IsNullOrEmpty(uploadedFileUrl))
                {

                    imageUrlResult = await ConvertPdfToJpeg(uploadedFileUrl, 1); // Assuming you want to convert the first page
                }

                string imageUrl = imageUrlResult.imageUrl;
                string imageName = imageUrlResult.imageName;

                return Json(new { ImageUrl = imageUrl, ImageFileName = imageName });

            }
            // Return a default image URL if no file is uploaded
            string imageUrlDefault = "https://accelracer.com/medicalbills/default.jpg";
            return Json(new { ImageUrl = imageUrlDefault, ImageFileName = "default.jpg" });
        }
        catch (Exception ex)
        {
            // Return a default image URL in case of an exception
            string imageUrlDefault = "https://accelracer.com/medicalbills/default.jpg";
            return Json(new { ImageUrl = imageUrlDefault, ImageFileName = "default.jpg" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> UploadPDFAndConvertAllPages(IFormFile pdfFile)
    {
        try
        {
            if (pdfFile != null && pdfFile.Length > 0)
            {
                // Save the uploaded PDF temporarily
                var tempPdfPath = Path.Combine(Path.GetTempPath(), pdfFile.FileName);
                using (var stream = new FileStream(tempPdfPath, FileMode.Create))
                {
                    await pdfFile.CopyToAsync(stream);
                }

                string pdfFilePath = tempPdfPath;

                // Upload the PDF file and get the URL
                string uploadedFileUrl = await UploadFilePDFAPI(pdfFilePath);

                ImageUrlsResult imageUrlsResult = new ImageUrlsResult();

                // Convert the uploaded PDF to JPEGs for all pages
                if (!string.IsNullOrEmpty(uploadedFileUrl))
                {
                    imageUrlsResult = await ConvertPdfToJpegAllPages(uploadedFileUrl); // This now converts all pages
                }

                // Handling multiple image URLs
                List<string> imageUrls = imageUrlsResult.imageUrls;
                string imageName = imageUrlsResult.imageName;

                return Json(new { ImageUrls = imageUrls, ImageFileName = imageName });
            }
            // Return a default image URL if no file is uploaded
            string imageUrlDefault = "https://accelracer.com/medicalbills/default.jpg";
            return Json(new { ImageUrls = new List<string> { imageUrlDefault }, ImageFileName = "default.jpg" });
        }
        catch (Exception ex)
        {
            // Return a default image URL in case of an exception
            string imageUrlDefault = "https://accelracer.com/medicalbills/default.jpg";
            return Json(new { ImageUrls = new List<string> { imageUrlDefault }, ImageFileName = "default.jpg" });
        }
    }

    public class UploadedImageDetails
    {
        public string imagefilePath { get; set; }
        //public string imagefileName { get; set; }
    }

    [HttpPost]
    public async Task<string> ProcessUploadedImage(UploadedImageDetails file)
    {
        try
        {
            string imagefilePath = file.imagefilePath;
            //string imagefileName = file.imagefileName;

          //  string result = await OllamaHelper.fnDescribeBill_Llava(imagefilePath);

            string result = await InvoiceHelper.AnalyzeInvoiceAsync(imagefilePath, endpoint, credential);

            return result;

        }
        catch (Exception ex)
        {
            Back4AppHelper.SaveException(ex);
            // Updated line to include employeeObjectId as employeeId parameter
            string polling_id = "";

            return polling_id;

        }
    }
}
