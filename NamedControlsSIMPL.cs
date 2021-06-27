using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace DSP_Suite.Qsys
{
    public class NamedControlsSIMPL
    {
        #region Fields

        private NamedControlsQsys namedControl;

        #endregion Fields

        #region Properties

        #endregion Properties

        #region Delegates and Events

        public delegate void NamedControlChange(ushort value, SimplSharpString data);
        public NamedControlChange onNamedControl { get; set; }

        #endregion

        #region Constructor

        public void Initialize(string name, string coreID, string pollgroup, ushort controlType)
        {
            namedControl = new NamedControlsQsys((eQSCNamedControlType)controlType, ProcessorHolder.holder[coreID], pollgroup, name);
            namedControl.onNamedControl += new NamedControlsQsys.NamedControlChange(namedControl_onNamedControl);
        }

        void namedControl_onNamedControl(eQSCNamedControlType ControlTypeFeedback, QsysNamedControlFeedback feedback)
        {
            switch (ControlTypeFeedback)
            {
                case eQSCNamedControlType.String:
                    onNamedControl(0, feedback.stringValue);
                    break;
                case eQSCNamedControlType.Bool:
                    onNamedControl(Convert.ToUInt16(feedback.boolvalue), "");
                    break;
                case eQSCNamedControlType.Integer:
                    onNamedControl(Convert.ToUInt16(feedback.boolvalue), "");
                    break;
                default:
                    break;
            }
        }


        #endregion Constructor

        #region Internal Methods


        #endregion

        #region Public Methods

        public void setInteger(ushort value)
        {
            namedControl.setInteger(value);
        }

        public void SetBool(ushort value)
        {
            namedControl.SetBool(Convert.ToBoolean(value));
        }

        public void SetString(string value)
        {
            namedControl.SetString(value);
        }


        #endregion Public Methods
    }
}