namespace PATHLY_API.Services
{
    public interface IRoadService
    {
        Task<string> SnapToRoadsAsync(string path);
    }
}
