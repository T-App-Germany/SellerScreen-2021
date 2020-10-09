using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SellerScreen
{
    public partial class MsgBox : Window
    {
        public MsgBox(string message, string img, bool okBtn, bool cancelBtn, bool yesBtn, bool noBtn)
        {
            InitializeComponent();

            MessageLbl.Content = message;

            if (img == "error")
            {
                Img.Source = new BitmapImage(new Uri(@"Resources/Button-7-close-icon.png", UriKind.Relative));
            }
            if (img == "warning")
            {
                Img.Source = new BitmapImage(new Uri(@"Resources/Problem-warning-icon.png", UriKind.Relative));
            }
            if (img == "info")
            {
                Img.Source = new BitmapImage(new Uri(@"Resources/Problem-info-icon.png", UriKind.Relative));
            }

            if (okBtn == false)
            {
                OkBtn.Visibility = Visibility.Collapsed;
            }
            if (cancelBtn == false)
            {
                CancelBtn.Visibility = Visibility.Collapsed;
            }
            if (yesBtn == false)
            {
                YesBtn.Visibility = Visibility.Collapsed;
            }
            if (noBtn == false)
            {
                NoBtn.Visibility = Visibility.Collapsed;
            }
        }

        private void YesBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
