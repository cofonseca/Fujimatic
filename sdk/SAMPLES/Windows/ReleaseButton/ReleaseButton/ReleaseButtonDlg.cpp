
// ReleaseButtonDlg.cpp : implementation file
//

#include "stdafx.h"

#include "FujiSDK.h"
#include "CameraControl.h"

#include "ReleaseButton.h"
#include "ReleaseButtonDlg.h"
#include "afxdialogex.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif


// CReleaseButtonDlg dialog
enum{
	TIMER_ID_LOUPE,
	TIMER_ID_DETECT,
	TIMER_ID_S1,
	TIMER_ID_S2,
	TIMER_ID_WATCHDOG,
};

enum{
	TIMER_ELAPSE_LOUPE = 150,
	TIMER_ELAPSE_DETECT = 300,
	TIMER_ELAPSE_S1 = 50,
	TIMER_ELAPSE_S2 = 50,
	TIMER_ELAPSE_WATCHDOG = 200,
};

#define LOUPE_R  (10.0)


CReleaseButtonDlg::CReleaseButtonDlg(CWnd* pParent /*=NULL*/)
	: CDialogEx(CReleaseButtonDlg::IDD, pParent)
{
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
	m_pCamera = NULL;
	m_pCsr = NULL;
}

void CReleaseButtonDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialogEx::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_S0, m_iconS0);
	DDX_Control(pDX, IDC_S1, m_iconS1);
	DDX_Control(pDX, IDC_S2, m_iconS2);
	DDX_Control(pDX, IDC_MSG, m_textMsg);
	DDX_Control(pDX, IDC_LOUPE, m_iconLoupe);
	DDX_Control(pDX, IDC_BUTTO_CAMERA, m_buttonCamera);
	DDX_Control(pDX, IDC_BUTTO_PC, m_buttonPC);
}

BEGIN_MESSAGE_MAP(CReleaseButtonDlg, CDialogEx)
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_WM_LBUTTONDOWN()
	ON_WM_TIMER()
	ON_BN_CLICKED(IDCANCEL, &CReleaseButtonDlg::OnBnClickedCancel)
END_MESSAGE_MAP()


// CReleaseButtonDlg message handlers

BOOL CReleaseButtonDlg::OnInitDialog()
{
	CDialogEx::OnInitDialog();

	// Set the icon for this dialog.  The framework does this automatically
	//  when the application's main window is not a dialog
	SetIcon(m_hIcon, TRUE);			// Set big icon
	SetIcon(m_hIcon, FALSE);		// Set small icon

	// TODO: Add extra initialization here
	GetClientRect(&m_rectWindow);

	m_iconLoupe.GetClientRect(&m_rectLoupe);
	m_iconLoupe.ShowWindow(SW_HIDE);

	RECT rectButton;
	m_iconS0.GetClientRect(&rectButton);

	SetButtonStatus(BUTTON_STATUS_HIDE);
	m_iconS0.SetWindowPos(&wndTop, (m_rectWindow.right - rectButton.right) / 2, (m_rectWindow.bottom - rectButton.bottom), 0, 0, SWP_NOZORDER | SWP_NOSIZE);
	m_iconS1.SetWindowPos(&wndTop, (m_rectWindow.right - rectButton.right) / 2, (m_rectWindow.bottom - rectButton.bottom), 0, 0, SWP_NOZORDER | SWP_NOSIZE);
	m_iconS2.SetWindowPos(&wndTop, (m_rectWindow.right - rectButton.right) / 2, (m_rectWindow.bottom - rectButton.bottom), 0, 0, SWP_NOZORDER | SWP_NOSIZE);


	SetState(STATE_DETECT);


	return TRUE;  // return TRUE  unless you set the focus to a control
}

// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.

void CReleaseButtonDlg::OnPaint()
{
	if (IsIconic())
	{
		CPaintDC dc(this); // device context for painting

		SendMessage(WM_ICONERASEBKGND, reinterpret_cast<WPARAM>(dc.GetSafeHdc()), 0);

		// Center icon in client rectangle
		int cxIcon = GetSystemMetrics(SM_CXICON);
		int cyIcon = GetSystemMetrics(SM_CYICON);
		CRect rect;
		GetClientRect(&rect);
		int x = (rect.Width() - cxIcon + 1) / 2;
		int y = (rect.Height() - cyIcon + 1) / 2;

		// Draw the icon
		dc.DrawIcon(x, y, m_hIcon);
	}
	else
	{
		CDialogEx::OnPaint();
	}
}

// The system calls this function to obtain the cursor to display while the user drags
//  the minimized window.
HCURSOR CReleaseButtonDlg::OnQueryDragIcon()
{
	return static_cast<HCURSOR>(m_hIcon);
}

void CReleaseButtonDlg::SetButtonStatus( int iButtonStatus )
{
	switch (iButtonStatus){
	case BUTTON_STATUS_HIDE:
		m_iconS0.ShowWindow(SW_HIDE);
		m_iconS1.ShowWindow(SW_HIDE);
		m_iconS2.ShowWindow(SW_HIDE);

		m_buttonCamera.ShowWindow(SW_HIDE);
		m_buttonPC.ShowWindow(SW_HIDE);
		break;
	case BUTTON_STATUS_S0:
		m_iconS0.ShowWindow(SW_SHOW);
		m_iconS1.ShowWindow(SW_HIDE);
		m_iconS2.ShowWindow(SW_HIDE);

		if (m_pCamera->IsAcceptReleaseInCameraPriorityMode()){
			if (m_pCamera->GetPriorityMode() == XSDK_PRIORITY_CAMERA){
				m_buttonCamera.ShowWindow(SW_SHOW);
				m_buttonPC.ShowWindow(SW_HIDE);
			}
			else{
				m_buttonCamera.ShowWindow(SW_HIDE);
				m_buttonPC.ShowWindow(SW_SHOW);
			}
		}
		else{
			m_buttonCamera.ShowWindow(SW_HIDE);
			m_buttonPC.ShowWindow(SW_HIDE);
		}

		break;
	case BUTTON_STATUS_S1:
		m_iconS0.ShowWindow(SW_HIDE);
		m_iconS1.ShowWindow(SW_SHOW);
		m_iconS2.ShowWindow(SW_HIDE);
		break;
	case BUTTON_STATUS_S2:
		m_iconS0.ShowWindow(SW_HIDE);
		m_iconS1.ShowWindow(SW_HIDE);
		m_iconS2.ShowWindow(SW_SHOW);
		break;
	};
}


void CReleaseButtonDlg::SetState(int iState)
{
	CString s, sFormat;
	m_iState = iState;
	switch (m_iState){
	case STATE_DETECT:
		m_pCamera = new CCameraControl();
		if (FUJI_SDK_ERR_OK != m_pCamera->GetError()){
			// SDK loading error
		}
		
		m_pCamera->SetDetectOption(XSDK_DSC_IF_USB, _T(""));
		m_pCamera->BeginConnectThread();

		s.LoadString(IDS_TITLE_NOCAMERA);
		SetWindowText(s);

		StartLoupe();
		SetButtonStatus(BUTTON_STATUS_HIDE);

		s.LoadString(IDS_LOUPE);
		m_textMsg.SetWindowText(s);

		SetTimer(TIMER_ID_DETECT, TIMER_ELAPSE_DETECT, NULL);
		break;
	case STATE_READY:
		m_pCamera->EndConnectThread();

		m_pCamera->BeginPollThread();
		SetTimer(TIMER_ID_WATCHDOG, TIMER_ELAPSE_WATCHDOG, NULL);

		EndLoupe();
		SetButtonStatus(BUTTON_STATUS_S0);

		s.LoadString(IDS_S0);
		m_textMsg.SetWindowText(s);

		sFormat.LoadString(IDS_TITLE_CONNECTED);
		s.Format(sFormat, m_pCamera->GetModelName(), m_pCamera->GetVersion() / 100, m_pCamera->GetVersion() % 100);
		SetWindowText(s);

		break;
	case STATE_S1:
		m_pCamera->BeginS1Thread();

		SetButtonStatus(BUTTON_STATUS_S1);

		m_textMsg.SetWindowText(_T(""));

		m_pCsr = new CWaitCursor();
		SetTimer(TIMER_ID_S1, TIMER_ELAPSE_S1, NULL);

		break;

	case STATE_S1_READY:
		m_pCamera->EndS1Thread();
		delete m_pCsr;
		m_pCsr = NULL;

		s.LoadString(IDS_S1);
		m_textMsg.SetWindowText(s);

		break;


	case STATE_S2:
		m_pCamera->BeginS2Thread();

		SetButtonStatus(BUTTON_STATUS_S2);

		m_textMsg.SetWindowText(_T(""));

		m_pCsr = new CWaitCursor();
		SetTimer(TIMER_ID_S2, TIMER_ELAPSE_S2, NULL);

		break;

	case STATE_S2_COMP:
		m_pCamera->EndS2Thread();
		delete m_pCsr;
		m_pCsr = NULL;

		break;

	case STATE_DISCONNECT:
		m_pCamera->TerminateConnection();
		delete m_pCamera;
		m_pCamera = NULL;
		break;
	}
}


void CReleaseButtonDlg::OnLButtonDown(UINT nFlags, CPoint point)
{
	ClientToScreen(&point);
	m_buttonCamera.ScreenToClient(&point);

	RECT rc;
	m_buttonCamera.GetClientRect(&rc);

	if ((m_pCamera->IsAcceptReleaseInCameraPriorityMode()) && (m_iState == STATE_READY) && (point.x > rc.left) && (point.x < rc.right) && (point.y > rc.top) && (point.y < rc.bottom)){

		// PC Priority <-> Camera Priority
		long lPriorityMode;
		lPriorityMode = m_pCamera->GetPriorityMode();
		if (lPriorityMode == XSDK_PRIORITY_PC){
			m_pCamera->SetPriorityMode(XSDK_PRIORITY_CAMERA);
		}
		else{
			m_pCamera->SetPriorityMode(XSDK_PRIORITY_PC);
		}

		SetButtonStatus(BUTTON_STATUS_S0);
	}
	else{
		switch (m_iState){
		case STATE_READY:
			SetState(STATE_S1);
			break;
		case STATE_S1_READY:
			SetState(STATE_S2);
			break;
		default:
			break;
		}
	}

	CDialogEx::OnLButtonDown(nFlags, point);
}


BOOL CReleaseButtonDlg::PreTranslateMessage(MSG* pMsg)
{
	if (WM_KEYDOWN == pMsg->message){
		if (pMsg->wParam == VK_RETURN ){
			switch (m_iState){
			case STATE_READY:
				SetState(STATE_S1);
				break;
			case STATE_S1_READY:
				SetState(STATE_S2);
				break;
			default:
				break;
			}
			return FALSE;
		}
		else if (pMsg->wParam == VK_ESCAPE){
			return FALSE;
		}
		else if (pMsg->wParam == VK_TAB){
			pMsg->wParam = VK_ESCAPE;
		}
	}

	return CDialogEx::PreTranslateMessage(pMsg);
}

void CReleaseButtonDlg::StartLoupe()
{
	m_iLoupeAngle = 0;
	m_iconLoupe.ShowWindow(SW_SHOW);
	m_boLoupe = TRUE;
	SetTimer(TIMER_ID_LOUPE, TIMER_ELAPSE_LOUPE, NULL);
}

void CReleaseButtonDlg::EndLoupe()
{
	m_boLoupe = FALSE;
	m_iconLoupe.ShowWindow(SW_HIDE);
}

void CReleaseButtonDlg::OnTimer(UINT_PTR nIDEvent)
{
	KillTimer(nIDEvent);
	switch (nIDEvent){
	case TIMER_ID_LOUPE:
		if (m_boLoupe){
			int iLoupeX, iLoupeY;
			iLoupeX = (m_rectWindow.right - m_rectLoupe.right) / 2;
			iLoupeY = (m_rectWindow.bottom - m_rectLoupe.bottom) / 2;
			iLoupeX -= (int)(LOUPE_R * sin((double)m_iLoupeAngle / 360.0 * 2.0 * 3.14));
			iLoupeY += (int)(LOUPE_R * cos((double)m_iLoupeAngle / 360.0 * 2.0 * 3.14));
			m_iconLoupe.SetWindowPos(&wndTop, iLoupeX, iLoupeY, 0, 0, SWP_NOZORDER | SWP_NOSIZE);
			m_iLoupeAngle += 30;
			if (m_iLoupeAngle == 360)m_iLoupeAngle = 0;
			SetTimer(TIMER_ID_LOUPE, TIMER_ELAPSE_LOUPE, NULL);
		}
		break;
	case TIMER_ID_WATCHDOG:
		if (m_pCamera->IsPollThread()){
			SetTimer(TIMER_ID_WATCHDOG, TIMER_ELAPSE_WATCHDOG, NULL);
		}
		else{
			SetState(STATE_DISCONNECT);
			SetState(STATE_DETECT);
		}
		break;
	case TIMER_ID_DETECT:
		if (m_pCamera->IsConnectThread()){
			SetTimer(TIMER_ID_DETECT, TIMER_ELAPSE_DETECT, NULL);
		}
		else{
			SetState(STATE_READY);
		}
		break;
	case TIMER_ID_S1:
		if (m_pCamera->IsS1Thread()){
			m_pCsr->Restore();
			SetTimer(TIMER_ID_S1, TIMER_ELAPSE_S1, NULL);
		}
		else{
			SetState(STATE_S1_READY);
		}
		break;
	case TIMER_ID_S2:
		if (m_pCamera->IsS2Thread()){
			m_pCsr->Restore();
			SetTimer(TIMER_ID_S2, TIMER_ELAPSE_S2, NULL);
		}
		else{
			SetState(STATE_S2_COMP);
			SetState(STATE_READY);
		}
		break;
	}

	CDialogEx::OnTimer(nIDEvent);
}


void CReleaseButtonDlg::OnBnClickedCancel()
{
	if (m_iState == STATE_S1_READY){
		m_pCamera->TerminateS1();
	}

	if (m_iState == STATE_S2){
		m_pCamera->TerminateS2();
	}

	m_pCamera->TerminateConnection();

	CDialogEx::OnCancel();
}
