using System;

namespace ZoomPos
{
	/// <summary>
	/// Camera information and flags for subthread control
	/// </summary>
	internal struct CAMERA_CONTROL_THREAD_PARAM
    {
        public IntPtr hCam;

        public bool boCapabilityTether;
        public bool boCapabilityBackupRestore;

        public bool phEventExitConnect;
        public bool phEventExitPoll;

        public int lInterface;
        public string ptchInterface;

        public int lFocalLength;
        public int l35mmFocalLength;
        public int lBusyCount; 
    }
}
