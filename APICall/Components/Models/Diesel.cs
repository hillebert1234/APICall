namespace APICall.Components.Models
{
    public class Diesel
    {
        public DateTime? Date { get; set; } 
        public string? Price { get; set; }
        public decimal? PriceValue { get { if (decimal.TryParse(Price, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var value)) { return value; } return null; } }
    }
    public class Gas
    {
        public DateTime? Date { get; set; }
        public string? Price { get; set; }
        public decimal? PriceValue { get { if (decimal.TryParse(Price, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var value)) { return value; } return null; } }
    }
}
