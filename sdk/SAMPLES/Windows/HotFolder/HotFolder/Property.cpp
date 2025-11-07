// Property.cpp : implementation file
//

#include "stdafx.h"

#include "resource.h"

#include "Property.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CProperty

CProperty::CProperty()
{
}

CProperty::~CProperty()
{
}

void CProperty::Init( PAPP_PROPERTY pProperty )
{
	try{
		::InitializeCriticalSection( &( pProperty->csProperty ) );
		pProperty->boCSEnabled = TRUE;
	} catch(...){
		pProperty->boCSEnabled = FALSE;
	}
}

void CProperty::Exit( PAPP_PROPERTY pProperty )
{
	if( pProperty->boCSEnabled ){
		::DeleteCriticalSection( &( pProperty->csProperty ) );
		pProperty->boCSEnabled = FALSE;
	}
}

void CProperty::Lock( PAPP_PROPERTY pProperty )
{
	if( pProperty->boCSEnabled ){
		::EnterCriticalSection( &( pProperty->csProperty ) );
	}
}

void CProperty::Unlock( PAPP_PROPERTY pProperty )
{
	if( pProperty->boCSEnabled ){
		::LeaveCriticalSection( &( pProperty->csProperty ) );
	}
}

void CProperty::GetDefaultFolder( LPTSTR pFolder )
{
	TCHAR pMyDocument[ MAX_PATH ];
	HMODULE hShell32;
	BOOL	(CALLBACK* pfnSHGetSpecialFolderPath)(HWND,LPTSTR,int,BOOL);
	hShell32 = ::LoadLibrary( _T("shell32.dll") );
#ifdef _UNICODE
	(*(FARPROC*)&pfnSHGetSpecialFolderPath = ::GetProcAddress(hShell32,"SHGetSpecialFolderPathW"));
#else
	(*(FARPROC*)&pfnSHGetSpecialFolderPath = ::GetProcAddress(hShell32,"SHGetSpecialFolderPathA"));
#endif
	pfnSHGetSpecialFolderPath(NULL, pMyDocument,0x0005,FALSE); // My Document
	::FreeLibrary( hShell32 );

	if( ( pMyDocument[ 0 ] == _T('\\') ) && ( pMyDocument[ 1 ] == _T('\\') ) ){
		// My Document is on NETWORK DRIVE
		wsprintf( pFolder, _T("C:\\FUJIFILM") );
	} else {
		// My Document is on LOCAL DRIVE
		if( pMyDocument[ lstrlen( pMyDocument ) - 1 ] == _T('\\') ){
			wsprintf( pFolder, _T("%sFUJIFILM"), pMyDocument );
		} else {
			wsprintf( pFolder, _T("%s\\FUJIFILM"), pMyDocument );
		}
	}
}

void CProperty::ReadRegistory( PAPP_PROPERTY pProperty )
{

	CProperty::Lock( pProperty );

	HKEY hkResult;
	DWORD dwDisposition;
	LONG lResult;

	lResult = ::RegCreateKeyEx( 
		HKEY_CURRENT_USER, 
		APP_PROPERTY_REGISTORY,
		0,
		_T(""),
		REG_OPTION_NON_VOLATILE, 
		KEY_ALL_ACCESS,
		NULL,
		&hkResult,
		&dwDisposition );

	if( lResult != ERROR_SUCCESS ){
		// ERROR
	} else {
		TCHAR pBuf[ MAX_PATH ];
		DWORD dwType;
		DWORD dwSize;
		
		// FOLDER
		dwSize = MAX_PATH;
		if( ERROR_SUCCESS == ::RegQueryValueEx( hkResult, APP_PROPERTY_SAVEFOLDERPATH, NULL, &dwType, (LPBYTE)pBuf, &dwSize ) ){
			wsprintf( pProperty->pDstFolder, _T("%s"), pBuf );
		} else {
			CProperty::GetDefaultFolder( pProperty->pDstFolder );

			CProperty::CreateFolderEx( pProperty->pDstFolder );

			::RegSetValueEx(
				hkResult, 
				APP_PROPERTY_SAVEFOLDERPATH, 
				0, 
				REG_SZ, 
				(const BYTE*)(LPCTSTR)pProperty->pDstFolder,
				sizeof( TCHAR ) * (lstrlen( pProperty->pDstFolder ) + 1 ) );
		}

		::RegCloseKey( hkResult );
	}

	CProperty::Unlock( pProperty );

}



void CProperty::WriteRegistory( PAPP_PROPERTY pProperty )
{
	CProperty::Lock( pProperty );

	HKEY hkResult;
	DWORD dwDisposition;
	LONG lResult;

	lResult = ::RegCreateKeyEx( 
		HKEY_CURRENT_USER, 
		APP_PROPERTY_REGISTORY,
		0,
		_T(""),
		REG_OPTION_NON_VOLATILE, 
		KEY_ALL_ACCESS,
		NULL,
		&hkResult,
		&dwDisposition );

	if( lResult != ERROR_SUCCESS ){
		// ERROR
	} else {

		// FOLDER
		::RegSetValueEx(
			hkResult, 
			APP_PROPERTY_SAVEFOLDERPATH, 
			0, 
			REG_SZ, 
			(const BYTE*)(LPCTSTR)pProperty->pDstFolder,
			sizeof( TCHAR ) * (lstrlen( pProperty->pDstFolder ) + 1 ) );

		::RegCloseKey( hkResult );
	}

	CProperty::Unlock( pProperty );
}




int CProperty::CreateFolderEx( LPCTSTR pFolder )
{
	int iRet;
	CString sFind = pFolder;

	if( sFind.GetLength() == 2 ){
		sFind += _T("\\*.*");
	} else if( sFind.GetLength() == 3 ){
		sFind += _T("*.*");
	} else {
		sFind = pFolder;
	}

	WIN32_FIND_DATA findData;
	HANDLE hFind = ::FindFirstFile( sFind, &findData );
	BOOL  boExists = FALSE;

	if( INVALID_HANDLE_VALUE == hFind ){
		// NOT EXIST
	} else {
		// EXIST
		::FindClose( hFind );

		if( FILE_ATTRIBUTE_DIRECTORY == findData.dwFileAttributes ){
			boExists = TRUE;
		} else {
			// FILE EXISTS
			TCHAR pOrgName[ MAX_PATH ];
			TCHAR pRenName[ MAX_PATH ];

			wsprintf( pOrgName, _T("%s"), pFolder );

			if( pOrgName[ lstrlen( pOrgName ) - 1 ] == _T('\\') ){
				pOrgName[ lstrlen( pOrgName ) - 1 ] = _T('\0');
			}

			int j = 0;
			BOOL boRenamed = FALSE;
			wsprintf( pRenName, _T("%s.BAK"), pOrgName );
			while( boRenamed == FALSE ){
				WIN32_FIND_DATA findDst;
				HANDLE hFindDst = ::FindFirstFile( pRenName, &findDst );
				if( INVALID_HANDLE_VALUE == hFindDst ){
					// Rename
					::MoveFile( pOrgName, pRenName );
					boRenamed = TRUE;
				} else {
					::FindClose( hFindDst );
					j ++;
					wsprintf( pRenName, _T("%s.BAK%d"), pOrgName, j );
				}
			}
		}
	}

	if( boExists == FALSE ){
		HMODULE hShell32;

		int	(CALLBACK* pfnSHCreateDirectoryEx)(HWND,LPCTSTR,const SECURITY_ATTRIBUTES*);
		hShell32 = ::LoadLibrary( _T("shell32.dll") );
#ifdef _UNICODE
		(*(FARPROC*)&pfnSHCreateDirectoryEx = ::GetProcAddress(hShell32,"SHCreateDirectoryExW"));
#else
		(*(FARPROC*)&pfnSHCreateDirectoryEx = ::GetProcAddress(hShell32,"SHCreateDirectoryExA"));
#endif
		iRet = pfnSHCreateDirectoryEx( NULL, pFolder, NULL );
		switch( iRet ){
		case ERROR_FILE_EXISTS:
		case ERROR_ALREADY_EXISTS:
			iRet = ERROR_SUCCESS;
			break;
		default:
			break;
		}

		::FreeLibrary( hShell32 );
	} else {
		iRet = ERROR_SUCCESS;
	}

	return iRet;
}

