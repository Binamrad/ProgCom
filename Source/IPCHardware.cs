using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProgCom
{
    public interface IPCHardware
    {
        //called once the device has been connected.
        void connect();
        //called if the device should be disconnected.
        void disconnect();
        //return address and length of requested segment
        Tuple<UInt16, int> getSegment(int id);
        //get number continuous memory blocks this device needs
        int getSegmentCount();
        //returns the id, length and position of a requested segment
        void recInterruptHandle(InterruptHandle seg);
        //memory read request
        Int32 memRead(UInt16 position);
        //memory write request
        void memWrite(UInt16 position, Int32 value);
        //update x cycles
        void tick(int ticks);
    }
}
