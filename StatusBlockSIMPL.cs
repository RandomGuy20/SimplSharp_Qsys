using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace DSP_Suite.Qsys
{
    public class StatusBlockSIMPL
    {
        #region Fields

        private StatusBlockQsys status;

        #endregion Fields

        #region Properties


        #endregion Properties

        #region Delegates and Events

        public delegate void CoreStatus(ushort status);
        public CoreStatus onCoreStatus { get; set; }

        #endregion

        #region Constructor

        public void Initialize(string name, string coreID, string pollgroup)
        {
            status = new StatusBlockQsys(ProcessorHolder.holder[coreID], pollgroup, name);
            status.onCoreStatus += new StatusBlockQsys.CoreStatus(status_onCoreStatus);
        }

        #endregion Constructor

        #region Internal Methods

        void status_onCoreStatus(eQSCCoreState status)
        {
            onCoreStatus((ushort)status);
        }

        #endregion

        #region Public Methods


        #endregion Public Methods
    }
}