using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace DSP_Suite.Qsys
{
    public class AES67Qsys
    {
        #region Fields

        private string name;
        private string multicastName;
        private string pollGroup;

        private bool registered;
        private bool isComponent;
        private bool isRX;
        private bool isEnabled;

        private List<Control> controls;
        private Component component;
        private ProcessorQsys core;

        #endregion Fields

        #region Constants

        private string[] netBuff = new string[] { "Default", "Extra 1ms", "Extra 2ms", "Extra 5ms" };
        private string[] netPort = new string[] { "LAN A", "LAN B" };
        private string streamEnable = "enable";
        private string streamInterface = "interface";
        private string streamNetworkBuffer = "network.buffer";
        private string streamStatus = "status";
        private string streamName = "stream.name";
        private string streamMulticast = "mcast.addr.spec";

        #endregion Constants

        #region Properties

        public bool IsRegistered { get { return registered; } }
        public bool IsComponent { get { return isComponent; } }
        public bool IsRX { get{return isRX;} }
        public bool IsEnabled { get{return isEnabled;} }

        public string Name { get{return name;} }
        public string StreamName { get { return multicastName; } }
        public string PollingGroup { get { return pollGroup; } }

        #endregion Properties

        #region Delegates and Events

        public delegate void StreamStatusEvent(eQSCStreamStatus status);
        public event StreamStatusEvent onStreamStatus;

        public delegate void StreamEnableEvent(bool status);
        public event StreamEnableEvent onStreamEnable;

        public delegate void StreamInterfaceEvent(eQSCNetworkInterface port, string portName);
        public event StreamInterfaceEvent onStreamInterface;

        public delegate void StreamNameEvent(string iFace);
        public event StreamNameEvent onStreamName;

        public delegate void StreamNetworkBufferEvent(eQSCNetworkBuffer buff, string buffName);
        public event StreamNetworkBufferEvent onStreamNetworkBuffer;

        public delegate void StreamMulticastEvent(string mcastAddress);
        public event StreamMulticastEvent onStreamMulticast;


        #endregion Delegate and Events

        #region Constructors

        public AES67Qsys(ProcessorQsys qsysCore,string componentName, bool IsRx, string PollGroup)
        {
            core = qsysCore;
            isRX = IsRx;
            name = componentName;
            component = new Component();
            component.Name = name;
            pollGroup = PollGroup;

            controls = new List<Control>();
            for (int i = 0; i < 6; i++)
                controls.Add(new Control());

            controls[0].Name = streamEnable;
            controls[1].Name = streamInterface;
            controls[2].Name = streamName;
            controls[3].Name = streamNetworkBuffer;
            controls[4].Name = streamStatus;
            controls[5].Name = streamMulticast;

            component.Controls = controls;

            if (core.RegisterComponent(component, pollGroup)) 
            {
                core.Components[component].QsysEvent += new EventHandler<QsysEventArgs>(AES67_qsysEvent);
                registered = true;
                isComponent = true;
                CrestronConsole.PrintLine("Component {0} was succesfully registered", component.Name);
            }
        }

        #endregion Constructors

        #region Internal Methods

        private void ComponentBuilder(string commandName, object value, eQSCAES67Controls type)
        {
            ComponentSet trigger = new ComponentSet();
            trigger.Params = new ComponentSetParams();
            trigger.Params.Name = name;

            ComponentSetControl setControl = new ComponentSetControl();
            setControl.Name = commandName;

            if (type == eQSCAES67Controls.Enable)
                setControl.Value = value;
            else
                setControl.Value = (string)value;

            trigger.Params.Controls = new List<ComponentSetControl>();
            trigger.Params.Controls.Add(setControl);

            core.QCommand(core.CommandBuider(trigger));
        }

        void AES67_qsysEvent(object sender, QsysEventArgs e)
        {
            if (e.name == streamEnable)
            {
                isEnabled = e.value > 0 ? true : false;
                onStreamEnable(isEnabled);
            }
            else if (e.name == streamInterface)
            {
                if (e.stringValue == netPort[0])
                    onStreamInterface(eQSCNetworkInterface.LAN_A, eQSCNetworkInterface.LAN_A.ToString("D"));
                else
                    onStreamInterface(eQSCNetworkInterface.LAN_B, eQSCNetworkInterface.LAN_B.ToString("D"));
            }
            else if (e.name == streamName)
            {
                onStreamName(e.stringValue);
            }
            else if (e.name == streamNetworkBuffer)
            {
                if (e.stringValue == netBuff[0])
                    onStreamNetworkBuffer(eQSCNetworkBuffer.Default, netBuff[0]);
                else if (e.stringValue == netBuff[1])
                    onStreamNetworkBuffer(eQSCNetworkBuffer.Extra1MS, netBuff[1]);
                else if (e.stringValue == netBuff[2])
                    onStreamNetworkBuffer(eQSCNetworkBuffer.Extra2MS, netBuff[2]);
                else if (e.stringValue == netBuff[3])
                    onStreamNetworkBuffer(eQSCNetworkBuffer.Extra5MS, netBuff[3]);
            }
            else if (e.name == streamStatus)
            {
                if (e.stringValue == "OK")
                    onStreamStatus(eQSCStreamStatus.STREAM_OK);
                else if (e.stringValue == "Initializing")
                    onStreamStatus(eQSCStreamStatus.STREAM_INITIALIZING);
                else if (e.stringValue == "Compromised")
                    onStreamStatus(eQSCStreamStatus.STREAM_COMPROMISED);
                else if (e.stringValue == "Missing" || e.stringValue == "Not Present - (address not specified)")
                    onStreamStatus(eQSCStreamStatus.STREAM_MISSING);
                else if (e.stringValue.Contains("Fault"))
                    onStreamStatus(eQSCStreamStatus.STREAM_FAULT);
            }
            else if (e.name == streamMulticast)
            {
                multicastName = e.stringValue;
                onStreamMulticast(multicastName);
            }
        }

        #endregion Internal Methods

        #region Public Methods

        public void EnableStream()
        {
            var index = controls.FindIndex(n => n.Name == streamEnable);
            if (isEnabled)
                ComponentBuilder(controls[index].Name, 0, eQSCAES67Controls.Enable);
            else
                ComponentBuilder(controls[index].Name, 1, eQSCAES67Controls.Enable);
        }

        public void SetStreamName(string stream)
        {
            var index = controls.FindIndex(n => n.Name == streamName);
            ComponentBuilder(controls[index].Name, stream, eQSCAES67Controls.StreamName);
        }

        public void SetNetworkBuffer(ushort buff)
        {
            var index = controls.FindIndex(n => n.Name == streamNetworkBuffer);
            ComponentBuilder(controls[index].Name, netBuff[buff], eQSCAES67Controls.StreamName);
        }

        public void SetNetInterface(ushort net)
        {
            var index = controls.FindIndex(n => n.Name == streamInterface);
            ComponentBuilder(controls[index].Name, netPort[net], eQSCAES67Controls.StreamName);
        }

        public void SetMulticast(string address)
        {
            var index = controls.FindIndex(n => n.Name == streamMulticast);
            ComponentBuilder(controls[index].Name, address, eQSCAES67Controls.StreamMulticast);
        }


        #endregion Public Methods
    }
}