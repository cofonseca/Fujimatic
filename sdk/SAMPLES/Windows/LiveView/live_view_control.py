import os
import tkinter as tk
from PIL import Image, ImageTk
from ctypes import *
import camera_control
import datetime
import io
import platform
import time


# Result
#XSDK_COMPLETE   = 0
#XSDK_ERROR      = -1
SDK_LOAD_ERROR   = -2
ARGV_ERROR       = -3
NOT_FOUND_CAMERA = -4


exposure_bias_dict = {
     150 : "+5",
     140 : "+4 2/3",
     130 : "+4 1/3",
     120 : "+4",
     110 : "+3 2/3",
     100 : "+3 1/3",
      90 : "+3",
      80 : "+2 2/3",
      70 : "+2 1/3",
      60 : "+2",
      50 : "+1 2/3",
      40 : "+1 1/3",
      30 : "+1",
      20 : "+2/3",
      10 : "+1/3",
       0 : "±0",
     -10 : "-1/3",
     -20 : "-2/3",
     -30 : "-1",
     -40 : "-1 1/3",
     -50 : "-1 2/3",
     -60 : "-2",
     -70 : "-2 1/3",
     -80 : "-2 2/3",
     -90 : "-3",
    -100 : "-3 1/3",
    -110 : "-3 2/3",
    -120 : "-4",
    -130 : "-4 1/3",
    -140 : "-4 2/3",
    -150 : "-5" }


class LiveViewControl:

    def __init__(self):
        self._camera_control = None


    def __del__(self):
        if self._camera_control is not None:
            del self._camera_control
            self._camera_control = None


    def initialize(self):
        result = camera_control.XSDK_ERROR
        self._camera_control = camera_control.CameraControl()
        if self._camera_control.load_library() == False: # Loda XAPI.so.
            # error
            return SDK_LOAD_ERROR
        result = self._camera_control.init() # Initialize SDK.
        if result != camera_control.XSDK_COMPLETE:
            # error
            return SDK_LOAD_ERROR
        
        self.camera_handle = c_longlong(0)
        self.is_check = 0
        self.is_communicatom_error = False
        return result
    
    
    def camera_conect(self, argv):
        result = camera_control.XSDK_ERROR
        interface = 0
        ip_address = ""
        device_name = ""
        camera_count = c_long(0)
        if len(argv) == 1:
            interface = camera_control.XSDK_DSC_IF_USB
            os_name = platform.system()
            if os_name == "Darwin":
                time.sleep(3)
        elif len(argv) == 2:
            if argv[1] == "-l":
                interface = camera_control.XSDK_DSC_IF_WIFI_LOCAL
            else:
                # error
                return ARGV_ERROR
        elif len(argv) == 3:
            if argv[1] == "-i":
                interface = camera_control.XSDK_DSC_IF_WIFI_IP
                ip_address = argv[2]
            else:
                # error
                return ARGV_ERROR
        elif len(argv) > 3:
            # error
            return ARGV_ERROR
        
        ip_address = create_string_buffer(ip_address.encode("UTF-8"), 20)
        device_name = create_string_buffer(device_name.encode("UTF-8"), 20)
        result = self._camera_control.detect(interface, ip_address, device_name, camera_count) # Camera search.
        if result == camera_control.XSDK_COMPLETE and camera_count.value <= 0:
            return NOT_FOUND_CAMERA
        if result != camera_control.XSDK_COMPLETE:
            # error
            return result
        
        if camera_count.value > 0:
            device = "ENUM:0"
            device = create_string_buffer(device.encode("UTF-8"), 10)
            camera_mode = c_long(0)
            result = self._camera_control.open(device, self.camera_handle, camera_mode, None) # Establish a session between the camera.
            if result != camera_control.XSDK_COMPLETE:
                # error
                return result
        
        result = self._camera_control.set_priority_mode(self.camera_handle, camera_control.XSDK_PRIORITY_CAMERA) # Set priority mode is camera.
        result = self._camera_control.set_media_record(self.camera_handle, camera_control.XSDK_MEDIAREC_RAWJPEG) # Set media recode is RAW and JPEG.
        return result
    
    
    def view_setting(self, show_list, exposure_bias):
        result = camera_control.XSDK_ERROR
        exposure_bias_list = []
        exposurebias_num = c_long(0)
        result = self._camera_control.cap_exposure_bias(self.camera_handle, exposurebias_num, exposure_bias_list) # Get a configurable number.
        if result != camera_control.XSDK_COMPLETE:
            # error
            return result
        for item in exposure_bias_list: # Update list.
             if item in exposure_bias_dict:
                show_list.append(exposure_bias_dict[item])
        
        result = self._camera_control.get_exposure_bias(self.camera_handle, exposure_bias) # Get exposure bias setting.
        return result
    
    
    def release(self):
        result = camera_control.XSDK_ERROR
        shot_option = c_long(0)
        status = c_long(0)
        result = self._camera_control.release_ex(self.camera_handle, camera_control.XSDK_RELEASE_EX_S1_ON_S2_ON_S2_OFF_S1_OFF, shot_option, status) # Take a picture.
        return result
    
    
    def set_exposure_bias(self, set_exposure_bias, get_exposure_bias):
        result = camera_control.XSDK_ERROR
        inverse_dict = {value: key for key, value in exposure_bias_dict.items()}
        if set_exposure_bias in inverse_dict:
            exposure_bias = inverse_dict[set_exposure_bias]
            result = self._camera_control.set_exposure_bias(self.camera_handle, exposure_bias) # Set exposure bias is select value.
            if result != camera_control.XSDK_COMPLETE:
                # error
                return result
                
        result = self._camera_control.get_exposure_bias(self.camera_handle, get_exposure_bias) # Get exposure bias setting.
        if result != camera_control.XSDK_COMPLETE:
            # error
            get_exposure_bias = c_long(exposure_bias)
            return result
        return result


    def live_view_start(self):
        result = camera_control.XSDK_ERROR
        result = self._camera_control.set_prop_l(self.camera_handle, c_long(camera_control.API_CODE.API_CODE_SetLiveViewImageQuality.value), 1, c_long(camera_control.SDK_LIVEVIEW_QUALITY_FINE)) # Set live view image quality is fine.
        
        result = self._camera_control.set_prop_l(self.camera_handle, camera_control.API_CODE.API_CODE_SetLiveViewImageSize.value, 1, camera_control.SDK_LIVEVIEW_SIZE_L) # Set live view image size is large.
        
        result = self._camera_control.set_prop_l(self.camera_handle, camera_control.API_CODE.API_CODE_SetLiveViewMode.value, 1, camera_control.SDK_LIVEVIEW_MODE1) # Set live view mode is mode1.
        #if result != camera_control.XSDK_COMPLETE:
            # error
            #return result
        
        result = self._camera_control.set_prop(self.camera_handle, camera_control.API_CODE.API_CODE_StartLiveView.value, 0) # Set start live view.
        return result


    def live_view_stop(self):
        result = camera_control.XSDK_ERROR
        try:
            if self.camera_handle.value != 0:
                result = self._camera_control.set_prop(self.camera_handle, camera_control.API_CODE.API_CODE_StopLiveView.value, 0) # Set stop live view.
                while True:
                    image_info = camera_control.ImageInformation()
                    result = self._camera_control.read_image_info(self.camera_handle, image_info) # Acquisition of image information.
                    if result != camera_control.XSDK_COMPLETE or image_info.format == camera_control.XSDK_IMAGEFORMAT_NONE:
                        break
                    
                    result = self._camera_control.delete_image(self.camera_handle) # Delete image information.
            
                result = self._camera_control.close(self.camera_handle) # Disestablish a session between the camera.
            
            if self._camera_control is not None:
                result = self._camera_control.exit() # Termination SDK.
                del self._camera_control
                self._camera_control = None
                
        except Exception as e:
            if self._camera_control is not None:
                result = self._camera_control.exit() # Termination SDK.
                del self._camera_control
                self._camera_control = None
        
        return 0


    def read_image(self):
        try:
            data = None
            self.is_communicatom_error = False
            image_info = camera_control.ImageInformation()
            result = self._camera_control.read_image_info(self.camera_handle, image_info) # Acquisition of image information.
            if result != camera_control.XSDK_COMPLETE:
                # error
                err_code = camera_control.ERR_CODE(self._camera_control.err_code.value)
                if err_code == camera_control.ERR_CODE.XSDK_ERRCODE_COMMUNICATION:
                    self.is_communicatom_error = True
                return None
            
            format = image_info.format & 0b11111111
            if format == camera_control.XSDK_IMAGEFORMAT_NONE: # Image format check.
                return None
            elif format == camera_control.XSDK_IMAGEFORMAT_LIVE: # Image format check.
                data_size = image_info.data_size
                data = create_string_buffer(data_size)
                result = self._camera_control.read_image(self.camera_handle, data, data_size) # Acquisition of image.
                if result != camera_control.XSDK_COMPLETE:
                    # error
                    return None
                return data
            else:
                if self.is_check == 1:
                    data_size = image_info.data_size
                    save_data = create_string_buffer(data_size)
                    result = self._camera_control.read_image(self.camera_handle, save_data, data_size) # Acquisition of image.
                    if result != camera_control.XSDK_COMPLETE:
                        #error
                        return None
                    base = os.path.dirname(os.path.abspath("_file_"))
                    file_name = ""
                    today = datetime.datetime.today()
                    if format == camera_control.XSDK_IMAGEFORMAT_RAW: # Image format check.
                        file_name = str(today.year) + today.strftime("%m") + today.strftime("%d") + "_" + today.strftime("%H") + today.strftime("%M")+ today.strftime("%S") + "_" + today.strftime("%f")[:-3] + ".RAF"
                    elif format == camera_control.XSDK_IMAGEFORMAT_JPEG: # Image format check.
                        file_name = str(today.year) + today.strftime("%m") + today.strftime("%d") + "_" + today.strftime("%H") + today.strftime("%M")+ today.strftime("%S") + "_" + today.strftime("%f")[:-3] + ".JPG"
                    elif format == camera_control.XSDK_IMAGEFORMAT_HEIF: # Image format check.
                        file_name = str(today.year) + today.strftime("%m") + today.strftime("%d") + "_" + today.strftime("%H") + today.strftime("%M")+ today.strftime("%S") + "_" + today.strftime("%f")[:-3] + ".HEIC"
                    else:
                        file_name = "none"
                    if file_name != "none":
                        path = os.path.normpath(os.path.join(base, file_name))
                        with open(path, "wb") as image_data: # Save Image.
                            image_data.write(save_data)
                else:
                    result = self._camera_control.delete_image(self.camera_handle) # Delete image information.
        except Exception as e:
            data = None
        return None
    
    
    def on_check(self, state):
        self.is_check = state # Check box status acquisition
        return 0
    
    
    def get_error_code(self):
        err_info = ""
        api_code = ""
        
        try:
            code = camera_control.API_CODE(self._camera_control.api_code.value)
            api_code = str(code.name)
        except ValueError:
            api_code = "0x" + hex(self._camera_control.api_code.value)[2:].zfill(4).upper()
        
        err_code = ""
        try:
            code2 = camera_control.ERR_CODE(self._camera_control.err_code.value)
            err_code = str(code2.name)
        except ValueError:
            err_code = "0x" + hex(self._camera_control.err_code.value)[2:].zfill(8).upper()
        
        err_info = "APICode : " + api_code + "\nERRCode : " + err_code
        return err_info
    
