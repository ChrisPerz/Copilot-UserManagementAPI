namespace UserManagementAPI.Services
{
    public interface IRouteCounterService
    {
        void Increment(string path);
        int GetSpecificPathCount(string path);
        Dictionary<string, int> GetCounts();
    }
}
