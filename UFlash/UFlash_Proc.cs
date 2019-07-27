using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace UFlash_config
{
    public partial class UFLASH_PROC
    {
        FileStream fs;
        BinaryReader bin_fs;

        delegate void ShowMsg_t(string Msg);
        ShowMsg_t ShowMsg_pf;
        RichTextBox RichTextBox_pc = null;

        HexPraser.Container_Class Container_dll = new HexPraser.Container_Class();

        public UInt32 progressbar_totalBytes
        {
            get
            {
                return 0x240000;
            }
        }
        public UInt32 progressbar_numBytesSent
        {
            get;
            set;
        }
        [DllImport("convert.dll")]
        static extern byte Convert(char[] hexfile, char[] binfile, UInt32 startAddr, UInt32 endAddr, byte padbyte);

        void RichText_ShowMsg(string msg)
        {
            RichTextBox_pc.Invoke(ShowMsg_pf, msg);
        }
        UInt32[] StartAddress = new UInt32[3] { 0x80000000, 0x80200000, 0x80240000 };
        UInt32[] EndAddress = new UInt32[3] { 0x801FFFFF, 0x8023FFFF, 0x8027FFFF };
        string[] MemoryDescription = new string[3] { "A0", "A1", "A2" };
        void DiagCom_Close()
        {
        }
        public byte UFlash_Proc(string srcHexPath, string ECUplatform, string updateType)
        {
            string binFilePath = "";
            byte temp;

            if (startAddress == 0 || endAddress == 0)
            {
                RichText_ShowMsg("不支持的平台刷新\n");
                return 0;
            }
            RichText_ShowMsg("Hex转BIN...");
            binFilePath = Path.ChangeExtension(srcHexPath, "bin");
            temp = Convert(srcHexPath.ToCharArray(), binFilePath.ToCharArray(), StartAddress[0], EndAddress[2], 0xFF);
            if (temp != 1)
            {
                RichText_ShowMsg("BIN文件转化失败\n");
                return 0;
            }
            RichText_ShowMsg("OK\n");

            RichText_ShowMsg("进入扩展会话模式...");
            if (1 != Uds_SessionControl(0x03))
            {
                RichText_ShowMsg("失败\n");
                DiagCom_Close();
            }
            RichText_ShowMsg("OK\n");
            RichText_ShowMsg("进入刷新会话模式...");
            if (1 != Uds_SessionControl(0x02))
            {
                RichText_ShowMsg("失败\n");
                DiagCom_Close();
                return 0;
            }
            RichText_ShowMsg("OK\n");

            RichText_ShowMsg("开始安全访问...");
            if (1 != Uds_SecurityAccess(0x01, 0x04, 0x04))
            {
                RichText_ShowMsg("失败\n");
                DiagCom_Close();
                return 0;
            }
            RichText_ShowMsg("OK\n");

            byte Idx = 0;
            while (Idx < StartAddress.Length)
            {
                RichText_ShowMsg("Erase " + MemoryDescription[Idx] + "...");
                if (1 != Uds_EraseFlash(0xFF00, StartAddress[Idx], EndAddress[Idx]))
                {
                    RichText_ShowMsg("失败\n");
                    DiagCom_Close();
                    return 0;
                }
                RichText_ShowMsg("OK\n");

                RichText_ShowMsg("Program " + MemoryDescription[Idx] + "...");
                if (1 != Uds_ProgramMemory(StartAddress[Idx], (UInt32)(StartAddress[Idx] - 0x80040000), (UInt32)(EndAddress[Idx] - StartAddress[Idx] + 1), binFilePath))
                {
                    RichText_ShowMsg("失败\n");
                    DiagCom_Close();
                    return 0;
                }
                RichText_ShowMsg("OK\n");

                RichText_ShowMsg("Checksum " + MemoryDescription[Idx] + "...");
                if (1 != Uds_Checksum(0xFF01, StartAddress[Idx], EndAddress[Idx], 1))
                {
                    RichText_ShowMsg("失败\n");
                    DiagCom_Close();
                    return 0;
                }
                RichText_ShowMsg("OK\n");

                Idx++;
            }

            RichText_ShowMsg("复位ECU");
            if (1 != Uds_Reset(0x01))
            {
                RichText_ShowMsg("失败\n");
                DiagCom_Close();
                return 0;
            }
            RichText_ShowMsg("OK\n");
            RichText_ShowMsg("刷新成功\n");
            DiagCom_Close();
            return 1;
        }


    }

}
