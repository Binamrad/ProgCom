using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace ProgCom
{
    class CPUem
    {
        //variables for the computer
        private int clockrate = 384000;//speed of computer (should be 384000)
        private UInt32 cyclesRunning;//how long the cpu has been running
        private Int32[] register;
        private Int32 EX;
        private UInt16 pc = 64;
        private MemoryModule memory;
        private Queue<Int32> interruptsPending;
        private SerialBus[] serials;

        //to add: msb, lsb, msbn, lsbn, movin -> subi X r0 n
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
            //initialise cpu-emulator
            pc = 64;//is this really right?
            memory = new MemoryModule();
            floatStack = new float[2, 4];
            fss = 0;
            fsp = new int[2];
            fsp[0] = -1;
            fsp[1] = -1;
            fsbc = new int[2];
            cyclesRunning = 0;
            interruptsPending = new Queue<Int32>();

            memory.Memory[41] = 1024;
            register = new Int32[32];

            //init serial buses
            serials = new SerialBus[8];
            for (int i = 0; i < 8; ++i) {
                serials[i] = new SerialBus((UInt16)(64+i*4), memory.Memory);
            }

            //init registers to deadd00d
            for (int x = 0; x < 31; ++x) {
                unchecked { register[x] = (Int32)0xdeadd00d; }
            }
            unchecked { EX = (Int32)0xdeadd00d; }
        }

        /****************** External interaction code goes here *****************/
        //returns an assembler which has the correct instruction set and produces correct code for this CPU
        public Assembler2 getCompatibleAssembler()
        {
            //create the instruction set for the assembler

            Dictionary<String, Instruction> Instr = new Dictionary<String, Instruction>();
            //to add:
            //movil
            //andi, ori, xori
            //all float instructions
            //rdx and other strange but possible instructions


            //add all instructions by the order of their values
            /*****************************************************************************************/
            //arithmetic
            Instr.Add("add", new Instruction(0x00, true, true, true, false, false, false, true, false, 0));
            Instr.Add("sub", new Instruction(0x01, true, true, true, false, false, false, false, false, 0));
            Instr.Add("mul", new Instruction(0x02, true, true, true, false, false, false, false, false, 0));
            Instr.Add("div", new Instruction(0x03, true, true, true, false, false, false, false, false, 0));
            Instr.Add("and", new Instruction(0x04, true, true, true, false, false, false, false, false, 0));
            Instr.Add("or", new Instruction(0x05, true, true, true, false, false, false, false, false, 0));
            Instr.Add("xor", new Instruction(0x06, true, true, true, false, false, false, false, false, 0));
            Instr.Add("not", new Instruction(0x07, false, false, true, false, false, false, false, false, 0));
            Instr.Add("addi", new Instruction(0x08, true, true, false, true, false, false, false, true, 0));
            Instr.Add("subi", new Instruction(0x09, true, true, false, true, false, false, false, true, 0));
            Instr.Add("muli", new Instruction(0x0a, true, true, false, true, false, false, false, true, 0));
            Instr.Add("divi", new Instruction(0x0b, true, true, false, true, false, false, false, true, 0));
            Instr.Add("andi", new Instruction(0x0c, true, true, false, true, false, false, false, true, 0));
            Instr.Add("ori", new Instruction(0x0d, true, true, false, true, false, false, false, true, 0));
            Instr.Add("xori", new Instruction(0x0e, true, true, false, true, false, false, false, true, 0));
            //0x0f --

            /*****************************************************************************************/
            //extended arithmetic
            Instr.Add("flcmp", new Instruction(0x10, true, true, true, false, false, false, false, false, 0));
            Instr.Add("shr", new Instruction(0x11, true, true, true, false, false, false, false, false, 0));
            Instr.Add("shl", new Instruction(0x12, true, true, true, false, false, false, false, false, 0));
            Instr.Add("ax", new Instruction(0x13, true, false, true, false, false, false, false, false, 0));
            Instr.Add("sx", new Instruction(0x14, true, false, true, false, false, false, false, false, 0));
            //0x15 float
            Instr.Add("fadd", new Instruction(0x15, false, false, false, false, false, false, false, false, 0));
            Instr.Add("fsub", new Instruction(0x15, false, false, false, false, false, false, false, false, 1));
            Instr.Add("fmul", new Instruction(0x15, false, false, false, false, false, false, false, false, 2));
            Instr.Add("fdiv", new Instruction(0x15, false, false, false, false, false, false, false, false, 3));
            Instr.Add("fmerge", new Instruction(0x15, false, false, false, false, false, false, false, false, 4));
            Instr.Add("ftoi", new Instruction(0x15, false, false, false, false, false, false, false, false, 5));
            Instr.Add("ftof", new Instruction(0x15, false, false, false, false, false, false, false, false, 6));
            //Instr.Add("fadd", new Instruction(0x15, false, false, false, false, false, false, false, false, 7));
            Instr.Add("fpop", new Instruction(0x15, true, false, false, false, false, false, false, false, 8));
            Instr.Add("fpush", new Instruction(0x15, true, false, false, false, false, false, false, false, 9));
            Instr.Add("fsel0", new Instruction(0x15, false, false, false, false, false, false, false, false, 10));
            Instr.Add("fsel1", new Instruction(0x15, false, false, false, false, false, false, false, false, 11));
            Instr.Add("fflush", new Instruction(0x15, false, false, false, false, false, false, false, false, 12));
            Instr.Add("fpush1", new Instruction(0x15, false, false, false, false, false, false, false, false, 13));
            Instr.Add("fpushn1", new Instruction(0x15, false, false, false, false, false, false, false, false, 14));
            Instr.Add("fpushpi", new Instruction(0x15, false, false, false, false, false, false, false, false, 15));
            //0x16 extended instruction set, register instructions
            //Instr.Add("nop", new Instruction(0x16, true, true, true, false, false, false, false, false, 0x0000));
            //Instr.Add("movbr", new Instruction(0x16, true, true, true, false, false, false, false, false, 0x0800));
            //Instr.Add("movhwr", new Instruction(0x16, true, true, true, false, false, false, false, false, 0x0900));
            //Instr.Add("movblr", new Instruction(0x1e6 true, true, true, false, false, false, false, false, 0x0a00));
            //Instr.Add("movhwlr", new Instruction(0x16, true, true, true, false, false, false, false, false, 0x0b00));
            Instr.Add("cmp", new Instruction(0x17, true, true, true, false, false, false, false, false, 0));
            //0x18 flcmpi
            Instr.Add("sri", new Instruction(0x19, true, true, false, true, false, false, false, false, 0));
            Instr.Add("sli", new Instruction(0x1a, true, true, false, true, false, false, false, false, 0));
            //0x1b axi
            //0x1c sxi
            //0x1d --
            //0x1e extended instruction set, immediate instructions
            //Instr.Add("nop", new Instruction(0x1e, true, true, false, true, false, false, false, false, 0x0000));
            Instr.Add("movb", new Instruction(0x1e, true, true, false, true, false, false, false, false, 0x0800));
            Instr.Add("movhw", new Instruction(0x1e, true, true, false, true, false, false, false, false, 0x0900));
            Instr.Add("movbl", new Instruction(0x1e, true, true, false, true, false, false, false, false, 0x0a00));
            Instr.Add("movhwl", new Instruction(0x1e, true, true, false, true, false, false, false, false, 0x0b00));
            //0x1f cmpi

            /****************************************************************************************/
            //branching
            Instr.Add("brr", new Instruction(0x20, false, false, true, false, false, false, false, false, 0));
            //0x21 bner
            Instr.Add("jmpr", new Instruction(0x22, false, false, true, false, false, false, false, false, 0));
            //0x23 blr
            //0x24 bler
            //0x25 bxr
            Instr.Add("callr", new Instruction(0x26, false, false, true, false, false, false, false, false, 0));
            //0x27 eret
            Instr.Add("br", new Instruction(0x28, false, false, false, true, false, false, true, false, 0));
            Instr.Add("halt", new Instruction(0x28, false, false, false, false, false, false, false, false, -1));
            Instr.Add("beq", new Instruction(0x28, true, true, false, true, false, false, true, false, 0));
            Instr.Add("bne", new Instruction(0x29, true, true, false, true, false, false, true, false, 0));
            Instr.Add("bi", new Instruction(0x29, true, false, false, true, false, false, true, false, 0));
            Instr.Add("jmp", new Instruction(0x2a, false, false, false, true, false, false, false, true, 0));
            Instr.Add("bl", new Instruction(0x2b, true, true, false, true, false, false, true, false, 0));
            Instr.Add("ble", new Instruction(0x2c, true, true, false, true, false, false, true, false, 0));
            Instr.Add("bx", new Instruction(0x2d, false, false, false, true, false, false, true, false, 0));
            Instr.Add("call", new Instruction(0x2e, false, false, false, true, false, false, true, false, 0));
            Instr.Add("eret", new Instruction(0x2f, false, false, false, false, false, false, false, false, 0));

            /***************************************************************************************/
            //data move
            Instr.Add("mov", new Instruction(0x30, true, false, true, false, false, false, false, false, 0));
            Instr.Add("nop", new Instruction(0x30, false, false, false, false, false, false, false, false, 0));
            //0x31 --
            //0x32 --
            //0x33 --
            Instr.Add("push", new Instruction(0x34, false, false, true, false, false, false, false, false, 0));
            Instr.Add("pop", new Instruction(0x35, true, false, false, false, false, false, false, false, 0));
            //0x36 --
            Instr.Add("int", new Instruction(0x37, false, false, true, false, false, false, false, false, 0));
            Instr.Add("movi", new Instruction(0x38, true, false, false, true, false, false, false, true, 0));
            Instr.Add("movil", new Instruction(0x38, true, false, false, true, true, false, false, true, 0));
            Instr.Add("movhi", new Instruction(0x39, true, false, false, true, false, false, false, true, 0));
            Instr.Add("rd", new Instruction(0x3a, true, true, false, true, false, false, false, true, 0));
            Instr.Add("wr", new Instruction(0x3b, true, true, false, true, false, false, false, true, 0));
            //0x3c --
            //0x3d --
            Instr.Add("rdx", new Instruction(0x3e, false, true, false, true, false, false, false, true, 0));
            //0x3f --

            //initialise the assembler
            Assembler2 assembler = new Assembler2(Instr);

            return assembler;
        }

        //various accessors
        public Int32[] Memory
        {
            get { return memory.Memory; }
        }
        public Int32[] Registers
        {
            get { return register; }
            set { register = value; }
        }
        public UInt16 PC
        {
            get { return pc; }
            set { pc = value; }
        }
        public Int32 getEX()
        {
            return EX;
        }
        public int ClockRate
        {
            get { return clockrate; }//might want to allow setting, as well.
        }
        public void spawnException(int i)
        {
            if (enabledInterrupts()) {
                interruptsPending.Enqueue(i);
            }
        }

        //runs one instruction and returns the cycles it took to run.
        //a negative number or zero implies various exceptional conditions have ocurred.
        public int tick()
        {
            int cyclesElapsed = nextInst();
            if (cyclesElapsed > 0) {
                //update timers and such, spawn interrupts
                handleHardware(cyclesElapsed);
                //handle all interrupts
                interruptHandle();
            }
            return cyclesElapsed;
        }
        /****************** Internal workings here ******************************/

        private int nextInst()
        {
            //TODO: add proper timings to all instructions
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
                        register[regA] = (Int32)exTmp;
                        EX = (Int32)(exTmp >> 32);
                        break;
                    case 1://Sub
                        exTmp = ((UInt64)valB & 0xffffffff) + ((UInt64)(-valC) & 0xffffffff);
                        register[regA] = (Int32)exTmp;
                        EX = (Int32)(exTmp >> 32);
                        break;
                    case 2://Mul
                        exTmp = ((UInt64)valB & 0xffffffff) * ((UInt64)valC & 0xffffffff);
                        register[regA] = (Int32)exTmp;
                        EX = (Int32)(exTmp >> 32);
                        executionTime += 9;
                        break;
                    case 3://Div
                        if (valC == 0) {
                            if (enabledInterrupts()) {
                                //illegalInstructionInterrupt
                                interruptsPending.Enqueue(258);
                            } else {
                                return -1;
                            }
                            break;
                        }
                        register[regA] = valB / valC;
                        EX = valB - register[regA] * valC;//this is equal to valB % valC on most systems.
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
                        register[regA] = (Int32)((UInt32)valB >> (valC & 0x1f));//logical right shift
                        EX = valB << 32 - (valC & 0x1f);
                        break;
                    case 2://Sl
                        register[regA] = valB << (valC & 0x3f);
                        EX = (Int32)((UInt32)valB >> (32 - valC & 0x3f));//logical right shift
                        break;
                    case 3://Ax
                        register[regA] = valC + EX;
                        break;
                    case 4://Sx
                        register[regA] = valC - EX;
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
                        if (EX != 0) {
                            pc += (UInt16)valC;
                            executionTime += 1;
                        }
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
                        interruptEnable();
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
                        executionTime += exTimeTMP - 1;
                        break;
                    case 3://Wr
                        //This is a bit of a hack, but it is the easiest way to implement sending over the serial bus
                        tmp = (UInt16)((valB + valC) & 0x0000ffff);
                        if (tmp >= 64 && tmp < 64 + 8 * 4) {
                            serials[(tmp - 64) >> 2].startSend();
                        }
                        executionTime += memory.writeMem(valA, (UInt16)tmp) - 1;
                        break;
                    case 4://Push
                        executionTime += memory.writeMem(valC, (UInt16)(register[14] & 0x0000ffff)) - 1;
                        register[14] = register[14] + 1;
                        break;
                    case 5://Pop
                        register[regA] = memory.readMem((UInt16)((register[14] - 1) & 0x0000ffff), out exTimeTMP);
                        register[14] = register[14] - 1;
                        executionTime += exTimeTMP - 1;
                        break;
                    case 6://Rdx
                        EX = memory.readMem((UInt16)((valB + valC) & 0x0000ffff), out exTimeTMP);
                        executionTime += exTimeTMP - 1;
                        break;
                    case 7://Int
                        if (enabledInterrupts()) {
                            interruptsPending.Enqueue(valC);
                        }
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
                    if (enabledInterrupts()) {
                        interruptsPending.Enqueue(258);
                    } else return -1;
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
            switch (inst) {
                case 0://true nop
                    break;
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
                default:
                    if (enabledInterrupts()) {
                        interruptsPending.Enqueue(258);
                        return 0;
                    } else return -1;
            }
            return extraExecTime;
        }

        //Interrupts are handled here
        private void interruptHandle()
        {
            //if interrupts are enabled
            if ((Memory[44] & 1) != 0) {
                //if an interrupt is pending
                //disable interrupts and jump to interrupt handler adress
                if (interruptsPending.Count != 0) {
                    register[31] = pc;
                    register[30] = interruptsPending.Dequeue();
                    interruptDisable();
                    pc = (UInt16)Memory[46];
                }
            }
        }

        //handles timers and such, spawns hardware interrupts
        private void handleHardware(int cyclesElapsed)
        {
            //handle serial buses
            foreach (SerialBus serb in serials) {
                serb.tick(cyclesElapsed);
            }

            //handle the floating point stacks
            fsbc[0] -= cyclesElapsed;
            if (fsbc[0] < 0)
                fsbc[0] = 0;
            fsbc[1] -= cyclesElapsed;
            if (fsbc[1] < 0)
                fsbc[1] = 0;

            //handle the timer(s)
            memory.Memory[40] += cyclesElapsed;
            //handle the clock
            cyclesRunning += (UInt32)cyclesElapsed;
            memory.Memory[45] = (Int32)cyclesRunning;

            //see if the timer should be reset, and if so, if an exception should be raised
            if (Memory[40] >= Memory[47] && Memory[47] != 0) {
                if (enabledInterrupts() && (Memory[44] & 2) != 0) {
                    interruptsPending.Enqueue(256);//change this to something reasonable
                }
                Memory[40] = 0;
            }

            //check all of the serial interfaces
            if (enabledInterrupts() && (Memory[44] & 16) != 0) {
                for (int i = 0; i < 8; ++i) {
                    int seriaLocation = 64 + (i << 2);
                    if ((memory.Memory[seriaLocation] & 1) != 0) {
                        if((memory.Memory[seriaLocation] & 2) != 0 || (memory.Memory[seriaLocation] & 4) != 0) {
                            interruptsPending.Enqueue(260 + i);
                        }
                    }
                }
            }
        }

        //enables/disables interrupts
        private void interruptEnable()
        {
            Memory[44] |= 1;
        }
        private void interruptDisable()
        {
            Memory[44] -= Memory[44] & 1;
        }
        //terutns true if interrupts are enabled
        public bool enabledInterrupts()
        {
            return (Memory[44] & 1) != 0;
        }
        //returns true if the specified bit in the IEnable adress is set to true
        public bool enabledInterruptNum(int i)
        {
            return enabledInterrupts() && (Memory[44] & (1 << i)) != 0;
        }

        public SerialBus[] SerialInterfaces
        {
            get { return serials; }
        }
    }
}