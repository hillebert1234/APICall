using APICall.Components.Models;
using System.Text.Json;

namespace APICall
{
    public class APIService
    {
        private readonly HttpClient _httpClient;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public APIService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // ---------------------------
        // GENERISK HJÆLPEMETODE
        // ---------------------------
        private async Task<List<T>> GetListAsync<T>(string url)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Fejl: {response.StatusCode}");
                    return new List<T>();
                }

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<T>>(json, _jsonOptions) ?? new List<T>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fejl ved kald til {url}: {ex.Message}");
                return new List<T>();
            }
        }

        // ---------------------------
        // DIESEL
        // ---------------------------
        public Task<List<Diesel>> GetDieselDataAsync() =>
            GetListAsync<Diesel>("https://opgaver.mercantec.tech/Opgaver/Diesel");

        // ---------------------------
        // GAS
        // ---------------------------
        public Task<List<Gas>> GetGasDataAsync() =>
            GetListAsync<Gas>("https://opgaver.mercantec.tech/Opgaver/Miles95");

        // ---------------------------
        // TRIVIA QUIZ
        // ---------------------------
        public async Task<List<QuizQuestion>> GetTriviaAsync(int amount = 10)
        {
            try
            {
                var url = $"https://opentdb.com/api.php?amount={amount}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return new List<QuizQuestion>();

                var stream = await response.Content.ReadAsStreamAsync();
                var data = await JsonSerializer.DeserializeAsync<OpenTdbResponse>(stream, _jsonOptions)
                          ?? new OpenTdbResponse();

                if (data.ResponseCode != 0 || data.Results.Count == 0)
                    return new List<QuizQuestion>();

                var rnd = new Random();
                var list = new List<QuizQuestion>();

                foreach (var question in data.Results)
                {
                    var options = new List<QuizAnswerOption>
                    {
                        new QuizAnswerOption
                        {
                            Text = HtmlUtil.Decode(question.CorrectAnswer),
                            IsCorrect = true
                        }
                    };

                    options.AddRange(
                        question.IncorrectAnswers.Select(a => new QuizAnswerOption
                        {
                            Text = HtmlUtil.Decode(a),
                            IsCorrect = false
                        })
                    );

                    // Shuffle
                    for (int i = options.Count - 1; i > 0; i--)
                    {
                        int j = rnd.Next(i + 1);
                        (options[i], options[j]) = (options[j], options[i]);
                    }

                    list.Add(new QuizQuestion
                    {
                        Category = HtmlUtil.Decode(question.Category),
                        Difficulty = HtmlUtil.Decode(question.Difficulty),
                        Question = HtmlUtil.Decode(question.Question),
                        Options = options
                    });
                }

                return list;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Trivia fejl: {ex.Message}");
                return new List<QuizQuestion>();
            }
        }
    }
}
