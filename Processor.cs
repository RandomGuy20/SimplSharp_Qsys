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
    public class Processor
    {/*
        #region Fields

        private static TCPIPClient core;

        private static CrestronQueue<string> commandQ;
        private static CrestronQueue<string> responseQ;

        private static CTimer commandTimer;
        private static CTimer responseTimer;
        private static CTimer heartBeatTimer;

        internal static Dictionary<string, ProcessorEvents> Processors = new Dictionary<string, ProcessorEvents>();
        internal static Dictionary<Component, QsysEvents> Components = new Dictionary<Component, QsysEvents>();
        internal static Dictionary<Control, QsysEvents> Controls = new Dictionary<Control, QsysEvents>();

        private static List<string> pollIds = new List<string>();


        private static StringBuilder rxData = new StringBuilder();

        public static bool Debug = false;

        public static string coreLogin;
        public static string coreUser;
        public static string coreID;

        public static string changeGroup = "Crestron";
        public static string jsonRPC = "2.0";
        public static string componentChangeGroup = "ChangeGroup.AddComponentControl";

        private static bool isBusy;


        #endregion

        #region Properties

        internal static bool isInitialized;
        internal static bool isRedundant;
        internal static bool isEmulator;
        internal static bool isActive;
        internal static bool isRegistered;
        internal static bool isConnected;


        internal static string activeString;
        internal static string designName;
        internal static string corePlatform;


        public bool IsInitialized { get { return isInitialized; } }
        #endregion

        #region Delegates

        #endregion

        #region Events

        #endregion

        #region Constructors

        public Processor(string ip)
        {
            try
            {
                if (!isInitialized)
                {
                    SendDebug("Qsys Core is Initializing");
                    commandQ = new CrestronQueue<string>();
                    responseQ = new CrestronQueue<string>();
                    commandTimer = new CTimer(ProcessCommand, null, 0, 15);
                    responseTimer = new CTimer(ProcessResponses, null, 0, 15);

                    core = new TCPIPClient(ip, 1710, 2500);
                    core.onIncomingData += new TCPIPClient.IncomingDataEventHandler(core_onIncomingData);
                    core.onStatusChange += new TCPIPClient.StatusChangeEventHandler(core_onStatusChange);
                }
            }
            catch (Exception e)
            {
                SendDebug("Error Initializing Qsys Processor is: " + e);
            }
        }

        #endregion

        #region Internal Methods

        internal static void SendDebug(string dataToSend)
        {
            if (Debug)
            {
                CrestronConsole.PrintLine(dataToSend);
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
        internal static bool RegisterComponent(Component component, string pollGroup)
        {
            try
            {
                lock (Components)
                {
                    if (!Components.ContainsKey(component))
                    {
                        Components.Add(component, new QsysEvents());

                        if (isInitialized && isConnected)
                        {
                            ChangeGroupAddComponent newComponent;

                            newComponent = new ChangeGroupAddComponent();
                            newComponent.id = pollGroup;
                            newComponent.Params = new ChangeGroupAddComponentParams();
                            newComponent.Params.Component = component;

                            AddPollGroup(pollGroup);

                            commandQ.Enqueue(CommandBuider(newComponent));

                            SendDebug("Processor registered component: " + component.Name + "-- polling group: " + pollGroup);
                        }
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
        internal static bool RegisterControl(Control control, string pollGroup)
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

        internal static bool RegisterProcessor(string id)
        {
            isRegistered = false;
            try
            {
                lock (Processors)
                {
                    if (!Processors.ContainsKey(id))
                    {
                        coreID = id;
                        Processors.Add(id, new ProcessorEvents());
                        SendDebug("Successfully Registered processor ID: " + id);
                        isRegistered = true;
                    }
                    return true;
                }
            }
            catch (Exception e)
            {
                SendDebug("Error registering Processor is: " + e);
                foreach (var item in Processors)
                {
                    item.Value.Send(coreID, new ProcessorEventsArgs(eQSCProcessorEventIDs.Registered, false, 0, ""));
                    item.Value.Send(coreID, new ProcessorEventsArgs(eQSCProcessorEventIDs.Connected, false, 0, ""));
                }
                return false;
            }
        }
        #endregion

        #region Helper Methods

        internal static string CommandBuider(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        internal static double ScaleToCore(double level)
        {
            return level / 65535.0;
        }

        internal static double ScaleFromCore(double level)
        {
            return level * 65535.0;
        }

        static internal string RemoveQuotes(string data)
        {
            return data.Trim('"');
        }

        internal static void QCommand(string data)
        {
            if (data.Length > 0 && data != string.Empty)
                commandQ.Enqueue(data);
        }

        internal static void AddPollGroup(string group)
        {
            if (!pollIds.Contains(group))
            {
                pollIds.Add(group);
            }
        }

        #endregion

        #region Timer Callbacks

        static void SendHeartBeat(object obj)
        {
            commandQ.Enqueue(CommandBuider(new KeepAliveHeartBeat()));
        }

        static void ProcessCommand(object obj)
        {
            try
            {
                if (!commandQ.IsEmpty)
                {
                    var data = commandQ.TryToDequeue();
                    if (Debug)
                    {
                        if (!data.Contains("NoOp") && !data.Contains("StatusGet"))
                            CrestronConsole.PrintLine("Command Being Sent to the Core is: " + data);
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

        static void ProcessResponses(object obj)
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
                        {
                            if (temp.Contains("Changes") || temp.Contains("VOIP") || temp.Contains("gain") || temp.Contains("contacts") || temp.Contains("EngineStatus"))
                            {
                                SendDebug("Processor Response: " + temp);
                            }
                        }

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

        static void SendStatusGet(object obj)
        {
            if (isConnected)
                commandQ.Enqueue(CommandBuider(new StatusGet()));
        }

        #endregion

        #region TCP Events

        static void core_onStatusChange(Crestron.SimplSharp.CrestronSockets.SocketStatus status)
        {
            if (status == Crestron.SimplSharp.CrestronSockets.SocketStatus.SOCKET_STATUS_CONNECTED)
            {
                isConnected = true;
                isInitialized = true;

                SendDebug("TCp/IP Connecttio to Core has been established");

                CrestronEnvironment.Sleep(3000);

                ChangeGroupAddComponent newComponent;

                foreach (var item in Components)
                {
                    newComponent = new ChangeGroupAddComponent();
                    newComponent.id = changeGroup;
                    newComponent.Params = new ChangeGroupAddComponentParams();
                    newComponent.Params.Component = item.Key;
                    commandQ.Enqueue(CommandBuider(newComponent));
                }

                ChangeGroupAddControl newControl;

                foreach (var item in Controls)
                {
                    newControl = new ChangeGroupAddControl();
                    newControl.Params = new ChangeGroupAddControlParams();
                    newControl.Params.Controls = new List<string>();
                    newControl.Params.Controls.Add(item.Key.Name);
                    commandQ.Enqueue(CommandBuider(newControl));
                }

                for (int i = 0; i < pollIds.Count; i++)
                {
                    var groupPolling = new ChangeGroupAutoPoll();
                    groupPolling.id = pollIds[i];

                    commandQ.Enqueue(CommandBuider(groupPolling));
                }

                if (heartBeatTimer != null)
                {
                    heartBeatTimer.Stop();
                    heartBeatTimer.Dispose();
                }

                heartBeatTimer = new CTimer(SendHeartBeat, null, 0, 5000);

                foreach (var item in Processors)
                {

                }


            }
        }

        static void core_onIncomingData(string data)
        {
            responseQ.Enqueue(data);
        }

        #endregion

        private static void ParseResponse(string data)
        {
            try
            {
                if (data.Length > 0)
                {
                    var response = JObject.Parse(data);
                    var settings = new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Ignore };
                    string getID = response["result"]["Id"].ToString();
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

                                    if (item.Key.Name == compChange.Component)
                                    {
                                        string temp = string.Format("Parse Response Component Change is: {0}, intValue is:{1},Position is:{2}, stringvalue is: {3}.", compChange.Name, compChange.Value, compChange.Position, compChange.String);
                                        SendDebug(temp);
                                        item.Value.Send(new QsysEventArgs(compChange.Name, Convert.ToDouble(compChange.Value), Convert.ToDouble(compChange.Position), Convert.ToString(compChange.String), choices));
                                    }
                                }
                            }
                            #endregion
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

                                foreach (var item in Processors)
                                {
                                    string temp = string.Format("Simpl Events ID's: Initialized:{0},Connected:{1},Emulator:{2},Active:{3},DesignNAme:{4},Platform:{5}", isInitialized, isConnected, isEmulator, isActive, designName, corePlatform);
                                    SendDebug(temp);
                                    item.Value.Send(coreID, new ProcessorEventsArgs(eQSCProcessorEventIDs.CoreStatus, true, 1, ""));
                                }
                            }
                            #endregion

                        }



                    }
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
    }*/
    }
}