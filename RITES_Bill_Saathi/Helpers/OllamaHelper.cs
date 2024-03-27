using System;
using Newtonsoft.Json;
using RestSharp;
using RITES_Bill_Saathi.Models;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using SixLabors.ImageSharp.Formats;
using System.Net.Http.Json;

namespace RITES_Bill_Saathi.Helpers
{
    public static class OllamaHelper
    {
    
        public static string systemPrompt = @"";

 
        public static async Task<string> fnDescribeBill_Llava(string imagePath)
        {
            try
            {
                string imgBase64 = await ImageConverter.ConvertImageToBase64Async(imagePath);

                string completePrompt = Prompts.PromptDocumentationIdentificationOnly;

                string url = @"http://localhost:11434";

                // Create a RestClient pointing to the base URL
                var client = new RestClient(url);

                // Create a RestRequest for the specific endpoint and set it to use POST method
                var request = new RestRequest("/api/generate", Method.Post);
                request.Method = Method.Post;

                var body = new
                {
                    model = "llava",
                    prompt = completePrompt,
                    images = new string[] { imgBase64 }
                };

                request.AddJsonBody(body);

                RestResponse response = await client.ExecuteAsync(request);

                string jsonResponse = JsonConvert.SerializeObject(response);

                string responseContent = response.Content;

                // Assuming each JSON object is separated by a newline for this example
                string[] jsonObjects = responseContent.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                // Getting all but the last JSON object
                List<string> allButLast = new List<string>(jsonObjects[..^1]); // ^1 is an index from the end operator, omitting the last item

                // Getting the last JSON object
                string lastJsonObject = jsonObjects[^1]; // ^1 refers to the last item

                string result = OllamaLlavaJsonProcessor.ConcatenateAllResponses(allButLast);

                return result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


     }
    }
