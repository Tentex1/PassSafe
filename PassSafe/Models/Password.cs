namespace PassSafe.Models
{
    using MauiIcons.Core;
    using MauiIcons.Material.Sharp;
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

        [ObservableProperty]
        private bool isFavorited = false;

        public string SecurityStatus { get; set; }

        public double SecurityProgress { get; set; }
    }
}
