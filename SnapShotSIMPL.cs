using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace DSP_Suite.Qsys
{
    public class SnapShotSIMPL
    {


        private SnapShotQsys snapshot;

        public delegate void ActiveSnapShot(ushort val);
        public ActiveSnapShot onActiveSnapShot { get; set; }

        public void Initialize(string name, string coreID, string pollgroup)
        {
            snapshot = new SnapShotQsys(ProcessorHolder.holder[coreID], pollgroup, name);
            snapshot.onActiveSnapShot += new SnapShotQsys.ActiveSnapShot(snapshot_onActiveSnapShot);
        }

        void snapshot_onActiveSnapShot(int snap)
        {
            onActiveSnapShot((ushort)snap);
        }

        public void SetSnap(ushort snap)
        {
            snapshot.SetSnap(snap);
        }

        public void RecallSnap(ushort snap)
        {
            snapshot.RecallSnap(snap);
        }
    }
}