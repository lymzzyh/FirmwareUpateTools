using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FirmwareUpateTools.Model
{
    public class STDFUAPI
    {
#region WindowsAPI
        public enum DIGCF
        {
            DIGCF_DEFAULT = 0x00000001, // only valid with DIGCF_DEVICEINTERFACE                 
            DIGCF_PRESENT = 0x00000002,
            DIGCF_ALLCLASSES = 0x00000004,
            DIGCF_PROFILE = 0x00000008,
            DIGCF_DEVICEINTERFACE = 0x00000010
        }
        [DllImport("setupapi.dll", SetLastError = true)]
        private static extern IntPtr SetupDiGetClassDevs(ref Guid ClassGuid, uint Enumerator, IntPtr HwndParent, DIGCF Flags);
        public IntPtr GetClassDevOfHandle(Guid HIDGuid)
        {
            return SetupDiGetClassDevs(ref HIDGuid, 0, IntPtr.Zero, DIGCF.DIGCF_PRESENT | DIGCF.DIGCF_DEVICEINTERFACE);
        }

        public struct SP_DEVICE_INTERFACE_DATA
        {
            public int cbSize;
            public Guid interfaceClassGuid;
            public int flags;
            public int reserved;
        }

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern Boolean SetupDiEnumDeviceInterfaces(IntPtr deviceInfoSet, IntPtr deviceInfoData, ref Guid interfaceClassGuid, UInt32 memberIndex, ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData);

        public bool GetEnumDeviceInterfaces(IntPtr HIDInfoSet, ref Guid HIDGuid, uint index, ref SP_DEVICE_INTERFACE_DATA interfaceInfo)
        {
            return SetupDiEnumDeviceInterfaces(HIDInfoSet, IntPtr.Zero, ref HIDGuid, index, ref interfaceInfo);
        }

        [StructLayout(LayoutKind.Sequential)]
        public class SP_DEVINFO_DATA
        {
            public int cbSize = Marshal.SizeOf(typeof(SP_DEVINFO_DATA));
            public Guid classGuid = Guid.Empty; // temp
            public int devInst = 0; // dumy
            public int reserved = 0;
        }

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr deviceInfoSet, ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData, IntPtr deviceInterfaceDetailData, uint deviceInterfaceDetailDataSize, ref uint requiredSize, SP_DEVINFO_DATA deviceInfoData);
        
        public bool GetDeviceInterfaceDetail(IntPtr deviceInfoSet, ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData, IntPtr deviceInterfaceDetailData, uint deviceInterfaceDetailDataSize, ref uint requiredSize, SP_DEVINFO_DATA deviceInfoData)
        {
            return SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref deviceInterfaceData, deviceInterfaceDetailData, deviceInterfaceDetailDataSize, ref requiredSize, deviceInfoData);
        }

        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        public struct SP_DEVICE_INTERFACE_DETAIL_DATA
        {
            internal int cbSize;
            internal short devicePath;
        }

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern Boolean SetupDiDestroyDeviceInfoList(IntPtr deviceInfoSet);
        public void DestroyDeviceInfoList(IntPtr HIDInfoSet)
        {
            SetupDiDestroyDeviceInfoList(HIDInfoSet);
        }

        /// Return Type: BOOL->int
        ///Property: DWORD->unsigned int
        ///PropertyRegDataType: PDWORD->DWORD*
        ///PropertyBuffer: PBYTE->BYTE*
        ///PropertyBufferSize: DWORD->unsigned int
        ///RequiredSize: PDWORD->DWORD*
        [DllImport("setupapi.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetupDiGetDeviceRegistryProperty(IntPtr lpInfoSet, SP_DEVINFO_DATA DeviceInfoData, UInt32 Property, UInt32 PropertyRegDataType, StringBuilder PropertyBuffer, UInt32 PropertyBufferSize, IntPtr RequiredSize);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetForegroundWindow();

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct DEV_BROADCAST_DEVICEINTERFACE
        {

            /// DWORD->unsigned int
            public uint dbcc_size;

            /// DWORD->unsigned int
            public uint dbcc_devicetype;

            /// DWORD->unsigned int
            public uint dbcc_reserved;

            /// GUID->_GUID
            public Guid dbcc_classguid;

            /// TCHAR[1]
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 1)]
            public string dbcc_name;
        }
        /// Return Type: HDEVNOTIFY->PVOID->void*
        ///hRecipient: HANDLE->void*
        ///NotificationFilter: LPVOID->void*
        ///Flags: DWORD->unsigned int
        [DllImport("user32.dll", EntryPoint = "RegisterDeviceNotification")]
        public static extern IntPtr RegisterDeviceNotification(System.IntPtr hRecipient, IntPtr NotificationFilter, uint Flags);

        public const int DBT_DEVTYP_DEVICEINTERFACE = 5;
        public const int DEVICE_NOTIFY_WINDOW_HANDLE = 0;


        [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct DEV_BROADCAST_HDR
        {

            /// DWORD->unsigned int
            public uint dbch_size;

            /// DWORD->unsigned int
            public uint dbch_devicetype;

            /// DWORD->unsigned int
            public uint dbch_reserved;
        }
        /// DBT_CONFIGCHANGECANCELED -> 0x0019
        public const int DBT_CONFIGCHANGECANCELED = 25;

        /// DBT_CONFIGCHANGED -> 0x0018
        public const int DBT_CONFIGCHANGED = 24;

        /// DBT_CUSTOMEVENT -> 0x8006
        public const int DBT_CUSTOMEVENT = 32774;

        /// DBT_DEVICEARRIVAL -> 0x8000
        public const int DBT_DEVICEARRIVAL = 32768;

        /// DBT_DEVICEQUERYREMOVE -> 0x8001
        public const int DBT_DEVICEQUERYREMOVE = 32769;

        /// DBT_DEVICEQUERYREMOVEFAILED -> 0x8002
        public const int DBT_DEVICEQUERYREMOVEFAILED = 32770;

        /// DBT_DEVICEREMOVECOMPLETE -> 0x8004
        public const int DBT_DEVICEREMOVECOMPLETE = 32772;

        /// DBT_DEVICEREMOVEPENDING -> 0x8003
        public const int DBT_DEVICEREMOVEPENDING = 32771;

        /// DBT_DEVICETYPESPECIFIC -> 0x8005
        public const int DBT_DEVICETYPESPECIFIC = 32773;

        /// DBT_DEVNODES_CHANGED -> 0x0007
        public const int DBT_DEVNODES_CHANGED = 7;

        /// DBT_QUERYCHANGECONFIG -> 0x0017
        public const int DBT_QUERYCHANGECONFIG = 23;

        /// DBT_USERDEFINED -> 0xFFFF
        public const int DBT_USERDEFINED = 65535;


        public const int WM_DEVICECHANGE = 537;

        public const int MAX_PATH = 260;
        #endregion


        #region STDFUAPI
        #region Define
        /// SPDRP_DEVICEDESC -> (0x00000000)
        public const int SPDRP_DEVICEDESC = 0;

        /// SPDRP_HARDWAREID -> (0x00000001)
        public const int SPDRP_HARDWAREID = 1;

        /// SPDRP_COMPATIBLEIDS -> (0x00000002)
        public const int SPDRP_COMPATIBLEIDS = 2;

        /// SPDRP_NTDEVICEPATHS -> (0x00000003)
        public const int SPDRP_NTDEVICEPATHS = 3;

        /// SPDRP_SERVICE -> (0x00000004)
        public const int SPDRP_SERVICE = 4;

        /// SPDRP_CONFIGURATION -> (0x00000005)
        public const int SPDRP_CONFIGURATION = 5;

        /// SPDRP_CONFIGURATIONVECTOR -> (0x00000006)
        public const int SPDRP_CONFIGURATIONVECTOR = 6;

        /// SPDRP_CLASS -> (0x00000007)
        public const int SPDRP_CLASS = 7;

        /// SPDRP_CLASSGUID -> (0x00000008)
        public const int SPDRP_CLASSGUID = 8;

        /// SPDRP_DRIVER -> (0x00000009)
        public const int SPDRP_DRIVER = 9;

        /// SPDRP_CONFIGFLAGS -> (0x0000000A)
        public const int SPDRP_CONFIGFLAGS = 10;

        /// SPDRP_MFG -> (0x0000000B)
        public const int SPDRP_MFG = 11;

        /// SPDRP_FRIENDLYNAME -> (0x0000000C)
        public const int SPDRP_FRIENDLYNAME = 12;

        /// SPDRP_LOCATION_INFORMATION -> (0x0000000D)
        public const int SPDRP_LOCATION_INFORMATION = 13;

        /// SPDRP_PHYSICAL_DEVICE_OBJECT_NAME -> (0x0000000E)
        public const int SPDRP_PHYSICAL_DEVICE_OBJECT_NAME = 14;

        /// SPDRP_CAPABILITIES -> (0x0000000F)
        public const int SPDRP_CAPABILITIES = 15;

        /// SPDRP_UI_NUMBER -> (0x00000010)
        public const int SPDRP_UI_NUMBER = 16;

        /// SPDRP_UPPERFILTERS -> (0x00000011)
        public const int SPDRP_UPPERFILTERS = 17;

        /// SPDRP_LOWERFILTERS -> (0x00000012)
        public const int SPDRP_LOWERFILTERS = 18;

        /// SPDRP_MAXIMUM_PROPERTY -> (0x00000013)
        public const int SPDRP_MAXIMUM_PROPERTY = 19;


        /// STDFU_ERROR_OFFSET -> 0x12340000
        public const int STDFU_ERROR_OFFSET = 305397760;

        /// STDFU_NOERROR -> STDFU_ERROR_OFFSET
        public const int STDFU_NOERROR = STDFU_ERROR_OFFSET;

        /// STDFU_MEMORY -> (STDFU_ERROR_OFFSET+1)
        public const int STDFU_MEMORY = (STDFU_ERROR_OFFSET + 1);

        /// STDFU_BADPARAMETER -> (STDFU_ERROR_OFFSET+2)
        public const int STDFU_BADPARAMETER = (STDFU_ERROR_OFFSET + 2);

        /// STDFU_NOTIMPLEMENTED -> (STDFU_ERROR_OFFSET+3)
        public const int STDFU_NOTIMPLEMENTED = (STDFU_ERROR_OFFSET + 3);

        /// STDFU_ENUMFINISHED -> (STDFU_ERROR_OFFSET+4)
        public const int STDFU_ENUMFINISHED = (STDFU_ERROR_OFFSET + 4);

        /// STDFU_OPENDRIVERERROR -> (STDFU_ERROR_OFFSET+5)
        public const int STDFU_OPENDRIVERERROR = (STDFU_ERROR_OFFSET + 5);

        /// STDFU_ERRORDESCRIPTORBUILDING -> (STDFU_ERROR_OFFSET+6)
        public const int STDFU_ERRORDESCRIPTORBUILDING = (STDFU_ERROR_OFFSET + 6);

        /// STDFU_PIPECREATIONERROR -> (STDFU_ERROR_OFFSET+7)
        public const int STDFU_PIPECREATIONERROR = (STDFU_ERROR_OFFSET + 7);

        /// STDFU_PIPERESETERROR -> (STDFU_ERROR_OFFSET+8)
        public const int STDFU_PIPERESETERROR = (STDFU_ERROR_OFFSET + 8);

        /// STDFU_PIPEABORTERROR -> (STDFU_ERROR_OFFSET+9)
        public const int STDFU_PIPEABORTERROR = (STDFU_ERROR_OFFSET + 9);

        /// STDFU_STRINGDESCRIPTORERROR -> (STDFU_ERROR_OFFSET+0xA)
        public const int STDFU_STRINGDESCRIPTORERROR = (STDFU_ERROR_OFFSET + 10);

        /// STDFU_DRIVERISCLOSED -> (STDFU_ERROR_OFFSET+0xB)
        public const int STDFU_DRIVERISCLOSED = (STDFU_ERROR_OFFSET + 11);

        /// STDFU_VENDOR_RQ_PB -> (STDFU_ERROR_OFFSET+0xC)
        public const int STDFU_VENDOR_RQ_PB = (STDFU_ERROR_OFFSET + 12);

        /// STDFU_ERRORWHILEREADING -> (STDFU_ERROR_OFFSET+0xD)
        public const int STDFU_ERRORWHILEREADING = (STDFU_ERROR_OFFSET + 13);

        /// STDFU_ERRORBEFOREREADING -> (STDFU_ERROR_OFFSET+0xE)
        public const int STDFU_ERRORBEFOREREADING = (STDFU_ERROR_OFFSET + 14);

        /// STDFU_ERRORWHILEWRITING -> (STDFU_ERROR_OFFSET+0xF)
        public const int STDFU_ERRORWHILEWRITING = (STDFU_ERROR_OFFSET + 15);

        /// STDFU_ERRORBEFOREWRITING -> (STDFU_ERROR_OFFSET+0x10)
        public const int STDFU_ERRORBEFOREWRITING = (STDFU_ERROR_OFFSET + 16);

        /// STDFU_DEVICERESETERROR -> (STDFU_ERROR_OFFSET+0x11)
        public const int STDFU_DEVICERESETERROR = (STDFU_ERROR_OFFSET + 17);

        /// STDFU_CANTUSEUNPLUGEVENT -> (STDFU_ERROR_OFFSET+0x12)
        public const int STDFU_CANTUSEUNPLUGEVENT = (STDFU_ERROR_OFFSET + 18);

        /// STDFU_INCORRECTBUFFERSIZE -> (STDFU_ERROR_OFFSET+0x13)
        public const int STDFU_INCORRECTBUFFERSIZE = (STDFU_ERROR_OFFSET + 19);

        /// STDFU_DESCRIPTORNOTFOUND -> (STDFU_ERROR_OFFSET+0x14)
        public const int STDFU_DESCRIPTORNOTFOUND = (STDFU_ERROR_OFFSET + 20);

        /// STDFU_PIPESARECLOSED -> (STDFU_ERROR_OFFSET+0x15)
        public const int STDFU_PIPESARECLOSED = (STDFU_ERROR_OFFSET + 21);

        /// STDFU_PIPESAREOPEN -> (STDFU_ERROR_OFFSET+0x16)
        public const int STDFU_PIPESAREOPEN = (STDFU_ERROR_OFFSET + 22);

        /// STDFU_TIMEOUTWAITINGFORRESET -> (STDFU_ERROR_OFFSET+0x17)
        public const int STDFU_TIMEOUTWAITINGFORRESET = (STDFU_ERROR_OFFSET + 23);

        /// STDFU_RQ_GET_DEVICE_DESCRIPTOR -> 0x02000000
        public const int STDFU_RQ_GET_DEVICE_DESCRIPTOR = 33554432;

        /// STDFU_RQ_GET_DFU_DESCRIPTOR -> 0x03000000
        public const int STDFU_RQ_GET_DFU_DESCRIPTOR = 50331648;

        /// STDFU_RQ_GET_STRING_DESCRIPTOR -> 0x04000000
        public const int STDFU_RQ_GET_STRING_DESCRIPTOR = 67108864;

        /// STDFU_RQ_GET_NB_OF_CONFIGURATIONS -> 0x05000000
        public const int STDFU_RQ_GET_NB_OF_CONFIGURATIONS = 83886080;

        /// STDFU_RQ_GET_CONFIGURATION_DESCRIPTOR -> 0x06000000
        public const int STDFU_RQ_GET_CONFIGURATION_DESCRIPTOR = 100663296;

        /// STDFU_RQ_GET_NB_OF_INTERFACES -> 0x07000000
        public const int STDFU_RQ_GET_NB_OF_INTERFACES = 117440512;

        /// STDFU_RQ_GET_NB_OF_ALTERNATES -> 0x08000000
        public const int STDFU_RQ_GET_NB_OF_ALTERNATES = 134217728;

        /// STDFU_RQ_GET_INTERFACE_DESCRIPTOR -> 0x09000000
        public const int STDFU_RQ_GET_INTERFACE_DESCRIPTOR = 150994944;

        /// STDFU_RQ_OPEN -> 0x0A000000
        public const int STDFU_RQ_OPEN = 167772160;

        /// STDFU_RQ_CLOSE -> 0x0B000000
        public const int STDFU_RQ_CLOSE = 184549376;

        /// STDFU_RQ_DETACH -> 0x0C000000
        public const int STDFU_RQ_DETACH = 201326592;

        /// STDFU_RQ_DOWNLOAD -> 0x0D000000
        public const int STDFU_RQ_DOWNLOAD = 218103808;

        /// STDFU_RQ_UPLOAD -> 0x0E000000
        public const int STDFU_RQ_UPLOAD = 234881024;

        /// STDFU_RQ_GET_STATUS -> 0x0F000000
        public const int STDFU_RQ_GET_STATUS = 251658240;

        /// STDFU_RQ_CLR_STATUS -> 0x10000000
        public const int STDFU_RQ_CLR_STATUS = 268435456;

        /// STDFU_RQ_GET_STATE -> 0x11000000
        public const int STDFU_RQ_GET_STATE = 285212672;

        /// STDFU_RQ_ABORT -> 0x12000000
        public const int STDFU_RQ_ABORT = 301989888;

        /// STDFU_RQ_SELECT_ALTERNATE -> 0x13000000
        public const int STDFU_RQ_SELECT_ALTERNATE = 318767104;

        /// STDFU_RQ_AWAITINGPNPUNPLUGEVENT -> 0x14000000
        public const int STDFU_RQ_AWAITINGPNPUNPLUGEVENT = 335544320;

        /// STDFU_RQ_AWAITINGPNPPLUGEVENT -> 0x15000000
        public const int STDFU_RQ_AWAITINGPNPPLUGEVENT = 352321536;

        /// STDFU_RQ_IDENTIFYINGDEVICE -> 0x16000000
        public const int STDFU_RQ_IDENTIFYINGDEVICE = 369098752;

        /// STATE_IDLE -> 0x00
        public const int STATE_IDLE = 0;

        /// STATE_DETACH -> 0x01
        public const int STATE_DETACH = 1;

        /// STATE_DFU_IDLE -> 0x02
        public const int STATE_DFU_IDLE = 2;

        /// STATE_DFU_DOWNLOAD_SYNC -> 0x03
        public const int STATE_DFU_DOWNLOAD_SYNC = 3;

        /// STATE_DFU_DOWNLOAD_BUSY -> 0x04
        public const int STATE_DFU_DOWNLOAD_BUSY = 4;

        /// STATE_DFU_DOWNLOAD_IDLE -> 0x05
        public const int STATE_DFU_DOWNLOAD_IDLE = 5;

        /// STATE_DFU_MANIFEST_SYNC -> 0x06
        public const int STATE_DFU_MANIFEST_SYNC = 6;

        /// STATE_DFU_MANIFEST -> 0x07
        public const int STATE_DFU_MANIFEST = 7;

        /// STATE_DFU_MANIFEST_WAIT_RESET -> 0x08
        public const int STATE_DFU_MANIFEST_WAIT_RESET = 8;

        /// STATE_DFU_UPLOAD_IDLE -> 0x09
        public const int STATE_DFU_UPLOAD_IDLE = 9;

        /// STATE_DFU_ERROR -> 0x0A
        public const int STATE_DFU_ERROR = 10;

        /// STATE_DFU_UPLOAD_SYNC -> 0x91
        public const int STATE_DFU_UPLOAD_SYNC = 145;

        /// STATE_DFU_UPLOAD_BUSY -> 0x92
        public const int STATE_DFU_UPLOAD_BUSY = 146;

        /// STATUS_OK -> 0x00
        public const int STATUS_OK = 0;

        /// STATUS_errTARGET -> 0x01
        public const int STATUS_errTARGET = 1;

        /// STATUS_errFILE -> 0x02
        public const int STATUS_errFILE = 2;

        /// STATUS_errWRITE -> 0x03
        public const int STATUS_errWRITE = 3;

        /// STATUS_errERASE -> 0x04
        public const int STATUS_errERASE = 4;

        /// STATUS_errCHECK_ERASE -> 0x05
        public const int STATUS_errCHECK_ERASE = 5;

        /// STATUS_errPROG -> 0x06
        public const int STATUS_errPROG = 6;

        /// STATUS_errVERIFY -> 0x07
        public const int STATUS_errVERIFY = 7;

        /// STATUS_errADDRESS -> 0x08
        public const int STATUS_errADDRESS = 8;

        /// STATUS_errNOTDONE -> 0x09
        public const int STATUS_errNOTDONE = 9;

        /// STATUS_errFIRMWARE -> 0x0A
        public const int STATUS_errFIRMWARE = 10;

        /// STATUS_errVENDOR -> 0x0B
        public const int STATUS_errVENDOR = 11;

        /// STATUS_errUSBR -> 0x0C
        public const int STATUS_errUSBR = 12;

        /// STATUS_errPOR -> 0x0D
        public const int STATUS_errPOR = 13;

        /// STATUS_errUNKNOWN -> 0x0E
        public const int STATUS_errUNKNOWN = 14;

        /// STATUS_errSTALLEDPKT -> 0x0F
        public const int STATUS_errSTALLEDPKT = 15;

        /// ATTR_DNLOAD_CAPABLE -> 0x01
        public const int ATTR_DNLOAD_CAPABLE = 1;

        /// ATTR_UPLOAD_CAPABLE -> 0x02
        public const int ATTR_UPLOAD_CAPABLE = 2;

        /// ATTR_MANIFESTATION_TOLERANT -> 0x04
        public const int ATTR_MANIFESTATION_TOLERANT = 4;

        /// ATTR_WILL_DETACH -> 0x08
        public const int ATTR_WILL_DETACH = 8;

        /// ATTR_ST_CAN_ACCELERATE -> 0x80
        public const int ATTR_ST_CAN_ACCELERATE = 128;

        /// OPERATION_DETACH -> 0
        public const int OPERATION_DETACH = 0;

        /// OPERATION_RETURN -> 1
        public const int OPERATION_RETURN = 1;

        /// OPERATION_UPLOAD -> 2
        public const int OPERATION_UPLOAD = 2;

        /// OPERATION_ERASE -> 3
        public const int OPERATION_ERASE = 3;

        /// OPERATION_UPGRADE -> 4
        public const int OPERATION_UPGRADE = 4;

        /// STDFUPRT_ERROR_OFFSET -> (0x12340000+0x5000)
        public const int STDFUPRT_ERROR_OFFSET = (305397760 + 20480);

        /// STDFUPRT_NOERROR -> (0x12340000)
        public const int STDFUPRT_NOERROR = 305397760;

        /// STDFUPRT_UNABLETOLAUNCHDFUTHREAD -> (STDFUPRT_ERROR_OFFSET+0x0001)
        public const int STDFUPRT_UNABLETOLAUNCHDFUTHREAD = (STDFUPRT_ERROR_OFFSET + 1);

        /// STDFUPRT_DFUALREADYRUNNING -> (STDFUPRT_ERROR_OFFSET+0x0007)
        public const int STDFUPRT_DFUALREADYRUNNING = (STDFUPRT_ERROR_OFFSET + 7);

        /// STDFUPRT_BADPARAMETER -> (STDFUPRT_ERROR_OFFSET+0x0008)
        public const int STDFUPRT_BADPARAMETER = (STDFUPRT_ERROR_OFFSET + 8);

        /// STDFUPRT_BADFIRMWARESTATEMACHINE -> (STDFUPRT_ERROR_OFFSET+0x0009)
        public const int STDFUPRT_BADFIRMWARESTATEMACHINE = (STDFUPRT_ERROR_OFFSET + 9);

        /// STDFUPRT_UNEXPECTEDERROR -> (STDFUPRT_ERROR_OFFSET+0x000A)
        public const int STDFUPRT_UNEXPECTEDERROR = (STDFUPRT_ERROR_OFFSET + 10);

        /// STDFUPRT_DFUERROR -> (STDFUPRT_ERROR_OFFSET+0x000B)
        public const int STDFUPRT_DFUERROR = (STDFUPRT_ERROR_OFFSET + 11);

        /// STDFUPRT_RETRYERROR -> (STDFUPRT_ERROR_OFFSET+0x000C)
        public const int STDFUPRT_RETRYERROR = (STDFUPRT_ERROR_OFFSET + 12);

        /// STDFUPRT_UNSUPPORTEDFEATURE -> (STDFUPRT_ERROR_OFFSET+0x000D)
        public const int STDFUPRT_UNSUPPORTEDFEATURE = (STDFUPRT_ERROR_OFFSET + 13);

        /// STDFUFILES_ERROR_OFFSET -> (0x12340000+0x6000)
        public const int STDFUFILES_ERROR_OFFSET = (305397760 + 24576);

        /// STDFUFILES_NOERROR -> (0x12340000+0x0000)
        public const int STDFUFILES_NOERROR = (305397760 + 0);

        /// STDFUFILES_BADSUFFIX -> (STDFUFILES_ERROR_OFFSET+0x0002)
        public const int STDFUFILES_BADSUFFIX = (STDFUAPI.STDFUFILES_ERROR_OFFSET + 2);

        /// STDFUFILES_UNABLETOOPENFILE -> (STDFUFILES_ERROR_OFFSET+0x0003)
        public const int STDFUFILES_UNABLETOOPENFILE = (STDFUAPI.STDFUFILES_ERROR_OFFSET + 3);

        /// STDFUFILES_UNABLETOOPENTEMPFILE -> (STDFUFILES_ERROR_OFFSET+0x0004)
        public const int STDFUFILES_UNABLETOOPENTEMPFILE = (STDFUAPI.STDFUFILES_ERROR_OFFSET + 4);

        /// STDFUFILES_BADFORMAT -> (STDFUFILES_ERROR_OFFSET+0x0005)
        public const int STDFUFILES_BADFORMAT = (STDFUAPI.STDFUFILES_ERROR_OFFSET + 5);

        /// STDFUFILES_BADADDRESSRANGE -> (STDFUFILES_ERROR_OFFSET+0x0006)
        public const int STDFUFILES_BADADDRESSRANGE = (STDFUAPI.STDFUFILES_ERROR_OFFSET + 6);

        /// STDFUFILES_BADPARAMETER -> (STDFUFILES_ERROR_OFFSET+0x0008)
        public const int STDFUFILES_BADPARAMETER = (STDFUAPI.STDFUFILES_ERROR_OFFSET + 8);

        /// STDFUFILES_UNEXPECTEDERROR -> (STDFUFILES_ERROR_OFFSET+0x000A)
        public const int STDFUFILES_UNEXPECTEDERROR = (STDFUAPI.STDFUFILES_ERROR_OFFSET + 10);

        /// STDFUFILES_FILEGENERALERROR -> (STDFUFILES_ERROR_OFFSET+0x000D)
        public const int STDFUFILES_FILEGENERALERROR = (STDFUAPI.STDFUFILES_ERROR_OFFSET + 13);

        #endregion
        [StructLayout(LayoutKind.Sequential,Pack =1)]
        public struct MAPPINGSECTOR
        {

            /// DWORD->unsigned int
            public uint dwStartAddress;

            /// DWORD->unsigned int
            public uint dwAliasedAddress;

            /// DWORD->unsigned int
            public uint dwSectorIndex;

            /// DWORD->unsigned int
            public uint dwSectorSize;

            /// BYTE->unsigned char
            public byte bSectorType;

            /// BOOL->int
            [MarshalAs(UnmanagedType.Bool)]
            public bool UseForOperation;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi,Pack =1)]
        public struct MAPPING
        {

            /// BYTE->unsigned char
            public byte nAlternate;

            /// char[260]
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string Name;

            /// DWORD->unsigned int
            public uint NbSectors;

            MAPPINGSECTOR pSectors;
        }
        [DllImport("STDFUPRT.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint STDFUPRT_DestroyMapping(ref IntPtr ppMapping);

        [DllImport("STDFU.dll", EntryPoint = "STDFU_Open")]
        public static extern uint STDFU_Open([MarshalAs(UnmanagedType.LPStr)] string szDevicePath, ref IntPtr phDevice);

        [StructLayout(LayoutKind.Sequential,Pack =1)]
        public struct USB_DEVICE_DESCRIPTOR
        {

            /// UCHAR->unsigned char
            public byte bLength;

            /// UCHAR->unsigned char
            public byte bDescriptorType;

            /// USHORT->unsigned short
            public ushort bcdUSB;

            /// UCHAR->unsigned char
            public byte bDeviceClass;

            /// UCHAR->unsigned char
            public byte bDeviceSubClass;

            /// UCHAR->unsigned char
            public byte bDeviceProtocol;

            /// UCHAR->unsigned char
            public byte bMaxPacketSize0;

            /// USHORT->unsigned short
            public ushort idVendor;

            /// USHORT->unsigned short
            public ushort idProduct;

            /// USHORT->unsigned short
            public ushort bcdDevice;

            /// UCHAR->unsigned char
            public byte iManufacturer;

            /// UCHAR->unsigned char
            public byte iProduct;

            /// UCHAR->unsigned char
            public byte iSerialNumber;

            /// UCHAR->unsigned char
            public byte bNumConfigurations;
        }

        [DllImport("STDFU.dll", EntryPoint = "STDFU_GetDeviceDescriptor", CallingConvention = CallingConvention.StdCall)]
        public static extern uint STDFU_GetDeviceDescriptor(ref IntPtr phDevice, ref IntPtr pDesc);

        [StructLayout(LayoutKind.Sequential,Pack =1)]
        public struct DFU_FUNCTIONAL_DESCRIPTOR
        {

            /// UCHAR->unsigned char
            public byte bLength;

            /// UCHAR->unsigned char
            public byte bDescriptorType;

            /// UCHAR->unsigned char
            public byte bmAttributes;

            /// USHORT->unsigned short
            public ushort wDetachTimeOut;

            /// USHORT->unsigned short
            public ushort wTransfertSize;

            /// USHORT->unsigned short
            public ushort bcdDFUVersion;
        }
        [DllImport("STDFU.dll", EntryPoint = "STDFU_GetDFUDescriptor", CallingConvention = CallingConvention.StdCall)]
        public static extern uint STDFU_GetDFUDescriptor(ref IntPtr phDevice, ref uint pDFUInterfaceNum, ref uint pNbOfAlternates, IntPtr pDesc);


        [DllImport("STDFUPRT.dll", EntryPoint = "STDFUPRT_CreateMappingFromDevice", CallingConvention = CallingConvention.StdCall)]
        public static extern uint STDFUPRT_CreateMappingFromDevice([MarshalAs(UnmanagedType.LPStr)] string szDevLink, ref IntPtr ppMapping, ref uint pNbAlternates);

        /// Return Type: DWORD->unsigned int
        ///phDevice: PHANDLE->HANDLE*
        [DllImport("STDFU.dll", EntryPoint = "STDFU_Close", CallingConvention = CallingConvention.StdCall)]
        public static extern uint STDFU_Close(ref IntPtr phDevice);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack =1)]
        public struct DFUSTATUS
        {

            /// UCHAR->unsigned char
            public byte bStatus;

            /// UCHAR[3]
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 3)]
            public string bwPollTimeout;

            /// UCHAR->unsigned char
            public byte bState;

            /// UCHAR->unsigned char
            public byte iString;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack =1)]
        public struct DFUThreadContext
        {

            /// GUID->_GUID
            public Guid DfuGUID;

            /// GUID->_GUID
            public Guid AppGUID;

            /// int
            public int Operation;

            /// BOOL->int
            [MarshalAs(UnmanagedType.Bool)]
            public bool bDontSendFFTransfersForUpgrade;

            /// HANDLE->void*
            public IntPtr hImage;

            /// char[260]
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDevLink;

            /// DWORD->unsigned int
            public uint dwTag;

            /// BYTE->unsigned char
            public byte Percent;

            /// WORD->unsigned short
            public ushort wTransferSize;

            /// DFUSTATUS
            public DFUSTATUS LastDFUStatus;

            /// int
            public int CurrentRequest;

            /// UINT->unsigned int
            public uint CurrentNBlock;

            /// UINT->unsigned int
            public uint CurrentLength;

            /// DWORD->unsigned int
            public uint CurrentAddress;

            /// UINT->unsigned int
            public uint CurrentImageElement;

            /// DWORD->unsigned int
            public uint ErrorCode;

            /// HANDLE->void*
            public IntPtr hDevice;
        }


        /// Return Type: DWORD->unsigned int
        ///pHandle: PHANDLE->HANDLE*
        [DllImport("STDFUFiles.dll", EntryPoint = "STDFUFILES_DestroyImage", CallingConvention = CallingConvention.StdCall)]
        public static extern uint STDFUFILES_DestroyImage(ref IntPtr pHandle);

        /// Return Type: DWORD->unsigned int
        ///pHandle: PHANDLE->HANDLE*
        ///pMapping: PHANDLE->HANDLE*
        [DllImport("STDFUFiles.dll", EntryPoint = "STDFUFILES_CreateImageFromMapping", CallingConvention = CallingConvention.StdCall)]
        public static extern uint STDFUFILES_CreateImageFromMapping(ref IntPtr pHandle, IntPtr pMapping);

        /// Return Type: DWORD->unsigned int
        ///Image: HANDLE->void*
        ///Name: PSTR->CHAR*
        [DllImport("STDFUFiles.dll", EntryPoint = "STDFUFILES_SetImageName", CallingConvention = CallingConvention.StdCall)]
        public static extern uint STDFUFILES_SetImageName(IntPtr Image, [MarshalAs(UnmanagedType.LPStr)] string Name);



        /// Return Type: DWORD->unsigned int
        ///Handle: HANDLE->void*
        ///pMapping: HANDLE->void*
        ///Operation: DWORD->unsigned int
        ///bTruncateLeadFFForUpgrade: BOOL->int
        [DllImport("STDFUFiles.dll", EntryPoint = "STDFUFILES_FilterImageForOperation", CallingConvention = CallingConvention.StdCall)]
        public static extern uint STDFUFILES_FilterImageForOperation(IntPtr Handle, IntPtr pMapping, uint Operation, [MarshalAs(UnmanagedType.Bool)] bool bTruncateLeadFFForUpgrade);


        /// Return Type: DWORD->unsigned int
        ///pContext: pDFUThreadContext->void*
        ///pOperationCode: PDWORD->DWORD*
        [DllImport("STDFUPRT.dll", EntryPoint = "STDFUPRT_LaunchOperation", CallingConvention = CallingConvention.StdCall)]
        public static extern uint STDFUPRT_LaunchOperation(IntPtr pContext, ref uint pOperationCode);


        /// Return Type: DWORD->unsigned int
        ///OperationCode: DWORD->unsigned int
        ///pContext: pDFUThreadContext->void*
        [DllImport("STDFUPRT.dll", EntryPoint = "STDFUPRT_GetOperationStatus", CallingConvention = CallingConvention.StdCall)]
        public static extern uint STDFUPRT_GetOperationStatus(uint OperationCode, IntPtr pContext);

        /// Return Type: DWORD->unsigned int
        ///OperationCode: DWORD->unsigned int
        ///pLastContext: pDFUThreadContext->void*
        [DllImport("STDFUPRT.dll", EntryPoint = "STDFUPRT_StopOperation", CallingConvention = CallingConvention.StdCall)]
        public static extern uint STDFUPRT_StopOperation(uint OperationCode, IntPtr pLastContext);

        /// Return Type: DWORD->unsigned int
        ///pPathFile: PSTR->CHAR*
        ///phFile: PHANDLE->HANDLE*
        ///Vid: WORD->unsigned short
        ///Pid: WORD->unsigned short
        ///Bcd: WORD->unsigned short
        [DllImport("STDFUFiles.dll", EntryPoint = "STDFUFILES_CreateNewDFUFile", CallingConvention = CallingConvention.StdCall)]
        public static extern uint STDFUFILES_CreateNewDFUFile([MarshalAs(UnmanagedType.LPStr)] string pPathFile, ref IntPtr phFile, ushort Vid, ushort Pid, ushort Bcd);


        /// Return Type: DWORD->unsigned int
        ///hFile: HANDLE->void*
        ///Image: HANDLE->void*
        [DllImport("STDFUFiles.dll", EntryPoint = "STDFUFILES_AppendImageToDFUFile", CallingConvention = CallingConvention.StdCall)]
        public static extern uint STDFUFILES_AppendImageToDFUFile(IntPtr hFile, IntPtr Image);

        /// Return Type: DWORD->unsigned int
        ///hFile: HANDLE->void*
        [DllImport("STDFUFiles.dll", EntryPoint = "STDFUFILES_CloseDFUFile", CallingConvention = CallingConvention.StdCall)]
        public static extern uint STDFUFILES_CloseDFUFile(IntPtr hFile);

        /// Return Type: DWORD->unsigned int
        ///Handle: HANDLE->void*
        ///pNbElements: PDWORD->DWORD*
        [DllImport("STDFUFiles.dll", EntryPoint = "STDFUFILES_GetImageNbElement", CallingConvention = CallingConvention.StdCall)]
        public static extern uint STDFUFILES_GetImageNbElement(IntPtr Handle, ref uint pNbElements);


        [StructLayout(LayoutKind.Sequential,Pack =1)]
        public struct DFUIMAGEELEMENT
        {

            /// DWORD->unsigned int
            public uint dwAddress;

            /// DWORD->unsigned int
            public uint dwDataLength;

            /// PBYTE->BYTE*
            public IntPtr Data;
        }
        /// Return Type: DWORD->unsigned int
        ///Handle: HANDLE->void*
        ///dwRank: DWORD->unsigned int
        ///pElement: DFUIMAGEELEMENT*
        [DllImport("STDFUFiles.dll", EntryPoint = "STDFUFILES_GetImageElement", CallingConvention = CallingConvention.StdCall)]
        public static extern uint STDFUFILES_GetImageElement(IntPtr Handle, uint dwRank, ref DFUIMAGEELEMENT pElement);

        /// Return Type: DWORD->unsigned int
        ///pPathFile: PSTR->CHAR*
        ///phFile: PHANDLE->HANDLE*
        ///pVid: PWORD->WORD*
        ///pPid: PWORD->WORD*
        ///pBcd: PWORD->WORD*
        ///pNbImages: PBYTE->BYTE*
        [DllImport("STDFUFiles.dll", EntryPoint = "STDFUFILES_OpenExistingDFUFile", CallingConvention = CallingConvention.StdCall)]
        public static extern uint STDFUFILES_OpenExistingDFUFile([MarshalAs(UnmanagedType.LPStr)] string pPathFile, ref IntPtr phFile, ref ushort pVid, ref ushort pPid, ref ushort pBcd, ref byte pNbImages);

        /// Return Type: DWORD->unsigned int
        ///hFile: HANDLE->void*
        ///Rank: int
        ///pImage: PHANDLE->HANDLE*
        [DllImport("STDFUFiles.dll", EntryPoint = "STDFUFILES_ReadImageFromDFUFile", CallingConvention = CallingConvention.StdCall)]
        public static extern uint STDFUFILES_ReadImageFromDFUFile(IntPtr hFile, int Rank, ref IntPtr pImage);

        /// Return Type: DWORD->unsigned int
        ///Image: HANDLE->void*
        ///pAlternate: PBYTE->BYTE*
        [DllImport("STDFUFiles.dll", EntryPoint = "STDFUFILES_GetImageAlternate", CallingConvention = CallingConvention.StdCall)]
        public static extern uint STDFUFILES_GetImageAlternate(System.IntPtr Image, ref byte pAlternate);

        /// Return Type: DWORD->unsigned int
        ///Image: HANDLE->void*
        ///Name: PSTR->CHAR*
        [DllImport("STDFUFiles.dll", EntryPoint = "STDFUFILES_GetImageName", CallingConvention = CallingConvention.StdCall)]
        public static extern uint STDFUFILES_GetImageName(IntPtr Image, StringBuilder Name);

        /// Return Type: DWORD->unsigned int
        ///hSource: HANDLE->void*
        ///pDest: PHANDLE->HANDLE*
        [DllImport("STDFUFiles.dll", EntryPoint = "STDFUFILES_DuplicateImage", CallingConvention = CallingConvention.StdCall)]
        public static extern uint STDFUFILES_DuplicateImage(IntPtr hSource, ref IntPtr pDest);

        #endregion






    }
}
