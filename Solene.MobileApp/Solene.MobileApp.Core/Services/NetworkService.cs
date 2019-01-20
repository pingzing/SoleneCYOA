using Newtonsoft.Json;
using Solene.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Solene.MobileApp.Core.Services
{
    public interface INetworkService
    {
        Task<MaybeResult<Player, GenericErrorResult>> CreatePlayer(Player player);
        Task<MaybeResult<List<Question>, GenericErrorResult>> GetPlayerQuestions(Guid id);
        Task<MaybeResult<bool, GenericErrorResult>> RegisterPushNotifications(Guid id, PushRegistrationRequest pushRegistration);
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
            var response = await PostAsJsonAsync($"player?{GetFunctionCode()}", player);
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

        public async Task<MaybeResult<bool, GenericErrorResult>> RegisterPushNotifications(Guid id, PushRegistrationRequest pushRegistration)
        {
            var response = await PostAsJsonAsync($"player/{id.ToString("N")}/push?{GetFunctionCode()}", pushRegistration);
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"Failed to register push notifications.");
                return NetworkMaybeResult.Failure<bool>(GenericErrorResult.BadRequest);
            }

            return NetworkMaybeResult.Success(true);
        }

        private async Task<HttpResponseMessage> PostAsJsonAsync<T>(string requestUri, T value)
        {
            // Android doesn't seem to understand the PostAsJsonAsync extension method, and always sends an empty body
            // unless we send it up manually. Shrug.
            if(Device.RuntimePlatform == Device.Android)
            {
                var content = new ObjectContent<T>(value, new JsonMediaTypeFormatter());
                string contentString = await content.ReadAsStringAsync();
                return await _httpClient.PostAsync(requestUri, new StringContent(contentString, Encoding.UTF8, "application/json"));
            }
            else
            {
                return await _httpClient.PostAsJsonAsync<T>(requestUri, value);
            }
        }

        private string GetFunctionCode([CallerMemberName]string functionName = null)
        {
            switch (functionName)
            {
                case nameof(CreatePlayer):
                    return $"code={Consts.Secrets.CreatePlayerFunctionCode}";
                case nameof(GetPlayerQuestions):
                    return $"code={Consts.Secrets.GetPlayerQuestionsFunctionCode}";
                case nameof(RegisterPushNotifications):
                    return $"code={Consts.Secrets.RegisterPushNotificationsCode}";                
                default:
                    throw new ArgumentOutOfRangeException($"No function code found for {functionName}");
            }
        }
    }
}
