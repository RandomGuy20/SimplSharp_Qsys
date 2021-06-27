using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace DSP_Suite.Qsys
{
    public class RouterSIMPL
    {
        #region Fields

        private RouterQsys router;

        #endregion Fields

        #region Properties

        #endregion Properties

        #region Delegates and Events

        public delegate void RouterChange(ushort inputRouted, ushort output);
        public RouterChange onRouterChange { get; set; }

        public delegate void RouterMuteChange(ushort state, ushort output);
        public RouterMuteChange onRouterMuteChange { get; set; }

        #endregion

        #region Constructors

        public void Initialize(string name, string coreID, string pollgroup, ushort outputs)
        {
            router = new RouterQsys(ProcessorHolder.holder[coreID], pollgroup, name, outputs);
            router.onRoutingChange += new RouterQsys.RoutedInputsEventHandler(router_onRoutingChange);
        }

        #endregion Constructors

        #region Internal Methods

        void router_onRoutingChange(int[] outputList)
        {

        }

        void router_onRoutingChange(int[] outputList, bool[] outputMuteState)
        {
            for (ushort i = 0; i < outputList.Length; i++)
            {
                onRouterChange((ushort)outputList[i], (ushort)i); ;
            }

            for (ushort i = 0; i < outputMuteState.Length; i++)
            {
                if (outputMuteState[i])
                {
                    onRouterChange((ushort)0, i);
                }
             }
        }

        #endregion

        #region Public Methods

        public void RouteInput(ushort input, ushort output)
        {
            router.RouteInput(input, output);
        }

        #endregion Public Methods
    }
}