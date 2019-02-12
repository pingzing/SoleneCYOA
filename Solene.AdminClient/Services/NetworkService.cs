using Newtonsoft.Json;
using Solene.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Solene.AdminClient.Services
{
    public class NetworkService
    {
        private static HttpClient _httpClient = new HttpClient();

        static NetworkService()
        {
            _httpClient.BaseAddress = new Uri("http://localhost:7071/api/");
        }

        public static async Task<Player> CreatePlayer(Player player)
        {
            var response = await _httpClient.PostAsJsonAsync($"player?{GetFunctionCode()}", player);
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"CreatePlayer failed: HTTP {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
                return null;
            }

            return await response.Content.ReadAsAsync<Player>();
        }

        public static async Task<Player> GetPlayer(Guid playerId)
        {
            var response = await _httpClient.GetAsync($"player/{playerId.ToString("N")}?{GetFunctionCode()}");
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"GetPlayer failed: HTTP {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
                return null;
            }

            return await response.Content.ReadAsAsync<Player>();
        }

        public static async Task<List<Player>> GetAllPlayers()
        {
            var response = await _httpClient.GetAsync($"players/?{GetFunctionCode()}");
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"GetAllPlayers failed: HTTP {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
                return null;
            }

            return await response.Content.ReadAsAsync<List<Player>>();
        }

        public static async Task<List<Question>> GetPlayerQuestions(Guid id)
        {
            var response = await _httpClient.GetAsync($"player/{id.ToString("N")}/questions?{GetFunctionCode()}");
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"GetPlayerQuestions for {id} failed: HTTP {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
                return null;
            }

            var questionsList = JsonConvert.DeserializeObject<List<Question>>(await response.Content.ReadAsStringAsync())
                .OrderBy(x => x.SequenceNumber)
                .ToList();
            return questionsList;
        }

        public static async Task<bool> DeletePlayer(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"player/{id.ToString("N")}?{GetFunctionCode()}");
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"DeletePlayer for {id} failed: HTTP {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
                return false;
            }

            return true;
        }

        public static async Task<Question> AddQuestion(Guid playerId, Question newQuestion)
        {
            string questionJson = JsonConvert.SerializeObject(newQuestion);
            var response = await _httpClient.PostAsync(
                $"player/{playerId.ToString("N")}/questions?{GetFunctionCode()}",
                new StringContent(questionJson));

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"AddQuestion for {playerId} failed: HTTP {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
                return null;
            }

            return await response.Content.ReadAsAsync<Question>();
        }

        public static async Task<PlayersAndDetails> GetAllPlayersAndQuestions()
        {
            var response = await _httpClient.GetAsync($"players-and-details?{GetFunctionCode()}");
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"GetAllPlayersAndQuestions failed: HTTP {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
            }

            return await response.Content.ReadAsAsync<PlayersAndDetails>();
        }

        private static string GetFunctionCode([CallerMemberName]string functionName = null)
        {
            switch (functionName)
            {
                case nameof(CreatePlayer):
                    return $"code={Consts.Secrets.CreatePlayerFunctionCode}";
                case nameof(GetPlayer):
                    return $"code={Consts.Secrets.GetPlayerFunctionCode}";
                case nameof(GetPlayerQuestions):
                    return $"code={Consts.Secrets.GetPlayerQuestionsFunctionCode}";
                case nameof(GetAllPlayers):
                    return $"code={Consts.Secrets.GetAllPlayersFunctionCode}";
                case nameof(DeletePlayer):
                    return $"code={Consts.Secrets.DeletePlayerFunctionCode}";
                case nameof(AddQuestion):
                    return $"code={Consts.Secrets.AddQuestionFunctionCode}";
                case nameof(GetAllPlayersAndQuestions):
                    return $"code={Consts.Secrets.GetAllPlayersAndQuestionsFunctionCode}";
                default:
                    throw new ArgumentOutOfRangeException($"No function code found for {functionName}");
            }
        }
    }
}
