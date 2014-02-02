import java.util.Collection;
import java.util.LinkedList;


public class Variable {
	public final String name;
	public final String type;
	private LinkedList<String> properties;
	
	public Variable(String name, String type) {
		this(name, type, null);
	}
	
	public Variable(String name, String type, Collection<String> properties) {
		this.name = name;
		this.type = type;
		properties = new LinkedList<String>();
		if(properties != null) {
			for(String s : properties) {
				this.properties.addLast(s);
			}
		}
	}
	
	public boolean hasProperty(String s) {
		for(String prop : properties) {
			if(prop.equals(s)) return true;
		}
		return false;
	}
	
	@Override
	public String toString() {
		return name+":"+type;
	}
}
