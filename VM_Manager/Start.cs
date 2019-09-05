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

namespace VMManager {
    public static class Start {
        private static TraceWriter logWriter;
        [FunctionName("start")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]HttpRequestMessage req,
            [Blob("certs/id_ed25519", FileAccess.Read)] Stream certBlob,
            [Blob("certs/credentials.json", FileAccess.Read)] Stream credentialsBlob,
            TraceWriter log) {
            log.Info("C# HTTP trigger function processed a request.");
            logWriter = log;
            await Authentication.ReadAuthenticationDataFromFile(credentialsBlob);
            return await Authentication.ParseAuthenticationData(req, certBlob) ? await startVN(req) : Authentication.AuthenticationError();
        }

        private static async Task<HttpResponseMessage> startVN(HttpRequestMessage req) {
            if (await Authentication.AuthenticateToAzure()) {
                string baseUri = "https://management.azure.com";
                string path = "/subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.Compute/virtualMachines/{2}/start?api-version=2018-06-01";
                path = string.Format(path, Authentication.ManagementData.SubscriptionID, Authentication.ManagementData.ResourceGroupName, Authentication.ManagementData.VMName);
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
