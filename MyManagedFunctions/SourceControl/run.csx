using System;
using System.Net;
using System.Collections.Generic;
using Microsoft.Azure;
using Microsoft.Azure.Common;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Azure.Services.AppAuthentication;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Configuration;
using System.Linq;

const string Passed = "Passed";
const string Failed = "Failed";

private static string vaultname = ConfigurationManager.AppSettings["KeyVault"];

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info("C# HTTP trigger function processed a request.");
 
    // parse query parameter
    string TenantID = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, "TenantID", true) == 0)
        .Value;

    // parse query parameter
    string SubId = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, "Sub", true) == 0)
        .Value;

    // parse query parameter
    string resourcegroup = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, "resourcegroup", true) == 0)
        .Value;

    // parse query parameter
    string AppServiceName = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, "AppServiceName", true) == 0)
        .Value;

    // parse query parameter
    string Repo = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, "Repo", true) == 0)
        .Value;

    // parse query parameter
    string Branch = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, "Branch", true) == 0)
        .Value;

    log.Info("Querystring Processed");    

     var myMessage = new MyMessage{        
            TenantId = TenantID,
            SubId = SubId,
            ResourceGroup = resourcegroup,
            AppServiceName = AppServiceName,
            Repo = Repo,
            Branch = Branch
        };
    log.Info("Created Message");

    var myMessageString = JsonConvert.SerializeObject(myMessage);

    log.Info("Message: " + myMessageString);

    log.Info("Retrieving Secret from " + vaultname);

    string StorageConnectionString =await GetSecret(vaultname,"ManagementStorage");
    
    log.Info("Retrieved ConnectionString");
   
    var storageAccount = CloudStorageAccount.Parse(StorageConnectionString);

    // create Queue Client
        var client = storageAccount.CreateCloudQueueClient();
    log.Info("Created Queue Client");

    // get Queue reference
     var queue = client.GetQueueReference("mymanagedapps");
 
    // create Queue if it doesn't yet exist
     queue.CreateIfNotExists();
    
    log.Info("Created Queue");

    // Create new Message
    var message = new CloudQueueMessage(myMessageString);
    log.Info("Created Queue Message");

     // Add / Send Message to the Queue
     queue.AddMessage(message);
    log.Info("Sent Message to Queue");
    
    var template = @"{'$schema': 'https://schema.management.azure.com/schemas/2015-01-01/deploymentTemplate.json#', 'contentVersion': '1.0.0.0', 'parameters': {}, 'variables': {}, 'resources': [],'outputs':{}}";
    template = template.Replace("'", "\"");

    HttpResponseMessage myResponse = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };

    myResponse.Content = new StringContent(template, System.Text.Encoding.UTF8, "application/json");

    return myResponse;
}

public static async Task<string> GetSecret(string vaultName,string nameKey){
    
   
    string vaultUrl = $"https://{vaultName}.vault.azure.net/secrets/{nameKey}";

      AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();

    var kv = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

    var secret = await kv.GetSecretAsync(vaultUrl).ConfigureAwait(false);

    var secretUri = secret.Value;

    return secretUri;
}


public class MyMessage
{
  public string TenantId { get; set; }
  public string SubId { get; set; }
  public string ResourceGroup { get; set; }
  public string AppServiceName { get; set; }
  public string Repo { get; set; }
  public string Branch { get; set; }
}