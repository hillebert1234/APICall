using APICall.Components.Models;
using System.Text.Json;

namespace APICall
{
    public class APIService
    {
        private readonly HttpClient _httpClient;
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public APIService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<List<Diesel>>GetDieselDataAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("https://opgaver.mercantec.tech/Opgaver/Diesel");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<List<Diesel>>(json, _jsonOptions);
                    return result ?? new List<Diesel>();
                }
                else
                {
                    Console.WriteLine($"Fejl: {response.StatusCode}");
                    return new List<Diesel>();
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Netværksfejl: {ex.Message}");
                return new List<Diesel>();
            }
        }
        public async Task<List<Gas>> GetGasDataAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("https://opgaver.mercantec.tech/Opgaver/Miles95");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<List<Gas>>(json, _jsonOptions);
                    return result ?? new List<Gas>();
                }
                else
                {
                    Console.WriteLine($"Fejl: {response.StatusCode}");
                    return new List<Gas>();
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Netværksfejl: {ex.Message}");
                return new List<Gas>();
            }
        }
    }
}
    
