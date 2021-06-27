using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using IPCommunicationSuite;
using IPCommunicationSuite.TCP;

namespace DSP_Suite.Qsys
{
    public class ProcessorQsys
    {
        #region Fields

        private TCPIPClient core;

        private CrestronQueue<string> commandQ;
        private CrestronQueue<string> responseQ;

        private CTimer commandTimer;
        private CTimer responseTimer;
        private CTimer heartBeatTimer;

        internal Dictionary<Component, QsysEvents> Components = new Dictionary<Component, QsysEvents>();
        internal Dictionary<Control, QsysEvents> Controls = new Dictionary<Control, QsysEvents>();

        private List<string> pollIds = new List<string>();

        private StringBuilder rxData = new StringBuilder();

        private string coreLogin;
        private string coreUser;
        private string coreID;
        private string coreIP;

        public static string jsonRPC = "2.0";
        public string componentChangeGroup = "ChangeGroup.AddComponentControl";

        private bool isBusy;

        public bool isInitialized = false;
        private bool isRedundant;
        private bool isEmulator;
        private bool isActive;
        private bool isRegistered;
        private bool isConnected;


        private string activeString;
        private string designName;
        private string corePlatform;


        #endregion

        #region Properties

        public bool Debug { get; set; }
        public bool IsRedundant { get{return isRedundant;} }
        public bool IsEmulator { get{ return isEmulator;} }
        public bool IsActive { get{ return isActive;} }
        public bool IsRegistered { get{return isRegistered;} }
        public bool IsConnected { get{return isConnected;} }

        public string DesignName { get { return RemoveQuotes(designName); } }
        public string CorePlatform { get{ return RemoveQuotes(corePlatform);} }
        public string CoreIpAddress { get {return coreIP;} }
        public string CoreID { get{return coreID;}}

        #endregion

        #region Delegates

        public delegate void QsysProcessorInfoEventHandler(ProcessorStatusEvents status);
        public delegate void QsysProcessorConnectionEventHandler(eQSCProcessorEventIDs ID, bool state, int value);

        #endregion

        #region Events

        public event QsysProcessorInfoEventHandler QsysProcessorInfoEvent;
        public event QsysProcessorConnectionEventHandler QsysProcessorConnectionEvent;

        #endregion

        #region Constructors

        public ProcessorQsys(string ip, string id)
        {
            try
            {
                ProcessorHolder.holder.Add(id, this);
                coreID = id;
                commandQ = new CrestronQueue<string>();
                responseQ = new CrestronQueue<string>();
                commandTimer = new CTimer(ProcessCommand, null, 0, 15);
                responseTimer = new CTimer(ProcessResponses, null, 0, 15);
                coreIP = ip;

                core = new TCPIPClient(coreIP, 1710, 55000);
                core.onIncomingData += new TCPIPClient.IncomingDataEventHandler(core_onIncomingData);
                core.onStatusChange += new TCPIPClient.StatusChangeEventHandler(core_onStatusChange);
                core.Connect();

            }
            catch (Exception e)
            {
                SendDebug("Error Initializing Qsys Processor is: " + e);
            }
        }

        #endregion

        #region Internal Methods

        internal void SendDebug(string dataToSend)
        {
            if (Debug)
            {
                CrestronConsole.PrintLine("\n" +dataToSend);
                ErrorLog.Error("\n" + dataToSend);
            }
        }

        #region Register Component and Controls and Processor ID
        /// <summary>
        /// If you do more than 9 blocks with the same poll group, qsys will not reliably send information back, 
        /// Make sure you do NOT use the same pollGroup name for more than 9 blocks
        /// </summary>
        /// <param name="component">Component to register</param>
        /// <param name="pollGroup">Name of the Polling group</param>
        /// <returns></returns>
        internal bool RegisterComponent(Component component, string pollGroup)
        {
            try
            {
                lock (Components)
                {
                    if (!Components.ContainsKey(component))
                    {
                        Components.Add(component, new QsysEvents());

                        ChangeGroupAddComponent newComponent;

                        newComponent = new ChangeGroupAddComponent();
                        newComponent.id = pollGroup;
                        newComponent.Params = new ChangeGroupAddComponentParams();
                        newComponent.Params.Component = component;

                        AddPollGroup(pollGroup);
                        commandQ.Enqueue(CommandBuider(newComponent));
                        SetAutoPoll();                 

                        SendDebug("Processor registered component: " + component.Name + "-- polling group: " + pollGroup);
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                SendDebug("Processor error when Registering Component is: " + ex);
                return false;
            }
        }

        /// <summary>
        /// If you do more than 9 blocks with the same poll group, qsys will not reliably send information back, 
        /// Make sure you do NOT use the same pollGroup name for more than 9 blocks
        /// </summary>
        /// <param name="control">Name of the named Control</param>
        /// <param name="pollGroup">Name ofthe pollinggroup</param>
        /// <returns></returns>
        internal bool RegisterControl(Control control, string pollGroup)
        {
            try
            {
                lock (Controls)
                {
                    if (!Controls.ContainsKey(control))
                    {
                        Controls.Add(control, new QsysEvents());
                        if (isInitialized && isConnected)
                        {
                            ChangeGroupAddControl newControl;
                            newControl = new ChangeGroupAddControl();
                            newControl.id = pollGroup;
                            newControl.Params = new ChangeGroupAddControlParams();
                            newControl.Params.Controls = new List<string>();

                            AddPollGroup(pollGroup);

                            commandQ.Enqueue(CommandBuider(newControl));
                        }
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                SendDebug("Processor Error Register Control is: " + ex);
                return false;
            }
        }

        internal void SetAutoPoll()
        {
            for (int i = 0; i < pollIds.Count; i++)
            {
                var groupPolling = new ChangeGroupAutoPoll();
                groupPolling.id = pollIds[i];

                commandQ.Enqueue(CommandBuider(groupPolling));
            }
        }

        #endregion

        #region Helper Methods

        public string CommandBuider(object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        public double ScaleToCore(double level)
        {
            return level / 65535.0;
        }

        public double ScaleFromCore(double level)
        {
            return level * 65535.0;
        }

        public string RemoveQuotes(string data)
        {
            return data.Trim('"');
        }

        internal void QCommand(string data)
        {
            if (data.Length > 0 && data != string.Empty)
                commandQ.Enqueue(data);
        }

        internal void AddPollGroup(string group)
        {
            if (!pollIds.Contains(group))
            {
                pollIds.Add(group);
            }
        }

        #endregion

        #region Timer Callbacks

        void SendHeartBeat(object obj)
        {
            commandQ.Enqueue(CommandBuider(new KeepAliveHeartBeat()));
        }

        void ProcessCommand(object obj)
        {
            try
            {
                if (!commandQ.IsEmpty)
                {
                    var data = commandQ.TryToDequeue();
                    if (Debug)
                    {
                        if (!data.Contains("NoOp") && !data.Contains("StatusGet"))
                            SendDebug("Command Being Sent to the Core is: " + data);
                    }

                    if (data.Length > 0 && data != null)
                        core.SendData(data + "\x00");

                }
                else
                {
                    commandQ.Clear();
                }
            }
            catch (Exception e)
            {
                SendDebug("Processor error Processing commands is: " + e);
            }
        }

        void ProcessResponses(object obj)
        {
            try
            {
                var data = responseQ.TryToDequeue();

                rxData.Append(data);
                if (!isBusy && rxData.Length > 0 && rxData.ToString().Contains("\x00"))
                {
                    isBusy = true;
                    while (rxData.ToString().Contains("\x00"))
                    {
                        var endOfLine = rxData.ToString().IndexOf("\x00");
                        var temp = rxData.ToString().Substring(0, endOfLine);
                        var collector = rxData.Remove(0, endOfLine + 1);
                        if (Debug)
                            if (temp.Contains("Changes") && temp.Contains("Component") || temp.Contains("VOIP") || temp.Contains("gain") || temp.Contains("contacts") || temp.Contains("EngineStatus"))
                                SendDebug("Processor Response: " + temp);

                        ParseResponse(temp);
                    }
                    isBusy = false;
                }
            }
            catch (Exception e)
            {
                SendDebug("Processor Error ProcessingResponse is: " + e.Message);
                isBusy = false;
            }
        }

        void SendStatusGet(object obj)
        {
            if (isConnected)
                commandQ.Enqueue(CommandBuider(new StatusGet()));
        }

        #endregion

        #region TCP Events

        void core_onStatusChange(Crestron.SimplSharp.CrestronSockets.SocketStatus status)
        {
            if (status == Crestron.SimplSharp.CrestronSockets.SocketStatus.SOCKET_STATUS_CONNECTED)
            {
                isConnected = true;
                isInitialized = true;

                SendDebug("TCp/IP Connecttio to Core has been established");

                CrestronEnvironment.Sleep(3000);

                ChangeGroupAddControl newControl;

                foreach (var item in Controls)
                {
                    newControl = new ChangeGroupAddControl();
                    newControl.Params = new ChangeGroupAddControlParams();
                    newControl.Params.Controls = new List<string>();
                    newControl.Params.Controls.Add(item.Key.Name);
                    commandQ.Enqueue(CommandBuider(newControl));
                }

                if (heartBeatTimer != null)
                {
                    heartBeatTimer.Stop();
                    heartBeatTimer.Dispose();
                }

                heartBeatTimer = new CTimer(SendHeartBeat, null, 5000, 5000);

                QsysProcessorConnectionEvent(eQSCProcessorEventIDs.Connected, true, 1);
                QsysProcessorConnectionEvent(eQSCProcessorEventIDs.Registered, true, 1);

            }
            else
            {
                QsysProcessorConnectionEvent(eQSCProcessorEventIDs.Connected, false, 0);
            }
            SendDebug("Core connection status is: " + status);
        }

        void core_onIncomingData(string data)
        {
            responseQ.Enqueue(data);
        }

        #endregion

        private void ParseResponse(string data)
        {
            try
            {
                if (data.Length > 0)
                {
                    var response = JObject.Parse(data);
                    var settings = new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Ignore };

                    if (data.Contains("Changes"))
                    {
                        IList<JToken> changes = response["params"]["Changes"].Children().ToList();

                        foreach (var change in changes)
                        {
                            SendDebug("Processor Response Data is: " + change.ToString());
                            ComponentChanges compChange = JsonConvert.DeserializeObject<ComponentChanges>(change.ToString(), settings);

                            #region Component Change
                            if (compChange.Component != null)
                            {
                                foreach (var item in Components)
                                {
                                    List<string> choices;
                                    choices = compChange.Choices != null ? compChange.Choices.ToList() : new List<string>();

                                    if (item.Key.Name == compChange.Component)
                                    {
                                        string temp = string.Format("Parse Response Component Change is: {0}, intValue is:{1},Position is:{2}, stringvalue is: {3}.", compChange.Name, compChange.Value, compChange.Position, compChange.String);
                                        SendDebug(temp);
                                        item.Value.Send(new QsysEventArgs(compChange.Name, Convert.ToDouble(compChange.Value), Convert.ToDouble(compChange.Position), Convert.ToString(compChange.String), choices));
                                    }
                                }
                            }
                            #endregion
                            #region Controls Change
                            else if (compChange.Name != null)
                            {
                                foreach (var item in Controls)
                                {
                                    List<string> choices;
                                    choices = compChange.Choices != null ? compChange.Choices.ToList() : new List<string>();

                                    if (item.Key.Name.Contains(compChange.Name))
                                    {
                                        string temp = string.Format("Parse Response Control Change is: {0}, intValue is:{1},Position is:{2}, stringvalue is: {3}.", compChange.Name, compChange.Value, compChange.Position, compChange.String);
                                        SendDebug(temp);
                                        item.Value.Send(new QsysEventArgs(compChange.Name, Convert.ToDouble(compChange.Value), Convert.ToDouble(compChange.Position), Convert.ToString(compChange.String), choices));
                                    }
                                }
                            }
                            #endregion


                        }
                     }
                    #region Core Status
                    else if (data.Contains("EngineStatus"))
                    {
                        JToken status = response["params"];
                        corePlatform = status["Platform"].ToString();
                        activeString = status["State"].ToString();

                        isActive = activeString != string.Empty && activeString.Contains("Active") ? true : false;
                        designName = status["DesignName"].ToString();
                        isRedundant = Convert.ToBoolean(status["IsRedundant"].ToString());
                        isEmulator = Convert.ToBoolean(status["IsEmulator"].ToString());

                        SendDebug("Parse Response processor State Reply is: " + data.ToString());

                        string temp = string.Format("Simpl Events ID's: Initialized:{0},Connected:{1},Emulator:{2},Active:{3},DesignNAme:{4},Platform:{5}", isInitialized, isConnected, isEmulator, isActive, designName, corePlatform);
                        SendDebug(temp);
                        QsysProcessorInfoEvent(new ProcessorStatusEvents(DesignName, CorePlatform, IsEmulator, IsRedundant, IsActive));
                        
                    }
                    #endregion
                }
            }
            catch (Exception e)
            {
                SendDebug("Error Processor parseResponse is: " + e);
            }
        }

        #endregion

        #region Public Methods

        #endregion
    }
}