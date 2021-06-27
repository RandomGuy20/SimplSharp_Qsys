using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace DSP_Suite.Qsys
{
    public class NamedControlsQsys
    {
        
        #region Fields

        private string name = "";
        private bool registered;

        private string pollGroup;

        private Control control;
        private ProcessorQsys core;
        private eQSCNamedControlType namedControlType;


        #endregion Fields

        #region Properties

        public string ComponentName { get { return name; } }
        public string PollingGroup { get{ return pollGroup;} }

        public bool IsRegistered { get { return registered; }}


        public eQSCNamedControlType NamedControlType { get{return namedControlType;} }

        #endregion Properties

        #region Delegates and Events

        public delegate void NamedControlChange(eQSCNamedControlType ControlTypeFeedback, QsysNamedControlFeedback feedback);
        public event NamedControlChange onNamedControl;

        #endregion

        #region Constructors

        public NamedControlsQsys(eQSCNamedControlType controlType,ProcessorQsys qsysCore, string groupID, string componentName)
        {
            core = qsysCore;

            namedControlType = controlType;

            name = componentName;
            pollGroup = groupID;



            control = new Control();
            control.Name = name;


            if (core.RegisterControl(control, groupID)) 
            {
                core.Controls[control].QsysEvent += new EventHandler<QsysEventArgs>(NamedControlsQsys_QsysEvent);
                registered = true;
                core.SendDebug("Control name: " + name  + "registered succesfully");
            }
        }

        #endregion Constructor

        #region Internal Methods

        void NamedControlsQsys_QsysEvent(object sender, QsysEventArgs e)
        {

            core.SendDebug("The Value Received for Control " + name + "is: " + e.value + " and the string is: " + e.stringValue);

            switch (namedControlType)
            {
                case eQSCNamedControlType.String:
                    onNamedControl(namedControlType, new QsysNamedControlFeedback(e.stringValue));
                    break;
                case eQSCNamedControlType.Bool:
                    onNamedControl(namedControlType, new QsysNamedControlFeedback(Convert.ToBoolean(e.value)));
                    break;
                case eQSCNamedControlType.Integer:
                    onNamedControl(namedControlType, new QsysNamedControlFeedback((int)e.value));
                    break;
                default:
                    break;
            }
        }

        private void CommandBuilder(eQSCNamedControlType controlType, object data)
        {
            ControlIntegerSet set = new ControlIntegerSet();
            set.Params = new ControlIntegerSetParams();
            set.Params.Name = name;
            switch (controlType)
            {
                case eQSCNamedControlType.Bool:
                    set.Params.Value = Convert.ToInt32(data);
                    break;
                case eQSCNamedControlType.Integer:
                    set.Params.Position = core.ScaleToCore(Convert.ToDouble(data));
                    break;
            }

            core.QCommand(core.CommandBuider(set));
        }

        #endregion

        #region Public Methods


        public void setInteger(int value)
        {
            CommandBuilder(namedControlType, core.ScaleToCore((double)value));
        }

        public void SetBool(bool value)
        {
            CommandBuilder(namedControlType, Convert.ToInt32(value));
        }

        public void SetString(string value)
        {
            ControlStringSet set = new ControlStringSet();
            set.Params = new ControlStringSetParams();
            set.Params.Name = name;
            set.Params.Value = value;

            core.QCommand(core.CommandBuider(set));
        }


        #endregion Public Methods
    }
}