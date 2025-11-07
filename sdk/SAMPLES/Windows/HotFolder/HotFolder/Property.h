#if !defined(AFX_PROPERTY_H__DD2AD040_D64D_41E4_9FDC_0B06949CD641__INCLUDED_)
#define AFX_PROPERTY_H__DD2AD040_D64D_41E4_9FDC_0B06949CD641__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000
// Property.h : header file
//


#define APP_PROPERTY_REGISTORY         (_T("Software\\COM.FUJIFILM.EID\\HotFolder"))
#define APP_PROPERTY_SAVEFOLDERPATH    (_T("SaveFolderPath"))

typedef struct _APP_PROPERTY {
	TCHAR   pDstFolder[ MAX_PATH ];
	CRITICAL_SECTION   csProperty;
	BOOL               boCSEnabled;
} APP_PROPERTY, *PAPP_PROPERTY;


/////////////////////////////////////////////////////////////////////////////
// CProperty


class CProperty
{
// Construction
public:
	CProperty();
	virtual ~CProperty();

	static void Init( PAPP_PROPERTY pProperty );
	static void Exit( PAPP_PROPERTY pProperty );
	static void Lock( PAPP_PROPERTY pProperty );
	static void Unlock( PAPP_PROPERTY pProperty );
	static void ReadRegistory( PAPP_PROPERTY pProperty );
	static void WriteRegistory( PAPP_PROPERTY pProperty );
	static void GetDefaultFolder( LPTSTR pFolder );
	static int CreateFolderEx( LPCTSTR pFolder );

protected:

};

#endif // !defined(AFX_PROPERTY_H__DD2AD040_D64D_41E4_9FDC_0B06949CD641__INCLUDED_)
