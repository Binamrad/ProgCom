import java.io.FileWriter;
import java.io.IOException;
import java.io.PrintWriter;
import java.util.HashMap;
import java.util.LinkedList;



//this code contains the output assembly code that the program outputs.
//will have at least three functions:
//add(string, string, string, string) <- adds an assembly code line
//optimise(int) <- optimises the output code, wherever possible
//write(string) <- writes the code to the desired filename
public class ASMCode {
	LinkedList<String> lines = new LinkedList<String>();
	LinkedList<String> data = new LinkedList<String>();
	
	HashMap<String, String> stringMap = new HashMap<String, String>();
	LinkedList<lineWrapper> functionLines = new LinkedList<lineWrapper>();
	boolean hasFCall;
	
	
	private interface lineWrapper {
		String compile(int stacksize, boolean funcCall);
	}
	
	private class stdLineWrapper implements lineWrapper {
		private String line;
		@Override
		public String compile(int stacksize, boolean funcCall) {
			return line;
		}
		public stdLineWrapper(String s) {
			line = s;
		}
	}
	
	private class functionReturnWrapper implements lineWrapper {
		@Override
		public String compile(int stacksize, boolean funcCall) {
			StringBuilder s = new StringBuilder();
			if(stacksize != 0) {
				s.append("\tmov\tsp, fp");
				s.append(System.getProperty("line.separator"));//system independent newline
				s.append("\tpop\tfp");
				s.append(System.getProperty("line.separator"));
			}
			if(hasFCall) {
				s.append("\tpop\tra");
				s.append(System.getProperty("line.separator"));
			}
			s.append("\tjmpr\tra");
			return s.toString();
		}
	}
	
	public void add(String inst, String reg1, String reg2, String reg3) {
		if(deadCode) return;
		String newInst = "\t"+inst;
		if(reg1 != null) {
			newInst = newInst + "\t" + reg1;
			if(reg2 != null) {
				newInst = newInst + ", " + reg2;
				if(reg3 != null) {
					newInst = newInst + ", " + reg3;
				}
			}
		}
		functionLines.addLast(new stdLineWrapper(newInst));
	}
	
	public void add(String inst, String reg1, String reg2) {
		add(inst, reg1, reg2, null);
	}
	public void add(String inst, String reg1) {
		add(inst, reg1, null, null);
	}
	public void add(String inst) {
		add(inst, null, null, null);
	}
	
	int labIndex = 0;
	
	public String addString(String s) {
		//makes sure we don't have duplicate strings in memory
		if(stringMap.containsKey(s)) return stringMap.get(s);
		//generate a label
		String label = "string" + labIndex++;
		data.addLast(label+":");
		data.addLast("#string " +s);
		stringMap.put(s, label);
		return label;
	}
	
	public void addGlobal(String data, String label) {
		//generate a label
		this.data.addLast(label+":");
		this.data.addLast(data);
	}
	
	public void put(String s) {
		if(!deadCode)
			functionLines.addLast(new stdLineWrapper(s));
	}
	
	private String fname;
	public void beginFunction(String name) {
		functionLines = new LinkedList<lineWrapper>();
		fname = name;
		hasFCall = false;
	}
	
	public void endFunction(int varCount) {
		lines.addLast("#global " + fname);
		lines.addLast(fname+":");
		if(hasFCall) {
			lines.addLast("\tpush\tra");
		}
		if(varCount != 0) {
			lines.addLast("\tpush\tfp");
			lines.addLast("\tmov\tfp, sp");
			lines.addLast("\taddi\tsp, sp, " + varCount);
		}
		for(lineWrapper w : functionLines) {
			lines.addLast(w.compile(varCount, hasFCall));
		}
	}
	
	public void setHasFCall() {
		if(deadCode) return;
		hasFCall = true;
	}
	
	public void functionReturn() {
		if(deadCode) return;
		functionLines.addLast(new functionReturnWrapper());
	}
	
	public void write(String filename) {
		//System.out.println(".text");
		//for(String s : lines) {
		//	System.out.println(s);
		//}
		//System.out.println(".data");
		//for(String s : data) {
		//	System.out.println(s);
		//}
		
		System.out.println("Compilation successful!");
		System.out.println("Writing...");
		try {
			PrintWriter out = new PrintWriter(new FileWriter(filename+".asm"));
			out.println(".text");
			for(String s : lines) {
				out.println(s);
			}
			out.println(".data");
			for(String s : data) {
				out.println(s);
			}
			out.close();
		} catch(IOException e) {
			System.out.println("Writing failed");
			return;
		}
		System.out.println("Program created!");
	}
	
	boolean deadCode;
	public void deadCode(boolean codeIsDead) {
		deadCode = codeIsDead;
		
	}
}
