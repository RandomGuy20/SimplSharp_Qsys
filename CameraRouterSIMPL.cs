using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace DSP_Suite.Qsys
{
    public class CameraRouterSIMPL
    {
        #region Fields

        private CameraRouterQsys camRouter;

        #endregion

        #region Properties

        #endregion Properties

        #region Delegates and Events

        public delegate void RouterChange(ushort inputRouted, ushort output);
        public RouterChange onRouterChange { get; set; }

        #endregion Delegates and Events

        #region Constructor

        public void Initialize(string name, string coreID, string pollgroup, ushort outputs)
        {
            camRouter = new CameraRouterQsys(ProcessorHolder.holder[coreID], pollgroup, name,outputs);
            camRouter.onRoutingChange += new CameraRouterQsys.RoutedInputsEventHandler(camRouter_onRoutingChange);
        }



        #endregion Constructor

        #region Internal Methods

        void camRouter_onRoutingChange(int[] outputList)
        {
            for (ushort i = 0; i < outputList.Length; i++)
            {
                onRouterChange((ushort)outputList[i],(ushort) i); ;
            }
        }

        #endregion Internal Methods

        #region Public Methods
        
        public void RouteInput(ushort input, ushort output)
        {
            camRouter.RouteInput(input, output);
        }

        #endregion Public Methods
    }
}