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

        public async Task<List<Diesel>> GetDieselDataAsync()
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

        public async Task<List<QuizQuestion>> GetTriviaAsync(int amount = 10)
        {
            try
            {
                var url = $"https://opentdb.com/api.php?amount={amount}";
                using var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Fejl: {response.StatusCode}");
                    return new List<QuizQuestion>();
                }

                await using var stream = await response.Content.ReadAsStreamAsync();
                var dto = await JsonSerializer.DeserializeAsync<OpenTdbResponse>(stream, _jsonOptions)
                          ?? new OpenTdbResponse();

                if (dto.ResponseCode != 0 || dto.Results.Count == 0)
                {
                    return new List<QuizQuestion>();
                }

                var rnd = new Random();
                var list = new List<QuizQuestion>(dto.Results.Count);

                foreach (var q in dto.Results)
                {
                    var options = new List<QuizAnswerOption>
                    {
                    new QuizAnswerOption
                    {
                        Text = HtmlUtil.Decode(q.CorrectAnswer),
                        IsCorrect = true
                    }
                };

                    options.AddRange(
                        q.IncorrectAnswers.Select(a => new QuizAnswerOption
                        {
                            Text = HtmlUtil.Decode(a),
                            IsCorrect = false
                        })
                    );

                    // Shuffle svarene
                    for (int i = options.Count - 1; i > 0; i--)
                    {
                        int j = rnd.Next(i + 1);
                        (options[i], options[j]) = (options[j], options[i]);
                    }

                    var question = new QuizQuestion
                    {
                        Category = HtmlUtil.Decode(q.Category),
                        Difficulty = HtmlUtil.Decode(q.Difficulty),
                        Question = HtmlUtil.Decode(q.Question),
                        Options = options
                    };

                    list.Add(question);
                }
                return list;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Netværksfejl: {ex.Message}");
                return new List<QuizQuestion>();
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON-fejl: {ex.Message}");
                return new List<QuizQuestion>();
            }
        }
    }
}