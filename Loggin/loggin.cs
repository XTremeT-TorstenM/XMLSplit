using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace XMLSplit.Logging {
    // Log Class die saemtliche Aktionen loggt
    public class Log {
        private List<string> log;
        private string logFile, logDir;
        private bool islog;
        public Log() {
            this.log = new List<string>();
            this.logDir = "";
            this.logFile = DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".log";
            this.islog = true;
            this.addLog(String.Format("Logfile: {0}", this.logFile));
        }

        // setzt das aktuelle Log Verzeichnis
        public void setLogDir(string logDir) {
            this.logDir = logDir;
            this.addLog(string.Format("\nSet logging Directory to: {0}", this.logDir));
        }

        // setzt Log Flag
        public void setLog(bool log) {
            this.islog = log;
        }

        // fuegt einen Eintrag ins log hinzu
        public void addLog(string message) {
            this.log.Add(message);
        }

        // Ausgabe Log in der Konsole
        public void writeLog() {
            foreach(var msg in this.log) {
                Console.WriteLine(msg);
            }
        }
        
        // Ausgabe Log in Logfile
        public void saveLog() {
            if (this.islog) {
                using (StreamWriter logtext = new StreamWriter(Path.Combine(this.logDir, this.logFile))) {
                    foreach (string msg in this.log) {
                        logtext.WriteLine(msg);
                    }
                }
            }
        }
    }
}