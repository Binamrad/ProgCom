
public interface lineWrapper {
	String compile(int stacksize, boolean funcCall);
	String getComponent(int i);
	void setComponent(int i, String component);
	int getComponents();
}
