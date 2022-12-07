using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace OSMake;

public static class CommandParser
{
    public static string Path { get { return Directory.GetCurrentDirectory(); } } 

    public static void Execute(string input)
    {
        if (input == null || input.Length == 0) { return; }

        string[] parts   = FormatInput(input);
        string   cmdname = parts[0].ToUpper();

        foreach (var cmd in Command.List)
        {
            if (cmd.Name == cmdname)
            {
                cmd.Handler(ReformInput(parts), parts.ToList());
                return;
            }
        }
        Debug.Error("Invalid command '%s'", cmdname);
    }

    public static Command FromName(string name)
    {
        foreach (var cmd in Command.List) { if (cmd.Name == name) { return cmd; } }
        Debug.Error("No command with name '%s'", name);
        return new Command("", CommandHandlers.SET_ASSEMBLER);
    }

    public static string FormatPath(string path, bool ending_slash = true)
    {
        path = path.Replace("\\", "/");
        if (!path.EndsWith("/") && ending_slash) { path += "/"; }
        return path;
    }

    private static string[] FormatInput(string input)
    {   
        List<string> parts = input.Replace("\t", " ").Split(' ').ToList();
        for (int i = 0; i < parts.Count; i++) 
        { 
            if (parts[i] == null || parts[i].Length == 0 || parts[i] == " ") 
            { 
                parts.RemoveAt(i); 
                i--;
            } 
        }
        return parts.ToArray();
    }

    public static string ReformInput(string[] parts)
    {
        string output = string.Empty;
        foreach (string part in parts) 
        { 
            output += part.Replace(" ", "").Replace("\t", "") + " "; 
        }
        if (output.EndsWith(" ")) { output = output.Remove(output.Length - 1, 1); }
        return output;
    }

    public static string ReformInput(string[] parts, int offset)
    {
        string output = string.Empty;
        for (int i = offset; i < parts.Length; i++)
        { 
            output += parts[i].Replace(" ", "").Replace("\t", "") + " "; 
        }
        if (output.EndsWith(" ")) { output = output.Remove(output.Length - 1, 1); }
        return output;
    }

    public static void SetDirectory(string path)
    {
        if (!Directory.Exists(path)) { Debug.Error("Invalid directory '%s'", path); return; }
        Global.Path = FormatPath(path);
        Debug.Log("Set active directory to '%s'\n", path);
    }
}
