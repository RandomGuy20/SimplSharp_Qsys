using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace DSP_Suite.Qsys
{
    public class RouterQsys
    {
        #region Fields

        private string name;
        private string pollGroup;

        private bool registered;

        private int maxOutput;

        private List<int> outputList;
        private List<bool> outputMuteList;

        private List<Control> controls;
        private List<Control> selects;
        private List<Control> mutes;
        private List<Control> commands;
        private Component component;
        private ProcessorQsys core;

        #endregion

        #region Properties

        public bool IsRegistered { get { return registered; } }

        public string PollingGroup { get { return pollGroup; } }
        public string ComponentName { get { return name; } }

        public int OutputAmount { get { return maxOutput; } }

        #endregion Properties

        #region Delegates and Events

        public delegate void RoutedInputsEventHandler(int[] outputList, bool[] outputMuteState);
        public event RoutedInputsEventHandler onRoutingChange;


        #endregion

        #region Constructor

        /// <summary>
        /// 
        /// </summary>
        /// <param name="qsysCore"></param>
        /// <param name="groupID"></param>
        /// <param name="componentName"></param>
        /// <param name="numOutputs">Number of Outputs on the router</param>
        /// <param name="numInputs">Number of Inputs on the Router</param>
        public RouterQsys(ProcessorQsys qsysCore, string groupID, string componentName, int numOutputs)
        {

            try
            {
                core = qsysCore;

                name = componentName;

                pollGroup = groupID;

                controls = new List<Control>();
                selects = new List<Control>();
                mutes = new List<Control>();


                component = new Component();
                component.Name = name;

                maxOutput = numOutputs;


                outputList = new List<int>();
                outputMuteList = new List<bool>();

                
                commands = new List<Control>(2);
                commands.Add(new Control());
                commands.Add(new Control());
                commands[0].Name = "select_{0}";
                commands[1].Name = "mute_{0}";

                for (int i = 0; i < maxOutput; i++)
                {
                    outputList.Add(i);
                    outputMuteList.Add(true);
                    selects.Add(new Control());
                    mutes.Add(new Control()); 
                    selects[i].Name = string.Format("select_{0}", i + 1);
                    mutes[i].Name = string.Format("mute_{0}", i+ 1);
                }

                controls = controls.Concat(selects).Concat(mutes).ToList();

                 component.Controls = controls;

                if (core.RegisterComponent(component, pollGroup))
                {
                    core.SendDebug("Component" + name + "was succesfully registered");
                    core.Components[component].QsysEvent += new EventHandler<QsysEventArgs>(RouterQsys_QsysEvent);
                    registered = true;
                }
            }
            catch (Exception e)
            {
                core.SendDebug("Component " + componentName + " Initialize error is: " + e);
            }

        }



        #endregion Constructor

        #region Internal Methods

        void RouterQsys_QsysEvent(object sender, QsysEventArgs e)
        {

            if (e.stringValue.Contains("mute"))
                outputMuteList[Int16.Parse(e.name.Remove(0, e.name.Length - 1)) - 1] = Convert.ToBoolean(e.value);
            else
                outputList[Int16.Parse(e.name.Remove(0, e.name.Length - 1)) - 1] = (int)e.value;

            onRoutingChange(outputList.ToArray(), outputMuteList.ToArray());
        }

        private void CommandBuilder(ushort value, string commandName)
        {
            ComponentSet routerSelect = new ComponentSet();
            routerSelect.Params = new ComponentSetParams();
            routerSelect.Params.Name = name;

            ComponentSetControl routerControl = new ComponentSetControl();


            routerControl.Name = commandName;
            routerControl.Value = value;

            routerSelect.Params.Controls = new List<ComponentSetControl>();
            routerSelect.Params.Controls.Add(routerControl);

            core.QCommand(core.CommandBuider(routerSelect));
        }


        #endregion

        #region Public Methods

        public void RouteInput(ushort input, ushort output)
        {
            if (output <= maxOutput)
            {
                if (input > 0)
                {
                    CommandBuilder(0, string.Format(commands[1].Name, output));
                    CommandBuilder(input, string.Format(commands[0].Name, output));
                }
                else
                {
                    CommandBuilder(1, string.Format(commands[1].Name, output));
                }
            }


        }

         #endregion Public Methods
    }
}