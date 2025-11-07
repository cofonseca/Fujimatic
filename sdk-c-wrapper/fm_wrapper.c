#include "fm_wrapper.h"
#include <stdio.h>

int fm_init(const char* sdk_path) {
    printf("Initializing Fuji SDK at %s\n", sdk_path);
    return 0;
}
int fm_connect() {
    printf("Connecting to camera...\n");
    return 0;
}
int fm_disconnect() {
    printf("Disconnecting camera...\n");
    return 0;
}
int fm_get_battery(int* percent) {
    *percent = 100; // mock value
    return 0;
}
int fm_set_shutter(int seconds) {
    printf("Set shutter speed to %d seconds\n", seconds);
    return 0;
}
int fm_capture() {
    printf("Triggering capture...\n");
    return 0;
}
int fm_download_last(const char* outdir, const char* filename) {
    printf("Downloading last image to %s/%s\n", outdir, filename);
    return 0;
}
