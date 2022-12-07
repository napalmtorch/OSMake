using System;

namespace OSMake;

public static class Debug
{
    public static string CreateFormattedString(string fmt, object[] args)
    {
        string output = string.Empty;
        int i = 0, a = 0;

        while (i < fmt.Length)
        {
            if (fmt[i] == '%')
            {
                i++;
                if (a < 0 || a >= args.Length) { output += fmt[i]; }
                else
                {
                    if (fmt[i] == 'd') { output += ((int)args[a++]).ToString(); }
                    else if (fmt[i] == 'u') { output += ((uint)args[a++]).ToString(); }
                    else if (fmt[i] == 'l') { output += ((ulong)args[a++]).ToString(); }
                    else if (fmt[i] == 'f') { output += ((float)args[a++]).ToString(); }
                    else if (fmt[i] == 'p') { output += ((uint)args[a++]).ToString("X8"); }
                    else if (fmt[i] == 'c') { output += ((char)args[a++]); }
                    else if (fmt[i] == 's') { output += args[a++].ToString(); }
                    else { output += fmt[i]; }
                }
            }
            else { output += fmt[i]; }
            i++;
        }
        return output;
    }

    public static void LogArguments(string fmt, object[] args)
    {
        string str = CreateFormattedString(fmt, args);
        Log(str);
    }

    public static void Log(string fmt, params object[] args)
    {
        string str = CreateFormattedString(fmt, args);
        Log(str);
    }

    public static void Log(string txt) { Console.Write(txt); }

    public static void Error(string fmt, params object[] args)
    {
        Console.Write('[');
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("ERROR");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("] ");
        LogArguments(fmt, args);
        Console.Write('\n');
        Console.ReadLine();
        Environment.Exit(1);
    }
}