using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ProgCom
{
    //controls the ship
    //mapped to 0 and 52
    //has 4 components + 3 at 52
    public class PCControlManager : PartModule, IPCHardware, PCGUIListener
    {
        Int32[] ctrls = new Int32[7];
        bool controlling;
        InterruptHandle inth;
        GUIStates guis = new GUIStates();
        public void connect()
        {
            //Do nothing here
        }
        
        public void disconnect()
        {
            //Holy crap do something here!
        }
        
        public Tuple<UInt16, int> getSegment(int id)
        {
            if (id == 0) {
                return new Tuple<UInt16, int>(0, 4);
            } else if (id == 1) {
                return new Tuple<UInt16, int>(56, 3);//figure out where this is supposed to be placed
            } else return null;
        }
        
        public int getSegmentCount()
        {
            return 2;
        }

        public Int32 memRead(UInt16 addr)
        {
            if (addr < 4) {
                //low memory addresses
                return ctrls[addr];
            } else {
                return ctrls[addr - 52];
            }
        }

        public void memWrite(UInt16 addr, Int32 data)
        {
            if (addr < 4) {
                ctrls[addr] = data;
            } else {
                ctrls[addr - 52] = data;
            }
        }

        public void recInterruptHandle(InterruptHandle inth)
        {
            this.inth = inth;
        }

        public void tick(int ticks)
        {
            controlling = true;
            //we set controlling to true if we receive updates on it.
            //If we don't receive updates, the emulator is turned off, and we should not attempt to control the craft
        }

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            if(state.Equals(PartModule.StartState.Editor)) return;//don't start stuff in the editor
            vessel.OnFlyByWire += performManouvers;
        }

        private void performManouvers(FlightCtrlState state)
        {

            if (controlling) {//TODO: have some kind of memory address that tells us if we should be controlling the craft
                float f;

                if (guis.ttl) {
                    f = (float)ctrls[0];
                    f /= 1024.0F;
                    state.mainThrottle = f; //set throttle to computer-specified value
                    //the range of throttle is 0.0F to 1.0F
                    //we clamp the values to make sure they are in range
                    state.mainThrottle = Mathf.Clamp(state.mainThrottle, 0.0F, +1.0F);
                }


                if (guis.ctl) {
                    f = (float)ctrls[1];
                    f /= 1024.0F;
                    state.yaw = f; //set yaw to computer-specified value

                    f = (float)ctrls[2];
                    f /= 1024.0F;
                    state.pitch = f; //set pitch to computer-specified value

                    f = (float)ctrls[3];
                    f /= 1024.0F;
                    state.roll = f; //set roll to computer-specified value */

                    //add control for translation.
                    f = (float)ctrls[4];
                    f /= 1024.0F;
                    state.X = f;
                    f = (float)ctrls[5];
                    f /= 1024.0F;
                    state.Y = f;
                    f = (float)ctrls[6];
                    f /= 1024.0F;
                    state.Z = f;


                    //the range of yaw, pitch, and roll is -1.0F to 1.0F, as are X, Y and Z
                    //we clamp the values to make sure they are in range
                    state.yaw = Mathf.Clamp(state.yaw, -1.0F, +1.0F);
                    state.pitch = Mathf.Clamp(state.pitch, -1.0F, +1.0F);
                    state.roll = Mathf.Clamp(state.roll, -1.0F, +1.0F);
                    state.X = Mathf.Clamp(state.X, -1.0F, +1.0F);
                    state.Y = Mathf.Clamp(state.Y, -1.0F, +1.0F);
                    state.Z = Mathf.Clamp(state.Z, -1.0F, +1.0F);
                }
                controlling = false;
            }
        }

        public void recGUIState(GUIStates g)
        {
            guis = g;
        }
    }
}
