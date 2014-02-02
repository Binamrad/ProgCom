using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
namespace ProgCom
{
    public class Keyboard : PartModule, IPCHardware, PCGUIListener
    {
        int charSend;
        protected Rect windowPos;
        bool sending = false;
        int bitSend = 0;
        int lastSent = 0;//here to make sure we don't send an infinite flood of characters to the cpu
        LinkedList<int> buttonsDown;
        protected int windowID;
        Int32[] memarea = new Int32[2];
        GUIStates guis = new GUIStates();
        InterruptHandle inth;

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            windowID = Util.random();
            windowPos = new Rect(Screen.width / 2, Screen.height / 2, 100, 100);
            buttonsDown = new LinkedList<int>();
            RenderingManager.AddToPostDrawQueue(3, new Callback(draw));
        }

        //if a key has been pressed, send one bit per cycle for 32 cycles
        public void tick(int c)
        {
            while (c > 0 && sending) {
                ++bitSend;
                if (bitSend == 32) {
                    sending = false;
                    bitSend = 0;
                    memarea[0] = charSend;
                    memarea[1] = 1;
                }
                --c;
            }
        }

        //do some gui stuff, check for events. When a button is pressed, send the character code to the bus
        protected void guiFunction(int windowID)
        {
            //input line
            if (Event.current.type == EventType.KeyDown || Event.current.type == EventType.KeyUp) {
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
                if ( !buttonsDown.Contains(charSend) && !sending) {
                    if (charSend < 0) {
                        buttonsDown.Remove(-charSend);
                    } else {
                        buttonsDown.AddFirst(charSend);
                    }
                    int tmp = lastSent;
                    lastSent = charSend;
                    sending = true;
                    bitSend = 0;
                }
                Event.current.Use();//removes event from event queue
            }
            GUILayout.TextField("");
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        public void draw()
        {
            if (guis.kbd) {
                GUI.skin = HighLogic.Skin;
                windowPos = GUILayout.Window(windowID, windowPos, guiFunction, "Keyboard", GUILayout.MinWidth(100));
            }
        }

        public void connect()
        {
            
        }

        public void disconnect()
        {
            
        }

        public Tuple<ushort, int> getSegment(int id)
        {
            return new Tuple<UInt16, int>(68, 2);
        }

        public int getSegmentCount()
        {
            return 1;
        }

        public void recInterruptHandle(InterruptHandle seg)
        {
            inth = seg;
        }

        public int memRead(ushort position)
        {
            return memarea[position - 68];
        }

        public void memWrite(ushort position, int value)
        {
            memarea[position - 68] = value;
        }

        public void recGUIState(GUIStates g)
        {
            guis = g;
        }
    }
}
