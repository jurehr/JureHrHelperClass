using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Globalization;
using System.Data;

namespace JureHR
{
    /// <summary>
    /// This class encodes and decodes Json strings.
    /// Spec. details, see http://www.json.org/
    /// 
    /// Json uses Arrays and Objects. These correspond here to the datatypes List of object and Hashtable.
    /// All numbers are parsed to doubles.
    /// </summary>
    public class Json
    {
        #region Public Static Methods

        /// <summary>
        /// Parses the string json into a value
        /// </summary>
        /// <param name="json">A Json string.</param>
        /// <returns>An Listof object, a Hashtable, a double, a string, null, true, or false</returns>
        public static object JsonDecode(string json)
        {
            // Save the string for debug information
            Json.instance.lastDecodedJson = json;

            if (json != null)
            {
                char[] charArray = json.ToCharArray();
                int index = 0;
                bool success = true;
                object value = Json.instance.ParseValue(charArray, ref index, ref success);
                if (success)
                {
                    Json.instance.lastErrorIndex = -1;
                }
                else
                {
                    Json.instance.lastErrorIndex = index;
                }
                return value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Converts a Hashtable / List of object object into a Json string
        /// </summary>
        /// <param name="json">A Hashtable / List of object</param>
        /// <returns>A Json encoded string, or null if object 'json' is not serializable</returns>
        public static string JsonEncode(object json)
        {
            StringBuilder builder = new StringBuilder(BUILDER_CAPACITY);
            bool success = Json.instance.SerializeValue(json, builder);
            return (success ? builder.ToString() : null);
        }

        /// <summary>
        /// On decoding, this function returns the position at which the parse failed (-1 = no error).
        /// </summary>
        /// <returns></returns>
        public static bool LastDecodeSuccessful()
        {
            return (Json.instance.lastErrorIndex == -1);
        }

        /// <summary>
        /// On decoding, this function returns the position at which the parse failed (-1 = no error).
        /// </summary>
        /// <returns></returns>
        public static int GetLastErrorIndex()
        {
            return Json.instance.lastErrorIndex;
        }

        /// <summary>
        /// If a decoding error occurred, this function returns a piece of the Json string 
        /// at which the error took place. To ease debugging.
        /// </summary>
        /// <returns></returns>
        public static string GetLastErrorSnippet()
        {
            if (Json.instance.lastErrorIndex == -1)
            {
                return "";
            }
            else
            {
                int startIndex = Json.instance.lastErrorIndex - 5;
                int endIndex = Json.instance.lastErrorIndex + 15;
                if (startIndex < 0)
                {
                    startIndex = 0;
                }
                if (endIndex >= Json.instance.lastDecodedJson.Length)
                {
                    endIndex = Json.instance.lastDecodedJson.Length - 1;
                }

                return Json.instance.lastDecodedJson.Substring(startIndex, endIndex - startIndex + 1);
            }
        }

        /// <summary>
        /// code to convert DataTable to JSON
        /// </summary>
        /// <param name="table"></param>
        /// <param name="stringName"></param>
        /// <returns>JSON string</returns>
        public static string MakeJsonOfDataTable(DataTable table, params string[] stringName)
        {
            StringBuilder sb = new StringBuilder();
            foreach (DataRow dr in table.Rows)
            {
                if (sb.Length != 0)
                    sb.Append(",");
                sb.Append("{");
                StringBuilder sb2 = new StringBuilder();
                foreach (DataColumn col in table.Columns)
                {
                    string fieldname = col.ColumnName;
                    object fieldvalue = dr[fieldname];
                    if (sb2.Length != 0) sb2.Append(",");
                    {
                        if ((fieldvalue is int) || (fieldvalue is double) || (fieldvalue is float))
                        {
                            sb2.Append(string.Format("\"{0}\":{1}", fieldname, fieldvalue));
                        }
                        if ((fieldvalue is string) || (fieldvalue is float))
                        {
                            sb2.Append(string.Format("\"{0}\":\"{1}\"", fieldname, fieldvalue));
                        }
                    }
                }
                sb.Append(sb2.ToString());
                sb.Append("}");
            }

            string tableName = "";

            if (stringName.Length > 0)
            {
                tableName = "{\"" + stringName[0] + "\":";
            }
            else if (table.TableName != string.Empty)
            {
                tableName = "{\"" + table.TableName + "\":";
            }
            else if ((table.TableName == string.Empty) && (stringName.Length == 0))
            {
                //tableName = "{\"json\":";
            }

            sb.Insert(0, tableName + "[");
            sb.Append("]");
            return sb.ToString();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Parse Object
        /// </summary>
        /// <param name="json"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected Hashtable ParseObject(char[] json, ref int index)
        {
            Hashtable table = new Hashtable();
            int token;

            // {
            NextToken(json, ref index);

            bool done = false;
            while (!done)
            {
                token = LookAhead(json, index);
                if (token == Json.TOKEN_NONE)
                {
                    return null;
                }
                else if (token == Json.TOKEN_COMMA)
                {
                    NextToken(json, ref index);
                }
                else if (token == Json.TOKEN_CURLY_CLOSE)
                {
                    NextToken(json, ref index);
                    return table;
                }
                else
                {

                    // name
                    string name = ParseString(json, ref index);
                    if (name == null)
                    {
                        return null;
                    }

                    // :
                    token = NextToken(json, ref index);
                    if (token != Json.TOKEN_COLON)
                    {
                        return null;
                    }

                    // value
                    bool success = true;
                    object value = ParseValue(json, ref index, ref success);
                    if (!success)
                    {
                        return null;
                    }

                    table[name] = value;
                }
            }

            return table;
        }

        /// <summary>
        /// ParseArray
        /// </summary>
        /// <param name="json"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected List<object> ParseArray(char[] json, ref int index)
        {
            List<object> array = new List<object>();

            // [
            NextToken(json, ref index);

            bool done = false;
            while (!done)
            {
                int token = LookAhead(json, index);
                if (token == Json.TOKEN_NONE)
                {
                    return null;
                }
                else if (token == Json.TOKEN_COMMA)
                {
                    NextToken(json, ref index);
                }
                else if (token == Json.TOKEN_SQUARED_CLOSE)
                {
                    NextToken(json, ref index);
                    break;
                }
                else
                {
                    bool success = true;
                    object value = ParseValue(json, ref index, ref success);
                    if (!success)
                    {
                        return null;
                    }

                    array.Add(value);
                }
            }

            return array;
        }

        /// <summary>
        /// ParseValue
        /// </summary>
        /// <param name="json"></param>
        /// <param name="index"></param>
        /// <param name="success"></param>
        /// <returns></returns>
        protected object ParseValue(char[] json, ref int index, ref bool success)
        {
            switch (LookAhead(json, index))
            {
                case Json.TOKEN_STRING:
                    return ParseString(json, ref index);
                case Json.TOKEN_NUMBER:
                    return ParseNumber(json, ref index);
                case Json.TOKEN_CURLY_OPEN:
                    return ParseObject(json, ref index);
                case Json.TOKEN_SQUARED_OPEN:
                    return ParseArray(json, ref index);
                case Json.TOKEN_TRUE:
                    NextToken(json, ref index);
                    return Boolean.Parse("TRUE");
                case Json.TOKEN_FALSE:
                    NextToken(json, ref index);
                    return Boolean.Parse("FALSE");
                case Json.TOKEN_NULL:
                    NextToken(json, ref index);
                    return null;
                case Json.TOKEN_NONE:
                    break;
            }

            success = false;
            return null;
        }

        /// <summary>
        /// ParseString
        /// </summary>
        /// <param name="json"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected string ParseString(char[] json, ref int index)
        {
            string s = "";
            char c;

            EatWhitespace(json, ref index);

            // "
            c = json[index++];

            bool complete = false;
            while (!complete)
            {

                if (index == json.Length)
                {
                    break;
                }

                c = json[index++];
                if (c == '"')
                {
                    complete = true;
                    break;
                }
                else if (c == '\\')
                {

                    if (index == json.Length)
                    {
                        break;
                    }
                    c = json[index++];
                    if (c == '"')
                    {
                        s += '"';
                    }
                    else if (c == '\\')
                    {
                        s += '\\';
                    }
                    else if (c == '/')
                    {
                        s += '/';
                    }
                    else if (c == 'b')
                    {
                        s += '\b';
                    }
                    else if (c == 'f')
                    {
                        s += '\f';
                    }
                    else if (c == 'n')
                    {
                        s += '\n';
                    }
                    else if (c == 'r')
                    {
                        s += '\r';
                    }
                    else if (c == 't')
                    {
                        s += '\t';
                    }
                    else if (c == 'u')
                    {
                        int remainingLength = json.Length - index;
                        if (remainingLength >= 4)
                        {
                            // fetch the next 4 chars
                            char[] unicodeCharArray = new char[4];
                            Array.Copy(json, index, unicodeCharArray, 0, 4);
                            // parse the 32 bit hex into an integer codepoint
                            uint codePoint = UInt32.Parse(new string(unicodeCharArray), NumberStyles.HexNumber);
                            // convert the integer codepoint to a unicode char and add to string
                            s += Char.ConvertFromUtf32((int)codePoint);
                            // skip 4 chars
                            index += 4;
                        }
                        else
                        {
                            break;
                        }
                    }

                }
                else
                {
                    s += c;
                }

            }

            if (!complete)
            {
                return null;
            }

            return s;
        }

        /// <summary>
        /// ParseNumber
        /// </summary>
        /// <param name="json"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected double ParseNumber(char[] json, ref int index)
        {
            EatWhitespace(json, ref index);

            int lastIndex = GetLastIndexOfNumber(json, index);
            int charLength = (lastIndex - index) + 1;
            char[] numberCharArray = new char[charLength];

            Array.Copy(json, index, numberCharArray, 0, charLength);
            index = lastIndex + 1;
            return Double.Parse(new string(numberCharArray), CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// GetLastIndexOfNumber
        /// </summary>
        /// <param name="json"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected int GetLastIndexOfNumber(char[] json, int index)
        {
            int lastIndex;
            for (lastIndex = index; lastIndex < json.Length; lastIndex++)
            {
                if ("0123456789+-.eE".IndexOf(json[lastIndex]) == -1)
                {
                    break;
                }
            }
            return lastIndex - 1;
        }

        /// <summary>
        /// EatWhitespace
        /// </summary>
        /// <param name="json"></param>
        /// <param name="index"></param>
        protected void EatWhitespace(char[] json, ref int index)
        {
            for (; index < json.Length; index++)
            {
                if (" \t\n\r".IndexOf(json[index]) == -1)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// LookAhead
        /// </summary>
        /// <param name="json"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected int LookAhead(char[] json, int index)
        {
            int saveIndex = index;
            return NextToken(json, ref saveIndex);
        }

        /// <summary>
        /// NextToken
        /// </summary>
        /// <param name="json"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected int NextToken(char[] json, ref int index)
        {
            EatWhitespace(json, ref index);

            if (index == json.Length)
            {
                return Json.TOKEN_NONE;
            }

            char c = json[index];
            index++;
            switch (c)
            {
                case '{':
                    return Json.TOKEN_CURLY_OPEN;
                case '}':
                    return Json.TOKEN_CURLY_CLOSE;
                case '[':
                    return Json.TOKEN_SQUARED_OPEN;
                case ']':
                    return Json.TOKEN_SQUARED_CLOSE;
                case ',':
                    return Json.TOKEN_COMMA;
                case '"':
                    return Json.TOKEN_STRING;
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case '-':
                    return Json.TOKEN_NUMBER;
                case ':':
                    return Json.TOKEN_COLON;
            }
            index--;

            int remainingLength = json.Length - index;

            // false
            if (remainingLength >= 5)
            {
                if (json[index] == 'f' &&
                    json[index + 1] == 'a' &&
                    json[index + 2] == 'l' &&
                    json[index + 3] == 's' &&
                    json[index + 4] == 'e')
                {
                    index += 5;
                    return Json.TOKEN_FALSE;
                }
            }

            // true
            if (remainingLength >= 4)
            {
                if (json[index] == 't' &&
                    json[index + 1] == 'r' &&
                    json[index + 2] == 'u' &&
                    json[index + 3] == 'e')
                {
                    index += 4;
                    return Json.TOKEN_TRUE;
                }
            }

            // null
            if (remainingLength >= 4)
            {
                if (json[index] == 'n' &&
                    json[index + 1] == 'u' &&
                    json[index + 2] == 'l' &&
                    json[index + 3] == 'l')
                {
                    index += 4;
                    return Json.TOKEN_NULL;
                }
            }

            return Json.TOKEN_NONE;
        }

        /// <summary>
        /// SerializeObjectOrArray
        /// </summary>
        /// <param name="objectOrArray"></param>
        /// <param name="builder"></param>
        /// <returns></returns>
        protected bool SerializeObjectOrArray(object objectOrArray, StringBuilder builder)
        {
            if (objectOrArray is Hashtable)
            {
                return SerializeObject((Hashtable)objectOrArray, builder);
            }
            else if (objectOrArray is List<object>)
            {
                return SerializeArray((List<object>)objectOrArray, builder);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// SerializeObject
        /// </summary>
        /// <param name="anObject"></param>
        /// <param name="builder"></param>
        /// <returns></returns>
        protected bool SerializeObject(Hashtable anObject, StringBuilder builder)
        {
            builder.Append("{");

            IDictionaryEnumerator e = anObject.GetEnumerator();
            bool first = true;
            while (e.MoveNext())
            {
                string key = e.Key.ToString();
                object value = e.Value;

                if (!first)
                {
                    builder.Append(", ");
                }

                SerializeString(key, builder);
                builder.Append(":");
                if (!SerializeValue(value, builder))
                {
                    return false;
                }

                first = false;
            }

            builder.Append("}");
            return true;
        }

        /// <summary>
        /// SerializeArray
        /// </summary>
        /// <param name="anArray"></param>
        /// <param name="builder"></param>
        /// <returns></returns>
        protected bool SerializeArray(List<object> anArray, StringBuilder builder)
        {
            builder.Append("[");

            bool first = true;
            for (int i = 0; i < anArray.Count; i++)
            {
                object value = anArray[i];

                if (!first)
                {
                    builder.Append(", ");
                }

                if (!SerializeValue(value, builder))
                {
                    return false;
                }

                first = false;
            }

            builder.Append("]");
            return true;
        }

        /// <summary>
        /// SerializeValue
        /// </summary>
        /// <param name="value"></param>
        /// <param name="builder"></param>
        /// <returns></returns>
        protected bool SerializeValue(object value, StringBuilder builder)
        {
            if (value is string)
            {
                SerializeString((string)value, builder);
            }
            else if (value is Hashtable)
            {
                SerializeObject((Hashtable)value, builder);
            }
            else if (value is List<object>)
            {
                SerializeArray((List<object>)value, builder);
            }
            else if (IsNumeric(value))
            {
                SerializeNumber(Convert.ToDouble(value), builder);
            }
            else if ((value is Boolean) && ((Boolean)value == true))
            {
                builder.Append("true");
            }
            else if ((value is Boolean) && ((Boolean)value == false))
            {
                builder.Append("false");
            }
            else if (value == null)
            {
                builder.Append("null");
            }
            else
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// SerializeString
        /// </summary>
        /// <param name="aString"></param>
        /// <param name="builder"></param>
        protected void SerializeString(string aString, StringBuilder builder)
        {
            builder.Append("\"");

            char[] charArray = aString.ToCharArray();
            for (int i = 0; i < charArray.Length; i++)
            {
                char c = charArray[i];
                if (c == '"')
                {
                    builder.Append("\\\"");
                }
                else if (c == '\\')
                {
                    builder.Append("\\\\");
                }
                else if (c == '\b')
                {
                    builder.Append("\\b");
                }
                else if (c == '\f')
                {
                    builder.Append("\\f");
                }
                else if (c == '\n')
                {
                    builder.Append("\\n");
                }
                else if (c == '\r')
                {
                    builder.Append("\\r");
                }
                else if (c == '\t')
                {
                    builder.Append("\\t");
                }
                else
                {
                    int codepoint = Convert.ToInt32(c);
                    if ((codepoint >= 32) && (codepoint <= 126))
                    {
                        builder.Append(c);
                    }
                    else
                    {
                        builder.Append("\\u" + Convert.ToString(codepoint, 16).PadLeft(4, '0'));
                    }
                }
            }

            builder.Append("\"");
        }

        /// <summary>
        /// SerializeNumber
        /// </summary>
        /// <param name="number"></param>
        /// <param name="builder"></param>
        protected void SerializeNumber(double number, StringBuilder builder)
        {
            builder.Append(Convert.ToString(number, CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Determines if a given object is numeric in any way
        /// (can be integer, double, etc). C# has no pretty way to do this.
        /// </summary>
        protected bool IsNumeric(object o)
        {
            try
            {
                Double.Parse(o.ToString());
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        #endregion

        #region Private Members

        /// <summary>
        /// On decoding, this value holds the position at which the parse failed (-1 = no error).
        /// </summary>
        protected int lastErrorIndex = -1;

        /// <summary>
        /// Stores the last string we tries to decode
        /// </summary>
        protected string lastDecodedJson = "";

        #endregion

        #region Private Static Members

        /// <summary>
        /// A static reference of the class
        /// </summary>
        protected static Json instance = new Json();

        #endregion

        #region Public Constants

        /// <summary>
        /// 0
        /// </summary>
        public const int TOKEN_NONE = 0;
        /// <summary>
        /// 1
        /// </summary>
        public const int TOKEN_CURLY_OPEN = 1;
        /// <summary>
        /// 2
        /// </summary>
        public const int TOKEN_CURLY_CLOSE = 2;
        /// <summary>
        /// 3
        /// </summary>
        public const int TOKEN_SQUARED_OPEN = 3;
        /// <summary>
        /// 4
        /// </summary>
        public const int TOKEN_SQUARED_CLOSE = 4;
        /// <summary>
        /// 5
        /// </summary>
        public const int TOKEN_COLON = 5;
        /// <summary>
        /// 6
        /// </summary>
        public const int TOKEN_COMMA = 6;
        /// <summary>
        /// 7
        /// </summary>
        public const int TOKEN_STRING = 7;
        /// <summary>
        /// 8
        /// </summary>
        public const int TOKEN_NUMBER = 8;
        /// <summary>
        /// 9
        /// </summary>
        public const int TOKEN_TRUE = 9;
        /// <summary>
        /// 10
        /// </summary>
        public const int TOKEN_FALSE = 10;
        /// <summary>
        /// 11
        /// </summary>
        public const int TOKEN_NULL = 11;

        #endregion

        #region Private Constants

        /// <summary>
        /// The maximum string length that this class can parse
        /// </summary>
        private const int BUILDER_CAPACITY = 2000;

        #endregion
    }
}
