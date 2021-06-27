using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace DSP_Suite.Qsys
{
    public class SystemMuteQsys
    {
        #region Fields

        private string name;
        private string pollGroup;

        private bool registered;

        private List<Control> controls;
        private Component component;
        private ProcessorQsys core;

        #endregion Fields

        #region Properties

        public bool IsRegistered { get { return registered; } }
  
        public string ComponentName { get { return name; } }
        public string PollingGroup { get { return pollGroup; } }

        #endregion Properties

        #region Delegates and Events

        public delegate void SendMute(bool mute);
        public event SendMute onSystemMute;

        public delegate void SystemGainChange(int gain);
        public event SystemGainChange onGainChange;


        #endregion

        #region Constructor

        public SystemMuteQsys(ProcessorQsys qsysCore, string groupID, string componentName)
        {

            try
            {
                name = componentName;
                core = qsysCore;

                pollGroup = groupID;

                name = componentName;

                component = new Component();
                component.Name = name;

                controls = new List<Control>();
                controls.Add(new Control());
                controls.Add(new Control());

                controls[0].Name = "mute";
                controls[1].Name = "gain";

                component.Controls = controls;

                if (core.RegisterComponent(component,pollGroup))
                {
                    core.Components[component].QsysEvent += new EventHandler<QsysEventArgs>(SystemMuteQsys_QsysEvent);
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

        void SystemMuteQsys_QsysEvent(object sender, QsysEventArgs e)
        {
            if (e.name == controls[0].Name)
            {
                onSystemMute(Convert.ToBoolean(e.value));
            }
            else if (e.name == controls[1].Name)
            {
                onGainChange((int)Math.Round(core.ScaleFromCore(e.position)));
            }
        }



        #endregion

        #region Public Methods

        public void MuteToggle(bool state)
        {

            var mute = Convert.ToInt16(state);

            ComponentSet muteState = new ComponentSet();
            muteState.Params = new ComponentSetParams();
            muteState.Params.Name = name;

            ComponentSetControl value = new ComponentSetControl();
            value.Name = controls[0].Name;

            value.Value = mute;

            muteState.Params.Controls = new List<ComponentSetControl>();
            muteState.Params.Controls.Add(value);

            core.QCommand(core.CommandBuider(muteState));
        }

        #endregion Public Methods
    }
}