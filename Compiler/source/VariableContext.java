import java.util.Collection;
import java.util.HashMap;
import java.util.LinkedList;


public class VariableContext {
	private String[] registers;
	private HashMap<String, Integer> varStackPos;
	private HashMap<String, String> globalVars;
	private final int regAmmount = 32-6;//r0, ra, fp, sp, ea, es are not permitted. Once I roll to the next patch, EX will be added to that.
	private final int mustPushLimit = 16;//the point at which we need to restore the registers after we've used them
	private int[] registerAge;
	private boolean[] altered;
	int stackOffset = 0;
	int varsAllocated = 0;
	int maxStackUsed = 0;//highest offset from fp + 1 used in function
	ASMCode asm;
	public VariableContext(ASMCode asm, Collection<Variable> globalVars) {
		//fix global variables
		
		//init all other things
		registers = new String[regAmmount];
		varStackPos = new HashMap<String,Integer>();
		this.globalVars = new HashMap<String, String>();
		for(Variable v : globalVars) {
			this.globalVars.put(v.name, v.name);
		}
		registerAge = new int[regAmmount];
		altered = new boolean[regAmmount];
		resetAlteredList();
		this.asm = asm;
	}
	//will arrange the parameters in r1->rN. if parameters are true, make sure that all variables are properly stored etc.
	public void arrange(LinkedList<String> varNames, boolean dataMove) {
		int i = 0;
		for(String variable : varNames) {
			if(!varExists(variable)) throw new SyntaxErrorException("Variable does not exist: " + variable);
			int varPos = isLoaded(variable);
			
			if(dataMove && varPos != i) {
				if(varNames.contains(registers[i])) {
					int x = 0;
					//find the position x that i is to be stored at, and store that, move i to x
					for(String s : varNames) {
						if(s.equals(registers[i])) {
							if(x==varPos) {//if this is the case we are about to overwrite a variable, we don't want that
								store(i);
								break;
							}
							store(x);
							registers[x] = registers[i];
							registers[i] = null;
							asm.add("mov", regName(x), regName(i));
							if(altered(i)) {
								unAlter(i);
								alter(x);
							}
						}
						++x;
					}
				} else {
					store(i);//make sure variable at i is not lost
				}
			}
			if(varPos == i) {//we don't need to do anything if this is condition is true
				++i;
				continue;
			} else if(varPos != -1 && varPos != -10) {
				if(dataMove) {
					if(!variable.startsWith("__TMP")) {
						store(varPos);
					}
					registers[varPos] = null;
					unAlter(varPos);
					asm.add("mov", regName(i), regName(varPos));//should we update the names of the variables in the various registers here? 
				}
			} else if(varPos == -10) {
				asm.add("mov", regName(i), "r0");
			} else if(dataMove) {
				//make sure to update this to accommodate for global variables when they are added
				if(varStackPos.containsKey(variable)) {
					asm.add("rd", regName(i), "fp", ""+varStackPos.get(variable));
				} else {
					asm.add("rd", regName(i), "r0", globalVars.get(variable));
				}
			}
			//TODO: make sure to update the list of recently used variables here (non-essential)
			//updateUses(i);//is this correct?
			registers[i] = variable;
			if(dataMove) {
				unAlter(i);
			} else {
				//this is the case just after a function returns or when a function is called
				//this will have to be kept in mind when this function is used
				alter(i);
			}
			++i;
		}
	}
	
	//associates a place on the stack with the string.
	public void create(Collection<String> varNames) {
		for(String s : varNames) {
			//tie the name to an address on the stack
			if(varStackPos.containsKey(s) || globalVars.containsKey(s)) throw new SyntaxErrorException("Cannot allocate duplicate entries of variables: " + s);
			
			varStackPos.put(s,stackOffset++);
		}
		//add instruction here:
		//asm.add("addi","sp", "sp", ""+varNames.size());
		varsAllocated += varNames.size();
	}
	
	//associates a label with a variable name
	public void createGlobal(String label) {
		globalVars.put(label, label);
	}
	
	//will load a variable into a register
	public String load(String a) {
		if(!varExists(a)) throw new SyntaxErrorException("Variable does not exist: " + a);
		int i = isLoaded(a);
		if( i == -1) {
			//set i to the register used longest ago
			i = getFree();
			registers[i] = a;
			
			//add instruction:
			if(varStackPos.containsKey(a)) {
				asm.add("rd", regName(i), "fp", ""+varStackPos.get(a));
			} else if(globalVars.containsKey(a)) {
				asm.add("rd", regName(i), "r0", globalVars.get(a));
			} else throw new InternalCompilerException("This error condition is impossible, contact the dev: " + a);
		}
		updateUses(i);
		return regName(i);
	}
	
	//will save a variable and remove its slot in the thingymajig
	public void unload(String a) {
		int i = isLoaded(a);
		if( i != -1) {
			store(i);//store variable
			registers[i] = null;//unload variable
			registerAge[i]+=99;//make register available sooner
		}
		
	}
	//will make sure value of variable is stored
	public void store(String a) {
		if("__ZERO".equals(a)) return;
		int i = isLoaded(a);
		store(i);//call helper method
	}
	private void store(int i) {
		if(i == -1) return;
		if(!altered(i)) return;//do not store if the value has not changed. Value should ALWAYS have changed if we need to store it
		String varName = registers[i];
		if(varName == null) return;
		//change line below to permit global variables somehow
		if(varStackPos.containsKey(varName)) {
			int addr = varStackPos.get(varName);
			asm.add("wr", regName(i), "fp", ""+addr);
			if(addr+1 > maxStackUsed) maxStackUsed = addr+1;
		} else if(globalVars.containsKey(varName)) {
			asm.add("wr", regName(i), "r0", globalVars.get(varName));//this needs to be changed for global variables
		} else throw new InternalCompilerException("Variable does not exist: " + varName);
		//reset altered flag
		unAlter(i);
	}
	//will make sure the variable is loaded, but not actually load any data for it
	public String freeVar(String a) {
		int i = isLoaded(a);
		if(i == -1) {
			i = getFree();
			registers[i] = a;
		}
		updateUses(i);
		alter(i);
		return regName(i);
	}
	//returns the index of a free/ed register
	private int getFree() {
		int i;
		for(i = 0; i < regAmmount && registers[i] != null; ++i);
		if(i == regAmmount) {
			//get register index of oldest variable
			i = getOldest();
			//make sure no data is lost
			store(i);
			//clear register association
			registers[i] = null;
		}
		return i;
	}
	
	//will unassociate all registers with variables, and save all data
	public void clearAll() {
		for(int i = 0; i < regAmmount; ++i) {
			registerAge[i] = 0;//reset age of all registers, not necessarily necessary, but I felt it is good to be tidy.
			store(i);//make sure no data is lost
			registers[i] = null;//un-associate variable names with registers
		}
		//resetAlteredList();//if something broke, fix it here
	}
	
	//return -1 if not loaded, otherwise return register index of variable
	private int isLoaded(String name) {
		if(name == "__ZERO") return -10;
		for(int i = 0; i < regAmmount; ++i) {
			if(name.equals(registers[i])) return i;
		}
		return -1;
	}
	
	//returns the register name given a register number
	private String regName(int i) {
		if(i == -10) return "r0";
		if(i < 12 && i >= 0) {
			return "r" + (i+1);
		} else if (i >= 12 && i < regAmmount){
			return "a" + (i-12);
		}
		throw new InternalCompilerException("Error in register determination, register does not exist: " + i);
		//return "Q1";//should probably throw exception or something here
	}
	//increments the age for all registers by one, except the specified one, which is set to 0
	private void updateUses(int ignore) {
		for(int i = 0; i < regAmmount; ++i) {
			if(i == ignore) {
				registerAge[i] = 0; 
			} else {
				++registerAge[i];
			}
		}
	}
	
	public boolean varExists(String varName) {
		return varStackPos.containsKey(varName) || globalVars.containsKey(varName) || varName.equals("__ZERO");
	}
	
	//returns the index of the register used least recently
	private int getOldest() {
		int oldestPos = -1;
		int oldestAge = -1;
		for(int i = 0; i < regAmmount; ++i) {
			if(registerAge[i] > oldestAge) {
				oldestAge = registerAge[i];
				oldestPos = i;
			}
		}
		return oldestPos;
	}
	
	public void storeAll() {
		for(int i = 0; i < mustPushLimit; ++i) {
			store(i);
		}
		resetAlteredList();
	}
	
	private boolean altered(int i) {
		if(i == -10) return false;
		return altered[i];
	}
	
	private void alter(int i) {
		if(i == -10) throw new SyntaxErrorException("Attempted to assign variable to constant 0");
		altered[i] = true;
	}
	
	public void alter(String varName) {
		int i = isLoaded(varName);
		if(i != -1) {
			alter(i);
		}
	}
	
	
	public void unAlter(String var) {
		int i = isLoaded(var);
		if(i == -1) return;
		unAlter(i);
	}
	
	private void unAlter(int i) {
		altered[i] = false;
	}
	
	private boolean isAltered(String var) {
		for(int i = 0; i < regAmmount; ++i) {
			if(var.equals(registers[i])) {
				return altered[i];
			}
		}
		return false;
	}
	
	public void addressOf(String dest, String a) {
		if(!varExists(a)) throw new InternalCompilerException("Variable does not exist: " + a);//update this when global variables are added
		if(varStackPos.containsKey(a)) {
			int addr = varStackPos.get(a);
			asm.add("addi",freeVar(dest), "fp", ""+addr);
			if(addr+1 > maxStackUsed) maxStackUsed = addr+1;
		} else if(globalVars.containsKey(a)) {
			asm.add("movi", freeVar(dest), globalVars.get(a));//this needs to be changed for global variables
		} else throw new InternalCompilerException("WARNING: varExists IN VariableContext RETURNS THE WRONG DAMN VALUE");
	}
	
	private void resetAlteredList() {
		for(int i = 0; i < regAmmount; ++i) {
			altered[i] = false;
		}
	}
	
	public int getVarCount() {
		return varsAllocated;
	}
	
	public int getStackSize() {
		return maxStackUsed;
	}
	
	//returns a representation of the current state of the registers
	public Tuple<String[], Boolean[]> getState() {
		String[] rcopy = new String[regAmmount];
		int iterator = 0;
		for(String s : registers) {
			rcopy[iterator++] = s;
		}
		
		Boolean[] bcopy = new Boolean[regAmmount];
		iterator = 0;
		for(boolean b : altered) {
			bcopy[iterator++] = b;
		}
		return new Tuple<String[],Boolean[]>(rcopy,bcopy);
	}
	//restores the registers to an earlier point
	public void restore(Tuple<String[],Boolean[]> state) {
		String[] regRestore = state.x;
		Boolean[] altRestore = state.y;
		//TODO:make sure to update the list of recently used variables here
		
		
		//ok, how we do this?
		// 1: all registers that do not match the previous state need to be saved if they have been altered
		for (int i = 0; i < regAmmount; ++i) {
			if(registers[i] == null) continue;
			if(!registers[i].equals(regRestore[i])) {
				//see if we can 'mov' the variable to the right register
				boolean moved = false;
				for(int j = 0; j < regRestore.length; ++j) {
					if(regRestore[j] == null) continue;
					if(registers[i].equals(regRestore[j])) {
						moved = true;
						if(altered(j)) {
							store(j);
							unAlter(j);
						}
						if(altered(i)) { 
							alter(j);
						}
						unAlter(i);
						registers[j] = registers[i];
						registers[i] = null;
						asm.add("mov", regName(j), regName(i));
						break;
					}
				}
				if(!moved) store(i);
			} else if(altered[i] && !altRestore[i]) {//if the variable is altered and should be, don't save. if altered and shouldn't be, save. if variable is not altered, don't save
				store(i);
			}
		}
		//next up: reload all values to the previous state.
		for(int i = 0; i < regAmmount; ++i) {
			if(regRestore[i] == null) {
				registers[i] = null;
				registerAge[i] = 10;//anything other than 0 will do here
				unAlter(i);
				continue;
			} else if (!regRestore[i].equals(registers[i])){
				//load variable from stack or from the global variables
				if(varStackPos.containsKey(regRestore[i])) {
					asm.add("rd", regName(i), "fp", ""+varStackPos.get(regRestore[i]));
				} else {
					asm.add("rd", regName(i), "r0", globalVars.get(regRestore[i]));
				}
				registerAge[i] = 0;
			}
			registers[i] = regRestore[i];
			if(altRestore[i]) alter(i);
		}
	}
	//sets the state to an earlier(?) state. This method will not ensure the integrity of data
	public void setState(Tuple<String[],Boolean[]> state) {
		for(int i = 0; i < regAmmount; ++i) {
			registers[i] = state.x[i];
			altered[i] = state.y[i];
		}
	}
	
	//stores all globals variables
	public void storeAllGlobals() {
		for(int i = 0; i < regAmmount; ++i) {
			if(registers[i] != null) {
				if(globalVars.containsKey(registers[i])) {
					store(i);
				}
			}
		}
	}
	
	public int varCount() {
		int vars = 0;
		for(int i = 0; i < regAmmount; ++i) {
			if(registers[i] != null) ++vars;
		}
		return vars;
	}
}
