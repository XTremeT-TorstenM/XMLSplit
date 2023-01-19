using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using System.Xml;
using System.Xml.Linq;

using XMLSplit.CSV;
using XMLSplit.Configuration;

namespace XMLSplit.XML {
    public class XMLFile {
        private string xmlFile, xmlFilePath, xmlFileName, xmlBackupFile;
        private Config config;
        private CSVEntry csventry;
        public XMLFile(string xmlFile, CSVEntry csventry, Config config) {
            this.config = config;
            this.csventry = csventry;
            this.xmlFile = xmlFile;
            this.xmlBackupFile = "";
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

        public bool isEmpty() {
            XDocument xmlTree = XDocument.Load(this.xmlFile);
            // Test ob in Datenstrom angegebenes Element mindestens einmal existiert
            var i = 0;
            try {
                IEnumerable<XElement> testofdatenstrom = xmlTree.Descendants(this.csventry.getDatenstrom());
                foreach (XElement _ in testofdatenstrom) {
                    i++;
                    if (i > 0) { break; }
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }
            return (i > 0) ? false: true;
        }

        public override string ToString()
        {
            return "File: " + this.getXMLFileName() + "\t" + "Path: " + this.getXMLFilePath() + "\n" + this.csventry;
        }

        ~XMLFile() {
            // Backup jedes XML Files im Destructor
            if (this.config.isBackup()) {
                this.backupXMLFile();
            }
        }
    }
}