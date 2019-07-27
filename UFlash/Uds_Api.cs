using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Threading;
using System.Runtime.InteropServices;

namespace UFlash_config
{
    public partial class UFLASH_PROC
    {
        enum Diag_StatusAL_e
        {
            SBDLITE_STAL_ERR_TP = (byte)SBDLITE_StatusTP_e.SBDLITE_TP_Status_Num,
            SBDLITE_STAL_POS,
            SBDLITE_STAL_NEG,
            SBDLITE_STAL_NORESP,
            SBDLITE_STAL_NIL,
        };

        byte[] Diag_ReqBuffer = new byte[4100];
        byte[] Diag_RespBuffer = new byte[4100];


        Diag_StatusAL_e Uds_TxMsg(byte[] ALMsgBuf, UInt16 NrALMsg)
        {
            SBDLITE_StatusTP_e TpRxState_e;
            UInt16 AL_P2 = 500;

            if (SBDLITE_StatusTP_e.SBDLITE_TP_TXOK != Tp_TxMsg(Diag_ReqBuffer, NrALMsg))
            {
                WriteGenLog("SBDLITE_STAL_ERR_TP", "debug");
                return Diag_StatusAL_e.SBDLITE_STAL_ERR_TP;
            }

            do
            {
                TpRxState_e = Tp_RxMsg(Diag_RespBuffer, ref NrALMsg, AL_P2);
                if (TpRxState_e == SBDLITE_StatusTP_e.SBDLITE_TP_RXERR_P2_TIMEOUT)
                {
                    WriteGenLog("SBDLITE_STAL_NORESP", "debug");
                    return Diag_StatusAL_e.SBDLITE_STAL_NORESP;
                }
                else
                {
                    if (TpRxState_e == SBDLITE_StatusTP_e.SBDLITE_TP_RXERR_N_Cr_TIMEOUT)
                    {
                        WriteGenLog("SBDLITE_STAL_ERR_TP", "debug");
                        return Diag_StatusAL_e.SBDLITE_STAL_ERR_TP;
                    }
                }

                if (!((Diag_RespBuffer[0] == 0x7F) && (Diag_RespBuffer[2] == 0x78)))
                {
                    break;
                }
                else
                {
                    AL_P2 = 55000;
                }
            }
            while (true);

            if (Diag_RespBuffer[0] == ALMsgBuf[0] + 0x40)
            {
                return Diag_StatusAL_e.SBDLITE_STAL_POS;
            }
            else
            {
                if ((Diag_RespBuffer[0] == 0x7F) && (Diag_RespBuffer[1] == ALMsgBuf[0]))
                {
                    return Diag_StatusAL_e.SBDLITE_STAL_NEG;
                }
                else
                {
                    return Diag_StatusAL_e.SBDLITE_STAL_NIL;
                }
            }
        }




        byte Uds_SessionControl(byte SessionId)
        {
            Diag_ReqBuffer[0] = 0x10;
            Diag_ReqBuffer[1] = SessionId;

            if (Uds_TxMsg(Diag_ReqBuffer, 2) == Diag_StatusAL_e.SBDLITE_STAL_POS)
            {
                return 1;
            }
            else
            {
                return 0;
            }

        }
        byte Uds_SecurityAccess(byte secLevel, byte seedLength, byte keyLength)
        {
            UInt32 seed;
            UInt32 key;

            Diag_ReqBuffer[0] = 0x27;
            Diag_ReqBuffer[1] = secLevel;

            if (Uds_TxMsg(Diag_ReqBuffer, 2) == Diag_StatusAL_e.SBDLITE_STAL_POS)
            {
                seed = (UInt32)((UInt32)(Diag_RespBuffer[2] << 24)
                    | (UInt32)(Diag_RespBuffer[3] << 16)
                    | (UInt32)(Diag_RespBuffer[4] << 8)
                    | (UInt32)(Diag_RespBuffer[5]));
                key = Calc_Key(seed);
                Diag_ReqBuffer[1] = (byte)(secLevel + 1);
                Diag_ReqBuffer[2] = (byte)(key >> 24);
                Diag_ReqBuffer[3] = (byte)(key >> 16);
                Diag_ReqBuffer[4] = (byte)(key >> 8);
                Diag_ReqBuffer[5] = (byte)(key >> 0);

                if (Uds_TxMsg(Diag_ReqBuffer, 6) == Diag_StatusAL_e.SBDLITE_STAL_POS)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }

        }


        byte Uds_ReadDataById(UInt16 RecordID)
        {
            Diag_ReqBuffer[0] = 0x22;
            Diag_ReqBuffer[1] = (byte)(RecordID >> 8);
            Diag_ReqBuffer[2] = (byte)RecordID;

            if (Uds_TxMsg(Diag_ReqBuffer, 3) == Diag_StatusAL_e.SBDLITE_STAL_POS)
            {
                return 1;
            }
            else
            {
                return 0;
            }

        }

        byte Uds_WriteDataByIdFromFile(UInt16 RecordId, UInt16 RecordLength, string binfilepath, UInt32 offset)
        {
            FileStream fs_logid;
            BinaryReader br_logid;

            Diag_ReqBuffer[0] = 0x2E;
            Diag_ReqBuffer[1] = (byte)(RecordId >> 8);
            Diag_ReqBuffer[2] = (byte)RecordId;
            Diag_ReqBuffer[x] = (byte)x
           
            if (Uds_TxMsg(Diag_ReqBuffer, x+1) == Diag_StatusAL_e.SBDLITE_STAL_POS)
            {
                return 1;
            }
            else
            {
                return 0;
            }

        }

        byte Uds_EraseFlash(UInt16 Rid, UInt32 StartAdr, UInt32 EndAdr)
        {
            Diag_ReqBuffer[0] = 0x31;
            Diag_ReqBuffer[1] = 0x01;
            Diag_ReqBuffer[2] = (byte)(Rid >> 8);
            Diag_ReqBuffer[3] = (byte)Rid;
            Diag_ReqBuffer[4] = (byte)(StartAdr >> 24);
            Diag_ReqBuffer[5] = (byte)(StartAdr >> 16);
            Diag_ReqBuffer[6] = (byte)(StartAdr >> 8);
            Diag_ReqBuffer[7] = (byte)StartAdr;
            Diag_ReqBuffer[8] = (byte)(EndAdr >> 24);
            Diag_ReqBuffer[9] = (byte)(EndAdr >> 16);
            Diag_ReqBuffer[10] = (byte)(EndAdr >> 8);
            Diag_ReqBuffer[11] = (byte)EndAdr;

            if (Uds_TxMsg(Diag_ReqBuffer, 12) == Diag_StatusAL_e.SBDLITE_STAL_POS)
            {
                return 1;
            }
            else
            {
                return 0;
            }

        }
        UInt32 Checksum;
        byte Uds_ProgramMemory(UInt32 DestStart, UInt32 SrcStart, UInt32 memSize, string sourcefile)
        {
            UInt32 numBytesLeft_u32;
            UInt32 numBytesSent = 0;
            int numBytesRead = 0;
            byte TxSeqCtr;

            numBytesLeft_u32 = memSize;
            Diag_ReqBuffer[0] = 0x34;
            Diag_ReqBuffer[1] = 0;
            Diag_ReqBuffer[2] = 0x34;
            Diag_ReqBuffer[3] = (byte)(DestStart >> 24);
            Diag_ReqBuffer[4] = (byte)(DestStart >> 16);
            Diag_ReqBuffer[5] = (byte)(DestStart >> 8);
            Diag_ReqBuffer[6] = (byte)(DestStart >> 0);
            Diag_ReqBuffer[7] = (byte)(numBytesLeft_u32 >> 16);
            Diag_ReqBuffer[8] = (byte)(numBytesLeft_u32 >> 8);
            Diag_ReqBuffer[9] = (byte)(numBytesLeft_u32 >> 0);

            if (Uds_TxMsg(Diag_ReqBuffer, 10) != Diag_StatusAL_e.SBDLITE_STAL_POS)
            {
                return 0;
            }

            fs = new FileStream(sourcefile, FileMode.Open);
            fs.Position = SrcStart;
            bin_fs = new BinaryReader(fs);
            TxSeqCtr = 1;

            while (numBytesLeft_u32 > 0)
            {
                Diag_ReqBuffer[0] = 0x36;
                Diag_ReqBuffer[1] = TxSeqCtr;
                TxSeqCtr++;
                if (numBytesLeft_u32 > 4093)
                {
                    numBytesRead = bin_fs.Read(Diag_ReqBuffer, 2, 4093);
                    if (numBytesRead == 4093)
                    {
                        numBytesLeft_u32 -= 4093;
                        numBytesSent = 4093;
                    }
                    else
                    {
                        fs.Close();
                        bin_fs.Close();
                        return 0;
                    }
                }
                else
                {
                    bin_fs.Read(Diag_ReqBuffer, 2, (int)numBytesLeft_u32);
                    numBytesSent = numBytesLeft_u32;
                    numBytesLeft_u32 = 0;
                }

                if (Uds_TxMsg(Diag_ReqBuffer, (UInt16)(numBytesSent + 2)) != Diag_StatusAL_e.SBDLITE_STAL_POS)
                {
                    fs.Close();
                    bin_fs.Close();
                    return 0;
                }
                progressbar_numBytesSent += numBytesSent;
            }

            Checksum = SBCCFL_Do8s32r(SrcStart, memSize, 1);
            fs.Close();
            bin_fs.Close();

            Diag_ReqBuffer[0] = 0x37;

            if (Uds_TxMsg(Diag_ReqBuffer, 1) != Diag_StatusAL_e.SBDLITE_STAL_POS)
            {
                return 0;
            }

            return 1;
        }

        byte Uds_Checksum(UInt16 Rid, UInt32 StartAdr, UInt32 EndAdr, byte CrcType)
        {
            Diag_ReqBuffer[0] = 0x31;
            Diag_ReqBuffer[1] = 0x01;
            Diag_ReqBuffer[2] = (byte)(Rid >> 8);
            Diag_ReqBuffer[3] = (byte)Rid;
            Diag_ReqBuffer[4] = (byte)(StartAdr >> 24);
            Diag_ReqBuffer[5] = (byte)(StartAdr >> 16);
            Diag_ReqBuffer[6] = (byte)(StartAdr >> 8);
            Diag_ReqBuffer[7] = (byte)StartAdr;
            Diag_ReqBuffer[8] = (byte)(EndAdr >> 24);
            Diag_ReqBuffer[9] = (byte)(EndAdr >> 16);
            Diag_ReqBuffer[10] = (byte)(EndAdr >> 8);
            Diag_ReqBuffer[11] = (byte)EndAdr;
            Diag_ReqBuffer[12] = (byte)(Checksum >> 24);
            Diag_ReqBuffer[13] = (byte)(Checksum >> 16);
            Diag_ReqBuffer[14] = (byte)(Checksum >> 8);
            Diag_ReqBuffer[15] = (byte)Checksum;


            if (Uds_TxMsg(Diag_ReqBuffer, 16) != Diag_StatusAL_e.SBDLITE_STAL_POS)
            {
                return 0;
            }
            else
            {
                return 1;
            }

        }
        byte Uds_Reset(byte RstType)
        {
            Diag_ReqBuffer[0] = 0x11;
            Diag_ReqBuffer[1] = RstType;

            if (Uds_TxMsg(Diag_ReqBuffer, 2) != Diag_StatusAL_e.SBDLITE_STAL_POS)
            {
                return 0;
            }
            else
            {
                return 1;
            }

        }

        unsafe UInt32 Calc_Key(UInt32 seed)
        {
            return seed;
        }


        UInt32 SBCCFL_CS_BLOCK_SIZE = 256;
        UInt32 SBCCFL_BytesLeft_u32;
        UInt32 oldcrc;
        byte[] CRCBuffer = new byte[300];
        unsafe UInt32 SBCCFL_Do8s32r(UInt32 StartAdr, UInt32 CRCLength, byte CrcType)
        {
            UInt32 SBCCFL_dChecksum_u32;

            SBCCFL_dChecksum_u32 = 0;
            oldcrc = 0xFFFFFFFF;

            fs.Position = StartAdr;
            SBCCFL_BytesLeft_u32 = CRCLength;
            while (SBCCFL_BytesLeft_u32 > 0)
            {
                fixed (byte* data_pu8 = CRCBuffer)
                {
                    if (SBCCFL_BytesLeft_u32 > SBCCFL_CS_BLOCK_SIZE)
                    {
                        bin_fs.Read(CRCBuffer, 0, (int)SBCCFL_CS_BLOCK_SIZE);
                        SBCCFL_dChecksum_u32 = SBCCFL_CaculateCRC(data_pu8, SBCCFL_CS_BLOCK_SIZE);
                        SBCCFL_BytesLeft_u32 -= SBCCFL_CS_BLOCK_SIZE;
                    }
                    else
                    {
                        bin_fs.Read(CRCBuffer, 0, (int)SBCCFL_BytesLeft_u32);
                        SBCCFL_dChecksum_u32 = SBCCFL_CaculateCRC(data_pu8, SBCCFL_BytesLeft_u32);
                        SBCCFL_BytesLeft_u32 = 0;
                    }
                }
            }

            return SBCCFL_dChecksum_u32;
        }
        unsafe UInt32 SBCCFL_CaculateCRC(byte* adStart_u32, UInt32 dSize_u32)
        {

            UInt32 temp;

            while ((dSize_u32--) != 0)
            {
                temp = (oldcrc ^ (*adStart_u32++)) & 0x000000FF;
                oldcrc = ((oldcrc >> 8) & 0x00FFFFFF) ^ crc32_table[temp];
            }
            if (SBCCFL_BytesLeft_u32 > SBCCFL_CS_BLOCK_SIZE)
            {
                return oldcrc;
            }
            else
            {
                temp = oldcrc;
                oldcrc = 0xFFFFFFFF;
                return ~temp;
            }
        }

        UInt32[] crc32_table = new UInt32[256] {
            0x00000000, 0x77073096, 0xee0e612c, 0x990951ba, 0x076dc419,
            0x706af48f, 0xe963a535, 0x9e6495a3, 0x0edb8832, 0x79dcb8a4,
            0xe0d5e91e, 0x97d2d988, 0x09b64c2b, 0x7eb17cbd, 0xe7b82d07,
            0x90bf1d91, 0x1db71064, 0x6ab020f2, 0xf3b97148, 0x84be41de,
            0x1adad47d, 0x6ddde4eb, 0xf4d4b551, 0x83d385c7, 0x136c9856,
            0x646ba8c0, 0xfd62f97a, 0x8a65c9ec, 0x14015c4f, 0x63066cd9,
            0xfa0f3d63, 0x8d080df5, 0x3b6e20c8, 0x4c69105e, 0xd56041e4,
            0xa2677172, 0x3c03e4d1, 0x4b04d447, 0xd20d85fd, 0xa50ab56b,
            0x35b5a8fa, 0x42b2986c, 0xdbbbc9d6, 0xacbcf940, 0x32d86ce3,
            0x45df5c75, 0xdcd60dcf, 0xabd13d59, 0x26d930ac, 0x51de003a,
            0xc8d75180, 0xbfd06116, 0x21b4f4b5, 0x56b3c423, 0xcfba9599,
            0xb8bda50f, 0x2802b89e, 0x5f058808, 0xc60cd9b2, 0xb10be924,
            0x2f6f7c87, 0x58684c11, 0xc1611dab, 0xb6662d3d, 0x76dc4190,
            0x01db7106, 0x98d220bc, 0xefd5102a, 0x71b18589, 0x06b6b51f,
            0x9fbfe4a5, 0xe8b8d433, 0x7807c9a2, 0x0f00f934, 0x9609a88e,
            0xe10e9818, 0x7f6a0dbb, 0x086d3d2d, 0x91646c97, 0xe6635c01,
            0x6b6b51f4, 0x1c6c6162, 0x856530d8, 0xf262004e, 0x6c0695ed,
            0x1b01a57b, 0x8208f4c1, 0xf50fc457, 0x65b0d9c6, 0x12b7e950,
            0x8bbeb8ea, 0xfcb9887c, 0x62dd1ddf, 0x15da2d49, 0x8cd37cf3,
            0xfbd44c65, 0x4db26158, 0x3ab551ce, 0xa3bc0074, 0xd4bb30e2,
            0x4adfa541, 0x3dd895d7, 0xa4d1c46d, 0xd3d6f4fb, 0x4369e96a,
            0x346ed9fc, 0xad678846, 0xda60b8d0, 0x44042d73, 0x33031de5,
            0xaa0a4c5f, 0xdd0d7cc9, 0x5005713c, 0x270241aa, 0xbe0b1010,
            0xc90c2086, 0x5768b525, 0x206f85b3, 0xb966d409, 0xce61e49f,
            0x5edef90e, 0x29d9c998, 0xb0d09822, 0xc7d7a8b4, 0x59b33d17,
            0x2eb40d81, 0xb7bd5c3b, 0xc0ba6cad, 0xedb88320, 0x9abfb3b6,
            0x03b6e20c, 0x74b1d29a, 0xead54739, 0x9dd277af, 0x04db2615,
            0x73dc1683, 0xe3630b12, 0x94643b84, 0x0d6d6a3e, 0x7a6a5aa8,
            0xe40ecf0b, 0x9309ff9d, 0x0a00ae27, 0x7d079eb1, 0xf00f9344,
            0x8708a3d2, 0x1e01f268, 0x6906c2fe, 0xf762575d, 0x806567cb,
            0x196c3671, 0x6e6b06e7, 0xfed41b76, 0x89d32be0, 0x10da7a5a,
            0x67dd4acc, 0xf9b9df6f, 0x8ebeeff9, 0x17b7be43, 0x60b08ed5,
            0xd6d6a3e8, 0xa1d1937e, 0x38d8c2c4, 0x4fdff252, 0xd1bb67f1,
            0xa6bc5767, 0x3fb506dd, 0x48b2364b, 0xd80d2bda, 0xaf0a1b4c,
            0x36034af6, 0x41047a60, 0xdf60efc3, 0xa867df55, 0x316e8eef,
            0x4669be79, 0xcb61b38c, 0xbc66831a, 0x256fd2a0, 0x5268e236,
            0xcc0c7795, 0xbb0b4703, 0x220216b9, 0x5505262f, 0xc5ba3bbe,
            0xb2bd0b28, 0x2bb45a92, 0x5cb36a04, 0xc2d7ffa7, 0xb5d0cf31,
            0x2cd99e8b, 0x5bdeae1d, 0x9b64c2b0, 0xec63f226, 0x756aa39c,
            0x026d930a, 0x9c0906a9, 0xeb0e363f, 0x72076785, 0x05005713,
            0x95bf4a82, 0xe2b87a14, 0x7bb12bae, 0x0cb61b38, 0x92d28e9b,
            0xe5d5be0d, 0x7cdcefb7, 0x0bdbdf21, 0x86d3d2d4, 0xf1d4e242,
            0x68ddb3f8, 0x1fda836e, 0x81be16cd, 0xf6b9265b, 0x6fb077e1,
            0x18b74777, 0x88085ae6, 0xff0f6a70, 0x66063bca, 0x11010b5c,
            0x8f659eff, 0xf862ae69, 0x616bffd3, 0x166ccf45, 0xa00ae278,
            0xd70dd2ee, 0x4e048354, 0x3903b3c2, 0xa7672661, 0xd06016f7,
            0x4969474d, 0x3e6e77db, 0xaed16a4a, 0xd9d65adc, 0x40df0b66,
            0x37d83bf0, 0xa9bcae53, 0xdebb9ec5, 0x47b2cf7f, 0x30b5ffe9,
            0xbdbdf21c, 0xcabac28a, 0x53b39330, 0x24b4a3a6, 0xbad03605,
            0xcdd70693, 0x54de5729, 0x23d967bf, 0xb3667a2e, 0xc4614ab8,
            0x5d681b02, 0x2a6f2b94, 0xb40bbe37, 0xc30c8ea1, 0x5a05df1b,
            0x2d02ef8d
            };

    }
}
