using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Reflection;
using System.Data.OleDb;
using System.Collections;
using System.Xml;
using System.Dynamic;
using System.Web.Script.Serialization;

namespace JureHR
{
    /// <summary>
    /// Data class for data manipulation
    /// </summary>
    public static class DataConversions
    {
        /// <summary>
        /// Get Top DataView Rows
        /// </summary>
        /// <param name="dv"></param>
        /// <param name="n"></param>
        /// <returns>Returns only top specified number of DataView rows</returns>
        public static DataView GetTopDataViewRows(DataView dv, Int32 n)
        {
            DataTable dt = dv.Table.Clone();
            for (int i = 0; i < n; i++)
            {
                if (i >= dv.Count)
                {
                    break;
                }
                dt.ImportRow(dv[i].Row);
            }
            return new DataView(dt, dv.RowFilter, dv.Sort, dv.RowStateFilter);
        }

        /// <summary>
        /// DataTable from XML
        /// </summary>
        /// <param name="FilePath"></param>
        /// <returns>Converts XML document in DataTable</returns>
        public static DataTable DataTableFromXML(string FilePath)
        {
            System.Data.DataTable dt = new System.Data.DataTable();
            try
            {
                if (File.Exists(FilePath))
                {
                    StreamReader StringReader = File.OpenText(FilePath);
                    string XMLString = StringReader.ReadToEnd();
                    StringReader.Dispose();
                    DataSet DataSet = new DataSet();
                    DataSet.ReadXml(new StringReader(XMLString));
                    XMLString = null;
                    dt = DataSet.Tables[0];
                    if (dt.Rows.Count == 0)
                    {
                        dt = new DataTable();
                    }
                }
            }
            catch (Exception ex)
            {
                Mailer.ErrNotify(ex,"Data DataTableFromXML");
            }
            return dt;
        }

        /// <summary>
        /// Get's Xml file as formatted text.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetXmlFileAsString(string fileName)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(fileName);
            var stringWriter = new StringWriter();
            var xmlTextWriter = new XmlTextWriter(stringWriter);
            xmlTextWriter.Formatting = Formatting.Indented;
            xmlDocument.WriteTo(xmlTextWriter);
            xmlTextWriter.Flush();
            return stringWriter.ToString();
        }

        /// <summary>
        /// Function to get object from byte array
        /// </summary>
        /// <param name="ByteArray">byte array to get object</param>
        /// <returns>object</returns>
        public static object ByteArrayToObject(byte[] ByteArray)
        {
            try
            {
                // convert byte array to memory stream
                MemoryStream MemoryStream = new MemoryStream(ByteArray);

                // create new BinaryFormatter
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter BinaryFormatter  = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

                // set memory stream position to starting point
                MemoryStream.Position = 0;

                // Deserializes a stream into an object graph and return as a object.
                return BinaryFormatter.Deserialize(MemoryStream);
            }
            catch (Exception ex)
            {
                Mailer.ErrNotify(ex, "Data ByteArrayToObject");
            }
            return null;
        }

        /// <summary>
        /// Function to get byte array from a object
        /// </summary>
        /// <param name="Object">object to get byte array</param>
        /// <returns>Byte Array</returns>
        public static byte[] ObjectToByteArray(object Object)
        {
            try
            {
                // create new memory stream
                MemoryStream MemoryStream = new MemoryStream();

                // create new BinaryFormatter
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter BinaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

                // Serializes an object, or graph of connected objects, to the given stream.
                BinaryFormatter.Serialize(MemoryStream, Object);

                // convert stream to byte array and return
                return MemoryStream.ToArray();
            }
            catch (Exception ex)
            {
                Mailer.ErrNotify(ex, "Data ObjectToByteArray");
            }
            return null;
        }

        /// <summary>
        /// Creates a CSV file
        /// </summary>
        /// <param name="AbsolutePathAndFileName"></param>
        /// <param name="TheDataTable"></param>
        /// <param name="Options">[0] = separator, e.g. ";" (default = ",")</param>
        public static void DataTableToCsvFile(string AbsolutePathAndFileName, DataTable TheDataTable, params string[] Options)
        {
            //variables
            string separator;
            if (Options.Length > 0)
            {
                separator = Options[0];
            }
            else
            {
                separator = ","; //default
            }

            string quote = "\"";
            //create CSV file
            StreamWriter sw = new StreamWriter(AbsolutePathAndFileName);

            //write header line
            int iColCount = TheDataTable.Columns.Count;
            for (int i = 0; i < iColCount; i++)
            {
                sw.Write(TheDataTable.Columns[i]);
                if (i < iColCount - 1)
                {
                    sw.Write(separator);
                }
            }
            sw.WriteLine();

            //write rows
            foreach (DataRow dr in TheDataTable.Rows)
            {
                for (int i = 0; i < iColCount; i++)
                {
                    if (!Convert.IsDBNull(dr[i]))
                    {
                        string data = dr[i].ToString().Replace(System.Environment.NewLine, " ").Trim();
                        sw.Write(quote + data + quote);
                    }
                    if (i < iColCount - 1)
                    {
                        sw.Write(separator);
                    }
                }
                sw.WriteLine();
            }
            sw.Close();
        }

        /// <summary>
        /// csv To DataTable
        /// </summary>
        /// <param name="file">Path to the file</param>
        /// <param name="isRowOneHeader">Is Row One Header</param>
        /// <param name="Options">CHAR [0] = separator, e.g. ';' (default = ',')</param>
        /// <returns>DataTable</returns>
        public static DataTable CsvToDataTable(string file, bool isRowOneHeader, params char[] Options)
        {

            DataTable csvDataTable = new DataTable();

            try
            {
                String[] csvData = File.ReadAllLines(Path.GetFullPath(file));
                if (csvData.Length == 0)
                {
                    Mailer.ErrNotify("CSV File Appears to be Empty", "csvToDataTable");
                }

                char separator;
                if (Options.Length > 0)
                {
                    separator = Options[0];
                }
                else
                {
                    separator = ','; //default
                }

                String[] headings = csvData[0].Split(separator);
                int index = 0; //will be zero or one depending on isRowOneHeader
                try
                {
                    if (isRowOneHeader)
                    {
                        index = 1; //so we won’t take headings as data

                        for (int i = 0; i < headings.Length; i++)
                        {
                            headings[i] = headings[i].Replace(" ", "_");
                            csvDataTable.Columns.Add(headings[i], typeof(string));
                        }
                    }
                    else //if no headers just go for col1, col2 etc.
                    {
                        for (int i = 0; i < headings.Length; i++)
                        {
                            csvDataTable.Columns.Add("col" + (i + 1).ToString(), typeof(string));
                        }
                    }

                    //populate the DataTable
                    for (int i = index; i < csvData.Length; i++)
                    {
                        DataRow row = csvDataTable.NewRow();
                        for (int j = 0; j < headings.Length; j++)
                        {

                            row[j] = csvData[i].Split(separator)[j];
                        }
                        csvDataTable.Rows.Add(row);
                    }
                }
                catch (Exception exe)
                {
                    Mailer.ErrNotify(exe, "Data csvToDataTable DataTable");
                }
            }

            catch (Exception ex)
            {
                Mailer.ErrNotify(ex.Message, "csvToDataTable");
            }
            //return the CSV DataTable
            return csvDataTable;

        }

        /// <summary>
        /// csv To DataTable only when is separated by coma
        /// </summary>
        /// <param name="path">Path of the file</param>
        /// <returns>DataTable</returns>
        public static DataTable CsvToDataTable(string path)
        {
            if (!File.Exists(path))
                return null;

            string full = Path.GetFullPath(path);
            string file = Path.GetFileName(full);
            string dir = Path.GetDirectoryName(full);

            OleDbConnection ExcelConnection = new OleDbConnection(@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + dir + ";Extended Properties=Text;");
            OleDbCommand ExcelCommand = new OleDbCommand(@"SELECT * FROM " + file, ExcelConnection);

            OleDbDataAdapter ExcelAdapter = new OleDbDataAdapter(ExcelCommand);

            ExcelConnection.Open();

            DataTable dt = new DataTable();

            try
            {
                //fill the DataTable
                ExcelAdapter.Fill(dt);
            }
            catch (InvalidOperationException e)
            {
                Mailer.ErrNotify(e.Message, "Data CsvToDataTable");
            }

            ExcelConnection.Close();
            return dt;
        }

        /// <summary>
        /// Two dimensional List To Data Table
        /// </summary>
        /// <typeparam name="T">Object Name</typeparam>
        /// <param name="items">List of objects</param>
        /// <returns></returns>
        public static DataTable ListToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }

        /// <summary>
        /// Convert Hashtable to Dictionary
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="table"></param>
        /// <returns></returns>
        public static Dictionary<K, V> HashtableToDictionary<K, V>(Hashtable table)
        {
            return table
              .Cast<DictionaryEntry>()
              .ToDictionary(kvp => (K)kvp.Key, kvp => (V)kvp.Value);
        }
    }
}
