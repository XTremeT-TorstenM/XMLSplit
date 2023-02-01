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
            // ToDo! Aenderung logfile name
            this.logFile = DateTime.Now.ToString("yyyy-dd-MM--HH-mm-ss") + ".log";
            this.islog = true;
            this.addLog(String.Format("Logfile: {0}", this.logFile));
        }

        // setzt das aktuelle Log Verzeichnis
        public void setLogDir(string logDir) {
            this.logDir = logDir;
            this.addLog(string.Format("\n# Set log dir: \'{0}\'", this.logDir), true);
        }

        // setzt Log Flag
        public void setLog(bool log) {
            this.islog = log;
        }

        // fuegt einen Eintrag ins log hinzu
        public void addLog(string message, bool logwithtime = false) {
            string logTime = DateTime.Now.ToString("yyyy-dd-MM--HH-mm-ss");
            if (logwithtime) {
                message = "\n# " + logTime.Trim() + "\n" + message.Trim();
            }
            this.log.Add(message);
        }

        // Ausgabe Log in Logfile
        public void saveLog() {
            // wenn log flag gesetzt ist
            if (this.islog) {
                using (StreamWriter logtext = new StreamWriter(Path.Combine(this.logDir, this.logFile))) {
                    // zeilenweise ausgabe in datei
                    foreach (string msg in this.log) {
                        logtext.WriteLine(msg);
                    }
                }
            }
        }
    }
}