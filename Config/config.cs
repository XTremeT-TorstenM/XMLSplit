using System;
using System.IO;
using System.Collections.Generic;

using XMLSplit.Logging;

namespace XMLSplit.Configuration {
    public class Config {
        private Dictionary<string, string> configs;
        public Config(Log log) {
            configs = new Dictionary<string, string>();
            foreach(string line in File.ReadLines("settings.conf")) {
                if (line.StartsWith("//")) {
                    continue;
                }
                var val = line.Split('=');
                string key = val[0].Trim();
                string value = val[1].Trim();
                configs.Add(key, value);
            }
            Console.WriteLine("Config Cons");
        }

        public void showConfig(string key) {
            Console.WriteLine(configs[key]);
        }

        public string getCSVFilename() {
            // existiert CSV File ?
            if (File.Exists(this.configs["csvFile"])) {
                return this.configs["csvFile"];
            }
            else {
                // logging / Ausgabe / exit
                Console.WriteLine("Error: Wrong CSV file !");
                Environment.Exit(0);
                return "error";
            }
        }

        public string getProductionPath() {
            if (Directory.Exists(this.configs["prodPath"])) {
                return this.configs["prodPath"];
            }
            else {
                // logging / Ausgabe / exit
                Console.WriteLine("Error: Wrong path to production files !");
                Environment.Exit(0);
                return "error";
            }
        }

        public bool isBackup() {
            return bool.Parse(this.configs["backup"]);
        }
        public string getBackupPath() {
            if (!Directory.Exists(this.configs["backupDir"])) {
                Directory.CreateDirectory(this.configs["backupDir"]);
            }
            return this.configs["backupDir"];
        }
    }
}