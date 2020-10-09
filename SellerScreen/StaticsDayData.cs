using System;
using System.IO;
using System.Xml.Serialization;

namespace SellerScreen
{
    [Serializable]
    public class StaticsDayData
    {
        public string[] SoldSlotName = Array.Empty<string>();
        public short[] SoldSlotNumber = Array.Empty<short>();
        public double[] SoldSlotCash = Array.Empty<double>();
        public double[] SoldSlotSinglePrice = Array.Empty<double>();
        public int LostProducts;
        public double LostCash;

        public TimeSpan[] pcTime = new TimeSpan[2];
        public int[] pcUsers = new int[2];
        public string[] userList;

        public void Save()
        {
            PathName pathN = new PathName();

            using (FileStream stream = new FileStream(path: $"{pathN.staticsFile}{DateTime.Now.Day}_{DateTime.Now.Month}_{DateTime.Now.Year}.xml", FileMode.Create))
            {
                XmlSerializer XML = new XmlSerializer(typeof(StaticsDayData));
                XML.Serialize(stream, this);
            }
        }

        public static StaticsDayData Load(DateTime date)
        {
            string day = date.Day.ToString();
            string month = date.Month.ToString();
            string year = date.Year.ToString();

            PathName pathN = new PathName();

            using (FileStream stream = new FileStream($"{pathN.staticsFile}{day}_{month}_{year}.xml", FileMode.Open))
            {
                XmlSerializer XML = new XmlSerializer(typeof(StaticsDayData));
                return (StaticsDayData)XML.Deserialize(stream);
            }
        }
    }
}