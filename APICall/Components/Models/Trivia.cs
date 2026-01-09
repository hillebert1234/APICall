using System.Net;
using System.Text.Json.Serialization;

namespace APICall.Components.Models
{
    public class OpenTdbResponse
    {
        [JsonPropertyName("response_code")]
        public int ResponseCode { get; set; }

        [JsonPropertyName("results")]
        public List<OpenTdbQuestion> Results { get; set; } = new();
    }

    public class OpenTdbQuestion
    {
        [JsonPropertyName("category")]
        public string? Category { get; set; } 

        [JsonPropertyName("type")]
        public string? Type { get; set; } // "multiple" eller "boolean"

        [JsonPropertyName("difficulty")]
        public string? Difficulty { get; set; } // "easy" / "medium" / "hard"

        [JsonPropertyName("question")]
        public string? Question { get; set; }

        [JsonPropertyName("correct_answer")]
        public string? CorrectAnswer { get; set; }

        [JsonPropertyName("incorrect_answers")]
        public List<string> IncorrectAnswers { get; set; } = new();
    }

    // Domænemodel til UI
    public class QuizQuestion
    {
        public string? Category { get; set; }
        public string? Difficulty { get; set; }
        public string? Question { get; set; } 
        public List<QuizAnswerOption> Options { get; set; } = new();
    }

    public class QuizAnswerOption
    {
        public string? Text { get; set; }
        public bool IsCorrect { get; set; }
    }

    public static class HtmlUtil
    {
        public static string Decode(string s) => WebUtility.HtmlDecode(s ?? "");
    }
}

