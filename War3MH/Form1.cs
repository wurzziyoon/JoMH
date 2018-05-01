using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Media;
using System.Threading;
using System.Diagnostics;
using Tools;

namespace War3MH
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private int[] BATTLECLIENTNAMEOFFSETLIST = new int[] { 0x46AF48, 0x46AFCC, 0x46B038, 0x46F76C, 0x4954E4, 0x495D14, 0x49BEB0, 0x49CF24, 0x49DF98, 0x49F00C };
        private int[] ISSO_OFFSETLIST = new int[] { 0x311C8, 0x3144C  };
        private int[] SSS_TRD_OFFSETLIST = new int[] { 0x3144C };
        private int[] ASSOCKET_OFFSETLIST = new int[] { 0x3EB8D };
        int two = 0, three = 0, four = 0;
        private Thread m_mapHackThread;
        private int m_gameModuleAddress;
        private delegate void MapHackThreadDelegate();
        private Tools.Tools.GameHelper m_gH;
        private Tools.Tools.GameHelper m_battleClientGH;
       // public Dictionary<ulong, byte[]> BigMapHack = new Dictionary<ulong, byte[]>();
        private delegate void m_closeFormDelegate();
        private bool[] m_mapHackOptions;

        private void CloseForm()
        {
            this.Close();
        }

        private void OpenMapHack()
        {
            string modulePath=null;
            ulong moudlueAddress = 0;
            GameHack.AppVersion gameVersion;
            while (true)
            {
                Process[] gameProcess = Process.GetProcessesByName("War3");
                Thread.Sleep(3000);
                if (gameProcess.Length == 1)
                {
                   // try
                    {
                        m_gH = new Tools.Tools.GameHelper();
                        m_gH.ReadProcess = gameProcess[0];
                        GameHack gameHack;
                        Tools.Tools.GameHelper.ModuleInfo[] moduleInfos = m_gH.GetEntryPoint();
                        foreach (Tools.Tools.GameHelper.ModuleInfo mi in moduleInfos)
                       {
                           if (mi.ModuleName.ToString().LastIndexOf("Game.dll")>0)
                           {
                               modulePath = mi.ModuleName.ToString();
                               moudlueAddress = mi.Address.ToUInt64();
                           }
                       }
                       FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(modulePath);
                       switch (fvi.FileVersion)
                       {
                           case "1, 20, 4, 6074": { gameVersion = GameHack.AppVersion.Ver_1_20E; gameHack = new GameHack(GameHack.AppVersion.Ver_1_20E,m_gH, m_mapHackOptions); break; }
                           case "1, 24, 1, 6374": { gameVersion = GameHack.AppVersion.Ver_1_24B; gameHack = new GameHack(GameHack.AppVersion.Ver_1_24B, m_gH, m_mapHackOptions); break; }
                           case "1, 24, 4, 6387": { gameVersion = GameHack.AppVersion.Ver_1_24E; gameHack = new GameHack(GameHack.AppVersion.Ver_1_24E, m_gH, m_mapHackOptions); break; }
                           case "1, 25, 1, 6397": { gameVersion = GameHack.AppVersion.Ver_1_25B; gameHack = new GameHack(GameHack.AppVersion.Ver_1_25B, m_gH, m_mapHackOptions); break; }
                           case "1, 26, 0, 6401": { gameVersion = GameHack.AppVersion.Ver_1_26B; gameHack = new GameHack(GameHack.AppVersion.Ver_1_26B, m_gH, m_mapHackOptions); break; }
                           default: 
                               {
                                   throw new Exception("Version not found!");
                                }
                       }
                       
                        SoundPlayer openSoundPlayer = new SoundPlayer(Properties.Resources.RGodLike);
                        openSoundPlayer.Play();
                        Thread.Sleep(1000);
                        this.Invoke(new m_closeFormDelegate(CloseForm));
                        break;
                    }
                    // catch (Exception e)
                    //{
                        //MessageBox.Show(e.StackTrace);
                        //this.Invoke(new m_closeFormDelegate(CloseForm));
                        //this.Close();
                    //}
                }
                Thread.Sleep(1000);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            m_mapHackOptions=new bool[6];
            foreach (Control cb in tabControl1.TabPages[0].Controls)
            {
                if (cb is CheckBox)
                {
                    CheckBox tmp = (CheckBox)cb;
                    switch (tmp.Name)
                    {
                        case "checkBox1":
                            {
                                m_mapHackOptions[0] = tmp.Checked ? true : false;
                                break;
                            }
                        case "checkBox2":
                            {
                                m_mapHackOptions[1] = tmp.Checked ? true : false;
                                break;
                            }
                        case "checkBox3":
                            {
                                m_mapHackOptions[2] = tmp.Checked ? true : false;
                                break;
                            }
                        case "checkBox4":
                            {
                                m_mapHackOptions[3] = tmp.Checked ? true : false;
                                break;
                            }
                        case "checkBox5":
                            {
                                m_mapHackOptions[4] = tmp.Checked ? true : false;
                                break;
                            }
                        case "checkBox6":
                            {
                                m_mapHackOptions[5] = tmp.Checked ? true : false;
                                break;
                            }
                    }
                }
            }
            m_mapHackThread = new Thread(new ThreadStart(OpenMapHack));
            m_mapHackThread.IsBackground = true;
            m_mapHackThread.Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            byte[] bs = Encoding.UTF8.GetBytes(textBox1.Text);
            string temp = "";
            foreach (byte b in bs)
            {
                //temp += "\\x"+b.ToString("x") ;
                temp += b.ToString("x") + " ";
            }
            temp = temp.Trim();
            Clipboard.SetText(temp);
        }
        private byte[] AnalysisArrayData(string data)
        {
            string[] dataArray = data.Split(new string[]{@"\x"}, StringSplitOptions.RemoveEmptyEntries);
            byte[] returnArray = new byte[dataArray.Length];
            for (int i = 0; i < dataArray.Length; i++)
            {
                try
                {
                    returnArray[i] = Convert.ToByte("0x" + dataArray[i].ToString(),16);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            return returnArray;
        }
        private void BattleClientClearNameMemory()
        {
            for (int i = 0; i < BATTLECLIENTNAMEOFFSETLIST.Length; i++)
            {
                for (int j = 0; j < 26; j++)
                {
                    m_battleClientGH.WriteByte(m_gameModuleAddress + BATTLECLIENTNAMEOFFSETLIST[i] + j, 0);
                }
            }
            for (int i = 0; i < ISSO_OFFSETLIST.Length; i++)
            {
                for (int j = 0; j < 26; j++)
                {
                    m_battleClientGH.WriteByte(two + BATTLECLIENTNAMEOFFSETLIST[i] + j, 0);
                }
            }
            for (int i = 0; i < SSS_TRD_OFFSETLIST.Length; i++)
            {
                for (int j = 0; j < 26; j++)
                {
                    m_battleClientGH.WriteByte(three + BATTLECLIENTNAMEOFFSETLIST[i] + j, 0);
                }
            }
            for (int i = 0; i < ASSOCKET_OFFSETLIST.Length; i++)
            {
                for (int j = 0; j < 26; j++)
                {
                    m_battleClientGH.WriteByte(four + BATTLECLIENTNAMEOFFSETLIST[i] + j, 0);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Process battleClientTask = Process.GetProcessesByName("11Game")[0];
            if (battleClientTask == null)
            {
                MessageBox.Show("Task not found");
                return;
            }
            else
            {
                m_battleClientGH = new Tools.Tools.GameHelper();
                string name = null;
                byte[] bys = Encoding.Unicode.GetBytes(textBox1.Text);
                foreach (byte b in bys)
                {
                    name += b.ToString() + " ";
                }
                Clipboard.SetText(name.Trim());
                if (bys.Length > 26)   //!=5
                {
                    textBox1.Text = "";
                    MessageBox.Show("长度太长");
                    return;
                }
                else
                {
                    name = textBox1.Text;
                    m_battleClientGH.ReadProcess = battleClientTask;
                    Tools.Tools.GameHelper.ModuleInfo[] moduleArray = m_battleClientGH.GetEntryPoint();
                    m_gameModuleAddress = (int)moduleArray[0].Address;

                    foreach (Tools.Tools.GameHelper.ModuleInfo mi in moduleArray)
                    {
                        if (mi.ModuleName.ToString().ToUpper().Contains("11SSO.DLL"))
                        {
                            two = (int)mi.Address;
                        }
                        if (mi.ModuleName.ToString().ToUpper().Contains("11SSO_TRD.DLL"))
                        {
                            three = (int)mi.Address;
                        }
                        if (mi.ModuleName.ToString().ToUpper().Contains("ASSOCKET.DLL"))
                        {
                            four = (int)mi.Address;
                        }
                    }
                    byte[] unicodeName = Encoding.Unicode.GetBytes(name);
                    BattleClientClearNameMemory();
                    for (int i = 0; i < BATTLECLIENTNAMEOFFSETLIST.Length; i++)
                    {
                        for (int j = 0; j < unicodeName.Length; j++)
                        {
                            m_battleClientGH.WriteByte(m_gameModuleAddress + BATTLECLIENTNAMEOFFSETLIST[i] + j, unicodeName[j]);
                        }
                    }
                    
                    for (int i = 0; i < ISSO_OFFSETLIST.Length; i++)
                    {
                        for (int j = 0; j < unicodeName.Length; j++)
                        {
                            m_battleClientGH.WriteByte(two + BATTLECLIENTNAMEOFFSETLIST[i] + j, unicodeName[j]);
                        }
                    }
                    for (int i = 0; i < SSS_TRD_OFFSETLIST.Length; i++)
                    {
                        for (int j = 0; j < unicodeName.Length; j++)
                        {
                            m_battleClientGH.WriteByte(three + BATTLECLIENTNAMEOFFSETLIST[i] + j, unicodeName[j]);
                        }
                    }
                    for (int i = 0; i < ASSOCKET_OFFSETLIST.Length; i++)
                    {
                        for (int j = 0; j < unicodeName.Length; j++)
                        {
                            m_battleClientGH.WriteByte(four + BATTLECLIENTNAMEOFFSETLIST[i] + j, unicodeName[j]);
                        }
                    }
                        MessageBox.Show(" 修改完成");
                }
            }
        }
    }
}
