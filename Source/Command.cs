using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using DiscUtils.Iso9660;

namespace OSMake;

public class Command
{
    public string Name  { get; private set; }
    public Action<string, List<string>> Handler { get; private set; }

    public Command(string name, Action<string, List<string>> handler)
    {
        this.Name    = name;
        this.Handler = handler;
    }

    public static Command SET_ASSEMBLER { get; private set; } = new Command("SET_ASSEMBLER", CommandHandlers.SET_ASSEMBLER);
    public static Command SET_COMPILER  { get; private set; } = new Command("SET_COMPILER",  CommandHandlers.SET_COMPILER);
    public static Command SET_LINKER    { get; private set; } = new Command("SET_LINKER",    CommandHandlers.SET_LINKER);
    public static Command SET_OBJDUMP   { get; private set; } = new Command("SET_OBJDUMP",   CommandHandlers.SET_OBJDUMP);
    public static Command SET_GRUB      { get; private set; } = new Command("SET_GRUB",      CommandHandlers.SET_GRUB);
    public static Command SET_LIMINE    { get; private set; } = new Command("SET_LIMINE",    CommandHandlers.SET_LIMINE);
    public static Command SET_EMULATOR  { get; private set; } = new Command("SET_EMULATOR",  CommandHandlers.SET_EMULATOR);

    public static Command SET_DIR       { get; private set; } = new Command("SET_DIR", CommandHandlers.SET_DIR);

    public static Command MK_DIR { get; private set; } = new Command("MK_DIR", CommandHandlers.MK_DIR);
    public static Command RMMK_DIR { get; private set; } = new Command("RMMK_DIR", CommandHandlers.RMMK_DIR);

    public static Command RM_DIR { get; private set; } = new Command("RM_DIR", CommandHandlers.RM_DIR);

    public static Command COMPILE { get; private set; } = new Command("COMPILE", CommandHandlers.COMPILE);
    public static Command COMPILE_PATH { get; private set; } = new Command("COMPILE_PATH", CommandHandlers.COMPILE_PATH);

    public static Command ASSEMBLE { get; private set; } = new Command("ASSEMBLE", CommandHandlers.ASSEMBLE);
    public static Command ASSEMBLE_PATH { get; private set; } = new Command("ASSEMBLE_PATH", CommandHandlers.ASSEMBLE_PATH);

    public static Command LINK { get; private set; } = new Command("LINK", CommandHandlers.LINK);
    public static Command LINK_PATH { get; private set; } = new Command("LINK_PATH", CommandHandlers.LINK_PATH);

    public static Command MK_ISO { get; private set; } = new Command("MK_ISO", CommandHandlers.MK_ISO);

    public static Command LIMINE { get; private set; } = new Command("LIMINE", CommandHandlers.LIMINE);

    public static Command RUN { get; private set; } = new Command("RUN", CommandHandlers.RUN);

    public static List<Command> List = new List<Command>()
    {
        SET_ASSEMBLER,
        SET_COMPILER,
        SET_LINKER,
        SET_OBJDUMP,
        SET_GRUB,
        SET_LIMINE,
        SET_EMULATOR,
        SET_DIR,
        MK_DIR,
        RM_DIR,
        RMMK_DIR,
        COMPILE,
        COMPILE_PATH,
        ASSEMBLE,
        ASSEMBLE_PATH,
        LINK,
        LINK_PATH,
        MK_ISO,
        LIMINE,
        RUN,
    };
}

public static class CommandHandlers
{
    private static void SetExecutable(string input, List<string> args, string type)
    {
        if (args.Count < 2) { Debug.Error("Expected location/command of %s executable", type); return; }

        Command cmd = CommandParser.FromName(args[0].ToUpper());
        if (args[1] == "!") { Debug.Log("Set executable - Type:%s Value:(disabled)\n", type.ToString().PadRight(10, ' ')); }
        else
        {
            string file = input.Substring(cmd.Name.Length + 1);
            switch (type)
            {
                default:          { Debug.Error("Fatal internal error while setting executable"); return; }
                case "assembler": { Global.Assembler  = file; break; }
                case "compiler":  { Global.Compiler   = file; break; }
                case "linker":    { Global.Linker     = file; break; }
                case "objdump":   { Global.ObjectDump = file; break; }
                case "grub":      { Global.Grub       = file; break; }
                case "limine":    { Global.Limine     = file; break; }
                case "emulator":  { Global.Emulator   = file; break; }
            }

            Debug.Log("Set executable - Type:%s Value:%s\n", type.ToString().PadRight(10, ' '), file);
        }
    }

    private static string GetFileName(string path, bool ext = false)
    {
        return (ext ? Path.GetFileName(path) : Path.GetFileNameWithoutExtension(path));
    }

    private static Process StartProcess(string name, string args)
    {
        try
        {
            Process proc = Process.Start(name, args);
            return proc;
        }
        catch (Exception ex) { Debug.Error("%s", ex.Message); return Process.GetCurrentProcess(); }
    }

    private static void RunExecutable(string input, List<string> args, string exec_name, string msg_name)
    {
        if (args.Count == 1) { Debug.Error("Expected output file"); return; }
        if (args.Count == 2) { Debug.Error("Expected input file"); return; }
        string f_out = Global.Path + args[1], f_in = Global.Path + args[2], comp_args = CommandParser.ReformInput(args.ToArray(), 3);

        if (!File.Exists(f_in)) { Debug.Error("Unable to locate file '%s'", f_in); return; }

        string proc_args = "";
        if (exec_name == Global.Compiler) { proc_args = " -o " + f_out + " -c " + f_in + " " + comp_args; }
        else if (exec_name == Global.Assembler) { proc_args = comp_args + " " + f_in + " -o " + f_out; }
        else { proc_args = " -o " + f_out + " " + f_in + " " + comp_args; }
        Process proc = StartProcess(exec_name, proc_args);
        proc.WaitForExit();
        
        if (proc.ExitCode != 0) { Debug.Error("%s Failed - Input:'%s'", msg_name, f_in); }
        Debug.Log("%s succeeded - Input:{ %s } Output:{ %s }\n", msg_name, f_in, f_out);        
    }

    public static void SET_ASSEMBLER(string input, List<string> args) { SetExecutable(input, args, "assembler"); }

    public static void SET_COMPILER(string input, List<string> args) { SetExecutable(input, args, "compiler"); }

    public static void SET_LINKER(string input, List<string> args) { SetExecutable(input, args, "linker"); }

    public static void SET_OBJDUMP(string input, List<string> args)
    { 
        SetExecutable(input, args, "objdump"); 
        Global.IsObjectDump = (args[1] != "!");
        if (!Global.IsObjectDump) { Global.ObjectDump = ""; }
    }

    public static void SET_GRUB(string input, List<string> args) 
    { 
        SetExecutable(input, args, "grub"); 
        Global.IsGrub = (args[1] != "!");
        if (!Global.IsGrub) { Global.Grub = ""; }
        if (args[1] != "!") { CommandParser.Execute("SET_LIMINE !"); }
    }

    public static void SET_LIMINE(string input, List<string> args) 
    { 
        SetExecutable(input, args, "limine"); 
        Global.IsLimine = (args[1] != "!");
        if (!Global.IsLimine) { Global.Limine = ""; }
        if (args[1] != "!") { CommandParser.Execute("SET_GRUB !"); }
    }

    public static void SET_EMULATOR(string input, List<string> args) { SetExecutable(input, args, "emulator"); }

    public static void SET_DIR(string input, List<string> args)
    {
        if (args.Count < 2) { Debug.Error("Expected path"); return; }
        string path = CommandParser.FormatPath(input.Substring(Command.SET_DIR.Name.Length + 1));
        CommandParser.SetDirectory(path);
    }

    public static void MK_DIR(string input, List<string> args)
    {
        if (args.Count < 2) { Debug.Error("Expected path"); return; }
        string path = CommandParser.FormatPath(input.Substring(Command.MK_DIR.Name.Length + 1));
        Directory.CreateDirectory(path);
        Debug.Log("Created directory at '%s'\n", path);
    }

    public static void RM_DIR(string input, List<string> args)
    {
        if (args.Count < 2) { Debug.Error("Expected path"); return; }
        string path = CommandParser.FormatPath(input.Substring(Command.MK_DIR.Name.Length + 1));
        if (!Directory.Exists(path)) { Debug.Error("Unable to locate directory '%s'", path); return; }
        Directory.Delete(path, true);
        Debug.Log("Deleted directory at '%s'\n", path);
    }

    public static void RMMK_DIR(string input, List<string> args)
    {
        if (args.Count < 2) { Debug.Error("Expected path"); return; }
        string path = Global.Path + input.Substring(Command.RMMK_DIR.Name.Length + 1);
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
            Debug.Log("Deleted directory at '%s'\n", path);
        }
        CommandParser.Execute("MK_DIR " + path);
    }

    public static void COMPILE(string input, List<string> args)
    {
        RunExecutable(input, args, Global.Compiler, "Compilation");
    }

    public static void COMPILE_PATH(string input, List<string> args)
    {
        if (args.Count == 1) { Debug.Error("Expected output path"); return; }
        if (args.Count == 2) { Debug.Error("Expected input path"); return; }
        string d_out = args[1], d_in = args[2], comp_args = CommandParser.ReformInput(args.ToArray(), 3);
        if (!d_out.EndsWith("/")) { d_out += "/"; }
        if (!d_in.EndsWith("/"))  { d_in += "/"; }

        foreach (var file in Directory.GetFiles(Global.Path + d_in, "*.c", SearchOption.AllDirectories))
        {
            string name = GetFileName(file, false);
            CommandParser.Execute("COMPILE " + d_out + name + ".o " + file.Substring(Global.Path.Length) + " " + comp_args);
        }
    }

    public static void ASSEMBLE(string input, List<string> args)
    {
        RunExecutable(input, args, Global.Assembler, "Assembler");
    }

    public static void ASSEMBLE_PATH(string input, List<string> args)
    {
        if (args.Count == 1) { Debug.Error("Expected output path"); return; }
        if (args.Count == 2) { Debug.Error("Expected input path"); return; }
        string d_out = args[1], d_in = args[2], comp_args = CommandParser.ReformInput(args.ToArray(), 3);
        if (!d_out.EndsWith("/")) { d_out += "/"; }
        if (!d_in.EndsWith("/"))  { d_in += "/"; }

        foreach (var file in Directory.GetFiles(Global.Path + d_in, "*.asm", SearchOption.AllDirectories))
        {
            string name = GetFileName(file, false);
            CommandParser.Execute("ASSEMBLE " + d_out + name + ".o " + file.Substring(Global.Path.Length) + " " + comp_args);
        }
    }

    public static void LINK(string input, List<string> args)
    {
        if (args.Count == 1) { Debug.Error("Expected input file"); return; }
        if (args.Count == 2) { Debug.Error("Expected output file"); return; }

        string[] f_ins = args[2].Split(',');
        string f_out = args[1], f_in = CommandParser.ReformInput(f_ins), comp_args = CommandParser.ReformInput(args.ToArray(), 3);

        for (int i = 0; i < f_ins.Length; i++)
        {
            if (!File.Exists(f_ins[i])) { Debug.Error("Unable to locate file '%s'", f_ins[i]); return; }
        }

        string proc_args = comp_args + " -o " + f_out + " " + f_in;

        Process proc = StartProcess(Global.Linker, proc_args);
        proc.WaitForExit();
        
        if (proc.ExitCode != 0) { Debug.Error("Linker Failed - Input:'%s'", f_in); }
        Debug.Log("Linker succeeded - Input:{ %s } Output:%s\n", f_in, f_out);    
    }

    public static void LINK_PATH(string input, List<string> args)
    {
        if (args.Count == 1) { Debug.Error("Expected input file"); return; }
        if (args.Count == 2) { Debug.Error("Expected output file"); return; }
        string f_out = Global.Path + args[1], d_in = args[2], comp_args = CommandParser.ReformInput(args.ToArray(), 3);

        if (!Directory.Exists(d_in)) { Debug.Error("Unable to locate directory '%s'", d_in); return; }

        string f_ins = string.Empty;
        foreach (string f in Directory.GetFiles(Global.Path + d_in)) { f_ins += f + ","; }
        if (f_ins.EndsWith(",")) { f_ins = f_ins.Remove(f_ins.Length - 1, 1 ); }

        CommandParser.Execute("LINK " + f_out + " " + f_ins + " " + comp_args);
    }

    public static void MK_ISO(string input, List<string> args)
    {
        string k_out    = Path.GetFileName(args[3]);
        byte[] cfg_data = File.ReadAllBytes(Global.Path + args[2]);

        if (Global.IsLimine)
        {
            var f_cd   = File.OpenRead(Global.Limine + "limine-cd.bin");
            var f_sys  = File.OpenRead(Global.Limine + "limine.sys");
            var f_krnl = File.OpenRead(Global.Path + args[3]);

            var iso = new CDBuilder()
            {
                UseJoliet = true,
                VolumeIdentifier = "OSMake",
                UpdateIsolinuxBootTable = true,
            };

            iso.AddFile("limine.sys", f_sys);
            iso.AddFile("limine.cfg", cfg_data);
            iso.AddFile(k_out, f_krnl);
            iso.SetBootImage(f_cd, BootDeviceEmulation.NoEmulation, 0);
            iso.Build(Global.Path + args[1]);
            Debug.Log("Successfully created ISO image at '%s'\n", Global.Path + args[1]);
        }
    }

    public static void LIMINE(string input, List<string> args)
    {
        string iso_file  = args[1];
        string proc_args = args[2];
        Process proc = StartProcess(Global.Limine + "limine-deploy.exe", proc_args + " " + Global.Path + iso_file);
        proc.WaitForExit();

        if (proc.ExitCode != 0) { Debug.Error("Failed to deploy limine bootloader"); return; }
        Debug.Log("Successfully deployed limine bootloader\n");
    }

    public static void RUN(string input, List<string> args)
    {
        string proc_args = "-cdrom " + Global.Path + args[1] + " -m 512M -usb -serial stdio -device AC97 -no-reboot -no-shutdown";
        Process proc = StartProcess(Global.Emulator, proc_args);
        proc.WaitForExit();

        if (proc.ExitCode != 0) { Debug.Error("Failed to run emulator"); return; }
        Debug.Log("Successfully ran emulator\n");
    }
}