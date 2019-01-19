using Newtonsoft.Json;
using Solene.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace Solene.MobileApp.Core.Services
{
    public interface INetworkService
    {
        Task<MaybeResult<Player, GenericErrorResult>> CreatePlayer(Player player);
        Task<MaybeResult<List<Question>, GenericErrorResult>> GetPlayerQuestions(Guid id);
    }

    public class NetworkService : INetworkService
    {
        private HttpClient _httpClient;

        public NetworkService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://solene.azurewebsites.net/api/");
        }

        public async Task<MaybeResult<Player, GenericErrorResult>> CreatePlayer(Player player)
        {
            var response = await _httpClient.PostAsJsonAsync($"player?{GetFunctionCode()}", player);
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"CreatePlayer failed: HTTP {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
                return NetworkMaybeResult.Failure<Player>(GenericErrorResult.NoResponse);
            }

            var createdPlayer = JsonConvert.DeserializeObject<Player>(await response.Content.ReadAsStringAsync());
            return NetworkMaybeResult.Success(createdPlayer);
        }

        public async Task<MaybeResult<List<Question>, GenericErrorResult>> GetPlayerQuestions(Guid id)
        {
            var response = await _httpClient.GetAsync($"player/{id.ToString("N")}/questions?{GetFunctionCode()}");
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"GetPlayerQuestions for {id} failed: HTTP {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
                return NetworkMaybeResult.Failure<List<Question>>(GenericErrorResult.NoResponse);
            }

            var questionsList = JsonConvert.DeserializeObject<List<Question>>(await response.Content.ReadAsStringAsync());
            return NetworkMaybeResult.Success(questionsList);            
        }

        private string GetFunctionCode()
        {
            return $"code={Consts.Secrets.AzureFunctionsCode}";
        }
    }
}
