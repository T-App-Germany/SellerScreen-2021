using System;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.ComponentModel;

namespace SellerScreen
{
    public partial class LogWindow : Window
    {
        public int id = -1;
        public bool[] displayLogTypes = new bool[5];
        private readonly PathName pathN = new PathName();

        public LogWindow()
        {
            InitializeComponent();
        }

        public void SaveLog(string textToSave)
        {
            try
            {
                try
                {
                    string[] lines = { textToSave };
                    File.AppendAllLines(Path.Combine(pathN.logFile, $"pcs-{DateTime.Now.Day}_{DateTime.Now.Month}_{DateTime.Now.Year}-log.txt"), lines);
                }
                catch
                {
                    File.WriteAllText(Path.Combine(pathN.logFile, $"pcs-{DateTime.Now.Day}_{DateTime.Now.Month}_{DateTime.Now.Year}-log.txt"), textToSave);
                }
            }
            catch
            {
                MessageBox.Show("Schreiben der Log-Datei nicht möglich!", "Log Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void NewLog(string textToWrite, int logType)
        {
            #region LogTypes
            //0 = Application
            //1 = successful
            //2 = error/failed
            //3 = events
            //4 = appTask
            #endregion

            if (!string.IsNullOrEmpty(textToWrite))
            {
                if (displayLogTypes[logType] == true)
                {
                    id++;
                    string[] separatingStrings = { " " };
                    string[] words = textToWrite.Split(separatingStrings, StringSplitOptions.RemoveEmptyEntries);
                    string timeStamp = $" [{DateTime.Now.ToShortDateString()}, {DateTime.Now.ToLongTimeString()}] {id}:";
                    txtblock.Inlines.Add(timeStamp);

                    foreach (var word in words)
                    {
                        if (word == "Storage" || word == "Shop" || word == "Page" || word == "StaticsData/Total" || word == "StaticsData/Day" || word == "Settings" || word == "Mainmenu")
                        {
                            txtblock.Inlines.Add(new Run($" {word}") { Foreground = Brushes.Orange });
                        }
                        else if (word == "Application" || word == "Main_Window" || word == "Log_Window" || word == "EditStorage_Window")
                        {
                            txtblock.Inlines.Add(new Run($" {word}") { Foreground = Brushes.DarkBlue });
                        }
                        else if (word == "successful" || word == "Reloaded" || word == "Restored")
                        {
                            txtblock.Inlines.Add(new Run($" {word}") { Foreground = Brushes.Green });
                        }
                        else if (word == "closed" || word == "failed!" || word == "error!" || word == "Error" || word == "Restoring" || word == "not")
                        {
                            txtblock.Inlines.Add(new Run($" {word}") { Foreground = Brushes.Red });
                        }
                        else if (word == "PageID" || word == "LastPayDate" || word == "Today")
                        {
                            txtblock.Inlines.Add(new Run($" {word}") { Foreground = Brushes.Brown });
                        }
                        else if (word == "Reloading..." || word == "Showing" || word == "Closing")
                        {
                            txtblock.Inlines.Add(new Run($" {word}") { Foreground = Brushes.Peru });
                        }
                        else
                        {
                            txtblock.Inlines.Add(new Run($" {word}"));
                        }
                    }
                    txtblock.Inlines.Add(";\n");
                    SaveLog($"{timeStamp} {textToWrite}");
                }
            }

            ScrollView.ScrollToBottom();
        }

        private void Logwindow_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            WindowState = WindowState.Minimized;
            ShowInTaskbar = false;
            ShowActivated = false;
        }

        public void SendEmail()
        {

        }
    }
}