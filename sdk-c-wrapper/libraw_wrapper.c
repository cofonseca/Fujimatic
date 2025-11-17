#include "libraw_wrapper.h"
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>

#ifdef _WIN32
    #include <windows.h>
#endif

// Include LibRaw C API header
#include <libraw/libraw.h>

// Global state
static libraw_data_t* g_rawdata = NULL;
static int verbose_logging = 0;

// Error code mapping
static int map_libraw_error(int lr_error) {
    // LibRaw uses positive error codes, we return negative for consistency
    return -lr_error;
}

int lr_init() {
    if (verbose_logging) {
        printf("lr_init: Initializing LibRaw library\n");
    }

    // Create LibRaw instance
    g_rawdata = libraw_init(0); // 0 = no special flags
    if (g_rawdata == NULL) {
        fprintf(stderr, "lr_init: Failed to initialize LibRaw\n");
        return -1;
    }

    if (verbose_logging) {
        printf("lr_init: LibRaw initialized successfully (version %s)\n", libraw_version());
    }

    return 0;
}

int lr_open_file(const char* filename) {
    if (g_rawdata == NULL) {
        fprintf(stderr, "lr_open_file: LibRaw not initialized. Call lr_init() first.\n");
        return -1;
    }

    if (filename == NULL || strlen(filename) == 0) {
        fprintf(stderr, "lr_open_file: Invalid filename\n");
        return -2;
    }

    if (verbose_logging) {
        printf("lr_open_file: Opening file '%s'\n", filename);
    }

    // Open and read metadata
    int ret = libraw_open_file(g_rawdata, filename);
    if (ret != LIBRAW_SUCCESS) {
        fprintf(stderr, "lr_open_file: Failed to open file '%s': %s\n",
                filename, libraw_strerror(ret));
        return map_libraw_error(ret);
    }

    if (verbose_logging) {
        printf("lr_open_file: File opened successfully\n");
        printf("  Image size: %d x %d\n",
               g_rawdata->sizes.width, g_rawdata->sizes.height);
        printf("  Camera: %s %s\n",
               g_rawdata->idata.make, g_rawdata->idata.model);
    }

    return 0;
}

int lr_unpack() {
    if (g_rawdata == NULL) {
        fprintf(stderr, "lr_unpack: LibRaw not initialized\n");
        return -1;
    }

    if (verbose_logging) {
        printf("lr_unpack: Unpacking raw image data\n");
    }

    // Unpack raw data
    int ret = libraw_unpack(g_rawdata);
    if (ret != LIBRAW_SUCCESS) {
        fprintf(stderr, "lr_unpack: Failed to unpack: %s\n", libraw_strerror(ret));
        return map_libraw_error(ret);
    }

    if (verbose_logging) {
        printf("lr_unpack: Unpacking complete\n");
    }

    return 0;
}

int lr_process() {
    if (g_rawdata == NULL) {
        fprintf(stderr, "lr_process: LibRaw not initialized\n");
        return -1;
    }

    if (verbose_logging) {
        printf("lr_process: Processing raw data (demosaic, white balance, color correction)\n");
    }

    // Set processing parameters for astrophotography
    // Use camera white balance (preserve color as captured)
    g_rawdata->params.use_camera_wb = 1;

    // Use high-quality AHD demosaicing for X-Trans sensors
    // LibRaw automatically selects appropriate algorithm for X-Trans
    g_rawdata->params.user_qual = 3; // 3 = AHD (best quality)

    // No auto-brightness adjustment (preserve histogram for stacking)
    g_rawdata->params.no_auto_bright = 1;

    // Output 16-bit data (preserve dynamic range)
    g_rawdata->params.output_bps = 16;

    // Use sRGB color space (standard for FITS)
    g_rawdata->params.output_color = 1; // 1 = sRGB

    // No gamma correction (linear data for astrophotography)
    g_rawdata->params.gamm[0] = 1.0;
    g_rawdata->params.gamm[1] = 1.0;

    if (verbose_logging) {
        printf("  Processing params: wb=camera, quality=AHD, bits=16, color=sRGB, gamma=linear\n");
    }

    // Process the image
    int ret = libraw_dcraw_process(g_rawdata);
    if (ret != LIBRAW_SUCCESS) {
        fprintf(stderr, "lr_process: Failed to process: %s\n", libraw_strerror(ret));
        return map_libraw_error(ret);
    }

    if (verbose_logging) {
        printf("lr_process: Processing complete\n");
    }

    return 0;
}

int lr_get_rgb_data(lr_rgb_image_t* image) {
    if (g_rawdata == NULL) {
        fprintf(stderr, "lr_get_rgb_data: LibRaw not initialized\n");
        return -1;
    }

    if (image == NULL) {
        fprintf(stderr, "lr_get_rgb_data: Invalid image pointer\n");
        return -2;
    }

    if (verbose_logging) {
        printf("lr_get_rgb_data: Extracting RGB image data\n");
    }

    // Get processed image
    int ret = 0;
    libraw_processed_image_t* proc_image = libraw_dcraw_make_mem_image(g_rawdata, &ret);
    if (proc_image == NULL) {
        fprintf(stderr, "lr_get_rgb_data: Failed to get processed image\n");
        return -3;
    }

    // Verify image format
    if (proc_image->type != LIBRAW_IMAGE_BITMAP) {
        fprintf(stderr, "lr_get_rgb_data: Unexpected image type: %d\n", proc_image->type);
        libraw_dcraw_clear_mem(proc_image);
        return -4;
    }

    if (proc_image->colors != 3) {
        fprintf(stderr, "lr_get_rgb_data: Expected 3 color channels, got %d\n", proc_image->colors);
        libraw_dcraw_clear_mem(proc_image);
        return -5;
    }

    if (proc_image->bits != 16) {
        fprintf(stderr, "lr_get_rgb_data: Expected 16-bit data, got %d-bit\n", proc_image->bits);
        libraw_dcraw_clear_mem(proc_image);
        return -6;
    }

    // Image dimensions
    int width = proc_image->width;
    int height = proc_image->height;
    int pixel_count = width * height;

    if (verbose_logging) {
        printf("  Image: %d x %d, %d-bit, %d channels\n",
               width, height, proc_image->bits, proc_image->colors);
    }

    // Allocate separate channel buffers
    image->width = width;
    image->height = height;
    image->r_channel = (unsigned short*)malloc(pixel_count * sizeof(unsigned short));
    image->g_channel = (unsigned short*)malloc(pixel_count * sizeof(unsigned short));
    image->b_channel = (unsigned short*)malloc(pixel_count * sizeof(unsigned short));

    if (image->r_channel == NULL || image->g_channel == NULL || image->b_channel == NULL) {
        fprintf(stderr, "lr_get_rgb_data: Memory allocation failed\n");
        free(image->r_channel);
        free(image->g_channel);
        free(image->b_channel);
        libraw_dcraw_clear_mem(proc_image);
        return -7;
    }

    // LibRaw outputs interleaved RGB data (RGBRGBRGB...)
    // Convert to planar format (RRR...GGG...BBB...) for FITS
    unsigned short* src = (unsigned short*)proc_image->data;
    for (int i = 0; i < pixel_count; i++) {
        image->r_channel[i] = src[i * 3 + 0];
        image->g_channel[i] = src[i * 3 + 1];
        image->b_channel[i] = src[i * 3 + 2];
    }

    // Free LibRaw's processed image
    libraw_dcraw_clear_mem(proc_image);

    if (verbose_logging) {
        printf("lr_get_rgb_data: Extracted %d pixels to planar RGB format\n", pixel_count);
    }

    return 0;
}

void lr_free_rgb_data(lr_rgb_image_t* image) {
    if (image == NULL) {
        return;
    }

    if (verbose_logging) {
        printf("lr_free_rgb_data: Freeing RGB channel data\n");
    }

    free(image->r_channel);
    free(image->g_channel);
    free(image->b_channel);

    image->r_channel = NULL;
    image->g_channel = NULL;
    image->b_channel = NULL;
    image->width = 0;
    image->height = 0;
}

int lr_get_metadata(lr_metadata_t* metadata) {
    if (g_rawdata == NULL) {
        fprintf(stderr, "lr_get_metadata: LibRaw not initialized\n");
        return -1;
    }

    if (metadata == NULL) {
        fprintf(stderr, "lr_get_metadata: Invalid metadata pointer\n");
        return -2;
    }

    if (verbose_logging) {
        printf("lr_get_metadata: Extracting metadata for FITS headers\n");
    }

    // Extract exposure time (convert to seconds)
    metadata->exposure_time = g_rawdata->other.shutter;

    // Extract ISO
    metadata->iso = (int)g_rawdata->other.iso_speed;

    // Extract timestamp
    time_t timestamp = g_rawdata->other.timestamp;
    struct tm* tm_info = gmtime(&timestamp);
    if (tm_info != NULL) {
        // Format as ISO 8601 (FITS standard)
        strftime(metadata->timestamp, sizeof(metadata->timestamp),
                "%Y-%m-%dT%H:%M:%S", tm_info);
    } else {
        strcpy(metadata->timestamp, "1970-01-01T00:00:00");
    }

    // Extract camera make and model
    strncpy(metadata->camera_make, g_rawdata->idata.make, sizeof(metadata->camera_make) - 1);
    metadata->camera_make[sizeof(metadata->camera_make) - 1] = '\0';

    strncpy(metadata->camera_model, g_rawdata->idata.model, sizeof(metadata->camera_model) - 1);
    metadata->camera_model[sizeof(metadata->camera_model) - 1] = '\0';

    // Extract image dimensions
    metadata->width = g_rawdata->sizes.width;
    metadata->height = g_rawdata->sizes.height;
    metadata->raw_width = g_rawdata->sizes.raw_width;
    metadata->raw_height = g_rawdata->sizes.raw_height;

    // Extract pixel size (in microns)
    // X-T3 has 3.76µm pixels (hardcoded for now - LibRaw doesn't expose pixel_aspect)
    metadata->pixel_width = 3.76f;
    metadata->pixel_height = 3.76f;

    if (verbose_logging) {
        printf("  Exposure: %.4f seconds\n", metadata->exposure_time);
        printf("  ISO: %d\n", metadata->iso);
        printf("  Timestamp: %s\n", metadata->timestamp);
        printf("  Camera: %s %s\n", metadata->camera_make, metadata->camera_model);
        printf("  Dimensions: %d x %d (raw: %d x %d)\n",
               metadata->width, metadata->height,
               metadata->raw_width, metadata->raw_height);
    }

    return 0;
}

void lr_cleanup() {
    if (g_rawdata == NULL) {
        return;
    }

    if (verbose_logging) {
        printf("lr_cleanup: Closing LibRaw instance\n");
    }

    libraw_close(g_rawdata);
    g_rawdata = NULL;

    if (verbose_logging) {
        printf("lr_cleanup: Cleanup complete\n");
    }
}

void lr_set_verbose(int enabled) {
    verbose_logging = enabled;
    if (verbose_logging) {
        printf("lr_set_verbose: Verbose logging enabled\n");
    }
}

int lr_extract_thumbnail(const char* filename, unsigned char** thumb_data, int* thumb_size) {
    if (filename == NULL || thumb_data == NULL || thumb_size == NULL) {
        fprintf(stderr, "lr_extract_thumbnail: Invalid parameters\n");
        return -1;
    }

    // Create a temporary LibRaw instance (don't use global state)
    libraw_data_t* rawdata = libraw_init(0);
    if (rawdata == NULL) {
        fprintf(stderr, "lr_extract_thumbnail: Failed to initialize LibRaw\n");
        return -2;
    }

    // Open the file
    int ret = libraw_open_file(rawdata, filename);
    if (ret != LIBRAW_SUCCESS) {
        fprintf(stderr, "lr_extract_thumbnail: Failed to open file: %s\n", libraw_strerror(ret));
        libraw_close(rawdata);
        return map_libraw_error(ret);
    }

    // Unpack thumbnail
    ret = libraw_unpack_thumb(rawdata);
    if (ret != LIBRAW_SUCCESS) {
        fprintf(stderr, "lr_extract_thumbnail: Failed to unpack thumbnail: %s\n", libraw_strerror(ret));
        libraw_close(rawdata);
        return map_libraw_error(ret);
    }

    // Check if thumbnail exists and is JPEG format
    if (rawdata->thumbnail.tformat != LIBRAW_THUMBNAIL_JPEG) {
        fprintf(stderr, "lr_extract_thumbnail: Thumbnail is not JPEG format (format=%d)\n",
                rawdata->thumbnail.tformat);
        libraw_close(rawdata);
        return -3;
    }

    // Get thumbnail size
    *thumb_size = rawdata->thumbnail.tlength;
    if (*thumb_size <= 0) {
        fprintf(stderr, "lr_extract_thumbnail: Invalid thumbnail size: %d\n", *thumb_size);
        libraw_close(rawdata);
        return -4;
    }

    // Allocate buffer for thumbnail data
    *thumb_data = (unsigned char*)malloc(*thumb_size);
    if (*thumb_data == NULL) {
        fprintf(stderr, "lr_extract_thumbnail: Failed to allocate memory for thumbnail (%d bytes)\n", *thumb_size);
        libraw_close(rawdata);
        return -5;
    }

    // Copy thumbnail data
    memcpy(*thumb_data, rawdata->thumbnail.thumb, *thumb_size);

    // Cleanup
    libraw_close(rawdata);

    if (verbose_logging) {
        printf("lr_extract_thumbnail: Extracted JPEG thumbnail (%d bytes) from '%s'\n", *thumb_size, filename);
    }

    return 0;
}
