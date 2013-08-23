using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KSP.IO;

namespace ProgCom
{
    //this is a basic tape drive
    //things to emulate:
    //read speed: 6000 words/second
    //  this is the maximum speed that the drive can read data from the tape is
    //travel speed: 12000 words/second
    //  this is the maximum tape wind speed
    //memory size: 256 kw
    //memory buffer: 64 words
    //  the ammount of words that can be read but untransmitted
    //tape layout:
    //0: not used
    //1: always 4711, or tape broken
    //2: "read only" mode if not 0
    //3: data begins here

    class TapeDrive : ASerialTranceiver
    {
        //memory manipulation variables
        private Int32[] mem;
        private UInt32 position;//bits!! shift right 5 for correct value.
        private UInt32 travelToPos;//when setting, take 32* specified adress +1
        private UInt32 toRead;
        private Int32 cyclesPerBit;
        private Int32 moveCycles;
        private bool traveling;
        //validity flags
        private bool canRead;
        private bool canWrite;

        //sending variables
        private Queue<Int32> sendCache;//the stuff that needs to be sent over the serial interface
        //writing variables
        private Queue<Int32> writeCache;//the stuff that needs to be sent over the serial interface
        private int intsToReceive;

        //tape size definitions and etc.
        private int tapeLength = 256 * 1024;

        //receiving variables
        public TapeDrive(Int32 clockspeed)
        {
            sendCache = new Queue<Int32>();
            writeCache = new Queue<Int32>();
            cyclesPerBit = clockspeed/(6000*32);
        }

        public string getStatus()
        {
            return "pos: " + position + " trv: " + traveling + " ttp: " + travelToPos + " sdc: " + sendCache.Count + " mvc: " + moveCycles + " trd: " + toRead + " trc: " + intsToReceive + " wch: " + writeCache.Count + " rdy: " + ready() + " crc: " + customReadyCondition() + fuckingString();
        }

        public void insertMedia(Int32[] media)
        {
            if (media.Length != tapeLength) {
                throw new FormatException("Tape length not allowed: " + media.Length + ", should be: " + (1024*256));
            }

            //remove previous flags
            traveling = false;
            toRead = 0;
            travelToPos = 0;
            position = 0;

            //validity checks on media here
            canRead = true;
            canWrite = true;
            if (media[1] != 4711) {
                canRead = false;
                canWrite = false;
            } else if (media[2] != 0) {
                canWrite = false;
            }
            mem = media;
        }

        public Int32[] loadTape(String name)
        {
            //find the file and load it. Return false if it does not exist
            //this function will call insertMedia to do the final stuff with the stuff
            TextReader t = TextReader.CreateForType<TapeDrive>(name);
            Int32[] tape = new Int32[1024 * 256];
            for (int i = 0; i < 1024 * 256; ++i) {
                String line = t.ReadLine();
                if (!Util.tryParseTo<Int32>(line, out tape[i])) {
                    throw new FormatException("Not an Int32: " + line + " in: " + name);
                }
            }

            return tape;
        }

        public void saveTape(String name, Int32[] tape)
        {
            //find the file and load it. Return false if it does not exist
            //this function will call insertMedia to do the final stuff with the stuff
            TextWriter t = TextWriter.CreateForType<TapeDrive>(name);
            foreach (int i in tape) {
                t.WriteLine(i);
            }
            t.Close();
        }

        private Int32 read()
        {
            if (mem == null) {
                return 0;
            } else {
                if (position < 0) {
                    return 0;
                } else {
                    uint tmp = (position >> 5);
                    if (tmp >= mem.Length) {
                        return -1;
                    } else return mem[tmp];
                }
            }
        }

        private void write(Int32 i)
        {
            if (mem == null) {
                //What do I do here?
            } else {
                if (position < 0) {
                    //what do I do here?
                } else {
                    uint tmp = (position >> 5);
                    if (tmp >= mem.Length) {
                        //what do I do here?
                    } else mem[tmp] = i;
                }
            }
        }

        protected override bool customReadyCondition()
        {
            return sendCache.Count == 0 && writeCache.Count == 0 && toRead == 0 && !traveling;
        }

        protected override void serialUpdate()
        {
            //either transfer or read or both
            //sending management
            if (sendCache.Count > 0 && canSend()) {
                send((UInt32)sendCache.Dequeue());
            }
            //receiving management
            if (hasReceived()) {
                interpretInt(getLastReceived());
            }

            //reading/writing management
            if(traveling == true) {
                if (position > travelToPos) {
                    moveCycles -= 2;//when not reading, you can wind the tape at double the speed
                } else if (position < travelToPos) {
                    moveCycles += 2;
                } else {
                    traveling = false;
                    moveCycles = 0;
                }
            } else if (toRead > 0) {//reading
                if (sendCache.Count < 32) {
                    if ((position & 0x1f) == 0x1f && moveCycles == 0) {
                        sendCache.Enqueue(read());
                        --toRead;
                    }
                    ++moveCycles;
                }
            } else if (writeCache.Count > 0) {//writing
                if ((position & 0x1f) == 0x1f && moveCycles == 0) {
                    write(writeCache.Dequeue());
                }
                ++moveCycles;
            }

            //update reader position
            if (moveCycles >= cyclesPerBit) {
                ++position;
                moveCycles -= cyclesPerBit;
            } else if (moveCycles <= -cyclesPerBit) {
                --position;
                moveCycles += cyclesPerBit;
            }
        }
        //this function has been split off from main code for clarity
        //it interprets a 32-bit value recived as an instruction and modifies the internal state to match
        /* communication protocol:
         * Receive
         * 0: do nothing
         * 1: move to high 3 bytes
         * 2: read high 3 bytes data
         * 3: stop reading
         * 4: purge cached data
         * 5: set pointer to 0
         * 6: set pointer to argument
         * 7: is tape writable?
         * 8: is tape readable?
         * 9: tape inserted?
         * 10: write next high 3 bytes of data to memory
         */
        private void interpretInt(Int32 i)
        {
            if (intsToReceive > 0) {
                if (writeCache.Count < 32) {
                    writeCache.Enqueue(i);
                }
                --intsToReceive;
                return;
            }

            int inst = i & 255;
            int argument = i >> 8;
            argument = argument & 0x00ffffff;
            switch (inst) {
                case 0:
                    break;
                case 1:
                    traveling = true;
                    travelToPos = (UInt32)(argument * 32);
                    break;
                case 2:
                    if (canRead) {
                        toRead = (UInt32)argument;
                    }
                    break;
                case 3:
                    toRead = 0;
                    break;
                case 4:
                    sendCache = new Queue<Int32>();
                    break;
                case 5:
                    if (mem != null) {
                        traveling = true;
                        travelToPos = 0;
                    }
                    break;
                case 6:
                    if (mem != null) {
                        traveling = true;
                        travelToPos = (UInt32)argument;
                    }
                    break;
                case 7:
                    //send boolean value, is tape inserted & valid & in non-read-only mode
                    if (canWrite) {
                        sendCache.Enqueue(1);
                    } else {
                        sendCache.Enqueue(0);
                    }
                    break;
                case 8:
                    //send boolean value, is tape valid and inserted
                    if (canRead) {
                        sendCache.Enqueue(1);
                    } else {
                        sendCache.Enqueue(0);
                    }
                    break;
                case 9:
                    //is tape inserted?
                    if (mem != null) {
                        sendCache.Enqueue(1);
                    } else {
                        sendCache.Enqueue(0);
                    }
                    break;
                case 10:
                    if (canWrite) {
                        intsToReceive = argument;
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
