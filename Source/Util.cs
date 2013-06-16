﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

//this class contains various utility functions that are needed in several places
namespace ProgCom
{
    static class Util
    {
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
        public static Int32[] strToInt32(String s)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(s);
            Int32[] final = new Int32[(bytes.Length >> 2) + 1];
            int bytePosition = 0;
            int i = 0;
            foreach (byte b in bytes) {
                final[i] |= (b << bytePosition);
                bytePosition += 8;
                if (bytePosition == 32) {
                    bytePosition = 0;
                    ++i;
                }
            }
            return final;
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
    }
}