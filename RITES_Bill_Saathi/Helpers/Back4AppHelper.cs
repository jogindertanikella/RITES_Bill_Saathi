using System;
using Newtonsoft.Json;
using Parse;
using RestSharp;
using RITES_Bill_Saathi.Models;

namespace RITES_Bill_Saathi.Helpers
{
	public class Back4AppHelper
	{
        public static async Task<bool> SaveEmployeeDetails_NewRecord(EmployeeDetails employeeDetails)
        {
            try
            {

                ParseObject temp_employeeDetails = new ParseObject("employeeDetails");
                temp_employeeDetails["employeeId"] = employeeDetails.employeeId;
                temp_employeeDetails["employeeName"] = employeeDetails.employeeName;

                await temp_employeeDetails.SaveAsync().ConfigureAwait(false);

                return true;
            }
            catch (Exception ex)
            {
                SaveException(ex);
                return false;
            }
        }

        public static async Task<string> SaveSimpleBillDetails_NewRecord(BillDetails billDetails)
        {
            try
            {

                ParseObject temp_billDetails = new ParseObject("billDetails");
                temp_billDetails["employee_objectId"] = "";
                temp_billDetails["employeeId"] = "";
                temp_billDetails["fileName"] = billDetails.fileName;
                temp_billDetails["filePath"] = billDetails.filePath;

                await temp_billDetails.SaveAsync().ConfigureAwait(false);

                string objectId = temp_billDetails.ObjectId;


             //   IncrementEmployeeBillsCount(billDetails.employee_objectId);

                return objectId;
            }
            catch (Exception ex)
            {
                SaveException(ex);
                return "notSaved";
            }
        }

        public static async Task<string> SaveBillDetails_NewRecord(BillDetails billDetails)
        {
            try
            {

                ParseObject temp_billDetails = new ParseObject("billDetails");
                temp_billDetails["employee_objectId"] = billDetails.employee_objectId;
                temp_billDetails["employeeId"] = billDetails.employeeId;
                temp_billDetails["fileName"] = billDetails.fileName;
                temp_billDetails["filePath"] = billDetails.filePath;

                await temp_billDetails.SaveAsync().ConfigureAwait(false);

                string objectId = temp_billDetails.ObjectId;


                IncrementEmployeeBillsCount(billDetails.employee_objectId);

                return objectId;
            }
            catch (Exception ex)
            {
                SaveException(ex);
                return "notSaved";
            }
        }


        public static async Task<bool> SaveException(Exception ex)
        {
            try
            {

                ParseObject tempwebserviceErrorException = new ParseObject("WebServiceErrorException");
                tempwebserviceErrorException["Message"] = ex.Message;
                tempwebserviceErrorException["StackTrace"] = ex.StackTrace;

                await tempwebserviceErrorException.SaveAsync().ConfigureAwait(false);

                return true;
            }
            catch (Exception ex1)
            {
                return false;
            }
        }

        public static async Task<List<EmployeeDetails>> fnGetAllEmployeeDetails()
        {
            try
            {
                List<EmployeeDetails> listEmployeeDetails = new List<EmployeeDetails>();

                var endpointUrl = Settings.back4app_server_id + "/classes/employeeDetails";

                var client = new RestClient(endpointUrl);

                var request = new RestRequest();

                request.Method = Method.Get;

                request.AddHeader("X-Parse-Application-Id", Settings.back4app_id);
                request.AddHeader("X-Parse-REST-API-Key", Settings.back4app_restapi_id);

                //request.AddQueryParameter("limit", "10");

                var response = client.Execute(request);

                var responseData = response.Content;

                var received_results_EmployeeDetails = JsonConvert.DeserializeObject<Results_EmployeeDetails>(responseData);

                listEmployeeDetails = received_results_EmployeeDetails.results.OrderByDescending(x => x.createdAt).ToList();

                return listEmployeeDetails;

            }
            catch (Exception ex)
            {

                SaveException(ex);

                List<EmployeeDetails> listEmployeeDetails = new List<EmployeeDetails>();

                return listEmployeeDetails;
            }

        }


        public static async Task<List<BillDetails>> fnGetAllBillDetails(string employee_objectId)
        {
            try
            {
                List<BillDetails> listBillDetails = new List<BillDetails>();

                var endpointUrl = Settings.back4app_server_id + "/classes/billDetails"; // Adjust the endpoint if necessary

                var client = new RestClient(endpointUrl);

                var request = new RestRequest();

                request.Method = Method.Get;

                request.AddHeader("X-Parse-Application-Id", Settings.back4app_id);
                request.AddHeader("X-Parse-REST-API-Key", Settings.back4app_restapi_id);

                // Add query parameter to filter bills by employee_objectId
                request.AddQueryParameter("where", JsonConvert.SerializeObject(new { employee_objectId = employee_objectId }));

                var response = await client.ExecuteAsync(request);

                var responseData = response.Content;

                var received_results_BillDetails = JsonConvert.DeserializeObject<Results_BillDetails>(responseData);

                listBillDetails = received_results_BillDetails.results.OrderByDescending(x => x.createdAt).ToList();

                return listBillDetails;
            }
            catch (Exception ex)
            {
                SaveException(ex);

                List<BillDetails> listBillDetails = new List<BillDetails>();

                return listBillDetails;
            }
        }

        public static async Task<string> fnGetBillDescription(string objectId)
        {
            try
            {
                var endpointUrl = Settings.back4app_server_id + "/classes/billDetails/" + objectId; // Adjusted to target a specific object

                var client = new RestClient(endpointUrl);

                var request = new RestRequest();

                request.Method = Method.Get;

                request.AddHeader("X-Parse-Application-Id", Settings.back4app_id);
                request.AddHeader("X-Parse-REST-API-Key", Settings.back4app_restapi_id);

                // No need for a query parameter here as we're directly accessing a specific object by its ID

                var response = await client.ExecuteAsync(request);

                var responseData = response.Content;

                var billDetail = JsonConvert.DeserializeObject<BillDetails>(responseData);

                // Assuming BillDetails has a property named 'billDescription'
                var billDescription = billDetail?.billDescription;

                return billDescription;
            }
            catch (Exception ex)
            {
                SaveException(ex);
                return ""; // Return null or an appropriate default value in case of an error
            }
        }


        public static async Task<BillDetails> fnGetBillDetails(string objectId)
        {
            try
            {
                var endpointUrl = Settings.back4app_server_id + "/classes/billDetails/" + objectId; // Adjusted to target a specific object

                var client = new RestClient(endpointUrl);

                var request = new RestRequest();

                request.Method = Method.Get;

                request.AddHeader("X-Parse-Application-Id", Settings.back4app_id);
                request.AddHeader("X-Parse-REST-API-Key", Settings.back4app_restapi_id);

                // No need for a query parameter here as we're directly accessing a specific object by its ID

                var response = await client.ExecuteAsync(request);

                var responseData = response.Content;

                var billDetail = JsonConvert.DeserializeObject<BillDetails>(responseData);

                return billDetail;
            }
            catch (Exception ex)
            {
                SaveException(ex);
                return null;
            }
        }

        public static async Task<bool> IncrementEmployeeBillsCount(string objectId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(objectId))
                {
                    throw new ArgumentException("objectId must be provided");
                }

                var endpointUrl = $"{Settings.back4app_server_id}/classes/employeeDetails/{objectId}";

                var client = new RestClient(endpointUrl);

                var request = new RestRequest();

                request.Method = Method.Put;

                request.AddHeader("X-Parse-Application-Id", Settings.back4app_id);
                request.AddHeader("X-Parse-REST-API-Key", Settings.back4app_restapi_id);
                request.AddHeader("Content-Type", "application/json");

                var payload = new
                {
                    employeeBills = new { __op = "Increment", amount = 1 }
                };

                request.AddJsonBody(JsonConvert.SerializeObject(payload));

                var response = await client.ExecuteAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return true; // Successfully incremented
                }
                else
                {
                    SaveException(new Exception($"Failed to increment employeeBills for objectId: {objectId}. Status code: {response.StatusCode}"));
                    return false; // Indicate failure
                }
            }
            catch (Exception ex)
            {
                SaveException(ex);
                return false; // Indicate failure
            }
        }


    }
}

