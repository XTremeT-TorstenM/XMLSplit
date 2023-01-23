using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using System.Xml;
using System.Xml.Linq;

using XMLSplit.CSV;
using XMLSplit.Configuration;
using System.Xml.Xsl;

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

        // Funktion, die ein XMLFile mit dem in CSVEntry enthaltenen xslt Dateien splittet
        // Dazu wird der .NET interne XSLT Prozessor verwendet
        public void split() {
            // check ob File noch vorhanden und Datenstrom beinhaltet
            if ((File.Exists(this.xmlFile) && !this.isEmpty())) {
                // check ob Target Verzeichnis noch erstellt werden muss
                if (!Directory.Exists(this.csventry.getTarget())) {
                    Directory.CreateDirectory(this.csventry.getTarget());
                }
                // einzeln splitten
                this.splitmitSKZ();
                this.splitohneSKZ();
            }
        }

        // split mit SKZ
        public void splitmitSKZ() {
            string mitfn = this.xmlFileName.Replace(".xml", ".MITSKZ.xml");
            string mittargetfn = Path.Combine(this.csventry.getTarget(), mitfn);
            string mitxslt = Path.Combine(this.config.getXSLTPath(), this.csventry.getMitSKZ());
            if (checkSplitFiles(mittargetfn, mitxslt)) {
                // Sonderfall im mitSKZ Split der eine einfache Kopie ist
                if (!mitxslt.EndsWith("komplett.xslt")) {
                    // split
                    Console.WriteLine("Split: {0} mit {1}", this.xmlFile, mitxslt);
                    Console.WriteLine("Output: {0}", mittargetfn);

                }
                else {
                    // Kopie Zweig
                    Console.WriteLine("Kopie {0} to {1}", this.xmlFile, mittargetfn);
                }
            }
            else {
                // log / Ausgabe / skip ?
                Console.WriteLine("Error: Xslt File {0} missing!", mitxslt);
            }
        }

        // split ohne SKZ
        public void splitohneSKZ() {
            string ohnefn = this.xmlFileName.Replace(".xml", ".OHNESKZ.xml");
            string ohnetargetfn = Path.Combine(this.csventry.getTarget(), ohnefn);
            string ohnexslt = Path.Combine(this.config.getXSLTPath(), this.csventry.getOhneSKZ());
            if (checkSplitFiles(ohnetargetfn, ohnexslt)) {
                // Sonderfall im ohneSKZ Split der kein Split durchfuehrt und Outputfile ignoriert
                if (!ohnexslt.EndsWith("leer.xslt")){
                    // split
                    Console.WriteLine("Split: {0} mit {1}", this.xmlFile, ohnexslt);
                    Console.WriteLine("Output: {0}", ohnetargetfn);

                }
                else {
                    // Nichts Zweig
                    // log / Ausgabe / skip ?
                    Console.WriteLine("Msg: Skip Outputfile {0} because xlst = leer.xslt!", ohnetargetfn);
                }
            }
            else {
                // log / Ausgabe / skip ?
                Console.WriteLine("Error: Xslt File {0} missing!", ohnexslt);
            }
        }

        public static bool checkSplitFiles(string f, string x) {
            // check ob Outputfile schon vorhanden ist.
            if (File.Exists(f)) {
                // log / Ausgabe / skip ?
                Console.WriteLine("Error: Outputfile {0} allready exists! Override!", f);
                // file loeschen
                File.Delete(f);
            }
            // check ob xslt file vorhanden ist
            if (File.Exists(x)) {
                return true;
            }
            return false;
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

        // public static void splitit(string xmlfile, string xsltfile, string xmloutfile, string datenstrom) {
        //     // lade ein XDocument mit dem XML
        //     XDocument xmlTree = XDocument.Load(xmlfile);
        //     XDocument newTree = new XDocument(new XDeclaration("1.0", "iso-8859-1", "yes"));

        //     using (XmlWriter writer = newTree.CreateWriter())
        //     {
        //         // xslcompiledtransform mit xslt laden
        //         XslCompiledTransform xslt = new XslCompiledTransform();
        //         xslt.Load(xsltfile);
        //         // in newTree transformieren
        //         xslt.Transform(xmlTree.CreateReader(), writer);
        //     }
        //     // als xml speichern
        //     newTree.Save(xmloutfile);
        // }

        // Backup + Delete jedes XML Files im Destructor
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