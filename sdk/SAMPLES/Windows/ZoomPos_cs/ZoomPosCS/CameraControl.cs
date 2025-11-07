using System;
using System.Collections.Generic;
using System.Threading;
using FujiSDK;

namespace CCameraControl
{
	public enum XSDK_ERRCODE : int
	{
		XSDK_ERRCODE_NOERR = 0x00000000,
		XSDK_ERRCODE_SEQUENCE = 0x00001001,
		XSDK_ERRCODE_PARAM = 0x00001002,
		XSDK_ERRCODE_INVALID_CAMERA = 0x00001003,
		XSDK_ERRCODE_LOADLIB = 0x00001004,
		XSDK_ERRCODE_UNSUPPORTED = 0x00001005,
		XSDK_ERRCODE_BUSY = 0x00001006,
		XSDK_ERRCODE_AF_TIMEOUT = 0x00001007,
		XSDK_ERRCODE_SHOOT_ERROR = 0x00001008,
		XSDK_ERRCODE_FRAME_FULL = 0x00001009,
		XSDK_ERRCODE_STANDBY = 0x00001010,
		XSDK_ERRCODE_NODRIVER = 0x00001011,
		XSDK_ERRCODE_NO_MODEL_MODULE = 0x00001012,
		XSDK_ERRCODE_API_NOTFOUND = 0x00001013,
		XSDK_ERRCODE_API_MISMATCH = 0x00001014,
		XSDK_ERRCODE_INVALID_USBMODE = 0x00001015,
		XSDK_ERRCODE_NOT_AVAILABLE_STATUS = 0x00001016,
		XSDK_ERRCODE_RUNNING_OTHER_FUNCTION = 0x00001017,
		XSDK_ERRCODE_COMMUNICATION = 0x00002001,
		XSDK_ERRCODE_TIMEOUT = 0x00002002,
		XSDK_ERRCODE_COMBINATION = 0x00002003,
		XSDK_ERRCODE_WRITEERROR = 0x00002004,
		XSDK_ERRCODE_CARDFULL = 0x00002005,
		XSDK_ERRCODE_HARDWARE = 0x00003001,
		XSDK_ERRCODE_INTERNAL = 0x00009001,
		XSDK_ERRCODE_MEMFULL = 0x00009002,
		XSDK_ERRCODE_UNKNOWN = 0x00009100
	};

	public enum API_CODE
	{
		API_CODE_Init = 0x1001,
		API_CODE_Exit = 0x1002,
		API_CODE_Detect = 0x1010,
		API_CODE_Append = 0x1012,
		API_CODE_Close = 0x1022,
		API_CODE_OpenEx = 0x1024,
		API_CODE_GetErrorNumber = 0x1031,
		API_CODE_GetDeviceInfo = 0x1041,
		API_CODE_GetSensitivity = 0x1313,
		API_CODE_GetDriveMode = 0x1378,
		API_CODE_CapLensZoomPos= 0x1321,
		API_CODE_GetLensZoomPos= 0x1323,
		API_CODE_GetMode = 0x137C
    };

	public enum Sensitivity
	{
        ISO40 = 40,
        ISO50 = 50,
		ISO60 = 60,
        ISO64 = 64,
        ISO80 = 80,
		ISO100 = 100,
		ISO125 = 125,
		ISO160 = 160,
		ISO200 = 200,
		ISO250 = 250,
		ISO320 = 320,
		ISO400 = 400,
		ISO500 = 500,
		ISO640 = 640,
		ISO800 = 800,
		ISO1000 = 1000,
		ISO1250 = 1250,
		ISO1600 = 1600,
		ISO2000 = 2000,
		ISO2500 = 2500,
		ISO3200 = 3200,
		ISO4000 = 4000,
		ISO5000 = 5000,
		ISO6400 = 6400,
		ISO8000 = 8000,
		ISO10000 = 10000,
		ISO12800 = 12800,
		ISO16000 = 16000,
		ISO20000 = 20000,
		ISO25600 = 25600,
		ISO32000 = 32000,
		ISO40000 = 40000,
		ISO51200 = 51200,
		ISO64000 = 64000,
		ISO80000 = 80000,
		ISO102400 = 102400,
		AUTO_1 = -1,
		AUTO_2 = -2,
		AUTO_3 = -3,
		AUTO_4 = -4,
		AUTO = -10,
		AUTO400 = -400,
		AUTO800 = -800,
		AUTO1600 = -1600,
		AUTO3200 = -3200,
		AUTO6400 = -6400
	}
	internal class CameraControl
	{
		private CFujiSDK m_FujiSDK;
		private long result;
		private int plERRCode;
		private int plAPICode;
		public XSDK_ERRCODE ERRCode;
		public API_CODE APICode;
		private Mutex cameraControlExclusion = new Mutex();
		public string sAPICode;
		public string sERRCode;
		public const int XSDK_DSC_IF_USB = 0x00000001;
		public const int XSDK_DSC_IF_WIFI_LOCAL = 0x00000010;
		public const int XSDK_DSC_IF_WIFI_IP = 0x00000020;

		public const int XSDK_DSC_MODE_TETHER = 0x0001;
		public const int XSDK_DSC_MODE_RAW = 0x0002;
		public const int XSDK_DSC_MODE_BR = 0x0004;
		public const int XSDK_DSC_MODE_WEBCAM = 0x0008;

		public const int XSDK_DRIVE_MODE_INVALID = 0xFFFF;
		public const int XSDK_DRIVE_MODE_CH = 0x0002;
		public const int XSDK_DRIVE_MODE_CL = 0x0003;
		public const int XSDK_DRIVE_MODE_S = 0x0004;
		public const int XSDK_DRIVE_MODE_MULTI_EXPOSURE = 0x0005;
		public const int XSDK_DRIVE_MODE_ADVFILTER = 0x0006;
		public const int XSDK_DRIVE_MODE_PANORAMA = 0x0007;
		public const int XSDK_DRIVE_MODE_MOVIE = 0x0008;
		public const int XSDK_DRIVE_MODE_HDR = 0x0009;
		public const int XSDK_DRIVE_MODE_BKT_AE = 0x000A;
		public const int XSDK_DRIVE_MODE_BKT_ISO = 0x000B;
		public const int XSDK_DRIVE_MODE_BKT_FILMSIMULATION = 0x000C;
		public const int XSDK_DRIVE_MODE_BKT_WHITEBALANCE = 0x000D;
		public const int XSDK_DRIVE_MODE_BKT_DYNAMICRANGE = 0x000E;
		public const int XSDK_DRIVE_MODE_FOCUS = 0x000F;
		public const int XSDK_DRIVE_MODE_PIXELSHIFTMULTISHOT = 0x0010;
		public const int XSDK_DRIVE_MODE_CH_CROP = 0x0011;
		public const int XSDK_DRIVE_MODE_PIXELSHIFTMULTISHOT_FEWERFRAMES = 0x0012;
		public const int XSDK_MODE_STILL_C0 = 0x0001;
	    public const int XSDK_MODE_STILL_C1 = 0x0002;
	    public const int XSDK_MODE_STILL_C2 = 0x0003;
	    public const int XSDK_MODE_STILL_C3 = 0x0004;
	    public const int XSDK_MODE_STILL_C4 = 0x0005;
	    public const int XSDK_MODE_STILL_C5 = 0x0006;
	    public const int XSDK_MODE_STILL_C6 = 0x0007;
		public const int XSDK_MODE_STILL_C7 = 0x0008;
		public const int XSDK_MODE_STILL_ADVFILTER = 0x0081;
		public const int XSDK_MODE_STILL_SP = 0x00B1;
		public const int XSDK_MODE_STILL_AUTO = 0x00F0;
		public const int XSDK_MODE_MOVIE_C0 = 0x0101;
	    public const int XSDK_MODE_MOVIE_C1 = 0x0102;
	    public const int XSDK_MODE_MOVIE_C2 = 0x0103;
	    public const int XSDK_MODE_MOVIE_C3 = 0x0104;
	    public const int XSDK_MODE_MOVIE_C4 = 0x0105;
	    public const int XSDK_MODE_MOVIE_C5 = 0x0106;
	    public const int XSDK_MODE_MOVIE_C6 = 0x0107;
		public const int XSDK_MODE_MOVIE_C7 = 0x0108;
		public const int XSDK_MODE_MOVIE_VLOG = 0x0161;

		public bool LoadLibrary()
		{
			cameraControlExclusion.WaitOne();
			try
			{
				m_FujiSDK = new CFujiSDK(null);

				m_FujiSDK.LoadSDKLibrary();

				if (m_FujiSDK.GetError() != (int)FUJI_SDK_ERR.FUJI_SDK_ERR_OK)
				{
					// err
					return false;
				}
			}
			finally
			{
				cameraControlExclusion.ReleaseMutex();
			}

			return true;
		}

		public long Init()
		{
			result = CFujiSDK.XSDK_ERROR;
			cameraControlExclusion.WaitOne();
			try
			{
				result = m_FujiSDK.XSDK_Init();
				if (result != CFujiSDK.XSDK_COMPLETE)
				{
					// err
					m_FujiSDK.XSDK_GetErrorNumber((IntPtr)null, ref plAPICode, ref plERRCode);
					ChangeValue(plAPICode, plERRCode);
				}
			}
			finally
			{
				cameraControlExclusion.ReleaseMutex();
			}

			return result;
		}

		public long Exit()
		{
			result = CFujiSDK.XSDK_ERROR;
			cameraControlExclusion.WaitOne();

			try
			{
				result = m_FujiSDK.XSDK_Exit();
				if (result != CFujiSDK.XSDK_COMPLETE)
				{
					// err
					m_FujiSDK.XSDK_GetErrorNumber((IntPtr)null, ref plAPICode, ref plERRCode);
					ChangeValue(plAPICode, plERRCode);
				}
			}
			finally
			{
				cameraControlExclusion.ReleaseMutex();
			}
			return result;
		}

		public long Detect(int iInterface, string sInterface, string sDeviceName, ref int iNumCameras)
		{
			result = CFujiSDK.XSDK_ERROR;
			cameraControlExclusion.WaitOne();

			try
			{
				result = m_FujiSDK.XSDK_Detect(iInterface, sInterface, sDeviceName, ref iNumCameras);
				if (result != CFujiSDK.XSDK_COMPLETE)
				{
					// err
					m_FujiSDK.XSDK_GetErrorNumber((IntPtr)null, ref plAPICode, ref plERRCode);
					ChangeValue(plAPICode, plERRCode);
				}
			}
			finally
			{
				cameraControlExclusion.ReleaseMutex();
			}
			return result;
		}

		public long Append(int iInterface, string sInterface, string sDeviceName, ref int iNumCameras, List<XSDK_CameraList> cameraList)
		{
			result = CFujiSDK.XSDK_ERROR;
			cameraControlExclusion.WaitOne();
			try
			{
				result = m_FujiSDK.XSDK_Append(iInterface, sInterface, sDeviceName, ref iNumCameras, cameraList);
				if (result != CFujiSDK.XSDK_COMPLETE)
				{
					// err
					m_FujiSDK.XSDK_GetErrorNumber((IntPtr)null, ref plAPICode, ref plERRCode);
					ChangeValue(plAPICode, plERRCode);
				}
			}
			finally
			{
				cameraControlExclusion.ReleaseMutex();
			}
			return result;
		}

		public long Open(string pDevice, ref IntPtr phCamera, ref int iCameraMode, IntPtr pOption)
		{
			result = CFujiSDK.XSDK_ERROR;
			cameraControlExclusion.WaitOne();
			try
			{
				result = m_FujiSDK.XSDK_OpenEx(pDevice, ref phCamera, ref iCameraMode, pOption);
				if (result != CFujiSDK.XSDK_COMPLETE)
				{
					// err
					m_FujiSDK.XSDK_GetErrorNumber(phCamera, ref plAPICode, ref plERRCode);
					ChangeValue(plAPICode, plERRCode);
				}
			}
			finally
			{
				cameraControlExclusion.ReleaseMutex();
			}
			return result;

		}

		public long Close(IntPtr phCamera)
		{
			result = CFujiSDK.XSDK_ERROR;
			cameraControlExclusion.WaitOne();
			try
			{
				result = m_FujiSDK.XSDK_Close(phCamera);
				if (result != CFujiSDK.XSDK_COMPLETE)
				{
					// err
					m_FujiSDK.XSDK_GetErrorNumber(phCamera, ref plAPICode, ref plERRCode);
					ChangeValue(plAPICode, plERRCode);

					return result;
				}
			}
			finally
			{
				cameraControlExclusion.ReleaseMutex();
			}
			return result;
		}

		public long GetDeviceInfo(IntPtr phCamera, ref XSDK_DeviceInformation pDevInfo)
		{
			result = CFujiSDK.XSDK_ERROR;
			cameraControlExclusion.WaitOne();
			try
			{
				result = m_FujiSDK.XSDK_GetDeviceInfo(phCamera, ref pDevInfo);
				if (result != CFujiSDK.XSDK_COMPLETE)
				{
					// err
					m_FujiSDK.XSDK_GetErrorNumber(phCamera, ref plAPICode, ref plERRCode);
					ChangeValue(plAPICode, plERRCode);
				}
			}
			finally
			{
				cameraControlExclusion.ReleaseMutex();
			}
			return result;
		}

		public long GetSensitivity(IntPtr phCamera, ref int iSensitivity)
		{
			result = CFujiSDK.XSDK_ERROR;
			cameraControlExclusion.WaitOne();
			try
			{
				result = m_FujiSDK.XSDK_GetSensitivity(phCamera, ref iSensitivity);

				if (result != CFujiSDK.XSDK_COMPLETE)
				{
					// err
					m_FujiSDK.XSDK_GetErrorNumber(phCamera, ref plAPICode, ref plERRCode);
					ChangeValue(plAPICode, plERRCode);
				}
			}
			finally 
			{
				cameraControlExclusion.ReleaseMutex(); 
			}
			return result;
		}

		public long GetDriveMode(IntPtr phCamera, ref int iDriveMode)
		{
			result = CFujiSDK.XSDK_ERROR;
			cameraControlExclusion.WaitOne();
			try
			{
				result = m_FujiSDK.XSDK_GetDriveMode(phCamera, ref iDriveMode);

				if (result != CFujiSDK.XSDK_COMPLETE)
				{
					// err
					m_FujiSDK.XSDK_GetErrorNumber(phCamera, ref plAPICode, ref plERRCode);
					ChangeValue(plAPICode, plERRCode);
					return result;
				}

				if(iDriveMode == XSDK_DRIVE_MODE_MOVIE)
				{
					return result;
				}

				int imode = 0;
				result = m_FujiSDK.XSDK_GetMode(phCamera, ref imode);

				if (result != CFujiSDK.XSDK_COMPLETE)
				{
					// err
					m_FujiSDK.XSDK_GetErrorNumber(phCamera, ref plAPICode, ref plERRCode);
					ChangeValue(plAPICode, plERRCode);

					return result;
				}

				if((0x0100 & imode) == 0x0100)
				{
					iDriveMode = XSDK_DRIVE_MODE_MOVIE;
				}

			}
			finally
			{
				cameraControlExclusion.ReleaseMutex();
			}

			return result;
		}

		public long CapLensZoomPos(IntPtr phCamera, ref int iNumZoomPos, ref int[] iLensZoomPos, ref int[] iFocusLength, ref int[] i35mmFocusLength)
		{
			result = CFujiSDK.XSDK_ERROR;
			cameraControlExclusion.WaitOne();
			try
			{
				result = m_FujiSDK.XSDK_CapLensZoomPos(phCamera, ref iNumZoomPos, ref iLensZoomPos, ref iFocusLength, ref i35mmFocusLength);

				if (result != CFujiSDK.XSDK_COMPLETE)
				{
					// err
					m_FujiSDK.XSDK_GetErrorNumber(phCamera, ref plAPICode, ref plERRCode);
					ChangeValue(plAPICode, plERRCode);

					return result;
				}

				iLensZoomPos = new int[iNumZoomPos];
				iFocusLength = new int[iNumZoomPos];
				i35mmFocusLength = new int[iNumZoomPos];

				result = m_FujiSDK.XSDK_CapLensZoomPos(phCamera, ref iNumZoomPos, ref iLensZoomPos, ref iFocusLength, ref i35mmFocusLength);

				if (result != CFujiSDK.XSDK_COMPLETE)
				{
					// err
					m_FujiSDK.XSDK_GetErrorNumber(phCamera, ref plAPICode, ref plERRCode);
					ChangeValue(plAPICode,plERRCode);
				}
			}
			finally
			{
				cameraControlExclusion.ReleaseMutex();
			}

			return result;
		}

		public long GetLensZoomPos(IntPtr phCamera, ref int iZoomPos)
		{
			result = CFujiSDK.XSDK_ERROR;
			cameraControlExclusion.WaitOne();
			try
			{
				result = m_FujiSDK.XSDK_GetLensZoomPos(phCamera, ref iZoomPos);

				if (result != CFujiSDK.XSDK_COMPLETE)
				{
					// err
					m_FujiSDK.XSDK_GetErrorNumber(phCamera, ref plAPICode, ref plERRCode);
					ChangeValue(plAPICode,plERRCode);
				}
			}
			finally
			{
				cameraControlExclusion.ReleaseMutex();
			}

			return result;
		}

		public void ChangeValue(int plAPICode, int plERRCode)
		{
			if(Enum.IsDefined(typeof(API_CODE), plAPICode))
			{
				APICode = (API_CODE)plAPICode;
				sAPICode = APICode.ToString();
			}
			else
			{
				sAPICode= "0x" + plAPICode.ToString("X4");
			}
			if (Enum.IsDefined(typeof(XSDK_ERRCODE), plERRCode))
			{
				ERRCode = (XSDK_ERRCODE)plERRCode;
				sERRCode = ERRCode.ToString();
			}
			else
			{
				sERRCode = "0x" + plERRCode.ToString("X8");
			}

			return;
		}

	}
}
