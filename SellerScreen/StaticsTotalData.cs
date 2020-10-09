using System;
using System.IO;
using System.Xml.Serialization;

namespace SellerScreen
{
    [Serializable]
    public class StaticsTotalData
    {
        public DateTime startDate;
        public int totalCustomers;
        public int totalSoldProducts;
        public double totalGottenCash;
        public int totalLostProducts;
        public double totalLostCash;

        public TimeSpan[] totalPcTime = new TimeSpan[2];
        public int[] totalPcUsers = new int[2];

        public string[] mostSoldProductsName = new string[5];
        public string[] highestEarningsProductsName = new string[5];
        public int[] mostSoldProductsNumber = new int[5];
        public double[] highestEarningsProductsNumber = new double[5];
        public double[] mostSoldProductsSinglePrice = new double[5];
        public double[] highestEarningsProductsSinglePrice = new double[5];

        public string[] productsNameList = Array.Empty<string>();
        public int[] productsNumberList = Array.Empty<int>();
        public double[] productsCashList = Array.Empty<double>();
        public double[] productsSinglePriceList = Array.Empty<double>();

        public void Save()
        {
            PathName pathN = new PathName();

            using (FileStream stream = new FileStream($"{pathN.settingsFile}TotalStatics.xml", FileMode.Create))
            {
                XmlSerializer XML = new XmlSerializer(typeof(StaticsTotalData));
                XML.Serialize(stream, this);
            }
        }

        public static StaticsTotalData Load()
        {
            PathName pathN = new PathName();

            using (FileStream stream = new FileStream($"{pathN.settingsFile}TotalStatics.xml", FileMode.Open))
            {
                XmlSerializer XML = new XmlSerializer(typeof(StaticsTotalData));
                return (StaticsTotalData)XML.Deserialize(stream);
            }
        }
    }
}