namespace PassSafe.Services
{
    using PassSafe.Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Handles all SQLite database operations including initialization, reading, and writing.
    /// </summary>
    public interface IDatabaseService
    {
        Task<bool> InitializeDatabaseAsync(string key);
        Task AddPasswordAsync(Password password);
        Task UpdatePasswordAsync(Password password);
        Task DeletePasswordAsync(int id);
        Task<List<Password>> GetDatabaseAsync();
    }
}