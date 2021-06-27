using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace DSP_Suite.Qsys
{
    public class AES67SIMPL
    {
        #region Fields

        private AES67Qsys aes;

        public delegate void StreamStatusEvent(ushort value);
        public StreamStatusEvent onStreamStatus { get; set; }

        public delegate void StreamEnableEvent(ushort value);
        public StreamEnableEvent onStreamEnable { get; set; }

        public delegate void StreamInterfaceEvent(ushort iFace);
        public StreamInterfaceEvent onStreamInterface { get; set; }

        public delegate void StreamNameEvent(SimplSharpString iFace);
        public StreamNameEvent onStreamName { get; set; }

        public delegate void StreamNetworkBufferEvent(ushort buff);
        public StreamNetworkBufferEvent onStreamNetworkBuffer { get; set; }

        public delegate void StreamMulticastEvent(SimplSharpString add);
        public StreamMulticastEvent onStreamMulticast { set; get; }


        #endregion Fields

        #region Constructor

        public void Initialize(string name, string coreID, string pollgroup, ushort rx)
        {
            aes = new AES67Qsys(ProcessorHolder.holder[coreID], name, Convert.ToBoolean(rx), pollgroup);
            aes.onStreamEnable += new AES67Qsys.StreamEnableEvent(aes_onStreamEnable);
            aes.onStreamInterface += new AES67Qsys.StreamInterfaceEvent(aes_onStreamInterface);
            aes.onStreamMulticast += new AES67Qsys.StreamMulticastEvent(aes_onStreamMulticast);
            aes.onStreamName += new AES67Qsys.StreamNameEvent(aes_onStreamName);
            aes.onStreamNetworkBuffer += new AES67Qsys.StreamNetworkBufferEvent(aes_onStreamNetworkBuffer);
            aes.onStreamStatus += new AES67Qsys.StreamStatusEvent(aes_onStreamStatus);
        }
        #endregion Constructor

        #region Internal Methods

        void aes_onStreamStatus(eQSCStreamStatus status)
        {
            onStreamStatus((ushort)status);
        }

        void aes_onStreamNetworkBuffer(eQSCNetworkBuffer buff, string buffName)
        {
            onStreamNetworkBuffer((ushort)buff);
        }

        void aes_onStreamName(string iFace)
        {
            onStreamName(iFace);
        }

        void aes_onStreamMulticast(string mcastAddress)
        {
            onStreamMulticast(mcastAddress);
        }

        void aes_onStreamInterface(eQSCNetworkInterface port, string portName)
        {
            onStreamInterface((ushort)port);
        }

        void aes_onStreamEnable(bool status)
        {
            onStreamEnable(Convert.ToUInt16(status));
        }

        #endregion Internal Methods

        #region Public Methods

        public void EnableStream()
        {
            aes.EnableStream();
        }

        public void SetStreamName(string stream)
        {
            aes.SetStreamName(stream);
        }

        public void SetNetworkBuffer(ushort buff)
        {
            aes.SetNetworkBuffer(buff);
        }

        public void SetNetInterface(ushort net)
        {
            aes.SetNetInterface(net);
        }

        public void SetMulticast(string address)
        {
            aes.SetMulticast(address);
        }

        #endregion Public Methods
    }
}