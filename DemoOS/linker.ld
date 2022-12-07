ENTRY(_start)
OUTPUT_FORMAT(elf32-i386)
OUTPUT_ARCH(i386:i386)

SECTIONS
{
	. = 1M;
	_kernel_start = .;
 
	.text BLOCK(4K) : ALIGN(4K)
	{
		*(.multiboot.text)
		*(.text)
	}
 
	.rodata BLOCK(4K) : ALIGN(4K)
	{
		*(.rodata)
	}

	.data BLOCK(4K) : ALIGN(4K)
	{
		start_ctors = .;
		KEEP(*( .init_array ));
		KEEP(*(SORT_BY_INIT_PRIORITY( .init_array.* )));
		end_ctors = .;
		*(.data)
	}

	_bss_start = .;
	.bss BLOCK(4K) : ALIGN(4K)
	{
		*(COMMON)
		*(.bss)
	}
	_bss_end = .;

	.kend BLOCK(4K) : ALIGN(4K)
	{
		*(.kend)
	}

	_kernel_end = ALIGN(4K);

	/DISCARD/ : { *(.fini_array*) *(.comment) *(.dis) }
}