[BITS 32]
[GLOBAL _mboot]
[GLOBAL _start]
[GLOBAL _stack_top]
[GLOBAL _stack_bottom]
[EXTERN _bss_start]
[EXTERN _bss_end]
[EXTERN _kernel_start]
[EXTERN _kernel_end]
[EXTERN main]

MBOOT_PAGE_ALIGN    equ 1 << 0
MBOOT_MEM_INFO      equ 1 << 1 
MBOOT_VID_INFO      equ 1 << 2
MBOOT_HEADER_MAGIC  equ 0x1BADB002
MBOOT_HEADER_FLAGS  equ MBOOT_PAGE_ALIGN | MBOOT_MEM_INFO ;| MBOOT_VID_INFO
MBOOT_CHECKSUM      equ -(MBOOT_HEADER_MAGIC + MBOOT_HEADER_FLAGS)

section .multiboot.text
align 4
_mboot:
    dd MBOOT_HEADER_MAGIC
    dd MBOOT_HEADER_FLAGS
    dd MBOOT_CHECKSUM   
    dd _mboot
    dd 0
    dd 0
    dd 0
    dd _start
    ;dd 0
    ;dd 1280, 720, 32

section .text
_start:
    cli
    mov  ebp, _stack_top            ; set stack base
    mov  esp, ebp                   ; set stack pointer
    
    mov edi, _bss_start             ; load bss address into destination register
    mov ecx, _bss_end               ; load bss end address into ECX register
    sub ecx, _bss_start             ; subtract start address from end address to get size of bss memory region
    mov al, 0                       ; value to fill region with
    rep stosb                       ; fill bss section with value in AL register
    
    push ebx                        ; push multiboot address to stack
    call main                       ; jump to kernel main function
    cli                             ; disable interrupts
    hlt                             ; halt the system


section .bss
align 4096
_stack_bottom:
    resb 64 * 1024
_stack_top: