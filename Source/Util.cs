using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using KSP.IO;

//this class contains various utility functions that are needed in several places
namespace ProgCom
{
    static class Util
    {
        static System.Random rng;

        //bitwise copy float to integer
        public static Int32 ftoi(float f)
        {
            byte[] b = BitConverter.GetBytes(f);
            return BitConverter.ToInt32(b, 0);
        }

        //bitwise copy integer to float
        public static float itof(Int32 f)
        {
            byte[] b = BitConverter.GetBytes(f);
            return BitConverter.ToSingle(b, 0);
        }

        //cut a string after a specific sequence
        public static String cutStrAfter(String s, String end)
        {
            int index = s.IndexOf(end);
            if (index >= 0) {
                s = s.Substring(0, index);
            }
            return s;
        }

        //cut a string before a specific sequence
        public static String cutStrBefore(String s, String end)
        {
            int index = s.IndexOf(end);
            if (index == -1) {
                return "";
            }
            index += end.Length;
            s = s.Substring(index, s.Length - index);
            return s;
        }

        //convert a string to 32-bit packed ascii format
        public static Int32[] strToInt32(String s, bool compactStrings)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(s);
            Int32[] final = new Int32[(compactStrings ? bytes.Length >> 2 : bytes.Length) + 1];
            int bytePosition = 0;
            int i = 0;

            if (compactStrings) {
                foreach (byte b in bytes) {
                    final[i] |= (b << bytePosition);
                    bytePosition += 8;
                    if (bytePosition == 32) {
                        bytePosition = 0;
                        ++i;
                    }
                }
            } else {
                foreach (byte b in bytes) {
                    final[i] = b;
                    ++i;
                }
            }
            return final;
        }

        //fix \n, \t etc. chars in a string to their counterparts
        public static String fixEscapeChars(String s)
        {
            String finalStr = "";//this is lazy, but I'm not really worried about quadratic complexity in this case
            bool prevEsc = false;
            foreach (char c in s) {
                if(c == '\\' && prevEsc == false) {
                    prevEsc = true;
                } else if (prevEsc == true) {
                    prevEsc = false;
                    switch (c) {
                        case 'n':
                            finalStr += '\n';
                            break;
                        case 'a':
                            finalStr += '\a';
                            break;
                        case 't':
                            finalStr += '\t';
                            break;
                        case '0':
                            finalStr += '\0';
                            break;
                        case '\\':
                            finalStr += '\\';
                            break;
                        case '\"':
                            finalStr += '\"';
                            break;
                        case 'f':
                            finalStr += '\f';
                            break;
                        case 'r':
                            finalStr += '\r';
                            break;
                        default:
                            throw new FormatException("Unrecognized escape sequence: \"\\" + c + "\" in: " + s);
                    }
                } else {
                    finalStr += c;
                }
            }
            return finalStr;
        }

        //set the specified bit to value
        public static int setBit(Int32 i, int bitPosition, bool bitVal)
        {
            return setBit(i, bitPosition, bitVal ? 1 : 0);
        }

        public static int setBit(Int32 i, int bitPosition, int bitVal)
        {
            i -= i & (1 << bitPosition);
            i |= (bitVal << bitPosition);
            return i;
        }

        // Parse decimal, binary, or hex strings to a supplied type
        public static T parseTo<T>(String s)
        {
            Type resultType = typeof(T);
            String methodName = "To" + resultType.Name;
            MethodInfo conversionMethod;
            object[] arguments;

            if (object.Equals(resultType, typeof(Single)) || object.Equals(resultType, typeof(Double))) {
                // toSingle and toDouble don't have a second base argument, so we have to handle them separately
                conversionMethod = typeof(Convert).GetMethod(methodName, new[] { typeof(String) });
                arguments = new object[] { s };
            }
            else {
                // Integral result -- determine base and pass that into the convert function
                int numberBase = 10;

                if (s.StartsWith("0x")) {
                    numberBase = 16;
                    s = s.Remove(0, 2);
                }
                else if (s.StartsWith("0b")) {
                    numberBase = 2;
                    s = s.Remove(0, 2);
                }

                conversionMethod = typeof(Convert).GetMethod(methodName, new[] { typeof(String), typeof(Int32) });
                arguments = new object[] { s, numberBase };
            }

            // Invoke the conversion
            return (T)conversionMethod.Invoke(null, arguments);
        }

        // "try" version of the above, in the spirit of Int32.TryParse
        public static bool tryParseTo<T>(String s, out T result)
        {
            try {
                result = parseTo<T>(s);
            } catch (Exception) {
                result = default(T);
                return false;
            }

            return true;
        }

        //random number generator
        public static int random()
        {
            if (rng == null) {
                rng = new System.Random();
            }
            return rng.Next();
        }
    }
}