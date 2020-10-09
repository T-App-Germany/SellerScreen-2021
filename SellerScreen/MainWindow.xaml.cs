using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Management;
using System.Reflection;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml;
using Xceed.Wpf.Toolkit;
using MessageBox = System.Windows.MessageBox;
using Path = System.IO.Path;

namespace SellerScreen
{
    public partial class MainWindow : Window
    {
        #region Assemblyattributaccessoren
        private static string GetAssemblyTitle()
        {
            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
            if (attributes.Length > 0)
            {
                AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                if (titleAttribute.Title != "")
                {
                    return titleAttribute.Title;
                }
            }
            return Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
        }

        private static string GetAssemblyVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private static string GetAssemblyDescription()
        {
            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
            if (attributes.Length == 0)
            {
                return "";
            }
            return ((AssemblyDescriptionAttribute)attributes[0]).Description;
        }

        private static string GetAssemblyProduct()
        {
            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
            if (attributes.Length == 0)
            {
                return "";
            }
            return ((AssemblyProductAttribute)attributes[0]).Product;
        }

        private static string GetAssemblyCopyright()
        {
            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            if (attributes.Length == 0)
            {
                return "";
            }
            return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
        }

        private static string GetAssemblyCompany()
        {
            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
            if (attributes.Length == 0)
            {
                return "";
            }
            return ((AssemblyCompanyAttribute)attributes[0]).Company;
        }
        #endregion

        #region Variablen
        private enum WindowsTheme
        {
            Light,
            Dark
        }

        private DispatcherTimer MessageTimer { get; set; } = new DispatcherTimer();
        private DispatcherTimer StaticsDaySelectionTimer{ get; set; } = new DispatcherTimer();
        private DispatcherTimer AniTimer { get; set; } = new DispatcherTimer();
        private readonly LogWindow logWindow = new LogWindow();
        private readonly PathName pathN = new PathName();

        private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        private const string RegistryValueName = "AppsUseLightTheme";

        private bool loadTotalStaticsError = false;
        private bool loadDayStaticsError = false;
        private DateTime lastPayDate = DateTime.Today.AddDays(-5);
        private bool[] displayLogTypes = new bool[5];
        private bool commissioningMode = false;
        private string AppTheme = "System";
        private readonly Random rnd = new Random();
        private int cloudCount = -1;
        private int page = 0;

        private short StorageSlots = 0;
        private readonly int StorageLimitedNumber = 10;
        private bool[] StorageSlotStatus = Array.Empty<bool>();
        private short[] StorageSlotNumber = Array.Empty<short>();
        private double[] StorageSlotPrice = Array.Empty<double>();
        private string[] StorageSlotName = Array.Empty<string>();
        private short StorageSelectedCount = 0;
        private bool[] StorageSelectedArray = Array.Empty<bool>();

        private short selectedItemsInt = 0;
        private short[] ShopSlotSelectedNumber = Array.Empty<short>();
        private string shopTask = "";
        private double mainPrice = 0;

        private string[] SoldSlotName = Array.Empty<string>();
        private short[] SoldSlotNumber = Array.Empty<short>();
        private double[] SoldSlotCash = Array.Empty<double>();
        private double[] SoldSlotSinglePrice = Array.Empty<double>();
        private int LostProducts;
        private double LostCash;
        private TimeSpan[] pcTime = new TimeSpan[2];
        private int[] pcUsers = new int[2];
        private string[] userList;

        private DateTime startDate;
        private int totalCustomers;
        private int totalSoldProducts;
        private double totalGottenCash;
        private int totalLostProducts;
        private double totalLostCash;
        private readonly TimeSpan[] totalPcTime = new TimeSpan[2];
        private readonly int[] totalPcUsers = new int[2];

        private readonly string[] mostSoldProductsName = new string[5];
        private readonly string[] highestEarningsProductsName = new string[5];
        private readonly int[] mostSoldProductsNumber = new int[5];
        private readonly double[] highestEarningsProductsNumber = new double[5];
        private double[] mostSoldProductsSinglePrice = new double[5];
        private double[] highestEarningsProductsSinglePrice = new double[5];

        private string[] productsNameList = Array.Empty<string>();
        private int[] productsNumberList = Array.Empty<int>();
        private double[] productsCashList = Array.Empty<double>();
        private double[] productsSinglePriceList = Array.Empty<double>();

        //private string[] planedUserNameList = Array.Empty<string>();
        //private int[] plannedUserStartTimeList = Array.Empty<int>();
        //private double[] plannedUserDurationList = Array.Empty<double>();
        #endregion

        #region Window
        public MainWindow()
        {
            InitializeComponent();
            AssemblyLbl.Content = $"{GetAssemblyProduct()}\t\tVersion {GetAssemblyVersion()}\t\t{GetAssemblyCopyright()}\t\t{GetAssemblyCompany()}";
            Title = GetAssemblyTitle();
            Page1HeaderLbl.Content = "Willkommen bei " + GetAssemblyTitle();

            MessageTimer.Tick += new EventHandler(MessageTimer_Tick);
            MessageTimer.Interval = new TimeSpan(0, 0, 0, 10);

            StaticsDaySelectionTimer.Tick += new EventHandler(StaticsDaySelectionTimer_Tick);
            StaticsDaySelectionTimer.Interval = new TimeSpan(0, 0, 0, 1, 5);

            AniTimer.Tick += new EventHandler(AniTimer_Tick);
            AniTimer.Interval = new TimeSpan(0, 0, 0, 2);

            Directory.CreateDirectory(Path.GetDirectoryName(pathN.settingsFile));
            Directory.CreateDirectory(Path.GetDirectoryName(pathN.staticsFile));
            Directory.CreateDirectory(Path.GetDirectoryName(pathN.logFile));
            Directory.CreateDirectory(Path.GetDirectoryName(pathN.graphicsFile));

            displayLogTypes[0] = true;
            displayLogTypes[2] = true;
            displayLogTypes[4] = true;
            logWindow.displayLogTypes = displayLogTypes;

            if (!File.Exists($"{pathN.settingsFile}Settings.xml") && !File.Exists($"{pathN.settingsFile}Storage.xml") && !File.Exists($"{pathN.settingsFile}TotalStatics.xml"))
            {
                commissioningMode = true;
                CommissioningMode();
            }

            if (commissioningMode == false)
            {
                Reload();
            }

            PageChange(1);
            SetWindowTheme();
            CloseShop();
            WatchTheme();
            RefreshMaximizeRestoreButton();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AniTimer.Start();
            StaticsDayYearUpDown.Value = DateTime.Now.Year;
            StaticsDayYearUpDown.Maximum = DateTime.Now.Year;
            StaticsDayMonthUpDown.Value = DateTime.Now.Month;
            logWindow.NewLog($"Main_Window loaded", 0);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (MessageBox.Show($"Möchten Sie {Title} wirklich schließen?", "Anwendung schließen", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                logWindow.NewLog($"Main_Window closed", 0);
                logWindow.NewLog($"Application is shutting down", 0);
                Application.Current.Shutdown();
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void ChromeMinimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            WindowState = System.Windows.WindowState.Minimized;
        }

        private void ChromeMaximizeRestoreBtn_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Maximized)
            {
                WindowState = System.Windows.WindowState.Normal;
            }
            else
            {
                WindowState = System.Windows.WindowState.Maximized;
            }
        }

        private void ChromeCloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void RefreshMaximizeRestoreButton()
        {
            if (WindowState == System.Windows.WindowState.Maximized)
            {
                ChromeMaximizeBtn.Visibility = Visibility.Collapsed;
                ChromeRestoreBtn.Visibility = Visibility.Visible;
            }
            else
            {
                ChromeMaximizeBtn.Visibility = Visibility.Visible;
                ChromeRestoreBtn.Visibility = Visibility.Collapsed;
            }
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            RefreshMaximizeRestoreButton();
        }
        #endregion

        #region Timer
        private void MessageTimer_Tick(object sender, EventArgs e)
        {

        }

        private void AniTimer_Tick(object sender, EventArgs e)
        {
            cloudCount++;

            if (cloudCount <= 10)
            {
                try
                {
                    int imgHeight = rnd.Next(10, 50);
                    int positionHeight = rnd.Next(0, int.Parse(Math.Round(WindowHeight.Height).ToString()) - imgHeight);
                    int speed = rnd.Next(2, 15);
                    int cloud = rnd.Next(1, 4);

                    Image img = new Image
                    {
                        Height = imgHeight,
                        Width = imgHeight * 3 / 2,
                        Source = new BitmapImage(new Uri($@"Resources/wolke{cloud}.png", UriKind.Relative)),
                        Name = $"cloud{cloudCount}",
                        Tag = cloud.ToString(),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top
                    };

                    RegisterName(img.Name, img);
                    SideBar.Children.Add(img);

                    ThicknessAnimation ani = new ThicknessAnimation
                    {
                        From = new Thickness(70, positionHeight, 0, 0),
                        To = new Thickness(-80, positionHeight, 0, 0),
                        Duration = TimeSpan.FromSeconds(speed),
                    };

                    img.BeginAnimation(MarginProperty, ani);
                }
                catch (Exception)
                {

                }
            }
            else
            {
                AniTimer.Stop();

                try
                {
                    DoubleAnimation ani = new DoubleAnimation()
                    {
                        From = 1,
                        To = 0,
                        Duration = TimeSpan.FromSeconds(2),
                        EasingFunction = new QuarticEase()
                    };
                    ani.Completed += new EventHandler(FadeAni_Completed);

                    for (int i = 0; i < cloudCount; i++)
                    {
                        Image img = (Image)FindName($"cloud{i}");
                        img.BeginAnimation(OpacityProperty, ani);
                        UnregisterName($"cloud{i}");
                    }
                }
                catch (Exception)
                {

                }

                cloudCount = -1;
            }
        }

        private void FadeAni_Completed(object sender, EventArgs e)
        {
            AniTimer.Start();
            SideBar.Children.Clear();
        }
        #endregion

        #region Funktionen
        private void CommissioningMode()
        {
            CommissioningModePanel.Visibility = Visibility.Visible;
            TopBar.IsEnabled = false;
        }

        private void PageChange(int newPage)
        {
            bool error = false;
            page = newPage;

            DoubleAnimation aniOUT = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromSeconds(1),
                EasingFunction = new PowerEase()
            };
            aniOUT.Completed += new EventHandler(PageFadeOut_Comleted);

            DoubleAnimation aniIN = new DoubleAnimation
            {
                To = 1,
                Duration = TimeSpan.FromSeconds(1),
                EasingFunction = new PowerEase()
            };

            switch (newPage)
            {
                case 1:
                    Page1Grid.BeginAnimation(OpacityProperty, aniIN);
                    Page2Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page3Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page4Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page5Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page6Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page7Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page8Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page9Grid.BeginAnimation(OpacityProperty, aniOUT);

                    Page1Grid.Visibility = Visibility.Visible;
                    break;
                case 2:
                    Page1Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page2Grid.BeginAnimation(OpacityProperty, aniIN);
                    Page3Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page4Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page5Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page6Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page7Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page8Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page9Grid.BeginAnimation(OpacityProperty, aniOUT);

                    Page2Grid.Visibility = Visibility.Visible;
                    break;
                case 3:
                    Page1Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page2Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page3Grid.BeginAnimation(OpacityProperty, aniIN);
                    Page4Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page5Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page6Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page7Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page8Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page9Grid.BeginAnimation(OpacityProperty, aniOUT);

                    Page3Grid.Visibility = Visibility.Visible;
                    break;
                case 4:
                    Page1Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page2Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page3Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page4Grid.BeginAnimation(OpacityProperty, aniIN);
                    Page5Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page6Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page7Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page8Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page9Grid.BeginAnimation(OpacityProperty, aniOUT);

                    Page4Grid.Visibility = Visibility.Visible;
                    break;
                case 5:
                    Page1Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page2Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page3Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page4Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page5Grid.BeginAnimation(OpacityProperty, aniIN);
                    Page6Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page7Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page8Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page9Grid.BeginAnimation(OpacityProperty, aniOUT);

                    Page5Grid.Visibility = Visibility.Visible;
                    break;
                case 6:
                    Page1Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page2Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page3Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page4Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page5Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page6Grid.BeginAnimation(OpacityProperty, aniIN);
                    Page7Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page8Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page9Grid.BeginAnimation(OpacityProperty, aniOUT);

                    Page6Grid.Visibility = Visibility.Visible;
                    break;
                case 7:
                    Page1Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page2Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page3Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page4Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page5Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page6Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page7Grid.BeginAnimation(OpacityProperty, aniIN);
                    Page8Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page9Grid.BeginAnimation(OpacityProperty, aniOUT);

                    Page7Grid.Visibility = Visibility.Visible;
                    break;
                case 8:
                    Page1Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page2Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page3Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page4Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page5Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page6Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page7Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page8Grid.BeginAnimation(OpacityProperty, aniIN);
                    Page9Grid.BeginAnimation(OpacityProperty, aniOUT);

                    Page8Grid.Visibility = Visibility.Visible;
                    break;
                case 9:
                    Page1Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page2Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page3Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page4Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page5Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page6Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page7Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page8Grid.BeginAnimation(OpacityProperty, aniOUT);
                    Page9Grid.BeginAnimation(OpacityProperty, aniIN);

                    Page9Grid.Visibility = Visibility.Visible;
                    break;
                default:
                    MessageBox.Show($"Die Seite {newPage} konnte nicht gefunden werden", "Menu Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                    logWindow.NewLog($"Changing Page to PageID {newPage} failed! Internal ERROR", 2);
                    error = true;
                    break;
            }

            if (error != true)
            {
                logWindow.NewLog($"Changing Page to PageID {newPage} successful", 1);
            }
        }

        private void PageFadeOut_Comleted(object sender, EventArgs e)
        {
            switch (page)
            {
                case 1:
                    Page2Grid.Visibility = Visibility.Collapsed;
                    Page3Grid.Visibility = Visibility.Collapsed;
                    Page4Grid.Visibility = Visibility.Collapsed;
                    Page5Grid.Visibility = Visibility.Collapsed;
                    Page6Grid.Visibility = Visibility.Collapsed;
                    Page7Grid.Visibility = Visibility.Collapsed;
                    Page8Grid.Visibility = Visibility.Collapsed;
                    Page9Grid.Visibility = Visibility.Collapsed;
                    break;
                case 2:
                    Page1Grid.Visibility = Visibility.Collapsed;
                    Page3Grid.Visibility = Visibility.Collapsed;
                    Page4Grid.Visibility = Visibility.Collapsed;
                    Page5Grid.Visibility = Visibility.Collapsed;
                    Page6Grid.Visibility = Visibility.Collapsed;
                    Page7Grid.Visibility = Visibility.Collapsed;
                    Page8Grid.Visibility = Visibility.Collapsed;
                    Page9Grid.Visibility = Visibility.Collapsed;
                    break;
                case 3:
                    Page1Grid.Visibility = Visibility.Collapsed;
                    Page2Grid.Visibility = Visibility.Collapsed;
                    Page4Grid.Visibility = Visibility.Collapsed;
                    Page5Grid.Visibility = Visibility.Collapsed;
                    Page6Grid.Visibility = Visibility.Collapsed;
                    Page7Grid.Visibility = Visibility.Collapsed;
                    Page8Grid.Visibility = Visibility.Collapsed;
                    Page9Grid.Visibility = Visibility.Collapsed;
                    break;
                case 4:
                    Page1Grid.Visibility = Visibility.Collapsed;
                    Page2Grid.Visibility = Visibility.Collapsed;
                    Page3Grid.Visibility = Visibility.Collapsed;
                    Page5Grid.Visibility = Visibility.Collapsed;
                    Page6Grid.Visibility = Visibility.Collapsed;
                    Page7Grid.Visibility = Visibility.Collapsed;
                    Page8Grid.Visibility = Visibility.Collapsed;
                    Page9Grid.Visibility = Visibility.Collapsed;
                    break;
                case 5:
                    Page1Grid.Visibility = Visibility.Collapsed;
                    Page2Grid.Visibility = Visibility.Collapsed;
                    Page3Grid.Visibility = Visibility.Collapsed;
                    Page4Grid.Visibility = Visibility.Collapsed;
                    Page6Grid.Visibility = Visibility.Collapsed;
                    Page7Grid.Visibility = Visibility.Collapsed;
                    Page8Grid.Visibility = Visibility.Collapsed;
                    Page9Grid.Visibility = Visibility.Collapsed;
                    break;
                case 6:
                    Page1Grid.Visibility = Visibility.Collapsed;
                    Page2Grid.Visibility = Visibility.Collapsed;
                    Page3Grid.Visibility = Visibility.Collapsed;
                    Page4Grid.Visibility = Visibility.Collapsed;
                    Page5Grid.Visibility = Visibility.Collapsed;
                    Page7Grid.Visibility = Visibility.Collapsed;
                    Page8Grid.Visibility = Visibility.Collapsed;
                    Page9Grid.Visibility = Visibility.Collapsed;
                    break;
                case 7:
                    Page1Grid.Visibility = Visibility.Collapsed;
                    Page2Grid.Visibility = Visibility.Collapsed;
                    Page3Grid.Visibility = Visibility.Collapsed;
                    Page4Grid.Visibility = Visibility.Collapsed;
                    Page5Grid.Visibility = Visibility.Collapsed;
                    Page6Grid.Visibility = Visibility.Collapsed;
                    Page8Grid.Visibility = Visibility.Collapsed;
                    Page9Grid.Visibility = Visibility.Collapsed;
                    break;
                case 8:
                    Page1Grid.Visibility = Visibility.Collapsed;
                    Page2Grid.Visibility = Visibility.Collapsed;
                    Page3Grid.Visibility = Visibility.Collapsed;
                    Page4Grid.Visibility = Visibility.Collapsed;
                    Page5Grid.Visibility = Visibility.Collapsed;
                    Page6Grid.Visibility = Visibility.Collapsed;
                    Page7Grid.Visibility = Visibility.Collapsed;
                    Page9Grid.Visibility = Visibility.Collapsed;
                    break;
                case 9:
                    Page1Grid.Visibility = Visibility.Collapsed;
                    Page2Grid.Visibility = Visibility.Collapsed;
                    Page3Grid.Visibility = Visibility.Collapsed;
                    Page4Grid.Visibility = Visibility.Collapsed;
                    Page5Grid.Visibility = Visibility.Collapsed;
                    Page6Grid.Visibility = Visibility.Collapsed;
                    Page7Grid.Visibility = Visibility.Collapsed;
                    Page8Grid.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void Reload()
        {
            MainProgBarShow();
            LoadSettings();
            LoadStorage();
            LoadTotalStatics();
            LoadDayStatics(DateTime.Now.Date);
            BuildSettings();
            BuildStorage(GetWindowsTheme());
            BuildShop(GetWindowsTheme(), "storage");
            BuildTotalStatics();
            BuildDayStatics();
            MainProgBarHide();
        }

        private void MainProgBarShow()
        {
            DoubleAnimation ani = new DoubleAnimation
            {
                From = 0,
                To = 5,
                Duration = TimeSpan.FromSeconds(1),
                EasingFunction = new QuarticEase()
            };

            MainProgBar.BeginAnimation(HeightProperty, ani);
        }

        private void MainProgBarHide()
        {
            DoubleAnimation ani = new DoubleAnimation
            {
                From = 5,
                To = 0,
                Duration = TimeSpan.FromSeconds(1),
                EasingFunction = new QuarticEase()
            };

            MainProgBar.BeginAnimation(HeightProperty, ani);
        }

        private void WatchTheme()
        {
            var currentUser = WindowsIdentity.GetCurrent();
            string query = string.Format(CultureInfo.InvariantCulture, @"SELECT * FROM RegistryValueChangeEvent WHERE Hive = 'HKEY_USERS' AND KeyPath = '{0}\\{1}' AND ValueName = '{2}'",
                currentUser.User.Value, RegistryKeyPath.Replace(@"\", @"\\"), RegistryValueName);

            try
            {
                var watcher = new ManagementEventWatcher(query);
                watcher.EventArrived += (sender, args) =>
                {
                    WindowsTheme newWindowsTheme = GetWindowsTheme();
                    // React to new theme
                    if (AppTheme == "System")
                    {
                        MessageBox.Show("Das System-App-Thema wurde geändert. Um dieses anzuwenden, müssen Sie in den Einstellungen das Thema manuell aktualisieren.", "Windows Thema", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                };

                // Start listening for events
                watcher.Start();
            }
            catch (Exception ex)
            {
                logWindow.NewLog($"Application is not able to read WindowsTheme! {ex.Message}", 2);
            }
        }

        private static WindowsTheme GetWindowsTheme()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath))
            {
                object registryValueObject = key?.GetValue(RegistryValueName);
                if (registryValueObject == null)
                {
                    return WindowsTheme.Light;
                }

                int registryValue = (int)registryValueObject;

                return registryValue > 0 ? WindowsTheme.Light : WindowsTheme.Dark;
            }
        }

        private void SetWindowTheme()
        {
            if (AppTheme == "System")
            {
                WindowsTheme initialTheme = GetWindowsTheme();
                if (initialTheme == WindowsTheme.Dark)
                    ApplyDarkTheme();
                else if (initialTheme == WindowsTheme.Light)
                    ApplyLightTheme();
            }
            else if (AppTheme == "Light")
                ApplyLightTheme();
            else if (AppTheme == "Dark")
                ApplyDarkTheme();
        }

        private void ApplyLightTheme()
        {
            SeperatorColor.Background = (Brush)new BrushConverter().ConvertFrom("#FFA0A0A0");
            ChromeBtnColor.Background = (Brush)new BrushConverter().ConvertFrom("#FF212121");
        }

        private void ApplyDarkTheme()
        {
            TextFontColor.Background = (Brush)new BrushConverter().ConvertFrom("#FFFFFFFF");
            SideBarsColor.Background = (Brush)new BrushConverter().ConvertFrom("#48484A");
            PageBackgroudColor.Background = (Brush)new BrushConverter().ConvertFrom("#FF323232");
            SeperatorColor.Background = (Brush)new BrushConverter().ConvertFrom("#FFFFFFFF");
            ChromeBtnColor.Background = (Brush)new BrushConverter().ConvertFrom("#FFF6F6F6");
        }

        private void SlotItemMouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is StackPanel scp)
            {
                ThicknessAnimation ani = new ThicknessAnimation
                {
                    To = new Thickness(0),
                    Duration = TimeSpan.FromMilliseconds(500),
                    EasingFunction = new QuadraticEase(),
                };
                scp.BeginAnimation(MarginProperty, ani);
            }
        }

        private void SlotItemMouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is StackPanel scp)
            {
                ThicknessAnimation ani = new ThicknessAnimation
                {
                    To = new Thickness(15, 0, 0, 0),
                    Duration = TimeSpan.FromMilliseconds(500),
                    EasingFunction = new QuadraticEase()
                };
                scp.BeginAnimation(MarginProperty, ani);
            }
        }
        #endregion

        #region Menu
        private void MessageListClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void OpenLog(object sender, RoutedEventArgs e)
        {
            logWindow.Show();
            logWindow.ShowInTaskbar = true;
            logWindow.ShowActivated = true;
            logWindow.WindowState = System.Windows.WindowState.Normal;
            logWindow.ScrollView.ScrollToBottom();
        }

        private void MainMenuPageImg_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Image img)
            {
                int tag = int.Parse(img.Tag.ToString());
                switch (tag)
                {
                    case 1:
                        MainMenuTitleLbl.Content = "Startseite";
                        break;
                    case 2:
                        MainMenuTitleLbl.Content = "Einstellungen";
                        break;
                    case 3:
                        MainMenuTitleLbl.Content = "Hilfe & Konatkt";
                        break;
                    case 4:
                        MainMenuTitleLbl.Content = "Kasse";
                        break;
                    case 5:
                        MainMenuTitleLbl.Content = "Schicht Manager";
                        break;
                    case 6:
                        MainMenuTitleLbl.Content = "Statistiken";
                        break;
                    case 7:
                        MainMenuTitleLbl.Content = "Benutzer Manager";
                        break;
                    case 8:
                        MainMenuTitleLbl.Content = "Lager";
                        break;
                    case 9:
                        MainMenuTitleLbl.Content = "Dateiexplorer";
                        break;
                }
            }
        }

        private void MainMenuPageImg_MouseLeave(object sender, MouseEventArgs e)
        {
            MainMenuTitleLbl.Content = "Hauptmenü";
        }

        private void MainMenuPageImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Image img)
            {
                int tag = int.Parse(img.Tag.ToString());
                switch (tag)
                {
                    case 1:
                        PageChange(1);
                        break;
                    case 2:
                        PageChange(2);
                        break;
                    case 3:
                        PageChange(3);
                        break;
                    case 4:
                        PageChange(4);
                        break;
                    case 5:
                        PageChange(5);
                        break;
                    case 6:
                        PageChange(6);
                        break;
                    case 7:
                        PageChange(7);
                        break;
                    case 8:
                        PageChange(8);
                        break;
                    case 9:
                        PageChange(9);
                        break;
                }
            }
        }
        #endregion

        #region Shop
        private void ShopSlotRestBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                int tag = int.Parse(btn.Tag.ToString());
                if (ShopSlotSelectedNumber[tag] != StorageSlotNumber[tag])
                {
                    ShopSlotSelectedNumber[tag] = StorageSlotNumber[tag];
                    IntegerUpDown iup = (IntegerUpDown)FindName($"ShopSlot{tag}SellNumberUpDown");
                    iup.Value = ShopSlotSelectedNumber[tag];
                }
                GetMainPrice();
            }
        }

        private void ShopSlotMinusBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                int tag = int.Parse(btn.Tag.ToString());
                if (ShopSlotSelectedNumber[tag] != 0)
                {
                    ShopSlotSelectedNumber[tag] -= 1;
                    if (ShopSlotSelectedNumber[tag] < 0)
                    {
                        ShopSlotSelectedNumber[tag] = 0;
                    }

                    IntegerUpDown iup = (IntegerUpDown)FindName($"ShopSlot{tag}SellNumberUpDown");
                    iup.Value = ShopSlotSelectedNumber[tag];
                }
                GetMainPrice();
            }
        }

        private void ShopSlotPlusBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                int tag = int.Parse(btn.Tag.ToString());
                short maxCount;
                if (shopTask == "sell")
                {
                    maxCount = StorageSlotNumber[tag];
                }
                else
                {
                    maxCount = SoldSlotNumber[tag];
                }

                if (ShopSlotSelectedNumber[tag] < maxCount)
                {
                    ShopSlotSelectedNumber[tag] += 1;
                    if (ShopSlotSelectedNumber[tag] > maxCount)
                    {
                        ShopSlotSelectedNumber[tag] = maxCount;
                    }
                    IntegerUpDown iup = (IntegerUpDown)FindName($"ShopSlot{tag}SellNumberUpDown");
                    iup.Value = ShopSlotSelectedNumber[tag];
                }
                GetMainPrice();
            }
        }

        private void ShopSlotSellNumberUpDown_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (sender is IntegerUpDown iup && iup.Value != null)
            {
                short tag = short.Parse(iup.Tag.ToString());
                short maxCount;
                if (shopTask == "sell")
                {
                    maxCount = StorageSlotNumber[tag];
                }
                else
                {
                    maxCount = SoldSlotNumber[tag];
                }

                if (ShopSlotSelectedNumber[tag] < maxCount)
                {
                    ShopSlotSelectedNumber[tag] = 0;
                    ShopSlotSelectedNumber[tag] += short.Parse(iup.Value.ToString());

                    if (ShopSlotSelectedNumber[tag] > maxCount)
                    {
                        ShopSlotSelectedNumber[tag] = maxCount;
                    }
                }
                GetMainPrice();
            }
        }

        private void BuildShop(WindowsTheme theme, string from)
        {
            int length = 0;
            string name = "";
            short number = 0;
            double price = 0;
            bool status = false;

            if (from == "storage")
            {
                length = StorageSlots;
                Array.Resize(ref ShopSlotSelectedNumber, StorageSlots + 1);
            }
            else
            {
                length = SoldSlotSinglePrice.Length;
                Array.Resize(ref ShopSlotSelectedNumber, SoldSlotSinglePrice.Length);
            }

            try
            {
                ShopSlotsPanel.Children.Clear();

                for (short i = 0; i < length; i++)
                {
                    switch (from)
                    {
                        case "storage":
                            name = StorageSlotName[i];
                            number = StorageSlotNumber[i];
                            price = StorageSlotPrice[i];
                            status = StorageSlotStatus[i];
                            if (IsStorageSlotError(i) == true)
                            {
                                status = false;
                            }
                            break;

                        case "statics":
                            name = SoldSlotName[i];
                            number = SoldSlotNumber[i];
                            price = SoldSlotSinglePrice[i];
                            if (number > 0)
                            {
                                status = true;
                            }
                            else
                            {
                                status = false;
                            }
                            break;

                        default:
                            break;
                    }

                    Array.Resize(ref ShopSlotSelectedNumber, StorageSlots + 1);

                    if (status == true)
                    {
                        Viewbox viewbox = new Viewbox();
                        string xamlString = "";
                        StringReader stringReader;
                        XmlReader xmlReader;

                        if (from == "storage")
                        {
                            if (status == true)
                            {
                                if (StorageSlotNumber[i] < StorageLimitedNumber)
                                {
                                    xamlString = XamlWriter.Save(ProductWarningVboxTemp);
                                }
                                else
                                {
                                    xamlString = XamlWriter.Save(ProductOkVboxTemp);
                                }
                            }
                            else
                            {
                                xamlString = XamlWriter.Save(ProductDisabledVboxTemp);
                            }
                        }
                        else
                        {
                            xamlString = XamlWriter.Save(ProductVboxTemp);
                        }

                        Viewbox statusVbox = new Viewbox();
                        stringReader = new StringReader(xamlString);
                        xmlReader = XmlReader.Create(stringReader);
                        statusVbox = (Viewbox)XamlReader.Load(xmlReader);
                        statusVbox.Margin = new Thickness(22, 5, 32, 5);
                        statusVbox.Height = 35;
                        statusVbox.Width = statusVbox.Height;
                        statusVbox.Name = $"ShopSlot{i}StatusVbox";
                        statusVbox.Tag = i;


                        TextBlock nameLbl = new TextBlock
                        {
                            Name = $"ShopSlot{i}NameLbl",
                            Tag = i,
                            Margin = new Thickness(10),
                            Width = 370,
                            VerticalAlignment = VerticalAlignment.Center,
                            Padding = new Thickness(5),
                            FontSize = 20,
                            Text = $"{name}",
                            ToolTip = $"{name}",
                            TextWrapping = TextWrapping.NoWrap,
                            TextTrimming = TextTrimming.CharacterEllipsis
                        };
                        if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name))
                        {
                            nameLbl.Text = "n/a";
                        }

                        Label numberLbl = new Label
                        {
                            Name = $"ShopSlot{i}NumberLbl",
                            Tag = i,
                            Margin = new Thickness(5, 10, 14, 10),
                            Width = 86,
                            HorizontalContentAlignment = HorizontalAlignment.Center,
                            VerticalContentAlignment = VerticalAlignment.Center,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            Padding = new Thickness(5),
                            FontSize = 20,
                            Content = $"{number}",
                        };

                        Label priceLbl = new Label
                        {
                            Name = $"ShopSlot{i}PriceLbl",
                            Tag = i,
                            Margin = new Thickness(10, 10, 18, 10),
                            Width = 70,
                            HorizontalContentAlignment = HorizontalAlignment.Center,
                            VerticalContentAlignment = VerticalAlignment.Center,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            Padding = new Thickness(5),
                            FontSize = 20,
                            Content = $"{price}"
                        };
                        for (int t = 0; t < 10; t++)
                        {
                            bool endsWithSearchResult = price.ToString().EndsWith($",{t}", StringComparison.CurrentCultureIgnoreCase);
                            if (endsWithSearchResult == true)
                            {
                                priceLbl.Content += "0";
                            }
                        }
                        priceLbl.Content += "€";

                        IntegerUpDown iup = new IntegerUpDown()
                        {
                            Name = $"ShopSlot{i}SellNumberUpDown",
                            Tag = i,
                            Margin = new Thickness(10, 10, 3, 10),
                            TextAlignment = TextAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            HorizontalContentAlignment = HorizontalAlignment.Center,
                            VerticalContentAlignment = VerticalAlignment.Center,
                            FontSize = 20,
                            Width = 62,
                            Padding = new Thickness(0),
                            BorderThickness = new Thickness(2),
                            Text = "0",
                            Value = 0,
                            ShowButtonSpinner = false,
                            Background = null,
                            BorderBrush = SeperatorColor.Background,
                            Foreground = TextFontColor.Background,
                            Maximum = number,
                            Minimum = 0
                        };
                        iup.ValueChanged += new RoutedPropertyChangedEventHandler<object>(ShopSlotSellNumberUpDown_ValueChanged); 

                        Button plusBtn = new Button()
                        {
                            Name = $"ShopSlot{i}AddItemBtn",
                            Tag = i,
                            Height = 35,
                            Width = 35,
                            Margin = new Thickness(10, 10, 5, 10),
                            FontSize = 20,
                            Padding = new Thickness(0),
                            VerticalContentAlignment = VerticalAlignment.Center,
                            HorizontalContentAlignment = HorizontalAlignment.Center
                        };
                        plusBtn.Click += new RoutedEventHandler(ShopSlotPlusBtn_Click);

                        xamlString = XamlWriter.Save(PlusVboxTemp);
                        stringReader = new StringReader(xamlString);
                        xmlReader = XmlReader.Create(stringReader);
                        viewbox = (Viewbox)XamlReader.Load(xmlReader);
                        plusBtn.Content = viewbox;

                        Button minusBtn = new Button()
                        {
                            Name = $"ShopSlot{i}RemoveItemBtn",
                            Tag = i,
                            Height = 35,
                            Width = 35,
                            Margin = new Thickness(5, 10, 5, 10),
                            FontSize = 20,
                            Padding = new Thickness(0),
                            VerticalContentAlignment = VerticalAlignment.Center,
                            HorizontalContentAlignment = HorizontalAlignment.Center
                        };
                        minusBtn.Click += new RoutedEventHandler(ShopSlotMinusBtn_Click);

                        xamlString = XamlWriter.Save(MinusVboxTemp);
                        stringReader = new StringReader(xamlString);
                        xmlReader = XmlReader.Create(stringReader);
                        viewbox = new Viewbox();
                        viewbox = (Viewbox)XamlReader.Load(xmlReader);
                        minusBtn.Content = viewbox;

                        Button restBtn = new Button()
                        {
                            Name = $"ShopSlot{i}AddAllItemsBtn",
                            Tag = i,
                            Margin = new Thickness(10, 10, 0, 10),
                            FontSize = 20,
                            Padding = new Thickness(0),
                            VerticalContentAlignment = VerticalAlignment.Center,
                            HorizontalContentAlignment = HorizontalAlignment.Center
                        };
                        restBtn.Click += new RoutedEventHandler(ShopSlotRestBtn_Click);

                        StackPanel stackPanel = new StackPanel()
                        {
                            Orientation = Orientation.Horizontal
                        };


                        Label lbl = new Label()
                        {
                            Content = "Rest",
                            VerticalAlignment = VerticalAlignment.Center,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Foreground = TextFontColor.Background
                        };

                        xamlString = XamlWriter.Save(RestVboxTemp);
                        stringReader = new StringReader(xamlString);
                        xmlReader = XmlReader.Create(stringReader);
                        viewbox = (Viewbox)XamlReader.Load(xmlReader);
                        stackPanel.Children.Add(viewbox);
                        stackPanel.Children.Add(lbl);
                        restBtn.Content = stackPanel;

                        StackPanel slotScp = new StackPanel
                        {
                            Name = $"ShopSlot{i}SlotPanel",
                            Tag = i,
                            Orientation = Orientation.Horizontal,
                            Margin = new Thickness(0),
                        };
                        if (i % 2 == 1)
                        {
                            slotScp.Background = Brushes.Transparent;
                        }
                        else
                        {
                            if (theme == WindowsTheme.Light)
                                slotScp.Background = Brushes.LightGray;
                            else
                                slotScp.Background = (Brush)new BrushConverter().ConvertFrom("#FF555555");
                        }

                        slotScp.MouseEnter += new MouseEventHandler(SlotItemMouseEnter);
                        slotScp.MouseLeave += new MouseEventHandler(SlotItemMouseLeave);
                        slotScp.Children.Add(statusVbox);
                        slotScp.Children.Add(nameLbl);
                        slotScp.Children.Add(numberLbl);
                        slotScp.Children.Add(priceLbl);
                        slotScp.Children.Add(iup);
                        slotScp.Children.Add(plusBtn);
                        slotScp.Children.Add(minusBtn);
                        slotScp.Children.Add(restBtn);
                        ShopSlotsPanel.Children.Add(slotScp);
                        ShopSlotsPanel.UpdateLayout();
                        GetMainPrice();

                        #region Unregister
                        try
                        {
                            UnregisterName(statusVbox.Name);
                        }
                        catch { }
                        try
                        {
                            UnregisterName(nameLbl.Name);
                        }
                        catch { }
                        try
                        {
                            UnregisterName(numberLbl.Name);
                        }
                        catch { }
                        try
                        {
                            UnregisterName(priceLbl.Name);
                        }
                        catch { }
                        try
                        {
                            UnregisterName(iup.Name);
                        }
                        catch { }
                        try
                        {
                            UnregisterName(plusBtn.Name);
                        }
                        catch { }
                        try
                        {
                            UnregisterName(minusBtn.Name);
                        }
                        catch { }
                        try
                        {
                            UnregisterName(restBtn.Name);
                        }
                        catch { }
                        try
                        {
                            UnregisterName(slotScp.Name);
                        }
                        catch { }
                        #endregion

                        RegisterName(statusVbox.Name, statusVbox);
                        RegisterName(nameLbl.Name, nameLbl);
                        RegisterName(numberLbl.Name, numberLbl);
                        RegisterName(priceLbl.Name, priceLbl);
                        RegisterName(iup.Name, iup);
                        RegisterName(plusBtn.Name, plusBtn);
                        RegisterName(minusBtn.Name, minusBtn);
                        RegisterName(restBtn.Name, restBtn);
                        RegisterName(slotScp.Name, slotScp);
                    }
                }
                logWindow.NewLog($"Building Shop successful", 1);
            }
            catch (Exception ex)
            {
                logWindow.NewLog($"Building Shop failed! {ex.Message}", 2);

            }

            NewPurchaseBtn.IsEnabled = true;
            ShopNoProductsFoundLbl.Visibility = Visibility.Collapsed;
            ShopProductsGrid.Visibility = Visibility.Visible;
            logWindow.NewLog($"Building Shop successful", 1);
            NewPurchaseBtn.IsEnabled = true;

            if (ShopSlotsPanel.Children.Count == 0)
            {
                NewPurchaseBtn.IsEnabled = false;
                ShopNoProductsFoundLbl.Visibility = Visibility.Visible;
                ShopProductsGrid.Visibility = Visibility.Collapsed;
            }

            if (lastPayDate.Date < DateTime.Now.Date)
            {
                CancelPurchaseBtn.IsEnabled = false;
                ComplainPurchaseBtn.IsEnabled = false;
            }
            else
            {
                CancelPurchaseBtn.IsEnabled = true;
                ComplainPurchaseBtn.IsEnabled = true;
            }
        }

        //private void BuildStaticsToShop()
        //{
        //    Reload();
        //    bool panelWasGrey = false;

        //    try
        //    {
        //        StaticsDayData staDL = StaticsDayData.Load(DateTime.Now.Date);
        //        ShopSlotsPanel.Children.Clear();
        //        for (int i = 0; i < SoldSlotSinglePrice.Length; i++)
        //        {
        //            Array.Resize(ref ShopSlotSelectedNumber, SoldSlotSinglePrice.Length);

        //            if (SoldSlotNumber[i] > 0)
        //            {
        //                StackPanel slotScp = new StackPanel
        //                {
        //                    Name = $"ShopSlot{i}SlotPanel",
        //                    Tag = i,
        //                    Margin = new Thickness(0),
        //                    Orientation = Orientation.Horizontal
        //                };
        //                slotScp.MouseEnter += new MouseEventHandler(SlotItemMouseEnter);
        //                slotScp.MouseLeave += new MouseEventHandler(SlotItemMouseLeave);
        //                if (panelWasGrey == true)
        //                {
        //                    slotScp.Background = Brushes.LightGray;
        //                    panelWasGrey = false;
        //                }
        //                else
        //                {
        //                    slotScp.Background = Brushes.White;
        //                    panelWasGrey = true;
        //                }

        //                Rectangle statusRtg = new Rectangle()
        //                {
        //                    Name = $"ShopSlot{i}StatusRtg",
        //                    Tag = i,
        //                    Height = 20,
        //                    Width = 20,
        //                    HorizontalAlignment = HorizontalAlignment.Center,
        //                    VerticalAlignment = VerticalAlignment.Center,
        //                    Margin = new Thickness(28, 5, 40, 5),
        //                    RadiusX = 10,
        //                    RadiusY = 10,
        //                    Fill = Brushes.Green
        //                };

        //                TextBlock nameLbl = new TextBlock
        //                {
        //                    Name = $"ShopSlot{i}NameLbl",
        //                    Tag = i,
        //                    Margin = new Thickness(10),
        //                    Width = 370,
        //                    VerticalAlignment = VerticalAlignment.Center,
        //                    Padding = new Thickness(5),
        //                    FontSize = 20,
        //                    Text = $"{SoldSlotName[i]}",
        //                    ToolTip = $"{SoldSlotName[i]}",
        //                    TextWrapping = TextWrapping.NoWrap,
        //                    TextTrimming = TextTrimming.CharacterEllipsis
        //                };

        //                Label numberLbl = new Label
        //                {
        //                    Name = $"ShopSlot{i}NumberLbl",
        //                    Tag = i,
        //                    Margin = new Thickness(5, 10, 14, 10),
        //                    Width = 86,
        //                    HorizontalContentAlignment = HorizontalAlignment.Center,
        //                    VerticalContentAlignment = VerticalAlignment.Center,
        //                    Padding = new Thickness(5),
        //                    FontSize = 20,
        //                    Content = $"{SoldSlotNumber[i]}",
        //                };

        //                Label priceLbl = new Label
        //                {
        //                    Name = $"ShopSlot{i}PriceLbl",
        //                    Tag = i,
        //                    Margin = new Thickness(10, 10, 18, 10),
        //                    Width = 70,
        //                    HorizontalContentAlignment = HorizontalAlignment.Center,
        //                    VerticalContentAlignment = VerticalAlignment.Center,
        //                    Padding = new Thickness(5),
        //                    FontSize = 20,
        //                    Content = $"{SoldSlotSinglePrice[i]}"
        //                };
        //                for (int t = 0; t < 10; t++)
        //                {
        //                    bool endsWithSearchResult = SoldSlotSinglePrice[i].ToString().EndsWith($",{t}", StringComparison.CurrentCultureIgnoreCase);
        //                    if (endsWithSearchResult == true)
        //                    {
        //                        priceLbl.Content += "0";
        //                        break;
        //                    }
        //                }
        //                priceLbl.Content += "€";

        //                IntegerUpDown iup = new IntegerUpDown()
        //                {
        //                    Name = $"ShopSlot{i}SellNumberUpDown",
        //                    Tag = i,
        //                    Margin = new Thickness(10, 10, 3, 10),
        //                    TextAlignment = TextAlignment.Center,
        //                    VerticalContentAlignment = VerticalAlignment.Center,
        //                    HorizontalContentAlignment = HorizontalAlignment.Center,
        //                    FontSize = 20,
        //                    Width = 62,
        //                    Padding = new Thickness(0),
        //                    IsReadOnly = true,
        //                    Text = ShopSlotSelectedNumber[i].ToString(),
        //                    ShowButtonSpinner = false
        //                };

        //                Button plusBtn = new Button()
        //                {
        //                    Name = $"ShopSlot{i}AddItemBtn",
        //                    Tag = i,
        //                    Content = "+",
        //                    Height = 35,
        //                    Width = 35,
        //                    Margin = new Thickness(3, 10, 3, 10),
        //                    FontSize = 20,
        //                    Padding = new Thickness(0),
        //                    VerticalContentAlignment = VerticalAlignment.Center,
        //                    HorizontalContentAlignment = HorizontalAlignment.Center
        //                };
        //                plusBtn.Click += new RoutedEventHandler(ShopSlotPlusBtn_Click);

        //                Button minusBtn = new Button()
        //                {
        //                    Name = $"ShopSlot{i}RemoveItemBtn",
        //                    Tag = i,
        //                    Content = "-",
        //                    Height = 35,
        //                    Width = 35,
        //                    Margin = new Thickness(3, 10, 3, 10),
        //                    FontSize = 20,
        //                    Padding = new Thickness(0),
        //                    VerticalContentAlignment = VerticalAlignment.Center,
        //                    HorizontalContentAlignment = HorizontalAlignment.Center
        //                };
        //                minusBtn.Click += new RoutedEventHandler(ShopSlotMinusBtn_Click);

        //                slotScp.Children.Add(statusRtg);
        //                slotScp.Children.Add(nameLbl);
        //                slotScp.Children.Add(numberLbl);
        //                slotScp.Children.Add(priceLbl);
        //                slotScp.Children.Add(iup);
        //                slotScp.Children.Add(plusBtn);
        //                slotScp.Children.Add(minusBtn);
        //                ShopSlotsPanel.Children.Add(slotScp);
        //                ShopSlotsPanel.UpdateLayout();
        //                GetMainPrice();

        //                try
        //                {
        //                    UnregisterName(statusRtg.Name);
        //                }
        //                catch { }
        //                try
        //                {
        //                    UnregisterName(nameLbl.Name);
        //                }
        //                catch { }
        //                try
        //                {
        //                    UnregisterName(numberLbl.Name);
        //                }
        //                catch { }
        //                try
        //                {
        //                    UnregisterName(priceLbl.Name);
        //                }
        //                catch { }
        //                try
        //                {
        //                    UnregisterName(iup.Name);
        //                }
        //                catch { }
        //                try
        //                {
        //                    UnregisterName(plusBtn.Name);
        //                }
        //                catch { }
        //                try
        //                {
        //                    UnregisterName(minusBtn.Name);
        //                }
        //                catch { }
        //                try
        //                {
        //                    UnregisterName(slotScp.Name);
        //                }
        //                catch { }

        //                RegisterName(statusRtg.Name, statusRtg);
        //                RegisterName(nameLbl.Name, nameLbl);
        //                RegisterName(numberLbl.Name, numberLbl);
        //                RegisterName(priceLbl.Name, priceLbl);
        //                RegisterName(iup.Name, iup);
        //                RegisterName(plusBtn.Name, plusBtn);
        //                RegisterName(minusBtn.Name, minusBtn);
        //                RegisterName(slotScp.Name, slotScp);
        //            }

        //            logWindow.NewLog($"Building StaticsData/Day to Shop successful", 1);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logWindow.NewLog($"Building StaticsData/Day to Shop failed! {ex.Message}", 2);

        //        CancelPurchaseBtn.IsEnabled = false;
        //        ComplainPurchaseBtn.IsEnabled = false;
        //    }
        //}

        private void GetMainPrice()
        {
            double[] price;
            if (shopTask == "sell")
            {
                price = StorageSlotPrice;
            }
            else
            {
                price = SoldSlotSinglePrice;
            }

            mainPrice = 0;
            for (int i = 0; i < price.Length; i++)
            {
                double singlePrice = ShopSlotSelectedNumber[i] * price[i];
                string txt = singlePrice.ToString();
                mainPrice += double.Parse(txt);
            }
            ShopMainPriceTxtBlock.Content = $"{mainPrice}";
            for (int t = 0; t < 10; t++)
            {
                bool endsWithSearchResult = mainPrice.ToString().EndsWith($",{t}", StringComparison.CurrentCultureIgnoreCase);
                if (endsWithSearchResult == true)
                {
                    ShopMainPriceTxtBlock.Content += "0";
                }
            }
            ShopMainPriceTxtBlock.Content += "€";

        }

        private void ClearShoppingCard()
        {
            for (int i = 0; i < ShopSlotSelectedNumber.Length; i++)
            {
                ShopSlotSelectedNumber[i] = 0;
            }

            GetMainPrice();
            BuildShop(GetWindowsTheme(), "storage");
        }

        private void NewPurchaseBtn_Click(object sender, RoutedEventArgs e)
        {
            Reload();
            OpenShop();

            TopBar.IsEnabled = false;
            shopTask = "sell";
            CustomerNumberTxtBlock.Content = totalCustomers + 1;
        }

        private void CancelPurchaseBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenShop();

            TopBar.IsEnabled = false;
            shopTask = "cancel";
            CustomerNumberTxtBlock.Content = totalCustomers;
        }

        private void ComplainPurchaseBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenShop();

            TopBar.IsEnabled = false;
            shopTask = "complain";
            CustomerNumberTxtBlock.Content = totalCustomers;
        }

        private void CancelComplainPurchaseBtn_GotFocus(object sender, RoutedEventArgs e)
        {
            BuildShop(GetWindowsTheme(), "statics");
        }

        private void CancelShoppingBtn_Click(object sender, RoutedEventArgs e)
        {
            CloseShop();

            TopBar.IsEnabled = true;
            shopTask = "";
            ClearShoppingCard();
            Reload();
        }

        private void ClearShoppingCardBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Warenkorb leeren?", "Warenkorb", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                ClearShoppingCard();
            }
        }

        private void PayPurchaseBtn_Click(object sender, RoutedEventArgs e)
        {
            CloseShop();

            for (int i = 0; i < StorageSlots; i++)
            {
                selectedItemsInt += ShopSlotSelectedNumber[i];
            }

            if (selectedItemsInt == 0)
            {
                MessageBox.Show("Der Warenborb ist leer!", "Bezahlvorgang nicht zulässig", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                LoadTotalStatics();
                LoadDayStatics(DateTime.Now.Date);

                if (loadTotalStaticsError != true)
                {
                    bool error = false;
                    GetMainPrice();

                    if (shopTask == "sell")
                    {
                        totalCustomers++;
                        totalGottenCash += mainPrice;

                        for (int n = 0; n < StorageSlots; n++)
                        {
                            if (ShopSlotSelectedNumber[n] != 0)
                            {
                                try
                                {
                                    StorageSlotNumber[n] -= ShopSlotSelectedNumber[n];
                                    totalSoldProducts += ShopSlotSelectedNumber[n];

                                    int i = -1;
                                    bool productFound = false;
                                    foreach (string s in productsNameList)
                                    {
                                        i++;
                                        if (StorageSlotName[n] == s && StorageSlotPrice[n] == productsSinglePriceList[n])
                                        {
                                            productsNumberList[i] += ShopSlotSelectedNumber[n];
                                            productsCashList[i] += ShopSlotSelectedNumber[n] * StorageSlotPrice[n];
                                            productFound = true;
                                            break;
                                        }
                                    }
                                    if (productFound == false)
                                    {
                                        Array.Resize(ref productsNameList, productsNameList.Length + 1);
                                        productsNameList[productsNameList.Length - 1] = StorageSlotName[n];

                                        Array.Resize(ref productsNumberList, productsNumberList.Length + 1);
                                        productsNumberList[productsNumberList.Length - 1] = ShopSlotSelectedNumber[n];

                                        Array.Resize(ref productsCashList, productsCashList.Length + 1);
                                        productsCashList[productsCashList.Length - 1] = StorageSlotPrice[n] * ShopSlotSelectedNumber[n];

                                        Array.Resize(ref productsSinglePriceList, productsSinglePriceList.Length + 1);
                                        productsSinglePriceList[productsSinglePriceList.Length - 1] = StorageSlotPrice[n];
                                    }

                                    i = -1;
                                    productFound = false;
                                    foreach (string s in SoldSlotName)
                                    {
                                        i++;
                                        if (StorageSlotName[n] == s && StorageSlotPrice[n] == productsSinglePriceList[n])
                                        {
                                            SoldSlotNumber[i] += ShopSlotSelectedNumber[n];
                                            SoldSlotCash[i] += ShopSlotSelectedNumber[n] * StorageSlotPrice[n];
                                            productFound = true;
                                            break;
                                        }
                                    }
                                    if (productFound == false)
                                    {
                                        Array.Resize(ref SoldSlotName, SoldSlotName.Length + 1);
                                        SoldSlotName[SoldSlotName.Length - 1] = StorageSlotName[n];

                                        Array.Resize(ref SoldSlotNumber, SoldSlotNumber.Length + 1);
                                        SoldSlotNumber[SoldSlotNumber.Length - 1] = ShopSlotSelectedNumber[n];

                                        Array.Resize(ref SoldSlotCash, SoldSlotCash.Length + 1);
                                        SoldSlotCash[SoldSlotCash.Length - 1] = StorageSlotPrice[n] * ShopSlotSelectedNumber[n];

                                        Array.Resize(ref SoldSlotSinglePrice, SoldSlotSinglePrice.Length + 1);
                                        SoldSlotSinglePrice[SoldSlotSinglePrice.Length - 1] = StorageSlotPrice[n];
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logWindow.NewLog($"Error in paying Purchase Slot{n} -> {ex.Message}", 2);

                                }
                            }
                        }

                        lastPayDate = DateTime.Now.Date;
                    }
                    else if (shopTask == "cancel")
                    {
                        LoadStorage();
                        totalGottenCash -= mainPrice;

                        for (int n = 0; n < SoldSlotName.Length; n++)
                        {
                            try
                            {
                                if (ShopSlotSelectedNumber[n] != 0)
                                {
                                    int i = -1;
                                    foreach (string s in productsNameList)
                                    {
                                        i++;
                                        if (SoldSlotName[n] == s && StorageSlotPrice[n] == productsSinglePriceList[n])
                                        {
                                            productsNumberList[i] -= ShopSlotSelectedNumber[n];
                                            productsCashList[i] -= ShopSlotSelectedNumber[n] * SoldSlotSinglePrice[n];
                                            break;
                                        }
                                    }

                                    i = -1;
                                    foreach (string s in SoldSlotName)
                                    {
                                        i++;
                                        if (SoldSlotName[n] == s && StorageSlotPrice[n] == SoldSlotSinglePrice[n])
                                        {
                                            SoldSlotNumber[i] -= ShopSlotSelectedNumber[n];
                                            SoldSlotCash[i] -= ShopSlotSelectedNumber[n] * SoldSlotSinglePrice[n];
                                            break;
                                        }
                                    }

                                    StorageSlotNumber[n] += ShopSlotSelectedNumber[n];
                                    totalSoldProducts -= ShopSlotSelectedNumber[n];
                                }
                            }
                            catch (Exception ex)
                            {
                                logWindow.NewLog($"Error in canceling Purchase Slot{n} -> {ex.Message}", 2);

                            }
                        }
                    }
                    else if (shopTask == "complain")
                    {
                        LoadStorage();

                        for (int n = 0; n < SoldSlotName.Length; n++)
                        {
                            try
                            {
                                LostProducts += ShopSlotSelectedNumber[n];
                                LostCash += SoldSlotSinglePrice[n] * ShopSlotSelectedNumber[n];
                            }
                            catch (Exception ex)
                            {
                                logWindow.NewLog($"Error in complaining Purchase Slot{n} -> {ex.Message}", 2);

                            }
                        }

                        totalGottenCash -= mainPrice;
                        totalLostCash += LostCash;
                        totalLostProducts += LostProducts;
                    }
                    else
                    {
                        MessageBox.Show("Bezahlvorgang konnte nicht definiert werden!", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                        error = true;
                    }

                    if (error != true)
                    {
                        SaveSettings();
                        SaveStorage();
                        SaveTotalStatics();
                        SaveDayStatics();
                        Reload();
                    }
                }
                else
                {

                }
            }
            ClearShoppingCard();
            SaveDayStatics();
            SaveTotalStatics();
            TopBar.IsEnabled = true;
        }

        private void OpenShop()
        {
            OpenShopToolsPanel.Visibility = Visibility.Visible;
            OpenShopStaticsPanel.Visibility = Visibility.Visible;
            ClosedShopToolsPanel.Visibility = Visibility.Collapsed;

            DoubleAnimation ani = new DoubleAnimation()
            {
                To = 1,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new QuadraticEase()
            };
            ShopProductsGrid.IsEnabled = true;
            ShopProductsGrid.BeginAnimation(OpacityProperty, ani);

        }

        private void CloseShop()
        {
            OpenShopToolsPanel.Visibility = Visibility.Collapsed;
            OpenShopStaticsPanel.Visibility = Visibility.Collapsed;
            ClosedShopToolsPanel.Visibility = Visibility.Visible;

            DoubleAnimation ani = new DoubleAnimation()
            {
                To = 0.2,
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new QuadraticEase()
            };
            ShopProductsGrid.IsEnabled = false;
            ShopProductsGrid.BeginAnimation(OpacityProperty, ani);

        }
        #endregion

        #region Storage
        private void StorageSlotEditVBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Viewbox vBox = sender as Viewbox;
            short tag = short.Parse(vBox.Tag.ToString());

            EditStorageSlot window = new EditStorageSlot("edit", tag)
            {
                slotName = StorageSlotName[tag],
                slotNumber = StorageSlotNumber[tag],
                slotStatus = StorageSlotStatus[tag],
                slotPrice = StorageSlotPrice[tag]
            };
            window.ShowDialog();

            if (window.DialogResult == true)
            {
                StorageSlotName[tag] = window.slotName;
                StorageSlotNumber[tag] = window.slotNumber;
                StorageSlotStatus[tag] = window.slotStatus;
                StorageSlotPrice[tag] = window.slotPrice;

                if (StorageSlotNumber[tag] < 0)
                {
                    StorageSlotNumber[tag] = 0;
                }

                SaveStorage();
                Reload();
            }
        }

        private void StorageSlotCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox chBox)
            {
                if (StorageSelectedCount == StorageSlots)
                {
                    StorageSelectAllSlotsChBox.IsChecked = false;
                }

                StorageSelectedCount--;

                if (StorageSelectedCount <= 0)
                {
                    StorageRemoveSlotBtn.IsEnabled = false;
                    StorageActivedSlotBtn.IsEnabled = false;
                    StorageDeactivedSlotBtn.IsEnabled = false;
                }

                StorageSelectedArray[int.Parse(chBox.Tag.ToString())] = false;
                StorageSelectAllSlotsChBox.IsChecked = false;
            }
        }

        private void StorageSlotCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox chBox)
            {
                StorageSelectedCount++;

                StorageRemoveSlotBtn.IsEnabled = true;
                StorageActivedSlotBtn.IsEnabled = true;
                StorageDeactivedSlotBtn.IsEnabled = true;

                StorageSelectedArray[int.Parse(chBox.Tag.ToString())] = true;

                if (StorageSelectedCount == StorageSlots)
                {
                    StorageSelectAllSlotsChBox.IsChecked = true;
                }
            }
        }

        private void StorageSelectAllSlotsChBox_Click(object sender, RoutedEventArgs e)
        {
            if (StorageSelectAllSlotsChBox.IsChecked == true)
            {
                StorageRemoveSlotBtn.IsEnabled = true;
                StorageActivedSlotBtn.IsEnabled = true;
                StorageDeactivedSlotBtn.IsEnabled = true;
                StorageSelectedCount = StorageSlots;

                for (int i = 0; i < StorageSlots; i++)
                {
                    StorageSelectedArray[i] = true;
                    CheckBox chBox = (CheckBox)FindName($"StorageSlot{i}ChBox");
                    if (chBox != null)
                    {
                        chBox.IsChecked = true;
                    }
                }
            }
            else
            {
                StorageRemoveSlotBtn.IsEnabled = false;
                StorageActivedSlotBtn.IsEnabled = false;
                StorageDeactivedSlotBtn.IsEnabled = false;
                StorageUncheckAll();

                for (int i = 0; i < StorageSlots; i++)
                {
                    CheckBox chBox = (CheckBox)FindName($"StorageSlot{i}ChBox");
                    if (chBox != null)
                    {
                        chBox.IsChecked = false;
                    }
                }
            }
        }

        private void BuildStorage(WindowsTheme theme)
        {
            if (StorageSlots == 0)
            {
                StorageNoProductsFoundLbl.Visibility = Visibility.Visible;
                StorageProductsGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                StorageNoProductsFoundLbl.Visibility = Visibility.Collapsed;
                StorageProductsGrid.Visibility = Visibility.Visible;

                try
                {
                    StorageSlotsPanel.Children.Clear();
                    string xamlString = "";
                    StringReader stringReader;
                    XmlReader xmlReader;

                    for (short i = 0; i < StorageSlots; i++)
                    {
                        CheckBox chBox = new CheckBox()
                        {
                            Name = $"StorageSlot{i}ChBox",
                            Tag = i,
                            VerticalAlignment = VerticalAlignment.Center,
                            Margin = new Thickness(10, 0, 0, 0),
                        };
                        chBox.Checked += new RoutedEventHandler(StorageSlotCheckBox_Checked);
                        chBox.Unchecked += new RoutedEventHandler(StorageSlotCheckBox_Unchecked);

                        Label slotLbl = new Label
                        {
                            Name = $"StorageSlot{i}SlotLbl",
                            Tag = i,
                            Margin = new Thickness(15, 0, 0, 0),
                            HorizontalContentAlignment = HorizontalAlignment.Center,
                            VerticalContentAlignment = VerticalAlignment.Center,
                            FontWeight = FontWeights.ExtraBlack,
                            Padding = new Thickness(0),
                            Height = 30,
                            Width = 40,
                            ToolTip = i + 1,
                            Content = $"{i + 1}"
                        };

                        if (StorageSlotStatus[i] == true)
                        {
                            if (IsStorageSlotError(i) == true)
                            {
                                xamlString = XamlWriter.Save(ProductErrorVboxTemp);
                            }
                            else
                            {
                                if (StorageSlotNumber[i] < StorageLimitedNumber)
                                {
                                    xamlString = XamlWriter.Save(ProductWarningVboxTemp);
                                }
                                else
                                {
                                    xamlString = XamlWriter.Save(ProductOkVboxTemp);
                                }
                            }
                        }
                        else
                        {
                            xamlString = XamlWriter.Save(ProductDisabledVboxTemp);
                        }
                        Viewbox statusVbox = new Viewbox();
                        stringReader = new StringReader(xamlString);
                        xmlReader = XmlReader.Create(stringReader);
                        statusVbox = (Viewbox)XamlReader.Load(xmlReader);
                        statusVbox.Margin = new Thickness(45, 5, 35, 5);
                        statusVbox.Height = 35;
                        statusVbox.Width = statusVbox.Height;
                        statusVbox.Name = $"StorageSlot{i}StatusVbox";
                        statusVbox.Tag = i;

                        TextBlock nameLbl = new TextBlock
                        {
                            Name = $"StorageSlot{i}NameLbl",
                            Tag = i,
                            Margin = new Thickness(10),
                            Width = 425,
                            VerticalAlignment = VerticalAlignment.Center,
                            FontSize = 20,
                            Text = $"{StorageSlotName[i]}",
                            ToolTip = $"{StorageSlotName[i]}",
                            TextTrimming = TextTrimming.CharacterEllipsis
                        };
                        if (string.IsNullOrEmpty(StorageSlotName[i]))
                        {
                            nameLbl.Text = "n/a";
                        }

                        Label numberLbl = new Label
                        {
                            Name = $"StorageSlot{i}NumberLbl",
                            Tag = i,
                            Margin = new Thickness(10, 10, 45, 10),
                            Width = 87,
                            HorizontalContentAlignment = HorizontalAlignment.Center,
                            VerticalContentAlignment = VerticalAlignment.Center,
                            Padding = new Thickness(5),
                            FontSize = 20,
                            Content = $"{StorageSlotNumber[i]}",
                        };

                        Label priceLbl = new Label
                        {
                            Name = $"StorageSlot{i}PriceLbl",
                            Tag = i,
                            Margin = new Thickness(10),
                            Width = 60,
                            HorizontalContentAlignment = HorizontalAlignment.Center,
                            VerticalContentAlignment = VerticalAlignment.Center,
                            Padding = new Thickness(0),
                            FontSize = 20,
                            Content = $"{StorageSlotPrice[i]}"
                        };
                        for (int t = 0; t < 10; t++)
                        {
                            bool endsWithSearchResult = StorageSlotPrice[i].ToString().EndsWith($",{t}", StringComparison.CurrentCultureIgnoreCase);
                            if (endsWithSearchResult == true)
                            {
                                priceLbl.Content += "0";
                            }
                        }
                        priceLbl.Content += "€";

                        Viewbox optionsVBox = new Viewbox();
                        xamlString = XamlWriter.Save(EditVBoxTemp);
                        stringReader = new StringReader(xamlString);
                        xmlReader = XmlReader.Create(stringReader);
                        optionsVBox = (Viewbox)XamlReader.Load(xmlReader);
                        optionsVBox.MouseDown += new MouseButtonEventHandler(StorageSlotEditVBox_MouseDown);
                        optionsVBox.Name = $"StorageSlot{i}OptionsScp";
                        optionsVBox.Tag = i;
                        optionsVBox.Margin = new Thickness(10);
                        optionsVBox.Width = 30;
                        optionsVBox.HorizontalAlignment = HorizontalAlignment.Center;
                        optionsVBox.VerticalAlignment = VerticalAlignment.Center;


                        StackPanel slotScp = new StackPanel
                        {
                            Name = $"StorageSlot{i}SlotPanel",
                            Tag = i,
                            Orientation = Orientation.Horizontal
                        };
                        if (i % 2 == 1)
                        {
                            slotScp.Background = Brushes.Transparent;
                        }
                        else
                        {
                            if (theme == WindowsTheme.Light)
                                slotScp.Background = Brushes.LightGray;
                            else
                                slotScp.Background = (Brush)new BrushConverter().ConvertFrom("#FF555555");
                        }

                        slotScp.MouseEnter += new MouseEventHandler(SlotItemMouseEnter);
                        slotScp.MouseLeave += new MouseEventHandler(SlotItemMouseLeave);
                        slotScp.Children.Add(chBox);
                        slotScp.Children.Add(slotLbl);
                        slotScp.Children.Add(statusVbox);
                        slotScp.Children.Add(nameLbl);
                        slotScp.Children.Add(numberLbl);
                        slotScp.Children.Add(priceLbl);
                        slotScp.Children.Add(optionsVBox);
                        StorageSlotsPanel.Children.Add(slotScp);
                        StorageSlotsPanel.UpdateLayout();

                        try
                        {
                            UnregisterName(chBox.Name);
                        }
                        catch { }
                        try
                        {
                            UnregisterName(slotLbl.Name);
                        }
                        catch { }
                        try
                        {
                            UnregisterName(statusVbox.Name);
                        }
                        catch { }
                        try
                        {
                            UnregisterName(nameLbl.Name);
                        }
                        catch { }
                        try
                        {
                            UnregisterName(numberLbl.Name);
                        }
                        catch { }
                        try
                        {
                            UnregisterName(priceLbl.Name);
                        }
                        catch { }
                        try
                        {
                            UnregisterName(optionsVBox.Name);
                        }
                        catch { }
                        try
                        {
                            UnregisterName(slotScp.Name);
                        }
                        catch { }

                        RegisterName(chBox.Name, chBox);
                        RegisterName(slotLbl.Name, slotLbl);
                        RegisterName(statusVbox.Name, statusVbox);
                        RegisterName(nameLbl.Name, nameLbl);
                        RegisterName(numberLbl.Name, numberLbl);
                        RegisterName(priceLbl.Name, priceLbl);
                        RegisterName(optionsVBox.Name, optionsVBox);
                        RegisterName(slotScp.Name, slotScp);

                        Array.Resize(ref StorageSelectedArray, StorageSlots);
                        StorageUncheckAll();
                    }
                    logWindow.NewLog($"Building Storage successful", 1);
                }
                catch (Exception ex)
                {
                    logWindow.NewLog($"Building Storage failed! {ex.Message}", 2);

                }
            }
        }

        private void LoadStorage()
        {
            try
            {
                StorageData stoL = StorageData.Load();

                StorageSlotName = Array.Empty<string>();
                StorageSlotStatus = Array.Empty<bool>();
                StorageSlotNumber = Array.Empty<short>();
                StorageSlotPrice = Array.Empty<double>();

                try
                {
                    StorageSlots = stoL.StorageSlots;

                    for (int i = 0; i < StorageSlots; i++)
                    {
                        try
                        {
                            Array.Resize(ref StorageSlotName, StorageSlotName.Length + 1);
                            StorageSlotName[StorageSlotName.Length - 1] = stoL.StorageSlotName[i];
                        }
                        catch (Exception ex)
                        {
                            logWindow.NewLog($"Error in Loading Storage/StorageSlotName{i} -> {ex.Message}", 2);

                        }
                        try
                        {
                            Array.Resize(ref StorageSlotNumber, StorageSlotNumber.Length + 1);
                            StorageSlotNumber[StorageSlotNumber.Length - 1] = stoL.StorageSlotNumber[i];
                        }
                        catch (Exception ex)
                        {
                            logWindow.NewLog($"Error in Loading Storage/StorageSlotNumber{i} -> {ex.Message}", 2);

                        }
                        try
                        {
                            Array.Resize(ref StorageSlotPrice, StorageSlotPrice.Length + 1);
                            StorageSlotPrice[StorageSlotPrice.Length - 1] = stoL.StorageSlotPrice[i];
                        }
                        catch (Exception ex)
                        {
                            logWindow.NewLog($"Error in Loading Storage/StorageSlotPrice{i} -> {ex.Message}", 2);

                        }
                        try
                        {
                            Array.Resize(ref StorageSlotStatus, StorageSlotStatus.Length + 1);
                            StorageSlotStatus[StorageSlotStatus.Length - 1] = stoL.StorageSlotStatus[i];
                        }
                        catch (Exception ex)
                        {
                            logWindow.NewLog($"Error in Loading Storage/StorageSlotStatus{i} -> {ex.Message}", 2);

                        }
                    }
                }
                catch (Exception ex)
                {
                    logWindow.NewLog($"Error in Loading Storage/StorageSlots -> {ex.Message}", 2);

                }

                logWindow.NewLog($"Loading Storage successful", 1);
            }
            catch (Exception ex)
            {
                logWindow.NewLog($"Loading Storage failed! {ex.Message}", 2);

            }
        }

        private void SaveStorage()
        {

            StorageData stoS = new StorageData();

            Array.Clear(stoS.StorageSlotName, 0, stoS.StorageSlotName.Length);
            Array.Clear(stoS.StorageSlotStatus, 0, stoS.StorageSlotStatus.Length);
            Array.Clear(stoS.StorageSlotNumber, 0, stoS.StorageSlotNumber.Length);
            Array.Clear(stoS.StorageSlotPrice, 0, stoS.StorageSlotPrice.Length);

            try
            {
                stoS.StorageSlots = StorageSlots;
            }
            catch (Exception ex)
            {
                logWindow.NewLog($"Error in Saving Storage/StorageSlots -> {ex.Message}", 2);

            }


            for (int i = 0; i < StorageSlots; i++)
            {
                try
                {
                    Array.Resize(ref stoS.StorageSlotName, stoS.StorageSlotName.Length + 1);
                    stoS.StorageSlotName[stoS.StorageSlotName.Length - 1] = StorageSlotName[i];
                }
                catch (Exception ex)
                {
                    logWindow.NewLog($"Error in Saving Storage/StorageSlotName{i} -> {ex.Message}", 2);

                }
                try
                {
                    Array.Resize(ref stoS.StorageSlotNumber, stoS.StorageSlotNumber.Length + 1);
                    stoS.StorageSlotNumber[stoS.StorageSlotNumber.Length - 1] = StorageSlotNumber[i];
                }
                catch (Exception ex)
                {
                    logWindow.NewLog($"Error in Saving Storage/StorageSlotNumber{i} -> {ex.Message}", 2);

                }
                try
                {
                    Array.Resize(ref stoS.StorageSlotPrice, stoS.StorageSlotPrice.Length + 1);
                    stoS.StorageSlotPrice[stoS.StorageSlotPrice.Length - 1] = StorageSlotPrice[i];
                }
                catch (Exception ex)
                {
                    logWindow.NewLog($"Error in Saving Storage/StorageSlotPrice{i} -> {ex.Message}", 2);

                }
                try
                {
                    Array.Resize(ref stoS.StorageSlotStatus, stoS.StorageSlotStatus.Length + 1);
                    stoS.StorageSlotStatus[stoS.StorageSlotStatus.Length - 1] = StorageSlotStatus[i];
                }
                catch (Exception ex)
                {
                    logWindow.NewLog($"Error in Saving Storage/StorageSlotStatus{i} -> {ex.Message}", 2);

                }
            }

            try
            {
                stoS.Save();
                logWindow.NewLog($"Saving Storage successful", 1);
            }
            catch (Exception ex)
            {
                logWindow.NewLog($"Saving Storage failed! {ex.Message}", 2);

            }

        }

        private void StorageAddSlotBtn_Click(object sender, RoutedEventArgs e)
        {
            EditStorageSlot window = new EditStorageSlot("new", 0);
            window.ShowDialog();

            if (window.DialogResult == true)
            {
                Array.Resize(ref StorageSlotName, StorageSlotName.Length + 1);
                StorageSlotName[StorageSlotName.Length - 1] = window.slotName;

                Array.Resize(ref StorageSlotStatus, StorageSlotStatus.Length + 1);
                StorageSlotStatus[StorageSlotStatus.Length - 1] = window.slotStatus;

                Array.Resize(ref StorageSlotNumber, StorageSlotNumber.Length + 1);
                StorageSlotNumber[StorageSlotNumber.Length - 1] = window.slotNumber;

                Array.Resize(ref StorageSlotPrice, StorageSlotPrice.Length + 1);
                StorageSlotPrice[StorageSlotPrice.Length - 1] = window.slotPrice;

                StorageSlots++;
                Array.Resize(ref StorageSelectedArray, StorageSlots);

                StorageUncheckAll();

                SaveStorage();
                LoadStorage();
                BuildStorage(GetWindowsTheme());
            }
        }

        private void StorageRemoveSlotBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Möchten Sie die ausgewählten Produktplätze wirklich löschen?\nDiese Aktion kann nicht rückgängig gemacht werden!", "Produktplätze löschen", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                for (int i = 0; i < StorageSlots; i++)
                {
                    if (StorageSelectedArray[i] == true)
                    {
                        StorageSlotName[i] = null;
                        StorageSlotStatus[i] = false;
                        StorageSlotNumber[i] = 0;
                        StorageSlotPrice[i] = 0;
                    }
                }

                StorageUncheckAll();
                StorageDeleteSpaces();
                SaveStorage();
                LoadStorage();
                BuildStorage(GetWindowsTheme());
            }
        }

        private void StorageActivedSlotBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Möchten Sie die ausgewählten Produktplätze wirklich aktivieren?", "Produktplätze aktivieren", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                for (int i = 0; i < StorageSlots; i++)
                {
                    if (StorageSelectedArray[i] == true)
                    {
                        StorageSlotStatus[i] = true;

                        if (StorageSlotPrice[i] == 0)
                        {
                            StorageSlotStatus[i] = false;
                        }

                        if (StorageSlotNumber[i] == 0)
                        {
                            StorageSlotStatus[i] = false;
                        }

                        if (string.IsNullOrEmpty(StorageSlotName[i]))
                        {
                            StorageSlotStatus[i] = false;
                        }
                    }
                }
                StorageUncheckAll();

                SaveStorage();
                LoadStorage();
                BuildStorage(GetWindowsTheme());
            }
        }

        private void StorageDeactivedSlotBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Möchten Sie die ausgewählten Produktplätze wirklich deaktivieren?", "Produktplätze deaktivieren", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                for (int i = 0; i < StorageSlots; i++)
                {
                    if (StorageSelectedArray[i] == true)
                    {
                        StorageSlotStatus[i] = false;
                    }
                }
                StorageUncheckAll();

                SaveStorage();
                LoadStorage();
                BuildStorage(GetWindowsTheme());
            }
        }

        private void StorageUncheckAll()
        {
            StorageSelectAllSlotsChBox.IsChecked = false;
            StorageSelectedCount = 0;
            StorageRemoveSlotBtn.IsEnabled = false;
            StorageActivedSlotBtn.IsEnabled = false;
            StorageDeactivedSlotBtn.IsEnabled = false;

            for (int i = 0; i < StorageSlots; i++)
            {
                StorageSelectedArray[i] = false;
            }
        }

        private void StorageDeleteSpaces()
        {
            for (int i = 0; i < StorageSlots; i++)
            {
                if (StorageSlotName[i] == null)
                {
                    for (int n = i; n < StorageSlots; n++)
                    {
                        try
                        {
                            StorageSlotName[n] = StorageSlotName[n + 1];
                            StorageSlotStatus[n] = StorageSlotStatus[n + 1];
                            StorageSlotNumber[n] = StorageSlotNumber[n + 1];
                            StorageSlotPrice[n] = StorageSlotPrice[n + 1];
                        }
                        catch { }
                    }
                    StorageSlots--;
                    i--;
                }
            }

            Array.Resize(ref StorageSlotName, StorageSlots);
            Array.Resize(ref StorageSlotStatus, StorageSlots);
            Array.Resize(ref StorageSlotNumber, StorageSlots);
            Array.Resize(ref StorageSlotPrice, StorageSlots);
        }

        private bool IsStorageSlotError(int i)
        {
            bool error = false;
            if (string.IsNullOrWhiteSpace(StorageSlotName[i]) || StorageSlotNumber[i] == 0 || StorageSlotPrice[i] == 0)
            {
                error = true;
            }
            return error;
        }
        #endregion

        #region PcTime
        private void NewPcUserBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DeletePcUserBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        #endregion

        #region Statics
        private void SaveTotalStatics()
        {
            for (int i = 0; i < productsNumberList.Length; i++)
            {
                if (productsNumberList[i] > mostSoldProductsNumber[0])
                {
                    mostSoldProductsNumber[4] = mostSoldProductsNumber[3];
                    mostSoldProductsNumber[3] = mostSoldProductsNumber[2];
                    mostSoldProductsNumber[2] = mostSoldProductsNumber[1];
                    mostSoldProductsNumber[1] = mostSoldProductsNumber[0];
                    mostSoldProductsNumber[0] = productsNumberList[i];

                    mostSoldProductsName[4] = mostSoldProductsName[3];
                    mostSoldProductsName[3] = mostSoldProductsName[2];
                    mostSoldProductsName[2] = mostSoldProductsName[1];
                    mostSoldProductsName[1] = mostSoldProductsName[0];
                    mostSoldProductsName[0] = productsNameList[i];
                }
                else if (productsNumberList[i] > mostSoldProductsNumber[1])
                {
                    mostSoldProductsNumber[4] = mostSoldProductsNumber[3];
                    mostSoldProductsNumber[3] = mostSoldProductsNumber[2];
                    mostSoldProductsNumber[2] = mostSoldProductsNumber[1];
                    mostSoldProductsNumber[1] = productsNumberList[i];

                    mostSoldProductsName[4] = mostSoldProductsName[3];
                    mostSoldProductsName[3] = mostSoldProductsName[2];
                    mostSoldProductsName[2] = mostSoldProductsName[1];
                    mostSoldProductsName[1] = productsNameList[i];
                }
                else if (productsNumberList[i] > mostSoldProductsNumber[2])
                {
                    mostSoldProductsNumber[4] = mostSoldProductsNumber[3];
                    mostSoldProductsNumber[3] = mostSoldProductsNumber[2];
                    mostSoldProductsNumber[2] = productsNumberList[i];

                    mostSoldProductsName[4] = mostSoldProductsName[3];
                    mostSoldProductsName[3] = mostSoldProductsName[2];
                    mostSoldProductsName[2] = productsNameList[i];
                }
                else if (productsNumberList[i] > mostSoldProductsNumber[3])
                {
                    mostSoldProductsNumber[4] = mostSoldProductsNumber[3];
                    mostSoldProductsNumber[3] = productsNumberList[i];

                    mostSoldProductsName[4] = mostSoldProductsName[3];
                    mostSoldProductsName[3] = productsNameList[i];
                }
                else if (productsNumberList[i] > mostSoldProductsNumber[4])
                {
                    mostSoldProductsNumber[4] = productsNumberList[i];

                    mostSoldProductsName[4] = productsNameList[i];
                }
            }

            for (int i = 0; i < productsCashList.Length; i++)
            {
                if (productsCashList[i] > highestEarningsProductsNumber[0])
                {
                    highestEarningsProductsNumber[4] = highestEarningsProductsNumber[3];
                    highestEarningsProductsNumber[3] = highestEarningsProductsNumber[2];
                    highestEarningsProductsNumber[2] = highestEarningsProductsNumber[1];
                    highestEarningsProductsNumber[1] = highestEarningsProductsNumber[0];
                    highestEarningsProductsNumber[0] = productsCashList[i];

                    highestEarningsProductsName[4] = highestEarningsProductsName[3];
                    highestEarningsProductsName[3] = highestEarningsProductsName[2];
                    highestEarningsProductsName[2] = highestEarningsProductsName[1];
                    highestEarningsProductsName[1] = highestEarningsProductsName[0];
                    highestEarningsProductsName[0] = productsNameList[i];
                }
                else if (productsCashList[i] > highestEarningsProductsNumber[1])
                {
                    highestEarningsProductsNumber[4] = highestEarningsProductsNumber[3];
                    highestEarningsProductsNumber[3] = highestEarningsProductsNumber[2];
                    highestEarningsProductsNumber[2] = highestEarningsProductsNumber[1];
                    highestEarningsProductsNumber[1] = productsNumberList[i];

                    highestEarningsProductsName[4] = highestEarningsProductsName[3];
                    highestEarningsProductsName[3] = highestEarningsProductsName[2];
                    highestEarningsProductsName[2] = highestEarningsProductsName[1];
                    highestEarningsProductsName[1] = productsNameList[i];
                }
                else if (productsCashList[i] > highestEarningsProductsNumber[2])
                {
                    highestEarningsProductsNumber[4] = highestEarningsProductsNumber[3];
                    highestEarningsProductsNumber[3] = highestEarningsProductsNumber[2];
                    highestEarningsProductsNumber[2] = productsNumberList[i];

                    highestEarningsProductsName[4] = highestEarningsProductsName[3];
                    highestEarningsProductsName[3] = highestEarningsProductsName[2];
                    highestEarningsProductsName[2] = productsNameList[i];
                }
                else if (productsCashList[i] > highestEarningsProductsNumber[3])
                {
                    highestEarningsProductsNumber[4] = highestEarningsProductsNumber[3];
                    highestEarningsProductsNumber[3] = productsNumberList[i];

                    highestEarningsProductsName[4] = highestEarningsProductsName[3];
                    highestEarningsProductsName[3] = productsNameList[i];
                }
                else if (productsCashList[i] > highestEarningsProductsNumber[4])
                {
                    highestEarningsProductsNumber[4] = productsNumberList[i];

                    highestEarningsProductsName[4] = productsNameList[i];
                }
            }

            StaticsTotalData staTS = new StaticsTotalData
            {
                startDate = startDate,
                totalCustomers = totalCustomers,
                totalSoldProducts = totalSoldProducts,
                totalGottenCash = totalGottenCash,
                totalLostCash = totalLostCash,
                totalLostProducts = totalLostProducts,
                productsNameList = productsNameList,
                productsNumberList = productsNumberList,
                productsCashList = productsCashList,
                productsSinglePriceList = productsSinglePriceList,
                totalPcTime = totalPcTime,
                totalPcUsers = totalPcUsers,
                mostSoldProductsName = mostSoldProductsName,
                mostSoldProductsNumber = mostSoldProductsNumber,
                mostSoldProductsSinglePrice = mostSoldProductsSinglePrice,
                highestEarningsProductsName = highestEarningsProductsName,
                highestEarningsProductsNumber = highestEarningsProductsNumber,
                highestEarningsProductsSinglePrice = highestEarningsProductsSinglePrice
            };

            try
            {
                staTS.Save();
                logWindow.NewLog($"Saving StaticsData/Total successful", 1);
            }
            catch (Exception ex)
            {
                logWindow.NewLog($"Saving StaticsData/Total failed! {ex.Message}", 2);
            }
        }

        private void LoadTotalStatics()
        {
            loadTotalStaticsError = false;

            try
            {
                StaticsTotalData staTL = StaticsTotalData.Load();

                try
                {
                    startDate = staTL.startDate;
                }
                catch (Exception ex)
                {
                    logWindow.NewLog($"Error in Loading StaticsData/Total -> Reading startDate failed! {ex.Message}", 2);

                }
                try
                {
                    totalCustomers = staTL.totalCustomers;
                }
                catch (Exception ex)
                {
                    logWindow.NewLog($"Error in Loading StaticsData/Total -> Reading totalCustomers failed! {ex.Message}", 2);

                }
                try
                {
                    totalSoldProducts = staTL.totalSoldProducts;
                }
                catch (Exception ex)
                {
                    logWindow.NewLog($"Error in Loading StaticsData/Total -> Reading totalSoldProducts failed! {ex.Message}", 2);

                }
                try
                {
                    totalGottenCash = staTL.totalGottenCash;
                }
                catch (Exception ex)
                {
                    logWindow.NewLog($"Error in Loading StaticsData/Total -> Reading totalGottenCash failed! {ex.Message}", 2);

                }
                try
                {
                    totalLostCash = staTL.totalLostCash;
                }
                catch (Exception ex)
                {
                    logWindow.NewLog($"Error in Loading StaticsData/Total -> Reading totalLostProducts failed! {ex.Message}", 2);

                }
                try
                {
                    totalLostProducts = staTL.totalLostProducts;
                }
                catch (Exception ex)
                {
                    logWindow.NewLog($"Error in Loading StaticsData/Total -> Reading totalLostProducts failed! {ex.Message}", 2);

                }
                try
                {
                    productsNumberList = staTL.productsNumberList;
                }
                catch (Exception ex)
                {
                    logWindow.NewLog($"Error in Loading StaticsData/Total -> Reading productsNumberList failed! {ex.Message}", 2);

                }
                try
                {
                    productsNameList = staTL.productsNameList;
                }
                catch (Exception ex)
                {
                    logWindow.NewLog($"Error in Loading StaticsData/Total -> Reading productsNameList failed! {ex.Message}", 2);

                }
                try
                {
                    productsCashList = staTL.productsCashList;
                }
                catch (Exception ex)
                {
                    logWindow.NewLog($"Error in Loading StaticsData/Total -> Reading productsCashList failed! {ex.Message}", 2);

                }
                try
                {
                    productsSinglePriceList = staTL.productsSinglePriceList;
                }
                catch (Exception ex)
                {
                    logWindow.NewLog($"Error in Loading StaticsData/Total -> Reading productsSinglePriceList failed! {ex.Message}", 2);

                }

                for (int i = 0; i < 2; i++)
                {
                    try
                    {
                        totalPcTime[i] = staTL.totalPcTime[i];
                    }
                    catch (Exception ex)
                    {
                        logWindow.NewLog($"Reading StaticsData/Total/totalPcTime{i} failed! {ex.Message}", 2);

                    }

                    try
                    {
                        totalPcUsers[i] = staTL.totalPcUsers[i];
                    }
                    catch (Exception ex)
                    {
                        logWindow.NewLog($"Reading StaticsData/Total/totalPcUsers{i} failed! {ex.Message}", 2);

                    }
                }

                for (int i = 0; i < 5; i++)
                {
                    try
                    {
                        mostSoldProductsName[i] = staTL.mostSoldProductsName[i];
                    }
                    catch (Exception ex)
                    {
                        logWindow.NewLog($"Reading StaticsData/Total/mostSoldProductsName{i} failed! {ex.Message}", 2);

                    }

                    try
                    {
                        mostSoldProductsNumber[i] = staTL.mostSoldProductsNumber[i];
                    }
                    catch (Exception ex)
                    {
                        logWindow.NewLog($"Reading StaticsData/Total/mostSoldProductsNumber{i} failed! {ex.Message}", 2);

                    }

                    try
                    {
                        mostSoldProductsSinglePrice[i] = staTL.mostSoldProductsSinglePrice[i];
                    }
                    catch (Exception ex)
                    {
                        logWindow.NewLog($"Reading StaticsData/Total/mostSoldProductsSinglePrice{i} failed! {ex.Message}", 2);

                    }

                    try
                    {
                        highestEarningsProductsName[i] = staTL.highestEarningsProductsName[i];
                    }
                    catch (Exception ex)
                    {
                        logWindow.NewLog($"Reading StaticsData/Total/highestEarningsProductsName{i} failed! {ex.Message}", 2);

                    }

                    try
                    {
                        highestEarningsProductsNumber[i] = staTL.highestEarningsProductsNumber[i];
                    }
                    catch (Exception ex)
                    {
                        logWindow.NewLog($"Reading StaticsData/Total/highestEarningsProductsNumber{i} failed! {ex.Message}", 2);

                    }

                    try
                    {
                        highestEarningsProductsSinglePrice[i] = staTL.highestEarningsProductsSinglePrice[i];
                    }
                    catch (Exception ex)
                    {
                        logWindow.NewLog($"Reading StaticsData/Total/highestEarningsProductsSinglePrice{i} failed! {ex.Message}", 2);

                    }
                }

                for (int i = 0; i < productsNumberList.Length; i++)
                {

                }

                logWindow.NewLog($"Loading StaticsData/Total successful", 1);
            }
            catch (Exception ex)
            {
                logWindow.NewLog($"Loading StaticsData/Total failed! {ex.Message}", 2);
                loadTotalStaticsError = true;

            }
        }

        private void BuildTotalStatics()
        {
            try
            {
                StaticsStartTimeLbl.Content = startDate.ToShortDateString();
                StaticsAllCustomerLbl.Content = totalCustomers.ToString();
                StaticsAllSoldProductsNumberLbl.Content = totalSoldProducts.ToString();
                StaticsAllGottenCashNumberLbl.Content = totalGottenCash;
                for (int t = 0; t < 10; t++)
                {
                    bool endsWithSearchResult = totalGottenCash.ToString().EndsWith($",{t}", StringComparison.CurrentCultureIgnoreCase);
                    if (endsWithSearchResult == true)
                    {
                        StaticsAllGottenCashNumberLbl.Content += "0";
                    }
                }
                StaticsAllGottenCashNumberLbl.Content += "€";

                StaticsAllLostCashNumberLbl.Content = totalLostCash;
                for (int t = 0; t < 10; t++)
                {
                    bool endsWithSearchResult = totalLostCash.ToString().EndsWith($",{t}", StringComparison.CurrentCultureIgnoreCase);
                    if (endsWithSearchResult == true)
                    {
                        StaticsAllLostCashNumberLbl.Content += "0";
                    }
                }
                StaticsAllLostCashNumberLbl.Content += "€";

                StaticsAllLostProductsNumberLbl.Content = totalLostProducts.ToString();
                StaticsTotalPc1TimeLbl.Content = pcTime[0].Hours + "," + pcTime[0].Minutes;
                StaticsTotalPc2TimeLbl.Content = pcTime[1].Hours + "," + pcTime[1].Minutes;
                StaticsTotalPc1UserLbl.Content = pcUsers[0].ToString();
                StaticsTotalPc2UserLbl.Content = pcUsers[1].ToString();

                StaticsAllProductTypesListView.Items.Clear();
                for (int i = 0; i < productsNameList.Length; i++)
                {
                    if (!string.IsNullOrEmpty(productsNameList[i]))
                    {
                        StaticsAllProductTypesListView.Items.Add(productsNameList[i].ToString());
                    }
                }
                StaticsAllProductTypesNumberLbl.Content = productsNameList.Length.ToString();


                Label lbl;
                for (int i = 0; i < 5; i++)
                {
                    lbl = (Label)FindName($"StaticsTotalMostSoldProductsName{i}");
                    lbl.Content = mostSoldProductsName[i];
                    if (string.IsNullOrEmpty(mostSoldProductsName[i]))
                    {
                        lbl.Content = "n/a";
                    }

                    lbl = (Label)FindName($"StaticsTotalMostSoldProductsNumber{i}");
                    lbl.Content = mostSoldProductsNumber[i];

                    lbl = (Label)FindName($"StaticsTotalMostSoldProductsSinglePrice{i}");
                    lbl.Content = mostSoldProductsSinglePrice[i];
                    for (int t = 0; t < 10; t++)
                    {
                        bool endsWithSearchResult = mostSoldProductsSinglePrice[i].ToString().EndsWith($",{t}", StringComparison.CurrentCultureIgnoreCase);
                        if (endsWithSearchResult == true)
                        {
                            lbl.Content += "0";
                        }
                    }
                    lbl.Content += "€";

                    lbl = (Label)FindName($"StaticsTotalHighestEarningsProductsName{i}");
                    lbl.Content = highestEarningsProductsName[i];
                    if (string.IsNullOrEmpty(highestEarningsProductsName[i]))
                    {
                        lbl.Content = "n/a";
                    }

                    lbl = (Label)FindName($"StaticsTotalHighestEarningsProductsNumber{i}");
                    lbl.Content = highestEarningsProductsNumber[i];
                    for (int t = 0; t < 10; t++)
                    {
                        bool endsWithSearchResult = highestEarningsProductsNumber[i].ToString().EndsWith($",{t}", StringComparison.CurrentCultureIgnoreCase);
                        if (endsWithSearchResult == true)
                        {
                            lbl.Content += "0";
                        }
                    }
                    lbl.Content += "€";

                    lbl = (Label)FindName($"StaticsTotalHighestEarningsProductsSinglePrice{i}");
                    lbl.Content = highestEarningsProductsSinglePrice[i];
                    for (int t = 0; t < 10; t++)
                    {
                        bool endsWithSearchResult = highestEarningsProductsSinglePrice[i].ToString().EndsWith($",{t}", StringComparison.CurrentCultureIgnoreCase);
                        if (endsWithSearchResult == true)
                        {
                            lbl.Content += "0";
                        }
                    }
                    lbl.Content += "€";
                }
                logWindow.NewLog($"Building StaticsData/Total successful", 1);
            }
            catch (Exception ex)
            {
                logWindow.NewLog($"Building StaticsData/Total failed! {ex.Message}", 2);

            }
        }

        private void SaveDayStatics()
        {
            try
            {
                StaticsDayData staDS = new StaticsDayData
                {
                    SoldSlotCash = SoldSlotCash,
                    SoldSlotNumber = SoldSlotNumber,
                    SoldSlotName = SoldSlotName,
                    SoldSlotSinglePrice = SoldSlotSinglePrice,
                    LostCash = LostCash,
                    LostProducts = LostProducts,
                    pcTime = pcTime,
                    pcUsers = pcUsers,
                    userList = userList
                };
                staDS.Save();

                logWindow.NewLog($"Saving StaticsData/Day successful", 1);
            }
            catch (Exception ex)
            {
                logWindow.NewLog($"Saving StaticsData/Day failed! {ex.Message}", 2);

            }
        }

        private void LoadDayStatics(DateTime date)
        {
            loadDayStaticsError = false;

            try
            {
                StaticsDayData staDL = StaticsDayData.Load(date.Date);

                try
                {
                    SoldSlotCash = staDL.SoldSlotCash;
                }
                catch (Exception ex)
                {
                    logWindow.NewLog($"Error in Loading StaticsData/Day -> Reading SoldSlotCash failed! {ex.Message}", 2);

                    loadDayStaticsError = true;
                }
                try
                {
                    SoldSlotNumber = staDL.SoldSlotNumber;
                }
                catch (Exception ex)
                {
                    logWindow.NewLog($"Error in Loading StaticsData/Day -> Reading SoldSlotNumber failed! {ex.Message}", 2);

                    loadDayStaticsError = true;
                }
                try
                {
                    SoldSlotName = staDL.SoldSlotName;
                }
                catch (Exception ex)
                {
                    logWindow.NewLog($"Error in Loading StaticsData/Day -> Reading SoldSlotName failed! {ex.Message}", 2);

                    loadDayStaticsError = true;
                }
                try
                {
                    SoldSlotSinglePrice = staDL.SoldSlotSinglePrice;
                }
                catch (Exception ex)
                {
                    logWindow.NewLog($"Error in Loading StaticsData/Day -> Reading SoldSlotSinglePrice failed! {ex.Message}", 2);

                    loadDayStaticsError = true;
                }
                try
                {
                    LostCash = staDL.LostCash;
                }
                catch (Exception ex)
                {
                    logWindow.NewLog($"Error in Loading StaticsData/Day -> Reading LostCash failed! {ex.Message}", 2);

                    loadDayStaticsError = true;
                }
                try
                {
                    LostProducts = staDL.LostProducts;
                }
                catch (Exception ex)
                {
                    logWindow.NewLog($"Error in Loading StaticsData/Day -> Reading LostProducts failed! {ex.Message}", 2);

                    loadDayStaticsError = true;
                }
                try
                {
                    pcTime = staDL.pcTime;
                }
                catch (Exception ex)
                {
                    logWindow.NewLog($"Error in Loading StaticsData/Day -> Reading pcTime failed! {ex.Message}", 2);

                    loadDayStaticsError = true;
                }
                try
                {
                    pcUsers = staDL.pcUsers;
                }
                catch (Exception ex)
                {
                    logWindow.NewLog($"Error in Loading StaticsData/Day -> Reading pcUsers failed! {ex.Message}", 2);

                    loadDayStaticsError = true;
                }
                try
                {
                    userList = staDL.userList;
                }
                catch (Exception ex)
                {
                    logWindow.NewLog($"Error in Loading StaticsData/Day -> Reading userList failed! {ex.Message}", 2);

                    loadDayStaticsError = true;
                }
            }
            catch (Exception ex)
            {
                logWindow.NewLog($"Loading StaticsData/Day failed! {ex.Message}", 2);

                loadDayStaticsError = true;
            }
        }

        private void BuildDayStatics()
        {
            if (loadDayStaticsError == false)
            {
                try
                {
                    string labelPoint(ChartPoint chartPoint)
                    {
                        return string.Format("{0} ({1:P})", chartPoint.Y, chartPoint.Participation);
                    }

                    SeriesCollection numbers = new SeriesCollection();
                    SeriesCollection cashs = new SeriesCollection();

                    StaticsDaySlotPanel.Children.Clear();
                    StaticsDaySlotNamePanel.Children.Clear();
                    StaticsDaySoldPanel.Children.Clear();
                    StaticsDayCashPanel.Children.Clear();
                    StaticsDaySinglePricePanel.Children.Clear();

                    for (int i = 0; i < SoldSlotName.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(SoldSlotName[i]))
                        {
                            Label lbl = new Label
                            {
                                Name = $"StaticsDaySlot{i}SlotLbl",
                                Tag = i,
                                Content = i+1,
                                Margin = new Thickness(5, 5, 0, 5),
                                VerticalContentAlignment = VerticalAlignment.Center,
                                FontWeight = FontWeights.Bold
                            };

                            Label lbl1 = new Label
                            {
                                Name = $"StaticsDaySlot{i}NameLbl",
                                Tag = i,
                                Content = $"{SoldSlotName[i]}",
                                Margin = new Thickness(5, 5, 5, 5),
                                VerticalContentAlignment = VerticalAlignment.Center,
                            };

                            Label lbl2 = new Label
                            {
                                Name = $"StaticsDaySlot{i}SinglePriceLbl",
                                Tag = i,
                                Content = $"{SoldSlotSinglePrice[i]}",
                                Margin = new Thickness(5, 5, 5, 5),
                                VerticalContentAlignment = VerticalAlignment.Center,
                                HorizontalContentAlignment = HorizontalAlignment.Center,
                            };

                            PieSeries series = new PieSeries()
                            {
                                Title = "Produkt " + (i+1),
                                Values = new ChartValues<double> { SoldSlotNumber[i] },
                                DataLabels = true,
                                LabelPoint = labelPoint
                            };

                            for (int t = 0; t < 10; t++)
                            {
                                bool endsWithSearchResult = SoldSlotSinglePrice[i].ToString().EndsWith($",{t}", StringComparison.CurrentCultureIgnoreCase);
                                if (endsWithSearchResult == true)
                                {
                                    lbl2.Content += "0";
                                }
                            }
                            lbl2.Content += "€";

                            Label lbl3 = new Label
                            {
                                Name = $"StaticsDaySlot{i}NumberLbl",
                                Tag = i,
                                Margin = new Thickness(5, 5, 5, 5),
                                VerticalContentAlignment = VerticalAlignment.Center,
                                HorizontalContentAlignment = HorizontalAlignment.Center,
                                Content = $"{SoldSlotNumber[i]}",
                            };

                            Label lbl4 = new Label
                            {
                                Name = $"StaticsDaySlot{i}CashLbl",
                                Tag = i,
                                Margin = new Thickness(5, 5, 5, 5),
                                VerticalContentAlignment = VerticalAlignment.Center,
                                HorizontalContentAlignment = HorizontalAlignment.Center,
                                Content = $"{SoldSlotCash[i]}",
                            };

                            PieSeries series2 = new PieSeries()
                            {
                                Title = "Produkt " + (i+1),
                                Values = new ChartValues<double> { SoldSlotCash[i] },
                                DataLabels = true,
                                LabelPoint = labelPoint
                            };

                            for (int t = 0; t < 10; t++)
                            {
                                bool endsWithSearchResult = SoldSlotCash[i].ToString().EndsWith($",{t}", StringComparison.CurrentCultureIgnoreCase);
                                if (endsWithSearchResult == true)
                                {
                                    lbl4.Content += "0";
                                }
                            }
                            lbl4.Content += "€";

                            try
                            {
                                UnregisterName(lbl.Name);
                            }
                            catch { }
                            try
                            {
                                UnregisterName(lbl1.Name);
                            }
                            catch { }
                            try
                            {
                                UnregisterName(lbl2.Name);
                            }
                            catch { }
                            try
                            {
                                UnregisterName(lbl3.Name);
                            }
                            catch { }
                            try
                            {
                                UnregisterName(lbl4.Name);
                            }
                            catch { }

                            RegisterName(lbl.Name, lbl);
                            RegisterName(lbl1.Name, lbl1);
                            RegisterName(lbl2.Name, lbl2);
                            RegisterName(lbl3.Name, lbl3);
                            RegisterName(lbl4.Name, lbl4);

                            StaticsDaySlotPanel.Children.Add(lbl);
                            StaticsDaySlotNamePanel.Children.Add(lbl1);
                            StaticsDaySinglePricePanel.Children.Add(lbl2);
                            StaticsDaySoldPanel.Children.Add(lbl3);
                            StaticsDayCashPanel.Children.Add(lbl4);

                            cashs.Add(series2);
                            numbers.Add(series);
                        }
                    }

                    StaticsDayChartOfCash.Series = numbers;
                    StaticsDayChartOfNumbers.Series = cashs;

                    StaticsDayPc1TimeLbl.Content = pcTime[0].Hours.ToString();
                    StaticsDayPc2TimeLbl.Content = pcTime[1].Hours.ToString();
                    StaticsDayPc1UsersLbl.Content = pcUsers[0].ToString();
                    StaticsDayPc2UsersLbl.Content = pcUsers[1].ToString();

                    StaticsDayPcUsersChart_PC1.Values = new ChartValues<int> { pcUsers[0] };
                    StaticsDayPcUsersChart_PC2.Values = new ChartValues<int> { pcUsers[1] };
                    StaticsDayPcTimeChart_PC1.Values = new ChartValues<int> { pcTime[0].Hours };
                    StaticsDayPcTimeChart_PC2.Values = new ChartValues<int> { pcTime[1].Hours };

                    double gottenCash = 0;
                    int soldProducts = 0;
                    for (int i = 0; i < SoldSlotName.Length; i++)
                    {
                        gottenCash += SoldSlotCash[i];
                        soldProducts += SoldSlotNumber[i];
                    }
                    StaticsDayGottenCashNumberLbl.Content = $"{gottenCash}";
                    for (int t = 0; t < 10; t++)
                    {
                        bool endsWithSearchResult = gottenCash.ToString().EndsWith($",{t}", StringComparison.CurrentCultureIgnoreCase);
                        if (endsWithSearchResult == true)
                        {
                            StaticsDayGottenCashNumberLbl.Content += "0";
                        }
                    }
                    StaticsDayGottenCashNumberLbl.Content += "€";

                    StaticsDaySoldProductsNumberLbl.Content = $"{soldProducts}";
                    StaticsDayLostProductsNumberLbl.Content = $"{LostProducts}";
                    StaticsDayLostCashNumberLbl.Content = $"{LostCash}";
                    for (int t = 0; t < 10; t++)
                    {
                        bool endsWithSearchResult = LostCash.ToString().EndsWith($",{t}", StringComparison.CurrentCultureIgnoreCase);
                        if (endsWithSearchResult == true)
                        {
                            StaticsDayLostCashNumberLbl.Content += "0";
                        }
                    }
                    StaticsDayLostCashNumberLbl.Content += "€";

                    logWindow.NewLog($"Building StaticsData/Day successful", 1);
                }
                catch (Exception ex)
                {
                    logWindow.NewLog($"Building StaticsData/Day failed! {ex.Message}", 2);

                }
            }
        }

        private void StaticsDayYearUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            StaticsDaySelectionTimer.Stop();
            StaticsDaySelectionTimer.Start();
        }

        private void StaticsDayMonthUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            StaticsDaySelectionTimer.Stop();
            StaticsDaySelectionTimer.Start();
        }

        private void StaticsDaySelectionTimer_Tick(object sender, EventArgs e)
        {
            StaticsDaySelectionTimer.Stop();
            string year = StaticsDayYearUpDown.Value.ToString();
            string month = StaticsDayMonthUpDown.Value.ToString();
            StaticsDayDateListBox.Items.Clear();

            for (int i = 1; i < 32; i++)
            {
                if (File.Exists(pathN.staticsFile + $"\\{i}_{month}_{year}.xml"))
                {
                    Label lbl = new Label
                    {
                        Content = $"{i}.{month}.{year}",
                        Tag = i
                    };
                    lbl.MouseDown += new MouseButtonEventHandler(StaticsDayDateLbl_MouseDown);
                    StaticsDayDateListBox.Items.Add(lbl);
                }
            }
        }

        private void StaticsDayDateLbl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Label lbl)
            {
                DateTime date = new DateTime(int.Parse(StaticsDayYearUpDown.Value.ToString()), int.Parse(StaticsDayMonthUpDown.Value.ToString()), int.Parse(lbl.Tag.ToString()));
                LoadDayStatics(date.Date);
                BuildDayStatics();
            }
        }
        #endregion

        #region Settings
        private void LoadSettings()
        {
            try
            {
                SettingsData setL = SettingsData.Load();
                try
                {
                    lastPayDate = setL.lastPayDate;
                }
                catch (Exception ex)
                {
                    logWindow.NewLog($"Error in Loading Settings -> Reading lastPayDate failded! {ex.Message}", 2);
                }
                try
                {
                    displayLogTypes = setL.displayLogTypes;
                }
                catch (Exception ex)
                {
                    logWindow.NewLog($"Error in Loading Settings -> Reading displayLogTypes failded! {ex.Message}", 2);
                }

                try
                {
                    AppTheme = setL.AppTheme;
                }
                catch (Exception ex)
                {
                    logWindow.NewLog($"Error in Loading Settings -> Reading AppTheme failded! {ex.Message}", 2);
                }

                logWindow.NewLog($"Loading Settings successful", 1);
            }
            catch (Exception ex)
            {
                logWindow.NewLog($"Loading Settings failed! {ex.Message}", 2);

            }
        }

        private void SaveSettings()
        {
            SettingsData setS = new SettingsData();

            try
            {
                setS.lastPayDate = lastPayDate;
                setS.displayLogTypes = displayLogTypes;
                setS.AppTheme = AppTheme;
                setS.Save();
                logWindow.NewLog($"Saving Settings successful", 1);
                logWindow.NewLog($"Settings changed!", 4);
            }
            catch (Exception ex)
            {
                logWindow.NewLog($"Saving Settings failded! {ex.Message}", 2);

            }
        }

        private void BuildSettings()
        {
            try
            {
                logWindow.displayLogTypes = displayLogTypes;
                SettingsLogType0CheckBox.IsChecked = displayLogTypes[0];
                SettingsLogType1CheckBox.IsChecked = displayLogTypes[1];
                SettingsLogType2CheckBox.IsChecked = displayLogTypes[2];
                SettingsLogType3CheckBox.IsChecked = displayLogTypes[3];
                SettingsLogType4CheckBox.IsChecked = displayLogTypes[4];

                logWindow.NewLog($"Building Settings successful", 1);
            }
            catch (Exception ex)
            {
                logWindow.NewLog($"Building Settings failded! {ex.Message}", 2);

            }
        }

        private void SettingsSaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Möchten Sie die Einstellungen wirklich speichern?", "Einstellungen speichern", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                displayLogTypes[0] = SettingsLogType0CheckBox.IsChecked.Value;
                displayLogTypes[1] = SettingsLogType1CheckBox.IsChecked.Value;
                displayLogTypes[2] = SettingsLogType2CheckBox.IsChecked.Value;
                displayLogTypes[3] = SettingsLogType3CheckBox.IsChecked.Value;
                displayLogTypes[4] = SettingsLogType4CheckBox.IsChecked.Value;

                SaveSettings();
                LoadSettings();
                BuildSettings();
            }
        }

        private void SettingsCancelBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadSettings();
            BuildSettings();
        }

        private void SettingsRestoreBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Möchten Sie die Einstellungen wirklich auf Werkseinstellungen zurücksetzen?", "Einstellungen wiederherstellen", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                logWindow.NewLog("Restoring defaults...", 4);

                lastPayDate = DateTime.Today;

                displayLogTypes[0] = true;
                displayLogTypes[1] = false;
                displayLogTypes[2] = true;
                displayLogTypes[3] = false;
                displayLogTypes[4] = true;

                SaveSettings();
                LoadSettings();
                BuildSettings();

                logWindow.NewLog("Restored defaults!", 4);
            }
        }

        private void StartInstallationBtn_Click(object sender, RoutedEventArgs e)
        {
            logWindow.NewLog("Starting installation...", 4);

            logWindow.NewLog("Setting up Settings...", 3);
            lastPayDate = DateTime.Today.AddDays(-1);
            displayLogTypes[0] = true;
            displayLogTypes[1] = false;
            displayLogTypes[2] = true;
            displayLogTypes[3] = false;
            displayLogTypes[4] = true;
            SaveSettings();

            logWindow.NewLog("Setting up Storage...", 3);
            Array.Resize(ref StorageSlotName, StorageSlotName.Length + 1);
            StorageSlotName[StorageSlotName.Length - 1] = "Default";

            Array.Resize(ref StorageSlotStatus, StorageSlotStatus.Length + 1);
            StorageSlotStatus[StorageSlotStatus.Length - 1] = false;

            Array.Resize(ref StorageSlotNumber, StorageSlotNumber.Length + 1);
            StorageSlotNumber[StorageSlotNumber.Length - 1] = 0;

            Array.Resize(ref StorageSlotPrice, StorageSlotPrice.Length + 1);
            StorageSlotPrice[StorageSlotPrice.Length - 1] = 0;
            SaveStorage();

            logWindow.NewLog("Setting up TotalStatics...", 3);
            startDate = DateTime.Today;
            totalCustomers = 0;
            totalSoldProducts = 0;
            totalGottenCash = 0;
            totalLostCash = 0;
            totalLostProducts = 0;
            SaveTotalStatics();

            CommissioningModePanel.Visibility = Visibility.Collapsed;
            TopBar.IsEnabled = true;
            commissioningMode = false;
            MessageBox.Show("Die Anwendung wurde aktiviert!", GetAssemblyTitle(), MessageBoxButton.OK, MessageBoxImage.Information);
            Reload();
        }

        private void SendLog_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                logWindow.SendEmail();
                logWindow.NewLog("Fertig", 2);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Der Log-Bericht konnte nicht an T-App Germany übermittelt werden. Bitte überprüfen Sie Ihre Internetverbindung!", "ACHTUNG: PCS Fehlerbeicht", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
                logWindow.NewLog($"Senden: {ex.Message}", 2);

            }
        }

        private void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            Reload();
        }
        #endregion
    }
}