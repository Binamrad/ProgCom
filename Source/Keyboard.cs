using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
namespace ProgCom
{
    class Keyboard : ISerial
    {
        ISerial connected;
        int charSend;
        protected Rect windowPos;
        bool sending = false;
        int bitSend = 0;
        int lastSent = 0;//here to make sure we don't send an infinite flood of characters to the cpu
        public bool visible = false;
        LinkedList<int> buttonsDown;
        protected int windowID;

        public Keyboard()
        {
            windowID = Util.random();
            windowPos = new Rect(Screen.width / 2, Screen.height / 2, 100, 100);
            buttonsDown = new LinkedList<int>();
        }

        public void connect(ISerial s)
        {
            connected = s;
        }

        public void disconnect()
        {
            connected = null;
        }

        // we don't care about these yet
        public void rec_bit(bool b) { }
        public void rec_send_done() { }
        public void rec_sending() { }
        public bool ready() { return false; }

        //if a key has been pressed, send one bit per cycle for 32 cycles
        public void tick(int c)
        {
            while (c > 0 && sending && connected.ready()) {
                connected.rec_bit(((charSend) & (1 << bitSend)) != 0);
                ++bitSend;
                if (bitSend == 32) {
                    sending = false;
                    bitSend = 0;
                    connected.rec_send_done();
                }
                --c;
            }
        }

        //do some gui stuff, check for events. When a button is pressed, send the character code to the bus
        protected void guiFunction(int windowID)
        {
            //input line
            if (Event.current.type == EventType.KeyDown || Event.current.type == EventType.KeyUp) {
                //TODO:if keyUp, send negative version of asdfgh <-Done?
                //TODO:if prev char sent is current char sending, don't send <-Done?
                //TODO:make some documentation on whatever the hell this thing outputs

                //get the button that is pressed
                char c = Event.current.character;
                int i = (int)c;
                i = i & 255;
                if (i == 0) {
                    i = (int)Event.current.keyCode;
                }
                //charSend = (Byte)i;
                charSend = i;
                //handle enter sending two different chars
                if (charSend == 13) {
                    charSend = 10;
                }

                if (Event.current.type == EventType.KeyUp) {
                    charSend = -charSend;
                }

                //send the stuff
                if (connected != null && !buttonsDown.Contains(charSend) && !sending) {
                    if (charSend < 0) {
                        buttonsDown.Remove(-charSend);
                    } else {
                        buttonsDown.AddFirst(charSend);
                    }
                    int tmp = lastSent;
                    lastSent = charSend;
                    //if (tmp != 0) {//remove this darnit
                    //    throw new FormatException("lastSent: " + tmp + " charSend: " + charSend);
                    //}
                    sending = true;
                    bitSend = 0;
                    connected.rec_sending();
                }
                Event.current.Use();
            }
            GUILayout.TextField("");
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        public void draw()
        {
            if (visible) {
                GUI.skin = HighLogic.Skin;
                windowPos = GUILayout.Window(windowID, windowPos, guiFunction, "Keyboard", GUILayout.MinWidth(100));
            }
        }
    }
}
