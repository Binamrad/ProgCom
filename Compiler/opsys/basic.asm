.text
#global main
main:
	push	ra
	push	fp
	mov	fp, sp
	addi	sp, sp, 4
;__TMP0=30
	movil	r1, 30
;alloc: __TMP0
	call	salloc
;alloc: vars
;__TMP0=30
	movil	r2, 30
;alloc: __TMP0
	wr	r1, r0, vars
	mov	r1, r2
	call	salloc
;alloc: varTypes
;__TMP0=128
	movil	r2, 128
;alloc: __TMP0
	wr	r1, r0, varTypes
	mov	r1, r2
	call	salloc
;alloc: stack
;freeStrNum=maxLines+1
	rd	r2, r0, maxLines
	addi	r3, r2, 1
;alloc: maxLines
	wr	r1, r0, stack
	mov	r1, r2
	wr	r3, r0, freeStrNum
	call	salloc
;alloc: lineNrs
;alloc: maxLines
	wr	r1, r0, lineNrs
	rd	r1, r0, maxLines
	call	salloc
	mov	r2, r1
;alloc: __TMP0
;__TMP0=(char##)__TMP0
;lineStr=__TMP0
;__TMP0=maxLines+1
	rd	r1, r0, maxLines
	addi	r1, r1, 1
;alloc: __TMP0
	wr	r2, r0, lineStr
	call	salloc
;alloc: __TMP0
;__TMP0=(char##)__TMP0
;freeStrs=__TMP0
	mov	r2, r1
;i=__ZERO
	mov	r1, r0
;START WHILE
	wr	r1, fp, 1
	wr	r2, r0, freeStrs
while2:
;i<freeStrNum
	rd	r1, r0, freeStrNum
	rd	r2, fp, 1
	ble	r1, r2, endwhile2
;__TMP0=freeStrs+i
	rd	r3, r0, freeStrs
	add	r4, r3, r2
	wr	r4, fp, 0
;__TMP1=67
	movil	r1, 67
;alloc: __TMP1
	call	salloc
;alloc: __TMP1
;__TMP1=(char#)__TMP1
;__TMP0<-__TMP1
	rd	r2, fp, 0
	wr	r1, r2, 0
;i++
	rd	r1, fp, 1
	addi	r1, r1, 1
	wr	r1, fp, 1
	br	while2
endwhile2:
;END WHILE
;__TMP1="\n\r"
	movi	r1, string0
;alloc: __TMP1
	call	prints
;START WHILE
while8:
;running!=__ZERO
	rd	r1, r0, running
	beq	r1, r0, endwhile8
;START IF
;mode==__ZERO
	rd	r2, r0, mode
	bne	r2, r0, if10
;__TMP1="Ready\n\r"
	movi	r1, string1
;alloc: __TMP1
	call	prints
	call	getString
	wr	r1, fp, 3
;alloc: string
;__TMP1=65
	movil	r2, 65
;alloc: string __TMP1
	call	reads
;alloc: string
	rd	r1, fp, 3
	call	strlen
;alloc: stringlen
;__TMP1=string+stringlen
	rd	r2, fp, 3
	add	r3, r2, r1
;__TMP0="\n"
	movi	r4, string2
;__TMP0=__TMP0#
	rd	r4, r4, 0
;__TMP1<-__TMP0
	wr	r4, r3, 0
;stringlen++
	addi	r1, r1, 1
;__TMP0=string+stringlen
	add	r3, r2, r1
;__TMP1="\r"
	movi	r4, string3
;__TMP1=__TMP1#
	rd	r4, r4, 0
;__TMP0<-__TMP1
	wr	r4, r3, 0
;stringlen++
	addi	r1, r1, 1
;__TMP1=string+stringlen
	add	r3, r2, r1
	mov	r1, r2
;__TMP0=(char)__ZERO
	andi	r4, r0, 255
;__TMP1<-__TMP0
	wr	r4, r3, 0
;alloc: string __ZERO
	mov	r2, r0
	call	line
	rd	r1, r0, running
	rd	r2, r0, mode
	br	endif10
if10:
;__TMP0=lineStr+execLine
;string=(lineStr+execLine)*
	rd	r4, r0, lineStr
	rd	r5, r0, execLine
	rdr	r1, r4, r5
	wr	r1, fp, 3
;execLine++
	addi	r5, r5, 1
;__TMP0=1
	movil	r2, 1
;alloc: string __TMP0
	wr	r5, r0, execLine
	call	line
;START IF
;execLine>=activeLines
	rd	r1, r0, execLine
	rd	r2, r0, activeLines
	bl	r1, r2, if14
;mode=__ZERO
	mov	r3, r0
	wr	r3, r0, mode
if14:
;END IF
	rd	r1, r0, running
	rd	r2, r0, mode
endif10:
;END IF
	br	while8
endwhile8:
;END WHILE
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
#global findLine
findLine:
;alloc: lineNr
;low=__ZERO
	mov	r2, r0
;high=activeLines-1
	rd	r3, r0, activeLines
	subi	r4, r3, 1
;START WHILE
while22:
;low<=high
	bl	r4, r2, endwhile22
;__TMP0=low+high
	add	r5, r2, r4
;mid=__TMP0>>1
	sri	r6, r5, 1
;__TMP0=lineNrs+mid
;index=(lineNrs+mid)*
	rd	r7, r0, lineNrs
	rdr	r5, r7, r6
;START IF
;index==lineNr
	bne	r5, r1, if24
	mov	r2, r6
;__TMP0=1
	movil	r1, 1
;alloc: __TMP0 mid
;return
	jmpr	ra
if24:
;START IF
;index<lineNr
	ble	r1, r5, if28
;low=mid+1
	addi	r2, r6, 1
	br	endif28
if28:
;high=mid-1
	subi	r4, r6, 1
endif28:
;END IF
endif24:
;END IF
	br	while22
endwhile22:
;END WHILE
;alloc: __ZERO __ZERO
	mov	r1, r0
	mov	r2, r0
;return
	jmpr	ra
#global deleteLine
deleteLine:
	push	ra
	push	fp
	mov	fp, sp
	addi	sp, sp, 1
;alloc: lineNr
;alloc: lineNr
	call	findLine
;alloc: status lineNr
;START IF
;status==__ZERO
	bne	r1, r0, if37
	wr	r2, fp, 0
;__TMP0="Line does not exist"
	movi	r1, string4
;alloc: __TMP0
	call	prints
;__TMP0="\n\r"
	movi	r1, string0
;alloc: __TMP0
	call	prints
;alloc: __ZERO
	mov	r1, r0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if37:
	wr	r2, fp, 0
;END IF
;__TMP0=lineStr+lineNr
;string=(lineStr+lineNr)*
	rd	r4, r0, lineStr
	rdr	r1, r4, r2
;alloc: string
	call	restoreString
;i=lineNr+1
	rd	r1, fp, 0
	addi	r2, r1, 1
;START WHILE
while43:
;i<activeLines
	rd	r3, r0, activeLines
	ble	r3, r2, endwhile43
;__TMP0=lineStr+i
	rd	r4, r0, lineStr
	add	r5, r4, r2
;__TMP0=__TMP0-1
	subi	r5, r5, 1
;__TMP1=lineStr+i
;__TMP1=(lineStr+i)*
	rdr	r6, r4, r2
;__TMP0<-__TMP1
	wr	r6, r5, 0
;__TMP1=lineNrs+i
	rd	r5, r0, lineNrs
	add	r6, r5, r2
;__TMP1=__TMP1-1
	subi	r6, r6, 1
;__TMP0=lineNrs+i
;__TMP0=(lineNrs+i)*
	rdr	r7, r5, r2
;__TMP1<-__TMP0
	wr	r7, r6, 0
;i++
	addi	r2, r2, 1
	br	while43
endwhile43:
;END WHILE
;activeLines--
	subi	r3, r3, 1
;__TMP0=1
	movil	r1, 1
	wr	r3, r0, activeLines
;alloc: __TMP0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
#global restoreString
restoreString:
;alloc: string
;__TMP0=freeStrs+freeStrNum
	rd	r2, r0, freeStrs
	rd	r3, r0, freeStrNum
	add	r4, r2, r3
;__TMP0<-string
	wr	r1, r4, 0
;freeStrNum++
	addi	r3, r3, 1
	wr	r3, r0, freeStrNum
;return
	jmpr	ra
#global sortLines
sortLines:
;i=1
	movil	r1, 1
;START WHILE
while51:
;i<activeLines
	rd	r2, r0, activeLines
	ble	r2, r1, endwhile51
;k=i
	mov	r3, r1
;START WHILE
while54:
;k>__ZERO
	ble	r3, r0, endwhile54
;__TMP0=lineNrs+k
;num1=(lineNrs+k)*
	rd	r5, r0, lineNrs
	rdr	r4, r5, r3
;__TMP0=lineNrs-1
	subi	r6, r5, 1
;__TMP0=__TMP0+k
;num2=(__TMP0+k)*
	rdr	r7, r6, r3
;START IF
;num1>num2
	ble	r4, r7, if57
;break
	br	endwhile54
if57:
;END IF
;__TMP1=lineNrs+k
	add	r8, r5, r3
;__TMP1<-num2
	wr	r7, r8, 0
;__TMP1=lineNrs-1
	subi	r8, r5, 1
;__TMP1=__TMP1+k
	add	r8, r8, r3
;__TMP1<-num1
	wr	r4, r8, 0
;__TMP1=lineStr+k
;str=(lineStr+k)*
	rd	r9, r0, lineStr
	rdr	r8, r9, r3
;__TMP1=lineStr+k
	add	r10, r9, r3
;__TMP2=lineStr-1
	subi	r11, r9, 1
;__TMP2=__TMP2+k
;__TMP2=(__TMP2+k)*
	rdr	r11, r11, r3
;__TMP1<-__TMP2
	wr	r11, r10, 0
;__TMP2=lineStr-1
	subi	r10, r9, 1
;__TMP2=__TMP2+k
	add	r10, r10, r3
;__TMP2<-str
	wr	r8, r10, 0
;k--
	subi	r3, r3, 1
	br	while54
endwhile54:
;END WHILE
;i++
	addi	r1, r1, 1
	br	while51
endwhile51:
;END WHILE
;return
	jmpr	ra
#global getString
getString:
;START IF
;freeStrNum<=__ZERO
	rd	r1, r0, freeStrNum
	bl	r0, r1, if67
;__TMP0=(char#)__ZERO
	mov	r1, r0
;alloc: __TMP0
;return
	jmpr	ra
if67:
;END IF
;freeStrNum--
	subi	r1, r1, 1
;__TMP0=freeStrs+freeStrNum
;string=(freeStrs+freeStrNum)*
	rd	r3, r0, freeStrs
	rdr	r2, r3, r1
	wr	r1, r0, freeStrNum
	mov	r1, r2
;alloc: string
;return
	jmpr	ra
#global line
line:
	push	ra
	push	fp
	mov	fp, sp
	addi	sp, sp, 6
	wr	r1, fp, 0
	wr	r2, fp, 1
;alloc: str ignoreLineNr
;alloc: str
	call	number
;alloc: status result str2
;START IF
;__TMP0=status==__ZERO
	cmp	r4, r1, r0
	not	r4
	andi	r4, r4, 1
;__TMP0=__TMP0|ignoreLineNr
	rd	r5, fp, 1
	or	r4, r4, r5
	beq	r4, r0, if77
;execStatement=1
	movil	r4, 1
	wr	r4, r0, execStatement
	br	endif77
if77:
;execStatement=__ZERO
	mov	r4, r0
;lineNumber=result
	mov	r6, r2
	wr	r4, r0, execStatement
	wr	r6, fp, 4
endif77:
	mov	r1, r3
;END IF
;alloc: str2
	call	statement
;alloc: status str2
;START IF
;__TMP0=status!=__ZERO
	cmp	r3, r1, r0
	andi	r3, r3, 1
;__TMP1=execStatement==__ZERO
	rd	r4, r0, execStatement
	cmp	r5, r4, r0
	not	r5
	andi	r5, r5, 1
;__TMP0=__TMP0&__TMP1
	and	r3, r3, r5
	beq	r3, r0, if82
	mov	r1, r2
;__TMP0="\n\r"
	movi	r2, string0
;alloc: str2 __TMP0
	call	findKeyword
;alloc: status str2
;START IF
;status==__ZERO
	bne	r1, r0, if84
	wr	r2, fp, 5
	wr	r1, fp, 2
;__TMP0="MISSING LINE BREAK\n\r"
	movi	r1, string5
;alloc: __TMP0
	call	prints
;alloc: str
	rd	r1, fp, 0
	call	restoreString
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if84:
;END IF
;START IF
;activeLines>=maxLines
	rd	r3, r0, activeLines
	rd	r4, r0, maxLines
	bl	r3, r4, if88
	wr	r2, fp, 5
	wr	r1, fp, 2
;__TMP0="PROGRAM LINE OVERFLOW\n\r"
	movi	r1, string6
;alloc: __TMP0
	call	prints
;alloc: str
	rd	r1, fp, 0
	call	restoreString
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if88:
;END IF
;__TMP0=lineStr+activeLines
	rd	r5, r0, lineStr
	add	r6, r5, r3
;__TMP0<-str
	rd	r7, fp, 0
	wr	r7, r6, 0
;__TMP0=lineNrs+activeLines
	rd	r6, r0, lineNrs
	add	r8, r6, r3
;__TMP0<-lineNumber
	rd	r9, fp, 4
	wr	r9, r8, 0
;activeLines++
	addi	r3, r3, 1
	wr	r3, r0, activeLines
	rd	r4, r0, execStatement
	br	endif82
if82:
;START IF
;ignoreLineNr==__ZERO
	rd	r3, fp, 1
	bne	r3, r0, if93
	wr	r1, fp, 2
	wr	r2, fp, 5
;alloc: str
	rd	r1, fp, 0
	call	restoreString
	rd	r1, fp, 2
	rd	r2, fp, 5
	rd	r3, fp, 1
	rd	r4, r0, execStatement
if93:
;END IF
endif82:
;END IF
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
#global statement
statement:
	push	ra
	push	fp
	mov	fp, sp
	addi	sp, sp, 9
;alloc: str
;__TMP0="LET"
	movi	r2, string7
;alloc: str __TMP0
	call	findKeyword
;alloc: status str
;START IF
;status!=__ZERO
	beq	r1, r0, if102
	mov	r1, r2
	wr	r1, fp, 0
;alloc: str
	call	variable
;alloc: status tmp str2
;START IF
;status==__ZERO
	bne	r1, r0, if104
	wr	r1, fp, 1
	wr	r3, fp, 4
	wr	r2, fp, 3
;__TMP0="Not a variable"
	movi	r2, string8
;alloc: str __TMP0
	rd	r1, fp, 0
	call	printSyntaxErr
;alloc: __ZERO str
	mov	r1, r0
	rd	r2, fp, 0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if104:
	wr	r2, fp, 3
	mov	r1, r3
	wr	r1, fp, 0
;END IF
;str=str2
;__TMP0="="
	movi	r2, string9
;alloc: str __TMP0
	call	findKeyword
;alloc: status str2
;START IF
;status==__ZERO
	bne	r1, r0, if108
	wr	r2, fp, 4
	wr	r1, fp, 1
;__TMP0="Expected '='"
	movi	r2, string10
;alloc: str __TMP0
	rd	r1, fp, 0
	call	printSyntaxErr
;alloc: __ZERO str
	mov	r1, r0
	rd	r2, fp, 0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if108:
	mov	r1, r2
;END IF
;str=str2
	wr	r1, fp, 0
;alloc: str
	call	expression
;alloc: status result str2
;START IF
;status==__ZERO
	bne	r1, r0, if112
	wr	r1, fp, 1
	wr	r3, fp, 4
	wr	r2, fp, 2
;__TMP0="Malformed expression"
	movi	r2, string11
;alloc: str __TMP0
	rd	r1, fp, 0
	call	printSyntaxErr
;alloc: __ZERO str
	mov	r1, r0
	rd	r2, fp, 0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if112:
;END IF
;str=str2
	mov	r4, r3
;START IF
;execStatement!=__ZERO
	rd	r5, r0, execStatement
	beq	r5, r0, if116
;__TMP0=vars+tmp
	rd	r6, r0, vars
	rd	r7, fp, 3
	add	r8, r6, r7
;__TMP0<-result
	wr	r2, r8, 0
if116:
	mov	r2, r4
;END IF
;__TMP0=1
	movil	r1, 1
;alloc: __TMP0 str
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if102:
	mov	r1, r2
;END IF
;__TMP0="PRINT"
	movi	r2, string12
;alloc: str __TMP0
	call	findKeyword
;alloc: status str
;START IF
;status!=__ZERO
	beq	r1, r0, if121
	mov	r1, r2
;alloc: str
	call	exprList
;alloc: status str
;alloc: status str
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if121:
	mov	r1, r2
;END IF
;__TMP0="IF"
	movi	r2, string13
;alloc: str __TMP0
	call	findKeyword
;alloc: status str
;START IF
;status!=__ZERO
	beq	r1, r0, if125
	mov	r1, r2
;alloc: str
	call	expression
;alloc: status exprRes1 str
;START IF
;status==__ZERO
	bne	r1, r0, if128
	wr	r1, fp, 1
	wr	r2, fp, 8
	mov	r1, r3
	wr	r1, fp, 0
;__TMP0="Malformed expression"
	movi	r2, string11
;alloc: str __TMP0
	call	printSyntaxErr
;alloc: __ZERO str
	mov	r1, r0
	rd	r2, fp, 0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if128:
	wr	r2, fp, 8
	mov	r1, r3
;END IF
;__TMP0=1
	movil	r2, 1
;alloc: str __TMP0
	call	relop
;alloc: status operator str
;START IF
;status==__ZERO
	bne	r1, r0, if132
	wr	r1, fp, 1
	wr	r2, fp, 6
	mov	r1, r3
	wr	r1, fp, 0
;__TMP0="Malformed comparator"
	movi	r2, string14
;alloc: str __TMP0
	call	printSyntaxErr
;alloc: __ZERO str
	mov	r1, r0
	rd	r2, fp, 0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if132:
	mov	r1, r3
	wr	r2, fp, 6
;END IF
;alloc: str
	call	expression
;alloc: status exprRes2 str
;START IF
;status==__ZERO
	bne	r1, r0, if136
	wr	r1, fp, 1
	mov	r1, r3
	wr	r1, fp, 0
;__TMP0="Malformed expression"
	movi	r2, string11
;alloc: str __TMP0
	call	printSyntaxErr
;alloc: __ZERO str
	mov	r1, r0
	rd	r2, fp, 0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if136:
;END IF
;START IF
;__TMP0=4
	movil	r4, 4
;operator==__TMP0
	rd	r5, fp, 6
	bne	r5, r4, if140
;tmp=exprRes1<exprRes2
	movi	r4, 0
	rd	r6, fp, 8
	ble	r2, r6, 1
	movi	r4, 1
	wr	r4, fp, 3
	br	endif140
if140:
;START IF
;__TMP0=7
	movil	r4, 7
;operator==__TMP0
	bne	r5, r4, if143
;tmp=exprRes1<=exprRes2
	movi	r4, 0
	rd	r6, fp, 8
	bl	r2, r6, 1
	movi	r4, 1
	wr	r4, fp, 3
	br	endif143
if143:
;START IF
;__TMP0=8
	movil	r4, 8
;operator==__TMP0
	bne	r5, r4, if146
;tmp=exprRes1>exprRes2
	movi	r4, 0
	rd	r6, fp, 8
	ble	r6, r2, 1
	movi	r4, 1
	wr	r4, fp, 3
	br	endif146
if146:
;START IF
;__TMP0=11
	movil	r4, 11
;operator==__TMP0
	bne	r5, r4, if149
;tmp=exprRes1>=exprRes2
	movi	r4, 0
	rd	r6, fp, 8
	bl	r6, r2, 1
	movi	r4, 1
	wr	r4, fp, 3
	br	endif149
if149:
;START IF
;__TMP0=12
	movil	r4, 12
;operator==__TMP0
	bne	r5, r4, if152
;tmp=exprRes1==exprRes2
	rd	r4, fp, 8
	cmp	r6, r4, r2
	not	r6
	andi	r6, r6, 1
	wr	r6, fp, 3
	br	endif152
if152:
;START IF
;__TMP0=6
	movil	r4, 6
;__TMP0=operator==__TMP0
	cmp	r4, r5, r4
	not	r4
	andi	r4, r4, 1
;__TMP1=9
	movil	r6, 9
;__TMP1=operator==__TMP1
	cmp	r6, r5, r6
	not	r6
	andi	r6, r6, 1
;__TMP0=__TMP0|__TMP1
	or	r4, r4, r6
	beq	r4, r0, if155
;tmp=exprRes1!=exprRes2
	rd	r4, fp, 8
	cmp	r6, r4, r2
	andi	r6, r6, 1
	wr	r6, fp, 3
	br	endif155
if155:
	wr	r1, fp, 1
	mov	r1, r3
	wr	r1, fp, 0
;__TMP0="Not a comparator"
	movi	r2, string15
;alloc: str __TMP0
	call	printSyntaxErr
;alloc: __ZERO str
	mov	r1, r0
	rd	r2, fp, 0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
endif155:
;END IF
endif152:
;END IF
endif149:
;END IF
endif146:
;END IF
endif143:
;END IF
endif140:
;END IF
;START IF
;__TMP0=tmp!=__ZERO
	rd	r4, fp, 3
	cmp	r6, r4, r0
	andi	r6, r6, 1
;__TMP1=execStatement==__ZERO
	rd	r7, r0, execStatement
	cmp	r8, r7, r0
	not	r8
	andi	r8, r8, 1
;__TMP0=__TMP0|__TMP1
	or	r6, r6, r8
	beq	r6, r0, if161
	mov	r1, r3
;alloc: str
	call	statement
;alloc: status str
;alloc: status str
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if161:
	mov	r2, r3
;END IF
;__TMP0=1
	movil	r1, 1
;alloc: __TMP0 str
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if125:
	mov	r1, r2
;END IF
;__TMP0="GOTO"
	movi	r2, string16
;alloc: str __TMP0
	call	findKeyword
;alloc: status str
;START IF
;status!=__ZERO
	beq	r1, r0, if167
	mov	r1, r2
;alloc: str
	call	expression
;alloc: status tmp str
;START IF
;status==__ZERO
	bne	r1, r0, if169
	wr	r1, fp, 1
	wr	r2, fp, 3
	mov	r1, r3
	wr	r1, fp, 0
;__TMP0="Malformed expression"
	movi	r2, string11
;alloc: str __TMP0
	call	printSyntaxErr
	rd	r1, fp, 1
	rd	r2, fp, 3
	rd	r3, fp, 0
if169:
;END IF
;START IF
	rd	r4, r0, execStatement
	beq	r4, r0, if172
;START IF
;mode==__ZERO
	rd	r5, r0, mode
	bne	r5, r0, if174
	wr	r1, fp, 1
	wr	r2, fp, 3
	wr	r3, fp, 0
	call	sortLines
	rd	r1, fp, 1
	rd	r2, fp, 3
	rd	r3, fp, 0
	rd	r4, r0, execStatement
	rd	r5, r0, mode
if174:
	mov	r1, r2
	wr	r3, fp, 0
;END IF
;alloc: tmp
	call	findLine
;alloc: status tmp
;START IF
;status==__ZERO
	bne	r1, r0, if177
	wr	r2, fp, 3
	wr	r1, fp, 1
;__TMP0="Line does not exist"
	movi	r2, string4
;alloc: str __TMP0
	rd	r1, fp, 0
	call	printSyntaxErr
;alloc: __ZERO str
	mov	r1, r0
	rd	r2, fp, 0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if177:
;END IF
;execLine=tmp
	mov	r3, r2
	wr	r3, r0, execLine
	rd	r3, fp, 0
	rd	r4, r0, execStatement
if172:
	mov	r2, r3
;END IF
;__TMP0=1
	movil	r1, 1
;alloc: __TMP0 str
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if167:
	mov	r1, r2
;END IF
;__TMP0="INPUT"
	movi	r2, string17
;alloc: str __TMP0
	call	findKeyword
;alloc: status str
;START IF
;status!=__ZERO
	beq	r1, r0, if184
	mov	r1, r2
;alloc: str
	call	varList
;alloc: status str
;START IF
;status==__ZERO
	bne	r1, r0, if186
;alloc: __ZERO str
	mov	r1, r0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if186:
;END IF
;__TMP0=1
	movil	r1, 1
;alloc: __TMP0 str
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if184:
	mov	r1, r2
;END IF
;__TMP0="GOSUB"
	movi	r2, string18
;alloc: str __TMP0
	call	findKeyword
;alloc: status str
;START IF
;status!=__ZERO
	beq	r1, r0, if192
;START IF
;execStatement!=__ZERO
	rd	r3, r0, execStatement
	beq	r3, r0, if194
;START IF
;__TMP0=128
	movil	r4, 128
;stackPointer>=__TMP0
	rd	r5, r0, stackPointer
	bl	r5, r4, if196
	wr	r1, fp, 1
	mov	r1, r2
	wr	r1, fp, 0
;__TMP0="Stack overflow"
	movi	r2, string19
;alloc: str __TMP0
	call	printSyntaxErr
;alloc: __ZERO str
	mov	r1, r0
	rd	r2, fp, 0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if196:
;END IF
;__TMP0=stack+stackPointer
	rd	r4, r0, stack
	add	r6, r4, r5
;__TMP0<-execLine
	rd	r7, r0, execLine
	wr	r7, r6, 0
;stackPointer++
	addi	r5, r5, 1
	wr	r5, r0, stackPointer
if194:
;END IF
;__TMP0=1
	movil	r1, 1
;alloc: __TMP0 str
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if192:
	mov	r1, r2
;END IF
;__TMP0="RETURN"
	movi	r2, string20
;alloc: str __TMP0
	call	findKeyword
;alloc: status str
;START IF
;status!=__ZERO
	beq	r1, r0, if203
;START IF
;execStatement!=__ZERO
	rd	r3, r0, execStatement
	beq	r3, r0, if205
;START IF
;stackPointer<=__ZERO
	rd	r4, r0, stackPointer
	bl	r0, r4, if207
	wr	r1, fp, 1
	mov	r1, r2
	wr	r1, fp, 0
;__TMP0="No return address"
	movi	r2, string21
;alloc: str __TMP0
	call	printSyntaxErr
;alloc: __ZERO str
	mov	r1, r0
	rd	r2, fp, 0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if207:
;END IF
;stackPointer--
	subi	r4, r4, 1
;__TMP0=stack+stackPointer
;execLine=(stack+stackPointer)*
	rd	r6, r0, stack
	rdr	r5, r6, r4
	wr	r4, r0, stackPointer
	wr	r5, r0, execLine
if205:
;END IF
;__TMP0=1
	movil	r1, 1
;alloc: __TMP0 str
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if203:
	mov	r1, r2
;END IF
;__TMP0="PEEK"
	movi	r2, string22
;alloc: str __TMP0
	call	findKeyword
;alloc: status str
;START IF
;status!=__ZERO
	beq	r1, r0, if214
	mov	r1, r2
;alloc: str
	call	expression
;alloc: status result str
;START IF
;status==__ZERO
	bne	r1, r0, if216
	wr	r1, fp, 1
	wr	r2, fp, 2
	mov	r1, r3
	wr	r1, fp, 0
;__TMP0="Malformed expression"
	movi	r2, string11
;alloc: str __TMP0
	call	printSyntaxErr
	rd	r1, fp, 1
	rd	r2, fp, 2
	rd	r3, fp, 0
if216:
	wr	r2, fp, 2
	mov	r1, r3
;END IF
;__TMP0=","
	movi	r2, string23
;alloc: str __TMP0
	call	findKeyword
;alloc: status str
;START IF
;status==__ZERO
	bne	r1, r0, if219
	wr	r1, fp, 1
	mov	r1, r2
	wr	r1, fp, 0
;__TMP0="Expected ','"
	movi	r2, string24
;alloc: str __TMP0
	call	printSyntaxErr
	rd	r1, fp, 1
	rd	r2, fp, 0
if219:
	mov	r1, r2
;END IF
;alloc: str
	call	variable
;alloc: status tmp str
;START IF
;status==__ZERO
	bne	r1, r0, if222
	wr	r1, fp, 1
	wr	r2, fp, 3
	mov	r1, r3
	wr	r1, fp, 0
;__TMP0="Not a variable"
	movi	r2, string8
;alloc: str __TMP0
	call	printSyntaxErr
	rd	r1, fp, 1
	rd	r2, fp, 3
	rd	r3, fp, 0
if222:
;END IF
;START IF
;execStatement!=__ZERO
	rd	r4, r0, execStatement
	beq	r4, r0, if225
;__TMP0=vars+tmp
	rd	r5, r0, vars
	add	r6, r5, r2
;__TMP1=(int#)result
	rd	r7, fp, 2
	mov	r8, r7
;__TMP1=__TMP1#
	rd	r8, r8, 0
;__TMP0<-__TMP1
	wr	r8, r6, 0
if225:
;END IF
;__TMP1=1
	movil	r5, 1
	wr	r2, fp, 3
	mov	r2, r3
if214:
	mov	r1, r2
;END IF
;__TMP1="POKE"
	movi	r2, string25
;alloc: str __TMP1
	call	findKeyword
;alloc: status str
;START IF
;status!=__ZERO
	beq	r1, r0, if229
	mov	r1, r2
;alloc: str
	call	expression
;alloc: status result str
;START IF
;status==__ZERO
	bne	r1, r0, if231
	wr	r1, fp, 1
	wr	r2, fp, 2
	mov	r1, r3
	wr	r1, fp, 0
;__TMP1="Malformed expression"
	movi	r2, string11
;alloc: str __TMP1
	call	printSyntaxErr
	rd	r1, fp, 1
	rd	r2, fp, 2
	rd	r3, fp, 0
if231:
	wr	r2, fp, 2
	mov	r1, r3
;END IF
;__TMP1=","
	movi	r2, string23
;alloc: str __TMP1
	call	findKeyword
;alloc: status str
;START IF
;status==__ZERO
	bne	r1, r0, if234
	wr	r1, fp, 1
	mov	r1, r2
	wr	r1, fp, 0
;__TMP1="Ecpected ','"
	movi	r2, string26
;alloc: str __TMP1
	call	printSyntaxErr
	rd	r1, fp, 1
	rd	r2, fp, 0
if234:
	mov	r1, r2
;END IF
;alloc: str
	call	expression
;alloc: status tmp str
;START IF
;status==__ZERO
	bne	r1, r0, if237
	wr	r1, fp, 1
	wr	r2, fp, 3
	mov	r1, r3
	wr	r1, fp, 0
;__TMP1="Malformed expression"
	movi	r2, string11
;alloc: str __TMP1
	call	printSyntaxErr
	rd	r1, fp, 1
	rd	r2, fp, 3
	rd	r3, fp, 0
if237:
;END IF
;START IF
;execStatement!=__ZERO
	rd	r4, r0, execStatement
	beq	r4, r0, if240
;__TMP1=(int#)result
	rd	r5, fp, 2
	mov	r6, r5
;__TMP1<-tmp
	wr	r2, r6, 0
if240:
;END IF
;__TMP1=1
	movil	r5, 1
	wr	r2, fp, 3
	mov	r2, r3
if229:
	mov	r1, r2
;END IF
;__TMP1="FCOL"
	movi	r2, string27
;alloc: str __TMP1
	call	findKeyword
;alloc: status str
;START IF
;status!=__ZERO
	beq	r1, r0, if244
	mov	r1, r2
;alloc: str
	call	expression
;alloc: status result str
;START IF
;result==__ZERO
	bne	r2, r0, if246
	wr	r1, fp, 1
	wr	r2, fp, 2
	mov	r1, r3
	wr	r1, fp, 0
;__TMP1="Malformed expression"
	movi	r2, string11
;alloc: str __TMP1
	call	printSyntaxErr
	rd	r1, fp, 1
	rd	r2, fp, 2
	rd	r3, fp, 0
if246:
;END IF
;START IF
;execStatement!=__ZERO
	rd	r4, r0, execStatement
	beq	r4, r0, if249
	wr	r3, fp, 0
	wr	r1, fp, 1
	mov	r1, r2
	wr	r1, fp, 2
;alloc: result
	call	setfgcol
	rd	r1, fp, 1
	rd	r2, fp, 2
	rd	r3, fp, 0
	rd	r4, r0, execStatement
if249:
	mov	r2, r3
;END IF
;__TMP1=1
	movil	r1, 1
;alloc: __TMP1 str
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if244:
	mov	r1, r2
;END IF
;__TMP1="BCOL"
	movi	r2, string28
;alloc: str __TMP1
	call	findKeyword
;alloc: status str
;START IF
;status!=__ZERO
	beq	r1, r0, if254
	mov	r1, r2
;alloc: str
	call	expression
;alloc: status result str
;START IF
;result==__ZERO
	bne	r2, r0, if256
	wr	r1, fp, 1
	wr	r2, fp, 2
	mov	r1, r3
	wr	r1, fp, 0
;__TMP1="Malformed expression"
	movi	r2, string11
;alloc: str __TMP1
	call	printSyntaxErr
	rd	r1, fp, 1
	rd	r2, fp, 2
	rd	r3, fp, 0
if256:
;END IF
;START IF
;execStatement!=__ZERO
	rd	r4, r0, execStatement
	beq	r4, r0, if259
	wr	r3, fp, 0
	wr	r1, fp, 1
	mov	r1, r2
	wr	r1, fp, 2
;alloc: result
	call	setbgcol
	rd	r1, fp, 1
	rd	r2, fp, 2
	rd	r3, fp, 0
	rd	r4, r0, execStatement
if259:
	mov	r2, r3
;END IF
;__TMP1=1
	movil	r1, 1
;alloc: __TMP1 str
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if254:
	mov	r1, r2
;END IF
;__TMP1="LIST"
	movi	r2, string29
;alloc: str __TMP1
	call	findKeyword
;alloc: status str
;START IF
;status!=__ZERO
	beq	r1, r0, if264
	wr	r1, fp, 1
	wr	r2, fp, 0
	call	sortLines
;tmp=__ZERO
	mov	r1, r0
;START WHILE
	wr	r1, fp, 3
while266:
;tmp<activeLines
	rd	r1, r0, activeLines
	rd	r2, fp, 3
	ble	r1, r2, endwhile266
;__TMP1=lineStr+tmp
;str2=(lineStr+tmp)*
	rd	r4, r0, lineStr
	rdr	r1, r4, r2
	wr	r1, fp, 4
;alloc: str2
	call	prints
;tmp++
	rd	r1, fp, 3
	addi	r1, r1, 1
	wr	r1, fp, 3
	br	while266
endwhile266:
;END WHILE
;alloc: status str
	rd	r1, fp, 1
	rd	r2, fp, 0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if264:
	mov	r1, r2
;END IF
;__TMP1="CLEAR"
	movi	r2, string30
;alloc: str __TMP1
	call	findKeyword
;alloc: status str
;START IF
;status!=__ZERO
	beq	r1, r0, if271
;tmp=activeLines-1
	rd	r3, r0, activeLines
	subi	r4, r3, 1
;START WHILE
	wr	r1, fp, 1
	wr	r2, fp, 0
	wr	r4, fp, 3
while273:
;tmp>=__ZERO
	rd	r1, fp, 3
	bl	r1, r0, endwhile273
;__TMP1=lineNrs+tmp
;__TMP1=(lineNrs+tmp)*
	rd	r3, r0, lineNrs
	rdr	r1, r3, r1
;alloc: __TMP1
	call	deleteLine
;alloc: __TMP1
;tmp--
	rd	r1, fp, 3
	subi	r1, r1, 1
	wr	r1, fp, 3
	br	while273
endwhile273:
;END WHILE
;stackPointer=__ZERO
	mov	r2, r0
;mode=__ZERO
	mov	r3, r0
;execLine=__ZERO
	mov	r4, r0
;__TMP1=1
	movil	r1, 1
	wr	r2, r0, stackPointer
	wr	r3, r0, mode
	wr	r4, r0, execLine
;alloc: __TMP1 str
	rd	r2, fp, 0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if271:
	mov	r1, r2
;END IF
;__TMP1="RUN"
	movi	r2, string31
;alloc: str __TMP1
	call	findKeyword
;alloc: status str
;START IF
;status!=__ZERO
	beq	r1, r0, if278
;START IF
;execStatement!=__ZERO
	rd	r3, r0, execStatement
	beq	r3, r0, if280
	wr	r1, fp, 1
	wr	r2, fp, 0
	call	sortLines
;mode=1
	movil	r1, 1
	wr	r1, r0, mode
	rd	r1, fp, 1
	rd	r2, fp, 0
	rd	r3, r0, execStatement
if280:
;END IF
;__TMP1=1
	movil	r1, 1
;alloc: __TMP1 str
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if278:
	mov	r1, r2
;END IF
;__TMP1="EDIT"
	movi	r2, string32
;alloc: str __TMP1
	call	findKeyword
;alloc: status str
;START IF
;status!=__ZERO
	beq	r1, r0, if285
	mov	r1, r2
;alloc: str
	call	expression
;alloc: status result str
;START IF
;status==__ZERO
	bne	r1, r0, if287
	wr	r1, fp, 1
	wr	r2, fp, 2
	mov	r1, r3
	wr	r1, fp, 0
;__TMP1="Malformed expression"
	movi	r2, string11
;alloc: str __TMP1
	call	printSyntaxErr
;alloc: __ZERO str
	mov	r1, r0
	rd	r2, fp, 0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if287:
;END IF
;START IF
;execStatement!=__ZERO
	rd	r4, r0, execStatement
	beq	r4, r0, if291
	wr	r1, fp, 1
	wr	r2, fp, 2
	wr	r3, fp, 0
	call	sortLines
;alloc: result
	rd	r1, fp, 2
	call	findLine
;alloc: status result
;START IF
;status==__ZERO
	bne	r1, r0, if293
	wr	r2, fp, 2
	wr	r1, fp, 1
;__TMP1="Line does not exist"
	movi	r2, string4
;alloc: str __TMP1
	rd	r1, fp, 0
	call	printSyntaxErr
;alloc: __ZERO str
	mov	r1, r0
	rd	r2, fp, 0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if293:
	wr	r1, fp, 1
	wr	r2, fp, 2
;END IF
;__TMP1=lineStr+result
;str2=(lineStr+result)*
	rd	r4, r0, lineStr
	rdr	r1, r4, r2
	wr	r1, fp, 4
;__TMP1=65
	movil	r2, 65
;alloc: str2 __TMP1
	call	edits
;alloc: str2
	rd	r1, fp, 4
	call	strlen
;alloc: status
;__TMP1=str2+status
	rd	r2, fp, 4
	add	r3, r2, r1
;__TMP0="\n"
	movi	r4, string2
;__TMP0=__TMP0#
	rd	r4, r4, 0
;__TMP1<-__TMP0
	wr	r4, r3, 0
;status++
	addi	r1, r1, 1
;__TMP0=str2+status
	add	r3, r2, r1
;__TMP1="\r"
	movi	r4, string3
;__TMP1=__TMP1#
	rd	r4, r4, 0
;__TMP0<-__TMP1
	wr	r4, r3, 0
;status++
	addi	r1, r1, 1
;__TMP1=str2+status
	add	r3, r2, r1
;__TMP0=(char)__ZERO
	andi	r4, r0, 255
;__TMP1<-__TMP0
	wr	r4, r3, 0
	rd	r2, fp, 2
	rd	r3, fp, 0
	rd	r4, r0, execStatement
if291:
	mov	r2, r3
;END IF
;__TMP0=1
	movil	r1, 1
;alloc: __TMP0 str
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if285:
	mov	r1, r2
;END IF
;__TMP0="END"
	movi	r2, string33
;alloc: str __TMP0
	call	findKeyword
;alloc: status str
;START IF
;status!=__ZERO
	beq	r1, r0, if300
;execLine=__ZERO
	mov	r3, r0
;mode=__ZERO
	mov	r4, r0
;__TMP0=1
	movil	r1, 1
	wr	r3, r0, execLine
	wr	r4, r0, mode
;alloc: __TMP0 str
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if300:
	mov	r1, r2
;END IF
;__TMP0="EXIT"
	movi	r2, string34
;alloc: str __TMP0
	call	findKeyword
;alloc: status str
;START IF
;status!=__ZERO
	beq	r1, r0, if304
;START IF
;execStatement!=__ZERO
	rd	r3, r0, execStatement
	beq	r3, r0, if306
;running=__ZERO
	mov	r4, r0
	wr	r4, r0, running
if306:
;END IF
;__TMP0=1
	movil	r1, 1
;alloc: __TMP0 str
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if304:
	mov	r1, r2
;END IF
;__TMP0="DELETE"
	movi	r2, string35
;alloc: str __TMP0
	call	findKeyword
;alloc: status str
;START IF
;status!=__ZERO
	beq	r1, r0, if311
;START IF
;execStatement!=__ZERO
	rd	r3, r0, execStatement
	beq	r3, r0, if313
	mov	r1, r2
;alloc: str
	call	expression
;alloc: status result str
;START IF
;status==__ZERO
	bne	r1, r0, if315
	wr	r1, fp, 1
	wr	r2, fp, 2
	mov	r1, r3
	wr	r1, fp, 0
;__TMP0="Malformed expression"
	movi	r2, string11
;alloc: str __TMP0
	call	printSyntaxErr
;alloc: __ZERO str
	mov	r1, r0
	rd	r2, fp, 0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if315:
	mov	r1, r2
	wr	r3, fp, 0
	wr	r1, fp, 2
;END IF
;alloc: result
	call	deleteLine
;alloc: status
	rd	r2, fp, 0
	rd	r3, r0, execStatement
if313:
;END IF
;alloc: status str
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if311:
	wr	r1, fp, 1
	mov	r1, r2
	wr	r1, fp, 0
;END IF
;mode=__ZERO
	mov	r3, r0
;__TMP0="Unrecognised keyword"
	movi	r2, string36
;alloc: str __TMP0
	wr	r3, r0, mode
	call	printSyntaxErr
;alloc: __ZERO str
	mov	r1, r0
	rd	r2, fp, 0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
#global exprList
exprList:
	push	ra
	push	fp
	mov	fp, sp
	addi	sp, sp, 9
	wr	r1, fp, 0
;alloc: str
;__TMP0=64
	movil	r1, 64
;alloc: __TMP0
	call	salloc
;alloc: __TMP0
;__TMP0=(char#)__TMP0
;buffer=__TMP0
	mov	r2, r1
;START WHILE
	wr	r2, fp, 2
while326:
;__ZERO==__ZERO
	bne	r0, r0, endwhile326
;__TMP0="\""
	movi	r2, string37
;alloc: str __TMP0
	rd	r1, fp, 0
	call	findKeyword
;alloc: status str
;START IF
;status!=__ZERO
	beq	r1, r0, if328
;c=str#
	rd	r3, r2, 0
;i=__ZERO
	mov	r4, r0
;START WHILE
	wr	r1, fp, 1
	wr	r2, fp, 0
	wr	r3, fp, 4
	wr	r4, fp, 5
while332:
;__TMP0="\""
	movi	r1, string37
;__TMP0=__TMP0#
	rd	r1, r1, 0
;c!=__TMP0
	rd	r2, fp, 4
	beq	r2, r1, endwhile332
;START IF
;__TMP0="\\"
	movi	r1, string38
;__TMP0=__TMP0#
	rd	r1, r1, 0
;c==__TMP0
	bne	r2, r1, if334
;str++
	rd	r1, fp, 0
	addi	r1, r1, 1
;c=str#
	rd	r2, r1, 0
;START IF
;__TMP0="n"
	movi	r3, string39
;__TMP0=__TMP0#
	rd	r3, r3, 0
;c==__TMP0
	bne	r2, r3, if336
;__TMP0=buffer+i
	rd	r3, fp, 2
	rd	r4, fp, 5
	add	r5, r3, r4
;__TMP1="\n"
	movi	r6, string2
;__TMP1=__TMP1#
	rd	r6, r6, 0
;__TMP0<-__TMP1
	wr	r6, r5, 0
	br	endif336
if336:
;START IF
;__TMP1="r"
	movi	r3, string40
;__TMP1=__TMP1#
	rd	r3, r3, 0
;c==__TMP1
	bne	r2, r3, if339
;__TMP1=buffer+i
	rd	r3, fp, 2
	rd	r4, fp, 5
	add	r5, r3, r4
;__TMP0="\r"
	movi	r6, string3
;__TMP0=__TMP0#
	rd	r6, r6, 0
;__TMP1<-__TMP0
	wr	r6, r5, 0
	br	endif339
if339:
;START IF
;__TMP0="b"
	movi	r3, string41
;__TMP0=__TMP0#
	rd	r3, r3, 0
;c==__TMP0
	bne	r2, r3, if342
	br	endif342
if342:
;START IF
;__TMP0="\""
	movi	r3, string37
;__TMP0=__TMP0#
	rd	r3, r3, 0
;c==__TMP0
	bne	r2, r3, if345
;__TMP0=buffer+i
	rd	r3, fp, 2
	rd	r4, fp, 5
	add	r5, r3, r4
;__TMP1="\""
	movi	r6, string37
;__TMP1=__TMP1#
	rd	r6, r6, 0
;__TMP0<-__TMP1
	wr	r6, r5, 0
	br	endif345
if345:
;START IF
;__TMP1="\\"
	movi	r3, string38
;__TMP1=__TMP1#
	rd	r3, r3, 0
;c==__TMP1
	bne	r2, r3, if348
;__TMP1=buffer+i
	rd	r3, fp, 2
	rd	r4, fp, 5
	add	r5, r3, r4
;__TMP0="\\"
	movi	r6, string38
;__TMP0=__TMP0#
	rd	r6, r6, 0
;__TMP1<-__TMP0
	wr	r6, r5, 0
	br	endif348
if348:
;START IF
;__TMP0="t"
	movi	r3, string42
;__TMP0=__TMP0#
	rd	r3, r3, 0
;c==__TMP0
	bne	r2, r3, if351
;__TMP0=buffer+i
	rd	r3, fp, 2
	rd	r4, fp, 5
	add	r5, r3, r4
;__TMP1="\t"
	movi	r6, string43
;__TMP1=__TMP1#
	rd	r6, r6, 0
;__TMP0<-__TMP1
	wr	r6, r5, 0
	br	endif351
if351:
	wr	r1, fp, 0
	wr	r2, fp, 4
;__TMP1="Illegal escape character"
	movi	r2, string44
;alloc: str __TMP1
	call	printSyntaxErr
;alloc: __ZERO str
	mov	r1, r0
	rd	r2, fp, 0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
endif351:
;END IF
endif348:
;END IF
endif345:
;END IF
endif342:
;END IF
endif339:
;END IF
endif336:
	wr	r2, fp, 4
	wr	r1, fp, 0
;END IF
	br	endif334
if334:
;START IF
;__TMP1="\n"
	movi	r1, string2
;__TMP1=__TMP1#
	rd	r1, r1, 0
;__TMP1=c==__TMP1
	cmp	r1, r2, r1
	not	r1
	andi	r1, r1, 1
;__TMP0=c==__ZERO
	cmp	r3, r2, r0
	not	r3
	andi	r3, r3, 1
;__TMP2="\r"
	movi	r4, string3
;__TMP2=__TMP2#
	rd	r4, r4, 0
;__TMP2=c==__TMP2
	cmp	r4, r2, r4
	not	r4
	andi	r4, r4, 1
;__TMP0=__TMP0|__TMP2
	or	r3, r3, r4
;__TMP1=__TMP1|__TMP0
	or	r1, r1, r3
	beq	r1, r0, if359
;__TMP1="Illegal character"
	movi	r2, string45
;alloc: str __TMP1
	rd	r1, fp, 0
	call	printSyntaxErr
;alloc: __ZERO str
	mov	r1, r0
	rd	r2, fp, 0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if359:
;END IF
;__TMP1=buffer+i
	rd	r1, fp, 2
	rd	r3, fp, 5
	add	r4, r1, r3
;__TMP1<-c
	wr	r2, r4, 0
endif334:
;END IF
;str++
	rd	r1, fp, 0
	addi	r1, r1, 1
	wr	r1, fp, 0
;i++
	rd	r3, fp, 5
	addi	r3, r3, 1
	wr	r3, fp, 5
;c=str#
	rd	r2, r1, 0
	wr	r2, fp, 4
	br	while332
endwhile332:
;END WHILE
;str++
	rd	r1, fp, 0
	addi	r1, r1, 1
;__TMP1=buffer+i
	rd	r3, fp, 2
	rd	r4, fp, 5
	add	r5, r3, r4
;__TMP0=(char)__ZERO
	andi	r6, r0, 255
;__TMP1<-__TMP0
	wr	r6, r5, 0
;START IF
;execStatement!=__ZERO
	rd	r5, r0, execStatement
	beq	r5, r0, if365
	wr	r1, fp, 0
	mov	r1, r3
;alloc: buffer
	call	prints
	rd	r1, fp, 0
	rd	r2, fp, 4
	rd	r3, fp, 2
	rd	r4, fp, 5
	rd	r5, r0, execStatement
if365:
	mov	r2, r1
;END IF
	rd	r1, fp, 1
	br	endif328
if328:
	mov	r1, r2
;alloc: str
	call	expression
;alloc: status result str
;START IF
;status!=__ZERO
	beq	r1, r0, if371
;START IF
;execStatement!=__ZERO
	rd	r4, r0, execStatement
	beq	r4, r0, if373
	wr	r1, fp, 1
	wr	r2, fp, 8
	wr	r3, fp, 0
;__TMP0=10
	movil	r3, 10
;alloc: buffer result __TMP0
	rd	r1, fp, 2
	call	itoa
;alloc: buffer
	rd	r1, fp, 2
	call	prints
	rd	r1, fp, 1
	rd	r2, fp, 8
	rd	r3, fp, 0
	rd	r4, r0, execStatement
if373:
;END IF
	br	endif371
if371:
	wr	r1, fp, 1
	wr	r2, fp, 8
	mov	r1, r3
	wr	r1, fp, 0
;__TMP0="Malformed expression"
	movi	r2, string11
;alloc: str __TMP0
	call	printSyntaxErr
;alloc: __ZERO str
	mov	r1, r0
	rd	r2, fp, 0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
endif371:
;END IF
	wr	r2, fp, 8
	mov	r2, r3
endif328:
	mov	r1, r2
;END IF
;__TMP0=","
	movi	r2, string23
;alloc: str __TMP0
	call	findKeyword
;alloc: status str
;START IF
;status==__ZERO
	bne	r1, r0, if381
	wr	r1, fp, 1
	wr	r2, fp, 0
;break
	rd	r2, fp, 4
	br	endwhile326
if381:
	wr	r2, fp, 0
	wr	r1, fp, 1
;END IF
	br	while326
endwhile326:
;END WHILE
;__TMP0=1
	movil	r1, 1
;alloc: __TMP0 str
	rd	r2, fp, 0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
#global varList
varList:
	push	ra
	push	fp
	mov	fp, sp
	addi	sp, sp, 5
	wr	r1, fp, 0
;alloc: str
;__TMP0=13
	movil	r1, 13
;alloc: __TMP0
	call	salloc
;alloc: __TMP0
;__TMP0=(char#)__TMP0
;str2=__TMP0
	mov	r2, r1
;status=1
	movil	r1, 1
;START WHILE
	wr	r1, fp, 2
	wr	r2, fp, 4
while390:
;status!=__ZERO
	rd	r1, fp, 2
	beq	r1, r0, endwhile390
;alloc: str
	rd	r1, fp, 0
	call	variable
;alloc: status vari str
;START IF
;status==__ZERO
	bne	r1, r0, if392
	wr	r1, fp, 2
	wr	r2, fp, 3
	mov	r1, r3
	wr	r1, fp, 0
;__TMP0="Not a variable"
	movi	r2, string8
;alloc: str __TMP0
	call	printSyntaxErr
;alloc: __ZERO str
	mov	r1, r0
	rd	r2, fp, 0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if392:
;END IF
;START IF
;execStatement!=__ZERO
	rd	r4, r0, execStatement
	beq	r4, r0, if396
	wr	r1, fp, 2
	wr	r2, fp, 3
	wr	r3, fp, 0
;__TMP0="?"
	movi	r1, string46
;alloc: __TMP0
	call	prints
;__TMP0=13
	movil	r2, 13
;alloc: str2 __TMP0
	rd	r1, fp, 4
	call	reads
;alloc: str2
	rd	r1, fp, 4
	call	atoi
;alloc: status result
;START IF
;status==__ZERO
	bne	r1, r0, if398
	wr	r1, fp, 2
;__TMP0="Not an integer"
	movi	r1, string47
;alloc: __TMP0
	call	prints
;alloc: __ZERO str
	mov	r1, r0
	rd	r2, fp, 0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if398:
;END IF
;__TMP0=vars+vari
	rd	r3, r0, vars
	rd	r4, fp, 3
	add	r5, r3, r4
;__TMP0<-result
	wr	r2, r5, 0
	mov	r2, r4
	rd	r3, fp, 0
	rd	r4, r0, execStatement
if396:
	wr	r2, fp, 3
	mov	r1, r3
;END IF
;__TMP0=","
	movi	r2, string23
;alloc: str __TMP0
	call	findKeyword
	wr	r2, fp, 0
	wr	r1, fp, 2
;alloc: status str
	br	while390
endwhile390:
;END WHILE
;__TMP0=1
	movil	r1, 1
;alloc: __TMP0 str
	rd	r2, fp, 0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
#global expression
expression:
	push	ra
	push	fp
	mov	fp, sp
	addi	sp, sp, 5
;alloc: str
;__TMP0="+"
	movi	r2, string48
;alloc: str __TMP0
	call	findKeyword
;alloc: status str
;tmp=__ZERO
	mov	r3, r0
;START IF
;status==__ZERO
	bne	r1, r0, if409
	wr	r3, fp, 4
	mov	r1, r2
;__TMP0="-"
	movi	r2, string49
;alloc: str __TMP0
	call	findKeyword
;alloc: status str
;START IF
;status!=__ZERO
	beq	r1, r0, if411
;tmp=1
	movil	r3, 1
	wr	r3, fp, 4
if411:
;END IF
	rd	r3, fp, 4
if409:
;END IF
;result=__ZERO
	mov	r4, r0
;START WHILE
	wr	r2, fp, 0
	wr	r3, fp, 4
	wr	r4, fp, 2
while415:
;__ZERO==__ZERO
	bne	r0, r0, endwhile415
;alloc: str
	rd	r1, fp, 0
	call	term
;alloc: status tmp2 str
;START IF
;status==__ZERO
	bne	r1, r0, if417
;alloc: status __ZERO str
	mov	r2, r0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if417:
;END IF
;START IF
;tmp==__ZERO
	rd	r4, fp, 4
	bne	r4, r0, if421
;result+=tmp2
	rd	r5, fp, 2
	add	r5, r5, r2
	wr	r5, fp, 2
	br	endif421
if421:
;result-=tmp2
	rd	r5, fp, 2
	sub	r5, r5, r2
	wr	r5, fp, 2
endif421:
	mov	r1, r3
;END IF
;__TMP0="+"
	movi	r2, string48
;alloc: str __TMP0
	call	findKeyword
;alloc: status str
;START IF
;status==__ZERO
	bne	r1, r0, if426
	mov	r1, r2
;__TMP0="-"
	movi	r2, string49
;alloc: str __TMP0
	call	findKeyword
;alloc: status str
;START IF
;status!=__ZERO
	beq	r1, r0, if428
;tmp=1
	movil	r3, 1
	wr	r3, fp, 4
	br	endif428
if428:
	wr	r2, fp, 0
;break
	br	endwhile415
endif428:
;END IF
	br	endif426
if426:
;tmp=__ZERO
	mov	r3, r0
	wr	r3, fp, 4
endif426:
	wr	r2, fp, 0
;END IF
	br	while415
endwhile415:
;END WHILE
;__TMP0=1
	movil	r1, 1
;alloc: __TMP0 result str
	rd	r2, fp, 2
	rd	r3, fp, 0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
#global term
term:
	push	ra
	push	fp
	mov	fp, sp
	addi	sp, sp, 5
;alloc: str
;result=1
	movil	r2, 1
;tmp=__ZERO
	mov	r3, r0
;START WHILE
	wr	r1, fp, 0
	wr	r2, fp, 2
	wr	r3, fp, 4
while443:
;__ZERO==__ZERO
	bne	r0, r0, endwhile443
;alloc: str
	rd	r1, fp, 0
	call	factor
;alloc: status tmp2 str
;START IF
;status==__ZERO
	bne	r1, r0, if445
;alloc: status __ZERO str
	mov	r2, r0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if445:
;END IF
;START IF
;tmp==__ZERO
	rd	r4, fp, 4
	bne	r4, r0, if449
;result*=tmp2
	rd	r5, fp, 2
	mul	r5, r5, r2
	wr	r5, fp, 2
	br	endif449
if449:
;result/=tmp2
	rd	r5, fp, 2
	div	r5, r5, r2
	wr	r5, fp, 2
endif449:
	mov	r1, r3
;END IF
;__TMP0="*"
	movi	r2, string50
;alloc: str __TMP0
	call	findKeyword
;alloc: status str
;START IF
;status==__ZERO
	bne	r1, r0, if454
	mov	r1, r2
;__TMP0="/"
	movi	r2, string51
;alloc: str __TMP0
	call	findKeyword
;alloc: status str
;START IF
;status!=__ZERO
	beq	r1, r0, if456
;tmp=1
	movil	r3, 1
	wr	r3, fp, 4
	br	endif456
if456:
	wr	r2, fp, 0
;break
	br	endwhile443
endif456:
;END IF
	br	endif454
if454:
;tmp=__ZERO
	mov	r3, r0
	wr	r3, fp, 4
endif454:
	wr	r2, fp, 0
;END IF
	br	while443
endwhile443:
;END WHILE
;__TMP0=1
	movil	r1, 1
;alloc: __TMP0 result str
	rd	r2, fp, 2
	rd	r3, fp, 0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
#global factor
factor:
;alloc: str
;nosave str
;alloc: str
;---BEGIN INLINE ASSEMBLER---
	push	ra
	call	variable
	beq	r1, r0, 3
	rd	r4, r0, vars
	rdr	r2, r4, r2
	br	factor_asm_end
	mov	r1, r3
	call	number
	bne	r1, r0, factor_asm_end
	rd	r4, r3, 0
	movi	r5, 40
	bne	r4, r5, factor_asm_failure
	addi	r1, r3, 1
	call	expression
	beq	r1, r0, factor_asm_failure
	rd	r4, r3, 0
	movi	r5, 41
	bne	r4, r5, factor_asm_failure
	addi	r3, r3, 1
	br	factor_asm_end
	factor_asm_failure:
	movi	r1, 0
	movi	r2, 0
	factor_asm_end:
	pop	ra
;---END INLINE ASSEMBLER---
;alloc: status result str
;nosave status
;nosave result
;nosave str
;alloc: status result str
;return
	jmpr	ra
#global variable
variable:
;alloc: str
;nosave str
;alloc: str
;---BEGIN INLINE ASSEMBLER---
	push	ra
	call	checkSpace
	rd	r4, r1, 0
	movi	r5, 65
	bl	r4, r5, variable_asm_not
	movi	r5, 90
	bl	r5, r4, variable_asm_not
	addi	r3, r1, 1
	movi	r1, 1
	subi	r2, r4, 65
	br	variable_asm_end
	variable_asm_not:
	mov	r3, r1
	movi	r1, 0
	movi	r2, 0
	variable_asm_end:
	pop	ra
;---END INLINE ASSEMBLER---
;alloc: status variable str
;nosave status
;nosave variable
;nosave str
;alloc: status variable str
;return
	jmpr	ra
#global number
number:
;alloc: str
;nosave str
;alloc: str
;---BEGIN INLINE ASSEMBLER---
	push	ra
	call	checkSpace
	push	a4
	mov	a4, r0
	call	digit
	beq	r1, r0, number_asm_end
	number_asm_loop:
	muli	a4, a4, 10
	add	a4, a4, r2
	mov	r1, r3
	call	digit
	bne	r1, r0, number_asm_loop
	movi	r1, 1
	number_asm_end:
	mov	r2, a4
	pop	a4
	pop	ra
;---END INLINE ASSEMBLER---
;alloc: status result str
;nosave status
;nosave result
;nosave str
;alloc: status result str
;return
	jmpr	ra
#global digit
digit:
;alloc: str
;nosave str
;alloc: str
;---BEGIN INLINE ASSEMBLER---
	rd	r2, r1, 0
	subi	r2, r2, 48
	bl	r2, r0, digit_asm_not
	movi	r4, 9
	bl	r4, r2, digit_asm_not
	addi	r3, r1, 1
	movi	r1, 1
	br	digit_asm_end
	digit_asm_not:
	mov	r3, r1
	movi	r1, 0
	movi	r2, 0
	digit_asm_end:
;---END INLINE ASSEMBLER---
;alloc: status digit str
;nosave status
;nosave digit
;nosave str
;alloc: status digit str
;return
	jmpr	ra
#global relop
relop:
	push	ra
	push	fp
	mov	fp, sp
	addi	sp, sp, 6
	wr	r2, fp, 1
;alloc: str recurse
;alloc: str
	call	checkSpace
;alloc: str
;string="<\0>\0=\0\0"
	movi	r2, string52
;result=1
	movil	r3, 1
;START WHILE
	wr	r1, fp, 0
	wr	r2, fp, 5
	wr	r3, fp, 3
while499:
;__TMP0=string#
	rd	r2, fp, 5
	rd	r1, r2, 0
	beq	r1, r0, endwhile499
;alloc: str string
	rd	r1, fp, 0
	call	findKeyword
;alloc: status str
;START IF
;status!=__ZERO
	beq	r1, r0, if501
;START IF
;recurse!=__ZERO
	rd	r3, fp, 1
	beq	r3, r0, if503
	mov	r1, r2
;alloc: str __ZERO
	mov	r2, r0
	call	relop
;alloc: status tmp str
;__TMP0=result<<2
	rd	r4, fp, 3
	sli	r5, r4, 2
;result=__TMP0|tmp
	or	r4, r5, r2
;result|=tmp
	or	r4, r4, r2
	mov	r2, r3
	wr	r4, fp, 3
	rd	r3, fp, 1
if503:
	mov	r3, r2
;END IF
;__TMP0=1
	movil	r1, 1
;alloc: __TMP0 result str
	rd	r2, fp, 3
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if501:
	wr	r2, fp, 0
;END IF
;string+=2
	rd	r3, fp, 5
	addi	r3, r3, 2
	wr	r3, fp, 5
;result++
	rd	r4, fp, 3
	addi	r4, r4, 1
	wr	r4, fp, 3
	br	while499
endwhile499:
;END WHILE
;alloc: __ZERO __ZERO str
	mov	r1, r0
	mov	r2, r0
	rd	r3, fp, 0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
#global checkSpace
checkSpace:
;alloc: str
;nosave str
;alloc: str
;---BEGIN INLINE ASSEMBLER---
	movi	r3, 32
	movi	r4, 9
	br checkSpace_loopCond
	checkSpace_loop:
	addi	r1, r1, 1
	checkSpace_loopCond:
	rd	r2, r1, 0
	beq	r2, r3, checkSpace_loop
	beq	r2, r4, checkSpace_loop
;---END INLINE ASSEMBLER---
;alloc: str
;nosave str
;alloc: str
;return
	jmpr	ra
#global findKeyword
findKeyword:
	push	ra
	push	fp
	mov	fp, sp
	addi	sp, sp, 2
	wr	r2, fp, 1
;alloc: str keyword
;nosave str
;alloc: str
	call	checkSpace
;alloc: str
;nosave str
;alloc: str keyword
	rd	r2, fp, 1
;---BEGIN INLINE ASSEMBLER---
	mov	r5, r1
	mov	r6, r1
	movi	r1, 1
	findKeyword_loop:
	rd	r3, r5, 0
	rd	r4, r2, 0
	beq	r4, r0, findKeyword_end
	addi	r5, r5, 1
	addi	r2, r2, 1
	beq	r3, r4, findKeyword_loop
	movi	r1, 0
	mov	r5, r6
	findKeyword_end:
	mov	r2, r5
;---END INLINE ASSEMBLER---
;alloc: status str
;nosave status
;nosave str
;alloc: status str
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
#global printSyntaxErr
printSyntaxErr:
	push	ra
	push	fp
	mov	fp, sp
	addi	sp, sp, 2
	wr	r1, fp, 0
	wr	r2, fp, 1
;alloc: str1 str2
;__TMP0="SYNTAX ERROR:\n\r"
	movi	r1, string53
;alloc: __TMP0
	call	prints
;alloc: str1
	rd	r1, fp, 0
	call	prints
;__TMP0="\n\r^\n\r"
	movi	r1, string54
;alloc: __TMP0
	call	prints
;alloc: str2
	rd	r1, fp, 1
	call	prints
;__TMP0="\n\r"
	movi	r1, string0
;alloc: __TMP0
	call	prints
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
#global prints
prints:
;alloc: string
;nosave string
;alloc: string
;---BEGIN INLINE ASSEMBLER---
	push	ra
	int	r0
	pop	ra
;---END INLINE ASSEMBLER---
;return
	jmpr	ra
#global printc
printc:
;alloc: c
;nosave c
;alloc: c
;---BEGIN INLINE ASSEMBLER---
	push	ra
	inti	1
	pop	ra
;---END INLINE ASSEMBLER---
;return
	jmpr	ra
#global readc
readc:
;---BEGIN INLINE ASSEMBLER---
	push	ra
	inti	3
	pop	ra
;---END INLINE ASSEMBLER---
;alloc: c
;alloc: c
;return
	jmpr	ra
#global reads
reads:
;alloc: stringbuf strlen
;nosave stringbuf
;nosave strlen
;alloc: stringbuf strlen
;---BEGIN INLINE ASSEMBLER---
	push	ra
	inti	2
	pop	ra
;---END INLINE ASSEMBLER---
;return
	jmpr	ra
#global edits
edits:
;alloc: stringbuf strlen
;nosave stringbuf
;nosave strlen
;alloc: stringbuf strlen
;---BEGIN INLINE ASSEMBLER---
	push	ra
	inti	8
	pop	ra
;---END INLINE ASSEMBLER---
;return
	jmpr	ra
#global scroll_screen
scroll_screen:
;---BEGIN INLINE ASSEMBLER---
	push	ra
	inti	4
	pop	ra
;---END INLINE ASSEMBLER---
;return
	jmpr	ra
#global fill
fill:
;alloc: c
;nosave c
;alloc: c
;---BEGIN INLINE ASSEMBLER---
	push	ra
	inti	5
	pop	ra
;---END INLINE ASSEMBLER---
;return
	jmpr	ra
#global setbgcol
setbgcol:
;alloc: col
;nosave col
;alloc: col
;---BEGIN INLINE ASSEMBLER---
	push	ra
	inti	6
	pop	ra
;---END INLINE ASSEMBLER---
;return
	jmpr	ra
#global setfgcol
setfgcol:
;alloc: col
;nosave col
;alloc: col
;---BEGIN INLINE ASSEMBLER---
	push	ra
	inti	7
	pop	ra
;---END INLINE ASSEMBLER---
;return
	jmpr	ra
#global startTask
startTask:
;alloc: f
;nosave f
;alloc: f
;---BEGIN INLINE ASSEMBLER---
	push	ra
	inti	131
	pop	ra
;---END INLINE ASSEMBLER---
;alloc: success
;alloc: success
;return
	jmpr	ra
#global endTask
endTask:
;---BEGIN INLINE ASSEMBLER---
	inti	130
	halt
;---END INLINE ASSEMBLER---
;return
	jmpr	ra
#global sleep
sleep:
;---BEGIN INLINE ASSEMBLER---
	inti	256
;---END INLINE ASSEMBLER---
;return
	jmpr	ra
#global mutexAquire
mutexAquire:
	push	ra
	push	fp
	mov	fp, sp
	addi	sp, sp, 2
;alloc: mutex
;alloc: mutex
	wr	r1, fp, 0
;---BEGIN INLINE ASSEMBLER---
	inti	128
;---END INLINE ASSEMBLER---
;alloc: i
;START WHILE
	wr	r1, fp, 1
while561:
;i==__ZERO
	rd	r1, fp, 1
	bne	r1, r0, endwhile561
	call	sleep
;alloc: mutex
	rd	r1, fp, 0
;---BEGIN INLINE ASSEMBLER---
	inti	128
;---END INLINE ASSEMBLER---
	wr	r1, fp, 1
;alloc: i
	br	while561
endwhile561:
;END WHILE
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
#global mutexRelease
mutexRelease:
;alloc: mutex
;nosave mutex
;alloc: mutex
;---BEGIN INLINE ASSEMBLER---
	push	ra
	inti	129
	pop	ra
;---END INLINE ASSEMBLER---
;return
	jmpr	ra
#global interruptRegister
interruptRegister:
;alloc: ID handler
;nosave ID
;nosave handler
;alloc: ID handler
;---BEGIN INLINE ASSEMBLER---
	push	ra
	inti	20
	pop	ra
;---END INLINE ASSEMBLER---
;return
	jmpr	ra
#global readTape
readTape:
;alloc: buffer len index
;nosave buffer
;nosave len
;nosave index
;alloc: buffer len index
;---BEGIN INLINE ASSEMBLER---
	push	ra
	inti	10
	pop	ra
;---END INLINE ASSEMBLER---
;alloc: index
;alloc: index
;return
	jmpr	ra
#global writeTape
writeTape:
;alloc: buffer len index
;nosave buffer
;nosave len
;nosave index
;alloc: buffer len index
;---BEGIN INLINE ASSEMBLER---
	push	ra
	inti	11
	pop	ra
;---END INLINE ASSEMBLER---
;alloc: index
;alloc: index
;return
	jmpr	ra
#global readFile
readFile:
	push	fp
	mov	fp, sp
	addi	sp, sp, 4
;alloc: buffer len index
;nosave buffer
;nosave len
;nosave index
;alloc: buffer len index status
	rd	r4, fp, 3
;---BEGIN INLINE ASSEMBLER---
	push	ra
	inti	12
	pop	ra
;---END INLINE ASSEMBLER---
;alloc: status index
;alloc: status index
;return
	mov	sp, fp
	pop	fp
	jmpr	ra
#global writeFile
writeFile:
	push	fp
	mov	fp, sp
	addi	sp, sp, 4
;alloc: buffer len index
;nosave buffer
;nosave len
;nosave index
;alloc: buffer len index status
	rd	r4, fp, 3
;---BEGIN INLINE ASSEMBLER---
	push	ra
	inti	13
	pop	ra
;---END INLINE ASSEMBLER---
;alloc: status index
;alloc: status index
;return
	mov	sp, fp
	pop	fp
	jmpr	ra
#global openFile
openFile:
;alloc: fileName
;nosave fileName
;alloc: fileName
;---BEGIN INLINE ASSEMBLER---
	push	ra
	inti	14
	pop	ra
;---END INLINE ASSEMBLER---
;alloc: index
;alloc: index
;return
	jmpr	ra
#global makeFile
makeFile:
	push	ra
;alloc: fileName
;__TMP0=1
	movil	r2, 1
;__TMP1=1
	movil	r3, 1
;alloc: fileName __TMP0 __TMP1
	call	makeFile_advanced
;alloc: __TMP1
;alloc: __TMP1
;return
	pop	ra
	jmpr	ra
#global makeFile_advanced
makeFile_advanced:
;alloc: fileName type length
;nosave fileName
;nosave length
;nosave type
;alloc: fileName type length
;---BEGIN INLINE ASSEMBLER---
	push	ra
	inti	15
	pop	ra
;---END INLINE ASSEMBLER---
;alloc: index
;alloc: index
;return
	jmpr	ra
#global salloc
salloc:
;alloc: len
;nosave len
;alloc: len
;---BEGIN INLINE ASSEMBLER---
	add	sp, sp, r1
	sub	r1, sp, r1
;---END INLINE ASSEMBLER---
;alloc: ret
;nosave ret
;alloc: ret
;return
	jmpr	ra
#global strlen
strlen:
;alloc: str
;len=__ZERO
	mov	r2, r0
;START WHILE
while613:
;__TMP0=str#
	rd	r3, r1, 0
	beq	r3, r0, endwhile613
;len++
	addi	r2, r2, 1
;str++
	addi	r1, r1, 1
	br	while613
endwhile613:
	mov	r1, r2
;END WHILE
;alloc: len
;return
	jmpr	ra
#global lowbit
lowbit:
;alloc: i
;tmp=i-1
	subi	r2, r1, 1
;tmp&=i
	and	r2, r2, r1
;__TMP0=tmp^i
	xor	r1, r2, r1
;alloc: __TMP0
;return
	jmpr	ra
#global bitnum
bitnum:
	push	fp
	mov	fp, sp
	addi	sp, sp, 1
;alloc: i
	wr	r1, fp, 0
;---BEGIN INLINE ASSEMBLER---
	.data
	BITNUM_DATASET:
	0  1  28 2  29 14 24 3 30 22 20 15 25 17 4  8
	31 27 13 23 21 19 16 7 26 12 18 6  11 5  10 9
	.text
	movi	r1, BITNUM_DATASET
;---END INLINE ASSEMBLER---
;alloc: datset
;__TMP0=125613361
;__TMP0=125613361
	movhi	r2, 1916
	ori	r2, r2, 46385
;__TMP0=i*__TMP0
	rd	r3, fp, 0
	mul	r2, r3, r2
;__TMP0=__TMP0>>27
	sri	r2, r2, 27
;datset+=__TMP0
	add	r1, r1, r2
;__TMP0=datset#
	rd	r1, r1, 0
;alloc: __TMP0
;return
	mov	sp, fp
	pop	fp
	jmpr	ra
#global char_pack
char_pack:
;alloc: c1 c2 c3 c4
;ret=c4<<24
	sli	r5, r4, 24
;__TMP0=c3<<16
	sli	r6, r3, 16
;ret|=__TMP0
	or	r5, r5, r6
;__TMP0=c2<<8
	sli	r6, r2, 8
;ret|=__TMP0
	or	r5, r5, r6
;ret|=c1
	or	r1, r5, r1
;alloc: ret
;return
	jmpr	ra
#global arrset
arrset:
;alloc: arr len val
;START WHILE
while627:
;len--
	subi	r2, r2, 1
;len>=__ZERO
	bl	r2, r0, endwhile627
;arr<-val
	wr	r3, r1, 0
;arr++
	addi	r1, r1, 1
	br	while627
endwhile627:
;END WHILE
;return
	jmpr	ra
#global strcmp
strcmp:
;alloc: str1 str2 maxlen
;i=__ZERO
	mov	r4, r0
;START WHILE
while633:
;i<maxlen
	ble	r3, r4, endwhile633
;__TMP0=str1+i
;c1=(str1+i)*
	rdr	r5, r1, r4
;__TMP0=str2+i
;c2=(str2+i)*
	rdr	r6, r2, r4
;START IF
;c1!=c2
	beq	r5, r6, if637
;alloc: __ZERO
	mov	r1, r0
;return
	jmpr	ra
if637:
;START IF
;c1==__ZERO
	bne	r5, r0, if641
;__TMP0=1
	movil	r1, 1
;alloc: __TMP0
;return
	jmpr	ra
if641:
;END IF
endif637:
;END IF
;i++
	addi	r4, r4, 1
;nosave c1
;nosave c2
	br	while633
endwhile633:
;END WHILE
;__TMP0=1
	movil	r1, 1
;alloc: __TMP0
;return
	jmpr	ra
#global itoa
itoa:
;alloc: outStr x base
;i=__ZERO
	mov	r4, r0
;START WHILE
while651:
;__TMP0=i==__ZERO
	cmp	r5, r4, r0
	not	r5
	andi	r5, r5, 1
;__TMP0=x|__TMP0
	or	r5, r2, r5
	beq	r5, r0, endwhile651
;tmp=x%base
	div	r0, r2, r3
	mov	r5, ex
;x/=base
	div	r2, r2, r3
;__TMP0="0123456789abcdef"
	movi	r6, string55
;__TMP0=__TMP0+tmp
;__TMP0=(__TMP0+tmp)*
	rdr	r6, r6, r5
;outStr<-__TMP0
	wr	r6, r1, 0
;i++
	addi	r4, r4, 1
;outStr++
	addi	r1, r1, 1
	br	while651
endwhile651:
;END WHILE
;outStr<-__ZERO
	wr	r0, r1, 0
;outStr--
	subi	r1, r1, 1
;i--
	subi	r4, r4, 1
;START WHILE
while655:
;i>__ZERO
	ble	r4, r0, endwhile655
;tmpchar=outStr#
	rd	r5, r1, 0
;__TMP0=outStr-i
	sub	r6, r1, r4
;__TMP0=__TMP0#
	rd	r6, r6, 0
;outStr<-__TMP0
	wr	r6, r1, 0
;__TMP0=outStr-i
	sub	r6, r1, r4
;__TMP0<-tmpchar
	wr	r5, r6, 0
;i-=2
	subi	r4, r4, 2
;outStr--
	subi	r1, r1, 1
	br	while655
endwhile655:
;END WHILE
;return
	jmpr	ra
#global atoi
atoi:
;alloc: str
;result=__ZERO
	mov	r2, r0
;c=str#
	rd	r3, r1, 0
;START WHILE
while663:
	beq	r3, r0, endwhile663
;START IF
;__TMP0="0"
	movi	r4, string56
;__TMP0=__TMP0#
	rd	r4, r4, 0
;__TMP0=c<__TMP0
	cmp	r4, r3, r4
	addi	r4, r4, 1
	cmp	r4, r4, r0
	xori	r4, r4, 1
;__TMP1="9"
	movi	r5, string57
;__TMP1=c>__TMP1
	cmp	r5, r5, r3
	addi	r5, r5, 1
	cmp	r5, r5, r0
	xori	r5, r5, 1
;__TMP0=__TMP0|__TMP1
	or	r4, r4, r5
	beq	r4, r0, if665
;alloc: __ZERO __ZERO
	mov	r1, r0
	mov	r2, r0
;return
	jmpr	ra
if665:
;END IF
;__TMP0=result*10
	muli	r4, r2, 10
;__TMP1="0"
	movi	r5, string56
;__TMP1=__TMP1#
	rd	r5, r5, 0
;__TMP1=c-__TMP1
	sub	r5, r3, r5
	andi	r5, r5, 255
;result=__TMP0+__TMP1
	add	r2, r4, r5
;str++
	addi	r1, r1, 1
;c=str#
	rd	r3, r1, 0
	br	while663
endwhile663:
;END WHILE
;__TMP1=1
	movil	r1, 1
;alloc: __TMP1 result
;return
	jmpr	ra
.data
lineNrs:
0
lineStr:
0
freeStrs:
0
freeStrNum:
0
activeLines:
0
maxLines:
512
mode:
0
execStatement:
0
execLine:
0
running:
1
vars:
0
varTypes:
0
stack:
0
stackPointer:
0
string0:
#string "\n\r"
string1:
#string "Ready\n\r"
string2:
#string "\n"
string3:
#string "\r"
string4:
#string "Line does not exist"
string5:
#string "MISSING LINE BREAK\n\r"
string6:
#string "PROGRAM LINE OVERFLOW\n\r"
string7:
#string "LET"
string8:
#string "Not a variable"
string9:
#string "="
string10:
#string "Expected '='"
string11:
#string "Malformed expression"
string12:
#string "PRINT"
string13:
#string "IF"
string14:
#string "Malformed comparator"
string15:
#string "Not a comparator"
string16:
#string "GOTO"
string17:
#string "INPUT"
string18:
#string "GOSUB"
string19:
#string "Stack overflow"
string20:
#string "RETURN"
string21:
#string "No return address"
string22:
#string "PEEK"
string23:
#string ","
string24:
#string "Expected ','"
string25:
#string "POKE"
string26:
#string "Ecpected ','"
string27:
#string "FCOL"
string28:
#string "BCOL"
string29:
#string "LIST"
string30:
#string "CLEAR"
string31:
#string "RUN"
string32:
#string "EDIT"
string33:
#string "END"
string34:
#string "EXIT"
string35:
#string "DELETE"
string36:
#string "Unrecognised keyword"
string37:
#string "\""
string38:
#string "\\"
string39:
#string "n"
string40:
#string "r"
string41:
#string "b"
string42:
#string "t"
string43:
#string "\t"
string44:
#string "Illegal escape character"
string45:
#string "Illegal character"
string46:
#string "?"
string47:
#string "Not an integer"
string48:
#string "+"
string49:
#string "-"
string50:
#string "*"
string51:
#string "/"
string52:
#string "<\0>\0=\0\0"
string53:
#string "SYNTAX ERROR:\n\r"
string54:
#string "\n\r^\n\r"
string55:
#string "0123456789abcdef"
string56:
#string "0"
string57:
#string "9"
