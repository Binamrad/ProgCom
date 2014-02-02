import java.util.LinkedList;


public class FunctionInfo {
	public final String name;
	public final LinkedList<Tuple<String, String>> parameters;
	public final LinkedList<String> returnTypes;
	public FunctionInfo(String name, LinkedList<Tuple<String, String>> paramters, LinkedList<String> returnTypes) {
		this.name = name;
		this.parameters = paramters;
		this.returnTypes = returnTypes;
	}
}
