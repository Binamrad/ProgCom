using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ProgCom
{
    //a basic programmable computer, for creating control programmes for KSP vehicles
    //runs a simulated version of an imaginary processor.
    /*
     * TODO:
     * move interrupts/numpad to new hardware interface <- done/wait
     * add additional instructions; additional partial moves and other things <-done/wait
     * additional peripherals: radar altitudimeter, target relative position etc. <- done/wait
     * overhaul cache memory <- done
     * improve keyboard interface <- done?
     * make sure "interrupts enabled" and "interrupt processing" are separated <- done?
     * fix weirdness with gui objects <- wait
     * screen drawing interrupts <- wait
     * text offset registers <- wait
     * more comparison instructions <- wait
     * memory management <- wait
     */
    public class ProgCom : PartModule
    {
        private CPUem CPU;
        private bool running = false;
        Assembler2 assembler;
        private bool partActive = false;
        private bool partGUI;
        bool debug;
        TapeDrive tapeDrive;

        /************************************************************************************************************************/
        //helper functions
        /***********************************************************************************************************************/
        private bool shipHasOtherActiveComputer()
        {
            foreach (Part p in this.vessel.Parts) {
               foreach(PartModule pm in p.Modules) {
                   if (pm is ProgCom) {
                       if (((ProgCom)pm).partActive && pm != this) {
                           return true;
                       }
                   }
                }
            }
            return false;
        }
        
        /*
            mem[4] = (Int32)(eastUnit.x * accuracy);
            mem[5] = (Int32)(eastUnit.y * accuracy);
            mem[6] = (Int32)(eastUnit.z * accuracy);
            mem[7] = (Int32)(upUnit.x * accuracy);
            mem[8] = (Int32)(upUnit.y * accuracy);
            mem[9] = (Int32)(upUnit.z * accuracy);
            mem[10] = (Int32)(northUnit.x * accuracy);
            mem[11] = (Int32)(northUnit.y * accuracy);
            mem[12] = (Int32)(northUnit.z * accuracy);
            mem[13] = (Int32)(shipEastVector.x * accuracy);
            mem[14] = (Int32)(shipEastVector.y * accuracy);
            mem[15] = (Int32)(shipEastVector.z * accuracy);
            mem[16] = (Int32)(shipUpVector.x * accuracy);
            mem[17] = (Int32)(shipUpVector.y * accuracy);
            mem[18] = (Int32)(shipUpVector.z * accuracy);
            mem[19] = (Int32)(shipNorthVector.x * accuracy);
            mem[20] = (Int32)(shipNorthVector.y * accuracy);
            mem[21] = (Int32)(shipNorthVector.z * accuracy);
            mem[22] = (Int32)(orbitalSpeed.x * speedAccuracy);
            mem[23] = (Int32)(orbitalSpeed.y * speedAccuracy);
            mem[24] = (Int32)(orbitalSpeed.z * speedAccuracy);
            mem[25] = (Int32)(groundSpeed.x * speedAccuracy);
            mem[26] = (Int32)(groundSpeed.y * speedAccuracy);
            mem[27] = (Int32)(groundSpeed.z * speedAccuracy);
            mem[28] = (Int32)(angularVelocity.x * speedAccuracy);
            mem[29] = (Int32)(angularVelocity.y * speedAccuracy);
            mem[30] = (Int32)(angularVelocity.z * speedAccuracy);
            mem[31] = (Int32)(UInt32)(altitude);
            //mem[32] = output 1
            //mem[33] = output 2
            //mem[34] = output 3
            //mem[35] = output 4
            //mem[36] = output msg
            //mem[37] = numpad input
            //mem[38] = boolean used to see if the input has changed
            //mem[39] = output formatting switch
            //mem[40] = timer
            //mem[41] = vectorAccuracy
            //mem[42] = screen mode
            //mem[43] = speed accuracy
            //mem[44] = interrupt enable
            //mem[45] = clock
            //mem[46] = interrupt handler adress (64 default)
            //mem[47] = timer interrupt frequency
            //mem[48] = player throttle
            //mem[49] = player yaw
            //mem[50] = player pitch
            //mem[51] = player roll
            //mem[52] = player trn up
            //mem[53] = player trn east
            //mem[54] = player trn forward
            //mem[55] = toggle Actiongroups
        */
        //runs through the instructions untill the necessary ammount of ticks has passed, or all programs have completed operation
        //returns the excess ticks
        private int cycle(int cycles)
        {
            int ticksElapsed = 0;
            while (ticksElapsed < cycles) {
                //debugging tools
                if (debug) {
                    int opcode = (CPU.getMem(CPU.PC) >> 26)&0x3f;//decompile instruction
                    int rA = (CPU.getMem(CPU.PC) >> 21) & 0x1f;
                    int rB = (CPU.getMem(CPU.PC) >> 16) & 0x1f;
                    int im = CPU.getMem(CPU.PC) & 0xffff;

                    print("PC: " + CPU.PC + " opcode: " + opcode.ToString("X2") + " r" + rA + ": " + CPU.Registers[rA] + " r" + rB + ": " + CPU.Registers[rB] + " im: " + im + " rC: " + CPU.Registers[im&0x1f] + " ra: " + CPU.Registers[15] + " sp: " + CPU.Registers[14] + ", " + CPU.getMem((UInt16)(CPU.Registers[14] - 1)));
                    print(tapeDrive.statusString());
                }
                
                int cyclesElapsed = CPU.tick();
                /*if (cyclesElapsed == 0) {//remove this sometime
                    running = false;
                    //stop the program
                    break;
                } else if (cyclesElapsed == -1) {
                    //stop the program
                    consoleWrite("Illegal instruction(" + CPU.Memory[CPU.PC - 1] + ") at address " + (CPU.PC - 1));
                    print("Illegal instruction(" + CPU.Memory[CPU.PC - 1] + ") at address " + (CPU.PC - 1));
                    running = false;
                    break;
                }*/
                ticksElapsed += cyclesElapsed;
            }
            return ticksElapsed;
        }

        private Int32[] loadAndMakeTape(String s, int loadLocation)
        {
            Int32[] code = assembler.assemble(s, loadLocation);
            Int32[] newCode = new Int32[1024 * 256];
            newCode[1] = 4711;
            newCode[3] = 1024;
            newCode[1024] = loadLocation;
            newCode[1025] = code.Length;
            int j = 0;
            foreach (Int32 i in code) {
                newCode[j + 1026] = i;
                ++j;
            }
            return newCode;
        }

        private int load(String s, bool loadTape, int loadLocation)
        {
            //what should I put into this iinterface?
            //we need to be able to do the following with programs:
            //1:    read assembly code, assemble it, and put the result in the tape drive <- this file
            //2:    read assembly code, assemble it, and save the tape with a filename
            //3:    

            consoleWrite("Loading file: \"" + s + "\"...");
            //assemble a program
            Int32[] tape = loadAndMakeTape(s, loadLocation);
            consoleWrite("Program assembled!");
            //save tape
            tapeDrive.saveTape(Util.cutStrAfter(s, ".") + ".pct", tape);
            consoleWrite("Tape created.");
            //load program into memory
            if (loadTape) {
                tapeDrive.insertMedia(tape);
                consoleWrite("Tape inserted.");
            }

            consoleWrite("Done");
            return 0;
        }

        protected int loadWrapper(String s, bool loadTape, int loadLocation)
        {
            try {
                return load(s, loadTape, loadLocation);
            }
            catch (FormatException f) {
                print(f.Message);
                consoleWrite("Compilation failed.");
                consoleWrite("Press Alt-F2 for details");
                running = false;
                return -1;
            }
            catch (Exception e) {
                consoleWrite("unexpected exception!");
                consoleWrite("Press Alt-F2 for details");
                print(e);
                running = false;
                return -1;
            }
        }


        /***************************************************************************************************************************/
        //below this point, various override methods for part are declared.
        //gui and so on is handled
        /***************************************************************************************************************************/
        protected Rect windowPos;
        protected Rect consolePos;
        protected Rect numpadPos;

        protected void onFlightStart()
        {
            //called at load at launchpad
            if (!shipHasOtherActiveComputer()) {
                print("ProgCom initialised!");
                partActive = true;
                
                //add the gui iff the vessel is the active vessel.
                if (this.vessel.isActiveVessel) {
                    partGUI = true;
                    RenderingManager.AddToPostDrawQueue(3, new Callback(drawGUI));//start the GUI
                }
            }
        }
        /*
         * are these really not needed anymore?
        protected override void onPartDestroy()
        {
            if (partActive) {
                vessel.OnFlyByWire -= new FlightInputCallback(performManouvers);

                //remove manouver-stuff when the part is destroyed
                if (this.vessel.isActiveVessel) {
                    RenderingManager.RemoveFromPostDrawQueue(3, new Callback(drawGUI)); //close the GUI
                    RenderingManager.RemoveFromPostDrawQueue(3, new Callback(monitor.draw));
                    RenderingManager.RemoveFromPostDrawQueue(3, new Callback(keyb.draw));//start the keyboard
                }
            }
        }

        protected override void onDisconnect()
        {
            //remove manouver sFlyByWire stuff when the part is disconnected
            if (this.vessel.isActiveVessel && partActive) {
                vessel.OnFlyByWire -= new FlightInputCallback(performManouvers);//this is in here to avoid potential nastiness related to removing nonexistant FlightInputCallbacks.
                RenderingManager.RemoveFromPostDrawQueue(3, new Callback(drawGUI)); //close the GUI
                RenderingManager.RemoveFromPostDrawQueue(3, new Callback(monitor.draw));
                RenderingManager.RemoveFromPostDrawQueue(3, new Callback(keyb.draw));//start the keyboard
            }
        }
        */

        private void init()
        {
            print("PROGCOM INITIALISING");
            //initialise cpu-emulator
            //lastLoaded = 128;
            CPU = new CPUem();

            windowID = Util.random();

            if (tapeDrive == null) {
                tapeDrive = new TapeDrive();
                //insert a blank tape
                Int32[] media = new Int32[256 * 1024];
                media[1] = 4711;
                tapeDrive.insertMedia(media);
            }
            //connect various peripherals to the CPU
            //when possible, separate these out as normal partmodules

            //connect tapedrive. When possible, add tape drive to appropriate stuff
            CPU.hwConnect(tapeDrive);

            running = false;
            
            //connect all hardware on this part
            foreach (PartModule pm in this.part.Modules) {
                if (pm == this) continue;
                if (pm is IPCHardware) {
                    print("HARDWARE IS FOUND");
                    try {
                        CPU.hwConnect((IPCHardware)pm);
                    }
                    catch (ArgumentException e) {
                        consoleWrite("Error when connecting hardware:");
                        consoleWrite(e.Message);
                    }
                }
            }

            //make sure all partmodules can read the gui state
            foreach (PartModule pm in this.part.Modules) {
                if (pm == this) continue;
                if (pm is PCGUIListener) {
                    print("GUIListener found!");
                    try {
                        ((PCGUIListener)pm).recGUIState(GUIstate);
                    }
                    catch (Exception e) {
                        consoleWrite("Error when connecting gui:");
                        consoleWrite(e.Message);
                    }
                }
            }

            //initialise the assembler
            assembler = new Assembler2();
        }

        //rect initialized here
        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            if(state.Equals(PartModule.StartState.Editor)) return;//don't start anything in the editor
            print("initialising progCom..."+state.ToString());
            if ((windowPos.x == 0) && (windowPos.y == 0))//windowPos is used to position the GUI window, lets set it in the center of the screen
            {
                windowPos = new Rect(Screen.width / 2, Screen.height / 2, 100, 100);
            }
            init();
            //vessel.OnFlyByWire += new FlightInputCallback(performManouvers);
            //RenderingManager.AddToPostDrawQueue(3, new Callback(drawGUI));//start the GUI
            onFlightStart();
            print("Hello, World!");
            consoleWrite("Computer online!");
        }

        
        private int cyclesPending = 0;
        //perform CPU cycling and the like here
        public override void OnUpdate()
        {
            if (CPU.hasErrors) {
                CPU.hasErrors = false;
                foreach (String s in CPU.errorMessages) {
                    print(s);
                    CPU.errorMessages.Clear();
                }
            }

            base.OnUpdate();

            //we must make sure that, if the vessel is gains or loses focus, that the gui responds accordingly
            if (partGUI == true && !vessel.isActiveVessel) {
                partGUI = false;

                RenderingManager.RemoveFromPostDrawQueue(3, new Callback(drawGUI)); //close the GUI
            } else if (partGUI == false && vessel.isActiveVessel && !shipHasOtherActiveComputer()) {
                partGUI = true;

                RenderingManager.AddToPostDrawQueue(3, new Callback(drawGUI));//start the GUI
            }
            //handle other computers suddenly appearing on the ship, such as when docking
            if (partActive && shipHasOtherActiveComputer()) {
                partActive = false;
                GUIstate.ctl = false;
                GUIstate.ttl = false;
            } else if (!partActive && !shipHasOtherActiveComputer()) {
                partActive = true;
            }


            //update computer and flight data
            if (running) {
                //updateFlightVariables();//make sure all the flight data is the most recent.
                //handle exceptions for hardware that is not handled internally in the CPU
                float time = UnityEngine.Time.deltaTime;
                cyclesPending += debug ? 1 : (int)(time * CPU.ClockRate);
                if (cyclesPending > CPU.ClockRate) {
                    cyclesPending = CPU.ClockRate;//to prevent lockup bugs due to ever-esclalating clock-cycles per update
                }
                cyclesPending -= cycle(cyclesPending);//Run as many cycles as we can, while correcting for instructions that might run for longer than allowed
            }
        }


        /***********************' GUI code here ****************************/

        //bool showGUI = true;
        GUIStates GUIstate = new GUIStates();
        int numpadInput = 0;
        String currentText = "";
        int consoleMax = -1;
        String[] consoleText = new String[16];
        int windowID;
        //handles how the window looks
        private void WindowGUI(int windowID)
        {
            //normal gui style
            GUIStyle mySty = new GUIStyle(GUI.skin.button);
            mySty.normal.textColor = mySty.focused.textColor = Color.white;
            mySty.hover.textColor = mySty.active.textColor = Color.yellow;
            mySty.onNormal.textColor = mySty.onFocused.textColor = mySty.onHover.textColor = mySty.onActive.textColor = Color.green;
            mySty.padding = new RectOffset(8, 8, 8, 8);

            //gui style when off
            GUIStyle offSty = new GUIStyle(GUI.skin.button);
            offSty.normal.textColor = offSty.focused.textColor = Color.grey;
            offSty.hover.textColor = offSty.active.textColor = Color.grey;
            offSty.onNormal.textColor = offSty.onFocused.textColor = offSty.onHover.textColor = offSty.onActive.textColor = Color.green;
            offSty.padding = new RectOffset(8, 8, 8, 8);

            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("ON/OFF", running ? mySty : offSty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
                {
                running = !running;//toggle on/off
            }
            if (GUILayout.Button("CTL", GUIstate.ctl ? mySty : offSty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
                {
                GUIstate.ctl = !GUIstate.ctl;//toggle on/off
            }
            if (GUILayout.Button("TTL", GUIstate.ttl ? mySty : offSty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
                {
                GUIstate.ttl = !GUIstate.ttl;//toggle on/off
            }
            if (GUILayout.Button("RST", mySty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
                {
                init();
            }
            if (GUILayout.Button("GUI", GUIstate.gui ? mySty : offSty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
                {
                GUIstate.gui = !GUIstate.gui;//toggle on/off
                windowPos.width = 0;
                windowPos.height = 0;
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            if (GUIstate.gui) {
                //second row of buttons
                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("MON", GUIstate.mon ? mySty : offSty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
                    {
                    GUIstate.mon = !GUIstate.mon;//toggle on/off
                }
                if (GUILayout.Button("KBD", GUIstate.kbd ? mySty : offSty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
                    {
                    GUIstate.kbd = !GUIstate.kbd;//toggle on/off
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();

                /*************************************************************************************************************/
                //try constructing something somewhat numpad-like
                GUILayout.BeginVertical();//I/O area
                GUILayout.BeginHorizontal();//for horisontally layering stuff
                GUILayout.BeginVertical();//Input panel

                GUILayout.BeginHorizontal();//draw numpad number display
                GUILayout.TextArea("" + numpadInput, GUILayout.MinWidth(40.0f));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();//upper numpad numbers
                if (GUILayout.Button("7", mySty, GUILayout.ExpandWidth(false)))//GUILayout.Button is "true" when clicked
                        {
                    numpadInput *= 10;
                    numpadInput += 7;
                }
                if (GUILayout.Button("8", mySty, GUILayout.ExpandWidth(false)))//GUILayout.Button is "true" when clicked
                        {
                    numpadInput *= 10;
                    numpadInput += 8;
                }
                if (GUILayout.Button("9", mySty, GUILayout.ExpandWidth(false)))//GUILayout.Button is "true" when clicked
                        {
                    numpadInput *= 10;
                    numpadInput += 9;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();//mid numpad numbers
                if (GUILayout.Button("4", mySty, GUILayout.ExpandWidth(false)))//GUILayout.Button is "true" when clicked
                        {
                    numpadInput *= 10;
                    numpadInput += 4;
                }
                if (GUILayout.Button("5", mySty, GUILayout.ExpandWidth(false)))//GUILayout.Button is "true" when clicked
                        {
                    numpadInput *= 10;
                    numpadInput += 5;
                }
                if (GUILayout.Button("6", mySty, GUILayout.ExpandWidth(false)))//GUILayout.Button is "true" when clicked
                        {
                    numpadInput *= 10;
                    numpadInput += 6;
                }
                if (GUILayout.Button("C", mySty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
                        {
                    numpadInput = 0;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();//low numpad numbers
                if (GUILayout.Button("1", mySty, GUILayout.ExpandWidth(false)))//GUILayout.Button is "true" when clicked
                        {
                    numpadInput *= 10;
                    numpadInput += 1;
                }
                if (GUILayout.Button("2", mySty, GUILayout.ExpandWidth(false)))//GUILayout.Button is "true" when clicked
                        {
                    numpadInput *= 10;
                    numpadInput += 2;
                }
                if (GUILayout.Button("3", mySty, GUILayout.ExpandWidth(false)))//GUILayout.Button is "true" when clicked
                        {
                    numpadInput *= 10;
                    numpadInput += 3;
                }
                if (GUILayout.Button("-", mySty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
                        {
                    if (numpadInput > 0) {
                        numpadInput *= -1;
                    }
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();//numpad 0/enter
                if (GUILayout.Button("0", mySty, GUILayout.ExpandWidth(false)))//GUILayout.Button is "true" when clicked
                        {
                    numpadInput *= 10;
                }
                if (GUILayout.Button("Enter", mySty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
                        {
                    CPU.Memory[37] = numpadInput;
                    numpadInput = 0;
                    CPU.Memory[38] = 1;
                }
                if (GUILayout.Button("+", mySty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
                        {
                    if (numpadInput < 0) {
                        numpadInput *= -1;
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();//input panel

                GUILayout.BeginVertical();//output numbars
                GUILayout.TextArea("0: " + memWrFormat(32, 0), GUILayout.MinWidth(130.0f));
                GUILayout.TextArea("1: " + memWrFormat(33, 1), GUILayout.MinWidth(130.0f));
                GUILayout.TextArea("2: " + memWrFormat(34, 2), GUILayout.MinWidth(130.0f));
                GUILayout.TextArea("3: " + memWrFormat(35, 3), GUILayout.MinWidth(130.0f));
                GUILayout.TextArea("MSG: " + memWrFormat(36, 4), GUILayout.MinWidth(130.0f));
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();//horisontal layering
                //console here:
                //text area
                GUILayout.TextField(getConsoleOutString(consoleText), GUILayout.MinWidth(30.0F));
                //input line
                if (Event.current.type == EventType.KeyDown) {
                    if (Event.current.keyCode == KeyCode.Return) {
                        consoleWrite(currentText);
                        parseInput(currentText);
                        currentText = "";//add reacting to inputed text later
                        Event.current.Use();
                    }
                }
                currentText = GUILayout.TextField(currentText, GUILayout.MinWidth(30.0F)); //you can play with the width of the text box
                GUILayout.EndVertical();//I/O area
            }

            //DragWindow makes the window draggable. The Rect specifies which part of the window it can by dragged by, and is 
            //clipped to the actual boundary of the window. You can also pass no argument at all and then the window can by
            //dragged by any part of it. Make sure the DragWindow command is AFTER all your other GUI input stuff, or else
            //it may "cover up" your controls and make them stop responding to the mouse.
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        protected void drawGUI()
        {
            GUI.skin = HighLogic.Skin;
            windowPos = GUILayout.Window(windowID, windowPos, WindowGUI, "Computer Control Panel", GUILayout.MinWidth(100));
        }

        private String memWrFormat(int i, int pos)
        {
            if ((CPU.Memory[39] & (0x3 << (pos << 1))) == 0)
                return "" + CPU.Memory[i];
            else return "" + (float)Util.itof(CPU.Memory[i]);
        }

        private void parseInput(String s)
        {
            UInt16 tmp;
            String[] S = s.Split(' ');
            switch (S.Length) {
                case 1:
                    if (S[0].Equals("clear")) {
                        clearConsole();
                    } else if (S[0].Equals("step")) {
                        cycle(1);
                    } else if (S[0].Equals("debug")) {
                        debug = !debug;
                    } else consoleWrite("Couldn't parse");
                    break;
                case 2:
                    if(S[0].Equals("insert")) {
                        if(S[1].Equals("empty")) {
                            Int32[] media = new Int32[1024 * 256];
                            media[1] = 4711;
                            tapeDrive.insertMedia(media);
                        } else if(S[1].Contains('.')) {
                            try {
                                tapeDrive.insertMedia(tapeDrive.loadTape(S[1]));
                                consoleWrite("Tape inserted!");
                            }
                            catch (Exception) {
                                consoleWrite("Operation failed!");
                            }
                        }
                    } else if(S[0].Equals("asm")) {
                        loadWrapper(S[1], false, 256);
                    } else if (S[0].Equals("eject")) {
                        if (!S[1].Equals("null")) {//If this is the case, just throw away the tape (and insert an empty one?)
                            //save the tape (and insert empty?)
                            if (S[1].Contains('.')) {
                                tapeDrive.saveTapeInternal(S[1]);
                            } else {
                                tapeDrive.saveTapeInternal(S[1]+".pct");
                            }
                        }
                        Int32[] media = new Int32[1024 * 256];//this should possibly be null instead
                        media[1] = 4711;
                        tapeDrive.insertMedia(media);

                    } else if (S[0].Equals("save")) {
                        if (!S[1].Equals("null")) {//If this is the case, just throw away the tape (and insert an empty one?)
                            //save the tape (and insert empty?)
                            if (S[1].Contains('.')) {
                                tapeDrive.saveTapeInternal(S[1]);
                            } else {
                                tapeDrive.saveTapeInternal(S[1] + ".pct");
                            }
                        }
                    } else if (S[0].Equals("print")) {
                        if (Util.tryParseTo<UInt16>(S[1], out tmp)) {
                            for (int i = 0; i < 100; ++i) {
                                print(CPU.getMem((UInt16)(i + tmp)));
                            }
                        } else {
                            consoleWrite("not an address: " + S[1]);
                        }
                    } else consoleWrite("Couldn't parse");
                    break;
                case 3:
                    if(S[0].Equals("insert")) {
                        if(S[1].Equals("asm")) {
                            //assemble some code and then stuff
                            loadWrapper(S[2], true, 256);
                        }
                    } else consoleWrite("Couldn't parse.");
                    break;
                case 4:
                    if(S[0].Equals("insert")) {
                        if(S[1].Equals("asm")) {
                            if (Util.tryParseTo<UInt16>(S[3], out tmp)) {
                                //assemble some code and then stuff
                                loadWrapper(S[2], true, tmp);
                            } else {
                                consoleWrite("Not A Number: " + S[3]);
                            }
                        }
                    } else consoleWrite("Couldn't parse.");
                    break;

            }
        }

        /** Code for manipulating the console goes here **/

        private String getConsoleOutString(String[] S)
        {
            String outStr = "";
            if (consoleMax == -1)//no commands in window
                return outStr;
            outStr = S[0];
            for (int i = 1; i < consoleMax; ++i) {
                outStr = S[i] + '\n' + outStr;
            }
            return outStr;
        }

        private void consoleWrite(String S)
        {
            ++consoleMax;
            if (consoleMax == 16)
                consoleMax = 15;
            String Next = S;
            for (int i = 0; i <= consoleMax; ++i) {
                String tmp = consoleText[i];
                consoleText[i] = Next;
                Next = tmp;
            }
        }

        private void clearConsole()
        {
            consoleMax = -1;
            windowPos.width = 0;
            windowPos.height = 0;
        }

    }
}