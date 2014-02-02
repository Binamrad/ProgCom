using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProgCom
{
    //this doesn't extend the partmodule thingamajig since I want this to always be present in the CPU
    class PCTimer : IPCHardware
    {
        UInt32[] memarea = new UInt32[3];
        UInt16 address = 59;
        InterruptHandle inth;
        const int timerOverflow = 256;

        public void connect()
        {
            
        }

        public void disconnect()
        {
            
        }

        public Tuple<ushort, int> getSegment(int id)
        {
            return new Tuple<UInt16, int>(address, 3);
        }

        public int getSegmentCount()
        {
            return 1;
        }

        public void recInterruptHandle(InterruptHandle seg)
        {
            inth = seg;
        }

        public int memRead(ushort position)
        {
            return (Int32)memarea[position - address];
        }

        public void memWrite(ushort position, int value)
        {
            if (position == address) return;
            memarea[position - address] = (uint)value;
        }

        public void tick(int ticks)
        {
            memarea[0] += (uint)ticks;
            memarea[1] += (uint)ticks;
            if (memarea[1] >= memarea[2] && memarea[2] > 0) {
                memarea[1] %= memarea[2];
                inth.interrupt(timerOverflow);
            }
        }
    }
}
