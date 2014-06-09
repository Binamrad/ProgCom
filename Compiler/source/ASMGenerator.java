import java.util.LinkedList;


public class ASMGenerator {
	ASMCode out;
	VariableContext vars;
	LinkedList<Variable> globals;
	public ASMGenerator(ASMCode c) {
		out = c;
		globals = new LinkedList<Variable>();
		vars = new VariableContext(out, globals);
	}
	/**********************************************************/
	//helper functions
	private void addInst3im(String inst, String param1, String param2, String param3) {
		String reg1, reg2;
		reg1 = vars.load(param1);
		reg2 = vars.load(param2);
		out.add(inst, reg1, reg2, param3);
	}
	
	private void addInst3imFree(String inst, String param1, String param2, String param3) {
		String reg1, reg2;
		reg2 = vars.load(param2);
		reg1 = vars.freeVar(param1);
		out.add(inst, reg1, reg2, param3);
	}
	
	private void addInst3imimFree(String inst, String param1, String param2, String param3) {
		String reg1;
		reg1 = vars.freeVar(param1);
		out.add(inst, reg1, param2, param3);
	}
	
	private void branch_simple(String param1, String param2) {
		String reg1 = vars.load(param1);
		out.add("bi", reg1, param2);
	}
	
	public void branch_plain(String param1) {
		out.add("br", param1);
	}
	
	private void addInst3(String inst, String param1, String param2, String param3) {
		String reg1, reg2, reg3;
		reg2 = vars.load(param2);
		reg3 = vars.load(param3);
		reg1 = vars.freeVar(param1);
		out.add(inst, reg1, reg2, reg3);
	}
	
	private void addInst2(String inst, String param1, String param2) {
		String reg1, reg2;
		reg2 = vars.load(param2);
		reg1 = vars.freeVar(param1);
		out.add(inst, reg1, reg2, null);
	}
	
	private void addInst2im(String inst, String param1, String param2) {
		String reg1;
		reg1 = vars.freeVar(param1);
		out.add(inst, reg1, param2);
	}
	
	private void addInst1(String inst, String param1, boolean load) {
		String reg1;
		if(load) reg1 = vars.load(param1);
		else reg1 = vars.freeVar(param1);
		out.add(inst, reg1, null, null);
	}
	
	/**********************************************************/
	//Arithmetic
	public void add(String dest, String param1, String param2, String type) {
		if(type.equals("float")) {
			addInst1("fpush",param1, true);
			addInst1("fpush",param2, true);
			out.add("fadd");
			addInst1("fpop",dest, false);
		} else {
			addInst3("add", dest, param1, param2);
			if(type.equals("char")) {
				addInst3im("andi", dest, dest, "255");
			}
		}
	}
	
	public void add_lit(String dest, String param1, int param2, String type) {
		if(type.equals("float")) throw new InternalCompilerException("float type in literal add");
		addInst3imFree("addi", dest, param1, ""+param2);
	}
	
	public void sub(String dest, String param1, String param2, String type) {
		if(type.equals("float")) {
			addInst1("fpush",param1, true);
			addInst1("fpush",param2, true);
			out.add("fsub");
			addInst1("fpop",dest, false);
		} else {
			addInst3("sub", dest, param1, param2);
			if(type.equals("char")) {
				addInst3im("andi", dest, dest, "255");
			}
		}
	}
	
	public void sub_lit(String dest, String param1, int param2, String type) {
		if(type.equals("float")) throw new InternalCompilerException("float type in literal subtract");
		addInst3imFree("subi", dest, param1, ""+param2);
	}
	
	public void mul(String dest, String param1, String param2, String type) {
		if(type.equals("float")) {
			addInst1("fpush",param1, true);
			addInst1("fpush",param2, true);
			out.add("fmul");
			addInst1("fpop",dest, false);
		} else {
			addInst3("mul", dest, param1, param2);
			if(type.equals("char")) {
				addInst3im("andi", dest, dest, "255");
			}
		}
	}
	
	public void mul_lit(String dest, String param1, int param2, String type) {
		if(type.equals("float")) throw new InternalCompilerException("float type in literal multiply");
		addInst3imFree("muli", dest, param1, ""+param2);
	}
	
	public void div(String dest, String param1, String param2, String type) {
		if(type.equals("float")) {
			addInst1("fpush",param1, true);
			addInst1("fpush",param2, true);
			out.add("fdiv");
			addInst1("fpop",dest, false);
		} else {
			addInst3("div", dest, param1, param2);
		}
	}
	
	public void div_lit(String dest, String param1, int param2, String type) {
		if(type.equals("float")) throw new InternalCompilerException("float type in literal divide");
		addInst3imFree("divi", dest, param1, ""+param2);
	}
	
	public void mod(String dest, String param1, String param2, String type) {
		if(type.equals("float")) throw new SyntaxErrorException("Operator % is not defined for variables of type float");
		
		String reg1, reg2;
		reg1 = vars.load(param1);
		reg2 = vars.load(param2);
		//String destReg = vars.freeVar(dest);
		out.add("div", "r0", reg1, reg2);
		addInst2im("mov", dest, "ex");
	}
	
	public void mod_lit(String dest, String param1, int param2, String type) {
		if(type.equals("float")) throw new SyntaxErrorException("Operator % is not defined for variables of type float");
		
		String reg1;
		reg1 = vars.load(param1);
		//String destReg = vars.freeVar(dest);
		out.add("divi", "r0", reg1, ""+param2);
		addInst2im("mov", dest, "ex");
	}
	
	public void increment(String var) {
		String reg = vars.load(var);
		vars.alter(var);
		out.add("addi", reg, reg, "1");
	}
	
	public void decrement(String var) {
		String reg = vars.load(var);
		vars.alter(var);
		out.add("subi", reg, reg, "1");
	}
	
	public void assign(String dest, String param1) {
		addInst2("mov", dest, param1);
	}
	
	public void indirectAssign_wr(String dest, String param1) {
		addInst3im("wr", param1, dest, "0");
	}
	
	public void dereference(String dest, String param1, int offset) {
		String reg1 = vars.freeVar(dest);
		String reg2 = vars.load(param1);
		out.add("rd", reg1, reg2, ""+offset);
	}
	
	public void dereference_tworeg(String dest, String param1, String param2) {
		String reg1 = vars.freeVar(dest);
		String reg2 = vars.load(param1);
		String reg3 = vars.load(param2);
		out.add("rdr", reg1, reg2, reg3);
		
	}
	
	public void addressOf(String dest, String param1, boolean force) {
		//outputs assembly code automagically
		if(force) addInst2im("movi", dest, param1);
		else vars.addressOf(dest, param1);
	}
	
	public void or(String dest, String param1, String param2, String type) {
		if(type.equals("float")) throw new SyntaxErrorException("Operator | is not defined for variables of type float");
		addInst3("or", dest, param1, param2);
	}
	
	public void or_lit(String dest, String param1, int param2, String type) {
		if(type.equals("float")) throw new SyntaxErrorException("Operator >> is not defined for variables of type float");
		addInst3imFree("ori", dest, param1, ""+param2);
	}
	
	public void rshift(String dest, String param1, String param2, String type) {
		if(type.equals("float")) throw new SyntaxErrorException("Operator >> is not defined for variables of type float");
		addInst3("shr", dest, param1, param2);
	}
	
	public void rshift_lit(String dest, String param1, int param2, String type) {
		if(type.equals("float")) throw new SyntaxErrorException("Operator >> is not defined for variables of type float");
		addInst3imFree("sri", dest, param1, ""+param2);
	}
	
	public void lshift(String dest, String param1, String param2, String type) {
		if(type.equals("float")) throw new SyntaxErrorException("Operator << is not defined for variables of type float");
		addInst3("shl", dest, param1, param2);
	}
	
	public void lshift_lit(String dest, String param1, int param2, String type) {
		if(type.equals("float")) throw new SyntaxErrorException("Operator << is not defined for variables of type float");
		addInst3imFree("sli", dest, param1, ""+param2);
	}
	
	public void xor(String dest, String param1, String param2, String type) {
		if(type.equals("float")) throw new SyntaxErrorException("Operator ^ is not defined for variables of type float");
		addInst3("xor", dest, param1, param2);
	}
	
	public void xor_lit(String dest, String param1, int param2, String type) {
		if(type.equals("float")) throw new SyntaxErrorException("Operator ^ is not defined for variables of type float");
		addInst3imFree("xori", dest, param1, ""+param2);
	}
	
	public void and(String dest, String param1, String param2, String type) {
		if(type.equals("float")) throw new SyntaxErrorException("Operator & is not defined for variables of type float");
		addInst3("and", dest, param1, param2);
	}
	
	public void and_lit(String dest, String param1, int param2, String type) {
		if(type.equals("float")) throw new SyntaxErrorException("Operator & is not defined for variables of type float");
		addInst3imFree("andi", dest, param1, ""+param2);
	}
	
	//return 1 if param1 and param2 non-zero, 0 otherwise
	public void land(String dest, String param1, String param2, String type) {
		String regDest = vars.freeVar(dest);
		String reg1 = vars.load(param1);
		String reg2 = vars.load(param2);
		String regTemp;
		if(regDest.equals(reg1) || regDest.equals(reg2)) {
			regTemp = "ex";
		} else {
			regTemp = regDest;
		}
		out.add("movi", regTemp, "1");
		out.add("bne", reg1, "r0", "1");
		out.add("movi", regTemp, "0");
		out.add("bne", reg2, "r0", "1");
		out.add("movi", regTemp, "0");
		if(!regTemp.equals(regDest)) {
			out.add("mov", regDest, regTemp);
		}
	}
	
	//return 1 if param1 or param2 non-zero, 0 otherwise
	public void lor(String dest, String param1, String param2, String type) {
		String regDest = vars.freeVar(dest);
		String reg1 = vars.load(param1);
		String reg2 = vars.load(param2);
		String regTemp;
		if(regDest.equals(reg1) || regDest.equals(reg2)) {
			regTemp = "ex";
		} else {
			regTemp = regDest;
		}
		out.add("movi", regTemp, "0");
		out.add("beq", reg1, "r0", "1");
		out.add("ori", regTemp, regTemp, "1");
		out.add("beq", reg2, "r0", "1");
		out.add("ori", regTemp, regTemp, "1");
		if(!regTemp.equals(regDest)) {
			out.add("mov", regDest, regTemp);
		}
	}
	
	//return 1 if precisely 1 of param1 and param2 non-zero, 0 otherwise
	public void lxor(String dest, String param1, String param2, String type) {
		String regDest = vars.freeVar(dest);
		String reg1 = vars.load(param1);
		String reg2 = vars.load(param2);
		String regTemp;
		if(regDest.equals(reg1) || regDest.equals(reg2)) {
			regTemp = "ex";
		} else {
			regTemp = regDest;
		}
		out.add("movi", regTemp, "0");
		out.add("beq", reg1, "r0", "1");
		out.add("xori", regTemp, regTemp, "1");
		out.add("beq", reg2, "r0", "1");
		out.add("xori", regTemp, regTemp, "1");
		if(!regTemp.equals(regDest)) {
			out.add("mov", regDest, regTemp);
		}
	}
	
	//this should probably be logical not, not equal to r0
	public void not(String dest, String param1, String type) {
		if(type.equals("float")) addInst3im("flcmp", dest, param1, "r0");
		else addInst3im("cmp", dest, param1, "r0");
		addInst1("not", dest, true);
		addInst3im("andi", dest, dest, "1");
	}
	
	public void negate(String dest, String param1, String type) {
		if(type.equals("float")) {
			String reg = vars.load(param1);
			out.add("fpush", "r0");
			out.add("fpush", reg);
			out.add("fsub");
			reg = vars.freeVar(dest);
			out.add("fpop", reg);
		} else {
			String reg1 = vars.load(param1);
			String reg2 = vars.freeVar(dest);
			out.add("sub",reg2, "r0", reg1);
		}
	}
	
	public void typeConvert(String dest, String src, String typeFrom, String typeTo) {
		if(typeFrom.equals(typeTo)) {
			if(dest != src) {
				assign(dest, src);
			}
			return;
		}
		boolean done = false;
		String regFrom = vars.load(src);
		String regDest = vars.freeVar(dest);
		if(typeFrom.equals("float")) {
			out.add("fpush", regFrom);
			out.add("ftoi");
			out.add("fpop",regDest);
			regFrom = regDest;
			typeFrom = "int";
			done = true;
		}
		if(typeFrom.equals(typeTo)) return;
		if(typeTo.equals("char")) {
			out.add("andi",regDest, regFrom, "255");
			done = true;
		} else if(typeTo.equals("float")) {
			out.add("fpush", regFrom);
			out.add("ftof");
			out.add("fpop",regDest);
			done = true;
		}
		if(!done) {
			if(dest != src) {
				assign(dest, src);
			}
		}
	}
	
	/*********************************************************/
	//comparison instructions 
	public void cmp_less(String dest, String param1, String param2, String type) {
		vars.freeVar(dest);
		if(type.equals("float") || dest.equals(param1) || dest.equals(param2)) {
			if(type.equals("float")) {
				addInst3("flcmp", dest, param1, param2);
			} else {
				addInst3("cmp", dest, param1, param2);
			}
			addInst3im("addi", dest, dest, "1");
			addInst3im("cmp", dest, dest, "r0");
			addInst3im("xori", dest, dest, "1");
		} else {
			addInst2im("movi", dest, "0");
			addInst3im("ble", param2, param1, "1");
			addInst2im("movi", dest, "1");
		}
	}
	
	public void cmp_more(String dest, String param1, String param2, String type) {
		cmp_less(dest, param2, param1, type);
	}
	
	public void cmp_lesseq(String dest, String param1, String param2, String type) {
		vars.freeVar(dest);
		if(type.equals("float") || dest.equals(param1) || dest.equals(param2)) {
			if(type.equals("float")) {
				addInst3("flcmp", dest, param1, param2);
			} else {
				addInst3("cmp", dest, param1, param2);
			}
			addInst3im("subi", dest, dest, "1");
			addInst3im("andi", dest, dest, "2");
			addInst3im("sri", dest, dest, "1");
		} else {
			addInst2im("movi", dest, "0");
			addInst3im("bl", param2, param1, "1");
			addInst2im("movi", dest, "1");
		}
	}
	
	public void cmp_moreeq(String dest, String param1, String param2, String type) {
		cmp_lesseq(dest, param2, param1, type);
	}
	
	public void cmp_equal(String dest, String param1, String param2, String type) {
		if(type.equals("float")) addInst3("flcmp", dest, param1, param2);
		else addInst3("cmp", dest, param1, param2);
		addInst1("not", dest, true);
		addInst3im("andi", dest, dest, "1");
	}
	
	public void cmp_notequal(String dest, String param1, String param2, String type) {
		if(type.equals("float")) addInst3("flcmp", dest, param1, param2);
		else addInst3("cmp", dest, param1, param2);
		addInst3im("andi", dest, dest, "1");
	}
	
	/**********************************************************/
	//variable declaration
	public void createVar(String name) {
		LinkedList<String> variables = new LinkedList<String>();
		variables.push(name);
		vars.create(variables);
		
	}
	
	//creates some variables and links them with registers in memory.
	public void createAndArrange(LinkedList<String> varNames) {
		
		if(varNames.size() == 0) return;//there was an anomalous "addi sp, sp, 0" in generated code, this fixed it.
		/*String s = "";
		for(String s2 : varNames) {
			//create some sort of comment here eventually
		}*/
		vars.create(varNames);
		//arrange all variables
		arrange(varNames, false);
	}
	
	public void arrange(LinkedList<String> names, boolean readVars) {
		//don't spend any time in this function if names is empty
		if(names.size() == 0) return;
		
		//create a comment
		String s = "alloc:";
		for(String s2 : names) {
			s = s + " " + s2;
		}
		comment(s);
		vars.arrange(names, readVars);
	}
	
	public void createVariables(LinkedList<String> names) {
		vars.create(names);
	}
	
	/****************************************************************/
	//branching
	public void iftype_branch(String label, String bool) {
		String reg1 = vars.load(bool);
		out.add("beq", reg1, "r0", label);
	}
	
	public void putLabel(String label) {
		out.put(label+":");
	}
	
	public void storeVars() {
		vars.storeAll();
	}
	
	public void clearVars() {
		vars.clearAll();
	}
	
	public void functionCall(String fname) {
		out.put("\tcall\t"+fname);
		out.setHasFCall();
	}
	public void functionPointerCall(String varName) {
		vars.load(varName);
		out.put("\tcallr\t"+vars.load(varName));
		out.setHasFCall();
	}
	
	//conditional branches
	public void branch_notequal(String param1, String param2, String label) {
		addInst3im("bne", param1, param2, label);
	}
	
	public void branch_equal(String param1, String param2, String label) {
		addInst3im("beq", param1, param2, label);
	}
	
	public void branch_less(String param1, String param2, String label) {
		addInst3im("bl", param1, param2, label);
	}
	
	public void branch_lesseq(String param1, String param2, String label) {
		addInst3im("ble", param1, param2, label);
	}
	
	public void branch_moreq(String param1, String param2, String label) {
		branch_lesseq(param2, param1, label);
	}
	
	public void branch_more(String param1, String param2, String label) {
		branch_less(param2, param1, label);
	}
	
	/*******************************************************************/
	//data
	
	//adds a string to the data table, moves a char# variable into dest
	public void addData_String(String dest, String str) {
		String label = out.addString(str);
		addInst2im("movi", dest, label);
	}
	
	//sets an integer into variable dest
	public void immediateInt(String dest, int value) {
		//assign to variable
		if(value >= 0 && value < 2097152) {
			addInst2im("movil",dest,""+value);
		} else if (value < 0 && value >= -0xffff) {
			addInst3imimFree("subi", dest, "r0", ""+-value);
		} else {
			//split the variable in two
			comment(dest+"="+value);
			addInst2im("movhi",dest,""+((value & 0xffff0000)>>>16));
			addInst3im("ori", dest, dest, ""+(value&0xffff));
		}
	}
	
	//sets dest to a float
	public void immediateFloat(String dest, float value) {
		comment(dest+"="+value);
		int res = Float.floatToRawIntBits(value);
		
		//split the variable in two
		addInst2im("movhi", dest, ""+((res & 0xffff0000)>>>16));
		addInst3im("ori", dest, dest, ""+(res&0xffff));
	}
	
	/******************************************************************/
	//other
	public void functionStart(String name) {
		//todo: make sure we start writing to a new function block here
		out.beginFunction(name);
	}
	public void functionEnd() {
		// in here: commit allocated variables to output
		// merge function data in to everything else
		// all other cleanup operations too
		out.endFunction(vars.getStackSize());
	}
	
	public void functionReturn() {
		//TODO: when we add the possibility for callee-stored registers, they need to be restored here
		comment("return");
		out.functionReturn();
	}
	
	public void comment(String s) {
		out.put(";"+s);
	}
	
	//remove all variables from the variables
	public void deleteVars() {
		vars = new VariableContext(out, globals);
	}
	
	public void putASM(String s) {
		out.put(s);
	}
	
	public void declareGlobal(String name, String type, int value) {
		out.addGlobal(""+value, name);
		globals.add(new Variable(name, type));
		vars.createGlobal(name);
	}
	
	public void declareGlobalFloat(String name, float value) {
		out.addGlobal(""+value, name);
	}
	
	public Tuple<String[], Boolean[]> saveState() {
		return vars.getState();
	}
	
	public void restoreState(Tuple<String[], Boolean[]> state) {
		vars.restore(state);
	}
	
	public void setState(Tuple<String[], Boolean[]> state) {
		vars.setState(state);
	}
	
	public void unload(String varName) {
		vars.unload(varName);
	}
	
	
	public void noSave(String var) {
		vars.unAlter(var);
	}
	
	public void storeAllGlobals() {
		vars.storeAllGlobals();
	}
	
	public void store(String var) {
		vars.store(var);
	}
	
	public int varCount() {
		return vars.varCount();
	}
	
	public void deadCode(boolean isCodeDead) {
		out.deadCode(isCodeDead);
	}
}
