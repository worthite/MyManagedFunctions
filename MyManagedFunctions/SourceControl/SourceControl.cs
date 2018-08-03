
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net.Http;
using System;

namespace MyManagedFunctions.SourceControl
{
    public static class SourceControl
    {
        [FunctionName("SourceControl")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequest req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            string azureResource = "https://management.azure.com/";
            string version = "2017-09-01";

            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            string accessToken = azureServiceTokenProvider.GetAccessTokenAsync(azureResource).Result;


            string TenantID = req.Query["TenantID"];
            string SubId = req.Query["Sub"];
            string resourcegroup = req.Query["resourcegroup"];
            string AppServiceName = req.Query["AppServiceName"];
            string Repo = req.Query["Repo"];
            string Branch = req.Query["Branch"];

            string Uri = string.Format("https://managementappnamedev-management.azurewebsites.net/api/ManagedSCM?TenantID={0}&Sub={1}&resourcegroup={2}&AppServiceName={3}&Repo={4}&Branch={5}", TenantID, SubId, resourcegroup, AppServiceName, Repo, Branch);

            var result = InitializeSourceControl(Uri, accessToken);

            return Uri != null
                ? (ActionResult)new OkObjectResult($"Hello, {AppServiceName}")
                : new BadRequestObjectResult("Invalid Request");
        }
               

        public static async Task<string> InitializeSourceControl(string thisURI, string thisAccessToken)
        {

            string uri = "";

            try
            {

                StringContent stringcontent = new StringContent(string.Empty);

                using (var client = new HttpClient())
                {
                    if (!string.IsNullOrWhiteSpace(thisAccessToken))
                    {

                        client.DefaultRequestHeaders.Clear();
                        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + thisAccessToken);
                    }
                    HttpResponseMessage response = await client.PostAsync(thisURI, stringcontent);
                    if (response.IsSuccessStatusCode)
                    {
                        uri = await response.Content.ReadAsStringAsync();
                    }
                }



            }
            catch (Exception ex)
            {

            }

            return uri;
        }
    }
}
