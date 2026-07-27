namespace PassSafe
{
    using PassSafe.ViewModels;

    public partial class MainWindow : Window
    {
        private DateTime? _lastDeactivatedTime;
        private MainViewModel _vm;

        public static bool IsAuthenticating { get; set; } = false;

        public MainWindow() => InitializeComponent();

        public MainWindow(MainViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
        }

        public MainWindow(Page page) : base(page) => InitializeComponent();

        private void Window_Deactivated(object sender, EventArgs e)
        {
            if (IsAuthenticating)
                return;

            _lastDeactivatedTime = DateTime.Now;
        }

        private async void Window_Activated(object sender, EventArgs e)
        {
            if (IsAuthenticating || !_lastDeactivatedTime.HasValue)
                return;

            DateTime lastDeactivated = _lastDeactivatedTime.Value;
            _lastDeactivatedTime = null;

            var autoLockSetting = Preferences.Get("AutoLockTime", "5");

            if (!int.TryParse(autoLockSetting, out int lockMinutes))
            {
                lockMinutes = 5;
            }

            if (lockMinutes < 0)
            {
                return;
            }

            var timePassed = DateTime.Now - lastDeactivated;

            if (lockMinutes == 0 || timePassed.TotalSeconds >= (lockMinutes * 60))
            {
                if (_vm != null)
                {
                    await _vm.InitializeCommand.ExecuteAsync(null);
                }
            }
        }
    }
}