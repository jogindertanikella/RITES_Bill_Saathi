using System;
using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using System.Net;
using System.Text.Json;
using System.Globalization;

namespace RITES_Bill_Saathi.Helpers
{
	public static class InvoiceHelper
    {



        // Assuming you're using this method within an async context
        public static async Task<string> AnalyzeInvoiceAsyncAllDetails(string fileURL, string endpoint, AzureKeyCredential credential)
        {
            DocumentAnalysisClient client = new DocumentAnalysisClient(new Uri(endpoint), credential);

            // Sample document
            Uri invoiceUri = new Uri(fileURL);

            AnalyzeDocumentOperation operation = await client.AnalyzeDocumentFromUriAsync(WaitUntil.Completed, "prebuilt-invoice", invoiceUri);

            AnalyzeResult result = operation.Value;

            // Initialize the result string
            string resultString = "";

            for (int i = 0; i < result.Documents.Count; i++)
            {
                resultString += $"Document {i}:\n";

                AnalyzedDocument document = result.Documents[i];

                if (document.Fields.TryGetValue("VendorName", out DocumentField? vendorNameField))
                {
                    if (vendorNameField.FieldType == DocumentFieldType.String)
                    {
                        string vendorName = vendorNameField.Value.AsString();
                        resultString += $"Vendor Name: '{vendorName}', with confidence {vendorNameField.Confidence}\n";
                    }
                }

                if (document.Fields.TryGetValue("CustomerName", out DocumentField? customerNameField))
                {
                    if (customerNameField.FieldType == DocumentFieldType.String)
                    {
                        string customerName = customerNameField.Value.AsString();
                        resultString += $"Customer Name: '{customerName}', with confidence {customerNameField.Confidence}\n";
                    }
                }

                if (document.Fields.TryGetValue("Items", out DocumentField? itemsField))
                {
                    if (itemsField.FieldType == DocumentFieldType.List)
                    {
                        foreach (DocumentField itemField in itemsField.Value.AsList())
                        {
                            resultString += "Item:\n";

                            if (itemField.FieldType == DocumentFieldType.Dictionary)
                            {
                                IReadOnlyDictionary<string, DocumentField> itemFields = itemField.Value.AsDictionary();

                                if (itemFields.TryGetValue("Description", out DocumentField? itemDescriptionField))
                                {
                                    if (itemDescriptionField.FieldType == DocumentFieldType.String)
                                    {
                                        string itemDescription = itemDescriptionField.Value.AsString();

                                        resultString += $"  Description: '{itemDescription}', with confidence {itemDescriptionField.Confidence}\n";
                                    }
                                }

                                if (itemFields.TryGetValue("Amount", out DocumentField? itemAmountField))
                                {
                                    if (itemAmountField.FieldType == DocumentFieldType.Currency)
                                    {
                                        CurrencyValue itemAmount = itemAmountField.Value.AsCurrency();

                                        resultString += $"  Amount: '{itemAmount.Symbol}{itemAmount.Amount}', with confidence {itemAmountField.Confidence}\n";
                                    }
                                }
                            }
                        }
                    }
                }

                if (document.Fields.TryGetValue("SubTotal", out DocumentField? subTotalField))
                {
                    if (subTotalField.FieldType == DocumentFieldType.Currency)
                    {
                        CurrencyValue subTotal = subTotalField.Value.AsCurrency();
                        resultString += $"Sub Total: '{subTotal.Symbol}{subTotal.Amount}', with confidence {subTotalField.Confidence}\n";
                    }
                }

                if (document.Fields.TryGetValue("TotalTax", out DocumentField? totalTaxField))
                {
                    if (totalTaxField.FieldType == DocumentFieldType.Currency)
                    {
                        CurrencyValue totalTax = totalTaxField.Value.AsCurrency();
                        resultString += $"Total Tax: '{totalTax.Symbol}{totalTax.Amount}', with confidence {totalTaxField.Confidence}\n";
                    }
                }

                if (document.Fields.TryGetValue("InvoiceTotal", out DocumentField? invoiceTotalField))
                {
                    if (invoiceTotalField.FieldType == DocumentFieldType.Currency)
                    {
                        CurrencyValue invoiceTotal = invoiceTotalField.Value.AsCurrency();
                        resultString += $"Invoice Total: '{invoiceTotal.Symbol}{invoiceTotal.Amount}', with confidence {invoiceTotalField.Confidence}\n";
                    }
                }
            }

            // Return the result string
            return resultString;
        }

        public static async Task<string> AnalyzeInvoiceAsync(string fileURL, string endpoint, AzureKeyCredential credential)
        {
            DocumentAnalysisClient client = new DocumentAnalysisClient(new Uri(endpoint), credential);
            Uri invoiceUri = new Uri(fileURL);
            AnalyzeDocumentOperation operation = await client.AnalyzeDocumentFromUriAsync(WaitUntil.Completed, "prebuilt-invoice", invoiceUri);
            AnalyzeResult result = operation.Value;

            // Initialize a dictionary to hold the combined totals and dates, using dynamic keys
            var combinedInfo = new Dictionary<string, List<Dictionary<string, object>>>();

            // Create a TextInfo based on the "en-US" culture
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

            foreach (AnalyzedDocument document in result.Documents)
            {
                double invoiceTotalAmount = 0;
                string vendorName = "";
                string invoiceDate = "";

                if (document.Fields.TryGetValue("VendorName", out DocumentField? vendorNameField) && vendorNameField.FieldType == DocumentFieldType.String)
                {
                    // Remove newline characters and apply title case to vendorName
                    vendorName = textInfo.ToTitleCase(vendorNameField.Value.AsString().Replace("\n", "").Replace("\r", ""));
                }

                if (document.Fields.TryGetValue("InvoiceTotal", out DocumentField? invoiceTotalField) && invoiceTotalField.FieldType == DocumentFieldType.Currency)
                {
                    invoiceTotalAmount = invoiceTotalField.Value.AsCurrency().Amount;
                }

                if (document.Fields.TryGetValue("InvoiceDate", out DocumentField? invoiceDateField) && invoiceDateField.FieldType == DocumentFieldType.Date)
                {
                    invoiceDate = invoiceDateField.Value.AsDate().ToString("yyyy-MM-dd");
                }

                string vendorTypeKey = GetVendorTypeKey(vendorName); // Determine the vendor type based on name

                // Ensure the list for the vendor type exists
                if (!combinedInfo.ContainsKey(vendorTypeKey))
                {
                    combinedInfo[vendorTypeKey] = new List<Dictionary<string, object>>();
                }

                // Find or create vendor entry
                var vendorEntry = combinedInfo[vendorTypeKey].FirstOrDefault(entry => entry.ContainsKey("Name") && (string)entry["Name"] == vendorName);

                if (vendorEntry != null)
                {
                    // Update total if vendor already exists
                    vendorEntry["Total"] = (double)vendorEntry["Total"] + invoiceTotalAmount;
                    // Assuming we want to keep the earliest date seen
                    string existingDate = (string)vendorEntry["Date"];
                    vendorEntry["Date"] = String.Compare(existingDate, invoiceDate) < 0 ? existingDate : invoiceDate;
                }
                else
                {
                    // Add new vendor entry
                    combinedInfo[vendorTypeKey].Add(new Dictionary<string, object>
            {
                { "Name", vendorName },
                { "Total", invoiceTotalAmount },
                { "Date", invoiceDate }
            });
                }
            }

            // Convert the dictionary to JSON and replace \u0027 with apostrophe
            string jsonResult = JsonSerializer.Serialize(combinedInfo, new JsonSerializerOptions { WriteIndented = true }).Replace("\\u0027", "'");

            // Return the modified JSON string
            return jsonResult;
        }

        // Helper method to determine the vendor type based on keywords, also removes newlines from vendorName
        private static string GetVendorTypeKey(string vendorName)
        {
            // Normalize vendorName by removing newlines
            string normalizedVendorName = vendorName.Replace("\n", " ").Replace("\r", " ");

            if (string.IsNullOrEmpty(normalizedVendorName)) return "vendorName";

            var pharmacyKeywords = new[] { "pharmacy", "druggist", "chemist" };
            var hospitalKeywords = new[] { "hospital", "clinic", "medical", "healthcare", "emergency", "surg", "matern", "intensive", "ICU", "pediat", "ambul", "trauma", "urgent", "institute", "health system", "polyclinic", "rehab", "nursing", "diagnostic", "cardio", "onco", "neuro", "ortho", "dental", "ophthal", "derma", "psych", "radiology", "laboratory", "pharma", "care" };
            var labKeywords = new[] { "lab", "laboratory", "diagnostics" };

            // Check if the normalizedVendorName contains any of the keywords for each category
            if (pharmacyKeywords.Any(keyword => normalizedVendorName.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
            {
                return "pharmacyName";
            }
            else if (hospitalKeywords.Any(keyword => normalizedVendorName.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
            {
                return "hospitalName";
            }
            else if (labKeywords.Any(keyword => normalizedVendorName.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
            {
                return "labName";
            }

            return "hospitalName"; // Default category
        }


        // Helper method to determine the vendor type based on keywords, also removes newlines from vendorName
        private static string GetVendorTypeKeyPartialAlso(string vendorName)
        {
            // Normalize vendorName by removing newlines
            string normalizedVendorName = vendorName.Replace("\n", " ").Replace("\r", " ");

            if (string.IsNullOrEmpty(normalizedVendorName)) return "vendorName";

            // Splitting the normalizedVendorName into parts (words)
            var nameParts = normalizedVendorName.Split(new[] { ' ', ',', '.', ';', '-' }, StringSplitOptions.RemoveEmptyEntries);

            var pharmacyKeywords = new[] { "pharmacy", "druggist", "chemist" };
            var hospitalKeywords = new[] { "hospital", "clinic", "medical", "healthcare", "emergency", "surg", "matern", "intensive", "ICU", "pediat", "ambul", "trauma", "urgent", "institute", "health system", "polyclinic", "rehab", "nursing", "diagnostic", "cardio", "onco", "neuro", "ortho", "dental", "ophthal", "derma", "psych", "radiology", "laboratory", "pharma", "care" };
            var labKeywords = new[] { "lab", "laboratory", "diagnostics" };

            // Function to check partial matches
            Func<string[], IEnumerable<string>, bool> containsPartialMatch = (parts, keywords) =>
            {
                return parts.Any(part =>
                    keywords.Any(keyword =>
                        part.StartsWith(keyword, StringComparison.OrdinalIgnoreCase) ||
                        keyword.StartsWith(part, StringComparison.OrdinalIgnoreCase))); // Checks for both directions of partial match
            };

            // Check if the nameParts contain any of the keywords or partial matches for each category
            if (containsPartialMatch(nameParts, pharmacyKeywords))
            {
                return "pharmacyName";
            }
            else if (containsPartialMatch(nameParts, hospitalKeywords))
            {
                return "hospitalName";
            }
            else if (containsPartialMatch(nameParts, labKeywords))
            {
                return "labName";
            }

            return "vendorName"; // Default category
        }

    }
}

