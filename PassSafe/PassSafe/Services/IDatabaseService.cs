using PassSafe.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PassSafe.Services
{
    public interface IDatabaseService
    {
        Task InitializeDatabase(string key);
        Task AddPassword(Password password);
        Task DeletePassword(int id);
        Task<List<Password>> GetDatabase();
    }
}
