using System;

namespace SellerScreen
{
    public class PathName
    {
        public readonly string settingsFile = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\T-App Germany\\SellerScreen\\settings\\";
        public readonly string staticsFile = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\T-App Germany\\SellerScreen\\statics\\";
        public readonly string graphicsFile = Environment.GetFolderPath(Environment.SpecialFolder.CommonPictures) + "\\T-App Germany\\SellerScreen\\Graphics\\";
        public readonly string tempFile = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\T-App Germany\\SellerScreen\\temp\\";
        public readonly string logFile = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\T-App Germany\\SellerScreen\\log\\";
    }
}