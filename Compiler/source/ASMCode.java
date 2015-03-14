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
	

	
	private class stdLineWrapper implements lineWrapper {
		private String[] lines;
		@Override
		public String compile(int stacksize, boolean funcCall) {
			String s = lines[0];
			if(!(lines[0].startsWith("\t") || lines[0].startsWith(";") || lines[0].endsWith(":")) ) {
				s = "\t"+s;
			}
			if(lines.length > 1) {
				s+="\t"+lines[1];
			}
			for(int i = 2; i < lines.length; ++i) {
				s+=", " + lines[i];
			}
			return s;
		}
		public stdLineWrapper(String[] s) {
			lines = s;
		}
		@Override
		public String getComponent(int i) {
			if(i >= lines.length) {
				throw new InternalCompilerException("WOAH, DID THE OPTIMISER JUST DO SOMETHING STUPID??:" + i + " WHEN LENGTH IS " + lines.length);
			}
			return lines[i];
		}
		@Override
		public int getComponents() {
			return lines.length;
		}
		@Override
		public void setComponent(int i, String component) {
			lines[i] = component;
			
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

		@Override
		public String getComponent(int i) {
			// TODO Auto-generated method stub
			return "jmpr";
		}

		@Override
		public int getComponents() {
			return 1;
		}

		@Override
		public void setComponent(int i, String component) {
			// TODO Auto-generated method stub
			
		}
	}
	
	public void add(String inst, String reg1, String reg2, String reg3) {
		if(deadCode) return;
		if(Optimise.isBranch(inst) && !inst.equals("call") && !inst.equals("callr")) {
			Optimise.optimiseUntilBranch(functionLines, 32, false);
		}
		
		
		String[] lines;
		if(reg1 != null) {
			if(reg2 != null) {
				if(reg3 != null) {
					lines = new String[4];
					lines[0] = inst;
					lines[1] = reg1;
					lines[2] = reg2;
					lines[3] = reg3;
				} else {
					lines = new String[3];
					lines[0] = inst;
					lines[1] = reg1;
					lines[2] = reg2;
				}
			} else {
				lines = new String[2];
				lines[0] = inst;
				lines[1] = reg1;
			}
		} else {
			lines = new String[1];
			lines[0] = inst;
		}
		functionLines.addLast(new stdLineWrapper(lines));
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
		if(!deadCode) {
			String[] lines = new String[1];
			lines[0] = s;
			functionLines.addLast(new stdLineWrapper(lines));
		}
	}
	
	private String fname;
	public void beginFunction(String name) {
		functionLines = new LinkedList<lineWrapper>();
		fname = name;
		hasFCall = false;
	}
	
	public void endFunction(int varCount) {
		varCount = Optimise.removeUnneccessaryStores(functionLines);
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
		//TODO: go through the functionlines and remove all unneeded stores (ie. stores where the stored value is never read
		for(lineWrapper w : functionLines) {
			lines.addLast(w.compile(varCount, hasFCall));
		}
	}
	
	
	public void functionCallOpts(int params) {
		if(deadCode) return;
		hasFCall = true;
		Optimise.optimiseUntilBranch(functionLines, params, true);
	}
	public void setHasFCall() {
		if(deadCode) return;
		hasFCall = true;
	}
	
	public void functionReturn(int params) {
		if(deadCode) return;
		Optimise.removeUnneccessaryStoresPreReturn(functionLines);
		Optimise.optimiseUntilBranch(functionLines, params, true);
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
