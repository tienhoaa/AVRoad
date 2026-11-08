using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace AVRoad.Security
{
    /// <summary>
    /// Trình xử lý JSON siêu nhẹ, không phụ thuộc thư viện ngoài, tương thích 100% với .NET 2.0 / 3.5 / 4.0+.
    /// </summary>
    public static class SimpleJson
    {
        public static object Deserialize(string json)
        {
            if (string.IsNullOrEmpty(json)) return null;
            int index = 0;
            return ParseValue(json.Trim(), ref index);
        }

        public static Dictionary<string, object> DeserializeObject(string json)
        {
            object obj = Deserialize(json);
            return obj as Dictionary<string, object>;
        }

        public static List<object> DeserializeList(string json)
        {
            object obj = Deserialize(json);
            return obj as List<object>;
        }

        /// <summary>
        /// Trích xuất chuỗi JSON gốc của một thuộc tính (rất quan trọng cho việc xác thực chữ ký RSA).
        /// Nếu thuộc tính là chuỗi: trả về chuỗi đã unescape.
        /// Nếu thuộc tính là object/array: trả về đoạn raw substring nguyên bản.
        /// </summary>
        public static string ExtractRawProperty(string json, string propertyName)
        {
            if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(propertyName))
                return null;

            int idx = 0;
            string targetKey = "\"" + propertyName + "\"";
            while (true)
            {
                int keyPos = json.IndexOf(targetKey, idx, StringComparison.OrdinalIgnoreCase);
                if (keyPos < 0) return null;

                // Tìm dấu hai chấm ':'
                int colonPos = json.IndexOf(':', keyPos + targetKey.Length);
                if (colonPos < 0) return null;

                // Bỏ qua khoảng trắng sau ':'
                int valStart = colonPos + 1;
                while (valStart < json.Length && char.IsWhiteSpace(json[valStart]))
                    valStart++;

                if (valStart >= json.Length) return null;

                char firstChar = json[valStart];
                if (firstChar == '"')
                {
                    // Là chuỗi JSON -> trích xuất nội dung
                    int endStr = valStart + 1;
                    bool escaped = false;
                    while (endStr < json.Length)
                    {
                        if (json[endStr] == '\\' && !escaped)
                        {
                            escaped = true;
                            endStr++;
                            continue;
                        }
                        if (json[endStr] == '"' && !escaped)
                            break;
                        escaped = false;
                        endStr++;
                    }
                    if (endStr < json.Length)
                    {
                        string rawStr = json.Substring(valStart + 1, endStr - valStart - 1);
                        return UnescapeString(rawStr);
                    }
                }
                else if (firstChar == '{' || firstChar == '[')
                {
                    // Là Object hoặc Array -> tìm dấu đóng tương ứng
                    char openC = firstChar;
                    char closeC = (firstChar == '{') ? '}' : ']';
                    int depth = 0;
                    bool inStr = false;
                    bool esc = false;
                    for (int i = valStart; i < json.Length; i++)
                    {
                        char c = json[i];
                        if (inStr)
                        {
                            if (c == '\\' && !esc) esc = true;
                            else if (c == '"' && !esc) inStr = false;
                            else esc = false;
                        }
                        else
                        {
                            if (c == '"') inStr = true;
                            else if (c == openC) depth++;
                            else if (c == closeC)
                            {
                                depth--;
                                if (depth == 0)
                                {
                                    return json.Substring(valStart, i - valStart + 1);
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Giá trị nguyên thủy (số, bool, null)
                    int valEnd = valStart;
                    while (valEnd < json.Length && json[valEnd] != ',' && json[valEnd] != '}' && json[valEnd] != ']' && !char.IsWhiteSpace(json[valEnd]))
                    {
                        valEnd++;
                    }
                    return json.Substring(valStart, valEnd - valStart).Trim();
                }

                idx = keyPos + targetKey.Length;
            }
        }

        private static object ParseValue(string json, ref int index)
        {
            SkipWhitespace(json, ref index);
            if (index >= json.Length) return null;

            char c = json[index];
            if (c == '{') return ParseObject(json, ref index);
            if (c == '[') return ParseArray(json, ref index);
            if (c == '"') return ParseString(json, ref index);
            if (c == 't' || c == 'T') return ParseTrue(json, ref index);
            if (c == 'f' || c == 'F') return ParseFalse(json, ref index);
            if (c == 'n' || c == 'N') return ParseNull(json, ref index);
            return ParseNumber(json, ref index);
        }

        private static Dictionary<string, object> ParseObject(string json, ref int index)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            index++; // skip '{'

            while (index < json.Length)
            {
                SkipWhitespace(json, ref index);
                if (index >= json.Length) break;
                if (json[index] == '}') { index++; return dict; }

                if (json[index] == ',') { index++; continue; }

                string key = ParseString(json, ref index);
                SkipWhitespace(json, ref index);

                if (index < json.Length && json[index] == ':') index++; // skip ':'
                SkipWhitespace(json, ref index);

                object val = ParseValue(json, ref index);
                dict[key] = val;

                SkipWhitespace(json, ref index);
                if (index < json.Length && json[index] == ',') index++;
                else if (index < json.Length && json[index] == '}') { index++; return dict; }
            }
            return dict;
        }

        private static List<object> ParseArray(string json, ref int index)
        {
            List<object> list = new List<object>();
            index++; // skip '['

            while (index < json.Length)
            {
                SkipWhitespace(json, ref index);
                if (index >= json.Length) break;
                if (json[index] == ']') { index++; return list; }

                if (json[index] == ',') { index++; continue; }

                object val = ParseValue(json, ref index);
                list.Add(val);

                SkipWhitespace(json, ref index);
                if (index < json.Length && json[index] == ',') index++;
                else if (index < json.Length && json[index] == ']') { index++; return list; }
            }
            return list;
        }

        private static string ParseString(string json, ref int index)
        {
            SkipWhitespace(json, ref index);
            if (index >= json.Length || json[index] != '"') return string.Empty;

            index++; // skip open quote
            int start = index;
            StringBuilder sb = null;

            while (index < json.Length)
            {
                char c = json[index];
                if (c == '\\')
                {
                    if (sb == null) sb = new StringBuilder();
                    sb.Append(json, start, index - start);
                    index++;
                    if (index >= json.Length) break;

                    char esc = json[index];
                    switch (esc)
                    {
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case '/': sb.Append('/'); break;
                        case 'b': sb.Append('\b'); break;
                        case 'f': sb.Append('\f'); break;
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case 'u':
                            if (index + 4 < json.Length)
                            {
                                string hex = json.Substring(index + 1, 4);
                                int code;
                                if (int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out code))
                                {
                                    sb.Append((char)code);
                                }
                                index += 4;
                            }
                            break;
                        default:
                            sb.Append(esc);
                            break;
                    }
                    index++;
                    start = index;
                }
                else if (c == '"')
                {
                    if (sb == null)
                    {
                        string res = json.Substring(start, index - start);
                        index++;
                        return res;
                    }
                    sb.Append(json, start, index - start);
                    index++;
                    return sb.ToString();
                }
                else
                {
                    index++;
                }
            }
            return (sb != null) ? sb.ToString() : json.Substring(start);
        }

        private static object ParseNumber(string json, ref int index)
        {
            SkipWhitespace(json, ref index);
            int start = index;
            while (index < json.Length && (char.IsDigit(json[index]) || json[index] == '-' || json[index] == '+' || json[index] == '.' || json[index] == 'e' || json[index] == 'E'))
            {
                index++;
            }
            string numStr = json.Substring(start, index - start);
            long lVal;
            if (long.TryParse(numStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out lVal))
            {
                if (lVal >= int.MinValue && lVal <= int.MaxValue) return (int)lVal;
                return lVal;
            }
            double dVal;
            if (double.TryParse(numStr, NumberStyles.Float, CultureInfo.InvariantCulture, out dVal))
            {
                return dVal;
            }
            return numStr;
        }

        private static bool ParseTrue(string json, ref int index)
        {
            index += 4; // "true"
            return true;
        }

        private static bool ParseFalse(string json, ref int index)
        {
            index += 5; // "false"
            return false;
        }

        private static object ParseNull(string json, ref int index)
        {
            index += 4; // "null"
            return null;
        }

        private static void SkipWhitespace(string json, ref int index)
        {
            while (index < json.Length && char.IsWhiteSpace(json[index]))
                index++;
        }

        private static string UnescapeString(string str)
        {
            if (string.IsNullOrEmpty(str)) return string.Empty;
            int idx = 0;
            string wrapped = "\"" + str + "\"";
            return ParseString(wrapped, ref idx);
        }

        public static string Serialize(object obj)
        {
            return Serialize(obj, false);
        }

        public static string Serialize(object obj, bool indent)
        {
            StringBuilder sb = new StringBuilder();
            SerializeValue(obj, sb, indent ? 0 : -1);
            return sb.ToString();
        }

        private static void SerializeValue(object obj, StringBuilder sb, int indentLevel)
        {
            if (obj == null)
            {
                sb.Append("null");
                return;
            }

            if (obj is string)
            {
                sb.Append('"');
                string s = (string)obj;
                for (int i = 0; i < s.Length; i++)
                {
                    char c = s[i];
                    switch (c)
                    {
                        case '"': sb.Append("\\\""); break;
                        case '\\': sb.Append("\\\\"); break;
                        case '\b': sb.Append("\\b"); break;
                        case '\f': sb.Append("\\f"); break;
                        case '\n': sb.Append("\\n"); break;
                        case '\r': sb.Append("\\r"); break;
                        case '\t': sb.Append("\\t"); break;
                        default:
                            if (c < 32)
                            {
                                sb.AppendFormat("\\u{0:x4}", (int)c);
                            }
                            else
                            {
                                sb.Append(c);
                            }
                            break;
                    }
                }
                sb.Append('"');
                return;
            }

            if (obj is bool)
            {
                sb.Append(((bool)obj) ? "true" : "false");
                return;
            }

            if (obj is int || obj is long || obj is short || obj is byte)
            {
                sb.Append(Convert.ToString(obj, CultureInfo.InvariantCulture));
                return;
            }

            if (obj is double || obj is float || obj is decimal)
            {
                sb.Append(Convert.ToString(obj, CultureInfo.InvariantCulture));
                return;
            }

            if (obj is DateTime)
            {
                sb.Append('"').Append(((DateTime)obj).ToString("yyyy-MM-ddTHH:mm:ss.fffZ")).Append('"');
                return;
            }

            if (obj is IDictionary)
            {
                IDictionary dict = (IDictionary)obj;
                sb.Append('{');
                if (indentLevel >= 0) sb.AppendLine();

                bool first = true;
                foreach (DictionaryEntry de in dict)
                {
                    if (!first)
                    {
                        sb.Append(',');
                        if (indentLevel >= 0) sb.AppendLine();
                    }
                    first = false;

                    if (indentLevel >= 0) sb.Append(new string(' ', (indentLevel + 1) * 2));
                    SerializeValue(Convert.ToString(de.Key), sb, -1);
                    sb.Append(':');
                    if (indentLevel >= 0) sb.Append(' ');
                    SerializeValue(de.Value, sb, indentLevel >= 0 ? indentLevel + 1 : -1);
                }

                if (indentLevel >= 0)
                {
                    sb.AppendLine();
                    sb.Append(new string(' ', indentLevel * 2));
                }
                sb.Append('}');
                return;
            }

            if (obj is IEnumerable)
            {
                IEnumerable list = (IEnumerable)obj;
                sb.Append('[');
                bool first = true;
                foreach (object item in list)
                {
                    if (!first) sb.Append(',');
                    first = false;
                    SerializeValue(item, sb, indentLevel >= 0 ? indentLevel + 1 : -1);
                }
                sb.Append(']');
                return;
            }

            SerializeValue(obj.ToString(), sb, indentLevel);
        }
    }
}
