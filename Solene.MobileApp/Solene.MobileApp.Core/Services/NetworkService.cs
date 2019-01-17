using Newtonsoft.Json;
using Solene.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Solene.MobileApp.Core.Services
{
    public interface INetworkService
    {
        Task<Player> CreatePlayer(Player player);
    }

    public class NetworkService : INetworkService
    {
        private HttpClient _httpClient;

        public NetworkService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://solene.azurewebsites.net/api/");
        }

        public async Task<Player> CreatePlayer(Player player)
        {
            var response = await _httpClient.PostAsJsonAsync($"player?{GetFunctionCode()}", player);
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"CreatePlayer failed: HTTP {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
                return null;
            }

            var createdPlayer = JsonConvert.DeserializeObject<Player>(await response.Content.ReadAsStringAsync());
            return createdPlayer;
        }

        private string GetFunctionCode()
        {
            return $"code={Consts.Secrets.AzureFunctionsCode}";
        }
    }
}
