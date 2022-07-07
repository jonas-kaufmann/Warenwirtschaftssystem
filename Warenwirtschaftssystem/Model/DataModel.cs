using System.Text.Json;
using System;
using System.IO;
using Warenwirtschaftssystem.Model.LocalAppData;
using Warenwirtschaftssystem.Model.Db;
using System.Globalization;

namespace Warenwirtschaftssystem.Model
{
    public class DataModel
    {
        public static readonly CultureInfo CultureInfo = CultureInfo.CreateSpecificCulture("de-DE");

        public ConnectionInfo ConnectionInfo { get; set; }
        private readonly string ConnInfoJSONPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Warenwirtschaftssystem\\Database\\ConnectionInfo.json";
        private readonly string StandardPrintersJSONPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Warenwirtschaftssystem\\Settings\\StandardPrinters.json";

        public string MainConnectionString { get; set; }
        public StandardPrinters StandardPrinters { get; set; }

        public DbModel CreateDbConnection() => new DbModel(MainConnectionString);

        public DataModel()
        {
            #region Connection Info aus JSON laden
            if (File.Exists(ConnInfoJSONPath))
            {
                try
                {

                    ConnectionInfo = JsonSerializer.Deserialize<ConnectionInfo>(File.ReadAllText(ConnInfoJSONPath));
                }
                catch
                {
                    ConnectionInfo = new ConnectionInfo();
                }
            }

            #endregion

            #region Standarddrucker aus JSON laden

            if (File.Exists(StandardPrintersJSONPath))
            {
                try
                {
                    using (var jsonFile = File.OpenRead(StandardPrintersJSONPath))
                    {
                        StandardPrinters = JsonSerializer.Deserialize<StandardPrinters>(File.ReadAllText(StandardPrintersJSONPath));
                    }
                }
                catch { }
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
            JsonSerializerOptions jsonOptions = new JsonSerializerOptions { WriteIndented = true };

            #region ConnectionInfo

            Directory.CreateDirectory(Path.GetDirectoryName(ConnInfoJSONPath));
            File.WriteAllText(ConnInfoJSONPath, JsonSerializer.Serialize(ConnectionInfo, typeof(ConnectionInfo), jsonOptions));

            #endregion

            #region StandardPrinters

            Directory.CreateDirectory(Path.GetDirectoryName(StandardPrintersJSONPath));
            File.WriteAllText(StandardPrintersJSONPath, JsonSerializer.Serialize(StandardPrinters, typeof(StandardPrinters), jsonOptions));
        }

        #endregion
    }

    public class StandardPrinters
    {
        public string TagPrinter { get; set; }
        public string TagLandscapePrinter { get; set; }
        public string BonPrinter { get; set; }
        public string DocumentPrinter { get; set; }
    }
}
