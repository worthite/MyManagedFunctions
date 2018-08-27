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

const string Passed = "Pass";
const string Failed = "Fail";

private static string vaultname = ConfigurationManager.AppSettings["KeyVault"];

public static void Run(TimerInfo myTimer, TraceWriter log)
{
    log.Info("C# HTTP trigger function processed a request.");
    
    

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

