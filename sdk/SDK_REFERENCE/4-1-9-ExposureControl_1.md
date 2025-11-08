FUJIFILM X Series/GFX System Digital Camera Control Software Development Kit - Rev. 1. 33.0.0  
 
 127 COMMON API 
CONFIDENTIAL 
 
4.1.9.  Exposure Control 
 
4.1.9.1.  XSDK_CapAEMode 
Description 
Queries supported exposure modes (P/A/S/M) to set. 
 
Syntax 
 
Parameters  
hCamera (IN) The camera handle. 
plNumAEMode (OUT) Returns the number of supported XSDK_SetAEMode settings. 
plAEMode (OUT) If not NULL, plAEMode will return a list of the XSDK_SetAEMode settings 
supported. 
XSDK_AE_OFF 0x0001 Manual mode 
XSDK_AE_APERTURE_PRIORITY 0x0003 Aperture-priority mode 
XSDK_AE_SHUTTER_PRIORITY 0x0004 Shutter-priority mode 
XSDK_AE_PROGRAM  0x0006 Program AE mode 
 
 
 
Return Value  
XSDK_COMPLETE : SUCCESS 
XSDK_ERROR : ERROR 
 
Remarks  
This function can be used in State S3. 
 
See Also  
XSDK_SetAEMode, XSDK_GetAEMode 
  XSDK_APIENTRY XSDK_CapAEMode( 
XSDK_HANDLE   hCamera, 
long*             plNumAEMode, 
long*             plAEMode 
); 

FUJIFILM X Series/GFX System Digital Camera Control Software Development Kit - Rev. 1. 33.0.0  
 
 128 COMMON API 
CONFIDENTIAL 
Sample 
long lNumAEMode; 
long* plAEMode; 
XSDK_CapAEMode ( hCam, & lNumAEMode, NULL ); 
plAEMode = new long [lNumAEMode]; 
XSDK_ CapAEMode ( hCam, &lNumAEMode, plAEMode ); 
: 
delete [] plAEMode; 
 
  

FUJIFILM X Series/GFX System Digital Camera Control Software Development Kit - Rev. 1. 33.0.0  
 
 129 COMMON API 
CONFIDENTIAL 
 
4.1.9.2.  XSDK_SetAEMode 
Description 
Sets the exposure mode setting. 
 
Syntax 
 
Parameters  
hCamera (IN) The camera handle. 
lAEMode (IN) The exposure mode to which the camera will be set. 
See plAEMode of “XSDK_CapAEMode ”. 
 
Return Value  
XSDK_COMPLETE : SUCCESS 
XSDK_ERROR : ERROR 
 
Remarks  
This function can be used in State S3. 
 
See Also  
XSDK_CapAEMode, XSDK_GetAEMode 
 
  XSDK_APIENTRY XSDK_SetAEMode( 
XSDK_HANDLE   hCamera, 
long              lAEMode, 
) 

FUJIFILM X Series/GFX System Digital Camera Control Software Development Kit - Rev. 1. 33.0.0  
 
 130 COMMON API 
CONFIDENTIAL 
 
4.1.9.3.  XSDK_GetAEMode 
Description 
Gets the exposure mode setting. 
 
Syntax 
 
Parameters  
hCamera (IN) The camera handle. 
plAEMode (OUT) Returns the exposure mode.  
See plAEMode of “XSDK_CapAEMode ”. 
 
Return Value  
XSDK_COMPLETE : SUCCESS 
XSDK_ERROR : ERROR 
 
Remarks  
This function can be used in State S3. 
 
See Also  
XSDK_CapAEMode, XSDK_SetAEMode 
 
 
 
  XSDK_APIENTRY XSDK_GetAEMode( 
XSDK_HANDLE   hCamera, 
long*        plAEMode, 
) 

FUJIFILM X Series/GFX System Digital Camera Control Software Development Kit - Rev. 1. 33.0.0  
 
 131 COMMON API 
CONFIDENTIAL 
 
4.1.9.4.  XSDK_CapShutterSpeed 
Description 
Queries supported shutter speeds to set. 
The results vary with the exposure mode and shutter type (mechanical or electronic); set the ex posure mode and 
shutter type before calling this function. (To set the shutter type via the SD K, only the XSDK_SetBackupSettings is 
available). 
 
Syntax 
 
Parameters  
hCamera (IN) The camera handle. 
plNumShutterSpeed (OUT) Returns the number of supported XSDK_SetShutterSpeed settings. 
plShutterSpeed (OUT) If not NULL, plShutterSpeed will return a list of the XSDK_SetShutterSpeed 
settings supported.  
Allocate sizeof(long) * (*plNumShutterSpeed) bytes of space before calling this 
function. 
Tv SS Value Macro definition 
17 3/6 1/180000 5 XSDK_SHUTTER_1_180000 
17 2/6 1/160000 6 XSDK_SHUTTER_1_160000 
17 1/128000 7 XSDK_SHUTTER_1_128000 
16 4/6 1/102400 9 XSDK_SHUTTER_1_102400 
16 2/6 1/80000 12 XSDK_SHUTTER_1_80000 
16 1/64000 15 XSDK_SHUTTER_1_60000 
15 4/6 1/51200 19 XSDK_SHUTTER_1_50000 
15 2/6 1/40000 24 XSDK_SHUTTER_1_40000 
15 1/32000 30 XSDK_SHUTTER_1_32000 
14 5/6  34  
14 4/6 1/25000 38 XSDK_SHUTTER_1_25600 
14 3/6 1/24000 43 XSDK_SHUTTER_1_24000 
14 2/6 1/20000 48 XSDK_SHUTTER_1_20000 
14 1/6  54  XSDK_APIENTRY XSDK_CapShutterSpeed( 
XSDK_HANDLE   hCamera, 
long*        plNumShutterSpeed, 
long*        plShutterSpeed, 
long*        plBulbCapable 
); 

FUJIFILM X Series/GFX System Digital Camera Control Software Development Kit - Rev. 1. 33.0.0  
 
 132 COMMON API 
CONFIDENTIAL 
14 1/16000 61 XSDK_SHUTTER_1_16000 
13 5/6  68  
13 4/6 1/13000 76 XSDK_SHUTTER_1_12800 
13 3/6 1/12000 86 XSDK_SHUTTER_1_12000 
13 2/6 1/10000 96 XSDK_SHUTTER_1_10000 
13 1/6  108  
13 1/8000 122 XSDK_SHUTTER_1_8000 
12 5/6  137  
12 4/6 1/6400 153 XSDK_SHUTTER_1_6400 
12 3/6 1/6000 172 XSDK_SHUTTER_1_6000 
12 2/6 1/5000 193 XSDK_SHUTTER_1_5000 
12 1/6  217  
12 1/4000 244 XSDK_SHUTTER_1_4000 
11 5/6  274  
11 4/6 1/3200 307 XSDK_SHUTTER_1_3200 
11 3/6 1/3000 345 XSDK_SHUTTER_1_3000 
11 2/6 1/2500 387 XSDK_SHUTTER_1_2500 
11 1/6  435  
11 1/2000 488 XSDK_SHUTTER_1_2000 
10 5/6  548  
10 4/6 1/1600 615 XSDK_SHUTTER_1_1600 
10 3/6 1/1500 690 XSDK_SHUTTER_1_1500 
10 2/6 1/1250 775 XSDK_SHUTTER_1_1250 
10 1/6  870  
10 1/1000 976 XSDK_SHUTTER_1_1000 
9 5/6  1096  
9 4/6 1/800 1230 XSDK_SHUTTER_1_800 
9 3/6 1/750 1381 XSDK_SHUTTER_1_750 
9 2/6 1/640 1550 XSDK_SHUTTER_1_640 
9 1/6  1740  
9 1/500 1953 XSDK_SHUTTER_1_500 
8 5/6  2192  
8 4/6 1/400 2460 XSDK_SHUTTER_1_400 
8 3/6 1/350 2762 XSDK_SHUTTER_1_350 
8 2/6 1/320 3100 XSDK_SHUTTER_1_320 
8 1/6  3480  
8 1/250 3906 XSDK_SHUTTER_1_250 
7 5/6  4384  

FUJIFILM X Series/GFX System Digital Camera Control Software Development Kit - Rev. 1. 33.0.0  
 
 133 COMMON API 
CONFIDENTIAL 
7 4/6 1/200 4921 XSDK_SHUTTER_1_200 
7 3/6 1/180 5524 XSDK_SHUTTER_1_180 
7 2/6 1/160 6200 XSDK_SHUTTER_1_160 
7 1/6  6960  
7 1/125 7812 XSDK_SHUTTER_1_125 
6 5/6  8769  
6 4/6 1/100 9843 XSDK_SHUTTER_1_100 
6 3/6 1/90 11048 XSDK_SHUTTER_1_90 
6 2/6 1/80 12401 XSDK_SHUTTER_1_80 
6 1/6  13920  
6 1/60 15625 XSDK_SHUTTER_1_60 
5 5/6  17538  
5 4/6 1/50 19686 XSDK_SHUTTER_1_50 
5 3/6 1/45 22097 XSDK_SHUTTER_1_45 
5 2/6 1/40 24803 XSDK_SHUTTER_1_40 
5 1/6  27840  
5 1/30 31250 XSDK_SHUTTER_1_30 
4 5/6  35076  
4 4/6 1/25 39372 XSDK_SHUTTER_1_25 
4 3/6 1/20 44194 XSDK_SHUTTER_1_20H 
4 2/6 1/20 49606 XSDK_SHUTTER_1_20 
4 1/6  55681  
4 1/15 62500 XSDK_SHUTTER_1_15 
3 5/6  70153  
3 4/6 1/13 78745 XSDK_SHUTTER_1_13 
3 3/6 1/10 88388 XSDK_SHUTTER_1_10H 
3 2/6 1/10 99212 XSDK_SHUTTER_1_10 
3 1/6  111362  
3 1/8 125000 XSDK_SHUTTER_1_8 
2 5/6  140307  
2 4/6 1/6 157490 XSDK_SHUTTER_1_6 
2 3/6 1/6 176776 XSDK_SHUTTER_1_6H 
2 2/6 1/5 198425 XSDK_SHUTTER_1_5 
2 1/6  222724  
2 1/4 250000 XSDK_SHUTTER_1_4 
1 5/6  280615  
1 4/6 1/3 314980 XSDK_SHUTTER_1_3 
1 3/6 1/3 353553 XSDK_SHUTTER_1_3H 

FUJIFILM X Series/GFX System Digital Camera Control Software Development Kit - Rev. 1. 33.0.0  
 
 134 COMMON API 
CONFIDENTIAL 
1 2/6 1/2.5 396850 XSDK_SHUTTER_1_2P5 
1 1/6  445449  
1 1/2 500000 XSDK_SHUTTER_1_2 
5/6  561231  
4/6 1/1.6 629960 XSDK_SHUTTER_1_1P6 
3/6 1/1.5 707106 XSDK_SHUTTER_1_1P5 
2/6 1/1.3 793700 XSDK_SHUTTER_1_1P3 
1/6  890898  
0 1” 1000000 XSDK_SHUTTER_1 
- 1/6  1122462  
- 2/6 1.3” 1259921 XSDK_SHUTTER_1P3 
- 3/6 1.5” 1414213 XSDK_SHUTTER_1P5 
- 4/6 1.6” 1587401 XSDK_SHUTTER_1P6 
- 5/6  1781797  
-1 2” 2000000 XSDK_SHUTTER_2 
-1 1/6  2244924  
-1 2/6 2.5” 2519842 XSDK_SHUTTER_2P5 
-1 3/6 3” 2828427 XSDK_SHUTTER_3H 
-1 4/6 3” 3174802 XSDK_SHUTTER_3 
-1 5/6  3563594  
-2 4” 4000000 XSDK_SHUTTER_4 
-2 1/6  4489848  
-2 2/6 5” 5039684 XSDK_SHUTTER_5 
-2 3/6 6” 5656854 XSDK_SHUTTER_6H 
-2 4/6 6” 6349604 XSDK_SHUTTER_6 
-2 5/6  7127189  
-3 8” 8000000 XSDK_SHUTTER_8 
-3 1/6  8979696  
-3 2/6 10” 10079368 XSDK_SHUTTER_10 
-3 3/6 10” 11313708 XSDK_SHUTTER_10H 
-3 4/6 13” 12699208 XSDK_SHUTTER_13 
-3 5/6  14254379  
-4 15” 16000000 XSDK_SHUTTER_15 
-4 1/6  17959392  
-4 2/6 20” 20158736 XSDK_SHUTTER_20 
-4 3/6 20” 22627416 XSDK_SHUTTER_20H 
-4 4/6 25” 25398416 XSDK_SHUTTER_25 
-4 5/6  28508758  

FUJIFILM X Series/GFX System Digital Camera Control Software Development Kit - Rev. 1. 33.0.0  
 
 135 COMMON API 
CONFIDENTIAL 
-5 30” 32000000 XSDK_SHUTTER_30 
  0 XSDK_SHUTTER_UNKNOWN 
 BULB -1 XSDK_SHUTTER_BULB 
 
Please refer the XAPI.H for the full list. 
plBulbCapable (OUT) 0: BULB not supported 
1: BULB supported in exposure mode M. 
 
 
Return Value  
XSDK_COMPLETE : SUCCESS 
XSDK_ERROR : ERROR 
 
Remarks  
This function can be used in State S3. 
The results vary with the exposure mode and shutter type (mechanical or electronic); set the ex posure mode and 
shutter type before calling this function. For example, in program AE mode, *plNumShutterSpeed will be zer o. 
 
See Also  
XSDK_SetShutterSpeed, XSDK_GetShutterSpeed 
 
Sample 
long lNumShutterSpeed, lBulbCapable; 
long* plShutterSpeed; 
XSDK_CapShutterSpeed  ( hCam, &lNumShutterSpeed, NULL, &lBulbCapable ); 
plShutterSpeed = new long [ lNumShutterSpeed ]; 
XSDK_CapShutterSpeed  ( hCam, &lNumShutterSpeed, plShutterSpeed, &lBulbCapable ); 
: 
delete [] plShutterSpeed; 
 
 
 
  