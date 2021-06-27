using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace DSP_Suite.Qsys
{
    public class SystemMuteSIMPL
    {
        #region Fields

        private SystemMuteQsys system;

        #endregion Fields

        #region Properties


        #endregion Properties

        #region Delegates and Events

        public delegate void SendMute(ushort mute);
        public SendMute onSystemMute { get; set; }

        public delegate void SystemGainChange(ushort gain);
        public SystemGainChange onGainChange { get; set; }


        #endregion

        #region Constructor

        public void Initialize(string name, string coreID, string pollgroup)
        {
            system = new SystemMuteQsys(ProcessorHolder.holder[coreID], pollgroup, name);
            system.onGainChange += new SystemMuteQsys.SystemGainChange(system_onGainChange);
            system.onSystemMute += new SystemMuteQsys.SendMute(system_onSystemMute);
        }
                
        #endregion Constructor

        #region Internal Methods

        void system_onSystemMute(bool mute)
        {
            onSystemMute(Convert.ToUInt16(mute));
        }

        void system_onGainChange(int gain)
        {
            onGainChange((ushort)gain);
        }

        #endregion

        #region Public Methods

        public void MuteToggle(ushort mute)
        {
            system.MuteToggle(Convert.ToBoolean(mute));
        }

        #endregion Public Methods
    }
}