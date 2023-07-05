using System;

public class EasyConsole
{
    public static string input(string message = null)
    {
            if (message != null) print(message);
            return Console.ReadLine();
    }
    public static void print(params object[] content)
    {
        if (content == null || content.Length == 0)
        {
            Console.WriteLine();
            return;
        }
        
        string s = "";
        foreach (object o in content)
        {
            s += Str(o)+" ";
        }
        Console.WriteLine(s[..^1]);
    }
    public static string Str(object? obj)
    {
        if (obj == null)
            return "null";
        if (obj.GetType().IsArray)
        {
            string s = "";
            foreach (var v in (Array)obj)
            {
                s += Str(v) + ", ";
            }
            if (s.Length > 0) s = s[..^2];
            return "[" + s + "]";
        }
        return obj.ToString();
    }
    public static void PrintLineByLine(Array array, string title = "", string seperator = ", ")
    {
        string s = title+": ";
        int i = 0;
        foreach (var v in array)
        {
            if (i == array.Length - 1) seperator = null;
            s += v.ToString() + seperator;
            i++;
        }
        print(s);
    }
}