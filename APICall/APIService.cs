using APICall.Components.Models;
using System.Text.Json;

namespace APICall
{
    public class APIService
    {
        // HttpClient injected via DI to perform HTTP requests to external APIs.
        private readonly HttpClient _httpClient;

        // JSON serializer options: case-insensitive property names so you don't need exact casing
        // in your C# models to match the JSON payloads.
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public APIService(HttpClient httpClient)
        {
            // Store the DI-provided HttpClient instance.
            _httpClient = httpClient;
        }
        // ---------------------------------------------------------
        // TRIVIA QUIZ (OpenTDB)
        // ---------------------------------------------------------
        // Fetches quiz questions from OpenTDB, transforms them into your internal QuizQuestion model,
        // HTML-decodes fields, builds answer options, and shuffles them.
        // Returns an empty list on any failure to keep the UI stable.
        public async Task<List<QuizQuestion>> GetTriviaAsync(int amount = 10)
        {
            try
            {
                // Build the OpenTDB query URL with the requested amount of questions.
                var url = $"https://opentdb.com/api.php?amount={amount}";

                // Execute the HTTP GET request.
                var response = await _httpClient.GetAsync(url);

                // If the HTTP call failed (non-2xx), return empty list.
                if (!response.IsSuccessStatusCode)
                    return new List<QuizQuestion>();

                // Read response content as a stream (efficient for larger payloads).
                var stream = await response.Content.ReadAsStreamAsync();

                // Deserialize the OpenTDB response into your DTO model.
                // If deserialization returns null, use a default empty object to avoid null refs.
                var data = await JsonSerializer.DeserializeAsync<OpenTdbResponse>(stream, _jsonOptions)
                          ?? new OpenTdbResponse();

                // OpenTDB responseCode: 0 means success. Also ensure we have results.
                if (data.ResponseCode != 0 || data.Results.Count == 0)
                    return new List<QuizQuestion>();

                // Prepare a random generator for shuffling answer options.
                var random = new Random();

                // Final list that will be returned to the caller.
                var list = new List<QuizQuestion>();

                // Transform each OpenTDB item into your internal QuizQuestion model.
                foreach (var question in data.Results)
                {
                    // Build the answer options list, starting with the correct answer
                    // (HTML-decoded and flagged as correct).
                    var options = new List<QuizAnswerOption>
                    {
                        new QuizAnswerOption
                        {
                            Text = HtmlUtil.Decode(question.CorrectAnswer),
                            IsCorrect = true
                        }
                    };

                    // Add incorrect answers (decoded) and flagged as not correct.
                    options.AddRange(
                        question.IncorrectAnswers.Select(answer => new QuizAnswerOption
                        {
                            Text = HtmlUtil.Decode(answer),
                            IsCorrect = false
                        })
                    );

                    // Shuffle the options using Fisher–Yates algorithm so correct answer is not always first.
                    for (int i = options.Count - 1; i > 0; i--)
                    {
                        int j = random.Next(i + 1);
                        (options[i], options[j]) = (options[j], options[i]);
                    }

                    // Build the QuizQuestion object with decoded category/difficulty/question text.
                    list.Add(new QuizQuestion
                    {
                        Category = HtmlUtil.Decode(question.Category),
                        Difficulty = HtmlUtil.Decode(question.Difficulty),
                        Question = HtmlUtil.Decode(question.Question),
                        Options = options
                    });
                }

                // Return the fully transformed list ready for the UI.
                return list;
            }
            // Network-related errors specific to this call: return empty list for resilience.
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Network error: {ex.Message}");
                return new List<QuizQuestion>();
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Json error: {ex.Message}");
                return new List<QuizQuestion>();
            }
        }
        // ---------------------------------------------------------
        // GENERIC HELPER METHOD
        // ---------------------------------------------------------
        // Fetches JSON from 'url' and deserializes it as List<T>.
        // Returns an empty list on any failure to keep consumers resilient.
        private async Task<List<T>> GetListAsync<T>(string url)
        {
            try
            {
                // Perform GET request to the specified URL.
                var response = await _httpClient.GetAsync(url);

                // If HTTP status code is not successful (e.g., 4xx/5xx), log and return empty list.
                if (response.IsSuccessStatusCode)
                {
                    // Read the response body as a string.
                    var json = await response.Content.ReadAsStringAsync();

                    // Attempt to deserialize into List<T>. If deserialization yields null, return new empty list.
                    return JsonSerializer.Deserialize<List<T>>(json, _jsonOptions) ?? new List<T>();
                }
                Console.WriteLine($"Error: {response.StatusCode}");
                return new List<T>();

            }
            // Network-related errors: DNS, connection refused, TLS, etc.
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Network error: {ex.Message}");
                return new List<T>();
            }
            // JSON parse/format errors: malformed JSON or mismatched schema for List<T>.
            catch (JsonException ex)
            {
                Console.WriteLine($"Json error: {ex.Message}");
                return new List<T>();
            }
        }

        // ---------------------------------------------------------
        // DIESEL
        // ---------------------------------------------------------
        // Convenience wrapper that fetches a List<Diesel> from the given endpoint.
        public Task<List<Diesel>> GetDieselDataAsync() =>
            GetListAsync<Diesel>("https://opgaver.mercantec.tech/Opgaver/Diesel");

        // ---------------------------------------------------------
        // GAS (Miles95)
        // ---------------------------------------------------------
        // Convenience wrapper that fetches a List<Gas> from the given endpoint.
        public Task<List<Gas>> GetGasDataAsync() =>
            GetListAsync<Gas>("https://opgaver.mercantec.tech/Opgaver/Miles95");
    }
}