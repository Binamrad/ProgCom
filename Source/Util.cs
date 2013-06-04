using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//this class contains various utility functions that are needed in several places
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
        if (index >= 0)
        {
            s = s.Substring(0, index);
        }
        return s;
    }

    //cut a string before a specific sequence
    public static String cutStrBefore(String s, String end)
    {
        int index = s.IndexOf(end);
        if (index == -1)
        {
            return "";
        }
        index += end.Length;
        s = s.Substring(index, s.Length-index);
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
}
