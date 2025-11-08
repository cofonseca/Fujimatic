FUJIFILM X Series/GFX System Digital Camera Control Software Development Kit - Rev. 1. 33.0.0  
 
 136 COMMON API 
CONFIDENTIAL 
 
4.1.9.5.  XSDK_SetShutterSpeed 
Description 
Sets the shutter speed value. 
 
Syntax 
 
Parameters  
hCamera (IN) The camera handle. 
lShutterSpeed (IN) The shutter speed to which the camera will be set.  
See plShutterSpeed of “XSDK_CapShutterSpeed ”. 
1ShutterSpeed is ignored if 1Bulb = 1. 
lBulb (IN) 0: lShutterSpeed is valid for setting the shutter speed 
1: BULB mode. 
 
Return Value  
XSDK_COMPLETE : SUCCESS 
XSDK_ERROR : ERROR 
 
Remarks  
This function can be used in State S3. 
 
See Also  
XSDK_CapShutterSpeed, XSDK_GetShutterSpeed 
 
 
 
  XSDK_APIENTRY XSDK_SetShutterSpeed( 
XSDK_HANDLE   hCamera, 
long        lShutterSpeed, 
long        lBulb 
) 

FUJIFILM X Series/GFX System Digital Camera Control Software Development Kit - Rev. 1. 33.0.0  
 
 137 COMMON API 
CONFIDENTIAL 
 
4.1.9.6.  XSDK_GetShutterSpeed 
Description 
Gets the shutter speed setting.  
Syntax 
 
Parameters  
hCamera (IN) The camera handle. 
plShutterSpeed (OUT) Returns the shutter speed. If BULB is selected for shutter speed, the function 
returns XSDK_SHUTTER_BULB. 
XSDK_SHUTTER_UNKNOWN is returned if the function called during playback 
or while the setup menu is in use.  
See the remarks for XSDK_SS_* for a macro definition.  
Calls to this function while the shutter button is pressed halfway may return shutter 
speeds in increments of 1/10 EV. For information on negative values, refer to the 
header file. 
plBulb (OUT) Receive the shutter speed setting is BULB or not. 
0: Camera not in BULB mode 
1: Camera in BULB mode 
 
Return Value  
XSDK_COMPLETE : SUCCESS 
XSDK_ERROR : ERROR 
 
Remarks  
This function can be used in State S3. 
In exposure modes P (program AE) and A (aperture-priority AE), the function can be used to query the shutt er speed 
calculated automatically by the camera, returning a value calculated based on increments of 1/10 Tv.  
Use the pre-defined macro XSDK_SS_* to help monitor shutter speed.  
The function returns a value for *plShutterSpeed of XSDK_SHUTTER_UNKNOWN for shutt er speeds that are not 
multiples of 1/6 EV. 
 
See Also  
XSDK_CapShutterSpeed, XSDK_SetShutterSpeed XSDK_APIENTRY XSDK_GetShutterSpeed( 
XSDK_HANDLE   hCamera, 
long*        plShutterSpeed, 
long*        plBulb 
) 

FUJIFILM X Series/GFX System Digital Camera Control Software Development Kit - Rev. 1. 33.0.0  
 
 138 COMMON API 
CONFIDENTIAL 
 
 
4.1.9.7.  XSDK_CapExposureBias 
Description 
Queries supported exposure compensations to set. 
The results for some models vary with the exposure mode; set the exposure mode before calling this function. 
 
Syntax 
 
Parameters  
hCamera (IN) The camera handle. 
plNumExposureBias (OUT) Returns the number of supported XSDK_SetExposureBias settings. 
plExposureBias (OUT) If not NULL, plExposureBias will return a list of the XSDK_SetExposureBias 
settings supported.  
Allocate sizeof(long) * (*plNumExposureBias) bytes of space before calling this 
function. 
Value Exposure Comp. Macro definition 
 :  : : 
-150 -5 EV XSDK_EXPOSURE_BIAS_M5P00 
 :  : : 
-90 -3 EV XSDK_EXPOSURE_BIAS_M3P00 
-87 -2.9 EV XSDK_EXPOSURE_BIAS_M2P90 
-85 -2 5/6 EV XSDK_EXPOSURE_BIAS_M2P83 
-84 -2.8 EV XSDK_EXPOSURE_BIAS_M2P80 
-81 -2.7 EV XSDK_EXPOSURE_BIAS_M2P70 
-80 -2 2/3 EV XSDK_EXPOSURE_BIAS_M2P67 
-78 -2.6 XSDK_EXPOSURE_BIAS_M2P60 
-75 -2 1/2 EV XSDK_EXPOSURE_BIAS_M2P50 
-72 -2.4 XSDK_EXPOSURE_BIAS_M2P40 
-70 -2 1/3 EV XSDK_EXPOSURE_BIAS_M2P33 
-69 -2.3 EV XSDK_EXPOSURE_BIAS_M2P30 
-66 -2.2 EV XSDK_EXPOSURE_BIAS_M2P20 
-65 -2 1/6 EV XSDK_EXPOSURE_BIAS_M2P17 
-63 -2.1 EV XSDK_EXPOSURE_BIAS_M2P10 XSDK_APIENTRY XSDK_CapExposureBias( 
XSDK_HANDLE   hCamera, 
long*        plNumExposureBias, 
long*        plExposureBias 
) 

FUJIFILM X Series/GFX System Digital Camera Control Software Development Kit - Rev. 1. 33.0.0  
 
 139 COMMON API 
CONFIDENTIAL 
-60 -2 EV XSDK_EXPOSURE_BIAS_M2P00 
 :  :  : 
0 0 EV XSDK_EXPOSURE_BIAS_0 
 :  : : 
+150 5 EV XSDK_EXPOSURE_BIAS_P5P00 
 :  : : 
 
Value Exposure Step Macro definition 
3 1/10 EV XSDK_EXPOSURE_BIAS_STEP_1_10 
5 1/6 EV XSDK_EXPOSURE_BIAS_STEP_1_6 
10 1/3 EV XSDK_EXPOSURE_BIAS_STEP_1_3 
15 1/2 EV XSDK_EXPOSURE_BIAS_STEP_1_2 
 
 
 
 
Return Value  
XSDK_COMPLETE : SUCCESS 
XSDK_ERROR : ERROR 
 
Remarks  
This function can be used in State S3. 
The results for some models vary with the exposure mode; set the exposure mode before calling this function. 
 
See Also  
XSDK_SetExposureBias, XSDK_GetExposureBias 
  

FUJIFILM X Series/GFX System Digital Camera Control Software Development Kit - Rev. 1. 33.0.0  
 
 140 COMMON API 
CONFIDENTIAL 
 
4.1.9.8.  XSDK_SetExposureBias 
Description 
Sets the exposure compensation value.  
Syntax 
 
Parameters  
hCamera (IN) The camera handle. 
lExposureBias (IN) The value to which exposure compensation will be set.  
See plExposureBias of XSDK_CapExposureBias for information on suppor ted 
values. 
 
Return Value  
XSDK_COMPLETE : SUCCESS 
XSDK_ERROR : ERROR 
 
Remarks  
This function can be used in State S3. 
 
See Also  
XSDK_CapExposureBias, XSDK_GetExposureBias 
  XSDK_APIENTRY XSDK_SetExposureBias( 
XSDK_HANDLE   hCamera, 
long        lExposureBias 
) 

FUJIFILM X Series/GFX System Digital Camera Control Software Development Kit - Rev. 1. 33.0.0  
 
 141 COMMON API 
CONFIDENTIAL 
 
4.1.9.9.  XSDK_GetExposureBias 
Description 
Gets the exposure compensation setting.  
Syntax 
 
Parameters  
hCamera (IN) The camera handle. 
plExposureBias (OUT) Returns the current exposure compensation value.  
See plExposureBias of XSDK_CapExposureBias for information on suppor ted 
values. 
 
Return Value  
XSDK_COMPLETE : SUCCESS 
XSDK_ERROR : ERROR 
 
Remarks  
This function can be used in State S3. 
 
See Also  
XSDK_CapExposureBias, XSDK_SetExposureBias 
 
  XSDK_APIENTRY XSDK_GetExposureBias( 
XSDK_HANDLE   hCamera, 
long*        plExposureBias 
) 

FUJIFILM X Series/GFX System Digital Camera Control Software Development Kit - Rev. 1. 33.0.0  
 
 142 COMMON API 
CONFIDENTIAL 
 
4.1.9.10.  XSDK_CapDynamicRange 
Description 
Queries supported dynamic ranges to set. 
 
Syntax 
 
Parameters  
hCamera (IN) The camera handle. 
plNumDynamicRange (IN) Returns the number of supported XSDK_SetDynamicRange settings. 
plDynamicRange (IN) If not NULL, plDynamicRange will return a list of the 
XSDK_SetDynamicRange settings supported.  
Allocate sizeof(long) * (*plNumDynamicRange) bytes of space before calling 
this function. 
0xffff XSDK_DRANGE_AUTO  Dynamic range AUTO 
Other values  Dynamic range in % 
 
 
 
 
Return Value  
XSDK_COMPLETE : SUCCESS 
XSDK_ERROR : ERROR 
 
Remarks  
This function can be used in State S3. 
 
See Also  
XSDK_CapSensitivity, XSDK_SetSensitivity, XSDK_GetSensitivity, XSDK _SetDynamicRange, 
XSDK_GetDynamicRange 
  XSDK_APIENTRY XSDK_CapSensitivityDR( 
XSDK_HANDLE   hCamera, 
long*             plNumDynamicRange, 
long*             plDynamicRange, 
); 

FUJIFILM X Series/GFX System Digital Camera Control Software Development Kit - Rev. 1. 33.0.0  
 
 143 COMMON API 
CONFIDENTIAL 
 
Sample  
long lNumDynamicRange; 
long* plDynamicRange; 
XSDK_CapDynamicRange  ( hCam, &lNumDynamicRange, NULL ); 
plDynamicRange = new long [lNumDynamicRange]; 
XSDK_CapDynamicRange  ( hCam, & lNumDynamicRange, plDynamicRange ); 
: 
delete []plDynamicRange; 
 
  