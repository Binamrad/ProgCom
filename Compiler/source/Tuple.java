
public class Tuple<A, B>{
	public final A x;
	public final B y;
	public Tuple(A X, B Y) {
		x = X;
		y = Y;
	}
	public String toString() {
		return "["+x.toString()+":"+y.toString()+"]";
		
	}
}
