using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace UFlash_config
{
    public struct sTCAN_MSG
    {
        /** 11/29 Bit */
        public UInt32 m_unMsgID;

        /** true, for (29 Bit) Frame */
        public byte m_ucEXTENDED;

        /** true, for remote request */
        public byte m_ucRTR;

        /** Data Length (0..8, 12, 16, 20, 24, 32, 48, 64) */
        public byte m_ucDataLen;

        /** Message Length */
        public byte m_ucChannel;

        /** Databytes 0..63 */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public byte[] m_ucData;

        /* CAN FD frame */
        public bool m_bCANFD;
    }


    public struct sTCANDATA
    {
        private int m_nSortField;

        /** Multiplication factor */
        private int m_nMFactor;

        /** Type of the message */
        public Byte m_ucDataType;

        /** Time stamp, Contains the val returned from QueryPerf..Counter() */
        public long m_lTickCount;

        public sTDATAINFO m_uDataInfo;

    }
    [StructLayout(LayoutKind.Explicit)]
    public struct sTDATAINFO
    {
        /** The received / transmitted message */
        [FieldOffset(0)]
        public sTCAN_MSG m_sCANMsg;

        /** error info */
        //[FieldOffset(0)]
        //public sERROR_INFO m_sErrInfo;
    }
    public partial class UFLASH_PROC 
    {
        
        public sTCAN_MSG[] sSTCAN_MSG_Tx;
        public sTCANDATA[] sg_receiveBuff = new sTCANDATA[10];
        public sTCAN_MSG STCAN_MSG_Tx;
        public sTCAN_MSG STCAN_MSG_RX;
        public sTCANDATA[] sg_asMsgBuffer = new sTCANDATA[2000];
        
        [StructLayout(LayoutKind.Sequential)]
        public struct NeoDevice
        {
            public UInt32 DeviceType;
            public int Handle;
            public int NumberOfClients;
            public int SerialNumber;
            public int MaxAllowedClients;
        }
        public NeoDevice[] sg_ndNeoToOpen = new NeoDevice[3];  //MaxDevice number 3, max -->255

        [StructLayout(LayoutKind.Sequential)]
        public struct icsSpyMessage
        {
            public UInt32 StatusBitField;
            public UInt32 StatusBitField2;
            public UInt32 TimeHardware;
            public UInt32 TimeHardware2;
            public UInt32 TimeSystem;
            public UInt32 TimeSystem2;
            public Byte TimeStampHardwareID;
            public Byte TimeStampSystemID;
            public Byte NetworkID;
            public Byte NodeID;
            public Byte Protocol;
            public Byte MessagePieceID;
            public Byte ExtraDataPtrEnabled;
            public Byte NumberBytesHeader;
            public Byte NumberBytesData;
            public short DescriptionID;
            public int ArbIDOrHeader;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public Byte[] Data;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public Byte[] Ack;
            public uint ExtraDataPtr;
            public Byte MiscData;
        }
        public icsSpyMessage[] s_asSpyMsg = new icsSpyMessage[20000];



        public struct CNetwork
        {
            public Byte m_nNoOfChannels;
            public Byte m_nNoOfDevices;
            public Byte m_hHardwareHandle;
            public Byte m_hNetworkHandle;
        }
        public CNetwork sg_odHardwareNetwork = new CNetwork();

        public struct tagDATINDSTR
        {
            public bool m_bIsConnected;
            public IntPtr m_hHandle;
            public bool m_bToContinue;
            public Byte m_unChannels;
        }

        public tagDATINDSTR s_DatIndThread = new tagDATINDSTR();

        [StructLayout(LayoutKind.Explicit)]
        public struct sTDATAINFO
        {
            /** The received / transmitted message */
            [FieldOffset(0)]
            public sTCAN_MSG m_sCANMsg;

            /** error info */
            //[FieldOffset(0)]
            //public sERROR_INFO m_sErrInfo;
        }



        [StructLayout(LayoutKind.Explicit, Size = 8)]
        public struct LARGE_INTEGER
        {
            [FieldOffset(0)]
            public Int64 QuadPart;
            [FieldOffset(0)]
            public UInt32 LowPart;
            [FieldOffset(4)]
            public Int32 HighPart;
        }
        public long sg_QueryTickCount;
        public long sg_lnFrequency;
        /* neo define */
        public int NEOVI_OK = 1;
        public const int NETID_HSCAN = 1;
        public const int NETID_MSCAN = 2;
        //Upper boundry of net id's
        public int NETID_MAX = 100;
        //int NEODEVICE_UNKNOWN = 0;
        public const int NEODEVICE_BLUE = 1;
        public const int NEODEVICE_SW_VCAN = 2;
        public const int NEODEVICE_DW_VCAN = 4;
        public const int NEODEVICE_FIRE = 8;
        public const int NEODEVICE_VCAN3 = 16;
        public int m_anhObject;   // Device key param
        public Byte sg_ucNoOfHardware = 0;
        public Byte sg_nNoOfChannels = 0;
        public const int TOTAL_ERROR = 600;
        public const int MAX_BUFFER_VALUECAN = 10000;
        public const int WAITTIME_NEOVI = 100;
        public const int NETWORKS_COUNT = 10;
        public const int ENTRIES_IN_GBUF = 2000;
        public UInt32 NEODEVICE_ALL = 0xFFFFFFFF;
        public const int MAX_DEVICES = 10;
        public int[] s_anErrorCodes = new int[TOTAL_ERROR];
        public long sg_TimeStamp = 0;
        public long sg_CurrSysTime = 0;
        // icsSpyDataCommon
        // constants for the status bitfield
        public const long SPY_STATUS_GLOBAL_ERR = 0x01;
        public const long SPY_STATUS_TX_MSG = 0x02;
        public const long SPY_STATUS_XTD_FRAME = 0x04;
        public const long SPY_STATUS_REMOTE_FRAME = 0x08;

        public const long SPY_STATUS_CRC_ERROR = 0x10;
        public const long SPY_STATUS_CAN_ERROR_PASSIVE = 0x20;
        public const long SPY_STATUS_INCOMPLETE_FRAME = 0x40;
        public const long SPY_STATUS_LOST_ARBITRATION = 0x80;

        public const long SPY_STATUS_UNDEFINED_ERROR = 0x100;
        public const long SPY_STATUS_CAN_BUS_OFF = 0x200;
        public const long SPY_STATUS_BUS_RECOVERED = 0x400;
        public const long SPY_STATUS_BUS_SHORTED_PLUS = 0x800;

        public const long SPY_STATUS_BUS_SHORTED_GND = 0x1000;
        public const long SPY_STATUS_CHECKSUM_ERROR = 0x2000;
        public const long SPY_STATUS_BAD_MESSAGE_BIT_TIME_ERROR = 0x4000;
        public const long SPY_STATUS_TX_NOMATCH = 0x8000;

        public const long SPY_STATUS_COMM_IN_OVERFLOW = 0x10000;
        //const long SPY_STATUS_COMM_OUT_OVERFLOW = 0x20000;
        public const long SPY_STATUS_EXPECTED_LEN_MISMATCH = 0x20000;
        //const long SPY_STATUS_COMM_MISC_ERROR = 0x40000;
        public const long SPY_STATUS_MSG_NO_MATCH = 0x40000;
        public const long SPY_STATUS_BREAK = 0x80000;

        public const long SPY_STATUS_AVSI_REC_OVERFLOW = 0x100000;
        public const long SPY_STATUS_TEST_TRIGGER = 0x200000;
        public const long SPY_STATUS_AUDIO_COMMENT = 0x400000;
        public const long SPY_STATUS_GPS_DATA = 0x800000;

        public const long SPY_STATUS_ANALOG_DIGITAL_INPUT = 0x1000000;
        public const long SPY_STATUS_TEXT_COMMENT = 0x2000000;
        public const long SPY_STATUS_NETWORK_MESSAGE_TYPE = 0x4000000;
        public const long SPY_STATUS_VSI_TX_UNDERRUN = 0x8000000;

        public const long SPY_STATUS_VSI_IFR_CRC_BIT = 0x10000000;
        public const long SPY_STATUS_INIT_MESSAGE = 0x20000000;
        public const long SPY_STATUS_LIN_MASTER = 0x20000000;
        public const long SPY_STATUS_HIGH_SPEED = 0x40000000;
        public const long SPY_STATUS_EXTENDED = 0x80000000; // if this bit is set than decode status bitfield3 in the ackbytes

        public const long VALUECAN_ERROR_BITS = SPY_STATUS_GLOBAL_ERR | SPY_STATUS_CRC_ERROR |
                                 SPY_STATUS_INCOMPLETE_FRAME | SPY_STATUS_UNDEFINED_ERROR
                                 | SPY_STATUS_BAD_MESSAGE_BIT_TIME_ERROR;

        public const byte TX_FLAG = 0x01;
        public const byte RX_FLAG = 0x02;
        public const byte ERR_FLAG = 0x04;
        public const byte INTR_FLAG = 0x08;

        public const Byte CREATE_MAP_TIMESTAMP = 0x1;
        public const Byte CALC_TIMESTAMP_READY = 0x2;

        public byte[] m_bytNetworkIDs = new byte[MAX_DEVICES];
        public static Byte sg_byCurrState = CREATE_MAP_TIMESTAMP;

        /* icsneo40.dll Basic Functions */
        [DllImport("icsneo40.dll")]
        public static extern int icsneoFindNeoDevices(UInt32 DeviceTypes, [Out] NeoDevice[] pNeoDevice, ref int pNumDevices);

        [DllImport("icsneo40.dll")]
        public static extern int icsneoOpenNeoDevice(ref NeoDevice pNeoDevice, ref int hObject, ref Byte bNetworkIDs, int bConfigRead, int bSyncToPC);

        [DllImport("icsneo40.dll")]
        public static extern int icsneoClosePort(int hObject, ref int pNumberOfErrors);

        /* icsneo40.dll Message Functions */
        [DllImport("icsneo40.dll")]
        public static extern int icsneoTxMessages(Int32 hObject, ref icsSpyMessage pMsg, int lNetworkID, int lNumMessages);

        [DllImport("icsneo40.dll")]
        public static extern int icsneoGetMessages(Int32 hObject, [Out] icsSpyMessage[] pMsg, ref Int32 pNumberOfMessages, ref Int32 pNumberOfErrors);

        [DllImport("icsneo40.dll")]
        public static extern int icsneoWaitForRxMessagesWithTimeOut(Int32 hObject, uint iTimeout);

        [DllImport("icsneo40.dll")]
        public static extern int icsneoGetTimeStampForMsg(int hObject, ref icsSpyMessage pMsg, ref double pTimeStamp);

        /* icsneo40.dll Device Functions */
        [DllImport("icsneo40.dll")]
        public static extern int icsneoGetHardwareLicense(int hObject, ref int pnHardwareLic);

        /* icsneo40.dll Error Functions */
        [DllImport("icsneo40.dll")]
        //typedef int (__stdcall* GETLASTAPIERROR)(int hObject, unsigned long* pErrorNumber);
        public static extern int icsneoGetLastAPIError(int hObject, ref UInt64 pErrorNumber);

        [DllImport("icsneo40.dll")]
        public static extern int icsneoGetErrorMessages(int hObject, ref int[] pErrorMsgs, ref int pNumberOfErrors);

        [DllImport("icsneo40.dll")]
        public static extern int icsneoGetErrorInfo(int lErrorNumber, ref char szErrorDescriptionShort, ref char szErrorDescriptionLong, ref int lMaxLengthShort,
                                                                              ref int lMaxLengthLong, ref int lErrorSeverity, ref int lRestartNeeded);

        [DllImport("kernel32.dll")]
        public extern static short QueryPerformanceCounter(ref long x);

        [DllImport("kernel32.dll")]
        public extern static short QueryPerformanceFrequency(ref long x);

        public void QPCounter(ref long x)
        {
            QueryPerformanceCounter(ref x);
        }
        public void QPFrequency(ref long x)
        {
            QueryPerformanceFrequency(ref x);
        }
        public long Ms_To_Ticks(int MilSec)
        {
            long freq = 0;
            QueryPerformanceFrequency(ref freq);
            return (MilSec * freq / 1000);
        }

        /**
        * \param[in] bConnect TRUE to Connect, FALSE to Disconnect
        * \return Returns S_OK if successful otherwise corresponding Error code.
        *
        * This function will connect the tool with hardware. This will
        * establish the data link between the application and hardware.
        * Parallel Port Mode: Controller will be disconnected.
        * USB: Client will be disconnected from the network
        */
        public int nConnect(bool bConnect)
        {
            int nReturn = -1;

            if (bConnect) //Disconnected and to be connected
            {
                bool bFound = false;
                if (sg_ucNoOfHardware != 0)
                {
                    bFound = true;
                }
                if (bFound == true)
                {
                    nReturn = NEOVI_OK;
                }
                else
                {
                    nReturn = nInitHwNetwork();
                }
                if (nReturn != NEOVI_OK)
                {
                    nDisconnectFromDriver();
                    // Try again
                    nReturn = nInitHwNetwork();

                }
                if (nReturn == NEOVI_OK)
                {
                    //nSetBaudRate();
                    vCreateTimeModeMapping();
                    nReturn = 1;
                }
                else
                {
                    nReturn = -1;
                }
            }
            return nReturn;
        }
        /**
            * Function to create time mode mapping
        */
        public void vCreateTimeModeMapping()
        {
            //MessageBox(0, L"TIME", L"", 0);
            sg_CurrSysTime = GetTimeStamp(false);
            //Query Tick Count
            QueryPerformanceCounter(ref sg_QueryTickCount);
            // Get frequency of the performance counter
            QueryPerformanceFrequency(ref sg_lnFrequency);
            // Convert it to time stamp with the granularity of hundreds of microsecond
            if ((sg_QueryTickCount * 10000) > sg_lnFrequency)
            {
                sg_TimeStamp = (sg_QueryTickCount * 10000) / sg_lnFrequency;
            }
            else
            {
                sg_TimeStamp = (sg_QueryTickCount / sg_lnFrequency) * 10000;
            }
        }
        /// 获取当前时间戳  
        /// </summary>  
        /// <param name="bflag">为真时获取10位时间戳,为假时获取13位时间戳.bool bflag = true</param>  
        /// <returns></returns>  
        public static long GetTimeStamp(bool bflag)
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            long ret = 0;
            if (bflag)
                ret = System.Convert.ToInt64(ts.TotalSeconds);
            else
                ret = System.Convert.ToInt64(ts.TotalMilliseconds);

            return ret;
        }
        /**
        * \return Operation Result. 0 incase of no errors. Failure Error codes otherwise.
        *
        * This is USB Specific function.This function will create
        * find number of hardware connected. It will create network as
        * per hardware count. This will popup hardware selection dialog
        * in case there are more hardware present.
        */
        public int nInitHwNetwork()
        {
            int nDevices = 0;
            int nReturn = 0;
            nDevices = nGetNoOfConnectedHardware(nDevices);

            //m_bInSimMode = (nDevices == 0);

            // Assign the device count
            sg_ucNoOfHardware = (Byte)nDevices;


            if (nDevices == 0)
            {
                //m_omMsg = "No Device found";
            }
            else
            {
                if (nDevices > 1
                    /* sg_ndNeoToOpen[0].DeviceType == NEODEVICE_VCAN3 */)
                {
                    //nReturn = nCreateMultipleHardwareNetwork(unDefaultChannelCnt);                   
                }
                else
                {
                    nReturn = nCreateSingleHardwareNetwork();
                    //m_omMsg = "Creating Single HardwareNetwork... \n";
                }
            }
            return nReturn;

        }
        /**
         * 
         * \param[in] nHardwareCount Hardware Count
         * \return Returns S_OK if successful otherwise corresponding Error code.
         *
         * Finds the number of hardware connected. This is applicable
         * only for USB device. For parallel port this is not required.
         */
        public int nGetNoOfConnectedHardware(int nHardwareCount)
        {
            nHardwareCount = 32;
            int nReturn = 0;
            if (icsneoFindNeoDevices(NEODEVICE_ALL, sg_ndNeoToOpen, ref nHardwareCount) > 0)
            {
                if (nHardwareCount == 0)
                {
                    //m_omMsg = "Query successful, but no device found" + "\n";
                }
                nReturn = nHardwareCount;
                //m_omMsg = nHardwareCount + " device found" + "\n";
            }
            else
            {
                nHardwareCount = 0;
                //m_omMsg = "Query for devices unsuccessful" + "\n";
            }
            // Return the operation result
            return nReturn;
        }
        /**
        * This is USB Specific Function. This will create a single
        * network with available single hardware.
        */
        public int nCreateSingleHardwareNetwork()
        {
            // Create network here with net handle 1
            s_DatIndThread.m_bToContinue = true;
            s_DatIndThread.m_bIsConnected = false;
            s_DatIndThread.m_unChannels = 1;

            int nResult = icsneoOpenNeoDevice(ref sg_ndNeoToOpen[0], ref m_anhObject, ref sg_nNoOfChannels, 1, 0);

            //If connection fails
            if (nResult != NEOVI_OK)
            {
                return -1;
            }
            //icsneoClosePort(m_anhObject, ref nResult);

            // Set the number of channels
            sg_odHardwareNetwork.m_nNoOfChannels = 1;
            sg_odHardwareNetwork.m_nNoOfDevices = 1;
            sg_nNoOfChannels = 1;
            // Assign hardware handle
            sg_odHardwareNetwork.m_hHardwareHandle = (Byte)sg_ndNeoToOpen[0].Handle;

            // Assign Net Handle
            sg_odHardwareNetwork.m_hNetworkHandle = m_bytNetworkIDs[0] = NETID_HSCAN;


            return 1;
        }

        /**
        * \param[in]  CurrSpyMsg Message polled from the bus in neoVI format
        * \param[out] sCanData Application specific data format
        * \param[in]  unChannel channel
        * \return TRUE (always)
        *
        * This will classify the messages, which can be one of Rx, Tx or
        * Error messages. In case of Err messages this identifies under
        * what broader category (Rx / Tx) does this occur.
        */
        public bool bClassifyMsgType(ref icsSpyMessage CurrSpyMsg,
           ref sTCANDATA sCanData)
        {
            if ((long.Parse(CurrSpyMsg.StatusBitField.ToString()) & long.Parse(VALUECAN_ERROR_BITS.ToString())) > 0)
            {
                sCanData.m_ucDataType = ERR_FLAG;
            }
            else if ((long.Parse(CurrSpyMsg.StatusBitField.ToString()) & long.Parse(SPY_STATUS_CAN_BUS_OFF.ToString())) > 0)
            {
                sCanData.m_ucDataType = ERR_FLAG;

            }
            else
            {
                //Check for RTR Message
                if ((long.Parse(CurrSpyMsg.StatusBitField.ToString()) & long.Parse(SPY_STATUS_REMOTE_FRAME.ToString())) > 0)
                {
                    sCanData.m_ucDataType = RX_FLAG;
                    sCanData.m_uDataInfo.m_sCANMsg.m_ucRTR = 1;
                }
                else
                {
                    sCanData.m_uDataInfo.m_sCANMsg.m_ucRTR = 0;
                }
                if ((long.Parse(CurrSpyMsg.StatusBitField.ToString()) & long.Parse(SPY_STATUS_TX_MSG.ToString())) > 0)
                {
                    sCanData.m_ucDataType = TX_FLAG;
                }
                else if ((long.Parse(CurrSpyMsg.StatusBitField.ToString()) & long.Parse(SPY_STATUS_NETWORK_MESSAGE_TYPE.ToString())) > 0)
                {
                    sCanData.m_ucDataType = RX_FLAG;
                }

                // Copy data length
                sCanData.m_uDataInfo.m_sCANMsg.m_ucDataLen = CurrSpyMsg.NumberBytesData;

                // Copy the message data
                sCanData.m_uDataInfo.m_sCANMsg.m_ucData = CurrSpyMsg.Data;

                // Copy the message ID
                sCanData.m_uDataInfo.m_sCANMsg.m_unMsgID = (uint)CurrSpyMsg.ArbIDOrHeader;

                // Check for extended message indication
                if ((long.Parse(CurrSpyMsg.StatusBitField.ToString()) & long.Parse(SPY_STATUS_XTD_FRAME.ToString())) > 0)
                {
                    sCanData.m_uDataInfo.m_sCANMsg.m_ucEXTENDED = 1;
                }
                else
                {
                    sCanData.m_uDataInfo.m_sCANMsg.m_ucEXTENDED = 0;
                }


            }
            return true;
        }
        /**
        * \param[in] psCanDataArray Pointer to CAN Message Array of Structures
        * \param[in] nMessage Maximun number of message to read or size of the CAN Message Array
        * \param[out] Message Actual Messages Read
        * \return Returns S_OK if successful otherwise corresponding Error code.
        *
        * This function will read multiple CAN messages from the driver.
        * The other fuctionality is same as single message read. This
        * will update the variable nMessage with the actual messages
        * read.
        */
        public int nReadMultiMessage(sTCANDATA[] psCanDataArray, ref int nMessage /* int nChannelIndex*/)
        {
            int i = 0;
            int nReturn = 0;
            int s_CurrIndex = 0;
            Int32 s_Messages = 0;
            Int32 nErrMsg = 0;
            if (s_CurrIndex == 0)
            {
                nReturn = icsneoGetMessages(m_anhObject, s_asSpyMsg, ref s_Messages, ref nErrMsg);
            }
            ushort ushRxErr = 0, ushTxErr = 0;
            if (nErrMsg > 0)
            {
                int nErrors = 0;
                nReturn = icsneoGetErrorMessages(m_anhObject, ref s_anErrorCodes, ref nErrors);
                if ((nReturn == NEOVI_OK) && (nErrors > 0))
                {
                    for (int j = 0; j < nErrors; j++)
                    {
                        switch (s_anErrorCodes[j])
                        {
                            case 2:   //NEOVI_ERROR_DLL_USB_SEND_DATA_ERROR
                                {
                                    ++ushTxErr;
                                }
                                break;
                            case 39: // NEOVI_ERROR_DLL_RX_MSG_FRAME_ERR
                            case 40: //NEOVI_ERROR_DLL_RX_MSG_FIFO_OVER
                            case 41: //NEOVI_ERROR_DLL_RX_MSG_CHK_SUM_ERR
                                {
                                    ++ushRxErr;
                                }
                                break;
                            default:
                                {
                                    // Do nothing until further clarification is received
                                }
                                break;
                        }
                    }
                }
            }
            // End of first level of error message processing

            // START
            /* Create the time stamp map. This means getting the local time and assigning
            offset value to the QuadPart.
            */
            //UInt64 QuadPartRef = 0;
            if (CREATE_MAP_TIMESTAMP == sg_byCurrState)
            {
                icsSpyMessage CurrSpyMsg = new icsSpyMessage();
                CurrSpyMsg = s_asSpyMsg[s_CurrIndex];

                double dTimestamp = 0;
                nReturn = icsneoGetTimeStampForMsg(m_anhObject, ref CurrSpyMsg, ref dTimestamp);
                if (nReturn == NEOVI_OK)
                {
                    sg_byCurrState = CALC_TIMESTAMP_READY;
                    nReturn = 1;

                    long g_QueryTickCount;
                    g_QueryTickCount = 0;
                    QueryPerformanceCounter(ref g_QueryTickCount);

                    long unConnectionTimer;
                    unConnectionTimer = ((g_QueryTickCount * 10000) / sg_lnFrequency) - sg_TimeStamp;
                    if ((dTimestamp * 10000) >= unConnectionTimer)
                    {
                        sg_TimeStamp = (long)((dTimestamp * 10000) - unConnectionTimer);
                    }
                    else
                    {
                        sg_TimeStamp = (long)(unConnectionTimer - (dTimestamp * 10000));
                    }
                }
                else
                {
                    nReturn = -1;
                }
            }
            //End
            int nLimForAppBuf = nMessage;
            bool bChannelCnfgrd = false;
            for (; (i < nLimForAppBuf) && (s_CurrIndex < s_Messages);)
            {
                sTCANDATA sCanData = new sTCANDATA();
                icsSpyMessage CurrSpyMsg = new icsSpyMessage();
                CurrSpyMsg = s_asSpyMsg[s_CurrIndex];
                if (true)
                {
                    bChannelCnfgrd = true;
                    sCanData.m_uDataInfo.m_sCANMsg.m_ucChannel = 0x01;  // tbd
                    double dTimestamp = 0;
                    icsneoGetTimeStampForMsg(m_anhObject, ref CurrSpyMsg, ref dTimestamp);
                    sCanData.m_lTickCount = (long)(dTimestamp * 10000);
                    bClassifyMsgType(ref CurrSpyMsg, ref sCanData);
                    psCanDataArray[i] = sCanData;
                }
                s_CurrIndex++;
                if (bChannelCnfgrd)
                {
                    i++;
                }
            }

            if ((s_CurrIndex == MAX_BUFFER_VALUECAN) || (s_CurrIndex == s_Messages))
            {
                s_CurrIndex = 0;
                s_Messages = 0;
            }

            nMessage = i;

            return 1;
        }
        public int CAN_Transmit(ref CAN_STAND_ADAPT[] sendbuf, uint sMsgStart,uint sMsgLength)
        {
            STCAN_MSG_Tx.m_ucChannel = 1;
            STCAN_MSG_Tx.m_bCANFD = false;
            STCAN_MSG_Tx.m_ucRTR = 0x0;
            STCAN_MSG_Tx.m_ucEXTENDED = 0x0;
            int count = 0;
            for (int i = 0; i < sMsgLength; i++)
            {
                STCAN_MSG_Tx.m_ucData = sendbuf[sMsgStart+i].Data;
                STCAN_MSG_Tx.m_unMsgID = sendbuf[sMsgStart + i].ID;
                STCAN_MSG_Tx.m_ucDataLen = sendbuf[sMsgStart + i].DataLen;
                if (1 != nWriteMessage(STCAN_MSG_Tx))
                {
                    return 0;
                }
                string[] Tx = CANTxMsgToString(STCAN_MSG_Tx);
                string msg = Tx[0] + "\t" + Tx[2] + "\t" + Tx[4] + "\t" + Tx[1] + "\t" + "d" + "  " + Tx[5] + "\t" + Tx[6];
                WriteGenLog(msg, "UDS");
                count++;
            }
            return count;

        }

        /// <summary>
        /// Transmit CAN message via ES581.3
        /// </summary>
        public int nWriteMessage(sTCAN_MSG sMessage)
        {
            int nReturn = 0;

            icsSpyMessage SpyMsg = new icsSpyMessage();
            SpyMsg.ArbIDOrHeader = (int)sMessage.m_unMsgID;
            SpyMsg.NumberBytesData = sMessage.m_ucDataLen;
            SpyMsg.StatusBitField = 0;
            SpyMsg.StatusBitField2 = 0;
            if (sMessage.m_ucRTR == 1)
            {
                SpyMsg.StatusBitField |= (uint)SPY_STATUS_REMOTE_FRAME;
            }
            if (sMessage.m_ucEXTENDED == 1)
            {
                SpyMsg.StatusBitField |= (uint)SPY_STATUS_XTD_FRAME;
            }
            SpyMsg.Data = sMessage.m_ucData;  // what if data> 8Byte?
            if (icsneoTxMessages(m_anhObject, ref SpyMsg, 1, 1) != 0)
            {
                nReturn = 1;
            }
            return nReturn;
        }

        // return CAN receive message number;
        // CAN data stored in fixed buff sg_receiveBuff[];
        public int CAN_GetReceiveNum(ref CAN_STAND_ADAPT buff)
        {
            int nSize = ProcessCANMsg(ref sg_asMsgBuffer);
            string msg = "nSize=" + nSize.ToString() + "\n";
            int j = 0;
            for (int i = 0; i < nSize; i++)
            {

                if (sg_asMsgBuffer[i].m_uDataInfo.m_sCANMsg.m_unMsgID == buff.ID)
                {
                    buff.Data = sg_asMsgBuffer[i].m_uDataInfo.m_sCANMsg.m_ucData;
                    j++;
                    string[] Rx = CANRxMsgToString(sg_asMsgBuffer[i]);
                    string s = Rx[0] + "\t" + Rx[2] + "\t" + Rx[4] + "\t" + Rx[1] + "\t" + "d" + "  " + Rx[5] + "\t" + Rx[6];
                    WriteGenLog(s, "UDS");
                }
            }
            return j;
        }
        
        /**
        * Processing of the received packets from bus
        */
        public int ProcessCANMsg(ref sTCANDATA[] sg_msg)
        {
            int nSize =500;
            nConnect(true);
            if (nReadMultiMessage(sg_msg, ref nSize) == 1)
            {
                return nSize;
                
            }
            else
            {
                nSize = 0;
            }
            return nSize;
        }
        /**
        * waitmessage with timeout
        */
        public int WaitForRxMessageWithTimeout()
        {
            int dwResult = icsneoWaitForRxMessagesWithTimeOut(m_anhObject, WAITTIME_NEOVI);
            return dwResult;
        }
        /**
        * \return Operation Result. 0 incase of no errors. Failure Error codes otherwise.
        *
        * This will close the connection with the driver. This will be
        * called before deleting HI layer. This will be called during
        * application close.
        */
        public int nDisconnectFromDriver()
        {
            int nReturn = 0;

            int nErrors = 0;
            if (m_anhObject != 0)
            {
                // First disconnect the COM
                if (icsneoClosePort(m_anhObject, ref nErrors) == 1)
                {
                    m_anhObject = 0;
                    nReturn = 1;
                }
            }
            sg_ucNoOfHardware = 0;

            return nReturn;
        }

        /// <summary>
        /// copy sTCANDATA to sTCAN_MSG
        /// </summary>
        /// <param name="sdata"></param>
        /// <param name="sMessage"></param>
        public void CopysTDataTosTMsg(sTCANDATA sdata, ref sTCAN_MSG sMessage)
        {
            
            sMessage.m_unMsgID = sdata.m_uDataInfo.m_sCANMsg.m_unMsgID;
            sMessage.m_ucDataLen = sdata.m_uDataInfo.m_sCANMsg.m_ucDataLen;
            sMessage.m_ucData = sdata.m_uDataInfo.m_sCANMsg.m_ucData;
            sMessage.m_ucChannel = sdata.m_uDataInfo.m_sCANMsg.m_ucChannel;
            sMessage.m_ucEXTENDED = sdata.m_uDataInfo.m_sCANMsg.m_ucEXTENDED;
            sMessage.m_ucRTR = sdata.m_uDataInfo.m_sCANMsg.m_ucRTR;
            sMessage.m_bCANFD = sdata.m_uDataInfo.m_sCANMsg.m_bCANFD;
        }

    }
}
