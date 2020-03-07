using System.Text.Json;
using System;
using System.IO;
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
                using (var jsonFile = File.OpenRead(ConnInfoJSONPath))
                {
                    var task = JsonSerializer.DeserializeAsync<ConnectionInfo>(jsonFile).AsTask();
                    task.Wait();
                    ConnectionInfo = task.Result;
                }
            }

            #endregion

            #region Standarddrucker aus JSON laden

            if (File.Exists(StandardPrintersJSONPath))
            {
                using (var jsonFile = File.OpenRead(StandardPrintersJSONPath))
                {
                    var task = JsonSerializer.DeserializeAsync<StandardPrinters>(jsonFile).AsTask();
                    task.Wait();
                    StandardPrinters = task.Result;
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
            JsonSerializerOptions jsonOptions = new JsonSerializerOptions { WriteIndented = true };

            #region ConnectionInfo

            Directory.CreateDirectory(Path.GetDirectoryName(ConnInfoJSONPath));
            using (var json = File.OpenWrite(ConnInfoJSONPath))
            {
                JsonSerializer.SerializeAsync(json, ConnectionInfo, typeof(ConnectionInfo), jsonOptions).Wait();
            }

            #endregion

            #region StandardPrinters

            Directory.CreateDirectory(Path.GetDirectoryName(StandardPrintersJSONPath));
            using (var json = File.OpenWrite(StandardPrintersJSONPath))
            {
                JsonSerializer.SerializeAsync(json, StandardPrinters, typeof(StandardPrinters), jsonOptions).Wait();
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
