using System;
using System.IO;
using System.Collections.Generic;

using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;

using XMLSplit.CSV;
using XMLSplit.Configuration;

namespace XMLSplit.XML {
    public static class HelperClass {
        // Hilfsfunktion um WildCard Matching mit Filename zu machen
        public static bool Glob(this string value, string pattern)
        {
            int pos = 0;

            while (pattern.Length != pos)
            {
                switch (pattern[pos])
                {
                    case '?':
                        break;

                    case '*':
                        for (int i = value.Length; i >= pos; i--)
                        {
                            if (Glob(value.Substring(i), pattern.Substring(pos + 1)))
                            {
                                return true;
                            }
                        }
                        return false;

                    default:
                        if (value.Length == pos || char.ToUpper(pattern[pos]) != char.ToUpper(value[pos]))
                        {
                            return false;
                        }
                        break;
                }

                pos++;
            }

            return value.Length == pos;
        }
    }

    public class XMLFilelist {
        private List<XMLFile> xmlFileList;
        private string xmlProdPath;
        private Config config;
        private CSVData csvData;

        public XMLFilelist(CSVData csvdata, Config config) {
            this.config = config;
            this.csvData = csvdata;
            this.xmlProdPath = this.config.getProductionPath();

            xmlFileList = new List<XMLFile>();
        }
        
        public void getFileList() {
            string [] tmpfileList = Directory.GetFiles(this.xmlProdPath, "*", SearchOption.AllDirectories);
            foreach(string file in tmpfileList) {
                foreach(CSVEntry csventry in this.csvData.getList()) {
                    if ((Path.GetDirectoryName(file) == csventry.getSOURCEPath()) && (Path.GetFileName(file).Glob(csventry.getSOURCEFile()))) {
                        XMLFile tmpXMLFile = new XMLFile(file, csventry, this.config);
                        this.xmlFileList.Add(tmpXMLFile);
                    }
                }
            }
        }

        public void showFileList() {
            foreach(XMLFile file in this.xmlFileList) {
                if (!file.isEmpty()) {
                    Console.WriteLine(file);
                }
            }
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
    }
}