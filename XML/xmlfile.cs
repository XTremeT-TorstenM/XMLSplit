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
    public class XMLFile {
        private string xmlFile, xmlFilePath = "", xmlFileName, xmlBackupFile = "";
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
            this.xmlFileName = Path.GetFileName(xmlFile);
            this.xmlFilePath = Path.GetDirectoryName(xmlFile);
            this.mitSKZfn = Path.Combine(this.csventry.getTarget(), this.xmlFileName.Replace(".xml", ".MITSKZ.xml"));
            this.mitxslt = Path.Combine(this.config.getXSLTPath(), this.csventry.getMitSKZ());
            this.ohneSKZfn = Path.Combine(this.csventry.getTarget(), this.xmlFileName.Replace(".xml", ".OHNESKZ.xml"));
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

        // logt Datei
        public void logXML() {
            this.log.addLog(string.Format("\n#Process: XML {0}", this.xmlFileName));
        }

        // Macht eine Sicherungskopie des akutellen XML Files
        public void backup() {
            // Backup Flag true ?
            if (this.config.isBackup()) {
                // Dateinamen fuer Backupfile aus XML Dateinamen generieren
                this.xmlBackupFile = Path.Combine(this.config.getBackupPath(), this.getXMLFileName().Replace(".xml", ".BACKUP.xml"));
                // wenn das Backup noch nicht existiert (sollte bei filename mit Datetime selten passieren)
                if (!File.Exists(this.xmlBackupFile))
                {
                    // log / file copy dest -> target
                    this.log.addLog(string.Format("Backup: {0} to {1}", this.xmlFileName, this.xmlBackupFile));
                    File.Copy(this.xmlFile, this.config.getBackupPath() + "\\" + this.getXMLFileName().Replace(".xml", ".BACKUP.xml"));
                }
                else
                {
                    // logging / Ausgabe 
                    this.log.addLog(string.Format("Backup: {0} allready exists!", this.xmlBackupFile));
                    Console.WriteLine("Error: Backupfile {0} allready exists !", this.xmlBackupFile);
                }
            }
        }

        // Loescht das abgearbeitete XML
        public void delete() {
            // Delete flag gesetzt ?
            if (this.config.isDeleteXMLFile())
            {
                // XMLFile loeschen
                this.log.addLog(string.Format("Delete: {0}", this.xmlFileName));
                File.Delete(this.xmlFile);
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
                string trtime = DateTime.Now.ToString("HH-mm-ss");
                this.log.addLog(string.Format("Begin transformation @ {0}", trtime));

                // jeweils mit und ohne SKZ splitten und Fehlerbehandlung
                string result = "Transformation succsessful !";
                // XDocument xmlTree = XDocument.Load(this.xmlFile);
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
                    // Console.WriteLine("Error on transformation: {0}", e);
                    this.log.addLog(result);
                    Console.WriteLine("Result: {0}", result);

                    trtime = DateTime.Now.ToString("HH-mm-ss");
                    this.log.addLog(string.Format("End transformation @ {0}", trtime));
                }
            }
            else {
                this.log.addLog(string.Format("XML file {0} does not contain any \"Datenstrom\" {1}", this.xmlFile, this.csventry.getDatenstrom()));
            }
        }

        // split mit SKZ
        public void transform_mitSKZ() {
            if (checkSplitFiles(this.mitSKZfn, this.mitxslt)) {
                // Sonderfall im mitSKZ Split der eine einfache Kopie ist
                if (!mitxslt.EndsWith("komplett.xslt")) {
                    // split / log / Ausgabe
                    this.log.addLog(string.Format("Transform {0} with {1}", this.xmlFile, this.mitxslt));

                    // neues XDocument fuer Transformation
                    XDocument newTree = new XDocument(new XDeclaration("1.0", "iso-8859-1", "yes"));

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
                    this.log.addLog(string.Format("Only copy from {0} to {1} needed!", this.xmlFile, this.mitSKZfn));
                    // Console.WriteLine("Kopie {0} to {1}", this.xmlFile, mittargetfn);
                    File.Copy(this.xmlFile, this.mitSKZfn);
                }
            }
            else {
                // log / Ausgabe / skip ?
                this.log.addLog(string.Format("Error: XSLT file {0} missing!", this.mitxslt));
                // Console.WriteLine("Error: Xslt File {0} missing!", mitxslt);
            }
        }

        // split ohne SKZ
        public void transform_ohneSKZ() {
            if (checkSplitFiles(this.ohneSKZfn, this.ohnexslt)) {
                // Sonderfall im ohneSKZ Split der kein Split durchfuehrt und Outputfile ignoriert
                if (!ohnexslt.EndsWith("leer.xslt")){
                    // split / log / Ausgabe
                    this.log.addLog(string.Format("Transform {0} with {1}", this.xmlFile, this.ohnexslt));

                    // neues XDocument fuer Transformation
                    XDocument newTree = new XDocument(new XDeclaration("1.0", "iso-8859-1", "yes"));

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
                    this.log.addLog(string.Format("Skip outputfile {0}! (leer.xslt)", this.ohneSKZfn));
                    // Console.WriteLine("Msg: Skip Outputfile {0} because xlst = leer.xslt!", ohnetargetfn);
                }
            }
            else {
                // log / Ausgabe / skip ?
                this.log.addLog(string.Format("Error: XSLT file {0} missing!", this.ohnexslt));
                // Console.WriteLine("Error: Xslt File {0} missing!", ohnexslt);
            }
        }

        // copy zum Printer
        public void copy2Printer() {
            // check Copy2Printer Flag 
            if (this.config.isCopy2Printer()) {
                string result = "Copy to printer successful!";
                try {
                    // copy mitSKZ + ohneSKZ zum Drucker
                    File.Copy(this.mitSKZfn, this.csventry.getPrinter());
                    File.Copy(this.ohneSKZfn, this.csventry.getPrinter());
                }
                catch (Exception e) {
                    result = "Copy to printer error: " + e.ToString();
                }
                finally {
                    this.log.addLog(result);
                }
            }
        }

        // check output File + xslt File
        public static bool checkSplitFiles(string f, string x) {
            // check ob Outputfile schon vorhanden ist.
            if (File.Exists(f)) {
                // Ausgabe / skip ?
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
        public override string ToString() {
            return "File: " + this.getXMLFileName() + "\t" + "Path: " + this.getXMLFilePath() + "\n" + this.csventry;
        }
    }
}