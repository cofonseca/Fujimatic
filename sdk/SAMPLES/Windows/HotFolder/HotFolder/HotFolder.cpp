
// HotFolder.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"

#include "Property.h"
#include "FujiSDK.h"

#include "HotFolder.h"

#include "HotFolderDlg.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif


// CHotFolderApp

BEGIN_MESSAGE_MAP(CHotFolderApp, CWinAppEx)
	ON_COMMAND(ID_HELP, &CWinApp::OnHelp)
END_MESSAGE_MAP()


// CHotFolderApp construction

CHotFolderApp::CHotFolderApp()
{
	// TODO: add construction code here,
	// Place all significant initialization in InitInstance
}


// The one and only CHotFolderApp object

CHotFolderApp theApp;


// CHotFolderApp initialization

BOOL CHotFolderApp::InitInstance()
{
	// InitCommonControlsEx() is required on Windows XP if an application
	// manifest specifies use of ComCtl32.dll version 6 or later to enable
	// visual styles.  Otherwise, any window creation will fail.
	INITCOMMONCONTROLSEX InitCtrls;
	InitCtrls.dwSize = sizeof(InitCtrls);
	// Set this to include all the common control classes you want to use
	// in your application.
	InitCtrls.dwICC = ICC_WIN95_CLASSES;
	InitCommonControlsEx(&InitCtrls);

	CWinAppEx::InitInstance();

	AfxEnableControlContainer();

	// Standard initialization
	// If you are not using these features and wish to reduce the size
	// of your final executable, you should remove from the following
	// the specific initialization routines you do not need
	// Change the registry key under which our settings are stored
	// TODO: You should modify this string to be something appropriate
	// such as the name of your company or organization
	SetRegistryKey(_T("COM.FUJIFILM.EID"));

	CString	 strMutex;
	HANDLE   hMutex;
	DWORD    dwMutexError;

	strMutex.LoadString( IDS_APP_MUTEX );
	hMutex = ::CreateMutex( NULL, 0, strMutex );
	dwMutexError = ::GetLastError();

	if( dwMutexError == ERROR_ALREADY_EXISTS ) {
		// Do nothing to exit
		CString sMsg;
		CString sTitle;
		sMsg.LoadString( IDS_ERR_MUTEX );
		sTitle.LoadString( IDS_TITLE_INFORMATION );
		::MessageBox( NULL, sMsg, sTitle, MB_ICONINFORMATION | MB_OK );
	} else {
		CoInitialize( NULL );

		CHotFolderDlg dlg;
		m_pMainWnd = &dlg;
		INT_PTR nResponse = dlg.DoModal();
		if (nResponse == IDOK)
		{
			// TODO: Place code here to handle when the dialog is
			//  dismissed with OK
		}
		else if (nResponse == IDCANCEL)
		{
			// TODO: Place code here to handle when the dialog is
			//  dismissed with Cancel
		}
	}

	if( hMutex != NULL ){
		::CloseHandle( hMutex );
	}

	// Since the dialog has been closed, return FALSE so that we exit the
	//  application, rather than start the application's message pump.
	return FALSE;
}

