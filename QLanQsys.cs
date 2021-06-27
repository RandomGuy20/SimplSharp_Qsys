using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace DSP_Suite.Qsys
{
    public class QLanQsys
    {
        #region Fields

        private string name = "";
        private string pollGroup;

        private bool registered;
        private bool isComponent;
        private bool isRx;
        private bool isEnabled;
        private bool streamIsActive;

        private List<Control> controls = new List<Control>();
        private Component component;

        private ProcessorQsys core;

        #region Constants
        private string streamEnable = "enable";
        private string streamStatus = "status";
        private string streamName = "stream.name";
        private string streamPrimary = "primary.ok";
        #endregion Constants

        #endregion Fields

        #region Properties

        public string ComponentName { get { return name; } }
        public string PollingGroup { get { return pollGroup; } }

        public bool IsRegistered { get { return registered; } }
        public bool IsComponent { get { return isComponent; } }
        public bool IsRX { get { return isRx; } }
        public bool IsEnabled { get{return isEnabled;} }
        public bool StreamActive { get { return streamIsActive; } }

        #endregion Properties

        #region Delegates 

        public delegate void QlanChangeEventHandler(eQlanArgs args, bool state);

        public delegate void QlanStreamNameChangeEventHandler(string newName);

        #endregion

        #region Events

        public event QlanChangeEventHandler onQlanBlockChange;

        public event QlanStreamNameChangeEventHandler onStreamNameChange;


        #endregion Events

        #region Constructors

        public QLanQsys(ProcessorQsys qsysCore, string groupID, string componentName, bool isRX)
        {
            try
            {
                isRx = isRX;
                name = componentName;
                pollGroup = groupID;

                core = qsysCore;

                component = new Component();
                component.Name = name;

                for (int i = 0; i < 4; i++)
                {
                    controls.Add(new Control());
                }

                controls[0].Name = streamEnable;
                controls[1].Name = streamName;
                controls[2].Name = streamPrimary;
                controls[3].Name = streamStatus;


                component.Controls = controls;

                if (core.RegisterComponent(component,pollGroup))
                {
                    core.Components[component].QsysEvent += new EventHandler<QsysEventArgs>(QLanQsys_QsysEvent);
                    registered = true;
                    isComponent = true;
                    core.SendDebug("Component" + name + "was succesfully registered");
                }
            }
            catch (Exception e)
            {
                core.SendDebug("Component " + componentName + "Initialize error is: " + e);
            }
        }



        #endregion Constructors

        #region Internal Methods

        void QLanQsys_QsysEvent(object sender, QsysEventArgs e)
        {
            if (e.name == streamEnable)
            {
                isEnabled = e.value > 0 ? true : false;

                onQlanBlockChange(eQlanArgs.Enabled, isEnabled);
            }
            else if (e.name == streamName)
            {
                onStreamNameChange(e.stringValue);
            }
            else if (e.name == streamPrimary)
            {
                streamIsActive = e.value > 0 ? true : false;
                onQlanBlockChange(eQlanArgs.StreamActive, streamIsActive);
            }
            else if (e.name == streamStatus)
            {
                if (e.stringValue == "OK")
                    onQlanBlockChange(eQlanArgs.BlockActive, true);
                else
                    onQlanBlockChange(eQlanArgs.BlockActive, false);

            }
        }

        private void ComponentBuilder(object value)
        {
            ComponentSet trigger = new ComponentSet();
            trigger.Params = new ComponentSetParams();
            trigger.Params.Name = name;

            ComponentSetControl setControl = new ComponentSetControl();
            

            if (value is int)
            {
                setControl.Value = (int)value;
                setControl.Name = controls[0].Name;
            }
            else if (value is string)
            {
                setControl.Value = (string)value;
                setControl.Name = controls[1].Name;
            }

            trigger.Params.Controls = new List<ComponentSetControl>();
            trigger.Params.Controls.Add(setControl);

            core.QCommand(core.CommandBuider(trigger));
        }

        #endregion

        #region Public Methods

        public void EnableBlock()
        {
            if (isEnabled)
                ComponentBuilder(0);
            else
                ComponentBuilder(1);
        }

        public void SetStreamName(string stream)
        {
            ComponentBuilder(stream);
        }

        #endregion Public Methods
    }
}