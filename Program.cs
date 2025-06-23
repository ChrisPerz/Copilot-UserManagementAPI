var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

var app = builder.Build();

// app.UseRouting();

app.MapGet("/", () => "This is the root path");

app.MapControllers();

app.Run();