namespace PassSafe.Models
{
    using CommunityToolkit.Mvvm.ComponentModel;
    using SQLite;

    /// <summary>
    /// Defines the <see cref="Password" />
    /// </summary>
    public partial class Password : ObservableObject
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Title { get; set; }

        public string UserName { get; set; }

        public string EncryptedPassword { get; set; }

        public string Icon { get; set; }

        public double SecurityProgress { get; set; }

        public string SecurityStatus { get; set; }

        public bool IsFavorited { get; set; }

        public string Category { get; set; }

        [property: Ignore]
        [ObservableProperty]
        private bool isPasswordVisible;

        [property: Ignore]
        [ObservableProperty]
        private string displayPassword = "••••••••";
    }
}
