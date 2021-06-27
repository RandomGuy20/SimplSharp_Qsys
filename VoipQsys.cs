using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Newtonsoft.Json;

namespace DSP_Suite.Qsys
{
    public class VoipQsys
    {
        #region Fields

        private string name;
        private string pollGroup;

        private bool registered;

        private List<Control> controls = new List<Control>();
        private Component component;
        private ProcessorQsys core;

        private List<string> controlName = new List<string>();
        private List<string> recentCalls = new List<string>();
        private List<string> keypad;

        #region Constants

        private const string callAutoAnswer = "call.autoanswer";
        private const string callAutoAnswerRings = "call.autoanswer.rings";
        private const string callBackspace = "call.backspace";
        private const string callClear = "call.clear";
        private const string callConnect = "call.connect";
        private const string callCidName = "call.cid.name";
        private const string callCidNumber = "call.cid.number";
        private const string callConnectTime = "call.connect.time";
        private const string callDisconnect = "call.disconnect";
        private const string callDetails = "call.connect.time";
        private const string callDialPad = "call.number";
        private const string callDndString = "call.dnd";
        private const string callOffHook = "call.offhook";
        private const string callState = "call.state";
        private const string callCallStatus = "call.status";
        private const string callClearCallHistory = "clear.call.history";
        private const string callRecentCalls = "recent.calls";
        private const string callRinging = "call.ringing";
        #endregion Constants



        private bool isConnected;
        private bool isDisconnected;
        private bool isAutoAnswer;
        private bool isDND;
        private string callerIdName;
        private string callerIdNumber;
        private string callStatus;
        private int autoAnswerRings;

        #endregion Fields

        #region Properties

        public bool IsRegistered { get { return registered; } }
        public bool IsDND { get { return isDND; } }
        public bool IsAutoAnswer { get { return isAutoAnswer; } }
        public bool IsConnected { get { return isConnected; } }
        public bool IsDisconnected { get { return isDisconnected; } }

        public string GetCallerIDName { get { return callerIdName; } }
        public string GetCallerIDNumber { get { return callerIdNumber; } }
        public string CallStatus { get { return callStatus; } }
        public string ComponentName { get { return name; } }

        public int AutoAnswerRings { get { return autoAnswerRings; } }

        #endregion Properties

        #region Delegates 

        public delegate void CallerIdNameEventHandler(string name);
   
        public delegate void CallerIdNumberEventHandler(string number);
        
        public delegate void DialStringEventHandler(string number);
  
        public delegate void DNDStatusEventHandler(bool state);
        
        public delegate void AutoAnswerRingAmountEventHandler(int rings);

        public delegate void AutoAnswerStatusEventHandler(bool status);
        
        public delegate void CallStateEventHandler(eQSCCallState state);
        
        public delegate void CallDurationEventHandler(string time);
        
        public delegate void CallStatusChangeEventHandler(string status);
        
        public delegate void RecentCallListEventHandler(string recentCall, int position);


        #endregion

        #region Events

        public event CallerIdNameEventHandler onCallerIdName;

        public event CallerIdNumberEventHandler onCallerIdNumber;

        public event DialStringEventHandler onDialString;

        public event DNDStatusEventHandler onDNDStatus;

        public event AutoAnswerRingAmountEventHandler onAutoAnswerAmount;

        public event AutoAnswerStatusEventHandler onAutoAnswerStatus;

        public event CallStateEventHandler onCallState;

        public event CallDurationEventHandler onCallDuration;

        public event CallStatusChangeEventHandler onCallStatus;

        public event RecentCallListEventHandler onRecentCallList;

        #endregion

        #region Constructor

        public VoipQsys(ProcessorQsys qsysCore, string groupID, string componentName)
        {
            try
            {
                core = qsysCore;

                pollGroup = groupID;

                name = componentName;

                component = new Component();
                component.Name = name;


                keypad = new List<string>();

                for (int i = 0; i <= 9; i++)
                    keypad.Add(string.Format("call_pinpad_{0}", i));

                keypad.Add("call_pinpad_#");
                keypad.Add("call_pinpad_*");

                for (int i = 0; i < 13; i++)
                      controls.Add(new Control());

                controls[0].Name = callAutoAnswer;
                controls[1].Name = callAutoAnswerRings;
                controls[2].Name = callCidName;
                controls[3].Name = callCidNumber;
                controls[4].Name = callConnectTime;
                controls[5].Name = callDetails;
                controls[6].Name = callDndString;
                controls[7].Name = callDialPad;
                controls[8].Name = callOffHook;
                controls[9].Name = callRinging;
                controls[10].Name = callState;
                controls[11].Name = callCallStatus;
                controls[12].Name = callRecentCalls;

                for (int i = 0; i < 13; i++)
                    controlName.Add(controls[i].Name);

                component.Controls = controls;

                if (core.RegisterComponent(component, pollGroup)) 
                {
                    core.Components[component].QsysEvent += new EventHandler<QsysEventArgs>(VoipQsys_QsysEvent);
                    registered = true;
                    core.SendDebug("Component" + name + "was succesfully registered");
                }
            }
            catch (Exception e)
            {
                core.SendDebug("Component " + componentName + "Initialize error is: " + e);
            }
        }
        
        #endregion Constructor

        #region Internal Methods

        void VoipQsys_QsysEvent(object sender, QsysEventArgs e)
        {
            try
            {
                #region DND
                if (e.name == callDndString)
                {
                    isDND = e.value > 0 ? true : false;
                    onDNDStatus(isDND);
                }
                #endregion DND
                #region Auto Answer
                else if (e.name == callAutoAnswerRings)
                {
                    autoAnswerRings = (int)e.value;
                    onAutoAnswerAmount(autoAnswerRings);
                }
                else if (e.name == callAutoAnswer)
                {
                    isAutoAnswer = e.value > 0 ? true : false;
                    onAutoAnswerStatus(isAutoAnswer);
                }
                #endregion Auto Answer
                #region DialPAd
                else if (e.name == callDialPad)
                {
                    onDialString(e.stringValue);
                }
                #endregion DialpAd
                #region Caller Info
                else if (e.name == callCidName)
                {
                    callerIdName = e.stringValue;
                    onCallerIdName(callerIdName);
                }
                else if (e.name == callCidNumber)
                {
                    callerIdNumber = e.stringValue;
                    onCallerIdNumber(callerIdNumber);
                }
                #endregion Caller Info
                #region Call State
                else if (e.name == callState)
                {
                    callStatus = e.stringValue;
                    if (e.stringValue == "outgoing")
                    {
                        isConnected = false;
                        isDisconnected = true;
                        onCallState(eQSCCallState.Outgoing);
                    }
                    else if (e.stringValue == "inactive")
                    {
                        isConnected = false;
                        isDisconnected = true;
                        onCallState(eQSCCallState.Inactive);
                    }
                    else if (e.stringValue == "incoming")
                    {
                         isConnected = false;
                        isDisconnected = true;
                        onCallState(eQSCCallState.Incoming);
                    }
                    else if (e.stringValue == "active")
                    {
                        isConnected = true;
                        isDisconnected = false;
                        onCallState(eQSCCallState.Active);
                    }
                }
                #endregion CallState
                #region Call ConnectTime
                else if (e.name == callConnectTime)
                {
                    onCallDuration(e.stringValue);
                }
                #endregion Call Connect Time
                else if (e.name == callOffHook)
                {
                    CrestronConsole.PrintLine("VOIP Off Hook State is: " + e.stringValue);
                }
                #region RecentCallList
                else if (e.name == callRecentCalls)
                {
                    try
                    {
                        recentCalls.Clear();
                        List<string> choices = e.choices;
                        foreach (var item in choices)
                        {
                            // if number has been called more than once, Qys does (2) and I dont want that showing up in the recent calls info
                            var newCall = JsonConvert.DeserializeObject<RecentCallBoxChoice>(item);
                            if (newCall.Text.Contains(' '))
                            {
                                var pos = newCall.Text.ToString().IndexOf(' ');
                                var noPar = newCall.Text.ToString().Substring(0, newCall.Text.Length - pos + 2);
                                recentCalls.Add(noPar);
                            }
                            else
                            {
                                recentCalls.Add(newCall.Text);
                            }
                        }
                        // If there arent 20 recent calls, sending a "" to update S+
                        if (recentCalls.Count < 20)
                        {
                            for (int i = recentCalls.Count; i < 20; i++)
                            {
                                recentCalls.Add("");
                            }
                        }
                        for (int i = 0; i < 20; i++)
                        {
                            onRecentCallList(recentCalls[i], (ushort)i);
                        }
                    }
                    catch (Exception err)
                    {
                        core.SendDebug("Voip: " + name + " error in recent calls is: " + err);
                    }
                }
                #endregion RecentCallList
                #region Call Status
                else if (e.name == callCallStatus)
                {
                    onCallStatus(e.stringValue);
                    callStatus = e.stringValue;
                }
                #endregion Call Status
            }
            catch (Exception error)
            {
                core.SendDebug("Error in Voip: " + name + " . Parsing Feedback is; " + error);
            }
        }

         private void ComponentBuilder(bool isDialString, object value, string commandName)
        {
             ComponentSet trigger = new ComponentSet();
            trigger.Params = new ComponentSetParams();
            trigger.Params.Name = name;

            ComponentSetControl setControl = new ComponentSetControl();
            setControl.Name = commandName;

            if (isDialString)
                setControl.Value = Convert.ToString(value);
            else
                setControl.Value = Convert.ToDouble(value);


            trigger.Params.Controls = new List<ComponentSetControl>();
            trigger.Params.Controls.Add(setControl);

            core.QCommand(core.CommandBuider(trigger));
        }

        #endregion

        #region Public Methods

        // Connects the call
        public void Connect()
        {
            ComponentBuilder(true, 1, callConnect);
        }

        // Call last Number
        public void Redial()
        {
            // If there is a call in the recent call log select the most recent one
            if (recentCalls[0].Length > 0)
            {
                RecentCall(1);
                Connect();
            }
        }

        // Disconnect From Call
        public void Disconnect()
        {
            ComponentBuilder(true, 1, callDisconnect);
        }

        // Toggle Auto Answer
        public void AutoAnswer(bool state)
        {
            ComponentBuilder(false, Convert.ToInt16(state), callAutoAnswer);
        }

        // Set Auto Answer Rings
        public void SetAutoAnswerRings(int value)
        {
            ComponentBuilder(false, value, callAutoAnswerRings);
        }

        // Toggle DND
        public void DND(bool state)
        {
            ComponentBuilder(false, Convert.ToInt16(state), callDndString);
        }

        // Hook Flash
        public void HookFlash()
        {
            // Kill this method from here and Simpl+???? or Just add to POTS
        }

        public void KPButton(string number)
        {
            // Builds the dial string
            int position = keypad.FindIndex(s => s.EndsWith(number));
            ComponentBuilder(false, 1, keypad[position]);
        }

        public void KpClear()
        {
            // Clears KP if there is value
            ComponentBuilder(false, 1, callClear);
        }

        public void KPDelete()
        {
            // Deletes Kp if there is value
            ComponentBuilder(false, 1, callBackspace);
        }

        public void RecentCall(int val)
        {
            // Select Call From Recent Call List

            if (recentCalls.Count >= val)
            {
                KpClear();
                var sub = recentCalls[(int)(val - 1)].ToString();
                if (sub.Contains(' '))
                    sub = sub.Remove(sub.IndexOf(' '), sub.Length - sub.IndexOf(' '));

                ComponentBuilder(false, sub, callDialPad);
            }



        }

        public void ClearRecentCalls()
        {
            ComponentBuilder(false, 1, callClearCallHistory);
        }

        #endregion Public Methods
    }
}