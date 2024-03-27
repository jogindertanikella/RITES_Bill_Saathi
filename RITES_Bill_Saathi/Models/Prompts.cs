using System;
using static System.Net.Mime.MediaTypeNames;
using System.Buffers.Text;
using System.Diagnostics.Metrics;
using System.Numerics;
using System.Reflection.Metadata;
using System.Security.Principal;
namespace RITES_Bill_Saathi.Models
{
    public static class Prompts
    {
        public static string PromptWithoutDateOnlyBillTypeAndAmount =
@"
Identify the Bill Type:

If the document includes terms like ""Consultation Fee,"" ""Doctor's Fee,"" it's a Prescription Bill.
If it lists ""Lab Tests,"" ""Diagnostic Services,"" ""X-ray,"" ""MRI,"" it's a Diagnostic Bill.
If there's a mention of medicines, ""Pharmacy Services,"" ""Medication,"" it's a Pharmacy Bill.
Find the Bill Amount:

Look for 'Grand Total,' 'Total,' or 'Sum Total' and note the specified amount.
Your output should strictly follow this format:

Bill Type: [Insert either Prescription Bill, Diagnostic Bill, or Pharmacy Bill here]
Bill Amount: [Insert the identified amount here]
";

        public static string PromptWithoutDate =
@"
Analyze the Document for Bill Type and Issuer Name:

Identify the Bill Type:

If the document includes terms like ""Consultation Fee,"" ""Doctor's Fee,"" it's a Prescription Bill.
If it lists ""Lab Tests,"" ""Diagnostic Services,"" ""X-ray,"" ""MRI,"" it's a Diagnostic Bill.
If there's a mention of medicines, ""Pharmacy Services,"" ""Medication,"" it's a Pharmacy Bill.
If the document is not a bill but a prescription by a doctor, then identify it as a Prescription.
Find the Bill Amount:

Look for 'Grand Total,' 'Total,' or 'Sum Total' and note the specified amount.
Identify the Bill Issuer Name:

Mention the Hospital or Clinic Name for Prescription and Diagnostic Bills.
Mention the Pharmacy Name for Pharmacy Bills.

For a Doctor's Prescription:

List all the medicines mentioned on the prescription.
Mention the Hospital or Clinic Name.
Your output should strictly follow this format:

For Bills:

Document Type: [Insert either Prescription Bill, Diagnostic Bill, or Pharmacy Bill here]
Bill Amount: [Insert the identified amount here]
Bill Issuer Name: [Insert the Hospital/Clinic/Pharmacy Name here]

For Doctor's Prescriptions:

Document Type: Prescription
Medicines Listed: [List all the medicines mentioned]
Hospital Name: [Mention the Hospital or Clinic Name]
";


        public static string PromptDocumentationIdentificationOnly =
            @"
Examine the uploaded image carefully.

Identify key features that are characteristic of specific types of documents such as:

Header Information: Look for any institutional logos or names (e.g., hospital, clinic, pharmacy, diagnostic centre).
Body Content: Search for specific sections that indicate the nature of the document (e.g., prescription details, itemized billing, diagnostic results).
Footer Information: Check for signatures, authorization, or institutional contacts.
Based on the identified features, classify the document into one of the following categories:

Doctor's Prescription
Doctor's Bill
Hospital's Bill
Clinic's Bill
Diagnostic Centre Bill
Pharmacy Bill
Output the classification as follows:

Document Type: [Insert the identified category here]
";

        public static string PromptWithoutDateOnlyBillType =
@"
Analyze the Image for Document Type:

Identify the Document Type:

If the document includes terms like Consultation, Receipt, Consultation Fee, Doctor's Fee, it's a Doctor's Bill.
If it lists Lab Tests, Diagnostic Services, X-ray, MRI, it's a Diagnostic Bill.
If the document mentions terms like medicines, Pharmacy, Druggist, Druggist, Chemist, Chemists, Pharmacy Services, Medication, it's a Pharmacy Bill.
A Bill will also include words such as Invoice, Receipt, Total, Sum Total, Grand Total, Net, GST, Taxes, etc. 

If the document is not a bill but a prescription by a doctor, then identify it as a Doctor's Prescription.

Your output should strictly follow this format:

Document Type: [Insert either Prescription or Doctor's Bill, Diagnostic Bill, or Pharmacy Bill here]

";

    }
}

