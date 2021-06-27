using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace DSP_Suite.Qsys
{
    #region Event Handlers

    public class ProcessorStatusEvents
    {
        public string DesignName;
        public string CorePlateform;
        public bool IsEmulator;
        public bool IsRedundant;
        public bool IsActive;


        public ProcessorStatusEvents(string designname, string platform, bool emulator, bool redundant, bool active)
        {
            DesignName = designname;
             CorePlateform = platform;
             IsEmulator = emulator;
             IsRedundant = redundant;
             IsActive = active;
        }
    }

    public class QsysEvents
    {
        private event EventHandler<QsysEventArgs> qsysEvent = delegate { };
        public event EventHandler<QsysEventArgs> QsysEvent
        {
            add
            {
                try
                {
                    if (!qsysEvent.GetInvocationList().Contains(value))
                    {
                        qsysEvent += value;
                    }
                }
                catch (Exception e)
                {
                    CrestronConsole.PrintLine("Qsys Events Event Error is: " + e);
                }
            }
            remove
            {
                QsysEvent -= value;
            }
        }

        internal void Send(QsysEventArgs e)
        {
            qsysEvent(this, e);
        }
    }

    //
    public class QsysEventArgs : EventArgs
    {
        public string name;
        public double value;
        public double position;
        public string stringValue;
        public List<string> choices;

        public QsysEventArgs(string Name, double Value, double Position, string StringValue, List<string> Choices)
        {
            name = Name;
            value = Value;
            position = Position;
            stringValue = StringValue;
            choices = Choices;
        }
    }

    public class QsysNamedControlFeedback
    {
        public string stringValue;
        public bool? boolvalue;
        public int? intValue;

        /// <summary>
        /// String Feedback
        /// </summary>
        /// <param name="stringData"></param>
        public QsysNamedControlFeedback(string stringData)
        {
            stringValue = stringData;
            boolvalue = false;
            intValue = -1;
        }

        /// <summary>
        /// Bool Feedback
        /// </summary>
        /// <param name="boolData"></param>
        public QsysNamedControlFeedback(bool boolData)
        {
            boolvalue = boolData;
            intValue = -1;
            stringValue = string.Empty;
        }

        /// <summary>
        /// Integer Feedback
        /// </summary>
        /// <param name="intData"></param>
        public QsysNamedControlFeedback(int intData)
        {
            intValue = intData;
            stringValue = string.Empty;
            boolvalue = false;
        }
    }

    #endregion

    public enum eQSCProcessorEventIDs
    {
        Registered,
        Connected
    }
    public enum eQSCNetworkBuffer
    {
        Default = 0,
        Extra1MS = 1, 
        Extra2MS = 2, 
        Extra5MS = 3
    }
    public enum eQSCNetworkInterface
    {
        LAN_A,
        LAN_B
    }
    public enum eQSCStreamStatus
    {
        STREAM_OK = 0,
        STREAM_INITIALIZING = 1,
        STREAM_COMPROMISED = 2,
        STREAM_MISSING = 3,
        STREAM_FAULT = 4
    }
    public enum eQSCAES67Controls
    {
        Enable = 0,
        StreamName = 1,
        StreamBuffer = 2,
        StreamInterface = 3,
        StreamMulticast = 4
    }
    public enum eQSCCameraMovements
    {
        ZOOM_IN = 0,
        ZOOM_OUT = 1,
        FOCUS_IN = 2,
        FOCUS_OUT = 3,
        PAN_LEFT = 4,
        PAN_RIGHT = 5,
        TILT_UP = 6,
        TILT_DOWN = 7
    }
    public enum eQSCCamControls
    {
        ZoomIn = 1,
        ZoomOut = 2,
        FocusIn = 10,
        FocusOut = 11,
        MoveLeft = 9,
        MoveRight = 8,
        MoveUp = 3,
        MoveDown = 4,
        LoadHome = 7,
        SaveHome = 6,
        Recallprivacy = 5
    }

    public enum eQSCNamedControlType
    {
        String = 0,
        Bool = 1,
        Integer = 2
    }

    public enum eQlanArgs
    {
        Enabled,
        BlockActive,
        StreamActive
    }

    public enum eQSCCoreState
    {
        CoreOK = 1,
        CoreInitializing = 2,
        CoreCompromised = 3,
        CoreMissing = 4,
        CoreFault = 5,
        CoreUnknown = 6,
        CoreNotPresent = 7
    }

    public enum eQSCCallState
    {
        Active = 1,
        Inactive = 2,
        Outgoing = 3,
        Incoming = 4
    }

}