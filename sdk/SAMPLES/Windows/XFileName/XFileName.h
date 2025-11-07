#pragma once

// XFileName.h
// 
// Important Notice:
//    - This source code is written only for little endian system
//    - This source code is not error free
//    - This source code is provided in AS IS BASIS

#ifdef WIN32
typedef unsigned char   u_char;
typedef signed char     s_char;
typedef unsigned short  u_short;
typedef unsigned long   u_long32;
typedef signed long     s_long32;
typedef unsigned int    u_int;
typedef signed int      s_int;
#else
typedef unsigned char   u_char;
typedef signed char     s_char;
typedef unsigned short  u_short;
typedef unsigned int    u_long32;
typedef signed int      s_long32;
typedef unsigned int    u_int;
typedef signed int      s_int;
#endif

#define MAX_PATH_DCF  (256)

enum{
	XFILENAME_ERROR_OK = 0,
	XFILENAME_ERROR_PARAMETER = -1,
	XFILENAME_ERROR_IMAGESIZE = -2,
	XFILENAME_ERROR_IMAGEFORMAT = -3,
	XFILENAME_ERROR_NOINFORMATION = -20,
};

class CXFileName
{
public:
	CXFileName(u_char* pucImage, u_long32 ul32ImageSize);
	~CXFileName();

	s_int GetFileSequentialNumber(u_long32* pulFileSequentialNumber);
	s_int GetFilePathOnMedia(u_long32 ul32Size, char* pchPath);
private:
	char      m_pcPathOnMedia[MAX_PATH_DCF];
	u_long32  m_ulFileSequentialNumber;
	bool      m_boFileSequentialNumber;
	bool      m_boJPG;
	bool      m_boRAF;
	bool      m_boHEIF;
	s_int     m_iErr;
};

