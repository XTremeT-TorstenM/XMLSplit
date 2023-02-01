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
            // ToDo! Aenderung der Angabe der Konfigurationsdatei (Argument etc.)
            this.log.addLog("\n# read configuration file \'settings.conf\'", true);
            configs = new Dictionary<string, string>();
            // wenn settings.conf existiert
            if (File.Exists("settings.conf")) {
                // lese jede Zeile aus Config File 
                foreach (string line in File.ReadLines("settings.conf")) {
                    // skip Kommentare
                    if (line.StartsWith("//")) {
                        continue;
                    }
                    // split am = Zeichen
                    var val = line.Split('=');
                    // Key des Dict ist Wert vor dem '='
                    string key = val[0].Trim();
                    // Value des Dict ist Wert nach dem '='
                    string value = val[1].Trim();
                    // Eintrag zum Dictionary hinzufuegen
                    configs.Add(key, value);
                }
                this.log.addLog("Configuration:");
                // fuer jedes Key Value Paar in dem Dictionary erstelle Eintrag im Log
                foreach (var kvp in this.configs) {
                    this.log.addLog(string.Format("{0} = {1}", kvp.Key, kvp.Value));
                }
                // Log Directory setzen
                this.log.setLogDir(this.getLogDir());
                // Log Flag setzen
                this.log.setLog(this.isLog());
            }
            else {
                // Fehlerausgabe und Exit
                Console.WriteLine("ERROR: No 'settings.conf' found! Exit");
                Environment.Exit(0);
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
                this.log.addLog(string.Format("\nError: CSV file \'{0}\' doesn`t exists!", this.configs["cssvFile"]));
                Console.WriteLine("\nError: CSV file \'{0}\' doesn`t exists!", this.configs["cssvFile"]);
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
                this.log.addLog(string.Format("Error: Production directory \'{0}\' doesn't exists!", this.configs["prodPath"]));
                Console.WriteLine("Error: Production directory \'{0}\' doesn't exists!", this.configs["prodPath"]);
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
                this.log.addLog(string.Format("Error: XSLT directory \'{0}\' doesn't exists!", this.configs["prodPath"]));
                Console.WriteLine("Error: XSLT directory \'{0}\' doesn't exists!", this.configs["prodPath"]);
                Environment.Exit(0);
                return "error";
            }
        }

        // gibt den Pfad fuer Backups zurueck
        public string getBackupPath(string diff, string Mandant) {
            // ist das Backup Verzeichnis vorhanden 
            string backupDir = this.configs[diff] + "\\" + Mandant;
            if (!Directory.Exists(backupDir)) {
                // erstelle das Verzeichniss
                this.log.addLog(string.Format("\nCreate backup directory \'{0}\'!", backupDir));
                Directory.CreateDirectory(backupDir);
            }
            return backupDir;
        }

        // gibt Backup Flag zurueck 
        public bool isBackup() {
            return bool.Parse(this.configs["backup"]);
        }

        // gibt den Pfad zum Backup Dir fuer gesplittete XML zurueck
        public string getBackupSplitPath() {
            // ist das Backup Verzeichnis vorhanden 
            if (!Directory.Exists(this.configs["backupSplitDir"])) {
                // erstelle das Verzeichniss
                this.log.addLog(string.Format("Create backup directory \'{0}\' for split files!", this.configs["backupDir"]));
                Directory.CreateDirectory(this.configs["backupSplitDir"]);
            }
            return this.configs["backupDir"];
        }

        // gibt Delete Flag zurueck
        public bool isDeleteXMLFile() {
            return bool.Parse(this.configs["delXMLFile"]);
        }

        // gibt Delete Split XML Flag zurueck
        public bool isDeleteSplitXMLFile() {
            return bool.Parse(this.configs["delSplitXMLFile"]);
        }

        // gibt Pfad zum Log Directory zurueck
        public string getLogDir() {
            // ist das Backup Verzeichnis vorhanden 
            if (!Directory.Exists(this.configs["logDir"])) {
                // erstelle das Verzeichniss
                this.log.addLog(string.Format("Create log directory \'{0}\'!", this.configs["logDir"]));
                Directory.CreateDirectory(this.configs["logDir"]);
            }
            return this.configs["logDir"];
        }
        
        // gibt Log Flag zurueck
        public bool isLog() {
            return bool.Parse(this.configs["log"]);
        }

        // gibt Printer Flag zurueck
        public bool isCopy2Printer() {
            return bool.Parse(this.configs["copy2printer"]);
        }
    }
}