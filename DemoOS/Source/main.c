
extern void video_clear(char, char);
extern void video_write(const char*);

void main(void* mboot)
{
    video_clear('X', 0x2D);
    video_write("HELLO WORLD HAHAHAHA");
    while (1);
}