using System;
using System.IO;
using System.Xml.Serialization;

namespace SellerScreen
{
    [Serializable]
    public class SettingsData
    {
        public DateTime lastPayDate;
        public short StorageLimitedNumber;
        public string AppTheme;

        public void Save()
        {
            PathName path = new PathName();

            using (FileStream stream = new FileStream(path.settingsFile + "Settings.xml", FileMode.Create))
            {
                XmlSerializer XML = new XmlSerializer(typeof(SettingsData));
                XML.Serialize(stream, this);
            }
        }

        public static SettingsData Load()
        {
            PathName path = new PathName();

            using (FileStream stream = new FileStream(path.settingsFile + "Settings.xml", FileMode.Open))
            {
                XmlSerializer XML = new XmlSerializer(typeof(SettingsData));
                return (SettingsData)XML.Deserialize(stream);
            }
        }
    }
}