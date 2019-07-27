using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;
using System.Runtime.InteropServices;
using System.Windows.Forms;

public struct CAN_STAND_ADAPT  //使用不安全代码
{
    public uint ID;
    public uint TimeStamp;        //时间标识
    public byte TimeFlag;         //是否使用时间标识
    public byte SendType;         //发送标志。保留，未用
    public byte RemoteFlag;       //是否是远程帧
    public byte ExternFlag;       //是否是扩展帧
    public byte DataLen;

    public byte[] Data;
}

namespace UFlash_config
{

    public partial class UFLASH_PROC
    {
        const float ISOTP_Nr = 100;
        const float ISOTP_Cr = 100; /* Time until reception of the next Consecutive Frame N_PDU (Timeout Value) */

        /* Parameter for Transmission Part */
        const float ISOTP_Bs = 100;  //Time until reception of the next flowControl N_PDU (Timeout Value)
        const float ISOTP_Cs = 75; // Time until transmission of the next ConsecutiveFrame N_PDU( Performance Requirement)

        const byte ISOTP_FC_CTS = 0x00;
        const byte ISOTP_FC_WTS = 0x01;      /* Wait  To Send                     */
        const byte ISOTP_FC_OVR = 0x02;      /* Overflow Flow Control             */

        const byte ISOTP_PCI_SF = 0x00;      /* PCI byte of Single Frame          */
        const byte ISOTP_PCI_FF = 0x10;      /* PCI byte of First  Frame          */
        const byte ISOTP_PCI_CF = 0x20;      /* PCI byte of Consecutive Frame     */
        const byte ISOTP_PCI_FC = 0x30;      /* PCI byte of Flow Control          */

        const byte NL_OK = 0x00;
        const byte NL_TIMEOUT_A = 0x01;
        const byte NL_TIMEOUT_Bs = 0x02;
        const byte NL_TIMEOUT_Cr = 0x03;
        const byte NL_WRONG_SN = 0x04;
        const byte NL_INVALID_FS = 0x05;
        const byte NL_UNEXP_PDU = 0x06;
        const byte NL_WFT_OVRN = 0x07;
        const byte NL_BUFFER_OVFLW = 0x08;
        const byte NL_WRONG_DLC = 0x09;
        const byte NL_ERROR = 0x0A;

        byte PadValue = 0xFF;

        CAN_STAND_ADAPT[] recvbuf = new CAN_STAND_ADAPT[10];
        CAN_STAND_ADAPT[] sendbuf = new CAN_STAND_ADAPT[600];
        
        public float tickToMs(long endTime, long beginTime)
        {
            long freq = 0;
            QueryPerformanceFrequency(ref freq);  //获取CPU频率
            return (((float)(endTime - beginTime) / freq) * 1000);
        }

        public void DelayMilSec(long tem_MilSec)
        {
            long stop_Value = 0;
            long start_Value = 0;
            long freq = 0;
            long n = 0;

            QueryPerformanceFrequency(ref freq);  //获取CPU频率
            long count = tem_MilSec * freq / 1000;
            QueryPerformanceCounter(ref start_Value); //获取初始前值

            while (n < count)
            {
                QueryPerformanceCounter(ref stop_Value);//获取终止变量值
                n = stop_Value - start_Value;
            }
        }

        public byte UFlash_InitDevice(UInt32 UdsReqId, UInt32 UdsRespId, UInt32 Baudrate, RichTextBox ShowLog)
        {
            RichTextBox_pc = ShowLog;
            ShowMsg_pf = RichTextBox_pc.AppendText;
            
           if(nConnect(true) != 1)
           {
                return 0;
           }
            int idx = 0;
            while (idx < recvbuf.Length)
            {
                recvbuf[idx].ID = UdsRespId;
                recvbuf[idx].RemoteFlag = 0;
                recvbuf[idx].DataLen = 8;
                idx++;
            }
            idx = 0;
            while (idx < sendbuf.Length)
            {
                sendbuf[idx].ID = UdsReqId;
                sendbuf[idx].RemoteFlag = 0;
                sendbuf[idx].ExternFlag = 0;
                sendbuf[idx].DataLen = 8;
                sendbuf[idx].Data = new byte[8];
                idx++;
            }


            return 1;
        }
        
        public byte UFlash_CloseDevice()
        {
            if (nDisconnectFromDriver() != 1)
            {
                return 0;
            }
            return 1;
        }

        enum SBDLITE_StatusTP_e
        {
            SBDLITE_TP_IDLE = 0,
            SBDLITE_TP_TXOK,
            SBDLITE_TP_RXOK,
            SBDLITE_TP_TXERR,
            SBDLITE_TP_TXERR_DRIVER_NOK,
            SBDLITE_TP_TXERR_MSG_TOOLONG,
            SBDLITE_TP_TXERR_N_BS_TIMEOUT,

            SBDLITE_TP_RXERR,
            SBDLITE_TP_RXERR_WRONGSEQ,
            SBDLITE_TP_RXERR_P2_TIMEOUT,
            SBDLITE_TP_RXERR_N_Cr_TIMEOUT,


            SBDLITE_TP_Status_Num,
        };

        UInt16 TpTx_BytesCtr_u16 = 0;
        UInt16 TpTx_LeftNrBytes = 0;
        UInt16 TpTx_TxMo_Handle = 0;
        int TpTx_RetValAPI = 0;
        byte TpTx_seqNo_u8;
        long TpTx_IsoTp_StartTime = 0;
        long TpTx_IsoTp_CurrentTime = 0;
        float TpTx_dTime = 0;
        UInt16 TpTx_NrCanMo_Sent_u16 = 0;
        UInt16 TpTx_NrCanMo_ToTx_u16 = 1;
        UInt16 TpTx_NrBS_Sent_u16 = 0;


        long TpRx_StartTime = 0;
        long TpRx_CurrentTime = 0;
        int TpRx_RetValAPI;
        float TpRx_dTime = 0;

        SBDLITE_StatusTP_e Tp_RxMsg(byte[] ALMsgBuf, ref UInt16 NrALMsg, UInt16 AL_P2)
        {
            byte tpciHighNibble_u8;
            byte tpciLowNibble_u8;
            UInt16 ExpectedLength = 0;
            UInt16 ReceivedLength = 0;

            QueryPerformanceCounter(ref TpRx_StartTime);
            TpRx_RetValAPI = 0;
            do
            {
                TpRx_RetValAPI = CAN_GetReceiveNum(ref recvbuf[0]);
                QueryPerformanceCounter(ref TpRx_CurrentTime);
                TpRx_dTime = tickToMs(TpRx_CurrentTime, TpRx_StartTime);
            } while ((TpRx_RetValAPI == 0) && (TpRx_dTime < AL_P2));

            if (TpRx_RetValAPI == 0)
            {
                return SBDLITE_StatusTP_e.SBDLITE_TP_RXERR_P2_TIMEOUT;
            }

            byte[] frame = recvbuf[0].Data;
            tpciHighNibble_u8 = (byte)(frame[0] & 0xF0);

            switch (tpciHighNibble_u8)
            {
                case ISOTP_PCI_SF:
                    tpciLowNibble_u8 = (byte)(frame[0] & 0x0F);
                    if (tpciLowNibble_u8 < 8)
                    {
                        for (byte idx = 0; idx < tpciLowNibble_u8; idx++)
                        {
                            ALMsgBuf[idx] = frame[idx + 1];
                        }
                        NrALMsg = tpciLowNibble_u8;
                        return SBDLITE_StatusTP_e.SBDLITE_TP_RXOK;
                    }
                    break;
                case ISOTP_PCI_FF:
                    ExpectedLength = (UInt16)(((frame[0] & 0x0F) << 8) | (frame[1]));
                    for (byte idx = 0; idx < 6; idx++)
                    {
                        ALMsgBuf[idx] = frame[idx + 1];
                    }
                    ReceivedLength = 6;

                    sendbuf[0].Data[0] = 0x30;
                    sendbuf[0].Data[1] = 0x00;
                    sendbuf[0].Data[2] = 0x00;
                    sendbuf[0].Data[3] = 0x00;
                    sendbuf[0].Data[4] = 0x00;
                    sendbuf[0].Data[5] = 0x00;
                    sendbuf[0].Data[6] = 0x00;
                    sendbuf[0].Data[7] = 0x00;
                    TpTx_RetValAPI = CAN_Transmit(ref sendbuf,0,1);
                    if (TpTx_RetValAPI != 1)
                    {
                        return SBDLITE_StatusTP_e.SBDLITE_TP_TXERR_DRIVER_NOK;
                    }
                    break;
                default:
                    return SBDLITE_StatusTP_e.SBDLITE_TP_RXERR_WRONGSEQ;
            }

            QueryPerformanceCounter(ref TpRx_StartTime);
            do
            {
                TpRx_RetValAPI = CAN_GetReceiveNum(ref recvbuf[0]);

                if (TpRx_RetValAPI > 0)
                {
                    frame = recvbuf[0].Data;
                    if ((ReceivedLength + 7) >= ExpectedLength)
                    {
                        for (byte idx = 0; idx < (ExpectedLength - ReceivedLength); idx++)
                        {
                            ALMsgBuf[ReceivedLength + idx] = frame[idx + 1];
                        }
                        ReceivedLength = ExpectedLength;
                        NrALMsg = ReceivedLength;
                    }
                    else
                    {
                        for (byte idx = 0; idx < 7; idx++)
                        {
                            ALMsgBuf[ReceivedLength + idx] = frame[idx + 1];
                        }
                        ReceivedLength += 7;
                    }
                    QueryPerformanceCounter(ref TpRx_StartTime);
                }
                QueryPerformanceCounter(ref TpRx_CurrentTime);
                TpRx_dTime = tickToMs(TpRx_CurrentTime, TpRx_StartTime);
            }
            while ((TpRx_dTime < ISOTP_Cr) && (ReceivedLength < ExpectedLength));

            if (ReceivedLength < ExpectedLength)
            {
                return SBDLITE_StatusTP_e.SBDLITE_TP_RXERR_N_Cr_TIMEOUT;
            }

            return SBDLITE_StatusTP_e.SBDLITE_TP_RXOK;
        }

        SBDLITE_StatusTP_e Tp_TxMsg(byte[] ALMsgBuf, UInt16 NrALMsg)
        {
            byte BSmax = 0;
            byte STmin = 0;
            if (NrALMsg > 4095)
            {
                WriteGenLog("SBDLITE_TP_TXERR_MSG_TOOLONG\n", "debug");
                return (SBDLITE_StatusTP_e.SBDLITE_TP_TXERR_MSG_TOOLONG);
            }

            if (NrALMsg < 8)
            {
                sendbuf[0].Data[0] = (byte)NrALMsg;
                for (TpTx_BytesCtr_u16 = 0; TpTx_BytesCtr_u16 < 7; TpTx_BytesCtr_u16++)
                 {
                    sendbuf[0].Data[TpTx_BytesCtr_u16 + 1] = PadValue;
                    sendbuf[0].Data[TpTx_BytesCtr_u16 + 1] = ALMsgBuf[TpTx_BytesCtr_u16]; 
                }
                TpTx_RetValAPI = CAN_Transmit(ref sendbuf,0,1);
                if (TpTx_RetValAPI != 1)
                {
                    WriteGenLog("SBDLITE_TP_TXERR_DRIVER_NOK\n", "debug");
                    return (SBDLITE_StatusTP_e.SBDLITE_TP_TXERR_DRIVER_NOK);
                }
                else
                {
                    return (SBDLITE_StatusTP_e.SBDLITE_TP_TXOK);
                }
            }
            else
            {
                sendbuf[0].Data[0] = (byte)(0x10 | (byte)(NrALMsg >> 8));
                sendbuf[0].Data[1] = (byte)NrALMsg;
                for (TpTx_BytesCtr_u16 = 0; TpTx_BytesCtr_u16 < 6; TpTx_BytesCtr_u16++)
                {
                    sendbuf[0].Data[TpTx_BytesCtr_u16+2] = ALMsgBuf[TpTx_BytesCtr_u16];
                }
                TpTx_LeftNrBytes = (UInt16)(NrALMsg - 6);
                TpTx_seqNo_u8 = 1;
                TpTx_TxMo_Handle = 1;
                TpTx_BytesCtr_u16 = 6;

                while (TpTx_LeftNrBytes > 0)
                {
                    if (TpTx_LeftNrBytes > 7)
                    {
                        sendbuf[TpTx_TxMo_Handle].Data[0] = (byte)(0x20 | TpTx_seqNo_u8);
                        for (UInt16 temp = 1; temp < 8; temp++, TpTx_BytesCtr_u16++)
                        {
                            sendbuf[TpTx_TxMo_Handle].Data[temp] = ALMsgBuf[TpTx_BytesCtr_u16];
                        }
                        
                        TpTx_LeftNrBytes -= 7;
                        TpTx_TxMo_Handle++;
                        TpTx_seqNo_u8++;
                        TpTx_seqNo_u8 &= 0x0F;
                    }
                    else
                    {

                        sendbuf[TpTx_TxMo_Handle].Data[0] = (byte)(0x20 | TpTx_seqNo_u8); 

                        for (UInt16 temp = 1; TpTx_BytesCtr_u16 < NrALMsg; temp++, TpTx_BytesCtr_u16++)
                        {
                            sendbuf[TpTx_TxMo_Handle].Data[temp] = ALMsgBuf[TpTx_BytesCtr_u16];
                        }
                       
                        TpTx_LeftNrBytes = 0;
                        TpTx_TxMo_Handle++;
                        TpTx_seqNo_u8++;
                        TpTx_seqNo_u8 &= 0x0F;
                    }
                }
                BSmax = 1;
                TpTx_NrBS_Sent_u16 = 0;
                TpTx_NrCanMo_Sent_u16 = 0;
                TpTx_NrCanMo_ToTx_u16 = 1;
                
                while (TpTx_NrCanMo_Sent_u16 < TpTx_TxMo_Handle)
                {
                    TpTx_RetValAPI = 0;

                    if (TpTx_NrCanMo_ToTx_u16 > 0)
                    {

                        TpTx_RetValAPI = CAN_Transmit(ref sendbuf, TpTx_NrCanMo_Sent_u16, TpTx_NrCanMo_ToTx_u16);

                        if (TpTx_RetValAPI != TpTx_NrCanMo_ToTx_u16)
                        {
                            WriteGenLog("SBDLITE_TP_TXERR_DRIVER_NOK2\n", "debug");
                            return (SBDLITE_StatusTP_e.SBDLITE_TP_TXERR_DRIVER_NOK);
                        }
                        TpTx_NrCanMo_Sent_u16 += TpTx_NrCanMo_ToTx_u16;
                        TpTx_NrBS_Sent_u16 += TpTx_NrCanMo_ToTx_u16;
                    }

                    if (TpTx_NrCanMo_Sent_u16 < TpTx_TxMo_Handle)
                    {
                        if (BSmax == 0)
                        {
                            DelayMilSec(STmin);
                            TpTx_NrCanMo_ToTx_u16 = 1;
                        }
                        else
                        {
                            if (TpTx_NrBS_Sent_u16 >= BSmax)
                            {
                                TpTx_RetValAPI = 0;
                                QueryPerformanceCounter(ref TpTx_IsoTp_StartTime);
                                do
                                {
                                    QueryPerformanceCounter(ref TpTx_IsoTp_CurrentTime);
                                    TpTx_dTime = tickToMs(TpTx_IsoTp_CurrentTime, TpTx_IsoTp_StartTime);
                                    TpTx_RetValAPI = CAN_GetReceiveNum(ref recvbuf[0]);
                                }
                                while ((TpTx_dTime < ISOTP_Bs) && (TpTx_RetValAPI == 0));

                                if (TpTx_RetValAPI > 0)
                                {
                                    byte tpciHighNibble_u8;
                                    byte tpciLowNibble_u8;

                                    byte[] frame = recvbuf[0].Data;
                                    tpciHighNibble_u8 = (byte)(frame[0] & 0xF0);
                                    if (tpciHighNibble_u8 == 0x30)
                                    {
                                        tpciLowNibble_u8 = (byte)(frame[0] & 0x0F);
                                        switch (tpciLowNibble_u8)
                                        {
                                            case ISOTP_FC_CTS:
                                                BSmax = frame[1];
                                                STmin = frame[2];
                                                if (BSmax == 0)
                                                {
                                                    if (STmin == 0)
                                                    {
                                                        TpTx_NrCanMo_ToTx_u16 = (UInt16)(TpTx_TxMo_Handle - TpTx_NrCanMo_Sent_u16);
                                                    }
                                                    else
                                                    {
                                                        TpTx_NrCanMo_ToTx_u16 = 1;
                                                    }

                                                }
                                                else
                                                {
                                                    TpTx_NrBS_Sent_u16 = 0;
                                                    if (STmin == 0)
                                                    {
                                                        if ((TpTx_NrCanMo_Sent_u16 + BSmax) <= TpTx_TxMo_Handle)
                                                        {
                                                            TpTx_NrCanMo_ToTx_u16 = (UInt16)(TpTx_TxMo_Handle - TpTx_NrCanMo_Sent_u16);
                                                        }
                                                        else
                                                        {
                                                            TpTx_NrCanMo_ToTx_u16 = BSmax;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        TpTx_NrCanMo_ToTx_u16 = 1;
                                                    }
                                                }
                                                break;
                                            case ISOTP_FC_WTS:
                                                TpTx_NrCanMo_ToTx_u16 = 0;
                                                break;
                                            case ISOTP_FC_OVR:
                                                //NLData_Indication(NL_BUFFER_OVFLW);
                                                break;
                                            default:
                                                //NLData_Indication(NL_INVALID_FS);
                                                break;
                                        }
                                    }
                                }
                                else
                                {
                                    WriteGenLog("SBDLITE_TP_TXERR_N_BS_TIMEOUT", "debug");
                                    return (SBDLITE_StatusTP_e.SBDLITE_TP_TXERR_N_BS_TIMEOUT);
                                }
                            }
                            else
                            {
                                DelayMilSec(STmin);
                                TpTx_NrCanMo_ToTx_u16 = 1;
                            }
                        }
                    }
                }
                return (SBDLITE_StatusTP_e.SBDLITE_TP_TXOK);
            }
        }
    }
}
