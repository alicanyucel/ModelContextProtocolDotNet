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