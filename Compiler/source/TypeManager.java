import java.util.HashMap;
import java.util.LinkedList;


public class TypeManager {
	HashMap<String, Variable> typeMap;
	
	public TypeManager() {
		typeMap = new HashMap<String, Variable>();
	}
	
	public Variable getVar(String s) {
		return typeMap.get(s);
	}
	
	public LinkedList<Variable> getAllVars() {
		LinkedList<Variable> list = new LinkedList<Variable>();
		for(Variable v : typeMap.values()) {
			list.addLast(v);
		}
		return list;
	}
	
	public boolean hasProperty(String varName, String property) {
		Variable v = getVar(varName);
		return v.hasProperty(property);
		
	}
	
	public void storeVar(Variable v) {
		typeMap.put(v.name, v);
	}
	
	public boolean sameType(String a, String b) {
		Variable v1 = getVar(a);
		Variable v2 = getVar(b);
		
		return typeCompatible(v1.type, v2.type);
	}
	
	
	private boolean accept2(String t1, String t2, String acc1, String acc2) {
		return (t1.equals(acc1) && t2.equals(acc2)) || (t2.equals(acc1) && t1.equals(acc2));
	}
	private boolean pointercorrect(String t1, String t2, String acc) {
		return (isPointer(t1) && t2.equals(acc)) || (isPointer(t2) && t1.equals(acc));
	}
	public boolean isPointer(String t) {
		return t.endsWith("#");
	}
	
	public boolean typeCompatible(String t1, String t2) {
		//this one need to accept:
		//pointer and char
		//pointer and int
		//int and char
		
		return (t1.equals("any") || t2.equals("any") || t1.equals(t2)) || accept2(t1, t2, "int", "char") || pointercorrect(t1, t2, "int") || pointercorrect(t1, t2, "char");
	}
	
	public String getTypeResult(String type1, String type2) {
		if(!typeCompatible(type1, type2)) throw new InternalCompilerException("Internal error: do not call getTypeResult on incompatible types you dork!");
		
		//if types are equal return type1
		if(type1.equals(type2)) return type1;
		
		//if one type is pointer and one is int return pointer
		if(isPointer(type1) && type2.equals("int")) return type1;
		if(isPointer(type2) && type1.equals("int")) return type2;
		
		//if one type is pointer and one is char return pointer
		if(isPointer(type1) && type2.equals("char")) return type1;
		if(isPointer(type2) && type1.equals("char")) return type2;
		
		//if one is char and one is int return int
		if( (type1.equals("char") && type2.equals("int")) || (type2.equals("char") && type1.equals("int"))) return "int";
		
		//if one type is any and one type is something else, return something else
		if(type1.equals("any")) return type2;
		if(type2.equals("any")) return type1;
		
		throw new InternalCompilerException("Somehow two incompatible types are listed as compatible. This should be fixed");
	}
	
	public void removeVar(String varName) {
		typeMap.remove(varName);
	}
	
	public void removeAll() {
		typeMap = new HashMap<String, Variable>();
	}
}
