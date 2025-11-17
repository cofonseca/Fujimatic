#ifndef LIBRAW_WRAPPER_H
#define LIBRAW_WRAPPER_H

#ifdef __cplusplus
extern "C" {
#endif

// Metadata structure for FITS headers
typedef struct {
    double exposure_time;      // Exposure time in seconds
    int iso;                   // ISO sensitivity
    char timestamp[32];        // Capture timestamp (ISO 8601 format)
    char camera_make[64];      // Camera manufacturer
    char camera_model[64];     // Camera model
    int width;                 // Image width in pixels
    int height;                // Image height in pixels
    float pixel_width;         // Pixel width in microns
    float pixel_height;        // Pixel height in microns
    int raw_width;             // Raw image width (before crop)
    int raw_height;            // Raw image height (before crop)
} lr_metadata_t;

// RGB image data structure (processed, 16-bit per channel)
typedef struct {
    unsigned short* r_channel;  // Red channel data (width × height)
    unsigned short* g_channel;  // Green channel data (width × height)
    unsigned short* b_channel;  // Blue channel data (width × height)
    int width;                  // Image width
    int height;                 // Image height
} lr_rgb_image_t;

// Initialize LibRaw library
// Returns: 0 on success, negative error code on failure
int lr_init();

// Open a RAW file and read metadata
// Parameters:
//   filename: Path to the RAF file
// Returns: 0 on success, negative error code on failure
int lr_open_file(const char* filename);

// Unpack raw image data from the opened file
// Returns: 0 on success, negative error code on failure
int lr_unpack();

// Process raw data (demosaic, white balance, color correction)
// Returns: 0 on success, negative error code on failure
int lr_process();

// Get processed RGB image data (16-bit per channel, planar format)
// Parameters:
//   image: Pointer to rgb_image structure to fill (caller must free channels later)
// Returns: 0 on success, negative error code on failure
// NOTE: Caller must call lr_free_rgb_data() to free allocated memory
int lr_get_rgb_data(lr_rgb_image_t* image);

// Free RGB image data allocated by lr_get_rgb_data()
// Parameters:
//   image: Pointer to rgb_image structure to free
void lr_free_rgb_data(lr_rgb_image_t* image);

// Get metadata for FITS headers
// Parameters:
//   metadata: Pointer to metadata structure to fill
// Returns: 0 on success, negative error code on failure
int lr_get_metadata(lr_metadata_t* metadata);

// Cleanup and close the currently opened file
void lr_cleanup();

// Enable/disable verbose logging
// Parameters:
//   enabled: 1 to enable, 0 to disable
void lr_set_verbose(int enabled);

// Extract embedded JPEG thumbnail from RAF file
// Parameters:
//   filename: Path to the RAF file
//   thumb_data: Pointer to receive allocated buffer containing JPEG data
//   thumb_size: Pointer to receive size of JPEG data in bytes
// Returns: 0 on success, negative error code on failure
// NOTE: Caller must free thumb_data with free() when done
int lr_extract_thumbnail(const char* filename, unsigned char** thumb_data, int* thumb_size);

#ifdef __cplusplus
}
#endif

#endif // LIBRAW_WRAPPER_H
