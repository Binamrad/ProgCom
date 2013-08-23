using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProgCom
{
    public interface ISerialConnector
    {
        void connect(ISerial toConnect, int slot);
        bool slotOccupied(int slot);
    }
}
