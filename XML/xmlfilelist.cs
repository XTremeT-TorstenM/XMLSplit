using System;
using System.IO;
using System.Collections.Generic;

using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;

using XMLSplit.CSV;
using XMLSplit.Configuration;
using XMLSplit.Logging;

namespace XMLSplit.XML {
    // XMLFileList Class die eine Liste von XML Files enthaelt
    public class XMLFilelist {
        private List<XMLFile> xmlFileList;
        // Pfad zum zu ueberpruefenden Verzeichniss
        private string xmlProdPath;
        // globale Config
        private Config config;
        // CSV Daten fuer Zuweisung zu jedem gefunden XML File
        private CSVData csvData;
        private Log log;

        // Konstruktor mit Parametern
        public XMLFilelist(CSVData csvdata, Config config, Log log) {
            this.log = log;
            this.config = config;
            this.csvData = csvdata;
            this.xmlProdPath = this.config.getProductionPath();

            xmlFileList = new List<XMLFile>();
            this.log.addLog("# Create new XML file list!", true);
        }
        
        // Funktion, die den Produktions Ordner nach allen Dateien (auch Unterverzeichnisse) scant
        // diese dann mit den CSV Daten abgleicht und ein zugehoeriges XMLFile Objekt erzeugt
        // und diese in einer Liste speichert
        public void getFileList() {
            // scan Dir
            string [] tmpfileList = Directory.GetFiles(this.xmlProdPath, "*", SearchOption.AllDirectories);
            // fuer jedes File in Liste
            foreach(string file in tmpfileList) {
                // iteriere ueber jeden CSV Eintrag aus csvData
                foreach(CSVEntry csventry in this.csvData.getList()) {
                    // Wenn Pfad vom File mit Pfad vom SOURCE Eintrag uebereinstimmt sowie der Filename mit der Wildcard matched
                    if ((Path.GetDirectoryName(file) == csventry.getSOURCEPath()) && (Path.GetFileName(file).Glob(csventry.getSOURCEFile()))) {
                        // log Match
                        this.log.addLog(string.Format("Found match: {0}\n\t{1}", file, csventry));
                        // erzeuge temporaeres XML File
                        XMLFile tmpXMLFile = new XMLFile(file, csventry, this.config, this.log);
                        // fuege temporaeres XML File der Liste hinzu
                        this.xmlFileList.Add(tmpXMLFile);
                    }
                }
            }
            if (this.xmlFileList.Count == 0) {
                this.log.addLog("No XML files found!!!");
            }
        }

        // Funktion, die ueber alle XML Files iteriert und verarbeitet 
        public void processAll() {
            foreach(XMLFile file in this.xmlFileList) {
                if (!file.isEmpty()) {
                    file.logXML();
                    file.transform();
                    file.backup();
                    file.copy2Printer();
                    file.delete();
                }
                else {
                    this.log.addLog(string.Format("XML file \'{0}\' has no Datenstrom \'{1}\'. Skip transformation!", file.getXMLFileName() ,file.getDatenstrom()), true);
                    Console.WriteLine("No Transformation needed!");
                    file.backup(false);
                    file.delete();
                }
            }
        }
    }

    public static class HelperClass {
        // Hilfsfunktion um WildCard Matching mit Filename zu machen (Globbing)
        public static bool Glob(this string value, string pattern) {
            int pos = 0;
            while (pattern.Length != pos) {
                switch (pattern[pos]) {
                    case '?':
                        break;
                    case '*':
                        for (int i = value.Length; i >= pos; i--) {
                            if (Glob(value.Substring(i), pattern.Substring(pos + 1))) {
                                return true;
                            }
                        }
                        return false;

                    default:
                        if (value.Length == pos || char.ToUpper(pattern[pos]) != char.ToUpper(value[pos])) {
                            return false;
                        }
                        break;
                }
                pos++;
            }
            return value.Length == pos;
        }
    }
}