using PassSafe.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace PassSafe.Helpers
{
    public class ViewModelLocator
    {
        public ShellViewModel ShellViewModel => App.Services.GetService<ShellViewModel>();
        public SetMasterPassViewModel SetMasterPassViewModel => App.Services.GetService<SetMasterPassViewModel>();
        public VaultViewModel VaultViewModel => App.Services.GetService<VaultViewModel>();
    }
}
