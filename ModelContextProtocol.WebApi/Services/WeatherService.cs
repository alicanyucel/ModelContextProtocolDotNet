using ModelContextProtocol.WebApi.Models;

namespace ModelContextProtocol.WebApi.Services;

public sealed class WeatherService
{
    public static List<Weather> Weathers { get; set; } = new()
    {
        new()
        {
            City="Ankara",
            Temp=12
        },
        new()
        {
            City="İstanbul",
            Temp=13
        },
        new()
        {
            City="Kayseri",
            Temp=14
        },
    };
    public Weather Get(string City)
    {
        var weather = Weathers.FirstOrDefault(x => x.City == City);
        if (weather == null)
        {
            throw new ArgumentException(nameof(weather));
        }
        return weather;
    }
}
