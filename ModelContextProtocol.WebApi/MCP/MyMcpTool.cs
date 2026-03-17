using ModelContextProtocol.Server;
using ModelContextProtocol.WebApi.Models;
using ModelContextProtocol.WebApi.Services;
using System.ComponentModel;


namespace ModelContextProtocol.WebApi.MCP;

[McpServerToolType]
public static class MyMcpTool
{
    [McpServerTool,Description("verilen şehir bilgiisne göre o şehrin sıcaklığını döndür")]
    public static Weather GetWeather([Description("Şehir bilgisi")] string city,WeatherService weatherService)
    {
        var res = weatherService.Get(city);
        return res;
    }
    [McpServerTool,Description("Personellerin tarih bazlı satış listesini döndürür")]
    public static List<Sale> GetSales()
    {
        return Sale.Sales;
    }
    [McpServerTool, Description("Yapılcak ödemelerin son tarihlerini ve firma bilgilerinni döndürür")]
    public static List<Payment> GetPayments()
    {
        return Payment.Payments;
    }
}
