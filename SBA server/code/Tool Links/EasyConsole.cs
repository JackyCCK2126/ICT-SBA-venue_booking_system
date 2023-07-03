using System;

//tool version: 1

namespace PieTool{

    class EasyConsole
    {
        public static string readLine(string message = null)
        {
            if (message != null) print(message);
            return Console.ReadLine();
        }
        public static void print(object content = null, object name = null)
        {
            if (name != null) name = name + ": ";
            Console.WriteLine(name + "" + content);
        }
        public static void reprint(object content = null, object name = null)
        {
            if (name != null) name = name + ": ";
            Console.Write("\r{0}", name + "" + content);
        }

        public static void printAll(Array array, string title = null, string seperator = ", ")
        {
            string s = "";
            if (title != null) s = title + ": ";
            s += ArrToString(array);
            print(s);
        }
        public static string ArrToString(Array array, string seperator = ", ")
        {
            string s = "";
            int i = 0;
            foreach (var v in array)
            {
                if (i == array.Length - 1) seperator = null;
                s += v.ToString() + seperator;
                i++;
            }
            return s;
        }
        public static void PrintIndexAndValues(Array Arr)
        {
            for (int i = 0; i < Arr.Length; i++)
            {
                print(Arr.GetValue(i), i);
            }
            print();
        }
    }
}