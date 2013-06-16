using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace ProgCom
{
    class Assembler2
    {
        private class FileData
        {
            public LinkedList<String> meta;
            public LinkedList<String> inst;
            public LinkedList<String> data;
            public LinkedList<String> globals;
            //public LinkedList<String> universals;
            public FileData()
            {
                meta = new LinkedList<String>();
                inst = new LinkedList<String>();
                data = new LinkedList<String>();
                globals = new LinkedList<String>();
            }
            //copies the specified data file last in the original data.
            public void mergeWith(FileData f)
            {
                //merge meta fields
                foreach (String s in f.meta) {
                    meta.AddLast(s);
                }
                //merge data fields
                foreach (String s in f.data) {
                    data.AddLast(s);
                }
                //merge instruction fields
                foreach (String s in f.inst) {
                    inst.AddLast(s);
                }
            }
            public void addMeta(String s)
            {
                meta.AddLast(s);
            }
            public void addData(String s)
            {
                //this bit is here to compensate for how the assembler loads files.
                s = s.Replace("- ", "-");
                data.AddLast(s);
            }
            public void addInst(String s)
            {
                inst.AddLast(s);
            }
        }
        private class Macro
        {
            private String name;
            private String[] parameters;
            //int paramCount;
            private LinkedList<String> lines;
            //the string passed to this function should be the WHOLE string, nothing cut out
            public Macro(String s)
            {
                lines = new LinkedList<String>();
                String[] strs = s.Split(' ');
                if (strs.Length < 3) {
                    throw new FormatException("too few arguments in macro declaration: " + s);
                }
                if (!strs[strs.Length - 1].Equals("{")) {
                    throw new FormatException("macro definition contains no code block start: " + s);
                }
                if (!strs[0].Equals("#macro")) {
                    throw new FormatException("Internal assembler error, macro string incorrect format");
                }
                name = strs[1];
                parameters = new String[strs.Length - 3];
                for (int i = 2; i < (strs.Length - 1); ++i) {
                    parameters[i - 2] = strs[i];
                }
            }
            public void addLine(String s)
            {
                lines.AddLast(s);
            }
            //returns the lines this macro will compile to with the specified parameters
            public LinkedList<String> getLines(String s)
            {
                String[] stuffs = stringParameterSplit(s);
                if (stuffs.Length != parameters.Length) {
                    String f = "";
                    int a = 0;
                    foreach (string t in stuffs) {
                        f = f + t + " ";
                        a++;
                    }
                    f = f + ": " + stuffs.Length;
                    int b = 0;
                    foreach (string t in parameters) {
                        f = f + t + " ";
                        b++;
                    }
                    f = f + ": " + parameters.Length + " | " + a + " " + b;
                    throw new FormatException(f);
                    //throw new FormatException("Too few/many parameters to macro: "+s);
                }
                LinkedList<String> tmp = new LinkedList<String>();
                foreach (String s2 in lines) {
                    String Stmp = s2;
                    for (int i = 0; i < parameters.Length; ++i) {
                        Stmp = Stmp.Replace(parameters[i], stuffs[i]);
                    }
                    tmp.AddLast(Stmp);
                }
                return tmp;
            }
            public String getName()//obvious function is obvious.
            {
                return name;
            }
        }

        Dictionary<String, Macro> macros;
        Dictionary<String, int> defines;
        Dictionary<String, int> stdDefines;
        LinkedList<String> filesIncluded;
        Dictionary<String, int> registers;
        Dictionary<String, Instruction> instructions;
        public Assembler2(Dictionary<String, Instruction> ins)
        {
            stdDefines = new Dictionary<string, int>();
            instructions = ins;
            //set up registers
            //TODO: this should not be hardcoded.
            registers = new Dictionary<String, int>();
            registers.Add("r0", 0);//zero
            registers.Add("zero", 0);//zero
            registers.Add("r1", 1);
            registers.Add("r2", 2);
            registers.Add("r3", 3);
            registers.Add("r4", 4);
            registers.Add("r5", 5);
            registers.Add("r6", 6);
            registers.Add("r7", 7);
            registers.Add("r8", 8);
            registers.Add("r9", 9);
            registers.Add("r10", 10);
            registers.Add("r11", 11);
            registers.Add("r12", 12);
            registers.Add("r13", 13);
            registers.Add("fp", 13);//frame pointer
            registers.Add("r14", 14);
            registers.Add("sp", 14);//stack pointer
            registers.Add("r15", 15);
            registers.Add("ra", 15);//return adress
            //more registers
            registers.Add("a0", 16);
            registers.Add("a1", 17);
            registers.Add("a2", 18);
            registers.Add("a3", 19);
            registers.Add("a4", 20);
            registers.Add("a5", 21);
            registers.Add("a6", 22);
            registers.Add("a7", 23);
            registers.Add("a8", 24);
            registers.Add("a9", 25);
            registers.Add("a10", 26);
            registers.Add("a11", 27);
            registers.Add("a12", 28);
            registers.Add("a13", 29);
            registers.Add("a14", 30);
            registers.Add("es", 30);//exception status
            registers.Add("a15", 31);
            registers.Add("ea", 31);//exception address


        }
        public void bindGlobalCall(String s, int i)
        {
            stdDefines.Add(s, i);
        }
        public Int32[] assemble(String fileName, int loadLocation)
        {
            defines = new Dictionary<string, int>(stdDefines);
            filesIncluded = new LinkedList<String>();
            filesIncluded.AddLast(fileName);
            macros = new Dictionary<String, Macro>();
            // step 1: load the file as a list of lines
            // step 2: remove all comments
            // step 3: replace all whitespace with single spaces
            // step 4: remove leading and closing whiteSpace
            // step 5: split the document into meta, data and text parts
            // step 5.4: localise the text as apropriate
            // Step 5.5: turn the macros into actual text here.
            // time for a TEST!!

            // step 6: perform these steps on all includes <- already done ^^
            // step 7: merge all meta/data/text sets <- done too
            // step 9: fix the data field <- what did this mean?
            // step 10:merge to integers <- TODO


            FileData f = loadText(fileName);
            KSP.IO.TextWriter txtOut = KSP.IO.TextWriter.CreateForType<Assembler2>("DEBUG_intermediate.txt");
            txtOut.WriteLine(".meta");
            foreach (String s in f.meta) {
                txtOut.WriteLine(s);
            }
            txtOut.WriteLine(".text");
            foreach (String s in f.inst) {
                txtOut.WriteLine(s);
            }
            txtOut.WriteLine(".data");
            foreach (String s in f.data) {
                txtOut.WriteLine(s);
            }
            txtOut.Close();
            //future code here
            //FileData f = getFileData(fileName)
            Int32[] i = finalise(f, loadLocation);
            //return i

            txtOut = KSP.IO.TextWriter.CreateForType<Assembler2>("DEBUG_code.txt");
            foreach (int x in i) {
                txtOut.WriteLine(x);
            }
            txtOut.Close();

            return i;
        }
        //takes a fileName and returns a filedata reperesentation of the file and it's includes
        private FileData loadText(String fileName)
        {
            FileData f;
            try {
                LinkedList<String> file = readFile(fileName);
                f = metadatatextSplit(file, fileName);
                localise(f, fileName);//make sure labels in f cannot be called accidentally by other sources
                unWrap(f);
            }
            catch (FormatException E) {
                throw new FormatException(E.Message + " in " + fileName);
            }
            return f;
        }
        //perform includes, compile and unroll all macros etc
        private void unWrap(FileData f)
        {
            //resolve includes and compile macros
            unWrapInner(f);
            //unroll macros
            LinkedList<String> tmp = new LinkedList<string>();
            foreach (String s in f.inst) {
                String ins = Util.cutStrAfter(s, " ");
                if (macros.ContainsKey(ins)) {
                    foreach (String s2 in macros[ins].getLines(s)) {
                        tmp.AddLast(s2);
                    }
                } else {
                    tmp.AddLast(s);
                }
            }
            f.inst = tmp;
        }
        private void unWrapInner(FileData f)
        {
            LinkedList<String> tmp = new LinkedList<String>();
            LinkedList<String> includes = new LinkedList<String>();
            //go through the entire file, resolve #macros, and #includes etc.
            bool writingMacro = false;
            Macro currentMacro = null;
            foreach (String s in f.inst) {
                if (s.StartsWith("#macro")) {
                    if (writingMacro) {
                        throw new FormatException("Macro definition found inside of macro definition. Are you missing a '}'?: " + s);
                    } else {
                        writingMacro = true;
                        currentMacro = new Macro(s);
                    }
                    continue;
                } else if (s.Equals("}") && writingMacro) {
                    writingMacro = false;
                    macros.Add(currentMacro.getName(), currentMacro);
                    currentMacro = null;
                    continue;
                } else if (writingMacro) {
                    if (s.StartsWith("#")) {
                        throw new FormatException("lines starting in '#' may not exist in macro definition: " + s);
                    }
                    currentMacro.addLine(s);
                    continue;
                }
                //with macro writing out of the way, we can concentrate on getting all the include statements out of the way
                if (s.StartsWith("#include")) {
                    String s2 = Util.cutStrBefore(s, " ");
                    includes.AddLast(s2);
                    continue;
                }

                //finally, put the remaining strings in the tmp linkedList
                tmp.AddLast(s);
            }
            //let f have the tmp file as the thing with the stuff
            f.inst = tmp;
            //go through f.data and remove all "allocate" entries
            tmp = new LinkedList<string>();
            foreach (String s in f.data) {
                if (s.StartsWith("#allocate")) {
                    String[] strs = s.Split(' ');
                    if (strs.Length > 2) {
                        throw new FormatException("Incorrect ammount of parameters in #allocate statement: " + s);
                    } else {
                        UInt16 a;
                        if (Util.tryParseTo<UInt16>(strs[1], out a)) {
                            for (int i = 0; i < a; ++i) {
                                tmp.AddLast("0");
                            }
                        } else {
                            throw new FormatException("Not A Number or too large/small in: " + s);
                        }
                    }
                } else if (s.StartsWith("#stringw")) {
                    String convert = s.Substring(s.IndexOf("\"") + 1);
                    convert = convert.Substring(0, convert.LastIndexOf("\""));
                    convert = Util.fixEscapeChars(convert);
                    Int32[] stringInts = Util.strToInt32(convert, false);
                    foreach (Int32 i in stringInts) {
                        tmp.AddLast(i.ToString());
                    }
                } else if (s.StartsWith("#string")) {
                    String convert = s.Substring(s.IndexOf("\"") + 1);
                    convert = convert.Substring(0, convert.LastIndexOf("\""));
                    convert = Util.fixEscapeChars(convert);
                    Int32[] stringInts = Util.strToInt32(convert, true);//this is a bit of a hack, but w/e
                    foreach (Int32 i in stringInts) {
                        tmp.AddLast(i.ToString());
                    }
                } else {
                    tmp.AddLast(s);
                }
            }
            f.data = tmp;

            //load all referenced files, and merge them with this one
            foreach (String s in includes) {
                if (!filesIncluded.Contains(s)) {
                    filesIncluded.AddLast(s);
                    FileData f2 = loadText(s);
                    f.mergeWith(f2);
                }
            }
        }
        //turn the file data into executable instructions
        private Int32[] finalise(FileData f, int startLocation)
        {
            //find all options:
            /* 0: pic
             * 1: mainJump
             * 2: autoStack
             * 3: alwaysErr
             * 4: autoLoader
             */
            bool[] opts = assignOpts(f.meta);

            //read through all instruction and data text, and set up a dictionary of where the labels should be pointing to
            int i = 0;
            if (!opts[0]) {
                i = startLocation;
            }
            if (opts[1]) {
                f.inst.AddFirst("br main");
            }
            if (opts[2]) {
                f.inst.AddFirst("movi sp, DATA_END");
            }
            if (opts[4]) {
                throw new FormatException("You don't need to declare AUTLDR yet, silly");
            }
            Dictionary<String, int> labels = new Dictionary<String, int>();
            labels.Add("TEXT_START", i);
            if (!opts[0]) {
                labels.Add("PROG_POS", startLocation);
            } else {
                labels.Add("PROG_POS", 0);
            }
            foreach (String s in f.inst) {
                if (s.EndsWith(":")) {
                    labels.Add(s.Substring(0, s.Length - 1), i);
                } else {
                    ++i;
                }
            }
            labels.Add("DATA_START", i);
            foreach (String s in f.data) {
                if (s.EndsWith(":")) {
                    labels.Add(s.Substring(0, s.Length - 1), i);
                } else {
                    i += s.Split(' ').Length;
                }
            }
            labels.Add("DATA_END", i);
            //go through the file data one last time and turn each line into int32 instruction/data form
            Int32[] retArr;
            if (!opts[0]) {
                retArr = new Int32[i - startLocation];
                i = startLocation;
            } else {
                retArr = new Int32[i];
                i = 0;
            }

            UInt16 currentInstruction = 0;
            foreach (String line in f.inst) {
                if (!line.EndsWith(":")) {
                    int a;
                    try {
                        a = compile(line, labels, opts[0], (UInt16)(currentInstruction + i));
                    }
                    catch (IndexOutOfRangeException E) {
                        throw new FormatException("Error in foreach compile statement: " + E.Message);
                    }
                    retArr[currentInstruction] = a;
                    ++currentInstruction;
                }
            }
            foreach (String line in f.data) {
                if (!line.EndsWith(":")) {
                    Int32[] res = evaluateNumerical(line);
                    foreach (Int32 n in res) {
                        retArr[currentInstruction] = n;
                        ++currentInstruction;
                    }
                }
            }


            return retArr;
        }

        //reads one line and converts it into an instruction
        //assumes that the string is an instruction. If that is not the case, Bad Things Happen.
        private Int32 compile(String s, Dictionary<String, int> labels, bool PIC, UInt16 lineLocation)
        {
            string[] words = stringParameterSplit(s);

            Instruction inst;
            int reg1 = 0;
            int reg2 = 0;
            int adress = 0;

            int wordNum = 0;
            String instStr = Util.cutStrAfter(s, " ");
            if (instructions.ContainsKey(instStr)) {
                inst = instructions[Util.cutStrAfter(s, " ")];
            } else {
                throw new FormatException("Not an instruction: " + s);
            }

            //check if the correct ammount of parameters have been supplied
            int parAmmount = 0;
            if (inst.regA) parAmmount++;
            if (inst.regB) parAmmount++;
            if (inst.regC || inst.Address) parAmmount++;
            if (words.Length != parAmmount) {
                throw new FormatException("Not enough parameters specified: " + s + ", expected " + parAmmount + "  got " + words.Length);
            }

            adress = inst.defaultConstant;
            if (inst.regA) {
                if (registers.ContainsKey(words[wordNum])) {
                    reg1 = registers[words[wordNum]];
                } else {
                    throw new FormatException("Not a register: " + words[wordNum] + " in: " + s);
                }
                ++wordNum;
            }
            if (inst.regB) {
                if (registers.ContainsKey(words[wordNum])) {
                    reg2 = registers[words[wordNum]];
                } else {
                    throw new FormatException("Not a register: " + words[wordNum] + " in: " + s);
                }
                ++wordNum;
            }
            if (inst.Address) {
                try {
                    adress |= evaluateLabel(words[wordNum], inst.mustRel, inst.mustAbs, PIC, lineLocation, labels);//i changed = to |= here, if something broke, change back
                }
                catch (FormatException E) {
                    throw E;//just let these exceptions pass
                }
                catch (Exception E) {
                    throw new FormatException("Internal assembler error: \"" + E + "\" in compile/evaluateLabel");
                }
            } else if (inst.regC) {
                if (registers.ContainsKey(words[wordNum])) {
                    adress |= registers[words[wordNum]];
                } else {
                    throw new FormatException("Not a register: " + words[wordNum] + " in: " + s);
                }
            }
            //make sure adress has the proper format
            if (inst.invertAdress) {
                adress = -adress;
            }
            if (inst.longAdress) {
                int tmp = adress & 0x1fffff;
                adress = adress & 0x001fffff;
            } else {
                adress = adress & 0x0000ffff;
            }

            //assemble the final instruction
            Int32 compiledInstruction = 0;
            compiledInstruction += inst.instructionNumber;
            compiledInstruction <<= 5;
            compiledInstruction += reg1;
            compiledInstruction <<= 5;
            compiledInstruction += reg2;
            compiledInstruction <<= 16;
            compiledInstruction += adress;
            //instruction layout: 6 bits instructions, 5 bit register, 5 bits register, 16 bit adress/5 bits register

            return compiledInstruction;

        }


        //if s is a register, this function will return the number corresponding to that register
        int evaluateReg(String s)
        {
            if (!registers.ContainsKey(s)) {
                throw new FormatException("Not a register: " + s);
            } else {
                return registers[s];
            }
        }
        //this might be a bit tricky. Oh, joy
        int evaluateLabel(String s, bool mustRel, bool mustAbs, bool PIC, UInt16 lineLocation, Dictionary<String, int> labels)
        {
            //algorithm:
            // I: check if is a label
            // I.I: if label, evaluate label
            String[] strs = s.Split(' ');
            int i = 0;
            bool addNext = true;
            bool subNext = false;
            foreach (String str in strs) {
                //check if string is + or -
                if (str.Equals("+")) {
                    if (subNext) {
                        throw new FormatException("a '+' may not follow a '-': " + s);
                    } else if (addNext) {
                        throw new FormatException("A '+' may not be entered twice in a row or as the first parameter: " + s);
                    }
                    addNext = true;
                    continue;
                }
                if (str.Equals("-")) {
                    if (addNext) {
                        addNext = false;
                        subNext = true;
                    } else if (subNext) {
                        throw new FormatException("two '-' may not be entered in a row: " + s);
                    }
                    subNext = true;
                    continue;
                }
                //check if string is a label
                //this might not be fun
                String[] localLabel = str.Split('%');
                if (localLabel.Length > 2) {
                    throw new FormatException("Excessive use of '%' has confused the assembler: " + s);
                }
                if (labels.ContainsKey(localLabel[0])) {
                    UInt16 tmp = (UInt16)labels[localLabel[0]];
                    if (localLabel.Length == 2) {
                        //see if we request the absolute or relative thingymajig here
                        //if pic, throw exception if use of absolute here
                        if (localLabel[1].Equals("abs")) {
                            i = addAddr(i, subNext, addNext, out subNext, out addNext, tmp, s);
                        } else if (localLabel[1].Equals("rel")) {
                            //relative
                            tmp -= (UInt16)(1 + lineLocation);
                            i = addAddr(i, subNext, addNext, out subNext, out addNext, tmp, s);
                        }
                    } else {
                        //just guess according to the instruction
                        // if PIC, use relative
                        //if rel, use rel
                        //if abs use abs
                        //if abs and pic, throw exception
                        if (mustRel) {
                            //relative
                            tmp -= (UInt16)(1 + lineLocation);
                            i = addAddr(i, subNext, addNext, out subNext, out addNext, tmp, s);
                        } else if (mustAbs) {
                            if (PIC) {
                                throw new FormatException("Instruction requiring absolute adress used in conjunction with PIC mode. Use %abs for closest approximation or manually calculate the absolute adress: " + s);
                            }
                            //absolute
                            i = addAddr(i, subNext, addNext, out subNext, out addNext, tmp, s);
                        } else {
                            //the fuck do we do here?
                            //use abs if not PIC, else use rel
                            if (PIC) {
                                //relative
                                tmp -= (UInt16)(1 + lineLocation);
                                i = addAddr(i, subNext, addNext, out subNext, out addNext, tmp, s);
                            } else {
                                //absolute
                                i = addAddr(i, subNext, addNext, out subNext, out addNext, tmp, s);
                            }
                        }
                    }
                    continue;
                } else if (defines.ContainsKey(localLabel[0]))//check if the label was a #defined value. this should always use absolute values unless otherwise specified. the GLOBAL_WHATEVER things should be confined here as well.
            {
                    if (localLabel.Length == 2) {
                        if (localLabel[1].Equals("rel")) {
                            if (PIC) {
                                throw new FormatException("Cannot find relative location of absolute position if PIC is enabled: " + s);
                            }
                            int tmp = defines[localLabel[0]];
                            tmp -= (UInt16)(1 + lineLocation);
                            i += tmp;
                        } else if (localLabel[1].Equals("abs")) {
                            i += (UInt16)defines[localLabel[0]];
                        }
                    } else {
                        i = addAddr(i, subNext, addNext, out subNext, out addNext, defines[localLabel[0]], s);
                    }
                    continue;
                }

                //check if string is a constant
                int res;
                if (Util.tryParseTo<Int32>(str, out res)) {
                    if (addNext) {
                        i += res;
                        addNext = false;
                    } else if (subNext) {
                        i -= res;
                        subNext = false;
                    } else {
                        throw new FormatException("A numerical constant must either be first in a line, or follow a '+' or '-': " + s);
                    }
                    continue;
                }
                throw new FormatException("Could not parse: " + str + " in: " + s + ". Too large or unrecognised label");
            }
            return i;
        }
        private int addAddr(int i, bool subNext, bool addNext, out bool subOut, out bool addOut, int tmp, String s)
        {
            if (addNext) {
                i += tmp;
                addOut = false;
                subOut = subNext;
            } else if (subNext) {
                i -= tmp;
                subOut = false;
                addOut = addNext;
            } else {
                throw new FormatException("A label must either be at the start of an adress field or following a '+' or '-': " + s);//there are an exreme ammount of error cases that needs to be reported separately in this function. This is awful.
            }
            return i;
        }

        Int32[] evaluateNumerical(String s)
        {
            String[] strs = s.Split(' ');
            Int32[] res = new Int32[strs.Length];
            for (int i = 0; i < strs.Length; ++i) {
                String str = strs[i];
                try {
                    if (defines.ContainsKey(str)) { //check if line is in defines
                        res[i] = defines[str];
                    } else if (str.Contains(".")) { //Decimal point means we're treating this as a float
                        res[i] = Util.ftoi(Util.parseTo<Single>(str));
                    } else {
                        res[i] = (Int32)Util.parseTo<UInt32>(str);
                    }
                }
                catch (Exception) {
                    throw new FormatException("Not a parseable number: " + str);
                }
            }
            return res;
        }

        //load all options from meta data
        private bool[] assignOpts(LinkedList<String> meta)
        {
            bool[] opts = new bool[5];
            opts[0] = false;//PIC
            opts[1] = true;//MAINJMP
            opts[2] = true;//AUTOSTACK
            opts[3] = false;//ALWERR
            opts[4] = false;//AUTLDR
            bool[] assigned = new bool[5];//has the above values been assigned;
            //read the meta part of the fileData, and determine how the program should be compiled
            foreach (String s in meta) {
                String[] pars = s.Split('=');
                if (pars.Length < 2 || pars.Length > 2) {
                    throw new FormatException("Incorrect formatting in meta field: " + s);
                }
                switch (pars[0]) {
                    case "PIC":
                        if (assigned[0]) {
                            //just in case
                            bool b = opts[0];
                            if (optsAssignment(opts, 0, pars[1]) != b) {
                                throw new FormatException("Contradictory assignements in meta field: " + s);
                            }
                        } else {
                            assigned[0] = true;
                            optsAssignment(opts, 0, pars[1]);
                        }
                        break;
                    case "MAINJMP":
                        if (assigned[1]) {
                            //just in case
                            bool b = opts[1];
                            if (optsAssignment(opts, 1, pars[1]) != b) {
                                throw new FormatException("Contradictory assignements in meta field: " + s);
                            }
                        } else {
                            assigned[1] = true;
                            optsAssignment(opts, 1, pars[1]);
                        }
                        break;
                    case "AUTOSTACK":
                        if (assigned[2]) {
                            //just in case
                            bool b = opts[2];
                            if (optsAssignment(opts, 0, pars[1]) != b) {
                                throw new FormatException("Contradictory assignements in meta field: " + s);
                            }
                        } else {
                            assigned[2] = true;
                            optsAssignment(opts, 2, pars[1]);
                        }
                        break;
                    case "ALWERR":
                        if (assigned[3]) {
                            //just in case
                            bool b = opts[0];
                            if (optsAssignment(opts, 3, pars[1]) != b) {
                                throw new FormatException("Contradictory assignements in meta field: " + s);
                            }
                        } else {
                            assigned[3] = true;
                            optsAssignment(opts, 3, pars[1]);
                        }
                        break;
                    case "AUTLDR":
                        if (assigned[4]) {
                            //just in case
                            bool b = opts[4];
                            if (optsAssignment(opts, 4, pars[1]) != b) {
                                throw new FormatException("Contradictory assignements in meta field: " + s);
                            }
                        } else {
                            assigned[4] = true;
                            optsAssignment(opts, 4, pars[1]);
                        }
                        break;
                    default:
                        throw new FormatException("Unrecognized assignement in meta field: " + s);
                }
            }
            return opts;
        }
        //helper functions for optsAssignment
        private bool optsAssignment(bool[] opts, int pos, String str)
        {
            if (str.Equals("FALSE")) {
                opts[pos] = false;
                return false;
            } else if (str.Equals("TRUE")) {
                opts[pos] = true;
                return true;
            }
            throw new FormatException("String not a boolean expression: " + str);
        }
        //reads a file and returns each line as a separate string in a linked list
        private LinkedList<String> readFile(String fileName)
        {
            KSP.IO.TextReader txtReader = KSP.IO.TextReader.CreateForType<Assembler2>(fileName);
            LinkedList<String> lines = new LinkedList<String>();
            while (true) {
                String nextLine = txtReader.ReadLine();
                if (nextLine == null)
                    break;
                else {
                    nextLine = performCulling(nextLine);
                    if (nextLine.Equals(""))
                        continue;
                }
                lines.AddLast(nextLine);
            }
            txtReader.Close();
            return lines;
        }
        //removes comments and unnesecary whitespace
        private String performCulling(String s)
        {
            //remove all comments
            s = Util.cutStrAfter(s, ";");

            //remove leading and trailing whitespace
            s = s.Trim();

            //now we can check if the line is a string
            if (s.StartsWith("#string")) {
                return s;//this should work
            }

            //make sure + and - have spaced around them
            s = s.Replace("+", " + ");
            s = s.Replace("-", " - ");

            //cut unneccesary whitespace
            Regex r = new Regex("\\s+");
            s = r.Replace(s, " ");

            //make sure the user has not been silly and put a space before %
            s = s.Replace(" %", "%");
            
            return s;
        }
        //splits the file into an object containing all three different fields of the text 
        private FileData metadatatextSplit(LinkedList<String> file, String fileName)
        {
            int mode = 0;
            FileData f = new FileData();
            String str = "";
            foreach (String s in file) {
                str = str + "\n" + s;

                if (s.Equals(".meta")) {
                    mode = 1;
                    continue;
                } else if (s.Equals(".data")) {
                    mode = 2;
                    continue;
                } else if (s.Equals(".text")) {
                    mode = 3;
                    continue;
                }
                switch (mode) {
                    case 0:
                        throw new FormatException("text outside of specified field in " + fileName + " :\"" + str + "\"");
                    case 1:
                        f.addMeta(s);
                        break;
                    case 2:
                        f.addData(s);
                        break;
                    case 3:
                        f.addInst(s);
                        break;
                }
            }
            return f;
        }
        // turn all labels not declared global into local labels
        // this is done by adding ":<filename>" after each mentioning of the stuff
        private void localise(FileData f, String filename)
        {
            //simple solution:
            // 1: find each label name
            // 2: for each label not declared global, do a find and replace on each line for the localised string with a space before
            //go through the file once and remove all the #global labels
            LinkedList<String> tmp = new LinkedList<String>();
            LinkedList<String> labels = new LinkedList<String>();
            foreach (String s in f.inst) {
                findLabels(f.globals, labels, tmp, s);
            }
            f.inst = tmp;
            tmp = new LinkedList<string>();
            foreach (String s in f.data) {
                findLabels(f.globals, labels, tmp, s);
            }
            f.data = tmp;

            //read through instructions and data and replace all non-global labels with local ones
            tmp = new LinkedList<string>();
            foreach (String s in f.inst) {
                String s2 = s;
                foreach (String l in labels) {
                    if (!f.globals.Contains(l))//change this to only run once later
                {
                        s2 = s2.Replace(l, l + ":" + filename);
                    }
                }
                tmp.AddLast(s2);
            }
            f.inst = tmp;

            tmp = new LinkedList<string>();
            foreach (String s in f.data) {
                if (s.StartsWith("#string")) {
                    tmp.AddLast(s);
                    continue;//avoid replacing content in strings
                }
                String s2 = s;
                foreach (String l in labels) {
                    if (!f.globals.Contains(l))//change this to only run once later
                {
                        s2 = s2.Replace(l, l + ":" + filename);
                    }
                }
                tmp.AddLast(s2);
            }
            f.data = tmp;

        }
        //goes through a string and files it in the appropriate linked list
        private void findLabels(LinkedList<String> glob, LinkedList<String> lab, LinkedList<String> lineList, String s)
        {
            if (s.StartsWith("#global")) {
                String[] strs = s.Split(' ');
                if (strs.Length > 2) {
                    throw new FormatException("Too many arguments in #global declaration: " + s);
                }
                glob.AddLast(strs[1]);
            } else if (s.EndsWith(":")) {
                lab.AddLast(s.Substring(0, s.Length - 1));
                lineList.AddLast(s);
            } else if (s.StartsWith("#define")) {
                //needs three parameters: #define, name and value
                String[] strs = s.Split(' ');
                if (strs.Length != 3)
                    throw new FormatException("Invalid ammount of parameters in #define statement: " + s);
                int a;
                if (Util.tryParseTo<Int32>(strs[2], out a)) {
                    //add the stuff to defines
                    defines.Add(strs[1], a);
                } else {
                    throw new FormatException("Not a Number: " + strs[2] + " in " + s);
                }

            } else {
                lineList.AddLast(s);
            }
        }
        //split the string until the first whitespace, and then split on subsequent commas. Return the array of Strings that is the result of this operation.
        private static String[] stringParameterSplit(String s)
        {
            if (s.Contains(" ")) {
                s = Util.cutStrBefore(s, " ");
                String[] strs = s.Split(',');
                for (int i = 0; i < strs.Length; ++i) {
                    strs[i] = strs[i].Trim();
                }
                return strs;
            }
            return new String[0];
        }
    }
}