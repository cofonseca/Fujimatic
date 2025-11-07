
// ZoomPosDlg.h : header file
//

#pragma once
#include "afxwin.h"


// CZoomPosDlg dialog
class CZoomPosDlg : public CDialogEx
{
// Construction
public:
	CZoomPosDlg(CWnd* pParent = NULL);	// standard constructor

// Dialog Data
	enum { IDD = IDD_ZOOMPOS_DIALOG };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support


// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	virtual BOOL OnInitDialog();
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	DECLARE_MESSAGE_MAP()
private:
	CStatic m_titleFocalLength;
	CStatic m_titleFocalLength35;
	CStatic m_titleCount;
	CStatic m_textFocalLength;
	CStatic m_textFocalLength35;
	CStatic m_textCount;
	CStatic m_textMsg;
	CStatic m_iconLoupe;

	int m_iState;
	double m_doAngle;

	CCameraControl* m_pCameraControl;

	void SetUI();
public:
	afx_msg void OnTimer(UINT_PTR nIDEvent);
	afx_msg void OnBnClickedCancel();
};
