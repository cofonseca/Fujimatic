using System;
using System.Threading;
using FujiSDK;
using CCameraControl;

namespace ZoomPos
{
	internal class ZoomPosControl
	{
		private static Mutex mutex = new Mutex();
		private CAMERA_CONTROL_THREAD_PARAM m_myParam;
		private Thread m_pThreadConnect;
		private Thread m_pThreadPoll;
		private CameraControl cameraControl;
		public const int FALSE = 0;
		public const int TRUE = 1;
		public const int NULL = 0;

		public ZoomPosControl()
		{
			m_myParam.hCam = IntPtr.Zero;

			cameraControl = new CameraControl();

			m_pThreadConnect = null;
			m_pThreadPoll = null;

			m_myParam.phEventExitConnect = false;
			m_myParam.phEventExitPoll = false;

			m_myParam.boCapabilityBackupRestore = false;
			m_myParam.boCapabilityTether = false;
		}

		~ZoomPosControl()
		{
			// Disconnect process after all subthreads are finished.
			TerminateConnection();
		}

		public bool LoadLibrary()
		{
			// Start SDK communication.
			bool bLoad = cameraControl.LoadLibrary();

			return bLoad;
		}

		public uint ThreadConnect()
		{
			int boOpen = 0;

			while (m_myParam.phEventExitConnect == false)
			{
				if (CFujiSDK.XSDK_COMPLETE == cameraControl.Init()) // Initialize SDK.
				{
					int lNumCameras = 0;
					string phcInterface = "";
					string phcDeviceNmae = "";

					// camera search.
					if (CFujiSDK.XSDK_COMPLETE == cameraControl.Detect(m_myParam.lInterface, phcInterface, phcDeviceNmae, ref lNumCameras))
					{
						if (lNumCameras > 0)
						{
							string pDevice = "ENUM:0";
							int lCapabilityBitmap = 0;
							// Establish a session between the camera.
							if (CFujiSDK.XSDK_COMPLETE == cameraControl.Open(pDevice, ref m_myParam.hCam, ref lCapabilityBitmap, (IntPtr)NULL))
							{
								// Open Success.
								if ((lCapabilityBitmap & CameraControl.XSDK_DSC_MODE_TETHER) != 0)
								{
									m_myParam.boCapabilityTether = true;
								}
								else
								{
									m_myParam.boCapabilityTether = false;
								}
								if ((lCapabilityBitmap & CameraControl.XSDK_DSC_MODE_BR) != 0)
								{
									m_myParam.boCapabilityBackupRestore = true;
								}
								else
								{
									m_myParam.boCapabilityBackupRestore = false;
								}

								int lDriveMode = 0;
								if (m_myParam.boCapabilityTether)
								{
									// Get drive mode setting
									if (CFujiSDK.XSDK_COMPLETE != cameraControl.GetDriveMode(m_myParam.hCam, ref lDriveMode))
									{
										lDriveMode = CameraControl.XSDK_DRIVE_MODE_S;
									}

									if (m_myParam.boCapabilityTether || m_myParam.boCapabilityBackupRestore)
									{
										if (lDriveMode == CameraControl.XSDK_DRIVE_MODE_MOVIE)
										{
											// DO NOT OPEN
											if (m_myParam.hCam != (IntPtr)NULL)
											{
												Thread.Sleep(600);
												cameraControl.Close(m_myParam.hCam); // Close the session between the camera.
												m_myParam.hCam = (IntPtr)NULL;
											}

											Thread.Sleep(600);

											cameraControl.Exit(); // SDK termination process.
										}
										else
										{
											boOpen = TRUE;
										}
									}
									else
									{
										// Open Error ( RAW Develop Mode )
										if (m_myParam.hCam != (IntPtr)NULL)
										{
											Thread.Sleep(600);
											cameraControl.Close(m_myParam.hCam); // Close the session between the camera.
											m_myParam.hCam = (IntPtr)NULL;
										}

										Thread.Sleep(600);

										cameraControl.Exit(); // SDK termination process.
									}
								}
							}
							else
							{
								// Open Error
								m_myParam.hCam = (IntPtr)NULL;

								cameraControl.Exit(); // SDK termination process.
							}
						}
						else
						{
							// No camera detected Error
							m_myParam.hCam = (IntPtr)NULL;

							cameraControl.Exit(); // SDK termination process.
						}
					}
					else
					{
						// Detect Error
						m_myParam.hCam = (IntPtr)NULL;

						cameraControl.Exit(); // SDK termination process.
					}
				}
				else
				{
					// Init Error
					m_myParam.hCam = (IntPtr)NULL;

					cameraControl.Exit();  // SDK termination process.

				}

				if (boOpen == 1)
				{
					break;
				}

				if (m_myParam.phEventExitConnect) break;
				Thread.Sleep(50);
				if (m_myParam.phEventExitConnect) break;
				Thread.Sleep(50);
				if (m_myParam.phEventExitConnect) break;
				Thread.Sleep(50);
				if (m_myParam.phEventExitConnect) break;
				Thread.Sleep(50);
				if (m_myParam.phEventExitConnect) break;
				Thread.Sleep(50);
				if (m_myParam.phEventExitConnect) break;
				Thread.Sleep(50);
			}
			return 0;
		}

		public int BeginConnectThread()
		{
			int iRet = 0;

			m_myParam.hCam = IntPtr.Zero;

			m_pThreadConnect = null;
			m_pThreadPoll = null;

			m_myParam.phEventExitConnect = false;
			m_myParam.phEventExitPoll = false;

			m_myParam.boCapabilityBackupRestore = false;
			m_myParam.boCapabilityTether = false;

			m_myParam.lFocalLength = -1;
			m_myParam.l35mmFocalLength = -1;
			m_myParam.lBusyCount = 0;

			// Connection process(started as a subthread)
			m_pThreadConnect = new Thread(() => ThreadConnect());
			if (m_pThreadConnect != null)
			{
				m_pThreadConnect.Start();
			}
			else
			{
				// ERROR
				iRet = -1;
			}
			return iRet;
		}

		// Abort connection process
		public int EndConnectThread()
		{
			int iRet = 0;

			if (IsConnectThread() == TRUE)
			{
				m_myParam.phEventExitConnect = true;
				m_pThreadConnect.Join();
			}

			m_pThreadConnect = null;
			return iRet;
		}

		// Find out if the connection process is working.
		public int IsConnectThread()
		{
			if (m_pThreadConnect == null)
			{
				return FALSE;
			}
			if (m_pThreadConnect.ThreadState == ThreadState.Unstarted || m_pThreadConnect.ThreadState == ThreadState.Stopped)
			{
				return FALSE;
			}
			return TRUE;
		}

		public uint ThreadPoll()
		{
			while (m_myParam.phEventExitPoll == false)
			{
				bool boDisconnected = false;

				int lNumZoomPos = 0;
				int[] plZoomPos = null;
				int[] plFocalLength = null;
				int[] pl35mmFocalLength = null;
				// Query supported zoom positions.
				if (CFujiSDK.XSDK_COMPLETE != cameraControl.CapLensZoomPos(m_myParam.hCam, ref lNumZoomPos, ref plZoomPos, ref plFocalLength, ref pl35mmFocalLength))
				{
					if (XSDK_ERRCODE.XSDK_ERRCODE_BUSY == cameraControl.ERRCode || XSDK_ERRCODE.XSDK_ERRCODE_NOT_AVAILABLE_STATUS == cameraControl.ERRCode || XSDK_ERRCODE.XSDK_ERRCODE_RUNNING_OTHER_FUNCTION == cameraControl.ERRCode)
					{
						// BUSY
						m_myParam.lBusyCount++;
					}
					else
					{
						// ERROR
						boDisconnected = true;
					}
				}
				else
				{
					//If LensZoomPos can be configured to set.
					if (lNumZoomPos > 0)
					{
						// OK
						int lZoomPos = 0;
						// Get LensZoomPos setting value
						if (CFujiSDK.XSDK_COMPLETE != cameraControl.GetLensZoomPos(m_myParam.hCam, ref lZoomPos))
						{
							if (XSDK_ERRCODE.XSDK_ERRCODE_BUSY == cameraControl.ERRCode || XSDK_ERRCODE.XSDK_ERRCODE_NOT_AVAILABLE_STATUS == cameraControl.ERRCode || XSDK_ERRCODE.XSDK_ERRCODE_RUNNING_OTHER_FUNCTION == cameraControl.ERRCode)
							{
								// BUSY
								m_myParam.lBusyCount++;
							}
							else
							{
								// ERROR
								boDisconnected = true;
							}
						}
						else
						{
							int j;
							for (j = 0; j < lNumZoomPos; j++)
							{
								// If the acquired ZoomPos is in the ZoomPos list.
								if (plZoomPos[j] == lZoomPos)
								{
									m_myParam.lFocalLength = plFocalLength[j];
									m_myParam.l35mmFocalLength = pl35mmFocalLength[j];
									break;
								}
							}
							if (j == lNumZoomPos)
							{
								// not breaked from the for loop
								m_myParam.lFocalLength = -1;
								m_myParam.l35mmFocalLength = -1;
							}
						}
					}
					else
					{
						// ERROR
						boDisconnected = true;
					}
				}

				// End when the connection to the camera is lost.
				if (boDisconnected) break;

				if (m_myParam.phEventExitPoll) break;
				Thread.Sleep(50);
				if (m_myParam.phEventExitPoll) break;
				Thread.Sleep(50);
				if (m_myParam.phEventExitPoll) break;
				Thread.Sleep(50);
				if (m_myParam.phEventExitPoll) break;
				Thread.Sleep(50);
				if (m_myParam.phEventExitPoll) break;
				Thread.Sleep(50);
				if (m_myParam.phEventExitPoll) break;
				Thread.Sleep(50);
			}

			return 0;
		}

		public int BeginPollThread()
		{
			int iRet = 0;

			m_myParam.lFocalLength = -1;
			m_myParam.l35mmFocalLength = -1;
			m_myParam.lBusyCount = 0;

			// LensZoomPos acquisition process (started as a subthread).
			m_pThreadPoll = new Thread(() => ThreadPoll());
			if (m_pThreadPoll != null)
			{
				m_pThreadPoll.Start();
			}
			else
			{
				//ERROR
				iRet = -1;
			}
			return iRet;
		}

		// Interrupt the process of acquiring LensZoomPos.
		public int EndPollThread()
		{
			int iRet = 0;

			if (IsPollThread() == TRUE)
			{
				m_myParam.phEventExitPoll = true;
				m_pThreadPoll.Join();
			}

			m_pThreadPoll = null;
			return iRet;
		}

		// Check if the connection is being processed.
		public int IsPollThread()
		{
			if (m_pThreadPoll == null)
			{
				return FALSE;
			}
			if (m_pThreadPoll.ThreadState == ThreadState.Unstarted || m_pThreadPoll.ThreadState == ThreadState.Stopped)
			{
				return FALSE;
			}
			return TRUE;
		}

		public int TerminateConnection()
		{
			if (IsConnectThread() == TRUE)
			{
				EndConnectThread();
			}

			if (IsPollThread() == TRUE)
			{
				EndPollThread();
			}

			if (m_myParam.hCam != (IntPtr)NULL)
			{
				Thread.Sleep(600);
				cameraControl.Close(m_myParam.hCam); // Close the session between the camera.
				m_myParam.hCam = (IntPtr)NULL;
			}

			Thread.Sleep(600);

			cameraControl.Exit(); // SDK termination process.

			return 0;
		}

		public int GetFocalLength()
		{
			return m_myParam.lFocalLength;
		}
		public int Get35mmFocalLength()
		{
			return m_myParam.l35mmFocalLength;
		}
		public int GetBusyCount()
		{
			return m_myParam.lBusyCount;
		}

		public void SetDetectOption(int lInterface, string pInterface)
		{
			m_myParam.lInterface = lInterface;
			m_myParam.ptchInterface = pInterface;
		}
	}
}
