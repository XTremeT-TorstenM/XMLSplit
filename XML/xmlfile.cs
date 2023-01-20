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
        private string xmlFile, xmlFilePath = "", xmlFileName, xmlBackupFile = "";
        private Config config;
        private CSVEntry csventry;

        // Konstruktor mit uebergebenen XML File, seinem CSVEntry und der globalen Config 
        public XMLFile(string xmlFile, CSVEntry csventry, Config config) {
            this.config = config;
            this.csventry = csventry;
            this.xmlFile = xmlFile;
            this.xmlFileName = Path.GetFileName(xmlFile);
            this.xmlFilePath = Path.GetDirectoryName(xmlFile);
        }
        public string getXMLFilePath() {
            return this.xmlFilePath;
        }

        public string getXMLFileName() {
            return this.xmlFileName;
        }

        // Macht eine Sicherungskopie des akutellen XML Files
        public void backupXMLFile() {
            // Dateinamen fuer Backupfile aus XML Dateinamen generieren
            this.xmlBackupFile = this.config.getBackupPath() + "\\" + this.getXMLFileName().Replace(".xml", ".BACKUP.xml");
            // wenn das Backup schon existiert ?
            if (!File.Exists(this.xmlBackupFile)) {
                // file copy dest -> target
                File.Copy(this.xmlFile, this.config.getBackupPath() + "\\" + this.getXMLFileName().Replace(".xml", ".BACKUP.xml"));
            }
            else {
                // logging / Ausgabe 
                Console.WriteLine("Error: Backupfile {0} allready exists !", this.xmlBackupFile);
            }
        }

        public bool isEmpty() {
            // lade XML File in ein XDocument
            XDocument xmlTree = XDocument.Load(this.xmlFile);
            // Test ob in Datenstrom angegebenes Element mindestens einmal existiert
            // Helfer Variable 
            var i = 0;
            try {
                // erstelle ein IEnumerable auf alle Elemente die Datenstrom enthalten
                IEnumerable<XElement> testofdatenstrom = xmlTree.Descendants(this.csventry.getDatenstrom());
                // wenn sich das IEnumerable iterieren laesst sind Elemente vorhandan
                foreach (XElement _ in testofdatenstrom) {
                    i++;
                    if (i > 0) { break; }
                }
            }
            // Exception bei evtl falsch deklariertem Datenstrom
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }
            // return ob leer oder nicht
            return (i > 0) ? false: true;
        }

        // Funktion, die ein XMLFile mit den in CSVEntry enthaltenen xslt Dateien splittet
        // Dazu wird der .NET interne XSLT Prozessor verwendet
        public void split() {
            // check ob File noch vorhanden und nicht leer ist was Datenstrom angeht
            if ((File.Exists(this.xmlFile) && !this.isEmpty())) {
                // check ob Target Verzeichnis noch erstellt werden muss
                if (!Directory.Exists(this.csventry.getTarget())) {
                    Directory.CreateDirectory(this.csventry.getTarget());
                }
                string mitfn = this.xmlFileName.Replace(".xml", ".MITSKZ.xml");
                string mittfn = Path.Combine(this.csventry.getTarget(), mitfn);
                string mitxslt = Path.Combine(this.config.getXSLTPath(), this.csventry.getMitSKZ());
                string ohnefn = this.xmlFileName.Replace(".xml", ".OHNESKZ.xml");
                string ohnetfn = Path.Combine(this.csventry.getTarget(), ohnefn);
                string ohnexslt = Path.Combine(this.config.getXSLTPath(), this.csventry.getOhneSKZ());
                // check fuer Output Files
                if (File.Exists(mittfn) || File.Exists(ohnetfn)) {
                    // logging / Ausgabe / skip ?
                    Console.WriteLine("Error: Outputfiles allready exists !");
                    // check fuer xslt Files
                    if (!File.Exists(mitxslt) || !File.Exists(ohnexslt)) {
                        // logging /Ausgabe / skip ?
                        Console.WriteLine("Error: XSLT file error !");
                    }
                }
                Console.WriteLine("{0} --- {1}", mittfn, mitxslt);
                Console.WriteLine("{0} --- {1}", ohnetfn, ohnexslt);
            }
        }

        // Override der ToString fuer Print
        public override string ToString()
        {
            return "File: " + this.getXMLFileName() + "\t" + "Path: " + this.getXMLFilePath() + "\n" + this.csventry;
        }

        // public void splitFiles() {
        //     // xslt directory 
        //     var cwd = Directory.GetCurrentDirectory();
        //     cwd = cwd + @"\XSLT";
        //     // temporaeres xslt dir
        //     cwd = @"C:\Temp\Testumgebung\Test\XSLT";
        //     // fuer jedes file mit seinen csv daten ...
        //     foreach(var kvp in this.xmlList) {
        //         // check ob zu splittendes file existiert
        //         if (File.Exists(kvp.Key)) {
        //             // wenn ziel ordner nicht existiert -> erstellen
        //             if (!Directory.Exists(kvp.Value.getTarget())) {
        //                 Directory.CreateDirectory(kvp.Value.getTarget());
        //             }
        //             // dateinamen fuer mitSKZ generieren / xslt pfad + datei aus csv data
        //             string mitfilename = Path.GetFileName(kvp.Key).Replace(".xml", ".MITSKZ.xml");
        //             string targetfile = Path.Combine(kvp.Value.getTarget(), mitfilename);
        //             string xsltfile = Path.Combine(cwd, kvp.Value.getMitSKZ());
        //             // falls file schon vorhanden -> loeschen
        //             if (File.Exists(targetfile)) {
        //                 File.Delete(targetfile);
        //             }
        //             // wenn die Transformation mit komplett.xslt staffindet -> direkte Kopie
        //             if (kvp.Value.getMitSKZ().EndsWith("komplett.xslt")) {
        //                 File.Copy(kvp.Key, targetfile);
        //             }
        //             else {
        //                 split(kvp.Key, xsltfile, targetfile, kvp.Value.getDatenstrom());
        //             }
        //             // dateinamen fuer ohneSKZ generieren / xslt pfad + datei aus csv data
        //             string ohnefilename = Path.GetFileName(kvp.Key).Replace(".xml", ".OHNESKZ.xml");
        //             targetfile = Path.Combine(kvp.Value.getTarget(), ohnefilename);
        //             xsltfile = Path.Combine(cwd, kvp.Value.getOhneSKZ());
        //             // falls file schon vorhanden -> loeschen
        //             if (File.Exists(targetfile)) {
        //                 File.Delete(targetfile);
        //             }
        //             // wenn die Transformation mit leer.xslt staffindet -> nichts machen
        //             if (kvp.Value.getOhneSKZ().EndsWith("leer.xslt")) {
        //             }
        //             else {
        //                 split(kvp.Key, xsltfile, targetfile, kvp.Value.getDatenstrom());
        //             }
        //         }
        //     }
        // }

        // public static void split(string xmlfile, string xsltfile, string xmloutfile, string datenstrom) {
        //     // lade ein XDocument mit dem XML
        //     XDocument xmlTree = XDocument.Load(xmlfile);
        //     // Test ob in Datenstrom angegebenes Element mindestens einmal existiert
        //     var i = 0;
        //     try {
        //         IEnumerable<XElement> testofdatenstrom = xmlTree.Descendants(datenstrom);
        //         foreach (XElement _ in testofdatenstrom) {
        //             i++;
        //             if (i > 0) { break; }
        //         }
        //     }
        //     catch (Exception e) {
        //         Console.WriteLine(e.Message);
        //     }
        //     // Wenn Datenstrom existiert fuehre Split durch
        //     if (i > 0) { 
        //         // neues XDocument fuer gesplittetes xml
        //         XDocument newTree = new XDocument(new XDeclaration("1.0", "iso-8859-1", "yes"));

        //         using (XmlWriter writer = newTree.CreateWriter())
        //         {
        //             // xslcompiledtransform mit xslt laden
        //             XslCompiledTransform xslt = new XslCompiledTransform();
        //             xslt.Load(xsltfile);
        //             // in newTree transformieren
        //             xslt.Transform(xmlTree.CreateReader(), writer);
        //         }
        //         // als xml speichern
        //         newTree.Save(xmloutfile);
        //     }
        // }

        // Backup jedes XML Files im Destructor + loeschen
        ~XMLFile() {
            // wenn XMLFile vorhanden
            if (File.Exists(this.xmlFile)) {
                // Backup Flag aus Config
                if (this.config.isBackup())
                {
                    // Backup
                    this.backupXMLFile();
                }
                // Delete Flag aus Config 
                if (this.config.isDeleteXMLFile()) {
                    // XMLFile loeschen
                    File.Delete(this.xmlFile);
                }
            }
        }
    }
}