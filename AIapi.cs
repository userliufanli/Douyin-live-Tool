using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WindowsFormsApp4
{
    class AIapi
    {
        private readonly string apiKey;
        private readonly string secretKey;

        public AIapi(string apiKey, string secretKey)
        {
            this.apiKey = apiKey;
            this.secretKey = secretKey;
        }

       
        public async Task<string> ChatWithDoubao(string question, string model = "ep-20250215031901-znmzz")
        {
            using (HttpClient client = new HttpClient())
            {
                string chatUrl = "https://ark.cn-beijing.volces.com/api/v3/context/create";
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                var requestData = new
                {
                    model = model,
                    messages = new[]
                   {
                        new
                        {
                            role = "system",
                            content = "你是一个臭嘴知识主播，你收到的每一句话都是一个观众的问题，你可以简短并且精准的回答问题"
                        }
                    },
                    ttl = 3600,
                    mode = "session"
                };

                string jsonData = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(chatUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    string readasstring = await response.Content.ReadAsStringAsync();
                    return readasstring;
                  
                }
                else
                {
                    throw new Exception($"调用 API 失败: {response.StatusCode}");
                }
            }
        }
        public async Task<string> ChatWithDoubaos(string question, string id, string model = "ep-20250215031901-znmzz" )
        {

            JsonDocument document = JsonDocument.Parse(id);
            JsonElement root = document.RootElement;

            if (root.TryGetProperty("id", out JsonElement idElement) && idElement.ValueKind == JsonValueKind.String)
            {
                id = idElement.GetString();
                
            }
            
            using (HttpClient client = new HttpClient())
            {
                string chatUrl = "https://ark.cn-beijing.volces.com/api/v3/context/chat/completions";
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                var requestData = new
                {

                    model = model,
                    context_id=id,
                    messages = new[]
                   {
                        new
                        {
                            role = "user",
                            content = question
                        }
                    },
                };

                string jsonData = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                //request.Content = content;
                HttpResponseMessage response = await client.PostAsync(chatUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    string readasstring = await response.Content.ReadAsStringAsync();
                    return readasstring;

                }
                else
                {
                    return "";
                    //throw new Exception($"调用 API 失败: {response.StatusCode}");
                }
            }
        }










    }
}
