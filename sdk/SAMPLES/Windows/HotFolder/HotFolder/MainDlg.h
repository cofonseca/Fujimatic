
// MainDlg.h : header file
//

#pragma once

enum {
	STATE_DISCONNECT  = 0,
	STATE_CONNECT     = 1,
	STATE_BUSY        = 2,
};

enum {
	ICON_OFF  = 0,
	ICON_ON   = 1,
	ICON_ACT  = 2,
	ICON_ERR  = 3,
};

enum {
	TIMER_ID_SPLASH        = 0,
	TIMER_ID_ICON          = 1,
	TIMER_ID_INIT          = 2,
	TIMER_ID_ELAPSE_DETECT = 3, // Not used
	TIMER_ID_WAITINIT      = 4,
	TIMER_ID_WATCHSESSION  = 5,
};

#define  TIMER_ELAPSE_SPLASH          (2500)
#define  TIMER_ELAPSE_ICON            (700)
#define  TIMER_ELAPSE_INIT            (100)
#define  TIMER_ELAPSE_WATCHSESSION    (500)
#define  TIMER_ELAPSE_DETECT          (1000/5)
#define  TIMER_ELAPSE_WAITINIT        (80)
#define  TIMER_ELAPSE_READIMAGE       (250/5)


typedef struct _THREAD_PARA {
	HANDLE*          phExit;
	CFujiSDK*        pSDK;
	XSDK_HANDLE*     phCam;
	APP_PROPERTY*    pProperty;
	int*             piState;
} THREAD_PARA, *PTHREAD_PARA;


// CMainDlg dialog
class CMainDlg : public CDialog
{
// Construction
public:
	CMainDlg(CWnd* pParent = NULL);	// standard constructor

// Dialog Data
	enum { IDD = IDD_MAIN };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support


// Implementation
protected:

	// Generated message map functions
	virtual BOOL OnInitDialog();
	virtual BOOL PreTranslateMessage(MSG* pMsg);
	afx_msg void OnDestroy();
	afx_msg LRESULT OnUserPopup(WPARAM wParam, LPARAM lParam);
	afx_msg void OnPopupExit();
	afx_msg void OnPopupFolder();
	afx_msg void OnTimer(UINT_PTR nIDEvent);
	DECLARE_MESSAGE_MAP()

public:
	static int CALLBACK BrowseCallbackProc(HWND hwnd,UINT uMsg,LPARAM lParam,LPARAM lpData);
	static UINT ThreadInit( LPVOID pParam );
	static UINT ThreadReadImage( LPVOID pParam );
	static BOOL IsFile( LPCTSTR pPath );

private:
	NOTIFYICONDATA m_NotifyIconData;
	int m_iState;
	HICON  m_phIcon[ 4 ];
	int m_iIconState;
	CStatic m_StaticSplash;
	CBitmap m_BitmapSplash;

	APP_PROPERTY m_Property;

	void SpecifyFolder();
	void CheckFolder( LPCTSTR pFolder, BOOL* pboErr );



	CFujiSDK*        m_pSDK;
	XSDK_HANDLE      m_hCam;

	THREAD_PARA      m_paraInit;
	HANDLE           m_hExitInit;
	CWinThread*      m_pThreadInit;

	THREAD_PARA      m_paraReadImage;
	HANDLE           m_hExitReadImage;
	CWinThread*      m_pThreadReadImage;

	BOOL             m_boExitRequest;

};


