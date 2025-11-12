using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserManagementAPI.Models;
using UserManagementAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<IRouteCounterService, RouteCounterService>();

// Add Identity services
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDBContext>()
    .AddDefaultTokenProviders();

builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseInMemoryDatabase("UserManagementDB"));

// Add authorization policies (without requiring authentication)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireITDepartment", policy =>
        policy.RequireClaim("Department", "IT"));
});

var app = builder.Build();

// Add Role and claim auth
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    if (!await roleManager.RoleExistsAsync("Admin"))
        await roleManager.CreateAsync(new IdentityRole("Admin"));

    if (!await roleManager.RoleExistsAsync("User"))
        await roleManager.CreateAsync(new IdentityRole("User"));

    var claim = new Claim("Permission", "CanAccessAdminPanel");
    var adminRole = await roleManager.FindByNameAsync("Admin");

    if (adminRole != null && !(await roleManager.GetClaimsAsync(adminRole))
            .Any(c => c.Type == claim.Type && c.Value == claim.Value))
    {
        await roleManager.AddClaimAsync(adminRole, claim);
    }
}


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