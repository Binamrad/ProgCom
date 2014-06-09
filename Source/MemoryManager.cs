using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProgCom
{
    //what is the purpose of this class?
    //answer: to manage what devices are connected to the  machine, and disconnect them on request so that they are not continuously updated
    class MemoryManager
    {
        //Do not allow for overlapping memory segments
        private IPCHardware[] deviceMap;
        private Int32[] memory;
        //LinkedList<IPCHardware> devices;
        public MemoryManager()
        {
            deviceMap = new IPCHardware[64 * 1024];//this might not be the most memory-efficient way to do this, but it allows for the fastest accesses
            memory = new Int32[1024 * 64];
        }
        //if no device is mapped to the address this returns the memory from address
        //otherwise it requests memory via the memory-mapped device
        public Int32 memoryRead(UInt16 address)
        {
            if (deviceMap[address] != null) return deviceMap[address].memRead(address);
            else return memory[address];
        }

        public void memoryWrite(UInt16 address, Int32 data)
        {
            if (deviceMap[address] != null) deviceMap[address].memWrite(address, data);
            else memory[address] = data;
        }

        public bool map(IPCHardware hw)
        {
            //read ammount of segments from hw
            int segCount = hw.getSegmentCount();
            //for each segment, allocate it
            for (int i = 0; i < segCount; ++i) {
                Tuple<UInt16, int> segment = hw.getSegment(i);
                for (int j = 0; j < segment.Item2; ++j) {
                    if (deviceMap[j + segment.Item1] != null) {
                        throw new ArgumentException("Intersecting segments are not allowed");
                    } else {
                        deviceMap[j + segment.Item1] = hw;
                    }
                }
            }
            //devices.AddLast(hw);
            return true;
        }

        public bool unmap(Int32 deviceNum)
        {
            return false;
        }

        public Int32[] Memory()
        {
            return memory;
        }


        UInt16 lastChecked;
        int lastDevCount;

        //returns the ammount of hardware devices in the specified range
        public int devicesInRange(UInt16 address, int range)
        {
            if (address == lastChecked) return lastDevCount;
            lastChecked = address;
            int devCount = 0;
            for (int i = address; i < range+address; ++i) {
                if (deviceMap[i] != null) {
                    ++devCount;
                    int j;
                    for (j = i+1; j < range+address; ++j) {
                        if (deviceMap[j] != deviceMap[i]) break;
                    }
                    i = j-1;
                }
            }
            lastDevCount = devCount;
            return devCount;
        }

    }
}
