using System;
namespace RITES_Bill_Saathi.Models
{

        public class ApiResponse
        {
            public List<string> Items { get; set; } = new List<string>();
            public string Title { get; set; }
            public string Type { get; set; }
            // Additional properties based on the schema can be added here
        }

        public class ResponseProcessor
        {
            public static string ExtractResult(ApiResponse response)
            {
                // Concatenate the items into a single string
                return string.Join("", response.Items);
            }
        }
    
}

