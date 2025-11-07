
// ZoomPos.h : main header file for the PROJECT_NAME application
//

#pragma once

#ifndef __AFXWIN_H__
	#error "include 'stdafx.h' before including this file for PCH"
#endif

#include "resource.h"		// main symbols


// CZoomPosApp:
// See ZoomPos.cpp for the implementation of this class
//

class CZoomPosApp : public CWinApp
{
public:
	CZoomPosApp();

// Overrides
public:
	virtual BOOL InitInstance();

// Implementation

	DECLARE_MESSAGE_MAP()
};

extern CZoomPosApp theApp;