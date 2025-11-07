
// MainDlg.cpp : implementation file
//

#include "stdafx.h"

#include "Property.h"
#include "FujiSDK.h"

#include "HotFolder.h"

#include "MainDlg.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

// CMainDlg dialog
#define WM_USER_POPUP	            ( WM_USER + 101 )




CMainDlg::CMainDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CMainDlg::IDD, pParent)
{
	m_phIcon[ ICON_OFF ] = AfxGetApp()->LoadIcon( IDI_OFF );
	m_phIcon[ ICON_ON  ] = AfxGetApp()->LoadIcon( IDI_ON );
	m_phIcon[ ICON_ACT ] = AfxGetApp()->LoadIcon( IDI_ACT );
	m_phIcon[ ICON_ERR ] = AfxGetApp()->LoadIcon( IDI_ERR );

	m_BitmapSplash.LoadBitmap( IDB_SPLASH );


	m_pSDK = NULL;
	m_hCam = NULL;

	m_hExitInit = NULL;
	m_pThreadInit = NULL;

	m_hExitReadImage = NULL;
	m_pThreadReadImage = NULL;

	m_boExitRequest = FALSE;
}

void CMainDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_STATIC_SPLASH, m_StaticSplash);
}

BEGIN_MESSAGE_MAP(CMainDlg, CDialog)
	ON_MESSAGE(WM_USER_POPUP, OnUserPopup)
	ON_WM_DESTROY()
	ON_WM_TIMER()
	ON_COMMAND(ID_POPUP_EXIT, &CMainDlg::OnPopupExit)
	ON_COMMAND(ID_POPUP_FOLDER, &CMainDlg::OnPopupFolder)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()


// CHotFolderDlg message handlers

BOOL CMainDlg::OnInitDialog()
{
	CDialog::OnInitDialog();

	CString sTitle;
	CString sMsg;

	// Load SDK
	BOOL boLoadSDKErr = FALSE;
	try{
		m_pSDK = new CFujiSDK( NULL, _T("LIBRARY") );
	} catch(...) {
		m_pSDK = NULL;
	}
	if( m_pSDK == NULL ){
		boLoadSDKErr = TRUE;
	} else {
		if( FUJI_SDK_ERR_OK == m_pSDK->GetError() ){
			// OK
		} else {
			boLoadSDKErr = TRUE;
			delete m_pSDK;
			m_pSDK = NULL;
		}
	}
	if( boLoadSDKErr ){
		sMsg.LoadString( IDS_ERR_SDK );
		sTitle.LoadString( IDS_TITLE_INFORMATION );
		::MessageBox( NULL, sMsg, sTitle, MB_OK | MB_ICONINFORMATION );
		EndDialog( IDCANCEL );
		return TRUE;
	}

	// Read settings from the registory
	CProperty::Init( &m_Property );
	CProperty::Lock( &m_Property );
	CProperty::ReadRegistory( &m_Property );
	CProperty::Unlock(  &m_Property );

	// Check the specified save-to folder
	BOOL boFolderAccessError;
	CheckFolder( m_Property.pDstFolder, &boFolderAccessError );
	if( boFolderAccessError ){
		sMsg.LoadString( IDS_ERR_FOLDER );
		sTitle.LoadString( IDS_TITLE_INFORMATION );
		::MessageBox( NULL, sMsg, sTitle, MB_OK | MB_ICONINFORMATION );
		SpecifyFolder();
	}

	// Initialize the splash dialog
	BITMAP bm;
	m_BitmapSplash.GetObject (sizeof(BITMAP), &bm );
	int iPrimaryW = GetSystemMetrics( SM_CXMAXIMIZED );
	int iPrimaryH = GetSystemMetrics( SM_CYMAXIMIZED );
	SetWindowPos( &wndTop, ( iPrimaryW - bm.bmWidth ) / 2, ( iPrimaryH - bm.bmHeight ) / 2, bm.bmWidth, bm.bmHeight, SWP_NOZORDER | SWP_SHOWWINDOW );
	m_StaticSplash.SetBitmap( (HBITMAP)m_BitmapSplash );

	// Initialize the state
	m_iState = STATE_DISCONNECT;
	
	// Set the task-tray icon
	m_NotifyIconData.cbSize = sizeof(NOTIFYICONDATA);
	m_NotifyIconData.uID = 0;
	m_NotifyIconData.hWnd = m_hWnd;
	m_NotifyIconData.uFlags = NIF_ICON | NIF_MESSAGE | NIF_TIP;
	m_NotifyIconData.hIcon = m_phIcon[ ICON_OFF ]; 
	m_NotifyIconData.uCallbackMessage = WM_USER_POPUP;
	sTitle.LoadString( IDS_TITLE );
	lstrcpy( m_NotifyIconData.szTip, sTitle );
	::Shell_NotifyIcon( NIM_ADD, &m_NotifyIconData );

	// Display the splash dialog 
	SetTimer( TIMER_ID_SPLASH, TIMER_ELAPSE_SPLASH, NULL );

	// Start camera connection thread
	SetTimer( TIMER_ID_INIT, TIMER_ELAPSE_INIT, NULL );

	// Start task-tray icon modification timer
	m_iIconState = 0;
	SetTimer( TIMER_ID_ICON, TIMER_ELAPSE_ICON, NULL );

	return TRUE;  // return TRUE  unless you set the focus to a control
}



void CMainDlg::OnDestroy()
{
	CDialog::OnDestroy();
	::Shell_NotifyIcon( NIM_DELETE, &m_NotifyIconData );
}


afx_msg LRESULT CMainDlg::OnUserPopup(WPARAM wParam, LPARAM lParam)
{
	POINT point;
	::GetCursorPos(&point);

	CMenu* pPopup;
	CMenu menu;

	switch(lParam) {
	case WM_RBUTTONDOWN:
	case WM_LBUTTONDOWN:
		SetForegroundWindow();
		VERIFY(menu.LoadMenu( IDR_MENU_POPUP ));
		pPopup = menu.GetSubMenu(0);
		ASSERT(pPopup != NULL);
		pPopup->TrackPopupMenu(TPM_LEFTALIGN | TPM_RIGHTBUTTON, point.x, point.y, this);
		PostMessage(WM_NULL);
		break;
	case WM_LBUTTONDBLCLK:
		if ( IsIconic() ) {
			ShowWindow( SW_RESTORE );
			SetTimer( TIMER_ID_SPLASH, TIMER_ELAPSE_SPLASH, NULL );
		}
		SetForegroundWindow();
		break;
	default:
		break;
	}

	return 0;
}


void CMainDlg::OnPopupFolder()
{
	SpecifyFolder();
}


void CMainDlg::OnPopupExit()
{
	// EXIT
	CWaitCursor csr;

	// Exitting
	m_boExitRequest = TRUE;
	KillTimer( TIMER_ID_ICON );
	KillTimer( TIMER_ID_INIT );
	KillTimer( TIMER_ID_WAITINIT );
	KillTimer( TIMER_ID_WATCHSESSION );

	// Stop ThreadInit()
	if( m_pThreadInit != NULL ){
		::SetEvent( m_hExitInit );
		while( ::WaitForSingleObject( m_pThreadInit->m_hThread, 0 ) == WAIT_TIMEOUT ){
			Sleep( 200 );
		}

		// Close EXIT event for ThreadInit				
		::CloseHandle( m_hExitInit );
		m_hExitInit = NULL;

		// Delete ThreadInit
		delete m_pThreadInit;
		m_pThreadInit = NULL;

	}

	// Stop ThreadReadImage()
	if( m_pThreadReadImage != NULL ){
		::SetEvent( m_hExitReadImage );
		while( ::WaitForSingleObject( m_pThreadReadImage->m_hThread, 0 ) == WAIT_TIMEOUT ){
			Sleep( 200 );
		}

		// Close EXIT event for ThreadInit				
		::CloseHandle( m_hExitReadImage );
		m_hExitReadImage = NULL;

		// Delete ThreadInit
		delete m_pThreadReadImage;
		m_pThreadReadImage = NULL;

	}

	if( m_hCam != NULL ){
		m_pSDK->m_pfnClose( m_hCam );
		Sleep( 600 );
		m_hCam = NULL;
		m_pSDK->m_pfnExit();
	}


	// UNLOAD SDK
	if( m_pSDK != NULL ){
		delete m_pSDK;
		m_pSDK = NULL;
	}

	// Finalize Registry Access
	CProperty::Exit( &m_Property );

	// Exit
	EndDialog( IDCANCEL );
}



void CMainDlg::CheckFolder( LPCTSTR pFolder, BOOL* pboErr )
{
	BOOL boErr = FALSE;

	CString sPath;
	sPath = pFolder;
	if( sPath.Right( 1 ).Compare( _T("\\") ) == 0 ){
	} else {
		sPath += _T("\\");
	}
	sPath += _T("___test.___");
	HANDLE hTest;
	DWORD dwWritten;
	DWORD dwData = 0;

	hTest = ::CreateFile( sPath,
		GENERIC_WRITE,
		FILE_SHARE_READ,
		NULL,
		CREATE_ALWAYS ,
		FILE_ATTRIBUTE_NORMAL,
		NULL );
	if ( hTest == INVALID_HANDLE_VALUE ){
		//FILE OPEN ERROR
		boErr = TRUE;
	} else {
		if( 0 == ::WriteFile( hTest, &dwData, sizeof( DWORD ), &dwWritten, NULL ) ) boErr = TRUE;
		if( sizeof( DWORD ) != dwWritten )boErr = TRUE;
		::FlushFileBuffers( hTest );
		::CloseHandle( hTest );
		::DeleteFile( sPath );
	}
	*pboErr = boErr;
}


void CMainDlg::SpecifyFolder()
{
	CString  sTitle;
	CString  sMsg;
	sTitle.LoadString( IDS_MSG_BROWSE );

    BROWSEINFO bInfo;
    LPITEMIDLIST pIDList;
    TCHAR ptchDisplayName[ MAX_PATH ];

    
    // BROWSEINFO
    bInfo.lpszTitle             = (LPCTSTR)sTitle;
    bInfo.hwndOwner             = NULL;
    bInfo.pidlRoot              = NULL;
    bInfo.pszDisplayName        = ptchDisplayName;
    bInfo.ulFlags               = BIF_RETURNONLYFSDIRS|0x0040;
    bInfo.lpfn                  = &BrowseCallbackProc;
	bInfo.lParam                = (LPARAM)( (LPCTSTR)( m_Property.pDstFolder ) );
	
    // Browse
	BOOL boErr = FALSE;
	pIDList = NULL;
    pIDList = ::SHBrowseForFolder(&bInfo);
    if(pIDList == NULL){
        // ERROR
		boErr = TRUE;
    }else{
        // ItemIDList
        if(!::SHGetPathFromIDList(pIDList, ptchDisplayName)){
			// ERROR
			boErr = TRUE;
        } else {
			BOOL boFolderAccessError;
			CheckFolder( ptchDisplayName, &boFolderAccessError );
			if( boFolderAccessError ) boErr = TRUE;
        }

        ::CoTaskMemFree( pIDList );

		if( boErr ){
			sMsg.LoadString( IDS_ERR_FOLDER );
			sTitle.LoadString( IDS_TITLE_INFORMATION );
			::MessageBox( NULL, sMsg, sTitle, MB_OK | MB_ICONINFORMATION );

		} else {
			lstrcpy( m_Property.pDstFolder, ptchDisplayName );

			CProperty::Lock( &m_Property );
			CProperty::WriteRegistory( &m_Property );
			CProperty::Unlock( &m_Property );
		}
	}
}


int CALLBACK CMainDlg::BrowseCallbackProc( HWND hwnd, UINT uMsg, LPARAM lParam, LPARAM lpData )
{
    if(uMsg==BFFM_INITIALIZED){
		::SendMessage(hwnd,BFFM_SETSELECTION,(WPARAM)TRUE,lpData);
    }
    return 0;
}




void CMainDlg::OnTimer(UINT_PTR nIDEvent)
{
	switch( nIDEvent ){
	case TIMER_ID_SPLASH:
		KillTimer( TIMER_ID_SPLASH );
		ShowWindow( SW_HIDE );
		ShowWindow( SW_MINIMIZE );
		break;

	case TIMER_ID_INIT:
		KillTimer( TIMER_ID_INIT );

		if( m_hExitInit != NULL ) ::CloseHandle( m_hExitInit );
		m_hExitInit = ::CreateEvent( NULL, TRUE, FALSE, NULL );

		m_paraInit.phExit = &m_hExitInit;
		m_paraInit.pSDK = m_pSDK;
		m_paraInit.phCam = &m_hCam;
		m_paraInit.pProperty = &m_Property; // NOT USED
		m_paraInit.piState = &m_iState; // NOT USED

		m_pThreadInit = AfxBeginThread(
			ThreadInit,
			(LPVOID)( &m_paraInit ),
			THREAD_PRIORITY_NORMAL,
			0,
			CREATE_SUSPENDED );

		if( m_pThreadInit != NULL ){		
			m_pThreadInit->m_bAutoDelete = FALSE;
			m_pThreadInit->ResumeThread();
			SetTimer( TIMER_ID_WAITINIT, TIMER_ELAPSE_WAITINIT, NULL );

		} else {
			::CloseHandle( m_hExitInit );
			m_hExitInit = NULL;
			SetTimer( TIMER_ID_INIT, TIMER_ELAPSE_INIT, NULL );
		}

		break;

	case TIMER_ID_WAITINIT:
		KillTimer( TIMER_ID_WAITINIT );

		// Wait for stopping threads
		if( m_pThreadInit != NULL ){
			if( ( ::WaitForSingleObject( m_pThreadInit->m_hThread, 0 ) != WAIT_TIMEOUT ) ){
				// Close EXIT event for ThreadInit				
				::CloseHandle( m_hExitInit );
				m_hExitInit = NULL;

				// Delete ThreadInit
				delete m_pThreadInit;
				m_pThreadInit = NULL;


				if( m_hCam != NULL ){
					m_iState = STATE_CONNECT;

					if( m_hExitReadImage != NULL ) ::CloseHandle( m_hExitReadImage );
					m_hExitReadImage = ::CreateEvent( NULL, TRUE, FALSE, NULL );

					m_paraReadImage.phExit = &m_hExitReadImage;
					m_paraReadImage.pSDK = m_pSDK;
					m_paraReadImage.phCam = &m_hCam;
					m_paraReadImage.pProperty = &m_Property;
					m_paraReadImage.piState = &m_iState;

					m_pThreadReadImage = AfxBeginThread(
						ThreadReadImage,
						(LPVOID)( &m_paraReadImage ),
						THREAD_PRIORITY_NORMAL,
						0,
						CREATE_SUSPENDED );

					if( m_pThreadReadImage != NULL ){
						m_pThreadReadImage->m_bAutoDelete = FALSE;
						m_pThreadReadImage->ResumeThread();

						SetTimer( TIMER_ID_WATCHSESSION, TIMER_ELAPSE_READIMAGE, NULL );

					} else {
						::CloseHandle( m_hExitReadImage );
						m_hExitReadImage = NULL;

					}

				} else {
					if( m_boExitRequest == FALSE ){
						SetTimer( TIMER_ID_WAITINIT, TIMER_ELAPSE_WAITINIT, NULL );
					}
					
				}
			} else {
				if( m_boExitRequest == FALSE ){
					SetTimer( TIMER_ID_WAITINIT, TIMER_ELAPSE_WAITINIT, NULL );
				}
			}
		} else {
			// UNEXPECTED CASE
			::CloseHandle( m_hExitInit );
			m_hExitInit = NULL;
			if( m_boExitRequest == FALSE ){
				SetTimer( TIMER_ID_INIT, TIMER_ELAPSE_INIT, NULL );
			}
		}
		break;

	case TIMER_ID_ICON:
		switch( m_iState ){
		case STATE_DISCONNECT:
			m_NotifyIconData.hIcon = m_phIcon[ ICON_OFF ];  // OFF
			break;

		case STATE_CONNECT:
			if( m_iIconState == 0 ){
				m_iIconState = 1;
				m_NotifyIconData.hIcon = m_phIcon[ ICON_ON ];  // ON
			} else {
				m_iIconState = 0;
				m_NotifyIconData.hIcon = m_phIcon[ ICON_ACT ];  // ACT
			};
			break;

		case STATE_BUSY:
			if( m_iIconState == 0 ){
				m_iIconState = 1;
				m_NotifyIconData.hIcon = m_phIcon[ ICON_ON ];  // ON
			} else {
				m_iIconState = 0;
				m_NotifyIconData.hIcon = m_phIcon[ ICON_ERR ];  // ERR
			};
			break;
		}
		::Shell_NotifyIcon( NIM_MODIFY, &m_NotifyIconData );
		break;


	case TIMER_ID_WATCHSESSION:
		KillTimer( TIMER_ID_WATCHSESSION );

		if( ( ::WaitForSingleObject( m_pThreadReadImage->m_hThread, 0 ) != WAIT_TIMEOUT ) ){
			// No ThreadReadImage
			::CloseHandle( m_hExitReadImage );
			m_hExitReadImage = NULL;

			delete m_pThreadReadImage;
			m_pThreadReadImage = NULL;

			m_iState = STATE_DISCONNECT;


			if( m_hCam != NULL ){
				m_pSDK->m_pfnClose( m_hCam );
				Sleep( 600 );
				m_hCam = NULL;
				m_pSDK->m_pfnExit();
			}

			if( m_boExitRequest == FALSE ){
				SetTimer( TIMER_ID_INIT, TIMER_ELAPSE_INIT, NULL );
			}
		} else {
			if( m_boExitRequest == FALSE ){
				SetTimer( TIMER_ID_WATCHSESSION, TIMER_ELAPSE_READIMAGE, NULL );
			}
		}
		break;

	default:
		break;
	}

	CDialog::OnTimer(nIDEvent);
}


BOOL CMainDlg::PreTranslateMessage(MSG* pMsg) 
{
	if( WM_KEYDOWN == pMsg->message ){
		if( pMsg->wParam == VK_RETURN || pMsg->wParam == VK_ESCAPE ){			
			return FALSE;
		}
	}
	return CDialog::PreTranslateMessage(pMsg);
}




UINT CMainDlg::ThreadInit( LPVOID pParam )
{
	PTHREAD_PARA  pPara = (PTHREAD_PARA)pParam;
	
	BOOL boBreak = FALSE;
	*( pPara->phCam ) = NULL;

	while( ::WaitForSingleObject( *( pPara->phExit ), 0 ) == WAIT_TIMEOUT ){

		if( XSDK_COMPLETE == pPara->pSDK->m_pfnInit( (LIB_HANDLE)( pPara->pSDK->GetHModule() ) ) ){

			long lNumCameras = 0;

			long lInterface = XSDK_DSC_IF_USB;  // | XSDK_DSC_IF_WIFI_LOCAL | XSDK_DSC_IF_WIFI_IP;

			if( XSDK_COMPLETE == pPara->pSDK->m_pfnDetect( lInterface, NULL, NULL, &lNumCameras ) ){
				if( lNumCameras > 0 ){
					long lCameraMode;
					if (XSDK_COMPLETE == pPara->pSDK->m_pfnOpenEx("ENUM:0", (pPara->phCam), &lCameraMode, NULL)){
						// Open Success
						if (lCameraMode&XSDK_DSC_MODE_TETHER){
							XSDK_DeviceInformation  devInfo;
							if (XSDK_COMPLETE == pPara->pSDK->m_pfnGetDeviceInfo(*(pPara->phCam), &devInfo)){

								// Open OK
								boBreak = TRUE;
							}
							else {

								long lAPICode, lErrCode;

								pPara->pSDK->m_pfnGetErrorNumber(*(pPara->phCam), &lAPICode, &lErrCode);
								if (XSDK_ERRCODE_BUSY == lErrCode || XSDK_ERRCODE_FORCEMODE_BUSY == lErrCode || XSDK_ERRCODE_RUNNING_OTHER_FUNCTION == lErrCode) {
									// ERROR

								}
								else {
									// BUSY
								}

								pPara->pSDK->m_pfnClose(*(pPara->phCam));
								Sleep(600);
								*(pPara->phCam) = NULL;
								pPara->pSDK->m_pfnExit();

							}
						}
						else{
							// Tether shooting is not supported
							pPara->pSDK->m_pfnClose(*(pPara->phCam));
							Sleep(600);
							*(pPara->phCam) = NULL;
							pPara->pSDK->m_pfnExit();
						}

					} else {

						// Open Error
						*( pPara->phCam ) = NULL;
						pPara->pSDK->m_pfnExit();
					}

				} else {
					// No camera detected Error
					*( pPara->phCam ) = NULL;
					pPara->pSDK->m_pfnExit();

				}
			} else {

				// Detect Error
				*( pPara->phCam ) = NULL;
				pPara->pSDK->m_pfnExit();

			}
		} else {
			// Init Error
			*( pPara->phCam ) = NULL;
			pPara->pSDK->m_pfnExit();
			boBreak = TRUE;
		}

		if( boBreak ) break;


		if( ::WaitForSingleObject( *( pPara->phExit ), 0 ) != WAIT_TIMEOUT ) break;
		Sleep( TIMER_ELAPSE_DETECT );
		if( ::WaitForSingleObject( *( pPara->phExit ), 0 ) != WAIT_TIMEOUT ) break;
		Sleep( TIMER_ELAPSE_DETECT );
		if( ::WaitForSingleObject( *( pPara->phExit ), 0 ) != WAIT_TIMEOUT ) break;
		Sleep( TIMER_ELAPSE_DETECT );
		if( ::WaitForSingleObject( *( pPara->phExit ), 0 ) != WAIT_TIMEOUT ) break;
		Sleep( TIMER_ELAPSE_DETECT );
		if( ::WaitForSingleObject( *( pPara->phExit ), 0 ) != WAIT_TIMEOUT ) break;
		Sleep( TIMER_ELAPSE_DETECT );
	}

	if( boBreak == FALSE ){
		// Escaped by Exit Event
		*( pPara->phCam ) = NULL;

		pPara->pSDK->EnterCriticalSection();
		pPara->pSDK->m_pfnExit();
		pPara->pSDK->LeaveCriticalSection();
	}

	AfxEndThread( 0, FALSE );
	return 0;
}


UINT CMainDlg::ThreadReadImage( LPVOID pParam )
{
	PTHREAD_PARA  pPara = (PTHREAD_PARA)pParam;
	DWORD dwElapse = TIMER_ELAPSE_READIMAGE;
	XSDK_ImageInformation  imgInfo;
	unsigned char* pBuf;

	while( ::WaitForSingleObject( *( pPara->phExit ), 0 ) == WAIT_TIMEOUT ){

		pPara->pSDK->EnterCriticalSection();

		imgInfo.lFormat = XSDK_IMAGEFORMAT_NONE;
		if( XSDK_COMPLETE == pPara->pSDK->m_pfnReadImageInfo( *( pPara->phCam ), &imgInfo ) ){
			long lFormat = XSDK_IMAGEFORMAT_RAW;
			CString sDstFolder = pPara->pProperty->pDstFolder;

			switch ((imgInfo.lFormat & 0x00FF)){
			case XSDK_IMAGEFORMAT_RAW:
				lFormat = XSDK_IMAGEFORMAT_RAW;
				break;
			case XSDK_IMAGEFORMAT_JPEG:
				lFormat = XSDK_IMAGEFORMAT_JPEG;
				break;
			case XSDK_IMAGEFORMAT_HEIF:
				lFormat = XSDK_IMAGEFORMAT_HEIF;
				break;
			}

			switch ((imgInfo.lFormat & 0x00FF)){
			case XSDK_IMAGEFORMAT_RAW:
			case XSDK_IMAGEFORMAT_JPEG:
			case XSDK_IMAGEFORMAT_HEIF:
				try{
					pBuf = new unsigned char [ imgInfo.lDataSize ];
				} catch(...) {
					pBuf = NULL;
				}
				
				if( pBuf != NULL ){
					if( XSDK_COMPLETE == pPara->pSDK->m_pfnReadImage( *( pPara->phCam ), pBuf, imgInfo.lDataSize ) ){
						CString sSaveToFolder = sDstFolder;
						CString sSeq;
						HANDLE hSeq;
						DWORD dwSeqRead;
						DWORD dwSeqTemp;
						DWORD dwSeqWritten;

						if( sSaveToFolder.Right( 1 ).Compare(_T("\\")) != 0 ){
							sSaveToFolder += _T("\\");
						}

						// Check .seq file
						int iSeq = 0;
						DWORD dwSeq;
						sSeq.Format( _T("%s.seq"), sSaveToFolder );

						hSeq = ::CreateFile( sSeq,
							GENERIC_READ,
							FILE_SHARE_READ,
							NULL,
							OPEN_EXISTING,
							FILE_ATTRIBUTE_NORMAL,
							NULL );
						if ( hSeq == INVALID_HANDLE_VALUE ){
							//FILE OPEN ERROR
							hSeq = ::CreateFile( sSeq,
								GENERIC_WRITE,
								FILE_SHARE_READ,
								NULL,
								CREATE_ALWAYS ,
								FILE_ATTRIBUTE_NORMAL,
								NULL );
							if ( hSeq == INVALID_HANDLE_VALUE ){
								//FILE OPEN ERROR
							} else {
								dwSeq = (DWORD)iSeq;
								::WriteFile( hSeq, &dwSeq, sizeof( DWORD ), &dwSeqWritten, NULL );
								::CloseHandle( hSeq );
							}
						} else {
							dwSeqRead = ::GetFileSize( hSeq, &dwSeqTemp );
							if( dwSeqRead == sizeof( DWORD ) ){
								::ReadFile( hSeq, &dwSeq, sizeof( DWORD ), &dwSeqRead, NULL );
								iSeq = (int)dwSeq;
							} else {
								//ERROR
							}
							::CloseHandle( hSeq );
						}

						iSeq ++;
						if( iSeq == 100000000 ) iSeq = 1;
						CString sSaveToName;
						sSaveToName.Format( _T("%08d"), iSeq );


						CString sSaveToPathTemp;
						CString sSaveToPath;
						int iCheck = 0;
						sSaveToPathTemp.Format( _T("%s%s.___"), sSaveToFolder, sSaveToName );
						switch (lFormat){
						case XSDK_IMAGEFORMAT_RAW:
							sSaveToPath.Format(_T("%s%s.RAF"), sSaveToFolder, sSaveToName);
							while (CMainDlg::IsFile(sSaveToPath)){
								iCheck++;
								sSaveToPath.Format(_T("%s%s(%d).RAF"), sSaveToFolder, sSaveToName, iCheck);
							}
							break;
						case XSDK_IMAGEFORMAT_JPEG:
							sSaveToPath.Format(_T("%s%s.JPG"), sSaveToFolder, sSaveToName);
							while (CMainDlg::IsFile(sSaveToPath)){
								iCheck++;
								sSaveToPath.Format(_T("%s%s(%d).JPG"), sSaveToFolder, sSaveToName, iCheck);
							}
							break;
						case XSDK_IMAGEFORMAT_HEIF:
							sSaveToPath.Format(_T("%s%s.HEIC"), sSaveToFolder, sSaveToName);
							while (CMainDlg::IsFile(sSaveToPath)){
								iCheck++;
								sSaveToPath.Format(_T("%s%s(%d).HEIC"), sSaveToFolder, sSaveToName, iCheck);
							}
						}

						HANDLE hImg;
						hImg = ::CreateFile( sSaveToPathTemp,
							GENERIC_WRITE,
							FILE_SHARE_READ,
							NULL,
							CREATE_ALWAYS ,
							FILE_ATTRIBUTE_NORMAL,
							NULL );
						if ( hImg == INVALID_HANDLE_VALUE ){
								// FILE OPEN ERROR

						} else {
							if( 0 == ::WriteFile( hImg, pBuf, imgInfo.lDataSize, &dwSeqWritten, NULL ) ){
								// WriteError

							} else if( imgInfo.lDataSize != dwSeqWritten ){
								// WriteError

							} else {
								// OK
							}
							::FlushFileBuffers( hImg );
							::CloseHandle( hImg );
						}



						if( 0 == ::MoveFileEx( sSaveToPathTemp, sSaveToPath, MOVEFILE_REPLACE_EXISTING | MOVEFILE_WRITE_THROUGH ) ){
							// ERROR to move
						}

						hSeq = ::CreateFile( sSeq,
							GENERIC_WRITE,
							FILE_SHARE_READ,
							NULL,
							CREATE_ALWAYS ,
							FILE_ATTRIBUTE_NORMAL,
							NULL );
						if ( hSeq == INVALID_HANDLE_VALUE ){
							//FILE OPEN ERROR
						} else {
							dwSeq = (DWORD)iSeq;
							::WriteFile( hSeq, &dwSeq, 4, &dwSeqWritten, NULL );
							::FlushFileBuffers( hSeq );
							::CloseHandle( hSeq );
						}



					}

					delete [] pBuf;
					pBuf = NULL;
				} else {
					// Memory error
				}
				break;
			}
		}
		pPara->pSDK->LeaveCriticalSection();


		// Check the camera alive?
		pPara->pSDK->EnterCriticalSection();
		BOOL boAlive = TRUE;
		XSDK_DeviceInformation  devInfo;
		if( XSDK_COMPLETE == pPara->pSDK->m_pfnGetDeviceInfo( *( pPara->phCam ), &devInfo ) ){
			// OK
			*( pPara->piState ) = STATE_CONNECT;
		} else {
			long lAPICode, lErrCode;
			pPara->pSDK->m_pfnGetErrorNumber(*( pPara->phCam ), &lAPICode, &lErrCode );
			if (XSDK_ERRCODE_BUSY == lErrCode || XSDK_ERRCODE_FORCEMODE_BUSY == lErrCode || XSDK_ERRCODE_RUNNING_OTHER_FUNCTION == lErrCode) {
				// ERROR
				boAlive = FALSE;
			} else {
				// BUSY
				*( pPara->piState ) = STATE_BUSY;
			}
		}
		pPara->pSDK->LeaveCriticalSection();

		if( boAlive == FALSE ) break;

		if( ::WaitForSingleObject( *( pPara->phExit ), 0 ) != WAIT_TIMEOUT ) break;
		Sleep( dwElapse );

		if( ::WaitForSingleObject( *( pPara->phExit ), 0 ) != WAIT_TIMEOUT ) break;		
		Sleep( dwElapse );
		
		if( ::WaitForSingleObject( *( pPara->phExit ), 0 ) != WAIT_TIMEOUT ) break;		
		Sleep( dwElapse );
		
		if( ::WaitForSingleObject( *( pPara->phExit ), 0 ) != WAIT_TIMEOUT ) break;
		Sleep( dwElapse );

		if( ::WaitForSingleObject( *( pPara->phExit ), 0 ) != WAIT_TIMEOUT ) break;
		Sleep( dwElapse );
	}

	AfxEndThread( 0, FALSE );
	return 0;

}

BOOL CMainDlg::IsFile( LPCTSTR pPath )
{
	WIN32_FIND_DATA  data;
	HANDLE hFind = ::FindFirstFile( pPath, &data );
	if( hFind == INVALID_HANDLE_VALUE ){
		return FALSE;
	} else {
		::FindClose( hFind );
		return TRUE;
	}
}
