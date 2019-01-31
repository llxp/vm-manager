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
    internal struct CertData {
        [JsonProperty("cert")]
        public string Cert { get; set; }
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
        public static AzureManagementData ManagementData { get; private set; }
        public static bool Authenticate(CertData cert, Stream certBlob) {
            try {
                if(certBlob.CanRead) {
                    using (StreamReader streamReader = new StreamReader(certBlob)) {
                        string fileContent = streamReader.ReadToEnd();
                        fileContent = fileContent.Trim().ReplaceAllCharacters('\n').ReplaceAllCharacters('\r').ToLowerInvariant();
                        cert.Cert = cert.Cert.Trim().ReplaceAllCharacters('\n').ReplaceAllCharacters('\r').ToLowerInvariant();
                        if (fileContent == cert.Cert) {
                            return true;
                        }
                    }
                }
            } catch (System.IO.IOException) {
                return false;
            }
            return false;
        }

        public static async Task<bool> ParseAuthenticationData(HttpRequestMessage req, Stream certBlob) {
            dynamic body = await req.Content.ReadAsStringAsync();
            try {
                CertData e = JsonConvert.DeserializeObject<CertData>(body as string);
                return Authenticate(e, certBlob);
            } catch (Exception) {
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
            path = string.Format(path, ManagementData.Tenant);
            HttpClient client = new HttpClient {
                BaseAddress = new Uri(baseUri)
            };
            Dictionary<string, string> formData = new Dictionary<string, string> {
                { "grant_type", "client_credentials" },
                { "client_id", ManagementData.ClientID },
                { "client_secret", ManagementData.Secret },
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
                } catch (JsonSerializationException) {
                }
            }
            return false;
        }

        public static async Task ReadAuthenticationDataFromFile(Stream stream) {
            try {
                if (stream.CanRead) {
                    using (StreamReader streamReader = new StreamReader(stream)) {
                        string fileContent = await streamReader.ReadToEndAsync();
                        ManagementData = JsonConvert.DeserializeObject<AzureManagementData>(fileContent);
                    }
                }
            } catch (Exception) {
                ManagementData = null;
            }
        }
    }

    internal static class StringExtensions {
        public static string ReplaceAllCharacters(this string s, params char[] removeChars) {
            StringBuilder sb = new StringBuilder(s.Length);
            foreach (char c in s) {
                if (!removeChars.Contains(c)) {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }
}
