using Newtonsoft.Json;
using System;
using System.IO;
using System.Security;
using Warenwirtschaftssystem.Model.LocalAppData;

namespace Warenwirtschaftssystem.Model
{
    public class DataModel
    {
        // Attribute
        public ConnectionInfo ConnectionInfo { get; set; }
        private readonly string ConnInfoJSONPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Warenwirtschaftssystem\\Database\\ConnectionInfo.json";
        private readonly string StandardPrintersJSONPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Warenwirtschaftssystem\\Settings\\StandardPrinters.json";

        public string MainConnectionString { get; set; }
        public StandardPrinters StandardPrinters { get; set; }

        // Konstruktor
        public DataModel()
        {
            #region Connection Info aus JSON laden

            if (File.Exists(ConnInfoJSONPath))
            {
                using (JsonReader json = new JsonTextReader(new StreamReader(ConnInfoJSONPath)))
                {
                    ConnectionInfo = new JsonSerializer().Deserialize<ConnectionInfo>(json);
                }
            }

            #endregion

            #region Standarddrucker aus JSON laden

            if (File.Exists(StandardPrintersJSONPath))
            {
                using (JsonReader json = new JsonTextReader(new StreamReader(StandardPrintersJSONPath)))
                {
                    StandardPrinters = new JsonSerializer().Deserialize<StandardPrinters>(json);
                }
            }

            if (StandardPrinters == null)
            {
                StandardPrinters = new StandardPrinters
                {
                    BonPrinter = "",
                    DocumentPrinter = "",
                    TagPrinter = "",
                    TagLandscapePrinter = ""
                };
            }

            #endregion
        }

        public void SaveToJSON()
        {
            #region ConnectionInfo

            Directory.CreateDirectory(Path.GetDirectoryName(ConnInfoJSONPath));
            using (StreamWriter json = new StreamWriter(ConnInfoJSONPath))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(json, ConnectionInfo);
            }

            #endregion

            #region StandardPrinters

            Directory.CreateDirectory(Path.GetDirectoryName(StandardPrintersJSONPath));
            using (StreamWriter json = new StreamWriter(StandardPrintersJSONPath))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(json, StandardPrinters);
            }

            #endregion
        }
    }

    public class StandardPrinters
    {
        public string TagPrinter { get; set; }
        public string TagLandscapePrinter { get; set; }
        public string BonPrinter { get; set; }
        public string DocumentPrinter { get; set; }
    }
}
