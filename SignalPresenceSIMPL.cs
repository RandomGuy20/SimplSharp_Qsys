using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace DSP_Suite.Qsys
{
    public class SignalPresenceSIMPL
    {
        #region Fields

        private SignalPresenceQsys presence;

        #endregion Fields

        #region Properties

        #endregion Properties

        #region Delegates and Events

        public delegate void SignalEventHandler(ushort signal);
        public SignalEventHandler onSignal { get; set; }

        #endregion

        #region Constructors

        public void Initialize(string name, string coreID, string pollgroup)
        {
            presence = new SignalPresenceQsys(ProcessorHolder.holder[coreID], pollgroup, name);
            presence.onSignalPresence += new SignalPresenceQsys.SignalPresenceEventHandler(presence_onSignalPresence);
        }

        #endregion Constructors

        #region Internal Methods

        void presence_onSignalPresence(bool signal)
        {
            onSignal(Convert.ToUInt16(signal));
        }

        #endregion Internal Methods

        #region Public Methods



        #endregion Public Methods
    }
}