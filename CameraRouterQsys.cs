using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace DSP_Suite.Qsys
{
    public class CameraRouterQsys
    {
        #region Fields

        private string name;
        private string pollGroup;
        private bool registered;
        private int maxOutput;
        private List<int> outputList;

        private List<Control> controls;
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

        public delegate void RoutedInputsEventHandler(int[] outputList);
        public event RoutedInputsEventHandler onRoutingChange;

        #endregion Delegates and Events

        #region Constructor

        /// <summary>
        /// 
        /// </summary>
        /// <param name="qsysCore"></param>
        /// <param name="groupID"></param>
        /// <param name="componentName"></param>
        /// <param name="numOutputs">Number of Outputs on the router</param>
        /// <param name="numInputs">Number of Inputs on the Router</param>
        public CameraRouterQsys(ProcessorQsys qsysCore, string groupID, string componentName, int numOutputs)
        {

            try
            {
                core = qsysCore;

                name = componentName;

                pollGroup = groupID;

                controls = new List<Control>();

                component = new Component();
                component.Name = name;

                maxOutput = numOutputs;


                outputList = new List<int>();

                for (int i = 0; i < maxOutput; i++)
                {
                    controls.Add(new Control());
                    controls[i].Name = string.Format("select_{0}", i + 1);
                    outputList.Add(i);
                }

                component.Controls = controls;

                if (core.RegisterComponent(component, groupID))
                {
                    registered = true;
                    core.SendDebug("Component" + name + "was succesfully registered");
                    core.Components[component].QsysEvent += new EventHandler<QsysEventArgs>(CameraRouterQsys_QsysEvent);
                }
            }
            catch (Exception e)
            {
                core.SendDebug("Component " + componentName + "Initialize error is: " + e);
            }

        }

        #endregion Constructor

        #region Internal Methods

        void CameraRouterQsys_QsysEvent(object sender, QsysEventArgs e)
        {
            outputList[Int16.Parse(e.name.Remove(0, e.name.Length - 1)) - 1] = (int)e.value;
            onRoutingChange(outputList.ToArray());
        }

        #endregion Internal Methods

        #region Public Methods

        public void RouteInput(ushort input, ushort output)
        {
            if (output <= maxOutput)
            {
                ComponentSet routerSelect = new ComponentSet();
                routerSelect.Params = new ComponentSetParams();
                routerSelect.Params.Name = name;

                ComponentSetControl routerControl = new ComponentSetControl();
                routerControl.Name = controls[output - 1].Name;
                routerControl.Value = input;

                routerSelect.Params.Controls = new List<ComponentSetControl>();
                routerSelect.Params.Controls.Add(routerControl);

                core.QCommand(core.CommandBuider(routerSelect));
            }

        }

        #endregion Public Methods
    }
}