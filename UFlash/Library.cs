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
        /// <summary>
        /// write log if error exist
        /// </summary>
        public void WriteGenLog(string msg,string type)
        {
            string filepath = "";
            filepath = AppDomain.CurrentDomain.BaseDirectory + "debug";
            if (!Directory.Exists(filepath))
            {
                Directory.CreateDirectory(filepath);
            }
            filepath = AppDomain.CurrentDomain.BaseDirectory + "debug\\" + type + ".log";

            using (StreamWriter sw = File.AppendText(filepath))
            {
                if(type == "UDS")
                {
                    sw.WriteLine(msg + "\n");
                }
                else
                {
                    sw.WriteLine("---   " + msg + "\n");
                }
                sw.Flush();
                sw.Close();
                sw.Dispose();
            }
        }

        /// <summary>
        /// Array transfer to Request string Message in 16 form
        /// </summary>
        public string ArrayToString(byte[] data)
        {
            string s = " ";
            if (data != null)
            {
                foreach (byte element in data)
                {
                    s = s + element.ToString("X2") + " ";
                }
            }

            return s;
        }

        /// <summary>
        /// This will receive can message and trans into datagridview type array.
        /// </summary>
        public string[] CANTxMsgToString(sTCAN_MSG sMessage)
        {
            string type = "Tx";
            byte[] tmpCAN = new byte[sMessage.m_ucDataLen];
            for (int i = 0; i < sMessage.m_ucDataLen; i++)
            {
                tmpCAN[i] = sMessage.m_ucData[i];
            }
            string CANData = ArrayToString(tmpCAN);
            
            string[] values = {
                    DateTime.Now.ToString("HH:mm:ss.ffff"),
                    type,
                    sMessage.m_ucChannel.ToString(),
                    GetFrameType(sMessage.m_ucData[0]),
                    sMessage.m_unMsgID.ToString("X2"),
                    sMessage.m_ucDataLen.ToString(),
                    CANData
                };
            return values;
        }

        /// <summary>
        /// This will receive can message and trans into datagridview type array.
        /// </summary>
        public string[] CANRxMsgToString(sTCANDATA sg_asMsgBuffer)
        {
            string TypeofControl = "";
            string type = "NULL";
            switch (sg_asMsgBuffer.m_ucDataType)
            {
                case 0x01:
                    type = "Tx";
                    break;
                case 0x02:
                    type = "Rx";
                    break;
                case 0x04:
                    type = "Error";
                    break;
                case 0x08:
                    type = "INTR_FLAG";
                    break;
                default:
                    break;
            }
            byte[] tmpCAN = new byte[sg_asMsgBuffer.m_uDataInfo.m_sCANMsg.m_ucDataLen];
            for (int i = 0; i < sg_asMsgBuffer.m_uDataInfo.m_sCANMsg.m_ucDataLen; i++)
            {
                tmpCAN[i] = sg_asMsgBuffer.m_uDataInfo.m_sCANMsg.m_ucData[i];
            }
            string CANData = ArrayToString(tmpCAN);
            GetFrameType(sg_asMsgBuffer.m_uDataInfo.m_sCANMsg.m_ucData[0]);
            string ID = sg_asMsgBuffer.m_uDataInfo.m_sCANMsg.m_unMsgID.ToString("X2");
            if ((ID != "7E0") && ID != "7E8" && ID != "7DF")
            {
                TypeofControl = "----";
            }
            string[] values = {
                    DateTime.Now.ToString("HH:mm:ss.ffff"),
                    type,
                    sg_asMsgBuffer.m_uDataInfo.m_sCANMsg.m_ucChannel.ToString(),
                    TypeofControl,
                    sg_asMsgBuffer.m_uDataInfo.m_sCANMsg.m_unMsgID.ToString("X2"),
                    sg_asMsgBuffer.m_uDataInfo.m_sCANMsg.m_ucDataLen.ToString(),
                    CANData
                };
            return values;
        }
    }
}
