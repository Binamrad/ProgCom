using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProgCom
{
    public interface ISerial
    {
        //called when the class should receive a bit
        void rec_bit(bool bit);

        //indicates that the connected interface has started sending data
        void rec_sending();

        //called when the connected interface has sent all data
        void rec_send_done();

        //called when you want to know if you can send to the interface
        bool ready();

        //connects to a serial interface
        void connect(ISerial sBus);

        //disconnects the module from all connected interfaces
        void disconnect();

        //called once per completed instruction
        void tick(int ticks);

    }
}