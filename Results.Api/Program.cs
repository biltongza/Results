using Results.Apis;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
var app = builder.Build();
app.MapOpenApi();

app
    .MapGroup("api")
    .MapResultApi();

app.Run();
