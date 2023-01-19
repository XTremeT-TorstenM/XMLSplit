using System;
using System.IO;
using System.Collections.Generic;
// using CSVDataClass;
// using CSVEntryClass;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;

using XMLSplit.XML;
using XMLSplit.CSV;

namespace XMLSplit.XML {
    public class XMLFilelist {
        // private List<Dictionary<string, CSVEntry>> xmlList;
        private Dictionary<string, CSVEntry> xmlList;

        public XMLFilelist(string xmlfilePath) {
            xmlList = new Dictionary<string, CSVEntry>();
        }
        
        public void getXMLFilelist(CSVData csvData) {
            foreach(CSVEntry entry in csvData.getList()) {
                if (Directory.Exists(entry.getSOURCEPath())) {
                    string[] files = Directory.GetFiles(entry.getSOURCEPath(), entry.getSOURCEFile());
                    if (files.Length > 0) {
                        foreach(string file in files) {
                            // var dicent = new Dictionary<string, CSVEntry>();
                            // dicent.Add(file, entry);
                            // this.xmlList.Add(dicent);
                            this.xmlList.Add(file, entry);
                        }
                    }
                }
            }
        }
        
        public void showFilelist() {
            foreach(var kvp in this.xmlList) {
                Console.WriteLine("Key: {0} Value: {1}", kvp.Key, kvp.Value);
            }
        }

        public bool backupFiles(string backupDir = @"C:\Temp\Testumgebung\Test_Backup") {
            if (!Directory.Exists(backupDir)) {
                Directory.CreateDirectory(backupDir);
            }
            foreach(var kvp in this.xmlList) {
                if (File.Exists(kvp.Key)) {
                    string backupfilename = Path.GetFileName(kvp.Key).Replace(".xml", ".Backup.xml");
                    string targetfile = Path.Combine(backupDir, backupfilename);
                    if (File.Exists(targetfile)) {
                        File.Delete(targetfile);
                    }
                    File.Copy(kvp.Key, targetfile);
                }
            }
            return true;
        }

        public void splitFiles() {
            // xslt directory 
            var cwd = Directory.GetCurrentDirectory();
            cwd = cwd + @"\XSLT";
            // temporaeres xslt dir
            cwd = @"C:\Temp\Testumgebung\Test\XSLT";
            // fuer jedes file mit seinen csv daten ...
            foreach(var kvp in this.xmlList) {
                // check ob zu splittendes file existiert
                if (File.Exists(kvp.Key)) {
                    // wenn ziel ordner nicht existiert -> erstellen
                    if (!Directory.Exists(kvp.Value.getTarget())) {
                        Directory.CreateDirectory(kvp.Value.getTarget());
                    }
                    // dateinamen fuer mitSKZ generieren / xslt pfad + datei aus csv data
                    string mitfilename = Path.GetFileName(kvp.Key).Replace(".xml", ".MITSKZ.xml");
                    string targetfile = Path.Combine(kvp.Value.getTarget(), mitfilename);
                    string xsltfile = Path.Combine(cwd, kvp.Value.getMitSKZ());
                    // falls file schon vorhanden -> loeschen
                    if (File.Exists(targetfile)) {
                        File.Delete(targetfile);
                    }
                    // wenn die Transformation mit komplett.xslt staffindet -> direkte Kopie
                    if (kvp.Value.getMitSKZ().EndsWith("komplett.xslt")) {
                        File.Copy(kvp.Key, targetfile);
                    }
                    else {
                        split(kvp.Key, xsltfile, targetfile, kvp.Value.getDatenstrom());
                    }
                    // dateinamen fuer ohneSKZ generieren / xslt pfad + datei aus csv data
                    string ohnefilename = Path.GetFileName(kvp.Key).Replace(".xml", ".OHNESKZ.xml");
                    targetfile = Path.Combine(kvp.Value.getTarget(), ohnefilename);
                    xsltfile = Path.Combine(cwd, kvp.Value.getOhneSKZ());
                    // falls file schon vorhanden -> loeschen
                    if (File.Exists(targetfile)) {
                        File.Delete(targetfile);
                    }
                    // wenn die Transformation mit leer.xslt staffindet -> nichts machen
                    if (kvp.Value.getOhneSKZ().EndsWith("leer.xslt")) {
                    }
                    else {
                        split(kvp.Key, xsltfile, targetfile, kvp.Value.getDatenstrom());
                    }
                }
            }
        }

        public static void split(string xmlfile, string xsltfile, string xmloutfile, string datenstrom) {
            // lade ein XDocument mit dem XML
            XDocument xmlTree = XDocument.Load(xmlfile);
            // Test ob in Datenstrom angegebenes Element mindestens einmal existiert
            var i = 0;
            try {
                IEnumerable<XElement> testofdatenstrom = xmlTree.Descendants(datenstrom);
                foreach (XElement _ in testofdatenstrom) {
                    i++;
                    if (i > 0) { break; }
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }
            // Wenn Datenstrom existiert fuehre Split durch
            if (i > 0) { 
                // neues XDocument fuer gesplittetes xml
                XDocument newTree = new XDocument(new XDeclaration("1.0", "iso-8859-1", "yes"));

                using (XmlWriter writer = newTree.CreateWriter())
                {
                    // xslcompiledtransform mit xslt laden
                    XslCompiledTransform xslt = new XslCompiledTransform();
                    xslt.Load(xsltfile);
                    // in newTree transformieren
                    xslt.Transform(xmlTree.CreateReader(), writer);
                }
                // als xml speichern
                newTree.Save(xmloutfile);
            }
        }
    }
}