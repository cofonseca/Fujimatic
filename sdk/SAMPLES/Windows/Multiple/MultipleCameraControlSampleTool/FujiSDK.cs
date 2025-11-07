using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using XSDK_CameraListPtr = System.IntPtr;

namespace FujiSDK
{
	public enum FUJI_SDK_ERR : int
	{
		FUJI_SDK_ERR_OK = 0,
		FUJI_SDK_ERR_LOADSDK = -1,
		FUJI_SDK_ERR_SDKVERSION = -2,
	};

	#region struct
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct XSDK_DeviceInformation
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
		public char[] strVendor;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
		public char[] strManufacturer;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
		public char[] strProduct;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
		public char[] strFirmware;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
		public char[] strDeviceType;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
		public char[] strSerialNo;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
		public char[] strFramework;
		public Byte bDeviceId;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
		public char[] strDeviceName;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
		public char[] strYNo;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct XSDK_LensInformation
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
		public char[] strModel;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
		public char[] strProductName;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
		public char[] strSerialNo;
		public int lISCapability;
		public int lMFCapability;
		public int lZoomPosCapability;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct XSDK_ImageInformation
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
		public char[] strInternalName;
		public int lFormat;
		public int lDataSize;
		public int lImagePixHeight;
		public int lImagePixWidth;
		public int lImageBitDepth;
		public int lPreviewSize;
		public IntPtr hImage;
	}
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct XSDK_CameraList
	{
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		public string strProduct;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		public string strSerialNo;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		public string strIPAddress;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		public string strFramework;

		// 他のフィールド...
		[MarshalAs(UnmanagedType.U1)]
		public bool bValid;
	};

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct SDK_MOVIE_AFC_CUSTOM
	{
		public int lTracking;
		public int lSpeed;
	};

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct SDK_FOCUS_POS_CAP
	{
		public int lSizeFocusPosCap;
		public int lStructVer;
		public int lFocusPlsINF;
		public int lFocusPlsMOD;
		public int lFocusOverSearchPlsINF;
		public int lFocusOverSearchPlsMOD;
		public int lFocusPlsFCSDepthCap;
		public int lMinDriveStepMFDriveEndThresh;
	};

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct SDK_AFZoneCustomCapablity
	{
		public int mode;
		public SDK_AFZoneCustom min;
		public SDK_AFZoneCustom max;
	};

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct SDK_AFZoneCustom
	{
		public int h;
		public int v;
	};

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct SDK_FocusArea
	{
		public int h;
		public int v;
		public int size;
	};

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct SDK_FaceFrameInfo
	{
		public int lID;
		public int lTime;
		public int lX;
		public int lY;
		public int lLength_H;
		public int lLength_V;
		public int lColorR;
		public int lColorG;
		public int lColorB;
		public int lAlpha;
		public int lType;
		public int lLikeness;
		public int lDisp;
		public int lSelected;
	};

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct SDK_CustomWBArea
	{
		public int lX;
		public int lY;
		public int lSize;
		public int lMode;
	};

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct SDK_FrameGuideGridInfo
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
		public int[] lGridH;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
		public int[] lGridV;
		public int lLineWidthH;
		public int lLineWidthV;
		public int lLineColorIndex;
		public int lLineAlpha;
	};

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct SDK_CropAreaFrameInfo
	{
		public int lX;
		public int lY;
		public int lLength_H;
		public int lLength_V;
		public int lColorR;
		public int lColorG;
		public int lColorB;
		public int lAlpha;
	};

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct SDK_FOCUS_LIMITER_INDICATOR
	{
		public int lCurrent;
		public int lDOF_Near;
		public int lDOF_Far;
		public int lPos_A;
		public int lPos_B;
		public int lStatus;
	};

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct SDK_TrackingAfFrameInfo
	{
		public int lX;
		public int lY;
		public int lLength_H;
		public int lLength_V;
		public int lColorR;
		public int lColorG;
		public int lColorB;
		public int lAlpha;
	};

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct SDK_MovieTransparentFrameInfo
	{
		public int lX;
		public int lY;
		public int lLength_H;
		public int lLength_V;
		public int lAlpha;
	};

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct SDK_MICLEVEL_INDICATOR
	{
		public int lDSC_L_Peak;
		public int lDSC_L_PeakHold;
		public int lDSC_R_Peak;
		public int lDSC_R_PeakHold;
		public int lXLR_1_Peak;
		public int lXLR_1_PeakHold;
		public int lXLR_2_Peak;
		public int lXLR_2_PeakHold;
		public int lWarning1;
		public int lWarning2;
		public int lMicLine;
	};

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct SDK_HistogramData
	{
		public int lLuminance;
		public int lColorR;
		public int lColorG;
		public int lColorB;
	};

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct SDK_FOCUS_LIMITER
	{
		public int lCurrent;
		public int lDOF_Near;
		public int lDOF_Far;
		public int lPos_A;
		public int lPos_B;
		public int lStatus;
	};

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct SDK_FocusMapData
	{
		public int lDistance;
		public int lColorR;
		public int lColorG;
		public int lColorB;
		public int lAlpha;
	};

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct SDK_PlayBackObjectInformation
	{
		public int lFormat;
		public int llDataSize;
		public int lThumbSize;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
		public byte[] strFileName;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
		public byte[] strcapturedata;
		public int lImagePixHeight;
		public int lImagePixWidth;
		public int lImageBitDepth;
		public int intDuration;
	};

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct SDK_FOLDER_INFO
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
		public char[] pFoldernameSuffix; 
		public int lFolderNumber;
		public int lMaxFrameNumber;
		public int lStatus;
	}
	#endregion // struct

	public class CFujiSDK : IDisposable
	{
		private int m_iErr;
		private IntPtr m_hModule;
		public const long XSDK_COMPLETE = 0;
		public const long XSDK_ERROR = -1;
		#region FujiSDKFunctionPointer
		private IntPtr m_pfnInit;
		private IntPtr m_pfnExit;
		private IntPtr m_pfnDetect;
		private IntPtr m_pfnAppend;
		private IntPtr m_pfnOpenEx;
		private IntPtr m_pfnClose;
		private IntPtr m_pfnPowerOFF;
		private IntPtr m_pfnGetErrorNumber;
		private IntPtr m_pfnGetVersionString;
		private IntPtr m_pfnGetErrorDetails;
		private IntPtr m_pfnGetDeviceInfo;
		private IntPtr m_pfnWriteDeviceName;
		private IntPtr m_pfnGetFirmwareVersion;
		private IntPtr m_pfnGetLensInfo;
		private IntPtr m_pfnGetLensVersion;
		private IntPtr m_pfnGetDeviceInfoEx;
		private IntPtr m_pfnCapPriorityMode;
		private IntPtr m_pfnSetPriorityMode;
		private IntPtr m_pfnGetPriorityMode;
		private IntPtr m_pfnCapRelease;
		private IntPtr m_pfnRelease;
		private IntPtr m_pfnGetReleaseStatus;
		private IntPtr m_pfnCapReleaseEx;
		private IntPtr m_pfnReleaseEx;
		private IntPtr m_pfnReadImageInfo;
		private IntPtr m_pfnReadPreview;
		private IntPtr m_pfnReadImage;
		private IntPtr m_pfnDeleteImage;
		private IntPtr m_pfnGetBufferCapacity;
		private IntPtr m_pfnCapAEMode;
		private IntPtr m_pfnSetAEMode;
		private IntPtr m_pfnGetAEMode;
		private IntPtr m_pfnCapShutterSpeed;
		private IntPtr m_pfnSetShutterSpeed;
		private IntPtr m_pfnGetShutterSpeed;
		private IntPtr m_pfnCapExposureBias;
		private IntPtr m_pfnSetExposureBias;
		private IntPtr m_pfnGetExposureBias;
		private IntPtr m_pfnCapDynamicRange;
		private IntPtr m_pfnSetDynamicRange;
		private IntPtr m_pfnGetDynamicRange;
		private IntPtr m_pfnCapSensitivity;
		private IntPtr m_pfnSetSensitivity;
		private IntPtr m_pfnGetSensitivity;
		private IntPtr m_pfnCapMeteringMode;
		private IntPtr m_pfnSetMeteringMode;
		private IntPtr m_pfnGetMeteringMode;
		private IntPtr m_pfnCapLensZoomPos;
		private IntPtr m_pfnGetLensZoomPos;
		private IntPtr m_pfnSetLensZoomPos;
		private IntPtr m_pfnCapAperture;
		private IntPtr m_pfnSetAperture;
		private IntPtr m_pfnGetAperture;
		private IntPtr m_pfnCapWBMode;
		private IntPtr m_pfnSetWBMode;
		private IntPtr m_pfnGetWBMode;
		private IntPtr m_pfnCapWBColorTemp;
		private IntPtr m_pfnSetWBColorTemp;
		private IntPtr m_pfnGetWBColorTemp;
		private IntPtr m_pfnCapMediaRecord;
		private IntPtr m_pfnSetMediaRecord;
		private IntPtr m_pfnGetMediaRecord;
		private IntPtr m_pfnCapForceMode;
		private IntPtr m_pfnSetForceMode;
		private IntPtr m_pfnSetBackupSettings;
		private IntPtr m_pfnGetBackupSettings;
		private IntPtr m_pfnCapDriveMode;
		private IntPtr m_pfnSetDriveMode;
		private IntPtr m_pfnGetDriveMode;
		private IntPtr m_pfnCapMode;
		private IntPtr m_pfnSetMode;
		private IntPtr m_pfnGetMode;
		private IntPtr m_pfnGetRecordingStatus;
		private IntPtr m_pfnCapMovieShutterSpeed;
		private IntPtr m_pfnSetMovieShutterSpeed;
		private IntPtr m_pfnGetMovieShutterSpeed;
		private IntPtr m_pfnCapMovieExposureBias;
		private IntPtr m_pfnSetMovieExposureBias;
		private IntPtr m_pfnGetMovieExposureBias;
		private IntPtr m_pfnCapMovieSensitivity;
		private IntPtr m_pfnSetMovieSensitivity;
		private IntPtr m_pfnGetMovieSensitivity;
		private IntPtr m_pfnCapMovieAperture;
		private IntPtr m_pfnSetMovieAperture;
		private IntPtr m_pfnGetMovieAperture;
		private IntPtr m_pfnCapMovieDynamicRange;
		private IntPtr m_pfnSetMovieDynamicRange;
		private IntPtr m_pfnGetMovieDynamicRange;
		private IntPtr m_pfnCapMovieMeteringMode;
		private IntPtr m_pfnSetMovieMeteringMode;
		private IntPtr m_pfnGetMovieMeteringMode;
		private IntPtr m_pfnCapMovieWBMode;
		private IntPtr m_pfnSetMovieWBMode;
		private IntPtr m_pfnGetMovieWBMode;
		private IntPtr m_pfnCapMovieWBColorTemp;
		private IntPtr m_pfnSetMovieWBColorTemp;
		private IntPtr m_pfnGetMovieWBColorTemp;
		private IntPtr m_pfnCapProp;
		private IntPtr m_pfnSetProp;
		private IntPtr m_pfnGetProp;
		#endregion // FujiSDKFunctionPointer

		#region DllImport for XAPI.
		[DllImport("kernel32.dll")]
		private static extern IntPtr LoadLibrary(string lpLibFileName);

		[DllImport("kernel32.dll")]
		private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

		[DllImport("kernel32.dll")]
		private static extern int FreeLibrary(IntPtr hModule);
		#endregion // DllImport for XAPI.

	/// <summary>
	/// Get the path where XAPI.DLL is stored
	/// </summary>
	/// <param name="strBasePath">null</param>
	public CFujiSDK(string strBasePath)
	{
		// Get My path
		string pFilePath = "";
		if (strBasePath == null)
		{
			pFilePath = AppDomain.CurrentDomain.BaseDirectory;
		}
		else
		{
			pFilePath = strBasePath;
		}
		pFilePath += "XAPI.DLL";

		Console.WriteLine("LoadLibrary({0})", pFilePath);
		m_hModule = LoadLibrary(pFilePath);
	}

	/// <summary>
	/// Load library
	/// </summary>
	public void LoadSDKLibrary()
	{
		m_iErr = (int)FUJI_SDK_ERR.FUJI_SDK_ERR_OK;

		if (IntPtr.Zero == m_hModule)
		{
			// ERROR
			m_iErr = (int)FUJI_SDK_ERR.FUJI_SDK_ERR_LOADSDK;
		}
		else
		{
			m_pfnInit = GetProcAddress(m_hModule, "XSDK_Init");
			m_pfnExit = GetProcAddress(m_hModule, "XSDK_Exit");
			m_pfnDetect = GetProcAddress(m_hModule, "XSDK_Detect");
			m_pfnAppend = GetProcAddress(m_hModule, "XSDK_Append");
			m_pfnOpenEx = GetProcAddress(m_hModule, "XSDK_OpenEx");
			m_pfnClose = GetProcAddress(m_hModule, "XSDK_Close");
			m_pfnPowerOFF = GetProcAddress(m_hModule, "XSDK_PowerOFF");
			m_pfnGetErrorNumber = GetProcAddress(m_hModule, "XSDK_GetErrorNumber");
			m_pfnGetVersionString = GetProcAddress(m_hModule, "XSDK_GetVersionString");
			m_pfnGetErrorDetails = GetProcAddress(m_hModule, "XSDK_GetErrorDetails");
			m_pfnGetDeviceInfo = GetProcAddress(m_hModule, "XSDK_GetDeviceInfo");
			m_pfnWriteDeviceName = GetProcAddress(m_hModule, "XSDK_WriteDeviceName");
			m_pfnGetFirmwareVersion = GetProcAddress(m_hModule, "XSDK_GetFirmwareVersion");
			m_pfnGetLensInfo = GetProcAddress(m_hModule, "XSDK_GetLensInfo");
			m_pfnGetDeviceInfoEx = GetProcAddress(m_hModule, "XSDK_GetDeviceInfoEx");
			m_pfnGetLensVersion = GetProcAddress(m_hModule, "XSDK_GetLensVersion");
			m_pfnCapPriorityMode = GetProcAddress(m_hModule, "XSDK_CapPriorityMode");
			m_pfnSetPriorityMode = GetProcAddress(m_hModule, "XSDK_SetPriorityMode");
			m_pfnGetPriorityMode = GetProcAddress(m_hModule, "XSDK_GetPriorityMode");
			m_pfnCapRelease = GetProcAddress(m_hModule, "XSDK_CapRelease");
			m_pfnRelease = GetProcAddress(m_hModule, "XSDK_Release");
			m_pfnGetReleaseStatus = GetProcAddress(m_hModule, "XSDK_GetReleaseStatus");
			m_pfnCapReleaseEx = GetProcAddress(m_hModule, "XSDK_CapReleaseEx");
			m_pfnReleaseEx = GetProcAddress(m_hModule, "XSDK_ReleaseEx");
			m_pfnReadImageInfo = GetProcAddress(m_hModule, "XSDK_ReadImageInfo");
			m_pfnReadPreview = GetProcAddress(m_hModule, "XSDK_ReadPreview");
			m_pfnReadImage = GetProcAddress(m_hModule, "XSDK_ReadImage");
			m_pfnDeleteImage = GetProcAddress(m_hModule, "XSDK_DeleteImage");
			m_pfnGetBufferCapacity = GetProcAddress(m_hModule, "XSDK_GetBufferCapacity");
			m_pfnCapAEMode = GetProcAddress(m_hModule, "XSDK_CapAEMode");
			m_pfnSetAEMode = GetProcAddress(m_hModule, "XSDK_SetAEMode");
			m_pfnGetAEMode = GetProcAddress(m_hModule, "XSDK_GetAEMode");
			m_pfnCapShutterSpeed = GetProcAddress(m_hModule, "XSDK_CapShutterSpeed");
			m_pfnSetShutterSpeed = GetProcAddress(m_hModule, "XSDK_SetShutterSpeed");
			m_pfnGetShutterSpeed = GetProcAddress(m_hModule, "XSDK_GetShutterSpeed");
			m_pfnCapExposureBias = GetProcAddress(m_hModule, "XSDK_CapExposureBias");
			m_pfnSetExposureBias = GetProcAddress(m_hModule, "XSDK_SetExposureBias");
			m_pfnGetExposureBias = GetProcAddress(m_hModule, "XSDK_GetExposureBias");
			m_pfnCapDynamicRange = GetProcAddress(m_hModule, "XSDK_CapDynamicRange");
			m_pfnSetDynamicRange = GetProcAddress(m_hModule, "XSDK_SetDynamicRange");
			m_pfnGetDynamicRange = GetProcAddress(m_hModule, "XSDK_GetDynamicRange");
			m_pfnCapSensitivity = GetProcAddress(m_hModule, "XSDK_CapSensitivity");
			m_pfnSetSensitivity = GetProcAddress(m_hModule, "XSDK_SetSensitivity");
			m_pfnGetSensitivity = GetProcAddress(m_hModule, "XSDK_GetSensitivity");
			m_pfnCapMeteringMode = GetProcAddress(m_hModule, "XSDK_CapMeteringMode");
			m_pfnSetMeteringMode = GetProcAddress(m_hModule, "XSDK_SetMeteringMode");
			m_pfnGetMeteringMode = GetProcAddress(m_hModule, "XSDK_GetMeteringMode");
			m_pfnCapLensZoomPos = GetProcAddress(m_hModule, "XSDK_CapLensZoomPos");
			m_pfnGetLensZoomPos = GetProcAddress(m_hModule, "XSDK_GetLensZoomPos");
			m_pfnSetLensZoomPos = GetProcAddress(m_hModule, "XSDK_SetLensZoomPos");
			m_pfnCapAperture = GetProcAddress(m_hModule, "XSDK_CapAperture");
			m_pfnSetAperture = GetProcAddress(m_hModule, "XSDK_SetAperture");
			m_pfnGetAperture = GetProcAddress(m_hModule, "XSDK_GetAperture");
			m_pfnCapWBMode = GetProcAddress(m_hModule, "XSDK_CapWBMode");
			m_pfnSetWBMode = GetProcAddress(m_hModule, "XSDK_SetWBMode");
			m_pfnGetWBMode = GetProcAddress(m_hModule, "XSDK_GetWBMode");
			m_pfnCapWBColorTemp = GetProcAddress(m_hModule, "XSDK_CapWBColorTemp");
			m_pfnSetWBColorTemp = GetProcAddress(m_hModule, "XSDK_SetWBColorTemp");
			m_pfnGetWBColorTemp = GetProcAddress(m_hModule, "XSDK_GetWBColorTemp");
			m_pfnCapMediaRecord = GetProcAddress(m_hModule, "XSDK_CapMediaRecord");
			m_pfnSetMediaRecord = GetProcAddress(m_hModule, "XSDK_SetMediaRecord");
			m_pfnGetMediaRecord = GetProcAddress(m_hModule, "XSDK_GetMediaRecord");
			m_pfnCapForceMode = GetProcAddress(m_hModule, "XSDK_CapForceMode");
			m_pfnSetForceMode = GetProcAddress(m_hModule, "XSDK_SetForceMode");
			m_pfnSetBackupSettings = GetProcAddress(m_hModule, "XSDK_SetBackupSettings");
			m_pfnGetBackupSettings = GetProcAddress(m_hModule, "XSDK_GetBackupSettings");
			m_pfnCapDriveMode = GetProcAddress(m_hModule, "XSDK_CapDriveMode");
			m_pfnSetDriveMode = GetProcAddress(m_hModule, "XSDK_SetDriveMode");
			m_pfnGetDriveMode = GetProcAddress(m_hModule, "XSDK_GetDriveMode");
			m_pfnCapMode = GetProcAddress(m_hModule, "XSDK_CapMode");
			m_pfnSetMode = GetProcAddress(m_hModule, "XSDK_SetMode");
			m_pfnGetMode = GetProcAddress(m_hModule, "XSDK_GetMode");
			m_pfnGetRecordingStatus = GetProcAddress(m_hModule, "XSDK_GetRecordingStatus");
			m_pfnCapMovieShutterSpeed = GetProcAddress(m_hModule, "XSDK_CapMovieShutterSpeed");
			m_pfnSetMovieShutterSpeed = GetProcAddress(m_hModule, "XSDK_SetMovieShutterSpeed");
			m_pfnGetMovieShutterSpeed = GetProcAddress(m_hModule, "XSDK_GetMovieShutterSpeed");
			m_pfnCapMovieExposureBias = GetProcAddress(m_hModule, "XSDK_CapMovieExposureBias");
			m_pfnSetMovieExposureBias = GetProcAddress(m_hModule, "XSDK_SetMovieExposureBias");
			m_pfnGetMovieExposureBias = GetProcAddress(m_hModule, "XSDK_GetMovieExposureBias");
			m_pfnCapMovieSensitivity = GetProcAddress(m_hModule, "XSDK_CapMovieSensitivity");
			m_pfnSetMovieSensitivity = GetProcAddress(m_hModule, "XSDK_SetMovieSensitivity");
			m_pfnGetMovieSensitivity = GetProcAddress(m_hModule, "XSDK_GetMovieSensitivity");
			m_pfnCapMovieAperture = GetProcAddress(m_hModule, "XSDK_CapMovieAperture");
			m_pfnSetMovieAperture = GetProcAddress(m_hModule, "XSDK_SetMovieAperture");
			m_pfnGetMovieAperture = GetProcAddress(m_hModule, "XSDK_GetMovieAperture");
			m_pfnCapMovieDynamicRange = GetProcAddress(m_hModule, "XSDK_CapMovieDynamicRange");
			m_pfnSetMovieDynamicRange = GetProcAddress(m_hModule, "XSDK_SetMovieDynamicRange");
			m_pfnGetMovieDynamicRange = GetProcAddress(m_hModule, "XSDK_GetMovieDynamicRange");
			m_pfnCapMovieMeteringMode = GetProcAddress(m_hModule, "XSDK_CapMovieMeteringMode");
			m_pfnSetMovieMeteringMode = GetProcAddress(m_hModule, "XSDK_SetMovieMeteringMode");
			m_pfnGetMovieMeteringMode = GetProcAddress(m_hModule, "XSDK_GetMovieMeteringMode");
			m_pfnCapMovieWBMode = GetProcAddress(m_hModule, "XSDK_CapMovieWBMode");
			m_pfnSetMovieWBMode = GetProcAddress(m_hModule, "XSDK_SetMovieWBMode");
			m_pfnGetMovieWBMode = GetProcAddress(m_hModule, "XSDK_GetMovieWBMode");
			m_pfnCapMovieWBColorTemp = GetProcAddress(m_hModule, "XSDK_CapMovieWBColorTemp");
			m_pfnSetMovieWBColorTemp = GetProcAddress(m_hModule, "XSDK_SetMovieWBColorTemp");
			m_pfnGetMovieWBColorTemp = GetProcAddress(m_hModule, "XSDK_GetMovieWBColorTemp");
			m_pfnCapProp = GetProcAddress(m_hModule, "XSDK_CapProp");
			m_pfnSetProp = GetProcAddress(m_hModule, "XSDK_SetProp");
			m_pfnGetProp = GetProcAddress(m_hModule, "XSDK_GetProp");

			if ((m_pfnInit == IntPtr.Zero)
				|| (m_pfnExit == IntPtr.Zero)
				|| (m_pfnDetect == IntPtr.Zero)
				|| (m_pfnAppend == IntPtr.Zero)
				|| (m_pfnOpenEx == IntPtr.Zero)
				|| (m_pfnClose == IntPtr.Zero)
				|| (m_pfnPowerOFF == IntPtr.Zero)
				|| (m_pfnGetErrorNumber == IntPtr.Zero)
				|| (m_pfnGetVersionString == IntPtr.Zero)
				|| (m_pfnGetErrorDetails == IntPtr.Zero)
				|| (m_pfnGetDeviceInfo == IntPtr.Zero)
				|| (m_pfnWriteDeviceName == IntPtr.Zero)
				|| (m_pfnGetFirmwareVersion == IntPtr.Zero)
				|| (m_pfnGetLensInfo == IntPtr.Zero)
				|| (m_pfnGetLensVersion == IntPtr.Zero)
				|| (m_pfnGetDeviceInfoEx == IntPtr.Zero)
				|| (m_pfnCapPriorityMode == IntPtr.Zero)
				|| (m_pfnSetPriorityMode == IntPtr.Zero)
				|| (m_pfnGetPriorityMode == IntPtr.Zero)
				|| (m_pfnCapRelease == IntPtr.Zero)
				|| (m_pfnRelease == IntPtr.Zero)
				|| (m_pfnGetReleaseStatus == IntPtr.Zero)
				|| (m_pfnCapReleaseEx == IntPtr.Zero)
				|| (m_pfnReleaseEx == IntPtr.Zero)
				|| (m_pfnReadImageInfo == IntPtr.Zero)
				|| (m_pfnReadPreview== IntPtr.Zero)
				|| (m_pfnReadImage == IntPtr.Zero)
				|| (m_pfnDeleteImage == IntPtr.Zero)
				|| (m_pfnGetBufferCapacity == IntPtr.Zero)
				|| (m_pfnCapAEMode == IntPtr.Zero)
				|| (m_pfnSetAEMode == IntPtr.Zero)
				|| (m_pfnGetAEMode == IntPtr.Zero)
				|| (m_pfnCapShutterSpeed == IntPtr.Zero)
				|| (m_pfnSetShutterSpeed == IntPtr.Zero)
				|| (m_pfnGetShutterSpeed == IntPtr.Zero)
				|| (m_pfnCapExposureBias == IntPtr.Zero)
				|| (m_pfnSetExposureBias == IntPtr.Zero)
				|| (m_pfnGetExposureBias == IntPtr.Zero)
				|| (m_pfnCapDynamicRange == IntPtr.Zero)
				|| (m_pfnSetDynamicRange == IntPtr.Zero)
				|| (m_pfnGetDynamicRange == IntPtr.Zero)
				|| (m_pfnCapSensitivity == IntPtr.Zero)
				|| (m_pfnSetSensitivity == IntPtr.Zero)
				|| (m_pfnGetSensitivity == IntPtr.Zero)
				|| (m_pfnCapMeteringMode == IntPtr.Zero)
				|| (m_pfnSetMeteringMode == IntPtr.Zero)
				|| (m_pfnGetMeteringMode == IntPtr.Zero)
				|| (m_pfnCapLensZoomPos == IntPtr.Zero)
				|| (m_pfnGetLensZoomPos == IntPtr.Zero)
				|| (m_pfnSetLensZoomPos == IntPtr.Zero)
				|| (m_pfnCapAperture == IntPtr.Zero)
				|| (m_pfnSetAperture == IntPtr.Zero)
				|| (m_pfnGetAperture == IntPtr.Zero)
				|| (m_pfnCapWBMode == IntPtr.Zero)
				|| (m_pfnSetWBMode == IntPtr.Zero)
				|| (m_pfnGetWBMode == IntPtr.Zero)
				|| (m_pfnCapWBColorTemp == IntPtr.Zero)
				|| (m_pfnSetWBColorTemp == IntPtr.Zero)
				|| (m_pfnGetWBColorTemp == IntPtr.Zero)
				|| (m_pfnCapMediaRecord == IntPtr.Zero)
				|| (m_pfnSetMediaRecord == IntPtr.Zero)
				|| (m_pfnGetMediaRecord == IntPtr.Zero)
				|| (m_pfnCapForceMode == IntPtr.Zero)
				|| (m_pfnSetForceMode == IntPtr.Zero)
				|| (m_pfnSetBackupSettings == IntPtr.Zero)
				|| (m_pfnGetBackupSettings == IntPtr.Zero)
				|| (m_pfnCapDriveMode == IntPtr.Zero)
				|| (m_pfnSetDriveMode == IntPtr.Zero)
				|| (m_pfnGetDriveMode == IntPtr.Zero)
				|| (m_pfnCapMode == IntPtr.Zero)
				|| (m_pfnSetMode == IntPtr.Zero)
				|| (m_pfnGetMode == IntPtr.Zero)
				|| (m_pfnGetRecordingStatus == IntPtr.Zero)
				|| (m_pfnCapMovieShutterSpeed == IntPtr.Zero)
				|| (m_pfnSetMovieShutterSpeed == IntPtr.Zero)
				|| (m_pfnGetMovieShutterSpeed == IntPtr.Zero)
				|| (m_pfnCapMovieExposureBias == IntPtr.Zero)
				|| (m_pfnSetMovieExposureBias == IntPtr.Zero)
				|| (m_pfnGetMovieExposureBias == IntPtr.Zero)
				|| (m_pfnCapMovieSensitivity == IntPtr.Zero)
				|| (m_pfnSetMovieSensitivity == IntPtr.Zero)
				|| (m_pfnGetMovieSensitivity == IntPtr.Zero)
				|| (m_pfnCapMovieAperture == IntPtr.Zero)
				|| (m_pfnSetMovieAperture == IntPtr.Zero)
				|| (m_pfnGetMovieAperture == IntPtr.Zero)
				|| (m_pfnCapMovieDynamicRange == IntPtr.Zero)
				|| (m_pfnSetMovieDynamicRange == IntPtr.Zero)
				|| (m_pfnGetMovieDynamicRange == IntPtr.Zero)
				|| (m_pfnCapMovieMeteringMode == IntPtr.Zero)
				|| (m_pfnSetMovieMeteringMode == IntPtr.Zero)
				|| (m_pfnGetMovieMeteringMode == IntPtr.Zero)
				|| (m_pfnCapMovieWBMode == IntPtr.Zero)
				|| (m_pfnSetMovieWBMode == IntPtr.Zero)
				|| (m_pfnGetMovieWBMode == IntPtr.Zero)
				|| (m_pfnCapMovieWBColorTemp == IntPtr.Zero)
				|| (m_pfnSetMovieWBColorTemp == IntPtr.Zero)
				|| (m_pfnGetMovieWBColorTemp == IntPtr.Zero)
				|| (m_pfnCapProp == IntPtr.Zero)
				|| (m_pfnSetProp == IntPtr.Zero)
				|| (m_pfnGetProp == IntPtr.Zero))
			{
				FreeLibrary(m_hModule);
				m_hModule = IntPtr.Zero;

				// ERROR
				m_iErr = (int)FUJI_SDK_ERR.FUJI_SDK_ERR_SDKVERSION;
			}
			else
			{
				// Success
			}
		}
		return;
	}

	public void Dispose()
	{
		if (IntPtr.Zero != m_hModule)
		{
			FreeLibrary(m_hModule);
			m_hModule = IntPtr.Zero;
			InitializeLibrary();
		}
	}

	~CFujiSDK()
	{
		Dispose();
	}

	#region Sample Implementation for C# of model common API.
	#region Delegate for C# of model common API.
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_Init_Delegate(IntPtr hLib);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_Exit_Delegate();

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_Detect_Delegate(int lInterface, string pInterface, string pDeviceName, ref int plCount);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_Append_Delegate(int lInterface, string pInterface, string pDeviceName, ref int plCount, XSDK_CameraListPtr pCameraList);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_OpenEx_Delegate(string pDevice, ref IntPtr phCamera, ref int plCameraMode, IntPtr pOption);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_Close_Delegate(IntPtr phCamera);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_PowerOFF_Delegate(IntPtr phCamera);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_GetErrorNumber_Delegate(IntPtr phCamera, ref int plAPICode, ref int plERRCode);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_GetVersionString_Delegate(StringBuilder pVersionString);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_GetErrorDetails_Delegate(IntPtr phCamera, ref int plERRCode);
	
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_GetDeviceInfo_Delegate(IntPtr phCamera, ref XSDK_DeviceInformation pDevInfo);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_WriteDeviceName_Delegate(IntPtr phCamera, string pDeviceName);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_GetFirmwareVersion_Delegate(IntPtr phCamera, StringBuilder pFirmwareVersion);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_GetLensInfo_Delegate(IntPtr phCamera, ref XSDK_LensInformation pLensInfo);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_GetLensVersion_Delegate(IntPtr phCamera, StringBuilder pLensVersion);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_GetDeviceInfoEx_Delegate(IntPtr phCamera, ref XSDK_DeviceInformation pDevInfo, ref int plNumAPICode, IntPtr plAPICode);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_CapPriorityMode_Delegate(IntPtr phCamera, ref int plNumPriorityMode, IntPtr plPriorityMode);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_SetPriorityMode_Delegate(IntPtr phCamera, int lPriorityMode);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_GetPriorityMode_Delegate(IntPtr phCamera, ref int plPriorityMode);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_CapRelease_Delegate(IntPtr phCamera, ref int plNumReleaseMode, IntPtr plReleaseMode);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_Release_Delegate(IntPtr phCamera, int lReleaseMode, ref int plShotOpt, ref int plAfStatus);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_GetReleaseStatus_Delegate(IntPtr phCamera, ref int plReleaseStatus);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_CapReleaseEx_Delegate(IntPtr phCamera, ref int plNumReleaseMode, IntPtr plReleaseMode);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_ReleaseEx_Delegate(IntPtr phCamera, int lReleaseMode, ref int plShotOpt, ref int plAfStatus);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_ReadImageInfo_Delegate(IntPtr phCamera, ref XSDK_ImageInformation pImgInfo);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_ReadPreview_Delegate(IntPtr phCamera, IntPtr pData, uint lDataSize);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_ReadImage_Delegate(IntPtr phCamera, IntPtr pData, uint lDataSize);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_DeleteImage_Delegate(IntPtr phCamera);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_GetBufferCapacity_Delegate(IntPtr phCamera, ref int plShootFrameNum, ref int plTotalFrameNum);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_CapAEMode_Delegate(IntPtr phCamera, ref int plNumAEMode, IntPtr plAEMode);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_SetAEMode_Delegate(IntPtr phCamera, int lAEMode);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_GetAEMode_Delegate(IntPtr phCamera, ref int plAEMode);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_CapShutterSpeed_Delegate(IntPtr phCamera, ref int plNumShutterSpeed, IntPtr plShutterSpeed, ref int plBulbCapable);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_SetShutterSpeed_Delegate(IntPtr phCamera, int lShutterSpeed, int lBulb);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_GetShutterSpeed_Delegate(IntPtr phCamera, ref int plShutterSpeed, ref int plBulb);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_CapExposureBias_Delegate(IntPtr phCamera, ref int plNumExposureBias, IntPtr plExposureBias);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_SetExposureBias_Delegate(IntPtr phCamera, int lExposureBias);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_GetExposureBias_Delegate(IntPtr phCamera, ref int plExposureBias);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_CapDynamicRange_Delegate(IntPtr phCamera, ref int plNumDynamicRange, IntPtr plDynamicRange);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_SetDynamicRange_Delegate(IntPtr phCamera, int lDynamicRange);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_GetDynamicRange_Delegate(IntPtr phCamera, ref int plDynamicRange);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_CapSensitivity_Delegate(IntPtr phCamera, ref int plNumSensitivity, IntPtr plSensitivity);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_SetSensitivity_Delegate(IntPtr phCamera, int lSensitivity);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_GetSensitivity_Delegate(IntPtr phCamera, ref int plSensitivity);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_CapMeteringMode_Delegate(IntPtr phCamera, ref int plNumMeteringMode, IntPtr plMeteringMode);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_SetMeteringMode_Delegate(IntPtr phCamera, int lMeteringMode);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_GetMeteringMode_Delegate(IntPtr phCamera, ref int plMeteringMode);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_CapLensZoomPos_Delegate(IntPtr phCamera, ref int plNumZoomPos, IntPtr plZoomPos, IntPtr plFocusLength, IntPtr pl35mmFocusLength);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_SetLensZoomPos_Delegate(IntPtr phCamera, int lZoomPos);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_GetLensZoomPos_Delegate(IntPtr phCamera, ref int plZoomPos);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_CapAperture_Delegate(IntPtr phCamera, int lZoomPos, ref int plNumAperture, IntPtr plFNumber);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_SetAperture_Delegate(IntPtr phCamera, int lFNumber);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_GetAperture_Delegate(IntPtr phCamera, ref int plFNumber);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_CapWBMode_Delegate(IntPtr phCamera, ref int plNumWBMode, IntPtr plWBMode);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_SetWBMode_Delegate(IntPtr phCamera, int lWBMode);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_GetWBMode_Delegate(IntPtr phCamera, ref int plWBMode);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_CapWBColorTemp_Delegate(IntPtr phCamera, ref int plNumWBColorTemp, IntPtr plWBColorTemp);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_SetWBColorTemp_Delegate(IntPtr phCamera, int lColorTemp);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_GetWBColorTemp_Delegate(IntPtr phCamera, ref int plColorTemp);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_CapMediaRecord_Delegate(IntPtr phCamera, ref int plNumMediaRecord, IntPtr plMediaRecord);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_SetMediaRecord_Delegate(IntPtr phCamera, int lMediaRecord);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_GetMediaRecord_Delegate(IntPtr phCamera, ref int plMediaRecord);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_CapForceMode_Delegate(IntPtr phCamera, ref int plNumForceMode, IntPtr plForceMode);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_SetForceMode_Delegate(IntPtr phCamera, int lForceMode);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_SetBackupSettings_Delegate(IntPtr phCamera, int lSize, IntPtr pBackup);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_GetBackupSettings_Delegate(IntPtr phCamera, ref int plSize, IntPtr pBackup);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_CapDriveMode_Delegate(IntPtr phCamera, ref int plNumDriveMode, IntPtr plDriveMode);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_SetDriveMode_Delegate(IntPtr phCamera, int lDriveMode);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_GetDriveMode_Delegate(IntPtr phCamera, ref int plDriveMode);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_CapMode_Delegate(IntPtr phCamera, ref int plNumMode, IntPtr plMode);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_SetMode_Delegate(IntPtr phCamera, int lMode);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_GetMode_Delegate(IntPtr phCamera, ref int plMode);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_GetRecordingStatus_Delegate(IntPtr phCamera, ref int plStatus);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_CapMovieShutterSpeed_Delegate(IntPtr phCamera, ref int plNumShutterSpeed, IntPtr pllShutterSpeed);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_SetMovieShutterSpeede_Delegate(IntPtr phCamera, long llShutterSpeed);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_GetMovieShutterSpeed_Delegate(IntPtr phCamera, ref long pllShutterSpeed);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_CapMovieExposureBias_Delegate(IntPtr phCamera, ref int plNumExposureBias, IntPtr plExposureBias);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_SetMovieExposureBias_Delegate(IntPtr phCamera, int lExposureBias);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_GetMovieExposureBias_Delegate(IntPtr phCamera, ref int plExposureBias);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_CapMovieSensitivity_Delegate(IntPtr phCamera, ref int plNumSensitivity, IntPtr plSensitivity);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_SetMovieSensitivity_Delegate(IntPtr phCamera, int lSensitivity);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_GetMovieSensitivity_Delegate(IntPtr phCamera, ref int plSensitivity);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_CapMovieAperture_Delegate(IntPtr phCamera, ref int plNumAperture, IntPtr plFNumber);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_SetMovieAperture_Delegate(IntPtr phCamera, int lFNumber);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_GetMovieAperture_Delegate(IntPtr phCamera, ref int plFNumber);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_CapMovieDynamicRange_Delegate(IntPtr phCamera, ref int plNumDynamicRange, IntPtr plDynamicRange);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_SetMovieDynamicRange_Delegate(IntPtr phCamera, int lDynamicRange);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_GetMovieDynamicRange_Delegate(IntPtr phCamera, ref int plDynamicRange);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_CapMovieMeteringMode_Delegate(IntPtr phCamera, ref int plNumMeteringMode, IntPtr plMeteringMode);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_SetMovieMeteringMode_Delegate(IntPtr phCamera, int lMeteringMode);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_GetMovieMeteringMode_Delegate(IntPtr phCamera, ref int plMeteringMode);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_CapMovieWBMode_Delegate(IntPtr phCamera, ref int plNumWBMode, IntPtr plWBMode);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_SetMovieWBMode_Delegate(IntPtr phCamera, int lWBMode);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_GetMovieWBMode_Delegate(IntPtr phCamera, ref int plWBMode);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_CapMovieWBColorTemp_Delegate(IntPtr phCamera, ref int plNumWBColorTemp, IntPtr plWBColorTemp);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_SetMovieWBColorTemp_Delegate(IntPtr phCamera, int lWBColorTemp);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_GetMovieWBColorTemp_Delegate(IntPtr phCamera, ref int plWBColorTemp);
	#endregion // Delegate for C# of model common API.

	/// <summary>
	///  Initializes and starts use of the SDK.
	/// </summary>
	/// <returns>
	/// API result.
	/// </returns>
	public long XSDK_Init()
	{
		long ret = XSDK_ERROR;

		if (m_pfnInit != IntPtr.Zero)
		{
			XSDK_Init_Delegate xsdkApi = (XSDK_Init_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnInit, typeof(XSDK_Init_Delegate));
			ret = xsdkApi(m_hModule);
		}

		return ret;
	}

	/// <summary>
	/// Finalizes and terminates use of the SDK.
	/// </summary>
	/// <returns>
	/// API result.
	/// </returns>
	public long XSDK_Exit()
	{
		long ret = XSDK_ERROR;

		if (m_pfnExit != IntPtr.Zero)
		{
			XSDK_Exit_Delegate xsdkApi = (XSDK_Exit_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnExit, typeof(XSDK_Exit_Delegate));
			ret = xsdkApi();
		}

		return ret;
	}

	/// <summary>
	/// Enumerates available cameras and generate a connected camera list.
	/// </summary>
	/// <param name="lInterface">How to search for a camera.</param>
	/// <param name="pInterface">Search criteria IP address.</param>
	/// <param name="pDeviceName">Search criteria device name.</param>
	/// <param name="plCount">Returns the number of devices detected.</param>
	/// <returns>
	/// API result.
	/// </returns>
	public long XSDK_Detect(int lInterface, string pInterface, string pDeviceName, ref int plCount)
	{
		long ret = XSDK_ERROR;

		plCount = 0;
		if (m_pfnDetect != IntPtr.Zero)
		{
			XSDK_Detect_Delegate xsdkApi = (XSDK_Detect_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnDetect, typeof(XSDK_Detect_Delegate));
			ret = xsdkApi(lInterface, pInterface, pDeviceName, ref plCount);
		}

		return ret;
	}

	/// <summary>
	/// Update the connected camera list.
	/// </summary>
	/// <param name="lInterface">How to search for a camera.</param>
	/// <param name="pInterface">Search criteria IP address.</param>
	/// <param name="pDeviceName">Search criteria device name.</param>
	/// <param name="plCount">Returns the number of devices detected.</param>
	/// <param name="pCameraList">Returns camera information.</param>
	/// <returns>
	/// API result.
	/// </returns>
	public long XSDK_Append(int lInterface, string pInterface, string pDeviceName, ref int plCount, List<XSDK_CameraList> pCameraList)
	{
		long ret = XSDK_ERROR;

		plCount = 128;
		int size = Marshal.SizeOf(typeof(XSDK_CameraList));
		IntPtr ptr = Marshal.AllocHGlobal(size * (int)plCount);
		if (m_pfnAppend != IntPtr.Zero)
		{
			XSDK_Append_Delegate xsdkApi = (XSDK_Append_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnAppend, typeof(XSDK_Append_Delegate));
			ret = xsdkApi(lInterface, pInterface, pDeviceName, ref plCount, ptr);
		}
		for (int i = 0; i < plCount; i++)
		{
			pCameraList.Add((XSDK_CameraList)Marshal.PtrToStructure(ptr + i * size, typeof(XSDK_CameraList)));
		}
		return ret;
	}


	/// <summary>
	/// Establishes a session between the camera and the computer.
	/// </summary>
	/// <param name="pDevice">Specify index or IP address.</param>
	/// <param name="phCamera">Returns the camera handle when the function completes.</param>
	/// <param name="plCameraMode">Returns a bitmap of camera features compatible with tethering operations.</param>
	/// <param name="pOption">nullptr</param>
	/// <returns>
	/// API result.
	/// </returns>
	public long XSDK_OpenEx(string pDevice, ref IntPtr phCamera, ref int plCameraMode, IntPtr pOption)
	{
		long ret = XSDK_ERROR;

		phCamera = IntPtr.Zero;
		plCameraMode = 0;
		if (m_pfnOpenEx != IntPtr.Zero)
		{
			XSDK_OpenEx_Delegate xsdkApi = (XSDK_OpenEx_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnOpenEx, typeof(XSDK_OpenEx_Delegate));
			ret = xsdkApi(pDevice, ref phCamera, ref plCameraMode, pOption);
		}

		return ret;
	}

	/// <summary>
	/// Disestablish a session between the camera and the computer.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <returns>
	/// API result.
	/// </returns>
	public long XSDK_Close(IntPtr phCamera)
	{
		long ret = XSDK_ERROR;

		if (m_pfnClose != IntPtr.Zero)
		{
			XSDK_Close_Delegate xsdkApi = (XSDK_Close_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnClose, typeof(XSDK_Close_Delegate));
			ret = xsdkApi(phCamera);
		}

		return ret;
	}

	/// <summary>
	/// Disestablish a session between the camera and the computer, and shut the camera down.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <returns>API result.</returns>
	public long XSDK_PowerOFF(IntPtr phCamera)
	{
		long ret = XSDK_ERROR;

		if (m_pfnPowerOFF != IntPtr.Zero)
		{
			XSDK_PowerOFF_Delegate xsdkApi = (XSDK_PowerOFF_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnPowerOFF, typeof(XSDK_PowerOFF_Delegate));
			ret = xsdkApi(phCamera);
		}

		return ret;
	}

	/// <summary>
	/// Gets the detailed result of the last called function.
	/// </summary>
	/// <param name="phCamera">Camera handle , phCamera can be set to NULL.</param>
	/// <param name="plAPICode">The last called API code.</param>
	/// <param name="plERRCode">See the ERROR CODES for details.</param>
	/// <returns>
	/// API result.
	/// </returns>
	public long XSDK_GetErrorNumber(IntPtr phCamera, ref int plAPICode, ref int plERRCode)
	{
		long ret = XSDK_ERROR;

		plAPICode = 0;
		plERRCode = 0;
		if (m_pfnGetErrorNumber != IntPtr.Zero)
		{
			XSDK_GetErrorNumber_Delegate xsdkApi = (XSDK_GetErrorNumber_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnGetErrorNumber, typeof(XSDK_GetErrorNumber_Delegate));
			ret = xsdkApi(phCamera, ref plAPICode, ref plERRCode);
		}

		return ret;
	}

	/// <summary>
	/// Gets version numbers in a string format.
	/// </summary>
	/// <param name="pVersionStringe">Returns the SDK version as a string.</param>
	/// <returns>API result.</returns>
	public long XSDK_GetVersionString(ref string pVersionStringe)
	{
		long ret = XSDK_ERROR;

		pVersionStringe = "";
		if (m_pfnGetVersionString != IntPtr.Zero)
		{
			XSDK_GetVersionString_Delegate xsdkApi = (XSDK_GetVersionString_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnGetVersionString, typeof(XSDK_GetVersionString_Delegate));
			System.Text.StringBuilder sb = new System.Text.StringBuilder(256);
			ret = xsdkApi(sb);
			if (ret == (int)FUJI_SDK_ERR.FUJI_SDK_ERR_OK)
			{
				pVersionStringe = sb.ToString();
			}
		}

		return ret;
	}

	/// <summary>
	/// Gets details of the busy error when the plERRCode returned by a call to XSDK_GetErrorNumber is XSDK_ERRCODE_RUNNING_OTHER_FUNCTION.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="plERRCode">Returns the function currently running.</param>
	/// <returns>API result.</returns>
	public long XSDK_GetErrorDetails(IntPtr phCamera, ref int plERRCode)
	{
		long ret = XSDK_ERROR;

		plERRCode = 0;
		if (m_pfnGetErrorDetails != IntPtr.Zero)
		{
			XSDK_GetErrorDetails_Delegate xsdkApi = (XSDK_GetErrorDetails_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnGetErrorDetails, typeof(XSDK_GetErrorDetails_Delegate));
			ret = xsdkApi(phCamera, ref plERRCode);
		}

		return ret;
	}

	/// <summary>
	/// Gets information about the connected camera.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="pDevInfo">Returns camera information.</param>
	/// <returns>API result.</returns>
	public long XSDK_GetDeviceInfo(IntPtr phCamera, ref XSDK_DeviceInformation pDevInfo)
	{
		long ret = XSDK_ERROR;

		if (m_pfnGetDeviceInfo != IntPtr.Zero)
		{
			XSDK_GetDeviceInfo_Delegate xsdkApi = (XSDK_GetDeviceInfo_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnGetDeviceInfo, typeof(XSDK_GetDeviceInfo_Delegate));
			ret = xsdkApi(phCamera, ref pDevInfo);
		}

		return ret;
	}

	/// <summary>
	/// Assigns a device-unique name to the camera.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="pDeviceName">A unique name. Up to 32 characters including the NULL terminator.</param>
	/// <returns>API result.</returns>
	public long XSDK_WriteDeviceName(IntPtr phCamera, string pDeviceName)
	{
		long ret = XSDK_ERROR;

		if (m_pfnWriteDeviceName != IntPtr.Zero)
		{
			XSDK_WriteDeviceName_Delegate xsdkApi = (XSDK_WriteDeviceName_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnWriteDeviceName, typeof(XSDK_WriteDeviceName_Delegate));
			ret = xsdkApi(phCamera, pDeviceName);
		}

		return ret;
	}

	/// <summary>
	/// Returns the camera firmware version as a string.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="pFirmwareVersion">Returns the camera firmware version as a string. </param>
	/// <returns>API result.</returns>
	public long XSDK_GetFirmwareVersion(IntPtr phCamera, ref string pFirmwareVersion)
	{
		long ret = XSDK_ERROR;

		pFirmwareVersion = "";
		if (m_pfnGetFirmwareVersion != IntPtr.Zero)
		{
			XSDK_GetFirmwareVersion_Delegate xsdkApi = (XSDK_GetFirmwareVersion_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnGetFirmwareVersion, typeof(XSDK_GetFirmwareVersion_Delegate));
			System.Text.StringBuilder sb = new System.Text.StringBuilder(256);
			ret = xsdkApi(phCamera, sb);
			if (ret == (int)FUJI_SDK_ERR.FUJI_SDK_ERR_OK)
			{
				pFirmwareVersion = sb.ToString();
			}
		}

		return ret;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="pLensInfo">Returns lens information.</param>
	/// <returns>API result.</returns>
	public long XSDK_GetLensInfo(IntPtr phCamera, ref XSDK_LensInformation pLensInfo)
	{
		long ret = XSDK_ERROR;
		if (m_pfnGetLensInfo != IntPtr.Zero)
		{
			XSDK_GetLensInfo_Delegate xsdkApi = (XSDK_GetLensInfo_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnGetLensInfo, typeof(XSDK_GetLensInfo_Delegate));
			ret = xsdkApi(phCamera, ref pLensInfo);
		}

		return ret;
	}

	/// <summary>
	/// Gets the firmware version of the lens attached to the camera in a string format.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="pLensVersion">Returns the lens firmware version as a string.</param>
	/// <returns>API result.</returns>
	public long XSDK_GetLensVersion(IntPtr phCamera, ref string pLensVersion)
	{
		long ret = XSDK_ERROR;

		pLensVersion = "";
		if (m_pfnGetLensVersion != IntPtr.Zero)
		{
			XSDK_GetLensVersion_Delegate xsdkApi = (XSDK_GetLensVersion_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnGetLensVersion, typeof(XSDK_GetLensVersion_Delegate));
			System.Text.StringBuilder sb = new System.Text.StringBuilder(256);
			ret = xsdkApi(phCamera, sb);
			if (ret == (int)FUJI_SDK_ERR.FUJI_SDK_ERR_OK)
			{
				pLensVersion = sb.ToString();
			}
		}

		return ret;
	}

	/// <summary>
	/// Gets information about the connected camera and supported APIs by the camera.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="pDevInfo">Returns camera information.</param>
	/// <param name="plNumAPICode">Returns the number of APICode supported.</param>
	/// <param name="plAPICode">If not NULL, plAPICode will return a list of APICode supported.</param>
	/// <returns>API result.</returns>
	public long XSDK_GetDeviceInfoEx(IntPtr phCamera, ref XSDK_DeviceInformation pDevInfo, ref int plNumAPICode, ref int[] plAPICode)
	{
		long ret = XSDK_ERROR;

		if (m_pfnGetDeviceInfoEx != IntPtr.Zero)
		{
				int num = plNumAPICode;
				IntPtr parray = IntPtr.Zero;
				XSDK_GetDeviceInfoEx_Delegate xsdkApi = (XSDK_GetDeviceInfoEx_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnGetDeviceInfoEx, typeof(XSDK_GetDeviceInfoEx_Delegate));
				System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
				try { }
				finally
				{
					if (num != 0)
					{
						parray = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * plNumAPICode);
					}
					try
					{
						ret = xsdkApi(phCamera, ref pDevInfo, ref plNumAPICode, parray);
						if (num != 0)
						{
							int max = plNumAPICode;
							if (num < max)
							{
								max = num;
							}
							Marshal.Copy(parray, plAPICode, 0, max);
						}
					}
					finally
					{
						if (num != 0)
						{
							Marshal.FreeCoTaskMem(parray);
						}
					}
				}

			}

		return ret;
	}

	/// <summary>
	/// Queries supported operation modes.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="plNumPriorityMode">Returns the supported number of settings for XSDK_SetPriorityMode. </param>
	/// <param name="plPriorityMode">If not NULL, plPriorityMode will return a list of the XSDK_SetPriorityMode settings supported.</param>
	/// <returns>API result.</returns>
	public long XSDK_CapPriorityMode(IntPtr phCamera, ref int plNumPriorityMode, ref int[] plPriorityMode)
	{
		long ret = XSDK_ERROR;

		if (m_pfnCapPriorityMode != IntPtr.Zero)
		{
			int num = plNumPriorityMode;
			IntPtr parray = IntPtr.Zero;
			XSDK_CapPriorityMode_Delegate xsdkApi = (XSDK_CapPriorityMode_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnCapPriorityMode, typeof(XSDK_CapPriorityMode_Delegate));
			System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
			try { }
			finally
			{
				if (num != 0)
				{
					parray = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * plNumPriorityMode);
				}
				try
				{
					ret = xsdkApi(phCamera, ref plNumPriorityMode, parray);
					if (num != 0)
					{
						int max = plNumPriorityMode;
						if (num < max)
						{
							max = num;
						}
						Marshal.Copy(parray, plPriorityMode, 0, max);
					}
				}
				finally
				{
					if (num != 0)
					{
						Marshal.FreeCoTaskMem(parray);
					}
				}
			}
		}
		else
		{
			plNumPriorityMode = 0;
		}

		return ret;
	}

	/// <summary>
	/// Sets the camera operation mode.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="lPriorityMode">The priority mode.</param>
	/// <returns>API result.</returns>
	public long XSDK_SetPriorityMode(IntPtr phCamera, int lPriorityMode)
	{
		long ret = XSDK_ERROR;

		if (m_pfnSetPriorityMode != IntPtr.Zero)
		{
			XSDK_SetPriorityMode_Delegate xsdkApi = (XSDK_SetPriorityMode_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnSetPriorityMode, typeof(XSDK_SetPriorityMode_Delegate));
			ret = xsdkApi(phCamera, lPriorityMode);
		}

		return ret;
	}

	/// <summary>
	/// Gets the current camera operation mode.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="plPriorityMode">The priority mode.</param>
	/// <returns>API result.</returns>
	public long XSDK_GetPriorityMode(IntPtr phCamera, ref int plPriorityMode)
	{
		long ret = XSDK_ERROR;

		plPriorityMode = 0;
		if (m_pfnGetPriorityMode != IntPtr.Zero)
		{
			XSDK_GetPriorityMode_Delegate xsdkApi = (XSDK_GetPriorityMode_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnGetPriorityMode, typeof(XSDK_GetPriorityMode_Delegate));
			ret = xsdkApi(phCamera, ref plPriorityMode);
		}

		return ret;
	}

	/// <summary>
	/// Queries supported release-related modes when the system is in PC priority mode.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="plNumReleaseMode">The number of supported XSDK_Release settings.</param>
	/// <param name="plReleaseMode">If plReleaseMode is NULL, the function will return only plNumReleaseMode with the number of supported XSDK_Release lReleaseMode settings. </param>
	/// <returns>API result.</returns>
	public long XSDK_CapRelease(IntPtr phCamera, ref int plNumReleaseMode, ref int[] plReleaseMode)
	{
		long ret = XSDK_ERROR;

		if (m_pfnCapRelease != IntPtr.Zero)
		{
			int num = plNumReleaseMode;
			IntPtr parray = IntPtr.Zero;
			XSDK_CapRelease_Delegate xsdkApi = (XSDK_CapRelease_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnCapRelease, typeof(XSDK_CapRelease_Delegate));
			System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
			try { }
			finally
			{
				if (num != 0)
				{
					if (num != 0)
					{
						parray = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * plNumReleaseMode);
					}
				}
				try
				{
					ret = xsdkApi(phCamera, ref plNumReleaseMode, parray);
					if (num != 0)
					{
						int max = plNumReleaseMode;
						if (num < max)
						{
							max = num;
						}
						Marshal.Copy(parray, plReleaseMode, 0, max);
					}
				}
				finally
				{
					if (num != 0)
					{
						Marshal.FreeCoTaskMem(parray);
					}
				}
			}
		}

		return ret;
	}

	/// <summary>
	/// Triggers shutter release-related operations when the system is in PC priority mode.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="lReleaseMode">If plReleaseMode is NULL, the function will return only plNumReleaseMode with the number of supported XSDK_Release lReleaseMode settings.</param>
	/// <param name="plShotOpt">Specifies the number of pictures to be taken per burst in burst photography modes and returns the number of pictures actually taken.</param>
	/// <param name="plAfStatus">
	/// Sometimes returns AF status when the function is called for S1- and S2-related operations.
	/// Calls to measure custom white balance return autoexposure status.
	/// </param>
	/// <returns>API result.</returns>
	public long XSDK_Release(IntPtr phCamera, int lReleaseMode, ref int plShotOpt, ref int plAfStatus)
	{
		long ret = XSDK_ERROR;

		plShotOpt = 0;
		plAfStatus = 0;
		if (m_pfnRelease != IntPtr.Zero)
		{
			XSDK_Release_Delegate xsdkApi = (XSDK_Release_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnRelease, typeof(XSDK_Release_Delegate));
			ret = xsdkApi(phCamera, lReleaseMode, ref plShotOpt, ref plAfStatus);
		}

		return ret;
	}

	/// <summary>
	/// Gets the status of release operation.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="plReleaseStatus">Returns release status via bit ON/OFF.</param>
	/// <returns>API result.</returns>
	public long XSDK_GetReleaseStatus(IntPtr phCamera, ref int plReleaseStatus)
	{
		long ret = XSDK_ERROR;
		
		plReleaseStatus = 0;
		if (m_pfnGetReleaseStatus != IntPtr.Zero)
		{
			XSDK_GetReleaseStatus_Delegate xsdkApi = (XSDK_GetReleaseStatus_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnGetReleaseStatus, typeof(XSDK_GetReleaseStatus_Delegate));
			ret = xsdkApi(phCamera, ref plReleaseStatus);
		}

		return ret;
	}

	/// <summary>
	/// Queries supported release-related modes when the system is in CAMERA priority mode.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="plNumReleaseMode">The number of supported XSDK_Release settings.</param>
	/// <param name="plReleaseMode">
	/// If pulReleaseMode is NULL, the function will return only plNumReleaseMode with the number of supported XSDK_ReleaseEx ulReleaseMode settings.
	/// Otherwise it will return pulReleaseMode with a list of the XSDK_ReleaseEx ulReleaseMode settings supported. 
	/// </param>
	/// <returns>API result.</returns>
	public long XSDK_CapReleaseEx(IntPtr phCamera, ref int plNumReleaseMode, ref int[] plReleaseMode)
	{
		long ret = XSDK_ERROR;

		if (m_pfnCapReleaseEx != IntPtr.Zero)
		{
			int num = plNumReleaseMode;
			IntPtr parray = IntPtr.Zero;
			XSDK_CapReleaseEx_Delegate xsdkApi = (XSDK_CapReleaseEx_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnCapReleaseEx, typeof(XSDK_CapReleaseEx_Delegate));
			System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
			try { }
			finally
			{
				if (num != 0)
				{
					if (num != 0)
					{
						parray = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * plNumReleaseMode);
					}
				}
				try
				{
					ret = xsdkApi(phCamera, ref plNumReleaseMode, parray);
					if (num != 0)
					{
						int max = plNumReleaseMode;
						if (num < max)
						{
							max = num;
						}
						Marshal.Copy(parray, plReleaseMode, 0, max);
					}
				}
				finally
				{
					if (num != 0)
					{
						Marshal.FreeCoTaskMem(parray);
					}
				}
			}
		}

		return ret;
	}

		/// <summary>
		/// Triggers shutter release-related operations when the system is in camera priority mode.
		/// </summary>
		/// <param name="phCamera">Camera handle.</param>
		/// <param name="lReleaseMode">
		/// If pulReleaseMode is NULL, the function will return only plNumReleaseMode with the number of supported XSDK_ReleaseEx ulReleaseMode settings.
		/// Otherwise it will return pulReleaseMode with a list of the XSDK_ReleaseEx ulReleaseMode settings supported. 
		/// </param>
		/// <param name="plShotOpt">Specifies the number of pictures to be taken per burst in burst photography modes and returns the number of pictures actually taken.</param>
		/// <param name="plAfStatus">
		/// Sometimes returns AF status when the function is called for S1- and S2-related operations.
		/// Calls to measure custom white balance return autoexposure status.
		/// </param>
		/// <returns>API result.</returns>
		public long XSDK_ReleaseEx(IntPtr phCamera, int lReleaseMode, ref int plShotOpt, ref int plAfStatus)
		{
			long ret = XSDK_ERROR;

			plShotOpt = 0;
			plAfStatus = 0;
			if (m_pfnReleaseEx != IntPtr.Zero)
			{
				XSDK_ReleaseEx_Delegate xsdkApi = (XSDK_ReleaseEx_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnReleaseEx, typeof(XSDK_ReleaseEx_Delegate));
				ret = xsdkApi(phCamera, lReleaseMode, ref plShotOpt, ref plAfStatus);
			}

			return ret;
		}

	/// <summary>
	/// Gets information from an image from the top of the in-camera buffer.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="pImgInfo">Returns the image information.</param>
	/// <returns>API result.</returns>
	public long XSDK_ReadImageInfo(IntPtr phCamera, ref XSDK_ImageInformation pImgInfo)
	{
		long ret = XSDK_ERROR;

		if (m_pfnReadImageInfo != IntPtr.Zero)
		{
			XSDK_ReadImageInfo_Delegate xsdkApi = (XSDK_ReadImageInfo_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnReadImageInfo, typeof(XSDK_ReadImageInfo_Delegate));
			ret = xsdkApi(phCamera, ref pImgInfo);
		}

		return ret;
	}

	/// <summary>
	/// Gets a low-resolution image of the image from the top of the in-camera buffer.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="pData">The read-image buffer.</param>
	/// <param name="lDataSize">The number of bytes allocated for pData.</param>
	/// <returns>API result.</returns>
	public long XSDK_ReadPreview(IntPtr phCamera, ref byte[] pData, int lDataSize)
	{
		long ret = XSDK_ERROR;

		if (m_pfnReadPreview != IntPtr.Zero && lDataSize != 0)
		{
			XSDK_ReadPreview_Delegate xsdkApi = (XSDK_ReadPreview_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnReadPreview, typeof(XSDK_ReadPreview_Delegate));
			System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
			try { }
			finally
			{
				IntPtr parray = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(byte)) * lDataSize);
				try
				{
					ret = xsdkApi(phCamera, parray, (uint)lDataSize);
					if (ret == (int)FUJI_SDK_ERR.FUJI_SDK_ERR_OK)
					{
						Marshal.Copy(parray, pData, 0, lDataSize);
					}
				}
				finally
				{
					Marshal.FreeCoTaskMem(parray);
				}
			}
		}

		return ret;
	}

	/// <summary>
	/// Gets a captured image from the top of the in-camera buffer and deletes it from the buffer.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="pData">The read-image buffer.</param>
	/// <param name="lDataSize">The number of bytes allocated for pData.</param>
	/// <returns>API result.</returns>
	public long XSDK_ReadImage(IntPtr phCamera, ref byte[] pData, int lDataSize)
	{
		long ret = XSDK_ERROR;

		if (m_pfnReadImage != IntPtr.Zero && lDataSize != 0)
		{
			XSDK_ReadImage_Delegate xsdkApi = (XSDK_ReadImage_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnReadImage, typeof(XSDK_ReadImage_Delegate));
			System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
			try { }
			finally
			{
				IntPtr parray = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(byte)) * lDataSize);
				try
				{
					ret = xsdkApi(phCamera, parray, (uint)lDataSize);
					if (ret == (int)FUJI_SDK_ERR.FUJI_SDK_ERR_OK)
					{
						Marshal.Copy(parray, pData, 0, lDataSize);
					}
				}
				finally
				{
					Marshal.FreeCoTaskMem(parray);
				}
			}
		}

		return ret;
	}

	/// <summary>
	/// Deletes a captured image from the top of the in-camera buffer.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <returns>API result.</returns>
	public long XSDK_DeleteImage(IntPtr phCamera)
	{
		long ret = XSDK_ERROR;

		if (m_pfnDeleteImage != IntPtr.Zero)
		{
			XSDK_DeleteImage_Delegate xsdkApi = (XSDK_DeleteImage_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnDeleteImage, typeof(XSDK_DeleteImage_Delegate));
			ret = xsdkApi(phCamera);
		}

		return ret;
	}

	/// <summary>
	/// Gets the status of the in-camera buffer.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="plShootFrameNum">Returns the number of frames shot (the number of captured images).</param>
	/// <param name="plTotalFrameNum">Returns the total number of frames.</param>
	/// <returns>API result.</returns>
	public long XSDK_GetBufferCapacity(IntPtr phCamera, ref int plShootFrameNum, ref int plTotalFrameNum)
	{
		long ret = XSDK_ERROR;

		if (m_pfnGetBufferCapacity != IntPtr.Zero)
		{
			XSDK_GetBufferCapacity_Delegate xsdkApi = (XSDK_GetBufferCapacity_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnGetBufferCapacity, typeof(XSDK_GetBufferCapacity_Delegate));
			ret = xsdkApi(phCamera, ref plShootFrameNum, ref plTotalFrameNum);
		}
		else
		{
			plShootFrameNum = 0;
			plTotalFrameNum = 0;
		}

		return ret;
	}

	/// <summary>
	/// Queries supported exposure modes (P/A/S/M) to set.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="plNumAEMode">Returns the number of supported XSDK_SetAEMode settings.</param>
	/// <param name="plAEMode">If not NULL, plAEMode will return a list of the XSDK_SetAEMode settings supported.</param>
	/// <returns>API result.</returns>
	public long XSDK_CapAEMode(IntPtr phCamera, ref int plNumAEMode, ref int[] plAEMode)
	{
		long ret = XSDK_ERROR;

		if (m_pfnCapAEMode != IntPtr.Zero)
		{
			int num = plNumAEMode;
			IntPtr parray = IntPtr.Zero;
			XSDK_CapAEMode_Delegate xsdkApi = (XSDK_CapAEMode_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnCapAEMode, typeof(XSDK_CapAEMode_Delegate));
			System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
			try { }
			finally
			{
				if (num != 0)
				{
					parray = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * plNumAEMode);
				}
				try
				{
					ret = xsdkApi(phCamera, ref plNumAEMode, parray);
					if (num != 0)
					{
						int max = plNumAEMode;
						if (num < max)
						{
							max = num;
						}
						Marshal.Copy(parray, plAEMode, 0, max);
					}
				}
				finally
				{
					if (num != 0)
					{
						Marshal.FreeCoTaskMem(parray);
					}
				}
			}
		}
		else
		{
			plNumAEMode = 0;
		}

		return ret;
	}

	/// <summary>
	/// Sets the exposure mode setting.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="lAEMode">The exposure mode to which the camera will be set.</param>
	/// <returns>API result.</returns>
	public long XSDK_SetAEMode(IntPtr phCamera, int lAEMode)
	{
		long ret = XSDK_ERROR;

		if (m_pfnSetAEMode != IntPtr.Zero)
		{
			XSDK_SetAEMode_Delegate xsdkApi = (XSDK_SetAEMode_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnSetAEMode, typeof(XSDK_SetAEMode_Delegate));
			ret = xsdkApi(phCamera, lAEMode);
		}

		return ret;
	}

	/// <summary>
	/// Gets the exposure mode setting.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="plAEMode">Returns the exposure mode. </param>
	/// <returns>API result.</returns>
	public long XSDK_GetAEMode(IntPtr phCamera, ref int plAEMode)
	{
		long ret = XSDK_ERROR;

		plAEMode = 0;
		if (m_pfnGetAEMode != IntPtr.Zero)
		{
			XSDK_GetAEMode_Delegate xsdkApi = (XSDK_GetAEMode_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnGetAEMode, typeof(XSDK_GetAEMode_Delegate));
			ret = xsdkApi(phCamera, ref plAEMode);
		}

		return ret;
	}

	/// <summary>
	/// Queries supported shutter speeds to set.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="plNumShutterSpeed">Returns the number of supported XSDK_SetShutterSpeed settings.</param>
	/// <param name="plShutterSpeed">If not NULL, plShutterSpeed will return a list of the XSDK_SetShutterSpeed settings supported. </param>
	/// <param name="plBulbCapable">BULB Capable.</param>
	/// <returns>API result.</returns>
	public long XSDK_CapShutterSpeed(IntPtr phCamera, ref int plNumShutterSpeed, ref int[] plShutterSpeed, ref int plBulbCapable)
	{
		long ret = XSDK_ERROR;

		if (m_pfnCapShutterSpeed != IntPtr.Zero)
		{
			int num = plNumShutterSpeed;
			IntPtr parray = IntPtr.Zero;
			XSDK_CapShutterSpeed_Delegate xsdkApi = (XSDK_CapShutterSpeed_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnCapShutterSpeed, typeof(XSDK_CapShutterSpeed_Delegate));
			System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
			try { }
			finally
			{
				if (num != 0)
				{
					parray = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * plNumShutterSpeed);
				}
				try
				{
					ret = xsdkApi(phCamera, ref plNumShutterSpeed, parray, ref plBulbCapable);
					if (num != 0)
					{
						int max = plNumShutterSpeed;
						if (num < max)
						{
							max = num;
						}
						Marshal.Copy(parray, plShutterSpeed, 0, max);
					}
				}
				finally
				{
					if (num != 0)
					{
						Marshal.FreeCoTaskMem(parray);
					}
				}
			}
		}
		else
		{
			plNumShutterSpeed = 0;
			plBulbCapable = 0;
		}

		return ret;
	}

	/// <summary>
	/// Sets the shutter speed value.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="lShutterSpeed">The shutter speed to which the camera will be set.</param>
	/// <param name="lBulb">BULB Capable.</param>
	/// <returns>API result.</returns>
	public long XSDK_SetShutterSpeed(IntPtr phCamera, int lShutterSpeed, int lBulb)
	{
		long ret = XSDK_ERROR;

		if (m_pfnSetShutterSpeed != IntPtr.Zero)
		{
			XSDK_SetShutterSpeed_Delegate xsdkApi = (XSDK_SetShutterSpeed_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnSetShutterSpeed, typeof(XSDK_SetShutterSpeed_Delegate));
			ret = xsdkApi(phCamera, lShutterSpeed, lBulb);
		}

		return ret;
	}

	/// <summary>
	/// Gets the shutter speed setting.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="plShutterSpeed">Returns the shutter speed. </param>
	/// <param name="plBulb">Returns  the shutter speed setting is BULB or not.</param>
	/// <returns>API result.</returns>
	public long XSDK_GetShutterSpeed(IntPtr phCamera, ref int plShutterSpeed, ref int plBulb)
	{
		long ret = XSDK_ERROR;

		plShutterSpeed = 0;
		plBulb = 0;
		if (m_pfnGetShutterSpeed != IntPtr.Zero)
		{
			XSDK_GetShutterSpeed_Delegate xsdkApi = (XSDK_GetShutterSpeed_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnGetShutterSpeed, typeof(XSDK_GetShutterSpeed_Delegate));
			ret = xsdkApi(phCamera, ref plShutterSpeed, ref plBulb);
		}

		return ret;
	}

	/// <summary>
	/// Queries supported exposure compensations to set.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="plNumExposureBias">Returns the number of supported XSDK_SetExposureBias settings.</param>
	/// <param name="plExposureBias">If not NULL, plExposureBias will return a list of the XSDK_SetExposureBias settings supported. </param>
	/// <returns>API result.</returns>
	public long XSDK_CapExposureBias(IntPtr phCamera, ref int plNumExposureBias, ref int[] plExposureBias)
	{
		long ret = XSDK_ERROR;

		if (m_pfnCapExposureBias != IntPtr.Zero)
		{
			int num = plNumExposureBias;
			IntPtr parray = IntPtr.Zero;
			XSDK_CapExposureBias_Delegate xsdkApi = (XSDK_CapExposureBias_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnCapExposureBias, typeof(XSDK_CapExposureBias_Delegate));
			System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
			try { }
			finally
			{
				if (num != 0)
				{
					parray = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * plNumExposureBias);
				}
				try
				{
					ret = xsdkApi(phCamera, ref plNumExposureBias, parray);
					if (num != 0)
					{
						int max = plNumExposureBias;
						if (num < max)
						{
							max = num;
						}
						Marshal.Copy(parray, plExposureBias, 0, max);
					}
				}
				finally
				{
					if (num != 0)
					{
						Marshal.FreeCoTaskMem(parray);
					}
				}
			}
		}
		else
		{
			plNumExposureBias = 0;
		}

		return ret;
	}

	/// <summary>
	/// Sets the exposure compensation value.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="lExposureBias">The value to which exposure compensation will be set.</param>
	/// <returns>API result.</returns>
	public long XSDK_SetExposureBias(IntPtr phCamera, int lExposureBias)
	{
		long ret = XSDK_ERROR;

		if (m_pfnSetExposureBias != IntPtr.Zero)
		{
			XSDK_SetExposureBias_Delegate xsdkApi = (XSDK_SetExposureBias_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnSetExposureBias, typeof(XSDK_SetExposureBias_Delegate));
			ret = xsdkApi(phCamera, lExposureBias);
		}

		return ret;
	}

	/// <summary>
	/// Gets the exposure compensation setting.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="plExposureBias">Returns the current exposure compensation value.</param>
	/// <returns>API result.</returns>
	public long XSDK_GetExposureBias(IntPtr phCamera, ref int plExposureBias)
	{
		long ret = XSDK_ERROR;

		plExposureBias = 0;
		if (m_pfnGetExposureBias != IntPtr.Zero)
		{
			XSDK_GetExposureBias_Delegate xsdkApi = (XSDK_GetExposureBias_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnGetExposureBias, typeof(XSDK_GetExposureBias_Delegate));
			ret = xsdkApi(phCamera, ref plExposureBias);
		}

		return ret;
	}

	/// <summary>
	/// Queries supported dynamic ranges to set.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="plNumDynamicRange">Returns the number of supported XSDK_SetDynamicRange settings.</param>
	/// <param name="plDynamicRange">If not NULL, plDynamicRange will return a list of the XSDK_SetDynamicRange settings supported. </param>
	/// <returns>API result.</returns>
	public long XSDK_CapDynamicRange(IntPtr phCamera, ref int plNumDynamicRange, ref int[] plDynamicRange)
	{
		long ret = XSDK_ERROR;

		if (m_pfnCapDynamicRange != IntPtr.Zero)
		{
			int num = plNumDynamicRange;
			IntPtr parray = IntPtr.Zero;
			XSDK_CapDynamicRange_Delegate xsdkApi = (XSDK_CapDynamicRange_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnCapDynamicRange, typeof(XSDK_CapDynamicRange_Delegate));
			System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
			try { }
			finally
			{
				if (num != 0)
				{
					parray = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * plNumDynamicRange);
				}
				try
				{
					ret = xsdkApi(phCamera, ref plNumDynamicRange, parray);
					if (num != 0)
					{
						int max = plNumDynamicRange;
						if (num < max)
						{
							max = num;
						}
						Marshal.Copy(parray, plDynamicRange, 0, max);
					}
				}
				finally
				{
					if (num != 0)
					{
						Marshal.FreeCoTaskMem(parray);
					}
				}
			}
		}
		else
		{
			plNumDynamicRange = 0;
		}

		return ret;
	}

	/// <summary>
	/// Sets the dynamic range value.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="lDynamicRange">The value to which dynamic range will be set. </param>
	/// <returns>API result.</returns>
	public long XSDK_SetDynamicRange(IntPtr phCamera, int lDynamicRange)
	{
		long ret = XSDK_ERROR;

		if (m_pfnSetDynamicRange != IntPtr.Zero)
		{
			XSDK_SetDynamicRange_Delegate xsdkApi = (XSDK_SetDynamicRange_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnSetDynamicRange, typeof(XSDK_SetDynamicRange_Delegate));
			ret = xsdkApi(phCamera, lDynamicRange);
		}

		return ret;
	}

	/// <summary>
	/// Gets the dynamic range setting.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="plDynamicRange">Returns the dynamic range.</param>
	/// <returns>API result.</returns>
	public long XSDK_GetDynamicRange(IntPtr phCamera, ref int plDynamicRange)
	{
		long ret = XSDK_ERROR;

		plDynamicRange = 0;
		if (m_pfnGetDynamicRange != IntPtr.Zero)
		{
			XSDK_GetDynamicRange_Delegate xsdkApi = (XSDK_GetDynamicRange_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnGetDynamicRange, typeof(XSDK_GetDynamicRange_Delegate));
			ret = xsdkApi(phCamera, ref plDynamicRange);
		}

		return ret;
	}

	/// <summary>
	/// Queries supported ISO sensitivities to set.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="plNumSensitivity">Returns the number of supported settings for XSDK_SetSensitivity.</param>
	/// <param name="plSensitivity">If not NULL, plSensitivity will return a list of the XSDK_SetSensitivity settings supported.</param>
	/// <returns>API result.</returns>
	public long XSDK_CapSensitivity(IntPtr phCamera, ref int plNumSensitivity, ref int[] plSensitivity)
	{
		long ret = XSDK_ERROR;

		if (m_pfnCapSensitivity != IntPtr.Zero)
		{
			int num = plNumSensitivity;
			IntPtr parray = IntPtr.Zero;
			XSDK_CapSensitivity_Delegate xsdkApi = (XSDK_CapSensitivity_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnCapSensitivity, typeof(XSDK_CapSensitivity_Delegate));
			System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
			try { }
			finally
			{
				if (num != 0)
				{
					parray = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * plNumSensitivity);
				}
				try
				{
					ret = xsdkApi(phCamera, ref plNumSensitivity, parray);
					if (num != 0)
					{
						int max = plNumSensitivity;
						if (num < max)
						{
							max = num;
						}
						Marshal.Copy(parray, plSensitivity, 0, max);
					}
				}
				finally
				{
					if (num != 0)
					{
						Marshal.FreeCoTaskMem(parray);
					}
				}
			}
		}
		else
		{
			plNumSensitivity = 0;
		}

		return ret;
	}

	/// <summary>
	/// Sets the ISO sensitivity value.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="lSensitivity">The value to which sensitivity will be set.</param>
	/// <returns>API result.</returns>
	public long XSDK_SetSensitivity(IntPtr phCamera, int lSensitivity)
	{
		long ret = XSDK_ERROR;

		if (m_pfnSetSensitivity != IntPtr.Zero)
		{
			XSDK_SetSensitivity_Delegate xsdkApi = (XSDK_SetSensitivity_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnSetSensitivity, typeof(XSDK_SetSensitivity_Delegate));
			ret = xsdkApi(phCamera, lSensitivity);
		}

		return ret;
	}

	/// <summary>
	/// Gets the ISO sensitivity setting.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="plSensitivity">Returns the current value for ISO sensitivity.</param>
	/// <returns>API result.</returns>
	public long XSDK_GetSensitivity(IntPtr phCamera, ref int plSensitivity)
	{
		long ret = XSDK_ERROR;

		plSensitivity = 0;
		if (m_pfnGetSensitivity != IntPtr.Zero)
		{
			XSDK_GetSensitivity_Delegate xsdkApi = (XSDK_GetSensitivity_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnGetSensitivity, typeof(XSDK_GetSensitivity_Delegate));
			ret = xsdkApi(phCamera, ref plSensitivity);
		}

		return ret;
	}

	/// <summary>
	/// Queries supported metering modes to set.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="plNumMeteringMode">Returns the number of supported XSDK_SetMeteringMode settings.</param>
	/// <param name="plMeteringMode">If not NULL, plMeteringMode will return a list of the XSDK_SetMeteringMode settings supported.</param>
	/// <returns>API result.</returns>
	public long XSDK_CapMeteringMode(IntPtr phCamera, ref int plNumMeteringMode, ref int[] plMeteringMode)
	{
		long ret = XSDK_ERROR;

		if (m_pfnCapMeteringMode != IntPtr.Zero)
		{
			int num = plNumMeteringMode;
			IntPtr parray = IntPtr.Zero;
			XSDK_CapMeteringMode_Delegate xsdkApi = (XSDK_CapMeteringMode_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnCapMeteringMode, typeof(XSDK_CapMeteringMode_Delegate));
			System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
			try { }
			finally
			{
				if (num != 0)
				{
					parray = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * plNumMeteringMode);
				}
				try
				{
					ret = xsdkApi(phCamera, ref plNumMeteringMode, parray);
					if (num != 0)
					{
						int max = plNumMeteringMode;
						if (num < max)
						{
							max = num;
						}
						Marshal.Copy(parray, plMeteringMode, 0, max);
					}
				}
				finally
				{
					if (num != 0)
					{
						Marshal.FreeCoTaskMem(parray);
					}
				}
			}
		}
		else
		{
			plNumMeteringMode = 0;
		}

		return ret;
	}

	/// <summary>
	/// Sets the metering mode.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="lMeteringMode">The metering mode</param>
	/// <returns>API result.</returns>
	public long XSDK_SetMeteringMode(IntPtr phCamera, int lMeteringMode)
	{
		long ret = XSDK_ERROR;

		if (m_pfnSetMeteringMode != IntPtr.Zero)
		{
			XSDK_SetMeteringMode_Delegate xsdkApi = (XSDK_SetMeteringMode_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnSetMeteringMode, typeof(XSDK_SetMeteringMode_Delegate));
			ret = xsdkApi(phCamera, lMeteringMode);
		}

		return ret;
	}

	/// <summary>
	/// Gets the metering mode setting.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="plMeteringMode">The metering mode</param>
	/// <returns>API result.</returns>
	public long XSDK_GetMeteringMode(IntPtr phCamera, ref int plMeteringMode)
	{
		long ret = XSDK_ERROR;

		plMeteringMode = 0;
		if (m_pfnGetMeteringMode != IntPtr.Zero)
		{
			XSDK_GetMeteringMode_Delegate xsdkApi = (XSDK_GetMeteringMode_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnGetMeteringMode, typeof(XSDK_GetMeteringMode_Delegate));
			ret = xsdkApi(phCamera, ref plMeteringMode);
		}

		return ret;
	}

	/// <summary>
	/// Queries supported zoom positions to set.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="plNumZoomPos">Returns the number of supported zoom positions.</param>
	/// <param name="plLensZoomPos">If not NULL, plZoomPos returns a list of the zoom positions supported.</param>
	/// <param name="plFocusLength">If not NULL, plFocalLength returns a list of the focal length positions supported. The values are 100 times the actual focal lengths.</param>
	/// <param name="pl35mmFocusLength">If not NULL, pl35mmFocalLength returns a list of the 35 mm-equivalent focal length positions supported. The values are 100 times the actual focal lengths. </param>
	/// <returns>API result.</returns>
	public long XSDK_CapLensZoomPos(IntPtr phCamera, ref int plNumZoomPos, ref int[] plLensZoomPos, ref int[] plFocusLength, ref int[] pl35mmFocusLength)
	{
		long ret = XSDK_ERROR;

		if (m_pfnCapLensZoomPos != IntPtr.Zero)
		{
			int num = plNumZoomPos;
			IntPtr parray1 = IntPtr.Zero;
			IntPtr parray2 = IntPtr.Zero;
			IntPtr parray3 = IntPtr.Zero;
			XSDK_CapLensZoomPos_Delegate xsdkApi = (XSDK_CapLensZoomPos_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnCapLensZoomPos, typeof(XSDK_CapLensZoomPos_Delegate));
			System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
			try { }
			finally
			{
				if (num != 0)
				{
					parray1 = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * plNumZoomPos);
					parray2 = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * plNumZoomPos);
					parray3 = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * plNumZoomPos);
				}
				try
				{
					ret = xsdkApi(phCamera, ref plNumZoomPos, parray1, parray2, parray3);
					if (num != 0)
					{
						Marshal.Copy(parray1, plLensZoomPos, 0, plLensZoomPos.Length);
						Marshal.Copy(parray2, plFocusLength, 0, plFocusLength.Length);
						Marshal.Copy(parray3, pl35mmFocusLength, 0, pl35mmFocusLength.Length);
					}
				}
				finally
				{
					if (num != 0)
					{
						Marshal.FreeCoTaskMem(parray1);
						Marshal.FreeCoTaskMem(parray2);
						Marshal.FreeCoTaskMem(parray3);
					}
				}
			}
		}
		else
		{
			plNumZoomPos = 0;
		}

		return ret;
	}

	/// <summary>
	/// Sets the zoom position.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="lZoomPos">The zoom position, in steps.</param>
	/// <returns>API result.</returns>
	public long XSDK_SetLensZoomPos(IntPtr phCamera, int lZoomPos)
	{
		long ret = XSDK_ERROR;

		if (m_pfnSetLensZoomPos != IntPtr.Zero)
		{
			XSDK_SetLensZoomPos_Delegate xsdkApi = (XSDK_SetLensZoomPos_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnSetLensZoomPos, typeof(XSDK_SetLensZoomPos_Delegate));
			ret = xsdkApi(phCamera, lZoomPos);
		}

		return ret;
	}

	/// <summary>
	/// Gets the zoom position setting.
	/// </summary>
	/// <param name="phCamera">Camera handle</param>
	/// <param name="plLensZoomPos">The zoom position, in steps.</param>
	/// <returns>API result.</returns>
	public long XSDK_GetLensZoomPos(IntPtr phCamera, ref int plLensZoomPos)
	{
		long ret = XSDK_ERROR;

		plLensZoomPos = 0;
		if (m_pfnGetLensZoomPos != IntPtr.Zero)
		{
			XSDK_GetLensZoomPos_Delegate xsdkApi = (XSDK_GetLensZoomPos_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnGetLensZoomPos, typeof(XSDK_GetLensZoomPos_Delegate));
			ret = xsdkApi(phCamera, ref plLensZoomPos);
		}

		return ret;
	}

	/// <summary>
	/// Queries supported aperture values to set.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="lZoomPos">The zoom position retrieved using “XSDK_CapLensZoomPos”.</param>
	/// <param name="plNumAperture">Returns the number of XSDK_SetAperture settings available at zoom position lZoomPos.</param>
	/// <param name="plFNumber">If not NULL, plFNumber returns a list of the XSDK_SetAperture settings available at zoom position lZoomPos.</param>
	/// <returns>API result.</returns>
	public long XSDK_CapAperture(IntPtr phCamera, int lZoomPos, ref int plNumAperture, ref int[] plFNumber)
	{
		long ret = XSDK_ERROR;

		if (m_pfnCapAperture != IntPtr.Zero)
		{
			int num = plNumAperture;
			IntPtr parray = IntPtr.Zero;
			XSDK_CapAperture_Delegate xsdkApi = (XSDK_CapAperture_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnCapAperture, typeof(XSDK_CapAperture_Delegate));
			System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
			try { }
			finally
			{
				if (num != 0)
				{
					parray = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * plNumAperture);
				}
				try
				{
					ret = xsdkApi(phCamera, lZoomPos, ref plNumAperture, parray);
					if (num != 0)
					{
						int max = plNumAperture;
						if (num < max)
						{
							max = num;
						}
						Marshal.Copy(parray, plFNumber, 0, max);
					}
				}
				finally
				{
					if (num != 0)
					{
						Marshal.FreeCoTaskMem(parray);
					}
				}
			}
		}
		else
		{
			plNumAperture = 0;
		}

		return ret;
	}

	/// <summary>
	/// Sets the aperture value.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="lFNumber">The aperture value, expressed as the F number multiplied by 100.</param>
	/// <returns>API result.</returns>
	public long XSDK_SetAperture(IntPtr phCamera, int lFNumber)
	{
		long ret = XSDK_ERROR;

		if (m_pfnSetAperture != IntPtr.Zero)
		{
			XSDK_SetAperture_Delegate xsdkApi = (XSDK_SetAperture_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnSetAperture, typeof(XSDK_SetAperture_Delegate));
			ret = xsdkApi(phCamera, lFNumber);
		}

		return ret;
	}

	/// <summary>
	/// Gets the aperture setting.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="lFNumber">
	/// Receive the aperture in F number.
	/// The value is hundredfold value of the actual aperture F number.
	/// </param>
	/// <returns>API result.</returns>
	public long XSDK_GetAperture(IntPtr phCamera, ref int lFNumber)
	{
		long ret = XSDK_ERROR;

		lFNumber = 0;
		if (m_pfnGetAperture != IntPtr.Zero)
		{
			XSDK_GetAperture_Delegate xsdkApi = (XSDK_GetAperture_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnGetAperture, typeof(XSDK_GetAperture_Delegate));
			ret = xsdkApi(phCamera, ref lFNumber);
		}

		return ret;
	}

	/// <summary>
	/// Queries supported white-balance modes to set.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="plNumWBMode">Returns the number of supported XSDK_SetWBMode settings.</param>
	/// <param name="plWBMode">If not NULL, plWBMode returns a list of the XSDK_SetWBMode settings supported. </param>
	/// <returns>API result.</returns>
	public long XSDK_CapWBMode(IntPtr phCamera, ref int plNumWBMode, ref int[] plWBMode)
	{
		long ret = XSDK_ERROR;

		if (m_pfnCapWBMode != IntPtr.Zero)
		{
			int num = plNumWBMode;
			IntPtr parray = IntPtr.Zero;
			XSDK_CapWBMode_Delegate xsdkApi = (XSDK_CapWBMode_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnCapWBMode, typeof(XSDK_CapWBMode_Delegate));
			System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
			try { }
			finally
			{
				if (num != 0)
				{
					parray = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * plNumWBMode);
				}
				try
				{
					ret = xsdkApi(phCamera, ref plNumWBMode, parray);
					if (num != 0)
					{
						int max = plNumWBMode;
						if (num < max)
						{
							max = num;
						}
						Marshal.Copy(parray, plWBMode, 0, max);
					}
				}
				finally
				{
					if (num != 0)
					{
						Marshal.FreeCoTaskMem(parray);
					}
				}
			}
		}
		else
		{
			plNumWBMode = 0;
		}

		return ret;
	}

	/// <summary>
	/// Sets the white-balance mode.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="lWBMode">The white-balance mode.</param>
	/// <returns>API result.</returns>
	public long XSDK_SetWBMode(IntPtr phCamera, int lWBMode)
	{
		long ret = XSDK_ERROR;

		if (m_pfnSetWBMode != IntPtr.Zero)
		{
			XSDK_SetWBMode_Delegate xsdkApi = (XSDK_SetWBMode_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnSetWBMode, typeof(XSDK_SetWBMode_Delegate));
			ret = xsdkApi(phCamera, lWBMode);
		}

		return ret;
	}

	/// <summary>
	/// Gets the white-balance mode setting.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="plWBMode">The current white-balance mode.</param>
	/// <returns>API result.</returns>
	public long XSDK_GetWBMode(IntPtr phCamera, ref int plWBMode)
	{
		long ret = XSDK_ERROR;

		plWBMode = 0;
		if (m_pfnGetWBMode != IntPtr.Zero)
		{
			XSDK_GetWBMode_Delegate xsdkApi = (XSDK_GetWBMode_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnGetWBMode, typeof(XSDK_GetWBMode_Delegate));
			ret = xsdkApi(phCamera, ref plWBMode);
		}

		return ret;
	}

	/// <summary>
	/// Queries supported color temperatures to set available when WBMode=ColorTemperature.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="plNumWBColorTemp">Returns the number of supported XSDK_SetWBColorTemp settings.</param>
	/// <param name="plWBColorTemp">If not NULL, plWBColorTemp returns a list of the XSDK_SetWBColorTemp settings supported. </param>
	/// <returns>API result.</returns>
	public long XSDK_CapWBColorTemp(IntPtr phCamera, ref int plNumWBColorTemp, ref int[] plWBColorTemp)
	{
		long ret = XSDK_ERROR;

		if (m_pfnCapWBColorTemp != IntPtr.Zero)
		{
			int num = plNumWBColorTemp;
			IntPtr parray = IntPtr.Zero;
			XSDK_CapWBColorTemp_Delegate xsdkApi = (XSDK_CapWBColorTemp_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnCapWBColorTemp, typeof(XSDK_CapWBColorTemp_Delegate));
			System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
			try { }
			finally
			{
				if (num != 0)
				{
					parray = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * plNumWBColorTemp);
				}
				try
				{
					ret = xsdkApi(phCamera, ref plNumWBColorTemp, parray);
					if (num != 0)
					{
						int max = plNumWBColorTemp;
						if (num < max)
						{
							max = num;
						}
						Marshal.Copy(parray, plWBColorTemp, 0, max);
					}
				}
				finally
				{
					if (num != 0)
					{
						Marshal.FreeCoTaskMem(parray);
					}
				}
			}
		}
		else
		{
			plNumWBColorTemp = 0;
		}

		return ret;
	}

	/// <summary>
	/// Sets the color temperature value for WBMode=ColorTemperature.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="lWBColorTemp">The color temperature, in degrees Kelvin.</param>
	/// <returns>API result.</returns>
	public long XSDK_SetWBColorTemp(IntPtr phCamera, int lWBColorTemp)
	{
		long ret = XSDK_ERROR;

		if (m_pfnSetWBColorTemp != IntPtr.Zero)
		{
			XSDK_SetWBColorTemp_Delegate xsdkApi = (XSDK_SetWBColorTemp_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnSetWBColorTemp, typeof(XSDK_SetWBColorTemp_Delegate));
			ret = xsdkApi(phCamera, lWBColorTemp);
		}

		return ret;
	}

	/// <summary>
	/// Gets the color temperature setting for WBMode=ColorTemperature.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="plWBColorTemp">Returns the current color temperature, in degrees Kelvin.</param>
	/// <returns>API result.</returns>
	public long XSDK_GetWBColorTemp(IntPtr phCamera, ref int plWBColorTemp)
	{
		long ret = XSDK_ERROR;

		plWBColorTemp = 0;
		if (m_pfnGetWBColorTemp != IntPtr.Zero)
		{
			XSDK_GetWBColorTemp_Delegate xsdkApi = (XSDK_GetWBColorTemp_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnGetWBColorTemp, typeof(XSDK_GetWBColorTemp_Delegate));
			ret = xsdkApi(phCamera, ref plWBColorTemp);
		}

		return ret;
	}

	/// <summary>
	/// Queries supported media recording control modes to set.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="plNumMediaRecord">Returns the number of supported XSDK_SetMediaRecord settings.</param>
	/// <param name="plMediaRecord">If not NULL, plMediaRecord returns a list of the XSDK_SetMediaRecord settings supported.</param>
	/// <returns>API result.</returns>
	public long XSDK_CapMediaRecord(IntPtr phCamera, ref int plNumMediaRecord, ref int[] plMediaRecord)
	{
		long ret = XSDK_ERROR;

		if (m_pfnCapMediaRecord != IntPtr.Zero)
		{
			int num = plNumMediaRecord;
			IntPtr parray = IntPtr.Zero;
			XSDK_CapMediaRecord_Delegate xsdkApi = (XSDK_CapMediaRecord_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnCapMediaRecord, typeof(XSDK_CapMediaRecord_Delegate));
			System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
			try { }
			finally
			{
				if (num != 0)
				{
					parray = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * plNumMediaRecord);
				}
				try
				{
					ret = xsdkApi(phCamera, ref plNumMediaRecord, parray);
					if (num != 0)
					{
						int max = plNumMediaRecord;
						if (num < max)
						{
							max = num;
						}
						Marshal.Copy(parray, plMediaRecord, 0, max);
					}
				}
				finally
				{
					if (num != 0)
					{
						Marshal.FreeCoTaskMem(parray);
					}
				}
			}
		}
		else
		{
			plNumMediaRecord = 0;
		}

		return ret;
	}

	/// <summary>
	/// Sets the media recording control modes for the tethering operation.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="lMediaRecord">The media recording control mode.</param>
	/// <returns>API result.</returns>
	public long XSDK_SetMediaRecord(IntPtr phCamera, int lMediaRecord)
	{
		long ret = XSDK_ERROR;

		if (m_pfnSetMediaRecord != IntPtr.Zero)
		{
			XSDK_SetMediaRecord_Delegate xsdkApi = (XSDK_SetMediaRecord_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnSetMediaRecord, typeof(XSDK_SetMediaRecord_Delegate));
			ret = xsdkApi(phCamera, lMediaRecord);
		}

		return ret;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="plMediaRecord">The current media recording control mode.</param>
	/// <returns>API result.</returns>
	public long XSDK_GetMediaRecord(IntPtr phCamera, ref int plMediaRecord)
	{
		long ret = XSDK_ERROR;

		plMediaRecord = 0;
		if (m_pfnGetMediaRecord != IntPtr.Zero)
		{
			XSDK_GetMediaRecord_Delegate xsdkApi = (XSDK_GetMediaRecord_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnGetMediaRecord, typeof(XSDK_GetMediaRecord_Delegate));
			ret = xsdkApi(phCamera, ref plMediaRecord);
		}

		return ret;
	}

	/// <summary>
	/// Queries supported operation modes to set.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="plNumForceMode">Returns the number of supported XSDK_SetForceMode settings.</param>
	/// <param name="plForceMode">If not NULL, plForceMode returns a list of the XSDK_SetForceMode settings supported.</param>
	/// <returns>API result.</returns>
	public long XSDK_CapForceMode(IntPtr phCamera, ref int plNumForceMode, ref int[] plForceMode)
	{
		long ret = XSDK_ERROR;

		if (m_pfnCapForceMode != IntPtr.Zero)
		{
			int num = plNumForceMode;
			IntPtr parray = IntPtr.Zero;
			XSDK_CapForceMode_Delegate xsdkApi = (XSDK_CapForceMode_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnCapForceMode, typeof(XSDK_CapForceMode_Delegate));
			System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
			try { }
			finally
			{
				if (num != 0)
				{
					parray = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * plNumForceMode);
				}
				try
				{
					ret = xsdkApi(phCamera, ref plNumForceMode, parray);
					if (num != 0)
					{
						int max = plNumForceMode;
						if (num < max)
						{
							max = num;
						}
						Marshal.Copy(parray, plForceMode, 0, max);
					}
				}
				finally
				{
					if (num != 0)
					{
						Marshal.FreeCoTaskMem(parray);
					}
				}
			}
		}
		else
		{
			plNumForceMode = 0;
		}

		return ret;
	}

		/// <summary>
	/// Forcibly changes the operating mode to SHOOTING MODE.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="lForceMode">The operating mode to SHOOTING MODE.<</param>
	/// <returns>API result.</returns>
	public long XSDK_SetForceMode(IntPtr phCamera, int lForceMode)
	{
		long ret = XSDK_ERROR;

		if (m_pfnSetForceMode != IntPtr.Zero)
		{
			XSDK_SetForceMode_Delegate xsdkApi = (XSDK_SetForceMode_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnSetForceMode, typeof(XSDK_SetForceMode_Delegate));
			ret = xsdkApi(phCamera, lForceMode);
		}

		return ret;
	}

	/// <summary>
	/// Restore camera backup settings.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="lSize">The size of the backup data.</param>
	/// <param name="pBackup">The backup data.</param>
	/// <returns>API result.</returns>
	public long XSDK_SetBackupSettings(IntPtr phCamera, int lSize, byte[] pBackup)
	{
		long ret = XSDK_ERROR;
		if (m_pfnGetBackupSettings != IntPtr.Zero)
		{

			XSDK_SetBackupSettings_Delegate xsdkApi = (XSDK_SetBackupSettings_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnSetBackupSettings, typeof(XSDK_SetBackupSettings_Delegate));
			System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
			try { }
			finally
			{
				IntPtr parray = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(byte)) * lSize);
				try
				{
					Marshal.Copy(pBackup, 0, parray, pBackup.Length);
					ret = xsdkApi(phCamera, lSize, parray);
					if (ret == (int)FUJI_SDK_ERR.FUJI_SDK_ERR_OK)
					{
						Marshal.Copy(parray, pBackup, 0, pBackup.Length);
					}
				}
				finally
				{
					Marshal.FreeCoTaskMem(parray);
				}
			 }
		}

		return ret;
	}

	/// <summary>
	/// Backup camera settings.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="lSize">The size of the backup data</param>
	/// <param name="pBackup">Camera setting data.</param>
	/// <returns>API result.</returns>
	public long XSDK_GetBackupSettings(IntPtr phCamera, ref int lSize, byte[] pBackup)
	{
		long ret = XSDK_ERROR;

		if (m_pfnGetBackupSettings != IntPtr.Zero)
		{
			int num = lSize;
			IntPtr parray = IntPtr.Zero;
			XSDK_GetBackupSettings_Delegate xsdkApi = (XSDK_GetBackupSettings_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnGetBackupSettings, typeof(XSDK_GetBackupSettings_Delegate));
			System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
			try { }
			finally
			{
				if (num != 0)
				{
					parray = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * lSize);
				}
				try
				{
					ret = xsdkApi(phCamera, ref lSize, parray);
					if (num != 0)
					{
						int max = lSize;
						if (num < max)
						{
							max = num;
						}
						Marshal.Copy(parray, pBackup, 0, max);
					}
				}
				finally
				{
					if (num != 0)
					{
						Marshal.FreeCoTaskMem(parray);
					}
				}
			}

		}

		return ret;
	}

	/// <summary>
	/// Queries supported drive modes.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="plNumDriveMode">Returns the number of XSDK_SetDriveMode settings supported.</param>
	/// <param name="plDriveMode">if plDriveMode is NULL, the function will return only plNumDriveMode with the number of supported XSDK_SetDriveMode settings.</param>
	/// <returns>API result.</returns>
	public long XSDK_CapDriveMode(IntPtr phCamera, ref int plNumDriveMode, ref int[] plDriveMode)
	{
		long ret = XSDK_ERROR;

		if (m_pfnCapDriveMode != IntPtr.Zero)
		{
			int num = plNumDriveMode;
			IntPtr parray = IntPtr.Zero;
			XSDK_CapDriveMode_Delegate xsdkApi = (XSDK_CapDriveMode_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnCapDriveMode, typeof(XSDK_CapDriveMode_Delegate));
			System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
			try { }
			finally
			{
				if (num != 0)
				{
					parray = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * plNumDriveMode);
				}
				try
				{
					ret = xsdkApi(phCamera, ref plNumDriveMode, parray);
					if (num != 0)
					{
						int max = plNumDriveMode;
						if (num < max)
						{
							max = num;
						}
						Marshal.Copy(parray, plDriveMode, 0, max);
					}
				}
				finally
				{
					if (num != 0)
					{
						Marshal.FreeCoTaskMem(parray);
					}
				}
			}
		}
		else
		{
			plNumDriveMode = 0;
		}

		return ret;
	}

	/// <summary>
	/// Sets the camera drive mode.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="lDriveMode">The drive mode.</param>
	/// <returns>API result.</returns>
	public long XSDK_SetDriveMode(IntPtr phCamera, int lDriveMode)
	{
		long ret = XSDK_ERROR;

		if (m_pfnSetDriveMode != IntPtr.Zero)
		{
			XSDK_SetDriveMode_Delegate xsdkApi = (XSDK_SetDriveMode_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnSetDriveMode, typeof(XSDK_SetDriveMode_Delegate));
			ret = xsdkApi(phCamera, lDriveMode);
		}

		return ret;
	}

	/// <summary>
	/// Gets the current camera drive mode.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="plDriveMode">Drive Mode</param>
	/// <returns>API result.</returns>
	public long XSDK_GetDriveMode(IntPtr phCamera, ref int plDriveMode)
	{
		long ret = XSDK_ERROR;

		plDriveMode = 0;
		if (m_pfnGetDriveMode != IntPtr.Zero)
		{
			XSDK_GetDriveMode_Delegate xsdkApi = (XSDK_GetDriveMode_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnGetDriveMode, typeof(XSDK_GetDriveMode_Delegate));
			ret = xsdkApi(phCamera, ref plDriveMode);
		}

		return ret;
	}

	/// <summary>
	/// Queries supported camera MODES.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="plNumMode">Returns the number of XSDK_SetMode settings supported.</param>
	/// <param name="plMode">If plMode is NULL, the function will return only plNumMode with the number of supported XSDK_SetMode settings.</param>
	/// <returns>API result.</returns>
	public long XSDK_CapMode(IntPtr phCamera, ref int plNumMode, ref int[] plMode)
	{
		long ret = XSDK_ERROR;

		if (m_pfnCapMode != IntPtr.Zero)
		{
			int num = plNumMode;
			IntPtr parray = IntPtr.Zero;
			XSDK_CapMode_Delegate xsdkApi = (XSDK_CapMode_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnCapMode, typeof(XSDK_CapMode_Delegate));
			System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
			try { }
			finally
			{
				if (num != 0)
				{
					parray = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * plNumMode);
				}
				try
				{
					ret = xsdkApi(phCamera, ref plNumMode, parray);
					if (num != 0)
					{
						int max = plNumMode;
						if (num < max)
						{
							max = num;
						}
						Marshal.Copy(parray, plMode, 0, max);
					}
				}
				finally
				{
					if (num != 0)
					{
						Marshal.FreeCoTaskMem(parray);
					}
				}
			}
		}
		else
		{
			plNumMode = 0;
		}

		return ret;
	}

	/// <summary>
	/// Sets the camera MODE
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="lDriveMode">The mode.</param>
	/// <returns>API result.</returns>
	public long XSDK_SetMode(IntPtr phCamera, int lDriveMode)
	{
		long ret = XSDK_ERROR;

		if (m_pfnSetDriveMode != IntPtr.Zero)
		{
			XSDK_SetDriveMode_Delegate xsdkApi = (XSDK_SetDriveMode_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnSetDriveMode, typeof(XSDK_SetDriveMode_Delegate));
			ret = xsdkApi(phCamera, lDriveMode);
		}

		return ret;
	}

	/// <summary>
	/// Gets the current camera MODE.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="plMode">The mode.</param>
	/// <returns>API result.</returns>
	public long XSDK_GetMode(IntPtr phCamera, ref int plMode)
	{
		long ret = XSDK_ERROR;

		plMode = 0;
		if (m_pfnGetMode != IntPtr.Zero)
		{
			XSDK_GetMode_Delegate xsdkApi = (XSDK_GetMode_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnGetMode, typeof(XSDK_GetMode_Delegate));
			ret = xsdkApi(phCamera, ref plMode);
		}

		return ret;
	}

	/// <summary>
	/// Gets the current video recording status.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="plStatus">The video recording status.</param>
	/// <returns>API result.</returns>
	public long XSDK_GetRecordingStatus(IntPtr phCamera, ref int plStatus)
	{
		long ret = XSDK_ERROR;

		plStatus = 0;
		if (m_pfnGetRecordingStatus != IntPtr.Zero)
		{
			XSDK_GetRecordingStatus_Delegate xsdkApi = (XSDK_GetRecordingStatus_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnGetRecordingStatus, typeof(XSDK_GetRecordingStatus_Delegate));
			ret = xsdkApi(phCamera, ref plStatus);
		}

		return ret;
	}

	/// <summary>
	/// Queries supported shutter speeds to set in movie mode.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="plNumShutterSpeed">Returns the number of supported XSDK_SetMovieShutterSpeed settings.</param>
	/// <param name="plShutterSpeed">If not NULL, pllShutterSpeed will return a list of the XSDK_SetMovieShutterSpeed settings supported.</param>
	/// <returns>API result.</returns>
	public long XSDK_CapMovieShutterSpeed(IntPtr phCamera, ref int plNumShutterSpeed, ref long[] plShutterSpeed)
	{
		long ret = XSDK_ERROR;

		if (m_pfnCapMovieShutterSpeed != IntPtr.Zero)
		{
			int num = plNumShutterSpeed;
			IntPtr parray = IntPtr.Zero;
			XSDK_CapMovieShutterSpeed_Delegate xsdkApi = (XSDK_CapMovieShutterSpeed_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnCapMovieShutterSpeed, typeof(XSDK_CapMovieShutterSpeed_Delegate));
			System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
			try { }
			finally
			{
				if (num != 0)
				{
					parray = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * plNumShutterSpeed);
				}
				try
				{
					ret = xsdkApi(phCamera, ref plNumShutterSpeed, parray);
					if (num != 0)
					{
						int max = plNumShutterSpeed;
						if (num < max)
						{
							max = num;
						}
						Marshal.Copy(parray, plShutterSpeed, 0, max);
					}
				}
				finally
				{
					if (num != 0)
					{
						Marshal.FreeCoTaskMem(parray);
					}
				}
			}
		}
		else
		{
			plNumShutterSpeed = 0;
		}

		return ret;
	}

	/// <summary>
	/// Sets the shutter speed value in movie mode.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="lShutterSpeed">The value to which shutter speed will be set.</param>
	/// <returns>API result.</returns>
	public long XSDK_SetMovieShutterSpeed(IntPtr phCamera, long lShutterSpeed)
	{
		long ret = XSDK_ERROR;

		if (m_pfnSetMovieShutterSpeed != IntPtr.Zero)
		{
			XSDK_SetMovieShutterSpeede_Delegate xsdkApi = (XSDK_SetMovieShutterSpeede_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnSetMovieShutterSpeed, typeof(XSDK_SetMovieShutterSpeede_Delegate));
			ret = xsdkApi(phCamera, lShutterSpeed);
		}

		return ret;
	}

	/// <summary>
	/// Gets the shutter speed setting in movie mode.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="plShutterSpeed">Returns the current value for shutter speed.</param>
	/// <returns>API result.</returns>
	public long XSDK_GetMovieShutterSpeed(IntPtr phCamera, ref long plShutterSpeed)
	{
		long ret = XSDK_ERROR;

		plShutterSpeed = 0;
		if (m_pfnGetMovieShutterSpeed != IntPtr.Zero)
		{
				XSDK_GetMovieShutterSpeed_Delegate xsdkApi = (XSDK_GetMovieShutterSpeed_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnGetMovieShutterSpeed, typeof(XSDK_GetMovieShutterSpeed_Delegate));
			ret = xsdkApi(phCamera, ref plShutterSpeed);
		}

		return ret;
	}

	/// <summary>
	/// Returns the current value for shutter speed.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="plNumExposureBias">Returns the number of supported XSDK_SetMovieExposureBias settings.</param>
	/// <param name="plExposureBias">If not NULL, plExposureBias will return a list of the XSDK_SetMovieExposureBias settings supported.</param>
	/// <returns>API result.</returns>
	public long XSDK_CapMovieExposureBias(IntPtr phCamera, ref int plNumExposureBias, ref int[] plExposureBias)
	{
		long ret = XSDK_ERROR;

		if (m_pfnCapMovieExposureBias != IntPtr.Zero)
		{
			int num = plNumExposureBias;
			IntPtr parray = IntPtr.Zero;
			XSDK_CapMovieExposureBias_Delegate xsdkApi = (XSDK_CapMovieExposureBias_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnCapMovieExposureBias, typeof(XSDK_CapMovieExposureBias_Delegate));
			System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
			try { }
			finally
			{
				if (num != 0)
				{
					parray = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * plNumExposureBias);
				}
				try
				{
					ret = xsdkApi(phCamera, ref plNumExposureBias, parray);
					if (num != 0)
					{
						int max = plNumExposureBias;
						if (num < max)
						{
							max = num;
						}
						Marshal.Copy(parray, plExposureBias, 0, max);
					}
				}
				finally
				{
					if (num != 0)
					{
						Marshal.FreeCoTaskMem(parray);
					}
				}
			}
		}
		else
		{
			plNumExposureBias = 0;
		}

		return ret;
	}

	/// <summary>
	/// Sets the exposure compensation value in movie mode.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="lExposureBias">The value to which exposure compensation will be set.</param>
	/// <returns>API result.</returns>
	public long XSDK_SetMovieExposureBias(IntPtr phCamera, int lExposureBias)
	{
		long ret = XSDK_ERROR;

		if (m_pfnSetMovieExposureBias != IntPtr.Zero)
		{
			XSDK_SetMovieExposureBias_Delegate xsdkApi = (XSDK_SetMovieExposureBias_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnSetMovieExposureBias, typeof(XSDK_SetMovieExposureBias_Delegate));
			ret = xsdkApi(phCamera, lExposureBias);
		}

		return ret;
	}

	/// <summary>
	/// Gets the exposure compensation setting in movie mode.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="plExposureBias">Returns the current exposure compensation value. </param>
	/// <returns>API result.</returns>
	public long XSDK_GetMovieExposureBias(IntPtr phCamera, ref int plExposureBias)
	{
		long ret = XSDK_ERROR;

		plExposureBias = 0;
		if (m_pfnGetMovieExposureBias != IntPtr.Zero)
		{
			XSDK_GetMovieExposureBias_Delegate xsdkApi = (XSDK_GetMovieExposureBias_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnGetMovieExposureBias, typeof(XSDK_GetMovieExposureBias_Delegate));
			ret = xsdkApi(phCamera, ref plExposureBias);
		}

		return ret;
	}

	/// <summary>
	/// Queries supported ISO sensitivities to set in movie mode.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="plNumSensitivity">Returns the number of supported XSDK_SetMovieSensitivity settings.Returns the number of supported XSDK_SetMovieSensitivity settings.</param>
	/// <param name="plSensitivity">If not NULL, plSensitivity will return a list of the XSDK_SetMovieSensitivity settings supported.</param>
	/// <returns>API result.</returns>
	public long XSDK_CapMovieSensitivity(IntPtr phCamera, ref int plNumSensitivity, ref int[] plSensitivity)
	{
		long ret = XSDK_ERROR;

		if (m_pfnCapMovieSensitivity != IntPtr.Zero)
		{
			int num = plNumSensitivity;
			IntPtr parray = IntPtr.Zero;
				XSDK_CapMovieSensitivity_Delegate xsdkApi = (XSDK_CapMovieSensitivity_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnCapMovieSensitivity, typeof(XSDK_CapMovieSensitivity_Delegate));
			System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
			try { }
			finally
			{
				if (num != 0)
				{
					parray = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * plNumSensitivity);
				}
				try
				{
					ret = xsdkApi(phCamera, ref plNumSensitivity, parray);
					if (num != 0)
					{
						int max = plNumSensitivity;
						if (num < max)
						{
							max = num;
						}
						Marshal.Copy(parray, plSensitivity, 0, max);
					}
				}
				finally
				{
					if (num != 0)
					{
						Marshal.FreeCoTaskMem(parray);
					}
				}
			}
		}
		else
		{
			plNumSensitivity = 0;
		}

		return ret;
	}

	/// <summary>
	/// Sets the ISO sensitivity value in movie mode.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="lSensitivity">The value to which ISO sensitivity will be set.</param>
	/// <returns>API result.</returns>
	public long XSDK_SetMovieSensitivity(IntPtr phCamera, int lSensitivity)
	{
		long ret = XSDK_ERROR;

		if (m_pfnSetMovieSensitivity != IntPtr.Zero)
		{
			XSDK_SetMovieSensitivity_Delegate xsdkApi = (XSDK_SetMovieSensitivity_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnSetMovieSensitivity, typeof(XSDK_SetMovieSensitivity_Delegate));
			ret = xsdkApi(phCamera, lSensitivity);
		}

		return ret;
	}

	/// <summary>
	/// Gets the ISO sensitivity setting in movie mode.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="plSensitivity">Returns the current value for ISO sensitivity.</param>
	/// <returns>API result.</returns>
	public long XSDK_GetMovieSensitivity(IntPtr phCamera, ref int plSensitivity)
	{
		long ret = XSDK_ERROR;

		plSensitivity = 0;
		if (m_pfnGetMovieSensitivity != IntPtr.Zero)
		{
			XSDK_GetMovieSensitivity_Delegate xsdkApi = (XSDK_GetMovieSensitivity_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnGetMovieSensitivity, typeof(XSDK_GetMovieSensitivity_Delegate));
			ret = xsdkApi(phCamera, ref plSensitivity);
		}

		return ret;
	}

	/// <summary>
	/// Queries supported aperture values to set in movie mode.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="plNumAperture">Returns the number of supported XSDK_SetMovieAperture settings.</param>
	/// <param name="plFNumber">If not NULL, plAperture will return a list of the XSDK_SetMovieAperture settings supported.</param>
	/// <returns>API result.</returns>
	public long XSDK_CapMovieAperture(IntPtr phCamera, ref int plNumAperture, ref int[] plFNumber)
	{
		long ret = XSDK_ERROR;

		if (m_pfnCapMovieAperture != IntPtr.Zero)
		{
			int num = plNumAperture;
			IntPtr parray = IntPtr.Zero;
			XSDK_CapMovieAperture_Delegate xsdkApi = (XSDK_CapMovieAperture_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnCapMovieAperture, typeof(XSDK_CapMovieAperture_Delegate));
			System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
			try { }
			finally
			{
				if (num != 0)
				{
					parray = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * plNumAperture);
				}
				try
				{
					ret = xsdkApi(phCamera, ref plNumAperture, parray);
					if (num != 0)
					{
						int max = plNumAperture;
						if (num < max)
						{
							max = num;
						}
						Marshal.Copy(parray, plFNumber, 0, max);
					}
				}
				finally
				{
					if (num != 0)
					{
						Marshal.FreeCoTaskMem(parray);
					}
				}
			}
		}
		else
		{
			plNumAperture = 0;
		}

		return ret;
	}

	/// <summary>
	/// Sets the aperture value in movie mode.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="lFNumber">
	/// The value to which aperture will be set.
	/// The aperture values returned are 100 times the actual F numbers.
	/// </param>
	/// <returns>API result.</returns>
	public long XSDK_SetMovieAperture(IntPtr phCamera, int lFNumber)
	{
		long ret = XSDK_ERROR;

		if (m_pfnSetMovieAperture != IntPtr.Zero)
		{
			XSDK_SetMovieAperture_Delegate xsdkApi = (XSDK_SetMovieAperture_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnSetMovieAperture, typeof(XSDK_SetMovieAperture_Delegate));
			ret = xsdkApi(phCamera, lFNumber);
		}

		return ret;
	}

	/// <summary>
	/// Gets the aperture setting in movie mode.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="lFNumber">Returns the current value for aperture.</param>
	/// <returns>API result.</returns>
	public long XSDK_GetMovieAperture(IntPtr phCamera, ref int lFNumber)
	{
		long ret = XSDK_ERROR;

		lFNumber = 0;
		if (m_pfnGetMovieAperture != IntPtr.Zero)
		{
			XSDK_GetMovieAperture_Delegate xsdkApi = (XSDK_GetMovieAperture_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnGetMovieAperture, typeof(XSDK_GetMovieAperture_Delegate));
			ret = xsdkApi(phCamera, ref lFNumber);
		}

		return ret;
	}

	/// <summary>
	/// Queries supported dynamic ranges to set in movie mode.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="plNumDynamicRange">Returns the number of supported XSDK_ SetMovieDynamicRange settings.</param>
	/// <param name="plDynamicRange">If not NULL, plDynamicRange returns a list of the XSDK_SetWBMode settings supported. </param>
	/// <returns>API result.</returns>
	public long XSDK_CapMovieDynamicRange(IntPtr phCamera, ref int plNumDynamicRange, ref int[] plDynamicRange)
	{
		long ret = XSDK_ERROR;

		if (m_pfnCapMovieDynamicRange != IntPtr.Zero)
		{
			int num = plNumDynamicRange;
			IntPtr parray = IntPtr.Zero;
			XSDK_CapMovieDynamicRange_Delegate xsdkApi = (XSDK_CapMovieDynamicRange_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnCapMovieDynamicRange, typeof(XSDK_CapMovieDynamicRange_Delegate));
			System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
			try { }
			finally
			{
				if (num != 0)
				{
					parray = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * plNumDynamicRange);
				}
				try
				{
					ret = xsdkApi(phCamera, ref plNumDynamicRange, parray);
					if (num != 0)
					{
						int max = plNumDynamicRange;
						if (num < max)
						{
							max = num;
						}
						Marshal.Copy(parray, plDynamicRange, 0, max);
					}
				}
				finally
				{
					if (num != 0)
					{
						Marshal.FreeCoTaskMem(parray);
					}
				}
			}
		}
		else
		{
			plNumDynamicRange = 0;
		}

		return ret;
	}

	/// <summary>
	/// Sets the dynamic range value in movie mode.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="lDynamicRange">The value to which dynamic range will be set. </param>
	/// <returns>API result.</returns>
	public long XSDK_SetMovieDynamicRange(IntPtr phCamera, int lDynamicRange)
	{
		long ret = XSDK_ERROR;

		if (m_pfnSetMovieDynamicRange != IntPtr.Zero)
		{
			XSDK_SetMovieDynamicRange_Delegate xsdkApi = (XSDK_SetMovieDynamicRange_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnSetMovieDynamicRange, typeof(XSDK_SetMovieDynamicRange_Delegate));
			ret = xsdkApi(phCamera, lDynamicRange);
		}

		return ret;
	}

	/// <summary>
	/// Gets the dynamic range setting in movie mode.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="plDynamicRange">Returns the current value for dynamic range.</param>
	/// <returns>API result.</returns>
	public long XSDK_GetMovieDynamicRange(IntPtr phCamera, ref int plDynamicRange)
	{
		long ret = XSDK_ERROR;

		plDynamicRange = 0;
		if (m_pfnGetMovieDynamicRange != IntPtr.Zero)
		{
			XSDK_GetMovieDynamicRange_Delegate xsdkApi = (XSDK_GetMovieDynamicRange_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnGetMovieDynamicRange, typeof(XSDK_GetMovieDynamicRange_Delegate));
			ret = xsdkApi(phCamera, ref plDynamicRange);
		}

		return ret;
	}

	/// <summary>
	/// Queries supported metering modes to set in movie mode.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="plNumMeteringMode">Returns the number of supported XSDK_SetMovieMeteringMode settings.</param>
	/// <param name="plMeteringMode">If not NULL, plMeteringMode returns a list of the XSDK_SetWBMode settings supported. </param>
	/// <returns>API result.</returns>
	public long XSDK_CapMovieMeteringMode(IntPtr phCamera, ref int plNumMeteringMode, ref int[] plMeteringMode)
	{
		long ret = XSDK_ERROR;

		if (m_pfnCapMovieMeteringMode != IntPtr.Zero)
		{
			int num = plNumMeteringMode;
			IntPtr parray = IntPtr.Zero;
			XSDK_CapMovieMeteringMode_Delegate xsdkApi = (XSDK_CapMovieMeteringMode_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnCapMovieMeteringMode, typeof(XSDK_CapMovieMeteringMode_Delegate));
			System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
			try { }
			finally
			{
				if (num != 0)
				{
					parray = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * plNumMeteringMode);
				}
				try
				{
					ret = xsdkApi(phCamera, ref plNumMeteringMode, parray);
					if (num != 0)
					{
						int max = plNumMeteringMode;
						if (num < max)
						{
							max = num;
						}
						Marshal.Copy(parray, plMeteringMode, 0, max);
					}
				}
				finally
				{
					if (num != 0)
					{
						Marshal.FreeCoTaskMem(parray);
					}
				}
			}
		}
		else
		{
			plNumMeteringMode = 0;
		}

		return ret;
	}

	/// <summary>
	/// Sets the metering mode in movie mode.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="lMeteringMode">The value to which metering mode will be set.</param>
	/// <returns>API result.</returns>
	public long XSDK_SetMovieMeteringMode(IntPtr phCamera, int lMeteringMode)
	{
		long ret = XSDK_ERROR;

		if (m_pfnSetMovieMeteringMode != IntPtr.Zero)
		{
			XSDK_SetMovieMeteringMode_Delegate xsdkApi = (XSDK_SetMovieMeteringMode_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnSetMovieMeteringMode, typeof(XSDK_SetMovieMeteringMode_Delegate));
			ret = xsdkApi(phCamera, lMeteringMode);
		}

		return ret;
	}

	/// <summary>
	/// Gets the metering mode setting in movie mode.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="plMeteringMode">Returns the current value for metering mode.</param>
	/// <returns>API result.</returns>
	public long XSDK_GetMovieMeteringMode(IntPtr phCamera, ref int plMeteringMode)
	{
		long ret = XSDK_ERROR;

		plMeteringMode = 0;
		if (m_pfnGetMovieMeteringMode != IntPtr.Zero)
		{
			XSDK_GetMovieMeteringMode_Delegate xsdkApi = (XSDK_GetMovieMeteringMode_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnGetMovieMeteringMode, typeof(XSDK_GetMovieMeteringMode_Delegate));
			ret = xsdkApi(phCamera, ref plMeteringMode);
		}

		return ret;
	}

	/// <summary>
	/// Queries supported white-balance modes to set in movie mode.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="plNumWBMode">Returns the number of supported XSDK_SetMovieWBMode settings.</param>
	/// <param name="plWBMode">If not NULL, plWBMode returns a list of the XSDK_SetMovieWBMode settings supported. </param>
	/// <returns>API result.</returns>
	public long XSDK_CapMovieWBMode(IntPtr phCamera, ref int plNumWBMode, ref int[] plWBMode)
	{
		long ret = XSDK_ERROR;

		if (m_pfnCapMovieWBMode != IntPtr.Zero)
		{
			int num = plNumWBMode;
			IntPtr parray = IntPtr.Zero;
			XSDK_CapMovieWBMode_Delegate xsdkApi = (XSDK_CapMovieWBMode_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnCapMovieWBMode, typeof(XSDK_CapMovieWBMode_Delegate));
			System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
			try { }
			finally
			{
				if (num != 0)
				{
					parray = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * plNumWBMode);
				}
				try
				{
					ret = xsdkApi(phCamera, ref plNumWBMode, parray);
					if (num != 0)
					{
						int max = plNumWBMode;
						if (num < max)
						{
							max = num;
						}
						Marshal.Copy(parray, plWBMode, 0, max);
					}
				}
				finally
				{
					if (num != 0)
					{
						Marshal.FreeCoTaskMem(parray);
					}
				}
			}
		}
		else
		{
			plNumWBMode = 0;
		}

		return ret;
	}

	/// <summary>
	/// Sets the white-balance mode in movie mode.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="lWBMode">The white-balance mode.</param>
	/// <returns>API result.</returns>
	public long XSDK_SetMovieWBMode(IntPtr phCamera, int lWBMode)
	{
		long ret = XSDK_ERROR;

		if (m_pfnSetMovieWBMode != IntPtr.Zero)
		{
			XSDK_SetMovieWBMode_Delegate xsdkApi = (XSDK_SetMovieWBMode_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnSetMovieWBMode, typeof(XSDK_SetMovieWBMode_Delegate));
			ret = xsdkApi(phCamera, lWBMode);
		}

		return ret;
	}

	/// <summary>
	/// Gets the white-balance mode setting in movie mode.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="plWBMode">The current white-balance mode. </param>
	/// <returns>API result.</returns>
	public long XSDK_GetMovieWBMode(IntPtr phCamera, ref int plWBMode)
	{
		long ret = XSDK_ERROR;

		plWBMode = 0;
		if (m_pfnGetWBMode != IntPtr.Zero)
		{
			XSDK_GetMovieWBMode_Delegate xsdkApi = (XSDK_GetMovieWBMode_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnGetMovieWBMode, typeof(XSDK_GetMovieWBMode_Delegate));
			ret = xsdkApi(phCamera, ref plWBMode);
		}

		return ret;
	}

	/// <summary>
	/// Queries supported color temperatures to set available in movie mode when WBMode = ColorTemperature.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="plNumWBColorTemp">Returns the number of supported XSDK_SetMovieWBColorTemp settings.</param>
	/// <param name="plWBColorTemp">If not NULL, plWBColorTemp returns a list of the XSDK_SetMovieWBColorTemp settings supported.</param>
	/// <returns>API result.</returns>
	public long XSDK_CapMovieWBColorTemp(IntPtr phCamera, ref int plNumWBColorTemp, ref int[] plWBColorTemp)
	{
		long ret = XSDK_ERROR;

		if (m_pfnCapWBColorTemp != IntPtr.Zero)
		{
			int num = plNumWBColorTemp;
			IntPtr parray = IntPtr.Zero;
			XSDK_CapMovieWBColorTemp_Delegate xsdkApi = (XSDK_CapMovieWBColorTemp_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnCapMovieWBColorTemp, typeof(XSDK_CapMovieWBColorTemp_Delegate));
			System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
			try { }
			finally
			{
				if (num != 0)
				{
					parray = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * plNumWBColorTemp);
				}
				try
				{
					ret = xsdkApi(phCamera, ref plNumWBColorTemp, parray);
					if (num != 0)
					{
						int max = plNumWBColorTemp;
						if (num < max)
						{
							max = num;
						}
						Marshal.Copy(parray, plWBColorTemp, 0, max);
					}
				}
				finally
				{
					if (num != 0)
					{
						Marshal.FreeCoTaskMem(parray);
					}
				}
			}
		}
		else
		{
			plNumWBColorTemp = 0;
		}

		return ret;
	}

	/// <summary>
	/// Sets the color temperature value in movie mode for WBMode = ColorTemperature.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="lWBColorTemp">The COLOR TEMPERATURE, in degrees Kelvin</param>
	/// <returns>API result.</returns>
	public long XSDK_SetMovieWBColorTemp(IntPtr phCamera, int lWBColorTemp)
	{
		long ret = XSDK_ERROR;

		if (m_pfnSetMovieWBColorTemp != IntPtr.Zero)
		{
			XSDK_SetMovieWBColorTemp_Delegate xsdkApi = (XSDK_SetMovieWBColorTemp_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnSetMovieWBColorTemp, typeof(XSDK_SetMovieWBColorTemp_Delegate));
			ret = xsdkApi(phCamera, lWBColorTemp);
		}

		return ret;
	}

	/// <summary>
	/// Gets the movie color temperature setting in movie mode for WBMode = ColorTemperature.
	/// </summary>
	/// <param name="phCamera">Camera handle.</param>
	/// <param name="plWBColorTemp">Returns the current COLOR TEMPERATURE, in degrees Kelvin.</param>
	/// <returns>API result.</returns>
	public long XSDK_GetMovieWBColorTemp(IntPtr phCamera, ref int plWBColorTemp)
	{
		long ret = XSDK_ERROR;

		plWBColorTemp = 0;
		if (m_pfnGetMovieWBColorTemp != IntPtr.Zero)
		{
			XSDK_GetMovieWBColorTemp_Delegate xsdkApi = (XSDK_GetMovieWBColorTemp_Delegate)Marshal.GetDelegateForFunctionPointer(m_pfnGetMovieWBColorTemp, typeof(XSDK_GetMovieWBColorTemp_Delegate));
			ret = xsdkApi(phCamera, ref plWBColorTemp);
		}

		return ret;
	}
	#endregion // Sample Implementation for C# of model common API.

	#region Sample Implementation for C# of model dependant API.

	#region Delegate for C# of model dependant API.
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_CapProp_Delegate_Param1(IntPtr phCamera, int lAPICode, int lAPIParam, ref int param1, IntPtr param2);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_CapProp_Delegate_Param2(IntPtr phCamera, int lAPICode, int lAPIParam, ref int param1, ref int param2, IntPtr param3, IntPtr param4);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_CapProp_Delegate_Param3(IntPtr phCamera, int lAPICode, int lAPIParam, ref int param1, IntPtr param2, ref int param3, IntPtr param4);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_CapProp_Delegate_Param4(IntPtr phCamera, int lAPICode, int lAPIParam, ref int param1, ref int param2, ref int param3, IntPtr param4, IntPtr param5, IntPtr param6);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_CapProp_Delegate_Param5(IntPtr phCamera, int lAPICode, int lAPIParam, int param1, ref int param2, IntPtr param3);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_SetProp_Delegate_Param1(IntPtr phCamera, int lAPICode, int lAPIParam, int param1);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_SetProp_Delegate_Param2(IntPtr phCamera, int lAPICode, int lAPIParam, int param1, int param2);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_SetProp_Delegate_Param3(IntPtr phCamera, int lAPICode, int lAPIParam, int param1, int param2, int param3);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_SetProp_Delegate_Param5(IntPtr phCamera, int lAPICode, int lAPIParam, int param1, int param2, int param3, int param4, int param5);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_SetProp_Delegate_Param6(IntPtr phCamera, int lAPICode, int lAPIParam, int param1, int param2, int param3, int param4, int param5, int param6);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_GetProp_Delegate_Param1(IntPtr phCamera, int lAPICode, int lAPIParam, ref int param1);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_GetProp_Delegate_Param2(IntPtr phCamera, int lAPICode, int lAPIParam, ref int param1, ref int param2);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_GetProp_Delegate_Param3(IntPtr phCamera, int lAPICode, int lAPIParam, ref int param1, ref int param2, ref int param3);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_GetProp_Delegate_Param4(IntPtr phCamera, int lAPICode, int lAPIParam, ref int param1, ref int param2, ref int param3, ref int param4);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_GetProp_Delegate_Param6(IntPtr phCamera, int lAPICode, int lAPIParam, ref int param1, ref int param2, ref int param3, ref int param4, ref int param5, ref int param6);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_GetProp_Delegate_Param8(IntPtr phCamera, int lAPICode, int lAPIParam, ref int param1, ref int param2, ref int param3, ref int param4, ref int param5, ref int param6, ref int param7, ref int param8);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_Function_Delegate_Special1(IntPtr phCamera, int lAPICode, int lAPIParam, int param1, IntPtr param2, IntPtr param3);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_Function_Delegate_Special2(IntPtr phCamera, int lAPICode, int lAPIParam, int param1, IntPtr param2);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_Function_Delegate_Special3(IntPtr phCamera, int lAPICode, int lAPIParam);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_Function_Delegate_Special4(IntPtr phCamera, int lAPICode, int lAPIParam, IntPtr param1);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_Function_Delegate_Special5(IntPtr phCamera, int lAPICode, int lAPIParam, IntPtr param1, IntPtr param2);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_Function_Delegate_Special6(IntPtr phCamera, int lAPICode, int lAPIParam, int param1, int param2, IntPtr param3);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_Function_Delegate_Special7(IntPtr phCamera, int lAPICode, int lAPIParam, int param1, int param2, int param3, IntPtr param4, IntPtr param5);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_Function_Delegate_Special8(IntPtr phCamera, int lAPICode, int lAPIParam, int param1, IntPtr param2, IntPtr param3, IntPtr param4, IntPtr param5);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_Function_Delegate_Special9(IntPtr phCamera, int lAPICode, int lAPIParam, IntPtr param1, IntPtr param2, IntPtr param3, IntPtr param4);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_Function_Delegate_Special10(IntPtr phCamera, int lAPICode, int lAPIParam, int param1, ref int param2, ref int param3, ref int param4, ref int param5);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_Function_Delegate_Special11(IntPtr phCamera, int lAPICode, int lAPIParam, IntPtr param1);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_Function_Delegate_Special12(IntPtr phCamera, int lAPICode, int lAPIParam, int param1, string param2, int param3);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_Function_Delegate_Special13(IntPtr phCamera, int lAPICode, int lAPIParam, IntPtr param1, IntPtr param2);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_Function_Delegate_Special14(IntPtr phCamera, int lAPICode, int lAPIParam, ref SDK_MOVIE_AFC_CUSTOM param1, ref SDK_MOVIE_AFC_CUSTOM param2);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_Function_Delegate_Special15(IntPtr phCamera, int lAPICode, int lAPIParam, StringBuilder param1);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_Function_Delegate_Special16(IntPtr phCamera, int lAPICode, int lAPIParam, string  param1);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_Function_Delegate_Special17(IntPtr phCamera, int lAPICode, int lAPIParam, ref int param1, IntPtr param2);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_Function_Delegate_Special18(IntPtr phCamera, int lAPICode, int lAPIParam, int param1, SDK_FocusArea param2);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_Function_Delegate_Special19(IntPtr phCamera, int lAPICode, int lAPIParam, int param1, StringBuilder param2);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_Function_Delegate_Special20(IntPtr phCamera, int lAPICode, int lAPIParam, int param1, ref int param2);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_Function_Delegate_Special21(IntPtr phCamera, int lAPICode, int lAPIParam, int param1, string param2);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_Function_Delegate_Special22(IntPtr phCamera, int lAPICode, int lAPIParam, int param1, IntPtr param2, ref int param3);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_Function_Delegate_Special23(IntPtr phCamera, int lAPICode, int lAPIParam, StringBuilder param1, StringBuilder param2);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_Function_Delegate_Special24(IntPtr phCamera, int lAPICode, int lAPIParam, ref int param1, ref SDK_FaceFrameInfo[] param2);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int XSDK_Function_Delegate_Special25(IntPtr phCamera, int lAPICode, int lAPIParam, ref int param1, ref int param2);

	#endregion // Delegate for C# of model dependant API.

	/// <summary>
	/// Queries supported values for a model-dependent function.
	/// </summary>
	/// <returns>API result.</returns>
	public long SDK_CapProp_LPPPP(IntPtr phCamera, int lApiCode, int lParam, int pParam1, ref int pParam2, ref int pParam3, ref int pParam4, ref int pParam5)
	{
		long ret = XSDK_ERROR;
		try
		{
			switch (lApiCode)
			{
				case 0x2324:
				case 0x3601:
					if (m_pfnCapProp != IntPtr.Zero)
					{
						XSDK_Function_Delegate_Special10 xsdkApi = (XSDK_Function_Delegate_Special10)Marshal.GetDelegateForFunctionPointer(m_pfnCapProp, typeof(XSDK_Function_Delegate_Special10));
						ret = xsdkApi(phCamera, lApiCode, lParam, pParam1, ref pParam2, ref pParam3, ref pParam4, ref pParam5);
					}
					else
					{
						pParam2 = 0;
						pParam3 = 0;
						pParam4 = 0;
						pParam5 = 0;
					}
					break;
				default:
					ret = XSDK_ERROR;
					break;
			}
		}
		catch(Exception)
		{
			ret = XSDK_ERROR;
		}
		return ret;
	}

	/// <summary>
	/// Queries supported values for a model-dependent function.
	/// </summary>
	/// <returns>API result.</returns>
	public long SDK_CapProp_LPP(IntPtr phCamera, int lApiCode, int lParam, int pParam1, ref object pParam2, ref object pParam3)
	{
		long ret = XSDK_ERROR;
		try
		{
			if (m_pfnCapProp != IntPtr.Zero)
			{
				switch (lApiCode)
				{
					case 0x2213:
					case 0x2235:
						IntPtr param2 = IntPtr.Zero;
						IntPtr param3 = IntPtr.Zero;
						XSDK_Function_Delegate_Special1 xsdkApi = (XSDK_Function_Delegate_Special1)Marshal.GetDelegateForFunctionPointer(m_pfnCapProp, typeof(XSDK_Function_Delegate_Special1));
						System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
						try { }
						finally
						{
							param2 = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(SDK_FocusArea)));
							param3 = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(SDK_FocusArea)));
							try
							{
								Marshal.StructureToPtr(pParam2, param2, false);
								Marshal.StructureToPtr(pParam3, param3, false);
								ret = xsdkApi(phCamera, lApiCode, lParam, pParam1, param2, param3);
								if (ret == (int)FUJI_SDK_ERR.FUJI_SDK_ERR_OK)
								{
									pParam2 = (SDK_FocusArea)Marshal.PtrToStructure(param2, typeof(SDK_FocusArea));
									pParam3 = (SDK_FocusArea)Marshal.PtrToStructure(param3, typeof(SDK_FocusArea));
								}
							}
							finally
							{
								Marshal.FreeCoTaskMem(param2);
								Marshal.FreeCoTaskMem(param3);
							}
						}
						break;
					default:
						int num = 0;
						int para2 = 0; 
						if (pParam2 is int)
						{
							num = (int)pParam2;
							para2 = (int)pParam2;
							para2 = (int)pParam2;
						}
						IntPtr parray = IntPtr.Zero;
						XSDK_CapProp_Delegate_Param5 xsdkApi2 = (XSDK_CapProp_Delegate_Param5)Marshal.GetDelegateForFunctionPointer(m_pfnCapProp, typeof(XSDK_CapProp_Delegate_Param5));
						System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
						try { }
						finally
						{
							if (num != 0)
							{
								parray = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * num);
							}
							try
							{
								ret = xsdkApi2(phCamera, lApiCode, lParam, pParam1, ref para2, parray);

								if (num != 0)
								{
									int max = para2;
									if (num < max)
									{
										max = num;
									}
									Marshal.Copy(parray, (int[])pParam3, 0, max);
									
								}
								if (pParam2 is int)
								{
									pParam2 = para2;
								}
								}
							finally
							{
								if (num != 0)
								{
									Marshal.FreeCoTaskMem(parray);
								}
							}
						}
					break;
				}
			}
		}
		catch(Exception)
		{ 
			ret = XSDK_ERROR; 
		}
		return ret;
	}

	/// <summary>
	/// Queries supported values for a model-dependent function.
	/// </summary>
	/// <returns>API result.</returns>
	public long SDK_CapProp_PPPP(IntPtr phCamera, int lApiCode, int lParam, ref object pParam1, ref object pParam2, ref object pParam3, ref object pParam4)
	{
		long ret = XSDK_ERROR;
		try
		{
			if (m_pfnCapProp != IntPtr.Zero)
			{
				int num1 = 0;
				int num2 = 0;
				int ipar1 = 0;
				int ipar2 = 0;
				IntPtr parray1 = IntPtr.Zero;
				IntPtr parray2 = IntPtr.Zero;
				switch (lApiCode)
				{
					case 0x2196:
					case 0x2243:
					case 0x3607:
					case 0x4012:
					case 0x423A:
						num1 = (int)pParam1;
						num2 = (int)pParam2;
						ipar1 = (int)pParam1;
						ipar2 = (int)pParam2;
						XSDK_CapProp_Delegate_Param2 xsdkApi1 = (XSDK_CapProp_Delegate_Param2)Marshal.GetDelegateForFunctionPointer(m_pfnCapProp, typeof(XSDK_CapProp_Delegate_Param2));
						System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
						try { }
						finally
						{
							if (num1 != 0)
							{
								parray1 = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * (int)pParam1);
							}
							if (num2 != 0)
							{
								parray2 = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * (int)pParam2);
							}
							try
							{
								ret = xsdkApi1(phCamera, lApiCode, lParam, ref ipar1, ref ipar2, parray1, parray2);

								if (num1 != 0)
								{
									int max1 = (int)pParam1;
									if (num1 < max1)
									{
										max1 = num1;
									}
									int[] ppara1 = new int[max1];
									for (int i = 0; i < max1; i++)
									{
										IntPtr currentPos = IntPtr.Add(parray1, i * Marshal.SizeOf(typeof(int)));
										ppara1[i] = Marshal.PtrToStructure<int>(currentPos);
									}
									pParam3 = ppara1;

								}
								if (num2 != 0)
								{
									int max2 = (int)pParam2;
									if (num2 < max2)
									{
										max2 = num2;
									}
										int[] ppara2 = new int[max2];
										for (int i = 0; i < max2; i++)
										{
											IntPtr currentPos = IntPtr.Add(parray2, i * Marshal.SizeOf(typeof(int)));
											ppara2[i] = Marshal.PtrToStructure<int>(currentPos);
										}
										pParam4 = ppara2;
									}
									pParam1 = ipar1;
									pParam2 = ipar2;
								}
							finally
							{
								if (num1 != 0)
								{
									Marshal.FreeCoTaskMem(parray1);
								}
								if (num2 != 0)
								{
									Marshal.FreeCoTaskMem(parray2);
								}
							}
						}
						break;

					case 0x2272:
					case 0x2274:
						if (pParam1 is int)
						{
							num1 = (int)pParam1;
						}
						if (pParam3 is int)
						{
							num2 = (int)pParam3;
						}
						XSDK_CapProp_Delegate_Param3 xsdkApi2 = (XSDK_CapProp_Delegate_Param3)Marshal.GetDelegateForFunctionPointer(m_pfnCapProp, typeof(XSDK_CapProp_Delegate_Param3));
						System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
						try { }
						finally
						{
							if (num1 != 0)
							{
								parray1 = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * (int)pParam1);
							}
							if (num2 != 0)
							{
								parray2 = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * (int)pParam2);
							}
							try
							{
								ret = xsdkApi2(phCamera, lApiCode, lParam, ref num1, parray1, ref num2, parray2);

								if (num1 != 0)
								{
									int max1 = (int)pParam1;
									if (num1 < max1)
									{
										max1 = num1;
									}
									Marshal.Copy(parray1, (int[])pParam2, 0, max1);
									if (pParam1 is int)
									{
										pParam1 = num1;
									}
								}
								if (num2 != 0)
								{
									int max2 = (int)pParam3;
									if (num2 < max2)
									{
										max2 = num2;
									}
									Marshal.Copy(parray2, (int[])pParam4, 0, max2);
									if (pParam3 is int)
									{
										pParam3 = num2;
									}
								}
							}
							finally
							{
								if (num1 != 0)
								{
									Marshal.FreeCoTaskMem(parray1);
								}
								if (num2 != 0)
								{
									Marshal.FreeCoTaskMem(parray2);
								}
							}
						}
						break;
					default:
						ret = XSDK_ERROR;
						break;
				}
			}
		}
		catch(Exception)
		{
			ret = XSDK_ERROR;
		}
		return ret;
	}

	/// <summary>
	/// Queries supported values for a model-dependent function.
	/// </summary>
	/// <returns>API result.</returns>
	public long SDK_CapProp_PPPPPP(IntPtr phCamera, int lApiCode, int lParam, ref int pParam1, ref int pParam2, ref int pParam3, ref int[] pParam4, ref int[] pParam5, ref int[] pParam6)
	{
		long ret = XSDK_ERROR;
		try
		{
			if (m_pfnCapProp != IntPtr.Zero)
			{
				switch (lApiCode)
				{
					case 0x4078:
						int num1 = 0;
						int num2 = 0;
						int num3 = 0;
						if (pParam1 is int)
						{
							num1 = (int)pParam1;
						}
						if (pParam2 is int)
						{
							num2 = (int)pParam2;
						}
						if (pParam3 is int)
						{
							num3 = (int)pParam3;
						}
						IntPtr parray1 = IntPtr.Zero;
						IntPtr parray2 = IntPtr.Zero;
						IntPtr parray3 = IntPtr.Zero;
						XSDK_CapProp_Delegate_Param4 xsdkApi = (XSDK_CapProp_Delegate_Param4)Marshal.GetDelegateForFunctionPointer(m_pfnCapProp, typeof(XSDK_CapProp_Delegate_Param4));
						System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
						try { }
						finally
						{
							if (num1 != 0)
							{
								parray1 = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * (int)pParam1);
							}
							if (num2 != 0)
							{
								parray2 = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * (int)pParam2);
							}
							if (num3 != 0)
							{
								parray3 = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * (int)pParam3);
							}
							try
							{
								ret = xsdkApi(phCamera, lApiCode, lParam, ref num1, ref num2, ref num3, parray1, parray2, parray3);

								if (num1 != 0)
								{
									int max1 = (int)pParam1;
									if (num1 < max1)
									{
										max1 = num1;
									}
									Marshal.Copy(parray1, (int[])pParam4, 0, max1);
									if (pParam1 is int)
									{
										pParam1 = num1;
									}
								}
								if (num2 != 0)
								{
									int max2 = (int)pParam2;
									if (num2 < max2)
									{
										max2 = num2;
									}
									Marshal.Copy(parray2, (int[])pParam5, 0, max2);
									if (pParam2 is int)
									{
										pParam2 = num2;
									}
								}
								if (num3 != 0)
								{
									int max3 = (int)pParam3;
									if (num3 < max3)
									{
										max3 = num3;
									}
									Marshal.Copy(parray3, (int[])pParam6, 0, max3);
									if (pParam3 is int)
									{
										pParam3 = num3;
									}
								}
							}
							finally
							{
								if (num1 != 0)
								{
									Marshal.FreeCoTaskMem(parray1);
								}
								if (num2 != 0)
								{
									Marshal.FreeCoTaskMem(parray2);
								}
								if (num3 != 0)
								{
									Marshal.FreeCoTaskMem(parray3);
								}
							}
						}
						break;
					default:
						ret = XSDK_ERROR;
						break;
				}
			}
		}
		catch(Exception)
		{
			ret = XSDK_ERROR;
		}
		return ret;
	}

	/// <summary>
	/// Queries supported values for a model-dependent function.
	/// </summary>
	/// <returns>API result.</returns>
	public long SDK_CapProp_PP(IntPtr phCamera, int lApiCode, int lParam, ref object pParam1, ref object pParam2)
	{
		long ret = XSDK_ERROR;
		try
		{
			if (m_pfnCapProp != IntPtr.Zero && m_pfnGetProp != IntPtr.Zero)
			{
				switch (lApiCode)
				{
					case 0x4237:
					case 0x4238:
						int par1 = 0;
						int par2 = 0;
						XSDK_Function_Delegate_Special25 xsdkApi = (XSDK_Function_Delegate_Special25)Marshal.GetDelegateForFunctionPointer(m_pfnCapProp, typeof(XSDK_Function_Delegate_Special25));
						ret = xsdkApi(phCamera, lApiCode, lParam, ref par1, ref par2);
						if (ret == (int)FUJI_SDK_ERR.FUJI_SDK_ERR_OK)
						{
							pParam1 = par1;
							pParam2 = par2;

						}
						break;
					case 0x362B:
							IntPtr par3 = IntPtr.Zero;
							IntPtr par4 = IntPtr.Zero;
							System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
						try { }
						finally
						{
							try
							{
							    par3 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SDK_MOVIE_AFC_CUSTOM)));
							    par4 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SDK_MOVIE_AFC_CUSTOM)));
								Marshal.StructureToPtr(pParam1, par3, false);
								Marshal.StructureToPtr(pParam2, par4, false);
								XSDK_Function_Delegate_Special5 xsdkApi1 = (XSDK_Function_Delegate_Special5)Marshal.GetDelegateForFunctionPointer(m_pfnCapProp, typeof(XSDK_Function_Delegate_Special5));
								ret = xsdkApi1(phCamera, lApiCode, lParam, par3, par4);
								if (ret == (int)FUJI_SDK_ERR.FUJI_SDK_ERR_OK)
								{
									pParam1 = (SDK_MOVIE_AFC_CUSTOM)Marshal.PtrToStructure(par3, typeof(SDK_MOVIE_AFC_CUSTOM));
									pParam2 = (SDK_MOVIE_AFC_CUSTOM)Marshal.PtrToStructure(par4, typeof(SDK_MOVIE_AFC_CUSTOM));
								}
							}
							finally
							{
								Marshal.FreeHGlobal(par3);
								Marshal.FreeHGlobal(par4);
							}
						}
						break;
					case 0x2259:
						int numFoucus = 0;
						IntPtr pFocusPos = IntPtr.Zero;
						numFoucus = (int)pParam1;
						int ipara1 = (int)pParam1;

						XSDK_CapProp_Delegate_Param1 xsdkApi2 = (XSDK_CapProp_Delegate_Param1)Marshal.GetDelegateForFunctionPointer(m_pfnCapProp, typeof(XSDK_CapProp_Delegate_Param1));
						System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
						try { }
						finally
						{
							if (numFoucus != 0)
							{
								pFocusPos = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SDK_FOCUS_POS_CAP)) * (int)pParam1);
							}
							try
							{
								ret = xsdkApi2(phCamera, lApiCode, lParam, ref ipara1, pFocusPos);

								if (numFoucus != 0)
								{
									int max = (int)pParam1;
									if (numFoucus < max)
									{
										max = numFoucus;
									}
										SDK_FOCUS_POS_CAP[] focusPosCap = new SDK_FOCUS_POS_CAP[ipara1];
										for (int i = 0; i < ipara1; i++)
										{
											IntPtr currentPos = IntPtr.Add(pFocusPos, i * Marshal.SizeOf(typeof(SDK_FOCUS_POS_CAP)));
											focusPosCap[i] = Marshal.PtrToStructure<SDK_FOCUS_POS_CAP>(currentPos);
										}
										pParam2 = focusPosCap;
									}
									pParam1 = ipara1;
								}
							finally
							{
								if (numFoucus != 0)
								{
									Marshal.FreeCoTaskMem(pFocusPos);
								}
							}
						}
						break;

					case 0x2287:
						int numAFZoneCustom = 0;
						IntPtr pAFZoneCustom = IntPtr.Zero;
						numAFZoneCustom = (int)pParam1;

						int ipara2 = (int)pParam1;

						XSDK_CapProp_Delegate_Param1 xsdkApi3 = (XSDK_CapProp_Delegate_Param1)Marshal.GetDelegateForFunctionPointer(m_pfnCapProp, typeof(XSDK_CapProp_Delegate_Param1));
						System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
						try { }
						finally
						{
							if (numAFZoneCustom != 0)
							{
								pAFZoneCustom = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SDK_AFZoneCustomCapablity)) * (int)pParam1);
							}
							try
							{
								ret = xsdkApi3(phCamera, lApiCode, lParam, ref ipara2, pAFZoneCustom);

								if (numAFZoneCustom != 0)
								{
									int max = (int)pParam1;
									if (numAFZoneCustom < max)
									{
										max = numAFZoneCustom;
									}
									SDK_AFZoneCustomCapablity[] _AFZoneCustomCapablities = new SDK_AFZoneCustomCapablity[ipara2];
									for (int i = 0; i < ipara2; i++)
									{
										IntPtr currentPos = IntPtr.Add(pAFZoneCustom, i * Marshal.SizeOf(typeof(SDK_AFZoneCustomCapablity)));
											_AFZoneCustomCapablities[i] = Marshal.PtrToStructure<SDK_AFZoneCustomCapablity>(currentPos);
									}
									pParam2 = _AFZoneCustomCapablities;

								}
									pParam1 = ipara2;
								}
							finally
							{
								if (numAFZoneCustom != 0)
								{
									Marshal.FreeCoTaskMem(pAFZoneCustom);
								}
							}
						}
						break;
					default:
						int num = 0;
							num = (int)pParam1;
						int	ipar1 = (int)pParam1;

							IntPtr parray = IntPtr.Zero;
						XSDK_CapProp_Delegate_Param1 xsdkApi4 = (XSDK_CapProp_Delegate_Param1)Marshal.GetDelegateForFunctionPointer(m_pfnCapProp, typeof(XSDK_CapProp_Delegate_Param1));
						System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
						try { }
						finally
						{
							if (num != 0)
							{
								parray = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * (int)pParam1);
							}
							try
							{
								ret = xsdkApi4(phCamera, lApiCode, lParam, ref ipar1, parray);

								if (num != 0)
								{
									int max = (int)pParam1;
									if (num < max)
									{
										max = num;
									}
									Marshal.Copy(parray, (int[])pParam2, 0, max);		
								}
									pParam1 = ipar1;
								}
							finally
							{
								if (num != 0)
								{
									Marshal.FreeCoTaskMem(parray);
								}
							}
						}
						break;
				}
			}
		}
		catch(Exception)
		{
			ret = XSDK_ERROR;
		}

		return ret;
	}

	/// <summary>
	/// Sets values for the model-dependent function.
	/// </summary>
	/// <returns>API result.</returns>
	public long SDK_SetProp_L(IntPtr phCamera, int lApiCode, int lParam, int lParam1)
	{
		long ret = XSDK_ERROR;
		if (m_pfnSetProp != IntPtr.Zero)
		{
			XSDK_SetProp_Delegate_Param1 xsdkApi = (XSDK_SetProp_Delegate_Param1)Marshal.GetDelegateForFunctionPointer(m_pfnSetProp, typeof(XSDK_SetProp_Delegate_Param1));
			ret = xsdkApi(phCamera, lApiCode, lParam, lParam1);
		}
		return ret;
	}

	public long SDK_SetProp_P(IntPtr phCamera, int lApiCode, int lParam, object lParam1)
	{
		long ret = XSDK_ERROR;
		try
		{
			if (m_pfnSetProp != IntPtr.Zero)
			{
				IntPtr par1;
				switch (lApiCode)
				{
					case 0x2167:
						SDK_FaceFrameInfo pVoid1;
						XSDK_Function_Delegate_Special11 xsdkApi1 = (XSDK_Function_Delegate_Special11)Marshal.GetDelegateForFunctionPointer(m_pfnSetProp, typeof(XSDK_Function_Delegate_Special11));
						System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
						pVoid1 = (SDK_FaceFrameInfo)lParam1;
						try { }
						finally
						{
							par1 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SDK_FaceFrameInfo)));
							try
							{
								Marshal.StructureToPtr(pVoid1, par1, false);
								ret = xsdkApi1(phCamera, lApiCode, lParam, par1);
							}
							finally
							{ 
								Marshal.FreeCoTaskMem(par1);
								
							}
						}
					    break;
				    case 0x362C:
						SDK_MOVIE_AFC_CUSTOM pVoid2;
						XSDK_Function_Delegate_Special11 xsdkApi2 = (XSDK_Function_Delegate_Special11)Marshal.GetDelegateForFunctionPointer(m_pfnSetProp, typeof(XSDK_Function_Delegate_Special11));
						System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
						pVoid2 = (SDK_MOVIE_AFC_CUSTOM)lParam1;
						try { }
						finally
						{
							par1 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SDK_MOVIE_AFC_CUSTOM)));
							try
							{								
								Marshal.StructureToPtr(pVoid2, par1, false);
								ret = xsdkApi2(phCamera, lApiCode, lParam, par1);
							}
							finally
							{
								Marshal.FreeCoTaskMem(par1);

							}
						}
						break;
				}
			}
		}
		catch(Exception)
		{
			ret = XSDK_ERROR;
		}
		return ret;
	}
	
	/// <summary>
	/// Sets values for the model-dependent function.
	/// </summary>
	/// <returns>API result.</returns>
	public long SDK_SetProp_LP(IntPtr phCamera, int lApiCode, int lParam, int lParam1, object lParam2)
	{ 
		long ret = XSDK_ERROR;

		try
		{
			if (m_pfnSetProp != IntPtr.Zero)
			{
				switch (lApiCode)
				{
					case 0x2205:
					case 0x2214:
						try { }
						finally
						{
							IntPtr param2 = IntPtr.Zero;
							XSDK_Function_Delegate_Special18 xsdkApi = (XSDK_Function_Delegate_Special18)Marshal.GetDelegateForFunctionPointer(m_pfnSetProp, typeof(XSDK_Function_Delegate_Special18));
							System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
							try { }
							finally
							{
								SDK_FocusArea focusArea = new SDK_FocusArea();
									focusArea = (SDK_FocusArea)lParam2;
								param2 = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(SDK_FocusArea)));
								try
								{
									Marshal.StructureToPtr(lParam2, param2, false);
									ret = xsdkApi(phCamera, lApiCode, lParam, lParam1, focusArea);
								}
								finally
								{
									Marshal.FreeCoTaskMem(param2);
								}
							}
						}
						break;
					case 0x2288:
						try { }
						finally
						{
							IntPtr param2 = IntPtr.Zero;
							XSDK_Function_Delegate_Special2 xsdkApi = (XSDK_Function_Delegate_Special2)Marshal.GetDelegateForFunctionPointer(m_pfnSetProp, typeof(XSDK_Function_Delegate_Special2));
							System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
							try { }
							finally
							{
								param2 = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(SDK_AFZoneCustomCapablity)));
								try
								{
									Marshal.StructureToPtr(lParam2, param2, false);
									ret = xsdkApi(phCamera, lApiCode, lParam, lParam1, param2);
								}
								finally
								{
									Marshal.FreeCoTaskMem(param2);
								}
							}
						}
						break;
					case 0x2353:
					case 0x2357:
						try { }
						finally
						{
							IntPtr param2 = IntPtr.Zero;
							XSDK_Function_Delegate_Special2 xsdkApi = (XSDK_Function_Delegate_Special2)Marshal.GetDelegateForFunctionPointer(m_pfnSetProp, typeof(XSDK_Function_Delegate_Special2));
							System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
							try { }
							finally
							{
								param2 = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(SDK_CustomWBArea)));
								try
								{
									Marshal.StructureToPtr(lParam2, param2, false);
									ret = xsdkApi(phCamera, lApiCode, lParam, lParam1, param2);
								}
								finally
								{
									Marshal.FreeCoTaskMem(param2);
								}
							}
						}
						break;
					case 0x4211:
						try { }
						finally
						{
							IntPtr param2 = IntPtr.Zero;
							XSDK_Function_Delegate_Special2 xsdkApi = (XSDK_Function_Delegate_Special2)Marshal.GetDelegateForFunctionPointer(m_pfnSetProp, typeof(XSDK_Function_Delegate_Special2));
							System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
							try { }
							finally
							{
								param2 = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(SDK_FrameGuideGridInfo)));
								try
								{
									Marshal.StructureToPtr(lParam2, param2, false);
									ret = xsdkApi(phCamera, lApiCode, lParam, lParam1, param2);
								}
								finally
								{
									Marshal.FreeCoTaskMem(param2);
								}
							}
						}
						break;
					default:
						ret = XSDK_ERROR;
						break;
				}
			}
		}
		catch (Exception)
		{
			ret = XSDK_ERROR;
		}
		return ret;
	}

	/// <summary>
	/// Sets values for the model-dependent function.
	/// </summary>
	/// <returns>API result.</returns>
	public long SDK_SetProp_LL(IntPtr phCamera, int lApiCode, int lParam, int lParam1, int lParam2)
	{
		long ret = XSDK_ERROR;
		if (m_pfnSetProp != IntPtr.Zero)
		{
			XSDK_SetProp_Delegate_Param2 xsdkApi = (XSDK_SetProp_Delegate_Param2)Marshal.GetDelegateForFunctionPointer(m_pfnSetProp, typeof(XSDK_SetProp_Delegate_Param2));
			ret = xsdkApi(phCamera, lApiCode, lParam, lParam1, lParam2);
		}
		return ret;
	}

	/// <summary>
	/// Sets values for the model-dependent function.
	/// </summary>
	/// <returns>API result.</returns>
	public long SDK_SetProp_LLL(IntPtr phCamera, int lApiCode, int lParam, int lParam1, int lParam2, int lParam3)
	{
		long ret = XSDK_ERROR;
		if (m_pfnSetProp != IntPtr.Zero)
		{
			XSDK_SetProp_Delegate_Param3 xsdkApi = (XSDK_SetProp_Delegate_Param3)Marshal.GetDelegateForFunctionPointer(m_pfnSetProp, typeof(XSDK_SetProp_Delegate_Param3));
			ret = xsdkApi(phCamera, lApiCode, lParam, lParam1, lParam2, lParam3);
		}
		return ret;
	}

	/// <summary>
	/// Sets values for the model-dependent function.
	/// </summary>
	/// <returns>API result.</returns>
	public long SDK_SetProp_LLLLL(IntPtr phCamera, int lApiCode, int lParam, int lParam1, int lParam2, int lParam3, int lParam4, int lParam5)
	{
		long ret = XSDK_ERROR;
		if (m_pfnSetProp != IntPtr.Zero)
		{
			XSDK_SetProp_Delegate_Param5 xsdkApi = (XSDK_SetProp_Delegate_Param5)Marshal.GetDelegateForFunctionPointer(m_pfnSetProp, typeof(XSDK_SetProp_Delegate_Param5));
			ret = xsdkApi(phCamera, lApiCode, lParam, lParam1, lParam2, lParam3, lParam4, lParam5);
		}
		return ret;
	}

	/// <summary>
	/// Sets values for the model-dependent function.
	/// </summary>
	/// <returns>API result.</returns>
	public long SDK_SetProp_LLLLLL(IntPtr phCamera, int lApiCode, int lParam, int lParam1, int lParam2, int lParam3, int lParam4, int lParam5, int lParam6)
	{
		long ret = XSDK_ERROR;
		if (m_pfnSetProp != IntPtr.Zero)
		{
			XSDK_SetProp_Delegate_Param6 xsdkApi = (XSDK_SetProp_Delegate_Param6)Marshal.GetDelegateForFunctionPointer(m_pfnSetProp, typeof(XSDK_SetProp_Delegate_Param6));
			ret = xsdkApi(phCamera, lApiCode, lParam, lParam1, lParam2, lParam3, lParam4, lParam5, lParam6);
		}
		return ret;
	}

	/// <summary>
	/// Sets values for the model-dependent function.
	/// </summary>
	/// <returns>API result.</returns>
	public long SDK_SetProp_LS(IntPtr phCamera, int lApiCode, int lParam, int lParam1, string lParam2)
	{
		long ret = XSDK_ERROR;

			try
			{
				if (m_pfnSetProp != IntPtr.Zero)
				{
					XSDK_Function_Delegate_Special21 xsdkApi = (XSDK_Function_Delegate_Special21)Marshal.GetDelegateForFunctionPointer(m_pfnSetProp, typeof(XSDK_Function_Delegate_Special21));
					
					ret = xsdkApi(phCamera, lApiCode, lParam, lParam1, lParam2);
				}
			}
			catch (Exception)
			{
				ret = XSDK_ERROR;
			}
		return ret;
	}

	/// <summary>
	/// Sets values for the model-dependent function.
	/// </summary>
	/// <returns>API result.</returns>
	public long SDK_SetProp_LSL(IntPtr phCamera, int lApiCode, int lParam, int lParam1, string lParam2, int lParam3)
	{
		long ret = XSDK_ERROR;

		try
		{
			if (m_pfnSetProp != IntPtr.Zero)
			{
				try { }
				finally
				{
					XSDK_Function_Delegate_Special12 xsdkApi = (XSDK_Function_Delegate_Special12)Marshal.GetDelegateForFunctionPointer(m_pfnSetProp, typeof(XSDK_Function_Delegate_Special12));
					System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
					IntPtr par1 = Marshal.StringToHGlobalAnsi(lParam2);
					try
					{
						ret = xsdkApi(phCamera, lApiCode, lParam, lParam1, lParam2, lParam3);
					}
					finally
					{
						Marshal.FreeCoTaskMem(par1);
					}
				}
			}
		}
		catch (Exception)
		{
			ret = XSDK_ERROR;
		}
		return ret;
	}

	/// <summary>
	/// Sets values for the model-dependent function.
	/// </summary>
	/// <returns>API result.</returns>
	public long SDK_SetProp_S(IntPtr phCamera, int lApiCode, int lParam, string lParam1)
	{
		long ret = XSDK_ERROR;

		try
		{
			if (m_pfnSetProp != IntPtr.Zero)
			{
				XSDK_Function_Delegate_Special16 xsdkApi = (XSDK_Function_Delegate_Special16)Marshal.GetDelegateForFunctionPointer(m_pfnSetProp, typeof(XSDK_Function_Delegate_Special16));
				ret = xsdkApi(phCamera, lApiCode, lParam, lParam1);
			}
		}
		catch (Exception)
		{
			ret = XSDK_ERROR;
		}
		return ret;
	}

	/// <summary>
	/// Sets values for the model-dependent function.
	/// </summary>
	/// <returns>API result.</returns>
	public long SDK_SetProp_SS(IntPtr phCamera, int lApiCode, int lParam, string lParam1, string lParam2)
	{
		long ret = XSDK_ERROR;

		try
		{
			if (m_pfnSetProp != IntPtr.Zero)
			{
				try { }
				finally
				{
					IntPtr par1 = Marshal.StringToHGlobalAnsi(lParam1);
					IntPtr par2 = Marshal.StringToHGlobalAnsi(lParam2);
					try
					{
						XSDK_Function_Delegate_Special13 xsdkApi = (XSDK_Function_Delegate_Special13)Marshal.GetDelegateForFunctionPointer(m_pfnSetProp, typeof(XSDK_Function_Delegate_Special13));
						System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
						ret = xsdkApi(phCamera, lApiCode, lParam, par1, par2);
					}
					finally
					{
						Marshal.FreeCoTaskMem(par1);
					}
				}
			}
		}
		catch (Exception)
		{
			ret = XSDK_ERROR;
		}
		return ret;
	}

	/// <summary>
	/// Sets values for the model-dependent function.
	/// </summary>
	/// <returns>API result.</returns>
	public long SDK_SetProp(IntPtr phCamera, int lApiCode, int lParam)
	{
		long ret = XSDK_ERROR;
		try
		{
			if (m_pfnSetProp != IntPtr.Zero)
			{
				XSDK_Function_Delegate_Special3 xsdkApi = (XSDK_Function_Delegate_Special3)Marshal.GetDelegateForFunctionPointer(m_pfnSetProp, typeof(XSDK_Function_Delegate_Special3));
				ret = xsdkApi(phCamera, lApiCode, lParam);
			}
		}
		catch (Exception)
		{
			ret = XSDK_ERROR;
		}
		return ret;
	}

	/// <summary>
	/// Gets the settings for the model-dependent function.
	/// </summary>
	/// <returns>API result.</returns>
	public long SDK_GetProp_P(IntPtr phCamera, int lApiCode, int lParam, ref object pParam1)
	{
		long ret = XSDK_ERROR;
		try
		{
			if (m_pfnGetProp != IntPtr.Zero)
			{
					IntPtr par1;
					XSDK_Function_Delegate_Special4 xsdkApi = (XSDK_Function_Delegate_Special4)Marshal.GetDelegateForFunctionPointer(m_pfnGetProp, typeof(XSDK_Function_Delegate_Special4));
					XSDK_GetProp_Delegate_Param1 xsdkApi2 = (XSDK_GetProp_Delegate_Param1)Marshal.GetDelegateForFunctionPointer(m_pfnGetProp, typeof(XSDK_GetProp_Delegate_Param1));
					XSDK_Function_Delegate_Special15 xsdkApi3 = (XSDK_Function_Delegate_Special15)Marshal.GetDelegateForFunctionPointer(m_pfnGetProp, typeof(XSDK_Function_Delegate_Special15));
					System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
					switch (lApiCode)
					{
						case 0x2269:				
							try { }
							finally
							{
								par1 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SDK_CropAreaFrameInfo)));
								try
								{	
									Marshal.StructureToPtr(pParam1,par1, false);
									ret = xsdkApi(phCamera, lApiCode, lParam,par1);
									pParam1 = (SDK_CropAreaFrameInfo)Marshal.PtrToStructure(par1, typeof(SDK_CropAreaFrameInfo));

								}
								finally
								{
									Marshal.FreeCoTaskMem(par1);
									
								}
							}
							break;
						case 0x226B:
							try { }
							finally
							{
								par1 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SDK_FOCUS_LIMITER_INDICATOR)));
								try
								{
									Marshal.StructureToPtr(pParam1,par1, false);
									ret = xsdkApi(phCamera, lApiCode, lParam,par1);
									pParam1 = (SDK_FOCUS_LIMITER_INDICATOR)Marshal.PtrToStructure(par1, typeof(SDK_FOCUS_LIMITER_INDICATOR));

								}
								finally
								{
									Marshal.FreeCoTaskMem(par1);

								}
							}
							break;
						case 0x228A:					
							try { }
							finally
							{
								par1 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SDK_TrackingAfFrameInfo)));
								try
								{	
									Marshal.StructureToPtr(pParam1,par1, false);
									ret = xsdkApi(phCamera, lApiCode, lParam,par1);
									pParam1 = (SDK_TrackingAfFrameInfo)Marshal.PtrToStructure(par1, typeof(SDK_TrackingAfFrameInfo));
								}
								finally
								{
									Marshal.FreeCoTaskMem(par1);

								}
							}
							break;
						case 0x357B:					
							try { }
							finally
							{
								par1 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SDK_MovieTransparentFrameInfo)));
								try
								{ 
									Marshal.StructureToPtr(pParam1,par1, false);
									ret = xsdkApi(phCamera, lApiCode, lParam,par1);
									pParam1 = (SDK_MovieTransparentFrameInfo)Marshal.PtrToStructure(par1, typeof(SDK_MovieTransparentFrameInfo));
								}
								finally
								{
									Marshal.FreeCoTaskMem(par1);

								}
							}
							break;
						case 0x362D:				
							try { }
							finally
							{
								par1 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SDK_MOVIE_AFC_CUSTOM)));
								try
								{
									Marshal.StructureToPtr(pParam1,par1, false);
									ret = xsdkApi(phCamera, lApiCode, lParam,par1);
									pParam1 = (SDK_MOVIE_AFC_CUSTOM)Marshal.PtrToStructure(par1, typeof(SDK_MOVIE_AFC_CUSTOM));
								}
								finally
								{
									Marshal.FreeCoTaskMem(par1);

								}
							}
							break;
						case 0x3922:				
							try { }
							finally
							{
								par1 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SDK_MICLEVEL_INDICATOR)));
								try
								{									
									Marshal.StructureToPtr(pParam1,par1, false);
									ret = xsdkApi(phCamera, lApiCode, lParam,par1);
									pParam1 = (SDK_MICLEVEL_INDICATOR)Marshal.PtrToStructure(par1, typeof(SDK_MICLEVEL_INDICATOR));
								}
								finally
								{
									Marshal.FreeCoTaskMem(par1);

								}
							}
							break;
						case 0x4044:
						case 0x407E:
							System.Text.StringBuilder sb = new System.Text.StringBuilder(256);
							ret = xsdkApi3(phCamera, lApiCode, lParam, sb);
							if (ret == (int)FUJI_SDK_ERR.FUJI_SDK_ERR_OK)
							{
								pParam1 = sb.ToString();
							}
							break;
						default:
							int iParam = 0;
							ret = xsdkApi2(phCamera, lApiCode, lParam, ref iParam);
							pParam1 = iParam;
							break;
					}
				}
		}
		catch (Exception)
		{
			ret = XSDK_ERROR;
		}
		return ret;
	}

	/// <summary>
	/// Gets the settings for the model-dependent function.
	/// </summary>
	/// <returns>API result.</returns>
	public long SDK_GetProp_PP(IntPtr phCamera, int lApiCode, int lParam, ref object pParam1, ref object pParam2)
	{
		long ret = XSDK_ERROR;
			try
			{
				if (m_pfnGetProp != IntPtr.Zero)
				{
					IntPtr par1;
					IntPtr par2;
					XSDK_Function_Delegate_Special5 xsdkApi = (XSDK_Function_Delegate_Special5)Marshal.GetDelegateForFunctionPointer(m_pfnGetProp, typeof(XSDK_Function_Delegate_Special5));
					XSDK_Function_Delegate_Special17 xsdkApi2 = (XSDK_Function_Delegate_Special17)Marshal.GetDelegateForFunctionPointer(m_pfnGetProp, typeof(XSDK_Function_Delegate_Special17));
					XSDK_GetProp_Delegate_Param2 xsdkApi3 = (XSDK_GetProp_Delegate_Param2)Marshal.GetDelegateForFunctionPointer(m_pfnGetProp, typeof(XSDK_GetProp_Delegate_Param2));
					XSDK_Function_Delegate_Special23 xsdkApi4= (XSDK_Function_Delegate_Special23)Marshal.GetDelegateForFunctionPointer(m_pfnGetProp, typeof(XSDK_Function_Delegate_Special23));
					XSDK_Function_Delegate_Special24 xsdkApi5 = (XSDK_Function_Delegate_Special24)Marshal.GetDelegateForFunctionPointer(m_pfnGetProp, typeof(XSDK_Function_Delegate_Special24));
					System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
					int[] param = new int[9];
					switch (lApiCode)
					{
						case 0x355A:
						case 0x355B:
						case 0x355C:
							try { }
							finally
							{
								par1 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)));
								par2 = IntPtr.Zero;

								int num = (int)pParam1;
								if (num != 0)
								{
									Marshal.WriteInt32(par1, num);
									Marshal.Copy(par1, param, 0, 1);
									par2 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(byte)) * param[0]);
								}
								try
								{
									ret = xsdkApi(phCamera, lApiCode, lParam, par1, par2);
									pParam1 = par1.ToInt32();
									if (num != 0)
									{
										int max = par1.ToInt32();
										if (num < max)
										{
											max = num;
										}
										Marshal.Copy(par2, (int[])pParam2, 0, max);
									}
								}
								finally
								{
									Marshal.FreeCoTaskMem(par1);
									if (num != 0)
									{
										Marshal.FreeCoTaskMem(par2);
									}
								}
							}
							break;
						case 0x2166:
							try { }
							finally
							{
								//par1 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)));
								int ipar = (int)pParam1;
								par2 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SDK_FaceFrameInfo)) * 16);
								SDK_FaceFrameInfo[] faceFrameInfo  = new SDK_FaceFrameInfo[ipar];
								try
								{
									ret = xsdkApi5(phCamera, lApiCode, lParam, ref ipar, ref faceFrameInfo);
									//pParam1 = par1.ToInt32();
									pParam2 = faceFrameInfo;
								}
								finally
								{
									//Marshal.FreeCoTaskMem(par1);
									Marshal.FreeCoTaskMem(par2);
								}
							}
							break;
						case 0x226C:
							try { }
							finally
							{
								par1 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)));
								par2 = IntPtr.Zero;

								int num = (int)pParam1;
								if (num != 0)
								{
									param[0] = num;
									par2 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SDK_FOCUS_LIMITER)) * param[0]);
								}
								try
								{
									ret = xsdkApi2(phCamera, lApiCode, lParam, ref num, par2);
									pParam1 = par1.ToInt32();
									if (num != 0)
									{
										int max = par1.ToInt32();
										if (num < max)
										{
											max = num;
										}
										Marshal.Copy(par2, (int[])pParam2, 0, max);
									}
								}
								finally
								{
									Marshal.FreeCoTaskMem(par1);
									if (num != 0)
									{
										Marshal.FreeCoTaskMem(par2);
									}
								}
								
							}
							break;
						case 0x3555:
							try { }
							finally
							{
								par2 = IntPtr.Zero;

								int num = (int)pParam1;
								int pa1 = (int)pParam1;
								SDK_HistogramData[] sD;
								if (num != 0)
								{
									param[0] = num;
									sD = new SDK_HistogramData[num];
									par2 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SDK_HistogramData)) * param[0]);
								}
								try
								{
									ret = xsdkApi2(phCamera, lApiCode, lParam, ref pa1, par2);
									pParam1 = pa1;
									if (num != 0)
									{
										int max = num;
										if (num < max)
										{
											max = num;
										}
										
										pParam2 = (SDK_HistogramData[])Marshal.PtrToStructure(par2, typeof(SDK_HistogramData[]));
									}
								}
								finally
								{
									if (num != 0)
									{
										Marshal.FreeCoTaskMem(par2);
									}
								}

							}
							break;
						case 0x3556:
							try { }
							finally
							{
								par1 = IntPtr.Zero;
								par2 = IntPtr.Zero;

								int num = (int)pParam1;
								if (num != 0)
								{
									Marshal.WriteInt32(par1, num);
									Marshal.Copy(par1, param, 0, 1);
									par2 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SDK_FocusMapData)) * param[0]);
								}
								try
								{
									ret = xsdkApi(phCamera, lApiCode, lParam, par1, par2);
									pParam1 = par1.ToInt32();
									if (num != 0)
									{
										int max = par1.ToInt32();
										if (num < max)
										{
											max = num;
										}
										Marshal.Copy(par2, (int[])pParam2, 0, max);
									}
								}
								finally
								{
									Marshal.FreeCoTaskMem(par1);
									if (num != 0)
									{
										Marshal.FreeCoTaskMem(par2);
									}
								}

							}
							break;
						case 0x4046:
								System.Text.StringBuilder sb1 = new System.Text.StringBuilder(256);
								System.Text.StringBuilder sb2= new System.Text.StringBuilder(256);

								ret = xsdkApi4(phCamera, lApiCode, lParam, sb1, sb2);
								pParam1 = sb1.ToString();
								pParam2 = sb2.ToString();

							break;
						default:
							int ipar1 = 0;
							int ipar2 = 0;
							ret = xsdkApi3(phCamera, lApiCode, lParam, ref ipar1, ref ipar2);
							pParam1 = ipar1;
							pParam2 = ipar2;
								
							break;
					}
				}
			}
			catch (Exception)
			{
				ret = XSDK_ERROR;
			}
		return ret;
	}

	/// <summary>
	/// Gets the settings for the model-dependent function.
	/// </summary>
	/// <returns>API result.</returns>
	public long SDK_GetProp_PPP(IntPtr phCamera, int lApiCode, int lParam, ref int pParam1, ref int pParam2, ref int pParam3)
	{
		long ret = XSDK_ERROR;
		try
		{
			switch (lApiCode)
			{
				default:
					if (m_pfnGetProp != IntPtr.Zero)
					{
						XSDK_GetProp_Delegate_Param3 xsdkApi = (XSDK_GetProp_Delegate_Param3)Marshal.GetDelegateForFunctionPointer(m_pfnGetProp, typeof(XSDK_GetProp_Delegate_Param3));
						ret = xsdkApi(phCamera, lApiCode, lParam, ref pParam1, ref pParam2, ref pParam3);
					}
					break;
			}
		}
		catch (Exception)
		{
			ret = XSDK_ERROR;
		}
		return ret;
	}

	/// <summary>
	/// Gets the settings for the model-dependent function.
	/// </summary>
	/// <returns>API result.</returns>
	public long SDK_GetProp_PPPP(IntPtr phCamera, int lApiCode, int lParam, ref int pParam1, ref int pParam2, ref int pParam3, ref int pParam4)
	{
		long ret = XSDK_ERROR;
		try
		{
			switch (lApiCode)
			{
				default:
					if (m_pfnGetProp != IntPtr.Zero)
					{
						XSDK_GetProp_Delegate_Param4 xsdkApi = (XSDK_GetProp_Delegate_Param4)Marshal.GetDelegateForFunctionPointer(m_pfnGetProp, typeof(XSDK_GetProp_Delegate_Param4));
						ret = xsdkApi(phCamera, lApiCode, lParam, ref pParam1, ref pParam2, ref pParam3, ref pParam4);
					}
					break;
			}
		}
		catch (Exception)
		{
			ret = XSDK_ERROR;
		}
		return ret;
	}

	/// <summary>
	/// Gets the settings for the model-dependent function.
	/// </summary>
	/// <returns>API result.</returns>
	public long SDK_GetProp_PPPPPP(IntPtr phCamera, int lApiCode, int lParam, ref int pParam1, ref int pParam2, ref int pParam3, ref int pParam4, ref int pParam5, ref int pParam6)
	{
		long ret = XSDK_ERROR;
		try
		{
			switch (lApiCode)
			{
				default:
					if (m_pfnGetProp != IntPtr.Zero)
					{
						XSDK_GetProp_Delegate_Param6 xsdkApi = (XSDK_GetProp_Delegate_Param6)Marshal.GetDelegateForFunctionPointer(m_pfnGetProp, typeof(XSDK_GetProp_Delegate_Param6));
						ret = xsdkApi(phCamera, lApiCode, lParam, ref pParam1, ref pParam2, ref pParam3, ref pParam4, ref pParam5, ref pParam6);
					}
					break;
			}
		}
		catch (Exception)
		{
			ret = XSDK_ERROR;
		}
		return ret;
	}

	/// <summary>
	/// Gets the settings for the model-dependent function.
	/// </summary>
	/// <returns>API result.</returns>
	public long SDK_GetProp_PPPPPPPP(IntPtr phCamera, int lApiCode, int lParam, ref int pParam1, ref int pParam2, ref int pParam3, ref int pParam4, ref int pParam5, ref int pParam6, ref int pParam7, ref int pParam8)
	{
		long ret = XSDK_ERROR;
		try
		{
			switch (lApiCode)
			{
				default:
					if (m_pfnGetProp != IntPtr.Zero)
					{
						XSDK_GetProp_Delegate_Param8 xsdkApi = (XSDK_GetProp_Delegate_Param8)Marshal.GetDelegateForFunctionPointer(m_pfnGetProp, typeof(XSDK_GetProp_Delegate_Param8));
						ret = xsdkApi(phCamera, lApiCode, lParam, ref pParam1, ref pParam2, ref pParam3, ref pParam4, ref pParam5, ref pParam6, ref pParam7, ref pParam8);
					}
					break;
			}
		}
		catch (Exception)
		{
			ret = XSDK_ERROR;
		}
		return ret;
	}

	/// <summary>
	/// Gets the settings for the model-dependent function.
	/// </summary>
	/// <returns>API result.</returns>
	public long SDK_GetProp_LP(IntPtr phCamera, int lApiCode, int lParam, int pParam1, ref object pParam2)
	{
		long ret = XSDK_ERROR;
		try
		{
			if (m_pfnGetProp != IntPtr.Zero)
			{
				IntPtr par2;
				XSDK_Function_Delegate_Special2 xsdkApi = (XSDK_Function_Delegate_Special2)Marshal.GetDelegateForFunctionPointer(m_pfnGetProp, typeof(XSDK_Function_Delegate_Special2));
				XSDK_Function_Delegate_Special19 xsdkApi2 = (XSDK_Function_Delegate_Special19)Marshal.GetDelegateForFunctionPointer(m_pfnGetProp, typeof(XSDK_Function_Delegate_Special19));
				XSDK_Function_Delegate_Special20 xsdkApi3 = (XSDK_Function_Delegate_Special20)Marshal.GetDelegateForFunctionPointer(m_pfnGetProp, typeof(XSDK_Function_Delegate_Special20));
				System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
				switch (lApiCode)
				{
					case 0x2206:
					case 0x2215:
						try { }
						finally
						{
							par2 = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(SDK_FocusArea)));
							try 
							{ 	
								Marshal.StructureToPtr(pParam2, par2, false);
								ret = xsdkApi(phCamera, lApiCode, lParam, pParam1, par2);
								pParam2 = (SDK_FocusArea)Marshal.PtrToStructure(par2, typeof(SDK_FocusArea));
							}
							finally
							{
								Marshal.FreeCoTaskMem(par2);
							}
						}
						break;
					case 0x2289:
						par2 = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(SDK_AFZoneCustomCapablity)));
						try
						{
							Marshal.StructureToPtr(pParam2, par2, false);
							ret = xsdkApi(phCamera, lApiCode, lParam, pParam1, par2);
							pParam2 = (SDK_AFZoneCustomCapablity)Marshal.PtrToStructure(par2, typeof(SDK_AFZoneCustomCapablity));
						}
						finally
						{
							Marshal.FreeCoTaskMem(par2);
						}
						break;
					case 0x2354:
					case 0x2358:
						try { }
						finally
						{
							par2 = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(SDK_CustomWBArea)));
							try
							{
								Marshal.StructureToPtr(pParam2, par2, false);
								ret = xsdkApi(phCamera, lApiCode, lParam, pParam1, par2);
								pParam2 = (SDK_CustomWBArea)Marshal.PtrToStructure(par2, typeof(SDK_CustomWBArea));
							}
							finally
							{
								Marshal.FreeCoTaskMem(par2);
							}
						}
						break;
					case 0x4048:
							System.Text.StringBuilder sb = new System.Text.StringBuilder(256);
							ret = xsdkApi2(phCamera, lApiCode, lParam, pParam1, sb);
							if(ret == XSDK_COMPLETE)
							{
								pParam2 = sb.ToString();
							}
						break;
					case 0x4212:
						try { }
						finally
						{
							par2 = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(SDK_FrameGuideGridInfo)));
							try
							{
								Marshal.StructureToPtr(pParam2, par2, false);
								ret = xsdkApi(phCamera, lApiCode, lParam, pParam1, par2);
								pParam2 = (SDK_FrameGuideGridInfo)Marshal.PtrToStructure(par2, typeof(SDK_FrameGuideGridInfo));
							}
							finally
							{
								Marshal.FreeCoTaskMem(par2);
							}
						}
						break;
					case 0x427A:
						try { }
						finally
						{
							par2 = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(SDK_PlayBackObjectInformation)));
							try
							{
								Marshal.StructureToPtr(pParam2, par2, false);
								ret = xsdkApi(phCamera, lApiCode, lParam, pParam1, par2);
								pParam2 = (SDK_PlayBackObjectInformation)Marshal.PtrToStructure(par2, typeof(SDK_PlayBackObjectInformation));
							}
							finally
							{
								Marshal.FreeCoTaskMem(par2);
							}
						}
						break;
					default:
						int ipar2 = 0;	
						ret = xsdkApi3(phCamera, lApiCode, lParam, pParam1, ref ipar2);
						pParam2 = ipar2;
						break;
				}
			}
		}
		catch (Exception)
		{
			ret = XSDK_ERROR;
		}
		return ret;
	}

	/// <summary>
	/// Gets the settings for the model-dependent function.
	/// </summary>
	/// <returns>API result.</returns>
	public long SDK_GetProp_LLP(IntPtr phCamera, int lApiCode, int lParam, int pParam1, int pParam2,  ref object pParam3)
	{
		long ret = XSDK_ERROR;
		try
		{
			if (m_pfnGetProp != IntPtr.Zero)
			{
				switch (lApiCode)
				{
					case 0x427B:
						byte[] data = new byte[pParam2];
						try { }
						finally
						{
							XSDK_Function_Delegate_Special6 xsdkApi = (XSDK_Function_Delegate_Special6)Marshal.GetDelegateForFunctionPointer(m_pfnGetProp, typeof(XSDK_Function_Delegate_Special6));
							System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
							IntPtr pData = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(byte)) * data.Length);
							try
							{
								ret = xsdkApi(phCamera, lApiCode, lParam, pParam1, pParam2, pData);
								if(ret == XSDK_COMPLETE)
								{
									Marshal.Copy(pData, data, 0, data.Length);
									pParam3 = data;
								}
							}
							finally
							{
								Marshal.FreeCoTaskMem(pData);
							}
						}
						break;
					default:
						ret = XSDK_ERROR;
						break;
				}
			}
		}
		catch (Exception)
		{
			ret = XSDK_ERROR;
		}
		return ret;
	}

	/// <summary>
	/// Gets the settings for the model-dependent function.
	/// </summary>
	/// <returns>API result.</returns>
	public long SDK_GetProp_LLLPP(IntPtr phCamera, int lApiCode, int lParam, int pParam1, int pParam2, int pParam3, ref int pParam4,  object pParam5)
	{
		long ret = XSDK_ERROR;
		try
		{
			if (m_pfnGetProp != IntPtr.Zero)
			{
				switch (lApiCode)
				{
					case 0x427C:
						try { }
						finally
						{
							XSDK_Function_Delegate_Special7 xsdkApi = (XSDK_Function_Delegate_Special7)Marshal.GetDelegateForFunctionPointer(m_pfnGetProp, typeof(XSDK_Function_Delegate_Special7));
							System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
							IntPtr pSize = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)));
							pSize = new IntPtr(pParam3);
							byte[] partialData = new byte[pParam4];
							IntPtr pPartialData = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(byte)) * partialData.Length);
							try
							{
								ret = xsdkApi(phCamera, lApiCode, lParam, pParam1, pParam2, pParam3, pSize, pPartialData);
								if (ret == XSDK_COMPLETE)
								{
									pParam4 =pSize.ToInt32();
									Marshal.Copy(pPartialData, partialData, 0, partialData.Length);
									pParam5 = partialData;
								}
							}
							finally
							{
								Marshal.FreeCoTaskMem(pSize);
								Marshal.FreeCoTaskMem(pPartialData);
							}
						}
						break;
					default:
						ret = XSDK_ERROR;
						break;
				}
			}
		}
		catch (Exception)
		{
			ret = XSDK_ERROR;
		}
		return ret;
	}

	/// <summary>
	/// Gets the settings for the model-dependent function.
	/// </summary>
	/// <returns>API result.</returns>
	public long SDK_GetProp_LPP(IntPtr phCamera, int lApiCode, int lParam, int pParam1, ref object pParam2, ref object pParam3)
	{
		long ret = XSDK_ERROR;
		try
		{
			IntPtr par1 = IntPtr.Zero;
			IntPtr par2 = IntPtr.Zero;
			if (m_pfnGetProp != IntPtr.Zero)
			{
				switch (lApiCode)
				{
					case 0x2305:
					case 0x3603:
						try { }
						finally
						{
							XSDK_Function_Delegate_Special1 xsdkApi = (XSDK_Function_Delegate_Special1)Marshal.GetDelegateForFunctionPointer(m_pfnGetProp, typeof(XSDK_Function_Delegate_Special1));
							System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
							par1 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)));
							par2 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)));
							try
							{
								ret = xsdkApi(phCamera, lApiCode, lParam, pParam1, par1, par2);
								if (ret == XSDK_COMPLETE)
								{
									pParam2 = par1.ToInt32();
									pParam3 = par2.ToInt32();
								}
							}
							finally
							{
								Marshal.FreeCoTaskMem(par1);
								Marshal.FreeCoTaskMem(par2);
							}
						}
						break;
					case 0x4075:
						try { }
						finally
						{
							XSDK_Function_Delegate_Special22 xsdkApi = (XSDK_Function_Delegate_Special22)Marshal.GetDelegateForFunctionPointer(m_pfnGetProp, typeof(XSDK_Function_Delegate_Special22));
							System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
							byte[] foldername = new byte[250];
							int array_size = Marshal.SizeOf(typeof(byte)) * foldername.Length;
							par1 = Marshal.AllocHGlobal(array_size);
							int ipara2 = 0;
							try
							{
								ret = xsdkApi(phCamera, lApiCode, lParam, pParam1, par1, ref ipara2);
								if (ret == XSDK_COMPLETE)
								{
									Marshal.Copy(par1, foldername, 0, foldername.Length);
									pParam3 = ipara2;
								}
							}
							finally
							{
								Marshal.FreeCoTaskMem(par1);
							}
						}
						break;
					case 0x4279:
						try { }
						finally
						{
							XSDK_Function_Delegate_Special1 xsdkApi = (XSDK_Function_Delegate_Special1)Marshal.GetDelegateForFunctionPointer(m_pfnGetProp, typeof(XSDK_Function_Delegate_Special1));
							System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
							int num = (int)pParam2;
							par1 = Marshal.AllocHGlobal(Marshal.SizeOf(num));
							if (num != 0)
							{
								par2 = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * (int)pParam2);
							}
							try
							{
								ret = xsdkApi(phCamera, lApiCode, lParam, pParam1, par1, par2);
								if (num != 0)
								{
									int max = (int)pParam2;
									if (num < max)
									{
										max = num;
									}
									Marshal.Copy(par2, (int[])pParam3, 0, max);
								}
							}
							finally
							{
								Marshal.FreeCoTaskMem(par1);

								if (num != 0)
								{
									Marshal.FreeCoTaskMem(par2);
								}
							}
						}
						break;
					default:
						ret = XSDK_ERROR;
						break;
				}
			}
		}
		catch (Exception)
		{
			ret = XSDK_ERROR;
		}
		return ret;
	}

	/// <summary>
	/// Gets the settings for the model-dependent function.
	/// </summary>
	/// <returns>API result.</returns>
	public long SDK_GetProp_LPPPP(IntPtr phCamera, int lApiCode, int lParam, int pParam1, ref object pParam2, ref object pParam3, ref object pParam4, ref object pParam5)
	{
		long ret = XSDK_ERROR;
		try
		{
			if (m_pfnGetProp != IntPtr.Zero)
			{
				IntPtr par1;
				IntPtr par2;
				IntPtr par3;
				IntPtr par4;
				switch (lApiCode)
				{
					case 0x4068:
						XSDK_Function_Delegate_Special8 xsdkApi = (XSDK_Function_Delegate_Special8)Marshal.GetDelegateForFunctionPointer(m_pfnGetProp, typeof(XSDK_Function_Delegate_Special8));
						System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
						try{}
						finally
						{
							par1 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)));
							par2 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)));
							par3 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)));
							par4 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)));
							try
							{
								ret = xsdkApi(phCamera, lApiCode, lParam, pParam1, par1, par2, par3, par4);
								if (ret != XSDK_COMPLETE)
								{
									pParam2 = par1.ToInt32();
									pParam3 = par2.ToInt32();
									pParam4 = par3.ToInt32();
									pParam5 = par4.ToInt32();
								}
							}
							finally
							{
								Marshal.FreeCoTaskMem(par1);
								Marshal.FreeCoTaskMem(par2);
								Marshal.FreeCoTaskMem(par3);
								Marshal.FreeCoTaskMem(par4);
							}
							
						}
						break;
					default:
						ret = XSDK_ERROR;
						break;
				}
			}
		}
		catch (Exception)
		{
			ret = XSDK_ERROR;
		}
		return ret;
	}

	/// <summary>
	/// Gets the settings for the model-dependent function.
	/// </summary>
	/// <returns>API result.</returns>
	public long SDK_GetProp_PPLP(IntPtr phCamera, int lApiCode, int lParam, ref object pParam1, ref object pParam2, int pParam3, ref object pParam4)
	{
		long ret = XSDK_ERROR;
		try
		{
			if (m_pfnGetProp != IntPtr.Zero)
			{
				IntPtr par1;
				IntPtr par2;
				IntPtr par3;
				IntPtr par4 = IntPtr.Zero;
				switch (lApiCode)
				{
					case 0x4076:
						XSDK_Function_Delegate_Special9 xsdkApi = (XSDK_Function_Delegate_Special9)Marshal.GetDelegateForFunctionPointer(m_pfnGetProp, typeof(XSDK_Function_Delegate_Special9));
						System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions(); // For memory leak prevention of "AllocCoTaskMem".
						try { }
						finally
						{
							par1 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)));
							par2 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)));
							par3 = new IntPtr(pParam3);
							int num = (int)par1;
							if (num != 0)
							{
								par2 = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(SDK_FOLDER_INFO)));
							}
							try
							{
								ret = xsdkApi(phCamera, lApiCode, lParam, par1, par2, par3, par4);
								if (ret != XSDK_COMPLETE)
								{
									pParam1 = par1;
									pParam2 = par2;
									if((int)pParam1 != 0)
									{
										pParam4 = (SDK_FOLDER_INFO)Marshal.PtrToStructure(par2, typeof(SDK_FOLDER_INFO));
									}
								}
							}
							finally
							{
								Marshal.FreeCoTaskMem(par1);
								Marshal.FreeCoTaskMem(par2);
								Marshal.FreeCoTaskMem(par3);
								if (num != 0)
								{
										Marshal.FreeCoTaskMem(par4);
								}		
							}

						}
						break;
					default:
						ret = XSDK_ERROR;
						break;
				}
			}
		}
		catch (Exception)
		{
			ret = XSDK_ERROR;
		}
		return ret;
	}
	#endregion Delegate for C# of model dependant API.

	public int GetError()
	{
		return m_iErr;
	}

	public void InitializeLibrary()
	{
			m_pfnInit = IntPtr.Zero;
			m_pfnExit = IntPtr.Zero;
			m_pfnDetect = IntPtr.Zero;
			m_pfnAppend = IntPtr.Zero;
			m_pfnOpenEx = IntPtr.Zero;
			m_pfnClose = IntPtr.Zero;
			m_pfnPowerOFF = IntPtr.Zero;
			m_pfnGetErrorNumber = IntPtr.Zero;
			m_pfnGetVersionString = IntPtr.Zero;
			m_pfnGetErrorDetails = IntPtr.Zero;
			m_pfnGetDeviceInfo = IntPtr.Zero;
			m_pfnWriteDeviceName = IntPtr.Zero;
			m_pfnGetFirmwareVersion = IntPtr.Zero;
			m_pfnGetLensInfo = IntPtr.Zero;
			m_pfnGetLensVersion = IntPtr.Zero;
			m_pfnGetDeviceInfoEx = IntPtr.Zero;
			m_pfnCapPriorityMode = IntPtr.Zero;
			m_pfnSetPriorityMode = IntPtr.Zero;
			m_pfnGetPriorityMode = IntPtr.Zero;
			m_pfnCapRelease = IntPtr.Zero;
			m_pfnRelease = IntPtr.Zero;
			m_pfnGetReleaseStatus = IntPtr.Zero;
			m_pfnCapReleaseEx = IntPtr.Zero;
			m_pfnReleaseEx = IntPtr.Zero;
			m_pfnReadImageInfo = IntPtr.Zero;
			m_pfnReadPreview = IntPtr.Zero;
			m_pfnReadImage = IntPtr.Zero;
			m_pfnDeleteImage = IntPtr.Zero;
			m_pfnGetBufferCapacity = IntPtr.Zero;
			m_pfnCapAEMode = IntPtr.Zero;
			m_pfnSetAEMode = IntPtr.Zero;
			m_pfnGetAEMode = IntPtr.Zero;
			m_pfnCapShutterSpeed = IntPtr.Zero;
			m_pfnSetShutterSpeed = IntPtr.Zero;
			m_pfnGetShutterSpeed = IntPtr.Zero;
			m_pfnCapExposureBias = IntPtr.Zero;
			m_pfnSetExposureBias = IntPtr.Zero;
			m_pfnGetExposureBias = IntPtr.Zero;
			m_pfnCapDynamicRange = IntPtr.Zero;
			m_pfnSetDynamicRange = IntPtr.Zero;
			m_pfnGetDynamicRange = IntPtr.Zero;
			m_pfnCapSensitivity = IntPtr.Zero;
			m_pfnSetSensitivity = IntPtr.Zero;
			m_pfnGetSensitivity = IntPtr.Zero;
			m_pfnCapMeteringMode = IntPtr.Zero;
			m_pfnSetMeteringMode = IntPtr.Zero;
			m_pfnGetMeteringMode = IntPtr.Zero;
			m_pfnCapLensZoomPos = IntPtr.Zero;
			m_pfnGetLensZoomPos = IntPtr.Zero;
			m_pfnSetLensZoomPos = IntPtr.Zero;
			m_pfnCapAperture = IntPtr.Zero;
			m_pfnSetAperture = IntPtr.Zero;
			m_pfnGetAperture = IntPtr.Zero;
			m_pfnCapWBMode = IntPtr.Zero;
			m_pfnSetWBMode = IntPtr.Zero;
			m_pfnGetWBMode = IntPtr.Zero;
			m_pfnCapWBColorTemp = IntPtr.Zero;
			m_pfnSetWBColorTemp = IntPtr.Zero;
			m_pfnGetWBColorTemp = IntPtr.Zero;
			m_pfnCapMediaRecord = IntPtr.Zero;
			m_pfnSetMediaRecord = IntPtr.Zero;
			m_pfnGetMediaRecord = IntPtr.Zero;
			m_pfnCapForceMode = IntPtr.Zero;
			m_pfnSetForceMode = IntPtr.Zero;
			m_pfnSetBackupSettings = IntPtr.Zero;
			m_pfnGetBackupSettings = IntPtr.Zero;
			m_pfnCapDriveMode = IntPtr.Zero;
			m_pfnSetDriveMode = IntPtr.Zero;
			m_pfnGetDriveMode = IntPtr.Zero;
			m_pfnCapMode = IntPtr.Zero;
			m_pfnSetMode = IntPtr.Zero;
			m_pfnGetMode = IntPtr.Zero;
			m_pfnGetRecordingStatus = IntPtr.Zero;
			m_pfnCapMovieShutterSpeed = IntPtr.Zero;
			m_pfnSetMovieShutterSpeed = IntPtr.Zero;
			m_pfnGetMovieShutterSpeed = IntPtr.Zero;
			m_pfnCapMovieExposureBias = IntPtr.Zero;
			m_pfnSetMovieExposureBias = IntPtr.Zero;
			m_pfnGetMovieExposureBias = IntPtr.Zero;
			m_pfnCapMovieSensitivity = IntPtr.Zero;
			m_pfnSetMovieSensitivity = IntPtr.Zero;
			m_pfnGetMovieSensitivity = IntPtr.Zero;
			m_pfnCapMovieAperture = IntPtr.Zero;
			m_pfnSetMovieAperture = IntPtr.Zero;
			m_pfnGetMovieAperture = IntPtr.Zero;
			m_pfnCapMovieDynamicRange = IntPtr.Zero;
			m_pfnSetMovieDynamicRange = IntPtr.Zero;
			m_pfnGetMovieDynamicRange = IntPtr.Zero;
			m_pfnCapMovieMeteringMode = IntPtr.Zero;
			m_pfnSetMovieMeteringMode = IntPtr.Zero;
			m_pfnGetMovieMeteringMode = IntPtr.Zero;
			m_pfnCapMovieWBMode = IntPtr.Zero;
			m_pfnSetMovieWBMode = IntPtr.Zero;
			m_pfnGetMovieWBMode = IntPtr.Zero;
			m_pfnCapMovieWBColorTemp = IntPtr.Zero;
			m_pfnSetMovieWBColorTemp = IntPtr.Zero;
			m_pfnGetMovieWBColorTemp = IntPtr.Zero;
			m_pfnCapProp = IntPtr.Zero;
			m_pfnSetProp = IntPtr.Zero;
			m_pfnGetProp = IntPtr.Zero;

			return;
	}
	}
}
