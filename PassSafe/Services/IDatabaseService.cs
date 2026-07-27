namespace PassSafe.Services
{
    using PassSafe.Models;
    using System.Collections.Generic;

    /// <summary>
    /// Defines the <see cref="IDatabaseService" />
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
