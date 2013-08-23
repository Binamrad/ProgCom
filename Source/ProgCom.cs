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
     * rethink how program loading should be done <- boot from tape. Fix if complaints
     * insert boot sequence upon init <- done
     * insert assembled program into tape <- done
     * #literal <- done?
     * fix bit-shift EX zero if shifted 0, full to-EX copy otherwise <- done
     * add possibility to specify load location for programs when assembled <- done
     * Finally update to goddamn partModule so that we can add the mod normally
     * link to 0.21.1
     * rle-compress tapes
     * overhaul cache memory <- postpone
     * add additional instructions; additional partial moves and cache bypass <-postpone
     * fix window ids <- done?
     * improve interrupts <- done
     * make sure TapeDrive respects canread/canwrite flags <- done
     * text offset registers <- postpone
     * documentation
     */
    public class ProgCom : PartModule, ISerialConnector
    {
        private CPUem CPU;
        //int lastLoaded = 128;//the earliest memory sequence anything can be safely loaded to
        private bool controlling = false;
        bool ttlControl = false;
        private bool running = false;
        Assembler2 assembler;
        private bool partGUI = false;
        private bool partActive = false;
        Monitor monitor;
        bool debug;
        Keyboard keyb;
        TapeDrive tapeDrive;

        /************************************************************************************************************************/
        //helper functions
        /***********************************************************************************************************************/
        private bool shipHasOtherActiveComputer()
        {
            foreach (Part p in this.vessel.Parts) {
               foreach(PartModule pm in p.Modules) {
                   if (pm is ProgCom) {
                       if (((ProgCom)pm).partActive && p != this) {
                           return true;
                       }
                   }
                }
            }
            return false;
        }

        private void updateFlightVariables()
        {
            //update the variables which describe the vessels location

            //position of the ship's center of mass:
            Vector3d position = vessel.findWorldCenterOfMass();//somewhat useless for anything other than as a parameter
            double altitude = vessel.mainBody.GetAltitude(position);

            //unit vectors in the up (normal to planet surface), east, and north (parallel to planet surface) directions
            Vector3d eastUnit = vessel.mainBody.getRFrmVel(position).normalized; //uses the rotation of the body's frame to determine "east"
            Vector3d upUnit = (position - vessel.mainBody.position).normalized;
            Vector3d northUnit = Vector3d.Cross(upUnit, eastUnit); //north = up cross east

            //vessel speeds
            Vector3d orbitalSpeed = vessel.orbit.GetVel();
            Vector3d groundSpeed = orbitalSpeed - vessel.mainBody.getRFrmVel(position);

            //local acceleration vector due to gravity:
            Vector3d geeAcceleration = FlightGlobals.getGeeForceAtPosition(position);

            //find angular velocity (roll rate, yaw rate, pitch rate)
            Vector3d angularVelocity = (Vector3d)vessel.transform.InverseTransformDirection(vessel.rigidbody.angularVelocity);

            //find ship coordinate system
            UnityEngine.Vector3 tmpVector = new UnityEngine.Vector3(1, 0, 0);
            Vector3d shipEastVector = (Vector3d)vessel.transform.TransformDirection(tmpVector);
            tmpVector.x = 0;
            tmpVector.y = 1;
            Vector3d shipUpVector = (Vector3d)vessel.transform.TransformDirection(tmpVector);
            tmpVector.y = 0;
            tmpVector.z = 1;
            Vector3d shipNorthVector = -(Vector3d)vessel.transform.TransformDirection(tmpVector);
            //figure out what these newfangled vector3d doodads do
            /*
            print("***************************************************");
            print("geeAccelleration: " + (UnityEngine.Vector3)geeAcceleration);
            print("altitude: " + altitude);
            print("eastUnit: " + (UnityEngine.Vector3)eastUnit);
            print("upUnit: " + (UnityEngine.Vector3)upUnit);
            print("northUnit: " + (UnityEngine.Vector3)northUnit);
            print("orbitalSpeed: " + (UnityEngine.Vector3)orbitalSpeed);
            print("groundSpeed: " + (UnityEngine.Vector3)groundSpeed);
            print("heading: " + (UnityEngine.Vector3)shipUpVector);
            print("angularveclocity: " + (UnityEngine.Vector3)angularVelocity);
            print("shipEast: " + (UnityEngine.Vector3)shipEastVector);
            print("shipUp: " + (UnityEngine.Vector3)shipUpVector);
            print("shipNorth: " + (UnityEngine.Vector3)shipNorthVector);
            print("***************************************************");
            */
            Int32[] mem = CPU.Memory;
            int accuracy = mem[41];//vector accuracy
            float speedAccuracy = mem[43] >= 0 ? (float)mem[43] : 1.0f/((float)-mem[43]);//speed accuracy
            //int speedAccuracy = 16;//old speedaccuracy model

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
            //mem[42] = thread id
            //mem[43] = program offset
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
        }

        //runs through the instructions untill the necessary ammount of ticks has passed, or all programs have completed operation
        //returns the excess ticks
        private int cycle(int cycles)
        {
            int ticksElapsed = 0;
            while (ticksElapsed < cycles) {
                //debugging tools
                if (debug) {
                    print("PC: " + CPU.PC + "current instruction: " + CPU.Memory[CPU.PC] + " ra: " + CPU.Registers[15] + " sp: " + CPU.Registers[14] + ", " + CPU.Memory[(UInt16)(CPU.Registers[14] - 1)]);
                    print(tapeDrive.getStatus());
                    print("r1: " + CPU.Registers[1]);
                }
                //print("PC: " + CPU.PC + "current instruction: " + CPU.Memory[CPU.PC] + " EX: " + CPU.getEX());
                int cyclesElapsed = CPU.tick();
                if (cyclesElapsed == 0) {
                    running = false;
                    //stop the program
                    break;
                } else if (cyclesElapsed == -1) {
                    //stop the program
                    consoleWrite("Illegal instruction(" + CPU.Memory[CPU.PC - 1] + ") at address " + (CPU.PC - 1));
                    print("Illegal instruction(" + CPU.Memory[CPU.PC - 1] + ") at address " + (CPU.PC - 1));
                    running = false;
                    break;
                }
                ticksElapsed += cyclesElapsed;
            }
            return ticksElapsed;
        }

        private Int32[] loadAndMakeTape(String s, int loadLocation)
        {
            Int32[] code = assembler.assemble(s, loadLocation);
            Int32[] newCode = new Int32[1024 * 256];
            newCode[1] = 4711;
            int j = 0;
            foreach (Int32 i in code) {
                newCode[j + 3] = i;
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
            //load program into memory
            if (loadTape) {
                tapeDrive.insertMedia(tape);
                consoleWrite("Tape inserted.");
            } else {
                //write to a file
                tapeDrive.saveTape(Util.cutStrAfter(s, ".") + ".pct", tape);
                consoleWrite("Tape created.");
            }

            consoleWrite("Done");
            return 0;
            /*
            consoleWrite("Loading file: \"" + s + "\"...");
            //assemble a program
            Int32[] newCode = assembler.assemble(s, lastLoaded);
            int loadLocation = lastLoaded;
            consoleWrite("Program assembled!");
            //load program into memory
            consoleWrite("writing...");
            for (int i = 0; i < newCode.Length; ++i) {
                if (lastLoaded > 65535) {
                    consoleWrite("ERROR: program loader wrote out of bounds");
                    lastLoaded = loadLocation;
                    throw new OutOfMemoryException("Loader wrote out of bounds");
                }
                CPU.Memory[lastLoaded++] = newCode[i];
            }
            consoleWrite("Program loaded at " + loadLocation);

            consoleWrite("Done");
            return loadLocation;
             * */
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

        protected void activateActionGroup(int i)
        {
            ActionGroupList a = this.vessel.ActionGroups;
            switch (i) {
                case 1:
                    a.ToggleGroup(KSPActionGroup.Custom01);
                    break;
                case 2:
                    a.ToggleGroup(KSPActionGroup.Custom02);
                    break;
                case 3:
                    a.ToggleGroup(KSPActionGroup.Custom03);
                    break;
                case 4:
                    a.ToggleGroup(KSPActionGroup.Custom04);
                    break;
                case 5:
                    a.ToggleGroup(KSPActionGroup.Custom05);
                    break;
                case 6:
                    a.ToggleGroup(KSPActionGroup.Custom06);
                    break;
                case 7:
                    a.ToggleGroup(KSPActionGroup.Custom07);
                    break;
                case 8:
                    a.ToggleGroup(KSPActionGroup.Custom08);
                    break;
                case 9:
                    a.ToggleGroup(KSPActionGroup.Custom09);
                    break;
                case 10:
                    a.ToggleGroup(KSPActionGroup.Custom10);
                    break;
                case 11:
                    a.ToggleGroup(KSPActionGroup.Abort);
                    break;
                case 12:
                    if (Staging.CurrentStage > 0) {
                        Staging.ActivateNextStage();
                    }
                    break;
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
                vessel.OnFlyByWire += new FlightInputCallback(performManouvers);
                //add the gui iff the vessel is the active vessel.
                if (this.vessel.isActiveVessel) {
                    partGUI = true;
                    RenderingManager.AddToPostDrawQueue(3, new Callback(monitor.draw));
                    RenderingManager.AddToPostDrawQueue(3, new Callback(drawGUI));//start the GUI
                    RenderingManager.AddToPostDrawQueue(3, new Callback(keyb.draw));//start the keyboard
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
        private void removeExternalGUI()
        {
            if (partActive) {
                //remove manouver-stuff when the part is destroyed
                if (this.vessel.isActiveVessel) {
                    if (monitor != null) {
                        RenderingManager.RemoveFromPostDrawQueue(3, new Callback(monitor.draw));
                        RenderingManager.RemoveFromPostDrawQueue(3, new Callback(keyb.draw));//start the keyboard
                    }
                }
            }
        }
        private void initExternalGUI()
        {
            if (partActive) {
                //remove manouver-stuff when the part is destroyed
                if (this.vessel.isActiveVessel) {
                    if (monitor != null) {
                        RenderingManager.AddToPostDrawQueue(3, new Callback(monitor.draw));
                        RenderingManager.AddToPostDrawQueue(3, new Callback(keyb.draw));//start the keyboard
                    }
                }
            }
        }

        private void init()
        {
            //initialise cpu-emulator
            //lastLoaded = 128;
            CPU = new CPUem();

            windowID = Util.random();
            removeExternalGUI();
            monitor = new Monitor(CPU.Memory, 63488, 65024, 63472, 42);
            keyb = new Keyboard();
            initExternalGUI();

            if (tapeDrive == null) {
                tapeDrive = new TapeDrive(CPU.ClockRate);
                //insert a blank tape
                Int32[] media = new Int32[256 * 1024];
                media[1] = 4711;
                tapeDrive.insertMedia(media);
            }

            //connect keyboard to serial port
            connect(keyb, 1);
            //connect tapedrive to serial port
            connect(tapeDrive, 0);

            
            running = false;
            //init default values in memory
            CPU.Memory[41] = 1024;//init vector precision
            CPU.Memory[43] = 16;//default speed precision
            int i = 65024;
            foreach (UInt32 font in monitor.getDefaultFont()) {
                CPU.Memory[i] = (Int32)font;
                ++i;
            }
            i = 63472;
            foreach (Int32 col in monitor.getDefaultColors()) {
                CPU.Memory[i] = col;
                ++i;
            }
            //boot loader
            Int32[] bootldr = {
                                  -1073741824,
                                  -1073741824,
                                  -532676599,
                                  -1207959536,
                                  -1207959531,
                                  -1608450052,
                                  -532675839,
                                  -1207959540,
                                  -532671486,
                                  -1207959542,
                                  608174100,
                                  -1207959538,
                                  -1541406717,
                                  -532676605,
                                  -1207959547,
                                  -1610547214,
                                  -333315948,
                                  541196289,
                                  -1405026312,
                                  -1610612724,
                                  -400555968,
                                  807469096,
                                  606142496,
                                  -1543372804,
                                  -331349950,
                                  -2013265905,
                                  -400555968,
                                  807469060,
                                  -1608450051,
                                  -400555967,
                                  -335544256,
                                  -2013265905
                              };
            for (int ldr = 0; ldr < bootldr.Length; ++ldr) {
                CPU.Memory[ldr + 96] = bootldr[ldr];
            }


            //initialise the assembler
            assembler = CPU.getCompatibleAssembler();
            assembler.bindGlobalCall("GLOBAL_MAINTHROTTLE", 0);//init all globaly shared memory positions
            assembler.bindGlobalCall("GLOBAL_YAW", 1);
            assembler.bindGlobalCall("GLOBAL_PITCH", 2);
            assembler.bindGlobalCall("GLOBAL_ROLL", 3);
            assembler.bindGlobalCall("GLOBAL_SURFACE_EAST", 4);
            assembler.bindGlobalCall("GLOBAL_SURFACE_UP", 7);
            assembler.bindGlobalCall("GLOBAL_SURFACE_NORTH", 10);
            assembler.bindGlobalCall("GLOBAL_VESSEL_X", 13);
            assembler.bindGlobalCall("GLOBAL_VESSEL_Y", 16);
            assembler.bindGlobalCall("GLOBAL_VESSEL_HEADING", 16);//ALTERNATE
            assembler.bindGlobalCall("GLOBAL_VESSEL_Z", 19);
            assembler.bindGlobalCall("GLOBAL_ORBITSPEED", 22);
            assembler.bindGlobalCall("GLOBAL_SURFACESPEED", 25);
            assembler.bindGlobalCall("GLOBAL_ANGULARVELOCITY", 28);
            assembler.bindGlobalCall("GLOBAL_ALTITUDE", 31);
            assembler.bindGlobalCall("GLOBAL_NUMPAD_OUT", 32);
            assembler.bindGlobalCall("GLOBAL_NUMPAD_MSG", 36);
            assembler.bindGlobalCall("GLOBAL_NUMPAD_IN", 37);
            assembler.bindGlobalCall("GLOBAL_NUMPAD_NEWIN", 38);
            assembler.bindGlobalCall("GLOBAL_NUMPAD_FORMAT", 39);
            assembler.bindGlobalCall("GLOBAL_TIMER", 40);
            assembler.bindGlobalCall("GLOBAL_VECTORACCURACY", 41);
            assembler.bindGlobalCall("GLOBAL_SCREEN_MODE", 42);
            assembler.bindGlobalCall("GLOBAL_SPEEDACCURACY", 43);
            assembler.bindGlobalCall("GLOBAL_IENABLE", 44);
            assembler.bindGlobalCall("GLOBAL_CLOCK", 45);
            assembler.bindGlobalCall("GLOBAL_IADRESS", 46);
            assembler.bindGlobalCall("GLOBAL_TIMER_MAX", 47);
            assembler.bindGlobalCall("GLOBAL_PILOT_THROTTLE", 48);
            assembler.bindGlobalCall("GLOBAL_PILOT_YAW", 49);
            assembler.bindGlobalCall("GLOBAL_PILOT_PITCH", 50);
            assembler.bindGlobalCall("GLOBAL_PILOT_ROLL", 51);
            assembler.bindGlobalCall("GLOBAL_PILOT_RCS_RIGHT", 52);
            assembler.bindGlobalCall("GLOBAL_PILOT_RCS_UP", 53);
            assembler.bindGlobalCall("GLOBAL_PILOT_RCS_FORWARD", 54);
            assembler.bindGlobalCall("GLOBAL_RCS_RIGHT", 52);
            assembler.bindGlobalCall("GLOBAL_RCS_UP", 53);
            assembler.bindGlobalCall("GLOBAL_RCS_FORWARD", 54);
            assembler.bindGlobalCall("GLOBAL_ACTIONGROUP", 55);

            //serial buses
            assembler.bindGlobalCall("GLOBAL_GSB0", 64);
            assembler.bindGlobalCall("GLOBAL_GSB1", 68);
            assembler.bindGlobalCall("GLOBAL_GSB2", 72);
            assembler.bindGlobalCall("GLOBAL_GSB3", 76);
            assembler.bindGlobalCall("GLOBAL_GSB4", 80);
            assembler.bindGlobalCall("GLOBAL_GSB5", 84);
            assembler.bindGlobalCall("GLOBAL_GSB6", 88);
            assembler.bindGlobalCall("GLOBAL_GSB7", 92);

            //monitor stuff
            assembler.bindGlobalCall("GLOBAL_SCREEN", 63488);
            assembler.bindGlobalCall("GLOBAL_SCREEN_COLOR", 63472);
            assembler.bindGlobalCall("GLOBAL_SCREEN_FONT", 65024);

            //other stuff
            assembler.bindGlobalCall("CPU_CLOCKRATE", CPU.ClockRate);
            assembler.bindGlobalCall("CPU_RAM", CPU.Memory.Length);
            assembler.bindGlobalCall("CPU_MAXADDRESS", CPU.Memory.Length-1);

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

        //set the ship controlls to whatever the computer has decided them to be, if the part is set to do so
        private void performManouvers(FlightCtrlState state)
        {
            //ActionGroupList a = this.vessel.ActionGroups;
            //a.SetGroup(KSPActionGroup.
            //in order to make sure we capture the player controls, we read the controls here
            //mem[48] = player throttle
            CPU.Memory[48] = (Int32)(state.mainThrottle * 1024.0);
            //mem[49] = player yaw
            CPU.Memory[49] = (Int32)(state.yaw * 1024.0);
            //mem[50] = player pitch
            CPU.Memory[50] = (Int32)(state.pitch * 1024.0);
            //mem[51] = player roll
            CPU.Memory[51] = (Int32)(state.roll * 1024.0);
            //mem[52] = player trn right
            CPU.Memory[52] = (Int32)(state.X * 1024);
            //mem[53] = player trn up
            CPU.Memory[53] = (Int32)(state.Y * 1024);
            //mem[54] = player trn forward
            CPU.Memory[54] = (Int32)(state.Z * 1024);


            if (running) {
                //set controlls to value in appropriate memory location
                float f;
                if (ttlControl) {
                    f = (float)CPU.Memory[0];
                    f /= 1024.0F;
                    state.mainThrottle = f; //set throttle to computer-specified value


                    //the range of throttle is 0.0F to 1.0F
                    //we clamp the values to make sure they are in range
                    state.mainThrottle = Mathf.Clamp(state.mainThrottle, 0.0F, +1.0F);
                }

                if (controlling) {
                    //if the user has specified it, activate a control stage
                    if (CPU.Memory[55] != 0) {
                        activateActionGroup(CPU.Memory[55]);
                        CPU.Memory[55] = 0;
                    }


                    f = (float)CPU.Memory[1];
                    f /= 1024.0F;
                    state.yaw = f; //set yaw to computer-specified value

                    f = (float)CPU.Memory[2];
                    f /= 1024.0F;
                    state.pitch = f; //set pitch to computer-specified value

                    f = (float)CPU.Memory[3];
                    f /= 1024.0F;
                    state.roll = f; //set roll to computer-specified value */

                    //add control for translation.
                    f = (float)CPU.Memory[52];
                    f /= 1024.0F;
                    state.X = f;
                    f = (float)CPU.Memory[53];
                    f /= 1024.0F;
                    state.Y = f;
                    f = (float)CPU.Memory[54];
                    f /= 1024.0F;
                    state.Z = f;


                    //the range of yaw, pitch, and roll is -1.0F to 1.0F, as are X, Y and Z
                    //we clamp the values to make sure they are in range
                    state.yaw = Mathf.Clamp(state.yaw, -1.0F, +1.0F);
                    state.pitch = Mathf.Clamp(state.pitch, -1.0F, +1.0F);
                    state.roll = Mathf.Clamp(state.roll, -1.0F, +1.0F);
                    state.X = Mathf.Clamp(state.X, -1.0F, +1.0F);
                    state.Y = Mathf.Clamp(state.Y, -1.0F, +1.0F);
                    state.Z = Mathf.Clamp(state.Z, -1.0F, +1.0F);
                }
            }
        }

        private int cyclesPending = 0;
        //perform CPU cycling and the like here
        public override void OnUpdate()
        {
            base.OnUpdate();

            //we must make sure that, if the vessel is gains or loses focus, that the gui responds accordingly
            if (partGUI == true && !vessel.isActiveVessel) {
                partGUI = false;

                RenderingManager.RemoveFromPostDrawQueue(3, new Callback(drawGUI)); //close the GUI
                RenderingManager.RemoveFromPostDrawQueue(3, new Callback(monitor.draw));
                RenderingManager.RemoveFromPostDrawQueue(3, new Callback(keyb.draw));//start the keyboard
            } else if (partGUI == false && vessel.isActiveVessel && !shipHasOtherActiveComputer()) {
                partGUI = true;

                RenderingManager.AddToPostDrawQueue(3, new Callback(drawGUI));//start the GUI
                RenderingManager.AddToPostDrawQueue(3, new Callback(monitor.draw));
                RenderingManager.AddToPostDrawQueue(3, new Callback(keyb.draw));//start the keyboard
            }
            //handle other computers suddenly appearing on the ship, such as when docking
            if (partActive && shipHasOtherActiveComputer()) {
                partActive = false;
                vessel.OnFlyByWire -= new FlightInputCallback(performManouvers);//remove autopilot stuff
            } else if (!partActive && !shipHasOtherActiveComputer()) {
                partActive = true;
                vessel.OnFlyByWire += new FlightInputCallback(performManouvers);//add autopilot stuff
            }


            //update computer and flight data
            if (running) {
                updateFlightVariables();//make sure all the flight data is the most recent.
                //handle exceptions for hardware that is not handled internally in the CPU
                if (CPU.enabledInterruptNum(2)) {
                    CPU.spawnException(257);
                }
                if (CPU.Memory[38] != 0 && CPU.enabledInterruptNum(3)) {
                    CPU.spawnException(259);
                }
                float time = UnityEngine.Time.deltaTime;
                cyclesPending += debug ? 1 : (int)(time * CPU.ClockRate);
                if (cyclesPending > CPU.ClockRate) {
                    cyclesPending = CPU.ClockRate;//to prevent lockup bugs due to ever-esclalating clock-cycles per update
                }
                cyclesPending -= cycle(cyclesPending);//Run as many cycles as we can, while correcting for instructions that might run for longer than allowed
                monitor.update();//draw the screen
            }
        }


        /***********************' GUI code here ****************************/

        bool showGUI = true;
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
            if (GUILayout.Button("CTL", controlling ? mySty : offSty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
                {
                controlling = !controlling;//toggle on/off
            }
            if (GUILayout.Button("TTL", ttlControl ? mySty : offSty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
                {
                ttlControl = !ttlControl;//toggle on/off
            }
            if (GUILayout.Button("RST", mySty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
                {
                init();
            }
            if (GUILayout.Button("GUI", showGUI ? mySty : offSty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
                {
                showGUI = !showGUI;//toggle on/off
                windowPos.width = 0;
                windowPos.height = 0;
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            if (showGUI) {
                //second row of buttons
                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("MON", monitor.visible ? mySty : offSty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
                    {
                    monitor.visible = !monitor.visible;//toggle on/off
                }
                if (GUILayout.Button("KBD", keyb.visible ? mySty : offSty, GUILayout.ExpandWidth(true)))//GUILayout.Button is "true" when clicked
                    {
                    keyb.visible = !keyb.visible;//toggle on/off
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
                    if (S[0].Equals("load")) {
                        loadWrapper(S[1], true, 128);
                    } else if(S[0].Equals("insert")) {
                        if(S[1].Equals("empty")) {
                            Int32[] media = new Int32[1024 * 256];
                            media[1] = 4711;
                            tapeDrive.insertMedia(media);
                        } else if(S[1].Contains('.')) {
                            tapeDrive.insertMedia(tapeDrive.loadTape(S[1]));
                        }
                    } else if(S[0].Equals("asm")) {
                        loadWrapper(S[1], false, 128);
                    } else if (S[0].Equals("print")) {
                        if (Util.tryParseTo<UInt16>(S[1], out tmp)) {
                            for (int i = 0; i < 100; ++i) {
                                print(CPU.Memory[i + tmp]);
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
                            loadWrapper(S[2], true, 128);
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

        /**************************External interaction stuff*****************/
        
        //connect an interface to a slot
        public void connect(ISerial toConnect, int slot)
        {
            CPU.SerialInterfaces[slot].disconnect();
            CPU.SerialInterfaces[slot].connect(toConnect);
            toConnect.connect(CPU.SerialInterfaces[slot]); 
        }

        //see if the slot is connected to something
        public bool slotOccupied(int slot)
        {
            return CPU.SerialInterfaces[slot].isOccupied();
        }
    }
}