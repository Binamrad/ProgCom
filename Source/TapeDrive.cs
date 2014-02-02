﻿using System;
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

    public class TapeDrive : PartModule, IPCHardware
    {
        //this should probably be done with an enum
        private const int IBF = 0;//input buffer full
        private const int OBE = 1;//output buffer empty
        private const int IE = 2;//interrupt enable
        private const int BUSY = 3;//busy
        private const int WBF = 4;//write buffer full
        private const int RBF = 5;//read buffer full
        private const int WRITING = 6;//writing stuff to tape
        private const int IOB = 7;//interrupt on OBE
        private const int NOTIFYINTERRUPT = 260;
        private const int ERRORINTERRUPT = 261;


        UInt16 baseAddress = 64;
        const int cyclesPerInt = 384000 / 6000;
        int cyclesToNext = cyclesPerInt;
        bool reading, writing, seeking;
        bool readable, writeable;
        int toRead = 0;
        int toWrite = 0;
        int seekTarget = 0;
        int pointer = 0;
        int cacheMaxSize = 32;
        InterruptHandle inth;
        int lastRec = -1;
        int lastRecA = -1;

        int lastCommand = -1;
        int lasth8 = -1;

        //memory area for the tape drive
        //0: input
        //1: output
        //2: status
        //3: interrupt
        Int32[] memarea = new Int32[4];
        LinkedList<Int32> readCache = new LinkedList<Int32>();
        LinkedList<Int32> writeCache = new LinkedList<Int32>();
        Int32[] tape;


        //this function has been split off from main code for clarity
        //it interprets a 32-bit value recived as an instruction and modifies the internal state to match
        /* communication protocol:
         * Receive
         * 0: do nothing
         * 1: read high 3 bytes of data
         * 2: move to position indicated in high 3 bytes
         * 3: the next high 3 bytes of data will be written to the tape
         * 4: stop current task
         * 5: is tape inserted?
         * 6: is tape readable?
         * 7: is tape writable?
         * 8: purge read buffer
         * 9: disable reaad buffering
         * 10: enable read buffering
         * 11: relative seek
         * 12: get tape pointer
         */
        private void interpretInt(Int32 i)
        {
            int high3bytes = (i >> 8) & 0xffffff;
            i &= 0xff;
            lasth8 = high3bytes;
            lastCommand = i;
            switch (i) {
                case 0://NOP
                    break;
                case 1://read high 3
                    if (getStatus(BUSY)) { busyException(); break; }
                    reading = true;
                    toRead = high3bytes;
                    setStatus(BUSY, true);
                    break;
                case 2://seek to high 3
                    if (getStatus(BUSY)) { busyException(); break; }
                    seeking = true;
                    seekTarget = high3bytes;
                    setStatus(BUSY, true);
                    break;
                case 3://write high 3 to mem
                    if (getStatus(BUSY)) { busyException(); break; }
                    writing = true;
                    toWrite = high3bytes;
                    setStatus(WRITING, true);
                    break;
                case 4://abort current task
                    setStatus(BUSY, false);
                    setStatus(WRITING, false);
                    writing = false;
                    reading = false;
                    seeking = false;
                    cyclesToNext = cyclesPerInt;
                    toWrite = 0;
                    toRead = 0;
                    seekTarget = pointer;
                    break;
                case 5://is a tape inserted?
                    if (getStatus(BUSY)) { busyException(); break; }
                    if (readCache.Count < cacheMaxSize) {
                        readCache.AddFirst(tape == null ? 0 : 1);
                    } else {
                        //bufferOverflowException
                        overflowException();
                    }
                    break;
                case 6://is the tape readable
                    if (getStatus(BUSY)) { busyException(); break; }
                    if (readCache.Count < cacheMaxSize) {
                        readCache.AddFirst(readable ? 1 : 0);
                    } else {
                        //bufferOverflowException
                        overflowException();
                    }
                    break;
                case 7://is tape writeable
                    if (getStatus(BUSY)) { busyException(); break; }
                    if (readCache.Count < cacheMaxSize) {
                        readCache.AddFirst(writeable ? 1 : 0);
                    } else {
                        //bufferOverflowException
                        overflowException();
                    }
                    break;
                case 8://putge data buffers
                    readCache.Clear();
                    writeCache.Clear();
                    break;
                case 9://disable data buffering
                    cacheMaxSize = 1;
                    break;
                case 10://enable data buffering
                    cacheMaxSize = 32;
                    break;
                case 11:
                    if (getStatus(BUSY)) { busyException(); break; }
                    //sign extend parameter
                    high3bytes <<= 8;
                    high3bytes >>= 8;
                    //set seek position
                    seekTarget = pointer + high3bytes;
                    seeking = true;
                    break;
                case 12:
                    if (getStatus(BUSY)) { busyException(); break; }
                    if (readCache.Count < cacheMaxSize) {
                        readCache.AddFirst(pointer);
                    } else {
                        //bufferOverflowException
                        overflowException();
                    }
                    break;
                default:
                    unrecognisedCommandException();
                    break;
            }
        }

        private void unrecognisedCommandException()
        {
            memarea[3] = 1;
        }

        //called if a task is interrupted
        private void busyException()
        {
            memarea[3] = 3;
        }

        //called if the data caches are being overfilled
        private void overflowException()
        {
            memarea[3] = 4;
        }

        //called if we try to read from/set the pointer to an area that is outside of the tape
        private void EOTException()
        {
            memarea[3] = 2;
        }

        //this should be done in-tape
        public void insertMedia(Int32[] tape)//make this private, and make loadTape call this function
        {
            if (tape == null || tape.Length != 256 * 1024) throw new ArgumentException("Incorrect parameters passed to tape drive at tape initialisation");
            this.tape = tape;
            readable = true;
            if (tape[2] == 0) writeable = true; else writeable = false;
            if (tape[1] != 4711) {
                readable = false;
                writeable = false;
            }
            pointer = 3;
        }

        public void connect()
        {
            //do something?
        }

        public void disconnect()
        {
            //restore to some state or another
        }

        public Tuple<ushort, int> getSegment(int id)
        {
            return new Tuple<ushort, int>(64, 4);
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
            position -= baseAddress;
            if(position == 1) setStatus(IBF, false);
            return memarea[position];
        }

        public void memWrite(ushort position, int value)
        {
            lastRec = value;
            lastRecA = position;
            position -= baseAddress;
            if(position == 2) {
                memarea[2] ^= memarea[2] & 2;
                memarea[2] |= value & 2;
            } else if (position == 0) {
                memarea[0] = value;
                setStatus(OBE, false);
            }
        }

        public void tick(int ticks)
        {
            handleSendRec();//send/receive stuff from the connected cpu
            performRWS(ticks);//read/write/seek if appropriate
            
            //if there is data in the interrupt section, interrupt
            if (memarea[3] != 0 && getStatus(IE)) {
                inth.interrupt(ERRORINTERRUPT);
            }
            //if interrupts from things happened, stuff
            if (getStatus(IE) && ( getStatus(IBF) || ( getStatus(IOB) && getStatus(OBE) ) )) {
                inth.interrupt(NOTIFYINTERRUPT);
            }
        }

        private void handleSendRec()
        {
            if (!getStatus(OBE)) {
                //check if we are filling up our buffer for stuff. if so, put on buffer
                if (toWrite > 0) {
                    if (writeCache.Count < cacheMaxSize) {//if buffer is full, do nothing
                        writeCache.AddFirst(memarea[0]);
                        --toWrite;
                        setStatus(OBE, true);
                    }
                } else {
                    //if not reading, interpret int instead
                    interpretInt(memarea[0]);
                    setStatus(OBE, true);
                }
            }

            //see if we should send something to the connected cpu
            if (!getStatus(IBF) && readCache.Count > 0) {
                memarea[1] = readCache.Last.Value;
                readCache.RemoveLast();
                setStatus(IBF, true);
            }
        }

        private void performRWS(int ticks)
        {
            if (reading && readCache.Count < cacheMaxSize) {
                cyclesToNext -= ticks;
                if (cyclesToNext <= 0) {
                    --toRead;
                    readCache.AddFirst(tape[pointer++]);
                    if (toRead == 0) {
                        reading = false;
                        cyclesToNext = cyclesPerInt;
                        setStatus(BUSY, false);
                    } else {
                        cyclesToNext += cyclesPerInt;
                    }
                }
            } else if (writing && writeCache.Count > 0) {
                cyclesToNext -= ticks;
                if (cyclesToNext <= 0) {
                    --toWrite;
                    tape[pointer++] = writeCache.Last();
                    writeCache.RemoveLast();
                    if (toWrite == 0) {
                        writing = false;
                        cyclesToNext = cyclesPerInt;
                        setStatus(WRITING, false);
                    } else {
                        cyclesToNext += cyclesPerInt;
                    }
                }
            } else if (seeking) {
                cyclesToNext -= ticks << 1;//just winding the tape is twice as fast
                if (cyclesToNext <= 0) {
                    if (pointer < seekTarget) {
                        ++pointer;
                        cyclesToNext += cyclesPerInt;
                    } else if (pointer > seekTarget) {
                        --pointer;
                        cyclesToNext += cyclesPerInt;
                    } else {
                        seeking = false;
                        cyclesToNext = cyclesPerInt;
                        setStatus(BUSY, false);
                    }

                }
            }
        }

        //status table:
        /* 0: IBF
         * 1: OBE
         * 2: IENABLE
         * 3: busy
         * 4: OCF
         * 5: ICF
         * */
        private void setStatus(int statusIndex, bool status)
        {
            int i = 1 << statusIndex;
            if (!status) {
                int j = memarea[2] & i;
                memarea[2] ^= j;
            } else {
                memarea[2] |= i;
            }
        }

        private bool getStatus(int statusIndex)
        {
            return (memarea[2] & 1 << statusIndex) != 0;
        }

        //************************************************
        //external stuff
        //replace this once we get a gui for the tape drive
        public void saveTapeInternal(String name)
        {
            saveTape(name, tape);
        }
        public void saveTape(String name, Int32[] tape)
        {
            //find the file and load it. Return false if it does not exist
            //this function will call insertMedia to do the final stuff with the stuff
            TextWriter t = TextWriter.CreateForType<TapeDrive>(name);
            //find the last line that is non-zero
            int index = 1024 * 256 - 1;
            while (tape[index] == 0) {
                --index;
            }
            for (int i = 0; i <= index; ++i) {
                t.WriteLine(tape[i]);
            }
            t.Close();
        }
        public string statusString()
        {
            return "p: " + pointer + " tp: " + tape[pointer >= tape.Length ? tape.Length - 1 : pointer] + " wc: " + writeCache.Count + " rc: " + readCache.Count + " read " + reading + " write " + writing + " seek " + seeking + "\ntrd: " + toRead + " twr: " + toWrite + " seekto: " + seekTarget + " lr: " + lastRec + " lra: " + lastRecA + " lcmd: " + lastCommand + " lh3: " + lasth8; 
        }
        public Int32[] loadTape(String name)
        {
            //find the file and load it. Return false if it does not exist
            //this function will call insertMedia to do the final stuff with the stuff
            TextReader t = TextReader.CreateForType<TapeDrive>(name);
            Int32[] tape = new Int32[1024 * 256];
            for (int i = 0; i < 1024 * 256; ++i) {
                String line = t.ReadLine();
                if (line == null || "".Equals(line)) return tape;
                if (!Util.tryParseTo<Int32>(line, out tape[i])) {
                    throw new FormatException("Not an Int32: " + line + " in: " + name);
                }
            }

            return tape;
        }
    }
}
