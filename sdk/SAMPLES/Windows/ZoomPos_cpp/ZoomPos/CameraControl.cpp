#include "stdafx.h"

#include "FujiSDK.h"
#include "CameraControl.h"


CCameraControl::CCameraControl()
{
	lstrcpy(m_myParam.ptchModel, _T(""));
	lstrcpy(m_myParam.ptchSerial, _T(""));
	lstrcpy(m_myParam.ptchOriginalName, _T(""));
	m_myParam.iVersion = 0;

	m_myParam.hCam = NULL;
	m_myParam.pSDK = this;

	m_hEventExitConnect = NULL;
	m_pThreadConnect = NULL;

	m_hEventExitPoll = NULL;
	m_pThreadPoll = NULL;

	m_myParam.phEventExitConnect = NULL;
	m_myParam.phEventExitPoll = NULL;

	m_myParam.boCapabilityBackupRestore = false;
	m_myParam.boCapabilityTether = false;
}


CCameraControl::~CCameraControl()
{
	EndConnectThread();
	EndPollThread();

	TerminateConnection();
}

int CCameraControl::GetError()
{
	int iRet;

	iRet = CFujiSDK::GetError();

	return iRet;
}



int CCameraControl::TerminateConnection()
{
	if (m_myParam.pSDK != NULL){

		if (IsConnectThread()){
			EndConnectThread();
		}

		if (IsPollThread()){
			EndPollThread();
		}

		EnterCriticalSection();
		if (m_myParam.hCam != NULL){
			Sleep(600);
			m_pfnClose(m_myParam.hCam);
			m_myParam.hCam = NULL;
		}
		LeaveCriticalSection();

		Sleep(600);

		EnterCriticalSection();
		if(m_pfnExit != NULL){
			m_pfnExit();
		}
		LeaveCriticalSection();
	}
	return 0;
}

int CCameraControl::BeginConnectThread()
{
	int iRet = 0;

	lstrcpy(m_myParam.ptchModel, _T(""));
	lstrcpy(m_myParam.ptchSerial, _T(""));
	lstrcpy(m_myParam.ptchOriginalName, _T(""));
	m_myParam.iVersion = 0;

	m_myParam.hCam = NULL;
	m_myParam.pSDK = this;

	m_hEventExitConnect = NULL;
	m_pThreadConnect = NULL;

	m_hEventExitPoll = NULL;
	m_pThreadPoll = NULL;

	m_myParam.phEventExitConnect = NULL;
	m_myParam.phEventExitPoll = NULL;

	m_myParam.boCapabilityBackupRestore = false;
	m_myParam.boCapabilityTether = false;

	m_myParam.lFocalLength = -1;
	m_myParam.l35mmFocalLength = -1;
	m_myParam.lBusyCount = 0;


	if (m_hEventExitConnect != NULL) ::CloseHandle(m_hEventExitConnect);
	m_hEventExitConnect = ::CreateEvent(NULL, TRUE, FALSE, NULL);
	m_myParam.phEventExitConnect = &m_hEventExitConnect;

	m_pThreadConnect = AfxBeginThread(
		CCameraControl::ThreadConnect,
		(LPVOID)(&m_myParam),
		THREAD_PRIORITY_NORMAL,
		0,
		CREATE_SUSPENDED);

	if (m_pThreadConnect != NULL){
		m_pThreadConnect->m_bAutoDelete = FALSE;
		m_pThreadConnect->ResumeThread();
	}
	else {
		//ERROR
		iRet = -1;
	}
	return iRet;
}



int CCameraControl::EndConnectThread()
{
	int iRet = 0;

	if (m_pThreadConnect != NULL){
		if (::WaitForSingleObject(m_pThreadConnect->m_hThread, 0) == WAIT_TIMEOUT){
			SetEvent(m_hEventExitConnect);
			while (::WaitForSingleObject(m_pThreadConnect->m_hThread, 0) == WAIT_TIMEOUT){
				Sleep(60);
			}
		}

		if (m_hEventExitConnect != NULL) ::CloseHandle(m_hEventExitConnect);
		m_hEventExitConnect = NULL;

		delete m_pThreadConnect;
		m_pThreadConnect = NULL;
	}
	else{
		// already ended
		if (m_hEventExitConnect != NULL) ::CloseHandle(m_hEventExitConnect);
		m_hEventExitConnect = NULL;
	}

	return iRet;
}




BOOL CCameraControl::IsConnectThread()
{
	if (NULL == m_pThreadConnect)return FALSE;
	if (::WaitForSingleObject(m_pThreadConnect->m_hThread, 0) == WAIT_TIMEOUT){
		return TRUE;
	}
	else{
		return FALSE;
	}
}




UINT CCameraControl::ThreadConnect(LPVOID pParameter)
{
	PCAMERA_CONTROL_THREAD_PARAM pPara = (PCAMERA_CONTROL_THREAD_PARAM)pParameter;
	HANDLE*  phEventExit = (pPara->phEventExitConnect);
	CCameraControl* pSDK = (CCameraControl *)(pPara->pSDK);

	BOOL boOpen = FALSE;

	while (::WaitForSingleObject(*phEventExit, 0) == WAIT_TIMEOUT){

		// if (XSDK_COMPLETE == pSDK->m_pfnInit((LIB_HANDLE)(pSDK->GetHModule()))){
		if (XSDK_COMPLETE == pSDK->m_pfnInit((LIB_HANDLE)(NULL))){

			long lNumCameras = 0;
			char* pchInterface;
			pchInterface = new char[1024];
			WideCharToMultiByte(CP_THREAD_ACP, 0, (LPWSTR)((LPCTSTR)pPara->ptchInterface), -1, (LPSTR)(pchInterface), 1024, NULL, NULL);

			if (XSDK_COMPLETE == pSDK->m_pfnDetect(pPara->lInterface, pchInterface, NULL, &lNumCameras)){

				if (lNumCameras > 0){

					char pDevice[ 8 ];
					strcpy_s( pDevice, 8, "ENUM:0");
					long lCapabilityBitmap;
					if (XSDK_COMPLETE == pSDK->m_pfnOpenEx(pDevice, &(pPara->hCam), &lCapabilityBitmap, NULL)){


						// Open Success
						if (lCapabilityBitmap & XSDK_DSC_MODE_TETHER){
							pPara->boCapabilityTether = true;
						}
						else {
							pPara->boCapabilityTether = false;
						}

						if (lCapabilityBitmap & XSDK_DSC_MODE_BR){
							pPara->boCapabilityBackupRestore = true;
						}
						else {
							pPara->boCapabilityBackupRestore = false;
						}


						long lDriveMode;
						if (pPara->boCapabilityTether){
							if (XSDK_COMPLETE == pSDK->m_pfnGetDriveMode(pPara->hCam, &lDriveMode)){
							}
							else{
								lDriveMode = XSDK_DRIVE_MODE_S;
							}
						}


						if (pPara->boCapabilityTether || pPara->boCapabilityBackupRestore){

							if (lDriveMode == XSDK_DRIVE_MODE_MOVIE){
								// DO NOT OPEN
								if (pPara->hCam != NULL){
									Sleep(600);
									pSDK->m_pfnClose(pPara->hCam);
									pPara->hCam = NULL;
								}

								Sleep(600);

								pSDK->EnterCriticalSection();
								pSDK->m_pfnExit();
								pSDK->LeaveCriticalSection();
							}
							else{

								XSDK_DeviceInformation  devInfo;
								if (XSDK_COMPLETE == pSDK->m_pfnGetDeviceInfo(pPara->hCam, &devInfo)){

									if (strcmp(devInfo.strProduct, "FinePix S5Pro") == 0){
										// Open Error ( S5Pro or ISPro )
										if (pPara->hCam != NULL){
											Sleep(600);
											pSDK->m_pfnClose(pPara->hCam);
											pPara->hCam = NULL;
										}

										Sleep(600);

										pSDK->EnterCriticalSection();
										pSDK->m_pfnExit();
										pSDK->LeaveCriticalSection();
									}
									else {

										// Open OK
										TCHAR ptchTmp[256];
										MultiByteToWideChar(CP_THREAD_ACP, MB_PRECOMPOSED, (LPCSTR)(devInfo.strManufacturer), -1, (LPWSTR)ptchTmp, 256);
										lstrcpy(pPara->ptchModel, ptchTmp);
										lstrcat(pPara->ptchModel, _T(" "));
										MultiByteToWideChar(CP_THREAD_ACP, MB_PRECOMPOSED, (LPCSTR)(devInfo.strProduct), -1, (LPWSTR)ptchTmp, 256);
										lstrcat(pPara->ptchModel, ptchTmp);

										MultiByteToWideChar(CP_THREAD_ACP, MB_PRECOMPOSED, (LPCSTR)(devInfo.strSerialNo), -1, (LPWSTR)ptchTmp, 256);
										lstrcpy(pPara->ptchSerial, ptchTmp);

										MultiByteToWideChar(CP_THREAD_ACP, MB_PRECOMPOSED, (LPCSTR)(devInfo.strDeviceName), -1, (LPWSTR)ptchTmp, 256);
										lstrcpy(pPara->ptchOriginalName, ptchTmp);

										// Convert F/W version string to int
										char pchFirmware[256];
										char* pchPtr;
										char* pchPtr2;
										int iMajor, iMinor;
										strcpy_s(pchFirmware, 256, devInfo.strFirmware);
										pchPtr = strchr(pchFirmware, '.');
										pchPtr2 = pchPtr;
										pchPtr2++;
										iMinor = atoi(pchPtr2);
										*pchPtr = '\0';
										iMajor = atoi(pchFirmware);
										pPara->iVersion = (int)(iMajor * 100 + iMinor);


										if (strcmp(devInfo.strProduct, "X-T1") == 0 && pPara->iVersion < 520){
											pPara->boCapabilityBackupRestore = false;
										}



										boOpen = TRUE;
									}

								}
								else {
									// Open Error ( GetDeviceInfo Error )
									if (pPara->hCam != NULL){
										Sleep(600);
										pSDK->m_pfnClose(pPara->hCam);
										pPara->hCam = NULL;
									}

									Sleep(600);

									pSDK->EnterCriticalSection();
									pSDK->m_pfnExit();
									pSDK->LeaveCriticalSection();
								}
							}

						}
						else {
							// Open Error ( RAW Develop Mode )
							if (pPara->hCam != NULL){
								Sleep(600);
								pSDK->m_pfnClose(pPara->hCam);
								pPara->hCam = NULL;
							}

							Sleep(600);

							pSDK->EnterCriticalSection();
							pSDK->m_pfnExit();
							pSDK->LeaveCriticalSection();
						}

					}
					else {

						// Open Error
						pPara->hCam = NULL;

						pSDK->EnterCriticalSection();
						pSDK->m_pfnExit();
						pSDK->LeaveCriticalSection();
					}

				}
				else {
					// No camera detected Error
					pPara->hCam = NULL;
					pSDK->EnterCriticalSection();
					pSDK->m_pfnExit();
					pSDK->LeaveCriticalSection();

				}
			}
			else {
				// Detect Error
				pPara->hCam = NULL;
				pSDK->EnterCriticalSection();
				pSDK->m_pfnExit();
				pSDK->LeaveCriticalSection();

			}

			if (pchInterface != NULL){
				delete pchInterface;
				pchInterface = NULL;
			}
		}
		else {
			// Init Error
			pPara->hCam = NULL;
			pSDK->EnterCriticalSection();
			pSDK->m_pfnExit();
			pSDK->LeaveCriticalSection();
		}

		if (boOpen) break;


		if (::WaitForSingleObject(*phEventExit, 0) != WAIT_TIMEOUT) break;
		Sleep(50);

		if (::WaitForSingleObject(*phEventExit, 0) != WAIT_TIMEOUT) break;
		Sleep(50);

		if (::WaitForSingleObject(*phEventExit, 0) != WAIT_TIMEOUT) break;
		Sleep(50);

		if (::WaitForSingleObject(*phEventExit, 0) != WAIT_TIMEOUT) break;
		Sleep(50);

		if (::WaitForSingleObject(*phEventExit, 0) != WAIT_TIMEOUT) break;
		Sleep(50);

		if (::WaitForSingleObject(*phEventExit, 0) != WAIT_TIMEOUT) break;
		Sleep(50);
	}

	AfxEndThread(0, FALSE);
	return 0;
}





int CCameraControl::BeginPollThread()
{
	int iRet = 0;

	m_myParam.lFocalLength = -1;
	m_myParam.l35mmFocalLength = -1;
	m_myParam.lBusyCount = 0;

	if (m_hEventExitPoll != NULL) ::CloseHandle(m_hEventExitPoll);
	m_hEventExitPoll = ::CreateEvent(NULL, TRUE, FALSE, NULL);
	m_myParam.phEventExitPoll = &m_hEventExitPoll;

	m_pThreadPoll = AfxBeginThread(
		CCameraControl::ThreadPoll,
		(LPVOID)(&m_myParam),
		THREAD_PRIORITY_NORMAL,
		0,
		CREATE_SUSPENDED);

	if (m_pThreadPoll != NULL){
		m_pThreadPoll->m_bAutoDelete = FALSE;
		m_pThreadPoll->ResumeThread();
	}
	else {
		//ERROR
		iRet = -1;
	}
	return iRet;
}



int CCameraControl::EndPollThread()
{
	int iRet = 0;

	if (m_pThreadPoll != NULL){
		if (::WaitForSingleObject(m_pThreadPoll->m_hThread, 0) == WAIT_TIMEOUT){
			SetEvent(m_hEventExitPoll);
			while (::WaitForSingleObject(m_pThreadPoll->m_hThread, 0) == WAIT_TIMEOUT){
				Sleep(60);
			}
		}

		if (m_hEventExitPoll != NULL) ::CloseHandle(m_hEventExitPoll);
		m_hEventExitPoll = NULL;

		delete m_pThreadPoll;
		m_pThreadPoll = NULL;
	}
	else{
		// already ended
		if (m_hEventExitPoll != NULL) ::CloseHandle(m_hEventExitPoll);
		m_hEventExitPoll = NULL;
	}

	return iRet;
}


BOOL CCameraControl::IsPollThread()
{
	if (NULL == m_pThreadPoll)return FALSE;
	if (::WaitForSingleObject(m_pThreadPoll->m_hThread, 0) == WAIT_TIMEOUT){
		return TRUE;
	}
	else{
		return FALSE;
	}
}


UINT CCameraControl::ThreadPoll(LPVOID pParameter)
{
	PCAMERA_CONTROL_THREAD_PARAM pPara = (PCAMERA_CONTROL_THREAD_PARAM)pParameter;
	HANDLE*  phEventExit = (pPara->phEventExitPoll);
	CCameraControl* pSDK = (CCameraControl *)(pPara->pSDK);

	long lAPICode, lErrCode;


	while (::WaitForSingleObject(*phEventExit, 0) == WAIT_TIMEOUT){
		bool boDisconnected = false;

		pSDK->EnterCriticalSection();
		XSDK_DeviceInformation  devInfo;
		if (XSDK_COMPLETE != pSDK->m_pfnGetDeviceInfo(pPara->hCam, &devInfo)){
			pSDK->m_pfnGetErrorNumber(pPara->hCam, &lAPICode, &lErrCode);
			if (XSDK_ERRCODE_BUSY == lErrCode || XSDK_ERRCODE_FORCEMODE_BUSY == lErrCode || XSDK_ERRCODE_RUNNING_OTHER_FUNCTION == lErrCode){
				// BUSY
			}
			else{
				// ERROR
				boDisconnected = true;
			}
		}

		long lNumZoomPos, lNumZoomPosBackup;
		if (XSDK_COMPLETE != pSDK->m_pfnCapLensZoomPos(pPara->hCam, &lNumZoomPos, NULL, NULL, NULL)){
			pSDK->m_pfnGetErrorNumber(pPara->hCam, &lAPICode, &lErrCode);
			if (XSDK_ERRCODE_BUSY == lErrCode || XSDK_ERRCODE_FORCEMODE_BUSY == lErrCode || XSDK_ERRCODE_RUNNING_OTHER_FUNCTION == lErrCode){
				// BUSY
				pPara->lBusyCount++;
			}
			else{
				// ERROR
				boDisconnected = true;
			}
		}
		else{
			if (lNumZoomPos > 0){
				long* plZoomPos = NULL;
				long* plFocalLength = NULL;
				long* pl35mmFocalLength = NULL;
				// Success
				try{
					plZoomPos = new long[lNumZoomPos];
				}
				catch (...){
					plZoomPos = NULL;
				}
				try{
					plFocalLength = new long[lNumZoomPos];
				}
				catch (...){
					plFocalLength = NULL;
				}
				try{
					pl35mmFocalLength = new long[lNumZoomPos];
				}
				catch (...){
					pl35mmFocalLength = NULL;
				}
				if (plZoomPos == NULL || plFocalLength == NULL || pl35mmFocalLength == NULL){
					// ERROR
					boDisconnected = true;

					delete[] plZoomPos;
					delete[] plFocalLength;
					delete[] pl35mmFocalLength;
				}
				else{
					lNumZoomPosBackup = lNumZoomPos;
					if (XSDK_COMPLETE != pSDK->m_pfnCapLensZoomPos(pPara->hCam, &lNumZoomPos, plZoomPos, plFocalLength, pl35mmFocalLength)){
						pSDK->m_pfnGetErrorNumber(pPara->hCam, &lAPICode, &lErrCode);
						if (XSDK_ERRCODE_BUSY == lErrCode || XSDK_ERRCODE_FORCEMODE_BUSY == lErrCode || XSDK_ERRCODE_RUNNING_OTHER_FUNCTION == lErrCode){
							// BUSY
							pPara->lBusyCount++;

						}
						else{
							// ERROR
							boDisconnected = true;
						}
					}
					else{
						if (lNumZoomPosBackup == lNumZoomPos){
							// OK
							long lZoomPos;
							if (XSDK_COMPLETE != pSDK->m_pfnGetLensZoomPos(pPara->hCam, &lZoomPos)){
								pSDK->m_pfnGetErrorNumber(pPara->hCam, &lAPICode, &lErrCode);
								if (XSDK_ERRCODE_BUSY == lErrCode || XSDK_ERRCODE_FORCEMODE_BUSY == lErrCode || XSDK_ERRCODE_RUNNING_OTHER_FUNCTION == lErrCode){
									// BUSY
									pPara->lBusyCount++;
								}
								else{
									// ERROR
									boDisconnected = true;
								}
							}
							else{
								CString s;

								long j;
								for (j = 0; j < lNumZoomPos; j++){
									if (plZoomPos[j] == lZoomPos){
										pPara->lFocalLength = plFocalLength[j];
										pPara->l35mmFocalLength = pl35mmFocalLength[j];
										break;
									}
								}
								if (j == lNumZoomPos){
									// not breaked from the for loop
									pPara->lFocalLength = -1;
									pPara->l35mmFocalLength = -1;
								}

							}
						}else{
							// The lens may be changed: Do nothing
						}
					}
				}

			}
			else{
				// ERROR
				boDisconnected = true;
			}
		}



		pSDK->LeaveCriticalSection();

		if (boDisconnected) break;
		if (::WaitForSingleObject(*phEventExit, 0) != WAIT_TIMEOUT) break;
		Sleep(50);
		if (::WaitForSingleObject(*phEventExit, 0) != WAIT_TIMEOUT) break;
		Sleep(50);
		if (::WaitForSingleObject(*phEventExit, 0) != WAIT_TIMEOUT) break;
		Sleep(50);
		if (::WaitForSingleObject(*phEventExit, 0) != WAIT_TIMEOUT) break;
		Sleep(50);
		if (::WaitForSingleObject(*phEventExit, 0) != WAIT_TIMEOUT) break;
		Sleep(50);
		if (::WaitForSingleObject(*phEventExit, 0) != WAIT_TIMEOUT) break;
		Sleep(50);
	}

	AfxEndThread(0, FALSE);
	return 0;
}














LPCTSTR CCameraControl::GetModelName()
{
	return m_myParam.ptchModel;
}

LPCTSTR CCameraControl::GetOriginalName()
{
	return m_myParam.ptchOriginalName;
}

LPCTSTR CCameraControl::GetSerial()
{
	return m_myParam.ptchSerial;
}

int CCameraControl::GetVersion()
{
	return m_myParam.iVersion;
}

bool CCameraControl::IsTetherCapable()
{
	return m_myParam.boCapabilityTether;
}

bool CCameraControl::IsBackupRestoreCapable()
{
	return m_myParam.boCapabilityBackupRestore;
}


void CCameraControl::SetDetectOption(long lInterface, LPCTSTR pInterface)
{
	m_myParam.lInterface = lInterface;
	lstrcpy( m_myParam.ptchInterface, pInterface );
}


long CCameraControl::GetFocalLength()
{
	return m_myParam.lFocalLength;
}


long CCameraControl::Get35mmFocalLength()
{
		return m_myParam.l35mmFocalLength;
}

long CCameraControl::GetBusyCount()
{
	return m_myParam.lBusyCount;
}