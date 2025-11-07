#ifndef FM_WRAPPER_H
#define FM_WRAPPER_H

#ifdef __cplusplus
extern "C" {
#endif

int fm_init(const char* sdk_path);
int fm_connect();
int fm_disconnect();
int fm_get_battery(int* percent);
int fm_set_shutter(int seconds);
int fm_capture();
int fm_download_last(const char* outdir, const char* filename);

#ifdef __cplusplus
}
#endif
#endif
