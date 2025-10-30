using UserManagementAPI.Models;
using UserManagementAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<IRouteCounterService, RouteCounterService>(); 

var app = builder.Build();

// Error Handler Middleware
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch(Exception ex)
    {
        Console.WriteLine("Internal server error ->" + ex);
    }

});

// Auth middleware
app.Use(async (context, next) =>
{
    //throw new Exception("Forced error in middleware");
    var token = context.Request.Headers["Authorization"].ToString();

    if(string.IsNullOrEmpty(token) || !token.StartsWith("Bearer "))
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsync("Unauthorized");
        return;
    }

    await next();
});

// Middleware to log the request path and increment the count
app.UseMiddleware<RouteCounterMiddleware>();

//Request Logger Middleware
app.Use(async (context, next) =>
{
    await next();
    RequestLog log = new RequestLog()
    {
        path = context.Request.Path,
        statusResponse = context.Response.StatusCode,
        httpMethod = context.Request.Method,
        timestamp = DateTime.UtcNow
    };
    Console.WriteLine($"[{log.timestamp}] The path: {log.path} was called with a method {log.httpMethod} \n Status code: {log.statusResponse}");
});

// app.UseRouting();  --> I asked COPILOT why this is not needed here, and it said that in ASP.NET Core 6 and later, routing is automatically configured when using minimal APIs or controllers.

app.MapGet("/", () => "This is the root path");

app.MapControllers();

app.Run();