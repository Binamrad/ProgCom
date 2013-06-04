using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

interface ISerial
{
    void rec_bit(bool bit);
    void rec_sending();
    void rec_trans_finished();
    void rec_send_done();
    void connect(ISerial sBus);
    void disconnect();
    void tick(int ticks);
    void startSend(int send);
}
