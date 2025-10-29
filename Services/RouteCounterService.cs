using System.Collections.Concurrent;
using UserManagementAPI.Services;

public class RouteCounterService : IRouteCounterService 
{ 
    private readonly ConcurrentDictionary<string, int> _counts = new(); 

    public void Increment(string path) 
    { 
        if (string.IsNullOrWhiteSpace(path)) 
            throw new ArgumentException("Path cannot be null or empty.", nameof(path)); 
        _counts.AddOrUpdate(path, 1, (key, current) => current + 1); 
    } 

    public int GetSpecificPathCount(string path) 
    { 
        if (string.IsNullOrWhiteSpace(path)) 
            throw new ArgumentException("Path cannot be null or empty.", nameof(path)); 
        if (!_counts.ContainsKey(path)) 
            return 0;
        _counts.TryGetValue(path, out int count); 
        return count; 
    }

    public Dictionary<string, int> GetCounts() 
    { 
        return new Dictionary<string, int>(_counts); 
    } 
} 