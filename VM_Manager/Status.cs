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

<<<<<<< HEAD
namespace VMManager
=======
namespace FunctionApp3
>>>>>>> 54ecdc91abc9326d183b6b1e2074238b760ce9a7
{
    public static class Status
    {
        private static TraceWriter logWriter;
        [FunctionName("status")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]HttpRequestMessage req,
            [Blob("certs/id_ed25519", FileAccess.Read)] Stream certBlob,
            [Blob("certs/credentials.json", FileAccess.Read)] Stream credentialsBlob,
            TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");
            logWriter = log;
            await Authentication.ReadAuthenticationDataFromFile(credentialsBlob);
            return await Authentication.ParseAuthenticationData(req, certBlob) ? await checkVM(req) : Authentication.AuthenticationError();
        }

        private static async Task<HttpResponseMessage> checkVM(HttpRequestMessage req) {
            if (await Authentication.AuthenticateToAzure()) {
                string baseUri = "https://management.azure.com";
                string path = "/subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.Compute/virtualMachines/{2}?$expand=instanceView&api-version=2018-06-01";
                path = string.Format(path, Authentication.ManagementData.SubscriptionID, Authentication.ManagementData.ResourceGroupName, Authentication.ManagementData.VMName);
                HttpClient client = new HttpClient() {
                    BaseAddress = new Uri(baseUri)
                };
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Authentication.BearerToken.AccessToken);
                HttpResponseMessage response = await client.GetAsync(path);
                if (response.IsSuccessStatusCode) {
                    dynamic content = await response.Content.ReadAsStringAsync();
                    try {
                        dynamic obj = JObject.Parse(content as string);
                        dynamic statuses = obj.properties.instanceView.statuses;
                        if (statuses is JArray statuses2) {
                            IEnumerable<JToken> status = statuses2.Where(o => o["code"].ToString().StartsWith("PowerState/"));
                            if (statuses2.Count > 0 && status.Count() == 1) {
                                string code = status.ElementAt(0)["code"].ToString();
                                if (code.Length > 0) {
                                    return returnPowerState(code.Substring("PowerState/".Length));
                                }
                            }
                        }
                    } catch(RuntimeBinderException ex) {
                        logWriter.Info(ex.Message);
                    }
                }
            }
            return returnPowerState("pending");
        }

        private static HttpResponseMessage returnPowerState(string powerState) {
            string json = JsonConvert.SerializeObject(powerState);
            return new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        }
    }
}
