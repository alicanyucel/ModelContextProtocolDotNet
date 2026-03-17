using ModelContextProtocol.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTransient<WeatherService>();
builder.Services.AddMcpServer().WithHttpTransport().WithToolsFromAssembly();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapGet("/", () => "hello world");
app.MapMcp("/mcp");
app.Run();
