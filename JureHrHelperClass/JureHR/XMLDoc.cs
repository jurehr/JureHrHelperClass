using System;
using System.Data;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Xsl;
using System.Security.Cryptography.Xml;
using System.Security.Cryptography;

namespace JureHR
{

    /// <summary>
    /// Manipulation of XML Document
    /// </summary>
    public static class XMLDoc
    {

        /// <summary>
        /// Adding Element and Attribute to XMLDocument
        /// </summary>
        public static class XmlElement
        {
            /// <summary>
            /// Adding Element to XMLDocument
            /// </summary>
            /// <param name="xmldoc"></param>
            /// <param name="XmlElement"></param>
            /// <param name="Name"></param>
            /// <param name="Content"></param>
            public static void AddXmlElement(XmlDocument xmldoc, ref System.Xml.XmlElement XmlElement, string Name, object Content)
            {
                Name = Name.Replace("(", "");
                Name = Name.Replace(")", "");
                Name = Name.Replace(" ", "");
                Name = Name.Replace(";", "");
                Name = Name.Replace(",", "");
                Name = Name.Replace(".", "");
                System.Xml.XmlElement xmlElement = xmldoc.CreateElement(Name);
                if (Content != null)
                {
                    xmlElement.InnerText = Content.ToString();
                }
                else
                {
                    xmlElement.InnerText = "";
                }
                XmlElement.AppendChild(xmlElement);
            }

            /// <summary>
            /// Adding Attribute to XMLDocument
            /// </summary>
            /// <param name="xmldoc"></param>
            /// <param name="XmlElement"></param>
            /// <param name="Name"></param>
            /// <param name="Content"></param>
            public static void AddXmlAttribute(XmlDocument xmldoc, ref System.Xml.XmlElement XmlElement, string Name, object Content)
            {
                Name = Name.Replace("(", "");
                Name = Name.Replace(")", "");
                Name = Name.Replace(" ", "");
                Name = Name.Replace(";", "");
                Name = Name.Replace(",", "");
                Name = Name.Replace(".", "");
                XmlAttribute xmlAttribute = xmldoc.CreateAttribute(Name);
                if (Content != null)
                {
                    xmlAttribute.InnerText = Content.ToString();
                }
                else
                {
                    xmlAttribute.InnerText = "";
                }
                XmlElement.Attributes.Append(xmlAttribute);
            }
        }

        /// <summary>
        /// XmlNode manipulation
        /// </summary>
        public static class XmlNode
        {
            /// <summary>
            /// Try To read XMLNode
            /// </summary>
            /// <param name="node"></param>
            /// <param name="nodeName"></param>
            /// <returns></returns>
            public static string TryToRead(System.Xml.XmlNode node, string nodeName)
            {
                return (node.SelectSingleNode(nodeName) != null && node.SelectSingleNode(nodeName).InnerText.Trim().Length > 0) ? node.SelectSingleNode(nodeName).InnerText : "";
            }

            /// <summary>
            /// Set the value of the XMLNode
            /// </summary>
            /// <param name="n"></param>
            /// <param name="Value"></param>
            public static void AssignNodeValue(ref System.Xml.XmlNode n, string Value)
            {
                if (n != null)
                {
                    n.InnerText = Value;
                }
            }
        }

        /// <summary>
        /// Converting XSLT to HTML and Back
        /// </summary>
        public static class XSLT
        {
            /// <summary>
            /// Converting XmlXslt file to Html
            /// </summary>
            /// <param name="xdoc"></param>
            /// <param name="xslFilePath"></param>
            /// <param name="XsltArgumentList"></param>
            /// <param name="outputString"></param>
            /// <returns></returns>
            public static bool XmlXsltToHtml(XmlDocument xdoc, string xslFilePath, XsltArgumentList XsltArgumentList, out string outputString)
            {
                bool result = false;
                outputString = null;
                XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
                StringWriter stringWriter = new StringWriter();
                try
                {
                    if (xdoc.ChildNodes.Count > 0)
                    {
                        xslCompiledTransform.Load(xslFilePath);
                        xslCompiledTransform.Transform(xdoc, XsltArgumentList, stringWriter);
                        outputString = stringWriter.ToString();
                        result = true;
                    }
                }
                catch (Exception exc)
                {
                   Mailer.ErrNotify(exc, "Utils\\XmlXslt_to_Html");
                }
                finally
                {
                    stringWriter.Close();
                }
                return result;
            }

            /// <summary>
            /// Converting XmlXslt from XMLReader to Html
            /// </summary>
            /// <param name="xdoc"></param>
            /// <param name="XmlReader"></param>
            /// <param name="XsltArgumentList"></param>
            /// <param name="outputString"></param>
            /// <returns></returns>
            public static bool XmlXsltToHtml(XmlDocument xdoc, XmlReader XmlReader, XsltArgumentList XsltArgumentList, out string outputString)
            {
                bool result = false;
                outputString = null;
                XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
                StringWriter stringWriter = new StringWriter();
                try
                {
                    if (xdoc.ChildNodes.Count > 0)
                    {
                        xslCompiledTransform.Load(XmlReader);
                        xslCompiledTransform.Transform(xdoc, XsltArgumentList, stringWriter);
                        outputString = stringWriter.ToString();
                        result = true;
                    }
                }
                catch (Exception exc)
                {
                    Mailer.ErrNotify(exc, "Utils\\XmlXslt_to_Html");
                }
                finally
                {
                    stringWriter.Close();
                }
                return result;
            }
        }

        /// <summary>
        /// XML Serialization
        /// </summary>
        public static class Serialization
        {
            /// <summary>
            /// Object To XmlDocument
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            public static XmlDocument ObjectToXmlDocument(object obj)
            {
                XmlDocument xmlDocument = new XmlDocument();
                try
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    StringWriter textWriter = new StringWriter(stringBuilder);
                    XmlSerializer xmlSerializer = new XmlSerializer(obj.GetType());
                    XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces();
                    xmlSerializerNamespaces.Add(string.Empty, string.Empty);
                    xmlSerializer.Serialize(textWriter, obj, xmlSerializerNamespaces);
                    xmlDocument.LoadXml(stringBuilder.ToString());
                }
                catch (Exception exc)
                {
                    Mailer.ErrNotify(exc, "Utils\\Serialization_ObjectToXml");
                }
                return xmlDocument;
            }

            /// <summary>
            /// XmlDocument To Object
            /// </summary>
            /// <param name="objType"></param>
            /// <param name="stringXmlDoc"></param>
            /// <returns></returns>
            public static object XmlDocumentToObject(Type objType, string stringXmlDoc)
            {
                object result = new object();
                try
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(objType);
                    StringReader textReader = new StringReader(stringXmlDoc);
                    result = xmlSerializer.Deserialize(textReader);
                }
                catch (Exception exc)
                {
                    Mailer.ErrNotify(exc, "Utils\\Serialization_XmlToObject");
                }
                return result;
            }

            /// <summary>
            /// DataTable AddTo XmlRoot
            /// </summary>
            /// <param name="XmlDoc"></param>
            /// <param name="XmlElement_Root"></param>
            /// <param name="_DataTable"></param>
            /// <param name="TableName"></param>
            /// <param name="RowName"></param>
            public static void DataTableAddToXmlRoot(XmlDocument XmlDoc, ref System.Xml.XmlElement XmlElement_Root, DataTable _DataTable, string TableName, string RowName)
            {
                if ((_DataTable.Rows.Count != 1 || _DataTable.Columns.Count != 1 || _DataTable.Rows[0][0].ToString().Length != 0) && _DataTable.Rows.Count > 0)
                {
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument = Serialization.DataTableToXmlDocument(_DataTable, TableName, RowName);
                    XmlElement_Root.AppendChild(XmlDoc.ImportNode(xmlDocument.SelectSingleNode(TableName), true));
                }
            }

            /// <summary>
            /// DataTable To XmlDocument
            /// </summary>
            /// <param name="_DataTable"></param>
            /// <param name="TableName"></param>
            /// <param name="RowName"></param>
            /// <returns></returns>
            public static XmlDocument DataTableToXmlDocument(DataTable _DataTable, string TableName, string RowName)
            {
                XmlDocument xmlDocument = new XmlDocument();
                TableName = TableName.Trim();
                if (TableName.Length == 0)
                {
                    TableName = "root";
                }
                System.Xml.XmlElement xmlElement = xmlDocument.CreateElement(TableName);
                RowName = RowName.Trim();
                if (RowName.Length == 0)
                {
                    RowName = _DataTable.TableName;
                }
                foreach (DataRow dataRow in _DataTable.Rows)
                {
                    System.Xml.XmlElement xmlElement2 = xmlDocument.CreateElement(RowName);
                    foreach (DataColumn dataColumn in _DataTable.Columns)
                    {
                        System.Xml.XmlElement xmlElement3 = xmlDocument.CreateElement(dataColumn.ColumnName);
                        xmlElement3.InnerText = dataRow[dataColumn].ToString();
                        xmlElement2.AppendChild(xmlElement3);
                    }
                    xmlElement.AppendChild(xmlElement2);
                }
                xmlDocument.AppendChild(xmlElement);
                return xmlDocument;
            }

            /// <summary>
            /// DataSet To XmlDocument
            /// </summary>
            /// <param name="_DataSet"></param>
            /// <returns></returns>
            public static XmlDocument DataSetToXmlDocument(DataSet _DataSet)
            {
                XmlDocument xmlDocument = new XmlDocument();
                System.Xml.XmlElement xmlElement = xmlDocument.CreateElement("root");
                foreach (DataTable dataTable in _DataSet.Tables)
                {
                    foreach (DataRow dataRow in dataTable.Rows)
                    {
                        System.Xml.XmlElement xmlElement2 = xmlDocument.CreateElement(dataTable.TableName);
                        foreach (DataColumn dataColumn in dataTable.Columns)
                        {
                            System.Xml.XmlElement xmlElement3 = xmlDocument.CreateElement(dataColumn.ColumnName);
                            xmlElement3.InnerText = dataRow[dataColumn].ToString();
                            xmlElement2.AppendChild(xmlElement3);
                        }
                        xmlElement.AppendChild(xmlElement2);
                    }
                }
                xmlDocument.AppendChild(xmlElement);
                return xmlDocument;
            }
        }

        /// <summary>
        /// XmlDocument To String
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <returns></returns>
        public static string ToString(XmlDocument xmlDoc)
        {
            string result = "";
            try
            {
                if (xmlDoc != null && xmlDoc.ChildNodes.Count > 0)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    StringWriter w = new StringWriter(stringBuilder);
                    XmlTextWriter xmlTextWriter = null;
                    try
                    {
                        xmlTextWriter = new XmlTextWriter(w);
                        xmlTextWriter.Formatting = Formatting.Indented;
                        xmlDoc.WriteTo(xmlTextWriter);
                    }
                    finally
                    {
                        if (xmlTextWriter != null)
                        {
                            xmlTextWriter.Close();
                        }
                    }
                    result = stringBuilder.ToString();
                }
            }
            catch (Exception exc)
            {
                Mailer.ErrNotify(exc, "Utils\\Xml.ToString");
            }
            return result;
        }

        /// <summary>
        /// XML digital signatures (XMLDSIG) allow you to verify that data was not altered after it was signed
        /// </summary>
        /// <param name="key"></param>
        /// <param name="doc"></param>
        public static void SignXmlDocument(RSA key, XmlDocument doc)
        {
            // Create a SignedXml object.
            SignedXml sxml = new SignedXml(doc)
            {
                // Add the key to the SignedXml document.
                SigningKey = key,
                SignedInfo = { CanonicalizationMethod = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315" }
            };
            // Create a reference to be signed.
            Reference r = new Reference("");
            // Add an enveloped transformation to the reference.
            r.AddTransform(new XmlDsigEnvelopedSignatureTransform(false));
            // Add the reference to the SignedXml object.
            sxml.AddReference(r);
            // Compute the signature.
            sxml.ComputeSignature();
            // Get the XML representation of the signature and save 
            // it to an XmlElement object.
            System.Xml.XmlElement sig = sxml.GetXml();
            // Append the element to the XML document.
            doc.DocumentElement.AppendChild(sig);
        }

        /// <summary>
        /// The key can then be retrieved to verify the XML digital signature
        /// </summary>
        /// <param name="key"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static bool VerifyXmlDocument(RSA key, XmlDocument doc)
        {
            SignedXml sxml = new SignedXml(doc);
            try
            {
                System.Xml.XmlNode sig = doc.GetElementsByTagName("Signature", "http://www.w3.org/2000/09/xmldsig#")[0];
                sxml.LoadXml((System.Xml.XmlElement)sig);
            }
            catch
            {
                return false;
            }
            return sxml.CheckSignature(key);
        }
    }
}