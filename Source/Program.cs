using System;
using System.IO;

namespace OSMake;

public static class Program
{
    public static void Main(string[] args)
    {
        Debug.Log("OSMake Utility alpha v1.2\n");
        Debug.Log("Current directory: %s\n", CommandParser.Path);

        CommandParser.Execute("SET_ASSEMBLER Dependencies/nasm.exe");
        CommandParser.Execute("SET_COMPILER  Dependencies/GCC/bin/i686-elf-gcc.exe");
        CommandParser.Execute("SET_LINKER    Dependencies/GCC/bin/i686-elf-ld.exe");
        CommandParser.Execute("SET_OBJDUMP   !");
        CommandParser.Execute("SET_LIMINE    Dependencies/Limine/");
        CommandParser.Execute("SET_EMULATOR  C:/Program Files (x86)/qemu/qemu-system-i386");

        CommandParser.Execute("SET_DIR      DemoOS");
        CommandParser.Execute("RMMK_DIR     Bin");

        CommandParser.Execute("ASSEMBLE_PATH    Bin            Source      -felf32");
        CommandParser.Execute("COMPILE_PATH     Bin            Source      -nostdlib -ffreestanding -Wall -Werror");
        CommandParser.Execute("LINK_PATH        Bin/kernel.elf Bin         -T DemoOS/linker.ld");
        CommandParser.Execute("MK_ISO Bin/myos.iso limine.cfg Bin/kernel.elf");
        CommandParser.Execute("LIMINE Bin/myos.iso --force-mbr");
        CommandParser.Execute("RUN Bin/myos.iso");
        Debug.Log("Successfully finished building project.\n");
        Console.ReadLine();
    }
}
