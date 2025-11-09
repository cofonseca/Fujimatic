#ifndef FM_WRAPPER_H
#define FM_WRAPPER_H

#ifdef __cplusplus
extern "C" {
#endif

int fm_init(const char* sdk_path);
int fm_connect();
int fm_disconnect();
int fm_get_battery(int* percent);
int fm_get_shutter(int* microseconds);
int fm_set_shutter(int microseconds);
int fm_list_shutter_speeds();
int fm_set_exposure_mode(int mode);
int fm_get_exposure_mode(int* mode);
int fm_get_iso(int* iso);
int fm_set_iso(int iso);
int fm_capture();
int fm_download_last(const char* outdir, const char* filename);
int fm_get_supported_shutter_count(int* count);
int fm_get_supported_shutter_speeds(int* count, int* speeds);
int fm_set_verbose(int enabled);

#ifdef __cplusplus
}
#endif
#endif
