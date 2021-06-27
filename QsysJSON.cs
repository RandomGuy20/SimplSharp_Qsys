using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DSP_Suite.Qsys
{
    #region Connecting

    #endregion Connecting

    #region KeepAliveHeartbeat

    public class KeepAliveHeartBeat
    {
        [JsonProperty]
        public string jsonrpc = ProcessorQsys.jsonRPC;

        [JsonProperty]
        public string method = "NoOp";

        [JsonProperty("params")]
        public KeepAliveParams Params { get; set; }
    }

    public class KeepAliveParams
    {

    }

    #endregion KeepAliveHeartbeat

    #region Status Get

    public class StatusGet
    {
        [JsonProperty]
        public string jsonrpc = ProcessorQsys.jsonRPC;

        [JsonProperty]
        public string method = "StatusGet";

        [JsonProperty]
        public int id = 1;

        [JsonProperty]
        public int Params = 0;
    }

    public class NullFeedback
    {
        [JsonProperty]
        public string jsonrpc = ProcessorQsys.jsonRPC;

        [JsonProperty]
        public string result { get; set; }

        [JsonProperty]
        public string id { get; set; }
    }

    #endregion Status Get

    #region ChangeGroupAddControl

    public class ChangeGroupAddControl
    {
        [JsonProperty]
        public string jsonrpc = ProcessorQsys.jsonRPC;

        [JsonProperty]
        public string id { get; set; }

        [JsonProperty]
        public string method = "ChangeGroup.AddControl";

        [JsonProperty("params")]
        public ChangeGroupAddControlParams Params { get; set; }

    }

    public class ChangeGroupAddControlParams
    {
        [JsonProperty]
        public string id { get; set; }

        [JsonProperty]
        public List<string> Controls { get; set; }
    }

    public class Control
    {
        public string Name { get; set; }
    }

    #endregion Change Group Add Control

    #region ChangeGroup Add Component

    public class ChangeGroupAddComponent
    {
        [JsonProperty]
        public string jsonrpc = ProcessorQsys.jsonRPC;

        [JsonProperty]
        public string id { get; set; }

        [JsonProperty]
        public string method = "ChangeGroup.AddComponentControl";

        [JsonProperty("params")]
        public ChangeGroupAddComponentParams Params { get; set; }
    }

    public class ChangeGroupAddComponentParams
    {
        [JsonProperty]
        public string id { get; set; }

        public Component Component { get; set; }
    }

    public class Component
    {
        [JsonProperty]
        public string Name { get; set; }
        public List<Control> Controls { get; set; }
    }

    #endregion ChangeGroup Add Component

    #region Component Set

    public class ComponentSet
    {
        [JsonProperty("jsonrpc")]
        public string Jsonrpc = "2.0";

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("method")]
        public string Method = "Component.Set";

        [JsonProperty("params")]
        public ComponentSetParams Params { get; set; }
    }

    public class ComponentSetParams
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Controls")]
        public IList<ComponentSetControl> Controls { get; set; }
    }

    public class ComponentSetControl
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Position")]
        public double? Position { get; set; }

        [JsonProperty("Value")]//[JsonProperty(Required = Required.Default)]
        public object Value { get; set; }

        [JsonProperty("String")]
        public string String { get; set; }

        [JsonProperty("Choices")]
        public List<string> Choices { get; set; }

        [JsonProperty("Ramp")]
        public long Ramp { get; set; } // = 0;
    }

    #endregion Component Set

    #region Component Changes

    public class ComponentChanges
    {
        //[JsonProperty("Component")]
        public string Component { get; set; }
        //[JsonProperty("Name")]
        public string Name { get; set; }
        //[JsonProperty("String")]
        public string String { get; set; }
        //[JsonProperty("Value")]
        public double Value { get; set; }
        //[JsonProperty("Position")]
        public double Position { get; set; }
        public IList<string> Choices { get; set; }
    }

    #endregion Component Changes

    #region AutoPoll

    public class ChangeGroupAutoPoll
    {
        [JsonProperty]
        public string jsonrpc = "2.0";

        [JsonProperty]
        public string id { get; set; }

        [JsonProperty]
        public string method = "ChangeGroup.AutoPoll";

        [JsonProperty("params")]
        AutoPollParams Params = new AutoPollParams();
    }

    public class AutoPollParams
    {
        [JsonProperty]
        public string id = "1";

        [JsonProperty]
        public double Rate = 0.15;
    }

    #endregion AutoPoll
    #region ControlIntegerSet

    public class ControlIntegerSet
    {
        [JsonProperty("jsonrpc")]
        public string Jsonrpc = ProcessorQsys.jsonRPC;

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("method")]
        public string Method = "Control.Set";

        [JsonProperty("params")]
        public ControlIntegerSetParams Params { get; set; }
    }

    public class ControlIntegerSetParams
    {
        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public double? Value { get; set; }
        [JsonProperty]
        public double? Position { get; set; }
    }

    #endregion

    #region Control String Set

    public class ControlStringSet
    {
        [JsonProperty("jsonrpc")]
        public string Jsonrpc = ProcessorQsys.jsonRPC;

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("method")]
        public string Method = "Control.Set";

        [JsonProperty("params")]
        public ControlStringSetParams Params { get; set; }
    }

    public class ControlStringSetParams
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Value")]
        public string Value { get; set; }
    }

    #endregion ControlString Set

    public class RecentCallBoxChoice
    {
        public string Text { get; set; }
        public string Color { get; set; }
        public string Icon { get; set; }
    }
}