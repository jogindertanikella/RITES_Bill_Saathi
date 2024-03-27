using System;
using Newtonsoft.Json;

namespace RITES_Bill_Saathi.Helpers
{
      public class PrimaryResponse_OllamaLlava_Json
    {
        public string model { get; set; }
        public DateTime created_at { get; set; }
        public string response { get; set; }
        public bool done { get; set; }
    }

    public class FinalResponse_OllamaLlava_Json
    {
        public string model { get; set; }
        public DateTime created_at { get; set; }
        public string response { get; set; }
        public bool done { get; set; }
        public List<int> context { get; set; }
        public long total_duration { get; set; }
        public long load_duration { get; set; }
        public int prompt_eval_count { get; set; }
        public long prompt_eval_duration { get; set; }
        public int eval_count { get; set; }
        public int eval_duration { get; set; }
    }



    public class OllamaLlavaJsonProcessor
	{
        public static string ConcatenateAllResponses(List<string> jsonStrings)
        {
            string result = string.Empty;

            foreach (string jsonString in jsonStrings)
            {
                // Deserialize each JSON string into PrimaryResponse_OllamaLlava_Json object
                var response = JsonConvert.DeserializeObject<PrimaryResponse_OllamaLlava_Json>(jsonString);

                // Check if deserialization was successful before accessing the response property
                if (response != null)
                {
                    result += response.response;
                }
            }

            return result;
        }
    }
}

