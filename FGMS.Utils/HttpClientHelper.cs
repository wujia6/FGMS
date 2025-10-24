using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace FGMS.Utils
{
    public class HttpClientHelper
    {
        private readonly IHttpClientFactory httpClientFactory;

        public HttpClientHelper(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<T> GetAsync<T>(string url, Dictionary<string, string> headerDict = null)
        {
            return await SendAsync<T>("application/json;charset=UTF-8", HttpMethod.Get, url, null, headerDict);
        }

        public async Task<T> PostAsync<T>(string url, dynamic requestData, Dictionary<string, string> headerDict = null)
        {
            return await SendAsync<T>("application/json", HttpMethod.Post, url, requestData, headerDict);
        }

        public async Task<T> PutAsync<T>(string url, dynamic requestData, Dictionary<string, string> headerDict = null)
        {
            return await SendAsync<T>("application/json", HttpMethod.Put, url, requestData, headerDict);
        }

        public async Task<T> DeleteAsync<T>(string url, Dictionary<string, string> headerDict = null)
        {
            return await SendAsync<T>("application/json", HttpMethod.Delete, url, null, headerDict);
        }

        public async Task<T> PostFormAsync<T>(HttpClient _client, string url, Dictionary<string, string> param)
        {
            try
            {
                using var multipartFormDataContent = new FormUrlEncodedContent(param);
                Console.WriteLine(JsonConvert.SerializeObject(param));
                var result = await _client.PostAsync(url, multipartFormDataContent).Result.Content.ReadAsStringAsync();
                var resp = JsonConvert.DeserializeObject<T>(result);
                return resp;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<HttpResponseMessage> SendRequestAsync(string url, string contentType, Dictionary<string, string>? headerDict = null)
        {
            try
            {
                var client = httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Clear();
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                if (headerDict != null)
                {
                    foreach (var header in headerDict)
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }
                }
                using HttpContent httpContent = new StringContent(string.Empty, Encoding.UTF8);
                httpContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
                request.Content = httpContent;
                var response = await client.SendAsync(request);
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ContentType"></param>
        /// <param name="method"></param>
        /// <param name="url"></param>
        /// <param name="requestData"></param>
        /// <param name="headerDict"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task<T> SendAsync<T>(string contentType, HttpMethod method, string url, dynamic requestData, Dictionary<string, string> headerDict = null)
        {
            string content = "";
            if (requestData != null)
            {
                content = JsonConvert.SerializeObject(requestData, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
            }
            try
            {
                var client = httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Clear();
                using var request = new HttpRequestMessage(method, url);
                if (headerDict != null)
                {
                    foreach (var d in headerDict)
                    {
                        request.Headers.Add(d.Key, d.Value);
                    }
                }
                using HttpContent httpContent = new StringContent(content, Encoding.UTF8);
                httpContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
                request.Content = httpContent;
                var response = await client.SendAsync(request).Result.Content.ReadAsStringAsync();
                //string responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(response)!;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
