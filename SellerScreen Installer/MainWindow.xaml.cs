using Microsoft.Win32;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace SellerScreen_Installer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            CreateUninstaller();
        }

        private void ChromeCloseBtn_Click(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ChromeMinimizeBtn_Click(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CreateUninstaller()
        {
            using (RegistryKey parent_ = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\", true))
            {
                if (parent_ == null)
                {
                    throw new Exception("Uninstall registry key not found.");
                }
                try
                {
                    RegistryKey key = null;

                    try
                    {
                        Guid UninstallGuid = new Guid("cc80b6f7-ad7e-4589-bc86-925c21280d22");
                        string guidText = UninstallGuid.ToString("B");
                        key = parent_.OpenSubKey(guidText, true) ??
                              parent_.CreateSubKey(guidText);

                        if (key == null)
                        {
                            throw new Exception(string.Format("Unable to create uninstaller '{0}\\{1}'", "text_test", guidText));
                        }

                        Assembly asm = GetType().Assembly;
                        Version v = asm.GetName().Version;
                        string exe = "\"" + asm.CodeBase.Substring(8).Replace("/", "\\\\") + "\"";

                        key.SetValue("DisplayName", "SellerScreen Pro");
                        key.SetValue("ApplicationVersion", v.ToString());
                        key.SetValue("Publisher", "T-App-Germany");
                        key.SetValue("DisplayIcon", exe);
                        key.SetValue("DisplayVersion", v.ToString(2));
                        key.SetValue("URLInfoAbout", "http://t-app-germany.com");
                        key.SetValue("Contact", "info@t-app-germany.com");
                        key.SetValue("InstallDate", DateTime.Now.ToString("yyyyMMdd"));
                        key.SetValue("UninstallString", exe + " /uninstallprompt");
                    }
                    finally
                    {
                        if (key != null)
                        {
                            key.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("An error occurred writing uninstall information to the registry.  The service is fully installed but can only be uninstalled manually through the command line.", ex);
                }
            }
        }
	}
}
