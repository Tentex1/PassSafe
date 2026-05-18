namespace PassSafe.Models
{
    using SQLite;

    /// <summary>
    /// Defines the <see cref="Password" />
    /// </summary>
    public class Password
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Title { get; set; }

        public string UserName { get; set; }

        public string EncryptedPassword { get; set; }
    }
}
