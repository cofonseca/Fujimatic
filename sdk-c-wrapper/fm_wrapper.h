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
int fm_get_image_quality(int* quality);
int fm_set_image_quality(int quality);
int fm_capture();
int fm_capture_bulb(int duration_seconds);  // BULB mode capture with timed exposure
int fm_download_last(const char* outdir, const char* filename);
int fm_get_supported_shutter_count(int* count);
int fm_get_supported_shutter_speeds(int* count, int* speeds);
int fm_get_focus_mode(int* mode);
int fm_set_focus_mode(int mode);
int fm_get_supported_focus_modes(int* count, int* modes);
int fm_adjust_focus(int direction, int steps);
int fm_trigger_autofocus();
int fm_set_verbose(int enabled);

// Live view functions
int fm_start_liveview();
int fm_stop_liveview();
int fm_get_liveview_frame(unsigned char** buffer, int* size);
void fm_free_liveview_frame(unsigned char* buffer);
int fm_is_liveview_active(int* is_active);
int fm_set_liveview_size(int size_code);  // 0=S (320px), 1=M (640px), 2=L (1024px)

#ifdef __cplusplus
}
#endif
#endif
