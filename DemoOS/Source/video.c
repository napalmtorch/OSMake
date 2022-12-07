#define VRAM_ADDR 0xB8000

void video_clear(char c, char attr)
{
    static char* vram = (char*)VRAM_ADDR;
    for (int i = 0; i < 80 * 25 * 2; i += 2)
    {
        vram[i + 0] = c;
        vram[i + 1] = attr;
    }
}

void video_write(const char* str)
{
    char* vram = (char*)VRAM_ADDR;
    while (*str != 0) { *vram++ = *str++; *vram++ = 0xF0; }
}