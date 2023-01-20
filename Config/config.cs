using System;
using System.IO;
using System.Collections.Generic;

using XMLSplit.Logging;

namespace XMLSplit.Configuration {
    // Config Class um alle moeglichen Einstellungen aus einer Datei zu lesen
    public class Config {
        // Dictionary fuer jeden Eintrag
        private Dictionary<string, string> configs;
        public Config(Log log) {
            configs = new Dictionary<string, string>();
            // lese jede Zeile aus Config File (evtl HardCoding noch mit Argument Liste umgehen)
            foreach(string line in File.ReadLines("settings.conf")) {
                // skip Kommentare
                if (line.StartsWith("//")) {
                    continue;
                }
                // split am = Zeichen
                var val = line.Split('=');
                // Key des Dict vor dem =
                string key = val[0].Trim();
                // Value des Dict nach dem =
                string value = val[1].Trim();
                // Eintrag hinzufuegen
                configs.Add(key, value);
            }
        }

        // gibt den Dateinamen des CSV Files aus der Config zurueck
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

        // gibt den Pfad des Produktions Ordners zurueck
        public string getProductionPath() {
            // existiert der Ordner
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

        // gibt Pfad zum Verzeichnis zurueck welches XSLT Dateien enthaelt
        public string getXSLTPath() {
            return this.configs["xsltDir"];
        }

        // gibt zurueck ob Backup Flag gesetzt ist und somit das Backup ausgefuehrt wird
        public bool isBackup() {
            return bool.Parse(this.configs["backup"]);
        }

        // gibt den Pfad fuer Backups zurueck
        public string getBackupPath() {
            // ist das Backup Verzeichnis vorhanden 
            if (!Directory.Exists(this.configs["backupDir"])) {
                // erstelle das Verzeichniss
                Directory.CreateDirectory(this.configs["backupDir"]);
            }
            return this.configs["backupDir"];
        }

        public bool isDeleteXMLFile() {
            return bool.Parse(this.configs["delXMLFile"]);
        }
    }
}