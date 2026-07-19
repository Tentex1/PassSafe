using PassSafe.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PassSafe.Services
{
    public interface IDatabaseService
    {
        Task InitializeDatabaseAsync(string key);
        Task AddPasswordAsync(Password password);
        Task UpdatePasswordAsync(Password password);
        Task DeletePasswordAsync(int id);
        Task<List<Password>> GetDatabaseAsync();
        Task<List<Password>> GetFavoritesAsync();
    }
}
