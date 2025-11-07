from ctypes import *
import os
import platform

FUJI_SDK_ERR_OK         = 0
FUJI_SDK_ERR_LOADSDK    = -1
FUJI_SDK_ERR_SDKVERSION = -2

class FujiSDK:
    
    def __init__(self):
        self.m_module = None
        self.m_err = None
        lib_path = ""
        
        try:
            os_name = platform.system()
            lib_path = ""
            if os_name == "Windows":
                lib_path = "dll/XAPI.dll"
            if os_name == "Darwin":
                lib_path = "bundle/XAPI.bundle/Contents/MacOS/XAPI"
            if os_name == "Linux":
                lib_path = "so/XAPI.so"
            
            base = os.path.dirname(os.path.realpath(__file__))
            path = os.path.normpath(os.path.join(base, lib_path))
            self.m_module = CDLL(path)
        except OSError as e:
            self.m_module = None
    
    
    def __del__(self):
        if self.m_module is not None:
            del self.m_module
            self.m_module = None
    
    
    def load_sdk_library(self):
        try:
            self.m_pfnInit = getattr(self.m_module, "XSDK_Init", None)
            self.m_pfnExit = getattr(self.m_module, "XSDK_Exit", None)
            self.m_pfnDetect = getattr(self.m_module, "XSDK_Detect", None)
            self.m_pfnAppend = getattr(self.m_module, "XSDK_Append", None)
            self.m_pfnClose = getattr(self.m_module, "XSDK_Close", None)
            self.m_pfnOpenEx = getattr(self.m_module, "XSDK_OpenEx", None)
            self.m_pfnPowerOFF = getattr(self.m_module, "XSDK_PowerOFF", None)
            self.m_pfnGetErrorNumber = getattr(self.m_module, "XSDK_GetErrorNumber", None)
            self.m_pfnGetVersionString = getattr(self.m_module, "XSDK_GetVersionString", None)
            self.m_pfnGetErrorDetails = getattr(self.m_module, "XSDK_GetErrorDetails", None)
            self.m_pfnGetDeviceInfo = getattr(self.m_module, "XSDK_GetDeviceInfo", None)
            self.m_pfnWriteDeviceName = getattr(self.m_module, "XSDK_WriteDeviceName", None)
            self.m_pfnGetFirmwareVersion = getattr(self.m_module, "XSDK_GetFirmwareVersion", None)
            self.m_pfnGetLensInfo = getattr(self.m_module, "XSDK_GetLensInfo", None)
            self.m_pfnGetLensVersion = getattr(self.m_module, "XSDK_GetLensVersion", None)
            self.m_pfnGetDeviceInfoEx = getattr(self.m_module, "XSDK_GetDeviceInfoEx", None)
            self.m_pfnCapPriorityMode = getattr(self.m_module, "XSDK_CapPriorityMode", None)
            self.m_pfnSetPriorityMode = getattr(self.m_module, "XSDK_SetPriorityMode", None)
            self.m_pfnGetPriorityMode = getattr(self.m_module, "XSDK_GetPriorityMode", None)
            self.m_pfnCapRelease = getattr(self.m_module, "XSDK_CapRelease", None)
            self.m_pfnRelease = getattr(self.m_module, "XSDK_Release", None)
            self.m_pfnGetReleaseStatus = getattr(self.m_module, "XSDK_GetReleaseStatus", None)
            self.m_pfnCapReleaseEx = getattr(self.m_module, "XSDK_CapReleaseEx", None)
            self.m_pfnReleaseEx = getattr(self.m_module, "XSDK_ReleaseEx", None)
            self.m_pfnCapReleaseStatus = getattr(self.m_module, "XSDK_CapReleaseStatus", None)
            self.m_pfnReadImageInfo = getattr(self.m_module, "XSDK_ReadImageInfo", None)
            self.m_pfnReadPreview = getattr(self.m_module, "XSDK_ReadPreview", None)
            self.m_pfnReadImage = getattr(self.m_module, "XSDK_ReadImage", None)
            self.m_pfnDeleteImage = getattr(self.m_module, "XSDK_DeleteImage", None)
            self.m_pfnGetBufferCapacity = getattr(self.m_module, "XSDK_GetBufferCapacity", None)
            self.m_pfnCapAEMode = getattr(self.m_module, "XSDK_CapAEMode", None)
            self.m_pfnSetAEMode = getattr(self.m_module, "XSDK_SetAEMode", None)
            self.m_pfnGetAEMode = getattr(self.m_module, "XSDK_GetAEMode", None)
            self.m_pfnCapShutterSpeed = getattr(self.m_module, "XSDK_CapShutterSpeed", None)
            self.m_pfnSetShutterSpeed = getattr(self.m_module, "XSDK_SetShutterSpeed", None)
            self.m_pfnGetShutterSpeed = getattr(self.m_module, "XSDK_GetShutterSpeed", None)
            self.m_pfnCapExposureBias = getattr(self.m_module, "XSDK_CapExposureBias", None)
            self.m_pfnSetExposureBias = getattr(self.m_module, "XSDK_SetExposureBias", None)
            self.m_pfnGetExposureBias = getattr(self.m_module, "XSDK_GetExposureBias", None)
            self.m_pfnCapDynamicRange = getattr(self.m_module, "XSDK_CapDynamicRange", None)
            self.m_pfnSetDynamicRange = getattr(self.m_module, "XSDK_SetDynamicRange", None)
            self.m_pfnGetDynamicRange = getattr(self.m_module, "XSDK_GetDynamicRange", None)
            self.m_pfnCapSensitivity = getattr(self.m_module, "XSDK_CapSensitivity", None)
            self.m_pfnSetSensitivity = getattr(self.m_module, "XSDK_SetSensitivity", None)
            self.m_pfnGetSensitivity = getattr(self.m_module, "XSDK_GetSensitivity", None)
            self.m_pfnCapMeteringMode = getattr(self.m_module, "XSDK_CapMeteringMode", None)
            self.m_pfnSetMeteringMode = getattr(self.m_module, "XSDK_SetMeteringMode", None)
            self.m_pfnGetMeteringMode = getattr(self.m_module, "XSDK_GetMeteringMode", None)
            self.m_pfnCapLensZoomPos = getattr(self.m_module, "XSDK_CapLensZoomPos", None)
            self.m_pfnGetLensZoomPos = getattr(self.m_module, "XSDK_GetLensZoomPos", None)
            self.m_pfnSetLensZoomPos = getattr(self.m_module, "XSDK_SetLensZoomPos", None)
            self.m_pfnCapAperture = getattr(self.m_module, "XSDK_CapAperture", None)
            self.m_pfnSetAperture = getattr(self.m_module, "XSDK_SetAperture", None)
            self.m_pfnGetAperture = getattr(self.m_module, "XSDK_GetAperture", None)
            self.m_pfnCapWBMode = getattr(self.m_module, "XSDK_CapWBMode", None)
            self.m_pfnSetWBMode = getattr(self.m_module, "XSDK_SetWBMode", None)
            self.m_pfnGetWBMode = getattr(self.m_module, "XSDK_GetWBMode", None)
            self.m_pfnCapWBColorTemp = getattr(self.m_module, "XSDK_CapWBColorTemp", None)
            self.m_pfnSetWBColorTemp = getattr(self.m_module, "XSDK_SetWBColorTemp", None)
            self.m_pfnGetWBColorTemp = getattr(self.m_module, "XSDK_GetWBColorTemp", None)
            self.m_pfnCapMediaRecord = getattr(self.m_module, "XSDK_CapMediaRecord", None)
            self.m_pfnSetMediaRecord = getattr(self.m_module, "XSDK_SetMediaRecord", None)
            self.m_pfnGetMediaRecord = getattr(self.m_module, "XSDK_GetMediaRecord", None)
            self.m_pfnCapForceMode = getattr(self.m_module, "XSDK_CapForceMode", None)
            self.m_pfnSetForceMode = getattr(self.m_module, "XSDK_SetForceMode", None)
            self.m_pfnSetBackupSettings = getattr(self.m_module, "XSDK_SetBackupSettings", None)
            self.m_pfnGetBackupSettings = getattr(self.m_module, "XSDK_GetBackupSettings", None)
            self.m_pfnCapDriveMode = getattr(self.m_module, "XSDK_CapDriveMode", None)
            self.m_pfnSetDriveMode = getattr(self.m_module, "XSDK_SetDriveMode", None)
            self.m_pfnGetDriveMode = getattr(self.m_module, "XSDK_GetDriveMode", None)
            self.m_pfnCapMode = getattr(self.m_module, "XSDK_CapMode", None)
            self.m_pfnSetMode = getattr(self.m_module, "XSDK_SetMode", None)
            self.m_pfnGetMode = getattr(self.m_module, "XSDK_GetMode", None)
            self.m_pfnCapRecordingStatus = getattr(self.m_module, "XSDK_CapRecordingStatus", None)
            self.m_pfnGetRecordingStatus = getattr(self.m_module, "XSDK_GetRecordingStatus", None)
            self.m_pfnCapMovieShutterSpeed = getattr(self.m_module, "XSDK_CapMovieShutterSpeed", None)
            self.m_pfnSetMovieShutterSpeed = getattr(self.m_module, "XSDK_SetMovieShutterSpeed", None)
            self.m_pfnGetMovieShutterSpeed = getattr(self.m_module, "XSDK_GetMovieShutterSpeed", None)
            self.m_pfnCapMovieExposureBias = getattr(self.m_module, "XSDK_CapMovieExposureBias", None)
            self.m_pfnSetMovieExposureBias = getattr(self.m_module, "XSDK_SetMovieExposureBias", None)
            self.m_pfnGetMovieExposureBias = getattr(self.m_module, "XSDK_GetMovieExposureBias", None)
            self.m_pfnCapMovieSensitivity = getattr(self.m_module, "XSDK_CapMovieSensitivity", None)
            self.m_pfnSetMovieSensitivity = getattr(self.m_module, "XSDK_SetMovieSensitivity", None)
            self.m_pfnGetMovieSensitivity = getattr(self.m_module, "XSDK_GetMovieSensitivity", None)
            self.m_pfnCapMovieAperture = getattr(self.m_module, "XSDK_CapMovieAperture", None)
            self.m_pfnSetMovieAperture = getattr(self.m_module, "XSDK_SetMovieAperture", None)
            self.m_pfnGetMovieAperture = getattr(self.m_module, "XSDK_GetMovieAperture", None)
            self.m_pfnCapMovieDynamicRange = getattr(self.m_module, "XSDK_CapMovieDynamicRange", None)
            self.m_pfnSetMovieDynamicRange = getattr(self.m_module, "XSDK_SetMovieDynamicRange", None)
            self.m_pfnGetMovieDynamicRange = getattr(self.m_module, "XSDK_GetMovieDynamicRange", None)
            self.m_pfnCapMovieMeteringMode = getattr(self.m_module, "XSDK_CapMovieMeteringMode", None)
            self.m_pfnSetMovieMeteringMode = getattr(self.m_module, "XSDK_SetMovieMeteringMode", None)
            self.m_pfnGetMovieMeteringMode = getattr(self.m_module, "XSDK_GetMovieMeteringMode", None)
            self.m_pfnCapMovieWBMode = getattr(self.m_module, "XSDK_CapMovieWBMode", None)
            self.m_pfnSetMovieWBMode = getattr(self.m_module, "XSDK_SetMovieWBMode", None)
            self.m_pfnGetMovieWBMode = getattr(self.m_module, "XSDK_GetMovieWBMode", None)
            self.m_pfnCapMovieWBColorTemp = getattr(self.m_module, "XSDK_CapMovieWBColorTemp", None)
            self.m_pfnSetMovieWBColorTemp = getattr(self.m_module, "XSDK_SetMovieWBColorTemp", None)
            self.m_pfnGetMovieWBColorTemp = getattr(self.m_module, "XSDK_GetMovieWBColorTemp", None)
            self.m_pfnCapProp = getattr(self.m_module, "XSDK_CapProp", None)
            self.m_pfnSetProp = getattr(self.m_module, "XSDK_SetProp", None)
            self.m_pfnGetProp = getattr(self.m_module, "XSDK_GetProp", None)
        
            if not all([self.m_pfnInit,
                        self.m_pfnExit,
                        self.m_pfnDetect,
                        self.m_pfnAppend,
                        self.m_pfnClose,
                        self.m_pfnOpenEx,
                        self.m_pfnPowerOFF,
                        self.m_pfnGetErrorNumber,
                        self.m_pfnGetVersionString,
                        self.m_pfnGetErrorDetails,
                        self.m_pfnGetDeviceInfo,
                        self.m_pfnWriteDeviceName,
                        self.m_pfnGetFirmwareVersion,
                        self.m_pfnGetLensInfo,
                        self.m_pfnGetLensVersion,
                        self.m_pfnGetDeviceInfoEx,
                        self.m_pfnCapPriorityMode,
                        self.m_pfnSetPriorityMode,
                        self.m_pfnGetPriorityMode,
                        self.m_pfnCapRelease,
                        self.m_pfnRelease,
                        self.m_pfnGetReleaseStatus,
                        self.m_pfnCapReleaseEx,
                        self.m_pfnReleaseEx,
                        self.m_pfnCapReleaseStatus,
                        self.m_pfnReadImageInfo,
                        self.m_pfnReadPreview,
                        self.m_pfnReadImage,
                        self.m_pfnDeleteImage,
                        self.m_pfnGetBufferCapacity,
                        self.m_pfnCapAEMode,
                        self.m_pfnSetAEMode,
                        self.m_pfnGetAEMode,
                        self.m_pfnCapShutterSpeed,
                        self.m_pfnSetShutterSpeed,
                        self.m_pfnGetShutterSpeed,
                        self.m_pfnCapExposureBias,
                        self.m_pfnSetExposureBias,
                        self.m_pfnGetExposureBias,
                        self.m_pfnCapDynamicRange,
                        self.m_pfnSetDynamicRange,
                        self.m_pfnGetDynamicRange,
                        self.m_pfnCapSensitivity,
                        self.m_pfnSetSensitivity,
                        self.m_pfnGetSensitivity,
                        self.m_pfnCapMeteringMode,
                        self.m_pfnSetMeteringMode,
                        self.m_pfnGetMeteringMode,
                        self.m_pfnCapLensZoomPos,
                        self.m_pfnGetLensZoomPos,
                        self.m_pfnSetLensZoomPos,
                        self.m_pfnCapAperture,
                        self.m_pfnSetAperture,
                        self.m_pfnGetAperture,
                        self.m_pfnCapWBMode,
                        self.m_pfnSetWBMode,
                        self.m_pfnGetWBMode,
                        self.m_pfnCapWBColorTemp,
                        self.m_pfnSetWBColorTemp,
                        self.m_pfnGetWBColorTemp,
                        self.m_pfnCapMediaRecord,
                        self.m_pfnSetMediaRecord,
                        self.m_pfnGetMediaRecord,
                        self.m_pfnCapForceMode,
                        self.m_pfnSetForceMode,
                        self.m_pfnSetBackupSettings,
                        self.m_pfnGetBackupSettings,
                        self.m_pfnCapDriveMode,
                        self.m_pfnSetDriveMode,
                        self.m_pfnGetDriveMode,
                        self.m_pfnCapMode,
                        self.m_pfnSetMode,
                        self.m_pfnGetMode,
                        self.m_pfnCapRecordingStatus,
                        self.m_pfnGetRecordingStatus,
                        self.m_pfnCapMovieShutterSpeed,
                        self.m_pfnSetMovieShutterSpeed,
                        self.m_pfnGetMovieShutterSpeed,
                        self.m_pfnCapMovieExposureBias,
                        self.m_pfnSetMovieExposureBias,
                        self.m_pfnGetMovieExposureBias,
                        self.m_pfnCapMovieSensitivity,
                        self.m_pfnSetMovieSensitivity,
                        self.m_pfnGetMovieSensitivity,
                        self.m_pfnCapMovieAperture,
                        self.m_pfnSetMovieAperture,
                        self.m_pfnGetMovieAperture,
                        self.m_pfnCapMovieDynamicRange,
                        self.m_pfnSetMovieDynamicRange,
                        self.m_pfnGetMovieDynamicRange,
                        self.m_pfnCapMovieMeteringMode,
                        self.m_pfnSetMovieMeteringMode,
                        self.m_pfnGetMovieMeteringMode,
                        self.m_pfnCapMovieWBMode,
                        self.m_pfnSetMovieWBMode,
                        self.m_pfnGetMovieWBMode,
                        self.m_pfnCapMovieWBColorTemp,
                        self.m_pfnSetMovieWBColorTemp,
                        self.m_pfnGetMovieWBColorTemp,
                        self.m_pfnCapProp,
                        self.m_pfnSetProp,
                        self.m_pfnGetProp,]):
                del self.m_module
                self.m_module = None
                self.m_err = FUJI_SDK_ERR_SDKVERSION
            else:
                self.m_err = FUJI_SDK_ERR_OK
        except OSError as e:
            del self.m_module
            self.m_module = None
            self.err = FUJI_SDK_ERR_LOADSDK
    
    
    def get_error(self):
        return self.m_err
    
    
    def get_module(self):
        return self.m_module
