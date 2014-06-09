using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProgCom
{
    //scans for player output
    //mapped to address 48
    //has 7 memory addresses:
    //mem[48] = player throttle
    //mem[49] = player yaw
    //mem[50] = player pitch
    //mem[51] = player roll
    //mem[52] = player trn up
    //mem[53] = player trn east
    //mem[54] = player trn forward

    public class PCPlayerCtrlListener : PartModule, IPCHardware
    {
        UInt16 address = 48;
        Int32[] memarea = new Int32[7];
        public void connect()
        {
            
        }

        public void disconnect()
        {
            
        }

        public Tuple<ushort, int> getSegment(int id)
        {
            return new Tuple<UInt16, int>(address, 7);
        }

        public int getSegmentCount()
        {
            return 1;
        }

        public void recInterruptHandle(InterruptHandle seg)
        {
            
        }

        public int memRead(ushort position)
        {
            return memarea[position - address];
        }

        public void memWrite(ushort position, int value)
        {
            memarea[position - address] = value;
        }

        public void tick(int ticks)
        {
            
        }

        public override void OnUpdate()
        {

        }

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            if (state.Equals(PartModule.StartState.Editor)) return;//don't start stuff in the editor
            vessel.OnFlyByWire += updateState;
        }

        private void updateState(FlightCtrlState state) {
            //in order to make sure we capture the player controls, we read the controls here
            //mem[48] = player throttle
            memarea[0] = (Int32)(state.mainThrottle * 1024.0);
            //mem[49] = player yaw
            memarea[1] = (Int32)(state.yaw * 1024.0);
            //mem[50] = player pitch
            memarea[2] = (Int32)(state.pitch * 1024.0);
            //mem[51] = player roll
            memarea[3] = (Int32)(state.roll * 1024.0);
            //mem[52] = player trn right
            memarea[4] = (Int32)(state.X * 1024);
            //mem[53] = player trn up
            memarea[5] = (Int32)(state.Y * 1024);
            //mem[54] = player trn forward
            memarea[6] = (Int32)(state.Z * 1024);
            
        }
    }
}
