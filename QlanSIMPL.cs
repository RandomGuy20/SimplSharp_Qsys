using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace DSP_Suite.Qsys
{
    public class QlanSIMPL
    {
        #region Fields

        private QLanQsys qLan;

        #endregion Fields

        #region Properties


        #endregion Properties

        #region Delegates and Events

        public delegate void EnabledEventHandler(ushort value);
        public EnabledEventHandler onEnabled { get; set; }

        public delegate void BlockIsActive(ushort value);
        public BlockIsActive onBlockActive { get; set; }

        public delegate void StreamIsActive(ushort value);
        public StreamIsActive onStreamActive { get; set; }

        public delegate void StreamNameChange(SimplSharpString name);
        public StreamNameChange onStreamNameChange { get; set; }

        #endregion

        #region Constructors

        public void Initialize(string name, string coreID, string pollgroup,ushort RX)
        {
            qLan = new QLanQsys(ProcessorHolder.holder[coreID], pollgroup, name, Convert.ToBoolean(RX));
            qLan.onQlanBlockChange += new QLanQsys.QlanChangeEventHandler(qLan_onQlanBlockChange);
            qLan.onStreamNameChange += new QLanQsys.QlanStreamNameChangeEventHandler(qLan_onStreamNameChange);
        }

        #endregion Constructors

        #region Internal Methods

        void qLan_onStreamNameChange(string newName)
        {
            onStreamNameChange(newName);
        }

        void qLan_onQlanBlockChange(eQlanArgs args, bool state)
        {
            switch (args)
            {
                case eQlanArgs.Enabled:
                    onEnabled(Convert.ToUInt16(state));
                    break;
                case eQlanArgs.BlockActive:
                    onBlockActive(Convert.ToUInt16(state));
                    break;
                case eQlanArgs.StreamActive:
                    onStreamActive(Convert.ToUInt16(state));
                    break;
                default:
                    break;
            }
        }

        #endregion Internal Methods

        #region Public Methods

        public void EnableBlock()
        {
            qLan.EnableBlock();
        }

        public void SetStreamName(string stream)
        {
            qLan.SetStreamName(stream);
        }

        #endregion Public Methods
    }
}