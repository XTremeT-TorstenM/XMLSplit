using System;
using System.IO;
using System.Collections.Generic;

using XMLSplit.Logging;

namespace XMLSplit.Configuration {
    // Config Class um alle Einstellungen aus einer Datei zu lesen
    public class Config {
        // Dictionary fuer jeden Eintrag
        private Dictionary<string, string> configs;
        private Log log;
        public Config(Log log) {
            this.log = log;
            this.log.addLog("\n# read configuration file settings.conf");
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
            this.log.addLog("\nConfiguration:");
            // fuer jedes Key Value Paar in dem Dictionary erstelle Eintrag im Log
            foreach(var kvp in this.configs) {
                this.log.addLog(string.Format("{0} = {1}", kvp.Key, kvp.Value));
            }
            // Log Directory setzen
            this.log.setLogDir(this.getLogDir());
            this.log.setLog(this.isLog());
        }

        // gibt den Dateinamen des CSV Files aus der Config zurueck
        public string getCSVFilename() {
            // existiert CSV File ?
            if (File.Exists(this.configs["csvFile"])) {
                return this.configs["csvFile"];
            }
            else {
                // logging / Ausgabe / exit
                this.log.addLog("\nError: CSV file doesn`t exists!");
                Console.WriteLine("Error: CSV file doesn`t exists!");
                Environment.Exit(0);
                return "error";
            }
        }

        // gibt den Pfad des Produktions Ordners zurueck
        public string getProductionPath() {
            // existiert der Ordner ?
            if (Directory.Exists(this.configs["prodPath"])) {
                return this.configs["prodPath"];
            }
            else {
                // logging / Ausgabe / exit
                this.log.addLog(string.Format("Error: Production directory {0} doesn't exists!", this.configs["prodPath"]));
                Console.WriteLine("Error: Production directory {0} doesn't exists!", this.configs["prodPath"]);
                Environment.Exit(0);
                return "error";
            }
        }

        // gibt Pfad zurueck der XSLT Dateien enthaelt
        public string getXSLTPath() {
            // existiert der Ordner ?
            if (Directory.Exists(this.configs["xsltDir"])) {
                return this.configs["xsltDir"];
            }
            else {
                // logging / Ausgabe / exit
                this.log.addLog(string.Format("Error: XSLT directory {0} doesn't exists!", this.configs["prodPath"]));
                Console.WriteLine("Error: XSLT directory {0} doesn't exists!", this.configs["prodPath"]);
                Environment.Exit(0);
                return "error";
            }
        }

        // gibt den Pfad fuer Backups zurueck
        public string getBackupPath() {
            // ist das Backup Verzeichnis vorhanden 
            if (!Directory.Exists(this.configs["backupDir"])) {
                // erstelle das Verzeichniss
                this.log.addLog(string.Format("Create backup directory {0}!", this.configs["backupDir"]));
                Directory.CreateDirectory(this.configs["backupDir"]);
            }
            return this.configs["backupDir"];
        }

        // gibt Backup Flag zurueck 
        public bool isBackup() {
            return bool.Parse(this.configs["backup"]);
        }

        // gibt Delete Flag zurueck
        public bool isDeleteXMLFile() {
            return bool.Parse(this.configs["delXMLFile"]);
        }

        // gibt Pfad zum Log Directory zurueck
        public string getLogDir() {
            // ist das Backup Verzeichnis vorhanden 
            if (!Directory.Exists(this.configs["logDir"])) {
                // erstelle das Verzeichniss
                this.log.addLog(string.Format("Create log directory {0}!", this.configs["logDir"]));
                Directory.CreateDirectory(this.configs["logDir"]);
            }
            return this.configs["logDir"];
        }
        
        // gibt Log Flag zurueck
        public bool isLog() {
            return bool.Parse(this.configs["log"]);
        }

        // gibt zurueck ob Printer Flag gesetzt ist
        public bool isCopy2Printer() {
            return bool.Parse(this.configs["copy2printer"]);
        }
    }
}