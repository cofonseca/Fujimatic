
// ReleaseButtonDlg.h : header file
//

#pragma once
#include "afxwin.h"

enum{
	BUTTON_STATUS_HIDE,
	BUTTON_STATUS_S0,
	BUTTON_STATUS_S1,
	BUTTON_STATUS_S2
};

enum{
	STATE_DETECT,
	STATE_READY,
	STATE_S1,
	STATE_S1_READY,
	STATE_S2,
	STATE_S2_COMP,
	STATE_DISCONNECT
};

// CReleaseButtonDlg dialog
class CReleaseButtonDlg : public CDialogEx
{
// Construction
public:
	CReleaseButtonDlg(CWnd* pParent = NULL);	// standard constructor

// Dialog Data
	enum { IDD = IDD_RELEASEBUTTON_DIALOG };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support

	afx_msg void OnLButtonDown(UINT nFlags, CPoint point);
	virtual BOOL PreTranslateMessage(MSG* pMsg);
	afx_msg void OnTimer(UINT_PTR nIDEvent);
	afx_msg void OnBnClickedCancel();

// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	virtual BOOL OnInitDialog();
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	DECLARE_MESSAGE_MAP()
private:
	CStatic m_iconS0;
	CStatic m_iconS1;
	CStatic m_iconS2;
	void SetButtonStatus(int iButtonStatus);
	CStatic m_textMsg;
	int     m_iState;
	RECT    m_rectWindow;
	RECT    m_rectLoupe;
	BOOL    m_boLoupe;
	int     m_iLoupeAngle;
	void StartLoupe();
	void EndLoupe();
	void SetState(int iState);
	CCameraControl* m_pCamera;
	CWaitCursor* m_pCsr;
	CStatic m_iconLoupe;
	CStatic m_buttonCamera;
	CStatic m_buttonPC;



};
