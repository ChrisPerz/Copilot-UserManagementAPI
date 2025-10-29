using UserManagementAPI.Services;
public class RouteCounterMiddleware 
{ 
    private readonly RequestDelegate _next; 

    public RouteCounterMiddleware(RequestDelegate next) 
    { 
        _next = next; 
    } 

    public async Task Invoke(HttpContext context, IRouteCounterService counterService) 
    { 
        counterService.Increment(context.Request.Path); 
        await _next(context); 
    } 
} 