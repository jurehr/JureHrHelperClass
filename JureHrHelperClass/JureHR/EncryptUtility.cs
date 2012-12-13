using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace JureHR
{
    /// <summary>
    /// Class Encrypt Utility for Data Encryption
    /// </summary>
    public static class EncryptUtility
    {
        private static readonly Random Random = new Random();  
       
        // can be any string
        #region Password Phrase
        
        private const string _passPhrase = "Pas5pr@se";          
        private static string tempPassPhrase;

        /// <summary>
        /// Password Phrase
        /// </summary>
        public static string PassPhrase 
        {
            get
            {
                if (String.IsNullOrEmpty(tempPassPhrase))
                {
                    return _passPhrase;
                }
                else
                {
                    return tempPassPhrase;
                }
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = _passPhrase;
                }
                tempPassPhrase = value;
            }
        }

        /// <summary>
        /// Generate Password Phrase
        /// </summary>
        /// <returns></returns>
        public static byte[] GeneratePassPhrase()
        {
            var passPhrase = new byte[16];
            Random.NextBytes(passPhrase);
            return passPhrase;
        }

        
        #endregion

        // can be any string        
        #region Salt Value

        private const string _saltValue = "s@1tValue";           
        private static string tempSaltValue;

        /// <summary>
        /// Salt consists of random bits
        /// </summary>
        public static string SaltValue
        {
            get
            {
                if (String.IsNullOrEmpty(tempSaltValue))
                {
                    return _saltValue;
                }
                else
                {
                    return tempSaltValue;
                }
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = _saltValue;
                }
                tempSaltValue = value;
            }
        }

        /// <summary>
        /// Generate Salt consists of random bits
        /// </summary>
        /// <returns>byte[]</returns>
        public static byte[] GenerateSalt()
        {
            var salt = new byte[16];
            Random.NextBytes(salt);
            return salt;
        }

        #endregion

        // must be 16 bytes
        #region Initialization Vector

        private const string _initVector = "@1B2c3D4e5F6g7H8";   
        private static string tempInitVector;

        /// <summary>
        /// Initialization vector
        /// </summary>
        public static string InitVector
        {
            get
            {
                if (String.IsNullOrEmpty(tempInitVector))
                {
                    return _initVector;
                }
                else
                {
                    return tempInitVector;
                }
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = _initVector;
                }
                tempInitVector = value;
            }
        }

        /// <summary>
        /// Generate Initialization vector
        /// </summary>
        /// <param name="algorithm"></param>
        /// <returns>byte[]</returns>
        public static byte[] GenerateInitializationVector(SymmetricAlgorithm algorithm)
        {
            if (algorithm == null) Mailer.ErrNotify("algorithm", "GenerateInitializationVector");

            int size = algorithm.LegalBlockSizes[0].MinSize / 8;
            var initVector = new byte[size];
            Random.NextBytes(initVector);
            return initVector;
        }

        #endregion

        // can be 256, 192 or 128
        #region KeySize

        private const int _keySize = 256;
        private static int tempKeySize;

        /// <summary>
        /// KeySize
        /// </summary>
        public static int KeySize
        {
            get
            {
                if (tempKeySize == 0)
                {
                    return _keySize;
                }
                else
                {
                    return tempKeySize;
                }
            }
            set
            {
                if (value == 0)
                {
                    value = _keySize;
                }
                tempKeySize = value;
            }
        }

        #endregion

        // can be any number 
        #region Password Iterations

        private const int _passwordIterations = 1000;
        private static int tempPasswordIterations;

        /// <summary>
        /// Password Iterations
        /// </summary>
        public static int PasswordIterations
        {
            get
            {
                if (tempPasswordIterations == 0)
                {
                    return _passwordIterations;
                }
                else
                {
                    return tempPasswordIterations;
                }
            }
            set
            {
                if (value == 0)
                {
                    value = _passwordIterations;
                }
                tempPasswordIterations = value;
            }
        }

        #endregion

        /// <summary>
        /// Enum Algorithm Types
        /// </summary>
        public enum Algorithm
        {
            /// <summary>
            /// Advanced Encryption Standard
            /// </summary>
            AES,
            /// <summary>
            /// Data Encryption Standard
            /// </summary>
            DES,
            /// <summary>
            /// (also known as ARC2) is a block cipher designed by Ron Rivest 
            /// </summary>
            RC2,
            /// <summary>
            /// Rijndael symmetric encryption algorithm 
            /// </summary>
            RIJNDAEL,
            /// <summary>
            /// Triple Data Encryption Algorithm
            /// </summary>
            TRIPLE_DES
        }

        /// <summary>
        /// Chose what type of Encryption are you using
        /// </summary>
        /// <param name="Algorithm">Creates SymmetricAlgorithm</param>
        /// <returns></returns>
        private static SymmetricAlgorithm AlgorithmValue(Algorithm Algorithm)
        {
            switch (Algorithm)
            {
                case Algorithm.AES:
                    return Aes.Create();
                case Algorithm.DES:
                    return DES.Create();
                case Algorithm.RC2:
                    return RC2.Create();
                case Algorithm.RIJNDAEL:
                    return Rijndael.Create();
                case Algorithm.TRIPLE_DES:
                    return TripleDES.Create();
            }
            return null;
        }                               
        
        /// <summary>
        /// Encrypt string using MD5. There is no decryption.
        /// </summary>
        /// <param name="data">String to decrypt</param>
        /// <returns>Encrypted string</returns>
        public static string EncryptMD5(string data)
        {
            byte[] result;
            MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider();

            result = provider.ComputeHash(Encoding.Default.GetBytes(data));
            StringBuilder sBuilder = new StringBuilder();

            for (int i = 0; i < result.Length; i++)
            {
                sBuilder.Append(result[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        /// <summary>
        /// Encrypt string using MD5 using SHA1.
        /// </summary>
        /// <param name="plainText">String to decrypt</param>
        /// <returns>Encrypted string</returns>
        public static string EncryptRSA(string plainText)
        {
            byte[] initVectorBytes = Encoding.ASCII.GetBytes(InitVector);
            byte[] saltValueBytes = Encoding.ASCII.GetBytes(SaltValue);

            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            PasswordDeriveBytes password = new PasswordDeriveBytes(PassPhrase, saltValueBytes, "SHA1", PasswordIterations);

            byte[] keyBytes = password.GetBytes(KeySize / 8);

            RijndaelManaged symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;

            ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);

            MemoryStream memoryStream = new MemoryStream();

            CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);

            cryptoStream.FlushFinalBlock();

            byte[] cipherTextBytes = memoryStream.ToArray();

            memoryStream.Close();
            cryptoStream.Close();

            string cipherText = Convert.ToBase64String(cipherTextBytes);
            return cipherText;
        }

        /// <summary>
        /// Load RSA Key
        /// 
        /// Key generation  
        ///
        ///1) Choose two large random prime numbers P and Q of similar length.  
        ///
        ///2) Compute N = P x Q. N is the modulus for both the Public and Private keys.
        ///
        ///3) PSI = (P-1)(Q-1) , PSI is also called the Euler's totient function. 
        ///
        ///4) Choose an integer E, such that 1 less than E less than PSI, making sure that E and PSI are co-prime.  E is the Public key exponent.
        ///
        ///5) Calculate D = E-1 ( mod PSI ) , normally using Extended Euclidean algorithm. D is the Private key exponent. 
        /// </summary>
        /// <param name="D">privateExponent : This one's easy -- it contains d, the private exponent</param>
        /// <param name="DP">exponent1 : d mod (p - 1)</param>
        /// <param name="DQ">exponent2 : d mod (q - 1)</param>
        /// <param name="Exponent">publicExponent : e, the public exponent</param>
        /// <param name="InverseQ">coefficient : (InverseQ)(q) = 1 mod p</param>
        /// <param name="Modulus">modulus : n</param>
        /// <param name="P">prime1 : Also self-explantory, p</param>
        /// <param name="Q">prime2 : q</param>
        /// <returns></returns>
        public static RSAParameters LoadKey(string D, string DP, string DQ, string Exponent, string InverseQ, string Modulus, string P, string Q)
        {
            return new RSAParameters { D = Convert.FromBase64String(D), DP = Convert.FromBase64String(DP), DQ = Convert.FromBase64String(DQ), Exponent = Convert.FromBase64String(Exponent), InverseQ = Convert.FromBase64String(InverseQ), Modulus = Convert.FromBase64String(Modulus), P = Convert.FromBase64String(P), Q = Convert.FromBase64String(Q) };
        }

        /// <summary>
        /// Decrypt string using MD5 using SHA1.
        /// </summary>
        /// <param name="cipherText">String to encryp</param>
        /// <returns>Decrypted string</returns>
        public static string DecryptRSA(string cipherText)
        {
            byte[] initVectorBytes = Encoding.ASCII.GetBytes(InitVector);
            byte[] saltValueBytes = Encoding.ASCII.GetBytes(SaltValue);

            byte[] cipherTextBytes = Convert.FromBase64String(cipherText);
            PasswordDeriveBytes password = new PasswordDeriveBytes(PassPhrase, saltValueBytes, "SHA1", PasswordIterations);

            byte[] keyBytes = password.GetBytes(KeySize / 8);

            RijndaelManaged symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;

            ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);

            MemoryStream memoryStream = new MemoryStream(cipherTextBytes);
            CryptoStream cryptoStream = new CryptoStream(
                memoryStream, decryptor, CryptoStreamMode.Read);

            byte[] plainTextBytes = new byte[cipherTextBytes.Length];
            int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
            memoryStream.Close();
            cryptoStream.Close();
            string plainText = Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
            return plainText;
        }

        /// <summary>
        /// Encrypt File using any SymmetricAlgorithm encription
        /// </summary>
        /// <param name="algorithm">Algorithm enum</param>
        /// <param name="filePath">Path of the file</param>
        public static void EncryptFile(Algorithm algorithm, string filePath)
        {
            EncryptFile(algorithm, filePath, Encoding.UTF8.GetBytes(PassPhrase), Encoding.UTF8.GetBytes(SaltValue), Encoding.UTF8.GetBytes(InitVector));
        }
        
        /// <summary>
        /// Encrypt File using any SymmetricAlgorithm encription
        /// </summary>
        /// <param name="algorithm">Algorithm enum</param>
        /// <param name="filePath">Path of the file</param>
        /// <param name="PassPhrase">Byte Password Phrase</param>
        /// <param name="SaltValue">Byte Salt Value</param>
        /// <param name="InitVector">Byte Initialization Vector</param>
        public static void EncryptFile(Algorithm algorithm, string filePath, byte[] PassPhrase, byte[] SaltValue, byte[] InitVector)
        {
            if (String.IsNullOrEmpty(filePath)) Mailer.ErrNotify("File path is null or empty", "filePath");
            if (!File.Exists(filePath)) Mailer.ErrNotify("File does not exist", filePath);
            
            var fileBytes = File.ReadAllBytes(filePath);
            var encryptedBytes = EncryptBytes(algorithm, fileBytes);

            File.WriteAllBytes(filePath, encryptedBytes);
        }

        /// <summary>
        /// Encrypt Text using any SymmetricAlgorithm encription
        /// </summary>
        /// <param name="algorithm">Algorithm enum</param>
        /// <param name="text">String to Encrypt</param>
        /// <returns>Encrypted String</returns>
        public static string EncryptText(Algorithm algorithm, string text)
        {
            if (String.IsNullOrEmpty(text)) Mailer.ErrNotify("Text is null or empty", "text");

            var encryptedBytes = EncryptBytes(algorithm, Encoding.UTF8.GetBytes(text), Encoding.UTF8.GetBytes(PassPhrase), Encoding.UTF8.GetBytes(SaltValue), Encoding.UTF8.GetBytes(InitVector));
            return Convert.ToBase64String(encryptedBytes);
        }

        /// <summary>
        /// Encrypt Text using any SymmetricAlgorithm encription
        /// </summary>
        /// <param name="algorithm">Algorithm enum</param>
        /// <param name="text">String to Encrypt</param>
        /// <param name="PassPhrase">Byte Password Phrase</param>
        /// <param name="SaltValue">Byte Salt Value</param>
        /// <param name="InitVector">Byte Initialization Vector</param>
        /// <returns>Encrypted String</returns>
        public static string EncryptText(Algorithm algorithm, string text, byte[] PassPhrase, byte[] SaltValue, byte[] InitVector)
        {
            if (String.IsNullOrEmpty(text)) Mailer.ErrNotify("Text is null or empty", "text");

            var encryptedBytes = EncryptBytes(algorithm, Encoding.UTF8.GetBytes(text), PassPhrase, SaltValue, InitVector);
            return Convert.ToBase64String(encryptedBytes);
        }

        /// <summary>
        /// Encrypt Bytes using any SymmetricAlgorithm encription
        /// </summary>
        /// <param name="algorithm">Algorithm enum</param>
        /// <param name="encryptedData">Bytes to Encrypt</param>
        /// <returns>Encrypted Bytes</returns>
        public static byte[] EncryptBytes(Algorithm algorithm, byte[] encryptedData)
        {
            return EncryptBytes(algorithm, encryptedData, Encoding.UTF8.GetBytes(PassPhrase), Encoding.UTF8.GetBytes(SaltValue), Encoding.UTF8.GetBytes(InitVector));
        }

        /// <summary>
        /// Encrypt Text using any SymmetricAlgorithm encription
        /// </summary>
        /// <param name="_algorithm">Algorithm enum</param>
        /// <param name="data">String to Encrypt</param>
        /// <param name="PassPhrase">Byte Password Phrase</param>
        /// <param name="SaltValue">Byte Salt Value</param>
        /// <param name="InitVector">Byte Initialization Vector</param>
        /// <returns>Encrypted String</returns>
        public static byte[] EncryptBytes(Algorithm _algorithm, byte[] data, byte[] PassPhrase, byte[] SaltValue, byte[] InitVector)
        {
            SymmetricAlgorithm algorithm = AlgorithmValue(_algorithm);

            if (algorithm == null) Mailer.ErrNotify("algorithm", "EncryptBytes");
            if (data == null || data.Length == 0) Mailer.ErrNotify("Data are empty", "data");
            
            byte[] keyBytes;
            using (var rfc2898DeriveBytes = new Rfc2898DeriveBytes(PassPhrase, SaltValue, PasswordIterations))
            {
                keyBytes = rfc2898DeriveBytes.GetBytes(KeySize / 8);
            }

            byte[] encrypted;
            using (var encryptor = algorithm.CreateEncryptor(keyBytes, InitVector))
            {
                using (var memoryStream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(data, 0, data.Length);
                        cryptoStream.FlushFinalBlock();

                        encrypted = memoryStream.ToArray();

                        memoryStream.Close();
                        cryptoStream.Close();
                    }
                }
            }

            return encrypted;
        }

        /// <summary>
        /// Decrypt File using any SymmetricAlgorithm encription
        /// </summary>
        /// <param name="algorithm">Algorithm enum</param>
        /// <param name="filePath">Path of the file</param>
        public static void DecryptFile(Algorithm algorithm, string filePath)
        {
            DecryptFile(algorithm, filePath, Encoding.UTF8.GetBytes(PassPhrase), Encoding.UTF8.GetBytes(SaltValue), Encoding.UTF8.GetBytes(InitVector));
        }

        /// <summary>
        /// Decrypt File using any SymmetricAlgorithm encription
        /// </summary>
        /// <param name="algorithm">Algorithm enum</param>
        /// <param name="filePath">Path of the file</param>
        /// <param name="PassPhrase">Byte Password Phrase</param>
        /// <param name="SaltValue">Byte Salt Value</param>
        /// <param name="InitVector">Byte Initialization Vector</param>
        public static void DecryptFile(Algorithm algorithm, string filePath, byte[] PassPhrase, byte[] SaltValue, byte[] InitVector)
        {
            if (String.IsNullOrEmpty(filePath)) Mailer.ErrNotify("File path is null or empty", "filePath");
            if (!File.Exists(filePath)) Mailer.ErrNotify("File does not exist", filePath);
            
            var fileBytes = File.ReadAllBytes(filePath);
            var decryptedBytes = DecryptBytes(algorithm, fileBytes);

            File.WriteAllBytes(filePath, decryptedBytes);
        }

        /// <summary>
        /// Decrypt Text using any SymmetricAlgorithm encription
        /// </summary>
        /// <param name="algorithm">Algorithm enum</param>
        /// <param name="encryptedText">String to Encrypt</param>
        /// <returns>Decrypted String</returns>
        public static string DecryptText(Algorithm algorithm, string encryptedText)
        {
            if (String.IsNullOrEmpty(encryptedText)) Mailer.ErrNotify("Encrypted text are empty", "encryptedText");
            
            var decrypted = DecryptBytes(algorithm, Convert.FromBase64String(encryptedText), Encoding.UTF8.GetBytes(PassPhrase), Encoding.UTF8.GetBytes(SaltValue), Encoding.UTF8.GetBytes(InitVector));
            return Encoding.UTF8.GetString(decrypted);
        }

        /// <summary>
        /// Decrypt Text using any SymmetricAlgorithm encription
        /// </summary>
        /// <param name="algorithm">Algorithm enum</param>
        /// <param name="encryptedText">String to Encrypt</param>
        /// <param name="PassPhrase">Byte Password Phrase</param>
        /// <param name="SaltValue">Byte Salt Value</param>
        /// <param name="InitVector">Byte Initialization Vector</param>
        /// <returns>Decrypted String</returns>
        public static string DecryptText(Algorithm algorithm, string encryptedText, byte[] PassPhrase, byte[] SaltValue, byte[] InitVector)
        {
            if (String.IsNullOrEmpty(encryptedText)) Mailer.ErrNotify("Encrypted text are empty", "encryptedText");

            var decrypted = DecryptBytes(algorithm, Convert.FromBase64String(encryptedText), PassPhrase, SaltValue, InitVector);
            return Encoding.UTF8.GetString(decrypted);
        }

        /// <summary>
        /// Decrypt Bytes using any SymmetricAlgorithm encription
        /// </summary>
        /// <param name="algorithm">Algorithm enum</param>
        /// <param name="encryptedData">Bytes to Encrypt</param>
        /// <returns>Decrypted Bytes</returns>
        public static byte[] DecryptBytes(Algorithm algorithm, byte[] encryptedData)
        {
            return DecryptBytes(algorithm, encryptedData, Encoding.UTF8.GetBytes(PassPhrase), Encoding.UTF8.GetBytes(SaltValue), Encoding.UTF8.GetBytes(InitVector));
        }

        /// <summary>
        /// Decrypt Text using any SymmetricAlgorithm encription
        /// </summary>
        /// <param name="_algorithm">Algorithm enum</param>
        /// <param name="encryptedData">String to Encrypt</param>
        /// <param name="PassPhrase">Byte Password Phrase</param>
        /// <param name="SaltValue">Byte Salt Value</param>
        /// <param name="InitVector">Byte Initialization Vector</param>
        /// <returns>Decrypt String</returns>
        public static byte[] DecryptBytes(Algorithm _algorithm, byte[] encryptedData, byte[] PassPhrase, byte[] SaltValue, byte[] InitVector)
        {
            SymmetricAlgorithm algorithm = AlgorithmValue(_algorithm);

            if (algorithm == null) Mailer.ErrNotify("algorithm", "DecryptBytes");
            if (encryptedData == null || encryptedData.Length == 0) Mailer.ErrNotify("Encrypted data is null or empty", "encryptedData");
            
            byte[] keyBytes;
            using (var rfc2898DeriveBytes = new Rfc2898DeriveBytes(PassPhrase, SaltValue, PasswordIterations))
            {
                keyBytes = rfc2898DeriveBytes.GetBytes(KeySize / 8);
            }

            byte[] plainTextBytes;
            int decryptedBytesCount;
            using (var decryptor = algorithm.CreateDecryptor(keyBytes, InitVector))
            {
                using (var memoryStream = new MemoryStream(encryptedData))
                {
                    using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        plainTextBytes = new byte[encryptedData.Length];
                        decryptedBytesCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);

                        memoryStream.Close();
                        cryptoStream.Close();
                    }
                }
            }

            return plainTextBytes.Take(decryptedBytesCount).ToArray();
        }

        /// <summary>
        /// Convert byte array to base64 string
        /// </summary>
        /// <param name="inputBytes"></param>
        /// <returns></returns>
        public static string ConvertToBase64(byte[] inputBytes)
        {
            string sOutput;
            sOutput = Convert.ToBase64String(inputBytes, 0, inputBytes.Length);
            return sOutput;
        }

        /// <summary>
        /// Convert base64 string to byte array
        /// </summary>
        /// <param name="inputCharacters"></param>
        /// <returns></returns>
        public static byte[] ConvertFromBase64(string inputCharacters)
        {
            byte[] arrOutput;
            arrOutput = Convert.FromBase64String(inputCharacters);
            return arrOutput;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inFileName"></param>
        /// <returns></returns>
        public static string FileConvertToBase64(string inFileName)
        {
            string base64String = "";
            FileStream myInputFile = null;
            byte[] binaryData;

            try
            {
                try
                {
                    //				FromBase64Transform myTransform = new FromBase64Transform(FromBase64TransformMode.IgnoreWhiteSpaces);
                    //					byte[] myOutputBytes = new byte[myTransform.OutputBlockSize];
                    //					 myInputFile = new FileStream(inFileName, FileMode.Open, FileAccess.Read);
                    //					
                    //
                    //					byte[] myInputBytes = new byte[myInputFile.Length];
                    //					myInputFile.Read(myInputBytes, 0, myInputBytes.Length);
                    //					int i = 0;
                    //					while(myInputBytes.Length - i > 4/*myTransform.InputBlockSize*/)
                    //					{
                    //						myTransform.TransformBlock(myInputBytes, i, 4/*myTransform.InputBlockSize*/, myOutputBytes, 0);
                    //						i += 4/*myTransform.InputBlockSize*/;
                    //						//myOutputFile.Write(myOutputBytes, 0, myTransform.OutputBlockSize);
                    //					}
                    //					myOutputBytes = myTransform.TransformFinalBlock(myInputBytes, i, myInputBytes.Length - i);
                    //	OutString=ConvertToBase64(myOutputBytes);
                    //OutString= myInputBytes.ToString();
                    //myTransform.

                    myInputFile = new FileStream(inFileName, FileMode.Open, FileAccess.Read);
                    binaryData = new Byte[myInputFile.Length];
                    long bytesRead = myInputFile.Read(binaryData, 0, (int)myInputFile.Length);
                    base64String = System.Convert.ToBase64String(binaryData, 0, binaryData.Length);
                }
                catch (Exception ex)
                {
                    Mailer.ErrNotify(ex, "EncriptUtility FileConvertToBase64");
                }
            }
            finally
            {
                //	myTransform.Clear();
                myInputFile.Close();

            }
            return base64String;

        }

        /// <summary>
        /// Convert from Base64 to File
        /// </summary>
        /// <param name="inputBase64Char"></param>
        /// <param name="outputFileName"></param>
        public static void Base64ConvertToFile(string inputBase64Char, string outputFileName)
        {
            FileStream myOutputFile = null;
            //System.IO.StreamWriter outFile=null;

            try
            {
                try
                {
                    byte[] bits = Convert.FromBase64String(inputBase64Char);
                    //	outFile = new System.IO.StreamWriter(outputFileName,false,System.Text.Encoding.ASCII);
                    //	outFile.Write(base64String);

                    myOutputFile = new FileStream(outputFileName, FileMode.Create, FileAccess.ReadWrite);
                    myOutputFile.Write(bits, 0, bits.Length);
                }
                catch (Exception ex)
                {
                    Mailer.ErrNotify(ex, "EncriptUtility FileConvertToBase64");
                }
            }
            finally
            {
                myOutputFile.Close();
                // outFile.Close();
            }
        }

        /// <summary>
        /// Decode base64 from file
        /// </summary>
        /// <param name="inFileName"></param>
        /// <param name="outFileName"></param>
        public static void DecodeFromFileBase64(string inFileName, string outFileName)
        {
            FileStream myInputFile = null;
            FileStream myOutputFile = null;
            FromBase64Transform myTransform = null;
            try
            {
                try
                {
                    myTransform = new FromBase64Transform(FromBase64TransformMode.DoNotIgnoreWhiteSpaces);//FromBase64TransformMode.DoNotIgnoreWhiteSpaces

                    byte[] myOutputBytes = new byte[myTransform.OutputBlockSize];

                    //Open the input and output files.
                    myInputFile = new FileStream(inFileName, FileMode.Open, FileAccess.Read);
                    myOutputFile = new FileStream(outFileName, FileMode.Create, FileAccess.Write);

                    //Retrieve the file contents into a byte array.
                    byte[] myInputBytes = new byte[myInputFile.Length];
                    myInputFile.Read(myInputBytes, 0, myInputBytes.Length);

                    //Transform the data in chunks the size of InputBlockSize.
                    int i = 0;
                    while (myInputBytes.Length - i > 4/*myTransform.InputBlockSize*/)
                    {
                        myTransform.TransformBlock(myInputBytes, i, 4/*myTransform.InputBlockSize*/, myOutputBytes, 0);
                        i += 4/*myTransform.InputBlockSize*/;
                        myOutputFile.Write(myOutputBytes, 0, myTransform.OutputBlockSize);
                    }

                    //Transform the final block of data.
                    myOutputBytes = myTransform.TransformFinalBlock(myInputBytes, i, myInputBytes.Length - i);
                    myOutputFile.Write(myOutputBytes, 0, myOutputBytes.Length);
                }
                catch (Exception ex)
                {
                    Mailer.ErrNotify(ex, "EncriptUtility FileConvertToBase64");
                }
            }
            finally
            {

                //Free up any used resources.
                myTransform.Clear();

                myInputFile.Close();
                myOutputFile.Close();
            }
        }

        /// <summary>
        /// Encode base64 from file
        /// </summary>
        /// <param name="inFileName"></param>
        /// <param name="outFileName"></param>
        public static void EncodeFromFileBase64(string inFileName, string outFileName)
        {

            FileStream myInputFile = null;
            FileStream myOutputFile = null;
            ToBase64Transform myTransform = null;
            try
            {
                try
                {
                    myTransform = new ToBase64Transform();

                    byte[] myOutputBytes = new byte[myTransform.OutputBlockSize];

                    //Open the input and output files.
                    myInputFile = new FileStream(inFileName, FileMode.Open, FileAccess.Read);
                    myOutputFile = new FileStream(outFileName, FileMode.Create, FileAccess.Write);

                    //Retrieve the file contents into a byte array.
                    byte[] myInputBytes = new byte[myInputFile.Length];
                    myInputFile.Read(myInputBytes, 0, myInputBytes.Length);

                    //Transform the data in chunks the size of InputBlockSize.
                    int i = 0;
                    while (myInputBytes.Length - i > 4/*myTransform.InputBlockSize*/)
                    {
                        myTransform.TransformBlock(myInputBytes, i, 4/*myTransform.InputBlockSize*/, myOutputBytes, 0);
                        i += 4/*myTransform.InputBlockSize*/;
                        myOutputFile.Write(myOutputBytes, 0, myTransform.OutputBlockSize);
                    }

                    //Transform the final block of data.
                    myOutputBytes = myTransform.TransformFinalBlock(myInputBytes, i, myInputBytes.Length - i);
                    myOutputFile.Write(myOutputBytes, 0, myOutputBytes.Length);
                }
                catch (Exception ex)
                {
                    Mailer.ErrNotify(ex, "EncriptUtility FileConvertToBase64");
                }
            }
            finally
            {

                //Free up any used resources.
                myTransform.Clear();

                myInputFile.Close();
                myOutputFile.Close();
            }
        }

    }   
}
