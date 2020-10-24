using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Management;
using System.Reflection;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
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
    public partial class MainWindow : IDisposable
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
        private enum AppTheme
        {
            Light,
            Dark
        }
        private enum Pages
        {
            Home,
            Settings,
            Help,
            Shop,
            TimeManager,
            Statics,
            UserManager,
            Storage,
            FileManager
        }

        private DispatcherTimer MessageTimer { get; set; } = new DispatcherTimer();
        private DispatcherTimer StaticsDaySelectionTimer { get; set; } = new DispatcherTimer();
        private DispatcherTimer SideObjectsAniTimer { get; set; } = new DispatcherTimer();
        private BackgroundWorker StorageLoader { get; set; } = new BackgroundWorker();
        private BackgroundWorker StorageSaver { get; set; } = new BackgroundWorker();
        private BackgroundWorker TotalStaticsLoader { get; set; } = new BackgroundWorker();
        private BackgroundWorker TotalStaticsSaver { get; set; } = new BackgroundWorker();
        private BackgroundWorker DayStaticsLoader { get; set; } = new BackgroundWorker();
        private BackgroundWorker DayStaticsSaver { get; set; } = new BackgroundWorker();
        private BackgroundWorker SettingsLoader { get; set; } = new BackgroundWorker();
        private BackgroundWorker SettingsSaver { get; set; } = new BackgroundWorker();
        public object RegistryKeyPath { get; private set; }

        private readonly LogWindow log = new LogWindow();
        private readonly PathName pathN = new PathName();
        private readonly ThemeData themeData = new ThemeData();
        private DateTime AppInstallStart;
        private DateTime lastPayDate = DateTime.Today.AddDays(-5);
        private bool AppInstallationMode = false;
        private string AppThemeStr = "System";
        private AppTheme Theme;
        private bool AppThemeChanged = false;
        private readonly Random rnd = new Random();
        private short SideObjectsCount = -1;
        private Pages WindowPage = Pages.Home;
        private int MsgNumber = -1;
        private short MsgItemTarget = -1;

        private short StorageLimitedNumber = 10;
        private bool[] StorageSlotStatus = Array.Empty<bool>();
        private short[] StorageSlotNumber = Array.Empty<short>();
        private double[] StorageSlotPrice = Array.Empty<double>();
        private string[] StorageSlotName = Array.Empty<string>();
        private short StorageSelectedCount = 0;
        private bool[] StorageSelectedArray = Array.Empty<bool>();
        private short[] InStorageSlots = Array.Empty<short>();

        private short selectedItemsInt = 0;
        private short[] ShopSlotSelectedNumber = Array.Empty<short>();
        private string ShopTask = "";
        private double ShopMainPrice = 0;

        private string[] StaticsDaySoldSlotName = Array.Empty<string>();
        private short[] StaticsDaySoldSlotNumber = Array.Empty<short>();
        private double[] StaticsDaySoldSlotCash = Array.Empty<double>();
        private double[] StaticsDaySoldSlotSinglePrice = Array.Empty<double>();
        private short StaticsDayLostProducts;
        private double StaticsDayLostCash;

        private TimeSpan[] StaticsDayPcUsage = Array.Empty<TimeSpan>();
        private short[] StaticsDayPcUsers = Array.Empty<short>();
        private string[] StaticsDayPcName = Array.Empty<string>();
        private readonly TimeSpan[] StaticsTotalPcUsage = Array.Empty<TimeSpan>();
        private readonly short[] StaticsTotalPcUsers = Array.Empty<short>();
        private readonly string[] StaticsTotalPcName = Array.Empty<string>();

        private DateTime StaticsTotalStartDate;
        private int StaticsTotalCustomers;
        private int StaticsTotalSoldProducts;
        private double StaticsTotalGottenCash;
        private int StaticsTotalLostProducts;
        private double StaticsTotalLostCash;

        private readonly string[] mostSoldProductsName = new string[5];
        private readonly string[] highestEarningsProductsName = new string[5];
        private readonly short[] mostSoldProductsNumber = new short[5];
        private readonly double[] highestEarningsProductsNumber = new double[5];
        private readonly double[] mostSoldProductsSinglePrice = new double[5];
        private readonly double[] highestEarningsProductsSinglePrice = new double[5];

        private string[] productsNameList = Array.Empty<string>();
        private short[] productsNumberList = Array.Empty<short>();
        private double[] productsCashList = Array.Empty<double>();
        private double[] productsSinglePriceList = Array.Empty<double>();

        private readonly DateTime ApplicationStart;
        private DateTime StorageSaverStart;
        private DateTime StorageLoaderStart;
        private DateTime StorageBuildStart;
        private DateTime ShopBuildingStart;
        private DateTime ShopSellProductStart;
        private DateTime ShopSellStart;
        private DateTime TotalStaticsSaveStart;
        private DateTime TotalStaticsLoadStart;
        private DateTime TotalStaticsBuildStart;
        private DateTime DayStaticsLoadStart;
        private DateTime DayStaticsBuildStart;
        private DateTime SettingsSaveStart;
        private DateTime SettingsBuildStart;
        private bool disposedValue;
        private DateTime SettingsLoadStart;

        //private string[] planedUserNameList = Array.Empty<string>();
        //private int[] plannedUserStartTimeList = Array.Empty<int>();
        //private double[] plannedUserDurationList = Array.Empty<double>();
        #endregion

        #region Window
        public MainWindow()
        {
            ApplicationStart = DateTime.Now;
            InitializeComponent();
            AssemblyLbl.Content = $"{GetAssemblyProduct()}\t\tVersion {GetAssemblyVersion()}\t\t{GetAssemblyCopyright()}\t\t{GetAssemblyCompany()}";
            Title = GetAssemblyTitle();
            Page1HeaderLbl.Content = "Willkommen bei " + GetAssemblyTitle();

            MessageTimer.Tick += new EventHandler(MessageTimer_Tick);
            MessageTimer.Interval = new TimeSpan(0, 0, 0, 10);

            StaticsDaySelectionTimer.Tick += new EventHandler(StaticsDaySelectionTimer_Tick);
            StaticsDaySelectionTimer.Interval = new TimeSpan(0, 0, 0, 1, 5);

            SideObjectsAniTimer.Tick += new EventHandler(AniTimer_Tick);
            SideObjectsAniTimer.Interval = new TimeSpan(0, 0, 0, 2);

            StorageLoader.DoWork += new DoWorkEventHandler(StorageLoaderRun);
            StorageLoader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(StorageLoadComplete);
            StorageLoader.ProgressChanged += new ProgressChangedEventHandler(StorageLoaderProgress);

            StorageSaver.DoWork += new DoWorkEventHandler(StorageStorageSaverRun);
            StorageSaver.RunWorkerCompleted += new RunWorkerCompletedEventHandler(StorageSaverComplete);
            StorageSaver.ProgressChanged += new ProgressChangedEventHandler(StorageSaverProgress);

            TotalStaticsLoader.DoWork += new DoWorkEventHandler(TotalStaticsLoaderRun);
            TotalStaticsLoader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(TotalStaticsLoaderComplete);
            TotalStaticsLoader.ProgressChanged += new ProgressChangedEventHandler(TotalStaticsLoaderProgress);

            TotalStaticsSaver.DoWork += new DoWorkEventHandler(TotalStaticsSaverRun);
            TotalStaticsSaver.RunWorkerCompleted += new RunWorkerCompletedEventHandler(TotalStaticsSaverComplete);
            TotalStaticsSaver.ProgressChanged += new ProgressChangedEventHandler(TotalStaticsSaverProgress);

            DayStaticsLoader.DoWork += new DoWorkEventHandler(DayStaticsLoaderRun);
            DayStaticsLoader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(DayStaticsLoaderComplete);
            DayStaticsLoader.ProgressChanged += new ProgressChangedEventHandler(DayStaticsLoaderProgress);

            DayStaticsSaver.DoWork += new DoWorkEventHandler(DayStaticsSaverRun);
            DayStaticsSaver.RunWorkerCompleted += new RunWorkerCompletedEventHandler(DayStaticsSaverComplete);
            DayStaticsSaver.ProgressChanged += new ProgressChangedEventHandler(DayStaticsSaverProgress);

            SettingsSaver.DoWork += new DoWorkEventHandler(SettingsSaverRun);
            SettingsSaver.RunWorkerCompleted += new RunWorkerCompletedEventHandler(SettingsSaverComplete);
            SettingsSaver.ProgressChanged += new ProgressChangedEventHandler(SettingsSaverProgress);

            SettingsLoader.DoWork += new DoWorkEventHandler(SettingsLoaderRun);
            SettingsLoader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(SettingsLoaderComplete);
            SettingsLoader.ProgressChanged += new ProgressChangedEventHandler(SettingsLoaderProgress);

            Directory.CreateDirectory(Path.GetDirectoryName(pathN.settingsFile));
            Directory.CreateDirectory(Path.GetDirectoryName(pathN.productsFile));
            Directory.CreateDirectory(Path.GetDirectoryName(pathN.staticsTotalFile));
            Directory.CreateDirectory(Path.GetDirectoryName(pathN.staticsDayFile));
            Directory.CreateDirectory(Path.GetDirectoryName(pathN.graphicsFile));

            MsgPanel.Children.Clear();

            if (!File.Exists($"{pathN.settingsFile}Settings.xml") && !File.Exists($"{pathN.settingsFile}Storage.xml") && !File.Exists($"{pathN.settingsFile}TotalStatics.xml"))
            {
                AppInstallationMode = true;
                InstallationMode();
            }

            if (AppInstallationMode == false)
            {
                Reload();
            }

            PageChange(Pages.Home);
            SetAppTheme();
            CloseShop();
            RefreshMaximizeRestoreButton();
            WatchTheme();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SideObjectsAniTimer.Start();
            StaticsDayYearUpDown.Value = DateTime.Now.Year;
            StaticsDayYearUpDown.Maximum = DateTime.Now.Year;
            StaticsDayMonthUpDown.Value = DateTime.Now.Month;
            log.NewLog(LogWindow.LogTypes.Info, LogWindow.LogThread.Anwendung, LogWindow.LogActions.Geladen, null, DateTime.Now, DateTime.Now - ApplicationStart);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (MessageBox.Show($"Möchten Sie {Title} wirklich schließen?", "Anwendung schließen", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                log.NewLog(LogWindow.LogTypes.Info, LogWindow.LogThread.Anwendung, LogWindow.LogActions.Herunterfahren, null, DateTime.Now, TimeSpan.Zero);
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

        private void WatchTheme()
        {
            WindowsIdentity currentUser = WindowsIdentity.GetCurrent();
            string query = string.Format(CultureInfo.InvariantCulture, @"SELECT * FROM RegistryValueChangeEvent WHERE Hive = 'HKEY_USERS' AND KeyPath = '{0}\\{1}' AND ValueName = '{2}'",
                currentUser.User.Value, themeData.RegistryKeyPath.Replace(@"\", @"\\"), themeData.RegistryValueName);

            try
            {
                ManagementEventWatcher watcher = new ManagementEventWatcher(query);
                watcher.EventArrived += (sender, args) =>
                {
                    if (AppThemeStr == "System")
                    {
                        AppThemeChanged = true;
                    }
                };

                watcher.Start();
            }
            catch (Exception ex)
            {
                log.NewLog(LogWindow.LogTypes.Warnung, LogWindow.LogThread.WindowsAppThema, LogWindow.LogActions.Lesen, ex.Message, DateTime.Now, TimeSpan.Zero);
            }
        }
        #endregion

        #region Timer
        private void MessageTimer_Tick(object sender, EventArgs e)
        {

        }

        private void AniTimer_Tick(object sender, EventArgs e)
        {
            SideObjectsCount++;

            if (SideObjectsCount <= 10)
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
                        Name = $"cloud{SideObjectsCount}",
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

                    if (AppThemeStr == "System" && AppThemeChanged == true)
                    {
                        SetAppTheme();
                    }
                }
                catch (Exception)
                {

                }
            }
            else
            {
                SideObjectsAniTimer.Stop();

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

                    for (int i = 0; i < SideObjectsCount; i++)
                    {
                        Image img = (Image)FindName($"cloud{i}");
                        img.BeginAnimation(OpacityProperty, ani);
                        UnregisterName($"cloud{i}");
                    }
                }
                catch { }

                SideObjectsCount = -1;
            }
        }

        private void FadeAni_Completed(object sender, EventArgs e)
        {
            SideObjectsAniTimer.Start();
            SideBar.Children.Clear();
        }
        #endregion

        #region Funktionen
        private void InstallationMode()
        {
            InstallationModePanel.Visibility = Visibility.Visible;
            TopBar.IsEnabled = false;
        }

        private void PageChange(Pages newPage)
        {
            WindowPage = newPage;

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
                case Pages.Home:
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
                case Pages.Settings:
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
                case Pages.Help:
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
                case Pages.Shop:
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
                case Pages.TimeManager:
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
                case Pages.Statics:
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
                case Pages.UserManager:
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
                case Pages.Storage:
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
                case Pages.FileManager:
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
            }
        }

        private void PageFadeOut_Comleted(object sender, EventArgs e)
        {
            switch (WindowPage)
            {
                case Pages.Home:
                    Page2Grid.Visibility = Visibility.Collapsed;
                    Page3Grid.Visibility = Visibility.Collapsed;
                    Page4Grid.Visibility = Visibility.Collapsed;
                    Page5Grid.Visibility = Visibility.Collapsed;
                    Page6Grid.Visibility = Visibility.Collapsed;
                    Page7Grid.Visibility = Visibility.Collapsed;
                    Page8Grid.Visibility = Visibility.Collapsed;
                    Page9Grid.Visibility = Visibility.Collapsed;
                    break;
                case Pages.Settings:
                    Page1Grid.Visibility = Visibility.Collapsed;
                    Page3Grid.Visibility = Visibility.Collapsed;
                    Page4Grid.Visibility = Visibility.Collapsed;
                    Page5Grid.Visibility = Visibility.Collapsed;
                    Page6Grid.Visibility = Visibility.Collapsed;
                    Page7Grid.Visibility = Visibility.Collapsed;
                    Page8Grid.Visibility = Visibility.Collapsed;
                    Page9Grid.Visibility = Visibility.Collapsed;
                    break;
                case Pages.Help:
                    Page1Grid.Visibility = Visibility.Collapsed;
                    Page2Grid.Visibility = Visibility.Collapsed;
                    Page4Grid.Visibility = Visibility.Collapsed;
                    Page5Grid.Visibility = Visibility.Collapsed;
                    Page6Grid.Visibility = Visibility.Collapsed;
                    Page7Grid.Visibility = Visibility.Collapsed;
                    Page8Grid.Visibility = Visibility.Collapsed;
                    Page9Grid.Visibility = Visibility.Collapsed;
                    break;
                case Pages.Shop:
                    Page1Grid.Visibility = Visibility.Collapsed;
                    Page2Grid.Visibility = Visibility.Collapsed;
                    Page3Grid.Visibility = Visibility.Collapsed;
                    Page5Grid.Visibility = Visibility.Collapsed;
                    Page6Grid.Visibility = Visibility.Collapsed;
                    Page7Grid.Visibility = Visibility.Collapsed;
                    Page8Grid.Visibility = Visibility.Collapsed;
                    Page9Grid.Visibility = Visibility.Collapsed;
                    break;
                case Pages.TimeManager:
                    Page1Grid.Visibility = Visibility.Collapsed;
                    Page2Grid.Visibility = Visibility.Collapsed;
                    Page3Grid.Visibility = Visibility.Collapsed;
                    Page4Grid.Visibility = Visibility.Collapsed;
                    Page6Grid.Visibility = Visibility.Collapsed;
                    Page7Grid.Visibility = Visibility.Collapsed;
                    Page8Grid.Visibility = Visibility.Collapsed;
                    Page9Grid.Visibility = Visibility.Collapsed;
                    break;
                case Pages.Statics:
                    Page1Grid.Visibility = Visibility.Collapsed;
                    Page2Grid.Visibility = Visibility.Collapsed;
                    Page3Grid.Visibility = Visibility.Collapsed;
                    Page4Grid.Visibility = Visibility.Collapsed;
                    Page5Grid.Visibility = Visibility.Collapsed;
                    Page7Grid.Visibility = Visibility.Collapsed;
                    Page8Grid.Visibility = Visibility.Collapsed;
                    Page9Grid.Visibility = Visibility.Collapsed;
                    break;
                case Pages.UserManager:
                    Page1Grid.Visibility = Visibility.Collapsed;
                    Page2Grid.Visibility = Visibility.Collapsed;
                    Page3Grid.Visibility = Visibility.Collapsed;
                    Page4Grid.Visibility = Visibility.Collapsed;
                    Page5Grid.Visibility = Visibility.Collapsed;
                    Page6Grid.Visibility = Visibility.Collapsed;
                    Page8Grid.Visibility = Visibility.Collapsed;
                    Page9Grid.Visibility = Visibility.Collapsed;
                    break;
                case Pages.Storage:
                    Page1Grid.Visibility = Visibility.Collapsed;
                    Page2Grid.Visibility = Visibility.Collapsed;
                    Page3Grid.Visibility = Visibility.Collapsed;
                    Page4Grid.Visibility = Visibility.Collapsed;
                    Page5Grid.Visibility = Visibility.Collapsed;
                    Page6Grid.Visibility = Visibility.Collapsed;
                    Page7Grid.Visibility = Visibility.Collapsed;
                    Page9Grid.Visibility = Visibility.Collapsed;
                    break;
                case Pages.FileManager:
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
            SettingsLoader.RunWorkerAsync();
            StorageLoader.RunWorkerAsync();
            TotalStaticsLoader.RunWorkerAsync();
            DayStaticsLoader.RunWorkerAsync(DateTime.Now.Date);
        }

        private void MainProgBarShow()
        {
            DoubleAnimation ani = new DoubleAnimation
            {
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
                To = 0,
                Duration = TimeSpan.FromSeconds(1),
                EasingFunction = new QuarticEase()
            };

            MainProgBar.BeginAnimation(HeightProperty, ani);
        }

        private void SetAppTheme()
        {
            if (AppThemeStr == "System")
            {
                string initialTheme = themeData.GetWindowsAppTheme().ToString();
                if (initialTheme == "Dark")
                {
                    ApplyDarkTheme();
                }
                else if (initialTheme == "Light")
                {
                    ApplyLightTheme();
                }
            }
            else if (AppThemeStr == "Light")
            {
                ApplyLightTheme();
            }
            else if (AppThemeStr == "Dark")
            {
                ApplyDarkTheme();
            }

            log.SetAppTheme();
        }

        private void ApplyLightTheme()
        {
            TextFontColor.Background = themeData.GetLightTheme("TextFontColor");
            SideBarsColor.Background = themeData.GetLightTheme("SideBarsColor");
            MainMenuGrid.Background = themeData.GetLightTheme("MainMenuGrid");
            PageBackgroudColor.Background = themeData.GetLightTheme("PageBackgroudColor");
            SeperatorColor.Background = themeData.GetLightTheme("SeperatorColor");
            ChromeBtnColor.Background = themeData.GetLightTheme("ChromeBtnColor");
            Theme = AppTheme.Light;

            StorageBuilder();
            BuildShop(AppThemeStr, "storage");
        }

        private void ApplyDarkTheme()
        {
            TextFontColor.Background = themeData.GetDarkTheme("TextFontColor");
            SideBarsColor.Background = themeData.GetDarkTheme("SideBarsColor");
            MainMenuGrid.Background = themeData.GetDarkTheme("MainMenuGrid");
            PageBackgroudColor.Background = themeData.GetDarkTheme("PageBackgroudColor");
            SeperatorColor.Background = themeData.GetDarkTheme("SeperatorColor");
            ChromeBtnColor.Background = themeData.GetDarkTheme("ChromeBtnColor");
            Theme = AppTheme.Dark;

            StorageBuilder();
            BuildShop(AppThemeStr, "storage");
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

        private void NewMsgItem(LogWindow.LogTypes type, LogWindow.LogThread thread, string bodyText, string ex)
        {
            MsgNumber++;

            if (type == LogWindow.LogTypes.Fehler)
            {
                MsgItemTemp.BorderBrush = Brushes.Red;
                MsgItemBackTemp.Background = Brushes.Red;
            }
            else if (type == LogWindow.LogTypes.Erfolg)
            {
                MsgItemTemp.BorderBrush = Brushes.Green;
                MsgItemBackTemp.Background = Brushes.Green;
            }
            else if (type == LogWindow.LogTypes.Warnung)
            {
                MsgItemTemp.BorderBrush = Brushes.Orange;
                MsgItemBackTemp.Background = Brushes.Orange;
            }
            else if (type == LogWindow.LogTypes.Info)
            {
                MsgItemTemp.BorderBrush = Brushes.DarkCyan;
                MsgItemBackTemp.Background = Brushes.DarkCyan;
            }

            MsgItemBodyTemp.Text = bodyText;
            MsgItemHeaderTemp.Text = thread.ToString();


            MsgItemBodyTemp.Foreground = TextFontColor.Background;
            MsgItemHeaderTemp.Foreground = TextFontColor.Background;

            if (!string.IsNullOrEmpty(ex))
            {
                MsgItemBodyTemp.Inlines.Add(new LineBreak());
                MsgItemBodyTemp.Inlines.Add(new Run(ex) { FontStyle = FontStyles.Italic });
            }

            string xamlString = XamlWriter.Save(MsgItemTemp);
            StringReader stringReader = new StringReader(xamlString);
            XmlReader xmlReader = XmlReader.Create(stringReader);
            Border item = (Border)XamlReader.Load(xmlReader);
            item.Name = $"MsgItem{MsgNumber}";
            item.Opacity = 0;
            item.Visibility = Visibility.Visible;
            Grid grid = (Grid)item.Child;
            Grid prog = (Grid)grid.Children[3];
            DoubleAnimation ani1 = new DoubleAnimation
            {
                To = 1,
                Duration = TimeSpan.FromSeconds(1),
            };
            DoubleAnimation ani = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromSeconds(6),
            };
            ani.Completed += new EventHandler(MsgTimeOut);

            MsgPanel.Children.Insert(0, item);
            MsgItemTarget++;
            item.BeginAnimation(OpacityProperty, ani1);
            prog.BeginAnimation(WidthProperty, ani);
        }

        private void MsgTimeOut(object sender, EventArgs e)
        {
            Border item = (Border)MsgPanel.Children[MsgItemTarget];
            MsgItemTarget--;

            DoubleAnimation ani = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromSeconds(1),
                EasingFunction = new PowerEase(),
            };
            ani.Completed += new EventHandler(MsgHidden);
            item.BeginAnimation(OpacityProperty, ani);
        }

        private void MsgHidden(object sender, EventArgs e)
        {
            MsgPanel.Children.RemoveAt(MsgPanel.Children.Count - 1);
        }
        #endregion

        #region Menu
        private void MessageListClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void OpenLog(object sender, RoutedEventArgs e)
        {
            log.AppThemeStr = AppThemeStr;
            log.SetAppTheme();
            log.ShowInTaskbar = true;
            log.ShowActivated = true;
            log.WindowState = System.Windows.WindowState.Normal;
            log.Show();
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
                        PageChange(Pages.Home);
                        break;
                    case 2:
                        PageChange(Pages.Settings);
                        break;
                    case 3:
                        PageChange(Pages.Help);
                        break;
                    case 4:
                        PageChange(Pages.Shop);
                        break;
                    case 5:
                        PageChange(Pages.TimeManager);
                        break;
                    case 6:
                        PageChange(Pages.Statics);
                        break;
                    case 7:
                        PageChange(Pages.UserManager);
                        break;
                    case 8:
                        PageChange(Pages.Storage);
                        break;
                    case 9:
                        PageChange(Pages.FileManager);
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
                if (ShopTask == "sell")
                {
                    maxCount = StorageSlotNumber[tag];
                }
                else
                {
                    maxCount = StaticsDaySoldSlotNumber[tag];
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
                if (ShopTask == "sell")
                {
                    maxCount = StorageSlotNumber[tag];
                }
                else
                {
                    maxCount = StaticsDaySoldSlotNumber[tag];
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

        private void BuildShop(string theme, string from)
        {
            ShopBuildingStart = DateTime.Now;
            int length = 0;
            string name = "";
            short number = 0;
            double price = 0;
            bool status = false;

            if (from == "storage")
            {
                length = InStorageSlots.Length;
                Array.Resize(ref ShopSlotSelectedNumber, InStorageSlots.Length);
            }
            else
            {
                length = StaticsDaySoldSlotSinglePrice.Length;
                Array.Resize(ref ShopSlotSelectedNumber, StaticsDaySoldSlotSinglePrice.Length);
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
                            name = StaticsDaySoldSlotName[i];
                            number = StaticsDaySoldSlotNumber[i];
                            price = StaticsDaySoldSlotSinglePrice[i];
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

                    Array.Resize(ref ShopSlotSelectedNumber, InStorageSlots.Length);

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
                            if (theme == "Light")
                            {
                                slotScp.Background = Brushes.LightGray;
                            }
                            else
                            {
                                slotScp.Background = (Brush)new BrushConverter().ConvertFrom("#FF555555");
                            }
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
                log.NewLog(LogWindow.LogTypes.Erfolg, LogWindow.LogThread.Kasse, LogWindow.LogActions.Bauen, null, DateTime.Now, DateTime.Now - ShopBuildingStart);
            }
            catch (Exception ex)
            {
                log.NewLog(LogWindow.LogTypes.Fehler, LogWindow.LogThread.Kasse, LogWindow.LogActions.Bauen, ex.Message, DateTime.Now, DateTime.Now - ShopBuildingStart);
            }

            NewPurchaseBtn.IsEnabled = true;
            ShopNoProductsFoundLbl.Visibility = Visibility.Collapsed;
            ShopProductsGrid.Visibility = Visibility.Visible;
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
            if (ShopTask == "sell")
            {
                price = StorageSlotPrice;
            }
            else
            {
                price = StaticsDaySoldSlotSinglePrice;
            }

            ShopMainPrice = 0;
            for (int i = 0; i < price.Length; i++)
            {
                double singlePrice = ShopSlotSelectedNumber[i] * price[i];
                string txt = singlePrice.ToString();
                ShopMainPrice += double.Parse(txt);
            }
            ShopMainPriceTxtBlock.Content = $"{ShopMainPrice}";
            for (int t = 0; t < 10; t++)
            {
                bool endsWithSearchResult = ShopMainPrice.ToString().EndsWith($",{t}", StringComparison.CurrentCultureIgnoreCase);
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
            BuildShop(themeData.GetWindowsAppTheme().ToString(), "storage");
        }

        private void NewPurchaseBtn_Click(object sender, RoutedEventArgs e)
        {
            Reload();
            OpenShop();

            TopBar.IsEnabled = false;
            ShopTask = "sell";
            CustomerNumberTxtBlock.Content = StaticsTotalCustomers + 1;
        }

        private void CancelPurchaseBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenShop();

            TopBar.IsEnabled = false;
            ShopTask = "cancel";
            CustomerNumberTxtBlock.Content = StaticsTotalCustomers;
        }

        private void ComplainPurchaseBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenShop();

            TopBar.IsEnabled = false;
            ShopTask = "complain";
            CustomerNumberTxtBlock.Content = StaticsTotalCustomers;
        }

        private void CancelComplainPurchaseBtn_GotFocus(object sender, RoutedEventArgs e)
        {
            BuildShop(themeData.GetWindowsAppTheme().ToString(), "statics");
        }

        private void CancelShoppingBtn_Click(object sender, RoutedEventArgs e)
        {
            CloseShop();

            TopBar.IsEnabled = true;
            ShopTask = "";
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
            ShopSellStart = DateTime.Now;
            CloseShop();

            for (int i = 0; i < InStorageSlots.Length; i++)
            {
                selectedItemsInt += ShopSlotSelectedNumber[i];
            }

            if (selectedItemsInt == 0)
            {
                MessageBox.Show("Der Warenborb ist leer!", "Bezahlvorgang nicht zulässig", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                bool error = false;
                GetMainPrice();

                if (ShopTask == "sell")
                {
                    ShopSellProductStart = DateTime.Now;
                    StaticsTotalCustomers++;
                    StaticsTotalGottenCash += ShopMainPrice;

                    for (int n = 0; n < InStorageSlots.Length; n++)
                    {
                        if (ShopSlotSelectedNumber[n] != 0)
                        {
                            try
                            {
                                StorageSlotNumber[n] -= ShopSlotSelectedNumber[n];
                                StaticsTotalSoldProducts += ShopSlotSelectedNumber[n];

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
                                foreach (string s in StaticsDaySoldSlotName)
                                {
                                    i++;
                                    if (StorageSlotName[n] == s && StorageSlotPrice[n] == productsSinglePriceList[n])
                                    {
                                        StaticsDaySoldSlotNumber[i] += ShopSlotSelectedNumber[n];
                                        StaticsDaySoldSlotCash[i] += ShopSlotSelectedNumber[n] * StorageSlotPrice[n];
                                        productFound = true;
                                        break;
                                    }
                                }
                                if (productFound == false)
                                {
                                    Array.Resize(ref StaticsDaySoldSlotName, StaticsDaySoldSlotName.Length + 1);
                                    StaticsDaySoldSlotName[StaticsDaySoldSlotName.Length - 1] = StorageSlotName[n];

                                    Array.Resize(ref StaticsDaySoldSlotNumber, StaticsDaySoldSlotNumber.Length + 1);
                                    StaticsDaySoldSlotNumber[StaticsDaySoldSlotNumber.Length - 1] = ShopSlotSelectedNumber[n];

                                    Array.Resize(ref StaticsDaySoldSlotCash, StaticsDaySoldSlotCash.Length + 1);
                                    StaticsDaySoldSlotCash[StaticsDaySoldSlotCash.Length - 1] = StorageSlotPrice[n] * ShopSlotSelectedNumber[n];

                                    Array.Resize(ref StaticsDaySoldSlotSinglePrice, StaticsDaySoldSlotSinglePrice.Length + 1);
                                    StaticsDaySoldSlotSinglePrice[StaticsDaySoldSlotSinglePrice.Length - 1] = StorageSlotPrice[n];
                                }
                            }
                            catch (Exception ex)
                            {
                                log.NewLog(LogWindow.LogTypes.Fehler, LogWindow.LogThread.Kasse, LogWindow.LogActions.Verkaufen, $"Produkt {n + 1}: {ex.Message}", DateTime.Now, DateTime.Now - ShopSellProductStart);
                            }
                        }
                    }

                    lastPayDate = DateTime.Now.Date;
                    log.NewLog(LogWindow.LogTypes.Info, LogWindow.LogThread.Kasse, LogWindow.LogActions.Verkaufen, $"Abgeschlossen", DateTime.Now, DateTime.Now - ShopSellStart);
                }
                else if (ShopTask == "cancel")
                {
                    ShopSellProductStart = DateTime.Now;
                    StorageLoader.RunWorkerAsync();
                    StaticsTotalGottenCash -= ShopMainPrice;

                    for (int n = 0; n < StaticsDaySoldSlotName.Length; n++)
                    {
                        try
                        {
                            if (ShopSlotSelectedNumber[n] != 0)
                            {
                                int i = -1;
                                foreach (string s in productsNameList)
                                {
                                    i++;
                                    if (StaticsDaySoldSlotName[n] == s && StorageSlotPrice[n] == productsSinglePriceList[n])
                                    {
                                        productsNumberList[i] -= ShopSlotSelectedNumber[n];
                                        productsCashList[i] -= ShopSlotSelectedNumber[n] * StaticsDaySoldSlotSinglePrice[n];
                                        break;
                                    }
                                }

                                i = -1;
                                foreach (string s in StaticsDaySoldSlotName)
                                {
                                    i++;
                                    if (StaticsDaySoldSlotName[n] == s && StorageSlotPrice[n] == StaticsDaySoldSlotSinglePrice[n])
                                    {
                                        StaticsDaySoldSlotNumber[i] -= ShopSlotSelectedNumber[n];
                                        StaticsDaySoldSlotCash[i] -= ShopSlotSelectedNumber[n] * StaticsDaySoldSlotSinglePrice[n];
                                        break;
                                    }
                                }

                                StorageSlotNumber[n] += ShopSlotSelectedNumber[n];
                                StaticsTotalSoldProducts -= ShopSlotSelectedNumber[n];
                            }
                        }
                        catch (Exception ex)
                        {
                            log.NewLog(LogWindow.LogTypes.Fehler, LogWindow.LogThread.Kasse, LogWindow.LogActions.Stornieren, $"Produkt {n + 1}: {ex.Message}", DateTime.Now, DateTime.Now - ShopSellProductStart);
                        }
                    }
                    log.NewLog(LogWindow.LogTypes.Info, LogWindow.LogThread.Kasse, LogWindow.LogActions.Stornieren, $"Abgeschlossen", DateTime.Now, DateTime.Now - ShopSellStart);
                }
                else if (ShopTask == "complain")
                {
                    ShopSellProductStart = DateTime.Now;
                    StorageLoader.RunWorkerAsync();

                    for (int n = 0; n < StaticsDaySoldSlotName.Length; n++)
                    {
                        try
                        {
                            StaticsDayLostProducts += ShopSlotSelectedNumber[n];
                            StaticsDayLostCash += StaticsDaySoldSlotSinglePrice[n] * ShopSlotSelectedNumber[n];
                        }
                        catch (Exception ex)
                        {
                            log.NewLog(LogWindow.LogTypes.Fehler, LogWindow.LogThread.Kasse, LogWindow.LogActions.Rücknehmen, $"Produkt {n + 1}: {ex.Message}", DateTime.Now, DateTime.Now - ShopSellProductStart);
                        }
                    }

                    StaticsTotalGottenCash -= ShopMainPrice;
                    StaticsTotalLostCash += StaticsDayLostCash;
                    StaticsTotalLostProducts += StaticsDayLostProducts;

                    log.NewLog(LogWindow.LogTypes.Info, LogWindow.LogThread.Kasse, LogWindow.LogActions.Rücknehmen, $"Abgeschlossen", DateTime.Now, DateTime.Now - ShopSellStart);
                }
                else
                {
                    MessageBox.Show("Bezahlvorgang konnte nicht definiert werden!", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                    error = true;
                }

                if (error != true)
                {
                    SettingsSaver.RunWorkerAsync();
                    StorageSaver.RunWorkerAsync();
                    TotalStaticsSaver.RunWorkerAsync();
                    DayStaticsSaver.RunWorkerAsync();
                    Reload();
                }
            }

            ClearShoppingCard();
            TopBar.IsEnabled = true;
        }

        private void OpenShop()
        {
            TotalStaticsLoader.RunWorkerAsync();
            DayStaticsLoader.RunWorkerAsync();
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
        private short GetNewProductId()
        {
            short i = -1;
            bool found = true;
            do
            {
                i++;
                if (!File.Exists(pathN.productsFile + $"{i}.xml"))
                {
                    found = false;
                }
            } while (found);
            return i;
        }

        private void StorageUncheckAll()
        {
            StorageSelectAllSlotsChBox.IsChecked = false;
            StorageSelectedCount = 0;
            StorageRemoveSlotBtn.IsEnabled = false;
            StorageActivedSlotBtn.IsEnabled = false;
            StorageDeactivedSlotBtn.IsEnabled = false;

            for (int i = 0; i < StorageSelectedArray.Length; i++)
            {
                StorageSelectedArray[i] = false;
            }
        }

        private void StorageDeleteSpaces()
        {
            int lenght = (short)InStorageSlots.Length;
            for (int i = 0; i < lenght; i++)
            {
                if (StorageSlotName[i] == null)
                {
                    for (int n = i; n < lenght; n++)
                    {
                        try
                        {
                            StorageSlotName[n] = StorageSlotName[n + 1];
                            StorageSlotStatus[n] = StorageSlotStatus[n + 1];
                            StorageSlotNumber[n] = StorageSlotNumber[n + 1];
                            StorageSlotPrice[n] = StorageSlotPrice[n + 1];
                            InStorageSlots[n] = InStorageSlots[n + 1];
                        }
                        catch { }
                    }
                    lenght--;
                    i--;
                }
            }

            Array.Resize(ref StorageSlotName, lenght);
            Array.Resize(ref StorageSlotStatus, lenght);
            Array.Resize(ref StorageSlotNumber, lenght);
            Array.Resize(ref StorageSlotPrice, lenght);
            Array.Resize(ref InStorageSlots, lenght);
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

        private void StorageBuilder()
        {
            StorageBuildStart = DateTime.Now;
            MainProgBarShow();
            if (InStorageSlots.Length == 0)
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

                    for (short i = 0; i < InStorageSlots.Length; i++)
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
                            if (Theme == AppTheme.Light)
                            {
                                slotScp.Background = Brushes.LightGray;
                            }
                            else
                            {
                                slotScp.Background = (Brush)new BrushConverter().ConvertFrom("#FF555555");
                            }
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

                        Array.Resize(ref StorageSelectedArray, InStorageSlots.Length);
                        StorageUncheckAll();
                    }
                    log.NewLog(LogWindow.LogTypes.Erfolg, LogWindow.LogThread.Lager, LogWindow.LogActions.Bauen, null, DateTime.Now, DateTime.Now - StorageBuildStart);
                    NewMsgItem(LogWindow.LogTypes.Erfolg, LogWindow.LogThread.Lager, "Bauen des Lagers erfolgreich!", null);
                }
                catch (Exception ex)
                {
                    log.NewLog(LogWindow.LogTypes.Fehler, LogWindow.LogThread.Lager, LogWindow.LogActions.Bauen, ex.Message, DateTime.Now, DateTime.Now - StorageBuildStart);
                    NewMsgItem(LogWindow.LogTypes.Fehler, LogWindow.LogThread.Lager, "Bauen des Lagers fehlgeschlagen!", ex.Message);
                }
            }
            MainProgBarHide();
        }

        private void StorageLoaderRun(object sender, DoWorkEventArgs e)
        {
            StorageLoaderStart = DateTime.Now;
            string errors = "";
            try
            {
                StorageData stoL = StorageData.Load();

                StorageSlotName = Array.Empty<string>();
                StorageSlotStatus = Array.Empty<bool>();
                StorageSlotNumber = Array.Empty<short>();
                StorageSlotPrice = Array.Empty<double>();

                try
                {
                    InStorageSlots = stoL.InStorageSlots;

                    foreach (short i in InStorageSlots)
                    {
                        ProductsData proL = ProductsData.Load(i);

                        try
                        {
                            Array.Resize(ref StorageSlotName, StorageSlotName.Length + 1);
                            StorageSlotName[StorageSlotName.Length - 1] = proL.ProductName;
                        }
                        catch (Exception ex)
                        {
                            errors += $"\nProdukt {i}, Name, {ex.Message}";
                        }

                        try
                        {
                            Array.Resize(ref StorageSlotNumber, StorageSlotNumber.Length + 1);
                            StorageSlotNumber[StorageSlotNumber.Length - 1] = proL.ProductNumber;
                        }
                        catch (Exception ex)
                        {
                            errors += $"\nProdukt {i}, Anzahl, {ex.Message}";
                        }

                        try
                        {
                            Array.Resize(ref StorageSlotPrice, StorageSlotPrice.Length + 1);
                            StorageSlotPrice[StorageSlotPrice.Length - 1] = proL.ProductPrice;
                        }
                        catch (Exception ex)
                        {
                            errors += $"\nProdukt {i}, Preis, {ex.Message}";
                        }

                        try
                        {
                            Array.Resize(ref StorageSlotStatus, StorageSlotStatus.Length + 1);
                            StorageSlotStatus[StorageSlotStatus.Length - 1] = proL.ProductStatus;
                        }
                        catch (Exception ex)
                        {
                            errors += $"\nProdukt {i}, Status, {ex.Message}";
                        }
                    }
                }
                catch (Exception ex)
                {
                    errors += $"\nLagerinhalt, {ex.Message}";
                }
            }
            catch (Exception ex)
            {
                errors += $"\nLager laden, {ex.Message}";
            }

            e.Result = errors;
        }

        private void StorageLoaderProgress(object sender, ProgressChangedEventArgs e)
        {
            MainProgBarShow();
        }

        private void StorageLoadComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            string errors = (string)e.Result;

            if (e.Cancelled)
            {
                log.NewLog(LogWindow.LogTypes.Warnung, LogWindow.LogThread.Lager, LogWindow.LogActions.Laden, "Abgebrochen", DateTime.Now, DateTime.Now - StorageLoaderStart);
                NewMsgItem(LogWindow.LogTypes.Warnung, LogWindow.LogThread.Lager, "Laden des Lagers abgebrochen!", null);
            }
            else
            {
                if (!string.IsNullOrEmpty(errors))
                {
                    log.NewLog(LogWindow.LogTypes.Fehler, LogWindow.LogThread.Lager, LogWindow.LogActions.Laden, errors, DateTime.Now, DateTime.Now - StorageLoaderStart);
                    NewMsgItem(LogWindow.LogTypes.Fehler, LogWindow.LogThread.Lager, "Laden des Lagers fehlerhaft!", "Siehe Log");
                }
                else
                {
                    log.NewLog(LogWindow.LogTypes.Erfolg, LogWindow.LogThread.Lager, LogWindow.LogActions.Laden, null, DateTime.Now, DateTime.Now - StorageLoaderStart);
                    NewMsgItem(LogWindow.LogTypes.Erfolg, LogWindow.LogThread.Lager, "Laden des Lagers erfolgreich!", null);
                }
            }
            MainProgBarHide();
            StorageBuilder();
            BuildShop(AppThemeStr, "storage");
        }

        private void StorageSaverProgress(object sender, ProgressChangedEventArgs e)
        {
            MainProgBarShow();
        }

        private void StorageSaverComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            string errors = (string)e.Result;

            if (e.Cancelled)
            {
                log.NewLog(LogWindow.LogTypes.Warnung, LogWindow.LogThread.Lager, LogWindow.LogActions.Speichern, "Abgebrochen", DateTime.Now, DateTime.Now - StorageSaverStart);
                NewMsgItem(LogWindow.LogTypes.Warnung, LogWindow.LogThread.Lager, "Speichern des Lagers abgebrochen!", null);
            }
            else
            {
                if (!string.IsNullOrEmpty(errors))
                {
                    log.NewLog(LogWindow.LogTypes.Fehler, LogWindow.LogThread.Lager, LogWindow.LogActions.Speichern, errors, DateTime.Now, DateTime.Now - StorageSaverStart);
                    NewMsgItem(LogWindow.LogTypes.Fehler, LogWindow.LogThread.Lager, "Speichern des Lagers fehlerhaft!", "Siehe Log");
                }
                else
                {
                    log.NewLog(LogWindow.LogTypes.Erfolg, LogWindow.LogThread.Lager, LogWindow.LogActions.Speichern, null, DateTime.Now, DateTime.Now - StorageSaverStart);
                    NewMsgItem(LogWindow.LogTypes.Erfolg, LogWindow.LogThread.Lager, "Speichern des Lagers erfolgreich!", null);
                }
            }

            MainProgBarHide();
        }

        private void StorageStorageSaverRun(object sender, DoWorkEventArgs e)
        {
            StorageSaverStart = DateTime.Now;
            string errors = "";

            StorageData stoS = new StorageData();
            ProductsData proS = new ProductsData();

            foreach (short i in InStorageSlots)
            {
                try
                {
                    proS.ProductName = StorageSlotName[i];
                }
                catch (Exception ex)
                {
                    errors += $"\nProdukt {i}, Name, {ex.Message}";
                }
                try
                {
                    proS.ProductNumber = StorageSlotNumber[i];
                }
                catch (Exception ex)
                {
                    errors += $"\nProdukt {i}, Anzahl, {ex.Message}";
                }
                try
                {
                    proS.ProductPrice = StorageSlotPrice[i];
                }
                catch (Exception ex)
                {
                    errors += $"\nProdukt {i}, Preis, {ex.Message}";
                }
                try
                {
                    proS.ProductStatus = StorageSlotStatus[i];
                }
                catch (Exception ex)
                {
                    errors += $"\nProdukt {i}, Status, {ex.Message}";
                }
                try
                {
                    proS.Save(i);
                }
                catch (Exception ex)
                {
                    errors += $"\nProdukt {i}, Speichern, {ex.Message}";
                }
            }

            try
            {
                stoS.InStorageSlots = InStorageSlots;
                stoS.Save();
            }
            catch (Exception ex)
            {
                errors += $"\nLager, Speichern, {ex.Message}";
            }

            e.Result = errors;
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

                Array.Resize(ref InStorageSlots, InStorageSlots.Length + 1);
                InStorageSlots[InStorageSlots.Length - 1] = GetNewProductId();

                Array.Resize(ref StorageSelectedArray, InStorageSlots.Length);

                StorageUncheckAll();

                StorageSaver.RunWorkerAsync();
                StorageLoader.RunWorkerAsync();
                StorageBuilder();
            }
        }

        private void StorageRemoveSlotBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Möchten Sie die ausgewählten Produktplätze wirklich löschen?\nDiese Aktion kann nicht rückgängig gemacht werden!", "Produktplätze löschen", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                for (int i = 0; i < InStorageSlots.Length; i++)
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
                StorageSaver.RunWorkerAsync();
                StorageLoader.RunWorkerAsync();
                StorageBuilder();
            }
        }

        private void StorageActivedSlotBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Möchten Sie die ausgewählten Produktplätze wirklich aktivieren?", "Produktplätze aktivieren", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                for (int i = 0; i < InStorageSlots.Length; i++)
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

                StorageSaver.RunWorkerAsync();
                StorageLoader.RunWorkerAsync();
                StorageBuilder();
            }
        }

        private void StorageDeactivedSlotBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Möchten Sie die ausgewählten Produktplätze wirklich deaktivieren?", "Produktplätze deaktivieren", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                for (int i = 0; i < InStorageSlots.Length; i++)
                {
                    if (StorageSelectedArray[i] == true)
                    {
                        StorageSlotStatus[i] = false;
                    }
                }
                StorageUncheckAll();

                StorageSaver.RunWorkerAsync();
                StorageLoader.RunWorkerAsync();
                StorageBuilder();
            }
        }

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

                StorageSaver.RunWorkerAsync();
                Reload();
            }
        }

        private void StorageSlotCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox chBox)
            {
                if (StorageSelectedCount == InStorageSlots.Length)
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

                if (StorageSelectedCount == InStorageSlots.Length)
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
                StorageSelectedCount = short.Parse(InStorageSlots.Length.ToString());

                for (int i = 0; i < InStorageSlots.Length; i++)
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

                for (int i = 0; i < InStorageSlots.Length; i++)
                {
                    CheckBox chBox = (CheckBox)FindName($"StorageSlot{i}ChBox");
                    if (chBox != null)
                    {
                        chBox.IsChecked = false;
                    }
                }
            }
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
        private void DayStaticsSaverProgress(object sender, ProgressChangedEventArgs e)
        {
            MainProgBarShow();
        }

        private void DayStaticsSaverComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            string error = (string)e.Result;

            if (e.Cancelled)
            {
                log.NewLog(LogWindow.LogTypes.Warnung, LogWindow.LogThread.Tagesstatistiken, LogWindow.LogActions.Speichern, "Abgebrochen", DateTime.Now, DateTime.Now - StorageSaverStart);
                NewMsgItem(LogWindow.LogTypes.Warnung, LogWindow.LogThread.Tagesstatistiken, "Speichern der Tagesstatistiken abgebrochen!", null);
            }
            else
            {
                if (!string.IsNullOrEmpty(error))
                {
                    log.NewLog(LogWindow.LogTypes.Fehler, LogWindow.LogThread.Tagesstatistiken, LogWindow.LogActions.Speichern, error, DateTime.Now, DateTime.Now - StorageSaverStart);
                    NewMsgItem(LogWindow.LogTypes.Fehler, LogWindow.LogThread.Tagesstatistiken, "Speichern der Tagesstatistiken fehlerhaft!", "Siehe Log");
                }
                else
                {
                    log.NewLog(LogWindow.LogTypes.Erfolg, LogWindow.LogThread.Tagesstatistiken, LogWindow.LogActions.Speichern, null, DateTime.Now, DateTime.Now - StorageSaverStart);
                    NewMsgItem(LogWindow.LogTypes.Erfolg, LogWindow.LogThread.Tagesstatistiken, "Speichern der Tagesstatistiken erfolgreich!", null);
                }
            }

            MainProgBarHide();
        }

        private void DayStaticsSaverRun(object sender, DoWorkEventArgs e)
        {
            try
            {
                StaticsDayData staDS = new StaticsDayData
                {
                    StaticsDaySoldSlotCash = StaticsDaySoldSlotCash,
                    StaticsDaySoldSlotNumber = StaticsDaySoldSlotNumber,
                    StaticsDaySoldSlotName = StaticsDaySoldSlotName,
                    StaticsDaySoldSlotSinglePrice = StaticsDaySoldSlotSinglePrice,
                    StaticsDayLostCash = StaticsDayLostCash,
                    StaticsDayLostProducts = StaticsDayLostProducts,
                    StaticsDayPcUsage = StaticsDayPcUsage,
                    StaticsDayPcUsers = StaticsDayPcUsers,
                    StaticsDayPcName = StaticsDayPcName
                };
                staDS.Save();
            }
            catch (Exception ex)
            {
                e.Result = ex.Message;
            }
        }

        private void DayStaticsLoaderProgress(object sender, ProgressChangedEventArgs e)
        {
            MainProgBarShow();
        }

        private void DayStaticsLoaderComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            string error = (string)e.Result;

            if (e.Cancelled)
            {
                log.NewLog(LogWindow.LogTypes.Warnung, LogWindow.LogThread.Tagesstatistiken, LogWindow.LogActions.Laden, "Abgebrochen", DateTime.Now, DateTime.Now - DayStaticsLoadStart);
                NewMsgItem(LogWindow.LogTypes.Warnung, LogWindow.LogThread.Tagesstatistiken, "Laden der Tagesstatistiken abgebrochen!", null);
            }
            else
            {
                if (!string.IsNullOrEmpty(error))
                {
                    log.NewLog(LogWindow.LogTypes.Fehler, LogWindow.LogThread.Tagesstatistiken, LogWindow.LogActions.Laden, error, DateTime.Now, DateTime.Now - DayStaticsLoadStart);
                    NewMsgItem(LogWindow.LogTypes.Fehler, LogWindow.LogThread.Tagesstatistiken, "Laden der Tagesstatistiken fehlerhaft!", "Siehe Log");
                }
                else
                {
                    log.NewLog(LogWindow.LogTypes.Erfolg, LogWindow.LogThread.Tagesstatistiken, LogWindow.LogActions.Laden, null, DateTime.Now, DateTime.Now - DayStaticsLoadStart);
                    NewMsgItem(LogWindow.LogTypes.Erfolg, LogWindow.LogThread.Tagesstatistiken, "Laden der Tagesstatistiken erfolgreich!", null);
                }
            }

            MainProgBarHide();
            BuildDayStatics();
        }

        private void DayStaticsLoaderRun(object sender, DoWorkEventArgs e)
        {
            DayStaticsLoadStart = DateTime.Now;
            DateTime date = (DateTime)e.Argument;
            string errors = "";

            try
            {
                StaticsDayData staDL = StaticsDayData.Load(date.Date);

                try
                {
                    StaticsDaySoldSlotCash = staDL.StaticsDaySoldSlotCash;
                }
                catch (Exception ex)
                {
                    errors += $"\nProdukt, Einnahmen, {ex.Message}";
                }
                try
                {
                    StaticsDaySoldSlotNumber = staDL.StaticsDaySoldSlotNumber;
                }
                catch (Exception ex)
                {
                    errors += $"\nProdukt, Anzahl, {ex.Message}";
                }
                try
                {
                    StaticsDaySoldSlotName = staDL.StaticsDaySoldSlotName;
                }
                catch (Exception ex)
                {
                    errors += $"\nProdukt, Name, {ex.Message}";
                }
                try
                {
                    StaticsDaySoldSlotSinglePrice = staDL.StaticsDaySoldSlotSinglePrice;
                }
                catch (Exception ex)
                {
                    errors += $"\nProdukt, Einzelpreis, {ex.Message}";
                }
                try
                {
                    StaticsDayLostCash = staDL.StaticsDayLostCash;
                }
                catch (Exception ex)
                {
                    errors += $"\nVerluste, {ex.Message}";
                }
                try
                {
                    StaticsDayLostProducts = staDL.StaticsDayLostProducts;
                }
                catch (Exception ex)
                {
                    errors += $"\nRücknahmen, {ex.Message}";
                }
                try
                {
                    StaticsDayPcUsage = staDL.StaticsDayPcUsage;
                }
                catch (Exception ex)
                {
                    errors += $"\nPCs, Benutzung, {ex.Message}";
                }
                try
                {
                    StaticsDayPcUsers = staDL.StaticsDayPcUsers;
                }
                catch (Exception ex)
                {
                    errors += $"\nPCs, Benutzer, {ex.Message}";
                }
                try
                {
                    StaticsDayPcName = staDL.StaticsDayPcName;
                }
                catch (Exception ex)
                {
                    errors += $"\nPCs, Name, {ex.Message}";
                }
            }
            catch (Exception ex)
            {
                errors += $"\nLaden, {ex.Message}";
            }

            e.Result = errors;
        }

        private void TotalStaticsLoaderProgress(object sender, ProgressChangedEventArgs e)
        {
            MainProgBarShow();
        }

        private void TotalStaticsLoaderComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            string error = (string)e.Result;

            if (e.Cancelled)
            {
                log.NewLog(LogWindow.LogTypes.Warnung, LogWindow.LogThread.Gesamtstatistiken, LogWindow.LogActions.Laden, "Abgebrochen", DateTime.Now, DateTime.Now - TotalStaticsLoadStart);
                NewMsgItem(LogWindow.LogTypes.Warnung, LogWindow.LogThread.Gesamtstatistiken, "Speichern der Gesamtstatistiken abgebrochen!", null);
            }
            else
            {
                if (!string.IsNullOrEmpty(error))
                {
                    log.NewLog(LogWindow.LogTypes.Fehler, LogWindow.LogThread.Gesamtstatistiken, LogWindow.LogActions.Laden, error, DateTime.Now, DateTime.Now - TotalStaticsLoadStart);
                    NewMsgItem(LogWindow.LogTypes.Fehler, LogWindow.LogThread.Gesamtstatistiken, "Laden der Gesamtstatistiken fehlerhaft!", "Siehe Log");
                }
                else
                {
                    log.NewLog(LogWindow.LogTypes.Erfolg, LogWindow.LogThread.Gesamtstatistiken, LogWindow.LogActions.Laden, null, DateTime.Now, DateTime.Now - TotalStaticsLoadStart);
                    NewMsgItem(LogWindow.LogTypes.Erfolg, LogWindow.LogThread.Gesamtstatistiken, "Laden der Gesamtstatistiken erfolgreich!", null);
                }
            }

            MainProgBarHide();
            BuildTotalStatics();
        }

        private void TotalStaticsLoaderRun(object sender, DoWorkEventArgs e)
        {
            TotalStaticsLoadStart = DateTime.Now;
            string errors = "";

            try
            {
                StaticsTotalData staTL = StaticsTotalData.Load();

                try
                {
                    StaticsTotalStartDate = staTL.StaticsTotalStartDate;
                }
                catch (Exception ex)
                {
                    errors += $"\nStartdatum, {ex.Message}";
                }
                try
                {
                    StaticsTotalCustomers = staTL.StaticsTotalCustomers;
                }
                catch (Exception ex)
                {
                    errors += $"\nKunden, {ex.Message}";
                }
                try
                {
                    StaticsTotalSoldProducts = staTL.StaticsTotalSoldProducts;
                }
                catch (Exception ex)
                {
                    errors += $"\nVerkaufte Produkte, {ex.Message}";
                }
                try
                {
                    StaticsTotalGottenCash = staTL.StaticsTotalGottenCash;
                }
                catch (Exception ex)
                {
                    errors += $"\nEinnahmen, {ex.Message}";
                }
                try
                {
                    StaticsTotalLostCash = staTL.StaticsTotalLostCash;
                }
                catch (Exception ex)
                {
                    errors += $"\nVerlsute, {ex.Message}";
                }
                try
                {
                    StaticsTotalLostProducts = staTL.StaticsTotalLostProducts;
                }
                catch (Exception ex)
                {
                    errors += $"\nRücknahmen, {ex.Message}";
                }
                try
                {
                    productsNumberList = staTL.productsNumberList;
                }
                catch (Exception ex)
                {
                    errors += $"\nProdukt, Anzahl, {ex.Message}";
                }
                try
                {
                    productsNameList = staTL.productsNameList;
                }
                catch (Exception ex)
                {
                    errors += $"\nProdukte, Name, {ex.Message}";
                }
                try
                {
                    productsCashList = staTL.productsCashList;
                }
                catch (Exception ex)
                {
                    errors += $"\nProdukte, Einnahmen, {ex.Message}";
                }
                try
                {
                    productsSinglePriceList = staTL.productsSinglePriceList;
                }
                catch (Exception ex)
                {
                    errors += $"\nProdukte, Einzelpreis, {ex.Message}";
                }

                for (int i = 0; i < staTL.StaticsTotalPcUsage.Length; i++)
                {
                    try
                    {
                        StaticsTotalPcUsage[i] = staTL.StaticsTotalPcUsage[i];
                    }
                    catch (Exception ex)
                    {
                        errors += $"\nPC {i + 1}, Benutzung, {ex.Message}";
                    }

                    try
                    {
                        StaticsTotalPcUsers[i] = staTL.StaticsTotalPcUsers[i];
                    }
                    catch (Exception ex)
                    {
                        errors += $"\nPC {i + 1}, Benutzer, {ex.Message}";
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
                        errors += $"\nBeleibtestes Produkt {i + 1}, Name, {ex.Message}";
                    }

                    try
                    {
                        mostSoldProductsNumber[i] = staTL.mostSoldProductsNumber[i];
                    }
                    catch (Exception ex)
                    {
                        errors += $"\nBeleibtestes Produkt {i + 1}, Anzahl, {ex.Message}";
                    }

                    try
                    {
                        mostSoldProductsSinglePrice[i] = staTL.mostSoldProductsSinglePrice[i];
                    }
                    catch (Exception ex)
                    {
                        errors += $"\nBeleibtestes Produkt {i + 1}, Einzelpreis, {ex.Message}";
                    }

                    try
                    {
                        highestEarningsProductsName[i] = staTL.highestEarningsProductsName[i];
                    }
                    catch (Exception ex)
                    {
                        errors += $"\nHöchste Einahmen bei Produkt {i + 1}, Name, {ex.Message}";
                    }

                    try
                    {
                        highestEarningsProductsNumber[i] = staTL.highestEarningsProductsNumber[i];
                    }
                    catch (Exception ex)
                    {
                        errors += $"\nHöchste Einahmen bei Produkt {i + 1}, Anzahl, {ex.Message}";
                    }

                    try
                    {
                        highestEarningsProductsSinglePrice[i] = staTL.highestEarningsProductsSinglePrice[i];
                    }
                    catch (Exception ex)
                    {
                        errors += $"\nHöchste Einahmen bei Produkt {i + 1}, Einzelpreis, {ex.Message}";
                    }
                }
            }
            catch (Exception ex)
            {
                errors += $"\nLaden, {ex.Message}";
            }

            e.Result = errors;
        }

        private void TotalStaticsSaverProgress(object sender, ProgressChangedEventArgs e)
        {
            MainProgBarShow();
        }

        private void TotalStaticsSaverComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            string error = (string)e.Result;

            if (e.Cancelled)
            {
                log.NewLog(LogWindow.LogTypes.Warnung, LogWindow.LogThread.Gesamtstatistiken, LogWindow.LogActions.Speichern, "Abgebrochen", DateTime.Now, DateTime.Now - StorageSaverStart);
                NewMsgItem(LogWindow.LogTypes.Warnung, LogWindow.LogThread.Gesamtstatistiken, "Speichern der Gesamtstatistiken abgebrochen!", null);
            }
            else
            {
                if (!string.IsNullOrEmpty(error))
                {
                    log.NewLog(LogWindow.LogTypes.Fehler, LogWindow.LogThread.Gesamtstatistiken, LogWindow.LogActions.Speichern, error, DateTime.Now, DateTime.Now - StorageSaverStart);
                    NewMsgItem(LogWindow.LogTypes.Fehler, LogWindow.LogThread.Gesamtstatistiken, "Speichern der Gesamtstatistiken fehlerhaft!", "Siehe Log");
                }
                else
                {
                    log.NewLog(LogWindow.LogTypes.Erfolg, LogWindow.LogThread.Gesamtstatistiken, LogWindow.LogActions.Speichern, null, DateTime.Now, DateTime.Now - StorageSaverStart);
                    NewMsgItem(LogWindow.LogTypes.Erfolg, LogWindow.LogThread.Gesamtstatistiken, "Speichern der Gesamtstatistiken erfolgreich!", null);
                }
            }

            MainProgBarHide();
        }

        private void TotalStaticsSaverRun(object sender, DoWorkEventArgs e)
        {
            TotalStaticsSaveStart = DateTime.Now;
            string error = "";
            StaticsTotalData staTS = new StaticsTotalData
            {
                StaticsTotalStartDate = StaticsTotalStartDate,
                StaticsTotalCustomers = StaticsTotalCustomers,
                StaticsTotalSoldProducts = StaticsTotalSoldProducts,
                StaticsTotalGottenCash = StaticsTotalGottenCash,
                StaticsTotalLostCash = StaticsTotalLostCash,
                StaticsTotalLostProducts = StaticsTotalLostProducts,
                productsNameList = productsNameList,
                productsNumberList = productsNumberList,
                productsCashList = productsCashList,
                productsSinglePriceList = productsSinglePriceList,
                StaticsTotalPcUsage = StaticsTotalPcUsage,
                StaticsTotalPcUsers = StaticsTotalPcUsers,
                StaticsTotalPcName = StaticsTotalPcName,
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
                log.NewLog(LogWindow.LogTypes.Erfolg, LogWindow.LogThread.Gesamtstatistiken, LogWindow.LogActions.Speichern, null, DateTime.Now, DateTime.Now - TotalStaticsSaveStart);
            }
            catch (Exception ex)
            {
                log.NewLog(LogWindow.LogTypes.Fehler, LogWindow.LogThread.Gesamtstatistiken, LogWindow.LogActions.Speichern, ex.Message, DateTime.Now, DateTime.Now - TotalStaticsSaveStart);
            }

            e.Result = error;
        }

        private void BuildTotalStatics()
        {
            TotalStaticsBuildStart = DateTime.Now;
            Label lbl;

            try
            {
                StaticsTotalStartTimeLbl.Content = StaticsTotalStartDate.ToShortDateString();
                StaticsTotalCustomersLbl.Content = StaticsTotalCustomers.ToString();
                StaticsTotalSoldProductsNumberLbl.Content = StaticsTotalSoldProducts.ToString();
                StaticsTotalGottenCashNumberLbl.Content = StaticsTotalGottenCash;
                for (int t = 0; t < 10; t++)
                {
                    bool endsWithSearchResult = StaticsTotalGottenCash.ToString().EndsWith($",{t}", StringComparison.CurrentCultureIgnoreCase);
                    if (endsWithSearchResult == true)
                    {
                        StaticsTotalGottenCashNumberLbl.Content += "0";
                    }
                }
                StaticsTotalGottenCashNumberLbl.Content += "€";

                StaticsTotalLostCashNumberLbl.Content = StaticsTotalLostCash;
                for (int t = 0; t < 10; t++)
                {
                    bool endsWithSearchResult = StaticsTotalLostCash.ToString().EndsWith($",{t}", StringComparison.CurrentCultureIgnoreCase);
                    if (endsWithSearchResult == true)
                    {
                        StaticsTotalLostCashNumberLbl.Content += "0";
                    }
                }
                StaticsTotalLostCashNumberLbl.Content += "€";

                StaticsAllLostProductsNumberLbl.Content = StaticsTotalLostProducts.ToString();

                for (int i = 0; i < StaticsTotalPcUsers.Length; i++)
                {
                    //StaticsTotalPc0NameLbl;
                    //StaticsTotalPc0SlotLbl;
                    //StaticsTotalPc0UsageLbl;
                    //StaticsTotalPc0UsersLbl;

                    Label slotLbl = new Label
                    {
                        Content = (i + 1).ToString(),
                        Margin = new Thickness(5),
                        FontWeight = FontWeights.Bold,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        Name = $"StaticsTotalPc{i}SlotLbl",
                        Tag = i.ToString()
                    };

                    Label nameLbl = new Label
                    {
                        Content = StaticsTotalPcName[i].ToString(),
                        Margin = new Thickness(5),
                        Name = $"StaticsTotalPc{i}NameLbl",
                        Tag = i.ToString()
                    };

                    Label usersLbl = new Label
                    {
                        Content = StaticsTotalPcUsers[i].ToString(),
                        Margin = new Thickness(5),
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        Name = $"StaticsTotalPc{i}UsersLbl",
                        Tag = i.ToString()
                    };

                    Label usageLbl = new Label
                    {
                        Content = StaticsTotalPcUsage[i].ToString(),
                        Margin = new Thickness(5),
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        Name = $"StaticsTotalPc{i}UsageLbl",
                        Tag = i.ToString()
                    };
                }

                //StaticsTotalPc1TimeLbl.Content = pcTime[0].Hours + "," + pcTime[0].Minutes;
                //StaticsTotalPc2TimeLbl.Content = pcTime[1].Hours + "," + pcTime[1].Minutes;
                //StaticsTotalPc1UserLbl.Content = pcUsers[0].ToString();
                //StaticsTotalPc2UserLbl.Content = pcUsers[1].ToString();

                StaticsAllProductTypesListView.Items.Clear();
                for (int i = 0; i < productsNameList.Length; i++)
                {
                    if (!string.IsNullOrEmpty(productsNameList[i]))
                    {
                        StaticsAllProductTypesListView.Items.Add(productsNameList[i].ToString());
                    }
                }
                StaticsAllProductTypesNumberLbl.Content = productsNameList.Length.ToString();


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
                log.NewLog(LogWindow.LogTypes.Erfolg, LogWindow.LogThread.Gesamtstatistiken, LogWindow.LogActions.Bauen, null, DateTime.Now, DateTime.Now - TotalStaticsBuildStart);
                NewMsgItem(LogWindow.LogTypes.Erfolg, LogWindow.LogThread.Gesamtstatistiken, "Bauen der Gesamtstatistiken erfolgreich", null);
            }
            catch (Exception ex)
            {
                log.NewLog(LogWindow.LogTypes.Fehler, LogWindow.LogThread.Gesamtstatistiken, LogWindow.LogActions.Bauen, null, DateTime.Now, DateTime.Now - TotalStaticsBuildStart);
                NewMsgItem(LogWindow.LogTypes.Fehler, LogWindow.LogThread.Gesamtstatistiken, "Bauen der Gesamtstatistiken fehlgeschlagen", ex.Message);
            }
        }

        private void BuildDayStatics()
        {
            DayStaticsBuildStart = DateTime.Now;
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

                for (int i = 0; i < StaticsDaySoldSlotName.Length; i++)
                {
                    if (!string.IsNullOrEmpty(StaticsDaySoldSlotName[i]))
                    {
                        Label lbl = new Label
                        {
                            Name = $"StaticsDaySlot{i}SlotLbl",
                            Tag = i,
                            Content = i + 1,
                            Margin = new Thickness(5, 5, 0, 5),
                            VerticalContentAlignment = VerticalAlignment.Center,
                            FontWeight = FontWeights.Bold
                        };

                        Label lbl1 = new Label
                        {
                            Name = $"StaticsDaySlot{i}NameLbl",
                            Tag = i,
                            Content = $"{StaticsDaySoldSlotName[i]}",
                            Margin = new Thickness(5, 5, 5, 5),
                            VerticalContentAlignment = VerticalAlignment.Center,
                        };

                        Label lbl2 = new Label
                        {
                            Name = $"StaticsDaySlot{i}SinglePriceLbl",
                            Tag = i,
                            Content = $"{StaticsDaySoldSlotSinglePrice[i]}",
                            Margin = new Thickness(5, 5, 5, 5),
                            VerticalContentAlignment = VerticalAlignment.Center,
                            HorizontalContentAlignment = HorizontalAlignment.Center,
                        };

                        PieSeries series = new PieSeries()
                        {
                            Title = "Produkt " + (i + 1),
                            Values = new ChartValues<double> { StaticsDaySoldSlotNumber[i] },
                            DataLabels = true,
                            LabelPoint = labelPoint
                        };

                        for (int t = 0; t < 10; t++)
                        {
                            bool endsWithSearchResult = StaticsDaySoldSlotSinglePrice[i].ToString().EndsWith($",{t}", StringComparison.CurrentCultureIgnoreCase);
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
                            Content = $"{StaticsDaySoldSlotNumber[i]}",
                        };

                        Label lbl4 = new Label
                        {
                            Name = $"StaticsDaySlot{i}CashLbl",
                            Tag = i,
                            Margin = new Thickness(5, 5, 5, 5),
                            VerticalContentAlignment = VerticalAlignment.Center,
                            HorizontalContentAlignment = HorizontalAlignment.Center,
                            Content = $"{StaticsDaySoldSlotCash[i]}",
                        };

                        PieSeries series2 = new PieSeries()
                        {
                            Title = "Produkt " + (i + 1),
                            Values = new ChartValues<double> { StaticsDaySoldSlotCash[i] },
                            DataLabels = true,
                            LabelPoint = labelPoint
                        };

                        for (int t = 0; t < 10; t++)
                        {
                            bool endsWithSearchResult = StaticsDaySoldSlotCash[i].ToString().EndsWith($",{t}", StringComparison.CurrentCultureIgnoreCase);
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

                //StaticsDayPc1TimeLbl.Content = pcTime[0].Hours.ToString();
                //StaticsDayPc2TimeLbl.Content = pcTime[1].Hours.ToString();
                //StaticsDayPc1UsersLbl.Content = pcUsers[0].ToString();
                //StaticsDayPc2UsersLbl.Content = pcUsers[1].ToString();

                //StaticsDayPcUsersChart_PC1.Values = new ChartValues<int> { pcUsers[0] };
                //StaticsDayPcUsersChart_PC2.Values = new ChartValues<int> { pcUsers[1] };
                //StaticsDayPcTimeChart_PC1.Values = new ChartValues<int> { pcTime[0].Hours };
                //StaticsDayPcTimeChart_PC2.Values = new ChartValues<int> { pcTime[1].Hours };

                double gottenCash = 0;
                int soldProducts = 0;
                for (int i = 0; i < StaticsDaySoldSlotName.Length; i++)
                {
                    gottenCash += StaticsDaySoldSlotCash[i];
                    soldProducts += StaticsDaySoldSlotNumber[i];
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
                StaticsDayLostProductsNumberLbl.Content = $"{StaticsDayLostProducts}";
                StaticsDayLostCashNumberLbl.Content = $"{StaticsDayLostCash}";
                for (int t = 0; t < 10; t++)
                {
                    bool endsWithSearchResult = StaticsDayLostCash.ToString().EndsWith($",{t}", StringComparison.CurrentCultureIgnoreCase);
                    if (endsWithSearchResult == true)
                    {
                        StaticsDayLostCashNumberLbl.Content += "0";
                    }
                }
                StaticsDayLostCashNumberLbl.Content += "€";

                log.NewLog(LogWindow.LogTypes.Erfolg, LogWindow.LogThread.Tagesstatistiken, LogWindow.LogActions.Bauen, null, DateTime.Now, DateTime.Now - DayStaticsBuildStart);
                NewMsgItem(LogWindow.LogTypes.Erfolg, LogWindow.LogThread.Tagesstatistiken, "Bauen der Tagesstatistiken erfolgreich!", null);
            }
            catch (Exception ex)
            {
                log.NewLog(LogWindow.LogTypes.Fehler, LogWindow.LogThread.Tagesstatistiken, LogWindow.LogActions.Bauen, ex.Message, DateTime.Now, DateTime.Now - DayStaticsBuildStart);
                NewMsgItem(LogWindow.LogTypes.Fehler, LogWindow.LogThread.Tagesstatistiken, "Bauen der Tagesstatistiken fehlgeschlagen!", ex.Message);
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
                if (File.Exists(pathN.staticsDayFile + $"\\{i}_{month}_{year}.xml"))
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
                DayStaticsLoader.RunWorkerAsync(date.Date);
            }
        }
        #endregion

        #region Settings
        private void SettingsSaverProgress(object sender, ProgressChangedEventArgs e)
        {
            MainProgBarShow();
        }

        private void SettingsSaverComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            string errors = (string)e.Result;

            if (e.Cancelled)
            {
                log.NewLog(LogWindow.LogTypes.Warnung, LogWindow.LogThread.Einstellungen, LogWindow.LogActions.Speichern, "Abgebrochen", DateTime.Now, DateTime.Now - SettingsSaveStart);
                NewMsgItem(LogWindow.LogTypes.Warnung, LogWindow.LogThread.Einstellungen, "Speichern der Einstellungen abgebrochen!", null);
            }
            else
            {
                if (!string.IsNullOrEmpty(errors))
                {
                    log.NewLog(LogWindow.LogTypes.Fehler, LogWindow.LogThread.Einstellungen, LogWindow.LogActions.Speichern, errors, DateTime.Now, DateTime.Now - SettingsSaveStart);
                    NewMsgItem(LogWindow.LogTypes.Fehler, LogWindow.LogThread.Einstellungen, "Speichern der Einstellungen fehlerhaft!", "Siehe Log");
                }
                else
                {
                    log.NewLog(LogWindow.LogTypes.Erfolg, LogWindow.LogThread.Einstellungen, LogWindow.LogActions.Speichern, null, DateTime.Now, DateTime.Now - SettingsSaveStart);
                    NewMsgItem(LogWindow.LogTypes.Erfolg, LogWindow.LogThread.Einstellungen, "Speichern der Einstellungen erfolgreich!", null);
                }
            }
            MainProgBarHide();
        }

        private void SettingsSaverRun(object sender, DoWorkEventArgs e)
        {
            SettingsSaveStart = DateTime.Now;
            SettingsData setS = new SettingsData();

            try
            {
                setS.lastPayDate = lastPayDate;
                setS.StorageLimitedNumber = StorageLimitedNumber;
                setS.AppTheme = AppThemeStr;
                setS.Save();
            }
            catch (Exception ex)
            {
                e.Result = ex.Message;
            }
        }

        private void SettingsLoaderProgress(object sender, ProgressChangedEventArgs e)
        {
            MainProgBarShow();
        }

        private void SettingsLoaderComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            string errors = (string)e.Result;

            if (e.Cancelled)
            {
                log.NewLog(LogWindow.LogTypes.Warnung, LogWindow.LogThread.Einstellungen, LogWindow.LogActions.Laden, "Abgebrochen", DateTime.Now, DateTime.Now - SettingsLoadStart);
                NewMsgItem(LogWindow.LogTypes.Warnung, LogWindow.LogThread.Einstellungen, "Laden der Einstellungen abgebrochen!", null);
            }
            else
            {
                if (!string.IsNullOrEmpty(errors))
                {
                    log.NewLog(LogWindow.LogTypes.Fehler, LogWindow.LogThread.Einstellungen, LogWindow.LogActions.Laden, errors, DateTime.Now, DateTime.Now - SettingsLoadStart);
                    NewMsgItem(LogWindow.LogTypes.Fehler, LogWindow.LogThread.Einstellungen, "Laden der Einstellungen fehlerhaft!", "Siehe Log");
                }
                else
                {
                    log.NewLog(LogWindow.LogTypes.Erfolg, LogWindow.LogThread.Einstellungen, LogWindow.LogActions.Laden, null, DateTime.Now, DateTime.Now - SettingsLoadStart);
                    NewMsgItem(LogWindow.LogTypes.Erfolg, LogWindow.LogThread.Einstellungen, "Laden der Einstellungen erfolgreich!", null);
                    BuildSettings();
                }
            }
            MainProgBarHide();
        }

        private void SettingsLoaderRun(object sender, DoWorkEventArgs e)
        {
            SettingsLoadStart = DateTime.Now;
            string error = "";
            try
            {
                SettingsData setL = SettingsData.Load();
                try
                {
                    lastPayDate = setL.lastPayDate;
                }
                catch (Exception ex)
                {
                    error += $"\nLetzer Verkauf, {ex.Message}";
                }
                try
                {
                    StorageLimitedNumber = setL.StorageLimitedNumber;
                }
                catch (Exception ex)
                {
                    error += $"\nLager-Warn-Limit, {ex.Message}";
                }

                try
                {
                    AppThemeStr = setL.AppTheme;
                }
                catch (Exception ex)
                {
                    error += $"\nThema, {ex.Message}";
                }
            }
            catch (Exception ex)
            {
                error += $"\nLaden, {ex.Message}";
            }

            e.Result = error;
        }

        private void BuildSettings()
        {
            SettingsBuildStart = DateTime.Now;
            try
            {
                if (AppThemeStr == "System")
                {
                    SettingsDesignSystemTogSw.IsOn = true;
                    SettingsDesignDarkTogSw.IsOn = false;
                    SettingsDesignLightTogSw.IsOn = false;
                }
                else if (AppThemeStr == "Light")
                {
                    SettingsDesignSystemTogSw.IsOn = false;
                    SettingsDesignDarkTogSw.IsOn = false;
                    SettingsDesignLightTogSw.IsOn = true;
                }
                else if (AppThemeStr == "Dark")
                {
                    SettingsDesignSystemTogSw.IsOn = false;
                    SettingsDesignDarkTogSw.IsOn = true;
                    SettingsDesignLightTogSw.IsOn = false;
                }

                log.NewLog(LogWindow.LogTypes.Erfolg, LogWindow.LogThread.Einstellungen, LogWindow.LogActions.Bauen, null, DateTime.Now, DateTime.Now - SettingsBuildStart);
                NewMsgItem(LogWindow.LogTypes.Erfolg, LogWindow.LogThread.Einstellungen, "Bauen der Einstellungen erfolgreich!", null);
            }
            catch (Exception ex)
            {
                log.NewLog(LogWindow.LogTypes.Fehler, LogWindow.LogThread.Einstellungen, LogWindow.LogActions.Bauen, ex.Message, DateTime.Now, DateTime.Now - SettingsBuildStart);
                NewMsgItem(LogWindow.LogTypes.Fehler, LogWindow.LogThread.Einstellungen, "Bauen der Einstellungen fehlgeschlagen!", ex.Message);
            }
        }

        private void SettingsSaveBtn_Click(object sender, RoutedEventArgs e)
        {
            MsgBox msgBox = new MsgBox("Möchten Sie die Einstellungen wirklich speichern?", "Einstellungen speichern", MsgBox.MsgButtons.YesNo, MsgBox.MsgIcon.Question);
            msgBox.ShowDialog();

            if (msgBox.DialogResult == true)
            {
                if (SettingsDesignSystemTogSw.IsOn == true)
                {
                    AppThemeStr = "System";
                }
                else if (SettingsDesignLightTogSw.IsOn == true)
                {
                    AppThemeStr = "Light";
                }
                else if (SettingsDesignDarkTogSw.IsOn == true)
                {
                    AppThemeStr = "Dark";
                }

                SettingsSaver.RunWorkerAsync();
                SetAppTheme();
            }
        }

        private void SettingsCancelBtn_Click(object sender, RoutedEventArgs e)
        {
            BuildSettings();
        }

        private void SettingsRestoreBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Möchten Sie die Einstellungen wirklich auf Werkseinstellungen zurücksetzen?", "Einstellungen wiederherstellen", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {

                lastPayDate = DateTime.Today;
                StorageLimitedNumber = 10;
                AppThemeStr = "System";

                SettingsSaver.RunWorkerAsync();
                BuildSettings();
                log.NewLog(LogWindow.LogTypes.Info, LogWindow.LogThread.Einstellungen, LogWindow.LogActions.Wiederherstellen, null, DateTime.Now, TimeSpan.Zero);
                NewMsgItem(LogWindow.LogTypes.Info, LogWindow.LogThread.Einstellungen, "Die Einstellungen wurden Wiederhergestellt!", null);
            }
        }

        private void StartInstallationBtn_Click(object sender, RoutedEventArgs e)
        {
            AppInstallStart = DateTime.Now;

            lastPayDate = DateTime.Today.AddDays(-1);
            StorageLimitedNumber = 10;
            AppThemeStr = "System";
            SettingsSaver.RunWorkerAsync();

            Array.Resize(ref StorageSlotName, StorageSlotName.Length + 1);
            StorageSlotName[StorageSlotName.Length - 1] = "Default";

            Array.Resize(ref StorageSlotStatus, StorageSlotStatus.Length + 1);
            StorageSlotStatus[StorageSlotStatus.Length - 1] = false;

            Array.Resize(ref StorageSlotNumber, StorageSlotNumber.Length + 1);
            StorageSlotNumber[StorageSlotNumber.Length - 1] = 0;

            Array.Resize(ref StorageSlotPrice, StorageSlotPrice.Length + 1);
            StorageSlotPrice[StorageSlotPrice.Length - 1] = 0;
            StorageSaver.RunWorkerAsync();

            StaticsTotalStartDate = DateTime.Today;
            StaticsTotalCustomers = 0;
            StaticsTotalSoldProducts = 0;
            StaticsTotalGottenCash = 0;
            StaticsTotalLostCash = 0;
            StaticsTotalLostProducts = 0;
            TotalStaticsSaver.RunWorkerAsync();

            InstallationModePanel.Visibility = Visibility.Collapsed;
            TopBar.IsEnabled = true;
            AppInstallationMode = false;
            log.NewLog(LogWindow.LogTypes.Info, LogWindow.LogThread.Anwendung, LogWindow.LogActions.Aktiviert, null, DateTime.Now, DateTime.Now - AppInstallStart);
            NewMsgItem(LogWindow.LogTypes.Info, LogWindow.LogThread.Anwendung, $"{GetAssemblyTitle()} wurde aktiviert!", null);
            Reload();
        }

        private void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            Reload();
        }

        private void SettingsDesignSystemTogSw_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            SettingsDesignDarkTogSw.IsOn = false;
            SettingsDesignLightTogSw.IsOn = false;

            SettingsDesignSystemTogSw.IsEnabled = false;
            SettingsDesignDarkTogSw.IsEnabled = true;
            SettingsDesignLightTogSw.IsEnabled = true;
        }

        private void SettingsDesignDarkTogSw_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            SettingsDesignSystemTogSw.IsOn = false;
            SettingsDesignLightTogSw.IsOn = false;

            SettingsDesignSystemTogSw.IsEnabled = true;
            SettingsDesignDarkTogSw.IsEnabled = false;
            SettingsDesignLightTogSw.IsEnabled = true;
        }

        private void SettingsDesignLightTogSw_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            SettingsDesignSystemTogSw.IsOn = false;
            SettingsDesignDarkTogSw.IsOn = false;

            SettingsDesignSystemTogSw.IsEnabled = true;
            SettingsDesignDarkTogSw.IsEnabled = true;
            SettingsDesignLightTogSw.IsEnabled = false;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    log.Dispose();
                    Window.Dispose();
                    StorageLoader.Dispose();
                    StorageSaver.Dispose();
                    TotalStaticsLoader.Dispose();
                    TotalStaticsSaver.Dispose();
                    DayStaticsLoader.Dispose();
                    DayStaticsSaver.Dispose();
                    SettingsLoader.Dispose();
                    SettingsSaver.Dispose();
                }

                // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
                // TODO: Große Felder auf NULL setzen
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}