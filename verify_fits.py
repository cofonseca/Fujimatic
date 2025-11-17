#!/usr/bin/env python3
"""Verify FITS file structure and display RGB image"""

import sys
import os

try:
    from astropy.io import fits
    import numpy as np
except ImportError:
    print("ERROR: astropy not installed")
    print("Install with: pip install astropy")
    sys.exit(1)

def verify_fits(filename):
    """Verify FITS file structure"""
    print(f"=== Verifying FITS: {filename} ===\n")

    # Open FITS file
    with fits.open(filename) as hdul:
        hdul.info()
        print()

        # Get primary HDU
        hdr = hdul[0].header
        data = hdul[0].data

        # Print header info
        print("=== Header Information ===")
        print(f"NAXIS   = {hdr.get('NAXIS', 'N/A')}")
        print(f"NAXIS1  = {hdr.get('NAXIS1', 'N/A')} (width)")
        print(f"NAXIS2  = {hdr.get('NAXIS2', 'N/A')} (height)")
        print(f"NAXIS3  = {hdr.get('NAXIS3', 'N/A')} (channels)")
        print(f"BITPIX  = {hdr.get('BITPIX', 'N/A')}")
        print(f"EXPTIME = {hdr.get('EXPTIME', 'N/A')} seconds")
        print(f"ISO     = {hdr.get('ISO', 'N/A')}")
        print(f"CAMERA  = {hdr.get('INSTRUME', 'N/A')}")
        print()

        # Print data info
        print("=== Data Structure ===")
        print(f"Shape: {data.shape}")
        print(f"Dtype: {data.dtype}")
        print(f"Min value: {data.min()}")
        print(f"Max value: {data.max()}")
        print(f"Mean value: {data.mean():.1f}")
        print()

        # Check channel structure
        if len(data.shape) == 3:
            print("=== RGB Channel Information ===")
            for i, channel in enumerate(['Red', 'Green', 'Blue']):
                ch_data = data[i]
                print(f"{channel:6s} - Shape: {ch_data.shape}, "
                      f"Range: {ch_data.min()}-{ch_data.max()}, "
                      f"Mean: {ch_data.mean():.1f}")
            print()

            # Check if channels are different (not all the same)
            if np.array_equal(data[0], data[1]) and np.array_equal(data[1], data[2]):
                print("WARNING: All channels are identical!")
            else:
                print("✓ Channels have different data (color image)")
            print()

        # Try to create matplotlib visualization
        try:
            import matplotlib.pyplot as plt

            if len(data.shape) == 3 and data.shape[0] == 3:
                # Transpose to (height, width, channels) for matplotlib
                rgb = np.transpose(data, (1, 2, 0))

                # Flip vertically (FITS is bottom-up)
                rgb_flipped = np.flipud(rgb)

                # Normalize to 0-1 range
                rgb_norm = rgb_flipped.astype(float) / 65535.0

                # Clip to valid range
                rgb_norm = np.clip(rgb_norm, 0, 1)

                # Create figure with subplots
                fig, axes = plt.subplots(2, 2, figsize=(12, 12))

                # Original (FITS orientation)
                axes[0, 0].imshow(rgb.astype(float) / 65535.0)
                axes[0, 0].set_title('Original (FITS orientation - bottom-up)')
                axes[0, 0].axis('off')

                # Flipped (normal orientation)
                axes[0, 1].imshow(rgb_norm)
                axes[0, 1].set_title('Flipped (normal orientation - top-down)')
                axes[0, 1].axis('off')

                # Red channel
                axes[1, 0].imshow(data[0], cmap='Reds')
                axes[1, 0].set_title('Red Channel')
                axes[1, 0].axis('off')

                # Green channel
                axes[1, 1].imshow(data[1], cmap='Greens')
                axes[1, 1].set_title('Green Channel')
                axes[1, 1].axis('off')

                plt.tight_layout()
                plt.savefig('fits_preview.png', dpi=150)
                print("✓ Saved preview image: fits_preview.png")
                print()

                # Show the plot
                plt.show()

        except ImportError:
            print("(matplotlib not installed - skipping visualization)")
        except Exception as e:
            print(f"Error creating visualization: {e}")

if __name__ == '__main__':
    if len(sys.argv) != 2:
        print("Usage: python verify_fits.py <fits_file>")
        sys.exit(1)

    fits_file = sys.argv[1]
    if not os.path.exists(fits_file):
        print(f"ERROR: File not found: {fits_file}")
        sys.exit(1)

    verify_fits(fits_file)
