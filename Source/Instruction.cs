using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class Instruction
{
    public UInt16 instructionNumber;
    public bool regA, regB, regC, Address, longAdress, invertAdress, mustRel, mustAbs;
    public int defaultConstant;
    public Instruction(UInt16 ins, bool rA, bool rB, bool rC, bool Addr, bool lA, bool invA, bool mRel, bool mAbs, int defcon)
    {//default instruction setup
        instructionNumber = ins;
        regA = rA;
        regB = rB;
        regC = rC;
        Address = Addr;
        longAdress = lA;
        invertAdress = invA;
        mustRel = mRel;
        mustAbs = mAbs;
        defaultConstant = defcon;
    }

}
