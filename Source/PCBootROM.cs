using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProgCom
{
    public class PCBootROM : PartModule, IPCHardware
    {
        //this should not be overwritten
        //TODO: update this to work with the new system for stuff.
        private Int32[] bootldr = {
                                -534773754,
                                -1207959524,
                                -1207959517,
                                -1608450052,
                                -534772990,
                                -1207959528,
                                -534773503,
                                -1207959530,
                                -1207959523,
                                -1608450058,
                                1746993160,
                                874577922,
                                -1207959535,
                                -534773247,
                                -1207959537,
                                -1207959530,
                                -1040187391,
                                -1067450367,
                                -1207959533,
                                -1038090239,
                                -1065353215,
                                1746993160,
                                874577921,
                                -1207959546,
                                -1207959539,
                                -333250560,
                                543358977,
                                612630529,
                                -1535049733,
                                -2013265904,
                                -398458814,
                                809631752,
                                -1539244035,
                                -398458814,
                                809631746,
                                -1606352899,
                                -333447104,
                                -2013265905,
                                -400555966,
                                807469057,
                                -1608450051,
                                -400555967,
                                -2013265905//43 instructions
                              };


        UInt16 address = 128;
        public void connect()
        {
            //do we do anything here? probably not
        }
        public void disconnect()
        {
            //is this ever going to be called?
        }
        public Tuple<UInt16, int> getSegment(int id)
        {
            return new Tuple<UInt16, int>(address, bootldr.Length);
        }
        public int getSegmentCount()
        {
            return 1;
        }
        public Int32 memRead(UInt16 address)
        {
            //subtract allocated address from presented address
            int index = address - this.address;
            if (index < 0 || index >= bootldr.Length) {
                throw new Exception("address out of range in PCBootROM. How did this happen? " +address + "" + this.address);
            }

            return bootldr[address - this.address];
        }

        public void memWrite(UInt16 address, Int32 data)
        {
            //this is ROM. Do nothing here.
        }

        public void recInterruptHandle(InterruptHandle segment)
        {
            //we don't need it
        }

        public void tick(int ticks) {

        }
    }
}
