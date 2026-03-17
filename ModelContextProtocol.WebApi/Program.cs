using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ModelContextProtocol.AspNetCore.Authentication;
using ModelContextProtocol.WebApi.Services;
using System.Text;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
var serverUrl = "http://localhost:5000";
var jwtSecret = "My secret key My secret key My secret key My secret key My secret key My secret key My secret key My secret key My secret key My secret key My secret key My secret key My secret key My secret key";
var jwtIssuer = serverUrl;   // "Issuer" yerine serverUrl
var jwtAudience = serverUrl; // "Audience" yerine serverUrl

builder.Services.AddAuthentication(options =>
{
    options.DefaultChallengeScheme = McpAuthenticationDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(opt =>
{
    opt.TokenValidationParameters.ValidateIssuer = true;
    opt.TokenValidationParameters.ValidateAudience = true;
    opt.TokenValidationParameters.ValidateIssuerSigningKey = true;
    opt.TokenValidationParameters.ValidateLifetime = true;
    opt.TokenValidationParameters.ValidIssuer = jwtIssuer;
    opt.TokenValidationParameters.ValidAudience = jwtAudience;
    opt.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
})
.AddMcp(options =>
{
    options.ResourceMetadata = new()
    {
        Resource = serverUrl,
        // Auth server'ınızın OAuth 2.0 metadata endpoint'i olmalı
        // GET /.well-known/oauth-authorization-server döndürmeli
        AuthorizationServers = new List<string> { serverUrl },
        ScopesSupported = new[] { "mcp:tools" }
    };
});
builder.Services.AddAuthorization();

builder.Services.AddHttpContextAccessor();

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

app.UseAuthentication();
app.UseAuthorization();
// ✅ OAuth Authorization Server Metadata (RFC 8414)
app.MapGet("/.well-known/oauth-authorization-server", () => Results.Json(new
{
    issuer = serverUrl,
    authorization_endpoint = $"{serverUrl}/oauth/authorize",
    token_endpoint = $"{serverUrl}/oauth/token",
    registration_endpoint = $"{serverUrl}/oauth/register",
    response_types_supported = new[] { "code" },
    grant_types_supported = new[] { "authorization_code", "refresh_token" },
    code_challenge_methods_supported = new[] { "S256" }, // PKCE
    scopes_supported = new[] { "mcp:tools" },
    token_endpoint_auth_methods_supported = new[] { "none" } // public client
}));

var registeredClients = new Dictionary<string, string>(); // clientId -> clientName

app.MapPost("/oauth/register", async (HttpContext context) =>
{
    var body = await context.Request.ReadFromJsonAsync<JsonElement>();

    var clientId = Guid.NewGuid().ToString("N");
    var clientName = body.TryGetProperty("client_name", out var name)
        ? name.GetString() ?? "unknown"
        : "unknown";

    registeredClients[clientId] = clientName;

    return Results.Json(new
    {
        client_id = clientId,
        client_name = clientName,
        redirect_uris = body.TryGetProperty("redirect_uris", out var uris)
            ? uris : default,
        grant_types = new[] { "authorization_code", "refresh_token" },
        response_types = new[] { "code" },
        token_endpoint_auth_method = "none"
    }, statusCode: 201);
});

app.MapGet("/oauth/authorize", (
    string response_type,
    string client_id,
    string redirect_uri,
    string code_challenge,
    string code_challenge_method,
    string? state,
    string? scope) =>
{
    // Login formunu göster
    var html = $"""
        <!DOCTYPE html>
        <html>
        <head><title>Login</title></head>
        <body>
            <h2>MCP Server Login</h2>
            <form method="post" action="/oauth/login">
                <input type="hidden" name="redirect_uri" value="{redirect_uri}" />
                <input type="hidden" name="code_challenge" value="{code_challenge}" />
                <input type="hidden" name="state" value="{state}" />
                <label>Username: <input type="text" name="username" /></label><br/>
                <label>Password: <input type="password" name="password" /></label><br/>
                <button type="submit">Login</button>
            </form>
        </body>
        </html>
        """;

    return Results.Content(html, "text/html");
});

var authCodes = new Dictionary<string, AuthCodeData>();

app.MapPost("/oauth/login", async (HttpContext context) =>
{
    var form = await context.Request.ReadFormAsync();
    var username = form["username"].ToString();
    var password = form["password"].ToString();
    var redirectUri = form["redirect_uri"].ToString();
    var codeChallenge = form["code_challenge"].ToString();
    var state = form["state"].ToString();

    
    var code = Guid.NewGuid().ToString("N");
    authCodes[code] = new AuthCodeData(username, codeChallenge, DateTime.UtcNow.AddMinutes(5));

  
    var redirectUrl = $"{redirectUri}?code={code}";
    if (!string.IsNullOrEmpty(state))
        redirectUrl += $"&state={state}";

    return Results.Redirect(redirectUrl);
});


static string Base64UrlEncode(byte[] input)
{
    return Convert.ToBase64String(input).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}

(string accessToken, string refreshToken) GenerateTokens(string username)
{
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var claims = new[] { new Claim(ClaimTypes.Name, username) };

    var token = new JwtSecurityToken(
        issuer: jwtIssuer,
        audience: jwtAudience,
        claims: claims,
        expires: DateTime.UtcNow.AddHours(1),
        signingCredentials: creds);

    var handler = new JwtSecurityTokenHandler();
    var accessToken = handler.WriteToken(token);
    var refreshToken = Guid.NewGuid().ToString("N");
    return (accessToken, refreshToken);
}

app.MapPost("/oauth/token", async (HttpContext context) =>
{
    var form = await context.Request.ReadFormAsync();
    var grantType = form["grant_type"].ToString();
    var code = form["code"].ToString();
    var codeVerifier = form["code_verifier"].ToString();
    var refreshToken = form["refresh_token"].ToString();

    if (grantType == "authorization_code")
    {
        // Code'u doğrula
        if (!authCodes.TryGetValue(code, out var codeData))
            return Results.Json(new { error = "invalid_grant" }, statusCode: 400);

        if (codeData.ExpiresAt < DateTime.UtcNow)
        {
            authCodes.Remove(code);
            return Results.Json(new { error = "invalid_grant" }, statusCode: 400);
        }

        // PKCE doğrula
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
        var computedChallenge = Base64UrlEncode(hash);

        if (computedChallenge != codeData.CodeChallenge)
            return Results.Json(new { error = "invalid_grant" }, statusCode: 400);

        authCodes.Remove(code); // Single use

        // JWT üret
        var (accessToken, newRefreshToken) = GenerateTokens(codeData.Username);

        return Results.Json(new
        {
            access_token = accessToken,
            refresh_token = newRefreshToken,
            token_type = "Bearer",
            expires_in = 3600
        });
    }

    if (grantType == "refresh_token")
    {
        var username = "test";//ValidateRefreshToken(refreshToken);
        if (username == null)
            return Results.Json(new { error = "invalid_grant" }, statusCode: 400);

        var (accessToken, newRefreshToken) = GenerateTokens(username);
        return Results.Json(new
        {
            access_token = accessToken,
            refresh_token = newRefreshToken,
            token_type = "Bearer",
            expires_in = 3600
        });
    }

    return Results.Json(new { error = "unsupported_grant_type" }, statusCode: 400);
});
app.MapGet("/", () => "hello world");
app.MapMcp("/mcp");
app.Run();
public record Sale(
    string Id,
    string Employee,
    DateOnly Date,
    decimal Amount)
{
    public static List<Sale> Sales = new List<Sale>
        {
            new("S001","Ali Yılmaz", new DateOnly(2026,2,10), 1200),
            new("S002","Ayşe Demir", new DateOnly(2026,2,11), 850),
            new("S003","Mehmet Kaya", new DateOnly(2026,2,12), 2100),
            new("S004","Fatma Şahin", new DateOnly(2026,2,13), 640),
            new("S005","Ali Yılmaz", new DateOnly(2026,2,14), 980),
            new("S006","Ayşe Demir", new DateOnly(2026,2,15), 1750),
            new("S007","Mehmet Kaya", new DateOnly(2026,2,16), 430),
            new("S008","Fatma Şahin", new DateOnly(2026,2,17), 2200),
            new("S009","Ali Yılmaz", new DateOnly(2026,2,18), 760),
            new("S010","Ayşe Demir", new DateOnly(2026,2,19), 950),

            new("S011","Mehmet Kaya", new DateOnly(2026,2,20), 1400),
            new("S012","Fatma Şahin", new DateOnly(2026,2,21), 880),
            new("S013","Ali Yılmaz", new DateOnly(2026,2,22), 1990),
            new("S014","Ayşe Demir", new DateOnly(2026,2,23), 1100),
            new("S015","Mehmet Kaya", new DateOnly(2026,2,24), 1700),
            new("S016","Fatma Şahin", new DateOnly(2026,2,25), 620),
            new("S017","Ali Yılmaz", new DateOnly(2026,2,26), 1340),
            new("S018","Ayşe Demir", new DateOnly(2026,2,27), 2700),
            new("S019","Mehmet Kaya", new DateOnly(2026,2,28), 540),
            new("S020","Fatma Şahin", new DateOnly(2026,3,1), 1500),

            new("S021","Ali Yılmaz", new DateOnly(2026,3,2), 910),
            new("S022","Ayşe Demir", new DateOnly(2026,3,3), 1880),
            new("S023","Mehmet Kaya", new DateOnly(2026,3,4), 760),
            new("S024","Fatma Şahin", new DateOnly(2026,3,5), 3200),
            new("S025","Ali Yılmaz", new DateOnly(2026,3,6), 1150),
            new("S026","Ayşe Demir", new DateOnly(2026,3,7), 1420),
            new("S027","Mehmet Kaya", new DateOnly(2026,3,8), 980),
            new("S028","Fatma Şahin", new DateOnly(2026,3,9), 2100),
            new("S029","Ali Yılmaz", new DateOnly(2026,3,10), 1760),
            new("S030","Ayşe Demir", new DateOnly(2026,3,11), 2300)
        };
}
public record AuthCodeData(string Username, string CodeChallenge, DateTime ExpiresAt);
public record Payment(
    string Id,
    string Company,
    string Reason,
    DateOnly DueDate,
    decimal Amount)
{
    public static List<Payment> Payments = new List<Payment>
{
    new("P001","EnerjiSA","Electricity Bill", new DateOnly(2026,03,01), 900),
    new("P002","Türk Telekom","Internet Service", new DateOnly(2026,03,02), 350),
    new("P003","ABC Plaza","Office Rent", new DateOnly(2026,03,03), 4500),
    new("P004","Microsoft","Azure Subscription", new DateOnly(2026,03,04), 1200),
    new("P005","Adobe","Creative Cloud License", new DateOnly(2026,03,05), 850),
    new("P006","Vodafone","Mobile Lines", new DateOnly(2026,03,06), 640),
    new("P007","Google","Workspace Subscription", new DateOnly(2026,03,07), 300),
    new("P008","Dell","Hardware Lease", new DateOnly(2026,03,08), 1750),
    new("P009","UPS","Logistics Service", new DateOnly(2026,03,09), 520),
    new("P010","Amazon","Cloud Services", new DateOnly(2026,03,10), 1600),

    new("P011","EnerjiSA","Electricity Bill", new DateOnly(2026,03,11), 910),
    new("P012","Türk Telekom","Internet Service", new DateOnly(2026,03,12), 350),
    new("P013","ABC Plaza","Office Rent", new DateOnly(2026,03,13), 4500),
    new("P014","Microsoft","Azure Subscription", new DateOnly(2026,03,14), 1180),
    new("P015","Adobe","Creative Cloud License", new DateOnly(2026,03,15), 860),
    new("P016","Vodafone","Mobile Lines", new DateOnly(2026,03,16), 620),
    new("P017","Google","Workspace Subscription", new DateOnly(2026,03,17), 300),
    new("P018","Dell","Hardware Lease", new DateOnly(2026,03,18), 1750),
    new("P019","UPS","Logistics Service", new DateOnly(2026,03,19), 540),
    new("P020","Amazon","Cloud Services", new DateOnly(2026,03,20), 1580),

    new("P021","EnerjiSA","Electricity Bill", new DateOnly(2026,03,21), 890),
    new("P022","Türk Telekom","Internet Service", new DateOnly(2026,03,22), 350),
    new("P023","ABC Plaza","Office Rent", new DateOnly(2026,03,23), 4500),
    new("P024","Microsoft","Azure Subscription", new DateOnly(2026,03,24), 1190),
    new("P025","Adobe","Creative Cloud License", new DateOnly(2026,03,25), 870),
    new("P026","Vodafone","Mobile Lines", new DateOnly(2026,03,26), 630),
    new("P027","Google","Workspace Subscription", new DateOnly(2026,03,27), 300),
    new("P028","Dell","Hardware Lease", new DateOnly(2026,03,28), 1750),
    new("P029","UPS","Logistics Service", new DateOnly(2026,03,29), 560),
    new("P030","Amazon","Cloud Services", new DateOnly(2026,03,30), 1620)
};
}