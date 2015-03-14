using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace ProgCom
{
    public class CPUem
    {
        public LinkedList<String> errorMessages;
        public Boolean hasErrors;

        private class IntStatus : IPCHardware
        {
            public const int IENABLED = 0;
            public const int IQUEUEHANDLE = 1;
            public int currentInterruptHandled = -1;

            private const ushort address = 44;
            private ushort _IADDRESS;
            private int _ISTATS;
            public void connect() {}
            public void disconnect() {}
            public bool getStatus(int statusID)
            {
                return ((_ISTATS >> statusID) & 1) != 0;
            }

            public void setStatus(int statusID, bool b)
            {
                if (b) {
                    _ISTATS |= (1 << statusID);
                } else {
                    _ISTATS ^= _ISTATS & (1 << statusID);
                }
            }

            public ushort getIAddr()
            {
                return _IADDRESS;
            }

            public Tuple<ushort, int> getSegment(int id)
            {
                return new Tuple<ushort, int>(address, 2);
            }

            public int getSegmentCount()
            {
                return 1;
            }

            public void recInterruptHandle(InterruptHandle seg) {}

            public int memRead(ushort position)
            {
                position -= address;
                if (position == 0) {
                    return _IADDRESS;
                } else {
                    return _ISTATS;
                }
            }

            public void memWrite(ushort position, int value)
            {
                position -= address;
                if (position == 0) {
                    _IADDRESS = (ushort)value;
                } else {
                    _ISTATS = value;
                }
            }

            public void tick(int ticks) {}
        }

        //variables for the computer
        private const int clockrate = 384000;//speed of computer cycles per second (should be 384000)
        private Int32[] register;
        private UInt16 pc = 128;
        private CacheManager memory;
        private Queue<Int32> interruptsPending;
        private LinkedList<IPCHardware> hardware;
        //define register numbers
        private const int exreg = 29;
        private IntStatus interruptStatus;
        //to add: msb, lsb, msbn, lsbn (most significant bit, least significant bit, most significant bit number (eg bit 23), least significant bit number)
        //to add: le lei leu leui leq leqi lequ lequi eq eqi equ equi neq neqi nequ nequi land landi lor lori lxor lxori
        //to add: fl, flr (I have completely forgotten what these mean)

        int[] fsp;//float stack pointer
        int[] fsbc;//float stack busy cycles
        int fss;//floating stack selected
        float[,] floatStack;//create a 2x4 array, one for each stack


        /****************** Startup code goes here ******************************/

        public CPUem()
        {
            init();
        }

        public void reset()
        {
            init();
        }

        private void init()
        {
            //various non-emulation related initialisation
            errorMessages = new LinkedList<String>();
            hasErrors = false;

            //initialise cpu-emulator
            pc = 128;//is this really right?
            memory = new CacheManager();
            floatStack = new float[2, 4];
            fss = 0;
            fsp = new int[2];
            fsp[0] = -1;
            fsp[1] = -1;
            fsbc = new int[2];
            interruptsPending = new Queue<Int32>();

            memory.Memory[41] = 1024;//see if we can't move this to the boot code
            register = new Int32[32];

            //init hardware
            hardware = new LinkedList<IPCHardware>();

            //initialise the timer
            hwConnect(new PCTimer());

            //initialise interrupt manager
            interruptStatus = new IntStatus();
            hwConnect(interruptStatus);

        }

        /****************** External interaction code goes here *****************/

        //various accessors
        public Int32 getMem(UInt16 address)
        {
            return memory.getMem(address);
        }
        public Int32[] Registers
        {
            get { return register; }
            //set { register = value; }
        }
        public UInt16 PC
        {
            get { return pc; }
        }
        public int ClockRate
        {
            get { return clockrate; }//might want to allow setting, as well.
        }
        public void spawnException(int i)
        {
            if (interruptsPending.Count < 256
                    && !interruptsPending.Contains(i)
                    && !(!interruptStatus.getStatus(IntStatus.IQUEUEHANDLE) && interruptStatus.currentInterruptHandled == i)
                    && interruptStatus.getStatus(IntStatus.IENABLED)) {
                interruptsPending.Enqueue(i);
            }
        }

        //runs one instruction and returns the cycles it took to run.
        //a negative number or zero implies various exceptional conditions have ocurred.
        public int tick()
        {
            int cyclesElapsed = 0;
            try {
                cyclesElapsed = nextInst();
            }
            catch (Exception e) {
                errorMessages.AddLast("Error in instruction stuff: " + e.Message);
                hasErrors = true;
                return 100;
            }
            try {
                //update timers and such, spawn interrupts
                handleHardware(cyclesElapsed);
            }
            catch (Exception e) {
                errorMessages.AddLast("Error in hardware stuff: " + e.Message);
                hasErrors = true;
                return 101;
            }
            try {
                //handle all interrupts
                interruptHandle();
            }
            catch (Exception e) {
                errorMessages.AddLast("Error in interrupt stuff: " + e.Message);
                hasErrors = true;
                return 102;
            }
            return cyclesElapsed;
        }
        /****************** Internal workings here ******************************/



        private int nextInst()
        {
            //bit of background stuff
            //r0 is always locked to 0
            register[0] = 0;
            //time the instruction took to complete
            int executionTime = 0;

            //divides the instruction in usable parts
            UInt32 instruction = memory.instructionLoad(pc, out executionTime);
            long inst = (instruction & 0xfc000000) >> 26;//why do I have to declare this long?!?
            //get immediate bit
            long immed = inst >> 3;
            immed = immed & 1;
            //get instruction type bits
            long type = inst >> 4;
            inst = inst & 7;

            long regA = (instruction & 0x03e00000) >> 21;
            long regB = (instruction & 0x001f0000) >> 16;
            UInt16 address = (UInt16)(instruction & 0x0000ffff);
            long regC = (address & 0x001f);

            Int32 valA = register[regA];
            Int32 valB = register[regB];
            Int32 valC = register[regC];

            if (immed != 0) {
                valC = address;
            }
            ++pc;
            UInt64 exTmp;
            Int32 exTimeTMP;
            int tmp = 0;

            if (type == 0) { //arithmetic
                switch (inst) {
                    case 0://Add
                        exTmp = ((UInt64)valB & 0xffffffff) + ((UInt64)valC & 0xffffffff);//apparently, C# does the sign-extendy thing even when casting to ulong
                        register[exreg] = (Int32)(exTmp >> 32);
                        register[regA] = (Int32)exTmp;
                        break;
                    case 1://Sub
                        exTmp = ((UInt64)valB & 0xffffffff) + ((UInt64)(-valC) & 0xffffffff);
                        register[exreg] = (Int32)(exTmp >> 32);
                        register[regA] = (Int32)exTmp;
                        break;
                    case 2://Mul
                        exTmp = ((UInt64)valB & 0xffffffff) * ((UInt64)valC & 0xffffffff);
                        register[exreg] = (Int32)(exTmp >> 32);
                        register[regA] = (Int32)exTmp;
                        executionTime += 9;
                        break;
                    case 3://Div
                        if (valC == 0) {
                            //illegalInstructionInterrupt
                            interruptsPending.Enqueue(258);//change to dividebyzerointerrupt when available
                            register[regA] = -1;
                            register[exreg] = -1;
                            break;
                        }
                        register[exreg] = valB - (valB / valC) * valC;//this is equal to valB % valC on most systems.
                        register[regA] = valB / valC;
                        executionTime += 39;
                        break;
                    case 4://And
                        register[regA] = valB & valC;
                        break;
                    case 5://Or
                        register[regA] = valB | valC;
                        break;
                    case 6://Xor
                        register[regA] = valB ^ valC;
                        break;
                    case 7://Not
                        register[regC] = valC ^ -1;
                        break;
                }
            } else if (type == 1) { //extended arithmetic
                switch (inst) {
                    case 0://Flcmp
                        float f1 = Util.itof(valB);
                        float f2 = Util.itof(valC);
                        if (f1 < f2) {
                            register[regA] = -1;
                        } else if (f1 > f2) {
                            register[regA] = 1;
                        } else {
                            register[regA] = 0;
                        }
                        break;
                    case 1://Sr
                        if (valC != 0) {
                            register[exreg] = valB << 32 - (valC & 0x1f);
                        } else {
                            register[exreg] = 0;
                        }
                        register[regA] = (Int32)((UInt32)valB >> (valC & 0x1f));//logical right shift
                        break;
                    case 2://Sl
                        if (valC != 0) {
                            register[exreg] = (Int32)((UInt32)valB >> (32 - valC & 0x3f));
                        } else {
                            register[exreg] = 0;
                        }
                        register[regA] = valB << (valC & 0x3f);
                        break;
                    case 3://Sra
                        if (valC != 0) {
                            register[exreg] = valB << 32 - (valC & 0x1f);
                        } else {
                            register[exreg] = 0;
                        }
                        register[regA] = valB >> (valC & 0x1f);//arithmetic right shift
                        break;
                    case 4://Sx
                        spawnException(258);
                        break;
                    case 5://Float
                        tmp = floatInst(address, regA);
                        if (tmp == -1) {
                            return tmp;
                        } else {
                            executionTime += tmp;
                        }
                        break;
                    case 6://Extnd
                        tmp = extndInst(address, (Int32)regA, (Int32)regB, (Int32)regC, valA, valB, valC, immed != 0);
                        if (tmp == -1) {
                            return tmp;
                        } else {
                            executionTime += tmp;
                        }
                        break;
                    case 7://Cmp
                        if (valB < valC) {
                            register[regA] = -1;
                        } else if (valB == valC) {
                            register[regA] = 0;
                        } else {
                            register[regA] = 1;
                        }
                        break;

                }
            } else if (type == 2) { //branching
                switch (inst) {
                    case 0://Beq/Br
                        if (valA == valB) {
                            pc += (UInt16)valC;
                            executionTime += 1;
                        }
                        break;
                    case 1://Bne
                        if (valA != valB) {
                            pc += (UInt16)valC;
                            executionTime += 1;
                        }
                        break;
                    case 2://Jmpeq/Jmp
                        if (valA == valB) {
                            pc = (UInt16)valC;
                            executionTime += 1;
                            valA = 4711;//this made the jmp instruction start working for some goddamned reason. It just goes to show, when in doubt: 4711
                        }
                        break;
                    case 3://Bl
                        if (valA < valB) {
                            pc += (UInt16)valC;
                            executionTime += 1;
                        }
                        break;
                    case 4://Ble
                        if (valA <= valB) {
                            pc += (UInt16)valC;
                            executionTime += 1;
                        }
                        break;
                    case 5://Bx
                        //TODO: DO something with this instruction
                        spawnException(258);
                        break;
                    case 6://Call
                        register[15] = pc;
                        if (immed != 0) {
                            pc += (UInt16)valC;
                        } else {
                            pc = (UInt16)valC;
                        }
                        executionTime += 1;
                        break;
                    case 7://eret
                        pc = (UInt16)register[31];
                        interruptStatus.setStatus(IntStatus.IQUEUEHANDLE, true);
                        executionTime += 1;
                        break;

                }
            } else if (type == 3) { //data move
                switch (inst) {
                    case 0://Mov
                        if (immed != 0) {
                            valC += (Int32)regB << 16;
                        }
                        register[regA] = valC;
                        break;
                    case 1://Movhi
                        register[regA] = valC << 16;
                        break;
                    case 2://Rd
                        register[regA] = memory.readMem((UInt16)((valB + valC) & 0x0000ffff), out exTimeTMP);
                        executionTime += exTimeTMP - 1 + memReadDelaySlots((int)regA);
                        break;
                    case 3://Wr
                        tmp = (UInt16)((valB + valC) & 0x0000ffff);
                        executionTime += memory.writeMem(valA, (UInt16)tmp) - 1;
                        break;
                    case 4://Push
                        executionTime += memory.writeMem(valC, (UInt16)(register[14] & 0x0000ffff)) - 1;
                        register[14] = register[14] + 1;
                        break;
                    case 5://Pop
                        register[regA] = memory.readMem((UInt16)((register[14] - 1) & 0x0000ffff), out exTimeTMP);
                        register[14] = register[14] - 1;
                        executionTime += exTimeTMP - 1 + memReadDelaySlots((int)regA);
                        break;
                    case 6://Rdx
                        spawnException(258);
                        //EX = memory.readMem((UInt16)((valB + valC) & 0x0000ffff), out exTimeTMP);
                        //executionTime += exTimeTMP - 1;
                        break;
                    case 7://Int
                        spawnException(valC);
                        executionTime += 1;
                        break;

                }
            }
            return executionTime;
        }

        private int floatInst(int ins, long regA)
        {
            //what are the instructions we run here?
            //fpush, fpush1, fpushn1, fpush0, fpushpi, fpop, fadd, fsub, fmul, fdiv, 
            int returnCycles = 1;
            ins = ins & 0xf;
            switch (ins) {
                case 0://fadd
                    if (fsp[fss] > 0) {
                        floatStack[fss, fsp[fss] - 1] += floatStack[fss, fsp[fss]];
                        fsp[fss] -= 1;
                    } else {
                        floatStack[fss, fsp[fss]] += floatStack[fss, fsp[fss] + 1];
                    }
                    fsbc[fss] += 2;
                    break;
                case 1://fsub
                    if (fsp[fss] > 0) {
                        floatStack[fss, fsp[fss] - 1] -= floatStack[fss, fsp[fss]];
                        fsp[fss] -= 1;
                    } else {
                        floatStack[fss, fsp[fss]] -= floatStack[fss, fsp[fss] + 1];
                    }
                    fsbc[fss] += 2;
                    break;
                case 2://fmul
                    if (fsp[fss] > 0) {
                        floatStack[fss, fsp[fss] - 1] *= floatStack[fss, fsp[fss]];
                        fsp[fss] -= 1;
                    } else {
                        floatStack[fss, fsp[fss]] *= floatStack[fss, fsp[fss] + 1];
                    }
                    fsbc[fss] += 15;
                    break;
                case 3://fdiv
                    if (fsp[fss] > 0) {
                        floatStack[fss, fsp[fss] - 1] /= floatStack[fss, fsp[fss]];
                        fsp[fss] -= 1;
                    } else {
                        floatStack[fss, fsp[fss]] /= floatStack[fss, fsp[fss] + 1];
                    }
                    fsbc[fss] += 60;
                    break;
                case 4://fmerge
                    if (fsbc[0] > fsbc[1]) {
                        fsbc[0] += 1;
                        fsbc[1] = fsbc[0];
                    } else {//the merge operation requires both stacks to be synchronised
                        fsbc[1] += 1;
                        fsbc[0] = fsbc[1];
                    }
                    int fsp1 = fsp[0];
                    if (fsp1 < 0) fsp1 = 0;
                    int fsp2 = fsp[1];
                    if (fsp2 < 0) fsp2 = 0;
                    floatStack[0, fsp1] += floatStack[1, fsp2];
                    fsbc[fss] += 2;
                    break;
                case 5://ftoi
                    floatStack[fss, fsp[fss]] = Util.itof((Int32)floatStack[fss, fsp[fss]]);
                    fsbc[fss] += 30;
                    break;
                case 6://ftof
                    floatStack[fss, fsp[fss]] = (float)Util.ftoi(floatStack[fss, fsp[fss]]);
                    fsbc[fss] += 30;
                    break;
                case 7://fss
                    register[regA] = (fsp[fss] & 0xffff) | fss<<16;;
                    fsbc[fss] += 1;
                    break;
                case 8://fpop
                    register[regA] = Util.ftoi(floatStack[fss, fsp[fss]]);
                    if (fsp[fss] > -1) {
                        register[regA] = Util.ftoi(floatStack[fss, fsp[fss]]);
                        fsp[fss] -= 1;
                    } else {
                        register[regA] = Util.ftoi(floatStack[fss, 0]);
                    }
                    returnCycles += fsbc[fss];
                    fsbc[fss] += 1;
                    break;
                case 9://fpush
                    if (fsp[fss] < 3) {
                        fsp[fss] += 1;
                    }
                    floatStack[fss, fsp[fss]] = Util.itof(register[regA]);
                    returnCycles += fsbc[fss];
                    fsbc[fss] += 1;
                    break;
                case 10://fsel0
                    fss = 0;
                    break;
                case 11://fsel1
                    fss = 1;
                    break;
                case 12://fflush
                    for (int i = 0; i < 4; ++i) {
                        floatStack[fss, i] = 0;
                    }
                    fsp[fss] = 0;
                    fsbc[fss] = 1;
                    break;
                case 13://fpush1
                    if (fsp[fss] < 3) {
                        fsp[fss] += 1;
                    }
                    floatStack[fss, fsp[fss]] = 1.0f;
                    fsbc[fss] += 1;
                    break;
                case 14://fpushn1
                    if (fsp[fss] < 3) {
                        fsp[fss] += 1;
                    }
                    floatStack[fss, fsp[fss]] = -1.0f;
                    fsbc[fss] += 1;
                    break;
                case 15://fpushpi
                    if (fsp[fss] < 3) {
                        fsp[fss] += 1;
                    }
                    floatStack[fss, fsp[fss]] = (float)Math.PI;
                    fsbc[fss] += 1;
                    break;
                default:
                    spawnException(258);
                    break;
            }
            return returnCycles;
        }

        private int extndInst(int address, int regA, int regB, int regC, int valA, int valB, int valC, bool immed) 
        {
            int inst = address >> 8;
            if (immed) {
                valC = valC & 255;
            }
            int extraExecTime = 0;
            int tmp;
            switch (inst) {
                case 0://true nop
                    break;
                //8-11 partial moves
                case 8://movb
                    valA ^= (valA & (255 << (8 * (valC & 3))));
                    register[regA] = valA | (valB & (255 << (8 * (valC & 3))));
                    break;
                case 9://movhw
                    valA ^= (valA & (65535 << (16 * (valC & 1))));
                    register[regA] = valA | (valB & (65535 << (16 * (valC & 1))));
                    break;
                case 10://movbl
                    valA ^= (valA & (255 << (8 * (valC & 3))));
                    register[regA] = valA | ((valB & 255) << (8 * (valC & 3)));
                    break;
                case 11://movhwl
                    valA ^= (valA & (65535 << (16 * (valC & 1))));
                    register[regA] = valA | ((valB & 65535) << (16 * (valC & 1)));
                    break;
                //12-15, might extend partial moves here
                //16-24, comparison instructions
                case 16://le
                    if(valB < valC) {
                        register[regA] = 1;
                    } else {
                        register[regA] = 0;
                    }
                    break;
                case 17://leq
                    if(valB <= valC) {
                        register[regA] = 1;
                    } else {
                        register[regA] = 0;
                    }
                    break;
                case 18://eq
                    if(valB == valC) {
                        register[regA] = 1;
                    } else {
                        register[regA] = 0;
                    }
                    break;
                case 19://neq
                    if(valB != valC) {
                        register[regA] = 1;
                    } else {
                        register[regA] = 0;
                    }
                    break;
                case 20://leu
                    if ((UInt32)valB < (UInt32)valC) {
                        register[regA] = 1;
                    } else {
                        register[regA] = 0;
                    }
                    break;
                case 21://lequ
                    if ((UInt32)valB <= (UInt32)valC) {
                        register[regA] = 1;
                    } else {
                        register[regA] = 0;
                    }
                    break;
                //22-24, reserved
                //24-27, logical arithmetic
                case 24://land
                    if (valB != 0 && valC != 0) {
                        register[regA] = 1;
                    } else {
                        register[regA] = 0;
                    }
                    break;
                case 25://lor
                    if (valB != 0 || valC != 0) {
                        register[regA] = 1;
                    } else {
                        register[regA] = 0;
                    }
                    break;
                case 26://lxor
                    if ((valB != 0 && valC == 0) || (valB == 0 && valC != 0)) {
                        register[regA] = 1;
                    } else {
                        register[regA] = 0;
                    }
                    break;
                //27, reserved
                //28-31, bit searching
                case 28://msb
                    tmp = 31;
                    while (((valC >> tmp) & 1) == 0 && tmp > 0) {
                        tmp--;
                    }
                    register[regA] = 1 << tmp;
                    break;
                case 29://lsb
                    tmp = valC - 1;
                    tmp = tmp ^ valC;
                    register[regA] = tmp;
                    break;
                case 30://msbn
                    tmp = 31;
                    while (((valC >> tmp) & 1) == 0 && tmp > 0) {
                        tmp--;
                    }
                    register[regA] = tmp;
                    break;
                case 31://lsbn
                    tmp = 0;
                    while (((valC >> tmp) & 1) == 0 && tmp < 31) {
                        tmp++;
                    }
                    register[regA] = tmp;
                    break;
                //32-35, bit twiddling
                case 32://sbit, set bit
                    register[regA] = valB | (1 << (valC & 0x1f));
                    break;
                case 33://sbitc, set bit conditional
                    register[regA] = valB != 0 ? valA | (1 << (valC & 0x1f)) : valA;
                    break;
                case 34://gbit, get bit
                    register[regA] = (Int32)((UInt32)(valB & (1 << (valC & 0x1f)))) >> (valC &0x1f);
                    break;
                case 35://xbit, xor bit
                    register[regA] = valB ^ (1 << (valC & 0x1f));
                    break;
                default:
                    spawnException(258);
                    return 0;
            }
            return extraExecTime;
        }

        private int memReadDelaySlots(int regNum)
        {
            //partially disassemble next instruction, if the specified register is used anywhere, return 1. else return 0
            int inst = memory.getMem(pc);
            int rA = inst >> (inst & 0x03e00000) >> 21;
            int rB = (inst & 0x001f0000) >> 16;
            int rC = (inst & 0x001f);
            //if immed bit is set, make rC negative so that it will not match regNum
            int immed = inst >> 29;
            immed &= 1;
            if (immed == 1) rC = -rC;

            if (rA == regNum || rB == regNum || rC == regNum) return 1;
            else return 0;
        }


        //Interrupts are handled here
        private void interruptHandle()
        {
            //if interrupts are enabled
            if (interruptStatus.getStatus(IntStatus.IQUEUEHANDLE)) {
                //if an interrupt is pending
                //disable interrupts and jump to interrupt handler adress
                if (interruptsPending.Count > 0) {
                    register[31] = pc;
                    register[30] = interruptsPending.Dequeue();
                    interruptStatus.setStatus(IntStatus.IQUEUEHANDLE, false);
                    interruptStatus.currentInterruptHandled = register[30];
                    pc = interruptStatus.getIAddr();
                }
            }
        }

        //handles timers and such, spawns hardware interrupts
        private void handleHardware(int cyclesElapsed)
        {

            //handle the floating point stacks
            fsbc[0] -= cyclesElapsed;
            if (fsbc[0] < 0)
                fsbc[0] = 0;
            fsbc[1] -= cyclesElapsed;
            if (fsbc[1] < 0)
                fsbc[1] = 0;

            //check all hardware
            foreach (IPCHardware hw in hardware) {
                try {
                    hw.tick(cyclesElapsed);
                }
                catch (Exception e) {
                    errorMessages.AddLast(e.Message + " in " + hw.GetType());
                    hasErrors = true;
                    try {
                        hw.disconnect();
                    } catch { };//we don't need to care about fuckups here, probably
                    hardware.Remove(hw);//make sure to unmap the hardware from memory
                    //move the above into a separate function
                }
            }

        }

        public void hwConnect(IPCHardware hw)
        {
            try {
                hw.connect();
                memory.hwConnect(hw);
                hw.recInterruptHandle(new InterruptHandle(this));
                hardware.AddLast(hw);
            }
            catch (Exception e) {
                //memory.hwDisconnect(hw);//implement, pronto
                hardware.Remove(hw);
                try {
                    hw.disconnect();
                }
                catch { }
                throw e;
            }
        }

        //public Int32[] Memory
        //{
        //    get
        //    {
        //        return memory.Memory;
        //    }
        //}
    }
}