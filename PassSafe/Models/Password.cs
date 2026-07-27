namespace PassSafe.Models
{
    using CommunityToolkit.Mvvm.ComponentModel;
    using SQLite;

    /// <summary>
    /// Represents a saved password entry in the Vault. 
    /// Inherits from ObservableObject to allow inline UI updates (like toggling password visibility).
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

        // Ignored by SQLite. Used strictly for UI to toggle password visibility without saving to DB.
        [property: Ignore]
        [ObservableProperty]
        private bool isPasswordVisible;

        // Ignored by SQLite. Used strictly for UI to display asterisks or the decrypted password.
        [property: Ignore]
        [ObservableProperty]
        private string displayPassword = "••••••••";
    }
}