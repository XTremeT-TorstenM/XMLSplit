using System;
using System.IO;

using XMLSplit.Configuration;

namespace XMLSplit.XML {
    public class XMLFile {
        private string xmlFile, xmlFilePath, xmlFileName, xmlBackupFile;
        private Config config;
        public XMLFile(string xmlFile, Config config) {
            this.config = config;
            this.xmlFile = xmlFile;
            int idx = xmlFile.LastIndexOf('\\');
            this.xmlFilePath = xmlFile.Substring(0, idx);
            this.xmlFileName = xmlFile.Substring(idx + 1);
        }
        public string getXMLFilePath() {
            return this.xmlFilePath;
        }

        public string getXMLFileName() {
            return this.xmlFileName;
        }

        public void backupXMLFile() {
            this.xmlBackupFile = this.config.getBackupPath() + "\\" + this.getXMLFileName().Replace(".xml", ".BACKUP.xml");
            if (!File.Exists(this.xmlBackupFile)) {
                File.Copy(this.xmlFile, this.config.getBackupPath() + "\\" + this.getXMLFileName().Replace(".xml", ".BACKUP.xml"));
            }
            else {
                // logging / Ausgabe 
                Console.WriteLine("Error: Backupfile {0} allready exists !", this.xmlBackupFile);
            }
        }

        public override string ToString()
        {
            return this.getXMLFilePath() + " --- " + this.getXMLFileName();
        }

        ~XMLFile() {
            // Backup jedes XML Files im Destructor
            if (this.config.isBackup()) {
                this.backupXMLFile();
            }
        }
    }
}