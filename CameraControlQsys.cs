using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace DSP_Suite.Qsys
{
    public class CameraControlQsys
    {
        #region Fields

        private string name = "";
        private string pollGroup;
        private bool registered;
        private bool isComponent;
        private bool isPrivacy;
        private bool isHome;

        private string camURL;

        private Component component;
        private List<Control> controls = new List<Control>();
        private ProcessorQsys core;

        #endregion Fields

        #region Constants

        private const string camUrl = "url.stream.1.display";
        private const string camZoomIn = "zoom.in";
        private const string camZoomOut = "zoom.out";
        private const string camTiltUp = "tilt.up";
        private const string camTitlDown = "tilt.down";
        private const string camPrivacyMode = "toggle.privacy";
        private const string camIsStreaming = "streaming.network";
        private const string camSetZoomSpeed = "setup.zoom.speed";
        private const string camSetTiltSpeed = "setup.tilt.speed";
        private const string camResetSetting = "setup.reset.settings";
        private const string camSetPanSpeed = "setup.pan.speed";
        private const string camAllowHDMI = "setup.hdmi.enable";
        private const string camSetFocusSpeed = "setup.focus.speed";
        private const string camSaveHomePos = "preset.home.save.trigger";
        private const string camLoadHome = "preset.home.load";
        private const string camPanRight = "pan.right";
        private const string camPanLeft = "pan.left";
        private const string camFocusNear = "focus.near";
        private const string camFocusFar = "focus.far";
        #endregion Constants

        #region Properties

        public bool IsRegistered { get { return registered; } }
        public bool IsComponent { get { return isComponent; } }
        public bool IsHome { get; set; }
        public bool IsPrivacy { get{return isPrivacy;} }

        public string CameraURL { get { return camURL; } }
        public string ComponentName { get { return name; } }
        public string PollingGroup { get { return pollGroup; } }


        #endregion Properties

        #region Delegates

        public delegate void CameraIsMovingEventHandler(eQSCCameraMovements movement, bool state);
        public delegate void CameraURl(string urlString);
        public delegate void PrivacyChange(bool value);
        public delegate void HomeRecalled(bool value);
        public delegate void HomeSaved(bool value);
        public delegate void StreamIsActive(bool value);

        #endregion Delegates 

        #region Events

        public event CameraIsMovingEventHandler onCameraMovement;
        public event CameraURl onCameraURL;
        public event PrivacyChange onPrivacy;
        public event HomeRecalled onHomeRecalled;
        public event HomeSaved onHomeSave;
        public event StreamIsActive onStream;

        #endregion Events

        #region Constructor

        public CameraControlQsys(ProcessorQsys qsysCore, string groupID, string componentName)
        {

            try
            {
                core = qsysCore;

                pollGroup = groupID;

                name = componentName;
                component = new Component();
                component.Name = name;

                for (int i = 0; i < 12; i++)
                {
                    controls.Add(new Control());
                }

                controls[0].Name = camUrl;
                controls[1].Name = camZoomIn;
                controls[2].Name = camZoomOut;
                controls[3].Name = camTiltUp;
                controls[4].Name = camTitlDown;
                controls[5].Name = camPrivacyMode;
                controls[6].Name = camSaveHomePos;
                controls[7].Name = camLoadHome;
                controls[8].Name = camPanRight;
                controls[9].Name = camPanLeft;
                controls[10].Name = camFocusNear;
                controls[11].Name = camFocusFar;

                component.Controls = controls;

                if (core.RegisterComponent(component, groupID))
                {
                    core.Components[component].QsysEvent += new EventHandler<QsysEventArgs>(CameraControlQsys_QsysEvent);
                    registered = true;
                    isComponent = true;
                    core.SendDebug("Component" + component.Name + " was succesfully registered");
                }
            }
            catch (Exception e)
            {
                core.SendDebug("Component " + componentName + "Initialize error is: " + e);
            }

        }

        #endregion Constructor

        #region Internal Methods

        void CameraControlQsys_QsysEvent(object sender, QsysEventArgs e)
        {
            #region Camera URL
            if (e.name == camUrl)
            {
                onCameraURL(e.stringValue);
                camURL = e.stringValue;
            }
            #endregion
            #region Zoom In
            else if (e.name == camZoomIn)
               onCameraMovement(eQSCCameraMovements.ZOOM_IN, Convert.ToBoolean(e.value));
            #endregion
            #region Zoom Out
            else if (e.name == camZoomOut)
                onCameraMovement(eQSCCameraMovements.ZOOM_OUT, Convert.ToBoolean(e.value));
            #endregion
            #region Tilt Up
            else if (e.name == camTiltUp)
                onCameraMovement(eQSCCameraMovements.TILT_UP, Convert.ToBoolean(e.value));
            #endregion
            #region Tilt Down
            else if (e.name == camTitlDown)
                onCameraMovement(eQSCCameraMovements.TILT_DOWN, Convert.ToBoolean(e.value));
            #endregion
            #region Privacy mode
            else if (e.name == camPrivacyMode)
            {
                onPrivacy(Convert.ToBoolean(e.value));
                isPrivacy = e.value == 1 ? true : false;
            }
            #endregion
            #region Home Saved
            else if (e.name == camSaveHomePos)
                onHomeSave(Convert.ToBoolean(e.value));
            #endregion
            #region Home Loaded
            else if (e.name == camLoadHome)
            {
                isHome = Convert.ToBoolean(e.value);
                onHomeRecalled(isHome);
            }
            #endregion
            #region pan Right
            else if (e.name == camPanRight)
                onCameraMovement(eQSCCameraMovements.PAN_RIGHT, Convert.ToBoolean(e.value));
            #endregion
            #region Pan Left
            else if (e.name == camPanLeft)
                onCameraMovement(eQSCCameraMovements.PAN_LEFT, Convert.ToBoolean(e.value));
            #endregion
            #region Focus Near
            else if (e.name == camFocusNear)
                onCameraMovement(eQSCCameraMovements.FOCUS_IN, Convert.ToBoolean(e.value));
            #endregion
            #region Focus far
            else if (e.name == camFocusFar)
                onCameraMovement(eQSCCameraMovements.FOCUS_OUT, Convert.ToBoolean(e.value));
            #endregion
            #region Cam Streaming
            else if (e.name == camIsStreaming)
                onStream(Convert.ToBoolean(e.value));
            #endregion
        }

        private void ComponentBuilder(eQSCCamControls command, object value, string commandName)
        {
            ComponentSet trigger = new ComponentSet();
            trigger.Params = new ComponentSetParams();
            trigger.Params.Name = name;

            ComponentSetControl setControl = new ComponentSetControl();
            setControl.Name = commandName;


            setControl.Value = Convert.ToDouble(value);

            trigger.Params.Controls = new List<ComponentSetControl>();
            trigger.Params.Controls.Add(setControl);

            core.QCommand(core.CommandBuider(trigger));
        }

        private int GetPosition(eQSCCamControls control)
        {
            return controls.FindIndex(s => s.Name == controls[(int)control].Name);
        }

        #endregion Internal Methods

        #region Public Methods

        public void MoveCamera(eQSCCamControls camControls, int value)
        {
            ComponentBuilder(camControls, value, controls[GetPosition(camControls)].Name);
        }

        public void Home()
        {
            ComponentBuilder(eQSCCamControls.LoadHome, 1, controls[GetPosition(eQSCCamControls.LoadHome)].Name);
        }

        public void SaveHome()
        {
            ComponentBuilder(eQSCCamControls.SaveHome, 1, controls[GetPosition(eQSCCamControls.SaveHome)].Name);
        }

        public void Privacy()
        {
            if (isPrivacy)
                ComponentBuilder(eQSCCamControls.Recallprivacy, 0, controls[GetPosition(eQSCCamControls.Recallprivacy)].Name);
            else
                ComponentBuilder(eQSCCamControls.Recallprivacy, 1, controls[GetPosition(eQSCCamControls.Recallprivacy)].Name);
        }

        #endregion Public Methods
    }
}