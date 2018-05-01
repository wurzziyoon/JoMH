using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GameHelper = Tools.Tools.GameHelper;

namespace War3MH
{
    class GameHack
    {
        /// <summary>
        /// 所有游戏版本
        /// </summary>
        public enum AppVersion
        {
            Ver_1_20E,
            Ver_1_24B,
            Ver_1_24E,
            Ver_1_25B,
            Ver_1_26B
        }
        private AppVersion m_appVersion;
        private GameHelper m_gameHelper;
        private ulong m_moduleAddress;
        private string m_modulePath;
        private IntPtr m_hProcess;
        private bool[] m_mapHackOptions;    //1.小地图显示单位 2.大地图显示单位 3.分辨换线 4.显示神符 5.敌方信号 6.视野外点击

        private delegate string m_getNameDelegate(TextBox nameTB);
        #region CancelFunction
        /*
        /// <summary>
        /// 全图功能
        /// </summary>
        private enum functionName 
        {
            BigmapRemoveMist,   //大地图去迷雾
            BigmapShowUnit,     //大地图显示单位
            BigmapShowInvisible,//大地图显示隐身
            BigmapAnalysisUnit, //大地图分辨幻象
            BigmapShowHierogram,//大地图显示神符
            SmallmapRemoveMist, //小地图去迷雾
            SmallmapShowUnit,   //小地图显示单位
            SmallmapShowInvisible//小地图显示隐身
           // ShowEnemySign,     //显示敌军信号(取消)
           // AllowBusiness      //允许交易(取消)
           // ShowSkills         //显示技能(取消)
        }
        /// <summary>
        /// 偏移信息
        /// </summary>
        public struct OffSet
        {
            public uint MemAdd;
            public byte[] Data;
            public byte[] Data2;
        }
        private Dictionary<functionName, OffSet> DetialOffset;*/
        #endregion
       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="version">游戏版本</param>
        /// <param name="opionts">全图功能</param>
        public GameHack(AppVersion version,GameHelper gameHelper,bool[] opionts)
        {
            this.m_appVersion = version;
            this.m_gameHelper = gameHelper;
            GameHelper.ModuleInfo[] miArray = gameHelper.GetEntryPoint();
            m_mapHackOptions = new bool[opionts.Length];
            for (int i = 0; i < opionts.Length; i++)
            {
                m_mapHackOptions[i] = opionts[i];
            }
            foreach (GameHelper.ModuleInfo mi in miArray)
            {
                if (mi.ModuleName.ToString().LastIndexOf("Game.dll") > 0)
                {
                    m_modulePath = mi.ModuleName.ToString();
                    m_moduleAddress = mi.Address.ToUInt64();
                    Open();
                    break;
                }
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="memAddress">内存偏移</param>
        /// <param name="data">数据(以\x的形式存储,字符串前加@确保未使用转义 如:\x6d\x3f等于byte[]{0x6d,0x3f})</param>
        private void Patch(uint memAddress, string data)
        {
            m_gameHelper.WriteMemWithoutVP(m_hProcess,(uint)m_moduleAddress+memAddress,AnalysisArrayData( data));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="memAddress">内存偏移</param>
        /// <param name="data">数据</param>
        private void Patch(uint memAddress, byte[] data)
        {
            m_gameHelper.WriteMemWithoutVP(m_hProcess, (uint)m_moduleAddress + memAddress, data);
        }
       
        /// <summary>
        /// 解析字符串
        /// </summary>
        /// <param name="data">数据(字符串前必须使用@)</param>
        /// <returns></returns>
        private byte[] AnalysisArrayData(string data)
        {
            string[] dataArray = data.Split(new string[] { @"\x" }, StringSplitOptions.RemoveEmptyEntries);
            byte[] returnArray = new byte[dataArray.Length];
            for (int i = 0; i < dataArray.Length; i++)
            {
                try
                {
                    returnArray[i] = Convert.ToByte("0x" + dataArray[i].ToString(), 16);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            return returnArray;
        }
        
        private void Open()
        {
            switch (this.m_appVersion)
            {
                case AppVersion.Ver_1_20E:
                    {
                        uint oldProctet;
                        m_hProcess = m_gameHelper.OpenProcess((uint)m_gameHelper.ReadProcess.Id);
                        m_gameHelper.VirtualProtect(m_hProcess, (IntPtr)m_moduleAddress + 0x1000, 0x704000, Tools.Tools.PAGE_EXECUTE_READWRITE, out oldProctet);
                        //1.小地图显示单位 2.大地图显示单位 3.分辨换线 4.显示神符 5.敌方信号 6.视野外点击
                        if (m_mapHackOptions[0]) 
                        {
                            //////////////////////////////////////////////////////显示单位
                            Patch(0x1491A8, @"\x00");
                            /////////////////////////////////////////////////////显示隐形
                            Patch(0x1494E0, @"\x33\xC0\x0F\x85");
                        }
                        if (m_mapHackOptions[1])
                        {
                            ///////////////////////////////////////////////////大地图显示单位
                            Patch(0x2A0930, @"\xD2");
                            ///////////////////////////////////////////////////大地图显示隐形
                            Patch(0x17D4C2, @"\x90\x90");
                            Patch(0x17D4CC, @"\xEB\x00\xEB\x00\x75\x30");
                            /////////////////////////////////////////////////野外显血
                            Patch(0x166E5E, @"\x90\x90\x90\x90\x90\x90\x90\x90");
                            Patch(0x16FE0A, @"\x33\xC0\x90\x90");
                        }
                        if (m_mapHackOptions[2])
                        {
                            ///////////////////////////////////////////////////////分辨幻影
                            Patch(0x1ACFFC, @"\x40\xC3");
                        }
                        if (m_mapHackOptions[3])
                        {
                            ///////////////////////////////////////////////////////显示神符
                            Patch(0x2A07C5, @"\x49\x4B\x33\xDB\x33\xC9");
                        }
                        if (m_mapHackOptions[4])
                        {
                            ////////////////////////////////////////////////////敌方信号
                            Patch(0x321CC4, @"\x39\xC0\x0F\x85");
                            Patch(0x321CD7, @"\x39\xC0\x75");
                        }
                        if (m_mapHackOptions[5])
                        {
                            ////////////////////////////////////////////////视野外点选
                            Patch(0x1BD5A7, @"\x90\x90");
                            Patch(0x1BD5BB, @"\xEB");
                        }
                        #region 未实现部分
                        /*
                        ///////////////////////////////////////////////////大地图去除迷雾
                            Patch(0x406B53, @"\x90\x8B\x09");
                        ///////////////////////////////////////////////////小地图去除迷雾
                            Patch(0x147C53, @"\xEC");
                        //////////////////////////////////////////////////他人提示
                            Patch(0x124DDD, @"\x39\xC0\x0F\x85");
                        /////////////////////////////////////////////////显示敌方头像
                            Patch(0x137BA5, @"\xE7\x7D");
                            Patch(0x137BAC, @"\x85\xA3\x02\x00\x00\xEB\xCE\x90\x90\x90\x90");
                        //////////////////////////////////////////////////盟友头像
                            Patch(0x137BA5, @"\xE7\x7D");
                            Patch(0x137BB1, @"\xEB\xCE\x90\x90\x90\x90");                      
                        //////////////////////////////////////////////资源面板
                            Patch(0x13EF03, @"\xEB");
                        /////////////////////////////////////////////允许交易
                            Patch(0x127B3D, @"\x40\xB8\x64");
                        //////////////////////////////////////////////显示技能
                            Patch(0x12DC1A, @"\x33");
                            Patch(0x12DC1B, @"\xC0");
                            Patch(0x12DC5A, @"\x33");
                            Patch(0x12DC5B, @"\xC0");
                            Patch(0x1BFABE, @"\xEB");
                            Patch(0x442CC0, @"\x90");
                            Patch(0x442CC1, @"\x40");
                            Patch(0x442CC2, @"\x30");
                            Patch(0x442CC3, @"\xC0");
                            Patch(0x442CC4, @"\x90");
                            Patch(0x442CC5, @"\x90");
                            Patch(0x443375, @"\x30");
                            Patch(0x443376, @"\xC0");
                            Patch(0x45A641, @"\x90");
                            Patch(0x45A642, @"\x90");
                            Patch(0x45A643, @"\x33");
                            Patch(0x45A644, @"\xC0");
                            Patch(0x45A645, @"\x90");
                            Patch(0x45A646, @"\x90");
                            Patch(0x45E79E, @"\x90");
                            Patch(0x45E79F, @"\x90");
                            Patch(0x45E7A0, @"\x33");
                            Patch(0x45E7A1, @"\xC0");
                            Patch(0x45E7A2, @"\x90");
                            Patch(0x45E7A3, @"\x90");
                            Patch(0x466527, @"\x90");
                            Patch(0x466528, @"\x90");
                            Patch(0x46B258, @"\x90");
                            Patch(0x46B259, @"\x33");
                            Patch(0x46B25A, @"\xC0");
                            Patch(0x46B25B, @"\x90");
                            Patch(0x46B25C, @"\x90");
                            Patch(0x46B25D, @"\x90");
                            Patch(0x4A11A0, @"\x33");
                            Patch(0x4A11A1, @"\xC0");
                            Patch(0x54C0BF, @"\x90");
                            Patch(0x54C0C0, @"\x33");
                            Patch(0x54C0C1, @"\xC0");
                            Patch(0x54C0C2, @"\x90");
                            Patch(0x54C0C3, @"\x90");
                            Patch(0x54C0C4, @"\x90");
                            Patch(0x5573FE, @"\x90");
                            Patch(0x5573FF, @"\x90");
                            Patch(0x557400, @"\x90");
                            Patch(0x557401, @"\x90");
                            Patch(0x557402, @"\x90");
                            Patch(0x557403, @"\x90");
                            Patch(0x55E15C, @"\x90");
                            Patch(0x55E15D, @"\x90");
                        ///////////////////////////////////////////////资源条
                            Patch(0x150981, @"\xEB\x02");
                            Patch(0x1509FE, @"\xEB\x02");
                            Patch(0x151597, @"\xEB\x02");
                            Patch(0x151647, @"\xEB\x02");
                            Patch(0x151748, @"\xEB\x02");
                            Patch(0x1BED19, @"\xEB\x02");
                            Patch(0x314A9E, @"\xEB\x02");
                            Patch(0x21EAD4, @"\xEB");
                            Patch(0x21EAE8, @"\x03");
                        
                       
                        /////////////////////////////////////////////////无限取消
                           // Patch(0x23D60F, @"\xEB");
                           // Patch(0x21EAD4, @"\x03");
                           // Patch(0x21EAE8, @"\x03");
                         * */
                        #endregion
                        //////////////////////////////////////////////////过-MH
                            Patch(0x2C5A7E, @"\x90\x90");
                        /////////////////////////////////////////反-AH
                            Patch(0x2C240C, @"\x3C\x4C\x74\x04\xB0\xFF\xEB\x04\xB0\xB0\x90\x90");
                            Patch(0x2D34ED, @"\xE9\xB3\x00\x00\x00\x90");
                            
                           bool isOK= m_gameHelper.VirtualProtect(m_hProcess, (IntPtr)m_moduleAddress + 0x1000, 0x704000, oldProctet, out oldProctet);
                            m_gameHelper.CloseHandle(m_hProcess);
                        break;
                    }
                case AppVersion.Ver_1_24B:
                    {
                        uint oldProctet;
                        m_hProcess = m_gameHelper.OpenProcess((uint)m_gameHelper.ReadProcess.Id);
                        m_gameHelper.VirtualProtect(m_hProcess, (IntPtr)m_moduleAddress + 0x1000, 0x87E000, Tools.Tools.PAGE_EXECUTE_READWRITE, out oldProctet);
                        //1.小地图显示单位 2.大地图显示单位 3.分辨换线 4.显示神符 5.敌方信号 6.视野外点击
                        if (m_mapHackOptions[0])
                        {
                            /////////////////////////////////////////////////////小地图显示单位
                            Patch(0x361EAB, @"\x90\x90\x39\x5E\x10\x90\x90\xB8\x00\x00\x00\x00\xEB\x07");
                            ///////////////////////////////////////////////////////小地图显示隐形
                            Patch(0x361EBC, @"\x00");
                        }
                        if (m_mapHackOptions[1])
                        {
                            ///////////////////////////////////////////////////////大地图显示单位
                            Patch(0x3A201D, @"\xEB");
                            ////////////////////////////////////////////////////////大地图显示隐形
                            Patch(0x3622D1, @"\x3B");
                            Patch(0x3622D4, @"\x85");
                            Patch(0x39A45B, @"\x90\x90\x90\x90\x90\x90");
                            Patch(0x39A46E, @"\x90\x90\x90\x90\x90\x90\x90\x90\x33\xC0\x40");
                        }
                        if (m_mapHackOptions[2])
                        {
                            /////////////////////////////////////////////////////分辨幻影
                            Patch(0x28351C, @"\x40\xC3");
                       
                        }
                        if (m_mapHackOptions[3])
                        {
                            /////////////////////////////////////////////////////显示神符
                            Patch(0x4076CA, @"\x90\x90");
                            Patch(0x3A1F5B, @"\xEB");
                        }
                        if (m_mapHackOptions[4])
                        {
                            //////////////////////////////////////////////////////敌方信号
                            Patch(0x43F956, @"\x3B");
                            Patch(0x43F959, @"\x85");
                            Patch(0x43F969, @"\x3B");
                            Patch(0x43F96C, @"\x85");
                        }
                        if (m_mapHackOptions[5])
                        {
                            ////////////////////////////////////////////////////////视野外点击
                            Patch(0x285C4C, @"\x90\x90");
                            Patch(0x285C62, @"\xEB");
                        }
                        #region 未实现部分
                        /*
                        /////////////////////////////////////////////////////////////////////大地图去除迷雾
                            Patch(0x74D103, @"\xC6\x04\x3E\x01\x90\x46");
                        ///////////////////////////////////////////////////////小地图去除迷雾
                            Patch(0x356FA5, @"\x90\x90");
                        
                        ////////////////////////////////////////////////////////他人提示
                            Patch(0x334529, @"\x39\xC0\x0F\x85");
                        ///////////////////////////////////////////////////////敌人头像
                            Patch(0x371640, @"\xE8\x3B\x28\x03\x00\x85\xC0\x0F\x85\x8F\x02\x00\x00\xEB\xC9\x90\x90\x90\x90");
                        /////////////////////////////////////////////////////盟友头像
                            Patch(0x371640, @"\xE8\x3B\x28\x03\x00\x85\xC0\x0F\x84\x8F\x02\x00\x00\xEB\xC9\x90\x90\x90\x90");
                        //////////////////////////////////////////////////////资源面板
                            Patch(0x3604CA, @"\x90\x90");
                        ///////////////////////////////////////////////////////允许交易
                            Patch(0x34E822, @"\xB8\xE0\x03\x00");
                            Patch(0x34E827, @"\x90");
                            Patch(0x34E82A, @"\xB8\x64\x90\x90");
                            Patch(0x34E82F, @"\x90");
                        //////////////////////////////////////////////////////查看技能
                            Patch(0x28EC8E, @"\xEB");
                            Patch(0x20318C, @"\x90\x90\x90\x90\x90\x90");
                            Patch(0x34FD28, @"\x90\x90");
                            Patch(0x34FD66, @"\x90\x90\x90\x90");
                       
                        /////////////////////////////////////////////////////////////无限取消
                          //  Patch(0x57B9FC, @"\xEB");
                          //  Patch(0x5B2CC7, @"\x03");
                          //  Patch(0x5B2CDB, @"\x03");
                         * */
                        #endregion
                        ////////////////////////////////////////////////////////过-MH
                            Patch(0x3C8407, @"\xEB\x11");
                            Patch(0x3C8427, @"\xEB\x11");
                        //////////////////////////////////////////////////////////反-AH
                            Patch(0x3C6E1C, @"\xB8\xFF\x00\x00\x00\xEB");
                            Patch(0x3CC2F2, @"\xEB");
                            GameHelper.ModuleInfo[] miArray = m_gameHelper.GetEntryPoint();
                            //ClearNameMemory((long)miArray[0].Address + 0x194120);
                            m_gameHelper.WriteMem((long)miArray[0].Address + 0x194120, AnalysisArrayData(ParseStrToHexStr("Turbu1ence")));
                            bool isOK = m_gameHelper.VirtualProtect(m_hProcess, (IntPtr)m_moduleAddress + 0x1000, 0x87E000, oldProctet, out oldProctet);
                            m_gameHelper.CloseHandle(m_hProcess);
                        break;
                    }
                case AppVersion.Ver_1_24E:
                    {
                        uint oldProctet;
                        m_hProcess = m_gameHelper.OpenProcess((uint)m_gameHelper.ReadProcess.Id);
                        m_gameHelper.VirtualProtect(m_hProcess, (IntPtr)m_moduleAddress + 0x1000, 0x87E000, Tools.Tools.PAGE_EXECUTE_READWRITE, out oldProctet);
                        //1.小地图显示单位 2.大地图显示单位 3.分辨换线 4.显示神符 5.敌方信号 6.视野外点击
                        if (m_mapHackOptions[0])
                        {
                            /////////////////////////////////////////////小地图显示单位
                            Patch(0x361F7C, @"\x00");
                        }
                        if (m_mapHackOptions[1])
                        {
                            ////////////////////////////大地图显示单位
                            Patch(0x39EBBC, @"\x75");
                            Patch(0x3A2030, @"\x90");
                            Patch(0x3A2031, @"\x90");
                            Patch(0x3A20DB, @"\x90");
                          //test  Patch(0x3A20DC, @"\x90");
                            ////////////////////////////////////////////////////////////////////////显示单位
                            // 	Patch(0x356FFE,@"\90");
                            // 	Patch(0x356FFF,@"\90");
                            // 	Patch(0x357000,@"\90");
                           Patch(0x362391, @"\x3B");
                            Patch(0x362394, @"\x85");
                            Patch(0x39A51B, @"\x90");
                            Patch(0x39A51C, @"\x90");
                            Patch(0x39A51D, @"\x90");
                            Patch(0x39A51E, @"\x90");
                            Patch(0x39A51F, @"\x90");
                            Patch(0x39A520, @"\x90");
                            Patch(0x39A52E, @"\x90");
                            Patch(0x39A52F, @"\x90");
                            Patch(0x39A530, @"\x90");
                            Patch(0x39A531, @"\x90");
                            Patch(0x39A532, @"\x90");
                            Patch(0x39A533, @"\x90");
                            Patch(0x39A534, @"\x90");
                            Patch(0x39A535, @"\x90");
                            Patch(0x39A536, @"\x33");
                            Patch(0x39A537, @"\xC0");
                            Patch(0x39A538, @"\x40");

                            //显示道具
                            Patch(0x3A201B, @"\xEB");
                            Patch(0x40A864, @"\x90");
                           //test Patch(0x40A865, @"\x90");
                        }
                        if (m_mapHackOptions[2])
                        {
                            ///////////////////////////////////////////分辨幻象
                            Patch(0x28357C, @"\x40");
                            Patch(0x28357D, @"\xC3");
                        }
                        if (m_mapHackOptions[3])
                        {
                            //////////////////////////////////////////////显示cd
                            Patch(0x28ECFE, @"\xEB");
                            Patch(0x34FE26, @"\x90");
                            Patch(0x34FE27, @"\x90");
                            Patch(0x34FE28, @"\x90");
                            Patch(0x34FE29, @"\x90");
                        }
                        if (m_mapHackOptions[4])
                        {
                            /////////////////////////////////////////////敌方信号
                            Patch(0x43F9A6, @"\x3B");
                            Patch(0x43F9A9, @"\x85");
                            Patch(0x43F9B9, @"\x3B");
                            Patch(0x43F9BC, @"\x85");
                        }
                        if (m_mapHackOptions[5])
                        {
                            ///////////////////////////////////////////////视野外框选
                            Patch(0x285CBC, @"\x90");
                            Patch(0x285CBD, @"\x90");
                            Patch(0x285CD2, @"\xEB");
                        }
                        #region 未实现部分
                        /*
                        ////////////////////////////////////////////////////////////////////////大地图去除迷雾
		                Patch(0x74D1B9,@"\xB2\x00\x90\x90\x90\x90");
	                    ////////////////////////////////////////////小地图 去除迷雾
		                    Patch(0x357065,@"\x90\x90");
	                    /////////////////////////////////////////////他人提示
		                    Patch(0x3345E9,@"\x39\xC0\x0F\x85");
	                    //////////////////////////////////////////////地方头像
		                    Patch(0x371700,@"\xE8\x3B\x28\x03\x00\x85\xC0\x0F\x85\x8F\x02\x00\x00\xEB\xC9\x90\x90\x90\x90");
	                    /////////////////////////////////////盟友头像
		                    Patch(0x371700,@"\xE8\x3B\x28\x03\x00\x85\xC0\x0F\x84\x8F\x02\x00\x00\xEB\xC9\x90\x90\x90\x90");
	                    //////////////////////////////////////显示资源
	                    Patch(0x36058A,@"\x90");
	                    Patch(0x36058B,@"\x90");
	                    ///////////////////////////////////////////允许交易
	                    Patch(0x34E8E2,@"\xB8");
	                    Patch(0x34E8E3,@"\xC8");
	                    Patch(0x34E8E4,@"\x00");
	                    Patch(0x34E8E5,@"\x00");
	                    Patch(0x34E8E7,@"\x90");
	                    Patch(0x34E8EA,@"\xB8");
	                    Patch(0x34E8EB,@"\x64");
	                    Patch(0x34E8EC,@"\x00");
	                    Patch(0x34E8ED,@"\x00");
	                    Patch(0x34E8EF,@"\x90");
	                    ///////////////////////////////////////////////显示技能
	                    Patch(0x2031EC,@"\x90");
	                    Patch(0x2031ED,@"\x90");
	                    Patch(0x2031EE,@"\x90");
	                    Patch(0x2031EF,@"\x90");
	                    Patch(0x2031F0,@"\x90");
	                    Patch(0x2031F1,@"\x90");
	                    Patch(0x34FDE8,@"\x90");
	                    Patch(0x34FDE9,@"\x90");
	                    //////////////////////////////////////////////显示cd
	                    Patch(0x28ECFE,@"\xEB");
	                    Patch(0x34FE26,@"\x90");
	                    Patch(0x34FE27,@"\x90");
	                    Patch(0x34FE28,@"\x90");
	                    Patch(0x34FE29,@"\x90");
	                    //////////////////////////////////////////////////无限取消
		                   // Patch(0x57BA7C,@"\xEB");
		                   // Patch(0x5B2D77,@"\x03");
		                   // Patch(0x5B2D8B,@"\x03");
                         * */
                        #endregion
                        /////////////////////////////////////////////////////过-MH
		                    Patch(0x3C84C7,@"\xEB\x11");
		                    Patch(0x3C84E7,@"\xEB\x11");
	                    ////////////////////////////////////////////////////反-AH
		                    Patch(0x3C6EDC,@"\xB8\xFF\x00\x00\x00\xEB");
		                    Patch(0x3CC3B2,@"\xEB");
                            GameHelper.ModuleInfo[] miArray = m_gameHelper.GetEntryPoint();
                           // ClearNameMemory((long)miArray[0].Address + 0x1984120);
                            m_gameHelper.WriteMem((long)miArray[0].Address + 0x1984120, AnalysisArrayData(ParseStrToHexStr("Turbu1ence")));
                            bool isOK = m_gameHelper.VirtualProtect(m_hProcess, (IntPtr)m_moduleAddress + 0x1000, 0x87E000, oldProctet, out oldProctet);
                            m_gameHelper.CloseHandle(m_hProcess);
                        break;
                    }
                case AppVersion.Ver_1_25B:
                    {

                        break;
                    }
                case AppVersion.Ver_1_26B:
                    {

                        break;
                    }
                default:
                    {
                        throw new Exception("Version not found");
                    }
            }
        }
        /// <summary>
        /// 讲字符串解析成十六进制字符串
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns></returns>
        private string ParseStrToHexStr(string str)
        {
            byte[] bArray = Encoding.UTF8.GetBytes(str);
            string temp = "";
            foreach (byte b in bArray)
            {
                temp += "\\x"+b.ToString("x") ;
            }
            temp = temp.Trim();
            return temp;
        }
        private string GetTextBoxStr(TextBox tb)
        {
            return tb.Text;
        }
        private void ClearNameMemory(long baseAddress)
        {
            for (int j = 0; j < 26; j++)
            {
                m_gameHelper.WriteMem((long)baseAddress + j, new byte[]{0x00});
            }
        }
    }
}
