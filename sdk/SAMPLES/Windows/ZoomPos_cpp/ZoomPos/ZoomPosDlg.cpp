// ZoomPosDlg.cpp : implementation file
//

#include "stdafx.h"
#include "ZoomPos.h"

#include "FujiSDK.h"
#include "CameraControl.h"

#include "ZoomPosDlg.h"
#include "afxdialogex.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif


// CZoomPosDlg dialog

#define OFFSET_LOUPE (60.0)
#define RANGE_LOUPE  (15.0)

#define  ID_TIMER_DETECTING  (0)
#define  ELAPSE_TIMER_DETECTING  (150)
#define  ID_TIMER_POLL  (1)
#define  ELAPSE_TIMER_POLL  (500)

enum{
	STATE_DETECTING,
	STATE_CONNECTED
};


CZoomPosDlg::CZoomPosDlg(CWnd* pParent /*=NULL*/)
	: CDialogEx(CZoomPosDlg::IDD, pParent)
{
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
	m_pCameraControl = NULL;
}

void CZoomPosDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialogEx::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_TITLE_FOCALLENGTH, m_titleFocalLength);
	DDX_Control(pDX, IDC_TITLE_FOCALLENGTH35, m_titleFocalLength35);
	DDX_Control(pDX, IDC_TITLE_COUNT, m_titleCount);
	DDX_Control(pDX, IDC_TEXT_FOCALLENGTH, m_textFocalLength);
	DDX_Control(pDX, IDC_TEXT_FOCALLENGTH35, m_textFocalLength35);
	DDX_Control(pDX, IDC_TEXT_COUNT, m_textCount);
	DDX_Control(pDX, IDC_TEXT_MSG, m_textMsg);
	DDX_Control(pDX, IDC_LOUPE, m_iconLoupe);
}

BEGIN_MESSAGE_MAP(CZoomPosDlg, CDialogEx)
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_WM_TIMER()
	ON_BN_CLICKED(IDCANCEL, &CZoomPosDlg::OnBnClickedCancel)
END_MESSAGE_MAP()


// CZoomPosDlg message handlers

BOOL CZoomPosDlg::OnInitDialog()
{
	CDialogEx::OnInitDialog();

	// Set the icon for this dialog.  The framework does this automatically
	//  when the application's main window is not a dialog
	SetIcon(m_hIcon, TRUE);			// Set big icon
	//SetIcon(m_hIcon, FALSE);		// Set small icon

	// TODO: Add extra initialization here
	m_doAngle = 0.0;


	// Start SDK communication
	m_pCameraControl = new CCameraControl();
	m_pCameraControl->SetDetectOption(XSDK_DSC_IF_USB, NULL);
	m_pCameraControl->BeginConnectThread();

	m_iState = STATE_DETECTING;
	SetUI();

	return TRUE;  // return TRUE  unless you set the focus to a control
}

// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.

void CZoomPosDlg::OnPaint()
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
HCURSOR CZoomPosDlg::OnQueryDragIcon()
{
	return static_cast<HCURSOR>(m_hIcon);
}


void CZoomPosDlg::SetUI()
{
	switch (m_iState){
	case STATE_DETECTING:
		m_titleFocalLength.ShowWindow(SW_HIDE);
		m_titleFocalLength35.ShowWindow(SW_HIDE);
		m_titleCount.ShowWindow(SW_HIDE);

		m_textFocalLength.ShowWindow(SW_HIDE);
		m_textFocalLength35.ShowWindow(SW_HIDE);
		m_textCount.ShowWindow(SW_HIDE);

		m_iconLoupe.ShowWindow(SW_SHOW);
		m_textMsg.ShowWindow(SW_SHOW);

		SetTimer(ID_TIMER_DETECTING, ELAPSE_TIMER_DETECTING, NULL);
		break;
	case STATE_CONNECTED:
		m_titleFocalLength.ShowWindow(SW_SHOW);
		m_titleFocalLength35.ShowWindow(SW_SHOW);
		m_titleCount.ShowWindow(SW_SHOW);

		m_textFocalLength.ShowWindow(SW_SHOW);
		m_textFocalLength35.ShowWindow(SW_SHOW);
		m_textCount.ShowWindow(SW_SHOW);

		m_iconLoupe.ShowWindow(SW_HIDE);
		m_textMsg.ShowWindow(SW_HIDE);

		CString s;
		long lFocalLength;
		lFocalLength = m_pCameraControl->GetFocalLength();
		if (lFocalLength > 0){
			s.Format(_T("%d.%02d mm"), (long)(lFocalLength / 100), ((long)(lFocalLength % 100)) );
		}
		else{
			s = _T("- - mm");
		}
		m_textFocalLength.SetWindowText(s);

		lFocalLength = m_pCameraControl->Get35mmFocalLength();
		if (lFocalLength > 0){
			s.Format(_T("%d.%02d mm"), (long)(lFocalLength / 100), ((long)(lFocalLength %100)));
		}
		else{
			s = _T("- - mm");
		}
		m_textFocalLength35.SetWindowText(s);

		s.Format(_T("%d"), m_pCameraControl->GetBusyCount());
		m_textCount.SetWindowText(s);

		break;
	}
}

void CZoomPosDlg::OnTimer(UINT_PTR nIDEvent)
{
	KillTimer(nIDEvent);
	
	switch (nIDEvent){
	case ID_TIMER_DETECTING:
		// User interface
		m_iconLoupe.SetWindowPos(&wndTop, (int)(OFFSET_LOUPE + RANGE_LOUPE * cos(m_doAngle)), (int)(OFFSET_LOUPE + RANGE_LOUPE * sin(m_doAngle)), 0, 0, SWP_NOOWNERZORDER | SWP_NOSIZE);

		RECT rect;
		rect.top = 0;
		rect.left = 0;
		rect.bottom = 64 + (long)(OFFSET_LOUPE + RANGE_LOUPE);
		rect.right = 64 + (long)(OFFSET_LOUPE + RANGE_LOUPE);
		InvalidateRect(&rect);

		m_doAngle += 0.628;
		if (m_doAngle >= 6.28)m_doAngle = 0.0;


		// Connection Check
		if (m_pCameraControl->IsConnectThread()){
			SetTimer(ID_TIMER_DETECTING, ELAPSE_TIMER_DETECTING, NULL);
		}
		else{
			m_pCameraControl->EndConnectThread();
			m_pCameraControl->BeginPollThread();

			m_iState = STATE_CONNECTED;
			SetUI();

			SetTimer(ID_TIMER_POLL, ELAPSE_TIMER_POLL, NULL);
		}
		
		break;

	case ID_TIMER_POLL:
		// Connection Check
		if (m_pCameraControl->IsPollThread()){
			SetUI();
			SetTimer(ID_TIMER_POLL, ELAPSE_TIMER_POLL, NULL);
		}
		else{
			m_pCameraControl->EndPollThread();
			m_pCameraControl->TerminateConnection();

			m_pCameraControl->SetDetectOption(XSDK_DSC_IF_USB, NULL);
			m_pCameraControl->BeginConnectThread();

			m_iState = STATE_DETECTING;
			SetUI();
		}
		break;
	}

	CDialogEx::OnTimer(nIDEvent);
}


void CZoomPosDlg::OnBnClickedCancel()
{
	if (m_pCameraControl != NULL){
		m_pCameraControl->TerminateConnection();
		delete m_pCameraControl;
		m_pCameraControl = NULL;
	}
	CDialogEx::OnCancel();
}
