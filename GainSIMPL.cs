using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace DSP_Suite.Qsys
{
    public class GainSIMPL
    {
        #region Fields

        private GainQsys gain;


        public delegate void VolumeLevel(ushort level);
        public VolumeLevel VolumeChange { get; set; }

        public delegate void MuteState(ushort mute);
        public MuteState MuteChange { get; set; }

        #endregion Fields

        public void Initialize(string name, string coreID, string pollgroup)
        {
            try
            {
                gain = new GainQsys(ProcessorHolder.holder[coreID], pollgroup, name);
                gain.onGainAudioChange += new GainQsys.GainLevelChangeEventHandler(gain_onGainAudioChange);
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("Error initializing Simpl+ gain is: " + e);
            }

        }

        void gain_onGainAudioChange(int level, bool mute)
        {
            VolumeChange((ushort)level);
            MuteChange(Convert.ToUInt16(mute));
        }

        public void NewMute(ushort value)
        {
            gain.SetMute(Convert.ToBoolean(value));
        }

        public void NewVolume(ushort value)
        {
            try
            {
                gain.NewVolume(value);
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("Erro gain change simpl9 is" + e);
            }

        }

    }
}