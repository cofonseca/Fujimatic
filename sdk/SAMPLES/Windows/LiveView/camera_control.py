import os
from ctypes import *
from enum import Enum
import fuji_sdk

# SDK Result
XSDK_COMPLETE = 0
XSDK_ERROR    = -1


# DSC connect I/F
XSDK_DSC_IF_USB        = 0x00000001  # USB
XSDK_DSC_IF_WIFI_LOCAL = 0x00000010  # Wi-Fi within local segment
XSDK_DSC_IF_WIFI_IP    = 0x00000020  # Wi-Fi specified IPv4 address

# Priority Mode
XSDK_PRIORITY_CAMERA = 0x0001  #Camera
XSDK_PRIORITY_PC     = 0x0002  #PC

# LiveView Image Quality
SDK_LIVEVIEW_QUALITY_FINE   = 0x0001  # Fine
SDK_LIVEVIEW_QUALITY_NORMAL = 0x0002  # Normal
SDK_LIVEVIEW_QUALITY_BASIC  = 0x0003  # Basic

# LiveView Image Size
SDK_LIVEVIEW_SIZE_L = 0x0001  # L(1280)
SDK_LIVEVIEW_SIZE_M = 0x0002  # M(800)
SDK_LIVEVIEW_SIZE_S = 0x0003  # S(640)

# LiveView Mode
SDK_LIVEVIEW_MODE1 = 0x0001
SDK_LIVEVIEW_MODE2 = 0x0002

# Release Mode
XSDK_RELEASE_SHOOT       = 0x0100
XSDK_RELEASE_N_S1OFF     = 0x0004
XSDK_RELEASE_SHOOT_S1OFF = (XSDK_RELEASE_SHOOT | XSDK_RELEASE_N_S1OFF)

# ReleaseEx Mode
XSDK_RELEASE_EX_S1_ON                     = 0x00010000
XSDK_RELEASE_EX_S2_ON                     = 0x00020000
XSDK_RELEASE_EX_S1_OFF                    = 0x00000001
XSDK_RELEASE_EX_S2_OFF                    = 0x00000002
XSDK_RELEASE_EX_S1_ON_S2_ON_S2_OFF_S1_OFF = (XSDK_RELEASE_EX_S1_ON | XSDK_RELEASE_EX_S2_ON | XSDK_RELEASE_EX_S2_OFF | XSDK_RELEASE_EX_S1_OFF)

# Image Format(ReadImageInfo)
XSDK_IMAGEFORMAT_RAW  = 1
XSDK_IMAGEFORMAT_LIVE = 4
XSDK_IMAGEFORMAT_NONE = 5
XSDK_IMAGEFORMAT_JPEG = 7
XSDK_IMAGEFORMAT_HEIF = 18

# Media Record
XSDK_MEDIAREC_RAWJPEG = 0x0001
XSDK_MEDIAREC_RAW     = 0x0002
XSDK_MEDIAREC_JPEG    = 0x0003
XSDK_MEDIAREC_OFF     = 0x0004

# API Code
class API_CODE(Enum):
    API_CODE_Init                       = 0x1001
    API_CODE_Exit                       = 0x1002
    API_CODE_Detect                     = 0x1010
    API_CODE_Close                      = 0x1022
    API_CODE_OpenEx                     = 0x1024
    API_CODE_GetErrorNumber             = 0x1031
    API_CODE_SetPriorityMode            = 0x1102
    API_CODE_ReleaseEx                  = 0x1116
    API_CODE_ReadImageInfo              = 0x1201
    API_CODE_ReadImage                  = 0x1203
    API_CODE_DeleteImage                = 0x1204
    API_CODE_CapExposureBias            = 0x1307
    API_CODE_SetExposureBias            = 0x1308
    API_CODE_GetExposureBias            = 0x1309
    API_CODE_SetMediaRecord             = 0x1352
    API_CODE_CapProp                    = 0x1401
    API_CODE_SetProp                    = 0x1402
    API_CODE_GetProp                    = 0x1403
    API_CODE_StartLiveView              = 0x3301
    API_CODE_StopLiveView               = 0x3302
    API_CODE_SetLiveViewImageQuality    = 0x3323
    API_CODE_SetLiveViewImageSize       = 0x3325
    API_CODE_SetLiveViewMode            = 0x332F

class ERR_CODE(Enum):
    XSDK_ERRCODE_NOERR                  = 0x00000000
    XSDK_ERRCODE_SEQUENCE               = 0x00001001
    XSDK_ERRCODE_PARAM                  = 0x00001002
    XSDK_ERRCODE_INVALID_CAMERA         = 0x00001003
    XSDK_ERRCODE_LOADLIB                = 0x00001004
    XSDK_ERRCODE_UNSUPPORTED            = 0x00001005
    XSDK_ERRCODE_BUSY                   = 0x00001006
    XSDK_ERRCODE_AF_TIMEOUT             = 0x00001007
    XSDK_ERRCODE_SHOOT_ERROR            = 0x00001008
    XSDK_ERRCODE_FRAME_FULL             = 0x00001009
    XSDK_ERRCODE_STANDBY                = 0x00001010
    XSDK_ERRCODE_NODRIVER               = 0x00001011
    XSDK_ERRCODE_NO_MODEL_MODULE        = 0x00001012
    XSDK_ERRCODE_API_NOTFOUND           = 0x00001013
    XSDK_ERRCODE_API_MISMATCH           = 0x00001014
    XSDK_ERRCODE_INVALID_USBMODE        = 0x00001015
    XSDK_ERRCODE_FORCEMODE_BUSY         = 0x00001016
    XSDK_ERRCODE_RUNNING_OTHER_FUNCTION = 0x00001017
    XSDK_ERRCODE_COMMUNICATION          = 0x00002001
    XSDK_ERRCODE_TIMEOUT                = 0x00002002
    XSDK_ERRCODE_COMBINATION            = 0x00002003
    XSDK_ERRCODE_WRITEERROR             = 0x00002004
    XSDK_ERRCODE_CARDFULL               = 0x00002005
    XSDK_ERRCODE_HARDWARE               = 0x00003001
    XSDK_ERRCODE_INTERNAL               = 0x00009001
    XSDK_ERRCODE_MEMFULL                = 0x00009002
    XSDK_ERRCODE_UNKNOWN                = 0x00009100

class ImageInformation(Structure):
    _fields_ = [('internal_name',    c_char * 32),
                ('format',           c_long),
                ('data_size',        c_long),
                ('image_pix_height', c_long),
                ('image_pix_width',  c_long),
                ('image_bit_depth',  c_long),
                ('preview_size',     c_long),
                ('image_handle',     c_void_p),]


class CameraControl:

    def __init__(self):
        self.fuji_sdk = None
        self.api_code = c_long(0)
        self.err_code = c_long(0)
    
    
    def __del__(self):
        if self.fuji_sdk is not None:
            del self.fuji_sdk
            self.fuji_sdk = None
    
    
    def load_library(self):
        self.fuji_sdk = fuji_sdk.FujiSDK()
        
        self.fuji_sdk.load_sdk_library()
        
        if self.fuji_sdk.get_error() != fuji_sdk.FUJI_SDK_ERR_OK:
            del self.fuji_sdk
            self.fuji_sdk = None
            return False
        return True
    
    
    def init(self):
        result = self.fuji_sdk.m_pfnInit(0)
        if result != XSDK_COMPLETE:
            self.fuji_sdk.m_pfnGetErrorNumber(None, byref(self.api_code), byref(self.err_code))
        
        return result
    
    
    def exit(self):
        result = self.fuji_sdk.m_pfnExit()
        if result != XSDK_COMPLETE:
            self.fuji_sdk.m_pfnGetErrorNumber(None, byref(self.api_code), byref(self.err_code))

        return result
    
    
    def detect(self, interface, ip_address, device_name, camera_count):
        result = self.fuji_sdk.m_pfnDetect(interface, ip_address, device_name, byref(camera_count))
        if result != XSDK_COMPLETE:
            self.fuji_sdk.m_pfnGetErrorNumber(None, byref(self.api_code), byref(self.err_code))
        
        return result
    
    
    def open(self, device, camera_handle, camera_mode, option):
        result = self.fuji_sdk.m_pfnOpenEx(device, byref(camera_handle), byref(camera_mode), option)
        if result != XSDK_COMPLETE:
            self.fuji_sdk.m_pfnGetErrorNumber(None, byref(self.api_code), byref(self.err_code))
        
        return result
    
    
    def close(self, camera_handle):
        result = self.fuji_sdk.m_pfnClose(camera_handle)
        if result != XSDK_COMPLETE:
            self.fuji_sdk.m_pfnGetErrorNumber(camera_handle, byref(self.api_code), byref(self.err_code))
        
        return result
    
    
    def set_priority_mode(self, camera_handle, priority_mode):
        result = self.fuji_sdk.m_pfnSetPriorityMode(camera_handle, priority_mode)
        if result != XSDK_COMPLETE:
            self.fuji_sdk.m_pfnGetErrorNumber(camera_handle, byref(self.api_code), byref(self.err_code))
       
        return result
    
    
    def release_ex(self, camera_handle, release_mode, shot_option, status):
        result = self.fuji_sdk.m_pfnReleaseEx(camera_handle, release_mode, byref(shot_option), byref(status))
        if result != XSDK_COMPLETE:
            self.fuji_sdk.m_pfnGetErrorNumber(camera_handle, byref(self.api_code), byref(self.err_code))
        
        return result
    
    
    def read_image_info(self, camera_handle, image_info):
        result = self.fuji_sdk.m_pfnReadImageInfo(camera_handle, byref(image_info))
        if result != XSDK_COMPLETE:
            self.fuji_sdk.m_pfnGetErrorNumber(camera_handle, byref(self.api_code), byref(self.err_code))
        
        return result
    
    
    def read_image(self, camera_handle, data, data_size):
        result = self.fuji_sdk.m_pfnReadImage(camera_handle, byref(data), data_size)
        if result != XSDK_COMPLETE:
            self.fuji_sdk.m_pfnGetErrorNumber(camera_handle, byref(self.api_code), byref(self.err_code))
        
        return result
    
    
    def delete_image(self, camera_handle):
        result = self.fuji_sdk.m_pfnDeleteImage(camera_handle)
        if result != XSDK_COMPLETE:
            self.fuji_sdk.m_pfnGetErrorNumber(camera_handle, byref(self.api_code), byref(self.err_code))
        
        return result
    
    
    def cap_exposure_bias(self, camera_handle, num_exposure_bias, exposure_bias_list):
        result = self.fuji_sdk.m_pfnCapExposureBias(camera_handle, byref(num_exposure_bias), None)
        if result != XSDK_COMPLETE:
            self.fuji_sdk.m_pfnGetErrorNumber(camera_handle, byref(self.api_code), byref(self.err_code))
        else:
            list_size = c_long * num_exposure_bias.value
            value_list = list_size()
            result = self.fuji_sdk.m_pfnCapExposureBias(camera_handle, byref(num_exposure_bias), value_list)
            if result != XSDK_COMPLETE:
                self.fuji_sdk.m_pfnGetErrorNumber(camera_handle, byref(self.api_code), byref(self.err_code))
            else:
                for i in range(num_exposure_bias.value):
                    exposure_bias_list.append(value_list[i])
        
        return result
    
    
    def set_exposure_bias(self, camera_handle, exposure_bias):
        result = self.fuji_sdk.m_pfnSetExposureBias(camera_handle, exposure_bias)
        if result != XSDK_COMPLETE:
            self.fuji_sdk.m_pfnGetErrorNumber(camera_handle, byref(self.api_code), byref(self.err_code))
        
        return result
    
    
    def get_exposure_bias(self, camera_handle, exposure_bias):
        result = self.fuji_sdk.m_pfnGetExposureBias(camera_handle, byref(exposure_bias))
        if result != XSDK_COMPLETE:
            self.fuji_sdk.m_pfnGetErrorNumber(camera_handle, byref(self.api_code), byref(self.err_code))
        
        return result
    
    
    def set_media_record(self, camera_handle, media_record):
        result = self.fuji_sdk.m_pfnSetMediaRecord(camera_handle, media_record)
        if result != XSDK_COMPLETE:
            self.fuji_sdk.m_pfnGetErrorNumber(camera_handle, byref(self.api_code), byref(self.err_code))
        
        return result
    
    
    def set_prop(self, camera_handle, api_code, api_param):
        result = self.fuji_sdk.m_pfnSetProp(camera_handle, api_code, api_param)
        if result != XSDK_COMPLETE:
            self.fuji_sdk.m_pfnGetErrorNumber(camera_handle, byref(self.api_code), byref(self.err_code))
        
        return result
    
    
    def set_prop_l(self, camera_handle, api_code, api_param, param1):
        result = self.fuji_sdk.m_pfnSetProp(camera_handle, api_code, api_param, param1)
        if result != XSDK_COMPLETE:
            self.fuji_sdk.m_pfnGetErrorNumber(camera_handle, byref(self.api_code), byref(self.err_code))
        
        return result

