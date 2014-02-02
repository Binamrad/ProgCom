using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProgCom
{
    //this class acts as a sort of barrier between progcom memory and whatever thing is trying to access it.
    //by doing this we make sure we always can know what data each device can access, which c
    public class InterruptHandle
    {
        CPUem cpu;
        public InterruptHandle(CPUem c)
        {
            cpu = c;
        }

        public void interrupt(Int32 intID)
        {
            cpu.spawnException(intID);
        }
    }
}
