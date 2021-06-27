using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace DSP_Suite.Qsys
{
    public class SignalPresenceQsys
    {
        #region Fields

        private string name = "";
        private string pollGroup;

        private bool registered;

        private Component component;
        private List<Control> controls;
        private ProcessorQsys core;


        private string presence = "signal.presence";

        #endregion Fields

        #region Properties

        public bool IsRegistered { get { return registered; } }

        public string PollingGroup { get { return pollGroup; } }
        public string ComponentName { get { return name; } }

        #endregion Properties

        #region Delegates and Events

        public delegate void SignalPresenceEventHandler(bool signal);
        public event SignalPresenceEventHandler onSignalPresence;


        #endregion

        #region Constructor

        public SignalPresenceQsys(ProcessorQsys qsysCore, string groupID, string componentName)
        {

            try
            {
                core = qsysCore;

                name = componentName;

                pollGroup = groupID;


                component = new Component();
                component.Name = name;

                controls = new List<Control>();
                controls.Add(new Control());
                controls[0].Name = presence;

                component.Controls = controls;

                if (core.RegisterComponent(component, pollGroup))
                {
                    core.Components[component].QsysEvent += new EventHandler<QsysEventArgs>(SignalPresenceQsys_QsysEvent);
                    registered = true;
                    core.SendDebug("Component" + name + "was succesfully registered");
                }
            }
            catch (Exception e)
            {
                core.SendDebug("Component " + componentName + " Initialize error is: " + e);
            }


        }



        #endregion Constructor

        #region Internal Methods

        void SignalPresenceQsys_QsysEvent(object sender, QsysEventArgs e)
        {
            onSignalPresence(Convert.ToBoolean(e.value));
        }

        #endregion

        #region Public Methods


        #endregion Public Methods
    }
}