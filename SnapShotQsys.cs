using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace DSP_Suite.Qsys
{
    public class SnapShotQsys
    {
        #region Fields

        private string name;
        private string pollGroup;

        private bool registered;

        private ProcessorQsys core;

        private Component component;

        private List<string> loadCommands;
        private List<string> saveCommads;
        private List<string> commands;

        private List<SnapFeedback> feedBacks = new List<SnapFeedback>();
        private List<Control> controls;

        #region Constants

        private const string saveString = "save.{0}";
        private const string loadString = "load.{0}";
        private const string snapMatch = "match.{0}";
        private const string snapLast = "last.{0}";
        private const string snapRamp = "ramp.time";

        #endregion Constants

        #endregion Fields

        #region Properties

        public bool IsRegistered { get { return registered; } }


        public string ComponentName { get { return name; } }
        public string PollingGroup { get { return pollGroup; } }

        #endregion Properties

        #region Delegates and Events

        public delegate void ActiveSnapShot(int snap);
        public event ActiveSnapShot onActiveSnapShot;

        #endregion

        #region Constructor

        public SnapShotQsys(ProcessorQsys qsysCore, string groupID, string componentName)
        {
            try
            {
                core = qsysCore;

                pollGroup = groupID;

                name = componentName;

                component = new Component();
                component.Name = name;

                controls = new List<Control>();

                loadCommands = new List<string>();
                saveCommads = new List<string>();
                commands = new List<string>();


                for (int i = 0; i < 8; i++)
                {
                    controls.Add(new Control());
                    controls[i].Name = string.Format(snapMatch, i + 1);
                }

                for (int i = 0; i < 8; i++)
                {

                    loadCommands.Add(string.Format(loadString, i + 1));
                    saveCommads.Add(string.Format(saveString, i + 1));
                    feedBacks.Add(new SnapFeedback());
                }

                commands = commands.Concat(loadCommands).Concat(saveCommads).ToList();

                component.Controls = controls;

                if (core.RegisterComponent(component, pollGroup))
                {
                    core.Components[component].QsysEvent += new EventHandler<QsysEventArgs>(SnapShotQsys_QsysEvent);
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

        void SnapShotQsys_QsysEvent(object sender, QsysEventArgs e)
        {
            var val = Convert.ToInt16(e.name.Substring(e.name.Length - 1, 1));
            feedBacks[val - 1].active = e.stringValue;

            var isActive = feedBacks.Any(s => s.active == "true");
            int index = feedBacks.FindIndex(s => s.active == "true");
            if (isActive)
                onActiveSnapShot(Convert.ToUInt16(index + 1));
            else if (!isActive)
                onActiveSnapShot(0);
        }

        // Build the component set command I did some of these using 
        public void ComponentBuilder(string command)
        {
            ComponentSet trigger = new ComponentSet();
            trigger.Params = new ComponentSetParams();
            trigger.Params.Name = name;

            ComponentSetControl routerControl = new ComponentSetControl();
            routerControl.Name = command;

            routerControl.Value = 1;

            trigger.Params.Controls = new List<ComponentSetControl>();
            trigger.Params.Controls.Add(routerControl);

            core.QCommand(core.CommandBuider(trigger));
        }


        #endregion

        #region Public Methods

        public void SetSnap(int snapNum)
        {
            ComponentBuilder(commands[commands.IndexOf(string.Format(saveString, snapNum))]);
        }

        public void RecallSnap(int snapNum)
        {
            ComponentBuilder(commands[commands.IndexOf(string.Format(loadString, snapNum))]);
        }

        #endregion Public Methods
    }
}