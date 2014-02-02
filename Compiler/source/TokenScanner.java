
public class TokenScanner {
	String s;
	String[] specialStrings = {"",""};
	int iter = 0;
	Token match = null;
	private final int line;
	public TokenScanner(String s, int line) {
		//string will be split after "//" to remove all comments
		this.s = s.split("//")[0];
		this.line = line;
	}
	
	public Token next() {
		Token ret = peek();//make sure we have a string loaded
		match = null;//remove stored value
		return ret;
	}
	
	public Token peek() {
		if(match != null) {
			return match;//if we already have found a string and not used it, return it here
		}
		
		//remove whitespace
		while(iter < s.length() && isWhitespace(s.charAt(iter))) ++iter;
		if(iter == s.length()) return null;//reached end of string
		
		//find the next token in the string
		if(isSpecial(s.charAt(iter))) {
			String str = getOperator();
			if(str != null) {
				match = new Token("operator",str,line);
			} else {
				str = getDelimeter();
				if(str == null) {
					throw new InternalCompilerException("Error in token scanner, not a delimeter in: " + s + " iter = " + iter + " s.length = " + s.length() + " charat = " + s.charAt(iter));
				}
				match = new Token("delimeter",str,line);
			}
		} else {
			String str = getWord();
			if(isLiteral(str)) {
				match = new Token("literal",str, line);
			} else if(isKeyword(str)) {
				match = new Token("keyword",str, line);
			} else {
				match = new Token("identifier",str, line);
			}
		}
		return match;
	}
	
	public boolean hasNext() {
		peek();
		return iter < s.length() || match != null;
	}
	
	private boolean isWhitespace(char c) {
		return c == ' ' || c == '\t' || c == '\n' || c == '\r';
	}
	private boolean isSpecial(char c) {
		char[] specialChars = {'+', '-', '*', '/', '>', '<', '&', '|', '^', '=', ';','}','{',':','$', '#', '@', '!', '(', ')', '%'};
		for(char special : specialChars) {
			if(c == special) return true;
		}
		return false;
	}
	
	private String getDelimeter() {
		String[] delimeters = {"{","}",":", "(", ")", ";"};
		return matchFromList(delimeters);
	}
	
	private String getOperator() {
		//TODO: change ";" to delimeter
		String[] operators = {"*=", "/=", "+=", "-=", "^=", "|=", "&=", "&&", "&", "||", "|", "^^", "^", "+++", "++", "+", "---", "--", "-", "==", "=", "*", "/", ">=", "<=", ">>", ">", "<-", "<<", "<", "!=", "$", "#", "@", "!", "(-)", "%"};
		return matchFromList(operators);
	}
	
	private String matchFromList(String[] possibleWords) {
		for(String operator : possibleWords) {
			int i = 0;
			boolean fail = false;
			while(i < operator.length()) {
				if(i+iter >= s.length() || operator.charAt(i) != s.charAt(iter+i)) {
					fail = true;
					break;
				}
				++i;
			}
			//if the previous loop exited prematurely, go back to start
			if(fail) continue;
			//otherwise we return the operator we just found
			iter += i;
			return operator;
		}
		return null;
	}
	
	private String getWord() {
		StringBuilder stb = new StringBuilder();
		
		
		boolean inString = false;
		boolean escapeQuote = false;
		
		if(s.charAt(iter) == '\"') {
			stb.append('"');
			++iter;
			inString = true; 
		}
		//add chars to the word until we find whitespace/a special character
		while( iter < s.length() && ( !( isSpecial(s.charAt(iter)) || isWhitespace(s.charAt(iter)) ) || inString) ) {
			if(s.charAt(iter) == '"') {
				if(!inString) {//this means we should stop reading string now
					return stb.toString();
				}	
				if(!escapeQuote) {
					inString = false;
					stb.append('"');
					++iter;
					break;
				} else {
					escapeQuote = false;
				}
			} else if(s.charAt(iter) == '\\') {
				escapeQuote = !escapeQuote;
			}  else {
				escapeQuote = false;
			}
			
			/*else if((s.charAt(iter) == '\n') && inString) {//I'm not sure if this code will ever run
				throw new SyntaxErrorException("EOL encountered before end of string");
			}*///this code would never run. make sure to have some other kind of guard against this
			
			stb.append(s.charAt(iter));
			++iter;
		}
		
		return stb.toString();
	}
	
	private boolean isKeyword(String s) {
		String[] keywords = {"while", "int", "char", "if", "else", "def", "break", "continue", "return", "float", "asm", "var", "__unload", "__nosave", "__store"};
		for(String keyword : keywords) {
			if(keyword.equals(s)) return true;
		}
		
		if(s.startsWith("func")) {
			//bit of a hack here
			String numstuff = s.substring(4,s.length());
			if(!Program.isNumeric(numstuff)) {
				System.out.println(numstuff);
				System.out.println(s + " is not a keyword");
				return false;
			}
			return true;
		}
		return false;
	}
	
	private boolean isLiteral(String s) {
		//if starts with " and ends with " return true
		if(s.startsWith("\"")) return true;
		
		//try parse to float
		try {
			Float.parseFloat(s);
			return true;
		} catch (Exception e) {
			//try parse to int
			try {
				Integer.parseInt(s);
				return true;
			} catch (Exception f) {
				return false;
			}
		}
	}
}
