namespace PassSafe.Views
{
    public partial class MainPage : TabbedPage
    {
        // Kilidimiz burada duracak
        private bool _isLoadedBefore = false;

        public MainPage(MainViewModel vm)
        {
            InitializeComponent();

            Loaded += async (s, e) =>
            {
                // EĞER DAHA ÖNCE ÇALIŞTIYSA İÇERİ GİRME, ÇIK!
                if (_isLoadedBefore) return;

                // İlk girişte kilidi true yapıyoruz ki arkadan gelen istekler patlasın
                _isLoadedBefore = true;

                // Artık güvenle sadece 1 kere çalışacak
                await vm.InitializeCommand.ExecuteAsync(null);
            };
        }
    }
}