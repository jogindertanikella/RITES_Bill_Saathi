using System;
using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using System.Net;

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

            // Sample document
            Uri invoiceUri = new Uri(fileURL);

            AnalyzeDocumentOperation operation = await client.AnalyzeDocumentFromUriAsync(WaitUntil.Completed, "prebuilt-invoice", invoiceUri);

            AnalyzeResult result = operation.Value;

            // Initialize the result string
            string resultString = "";

            foreach (AnalyzedDocument document in result.Documents)
            {
                if (document.Fields.TryGetValue("VendorName", out DocumentField? vendorNameField) && vendorNameField.FieldType == DocumentFieldType.String)
                {
                    string vendorName = vendorNameField.Value.AsString();
                    resultString += $"Vendor Name: '{vendorName}'\n";
                }

                if (document.Fields.TryGetValue("InvoiceTotal", out DocumentField? invoiceTotalField) && invoiceTotalField.FieldType == DocumentFieldType.Currency)
                {
                    CurrencyValue invoiceTotal = invoiceTotalField.Value.AsCurrency();
                    resultString += $"Invoice Total: '{invoiceTotal.Symbol}{invoiceTotal.Amount}'\n";
                }
            }

            // Return the result string with vendor name and invoice total
            return resultString.Trim();
        }


    }
}

