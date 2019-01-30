using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FunctionApp3 {
    internal sealed class CertData {
        public string cert { get; set; }
    }

    internal sealed class AuthenticationResponse {
        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        public long ExpiresIn { get; set; }

        [JsonProperty("ext_expires_in")]
        public long ExtExpiresIn { get; set; }

        [JsonProperty("expires_on")]
        public long ExpiresOn { get; set; }

        [JsonProperty("not_before")]
        public long NotBefore { get; set; }

        [JsonProperty("resource")]
        public Uri Resource { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }

    internal sealed class Authentication {
        public static AuthenticationResponse BearerToken { get; private set; }
        public static bool Authenticate(CertData cert, Stream certBlob) {
            try {
                //string path = "D:\\home\\site\\wwwroot";
                //string path2 = ".";
                //string text = await Task.FromResult<string>(File.ReadAllText(path + "\\id_ed25519", Encoding.UTF8));
                if(certBlob.CanRead) {
                    string fileContent = new StreamReader(certBlob).ReadToEnd();
                    fileContent = fileContent.Trim();
                    cert.cert = cert.cert.Trim();
                    if (fileContent == cert.cert) {
                        return true;
                    }
                }
            } catch (System.IO.IOException ex) {
                return false;
            }
            return false;
        }

        public static async Task<bool> ParseAuthenticationData(HttpRequestMessage req, Stream certBlob) {
            dynamic body = await req.Content.ReadAsStringAsync();
            try {
                CertData e = JsonConvert.DeserializeObject<CertData>(body as string);
                return Authenticate(e, certBlob);
            } catch (Exception ex) {
                return false;
            }
        }

        public static HttpResponseMessage AuthenticationError() {
            var testJsonStr = new { status = "Authentication Error" };
            string json = JsonConvert.SerializeObject(testJsonStr);
            return new HttpResponseMessage(HttpStatusCode.Forbidden) {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        }

        public static async Task<bool> AuthenticateToAzure() {
            string baseUri = "https://login.microsoftonline.com";
            string path = "/{0}/oauth2/token";
            string resource = "https://management.azure.com";
            path = string.Format(path, AzureManagementData.DOMAIN);
            HttpClient client = new HttpClient {
                BaseAddress = new Uri(baseUri)
            };
            Dictionary<string, string> formData = new Dictionary<string, string> {
                { "grant_type", "client_credentials" },
                { "client_id", AzureManagementData.CLIENTID },
                { "client_secret", AzureManagementData.SECRET },
                { "resource", resource }
            };
            HttpResponseMessage response = await client.PostAsync(path, new FormUrlEncodedContent(formData));
            if(response.IsSuccessStatusCode) {
                try {
                    HttpContent content = response.Content;
                    dynamic body = await content.ReadAsStringAsync();
                    AuthenticationResponse jsonObj = JsonConvert.DeserializeObject<AuthenticationResponse>(body as string);
                    if(jsonObj != null) {
                        BearerToken = jsonObj;
                        return true;
                    }
                } catch(JsonSerializationException ex) {
                }
            }
            return false;
        }
    }
}
