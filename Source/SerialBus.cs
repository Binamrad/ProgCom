using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace ProgCom
{
    class SerialBus : ISerial
    {
        Int32[] localMemRef;
        ISerial connected;
        UInt16 memPointer;
        Int32 currentBit;
        bool sending = false;
        bool receiving = false;

        //TODO:
        //make interact with memory, sending/receiving bits etc.
        //  0:  flags register
        //      bits:   0:  interrupt enable
        //              1:  output buffer empty
        //              2:  input buffer full
        //              3:  sending
        //              4:  receiving
        //              5:  recipient ready
        //  1:  receive reg
        //  2:  send reg
        //  3:  reserved

        public SerialBus(UInt16 memLocation, Int32[] mem)
        {
            memPointer = memLocation;
            localMemRef = mem;
            connected = null;
        }
        public void rec_bit(bool bit)
        {
            localMemRef[memPointer + 1] = ((Int32)(((UInt32)localMemRef[memPointer + 1]) >> 1) + ((bit ? 1 : 0) << 31));
        }
        public void rec_sending()
        {
            receiving = true;
        }
        public void rec_send_done()
        {
            receiving = false;
            //set bit 2 to 1
            localMemRef[memPointer] = Util.setBit(((Int32)localMemRef[memPointer]), 2, 1);
        }
        public bool ready()
        {
            return (localMemRef[memPointer] & (1 << 2)) == 0;
        }
        public void connect(ISerial sBus)
        {
            connected = sBus;
        }
        public void disconnect()
        {
            if (connected != null) {
                ISerial tmp = connected;
                connected = null;
                tmp.disconnect();
            }
        }
        public void startSend()
        {
            currentBit = 0;
            if (connected != null) {
                sending = true;
                connected.rec_sending();
            }
        }
        public void tick(int ticks)
        {
            if (connected == null)
                return;
            for (int i = 0; i < ticks && isSending(); ++i) {
                if (currentBit == 32) {
                    connected.rec_send_done();
                    //set bit 1 t0 1
                    localMemRef[memPointer] = Util.setBit(((Int32)localMemRef[memPointer]), 1, 1);
                    sending = false;
                } else {
                    sendBit();
                }
            }
 
            //cycle connected interface
            connected.tick(ticks);

            //set bit 5 to connected.ready()
            localMemRef[memPointer] = Util.setBit(((Int32)localMemRef[memPointer]), 5, connected.ready());
            //set bit 4 to 0
            localMemRef[memPointer] = Util.setBit(((Int32)localMemRef[memPointer]), 4, receiving);
            //set bit 3 to sending status
            localMemRef[memPointer] = Util.setBit(((Int32)localMemRef[memPointer]), 3, sending);
        }
        private void sendBit()
        {
            connected.rec_bit((localMemRef[memPointer + 2] & (1 << currentBit)) != 0);
            ++currentBit;
        }
        private bool isSending()
        {
            return sending;
        }
        public bool isOccupied()
        {
            return connected != null;
        }
    }
}