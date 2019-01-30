using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FunctionApp3
{
    public static class Stop
    {
        private static TraceWriter logWriter;
        [FunctionName("stop")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]HttpRequestMessage req,
            [Blob("certs/id_ed25519", FileAccess.Read)] Stream certBlob,
            TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");
            log.Info(Directory.GetCurrentDirectory());
            logWriter = log;
            return await Authentication.ParseAuthenticationData(req, certBlob) ? await stopVM(req) : Authentication.AuthenticationError();
        }

        private static async Task<HttpResponseMessage> stopVM(HttpRequestMessage req) {
            if (await Authentication.AuthenticateToAzure()) {
                string baseUri = "https://management.azure.com";
                string path = "/subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.Compute/virtualMachines/{2}/deallocate?api-version=2018-06-01";
                path = string.Format(path, AzureManagementData.SUBSCRIPTIONID, AzureManagementData.RESOURCEGROUPNAME, AzureManagementData.VMNAME);
                HttpClient client = new HttpClient() {
                    BaseAddress = new Uri(baseUri)
                };
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Authentication.BearerToken.AccessToken);
                HttpResponseMessage response = await client.PostAsync(path, new StringContent("", Encoding.UTF8, "application/json"));
                if (response.IsSuccessStatusCode) {
                    return returnPowerState("success");
                }
            }
            string json = JsonConvert.SerializeObject("Error");
            return new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        }

        private static HttpResponseMessage returnPowerState(string powerState) {
            string json = JsonConvert.SerializeObject(powerState);
            return new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        }
    }
}
