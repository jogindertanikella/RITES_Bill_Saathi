using System;
using Newtonsoft.Json;
using RestSharp;
using RITES_Bill_Saathi.Models;

namespace RITES_Bill_Saathi.Helpers
{
    public static class ReplicateHelper
    {
        public static string actualPrompt1 = @"Given an image of a bill, identify and extract the net amount payable. The field indicating the net amount payable may be labeled with various terms including, but not limited to, 'Grand Total,' 'Total,' 'Sum Total,' and other similar variants. The bill may contain multiple figures related to taxes, discounts, subtotals, etc., but you must specifically identify and return the final amount that needs to be paid. Ensure accuracy in recognizing the correct label amidst potentially similar terms and extract the exact amount specified as the net payable total. Do not do any calculations.";

        public static string actualPrompt2 = @"""
To review the image provided and accurately describe its content, follow these steps, adjusting your conclusion based on the content of the image:

First, determine if the image is a bill. If it is, identify the net amount payable by looking for:

'Grand Total.' If found, note the amount next to it.
If 'Grand Total' is not there, search for 'Total.' Note the amount next to it if found.
If neither 'Grand Total' nor 'Total' are found, look for 'Sum Total' and note the amount next to it.
After identifying the correct amount, assess the type of bill based on key indicators:
Prescription bill for doctor consultation: Look for ""Consultation Fee,"" ""Doctor's Fee,"" etc.
Diagnostic bill for lab tests and procedures: Search for ""Lab Tests,"" ""Diagnostic Services,"" ""X-ray,"" ""MRI,"" etc.
Pharmacy bill for medicines: Look for a list of medicines, ""Pharmacy Services,"" ""Medication,"" etc.
Respond with: 'The total amount of the bill is [amount], and it is a [bill type] bill.' Replace [amount] with the actual figure identified and [bill type] with the type of bill.
If the image is not a bill but a prescription, note the distinctive features such as doctor's instructions, medication names, dosages, and the doctor's signature. Then, respond with: 'This is a prescription.'

If the image is neither a bill nor a prescription, determine if it is related to medical content by looking for any medical terminology, imagery, or context. If it lacks any medical relevance, respond with: 'This is not a medical document.'

By following these guidelines, you will be able to accurately describe the content of the image provided, ensuring clarity whether it's a bill (and its type), a prescription, or a non-medical document.""";

        public static string systemPrompt = @"";

 
        public static async Task<string> fnDescribeBill_Llava(string imagePath, string objectId)
        {
            try
            {
               
                string completePrompt = actualPrompt2 + "*" + objectId;

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