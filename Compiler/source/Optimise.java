import java.util.HashMap;
import java.util.HashSet;
import java.util.LinkedList;



public class Optimise {
	
	public static int removeUnneccessaryStores(LinkedList<lineWrapper> functionLines) {
		int maxVar = 0;
		HashSet<Integer> used = new HashSet<Integer>();
		LinkedList<lineWrapper> functionLinesCopy = new LinkedList<lineWrapper>();
		for(lineWrapper l : functionLines) {
			if(l.getComponents() == 4) {
				if(l.getComponent(0).equals("addi") || l.getComponent(0).equals("rd")) {
					if(l.getComponent(2).equals("fp")) {
						int i = Integer.parseInt(l.getComponent(3));
						used.add(i);
						if (i+1 > maxVar) {
							maxVar = i+1;
						}
					}
				}
			}
		}
		//now we have a set of all used stack spaces.
		//remove all stores that are not writing to those locations
		int i = 0;
		for(lineWrapper l : functionLines) {
			if(l.getComponents() == 4) {
				if(l.getComponent(0).equals("wr") && l.getComponent(2).equals("fp")) {
					if(used.contains(Integer.parseInt(l.getComponent(3)))) {
						functionLinesCopy.addLast(l);
					} else {
						i++;
					}
				} else {
					functionLinesCopy.addLast(l);
				}
			} else {
				functionLinesCopy.addLast(l);
			}
		}
		//System.out.println("Supposedly culled " + i + " lines");
		//replace contents of functionLines with the culled list
		functionLines.clear();
		functionLines.addAll(functionLinesCopy);
		return maxVar;
	}
	
	
	public static void removeUnneccessaryStoresPreReturn(LinkedList<lineWrapper> functionLines) {
		if(functionLines.size() == 0) {
			return;
		}
		//Acquire a list of instructions to examine
		LinkedList<lineWrapper> examinedLines = new LinkedList<lineWrapper>();
		lineWrapper ltemp = functionLines.removeLast();
		while(ltemp != null && !isBranch(ltemp.getComponent(0))) {
			examinedLines.addFirst(ltemp);
			if(functionLines.size() > 0) {
				ltemp = functionLines.removeLast();
			} else {
				ltemp = null;
			}
		}
		if(ltemp != null) {
			functionLines.addLast(ltemp);
		}
		
		
		LinkedList<lineWrapper> tempLines = new LinkedList<lineWrapper>();
		LinkedList<Integer> placesLoadedFrom = new LinkedList<Integer>();
		while(examinedLines.size() > 0) {
			lineWrapper l = examinedLines.removeLast();
			if(l.getComponent(0).equals("wr") && l.getComponent(2).equals("fp")) {
				if(placesLoadedFrom.contains(Integer.parseInt(l.getComponent(3)))) {
					tempLines.addFirst(l);
				} else {
					//System.out.println("Culled " + l.compile(0, false));
				}
			} else if(l.getComponent(0).equals("rd") && l.getComponent(2).equals("fp")) {
				tempLines.addFirst(l);
				placesLoadedFrom.add(Integer.parseInt(l.getComponent(3)));
			} else {
				tempLines.addFirst(l);
			}
		}
		//put all lines back 
		for(lineWrapper l : tempLines) {//restore the optimised lines to the end of the thing
			functionLines.addLast(l);
		}
	}
	
	
	public static void optimiseUntilBranch(LinkedList<lineWrapper> functionLines, int endRegisters, boolean useMaxRegs) {
		if(functionLines.size() == 0) {
			return;
		}
		
		LinkedList<lineWrapper> examinedLines = new LinkedList<lineWrapper>();
		
		lineWrapper ltemp = functionLines.removeLast();
		while(ltemp != null && !isBranch(ltemp.getComponent(0))) {
			examinedLines.addFirst(ltemp);
			if(functionLines.size() != 0) {
				ltemp = functionLines.removeLast();
			} else {
				ltemp = null;
			}
		}
		if(ltemp != null) {
			functionLines.addLast(ltemp);
		}
		
		//System.out.println("---------------------------------------------------------------");
		//System.out.println("BEFORE:");
		//for(lineWrapper l : examinedLines) {
		//	System.out.println(l.compile(0, false));
		//}
		
		LinkedList<lineWrapper> tmpLines = new LinkedList<lineWrapper>();
		boolean moved = false;
		int q = 0;
		do {
			moved = false;
			while(examinedLines.size() > 0) {
				lineWrapper l = examinedLines.removeLast();
				moved = moveInstructionUp(examinedLines, l, tmpLines, endRegisters) ? true : moved;
			}
			for(lineWrapper l : tmpLines) {
				examinedLines.addLast(l);
			}
			tmpLines.clear();
			++q;
		} while(moved && q < examinedLines.size());
		
		
		//System.out.println("\n\nAFTER:");
		//for(lineWrapper l : examinedLines) {
		//	System.out.println(l.compile(0, false));
		//}
		
		
		for(lineWrapper l : examinedLines) {//restore the optimised lines to the end of the thing
			functionLines.addLast(l);
		}
	}
	
	//will move an instruction 'l' as far up as possible in the list examinedLines
	//all instructions that are placed below it are placed in outList
	private static boolean moveInstructionUp(LinkedList<lineWrapper> examinedLines, lineWrapper l, LinkedList<lineWrapper> outList, int endReg) {
		//System.out.println(examinedLines.size());
		if(l.getComponent(0).equals("mov") && l.getComponent(2).equals("ex")) {
			outList.addFirst(l);
			return false;
		}
		
		if(examinedLines.size() == 0) {
			outList.addFirst(l);
			return false;
		}
		if(l.getComponent(0).equals("mov")) {
			if( (getRegNum(l.getComponent(2)) <= endReg && !usesReg(outList, l.getComponent(2)))
					|| ( usesReg(outList, l.getComponent(2)) && !assignsRegBeforeDereference(outList, l.getComponent(2)) ) ) {
				//System.out.println("tried to move " + l.compile(0, false) + " but second register was used later on");
				outList.addFirst(l);
				return false;
			}
			
			boolean retVal = false;
			while(examinedLines.size() > 0) {
				lineWrapper l2 = examinedLines.removeLast();
				if(l2.getComponent(0).startsWith(";")) {
					outList.addFirst(l2);
					continue;
				}
				if(l2.getComponents() == 2) {
					if(l2.getComponent(1).equals(l.getComponent(2)) || l2.getComponent(1).equals(l.getComponent(1))) {
						outList.addFirst(l);
						examinedLines.addLast(l2);
						if(retVal) {
							//System.out.println("moved " + l.compile(0, false));
						}
						return retVal;
					} else {
						outList.addFirst(l2);
					}
				} else {
					if(l2.getComponent(1).equals(l.getComponent(2))) {
						l2.setComponent(1, l.getComponent(1));
						outList.addFirst(l2);
						if(!l2.getComponent(0).equals("wr") && !l2.getComponent(0).equals("wrr")) {
							//System.out.println("moved " + l.compile(0, false));
							return true;
						} else {
							//make sure rB and rC have changes reflected on them
							//we only need to do this when reading stuff, since otherwise we would have stopped by now
							for(int i = 2; i < l2.getComponents(); ++i) {
								if( l2.getComponent(i).equals(l.getComponent(2)) ) {
									l2.setComponent(i, l.getComponent(1));
								}
							}
						}
					} else if( l2.getComponent(2).equals(l.getComponent(1))
							|| ( l2.getComponents()==4 && l2.getComponent(3).equals(l.getComponent(1)) )
							|| ( (l2.getComponent(0).equals("wr") || l2.getComponent(0).equals("wrr")) && l2.getComponent(1).equals(l.getComponent(1)) ) ) {
						outList.addFirst(l);
						examinedLines.addLast(l2);
						if(retVal) {
							//System.out.println("moved " + l.compile(0, false));
						} else {
							//System.out.println("couldn't move  " + l.compile(0, false) + " since above it was " + l2.compile(0, false));
						}
						return retVal;
					} else {
						for(int i = 2; i < l2.getComponents(); ++i) {// check if any of the components need to be changed to component 1
							if( l2.getComponent(i).equals(l.getComponent(2)) ) {
								l2.setComponent(i, l.getComponent(1));
							}
						}
						outList.addFirst(l2);
					}
				}
				retVal = true;
			}
			outList.addFirst(l);
			if(retVal) {
				//System.out.println("reach the end of the mov loop with " + l.compile(0, false));
			}
			return retVal;
			//go upwards until component 2 is assigned to, add the mov after that instruction
		} else if(l.getComponent(0).equals("wr") && l.getComponent(2).equals("fp")) {
			//go upwards until component 1 is assigned to, add the wr after that instruction
			boolean retVal = false;
			while(examinedLines.size() > 0) {
				lineWrapper l2 = examinedLines.removeLast();
				if(l2.getComponent(0).startsWith(";")) {
					outList.addFirst(l2);
					continue;
				}
				if(l2.getComponents() == 2) {
					if(l2.getComponent(1).equals(l.getComponent(1))) {
						outList.addFirst(l);
						examinedLines.addLast(l2);
						if(retVal) {
							//System.out.println("moved " + l.compile(0, false));
						}
						return retVal;
					} else {
						outList.addFirst(l2);
					}
				} else {
					if(l2.getComponent(1).equals(l.getComponent(1)) && !(l2.getComponent(0).equals("wr") || l2.getComponent(0).equals("wrr"))) {
						outList.addFirst(l);
						examinedLines.addLast(l2);
						if(retVal) {
							//System.out.println("moved " + l.compile(0, false));
						}
						return retVal;
					} else {
						outList.addFirst(l2);
					}
				}
				retVal = true;
			}
			outList.addFirst(l);
			if(retVal) {
				//System.out.println("moved " + l.compile(0, false));
			}
			return true;
		} else {
			outList.addFirst(l);
			return false;
		}
		
	}
	
	private static int getRegNum(String s) {
		if(s.startsWith("r")) {
			return Integer.parseInt(s.substring(1));
		} else if (s.startsWith("a")) {
			return Integer.parseInt(s.substring(1))+12;
		}
		throw new InternalCompilerException("WOAH WAT THE FUCK M8");
	}
	
	private static boolean usesReg(LinkedList<lineWrapper> list, String reg) {
		for(lineWrapper l : list) {
			for(int i = 1; i < l.getComponents(); ++i) {
				if(l.getComponent(i).equals(reg)) {
					return true;
				}
			}
		}
		return false;
	}
	
	private static boolean assignsRegBeforeDereference(LinkedList<lineWrapper> list, String reg) {
		for(lineWrapper l : list) {
			if(l.getComponents() > 1) {
				for(int i = 2; i < l.getComponents(); ++i) {
					if(l.getComponent(i).equals(reg)) {
						return false;
					}
				}
				
				if(l.getComponent(1).equals(reg)) {
					if(l.getComponent(0).equals("wr") || l.getComponent(0).equals("wrr")) {
						return false;
					} else {
						return true;
					}
				}
			}
		}
		return false;
	}
	
	public static boolean isBranch(String s) {
		return(s.equals("br") || s.equals("brr") || s.equals("jmp")
				|| s.equals("beq") || s.equals("bne") || s.equals("jmpr")
				|| s.equals("bl") || s.equals("ble") || s.equals("call")
				|| s.equals("callr") || s.endsWith(":") || s.equals(";---END INLINE ASSEMBLER---"));
	}
	
}
