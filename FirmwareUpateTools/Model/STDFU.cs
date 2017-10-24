using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;

namespace FirmwareUpateTools.Model
{
    public class DFUDevice
    {
        private string devicePath;
        private string name;
        private uint nbAlternates;

        public string DevicePath { get => devicePath; set => devicePath = value; }
        public string Name { get => name; set => name = value; }
        public uint NbAlternates { get => nbAlternates; set => nbAlternates = value; }
    }
    public class STDFU
    {
        public const Int32 INVALID_HANDLE_VALUE = -1;
        private Guid DFU_GUID = new Guid(0x3fe809ab, 0xfb91, 0x4cb5, 0xa6, 0x43, 0x69, 0x67, 0x0d, 0x52, 0x36, 0x6e);
        private STDFUAPI sTDFUAPI = new STDFUAPI();
        
        public List<DFUDevice> Device = new List<DFUDevice>();
        public int m_CurrentDevice = 0;
        private IntPtr m_pMapping = IntPtr.Zero;
        private uint m_NbAlternates;
        private STDFUAPI.DFU_FUNCTIONAL_DESCRIPTOR m_CurrDevDFUDesc = new STDFUAPI.DFU_FUNCTIONAL_DESCRIPTOR();
        private IntPtr hDle;
        private IntPtr m_DeviceDesc;
        public int m_CurrentTarget = 0;
        private IntPtr m_BufferedImage;
        private DateTime startTime = new DateTime();
        private DateTime endTime = new DateTime();
        private TimeSpan elapsedTime = new TimeSpan();
        private uint m_OperationCode;
        public string m_UpFileName = "";
        public string m_DownFileName = "";
        private bool Verify = false;
        private bool Optimize = false;
        private System.Threading.Timer threadTimer;
        private int contextPercent = 0;
        public event EventHandler<EventArgs> ContextPercentChanged;
        protected virtual void OnContextPercentChanged(EventArgs e)
        {
            ContextPercentChanged?.Invoke(this, e);
        }
        public int ContextPercent { get => contextPercent; set
            {
                contextPercent = value;
                OnContextPercentChanged(null);
            }
        }



        private string printText = "";
        public event EventHandler<EventArgs> PrintTextChanged;
        protected virtual void OnPrintTextChanged(EventArgs e)
        {
            PrintTextChanged?.Invoke(this, e);
        }
        public string PrintText { get => printText; set
            {
                printText = value;
                OnPrintTextChanged(null);
            }
        }

        

        public STDFU()
        {
            threadTimer = new System.Threading.Timer(new System.Threading.TimerCallback(TimerProc), null, -1, -1);
        }
        void HandleError(STDFUAPI.DFUThreadContext Context)
        {
            string Alternate = "", Operation = "", TransferSize, LastDFUStatus = "", CurrentStateMachineTransition = "", CurrentRequest = "", StartAddress = "", EndAddress = "", CurrentNBlock, CurrentLength, ErrorCode, Percent;
            string CurrentTarget;

            CurrentTarget = "Target:" + m_CurrentTarget.ToString() + "\n";
            switch (Context.Operation)
            {
                case STDFUAPI.OPERATION_UPLOAD:
                    Operation = "Operation: UPLOAD\n";
                    break;
                case STDFUAPI.OPERATION_UPGRADE:
                    Operation = "Operation: UPGRADE\n";
                    break;
                case STDFUAPI.OPERATION_DETACH:
                    Operation = "Operation: DETACH\n";
                    break;
                case STDFUAPI.OPERATION_RETURN:
                    Operation = "Operation: RETURN\n";
                    break;
            }

            TransferSize = "TransferSize: " + Context.wTransferSize.ToString() + "\n";

            switch (Context.LastDFUStatus.bState)
            {
                case STDFUAPI.STATE_IDLE:
                    LastDFUStatus += "DFU State: STATE_IDLE\n";
                    break;
                case STDFUAPI.STATE_DETACH:
                    LastDFUStatus += "DFU State: STATE_DETACH\n";
                    break;
                case STDFUAPI.STATE_DFU_IDLE:
                    LastDFUStatus += "DFU State: STATE_DFU_IDLE\n";
                    break;
                case STDFUAPI.STATE_DFU_DOWNLOAD_SYNC:
                    LastDFUStatus += "DFU State: STATE_DFU_DOWNLOAD_SYNC\n";
                    break;
                case STDFUAPI.STATE_DFU_DOWNLOAD_BUSY:
                    LastDFUStatus += "DFU State: STATE_DFU_DOWNLOAD_BUSY\n";
                    break;
                case STDFUAPI.STATE_DFU_DOWNLOAD_IDLE:
                    LastDFUStatus += "DFU State: STATE_DFU_DOWNLOAD_IDLE\n";
                    break;
                case STDFUAPI.STATE_DFU_MANIFEST_SYNC:
                    LastDFUStatus += "DFU State: STATE_DFU_MANIFEST_SYNC\n";
                    break;
                case STDFUAPI.STATE_DFU_MANIFEST:
                    LastDFUStatus += "DFU State: STATE_DFU_MANIFEST\n";
                    break;
                case STDFUAPI.STATE_DFU_MANIFEST_WAIT_RESET:
                    LastDFUStatus += "DFU State: STATE_DFU_MANIFEST_WAIT_RESET\n";
                    break;
                case STDFUAPI.STATE_DFU_UPLOAD_IDLE:
                    LastDFUStatus += "DFU State: STATE_DFU_UPLOAD_IDLE\n";
                    break;
                case STDFUAPI.STATE_DFU_ERROR:
                    LastDFUStatus += "DFU State: STATE_DFU_ERROR\n";
                    break;
                default:
                    LastDFUStatus += "DFU State: (Unknown 0x"+ Context.LastDFUStatus.bState.ToString("X2")+ ")\n";
                    break;
            }
            switch (Context.LastDFUStatus.bStatus)
            {
                case STDFUAPI.STATUS_OK:
                    LastDFUStatus += "DFU Status: STATUS_OK\n";
                    break;
                case STDFUAPI.STATUS_errTARGET:
                    LastDFUStatus += "DFU Status: STATUS_errTARGET\n";
                    break;
                case STDFUAPI.STATUS_errFILE:
                    LastDFUStatus += "DFU Status: STATUS_errFILE\n";
                    break;
                case STDFUAPI.STATUS_errWRITE:
                    LastDFUStatus += "DFU Status: STATUS_errWRITE\n";
                    break;
                case STDFUAPI.STATUS_errERASE:
                    LastDFUStatus += "DFU Status: STATUS_errERASE\n";
                    break;
                case STDFUAPI.STATUS_errCHECK_ERASE:
                    LastDFUStatus += "DFU Status: STATUS_errCHECK_ERASE\n";
                    break;
                case STDFUAPI.STATUS_errPROG:
                    LastDFUStatus += "DFU Status: STATUS_errPROG\n";
                    break;
                case STDFUAPI.STATUS_errVERIFY:
                    LastDFUStatus += "DFU Status: STATUS_errVERIFY\n";
                    break;
                case STDFUAPI.STATUS_errADDRESS:
                    LastDFUStatus += "DFU Status: STATUS_errADDRESS\n";
                    break;
                case STDFUAPI.STATUS_errNOTDONE:
                    LastDFUStatus += "DFU Status: STATUS_errNOTDONE\n";
                    break;
                case STDFUAPI.STATUS_errFIRMWARE:
                    LastDFUStatus += "DFU Status: STATUS_errFIRMWARE\n";
                    break;
                case STDFUAPI.STATUS_errVENDOR:
                    LastDFUStatus += "DFU Status: STATUS_errVENDOR\n";
                    break;
                case STDFUAPI.STATUS_errUSBR:
                    LastDFUStatus += "DFU Status: STATUS_errUSBR\n";
                    break;
                case STDFUAPI.STATUS_errPOR:
                    LastDFUStatus += "DFU Status: STATUS_errPOR\n";
                    break;
                case STDFUAPI.STATUS_errUNKNOWN:
                    LastDFUStatus += "DFU Status: STATUS_errUNKNOWN\n";
                    break;
                case STDFUAPI.STATUS_errSTALLEDPKT:
                    LastDFUStatus += "DFU Status: STATUS_errSTALLEDPKT\n";
                    break;
                default:
                    LastDFUStatus += "DFU State: (Unknown 0x" + Context.LastDFUStatus.bState.ToString("X2") + ")\n";
                    break;
            }

            switch (Context.CurrentRequest)
            {
                case STDFUAPI.STDFU_RQ_GET_DEVICE_DESCRIPTOR:
                    CurrentRequest += "Request: Getting Device Descriptor. \n";
                    break;
                case STDFUAPI.STDFU_RQ_GET_DFU_DESCRIPTOR:
                    CurrentRequest += "Request: Getting DFU Descriptor. \n";
                    break;
                case STDFUAPI.STDFU_RQ_GET_STRING_DESCRIPTOR:
                    CurrentRequest += "Request: Getting String Descriptor. \n";
                    break;
                case STDFUAPI.STDFU_RQ_GET_NB_OF_CONFIGURATIONS:
                    CurrentRequest += "Request: Getting amount of configurations. \n";
                    break;
                case STDFUAPI.STDFU_RQ_GET_CONFIGURATION_DESCRIPTOR:
                    CurrentRequest += "Request: Getting Configuration Descriptor. \n";
                    break;
                case STDFUAPI.STDFU_RQ_GET_NB_OF_INTERFACES:
                    CurrentRequest += "Request: Getting amount of interfaces. \n";
                    break;
                case STDFUAPI.STDFU_RQ_GET_NB_OF_ALTERNATES:
                    CurrentRequest += "Request: Getting amount of alternates. \n";
                    break;
                case STDFUAPI.STDFU_RQ_GET_INTERFACE_DESCRIPTOR:
                    CurrentRequest += "Request: Getting interface Descriptor. \n";
                    break;
                case STDFUAPI.STDFU_RQ_OPEN:
                    CurrentRequest += "Request: Opening device. \n";
                    break;
                case STDFUAPI.STDFU_RQ_CLOSE:
                    CurrentRequest += "Request: Closing device. \n";
                    break;
                case STDFUAPI.STDFU_RQ_DETACH:
                    CurrentRequest += "Request: Detach Request. \n";
                    break;
                case STDFUAPI.STDFU_RQ_DOWNLOAD:
                    CurrentRequest += "Request: Download Request. \n";
                    break;
                case STDFUAPI.STDFU_RQ_UPLOAD:
                    CurrentRequest += "Request: Upload Request. \n";
                    break;
                case STDFUAPI.STDFU_RQ_GET_STATUS:
                    CurrentRequest += "Request: Get Status Request. \n";
                    break;
                case STDFUAPI.STDFU_RQ_CLR_STATUS:
                    CurrentRequest += "Request: Clear Status Request. \n";
                    break;
                case STDFUAPI.STDFU_RQ_GET_STATE:
                    CurrentRequest += "Request: Get State Request. \n";
                    break;
                case STDFUAPI.STDFU_RQ_ABORT:
                    CurrentRequest += "Request: Abort Request. \n";
                    break;
                case STDFUAPI.STDFU_RQ_SELECT_ALTERNATE:
                    CurrentRequest += "Request: Selecting target. \n";
                    break;
                case STDFUAPI.STDFU_RQ_AWAITINGPNPUNPLUGEVENT:
                    CurrentRequest += "Request: Awaiting UNPLUG EVENT. \n";
                    break;
                case STDFUAPI.STDFU_RQ_AWAITINGPNPPLUGEVENT:
                    CurrentRequest += "Request: Awaiting PLUG EVENT. \n";
                    break;
                case STDFUAPI.STDFU_RQ_IDENTIFYINGDEVICE:
                    CurrentRequest += "Request: Identifying device after reenumeration. \n";
                    break;
                default:
                    CurrentRequest += "Request: (unknown 0x"+ Context.CurrentRequest.ToString("X8")+ "). \n";
                    break;
            }

            CurrentNBlock = "CurrentNBlock: 0x" + Context.CurrentNBlock.ToString("X4") + "\n";
            CurrentLength = "CurrentLength: 0x" + Context.CurrentLength.ToString("X4") + "\n";
            Percent = "Percent: " + Context.Percent.ToString() + "%\n";

            switch (Context.ErrorCode)
            {
                case STDFUAPI.STDFUPRT_NOERROR:
                    ErrorCode = "Error Code: no error (!)\n";
                    break;
                case STDFUAPI.STDFUPRT_UNABLETOLAUNCHDFUTHREAD:
                    ErrorCode = "Error Code: Unable to launch operation (Thread problem)\n";
                    break;
                case STDFUAPI.STDFUPRT_DFUALREADYRUNNING:
                    ErrorCode = "Error Code: DFU already running\n";
                    break;
                case STDFUAPI.STDFUPRT_BADPARAMETER:
                    ErrorCode = "Error Code: Bad parameter\n";
                    break;
                case STDFUAPI.STDFUPRT_BADFIRMWARESTATEMACHINE:
                    ErrorCode = "Error Code: Bad state machine in firmware\n";
                    break;
                case STDFUAPI.STDFUPRT_UNEXPECTEDERROR:
                    ErrorCode = "Error Code: Unexpected error\n";
                    break;
                case STDFUAPI.STDFUPRT_DFUERROR:
                    ErrorCode = "Error Code: DFU error\n";
                    break;
                case STDFUAPI.STDFUPRT_RETRYERROR:
                    ErrorCode = "Error Code: Retry error\n";
                    break;
                default:
                    ErrorCode = "Error Code: Unknown error 0x" + Context.ErrorCode.ToString("X8") + ". \n";
                    break;
            }


            if (m_CurrentTarget >= 0)
            {
                //m_Progress.SetWindowText(Tmp);
                PrintText +=(string.Format("\nTarget {0:D}: ", m_CurrentTarget) + ErrorCode);
            }
            else
                //m_Progress.SetWindowText(ErrorCode);
                PrintText +=("\n" + ErrorCode);

            //AfxMessageBox(CurrentTarget+ErrorCode+Alternate+Operation+TransferSize+LastDFUStatus+CurrentStateMachineTransition+CurrentRequest+StartAddress+EndAddress+CurrentNBlock+CurrentLength+Percent);
            PrintText +=(CurrentTarget + ErrorCode + Alternate + Operation + TransferSize + LastDFUStatus + CurrentStateMachineTransition + CurrentRequest + StartAddress + EndAddress + CurrentNBlock + CurrentLength + Percent);


        }

        void TimerProc(object status)
        {


            STDFUAPI.DFUThreadContext Context = new STDFUAPI.DFUThreadContext();
            uint dwRet;

            if (m_OperationCode>0)
            {

                endTime = DateTime.Now;
                elapsedTime = endTime - startTime;

                PrintText += string.Format(" Duration: {0:D2}:{1:D2}:{2:D2}", elapsedTime.Minutes, elapsedTime.Seconds, elapsedTime.Milliseconds);


                // Get the operation status
                IntPtr pContext = Marshal.AllocHGlobal(Marshal.SizeOf(Context));
                Marshal.StructureToPtr(Context, pContext,true);

                STDFUAPI.STDFUPRT_GetOperationStatus(m_OperationCode, pContext);

                Context = (STDFUAPI.DFUThreadContext)Marshal.PtrToStructure(pContext, typeof(STDFUAPI.DFUThreadContext));
                Marshal.FreeHGlobal(pContext);

                if (Context.ErrorCode != STDFUAPI.STDFUPRT_NOERROR)
                {
                    pContext = Marshal.AllocHGlobal(Marshal.SizeOf(Context));
                    Marshal.StructureToPtr(Context, pContext, true);

                    STDFUAPI.STDFUPRT_StopOperation(m_OperationCode, pContext);

                    Context = (STDFUAPI.DFUThreadContext)Marshal.PtrToStructure(pContext, typeof(STDFUAPI.DFUThreadContext));
                    Marshal.FreeHGlobal(pContext);

                    if (Context.Operation == STDFUAPI.OPERATION_UPGRADE)
                        m_BufferedImage = IntPtr.Zero;
                    if (Context.Operation == STDFUAPI.OPERATION_UPLOAD)
                    {
                        if (m_BufferedImage != IntPtr.Zero) // Verify
                        {
                            STDFUAPI.STDFUFILES_DestroyImage(ref m_BufferedImage);
                            m_BufferedImage = IntPtr.Zero;
                        }
                    }
                    STDFUAPI.STDFUFILES_DestroyImage(ref Context.hImage);

                    m_OperationCode = 0;
                    if (Context.ErrorCode == STDFUAPI.STDFUPRT_UNSUPPORTEDFEATURE)
                        //AfxMessageBox("This feature is not supported by this firmware.");
                        PrintText +=("This feature is not supported by this firmware.");
                    else

                    {
                        HandleError(Context);
                        threadTimer.Change(-1, -1);
                        return;
                    }
                    
                }
                else
                {
                    switch (Context.Operation)
                    {
                        case STDFUAPI.OPERATION_UPLOAD:
                            {
                                PrintText += string.Format("\rTarget {0:D}: Uploading ({1:D2}%)...", m_CurrentTarget, Context.Percent);
                                ContextPercent = Context.Percent;

                                //printf("%i KB(%i Bytes) of %i KB(%i Bytes) \n", ((STDFUFILES_GetImageSize(Context.hImage)/1024)*Context.Percent)/100,  (STDFUFILES_GetImageSize(Context.hImage)*Context.Percent)/100, STDFUFILES_GetImageSize(Context.hImage)/1024,  STDFUFILES_GetImageSize(Context.hImage));



                                // m_DataSize.Format("%i KB(%i Bytes) of %i KB(%i Bytes)", ((STDFUFILES_GetImageSize(Context.hImage)/1024)*Context.Percent)/100,  (STDFUFILES_GetImageSize(Context.hImage)*Context.Percent)/100, STDFUFILES_GetImageSize(Context.hImage)/1024,  STDFUFILES_GetImageSize(Context.hImage));


                                break;
                            }
                        case STDFUAPI.OPERATION_UPGRADE:
                            {
                                PrintText += string.Format("\rTarget {0:D}: Upgrading - Download Phase ({1:D2}%)...", m_CurrentTarget, Context.Percent);
                                ContextPercent = Context.Percent;
                                //printf("%i KB(%i Bytes) of %i KB(%i Bytes) \n", ((STDFUFILES_GetImageSize(Context.hImage)/1024)*Context.Percent)/100,  (STDFUFILES_GetImageSize(Context.hImage)*Context.Percent)/100, STDFUFILES_GetImageSize(Context.hImage)/1024,  STDFUFILES_GetImageSize(Context.hImage));

                                //m_DataSize.Format("%i KB(%i Bytes) of %i KB(%i Bytes)", ((STDFUFILES_GetImageSize(Context.hImage)/1024)*Context.Percent)/100,  (STDFUFILES_GetImageSize(Context.hImage)*Context.Percent)/100, STDFUFILES_GetImageSize(Context.hImage)/1024,  STDFUFILES_GetImageSize(Context.hImage));


                                break;
                            }
                        case STDFUAPI.OPERATION_DETACH:
                            PrintText += string.Format("\rDetaching ({0:D2}%)...", Context.Percent);
                            ContextPercent = Context.Percent;
                            break;
                        case STDFUAPI.OPERATION_RETURN:
                            PrintText += string.Format("\rLeaving DFU mode ({0:D2}%)...", Context.Percent);
                            ContextPercent = Context.Percent;
                            break;
                        default:
                            PrintText += string.Format("\rTarget {0:D}: Upgrading - Erase Phase ({1:D2}%)...", m_CurrentTarget, Context.Percent);
                            ContextPercent = Context.Percent;
                            break;
                    }

                    //	m_Progress.SetWindowText(Tmp);
                    //	m_Progress.SetPos(Context.Percent);
                    if (Context.Percent == 100)
                    {
                        if (Context.Operation == STDFUAPI.OPERATION_ERASE)
                        {
                            pContext = Marshal.AllocHGlobal(Marshal.SizeOf(Context));
                            Marshal.StructureToPtr(Context, pContext, true);
                            // After the erase, relaunch the Upgrade phase !

                            STDFUAPI.STDFUPRT_StopOperation(m_OperationCode, pContext);

                            Context = (STDFUAPI.DFUThreadContext)Marshal.PtrToStructure(pContext, typeof(STDFUAPI.DFUThreadContext));
                            Marshal.FreeHGlobal(pContext);

                            STDFUAPI.STDFUFILES_DestroyImage(ref Context.hImage);

                            Context.Operation = STDFUAPI.OPERATION_UPGRADE;
                            Context.hImage = m_BufferedImage;

                            pContext = Marshal.AllocHGlobal(Marshal.SizeOf(Context));
                            Marshal.StructureToPtr(Context, pContext, true);

                            dwRet = STDFUAPI.STDFUPRT_LaunchOperation(pContext, ref m_OperationCode);

                            Context = (STDFUAPI.DFUThreadContext)Marshal.PtrToStructure(pContext, typeof(STDFUAPI.DFUThreadContext));
                            Marshal.FreeHGlobal(pContext);
                            if (dwRet != STDFUAPI.STDFUPRT_NOERROR)
                            {
                                Context.ErrorCode = dwRet;
                                HandleError(Context);
                                //	printf( "Error");
                                threadTimer.Change(-1, -1);
                                return;
                            }
                        }
                        else
                        {
                            bool bAllTargetsFinished = true;

                            pContext = Marshal.AllocHGlobal(Marshal.SizeOf(Context));
                            Marshal.StructureToPtr(Context, pContext, true);

                            STDFUAPI.STDFUPRT_StopOperation(m_OperationCode, pContext);
                            
                            Context = (STDFUAPI.DFUThreadContext)Marshal.PtrToStructure(pContext, typeof(STDFUAPI.DFUThreadContext));
                            Marshal.FreeHGlobal(pContext);
                            //m_Progress.SetPos(100);
                            m_OperationCode = 0;

                            //int Sel=m_CtrlDevices.GetCurSel();
                            if (Context.Operation == STDFUAPI.OPERATION_RETURN)
                            {
                                /*delete m_DetachedDevs[m_DetachedDev];
                                m_DetachedDevs.RemoveKey(m_DetachedDev);
                                if (Sel!=-1)
                                {
                                    m_CtrlDFUDevices.InsertString(Sel, Context.szDevLink);
                                    m_CurrDFUName=Context.szDevLink;
                                }*/
                                PrintText +=("\nSuccessfully left DFU mode !\n");
                            }
                            if (Context.Operation == STDFUAPI.OPERATION_DETACH)
                            {
                                /*CString Tmp=Context.szDevLink;

                                Tmp.MakeUpper();
                                m_DetachedDevs[Tmp]=m_DetachedDevs[m_DetachedDev];
                                ((PUSB_DEVICE_DESCRIPTOR)(m_DetachedDevs[Tmp]))->bLength=18;
                                m_DetachedDevs.RemoveKey(m_DetachedDev);
                                if (Sel!=-1)
                                {
                                    m_CtrlDFUDevices.InsertString(Sel, Context.szDevLink);
                                    m_CurrDFUName=Context.szDevLink;
                                }*/
                                PrintText +=("\nSuccessfully detached !\n");
                            }
                            if (Context.Operation == STDFUAPI.OPERATION_UPLOAD)
                            {
                                if (m_BufferedImage == IntPtr.Zero) // This was a standard Upload
                                {
                                    IntPtr hFile = IntPtr.Zero;
                                    STDFUAPI.USB_DEVICE_DESCRIPTOR Desc = (STDFUAPI.USB_DEVICE_DESCRIPTOR)Marshal.PtrToStructure(m_DeviceDesc, typeof(STDFUAPI.USB_DEVICE_DESCRIPTOR));

                                    ushort Vid, Pid, Bcd;
                                    
                                    Vid = Desc.idVendor;
                                    Pid = Desc.idProduct;
                                    Bcd = Desc.bcdDevice;

                                    /*Desc=(PUSB_DEVICE_DESCRIPTOR)(&m_CurrDevDFUDesc);
                                    if ( (Desc) && (Desc->bLength) )
                                    {
                                        Vid=Desc->idVendor;
                                        Pid=Desc->idProduct;
                                        Bcd=Desc->bcdDevice;
                                    }*/

                                    //if (m_CtrlDevTargets.GetNextItem(-1, LVIS_SELECTED)==m_CurrentTarget)
                                    dwRet = STDFUAPI.STDFUFILES_CreateNewDFUFile(m_UpFileName,ref hFile, Vid, Pid, Bcd);
                                    //else
                                    //	dwRet=STDFUFILES_OpenExistingDFUFile((LPSTR)(LPCSTR)m_UpFileName, &hFile, NULL, NULL, NULL, NULL);


                                    if (dwRet == STDFUAPI.STDFUFILES_NOERROR)
                                    {
                                        dwRet = STDFUAPI.STDFUFILES_AppendImageToDFUFile(hFile, Context.hImage);
                                        if (dwRet == STDFUAPI.STDFUFILES_NOERROR)
                                        {
                                            PrintText +=("Upload successful !\n");
                                            //m_CurrentTarget=m_CtrlDevTargets.GetNextItem(m_CurrentTarget, LVIS_SELECTED);
                                            /*if (m_CurrentTarget>=0)
                                            {
                                                bAllTargetsFinished=FALSE;
                                                LaunchUpload();
                                            }*/

                                            threadTimer.Change(-1, -1);
                                            STDFUAPI.STDFUFILES_CloseDFUFile(hFile);
                                            return;
                                        }
                                        else
                                            PrintText +=("Unable to append image to DFU file...");
                                        STDFUAPI.STDFUFILES_CloseDFUFile(hFile);
                                    }
                                    else
                                        PrintText +=("Unable to create a new DFU file...");
                                }
                                else // This was a verify
                                {
                                    // We need to compare our Two images
                                    uint i, j;
                                    uint MaxElements = 0;
                                    bool bDifferent = false, bSuccess = true;

                                    dwRet = STDFUAPI.STDFUFILES_GetImageNbElement(Context.hImage, ref MaxElements);
                                    if (dwRet == STDFUAPI.STDFUFILES_NOERROR)
                                    {

                                        for (i = 0; i < MaxElements; i++)
                                        {
                                            STDFUAPI.DFUIMAGEELEMENT ElementRead = new STDFUAPI.DFUIMAGEELEMENT();
                                            STDFUAPI.DFUIMAGEELEMENT ElementSource = new STDFUAPI.DFUIMAGEELEMENT();

                                            // Get the Two elements
                                            dwRet = STDFUAPI.STDFUFILES_GetImageElement(Context.hImage, i,ref ElementRead);
                                            if (dwRet == STDFUAPI.STDFUFILES_NOERROR)
                                            {
                                                ElementRead.Data = Marshal.AllocHGlobal((int)ElementRead.dwDataLength);
                                                dwRet = STDFUAPI.STDFUFILES_GetImageElement(Context.hImage, i,ref ElementRead);
                                                if (dwRet == STDFUAPI.STDFUFILES_NOERROR)
                                                {
                                                    ElementSource.Data = Marshal.AllocHGlobal((int)ElementRead.dwDataLength); // Should be same lengh in source and read
                                                    dwRet = STDFUAPI.STDFUFILES_GetImageElement(m_BufferedImage, i,ref ElementSource);
                                                    if (dwRet == STDFUAPI.STDFUFILES_NOERROR)
                                                    {
                                                        byte[] SourceData = new byte[ElementRead.dwDataLength];
                                                        Marshal.Copy(ElementSource.Data, SourceData, 0, SourceData.Length);
                                                        Marshal.FreeHGlobal(ElementSource.Data);

                                                        byte[] ReadData = new byte[ElementRead.dwDataLength];
                                                        Marshal.Copy(ElementRead.Data, ReadData, 0, ReadData.Length);
                                                        Marshal.FreeHGlobal(ElementRead.Data);
                                                        for (j = 0; j < ElementRead.dwDataLength; j++)
                                                        {

                                                            if (SourceData[j] != ReadData[j])
                                                            {
                                                                bDifferent = true;
                                                                PrintText += ("Verify successful, but data not matching...\r\n");
                                                                PrintText += string.Format("Matching not good. First Difference at address 0x{0:X8}:\nFile  byte  is  0x{0:X2}.\nRead byte is 0x{0:X2}.", ElementSource.dwAddress + j, SourceData[j], ReadData[j]);
                                                                break;
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        PrintText +=("Unable to get data from source image...\r\n");
                                                        bSuccess = false;
                                                        break;
                                                    }
                                                }
                                                else
                                                {
                                                    Marshal.FreeHGlobal(ElementRead.Data);
                                                    PrintText +=("Unable to get data from read image...\r\n");
                                                    bSuccess = false;
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                PrintText +=("Unable to get data from read image...\r\n");
                                                bSuccess = false;
                                                break;
                                            }
                                            if (bDifferent)
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        PrintText +=("Unable to get elements from read image...");
                                        bSuccess = true;
                                    }

                                    STDFUAPI.STDFUFILES_DestroyImage(ref m_BufferedImage);
                                    m_BufferedImage = IntPtr.Zero;
                                    if (bSuccess)
                                    {
                                        if (!bDifferent)
                                            PrintText +=("\nVerify successful !\n");
                                        /*m_CurrentTarget=m_CtrlDevTargets.GetNextItem(m_CurrentTarget, LVIS_SELECTED);
                                        if (m_CurrentTarget>=0)
                                        {
                                            bAllTargetsFinished=FALSE;
                                            LaunchVerify();
                                        }*/

                                        threadTimer.Change(-1, -1);
                                        return;

                                    }
                                }

                            }
                            if (Context.Operation == STDFUAPI.OPERATION_UPGRADE)
                            {
                                PrintText +=("\nUpgrade successful !\n");
                                m_BufferedImage = IntPtr.Zero;


                                /*m_CurrentTarget=m_CtrlDevTargets.GetNextItem(m_CurrentTarget, LVIS_SELECTED);
                                if (m_CurrentTarget>=0)
                                {
                                    bAllTargetsFinished=FALSE;
                                    LaunchUpgrade();
                                }*/
                            }
                            if (bAllTargetsFinished)
                            {
                                if ((Context.Operation == STDFUAPI.OPERATION_UPGRADE))
                                {
                                    if (Verify)
                                    {
                                        //// After the upgrade , relaunch the Upgrade verify !
                                        //string Tempo, DevId, FileId;

                                        //m_CurrentTarget=m_CtrlDevTargets.GetNextItem(-1, LVIS_SELECTED);
                                        /*if (m_CurrentTarget==-1)
                                        {
                                            HandleTxtError("Please select one or several targets before !");
                                            return;
                                        }

                                        m_CtrlDevAppVid.GetWindowText(DevId);
                                        if (DevId.IsEmpty())
                                        {
                                            if (AfxMessageBox("Your device was plugged in DFU mode. \nSo it is impossible to make sure this file is correct for this device.\n\nContinue however ?", MB_YESNO)!=IDYES)
                                                return;
                                        }
                                        else
                                        {
                                            m_CtrlFileVid.GetWindowText(FileId);
                                            if (FileId!=DevId)
                                            {
                                                if (AfxMessageBox("This file is not supposed to be used with that device.\n\nContinue however ?", MB_YESNO)!=IDYES)
                                                    return;
                                            }
                                            else
                                            {
                                                m_CtrlDevAppPid.GetWindowText(DevId);
                                                m_CtrlFilePid.GetWindowText(FileId);
                                                if (FileId!=DevId)
                                                {
                                                    if (AfxMessageBox("This file is not supposed to be used with that device.\n\nContinue however ?", MB_YESNO)!=IDYES)
                                                        return;
                                                }
                                            }
                                        }*/

                                        LaunchVerify();
                                    }
                                    else
                                    {

                                        threadTimer.Change(-1, -1);
                                        return;
                                    }


                                }
                                if ((Context.Operation == STDFUAPI.OPERATION_UPLOAD) &&
                                     (String.Equals(m_UpFileName, m_DownFileName, StringComparison.CurrentCultureIgnoreCase)))
                                {
                                    byte NbTargets = 0;
                                    IntPtr hFile = IntPtr.Zero;
                                    ushort vpd = 0;
                                    //	m_CtrlFileTargets.ResetContent();
                                    if (STDFUAPI.STDFUFILES_OpenExistingDFUFile(m_UpFileName, ref hFile, ref vpd, ref vpd, ref vpd,ref NbTargets) == STDFUAPI.STDFUFILES_NOERROR)
                                    {
                                        for (int i = 0; i < NbTargets; i++)
                                        {
                                            IntPtr Image = IntPtr.Zero;
                                            byte Alt = 0;
                                            StringBuilder Name = new StringBuilder();

                                            if (STDFUAPI.STDFUFILES_ReadImageFromDFUFile(hFile, i,ref Image) == STDFUAPI.STDFUFILES_NOERROR)
                                            {
                                                if (STDFUAPI.STDFUFILES_GetImageAlternate(Image,ref Alt) == STDFUAPI.STDFUFILES_NOERROR)
                                                {
                                                    STDFUAPI.STDFUFILES_GetImageName(Image, Name);
                                                    //Tempo.Format("%02i\t%s", Alt, Name);

                                                    //	m_CtrlFileTargets.AddString(Tempo);
                                                }
                                                STDFUAPI.STDFUFILES_DestroyImage(ref Image);
                                            }
                                        }
                                        STDFUAPI.STDFUFILES_CloseDFUFile(hFile);
                                    }
                                }

                                if ((Context.Operation == STDFUAPI.OPERATION_DETACH) || (Context.Operation == STDFUAPI.OPERATION_RETURN))
                                {

                                    /*Refresh();*/
                                }
                                else
                                {

                                }
                            }
                            STDFUAPI.STDFUFILES_DestroyImage(ref Context.hImage);
                        }
                    }
                }
            }


        }
        public int Refresh()
        {
            Device.RemoveAll(x => x != null);
            IntPtr info = sTDFUAPI.GetClassDevOfHandle(DFU_GUID);
            uint index = 0;

            if (info != IntPtr.Zero)
            {
                STDFUAPI.SP_DEVICE_INTERFACE_DATA ifData = new STDFUAPI.SP_DEVICE_INTERFACE_DATA();
                ifData.cbSize = Marshal.SizeOf(ifData);
                for(index = 0; sTDFUAPI.GetEnumDeviceInterfaces(info, ref DFU_GUID, index, ref ifData);++index)
                {
                    uint needed = 0;
                    sTDFUAPI.GetDeviceInterfaceDetail(info,ref ifData, IntPtr.Zero, 0,ref needed, null);
                    IntPtr pDetail = Marshal.AllocHGlobal((int)needed);
                    STDFUAPI.SP_DEVICE_INTERFACE_DETAIL_DATA detail = new STDFUAPI.SP_DEVICE_INTERFACE_DETAIL_DATA();
                    detail.cbSize = Marshal.SizeOf(typeof(STDFUAPI.SP_DEVICE_INTERFACE_DETAIL_DATA));
                    Marshal.StructureToPtr(detail, pDetail, false);
                    STDFUAPI.SP_DEVINFO_DATA did = new STDFUAPI.SP_DEVINFO_DATA();
                    DFUDevice dFUDevice = new DFUDevice();
                    if (sTDFUAPI.GetDeviceInterfaceDetail(info, ref ifData, pDetail, needed,ref needed, did))
                    {
                        // Add the link to the list of all DFU devices

                        dFUDevice.DevicePath = (Marshal.PtrToStringAuto((IntPtr)((int)pDetail + 4)));
                    }
                    StringBuilder Product = new StringBuilder();
                    if (STDFUAPI.SetupDiGetDeviceRegistryProperty(info,did, STDFUAPI.SPDRP_DEVICEDESC, 0, Product, 253,IntPtr.Zero))
                    {
                        dFUDevice.Name = Product.ToString();
                    }
                    else
                    {
                        dFUDevice.Name = ("(Unnamed DFU device)");
                    }
                    Marshal.FreeHGlobal(pDetail);
                    Device.Add(dFUDevice);
                }
                sTDFUAPI.DestroyDeviceInfoList(info);
            }
           if(m_pMapping != IntPtr.Zero)
            {
                STDFUAPI.STDFUPRT_DestroyMapping(ref m_pMapping);
                m_pMapping = IntPtr.Zero;
            }
            m_NbAlternates = 0;

            if(index >0)
            {
                if (STDFUAPI.STDFU_Open(Device[m_CurrentDevice].DevicePath, ref hDle) == STDFUAPI.STDFU_NOERROR)   //  !!!! To be changed
                {
                    if (STDFUAPI.STDFU_GetDeviceDescriptor(ref hDle, ref m_DeviceDesc) == STDFUAPI.STDFU_NOERROR)
                    {
                        uint Dummy1 = 0;
                        uint Dummy2 = 0;
                        IntPtr pm_CurrDevDFUDesc = Marshal.AllocHGlobal(Marshal.SizeOf(m_CurrDevDFUDesc));
                        if (STDFUAPI.STDFU_GetDFUDescriptor(ref hDle, ref Dummy1, ref Dummy2, pm_CurrDevDFUDesc) == STDFUAPI.STDFUPRT_NOERROR)
                        {
                            m_CurrDevDFUDesc = (STDFUAPI.DFU_FUNCTIONAL_DESCRIPTOR)Marshal.PtrToStructure(pm_CurrDevDFUDesc, typeof(STDFUAPI.DFU_FUNCTIONAL_DESCRIPTOR));
                            Marshal.FreeHGlobal(pm_CurrDevDFUDesc);
                            if ((m_CurrDevDFUDesc.bcdDFUVersion < 0x011A) || (m_CurrDevDFUDesc.bcdDFUVersion >= 0x0120))
                            {
                                if (m_CurrDevDFUDesc.bcdDFUVersion != 0)
                                    MessageBox.Show("Bad DFU protocol version. Should be 1.1A");
                            }
                            else
                            {
                                // Tries to get the mapping
                                if (STDFUAPI.STDFUPRT_CreateMappingFromDevice(Device[m_CurrentDevice].DevicePath, ref m_pMapping, ref m_NbAlternates) == STDFUAPI.STDFUPRT_NOERROR)
                                {
                                    Device[m_CurrentDevice].NbAlternates = m_NbAlternates;
                                    //bSuccess = TRUE;
                                }
                                else
                                    MessageBox.Show("Unable to find or decode device mapping... Bad Firmware");
                            }
                        }
                    }
                    else
                        MessageBox.Show("Unable to get DFU descriptor... Bad Firmware");
                }
                else
                    MessageBox.Show("Unable to get descriptors... Bad Firmware");
                STDFUAPI.STDFU_Close(ref hDle);
                return 0;
            }
            return 1;
        }

        private int LaunchUpload()
        {
            STDFUAPI.DFUThreadContext Context = new STDFUAPI.DFUThreadContext();
            uint dwRet;
            int TargetSel = m_CurrentTarget;
            IntPtr hImage = IntPtr.Zero;

            // prepare the asynchronous operation
            Context.szDevLink = Device[m_CurrentDevice].DevicePath;
            Context.DfuGUID = DFU_GUID;

            Context.Operation = STDFUAPI.OPERATION_UPLOAD;
            if (m_BufferedImage != IntPtr.Zero)
                STDFUAPI.STDFUFILES_DestroyImage(ref m_BufferedImage);
            m_BufferedImage = IntPtr.Zero;

            string Name;
            STDFUAPI.MAPPING pMappinglength = new STDFUAPI.MAPPING();
            STDFUAPI.MAPPING pMapping = (STDFUAPI.MAPPING)Marshal.PtrToStructure(IntPtr.Add(m_pMapping, TargetSel * Marshal.SizeOf(pMappinglength)), typeof(STDFUAPI.MAPPING));
            
            Name = pMapping.Name;
            STDFUAPI.STDFUFILES_CreateImageFromMapping(ref hImage, IntPtr.Add(m_pMapping, TargetSel * Marshal.SizeOf(pMappinglength)));
            STDFUAPI.STDFUFILES_SetImageName(hImage, Name);
            STDFUAPI.STDFUFILES_FilterImageForOperation(hImage, IntPtr.Add(m_pMapping, TargetSel * Marshal.SizeOf(pMappinglength)), STDFUAPI.OPERATION_UPLOAD, false);
            Context.hImage = hImage;

            startTime = DateTime.Now;


            //printf("0 KB(0 Bytes) of %i KB(%i Bytes) \n", STDFUFILES_GetImageSize(Context.hImage)/1024,  STDFUFILES_GetImageSize(Context.hImage));

            IntPtr pContext = Marshal.AllocHGlobal(Marshal.SizeOf(Context));
            Marshal.StructureToPtr(Context, pContext, true);
            dwRet = STDFUAPI.STDFUPRT_LaunchOperation(pContext, ref m_OperationCode);
            Context = (STDFUAPI.DFUThreadContext)Marshal.PtrToStructure(pContext, typeof(STDFUAPI.DFUThreadContext));
            Marshal.FreeHGlobal(pContext);
            if (dwRet != STDFUAPI.STDFUPRT_NOERROR)
            {
                Context.ErrorCode = dwRet;
                HandleError(Context);
                return 1;
            }
            else
            {
                return 0;
            }
        }
        private int LaunchUpgrade()
        {
            STDFUAPI.DFUThreadContext Context = new STDFUAPI.DFUThreadContext();
            IntPtr hFile = IntPtr.Zero;
            byte NbTargets = 0;
            bool bFound = false;
            uint dwRet;
            int i = 0, TargetSel = m_CurrentTarget;
            IntPtr hImage = IntPtr.Zero;
            ushort vpd = 0;

            // Get the image of the selected target
            dwRet = STDFUAPI.STDFUFILES_OpenExistingDFUFile(m_DownFileName, ref hFile, ref vpd, ref vpd, ref vpd, ref NbTargets);
            if (dwRet == STDFUAPI.STDFUFILES_NOERROR)
            {
                for (i = 0; i < NbTargets; i++)
                {
                    IntPtr Image = IntPtr.Zero;
                    byte Alt = 0;

                    if (STDFUAPI.STDFUFILES_ReadImageFromDFUFile(hFile, i,ref Image) == STDFUAPI.STDFUFILES_NOERROR)
                    {
                        if (STDFUAPI.STDFUFILES_GetImageAlternate(Image, ref Alt) == STDFUAPI.STDFUFILES_NOERROR)
                        {
                            if (Alt == TargetSel)
                            {
                                hImage = Image;
                                bFound = true;
                                break;
                            }
                        }
                        STDFUAPI.STDFUFILES_DestroyImage(ref Image);
                    }
                }
                STDFUAPI.STDFUFILES_CloseDFUFile(hFile);
            }
            else
            {
                Context.ErrorCode = dwRet;
                HandleError(Context);
            }

            if (!bFound)
            {
                PrintText +=("Unable to find data for that device/target from the file ...");
                return 1;
            }
            else
            {
                // prepare the asynchronous operation: first is erase !
                Context.szDevLink = Device[m_CurrentDevice].DevicePath;
                Context.DfuGUID = DFU_GUID;
                Context.Operation = STDFUAPI.OPERATION_ERASE;
                Context.bDontSendFFTransfersForUpgrade = Optimize;
                if (m_BufferedImage != IntPtr.Zero)
                    STDFUAPI.STDFUFILES_DestroyImage(ref m_BufferedImage);
                // Let's backup our data before the filtering for erase. The data will be used for the upgrade phase
                STDFUAPI.STDFUFILES_DuplicateImage(hImage,ref m_BufferedImage);
                STDFUAPI.MAPPING pMappinglength = new STDFUAPI.MAPPING();
                STDFUAPI.STDFUFILES_FilterImageForOperation(m_BufferedImage,IntPtr.Add( m_pMapping, TargetSel * Marshal.SizeOf(pMappinglength)), STDFUAPI.OPERATION_UPGRADE, Optimize);
                STDFUAPI.STDFUFILES_FilterImageForOperation(hImage, IntPtr.Add(m_pMapping, TargetSel * Marshal.SizeOf(pMappinglength)), STDFUAPI.OPERATION_ERASE, Optimize);
                Context.hImage = hImage;

                //printf("0 KB(0 Bytes) of %i KB(%i Bytes) \n", STDFUFILES_GetImageSize(m_BufferedImage)/1024,  STDFUFILES_GetImageSize(m_BufferedImage));


                startTime = DateTime.Now;
                IntPtr pContext = Marshal.AllocHGlobal(Marshal.SizeOf(Context));
                Marshal.StructureToPtr(Context, pContext, true);

                dwRet = STDFUAPI.STDFUPRT_LaunchOperation(pContext, ref m_OperationCode);

                Context = (STDFUAPI.DFUThreadContext)Marshal.PtrToStructure(pContext, typeof(STDFUAPI.DFUThreadContext));
                Marshal.FreeHGlobal(pContext);
                if (dwRet != STDFUAPI.STDFUPRT_NOERROR)
                {
                    Context.ErrorCode = dwRet;
                    HandleError(Context);
                    return 1;
                }
                else
                {

                    return 0;
                }
            }
        }
        private int LaunchVerify()
        {
            STDFUAPI.DFUThreadContext Context = new STDFUAPI.DFUThreadContext();
            bool bFound = false;
            uint dwRet;
            int i, TargetSel = m_CurrentTarget;
            IntPtr hImage = IntPtr.Zero;
            IntPtr hFile = IntPtr.Zero;
            byte NbTargets = 0;

            // Get the image of the selected target
            ushort vpd = 0;
            dwRet = STDFUAPI.STDFUFILES_OpenExistingDFUFile(m_DownFileName, ref hFile, ref vpd, ref vpd , ref vpd, ref NbTargets);
            if (dwRet == STDFUAPI.STDFUFILES_NOERROR)
            {
                for (i = 0; i < NbTargets; i++)
                {
                    IntPtr Image = IntPtr.Zero;
                    byte Alt = 0;

                    if (STDFUAPI.STDFUFILES_ReadImageFromDFUFile(hFile, i, ref Image) == STDFUAPI.STDFUFILES_NOERROR)
                    {
                        if (STDFUAPI.STDFUFILES_GetImageAlternate(Image, ref Alt) == STDFUAPI.STDFUFILES_NOERROR)
                        {
                            if (Alt == TargetSel)
                            {
                                hImage = Image;
                                bFound = true;
                                break;
                            }
                        }
                        STDFUAPI.STDFUFILES_DestroyImage(ref Image);
                    }
                }
                STDFUAPI.STDFUFILES_CloseDFUFile(hFile);
            }
            else
            {
                Context.ErrorCode = dwRet;
                HandleError(Context);
            }

            if (!bFound)
            {
                PrintText +=("Unable to find data for that target in the dfu file...");
                return 1;
            }
            else
            {

                // prepare the asynchronous operation
                Context.szDevLink = Device[m_CurrentDevice].DevicePath;
                Context.DfuGUID = DFU_GUID;
                Context.Operation = STDFUAPI.OPERATION_UPLOAD;
                if (m_BufferedImage != IntPtr.Zero)
                    STDFUAPI.STDFUFILES_DestroyImage(ref m_BufferedImage);
                STDFUAPI.MAPPING pMappinglength = new STDFUAPI.MAPPING();
                STDFUAPI.STDFUFILES_FilterImageForOperation(hImage, IntPtr.Add(m_pMapping, TargetSel * Marshal.SizeOf(pMappinglength)) , STDFUAPI.OPERATION_UPLOAD, false);
                Context.hImage = hImage;
                // Let's backup our data before the upload. The data will be used after the upload for comparison
                STDFUAPI.STDFUFILES_DuplicateImage(hImage,ref m_BufferedImage);

                startTime = DateTime.Now;
                IntPtr pContext = Marshal.AllocHGlobal(Marshal.SizeOf(Context));
                Marshal.StructureToPtr(Context, pContext, true);

                dwRet = STDFUAPI.STDFUPRT_LaunchOperation(pContext,ref m_OperationCode);

                Context = (STDFUAPI.DFUThreadContext)Marshal.PtrToStructure(pContext, typeof(STDFUAPI.DFUThreadContext));
                Marshal.FreeHGlobal(pContext);

                if (dwRet != STDFUAPI.STDFUPRT_NOERROR)
                {
                    Context.ErrorCode = dwRet;
                    HandleError(Context);
                    return 1;
                }
                else
                {

                    return 0;
                }
            }
        }



        public void UploadDFUFile(string fileName)
        {
            m_UpFileName = fileName;
            if (File.Exists(fileName) == false)
            {
                FileStream openFile = new FileStream(fileName, FileMode.Create);
                openFile.Flush();
                openFile.Close();
            }
            m_CurrentTarget = 0;
            LaunchUpload();
            threadTimer.Change(0, 600);
            
        }

        public void UploadDFUFile(string fileName,int Target)
        {
            m_UpFileName = fileName;
            if (File.Exists(fileName) == false)
            {
                FileStream openFile = new FileStream(fileName, FileMode.Create);
                openFile.Flush();
                openFile.Close();
            }
            m_CurrentTarget = Target;
            LaunchUpload();
            threadTimer.Change(0, 600);

        }

        public void UpgradeDFUFile(string fileName)
        {
            m_DownFileName = fileName;
            if (File.Exists(fileName) == false)
            {
                return;
            }
            m_CurrentTarget = 0;
            Verify = true;
            Optimize = true;
            LaunchUpgrade();
            threadTimer.Change(0, 600);

        }

        public void UpgradeDFUFile(string fileName , bool isVerify)
        {
            m_DownFileName = fileName;
            if (File.Exists(fileName) == false)
            {
                return;
            }
            m_CurrentTarget = 0;
            Verify = isVerify;
            Optimize = true;
            LaunchUpgrade();
            threadTimer.Change(0, 600);

        }

        public void UpgradeDFUFile(string fileName, bool isVerify,bool isOptimize)
        {
            m_DownFileName = fileName;
            if (File.Exists(fileName) == false)
            {
                return;
            }
            m_CurrentTarget = 0;
            Verify = isVerify;
            Optimize = isOptimize;
            LaunchUpgrade();
            threadTimer.Change(0, 600);
        }


        public void OnCancel()
        {
            bool bStop = true;
            STDFUAPI.DFUThreadContext Context = new STDFUAPI.DFUThreadContext();

            if (m_OperationCode > 0)
            {
                bStop = false;
                if (MessageBox.Show("Operation on-going. Leave anyway ?", "Warning", MessageBoxButton.OKCancel) == MessageBoxResult.OK) 
                    bStop = true;
            }

            if (bStop)
            {
                if (m_OperationCode>0)
                {
                    IntPtr pContext = Marshal.AllocHGlobal(Marshal.SizeOf(Context));
                    Marshal.StructureToPtr(Context, pContext, true);
                    STDFUAPI.STDFUPRT_StopOperation(m_OperationCode, pContext);
                    Context = (STDFUAPI.DFUThreadContext)Marshal.PtrToStructure(pContext, typeof(STDFUAPI.DFUThreadContext));
                    Marshal.FreeHGlobal(pContext);
                }


                STDFUAPI.STDFUFILES_DestroyImage(ref Context.hImage);
                if (m_BufferedImage != IntPtr.Zero)
                    STDFUAPI.STDFUFILES_DestroyImage(ref m_BufferedImage);

                //	KillTimer(1);
                if (m_pMapping != IntPtr.Zero)
                    STDFUAPI.STDFUPRT_DestroyMapping(ref m_pMapping);
                /*	POSITION Pos=m_DetachedDevs.GetStartPosition();
                    while (Pos)
                    {
                        CString Key;
                        void *Value;

                        m_DetachedDevs.GetNextAssoc(Pos, Key, Value);
                        delete (PUSB_DEVICE_DESCRIPTOR)Value;
                    }
                    m_DetachedDevs.RemoveAll();*/
                m_pMapping = IntPtr.Zero;
                m_NbAlternates = 0;

            }
        }
        #region DeviceNotification
        public void RegisterDeviceNotification(IntPtr WindowHandel)
        {
            IntPtr hDevNotify;

            STDFUAPI.DEV_BROADCAST_DEVICEINTERFACE NotificationFilter = new STDFUAPI.DEV_BROADCAST_DEVICEINTERFACE();
            NotificationFilter.dbcc_size = (uint)Marshal.SizeOf(NotificationFilter);
            NotificationFilter.dbcc_devicetype = STDFUAPI.DBT_DEVTYP_DEVICEINTERFACE;
            NotificationFilter.dbcc_classguid = DFU_GUID;
            IntPtr pNotificationFilter = Marshal.AllocHGlobal(Marshal.SizeOf(NotificationFilter));
            Marshal.StructureToPtr(NotificationFilter, pNotificationFilter, true);
            hDevNotify = STDFUAPI.RegisterDeviceNotification(WindowHandel, pNotificationFilter, STDFUAPI.DEVICE_NOTIFY_WINDOW_HANDLE);
            if (hDevNotify == IntPtr.Zero)
            {
                MessageBox.Show("注册USB设备通知失败", "错误");
            }
            Marshal.FreeHGlobal(pNotificationFilter);
            HwndSource hwndSource = HwndSource.FromHwnd(WindowHandel);
            hwndSource.AddHook(new HwndSourceHook(WndProc));
        }

        public event EventHandler<DeviceNotificationEventArgs> DeviceNotificationFilter;
        protected virtual void OnDeviceNotificationFilter(DeviceNotificationEventArgs e)
        {
            DeviceNotificationFilter?.Invoke(this, e);
        }
        public  IntPtr WndProc( IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            //if (lParam == IntPtr.Zero) return IntPtr.Zero;
            //STDFUAPI.DEV_BROADCAST_HDR lpdb = (STDFUAPI.DEV_BROADCAST_HDR)Marshal.PtrToStructure(lParam, typeof(STDFUAPI.DEV_BROADCAST_HDR));
            switch (msg)
            {
                case STDFUAPI.WM_DEVICECHANGE:
                    handled = true;
                    if ((int)wParam == STDFUAPI.DBT_DEVICEARRIVAL)
                    {
                        // 设备插入  
                        OnDeviceNotificationFilter(new DeviceNotificationEventArgs(true));
                    }
                    else if ((int)wParam == STDFUAPI.DBT_DEVICEREMOVECOMPLETE)
                    {
                        // 设备拔出  
                        OnDeviceNotificationFilter(new DeviceNotificationEventArgs(false));
                    }
                    break;

            }
            return IntPtr.Zero;
        }
        public  class DeviceNotificationEventArgs: EventArgs
        {
            public DeviceNotificationEventArgs(bool isArrived)
            {
                IsArrived = isArrived;
            }
            public DeviceNotificationEventArgs()
            {
                IsArrived = false;
            }
            public bool IsArrived;
        }
#endregion
    }
}
