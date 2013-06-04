using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class SerialBus : ISerial
{
    UInt32[] localMemoryReference;
    ISerial connected;
    uint memPointer;
    bool reading;
    bool sending;
    int sendCycles; 
    Int32 bufferedInt;
    Int32 sendInt;
    public SerialBus(uint memLocation)
    {
        memPointer = memLocation;
        connected = null;
    }

    public void rec_bit(bool bit)
    {
        bufferedInt = bufferedInt << 1;
        bufferedInt += bit ? 1 : 0;
    }
    public void rec_sending()
    {
        reading = true;
    }
    public void rec_trans_finished()
    {
        reading = false;
    }
    public void rec_send_done()
    {

    }
    public void connect(ISerial sBus)
    {
        connected = sBus;
    }
    public void disconnect()
    {
        connected = null;
    }
    public void startSend(int i)
    {
        sendInt = i;
        if(connected != null) {
            connected.rec_sending();
        }
        sending = true;
    }
    public void tick(int ticks)
    {
        for (int i = 0; i < ticks; ++i) {
            if (sending) {
                if (sendInt == 32) {
                    sendCycles = 0;
                    sending = false;
                } else {
                    sendBit();
                }
            }
        }
    }
    private void sendBit()
    {

    }
}
