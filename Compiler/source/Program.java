import java.io.BufferedReader;
import java.io.FileNotFoundException;
import java.io.FileReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.util.HashMap;
import java.util.LinkedList;



//this class contains information on the program as a whole, as well as the main method
//syntax example:
//def asdfg int a b : int int {
//	int a b c;
//	int# d;
//	d a @ = ;
//  a b c + =;
//	a b +=//equivalent to a = a + b
//	c d # =;
//	i int;
//	i 0 =;
//	while i c < ; {
//		if a d >=; break;
//		i++;
//	}
//	return a b;
//}
//design strategy: keywords left, operators right

public class Program {
	private static LinkedList<String> getIncludeNames(String fileName) {
		LinkedList<String> names = new LinkedList<String>();
		BufferedReader bf;
		if(COMPILER_DEBUG_ENABLED) {
			System.out.println("including " + fileName);
		}
		String filePath = fileName.substring(0,fileName.lastIndexOf('/')+1);
		try {
			bf = new BufferedReader(new FileReader(fileName));
			String s;
			int lines = 0;
			while( (s = bf.readLine()) != null) {
				++lines;
				String[] strs = s.split("\\s+");
				if(strs.length > 0) {
					int offset = 0;
					if(strs[0].equals("")) {
						offset++;
					}
					if(strs.length > offset) {
						if(strs[offset].equals("//")) {
							offset++;
						}
					} else {
						continue;
					}
					
					if(strs.length < 2+offset) {
						if(COMPILER_DEBUG_ENABLED) {
							System.out.println(s + " is too short");
						}
						continue;
					}
					
					if(strs[offset].equals("//#include") || strs[offset].equals("#include")) {
						if(strs.length > 2+offset && !strs[2+offset].startsWith("//")) {
							bf.close();
							throw new SyntaxErrorException("You cannot include multiple files in a single line");
						}
						if(!names.contains(filePath+strs[1+offset])) {
							names.addLast(filePath+strs[1+offset]);
							LinkedList<String> nextList = getIncludeNames(filePath+strs[1+offset]);
							for(String s2 : nextList) {
								names.add(s2);
							}
						}
					} if(strs[offset].equals("//#asminclude") || strs[offset].equals("#asminclude")) {
						if(strs.length > 2+offset && !strs[2+offset].startsWith("//")) {
							bf.close();
							throw new SyntaxErrorException("You cannot include multiple files in a single line");
						}
						//find some way of forcing the ASMGenerator to emit "#include <filename>" from here
					} else {
						if(COMPILER_DEBUG_ENABLED) {
							System.out.println(s + " is not an include");
						}
					}
					
				}
				
			}
			bf.close();
			if(COMPILER_DEBUG_ENABLED) {
				System.out.println("read " + lines + " lines");
			}
		} catch (FileNotFoundException e) {
			throw new SyntaxErrorException("File not found: " + fileName);
		} catch (IOException e) {
			System.out.println("SHIT GOT FUCKED UP SON");
			e.printStackTrace();
			throw new InternalCompilerException("WHAT THE HELL");
		}
		return names;
	}
	
	private final static boolean COMPILER_DEBUG_ENABLED = false;
	public static void main(String[] args) {
		//TODO:
		
		//up next;
		//#asminclude
		//when returning from a function, don't save variables that are not going to be used
		//see if it is possible to remove some really dumb register assignments afterwards:
		//		movi	r1, string14
		//		mov		r2, r1 <- this could be removed
		//		rd		r1, fp, 0 <- this makes that clear
		//		call	findKeyword
		
		//NEED:
		// arithmetical right shift, >>>
		// specify strings and addresses in global variables
		
		//WANT:
		// volatile <- put in asmgen somewhere. will have to wait for emulator rewrite(?)
		// figure out some way of storing the type in function pointers
		// _ will ignore function returns/parameters, is "invalid" variable name, make sure that this is not usable elsewhere
		// optimisation: tail recursion
		// optimisation: in loops where a function call is used, move some variables to higher registers so restore is not needed
		// make it possible to use character literals
		
		//I'm too lazy too look up what the correct thing to check for is
		if(args == null || args.length == 0) {
			//ask the user for a list of files
			System.out.println("Specify a list of filenames separated by spaces: ");
			 BufferedReader br = new BufferedReader(new InputStreamReader(System.in));
			String s = "";
			try {
				s = br.readLine();
			} catch (IOException e) {
				System.out.println("No files specified");
			}
			args = s.split(" ");
		}
		LinkedList<Token> tokens = new LinkedList<Token>();
		LinkedList<String> files = new LinkedList<String>();
		for(String fileName : args) {
			if(!files.contains(fileName)) {
				files.add(fileName);
			}
			files.addAll(getIncludeNames(fileName));
		}
		
		for(String fileName : files) {
			try {
				BufferedReader bf = new BufferedReader(new FileReader(fileName));
				String line = bf.readLine();
				int lineNR = 0;
				while( line != null) {
					TokenScanner t = new TokenScanner(line, lineNR);
					while(t.hasNext()) {
						//System.out.println(t.peek());
						tokens.addLast(t.next());
					}
					//System.out.print("Read: ");
					//System.out.println(line);
					line = bf.readLine();
					++lineNR;
				}
				bf.close();
				
			} catch (FileNotFoundException f) {
				System.out.println("File not Found: " + fileName);
				return;
			} catch (SyntaxErrorException n) {
				System.out.println("Syntax Error:");
				System.out.println(n.getMessage());
				//n.printStackTrace();
				return;
			} catch (InternalCompilerException in) {
				System.out.println("Internal Error:");
				in.printStackTrace();
				return;
			} catch (IOException e) {
				e.printStackTrace();
				return;
			}
		}
		Program p = new Program(tokens);
		p.output(args[0]);
	}
	
	/*********************************************************************************/
	//create a list of tokens
	//create a ListIterator for the token list
	//create ASMCode and ASMGenerator objects
	ASMCode code;
	ASMGenerator gen;
	TypeManager types;
	
	public void output(String filename) {
		if(filename.endsWith(".pcc")) {
			filename = filename.substring(0,filename.length()-4);
		}
		code.write(filename);
	}
	
	public Program(LinkedList<Token> tokens) {
		iterator = 0;
		this.tokens = new Token[tokens.size()];
		int i = 0;
		for(Token t : tokens) {
			this.tokens[i] = t;
			++i;
		}
		variableStack = new LinkedList<String>();
		tmpvarsFree = new LinkedList<String>();
		breakStack = new LinkedList<String>();
		continueStack = new LinkedList<String>();
		globalVars = new LinkedList<Variable>();
		globalVars.add(new Variable("__ZERO", "any"));
		
		types = new TypeManager();
		types.storeVar(new Variable("__ZERO", "any"));
		code = new ASMCode();
		gen = new ASMGenerator(code);
		/**************************************/
		//do preprocessing step
		
		//perform includes of all files included
		
		//due to the way we handle compiling we need to have a complete list of all functions
		functionLookup = getAllFunctions();
		//reset iterator after function scanning
		iterator = 0;
		
		/**************************************/
		//create the program
		System.out.println("Compiling...");
		while(peek() != null) {
			if(!(function() || globalVar())) throw new SyntaxErrorException("syntax error, got: " + peek() + " expected var or def.");
		}
	}
	
	/*********************************************************************/
	//more code below here
	
	
	
	int iterator;
	Token[] tokens;
	HashMap<String, FunctionInfo> functionLookup;
	LinkedList<Variable> globalVars;
	
	//returns a list of function values tied to their things
	private HashMap<String,FunctionInfo> getAllFunctions() {
		HashMap<String, FunctionInfo> functionMap = new HashMap<String, FunctionInfo>();
		while(peek() != null) {
			if(!(peek().type.equals("keyword") && peek().value.equals("def"))) {
				next();
				continue;
			}
			FunctionInfo f = getFunctionInfo();
			functionMap.put(f.name, f);
		}
		return functionMap;
	}
	
	public FunctionInfo getFunctionInfo() {
		//find def, find fname, find type varname alternating, find ":", find types, end at {
		Token t = peek();
		if(!(t.type.equals("keyword") && t.value.equals("def"))) throw new InternalCompilerException("Some idiot removed/forgot to check for \"def\" from line " + t.line + ".\n Shout at the dev to fix this bug");
		next();
		
		//find fname
		String fname;
		t = next();
		if(!t.type.equals("identifier")) throw new SyntaxErrorException("Not a valid function name: " + t.value + " at line " + t.line);
		fname = t.value;
		
		LinkedList<Tuple<String, String>> varThings = new LinkedList<Tuple<String, String>>();
		//find type and then varname until next is 
		for(t = peek();t.type.equals("keyword") && isType(t.value);t = peek()) {
			//next();
			//String type = t.value;
			String type = getType();
			LinkedList<String> varNames = new LinkedList<String>();
			//find varnames
			while(peek().type.equals("identifier")) {
				varNames.add(next().value);
			}
			for(String varName : varNames) {
				varThings.add(new Tuple<String, String>(varName, type));
			}
		}
		//find :
		t = next();
		if(!(t.type.equals("delimeter") && t.value.equals(":"))) throw new SyntaxErrorException("Missing or misplaced ':' in function declaration at line " + t.line);
		
		//after finding ":", find types until we reach ending
		LinkedList<String> returnThings = new LinkedList<String>();
		
		t = peek();
		while(t.type.equals("keyword") && isType(t.value)) {
			//next();
			returnThings.addLast(getType());//t.value);
			t = peek();
		}
		
		//after not type token, return
		
		FunctionInfo finfo = new FunctionInfo(fname, varThings, returnThings);
		return finfo;
	}
	
	
	//methods for handling the iteration through the array of tokens.
	private Token next() {
		if(iterator >= tokens.length) return null;
		return tokens[iterator++];
	}
	
	private Token peek() {
		if(iterator >= tokens.length)return null;
		return tokens[iterator];
	}
	
	/**********************/
	//various helper functions
	
	//destination operand operand operators.
	private boolean isDOOoperator(String s) {
		String[] doo_operators = { "+", "-", "*", "/", "%", "|", "^", "&", "<", ">", "<=", ">=", "<<", ">>", "&&", "||", "^^", "==", "!=" };
		for(String op : doo_operators) {
			if(op.equals(s)) return true;
		}
		return false;
	}
	
	//operations that needs an operator and returns something else to the stack
	//+++, ---, #, <- etc
	private boolean isDOoperator(String s) {
		String[] do_operators = { "+++", "---", "!"};
		for(String op : do_operators) {
			if(op.equals(s)) return true;
		}
		return false;
	}
	
	private boolean hybridAssign(String s) {
		String[] do_operators = {"+=", "-=", "*=", "/=", "|=", "^=", "&="};
		for(String op : do_operators) {
			if(op.equals(s)) return true;
		}
		return false;
	}
	
	//performs something on a signle variable and pushes it back to the stack
	//++, --, 
	private boolean isDoperator(String s) {
		String[] d_operators = { "++", "--", "(-)"};
		for(String op : d_operators) {
			if(op.equals(s)) return true;
		}
		return false;
	}
	
	//checks if the operator is a comparison
	// < > <= >= == != etc
	public boolean isComparison(String s) {
		String[] d_operators = { "<", ">", "<=",">=","==", "!=", "&&", "^^", "||"};
		for(String op : d_operators) {
			if(op.equals(s)) return true;
		}
		return false;
	}
	
	private void assertVarExists(String varName, int line) {
		if(types.getVar(varName) == null) {
			String errMsg = "Variable does not exist: " + varName + " at line " + line +  "\nThe following variables exist:\n";
			
			for(Variable v : types.getAllVars()) {
				errMsg += v + "\n";
			}
			throw new SyntaxErrorException(errMsg);
		}
	}
	
	private boolean isPointerOperation(String s) {
		String[] d_operators = { "<-", "#", "@" };
		for(String op : d_operators) {
			if(op.equals(s)) return true;
		}
		return false;
	}
	/***********/
	//method for each possible condition of the code
	private LinkedList<String> variableStack;
	private LinkedList<String> breakStack;
	private LinkedList<String> continueStack;
	//register states
	private LinkedList<Tuple<String[], Boolean[]>>  whilePreCondState = new LinkedList<Tuple<String[], Boolean[]>>();
	private LinkedList<Tuple<String[], Boolean[]>>  whilePostCondState = new LinkedList<Tuple<String[], Boolean[]>>();
	
	
	//arithmetic that modifies a variable and pushes it back to the stack
	private boolean aritm_d() {
		Token t = peek();
		if(!(t.type.equals("operator") && isDoperator(t.value))) return false;
		next();
		
		if(variableStack.size() < 1) throw new SyntaxErrorException("Not enough variables on stack to use '" + t.value+"'" + " at line " + t.line);
		
		String var = variableStack.pop();
		
		gen.comment(var+t.value);
		
		if(t.value.equals("++")) {
			gen.increment(var);
			//System.out.println("incremented " + var);
		} else if(t.value.equals("--")) {
			gen.decrement(var);
		} else if(t.value.equals("(-)")) {
			String tmp = var;//I cannot be arsed to optimise this at the moment, make sure temporary variables get reused here at some point
			var = getTemporaryVar(types.getVar(var).type);
			gen.negate(var, tmp, types.getVar(tmp).type);
			freeTmpVar(tmp);
		} else {
			throw new InternalCompilerException("Why was this in aritm_d?: " + t.value + " at line " + t.line);
		}
		
		variableStack.push(var);
		return aritm();
	}
	
	//arithmetic on the form: destination operand
	private boolean aritm_do() {
		//example of possible operations: ! +++ --- (+++ = postdecrement etc)
		Token t = peek();
		if(!(t.type.equals("operator") && isDOoperator(t.value))) return false;
		next();
		
		if(variableStack.size() < 1) throw new SyntaxErrorException("Not enough variables on stack to use '" + t.value+"'" + " at line " + t.line);
		
		String var = variableStack.pop();
		assertVarExists(var, t.line);
		String type = types.getVar(var).type;
		
		//awful copy-paste coding here
		String dest;
		if(peek().value.equals("=")) {
			//this will need to be expanded later on
			next();
			dest = variableStack.pop();
			if(!types.typeCompatible(types.getVar(dest).type, type)) throw new SyntaxErrorException("Incompatible types: " + types.getVar(dest).type + " and " + type + " at line " + t.line);
		} else {
			dest = getTemporaryVar(type);
		}
		gen.comment(dest+"="+var+t.value);
		
		if(t.value.equals("+++")) {
			//make copy, set copy on variabestack, increment
			gen.assign(dest, var);
			gen.increment(var);
		} else if(t.value.equals("---")) {
			//make copy, set copy on variabestack, decrement
			gen.assign(dest, var);
			gen.decrement(var);
		} else throw new InternalCompilerException("Why was this in aritm_do?: " + t.value + " from line " + t.line);
		variableStack.push(dest);
		freeTmpVar(var);
		return aritm();
	}
	
	//arithmetic on the form: destination operand
	private boolean aritm_hybridAssign() {
		//example of possible operations: +=, -=, *= etc
		Token t = peek();
		if(!(t.type.equals("operator") && hybridAssign(t.value))) return false;
		next();
		
		
		if(variableStack.size() < 2) throw new SyntaxErrorException("Not enough variables on stack to use '" + t.value+"'" + " at line " + t.line);
		
		String var = variableStack.pop();
		String type = types.getVar(var).type;
		String dest = variableStack.pop();
		
		if(!types.typeCompatible(types.getVar(dest).type, type)) throw new SyntaxErrorException("Incompatible types: " + types.getVar(dest).type + " and " + type + " at line " + t.line);
		gen.comment(dest+t.value+var);
		
		if(t.value.equals("+=")) {
			gen.add(dest, dest, var, type);
		} else if(t.value.equals("-=")) {
			gen.sub(dest, dest, var, type);
		} else if(t.value.equals("*=")) {
			gen.mul(dest, dest, var, type);
		} else if(t.value.equals("/=")) {
			gen.div(dest, dest, var, type);
		} else if(t.value.equals("^=")) {
			gen.xor(dest, dest, var, type);
		} else if(t.value.equals("|=")) {
			gen.or(dest, dest, var, type);
		} else if(t.value.equals("&=")) {
			gen.and(dest, dest, var, type);
		} else throw new InternalCompilerException("NOT A COMPOUND ASSIGNMENT AFTER ALL: " + t.value + " from line " + t.line);
		variableStack.push(dest);
		freeTmpVar(var);
		return aritm();
	}
	
	//arithmetic on the form: destination operand operand
	private boolean aritm_doo() {
		Token t = peek();
		if(COMPILER_DEBUG_ENABLED) {
			System.out.println("aritm_doo: " + t);
		}
		if(!(t.type.equals("operator") && isDOOoperator(t.value))) return false;
		next();
		
		if(variableStack.size() < 2) throw new SyntaxErrorException("Not enough variables on stack to use '" + t.value+"'" + " at line " + t.line);
		//load all appropriate 
		String var2 = variableStack.pop();
		String var1 = variableStack.pop();
		assertVarExists(var1, t.line);
		assertVarExists(var2, t.line);
		
		
		String type1 = types.getVar(var1).type;
		String type2 = types.getVar(var2).type;
		if(!types.typeCompatible(type1, type2)) {
			throw new SyntaxErrorException("Missmatched types: " + var1 + ":" + type1 + ", " + var2 + ":" + type2 + " at line " + t.line);
		}
		String typeResult = types.getTypeResult(type1, type2);
		if(isComparison(t.value)) typeResult = "int";
		
		String dest;
		boolean reuseVar1 = false;
		boolean reuseVar2 = false;
		boolean hasAssigned = false;
		if(peek().value.equals("=")) {
			//this will need to be expanded later on
			hasAssigned = true;
			next();
			dest = variableStack.pop();
			if(!types.typeCompatible(types.getVar(dest).type, typeResult)) throw new SyntaxErrorException("Incompatible types at line " +t.line);
		} else {
			if(var1.startsWith("__TMP")) {
				reuseVar1 = true;
				dest = var1;
				retypeTmpVar(var1, typeResult);
			} else if(var2.startsWith("__TMP")) {
				reuseVar2 = true;
				dest = var2;
				retypeTmpVar(var2, typeResult);
			} else {
				dest = getTemporaryVar(typeResult);
			}
		}
		
		gen.comment(dest+"="+var1+t.value+var2);
		if(t.value.equals("+")) {
			//if the next operation is "#", fold the addition into the next instruction
			Token t2 = peek();
			if(t2.type.equals("operator") && t2.value.equals("#") && !hasAssigned) {
				next();
				//do something to resolve types here
				if(!types.isPointer(typeResult)) {
					throw new SyntaxErrorException("Tried to dereference non-pointer type ("+typeResult+") at line: " + t2.line);
				}
				//do another pass to resolve dest here
				//set typeresut to the right type
				typeResult = typeResult.substring(0, typeResult.length()-1);
				//check if next operator is "="
				t2 = peek();
				if(t2.type.equals("operator") && t2.value.equals("=")) {
					next();
					if(!dest.equals(var1) && !dest.equals(var2)) {
						freeTmpVar(dest);//since we get a new variable, we must free any temporary variables from the stack
					}
					dest = variableStack.pop();
				} else if(dest.startsWith("__TMP")) {
					//reallocate the temporary thing to right
					retypeTmpVar(dest, typeResult);
				}
				//perform typeCheck
				if(!types.typeCompatible(types.getVar(dest).type, typeResult)) throw new SyntaxErrorException("Incompatible types at line " +t2.line);
				gen.comment(dest+"="+"("+var1+"+"+var2+")*");
				gen.dereference_tworeg(dest, var1, var2);
			} else {
				gen.add(dest, var1, var2, typeResult);
			}
		} else if(t.value.equals("-")) {
			gen.sub(dest, var1, var2, typeResult);
		} else if(t.value.equals("/")) {
			gen.div(dest, var1, var2, typeResult);
		} else if(t.value.equals("*")) {
			gen.mul(dest, var1, var2, typeResult);
		} else if(t.value.equals("|")) {
			gen.or(dest, var1, var2, typeResult);
		} else if(t.value.equals("^")) {
			gen.xor(dest, var1, var2, typeResult);
		} else if(t.value.equals("&")) {
			gen.and(dest, var1, var2, typeResult);
		} else if(t.value.equals("&&")) {
			gen.land(dest, var1, var2, typeResult);
		} else if(t.value.equals("^^")) {
			gen.lxor(dest, var1, var2, typeResult);
		} else if(t.value.equals("||")) {
			gen.lor(dest, var1, var2, typeResult);
		} else if(t.value.equals("%")) {
			gen.mod(dest, var1, var2, typeResult);
		} else if(t.value.equals("<")) {
			gen.cmp_less(dest, var1, var2, typeResult);
		} else if(t.value.equals(">")) {
			gen.cmp_more(dest, var1, var2, typeResult);
		} else if(t.value.equals("<=")) {
			gen.cmp_lesseq(dest, var1, var2, typeResult);
		} else if(t.value.equals(">=")) {
			gen.cmp_moreeq(dest, var1, var2, typeResult);
		} else if(t.value.equals("!=")) {
			gen.cmp_notequal(dest, var1, var2, typeResult);
		} else if(t.value.equals("==")) {
			gen.cmp_equal(dest, var1, var2, typeResult);
		} else if(t.value.equals("<<")) {
			gen.lshift(dest, var1, var2, typeResult);
		} else if(t.value.equals(">>")) {
			gen.rshift(dest, var1, var2, typeResult);
		} else {
			throw new InternalCompilerException("THIS SHOULD NOT BE POSSIBLE: operator not implemented: " + t.value + " from line " + t.line);
		}
		
		variableStack.push(dest);
		if(reuseVar1) {
			retypeTmpVar(var1, typeResult);
		} else {
			freeTmpVar(var1);
		}
		if(reuseVar2) {
			retypeTmpVar(var2, typeResult);
		} else {
			freeTmpVar(var2);
		}
		//we need to add this variable and the previous one to the
		return aritm();
	}
	
	//arithmetic on the form: destination operand literal
	private boolean aritm_dooliteral(int literal) {
		Token t = peek();
		if(!(t.type.equals("operator") && isDOOoperator(t.value))) return false;
		next();
		
		if(variableStack.size() < 1) throw new SyntaxErrorException("Not enough variables on stack to use '" + t.value+"'" + " at line " + t.line);
		//load all appropriate 
		String var1 = variableStack.pop();
		assertVarExists(var1, t.line);
		
		
		String type1 = types.getVar(var1).type;
		
		if(!types.typeCompatible(type1, "int")) throw new SyntaxErrorException("missmatched types: " + var1 + ":"+ type1 + ", " + literal + ":int" + " at line " + t.line);
		String typeResult = types.getTypeResult(type1, "int");
		
		boolean reuseVar = false;
		boolean hasAssigned = false;
		String dest;
		if(peek().value.equals("=")) {
			//this will need to be expanded later on
			next();
			dest = variableStack.pop();
			hasAssigned = true;
			if(!types.typeCompatible(types.getVar(dest).type, typeResult)) throw new SyntaxErrorException("Incompatible types at line " +t.line);
		} else {
			if(var1.startsWith("__TMP")) {
				dest = var1;
				retypeTmpVar(dest, typeResult);
				reuseVar = true;
			} else {
				dest = getTemporaryVar(typeResult);
			}
		}
		
		gen.comment(dest+"="+var1+t.value+""+literal);
		if(t.value.equals("+")) {
			//if the next operation is "#", fold the addition into the next instruction
			Token t2 = peek();
			if(t2.type.equals("operator") && t2.value.equals("#") && !hasAssigned) {
				next();
				//do something to resolve types here
				if(!types.isPointer(typeResult)) {
					throw new SyntaxErrorException("Tried to dereference non-pointer type ("+typeResult+") at line: " + t2.line);
				}
				//do another pass to resolve dest here
				//set typeresut to the right type
				typeResult = typeResult.substring(0, typeResult.length()-1);
				//check if next operator is "="
				t2 = peek();
				if(t2.type.equals("operator") && t2.value.equals("=")) {
					next();
					if(!dest.equals(var1)) {
						freeTmpVar(dest);
					}
					dest = variableStack.pop();
				} else if(dest.startsWith("__TMP")) {
					//reallocate the temporary thing to right
					retypeTmpVar(dest, typeResult);
				}
				//perform typeCheck
				if(!types.typeCompatible(types.getVar(dest).type, typeResult)) throw new SyntaxErrorException("Incompatible types at line " +t2.line);
				gen.comment(dest+"="+"("+var1+"+"+literal+")*");
				gen.dereference(dest, var1, literal);
			} else {
				gen.add_lit(dest, var1, literal, typeResult);
			}
		} else if(t.value.equals("-")) {
			gen.sub_lit(dest, var1, literal, typeResult);
		} else if(t.value.equals("/")) {
			gen.div_lit(dest, var1, literal, typeResult);
		} else if(t.value.equals("*")) {
			gen.mul_lit(dest, var1, literal, typeResult);
		} else if(t.value.equals("|")) {
			gen.or_lit(dest, var1, literal, typeResult);
		} else if(t.value.equals("^")) {
			gen.xor_lit(dest, var1, literal, typeResult);
		} else if(t.value.equals("&")) {
			gen.and_lit(dest, var1, literal, typeResult);
		} else if(t.value.equals("%")) {
			gen.mod_lit(dest, var1, literal, typeResult);
		} else if(t.value.equals("<<")) {
			gen.lshift_lit(dest, var1, literal, typeResult);
		} else if(t.value.equals(">>")) {
			gen.rshift_lit(dest, var1, literal, typeResult);
		} else {
			throw new InternalCompilerException("THIS SHOULD NOT BE POSSIBLE: literal operator not implemented: " + t.value + " from line " + t.line);
		}
		
		variableStack.push(dest);
		if(!reuseVar) {
			freeTmpVar(var1);
		}
		//we need to add this variable and the previous one to the
		return aritm();
	}
	
	//arithmetic on the form: destination operand
	private boolean aritm_hybridAssignLiteral(int literal) {
		//example of possible operations: +=, -=, *= etc
		Token t = peek();
		if(!(t.type.equals("operator") && hybridAssign(t.value))) return false;
		next();
		
		
		if(variableStack.size() < 1) throw new SyntaxErrorException("Not enough variables on stack to use '" + t.value+"'" + " at line " + t.line);
		
		String type = "int";
		String dest = variableStack.pop();
		
		if(!types.typeCompatible(types.getVar(dest).type, type)) throw new SyntaxErrorException("Incompatible types: " + types.getVar(dest).type + " and " + type + " at line " + t.line);
		gen.comment(dest+t.value+literal);
		
		if(t.value.equals("+=")) {
			gen.add_lit(dest, dest, literal, type);
		} else if(t.value.equals("-=")) {
			gen.sub_lit(dest, dest, literal, type);
		} else if(t.value.equals("*=")) {
			gen.mul_lit(dest, dest, literal, type);
		} else if(t.value.equals("/=")) {
			gen.div_lit(dest, dest, literal, type);
		} else if(t.value.equals("^=")) {
			gen.xor_lit(dest, dest, literal, type);
		} else if(t.value.equals("|=")) {
			gen.or_lit(dest, dest, literal, type);
		} else if(t.value.equals("&=")) {
			gen.and_lit(dest, dest, literal, type);
		} else throw new InternalCompilerException("NOT A COMPOUND ASSIGNMENT AFTER ALL: " + t.value + " from line " + t.line);
		variableStack.push(dest);
		
		return aritm();
	}
	
	//tries to perform all arithmetic actions, returns true if they succeed
	private boolean aritm() {
		if(COMPILER_DEBUG_ENABLED) {
			System.out.println("next token: "+peek());
			System.out.println("vars on stack: ");
			for(String s : variableStack) {
				System.out.println(types.getVar(s));
			}
		}
			
		return aritm_doo() || aritm_do() || aritm_d() || aritm_hybridAssign() || pointerAritm() || literal() || variable() || assign() || funcCall() || end() || doNothing() || typeCast();
	}
	
	//assigns the value of one variable to another
	private boolean assign() {
		Token t = peek();
		if(!t.value.equals("=")) return false;
		next();
		
		if(variableStack.size() < 2) {
			String returnString = "";
			for(String s : variableStack) {
				returnString += s + "\n";
			}
			throw new SyntaxErrorException("Not enough variables on stack to use operator '=' at line " + t.line +"\nvariables on stack:\n" + returnString);
		}
		String src = variableStack.pop();
		String dest = variableStack.pop();
		
		assertVarExists(src, t.line);
		assertVarExists(dest, t.line);
		
		
		if(!types.typeCompatible(types.getVar(dest).type, types.getVar(src).type)) throw new SyntaxErrorException("Incompatible types: " + types.getVar(dest).type + " and " + types.getVar(src).type + " at line " + t.line);
		
		gen.comment(dest+"="+src);
		gen.assign(dest, src);
		variableStack.push(dest);
		freeTmpVar(src);
		
		return aritm();
	}
	
	//several statements, clumped together
	private boolean block( boolean mustReturn) {
		//find {
		Token t = peek();
		if(!t.value.equals("{")) return false;
		next();
		
		if(COMPILER_DEBUG_ENABLED) {
			System.out.println("BLOCK START");
		}
		
		boolean hasReturn = false;
		while(true) {
			//System.out.println("IN BLOCK: "+peek());
			if(!(inlineasm() || statement())) {//find statements
				if(!declareVars()) {
					//System.out.println("BLOCK RETURN EVALUATION ON TOKEN " + peek());
					if(function_return()) {
						//System.out.println("BLOCK RETURN EVALUATION PASSED");
						hasReturn = true; 
					}
					else break;
				}
			}
		}
		//find }
		t = next();
		if(!t.value.equals("}")) throw new SyntaxErrorException("Malformed block statement, expected '}' got " + t.value + " at line " + t.line);
		
		if(mustReturn && !hasReturn) {
			throw new SyntaxErrorException("Missing return statement in: " + currentFname + " at line " + t.line);
		}
		//make sure we get a return one in there somehow <- ??? what does this mean?
		
		
		return true;
	}
	
	//break out of while loop
	private boolean break_while() {
		Token t = peek();
		if(!(t.type.equals("keyword") && t.value.equals("break"))) return false;
		next();
		
		if(breakStack.size() < 1) throw new SyntaxErrorException("Break statement must be placed inside of a while-loop. Error at line " + t.line);
		
		gen.comment("break");
		//gen.storeVars();
		gen.restoreState(whilePostCondState.peek());
		gen.branch_plain(breakStack.peek());
		if(!codeIsDead()) startDeadCode();
		
		return end();
	}
	
	//conditional branch in if- and while-statements
	private void conditional_branch(String label) {
		//do it simple:
		//if the last value before end is comparison operator:
		// 1: replace with dummy do-nothing operation
		// 2: do variable() to get stuff on stack
		// 3: do an inverted branch to the provided label
		
		//additional bonus operation: if the next statement is "continue;" replace the label with appropriate continue thing
		
		int itcopy = iterator;
		while(!tokens[itcopy].value.equals(";")) {
			++itcopy;
			if(itcopy == tokens.length) throw new SyntaxErrorException("EOF encountered in branch-type statement");
		}
		
		//see if we can do an optimised branch
		//what should we do here if the two final values are floats?
		//then the equality needs to be manually evaluated
		Token t = tokens[itcopy-1];
		if(t.type.equals("operator") && isComparison(t.value)) {
			//token with dummy operation
			tokens[itcopy-1] = new Token("operator", "??", 0);
			//throw new InternalCompilerException("TODO: Not Implemented in conditional_branch");
			//perform the stack evaluation
			if(!aritm()) throw new SyntaxErrorException("Malformed or missing boolean expression in conditional branch statement at line " + t.line);
			if(variableStack.size() != 2) throw new SyntaxErrorException("Multiple or no variables on evaluation stack at conditional branch statement at line " + t.line);
			
			//get variables
			String var2 = variableStack.pop();
			String var1 = variableStack.pop();
			assertVarExists(var1, t.line);
			assertVarExists(var2, t.line);
			//get types
			String type2 = types.getVar(var2).type;
			String type1 = types.getVar(var1).type;
			
			gen.comment(var1 + t.value + var2);
			//do type check
			if(!types.typeCompatible(type1, type2)) throw new SyntaxErrorException("Incompatible types: "+type1 + " and " + type2 + " in branch statement evauation at line " + t.line);
			
			
			//if float, evaluate manually, branch and return
			if(type1.equals("float")) {
				String dest = getTemporaryVar("int");
				if(t.value.equals("<")) {
					gen.cmp_less(dest, var1, var2, type1);
				} else if(t.value.equals(">")) {
					gen.cmp_more(dest, var1, var2, type1);
				} else if(t.value.equals("<=")) {
					gen.cmp_lesseq(dest, var1, var2, type1);
				} else if(t.value.equals(">=")) {
					gen.cmp_moreeq(dest, var1, var2, type1);
				} else if(t.value.equals("!=")) {
					gen.cmp_notequal(dest, var1, var2, type1);
				} else if(t.value.equals("==")) {
					gen.cmp_equal(dest, var1, var2, type1);
				} else {
					throw new InternalCompilerException("Why was this treaded as a comparison operation: " + t.value + " from line " + t.line);
				}
				gen.iftype_branch(label, dest);
				freeTmpVar(dest);
				freeTmpVar(var2);
				freeTmpVar(var1);
				return;
			}
			//else perform optimal branch and return
			//branches must be testing opposite condition since this skips code, but the condition tests if it should be run
			if(t.value.equals("<")) {
				gen.branch_moreq( var1, var2, label);
			} else if(t.value.equals(">")) {
				gen.branch_lesseq(var1, var2, label);
			} else if(t.value.equals("<=")) {
				gen.branch_more(var1, var2, label);
			} else if(t.value.equals(">=")) {
				gen.branch_less(var1, var2, label);
			} else if(t.value.equals("!=")) {
				gen.branch_equal(var1, var2, label);
			} else if(t.value.equals("==")) {
				gen.branch_notequal(var1, var2, label);
			}
			freeTmpVar(var2);
			freeTmpVar(var1);
			return;
		}
		//if not, do a normal variable and branch
		if(!aritm()) throw new SyntaxErrorException("Malformed or missing boolean expression in conditional branch statement at line " + t.line);
		//check amount of parameters on stack
		if(variableStack.size() != 1) throw new SyntaxErrorException("Multiple or no variables on evaluation stack at conditional branch statement at line " + t.line);
		//branch to label
		String var = variableStack.pop();
		assertVarExists(var, t.line);
		gen.iftype_branch(label, var);
		freeTmpVar(var);
		return;
	}
	
	//continue looping while loop
	private boolean continue_while() {
		Token t = peek();
		if(!(t.type.equals("keyword") && t.value.equals("continue"))) return false;
		next();
		
		if(continueStack.size() < 1) throw new SyntaxErrorException("Continue statement must be placed inside of a while-loop. Error at line " + t.line);
		
		gen.comment("continue");
		//gen.storeVars();
		gen.restoreState(whilePreCondState.peek());
		gen.branch_plain(continueStack.peek());
		if(!codeIsDead()) startDeadCode();
		
		return end();
	}
	
	
	private boolean doNothing() {
		Token t = peek();
		if(!(t.type.equals("operator") && t.value.equals("??"))) return false;
		next();
		return aritm();
	}
	
	//returns the stuff from a function type
	LinkedList<String> getfuncpreturn(String type) {
		LinkedList<String> retList = new LinkedList<String>();
		String[] stuff = type.split("£");
		if(stuff.length!=2) throw new InternalCompilerException("Something went wrong in checking return types of function pointer for type: " + type);
		//String retTypes = stuff[2];
		
		return retList;
	}
	
	//return s the other stuff in the same way as above
	LinkedList<String> getfuncparam(String type) {
		LinkedList<String> retList = new LinkedList<String>();
		//String[] stuff = type.split("£");
		return retList;
	}
	
	//returns int int int when passed intintint etc
	LinkedList<String> expandcompacttypelist(String s) {
		s.replace("int", "int ");
		s.replace("float", "float ");
		s.replace("char", "char ");
		s.replace(" #", "# ");//fix pointer things
		s.trim();
		LinkedList<String> returnList = new LinkedList<String>();
		for(String str : s.split(" "))
			returnList.addLast(str);
		return returnList;
	}
	
	//returns true if type is a function pointer
	boolean isFPtr(String s) {
		if(!s.startsWith("func")) return false;
		if(!isNumeric(s.substring(4,s.length()))) return false;
		return true;
	}
	
	//helper function
	private boolean isType(String s) {
		//a function pointer does not have type checking
		if(s.startsWith("func")) {
			String numstuff = s.substring(4,s.length());
			if(!isNumeric(numstuff)) return false;
			return true;
		}
		
		return "int".equals(s) || "float".equals(s) || "char".equals(s);
	}
	
	//scans for not only type keyword but pointer operations etc.
	private String getType() {
		Token t = next();
		String s = t.value;
		if(!isType(s)) throw new InternalCompilerException("Not a type: " + t);
		t = peek();
		while(t.type.equals("operator") && t.value.equals("#")) {
			s += "#";
			next();
			t = peek();
		}
		
		return s;
	}
	
	public static boolean isNumeric(String s) {//this unction is not in the right place, swap over to something else some time
		try {
			Integer.parseInt(s);
		} catch (Exception e){
			return false;
		}
		return true;
	}
	
	//declare a list of variables
	private boolean declareVars() {
		Token t = peek();
		//System.out.println("ATTEMPTING declarevars ON " + t.value);
		if(!(t.type.equals("keyword") && isType(t.value))) return false;
		
		String type = getType();
		
		LinkedList<String> declaredVars = new LinkedList<String>();
		
		//loop through all variables until we find the end
		while(!end()) {
			t = peek();
			if(!t.type.equals("identifier")) throw new SyntaxErrorException("invalid variable name: \"" + t.value + "\" at line " + t.line);
			if(declaredVars.contains(t.value)) throw new SyntaxErrorException("Variable name " + t.value + " has already been used as the name for a local variable");
			next();
			declaredVars.push(t.value);
			types.storeVar(new Variable(t.value, type));
		}
		
		gen.createVariables(declaredVars);
		
		return true;
	}
	
	//final thingymajig of a statement
	private boolean end() {
		Token t = peek();
		if(!t.value.equals(";")) return false;
		next();
		return true;
	}
	
	
	//calls a function and arranges parameters and return values appropriately
	private boolean funcCall() {
		Token t = peek();
		if(!(t.type.equals("operator") && t.value.equals("$"))) return false;
		next();
		
		//pop function name off stack
		String fname = variableStack.pop();
		
		FunctionInfo f;
		//check that the function exists
		if(!functionLookup.containsKey(fname)) {
			assertVarExists(fname, t.line);
			//otherwise check if the parameter is a function pointer
			String varType = types.getVar(fname).type;
			if(isFPtr(varType)) {
				LinkedList<Tuple<String, String>> tmpParamNames = new LinkedList<Tuple<String, String>>();
				LinkedList< String> tmpReturnTypes = new LinkedList<String>();
				int paramCount = Integer.parseInt(varType.substring(4,varType.length()));
				for(int i = 0; i < paramCount; ++i) {
					tmpParamNames.add(new Tuple<String, String>("foo", "any"));
				}
				for(int i = 0; i < paramCount; ++i) {
					tmpReturnTypes.add("any");
				}
				
				f = new FunctionInfo("__functionpointer",tmpParamNames, tmpReturnTypes);
			} else throw new SyntaxErrorException("Not a function: " + fname + " at line " + t.line);
		} else {
			//get function information
			f = functionLookup.get(fname);
		}
		
		//make sure that there are enough variables on the stack to act as parameters
		if(variableStack.size() < f.parameters.size()) throw new SyntaxErrorException("Not enough variables on stack for parameters to: "+ fname + " at line " + t.line);
		
		//start pop-ing off variables from stack
		LinkedList<String> vars = new LinkedList<String>();
		LinkedList<String> paramNames = new LinkedList<String>();
		for(int i = 0; i < f.parameters.size(); ++i) {
			vars.addFirst(variableStack.pop());
		} for(int i = 0; i < f.parameters.size(); ++i) {
			String varName = vars.pop();
			assertVarExists(varName, t.line);
			paramNames.push(varName);
			//perform type checking on return parameters here
			//as the variables pops of the stack they need to be compared to proper stuff in function
			if(!types.typeCompatible(types.getVar(varName).type, f.parameters.get(i).y)) {//awful, awful time complexity. Optimise later
				throw new SyntaxErrorException("Type missmatch in function call to \"" + fname + "\": " + types.getVar(varName).type + " and " + f.parameters.get(i).y + " at line " + t.line);
			}
			//freeTmpVar(varName);//perform type checking before this
			//see if whangfwarp
		}
		vars = new LinkedList<String>();//screw it
		int q = paramNames.size();
		for(int i = 0; i < q; ++i) {
			//reverse the darn list again...
			vars.addLast(paramNames.removeLast());
		}
		
		t = peek();
		LinkedList<String> exceptions = new LinkedList<String>();//get a list of the variables that are to be excepted from being stored, namely those that are going to be assigned to
		if( t.type.equals("operator") && t.value.equals("=") ) {
			if(f.returnTypes.size() > variableStack.size()) {
				throw new SyntaxErrorException("There are not enough variables on the stack to assign from function call " + f.name +  " at line " + t.line);
			}
			for(int i = 0; i < f.returnTypes.size(); ++i) {
				exceptions.addFirst(variableStack.pop());
			}
			for(String s : exceptions) {
				//System.out.println("DIDn't save " + s + " at line " + t.line);
				variableStack.push(s);
			}
		}
		
		//arrange the parameters for the function
		gen.arrange(vars, true, exceptions);//exceptions is the variables we do not need to store since they are part of the return values
		//make sure temporary variables are deallocated
		for(String s : vars) {
			freeTmpVar(s);
		}
		
		gen.clearVars(exceptions);
		if(f.name.equals("__functionpointer")) {
			gen.functionPointerCall(fname, q);
		} else gen.functionCall(fname, q);
		
		t = peek();
		if(t.type.equals("operator") && t.value.equals("=")) {
			next();
			if(f.returnTypes.size() == 0) throw new SyntaxErrorException("Functions that returns void can not be used as part of a statement: " + f.name + " at line " + t.line);
			//make sure there are enough variables on the stack to arrange them
			if(variableStack.size()!=f.returnTypes.size()){
				if(variableStack.size()>f.returnTypes.size())throw new SyntaxErrorException("What are all these extra variables on the stack? What am I supposed to do with these? (function call: " + fname + ")" + " at line " + t.line);
				else throw new SyntaxErrorException("Too few variables on the stack at function call: " + fname + " at line " + t.line);
			}
			//reverse the darn stack
			vars = new LinkedList<String>();//screw it
			for(String s : variableStack) {
				//reverse the darn list again...
				//paramNames.addFirst(paramNames.removeLast());//this is dumb. why am i dumb?
				vars.addFirst(s);
			}
			//arrange the next variables to the stuff
			gen.arrange(vars, false, new LinkedList<String>());
			//make sure we didn't assign anything to a temporary variable
			for(String s : variableStack) {
				if(s.startsWith("__TMP")) throw new SyntaxErrorException("Temporary values may not be assigned to in function calls: " + s + " at line " + t.line);
			}
			//perform type checking on return parameters here
			int i = 0;
			for(String s : variableStack) {
				if(!types.typeCompatible(types.getVar(s).type, f.returnTypes.get(f.returnTypes.size()-i-1))) {//awful, awful time complexity. Optimise later
					throw new SyntaxErrorException("Type missmatch in function return from \"" + fname + "\": " + types.getVar(s).type + " and " + f.returnTypes.get(i) + " at line " + t.line);
				}
				++i;
			}
			//make sure that the variable stack is cleared
			purgeStack();
			return end();
		}
		//otherwise we get a temporary variable, arrage it and put it on the stack.
		if(f.returnTypes.size() == 0){
			if(!end()) throw new SyntaxErrorException("Functions that returns void can not be used as part of a larger statement: " + f.name + " at line " + t.line);
			return true;
		}
		if(f.returnTypes.size() > 1) throw new SyntaxErrorException("Functions that return multiple parameters cannot be used in arithmetic, must be followed by '=': " + f.name + " at line " + t.line);
		String dest = getTemporaryVar(f.returnTypes.getFirst());
		LinkedList<String> someList = new LinkedList<String>();
		someList.add(dest);
		
		//perform type checking on return parameters here
		if(!types.typeCompatible(types.getVar(someList.getFirst()).type, f.returnTypes.get(0))) {//awful, awful time complexity. Optimise later
			throw new SyntaxErrorException("Type missmatch in function return from \"" + fname + "\" at line " + t.line);
		}
		
		gen.arrange(someList, false, new LinkedList<String>());
		variableStack.push(dest);
		
		return aritm();
	}
	
	//declaration of a function
	private boolean function() {
		//function declaration, how?
		// func int int fname int int { ?
		//doesn't really work with RPN though
		// func int int -> int int : fname {
		//better, can we do more?
		//omit func? how about def
		// def int int -> int int : fname {
		//slightly better
		// def fname int int -> int int {
		//good, but must specify input variable names
		// def fname a b int int -> int int { // perhaps a little wordy? is there some other way?
		// def fname int int -> int int { a b in; //a bit tricky, first thing in function must be variable names
		// def fname a b int c d float -> int int {//would be better, I think this is manageable
		// def fname a b int c d float : int int { //good since we remove the -> operator from declarations
		// def fname int a b float b c : int int { //more consistent with in-function syntax
		
		//System.out.println("Testing function, next is: " + peek().value);
		
		Token t = peek();
		if(!(t.type.equals("keyword") && t.value.equals("def"))) return false;
		FunctionInfo f = getFunctionInfo();
		
		//find parameter types
		LinkedList<Tuple<String, String>> params = f.parameters;
		
		//find output types
		returnTypes = f.returnTypes;
		
		//find name
		String name = f.name;
		currentFname = name;
		
		//write function start code
		gen.functionStart(name);
		
		//create the parameter variables
		//start by fixing a list of variable names
		LinkedList<String> varNames = new LinkedList<String>();
		for(Tuple<String, String> var : params) {
			varNames.addLast(var.x);
			types.storeVar(new Variable(var.x, var.y));
		}
		gen.createAndArrange(varNames);
		
		//find a block of code with return statement
		if(!block(true)) throw new SyntaxErrorException("Function declaration lacks valid block at line " + t.line);
		//perform cleanup
		if(codeIsDead()) stopDeadCode();
		gen.functionEnd();
		gen.deleteVars();
		returnTypes = null;
		variableStack = new LinkedList<String>();
		types = new TypeManager();
		//re-add all global variables
		for(Variable v : globalVars) {
			types.storeVar(v);
		}
		
		//reset temporary variables
		tmpvarsFree = new LinkedList<String>();
		tmpVarIterator = 0;
		
		//return successful
		return true;
	}
	
	
	private boolean globalVar() {
		//what are the prerequisites for this feature?
		// variablecontext need to recognise that a variable is global
		// how do we handle assigning things to a global variable in asmgenerator?
		// normally we assign to a register, what happens where 
		// variablecontext handles it? what happens if we have cached a global variable and declare a local one with the same name?
		
		//find var or abort
		Token t = peek();
		if(!(t.type.equals("keyword") && t.value.equals("var"))) {
			return false;
		}
		next();
		
		//System.out.println("in global var, just got " + t.value);
		
		//find vartype
		t = peek();
		if(!isType(t.value)) throw new SyntaxErrorException("Expected type in global variable declaration, got: " + t.value + " at line " + t.line);
		String type = getType();
		//System.out.println("in global var, just got " + t.value);
		//find identifier
		t = next();
		if(!t.type.equals("identifier")) throw new SyntaxErrorException("The var keyword needs to be followed by a valid variable name, got: " + t.value + " at line " + t.line);
		String name = t.value;
		//System.out.println("in global var, just got " + t.value);
		//find end or =
		t = peek();
		if(t.type.equals("operator") && t.value.equals("=")) {
			next();
			//how to handle strings? not quite sure if I allowed them in the data section. TODO: add possibility for labels in data section <- done
			//TODO: add support for strings
			
			//what can we do here?
			// * pointers of all types, initialised to integers.
			// * Can not do string pointers yet, unfortunately. assign all strings to chars, and take address later
			// * floats, ints, chars should be permitted
			
			//what does this tell us?
			// permit floats
			// permit ints
			// permit strings? wait until I have updated the assembler
			t = next();
			if(!t.type.equals("literal")) throw new InternalCompilerException("No literal specified in global variable assignation, got: " + t.value + " at line " + t.line);
			String lit = t.value;
			
			//try parsing the literal
			//AWFUL COPY-AND-PASTE CODE INCOMING:
			//otherwise, return int or float in movement code
			boolean negate = false;
			t = peek();
			if(t.type.equals("operator") && t.value.equals("(-)")) {
				negate = true;
				next();
			}
			
			try {
				int res = Integer.parseInt(lit);
				
				//perform type checking
				if(!type.equals("int") && !type.endsWith("#") && !type.equals("char")) throw new SyntaxErrorException("Cannot assign an int to a " + type + " in global variable declaration to " + name + " at line " + t.line);
				if(type.equals("char") && res > 255) throw new SyntaxErrorException("Value index out of range: a value grater than 255 does not fit into a char. in global variable declaration for: " +name + " at line " + t.line);
				
				res *= negate ? -1 : 1;
				gen.declareGlobal(name, type, res);
			} catch (NumberFormatException e) {
				try {
					float fres = Float.parseFloat(lit);
					//perform type checking
					if(!type.equals("float")) throw new SyntaxErrorException("Cannot assign a float to a " + type + " in global variable declaration to " + name + " at line " + t.line);
					
					fres *= negate ? -1.0 : 1.0;
					gen.declareGlobalFloat(name, fres);
				} catch (Exception anotherGoddamnedException) {
					throw new InternalCompilerException("This should not have evaluated as literal: " + name + " at line " + t.line);
				}
			}
		} else {
			if(type.equals("float")) {
				gen.declareGlobalFloat(name, 0.0f);
			} else {
				gen.declareGlobal(name, type, 0);
			}
		}
		//find end or throw exception
		t = peek();
		if(!end()) throw new SyntaxErrorException("Global variable declaration needs to be followed by a ';', got: " + t.value + " at line " + t.line);
		//System.out.println("in global var, just got " + t.value);
		//System.out.println("next up: " + peek().value);
		//add variable to global list
		//do something here darnit
		Variable v = new Variable(name, type);
		globalVars.addLast(v);
		types.storeVar(v);
		//System.out.println("Stored variable " + v.name + v.type);
		//these variables will be added to the appropriate contexts in function()
		//System.out.println("leaving globalVar, next is: " + peek().value);
		return true;
	}
	
	private boolean ifthen() {
		//TODO: we must clear all variables we find inside the if-statement
		String label = getUniqueLabel("if");
		Token t = peek();
		if(!(t.type.equals("keyword") && t.value.equals("if"))) return false;
		next();
		
		if(codeIsDead()) startDeadCode();
		gen.comment("START IF");
		
		conditional_branch(label);
		Tuple<String[], Boolean[]> state = gen.saveState();
		
		if(!statement()) throw new SyntaxErrorException("Invalid if-statement at line " + t.line +"; following statement missing or malformed");
		
		//after the if-statement all variables should be reloaded, since we no longer know the register layout
		gen.restoreState(state);
		
		if(COMPILER_DEBUG_ENABLED) {
			//print the state
			System.out.println("Restored variables: ");
			for(int i = 0; i < state.x.length; ++i){
				System.out.println(state.x[i]+";"+state.y[i]);
			}
		}
		
		//return elsedo(label);//change to true
		t = peek();
		if(t.type.equals("keyword") && t.value.equals("else")) {
			next();
			
			//save all registers and unassociate the values of the registers with the variables
			//gen.clearVars();
			//add a branch to end of if-statement
			gen.branch_plain("end"+label);//prepend "end here too
			
			//make sure that the else statement doesn't disappear due to the dead code check
			if(codeIsDead()) stopDeadCode();
			if(codeIsDead()) startDeadCode();//I'm not sure if this is right, but I think so. The whole dead code detection thing is a bit of a cludge.
			
			gen.putLabel(label);
			if(!statement()) throw new SyntaxErrorException("Keyword 'else' must be followed by a valid statement. Error at line " + t.line);
			//gen.clearVars();
			gen.restoreState(state);
			if(codeIsDead()) stopDeadCode();
			gen.putLabel("end"+label);
			
		} else {
			//gen.clearVars();
			if(codeIsDead()) stopDeadCode();
			gen.putLabel(label);
		}
		
		gen.comment("END IF");
		return true;
	}
	
	private boolean literal() {
		Token t = peek();
		if(!(t.type.equals("literal"))) return false;
		next();
		
		//System.out.println("in literal: " + t);
		
		String dest;
		//if function starts with " it is a string, push to data section and return pointer
		if(t.value.startsWith("\"")) {
			if(peek().type.equals("operator") && peek().value.equals("=")) {
				next();
				dest = variableStack.pop();
			} else {
				dest = getTemporaryVar("char#");
			}
			//perform type checking
			if(!types.getVar(dest).type.equals("char#")) throw new SyntaxErrorException("Cannot assign a string to anything but a char# at line " + t.line);
			
			gen.comment(dest+"="+t.value);
			
			gen.addData_String(dest, t.value);
			variableStack.push(dest);
			return aritm();
		}
		
		String name = t.value;
		t = peek();
		boolean negate = false;
		if(t.type.equals("operator") && t.value.equals("(-)")) {
			negate = true;
			next();
		}
		
		//otherwise, return int or float in movement code
		try {
			int res = Integer.parseInt(name);
			
			if(res == 0) {
				//optimisation, since we have preloaded 0 available
				variableStack.push("__ZERO");
				return aritm();
			}
			
			//get destination
			if(peek().type.equals("operator") && peek().value.equals("=")) {
				next();
				dest = variableStack.pop();
			} else if( !negate && res < 65536 && isLiteralCompatibleOperator(peek())) {
				return aritm_dooliteral(res) || aritm_hybridAssignLiteral(res);
			} else {
				dest = getTemporaryVar("int");
			}
			
			if(COMPILER_DEBUG_ENABLED) {
				System.out.println("literalTypeCheck: dest: " + dest + " destType: " + types.getVar(dest) + "");
			}
			assertVarExists(dest, t.line);
			//perform type checking
			if(!types.getVar(dest).type.equals("int") && !types.getVar(dest).type.endsWith("#")) throw new SyntaxErrorException("Cannot assign an int to a " + types.getVar(dest).type + " at line " + t.line);
			
			res *= negate ? -1 : 1;
			gen.comment(dest+"="+res);
			gen.immediateInt(dest, res);
			variableStack.push(dest);
		} catch (NumberFormatException e) {
			try {
				//get destination
				if(peek().type.equals("operator") && peek().value.equals("=")) {
					next();
					dest = variableStack.pop();
				} else {
					dest = getTemporaryVar("float");
				}
				
				//perform type checking
				if(!types.getVar(dest).type.equals("float")) throw new SyntaxErrorException("Cannot assign a float to a " + types.getVar(dest).type + " at line " + t.line);
				
				float fres = Float.parseFloat(name);
				fres *= negate ? -1.0 : 1.0;
				gen.comment(dest+"="+fres);
				gen.immediateFloat(dest, fres);
				variableStack.push(dest);
			} catch (Exception anotherGoddamnedException) {
				throw new InternalCompilerException("This should not have evaluated as literal: " + name + " from line " + t.line);
			}
		}
		
		return aritm();
	}

	private boolean isLiteralCompatibleOperator(Token t) {
		if(!t.type.equals("operator")) return false;
		String[] do_operators = { "&", "|", "^", "+", "-", "*", "/", ">>", "<<", "+=", "-=", "/=", "*=", "|=", "&=", "^="};
		for(String op : do_operators) {
			if(op.equals(t.value)) return true;
		}
		return false;
		
		
	}
	
	
	
	//handle pointers and etc
	private boolean pointerAritm() {
		Token t = peek();
		if(!(t.type.equals("operator") && isPointerOperation(t.value))) return false;
		next();
		String varType;
		String var = variableStack.pop();
		boolean fpointer = false;
		if(types.getVar(var) != null) {
			varType = types.getVar(var).type;
		} else {
			varType = "func";
			FunctionInfo f = functionLookup.get(var);
			if(f == null) throw new SyntaxErrorException("Not a variable: "+var + " at line " + t.line);
			varType += f.parameters.size();
			fpointer = true;
		}
		
		
		if(t.value.equals("<-")) {
			//pop one more variable variables
			if(variableStack.size() == 0) throw new SyntaxErrorException("Operation '<-' needs at least two variables be present on the stack to operate, found 0. At line " + t.line);
			String dest = variableStack.pop();
			String destType = types.getVar(dest).type;
			
			//perform type checking
			//dest need to be pointer
			if(!destType.endsWith("#")) throw new SyntaxErrorException("<- needs to be used on pointer, got: " + destType + " at line " + t.line);
			//make sure dest is a pointer to type of var
			if(!types.typeCompatible(destType.substring(0, destType.length()-1),varType)) throw new SyntaxErrorException("Indirect assignment between incompatible types: " + destType +" and " +varType + " at line " + t.line);
			
			//output code
			gen.comment(dest+"<-"+var);
			gen.indirectAssign_wr(dest, var);
			
			//push value back to stack
			variableStack.push(var);
			//free temporary variables from the assigning thing
			freeTmpVar(dest);
			return aritm();
		}
		
		//make sure that varType has the right type
		if(t.value.equals("#")) {
			if(!varType.endsWith("#")) throw new SyntaxErrorException("# needs to be used on pointer, got: " + varType + " at line " + t.line);
			varType = varType.substring(0, varType.length()-1);
		} else if(t.value.equals("@") && !fpointer) {
			varType = varType+"#";
		}
		
		//perform some more advanced metrics to get destination variable
		//awful copy-paste coding here
		String dest;
		boolean varReuse = false;
		if(peek().value.equals("=")) {
			//this will need to be expanded later on
			next();
			dest = variableStack.pop();
			if(!types.typeCompatible(types.getVar(dest).type, varType)) throw new SyntaxErrorException("Incompatible types: " + types.getVar(dest).type + " and " + varType + " at line " + t.line);
		} else {
			if(var.startsWith("__TMP")) {
				dest = retypeTmpVar(var, varType);
				varReuse = true;
			} else {
				dest = getTemporaryVar(varType);
			}
		}
		gen.comment(dest+"="+var+t.value);
		
		if(t.value.equals("#")) {
			gen.dereference(dest, var, 0);
		} else if(t.value.equals("@")) {
			gen.addressOf(dest, var, fpointer);
		}
		variableStack.push(dest);
		if(!varReuse) {
			freeTmpVar(var);
		}
		return aritm();
	}
	
	private boolean purgeStack() {
		//should we check that there is maximum one variable on the stack when we get here?
		//clear the variable stack
		while(variableStack.size() != 0) {
			freeTmpVar(variableStack.pop());
		}
		return true;
	}
	
	//finds one of several constructs: arithmetic, inline assembler, etc
	private boolean statement() {
		int itmp = iterator;//we bookmark our position among the tokens. Not sure if this is neccessary, but I'm not gonna bother removing it
		if(aritm()) return purgeStack();//should we relly purge the stack after each thing? it should be empty, but should we not rather enforce that?
		iterator = itmp;
		if(whiledo()) return purgeStack();
		iterator = itmp;
		if(ifthen()) return purgeStack();
		iterator = itmp;
		if(block(false)) return purgeStack();
		iterator = itmp;
		if(break_while()) return purgeStack();
		iterator = itmp;
		if(continue_while()) return purgeStack();
		iterator = itmp;
		if(inlineasm()) return purgeStack();
		iterator = itmp;
		if(unload()) return purgeStack();
		iterator = itmp;
		if(nosave()) return purgeStack();
		iterator = itmp;
		if(savevar()) return purgeStack();
		return false;
	}
	
	private boolean typeCast() {
		//how to do this?
		//find (
		Token t = peek();
		if(!(t.type.equals("delimeter") && t.value.equals("("))) {
			//negation operator
			if(t.type.equals("operator") && t.value.equals("(-)")) {
				if(variableStack.size() == 0) throw new SyntaxErrorException("No variables on stack at negation at line " + t.line);
				next();
				//read variables and stuff
				//readvar
				String src = variableStack.pop();
				//gettmpvar
				String type = types.getVar(src).type;
				String dest = getTemporaryVar(type);
				
				//make top of stack negative
				gen.negate(dest, src, type);
				variableStack.push(dest);
				freeTmpVar(src);
				return aritm();
			}
			return false;
		}
		if(variableStack.size() == 0) throw new SyntaxErrorException("No variables on stack at type cast at line " + t.line);
		next();
		//find type
		String type = getType();
		//find )
		t = next();
		if(!(t.type.equals("delimeter") && t.value.equals(")"))) throw new SyntaxErrorException("Missing ')' in type casting at line " + t.line);
		//make copy of top of stack
		//readvar
		String src = variableStack.pop();
		assertVarExists(src, t.line);
		boolean reuseVar = false;
		//gettmpvar
		String typeFrom = types.getVar(src).type;
		String dest;
		if(src.startsWith("__TMP")) {
			dest = retypeTmpVar(src, type);
			reuseVar = true;
		} else {
			dest = getTemporaryVar(type);
		}
		//optimise this by seeing if i can get the destination from variable stack
		//eg: dest src(type)=;
		
		//cast to corrent type
		gen.comment(dest + "=(" + type + ")"+src);
		gen.typeConvert(dest,src,typeFrom,type);
		variableStack.push(dest);
		if(!reuseVar) {
			freeTmpVar(src);
		}
		
		return aritm();
	}
	
	private boolean unload() {
		Token t = peek();
		if(!(t.type.equals("keyword") && t.value.equals("__unload"))) return false;
		next();
		
		if(!aritm()) throw new SyntaxErrorException("A variable needs to be specified after keyword 'unload' at line " + t.line);
		if(variableStack.size() != 1) throw new SyntaxErrorException("Not enough/too many variables specified after 'unload' keyword at line " + t.line);
		
		
		String var = variableStack.pop();
		gen.comment("unload " + var);
		freeTmpVar(var);
		gen.unload(var);
		
		return true;
		
	}
	
	private boolean nosave() {
		Token t = peek();
		if(!(t.type.equals("keyword") && t.value.equals("__nosave"))) return false;
		next();
		
		if(!aritm()) throw new SyntaxErrorException("A variable needs to be specified after keyword 'nosave' at line " +t.line);
		if(variableStack.size() != 1) throw new SyntaxErrorException("Not enough/too many variables specified after 'nosave' keyword at line " + t.line);
		
		
		String var = variableStack.pop();
		gen.comment("nosave " + var);
		freeTmpVar(var);
		gen.noSave(var);
		
		return true;
	}
	
	private boolean savevar() {
		Token t = peek();
		if(!(t.type.equals("keyword") && t.value.equals("__store"))) return false;
		next();
		
		if(!aritm()) throw new SyntaxErrorException("A variable needs to be specified after keyword 'store' at line " +t.line);
		if(variableStack.size() != 1) throw new SyntaxErrorException("Not enough/too many variables specified after 'store' keyword at line " + t.line);
		
		
		String var = variableStack.pop();
		gen.comment("store " + var);
		freeTmpVar(var);
		gen.store(var);
		
		return true;
	}
	
	
	//as the name dictates
	private boolean variable() {
		
		Token t = peek();
		if(!t.type.equals("identifier")) return false;
		next();
		//String varName = t.value;
		
		variableStack.push(t.value);
		return aritm();
	}
	
	//returns true if the next statement contains a function call
	//OBS: this function destroys the state of the compiler,
	//make sure to restore the token pointer after calling
	private boolean nextStatementHasFCall() {
		Token t = next();
		
		if(t.value.equals("{")) {
			int bracketCount = 1;
			while (bracketCount > 0) {
				if(t.value.equals("$")) return true;
				if(t.value.equals("{")) ++bracketCount;
				if(t.value.equals("}")) --bracketCount;
				t = next();
			}
		} else {
			while(!t.value.equals(";")) {
				if(t.value.equals("$")) return true;
				t = next();
			}
		}
		
		return false;
	}
	
	//while loops are handled here
	private boolean whiledo() {
		//find while
		Token t = peek();
		if(!(t.type.equals("keyword") && t.value.equals("while"))) return false;
		next();
		
		if(codeIsDead()) startDeadCode();
		gen.comment("START WHILE");
		//here we need to do an omptimisation: if more than 12 variables on stack, save variables
		//if loop contains a function call, unload all variables
		int itmp = iterator;
		if(nextStatementHasFCall() || nextStatementHasFCall()) {//we check both in boolean statement and in the looped statement
			gen.clearVars(new LinkedList<String>());
		} else if(gen.varCount() >= 12) {
			gen.storeVars();
		}
		iterator = itmp;
		
		
		//save the state of the registers so we can restore it after the loop
		Tuple<String[], Boolean[]> state = gen.saveState();
		whilePreCondState.push(state);
		String label = getUniqueLabel("while");
		gen.putLabel(label);
		continueStack.push(label);
		breakStack.push("end"+label);
		
		
		//do a conditional branch
		conditional_branch("end"+label);
		purgeStack();//this will remove all temporary variables from the stack
		//gen.whiledo();
		Tuple<String[], Boolean[]> postCondState = gen.saveState();
		whilePostCondState.push(postCondState);
		
		//find statement
		if(!statement()) throw new SyntaxErrorException("While statement needs a statement following it. Error at line " + t.line);
		gen.restoreState(state);
		
		if(COMPILER_DEBUG_ENABLED) {
			//print the state
			System.out.println("Restored variables: ");
			for(int i = 0; i < state.x.length; ++i){
				System.out.println(state.x[i]+";"+state.y[i]);
			}
		}
		
		gen.branch_plain(label);
		if(codeIsDead()) stopDeadCode();
		gen.putLabel("end"+label);
		gen.comment("END WHILE");
		
		//Note: important: make sure that variables are unloaded before the loop and just before branchback in the loop
		//ASLO: make sure variable declarations are pushed outside so that we don't run into infinite things from space
		//in the meantime, forbid variable declarations if if-statements/while-statements
		
		//remove break and continue labels
		breakStack.pop();
		continueStack.pop();
		
		//this is needed as the state might be somewhat different from the one restored to in the loop
		//we don't need to actually move any variables, since the calculation that changed the state will already have done so
		gen.setState(postCondState);
		
		return true;
	}
	
	//searches for inline assembly code
	private boolean inlineasm() {
		
		
		//find asm
		Token t = peek();
		if(!(t.type.equals("keyword") && t.value.equals("asm"))) return false;
		next();
				
		//find variables
		aritm();
		//arrange varstack
		//reverse the stack first, though
		LinkedList<String> varsRev = new LinkedList<String>();
		for(String s : variableStack) {
			varsRev.addFirst(s);
		}
		purgeStack();//free all values on the variable stack
		gen.arrange(varsRev,true, new LinkedList<String>());
		//forget register layout
		gen.clearVars(new LinkedList<String>());//the recently arranged registers should be marked unaltered or something
		//inline the assembler code
		t = next();
		gen.comment("---BEGIN INLINE ASSEMBLER---");
		if(!(t.type.equals("literal") && t.value.startsWith("\"") && t.value.endsWith("\""))) throw new SyntaxErrorException("asm statement needs a string describing some assembler code at line " + t.line);
		gen.putASM("\t"+t.value.substring(1,t.value.length()-1));
		for(t = peek(); !end(); t = peek()) {
			next();
			if(!(t.type.equals("literal") && t.value.startsWith("\"") && t.value.endsWith("\""))) throw new SyntaxErrorException("asm statement needs a string describing some assembler code at line " + t.line);
			gen.putASM("\t"+t.value.substring(1,t.value.length()-1));
		}
		gen.comment("---END INLINE ASSEMBLER---");
		//find list of variables
		LinkedList<String> varList = new LinkedList<String>();
		while(!end()) {
			t = next();
			if(!t.type.equals("identifier")) throw new SyntaxErrorException("After the list of strings in asm there needs to be a list of variables or a ;, got: " + t.value + " at line " +t.line);
			else varList.addLast(t.value);
		}
		gen.arrange(varList, false, new LinkedList<String>());
		return true;
	}
	
	String currentFname;
	LinkedList<String> returnTypes;
	boolean hasreturned = false;
	private boolean function_return() {
		//take the variable stack and 
		Token t = peek();
		if(!(t.type.equals("keyword") && t.value.equals("return"))) return false;
		next();
		aritm();// is this right? yes
		//gen.comment("return");
		if(variableStack.size() != returnTypes.size()) {
			System.out.println("****");
			for(String s : variableStack) {
				System.out.println(s);
			}
			throw new SyntaxErrorException("Incorrent ammount of return parameters in: " + currentFname + "expected " + returnTypes.size() + " got " + variableStack.size()+ " at line " + t.line);
		}
		//do a type check on the variables in the return thingymajig
		LinkedList<String> tmpList = new LinkedList<String>();
		int i = 0;
		for(String s : variableStack) {
			if(!types.typeCompatible(types.getVar(s).type, returnTypes.get(returnTypes.size()-i-1))) {
				throw new SyntaxErrorException("Type missmatch in function return from " + currentFname + ": " + types.getVar(s).type + " and " + returnTypes.get(returnTypes.size()-i-1) + " at line " + t.line);
			}
			tmpList.addFirst(s);
			++i;
		}
		
		//arrange all variables in proper order in registers
		//variable 1 in r1, 2 in r2 etc.
		gen.storeAllGlobals();
		gen.arrange(tmpList, true, new LinkedList<String>());
		
		gen.functionReturn(returnTypes.size());
		if(!codeIsDead()) startDeadCode();
		
		//empty variable stack
		purgeStack();
		return true;
	}
	
	/**************************************/
	//behind the scenes stuff
	//split this off into a managed for temporary variables, probably VariableContext
	LinkedList<String> tmpvarsFree;
	int tmpVarIterator;
	private String getTemporaryVar(String type) {
		String target;
		if(tmpvarsFree.size() == 0) {
			gen.createVar("__TMP"+tmpVarIterator);
			types.storeVar(new Variable("__TMP"+tmpVarIterator, type));
			target = "__TMP"+tmpVarIterator++;
		} else {
			target = tmpvarsFree.pop();
			types.storeVar(new Variable(target, type));
		}
		return target;
	}
	
	//frees a temporary variable so that new things can be assigned to it
	private void freeTmpVar(String varName) {
		if(varName.startsWith("__TMP")) {
			types.removeVar(varName);
			tmpvarsFree.push(varName);
			gen.noSave(varName);
			gen.unload(varName);
		}
	}
	
	// this will not work atm, see if there is a possible recode of the section getTmpVar appears in.
	private String retypeTmpVar(String var, String newType) {
		if(var.startsWith("__TMP")) {
			types.removeVar(var);
			types.storeVar(new Variable(var, newType));
		} else {
			throw new InternalCompilerException("Attempted to re-type a non-temporary variable");
		}
		return var;
	}
	
	int uniqueLabel = 1;
	private String getUniqueLabel(String s) {
		return s + uniqueLabel++;
	}
	
	int deadCodeLevels = 0;
	private void startDeadCode() {
		++deadCodeLevels;
		//System.out.println("starting code deadening at level " + deadCodeLevels);
		gen.deadCode(true);
	}
	
	private boolean codeIsDead() {
		return deadCodeLevels > 0;
	}
	
	private void stopDeadCode() {
		if(deadCodeLevels == 0) throw new InternalCompilerException("Tried to un-dead non-dead code");
		else {
			//System.out.println("ending code deadening at level " + deadCodeLevels);
			--deadCodeLevels;
			if(deadCodeLevels == 0) gen.deadCode(false);
		}
	}
}
