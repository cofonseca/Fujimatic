// XFileName.cpp
// 
// Important Notice:
//    - This source code is written only for little endian system
//    - This source code is not error free
//    - This source code is provided in AS IS BASIS


#include <string.h>
#include "XFileName.h"


CXFileName::CXFileName(u_char* pucImage, u_long32 ul32ImageSize)
{
	m_pcPathOnMedia[0] = '\0';
	m_boFileSequentialNumber = false;
	m_boJPG = false;
	m_boRAF = false;
	m_boHEIF = false;

	// Parsing
	u_char*   pucExif;
	u_long32  ulSize;
	u_short*  pusApp1Length;
	m_iErr = XFILENAME_ERROR_OK;

	// Check file type
	if(    pucImage[0] == 0xFF 
	    && pucImage[1] == 0xD8
	    && pucImage[2] == 0xFF
	    && pucImage[3] == 0xE1
	    && pucImage[6] == 'E'
	    && pucImage[7] == 'x'
	    && pucImage[8] == 'i'
	    && pucImage[9] == 'f'){

		// Exif JPEG
		pucExif = pucImage;

		pusApp1Length = (u_short*)(&pucExif[4]);
		ulSize = (*pusApp1Length) - 6;

		if (ulSize > ul32ImageSize){
			// image size too short
			m_iErr = XFILENAME_ERROR_IMAGESIZE;
		}
		else{
			pucExif += 12;
			ulSize -= 12;
			m_boJPG = true;
		}
	}
	else if( pucImage[0] == 'F'
	    && pucImage[1] == 'U'
	    && pucImage[2] == 'J'
	    && pucImage[3] == 'I'
	    && pucImage[8] == 'C'
	    && pucImage[9] == 'C'
	    && pucImage[10] == 'D'
	    && pucImage[11] == '-'
	    && pucImage[12] == 'R'
	    && pucImage[13] == 'A'
	    && pucImage[14] == 'W'){

		// RAF
		u_long32 ulPreviewOffset = pucImage[0x54];
		ulPreviewOffset <<= 8;
		ulPreviewOffset |= pucImage[0x55];
		ulPreviewOffset <<= 8;
		ulPreviewOffset |= pucImage[0x56];
		ulPreviewOffset <<= 8;
		ulPreviewOffset |= pucImage[0x57];
		pucExif = pucImage;
		pucExif += ulPreviewOffset;

		pusApp1Length = (u_short*)(&pucExif[4]);
		ulSize = (*pusApp1Length) - 6;

		if (ulSize > ul32ImageSize){
			// image size too short
			m_iErr = XFILENAME_ERROR_IMAGESIZE;
		}
		else{
			pucExif += 12;
			ulSize -= 12;
			m_boRAF = true;
		}
	}
	else if( pucImage[4] == 'f'
	    && pucImage[5] == 't'
	    && pucImage[6] == 'y'
	    && pucImage[7] == 'p'
	    && pucImage[8] == 'h'
	    && pucImage[9] == 'e'
	    && pucImage[10] == 'i'
	    && pucImage[11] == 'x'){

		// HEIF
		u_long32 boxSize;
		u_long32 metaBoxSize;
		u_long32 metaInex;
		char boxName[4] = {};
		u_char exifItemId[2] = {};
		u_char *pTmp;
		u_long32 ilocItemCount;
		u_long32 exifOffset;

		pucExif = (u_char *)0;
		m_iErr = XFILENAME_ERROR_IMAGEFORMAT;
		boxSize = (pucImage[0] << 24) + (pucImage[1] << 16) + (pucImage[2] << 8) + pucImage[3];
		if (boxSize < ul32ImageSize){
			pTmp = &pucImage[boxSize];	// pTmp is ftyp box's next box.
			if(pTmp[4] == 'm'
				&& pTmp[5] == 'e'
				&& pTmp[6] == 't'
				&& pTmp[7] == 'a'){
				// Find meta Box
				// Code is based on meta box version 0.
				metaBoxSize = (pTmp[0] << 24) + (pTmp[1] << 16) + (pTmp[2] << 8) + pTmp[3];
				if (metaBoxSize < ul32ImageSize){
					pTmp += 12;	// pTmp is first box inside meta box. (12Byte = BoxSize(4Byte) + BoxName(4Byte) + BoxData(4Byte))
					metaInex = 0;
					while (metaInex < metaBoxSize){
						boxSize = (pTmp[0] << 24) + (pTmp[1] << 16) + (pTmp[2] << 8) + pTmp[3];
						memcpy(boxName, pTmp + 4, 4);
						if (strncmp(boxName, "iinf", 4) == 0) {
							// Code is based on iinf box version 0.
							metaInex += 14;
							pTmp += 14;// pTmp is first box inside iinf box. (14Byte = BoxSize(4Byte) + BoxName(4Byte) + BoxData(6Byte))
						} else if (strncmp(boxName, "infe", 4) == 0) {
							if (pTmp[16] == 'E'
								&& pTmp[17] == 'x'
								&& pTmp[18] == 'i'
								&& pTmp[19] == 'f'
								&& pTmp[20] == '\0') {
								// Find Exif's infe box inside iinf box.
								// Code is based on Exif's infe box version 2.
								exifItemId[0] = pTmp[12];
								exifItemId[1] = pTmp[13];
							}
							metaInex += boxSize;
							pTmp += boxSize;	// pTmp is next box.
						} else if (strncmp(boxName, "iloc", 4) == 0) {
							// Find iloc box inside meta box.
							// Code is based on iloc box version 1.
							ilocItemCount = (pTmp[14] << 8) + pTmp[15];
							metaInex += 16;
							pTmp += 16;
							for (u_long32 j = 0; j < ilocItemCount; j++, pTmp += 16, metaInex += 16) {
								if (metaBoxSize <= metaInex) {
									break;	// ERROR: over meta box.
								}
								if (pTmp[0] == exifItemId[0] && pTmp[1] == exifItemId[1]) {
									// Find Exif's iloc item.
									exifOffset = (pTmp[8] << 24) + (pTmp[9] << 16) + (pTmp[10] << 8) + pTmp[11];
									pucExif = pucImage + exifOffset;
									pucExif += 10;
									m_boHEIF = true;
									m_iErr = XFILENAME_ERROR_OK;
									break;
								}
							}
							if (pucExif != nullptr) {
								break;	// Completed HEIF analyze.
							}
						} else {
							metaInex += boxSize;
							pTmp += boxSize;	// pTmp is next box.
						}
					}
				}
			}
		}

	}
	else{
		// unknown image format
		pucExif = (u_char*)0;
		m_iErr = XFILENAME_ERROR_IMAGEFORMAT;
	}


	if (m_iErr == XFILENAME_ERROR_OK){
		// The 0th IFD of all X/GFX series cameras is written in little endian (II)
		u_char*    pucPtr;
		u_short*   pusEntryCount;
		u_short    usCount;
		u_short*   pusTag;
		u_long32*  pulValue;

		pusEntryCount = (u_short*)(&pucExif[8]);
		pucPtr = pucExif;
		pucPtr += 10;

		u_long32 ulExifIFDOffset = 0;
		for (usCount = 0; usCount < *pusEntryCount; usCount++){
			pusTag = (u_short*)(pucPtr);
			if (*pusTag == 0x8769){
				// Exif IFD offset
				pulValue = (u_long32*)(pucPtr + 8);
				ulExifIFDOffset = *pulValue;
				break;
			}
			pucPtr += 12;
		}

		if (ulExifIFDOffset == 0){
			m_iErr = XFILENAME_ERROR_IMAGEFORMAT;
		}
		else{
			// Find 0th IFD
			pucPtr = pucExif;
			pucPtr += ulExifIFDOffset;

			pusEntryCount = (u_short*)(pucPtr);
			pucPtr += 2;

			u_long32 ulMakerNoteOffset = 0;
			for (usCount = 0; usCount < *pusEntryCount; usCount++){
				pusTag = (u_short*)(pucPtr);
				if (*pusTag == 0x927C){
					// MakerNote offset
					pulValue = (u_long32*)(pucPtr + 8);
					ulMakerNoteOffset = *pulValue;
					break;
				}
				pucPtr += 12;
			}

			if (ulMakerNoteOffset == 0){
				m_iErr = XFILENAME_ERROR_NOINFORMATION;
			}
			else{
				// Find MakerNote
				pucPtr = pucExif;
				pucPtr += ulMakerNoteOffset;
				u_char* pucMakerNote = pucPtr;

				pucPtr += 12;
				pusEntryCount = (u_short*)(pucPtr);

				pucPtr += 2;

				u_long32 ulFilePathOffset = 0;
				for (usCount = 0; usCount < *pusEntryCount; usCount++){
					pusTag = (u_short*)(pucPtr);
					if (*pusTag == 0x1438){
						// SequentialNumber
						pulValue = (u_long32*)(pucPtr + 8);
						m_ulFileSequentialNumber = *pulValue;
						m_boFileSequentialNumber = true;
					}
					else if (*pusTag == 0x1439){
						// FilePath offset
						pulValue = (u_long32*)(pucPtr + 8);
						ulFilePathOffset = *pulValue;
						break;
					}
					pucPtr += 12;
				}

				if (ulFilePathOffset == 0){
					// NO PathOnMedia information
				}
				else{
					// Find PathOnMedia information
					pucPtr = pucMakerNote;
					pucPtr += ulFilePathOffset;

					if (*pucPtr == 0x00){
						// NO PathOnMedia information
					}
					else{
						strcpy_s(m_pcPathOnMedia, MAX_PATH_DCF, (char*)pucPtr);
					}
				}
			}
		}
	}
}


CXFileName::~CXFileName()
{
}


s_int CXFileName::GetFileSequentialNumber(u_long32* pulFileSequentialNumber)
{
	s_int iErr = XFILENAME_ERROR_OK;

	if (m_iErr == XFILENAME_ERROR_OK){
		if (m_boFileSequentialNumber){
			if (pulFileSequentialNumber != (u_long32*)0){
				*pulFileSequentialNumber = m_ulFileSequentialNumber;
			}
			else{
				iErr = XFILENAME_ERROR_PARAMETER;
			}
		}
		else{
			iErr = XFILENAME_ERROR_NOINFORMATION;
		}
	}
	else{
		iErr = m_iErr;
	}
	return iErr;
}

s_int CXFileName::GetFilePathOnMedia(u_long32 ul32Size, char* pchPath)
{
	s_int iErr = XFILENAME_ERROR_OK;

	if (m_iErr == XFILENAME_ERROR_OK){
		if (m_pcPathOnMedia[0] == '\0'){
			iErr = XFILENAME_ERROR_NOINFORMATION;
		}
		else{
			if (ul32Size >= MAX_PATH_DCF){
				strcpy_s(pchPath, MAX_PATH_DCF, m_pcPathOnMedia);
			}
			else{
				iErr = XFILENAME_ERROR_PARAMETER;
			}
		}
	}
	else{
		iErr = m_iErr;
	}
	return iErr;
}
