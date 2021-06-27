using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace DSP_Suite.Qsys
{
    public class ProcessorSIMPL
    {
        ProcessorQsys core;

        public delegate void CoreStatusUpdated(SimplSharpString designName, SimplSharpString corePlatform, ushort isEmulator, ushort isRedundant, ushort isActive);
        public CoreStatusUpdated CoreIsUpdated { get; set; }

        public delegate void CoreInitialized(ushort isInitialized);
        public CoreInitialized CoreIsInitialized { get; set; }

        public delegate void CoreConnection(ushort isConnected);
        public CoreConnection CoreIsConnected { get; set; }

        public void Initialize(string ipaddress, string coreID)
        {
            try
            {
                core = new ProcessorQsys(ipaddress, coreID);
                core.QsysProcessorConnectionEvent += new ProcessorQsys.QsysProcessorConnectionEventHandler(core_QsysProcessorConnectionEvent);
                core.QsysProcessorInfoEvent += new ProcessorQsys.QsysProcessorInfoEventHandler(core_QsysProcessorInfoEvent);
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("Error in Processor Simpl Initialize is: " + e);
            }

        }

        void core_QsysProcessorInfoEvent(ProcessorStatusEvents status)
        {
            CoreIsUpdated(status.DesignName, status.CorePlateform, Convert.ToUInt16(status.IsEmulator), Convert.ToUInt16(status.IsRedundant), Convert.ToUInt16(status.IsActive));
        }

        void core_QsysProcessorConnectionEvent(eQSCProcessorEventIDs ID, bool state, int value)
        {
            switch (ID)
            {
                case eQSCProcessorEventIDs.Registered:
                    CoreIsInitialized((ushort)value);
                    break;
                case eQSCProcessorEventIDs.Connected:
                    CoreIsConnected((ushort)value);
                    break;
                default:
                    break;
            }
        }

        public void setDebug(ushort val)
        {
            core.Debug = Convert.ToBoolean(val);
        }
    }
}