
public class Token {
	//this class is a tuple of stwo strings describing a small bit of the program
	//example data:
	//	type	= "operator"
	//	value	= "++"
	public final String type;
	public final String value;
	public final int line;
	public Token(String type, String value, int line) {
		this.type = type;
		this.value = value;
		this.line = line;
	}
	@Override
	public String toString() {
		return line+":\""+type+"\":\""+value+"\"";
	}
}
