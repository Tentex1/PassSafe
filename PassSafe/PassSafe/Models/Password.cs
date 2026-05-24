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

        public string Icon { get; set; }

        public string SecurityStatus { get; set; }

        public double SecurityProgress { get; set; }
    }
}
