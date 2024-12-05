using System.Text.Json;

namespace Services;

public class JsonHelper
{
    public decimal JsonBody(string responseBody)
    {
       return decimal.Parse(JsonDocument.Parse(responseBody)
            .RootElement
            .GetProperty("amount")
            .ToString());
    }
}
