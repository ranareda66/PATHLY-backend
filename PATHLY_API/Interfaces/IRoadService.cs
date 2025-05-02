namespace PATHLY_API.Interfaces
{
    public interface IRoadService
    {
        Task<string> SnapToRoadsAsync(string path);
    }
}
