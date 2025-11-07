#pragma once

enum {
	CAMERA_CONTROL_RET_SUCCESS = 0,
	CAMERA_CONTROL_RET_BUSY = 1,
	CAMERA_CONTROL_RET_ERROR = -1,
};







typedef struct _CAMERA_CONTROL_THREAD_PARAM {
	PVOID            pSDK;

	XSDK_HANDLE      hCam;
	TCHAR            ptchModel[128];
	TCHAR            ptchSerial[128];
	TCHAR            ptchOriginalName[128];
	int              iVersion;

	bool             boCapabilityTether;
	bool             boCapabilityBackupRestore;

	HANDLE*          phEventExitConnect;
	HANDLE*          phEventExitPoll;

	long             lInterface;
	TCHAR            ptchInterface[ 1024 ];

	long             lFocalLength;
	long             l35mmFocalLength;
	long             lBusyCount;

} CAMERA_CONTROL_THREAD_PARAM, *PCAMERA_CONTROL_THREAD_PARAM;

#define THREAD_POLL_AEMODE            (0x00000001)
#define THREAD_POLL_SS                (0x00000002)
#define THREAD_POLL_F                 (0x00000004)
#define THREAD_POLL_ISO               (0x00000008)
#define THREAD_POLL_WB                (0x00000010)
#define THREAD_POLL_EXPOSUREBIAS      (0x00000020)


class CCameraControl :
	public CFujiSDK
{
public:
	CCameraControl();
	~CCameraControl();
	int GetError();

public:
	static UINT ThreadConnect(LPVOID pParameter);
	int BeginConnectThread();
	int EndConnectThread();
	BOOL IsConnectThread();

	static UINT ThreadPoll(LPVOID pParameter);
	int BeginPollThread();
	int EndPollThread();
	BOOL IsPollThread();

	int TerminateConnection();

	


private:

	CAMERA_CONTROL_THREAD_PARAM  m_myParam;

	HANDLE   m_hEventExitConnect;
	CWinThread* m_pThreadConnect;

	HANDLE   m_hEventExitPoll;
	CWinThread* m_pThreadPoll;


public:
	LPCTSTR GetModelName();
	LPCTSTR GetOriginalName();
	LPCTSTR GetSerial();
	int GetVersion();
	bool IsTetherCapable();
	bool IsBackupRestoreCapable();
	long GetFocalLength();
	long Get35mmFocalLength();
	long GetBusyCount();

	void SetDetectOption(long lInterface, LPCTSTR pInterface);



};

