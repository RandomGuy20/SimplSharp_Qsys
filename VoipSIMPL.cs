using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace DSP_Suite.Qsys
{
    public class VoipSIMPL
    {
        #region Fields

        private VoipQsys dialer;

        #endregion Fields

        #region Properties

        #endregion Properties

        #region Delegates and Events

        public delegate void CallerIDName(SimplSharpString name);
        public CallerIDName onCallerIdName { get; set; }

        public delegate void CallerIdNumber(SimplSharpString number);
        public CallerIdNumber onCallerIdNumber { get; set; }

        public delegate void DialString(SimplSharpString number);
        public DialString onDialString { get; set; }

        public delegate void DNDStatus(ushort value);
        public DNDStatus onDNDStatus { get; set; }

        public delegate void AutoAnswerRingAmount(ushort value);
        public AutoAnswerRingAmount onAutoAnswerAmount { get; set; }

        public delegate void AutoAnswerStatus(ushort status);
        public AutoAnswerStatus onAutoAnswerStatus { get; set; }

        public delegate void CallState(ushort val);
        public CallState onCallState { get; set; }

        public delegate void CallDuration(SimplSharpString time);
        public CallDuration onCallDuration { get; set; }

        public delegate void CallStatusChange(SimplSharpString status);
        public CallStatusChange onCallStatus { get; set; }

        public delegate void RecentCallList(SimplSharpString recentCall, ushort position);
        public RecentCallList onRecentCallList { get; set; }

        #endregion

        #region Constructor

        public void Initialize(string name, string coreID, string pollgroup)
        {
            dialer = new VoipQsys(ProcessorHolder.holder[coreID], pollgroup, name);
            dialer.onAutoAnswerAmount += new VoipQsys.AutoAnswerRingAmountEventHandler(dialer_onAutoAnswerAmount);
            dialer.onAutoAnswerStatus += new VoipQsys.AutoAnswerStatusEventHandler(dialer_onAutoAnswerStatus);
            dialer.onCallDuration += new VoipQsys.CallDurationEventHandler(dialer_onCallDuration);
            dialer.onCallerIdName += new VoipQsys.CallerIdNameEventHandler(dialer_onCallerIdName);
            dialer.onCallerIdNumber += new VoipQsys.CallerIdNumberEventHandler(dialer_onCallerIdNumber);
            dialer.onCallState += new VoipQsys.CallStateEventHandler(dialer_onCallState);
            dialer.onCallStatus += new VoipQsys.CallStatusChangeEventHandler(dialer_onCallStatus);
            dialer.onDialString += new VoipQsys.DialStringEventHandler(dialer_onDialString);
            dialer.onDNDStatus += new VoipQsys.DNDStatusEventHandler(dialer_onDNDStatus);
            dialer.onRecentCallList += new VoipQsys.RecentCallListEventHandler(dialer_onRecentCallList);
        }

        #endregion Constructor

        #region Internal Methods

        void dialer_onRecentCallList(string recentCall, int position)
        {
            onRecentCallList(recentCall, (ushort)position);
        }

        void dialer_onDNDStatus(bool state)
        {
            onDNDStatus(Convert.ToUInt16(state));
        }

        void dialer_onDialString(string number)
        {
            onDialString(number);
        }

        void dialer_onCallStatus(string status)
        {
            onCallStatus(status);
        }

        void dialer_onCallState(eQSCCallState state)
        {
            onCallState((ushort)state);
        }

        void dialer_onCallerIdNumber(string number)
        {
            onCallerIdNumber(number);
        }

        void dialer_onCallerIdName(string name)
        {
            onCallerIdName(name);
        }

        void dialer_onCallDuration(string time)
        {
            onCallDuration(time);
        }

        void dialer_onAutoAnswerStatus(bool status)
        {
            onAutoAnswerStatus(Convert.ToUInt16(status));
        }

        void dialer_onAutoAnswerAmount(int rings)
        {
            onAutoAnswerAmount((ushort)rings);
        }


        #endregion Internal Methods

        #region Public Methods

        // Connects the call
        public void Connect()
        {
            dialer.Connect();
        }

        // Call last Number
        public void Redial()
        {
            dialer.Redial();
        }

        // Disconnect From Call
        public void Disconnect()
        {
            dialer.Disconnect();
        }

        // Toggle Auto Answer
        public void AutoAnswer(ushort value)
        {
            dialer.AutoAnswer(Convert.ToBoolean(value));
        }

        // Set Auto Answer Rings
        public void SetAutoAnswerRings(ushort value)
        {
            // Sets Amount of rings for Auto Answer
            dialer.SetAutoAnswerRings(value);
        }

        // Toggle DND
        public void DND(ushort value)
        {
            dialer.DND(Convert.ToBoolean(value));
        }

        // Hook Flash
        public void HookFlash()
        {
            // Kill this method from here and Simpl+???? or Just add to POTS
        }

        public void KPButton(string number)
        {
            // Builds the dial string
            dialer.KPButton(number);
        }

        public void KpClear()
        {
            // Clears KP if there is value
            dialer.KpClear();
        }

        public void KPDelete()
        {
            // Deletes Kp if there is value
            dialer.KPDelete();
        }

        public void RecentCall(ushort val)
        {
            dialer.RecentCall(val);
        }

        public void ClearRecentCalls()
        {
            dialer.ClearRecentCalls();
        }

        #endregion Public Methods
    }
}