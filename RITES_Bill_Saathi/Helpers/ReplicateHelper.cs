using System;
using Newtonsoft.Json;
using RestSharp;
using RITES_Bill_Saathi.Models;

namespace RITES_Bill_Saathi.Helpers
{
    public static class ReplicateHelper
    {
        public static string actualPrompt1 = @"Given an image of a bill, identify and extract the net amount payable. The field indicating the net amount payable may be labeled with various terms including, but not limited to, 'Grand Total,' 'Total,' 'Sum Total,' and other similar variants. The bill may contain multiple figures related to taxes, discounts, subtotals, etc., but you must specifically identify and return the final amount that needs to be paid. Ensure accuracy in recognizing the correct label amidst potentially similar terms and extract the exact amount specified as the net payable total. Do not do any calculations.";



        public static string systemPrompt = @"";

 
        public static async Task<string> fnDescribeBill_Llava(string imagePath, string objectId)
        {
            try
            {
               
                string completePrompt = Prompts.PromptWithoutDate + "ʠ" + objectId;

                string url = @"https://api.replicate.com/v1/predictions";

                var client = new RestClient(url);
                var request = new RestRequest();
                request.Method = Method.Post;

                // Replace your actual API token here
                var apiToken = Settings.replicateAPiKey;

                request.AddHeader("Authorization", $"Token {apiToken}");
                request.AddHeader("Content-Type", "application/json");
                var body = new
                {
                    version = "41ecfbfb261e6c1adf3ad896c9066ca98346996d7c4045c5bc944a79d430f174",
                    input = new
                    {
                        image = imagePath,
                        top_p = 1,
                        prompt = completePrompt,
                        max_tokens = 1024,
                        temperature = 0.2
                    },
                    webhook = Settings.replicateCallBackURL,
                    webhook_events_filter = new string[] { "completed" }
                };

                request.AddJsonBody(body);

                RestResponse response = await client.ExecuteAsync(request);

                string jsonResponse = JsonConvert.SerializeObject(response);

                string responseContent = response.Content;

                // Deserialize the JSON response
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(responseContent);

                // Extract and print the result
                // var result = responseContent;// ResponseProcessor.ExtractResult(apiResponse);

                string result = JsonConvert.SerializeObject(response);

                return result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

  
    }

}