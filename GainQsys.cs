using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSP_Suite.Qsys
{
    public class GainQsys
    {
        #region Fields

        private string name;
        private string pollGroup;
        private bool registered;
        private bool currentMute;
        private bool isComponent;
        private int currentLevel;

        private List<Control> controls;
        private Component component;

        private ProcessorQsys core;



        #endregion

        #region Properties

        public bool IsRegistered { get { return registered; } }
        public bool IsMute { get { return currentMute; } }
        public bool IsComponent { get { return isComponent; } }
        public int  Level { get { return currentLevel; } }

        public string ComponentName { get { return name; } }
        public string PollingGroup { get { return pollGroup; } }

        #endregion

        #region Delegates

        public delegate void GainLevelChangeEventHandler(int level,bool mute);

        #endregion

        #region Events

        public event GainLevelChangeEventHandler onGainAudioChange;

        #endregion

        #region Constructors

        public GainQsys(ProcessorQsys qsysCore, string groupID, string componentName)
        {
            try
            {
                core = qsysCore;

                pollGroup = groupID;

                name = componentName;

                component = new Component();
                component.Name = name;

                controls = new List<Control>();
                controls.Add(new Control());
                controls.Add(new Control());
                controls[0].Name = "gain";
                controls[1].Name = "mute";

                component.Controls = controls;

                if (core.RegisterComponent(component, groupID))
                {
                    core.Components[component].QsysEvent += new EventHandler<QsysEventArgs>(GainQsys_QsysEvent);
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

        #endregion

        #region Internal Methods

        void GainQsys_QsysEvent(object sender, QsysEventArgs e)
        {
            if (e.name == "gain")
            {
                currentLevel = (int)Math.Round(core.ScaleFromCore(e.position));
                onGainAudioChange(currentLevel, currentMute);
            }
            else if (e.name == "mute")
            {
                if (e.position == 1.0)
                    currentMute = true;
                else
                    currentMute = false;

                currentMute = (int)e.position == 1.0 ? true : false;
                onGainAudioChange(currentLevel, currentMute);
            }
        }

        private void CommandBuilder(object obj)
        {
            try
            {
                ComponentSet comp = new ComponentSet();
                comp.Params = new ComponentSetParams();
                comp.Params.Name = name;

                ComponentSetControl setComp = new ComponentSetControl();

                if (obj is int)
                {
                    int level = Convert.ToInt32(obj);
                    setComp.Name = controls[0].Name;
                    setComp.Position = core.ScaleToCore(level);
                }
                else if (obj is bool)
                {
                    bool state = Convert.ToBoolean(obj);
                    setComp.Name = controls[1].Name;
                    setComp.Value = state == true ? 1 : 0;
                }

                comp.Params.Controls = new List<ComponentSetControl>();
                comp.Params.Controls.Add(setComp);

                core.QCommand(core.CommandBuider(comp));
            }
            catch (Exception e)
            {
                core.SendDebug("Error in GainQsys Command Builder is: " + e);
            }

        }


        #endregion

        #region Public Methods

        /// <summary>
        /// Volume should come in 0 - 65535, feedback will come in at the same scale
        /// </summary>
        /// <param name="level"></param>
        /// <param name="componentName"></param>
        public void NewVolume(int level)
        {
            try
            {
                CommandBuilder(level);
            }
            catch (Exception e)
            {
                core.SendDebug("Error with: " + name + " setting new volume is: " + e);
            }
        }

        public void SetMute(bool state)
        {
            CommandBuilder(state);
        }

        #endregion

    }
}