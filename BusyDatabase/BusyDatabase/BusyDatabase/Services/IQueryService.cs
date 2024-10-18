
namespace BusyDatabase.Services
{
    public interface IQueryService
    {
        Task<string> GetAsync(string key);
    }
}