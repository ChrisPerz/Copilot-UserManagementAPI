using UserManagementAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<IRouteCounterService, RouteCounterService>(); 

var app = builder.Build();

// Middleware to log the request path and increment the count
app.UseMiddleware<RouteCounterMiddleware>(); 

// app.UseRouting();  --> I asked COPILOT why this is not needed here, and it said that in ASP.NET Core 6 and later, routing is automatically configured when using minimal APIs or controllers.

app.MapGet("/", () => "This is the root path");

app.MapControllers();

app.Run();