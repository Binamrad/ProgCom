using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProgCom
{
    public abstract class ASerialTransceiver : ISerial
    {
        //connected interface
        private ISerial connected;
        //sending stuff
        private bool sending;
        private UInt32 sendInt;
        private int bitToSend;
        //receiving stuff
        private bool receiving;
        private bool validInput;
        //private int recBit;
        private UInt32 recInt;


        protected void send(UInt32 i)
        {
            if (connected != null) {
                sending = true;
                sendInt = i;
                bitToSend = 0;
            }
        }

        protected void stopSend()
        {
            if (connected != null) {
                connected.rec_send_done();
                sending = false;
            }
        }

        protected string fuckingString()
        {
            return " rec: " + receiving + " vip: " + validInput + " cst: " + customReadyCondition();
        }

        protected bool canSend()
        {
            return !sending && connected != null && connected.ready();
        }

        protected bool hasReceived()
        {
            return validInput;
        }

        protected Int32 getLastReceived()
        {
            validInput = false;
            return (Int32)recInt;
        }

        protected abstract void serialUpdate();
        protected virtual bool customReadyCondition()
        {
            return true;
        }

        /***********************************/
        //ISerial overrides here
        public void rec_sending()
        {
            receiving = true;
            validInput = false;
            recInt = 0;
        }

        public void rec_send_done()
        {
            receiving = false;
            validInput = true;
        }

        public void rec_bit(bool b)
        {
            recInt = (recInt >> 1) | (UInt32)(b ? 1 << 31: 0);
        }

        public void connect(ISerial I)
        {
            connected = I;
        }

        public void disconnect()
        {
            connected = null;
            sending = false;
            receiving = false;
        }

        public bool ready()
        {
            return (!receiving) && (!validInput) && customReadyCondition();
        }

        public void tick(int ticks)
        {
            
            for (int i = 0; i < ticks; ++i) {
                if (sending) {
                    if (bitToSend == 32) {
                        stopSend();
                    } else {
                        connected.rec_bit((sendInt & (1 << bitToSend)) != 0);
                        ++bitToSend;
                    }
                }
                serialUpdate();
            }
        }
    }
}
