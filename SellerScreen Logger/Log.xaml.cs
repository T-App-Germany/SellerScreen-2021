using System;
using System.IO;
using System.Windows;
using System.ComponentModel;
using System.Xml;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Documents;
using System.Windows.Data;
using System.Globalization;

namespace SellerScreen_Logger
{
    public partial class LogWindow : Window
    {
        public enum LogTypes
        {
            Info,
            Success,
            Error,
            Warning
        };
        public BackgroundWorker LogSaver = new BackgroundWorker();
        public int id = -1;
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
        }

        private void SaveLog(object sender, DoWorkEventArgs e)
        {
            string textToSave = (string)e.Argument;
            try
            {
                string[] lines = { textToSave };
                File.AppendAllLines(Path.Combine(logFile, $"log-{DateTime.Now.Day}_{DateTime.Now.Month}_{DateTime.Now.Year}.txt"), lines);
            }
            catch
            {
                MessageBox.Show("Schreiben der Log-Datei nicht möglich!", "Log Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
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
                string timeStamp = $"{DateTime.Now.ToShortDateString()}, {DateTime.Now.ToLongTimeString()} Uhr";
                string startEndStamp = $"Start: {start.ToShortDateString()}, {start.ToLongTimeString()} Uhr          Ende: {end.ToShortDateString()}, {end.ToLongTimeString()} Uhr";
                string info = $"{id}: {logType}: ";
                //LogSaver.RunWorkerAsync($"[{timeStamp}] {info} {text}");

                if (logType == LogTypes.Error)
                {
                    LogErrorHeaderTemp.Text = header;
                    LogErrorTextTemp.Text = text;
                    if (!string.IsNullOrEmpty(ex))
                    {
                        LogErrorTextTemp.Inlines.Add(new LineBreak());
                        LogErrorTextTemp.Inlines.Add(new Run(ex) { FontStyle = FontStyles.Italic });
                    }
                    LogErrorInfoTxtTemp.Text = $"ID: {id}          {startEndStamp}";

                    xamlString = XamlWriter.Save(LogErrorItemTemp);
                    stringReader = new StringReader(xamlString);
                    xmlReader = XmlReader.Create(stringReader);
                    item = (Border)XamlReader.Load(xmlReader);
                    item.Tag = id;
                }
                else if (logType == LogTypes.Success)
                {
                    LogSuccessHeaderTemp.Text = header;
                    LogSuccessTextTemp.Text = text;
                    if (!string.IsNullOrEmpty(ex))
                    {
                        LogSuccessTextTemp.Inlines.Add(new LineBreak());
                        LogSuccessTextTemp.Inlines.Add(new Run(ex) { FontStyle = FontStyles.Italic });
                    }
                    LogSuccessInfoTxtTemp.Text = $"ID: {id}          {startEndStamp}";

                    xamlString = XamlWriter.Save(LogSuccessItemTemp);
                    stringReader = new StringReader(xamlString);
                    xmlReader = XmlReader.Create(stringReader);
                    item = (Border)XamlReader.Load(xmlReader);
                    item.Tag = id;
                }
                else if (logType == LogTypes.Warning)
                {
                    LogWarningHeaderTemp.Text = header;
                    LogWarningTextTemp.Text = text;
                    if (!string.IsNullOrEmpty(ex))
                    {
                        LogErrorTextTemp.Inlines.Add(new LineBreak());
                        LogErrorTextTemp.Inlines.Add(new Run(ex) { FontStyle = FontStyles.Italic });
                    }
                    LogWarningInfoTxtTemp.Text = $"ID: {id}          {timeStamp}";

                    xamlString = XamlWriter.Save(LogWarningItemTemp);
                    stringReader = new StringReader(xamlString);
                    xmlReader = XmlReader.Create(stringReader);
                    item = (Border)XamlReader.Load(xmlReader);
                    item.Tag = id;
                }
                else if (logType == LogTypes.Info)
                {
                    LogInfoTextTemp.Text = text;
                    if (!string.IsNullOrEmpty(ex))
                    {
                        LogInfoTextTemp.Inlines.Add(new LineBreak());
                        LogInfoTextTemp.Inlines.Add(new Run(ex) { FontStyle = FontStyles.Italic });
                    }
                    LogInfoInfoTxtTemp.Text = $"ID: {id}          {timeStamp}";

                    xamlString = XamlWriter.Save(LogInfoItemTemp);
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
    }
}
