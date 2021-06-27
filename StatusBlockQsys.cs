using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace DSP_Suite.Qsys
{
    public class StatusBlockQsys
    {
        #region Fields

        private string name;
        private string pollGroup;
        private bool registered;



        private static List<Control> controls = new List<Control>();
        private static Component component;

        private ProcessorQsys core;

        #endregion Fields

        #region Properties

        public bool IsRegistered { get { return registered; } }

        public string ComponentName { get { return name; } }
        public string PollingGroup { get { return pollGroup; } }

        #endregion Properties

        #region Delegates and Events


        public delegate void CoreStatus(eQSCCoreState status);
        public event CoreStatus onCoreStatus;

        #endregion

        #region Constructor

        public StatusBlockQsys(ProcessorQsys qsysCore, string groupID, string componentName)
        {

            try
            {
                core = qsysCore;

                pollGroup = groupID;

                name = componentName;

                component = new Component();
                component.Name = name;

                controls.Add(new Control());
                controls[0].Name = "status";

                component.Controls = controls;

                if (core.RegisterComponent(component, pollGroup))
                {
                    core.Components[component].QsysEvent += new EventHandler<QsysEventArgs>(StatusBlockQsys_QsysEvent);
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

        void StatusBlockQsys_QsysEvent(object sender, QsysEventArgs e)
        {
            if (e.name == "status")
            {
                if (e.stringValue == "OK")
                    onCoreStatus(eQSCCoreState.CoreOK);
                else if (e.stringValue == "Initializing")
                    onCoreStatus(eQSCCoreState.CoreInitializing);
                else if (e.stringValue == "Compromised")
                    onCoreStatus(eQSCCoreState.CoreCompromised);
                else if (e.stringValue == "Missing")
                    onCoreStatus(eQSCCoreState.CoreMissing);
                else if (e.stringValue == "Fault")
                    onCoreStatus(eQSCCoreState.CoreFault);
                else if (e.stringValue == "Unknown")
                    onCoreStatus(eQSCCoreState.CoreUnknown);
                else if (e.stringValue == "NotPresent")
                    onCoreStatus(eQSCCoreState.CoreNotPresent);

            }
        }

        private void BuildComponent(Control control, int iValue)
        {
            ComponentSet trigger = new ComponentSet();
            trigger.Params = new ComponentSetParams();
            trigger.Params.Name = name;

            ComponentSetControl value = new ComponentSetControl();
            value.Name = controls[0].Name;

            value.Value = iValue;

            trigger.Params.Controls = new List<ComponentSetControl>();
            trigger.Params.Controls.Add(value);

            core.QCommand(core.CommandBuider(trigger));
        }

        #endregion

        #region Public Methods


        #endregion Public Methods
    }
}