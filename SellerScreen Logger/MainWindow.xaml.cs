using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Xml;

namespace SellerScreen
{
    public partial class LogWindow : Window
    {
        public enum LogTypes
        {
            Info,
            Erfolg,
            Fehler,
            Warnung
        };
        public BackgroundWorker LogSaver = new BackgroundWorker();
        public int id = -1;
        private string[] linesToSave = Array.Empty<string>();
        public Visibility[] displayLogTypes = new Visibility[4];
        public readonly string logFile = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\T-App Germany\\SellerScreen\\log\\";

        public LogWindow()
        {
            InitializeComponent();
            LogSaver.DoWork += new DoWorkEventHandler(SaveLog);
            LogSaver.RunWorkerCompleted += new RunWorkerCompletedEventHandler(LogSaved);
            LogSaver.ProgressChanged += new ProgressChangedEventHandler(SavingLog);
            Directory.CreateDirectory(Path.GetDirectoryName(logFile));
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
                File.AppendAllLines(Path.Combine(logFile, $"log-{DateTime.Now.Day}_{DateTime.Now.Month}_{DateTime.Now.Year}.txt"), lines);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Schreiben der Log-Datei nicht möglich!\n\n{ex}", "Log Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void NewLog(string text, string header, string ex, DateTime start, DateTime end, LogTypes logType)
        {
            if (!string.IsNullOrEmpty(text))
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

                info += $" | {logType}";
                repeat = 7 - logType.ToString().Length;
                for (int i = 0; i < repeat; i++)
                {
                    info += " ";
                }

                if (logType != LogTypes.Info)
                {
                    info += $" | {header}";
                    repeat = 17 - header.ToString().Length;
                    for (int i = 0; i < repeat; i++)
                    {
                        info += " ";
                    }
                }

                Array.Resize(ref linesToSave, linesToSave.Length + 1);
                info += $" | {text}";
                if (!string.IsNullOrEmpty(ex))
                {
                    info += $"\n";
                    for (int i = 0; i < 35; i++)
                    {
                        info += " ";
                    }
                    info += $"Bericht:  {ex}";
                }
                LogTxtBlock.Text += $"\n{info}";
                linesToSave[linesToSave.Length - 1] = $"\n{info}";
                if (LogSaver.IsBusy == false)
                {
                    LogSaver.RunWorkerAsync(linesToSave);
                    linesToSave = Array.Empty<string>();
                }

                if (logType == LogTypes.Fehler)
                {
                    LogErrorHeaderTemp.Text = header;
                    LogErrorTextTemp.Text = text;
                    if (!string.IsNullOrEmpty(ex))
                    {
                        LogErrorTextTemp.Inlines.Add(new LineBreak());
                        LogErrorTextTemp.Inlines.Add(new Run(ex) { FontStyle = FontStyles.Italic });
                    }
                    LogErrorInfoTxtTemp.Text = $"ID: {id}          Start: {start.ToShortDateString()}, {start.ToLongTimeString()} Uhr          Ende: {end.ToShortDateString()}, {end.ToLongTimeString()} Uhr";

                    xamlString = XamlWriter.Save(LogErrorItemTemp);
                    stringReader = new StringReader(xamlString);
                    xmlReader = XmlReader.Create(stringReader);
                    item = (Border)XamlReader.Load(xmlReader);
                    item.Tag = id;
                }
                else if (logType == LogTypes.Warnung)
                {
                    LogWarningHeaderTemp.Text = header;
                    LogWarningTextTemp.Text = text;
                    if (!string.IsNullOrEmpty(ex))
                    {
                        LogWarningTextTemp.Inlines.Add(new LineBreak());
                        LogWarningTextTemp.Inlines.Add(new Run(ex) { FontStyle = FontStyles.Italic });
                    }
                    LogWarningInfoTxtTemp.Text = $"ID: {id}          {DateTime.Now.ToShortDateString()}, {DateTime.Now.ToLongTimeString()} Uhr";

                    xamlString = XamlWriter.Save(LogWarningItemTemp);
                    stringReader = new StringReader(xamlString);
                    xmlReader = XmlReader.Create(stringReader);
                    item = (Border)XamlReader.Load(xmlReader);
                    item.Tag = id;
                }

                LogItemPanel.Children.Add(item);
                ScrollView.ScrollToBottom();
            }
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

        private void LogModeSwitch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
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
    }
}