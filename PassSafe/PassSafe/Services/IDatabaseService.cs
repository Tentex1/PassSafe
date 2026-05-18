using PassSafe.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PassSafe.Services
{
    public interface IDatabaseService
    {
        Task InitializeDatabase(string key);
        Task<List<Password>> GetDatabase();
    }
}
