using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using System.Xml;
using System.Xml.Xsl;
using System.Xml.Linq;

using XMLSplit.CSV;
using XMLSplit.Configuration;
using XMLSplit.Logging;

namespace XMLSplit.XML {
    // Klasse die ein XML File repraesentiert
    // Zu diesem gehoert neben dem Dateinamen noch das zugehoerige CSV Entry welches vorher ermittelt wird
    public class XMLFile {
        // xmlFile = xmlFilePath + xmlFileName
        private string xmlFile, xmlFilePath = "", xmlFileName;
        private string mitSKZfn, mitxslt, ohneSKZfn, ohnexslt;
        private Config config;
        private CSVEntry csventry;
        private Log log;
        private XDocument xmlTree;

        // Konstruktor mit uebergebenen XML File, seinem CSVEntry und der globalen Config 
        public XMLFile(string xmlFile, CSVEntry csventry, Config config, Log log) {
            this.log = log;
            this.config = config;
            this.csventry = csventry;
            this.xmlFile = xmlFile;
            this.xmlTree = new XDocument();
            this.xmlFileName = Path.GetFileName(xmlFile);
            // ToDo! Abfangen / Ueberpruefen des Null Verweises
            this.xmlFilePath = Path.GetDirectoryName(xmlFile);
            // Dateinamen fuer Splitting generieren und XSLT zuweisen
            this.mitSKZfn = Path.Combine(this.csventry.getTarget(), this.getXMLmitSKZFileName());
            this.mitxslt = Path.Combine(this.config.getXSLTPath(), this.csventry.getMitSKZ());
            this.ohneSKZfn = Path.Combine(this.csventry.getTarget(), this.getXMLohneSKZFileName());
            this.ohnexslt = Path.Combine(this.config.getXSLTPath(), this.csventry.getOhneSKZ());

            // XDocument nur einmal laden (bei einer 700Mb XML belegt dieses Objekt 1700Mb im Speicher)
            if (File.Exists(this.xmlFile)) {
                this.xmlTree = XDocument.Load(this.xmlFile);
            }
        }

        // gibt Datei Pfad zurueck
        public string getXMLFilePath() {
            return this.xmlFilePath;
        }

        // gibt Dateinamen zurueck
        public string getXMLFileName() {
            return this.xmlFileName;
        }

        // generiert MITSKZ Dateinamen
        public string getXMLmitSKZFileName() {
            return this.xmlFileName.Replace(".xml", ".MITSKZ.xml");
        }

        // generiert OHNESKZ Dateinamen
        public string getXMLohneSKZFileName() {
            return this.xmlFileName.Replace(".xml", ".OHNESKZ.xml");
        }

        // logt Prozessstart jeder XML Datei
        public void logXML() {
            this.log.addLog(string.Format("# Process: {0}", this.xmlFileName), true);
        }

        // Gibt den String Datenstrom aus der CSV Entry zurueck
        public string getDatenstrom() {
            return this.csventry.getDatenstrom();
        }

        // Macht eine Sicherungskopie des akutellen XML Files
        // Bei backupSplits wird auch eine Sicherung der gesplitteten XML durchgefuehrt
        public void backup(bool backupSplits = true) {
            // Backup Flag true ?
            if (this.config.isBackup()) {
                // Dateinamen fuer Backupfile aus XML Dateinamen generieren
                string xmlBackupFile = Path.Combine(this.config.getBackupPath("backupDir", this.csventry.getMandant()), this.getXMLFileName().Replace(".xml", ".BACKUP.xml"));
                this.backupFile(this.xmlFile, xmlBackupFile);
                // Backup der Split XMLs
                if (backupSplits) {
                    string xmlmitSKZBackupFile = Path.Combine(this.config.getBackupPath("backupSplitDir", this.csventry.getMandant()), this.getXMLmitSKZFileName().Replace(".xml", ".BACKUP.xml"));
                    this.backupFile(this.mitSKZfn, xmlmitSKZBackupFile);
                    // kein Backup bei OHNESKZ = leer.xslt da hier ein Splitting verworfen wird
                    if (!this.ohnexslt.EndsWith("leer.xslt")) {
                        string xmlohneSKZBackupFile = Path.Combine(this.config.getBackupPath("backupSplitDir", this.csventry.getMandant()), this.getXMLohneSKZFileName().Replace(".xml", ".BACKUP.xml"));
                        this.backupFile(this.ohneSKZfn, xmlohneSKZBackupFile);
                    }
                }
            }
        }

        // Hilfsfunktion die noch ueberprueft ob Dateien existieren
        public void backupFile(string sourcefile, string destfile) {
            // existiert sourcefile ?
            if (File.Exists(sourcefile)) {
                // wenn backup noch nicht existiert
                if (!File.Exists(destfile)) {
                    // log / file copy dest -> target
                    this.log.addLog(string.Format("Backup: \'{0}\' to \'{1}\'", sourcefile, destfile));
                    File.Copy(sourcefile, destfile);
                }
                else {
                    // logging / Ausgabe 
                    this.log.addLog(string.Format("Error: Backup: \'{0}\' allready exists! Did not backup!", destfile));
                    Console.WriteLine("Error: Backupfile \'{0}\' allready exists! Did not backup!", destfile);
                }
            }
            else {
                // logging / Ausgabe 
                this.log.addLog(string.Format("Error: Source file \'{0}\' for Backup doesn't exist!", sourcefile));
                Console.WriteLine("Error: Source file \'{0}\' for Backup doesn't exist!", sourcefile);
            }
        }

        // Loescht das XMLs je nach Flag
        public void delete() {
            // Delete flag fuer Production XML gesetzt ?
            if (this.config.isDeleteXMLFile()) {
                // XMLFile loeschen
                if (File.Exists(this.xmlFile)) {
                    this.log.addLog(string.Format("\nDelete: \'{0}\'", this.xmlFileName));
                    File.Delete(this.xmlFile);
                }
            }
            // Delete Flag fuer Split XML gesetzt ?
            if (this.config.isDeleteSplitXMLFile()) {
                // XMLFile mit SKZ loeschen
                if (File.Exists(this.mitSKZfn)) {
                    this.log.addLog(string.Format("\nDelete: \'{0}\'", this.mitSKZfn));
                    File.Delete(this.mitSKZfn);
                }

                // XMLFile ohne SKZ loeschen
                if (File.Exists(this.ohneSKZfn)) {
                        this.log.addLog(string.Format("\nDelete: \'{0}\'", this.ohneSKZfn));
                        File.Delete(this.ohneSKZfn);
                }
            }
        }

        // gibt zurueck ob XML Datenstrom beinhaltet
        public bool isEmpty() {
            // Test ob in Datenstrom angegebenes Element mindestens einmal existiert
            // Helfer Variable 
            var i = 0;
            try {
                // erstelle ein IEnumerable auf alle Elemente die Datenstrom enthalten
                IEnumerable<XElement> testofdatenstrom = this.xmlTree.Descendants(this.csventry.getDatenstrom());
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
        public void transform() {
            // check ob File noch vorhanden und Datenstrom beinhaltet
            if ((File.Exists(this.xmlFile) && !this.isEmpty())) {
                // check ob Target Verzeichnis noch erstellt werden muss
                if (!Directory.Exists(this.csventry.getTarget())) {
                    this.log.addLog(string.Format("Create Targetdirectory: {0}", this.csventry.getTarget()));
                    Directory.CreateDirectory(this.csventry.getTarget());
                }
                // Start der Transformation
                this.log.addLog("# Begin transformation", true);

                // jeweils mit und ohne SKZ splitten und Fehlerbehandlung
                string result = "Transformation succsessful !";
                try {
                    this.transform_mitSKZ();
                    this.transform_ohneSKZ();
                }
                catch (Exception e) {
                    // result mit Fehler laden
                    result = "Transformation Error: " + e.ToString() + "\nTransformation canceled!"; 
                }
                finally {
                    // log / Ausgabe
                    this.log.addLog(result);
                    Console.WriteLine("Result: {0}", result);
                    this.log.addLog("# End transformation", true);
                }
            }
            else {
                this.log.addLog(string.Format("XML file \'{0}\' does not contain any \"Datenstrom\" {1}", this.xmlFile, this.csventry.getDatenstrom()));
            }
        }

        // split mit SKZ
        public void transform_mitSKZ() {
            if (checkSplitFiles(this.mitSKZfn, this.mitxslt)) {
                // Sonderfall im mitSKZ Split der eine einfache Kopie ist
                if (!this.mitxslt.EndsWith("komplett.xslt")) {
                    // split / log / Ausgabe
                    this.log.addLog(string.Format("Transform \'{0}\' with \'{1}\'", Path.GetFileName(this.xmlFile), Path.GetFileName(this.mitxslt)));
                    this.log.addLog(string.Format("\t\t--> \'{0}\'", this.getXMLmitSKZFileName()));

                    // neues XDocument fuer Transformation
                    XDocument newTree = new XDocument(new XDeclaration("1.0", "iso-8859-1", "no"));

                    using (XmlWriter writer = newTree.CreateWriter())
                    {
                        // xslcompiledtransform mit xslt laden
                        XslCompiledTransform xslt = new XslCompiledTransform();
                        xslt.Load(this.mitxslt);
                        // vom alten xmlTree lesen und in neuen transformieren
                        xslt.Transform(this.xmlTree.CreateReader(), writer);
                    }
                    // als xml speichern
                    newTree.Save(this.mitSKZfn);

                }
                else {
                    // Kopie Zweig / log / Ausgabe
                    this.log.addLog(string.Format("Only copy from \'{0}\' to \'{1}\' needed! (komplett.xslt)", Path.GetFileName(this.xmlFile), this.mitSKZfn));
                    // Console.WriteLine("Kopie {0} to {1}", this.xmlFile, mittargetfn);
                    File.Copy(this.xmlFile, this.mitSKZfn);
                }
            }
            else {
                // log / Ausgabe / skip ?
                this.log.addLog(string.Format("Error: XSLT file \'{0}\' missing!", this.mitxslt));
                Console.WriteLine("Error: Xslt File \'{0}\' missing!", mitxslt);
            }
        }

        // split ohne SKZ
        public void transform_ohneSKZ() {
            if (checkSplitFiles(this.ohneSKZfn, this.ohnexslt)) {
                // Sonderfall im ohneSKZ Split der kein Split durchfuehrt und Outputfile ignoriert
                if (!this.ohnexslt.EndsWith("leer.xslt")){
                    // split / log / Ausgabe
                    this.log.addLog(string.Format("Transform \'{0}\' with \'{1}\'", Path.GetFileName(this.xmlFile), Path.GetFileName(this.ohnexslt)));
                    this.log.addLog(string.Format("\t\t--> \'{0}\'", this.getXMLohneSKZFileName()));

                    // neues XDocument fuer Transformation
                    XDocument newTree = new XDocument(new XDeclaration("1.0", "iso-8859-1", "no"));

                    using (XmlWriter writer = newTree.CreateWriter())
                    {
                        // xslcompiledtransform mit xslt laden
                        XslCompiledTransform xslt = new XslCompiledTransform();
                        xslt.Load(this.ohnexslt);
                        // vom alten xmlTree lesen und in neuen transformieren
                        xslt.Transform(this.xmlTree.CreateReader(), writer);
                    }
                    // als xml speichern
                    newTree.Save(this.ohneSKZfn);

                }
                else {
                    // leer.xslt Zweig
                    // log / Ausgabe / skip ?
                    this.log.addLog(string.Format("Skip outputfile \'{0}\'! (leer.xslt)", this.ohneSKZfn));
                    // Console.WriteLine("Msg: Skip Outputfile {0} because xlst = leer.xslt!", ohnetargetfn);
                }
            }
            else {
                // log / Ausgabe / skip ?
                this.log.addLog(string.Format("Error: XSLT file \'{0}\' missing!", this.ohnexslt));
                // Console.WriteLine("Error: Xslt File {0} missing!", ohnexslt);
            }
        }

        // copy zum Printer
        public void copy2Printer() {
            // check Copy2Printer Flag und ob Transformation wieder in das Production Dir schreibt (SOURCE aus CSV in Target enthalten ist) 
            if (this.config.isCopy2Printer() && !(this.csventry.getTarget().Contains(this.csventry.getSOURCEPath()))) {
                bool printerror = false;
                string result = "\n";
                try {
                    // copy mitSKZ + ohneSKZ zum Drucker
                    if (File.Exists(this.mitSKZfn)) {
                        File.Copy(this.mitSKZfn, this.csventry.getPrinter());
                        result = result + string.Format("Copy \'{0}\' to \'{1}\'", Path.GetFileName(this.mitSKZfn), this.csventry.getPrinter()) + "\n";
                    }
                    if (File.Exists(this.ohneSKZfn)) {
                        File.Copy(this.ohneSKZfn, this.csventry.getPrinter());
                        result = result + string.Format("Copy \'{0}\' to \'{1}\'", Path.GetFileName(this.ohneSKZfn), this.csventry.getPrinter()) + "\n";
                    }
                }
                catch (Exception e) {
                    result = result + "Copy to printer error: " + e.ToString();
                    printerror = true;
                }
                finally {
                    if (!printerror) {
                        result = result + "\nCopy to printer with no errors!";
                    }
                    this.log.addLog(result);
                }
            }
            else {
                this.log.addLog("Either Copy2Printer Flag disabled or transformation to production directory !!! -> No copy to printer necessary.", true);
            }
        }

        // check output File + xslt File
        public static bool checkSplitFiles(string f, string x) {
            // check ob Outputfile schon vorhanden ist.
            if (File.Exists(f)) {
                // Ausgabe / skip ?
                Console.WriteLine("Error: Outputfile \'{0}\' allready exists! Override!", f);
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
        public override string ToString() {
            return "File: " + this.getXMLFileName() + "\t" + "Path: " + this.getXMLFilePath() + "\n" + this.csventry;
        }
    }
}