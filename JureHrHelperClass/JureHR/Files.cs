using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using System.Web;
using System.IO.Compression;
using System.Security.Cryptography;

namespace JureHR
{
    /// <summary>
    /// Class Files for File Manipulation
    /// </summary>
    public static class Files
    {
        
        /// <summary>
        /// Methods to upload file to FTP Server
        /// </summary>
        /// <param name="FileName">local source file name</param>
        /// <param name="UploadPath">Upload FTP path including Host name</param>
        /// <param name="FTPUser">FTP login username</param>
        /// <param name="FTPPass">FTP login password</param>
        public static void UploadFile(string FileName, string UploadPath, string FTPUser, string FTPPass)
        {
            FileInfo FileInfo = new FileInfo(FileName);

            // Create FtpWebRequest object from the Uri provided
            System.Net.FtpWebRequest FtpWebRequest = (System.Net.FtpWebRequest)System.Net.FtpWebRequest.Create(new Uri(UploadPath));

            // Provide the WebPermission Credintials
            FtpWebRequest.Credentials = new System.Net.NetworkCredential(FTPUser, FTPPass);

            // By default KeepAlive is true, where the control connection is not closed
            // after a command is executed.
            FtpWebRequest.KeepAlive = false;

            // set timeout for 20 seconds
            FtpWebRequest.Timeout = 20000;

            // Specify the command to be executed.
            FtpWebRequest.Method = System.Net.WebRequestMethods.Ftp.UploadFile;

            // Specify the data transfer type.
            FtpWebRequest.UseBinary = true;

            // Notify the server about the size of the uploaded file
            FtpWebRequest.ContentLength = FileInfo.Length;

            // The buffer size is set to 2kb
            int buffLength = 2048;
            byte[] buff = new byte[buffLength];

            // Opens a file stream (FileStream) to read the file to be uploaded
            FileStream FileStream = FileInfo.OpenRead();

            try
            {
                // Stream to which the file to be upload is written
                Stream Stream = FtpWebRequest.GetRequestStream();

                // Read from the file stream 2kb at a time
                int contentLen = FileStream.Read(buff, 0, buffLength);

                // Till Stream content ends
                while (contentLen != 0)
                {
                    // Write Content from the file stream to the FTP Upload Stream
                    Stream.Write(buff, 0, contentLen);
                    contentLen = FileStream.Read(buff, 0, buffLength);
                }

                // Close the file stream and the Request Stream
                Stream.Close();
                Stream.Dispose();
                FileStream.Close();
                FileStream.Dispose();
            }
            catch (Exception ex)
            {
                Mailer.ErrNotify(ex, "Files UploadFile");
            }
        }

        /// <summary>
        /// Function to save byte array to a file
        /// </summary>
        /// <param name="FileName">File name to save byte array</param>
        /// <param name="ByteArray">Byte array to save to external file</param>
        /// <returns>Return true if byte array save successfully, if not return false</returns>
        public static bool SaveByteArrayToFile(string FileName, byte[] ByteArray)
        {
            try
            {
                // Open file for reading
                FileStream FileStream = new FileStream(FileName, FileMode.Create, FileAccess.Write);

                // Writes a block of bytes to this stream using data from a byte array.
                FileStream.Write(ByteArray, 0, ByteArray.Length);

                // close file stream
                FileStream.Close();

                return true;
            }
            catch (Exception ex)
            {
                Mailer.ErrNotify(ex, "Files SaveByteArrayToFile");
            }

            // error occured, return false
            return false;
        }

        /// <summary>
        /// Function to get byte array from a file
        /// </summary>
        /// <param name="FileName">File name to get byte array</param>
        /// <returns>Byte Array</returns>
        public static byte[] FileToByteArray(string FileName)
        {
            byte[] Buffer = null;

            try
            {
                // Open file for reading
                FileStream FileStream = new FileStream(FileName, FileMode.Open, FileAccess.Read);

                // attach filestream to binary reader
                BinaryReader BinaryReader = new BinaryReader(FileStream);

                // get total byte length of the file
                long TotalBytes = new FileInfo(FileName).Length;

                // read entire file into buffer
                Buffer = BinaryReader.ReadBytes((Int32)TotalBytes);

                // close file reader
                FileStream.Close();
                FileStream.Dispose();
                BinaryReader.Close();
            }
            catch (Exception ex)
            {
                Mailer.ErrNotify(ex, "Files FileToByteArray");
            }

            return Buffer;
        }

        /// <summary>
        /// Copyes file
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="overwrite"></param>
        /// <returns>returns bool</returns>
        public static bool CopyFile(string from, string to, bool overwrite)
        {
            if (!File.Exists(from))
            {
                return false;
            }

            try
            {
                File.Copy(from, to, overwrite);
            }
            catch (Exception ex)
            {
                Mailer.ErrNotify(ex, "Files copyFile");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Copy directory structure recursively
        /// </summary>
        /// <param name="Src"></param>
        /// <param name="Dst"></param>
        public static void CopyDirectory(string Src, string Dst)
        {
            String[] Files;

            if (Dst[Dst.Length - 1] != Path.DirectorySeparatorChar)
                Dst += Path.DirectorySeparatorChar;
            if (!Directory.Exists(Dst)) Directory.CreateDirectory(Dst);
            Files = Directory.GetFileSystemEntries(Src);
            foreach (string Element in Files)
            {
                // Sub directories
                if (Directory.Exists(Element))
                    CopyDirectory(Element, Dst + Path.GetFileName(Element));
                // Files in directory
                else
                    File.Copy(Element, Dst + Path.GetFileName(Element), true);
            }
        }

        /// <summary>
        /// Creates Txt File
        /// </summary>
        /// <param name="path"></param>
        /// <returns>returns bool</returns>
        public static bool CreateTextFile(string path)
        {
            if (!File.Exists(path))
            {
                FileStream fs = File.Create(path);
                fs.Close();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Deletes File
        /// </summary>
        /// <param name="file"></param>
        /// <returns>returns bool</returns>
        public static bool DeleteFile(string file)
        {
            if (!File.Exists(file))
            {
                return false;
            }

            try
            {
                File.Delete(file);
                return true;
            }
            catch (Exception ex)
            {
                Mailer.ErrNotify(ex, "Files deleteFile");
                return false;
            }
        }

        /// <summary>
        /// Create Fresh Temp Dir in Windows Temp Folder
        /// </summary>
        /// <returns></returns>
        public static string CreateFreshTempDir()
        {
            int c = 0;
            string tmpPath = Path.GetTempPath();
            string currentDirectory = tmpPath;
            while (Directory.Exists(currentDirectory))
            {
                c++;
                currentDirectory = Path.Combine(tmpPath, c.ToString());
            }
            Directory.CreateDirectory(currentDirectory);
            return currentDirectory;
        }

        /// <summary>
        /// Create Fresh Temp file in C://
        /// </summary>
        /// <returns></returns>
        public static string GetNewTempFile()
        {
            Random r2 = new Random();
            Random r = new Random(((int)(DateTime.Now.Ticks - 0x5f5e100L)) + r2.Next(0, 0x186a0));
            return string.Concat(new object[] { @"c:\tmp", DateTime.Now.Year.ToString(), DateTime.Now.Month.ToString(), DateTime.Now.Day, DateTime.Now.Ticks.ToString(), ".", r.Next(0, 0x63).ToString(), ".txt" });
        }

        /// <summary>
        /// formats file size when provided in bytes
        /// </summary>
        /// <param name="FileSize"></param>
        /// <returns></returns>
        public static string FormatSize(double FileSize)
        {
            try
            {
                if (FileSize < 1024) //<1KB
                {
                    return String.Format("{0:N0} B", FileSize);
                }
                else if (FileSize < 1048576) //<1MB
                {
                    return String.Format("{0:N2} KB", FileSize / 1024); //1KB in bytes
                }
                else if (FileSize < 1073741824) //<1GB
                {
                    return String.Format("{0:N2} MB", FileSize / 1048576); //1MB in bytes
                }
                else if (FileSize < 1099511627776) //<1TB
                {
                    return String.Format("{0:N2} GB", FileSize / 1073741824); //1GB in bytes
                }
                else if (FileSize < 1125899906842624) //<1TeB
                {
                    return String.Format("{0:N2} TB", FileSize / (1099511627776)); //1TB in bytes
                }
                else
                {
                    return "Your logic is flwed.";
                }
            }
            catch (Exception ex)
            {
                Mailer.ErrNotify(ex, "Files formatSize");
                return "Your logic is flwed.";
            }
        }

        /// <summary>
        /// formats file size when provided in bytes
        /// </summary>
        /// <param name="FileSize"></param>
        /// <param name="DecimalPlaces"></param>
        /// <returns></returns>
        public static string FormatSize(double FileSize, int DecimalPlaces)
        {
            if (FileSize > Math.Pow(1024.0, 6.0))
            {
                return (Math.Round((double)((FileSize) / Math.Pow(1024.0, 6.0)), DecimalPlaces) + " EB");
            }
            if (FileSize > Math.Pow(1024.0, 5.0))
            {
                return (Math.Round((double)((FileSize) / Math.Pow(1024.0, 5.0)), DecimalPlaces) + " PB");
            }
            if (FileSize > Math.Pow(1024.0, 4.0))
            {
                return (Math.Round((double)((FileSize) / Math.Pow(1024.0, 4.0)), DecimalPlaces) + " TB");
            }
            if (FileSize > Math.Pow(1024.0, 3.0))
            {
                return (Math.Round((double)((FileSize) / Math.Pow(1024.0, 3.0)), DecimalPlaces) + " GB");
            }
            if (FileSize > Math.Pow(1024.0, 2.0))
            {
                return (Math.Round((double)((FileSize) / Math.Pow(1024.0, 2.0)), DecimalPlaces) + " MB");
            }
            if (FileSize > Math.Pow(1024.0, 1.0))
            {
                return (Math.Round((double)((FileSize) / Math.Pow(1024.0, 1.0)), DecimalPlaces) + " KB");
            }
            return (FileSize + " B");
        }

        /// <summary>
        /// gets folder tree size in bytes
        /// </summary>
        /// <param name="physicalPath"></param>
        /// <returns></returns>
        public static double GetFolderSize(string physicalPath)
        {
            try
            {
                double dblDirSize = 0;
                DirectoryInfo objDirInfo = new DirectoryInfo(physicalPath);
                Array arrChildFiles = objDirInfo.GetFiles();
                Array arrSubFolders = objDirInfo.GetDirectories();
                foreach (FileInfo objChildFile in arrChildFiles)
                {
                    dblDirSize += objChildFile.Length;
                }
                foreach (DirectoryInfo objSubFolder in arrSubFolders)
                {
                    dblDirSize += GetFolderSize(objSubFolder.FullName);
                }
                return dblDirSize;
            }
            catch (Exception ex)
            {
                Mailer.ErrNotify(ex, "Files getFolderSize");
                return 0;
            }
        }

        /// <summary>
        /// Moves File
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="overwrite"></param>
        /// <returns>returns bool</returns>
        public static bool MoveFile(string from, string to, bool overwrite)
        {
            if (!File.Exists(from))
            {
                return false;
            }

            //overwrite
            if (overwrite)
            {
                if (File.Exists(to))
                {
                    File.Delete(to);
                }

                File.Move(from, to);
                return true;
            }
            else
            {
                return false;
            }

        }
        
        /// <summary>
        /// read text file to string array
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string[] ReadTextFile(string file)
        {
            try
            {
                return File.ReadAllLines(file);
            }
            catch (Exception ex)
            {

                Mailer.ErrNotify(ex, "Files readTextFile");
                return null;
            }
        }

        /// <summary>
        /// returns StreamWriter to a file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static StreamWriter WriteTextFile(string path)
        {
            try
            {
                FileStream file = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter obj = new StreamWriter(file);
                return obj;
            }
            catch (Exception ex)
            {
                Mailer.ErrNotify(ex, "Files writeTextFile");
                return null;
            }
        }

        /// <summary>
        /// method for adding new data to the beginning of a file
        /// </summary>
        /// <param name="file">the file we're adding to</param>
        /// <param name="newValue">the value we want to write</param>
        private static void WriteToStartOfFile(string file, string newValue)
        {
            char[] buffer = new char[10000];

            string tempFile = file + ".tmp";
            File.Move(file, tempFile);

            using (StreamReader reader = new StreamReader(tempFile))
            {
                using (StreamWriter writer = new StreamWriter(file, false))
                {
                    writer.Write(newValue);

                    int totalRead;
                    while ((totalRead = reader.Read(buffer, 0, buffer.Length)) > 0)
                        writer.Write(buffer, 0, totalRead);
                }

                File.Delete(tempFile);
            }
        }

        /// <summary>
        /// Compresses byte array to new byte array.
        /// </summary>
        public static byte[] CompressGZip(byte[] raw)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(memory, CompressionMode.Compress, true))
                {
                    gzip.Write(raw, 0, raw.Length);
                }
                return memory.ToArray();
            }
        }

        /// <summary>
        /// Decompress byte array to new byte array
        /// </summary>
        /// <param name="gzip"></param>
        /// <returns></returns>
        public static byte[] DecompressGZip(byte[] gzip)
        {
            // Create a GZIP stream with decompression mode.
            // ... Then create a buffer and write into while reading from the GZIP stream.
            using (GZipStream stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress))
            {
                const int size = 4096;
                byte[] buffer = new byte[size];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);
                    return memory.ToArray();
                }
            }
        }

        /// <summary>
        ///  method for retrieving files from a directory using LINQ
        /// </summary>
        /// <param name="dir">directory we want searched</param>
        /// <param name="patterns">search patterns we're using example: "*.sys"</param>
        /// <returns>list of matching files</returns>
        public static List<string> GetFiles(string dir, List<string> patterns)
        {
            List<string> matches = new List<string>();

            //loop thorugh all extensions provided
            foreach (string pattern in patterns)
            {
                //use LINQ to get each file with the specified file type
                var matchingFiles = from file in Directory.GetFiles(dir, pattern)
                                    select file;

                //now add all files to our list
                matches.AddRange(matchingFiles);
            }

            return matches;
        }

        /// <summary>
        /// method for merging 2 files into a single file
        /// </summary>
        /// <param name="fileToAppendTo">the file to merge with</param>
        /// <param name="fileToAppend">the file to merge</param>
        /// <returns></returns>
        public static bool MergeFiles(string fileToAppendTo, string fileToAppend)
        {
            try
            {
                //open a file stream of the file that will be holding the 2nd file
                //appended to it
                using (var streamAppend = new FileStream(fileToAppendTo, FileMode.Append))
                {
                    //open a file stream for the file being appended
                    using (var streamOpen = new FileStream(fileToAppend, FileMode.Open))
                    {
                        //create byte array the size of the 2nd file
                        byte[] array = new byte[streamOpen.Length];
                        //read the file into the byte array
                        streamOpen.Read(array, 0, (int)streamOpen.Length);
                        //let's make sure the stream is writable
                        if (streamAppend.CanWrite)
                            //write the 2nd file to the first file
                            streamAppend.Write(array, 0, (int)streamOpen.Length);
                    }
                }
                //if we've made it this file all went well
                return true;
            }
            catch (IOException ex)
            {
                Mailer.ErrNotify(string.Format("Error merging files: {0}", ex.Message), "Merge Files Error");
                return false;
            }
        }

        /// <summary>
        /// Split File Per MB
        /// </summary>
        /// <param name="SourceFile">"C:\\Test\\Test.txt"</param>
        /// <param name="sizeInMB">Size of file in MB</param>
        /// <returns>Returns bool</returns>
        public static bool SplitFilePerMB(string SourceFile, int sizeInMB)
        {

            int maxFileSize = (sizeInMB * 1048576);
            byte[] buffer = new byte[maxFileSize];

            try
            {
                FileStream fs = new FileStream(SourceFile, FileMode.Open, FileAccess.Read);
                //int SizeofEachFile = (int)Math.Ceiling((double)fs.Length / nNoofFiles);
                int i = 0;
                while (fs.Position < fs.Length)
                {
                    
                    string baseFileName = Path.GetFileNameWithoutExtension(SourceFile);
                    string Extension = Path.GetExtension(SourceFile);
                    FileStream outputFile = new FileStream(Path.GetDirectoryName(SourceFile) + "\\" + baseFileName + "." + i.ToString().PadLeft(5, Convert.ToChar("0")) + Extension + ".tmp", FileMode.Create, FileAccess.Write);
                    string mergeFolder = Path.GetDirectoryName(SourceFile);
                    int bytesRead = 0;
                    if ((bytesRead = fs.Read(buffer, 0, maxFileSize)) > 0)
                    {
                        outputFile.Write(buffer, 0, bytesRead);
                        //outp.Write(buffer, 0, BytesRead);
                        string packet = baseFileName + "." + i.ToString().PadLeft(3, Convert.ToChar("0")) + Extension.ToString();
                        List<string> Packets = new List<string>();
                        Packets.Add(packet);
                    }
                    outputFile.Close();
                    i += 1;
                }
                fs.Close();
            }
            catch (Exception Ex)
            {
                Mailer.ErrNotify( Ex.Message, "Files SplitFilePerMB");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Split File Per Num Of Files you select
        /// </summary>
        /// <param name="SourceFile">"C:\\Test\\Test.txt"</param>
        /// <param name="nNoofFiles">Number of files to divide</param>
        /// <returns>Returns bool</returns>
        public static bool SplitFilePerNumOfFiles(string SourceFile, int nNoofFiles)
        {
            try
            {
                FileStream fs = new FileStream(SourceFile, FileMode.Open, FileAccess.Read);
                int SizeofEachFile = (int)Math.Ceiling((double)fs.Length / nNoofFiles);
                for (int i = 0; i < nNoofFiles; i++)
                {
                    string baseFileName = Path.GetFileNameWithoutExtension(SourceFile);
                    string Extension = Path.GetExtension(SourceFile);
                    FileStream outputFile = new FileStream(Path.GetDirectoryName(SourceFile) + "\\" + baseFileName + "." +
                        i.ToString().PadLeft(5, Convert.ToChar("0")) + Extension + ".tmp", FileMode.Create, FileAccess.Write);
                    string mergeFolder = Path.GetDirectoryName(SourceFile);
                    int bytesRead = 0;
                    byte[] buffer = new byte[SizeofEachFile];
                    if ((bytesRead = fs.Read(buffer, 0, SizeofEachFile)) > 0)
                    {
                        outputFile.Write(buffer, 0, bytesRead);
                        //outp.Write(buffer, 0, BytesRead);
                        string packet = baseFileName + "." + i.ToString().PadLeft(3, Convert.ToChar("0")) + Extension.ToString();
                        List<string> Packets = new List<string>();
                        Packets.Add(packet);
                    }
                    outputFile.Close();
                }
                fs.Close();
            }
            catch (Exception Ex)
            {
                Mailer.ErrNotify(Ex.Message, "Files SplitFilePerMB");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Merge Multiple Files in one
        /// </summary>
        /// <param name="imputFolderName">Path to the folder eg:"C:\\Test\\"</param>
        /// <param name="saveFileFolder">Path to save folder eg:"C:\\"</param>
        /// <param name="deleteTemp">Option to delete files after they merge</param>
        /// <returns>Bool value</returns>
        public static bool MergeFile(string imputFolderName, string saveFileFolder, bool deleteTemp)
        {
            bool Output = false;
            try
            {
                string[] tmpfiles = Directory.GetFiles(imputFolderName, "*.tmp");
                FileStream outPutFile = null;
                string PrevFileName = "";
                foreach (string tempFile in tmpfiles)
                {
                    string fileName = Path.GetFileNameWithoutExtension(tempFile);
                    string baseFileName = fileName.Substring(0, fileName.IndexOf(Convert.ToChar(".")));
                    string extension = Path.GetExtension(fileName);
                    if (!PrevFileName.Equals(baseFileName))
                    {
                        if (outPutFile != null)
                        {
                            outPutFile.Flush();
                            outPutFile.Close();
                        }
                        outPutFile = new FileStream(saveFileFolder + "\\" + baseFileName + extension, FileMode.OpenOrCreate, FileAccess.Write);
                    }
                    int bytesRead = 0;
                    byte[] buffer = new byte[1024];
                    FileStream inputTempFile = new FileStream(tempFile, FileMode.OpenOrCreate, FileAccess.Read);
                    while ((bytesRead = inputTempFile.Read(buffer, 0, 1024)) > 0)
                        outPutFile.Write(buffer, 0, bytesRead);
                    inputTempFile.Close();
                    if (deleteTemp)
                    {
                        File.Delete(tempFile);
                    }
                    PrevFileName = baseFileName;
                }
                outPutFile.Close();
            }
            catch
            {
            }
            return Output;
        }

        /// <summary>
        ///over loaded version where extension doesn't thave to be .tmp
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static string UniqueFileName(string extension)
        {
            Guid myGuid = Guid.NewGuid();
            string tPath = Path.GetTempPath();
            tPath = Path.Combine(tPath, myGuid.ToString() + extension);
            return tPath;
        }

        /// <summary>
        /// Replaces filename ilegal caracters with underscore 
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string GetSafeFilename(string filename)
        {
            foreach (char invalid in Path.GetInvalidFileNameChars())
            {
                filename = filename.Replace(invalid, '_');
            }
            return filename;
        }

        /// <summary>
        /// Replaces pathname ilegal caracters with underscore 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetSafePathname(string path)
        {
            foreach (char invalid in Path.GetInvalidPathChars())
            {
                path = path.Replace(invalid, '_');
            }
            return path;
        }

        /// <summary>
        /// inserts line into a file at given position (line number)
        /// </summary>
        /// <param name="fName"></param>
        /// <param name="lineNumber"></param>
        /// <param name="lineX"></param>
        public static void PutLineAt(string fName, int lineNumber, string lineX)
        {
            string strTextFileName = fName;
            int iInsertAtLineNumber = lineNumber;
            string strTextToInsert = lineX;
            System.Collections.ArrayList lines = new System.Collections.ArrayList();
            StreamReader rdr = new StreamReader(
                strTextFileName);
            string line;
            while ((line = rdr.ReadLine()) != null)
            {
                lines.Add(line);
            }

            rdr.Close();
            if (lines.Count > iInsertAtLineNumber)
            {
                lines.Insert(iInsertAtLineNumber,
                   strTextToInsert);
            }
            else
            {
                lines.Add(strTextToInsert);
            }
            StreamWriter wrtr = new StreamWriter(
                strTextFileName);
            foreach (string strNewLine in lines)
                wrtr.WriteLine(strNewLine);
            wrtr.Close();
        }

        /// <summary>
        /// Verifies the file is corect size or hash
        /// </summary>
        /// <param name="file"></param>
        /// <param name="length"></param>
        /// <param name="md5">Optional</param>
        /// <param name="sha1">Optional</param>
        /// <param name="crc32">Optional</param>
        /// <returns></returns>
        public static bool ValidateFile(string file, long length, string md5 = "", string sha1 = "", string crc32 = "")
        {
            if (string.IsNullOrEmpty(file) || !File.Exists(file))
            {
                return false;
            }
            if (length > -1L)
            {
                StreamReader sr = new StreamReader(file);
                bool failed = false;
                if (sr.BaseStream.Length != length)
                {
                    failed = true;
                }
                sr.Close();
                sr.Dispose();
                if (failed)
                {
                    return false;
                }
            }
            if (!string.IsNullOrEmpty(md5))
            {
                if (md5.Length != 0x20)
                {
                    throw new Exception("MD5 hash was not of the correct length (32)");
                }
                if (!md5.Equals(GetMD5Hash(file), StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }
            if (!string.IsNullOrEmpty(sha1))
            {
                if (sha1.Length != 40)
                {
                    throw new Exception("SHA1 hash was not of the correct length (40)");
                }
                if (!sha1.Equals(GetSHA1Hash(file), StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }
            if (!string.IsNullOrEmpty(crc32) && (crc32.Length != 8))
            {
                throw new Exception("CRC32 hash was not of the correct length (8)");
            }
            return true;
        }

        /// <summary>
        /// Returns MIME type of given file extension
        /// </summary>
        /// <param name="fileExtension"></param>
        /// <returns></returns>
        public static string ReturnMIMEType(string fileExtension)
        {
            switch (fileExtension)
            {
                case ".ppt":
                    return "application/vnd.ms-powerpoint";
                case ".pptx":
                    return "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                case ".psd":
                    return "image/photoshop";
                case ".ps":
                case ".eps":
                case ".ai":
                    return "application/postscript";
                case ".txt":
                    return "text/plain";
                case ".doc":
                    return "application/ms-word";
                case ".docx":
                    return "pplication/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case ".tiff":
                case ".tif":
                    return "image/tiff";
                case ".zip":
                case ".7z":
                case ".sit":
                case ".sitx":
                    return "application/zip";
                case ".xls":
                case ".csv":
                case ".xlst":
                    return "application/vnd.ms-excel";
                case ".xlsx":
                    return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case ".gif":
                    return "image/gif";
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".bmp":
                    return "image/bmp";
                case "png":
                    return "image/png";
                case ".rtf":
                    return "application/rtf";
                case ".pdf":
                    return "application/pdf";
                case ".fdf":
                    return "application/vnd.fdf";
                case ".dwg":
                    return "image/vnd.dwg";
                case ".xdp":
                    return "application/vnd.adobe.xdp+xml";
                default:
                    return "application/octet-stream";
            }
        }

        /// <summary>
        /// Get MD5 Hash
        /// </summary>
        /// <param name="pathName"></param>
        /// <returns></returns>
        public static string GetMD5Hash(string pathName)
        {
            string strResult = "";
            FileStream oFileStream = null;
            MD5CryptoServiceProvider oMD5Hasher = new MD5CryptoServiceProvider();
            try
            {
                oFileStream = new FileStream(pathName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                byte[] arrbytHashValue = oMD5Hasher.ComputeHash(oFileStream);
                oFileStream.Close();
                strResult = BitConverter.ToString(arrbytHashValue).Replace("-", "");
            }
            catch (Exception)
            {
            }
            return strResult;
        }

        /// <summary>
        /// Get SHA1 Hash
        /// </summary>
        /// <param name="pathName"></param>
        /// <returns></returns>
        public static string GetSHA1Hash(string pathName)
        {
            string strResult = "";
            FileStream oFileStream = null;
            SHA1CryptoServiceProvider oSHA1Hasher = new SHA1CryptoServiceProvider();
            try
            {
                oFileStream = new FileStream(pathName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                byte[] arrbytHashValue = oSHA1Hasher.ComputeHash(oFileStream);
                oFileStream.Close();
                strResult = BitConverter.ToString(arrbytHashValue).Replace("-", "");
            }
            catch (Exception)
            {
            }
            return strResult;
        }

    }
}
