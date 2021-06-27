using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace DSP_Suite.Qsys
{
    public class CameraControlSIMPL
    {
        #region Fields

        private CameraControlQsys camControl;

        #endregion Fields

        #region Properties

        #endregion Properties

        #region Delegates and Events

        public delegate void CameraURl(SimplSharpString urlString);
        public CameraURl onCameraURL { get; set; }

        public delegate void ZoomChange(ushort control, ushort value);
        public ZoomChange onZoom { get; set; }

        public delegate void FocusChange(ushort control, ushort value);
        public FocusChange onFocus { get; set; }

        public delegate void VerticalMovement(ushort control, ushort value);
        public VerticalMovement onVerticalMovement { get; set; }

        public delegate void HorizontalMovement(ushort control, ushort value);
        public HorizontalMovement onHorizontalMovement { get; set; }

        public delegate void PrivacyChange(ushort value);
        public PrivacyChange onPrivacy { get; set; }

        public delegate void HomeRecalled(ushort value);
        public HomeRecalled onHomeRecalled { get; set; }

        public delegate void HomeSaved(ushort value);
        public HomeSaved onHomeSave { get; set; }

        public delegate void StreamIsActive(ushort value);
        public StreamIsActive onStream { get; set; }

        #endregion Delegates and Events

        #region Constructor
        public void Initialize(string name, string coreID, string pollgroup)
        {

            try
            {
                camControl = new CameraControlQsys(ProcessorHolder.holder[coreID], pollgroup, name);
                camControl.onCameraMovement += new CameraControlQsys.CameraIsMovingEventHandler(camControl_onCameraMovement);
                camControl.onCameraURL += new CameraControlQsys.CameraURl(camControl_onCameraURL);
                camControl.onHomeRecalled += new CameraControlQsys.HomeRecalled(camControl_onHomeRecalled);
                camControl.onHomeSave += new CameraControlQsys.HomeSaved(camControl_onHomeSave);
                camControl.onPrivacy += new CameraControlQsys.PrivacyChange(camControl_onPrivacy);
                camControl.onStream += new CameraControlQsys.StreamIsActive(camControl_onStream);
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("Error initializing Simpl+ Cam Control is: " + e);
            }

            
        }

        #endregion Constructor

        #region Internal Methods

        void camControl_onStream(bool value)
        {
            onStream(Convert.ToUInt16(value));
        }

        void camControl_onPrivacy(bool value)
        {
            onPrivacy(Convert.ToUInt16(value));
        }

        void camControl_onHomeSave(bool value)
        {
            onHomeSave(Convert.ToUInt16(value));
        }

        void camControl_onHomeRecalled(bool value)
        {
            onHomeRecalled(Convert.ToUInt16(value));
        }

        void camControl_onCameraURL(string urlString)
        {
            onCameraURL(urlString);
        }

        void camControl_onCameraMovement(eQSCCameraMovements movement, bool state)
        {
            switch (movement)
            {
                case eQSCCameraMovements.ZOOM_IN:
                case eQSCCameraMovements.ZOOM_OUT:
                    onZoom((ushort)movement, Convert.ToUInt16(state));
                    break;
                case eQSCCameraMovements.FOCUS_IN:
                case eQSCCameraMovements.FOCUS_OUT:
                    onFocus((ushort)movement, Convert.ToUInt16(state));
                    break;
                case eQSCCameraMovements.PAN_RIGHT:
                case eQSCCameraMovements.PAN_LEFT:
                    onHorizontalMovement((ushort)movement, Convert.ToUInt16(state));
                    break;
                case eQSCCameraMovements.TILT_UP:
                case eQSCCameraMovements.TILT_DOWN:
                    onVerticalMovement((ushort)movement, Convert.ToUInt16(state));
                    break;
                default:
                    break;
            }
        }

        #endregion Internal Methods

        #region Public Methods

        public void MoveCamera(ushort command, ushort value)
        {
            camControl.MoveCamera((eQSCCamControls)command, value);
        }

        public void Home()
        {
            camControl.Home();
        }

        public void SaveHome()
        {
            camControl.SaveHome();
        }

        public void Privacy()
        {
            camControl.Privacy();
        }

        #endregion Public Methods
    }
}