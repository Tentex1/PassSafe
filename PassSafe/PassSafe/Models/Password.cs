using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace PassSafe.Models
{
    public class Password
    {
        public class PasswordEntry
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }
            public string Title { get; set; } 
            public string UserName { get; set; }
            public string EncryptedPassword { get; set; } 
        }
    }
}
