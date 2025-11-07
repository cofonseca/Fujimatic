#include "stdafx.h"

#include "FujiSDK.h"
#include "CameraControl.h"


CCameraControl::CCameraControl()
{
	lstrcpy(m_myParam.ptchModel, _T(""));
	m_myParam.iVersion = 0;

	m_myParam.hCam = NULL;
	m_myParam.pSDK = this;

	m_hEventExitConnect = NULL;
	m_pThreadConnect = NULL;

	m_hEventExitPoll = NULL;
	m_pThreadPoll = NULL;

	m_hEventExitS1 = NULL;
	m_pThreadS2 = NULL;

	m_hEventExitS2 = NULL;
	m_pThreadS2 = NULL;

	m_myParam.phEventExitConnect = NULL;
	m_myParam.phEventExitPoll = NULL;
	m_myParam.phEventExitS1 = NULL;
	m_myParam.phEventExitS2 = NULL;

	m_myParam.boCapabilityBackupRestore = false;
	m_myParam.boCapabilityTether = false;

	m_myParam.boAcceptReleaseInCameraPriorityMode = false;
}


CCameraControl::~CCameraControl()
{
	EndConnectThread();
	EndPollThread();
	EndS1Thread();
	EndS2Thread();

	TerminateConnection();
}

int CCameraControl::GetError()
{
	int iRet;

	iRet = CFujiSDK::GetError();

	return iRet;
}


int CCameraControl::TerminateS1()
{
	long lShootOpt;
	long lAfStatus;

	EnterCriticalSection();
	if (IsAcceptReleaseInCameraPriorityMode()){
		m_pfnReleaseEx(m_myParam.hCam, XSDK_RELEASE_EX_S1_OFF, &lShootOpt, &lAfStatus);
	}
	else{
		m_pfnRelease(m_myParam.hCam, XSDK_RELEASE_N_S1OFF, &lShootOpt, &lAfStatus);
	}
	LeaveCriticalSection();

	bool boNotFinalized = true;
	while (boNotFinalized){
		EnterCriticalSection();
		if (XSDK_COMPLETE == m_pfnSetPriorityMode(m_myParam.hCam, XSDK_PRIORITY_CAMERA)){
			boNotFinalized = false;
		}
		LeaveCriticalSection();
		Sleep(50);
	}
	
	return 0;
}


int CCameraControl::TerminateS2()
{
	long lShootOpt;
	long lAfStatus;

	EnterCriticalSection();
	if (IsAcceptReleaseInCameraPriorityMode()){
		m_pfnReleaseEx(m_myParam.hCam, XSDK_RELEASE_EX_CANCEL, &lShootOpt, &lAfStatus);
	}
	else{
		m_pfnRelease(m_myParam.hCam, XSDK_RELEASE_CANCEL, &lShootOpt, &lAfStatus);
	}
	LeaveCriticalSection();

	bool boNotFinalized = true;
	while (boNotFinalized){
		EnterCriticalSection();
		if (XSDK_COMPLETE == m_pfnSetPriorityMode(m_myParam.hCam, XSDK_PRIORITY_CAMERA)){
			boNotFinalized = false;
		}
		LeaveCriticalSection();
		// Waiting for the in-camera buffer empty
		Sleep(10);
		Sleep(10);
		Sleep(10);
		Sleep(10);
		Sleep(10);
	}

	return 0;
}

int CCameraControl::SetPriorityMode(long lPriorityMode)
{
	bool boNotSet = true;
	while (boNotSet){
		EnterCriticalSection();
		if (XSDK_COMPLETE == m_pfnSetPriorityMode(m_myParam.hCam, lPriorityMode)){
			boNotSet = false;
		}
		LeaveCriticalSection();
		// Waiting for the in-camera buffer empty
		Sleep(10);
		Sleep(10);
		Sleep(10);
		Sleep(10);
		Sleep(10);
	}

	return 0;
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

		if (IsS1Thread()){
			EndS1Thread();
		}

		if (IsS2Thread()){
			EndS2Thread();
		}

		EnterCriticalSection();
		if (m_myParam.hCam != NULL){
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
	m_myParam.iVersion = 0;

	m_myParam.hCam = NULL;
	m_myParam.pSDK = this;

	m_hEventExitConnect = NULL;
	m_pThreadConnect = NULL;

	m_hEventExitPoll = NULL;
	m_pThreadPoll = NULL;

	m_hEventExitS1 = NULL;
	m_pThreadS1 = NULL;

	m_hEventExitS2 = NULL;
	m_pThreadS2 = NULL;

	m_myParam.phEventExitConnect = NULL;
	m_myParam.phEventExitPoll = NULL;
	m_myParam.phEventExitS1 = NULL;
	m_myParam.phEventExitS2 = NULL;

	m_myParam.boCapabilityBackupRestore = false;
	m_myParam.boCapabilityTether = false;


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
	BOOL boPCPriority = FALSE;

	while (::WaitForSingleObject(*phEventExit, 0) == WAIT_TIMEOUT){

		if (boOpen == FALSE){
			// if (XSDK_COMPLETE == pSDK->m_pfnInit((LIB_HANDLE)(pSDK->GetHModule()))){
			if (XSDK_COMPLETE == pSDK->m_pfnInit((LIB_HANDLE)(NULL))){

				long lNumCameras = 0;
				char* pchInterface;
				pchInterface = new char[1024];
				WideCharToMultiByte(CP_THREAD_ACP, 0, (LPWSTR)((LPCTSTR)pPara->ptchInterface), -1, (LPSTR)(pchInterface), 1024, NULL, NULL);

				if (XSDK_COMPLETE == pSDK->m_pfnDetect(pPara->lInterface, pchInterface, NULL, &lNumCameras)){

					if (lNumCameras > 0){

						char pDevice[8];
						strcpy_s(pDevice, 8, "ENUM:0");
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
											MultiByteToWideChar(CP_THREAD_ACP, MB_PRECOMPOSED, (LPCSTR)(devInfo.strProduct), -1, (LPWSTR)ptchTmp, 256);
											lstrcat(pPara->ptchModel, ptchTmp);

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

											long lNumReleaseMode;
											if (XSDK_COMPLETE == pSDK->m_pfnCapReleaseEx(pPara->hCam, &lNumReleaseMode, NULL)){
												if (lNumReleaseMode != 0){
													pPara->boAcceptReleaseInCameraPriorityMode = true;
												}
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
		}

		if (boOpen){
			if (boPCPriority == FALSE){
				long lAPICode, lErrCode;
				pSDK->EnterCriticalSection();
				if (XSDK_COMPLETE != pSDK->m_pfnSetPriorityMode(pPara->hCam, XSDK_PRIORITY_PC)){
					pSDK->m_pfnGetErrorNumber(pPara->hCam, &lAPICode, &lErrCode);
					if (XSDK_ERRCODE_BUSY == lErrCode || XSDK_ERRCODE_FORCEMODE_BUSY == lErrCode || XSDK_ERRCODE_RUNNING_OTHER_FUNCTION == lErrCode){
						// BUSY
					}
					else{
						// ERROR
					}
				}
				else{
					// Set the media recording = ON
					pSDK->m_pfnSetMediaRecord(pPara->hCam, XSDK_MEDIAREC_RAWJPEG);

					boPCPriority = TRUE;
				}
				pSDK->LeaveCriticalSection();
			}
		}

		if (boOpen && boPCPriority) break;


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
		XSDK_ImageInformation  imageInfo;
		if (XSDK_COMPLETE != pSDK->m_pfnReadImageInfo(pPara->hCam, &imageInfo)){
			pSDK->m_pfnGetErrorNumber(pPara->hCam, &lAPICode, &lErrCode);
			if (XSDK_ERRCODE_BUSY == lErrCode || XSDK_ERRCODE_FORCEMODE_BUSY == lErrCode || XSDK_ERRCODE_RUNNING_OTHER_FUNCTION == lErrCode){
				// BUSY
			}
			else{
				// ERROR
				boDisconnected = true;
			}
		}
		if (imageInfo.lFormat != XSDK_IMAGEFORMAT_NONE){
			pSDK->m_pfnDeleteImage(pPara->hCam);
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







int CCameraControl::BeginS1Thread()
{
	int iRet = 0;

	if (m_hEventExitS1 != NULL) ::CloseHandle(m_hEventExitS1);
	m_hEventExitS1 = ::CreateEvent(NULL, TRUE, FALSE, NULL);
	m_myParam.phEventExitS1 = &m_hEventExitS1;

	m_pThreadS1 = AfxBeginThread(
		CCameraControl::ThreadS1,
		(LPVOID)(&m_myParam),
		THREAD_PRIORITY_NORMAL,
		0,
		CREATE_SUSPENDED);

	if (m_pThreadS1 != NULL){
		m_pThreadS1->m_bAutoDelete = FALSE;
		m_pThreadS1->ResumeThread();
	}
	else {
		//ERROR
		iRet = -1;
	}
	return iRet;
}



int CCameraControl::EndS1Thread()
{
	int iRet = 0;

	if (m_pThreadS1 != NULL){
		if (::WaitForSingleObject(m_pThreadS1->m_hThread, 0) == WAIT_TIMEOUT){
			SetEvent(m_hEventExitS1);
			while (::WaitForSingleObject(m_pThreadS1->m_hThread, 0) == WAIT_TIMEOUT){
				Sleep(60);
			}
		}

		if (m_hEventExitS1 != NULL) ::CloseHandle(m_hEventExitS1);
		m_hEventExitS1 = NULL;

		delete m_pThreadS1;
		m_pThreadS1 = NULL;
	}
	else{
		// already ended
		if (m_hEventExitS1 != NULL) ::CloseHandle(m_hEventExitS1);
		m_hEventExitS1 = NULL;
	}

	return iRet;
}


BOOL CCameraControl::IsS1Thread()
{
	if (NULL == m_pThreadS1)return FALSE;
	if (::WaitForSingleObject(m_pThreadS1->m_hThread, 0) == WAIT_TIMEOUT){
		return TRUE;
	}
	else{
		return FALSE;
	}
}


UINT CCameraControl::ThreadS1(LPVOID pParameter)
{
	PCAMERA_CONTROL_THREAD_PARAM pPara = (PCAMERA_CONTROL_THREAD_PARAM)pParameter;
	HANDLE*  phEventExit = (pPara->phEventExitS1);
	CCameraControl* pSDK = (CCameraControl *)(pPara->pSDK);

	long lAPICode, lErrCode;
	long lStatus;
	bool boS1Done = false;
	bool boDisconnected = false;
	while (::WaitForSingleObject(*phEventExit, 0) == WAIT_TIMEOUT){
		long lShotOpt = 1;
		pSDK->EnterCriticalSection();

		long lPriorityMode;
		pSDK->m_pfnGetPriorityMode(pPara->hCam, &lPriorityMode);

		long lRet;
		if (pPara->boAcceptReleaseInCameraPriorityMode && lPriorityMode == XSDK_PRIORITY_CAMERA){
			lRet = pSDK->m_pfnReleaseEx(pPara->hCam, XSDK_RELEASE_EX_S1_ON, &lShotOpt, &lStatus);
		}
		else{
			lRet = pSDK->m_pfnRelease(pPara->hCam, XSDK_RELEASE_S1ON, &lShotOpt, &lStatus);
		}

		if (XSDK_COMPLETE != lRet){
			pSDK->m_pfnGetErrorNumber(pPara->hCam, &lAPICode, &lErrCode);
			if (XSDK_ERRCODE_BUSY == lErrCode || XSDK_ERRCODE_FORCEMODE_BUSY == lErrCode || XSDK_ERRCODE_RUNNING_OTHER_FUNCTION == lErrCode){
				// BUSY
			}
			else{
				// ERROR
				boDisconnected = true;
			}
		}
		else{
			boS1Done = true;
		}
		pSDK->LeaveCriticalSection();


		if (boS1Done) break;
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




int CCameraControl::BeginS2Thread()
{
	int iRet = 0;

	if (m_hEventExitS2 != NULL) ::CloseHandle(m_hEventExitS2);
	m_hEventExitS2 = ::CreateEvent(NULL, TRUE, FALSE, NULL);
	m_myParam.phEventExitS2 = &m_hEventExitS2;

	m_pThreadS2 = AfxBeginThread(
		CCameraControl::ThreadS2,
		(LPVOID)(&m_myParam),
		THREAD_PRIORITY_NORMAL,
		0,
		CREATE_SUSPENDED);

	if (m_pThreadS2 != NULL){
		m_pThreadS2->m_bAutoDelete = FALSE;
		m_pThreadS2->ResumeThread();
	}
	else {
		//ERROR
		iRet = -1;
	}
	return iRet;
}



int CCameraControl::EndS2Thread()
{
	int iRet = 0;

	if (m_pThreadS2 != NULL){
		if (::WaitForSingleObject(m_pThreadS2->m_hThread, 0) == WAIT_TIMEOUT){
			SetEvent(m_hEventExitS2);
			while (::WaitForSingleObject(m_pThreadS2->m_hThread, 0) == WAIT_TIMEOUT){
				Sleep(60);
			}
		}

		if (m_hEventExitS2 != NULL) ::CloseHandle(m_hEventExitS2);
		m_hEventExitS2 = NULL;

		delete m_pThreadS2;
		m_pThreadS2 = NULL;
	}
	else{
		// already ended
		if (m_hEventExitS2 != NULL) ::CloseHandle(m_hEventExitS2);
		m_hEventExitS2 = NULL;
	}

	return iRet;
}


BOOL CCameraControl::IsS2Thread()
{
	if (NULL == m_pThreadS2)return FALSE;
	if (::WaitForSingleObject(m_pThreadS2->m_hThread, 0) == WAIT_TIMEOUT){
		return TRUE;
	}
	else{
		return FALSE;
	}
}


UINT CCameraControl::ThreadS2(LPVOID pParameter)
{
	PCAMERA_CONTROL_THREAD_PARAM pPara = (PCAMERA_CONTROL_THREAD_PARAM)pParameter;
	HANDLE*  phEventExit = (pPara->phEventExitS2);
	CCameraControl* pSDK = (CCameraControl *)(pPara->pSDK);

	long lAPICode, lErrCode;
	bool boS2Done = false;
	bool boS2Comp = false;
	bool boDisconnected = false;

	while (::WaitForSingleObject(*phEventExit, 0) == WAIT_TIMEOUT){

		if (boS2Done == false){
			pSDK->EnterCriticalSection();
			long lShotOpt = 1;
			long lStatus;

			long lPriorityMode;
			pSDK->m_pfnGetPriorityMode(pPara->hCam, &lPriorityMode);

			long lRet;
			if (pPara->boAcceptReleaseInCameraPriorityMode && lPriorityMode == XSDK_PRIORITY_CAMERA){
				lRet = pSDK->m_pfnReleaseEx(pPara->hCam, XSDK_RELEASE_EX_S2_ON_S2_OFF_S1_OFF, &lShotOpt, &lStatus);
			}
			else{
				lRet = pSDK->m_pfnRelease(pPara->hCam, XSDK_RELEASE_S2_S1OFF, &lShotOpt, &lStatus);
			}

			if (XSDK_COMPLETE != lRet){
				pSDK->m_pfnGetErrorNumber(pPara->hCam, &lAPICode, &lErrCode);
				if (XSDK_ERRCODE_BUSY == lErrCode || XSDK_ERRCODE_FORCEMODE_BUSY == lErrCode || XSDK_ERRCODE_RUNNING_OTHER_FUNCTION == lErrCode){
					// BUSY
				}
				else{
					// ERROR
					boDisconnected = true;
				}
			}
			else{
				boS2Done = true;
			}
			pSDK->LeaveCriticalSection();

		}


		if (boS2Done == true && boS2Comp == false){
			bool boBusy = false;
			pSDK->EnterCriticalSection();
			if (XSDK_COMPLETE != pSDK->m_pfnSetMediaRecord(pPara->hCam, XSDK_MEDIAREC_RAWJPEG)){
				pSDK->m_pfnGetErrorNumber(pPara->hCam, &lAPICode, &lErrCode);
				if (XSDK_ERRCODE_BUSY == lErrCode || XSDK_ERRCODE_FORCEMODE_BUSY == lErrCode || XSDK_ERRCODE_RUNNING_OTHER_FUNCTION == lErrCode){
					// BUSY
					boBusy = true;
				}
				else{
					// ERROR
					boDisconnected = true;
				}
			}
			pSDK->LeaveCriticalSection();

			if (boBusy){
			}
			else{
				boS2Comp = true;
			}
		}


		if (boS2Comp) break;
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

bool CCameraControl::IsAcceptReleaseInCameraPriorityMode()
{
	return m_myParam.boAcceptReleaseInCameraPriorityMode;
}

void CCameraControl::SetDetectOption(long lInterface, LPCTSTR pInterface)
{
	m_myParam.lInterface = lInterface;
	lstrcpy( m_myParam.ptchInterface, pInterface );
}

long CCameraControl::GetPriorityMode()
{
	long lPriorityMode;
	m_pfnGetPriorityMode(m_myParam.hCam, &lPriorityMode);
	return lPriorityMode;
}
