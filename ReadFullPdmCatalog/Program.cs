using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadFullPdmCatalog
{
    class Program
    {
        static string catalogFile = @"P:\Architecture Group\Projects\PDM Migration\extracts\PDM-Catalog.csv";
        static string inputFile = @"P:\Architecture Group\Projects\PDM Migration\extracts\CHT\CHT_full_201710092104.txt";
        static string serverName = "pdm.cht.moog.com";
        static string outputFile = @"C:\Users\mvinti\Desktop\PDM\TestDataExtracts\CHT\test_cht.txt";
        static string misfitToys = @"C:\Users\mvinti\Desktop\PDM\TestDataExtracts\CHT\test_cht_misfits.txt";
        static string uncRawPrefix = @"\\eacmpnas01.moog.com\Vol5_Data\PDM\migration\pdm.cht.moog.com";
        static string uncPdfPrefix = @"\\eacmpnas01.moog.com\Vol5_Data\PDM\migration\pdm.cht.moog.com";

        static bool IsExt(string token)
        {
            switch (token.ToLower())
            {
                case "7z":
                case "cad":
                case "cg4":
                case "csh":
                case "csv":
                case "db":
                case "dis":
                case "dll_crea":
                case "doc":
                case "docx":
                case "dos":
                case "dot":
                case "dwg":
                case "dxf":
                case "edt":
                case "flv":
                case "gif":
                case "gp4":
                case "gwk":
                case "hp":
                case "hpdf":
                case "hpg":
                case "hpp":
                case "htm":
                case "html":
                case "ini":
                case "jpg":
                case "js":
                case "mdb":
                case "mht":
                case "mil":
                case "mpg":
                case "msg":
                case "obd":
                case "oft":
                case "pcx":
                case "pdf":
                case "plt":
                case "png":
                case "ppt":
                case "pptx":
                case "pra":
                case "prt":
                case "reg":
                case "rss":
                case "rst":
                case "rtf":
                case "smf":
                case "ss":
                case "ss_old":
                case "tif":
                case "txt":
                case "url":
                case "vsd":
                case "wdf":
                case "wrl":
                case "xls":
                case "xlsx":
                case "xlt":
                case "xps":
                case "xs":
                case "xst":
                case "z":
                case "z_old":
                case "zip":
                    return true;
                default:
                    return false;
            }
        }

        public static void JobTicketGenerator(Dictionary<string, List<string>> dictionary)
        {
            foreach (KeyValuePair<string, List<string>> kvp in dictionary)
            {
                StringBuilder jobTicket = new StringBuilder();

                jobTicket.AppendLine("<?xml version=\"1.0\" encoding=\"ISO-8859-1\" ?>");
                jobTicket.AppendLine("<?AdlibExpress applanguage = \"USA\" appversion = \"4.11.0\" dtdversion = \"2.6\" ?>");
                jobTicket.AppendLine("<!DOCTYPE JOBS SYSTEM \"X:\\PDM\\AdlibExpress.dtd\">");
                jobTicket.AppendLine("<JOBS xmlns:JOBS=\"http://www.adlibsoftware.com\" xmlns:JOB=\"http://www.adlibsoftware.com\">");
                jobTicket.AppendLine("<JOB>");
                jobTicket.AppendLine("<JOB:DOCINPUTS>");

                foreach (var i in kvp.Value)
                {
                    jobTicket.AppendLine("<JOB:DOCINPUT FILENAME=\"" + i + "\" FOLDER =\"X:\\PDM\\in\\\" />");
                }

                jobTicket.AppendLine("</JOB:DOCINPUTS>");
                jobTicket.AppendLine("<JOB:DOCOUTPUTS>");
                jobTicket.AppendLine("<JOB:DOCOUTPUT FILENAME = \"" + kvp.Key + ".pdf\" FOLDER = \"X:\\PDM\\out\\\" DOCTYPE=\"PDF\" />");
                jobTicket.AppendLine("</JOB:DOCOUTPUTS>");
                jobTicket.AppendLine("<JOB:SETTINGS>");
                jobTicket.AppendLine("<JOB:PDFSETTINGS JPEGCOMPRESSIONLEVEL=\"5\" MONOIMAGECOMPRESSION=\"Default\" GRAYSCALE=\"No\" PAGECOMPRESSION=\"Yes\" DOWNSAMPLEIMAGES=\"No\" RESOLUTION=\"1200\" PDFVERSION=\"PDFVersion15\" PDFVERSIONINHERIT=\"No\" PAGES=\"All\" />");
                jobTicket.AppendLine("</JOB:SETTINGS>");
                jobTicket.AppendLine("</JOB>");
                jobTicket.AppendLine("</JOBS>");

                //File.WriteAllText(@"\\eacmpnas01.moog.com\Vol3_Data\PDM\migration\pdm.moog.com\jobs\" + kvp.Key + ".xml", jobTicket.ToString());
                //File.WriteAllText(@"C:\Users\mvinti\Desktop\PDM\XmlChallenge\jobTickets\" + kvp.Key + ".xml", jobTicket.ToString());
            }
        }

        public static string BuildUncRawPath(string filePath)
        {
            StringBuilder uncRawPathName = new StringBuilder();

            if (filePath.EndsWith("._"))
            {
                filePath = filePath.Remove(filePath.Length - 2);
            }

            uncRawPathName.Append(uncRawPrefix);
            uncRawPathName.Append(filePath);
            uncRawPathName.Replace('/', '\\');

            return uncRawPathName.ToString();
        }

        public static string BuildUncPdfPath(string itemName, string itemRev)
        {
            StringBuilder uncPdfPathName = new StringBuilder();

            uncPdfPathName.Append(uncPdfPrefix + "\\" + itemName);

            if (!String.IsNullOrEmpty(itemRev))
            {
                uncPdfPathName.Append("." + itemRev);
            }

            uncPdfPathName.Append(".pdf");
            uncPdfPathName.Replace('/', '\\');

            return uncPdfPathName.ToString();
        }

        static void Main(string[] args)
        {
            List<string> delimitedDataField = new List<string>();
            List<string> islandOfMisfitToys = new List<string>();
            //List<string> pdmCatalogFileNames = new List<string>();
            Hashtable hashtable = new Hashtable();
            Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();

            int counter = 0;
            int counterSS = 0;
            string line;

            // Load PDM Catalog Data
            StreamReader sr = new StreamReader(catalogFile);
            string headerLine = sr.ReadLine();
            string catalogLine;

            while ((catalogLine = sr.ReadLine()) != null)
            {
                var pdmCatalogItem = new PdmCatalogItem();
                List<string> pdmCatalog = catalogLine.Split(',').ToList();

                pdmCatalogItem.Server = pdmCatalog[0];
                pdmCatalogItem.ObjType = Convert.ToInt32(pdmCatalog[1]);
                pdmCatalogItem.FileName = pdmCatalog[2];

                //pdmCatalogFileNames.Add(pdmCatalogItem.FileName);
                if (!hashtable.ContainsKey(pdmCatalogItem.FileName))
                {
                    hashtable.Add(pdmCatalogItem.FileName, null);
                }
            }
            
            // Build the extract
            StreamReader file = new StreamReader(inputFile);

            delimitedDataField.Add("FILE_SIZE,LAST_ACCESSED,ITEM,REV,SHEET,SERVER,UNC_RAW,UNC_PDF");

            while ((line = file.ReadLine()) != null)
            {
                Console.WriteLine(line);

                // if line ends with.ss, ignore
                if (line.EndsWith(".ss"))
                {
                    counterSS++;
                    continue;
                }

                var item = new MBSItem();
                item.HasRev = false;
                item.HasSht = false;
                item.HasExt = false;

                List<string> data = line.Split(' ').ToList();
                data.RemoveAll(String.IsNullOrEmpty);

                item.FileSize = Convert.ToInt64(data[0]);

                item.FileDateTime = Convert.ToDateTime(data[1] + ' ' + data[2] + ' ' + data[3]);

                //item.FileDateTime = data[1] is month/day/year; data[2] is HH:MM:SS; data[3] is AM/PM 
                //item.FileDateTime = Convert.ToDateTime(item.FileMonth + ' ' + item.FileDay + ' ' + item.FileYear + ' ' + item.FileTime);

                item.FilePath = data[4];

                // parse file name
                var idx1 = item.FilePath.LastIndexOf('\\');
                var pathOnly = item.FilePath.Substring(2, idx1 + 1);
                var fileName = item.FilePath.Substring(idx1 + 1);

                string[] dataFileSplit = item.FilePath.Split('.');

                var idx2 = dataFileSplit[0].LastIndexOf('\\');
                item.ItemName = dataFileSplit[0].Substring(idx2 + 1);

                if (dataFileSplit.Length == 2)
                {
                    item.HasExt = true;
                    item.ItemExt = dataFileSplit[1];
                    string uncRawPath = BuildUncRawPath(item.FilePath);
                    string uncPdfPath = BuildUncPdfPath(item.ItemName, item.ItemRev);
                    delimitedDataField.Add(item.FileSize + "," + item.FileDateTime.ToString("MMM d yyyy HH:mm") + "," + item.ItemName + ",,," + item.FilePath);
                }

                else if (dataFileSplit.Length > 2)
                {
                    if (!IsExt(dataFileSplit[1]))
                    {
                        item.HasRev = true;
                        item.ItemRev = dataFileSplit[1];
                    }
                    else
                    {
                        item.HasExt = true;
                        item.ItemExt = dataFileSplit[1];
                    }

                    if (!IsExt(dataFileSplit[2]))
                    {
                        item.HasSht = true;
                        item.ItemSht = dataFileSplit[2];
                    }
                    else
                    {
                        item.HasExt = true;
                        item.ItemExt = dataFileSplit[2];
                    }

                    if (dataFileSplit.Length > 3 && IsExt(dataFileSplit[3]))
                    {
                        item.HasExt = true;
                        item.ItemExt = dataFileSplit[3];
                    }

                    //if (!pdmCatalogFileNames.Contains(fileName))
                    //{
                    //    islandOfMisfitToys.Add(item.FilePath);
                    //    Console.WriteLine("Not found in catalog: " + fileName);
                    //    continue;
                    //}

                    //hashtable check
                    if (!hashtable.ContainsKey(fileName))
                    {
                        islandOfMisfitToys.Add(item.FilePath);
                        Console.WriteLine("Not found in catalog: " + fileName);
                        continue;
                    }

                    StringBuilder output = new StringBuilder(item.FileSize + "," + item.FileDateTime.ToString("MMM d yyyy HH:mm") + "," + item.ItemName);

                    output.Append((item.HasRev) ? ("," + item.ItemRev) : (","));
                    output.Append((item.HasSht) ? ("," + item.ItemSht) : (","));
                    output.Append(("," + serverName));

                    string uncRawPath = BuildUncRawPath(item.FilePath);
                    string uncPdfPath = BuildUncPdfPath(item.ItemName, item.ItemRev);

                    output.Append("," + uncRawPath + "," + uncPdfPath);

                    if (!item.HasRev && !item.HasSht && !item.HasExt)
                    {
                        islandOfMisfitToys.Add(item.FilePath);
                    }
                    else
                    {
                        delimitedDataField.Add(output.ToString());
                    }
                }

                else
                {
                    islandOfMisfitToys.Add(item.FilePath);
                }

                counter++;

                string uID = item.ItemName + "." + item.ItemRev;

                //if (!dictionary.Keys.Contains(uID))
                //{
                //    List<string> valueFilePath = new List<string>();
                //    valueFilePath.Add(fullPathName.ToString());
                //    dictionary.Add(uID, valueFilePath);
                //}
                //else
                //{
                //    dictionary[uID].Add(fullPathName.ToString());
                //}
            }

            //foreach (KeyValuePair<string, List<string>> dictionaryKeyValuePair in dictionary)
            //{
            //    foreach (var derp in dictionaryKeyValuePair.Value)
            //        Console.WriteLine(dictionaryKeyValuePair.Key + "." + derp);
            //}

            //JobTicketGenerator(dictionary);

            //Console.WriteLine(dictionary.Count);

            File.WriteAllLines(outputFile, delimitedDataField);
            File.WriteAllLines(misfitToys, islandOfMisfitToys); 
        }
    }
}