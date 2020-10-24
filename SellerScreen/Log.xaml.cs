using System;
using System.IO;
using System.Windows;
using System.ComponentModel;
using System.Xml;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Documents;

namespace SellerScreen
{
    public partial class LogWindow : IDisposable
    {
        public enum LogTypes
        {
            Info,
            Erfolg,
            Fehler,
            Warnung
        };
        public enum LogActions
        {
            Laden,
            Speichern,
            Bauen,
            Ändern,
            Geladen,
            Herunterfahren,
            Abbrechen,
            Lesen,
            Verkaufen,
            Stornieren,
            Rücknehmen,
            Wiederherstellen,
            Aktiviert
        };
        public enum LogThread
        {
            Startseite,
            Einstellungen,
            Hilfe_Center,
            Kasse,
            Schichtverwaltung,
            Gesamtstatistiken,
            Tagesstatistiken,
            Benutzerverwaltung,
            Lager,
            Dateiverwaltung,
            Anwendung,
            WindowsAppThema,
            Ansichtswechsel
        }

        private readonly BackgroundWorker LogSaver = new BackgroundWorker();
        private int id = -1;
        private string[] linesToSave = Array.Empty<string>();
        public Visibility[] displayLogTypes = new Visibility[4];
        private readonly PathName pathN = new PathName();
        private readonly ThemeData themeData = new ThemeData();
        public string AppThemeStr = "System";

        public LogWindow()
        {
            InitializeComponent();
            LogSaver.DoWork += new DoWorkEventHandler(SaveLog);
            LogSaver.RunWorkerCompleted += new RunWorkerCompletedEventHandler(LogSaved);
            LogSaver.ProgressChanged += new ProgressChangedEventHandler(SavingLog);
            Directory.CreateDirectory(Path.GetDirectoryName(pathN.logFile));
            SetAppTheme();
        }

        private void SavingLog(object sender, ProgressChangedEventArgs e)
        {
            ProgBar.Value = e.ProgressPercentage;
            ProgBar.Visibility = Visibility.Visible;
        }

        private void LogSaved(object sender, RunWorkerCompletedEventArgs e)
        {
            ProgBar.Value = 0;
            ProgBar.Visibility = Visibility.Collapsed;

            if (linesToSave != Array.Empty<string>() && LogSaver.IsBusy == false)
            {
                LogSaver.RunWorkerAsync(linesToSave);
                linesToSave = Array.Empty<string>();
            }
        }

        private void SaveLog(object sender, DoWorkEventArgs e)
        {
            string[] lines = (string[])e.Argument;
            try
            {
                File.AppendAllLines(Path.Combine(pathN.logFile, $"log-{DateTime.Now.Day}_{DateTime.Now.Month}_{DateTime.Now.Year}.txt"), lines);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Schreiben der Log-Datei nicht möglich!\n\n{ex}", "Log Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void NewLog(LogTypes type, LogThread thread, LogActions action, string exception, DateTime end, TimeSpan duration)
        {
            string xamlString;
            StringReader stringReader;
            XmlReader xmlReader;
            Border item = new Border();
            id++;
            string info = $"{DateTime.Now.ToShortDateString()}, {DateTime.Now.ToLongTimeString()} Uhr";
            info += $" | {id}";
            int repeat = 5 - id.ToString().Length;
            for (int i = 0; i < repeat; i++)
            {
                info += " ";
            }

            info += $" | {thread}";
            repeat = 18 - thread.ToString().Length;
            for (int i = 0; i < repeat; i++)
            {
                info += " ";
            }

            info += $" | {type}";
            repeat = 7 - type.ToString().Length;
            for (int i = 0; i < repeat; i++)
            {
                info += " ";
            }

            info += $" | {action}";
            if (!string.IsNullOrEmpty(exception))
            {
                char trim = '\n';
                exception.TrimStart(trim);
                repeat = 9 - action.ToString().Length;
                for (int i = 0; i < repeat; i++)
                {
                    info += " ";
                }
                info += $"{exception}";
            }
            LogTxtBlock.Text += $"\n{info}";

            Array.Resize(ref linesToSave, linesToSave.Length + 1);
            linesToSave[linesToSave.Length - 1] = $"\n{info}";

            if (LogSaver.IsBusy == false)
            {
                LogSaver.RunWorkerAsync(linesToSave);
                linesToSave = Array.Empty<string>();
            }

            if (type == LogTypes.Fehler)
            {
                LogErrorHeaderTemp.Text = thread.ToString();
                LogErrorTextTemp.Text = action.ToString();
                if (!string.IsNullOrEmpty(exception))
                {
                    LogErrorTextTemp.Inlines.Add(new LineBreak());
                    LogErrorTextTemp.Inlines.Add(new Run(exception) { FontStyle = FontStyles.Italic });
                }
                LogErrorInfoTxtTemp.Text = $"ID: {id}          {end.ToShortDateString()}, {end.ToLongTimeString()} Uhr          Dauer: {duration.TotalSeconds} Sekunden";

                xamlString = XamlWriter.Save(LogErrorItemTemp);
                stringReader = new StringReader(xamlString);
                xmlReader = XmlReader.Create(stringReader);
                item = (Border)XamlReader.Load(xmlReader);
                item.Tag = id;
            }
            else if (type == LogTypes.Warnung)
            {
                LogWarningHeaderTemp.Text = thread.ToString();
                LogWarningTextTemp.Text = action.ToString();
                if (!string.IsNullOrEmpty(exception))
                {
                    LogWarningTextTemp.Inlines.Add(new LineBreak());
                    LogWarningTextTemp.Inlines.Add(new Run(exception) { FontStyle = FontStyles.Italic });
                }
                LogWarningInfoTxtTemp.Text = $"ID: {id}          {end.ToShortDateString()}, {end.ToLongTimeString()} Uhr";

                xamlString = XamlWriter.Save(LogWarningItemTemp);
                stringReader = new StringReader(xamlString);
                xmlReader = XmlReader.Create(stringReader);
                item = (Border)XamlReader.Load(xmlReader);
                item.Tag = id;
            }

            LogItemPanel.Children.Add(item);
            ScrollView.ScrollToBottom();
        }

        private void LogWindow_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            WindowState = WindowState.Minimized;
            ShowInTaskbar = false;
            ShowActivated = false;
        }

        private void ChromeMinimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Log_StateChanged(object sender, EventArgs e)
        {
            ScrollView.ScrollToBottom();
        }

        private void LogModeSwitch_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (LogModeSwitch.IsOn == false)
            {
                NormalModeGrid.Visibility = Visibility.Collapsed;
                ExpertModeGrid.Visibility = Visibility.Visible;
            }
            else
            {
                NormalModeGrid.Visibility = Visibility.Visible;
                ExpertModeGrid.Visibility = Visibility.Collapsed;
            }
        }

        public void SetAppTheme()
        {
            if (AppThemeStr == "System")
            {
                string initialTheme = themeData.GetWindowsAppTheme().ToString();
                if (initialTheme == "Dark")
                    ApplyDarkTheme();
                else if (initialTheme == "Light")
                    ApplyLightTheme();
            }
            else if (AppThemeStr == "Light")
                ApplyLightTheme();
            else if (AppThemeStr == "Dark")
                ApplyDarkTheme();
        }

        private void ApplyLightTheme()
        {
            TextFontColor.Background = themeData.GetLightTheme("TextFontColor");
            SideBarsColor.Background = themeData.GetLightTheme("SideBarsColor");
            PageBackgroudColor.Background = themeData.GetLightTheme("PageBackgroudColor");
            ChromeBtnColor.Background = themeData.GetLightTheme("ChromeBtnColor");
        }

        private void ApplyDarkTheme()
        {
            TextFontColor.Background = themeData.GetDarkTheme("TextFontColor");
            SideBarsColor.Background = themeData.GetDarkTheme("SideBarsColor");
            PageBackgroudColor.Background = themeData.GetDarkTheme("PageBackgroudColor");
            ChromeBtnColor.Background = themeData.GetDarkTheme("ChromeBtnColor");
        }

        public void Dispose()
        {
            ((IDisposable)LogSaver).Dispose();
        }
    }
}