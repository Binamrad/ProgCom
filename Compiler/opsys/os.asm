.text
#global osmain
osmain:
	push	ra
	push	fp
	mov	fp, sp
	addi	sp, sp, 7
;__TMP0=128
	movil	r1, 128
;__TMP1=os_mutex_aq@
	movi	r2, os_mutex_aq
;__TMP1=(func0)__TMP1
;alloc: __TMP0 __TMP1
	call	interruptRegister
;__TMP1=129
	movil	r1, 129
;__TMP0=os_mutex_drop@
	movi	r2, os_mutex_drop
;__TMP0=(func0)__TMP0
;alloc: __TMP1 __TMP0
	call	interruptRegister
;__TMP0=10
	movil	r1, 10
;__TMP1=os_tape_read@
	movi	r2, os_tape_read
;__TMP1=(func0)__TMP1
;alloc: __TMP0 __TMP1
	call	interruptRegister
;__TMP1=11
	movil	r1, 11
;__TMP0=os_tape_write@
	movi	r2, os_tape_write
;__TMP0=(func0)__TMP0
;alloc: __TMP1 __TMP0
	call	interruptRegister
;__TMP0=12
	movil	r1, 12
;__TMP1=os_tape_readfile@
	movi	r2, os_tape_readfile
;__TMP1=(func0)__TMP1
;alloc: __TMP0 __TMP1
	call	interruptRegister
;__TMP1=13
	movil	r1, 13
;__TMP0=os_tape_writefile@
	movi	r2, os_tape_writefile
;__TMP0=(func0)__TMP0
;alloc: __TMP1 __TMP0
	call	interruptRegister
;__TMP0=14
	movil	r1, 14
;__TMP1=openfile@
	movi	r2, openfile
;__TMP1=(func0)__TMP1
;alloc: __TMP0 __TMP1
	call	interruptRegister
;__TMP1=15
	movil	r1, 15
;__TMP0=os_create_file@
	movi	r2, os_create_file
;__TMP0=(func0)__TMP0
;alloc: __TMP1 __TMP0
	call	interruptRegister
;alloc: __ZERO
	mov	r1, r0
	call	setbgcol
;__TMP0=1
	movil	r1, 1
;alloc: __TMP0
	call	setfgcol
;__TMP0="PC-OS version 0.1\n\r"
	movi	r1, string0
;alloc: __TMP0
	call	prints
;__TMP0=32
	movil	r1, 32
;alloc: __TMP0
	call	salloc
;alloc: __TMP0
;__TMP0=(char#)__TMP0
;stringbuffer=__TMP0
	mov	r2, r1
;strpointer=__ZERO
	mov	r1, r0
;running=1
	movil	r3, 1
;START WHILE
	wr	r2, fp, 2
	wr	r3, fp, 4
while4:
	rd	r1, fp, 4
	beq	r1, r0, endwhile4
;program=__ZERO
	mov	r2, r0
	wr	r2, fp, 5
;__TMP0=(int#)stringbuffer
	rd	r1, fp, 2
;__TMP1=32
	movil	r2, 32
;alloc: __TMP0 __TMP1 __ZERO
	mov	r3, r0
	call	arrset
;__TMP1="Ready\n\r"
	movi	r1, string1
;alloc: __TMP1
	call	prints
;__TMP1=32
	movil	r2, 32
;alloc: stringbuffer __TMP1
	rd	r1, fp, 2
	call	reads
;START IF
;__TMP1="halt"
	movi	r1, string2
;__TMP0=32
	movil	r2, 32
	wr	r2, fp, 0
;alloc: stringbuffer __TMP1 __TMP0
	mov	r2, r1
	rd	r1, fp, 2
	rd	r3, fp, 0
	call	strcmp
;alloc: __TMP0
	beq	r1, r0, if7
;running=__ZERO
	mov	r1, r0
	wr	r1, fp, 4
;__TMP0="goodbye"
	movi	r1, string3
;alloc: __TMP0
	call	prints
	br	endif7
if7:
;START IF
;__TMP0="clear"
	movi	r1, string4
;__TMP1=32
	movil	r2, 32
	wr	r2, fp, 1
;alloc: stringbuffer __TMP0 __TMP1
	mov	r2, r1
	rd	r1, fp, 2
	rd	r3, fp, 1
	call	strcmp
;alloc: __TMP1
	beq	r1, r0, if10
;i=__ZERO
	mov	r1, r0
;START WHILE
	wr	r1, fp, 6
while13:
;__TMP1=32
	movil	r1, 32
;i<__TMP1
	rd	r2, fp, 6
	ble	r1, r2, endwhile13
	call	scroll_screen
;i++
	rd	r1, fp, 6
	addi	r1, r1, 1
	wr	r1, fp, 6
	br	while13
endwhile13:
;END WHILE
	br	endif10
if10:
;START IF
;__TMP1="reboot"
	movi	r1, string5
;__TMP0=32
	movil	r2, 32
	wr	r2, fp, 0
;alloc: stringbuffer __TMP1 __TMP0
	mov	r2, r1
	rd	r1, fp, 2
	rd	r3, fp, 0
	call	strcmp
;alloc: __TMP0
	beq	r1, r0, if17
;__TMP0="rebooting..."
	movi	r1, string6
;alloc: __TMP0
	call	prints
	call	reboot
	br	endif17
if17:
;START IF
;__TMP0="format"
	movi	r1, string7
;__TMP1=32
	movil	r2, 32
	wr	r2, fp, 1
;alloc: stringbuffer __TMP0 __TMP1
	mov	r2, r1
	rd	r1, fp, 2
	rd	r3, fp, 1
	call	strcmp
;alloc: __TMP1
	beq	r1, r0, if20
	call	format
	br	endif20
if20:
;START IF
;__TMP1="list"
	movi	r1, string8
;__TMP0=32
	movil	r2, 32
	wr	r2, fp, 0
;alloc: stringbuffer __TMP1 __TMP0
	mov	r2, r1
	rd	r1, fp, 2
	rd	r3, fp, 0
	call	strcmp
;alloc: __TMP0
	beq	r1, r0, if23
	call	list
	br	endif23
if23:
;START IF
;__TMP0="copy"
	movi	r1, string9
;__TMP1=32
	movil	r2, 32
	wr	r2, fp, 1
;alloc: stringbuffer __TMP0 __TMP1
	mov	r2, r1
	rd	r1, fp, 2
	rd	r3, fp, 1
	call	strcmp
;alloc: __TMP1
	beq	r1, r0, if26
	call	fileCopy
	br	endif26
if26:
;alloc: stringbuffer
	rd	r1, fp, 2
	call	load_program
;alloc: program
;START IF
;program!=__ZERO
	beq	r1, r0, if30
	wr	r1, fp, 5
;---BEGIN INLINE ASSEMBLER---
	.data
	__OSTMPSPSTORE:
	0
	.text
	wr	sp, r0, __OSTMPSPSTORE
;---END INLINE ASSEMBLER---
;alloc: stringbuffer
	rd	r1, fp, 2
	rd	r1, fp, 5
	callr	r1
;alloc: __TMP1
;---BEGIN INLINE ASSEMBLER---
	rd	sp, r0, __OSTMPSPSTORE
;---END INLINE ASSEMBLER---
	rd	r1, fp, 5
if30:
;END IF
	wr	r1, fp, 5
endif26:
;END IF
endif23:
;END IF
endif20:
;END IF
endif17:
;END IF
endif10:
;END IF
endif7:
;END IF
	br	while4
endwhile4:
;END WHILE
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
#global load_program
load_program:
	push	ra
	push	fp
	mov	fp, sp
	addi	sp, sp, 8
	wr	r1, fp, 0
;alloc: input
;__TMP0=12
	movil	r1, 12
;alloc: __TMP0
	call	salloc
	mov	r1, r1
	wr	r1, fp, 1
;alloc: __TMP0
;__TMP0=(char#)__TMP0
;programName=__TMP0
;__TMP0=(int#)programName
;__TMP1=12
	movil	r2, 12
;alloc: __TMP0 __TMP1 __ZERO
	mov	r3, r0
	call	arrset
;counter=__ZERO
	mov	r1, r0
;START WHILE
	wr	r1, fp, 4
while39:
;__TMP1=input#
	rd	r2, fp, 0
	rd	r1, r2, 0
;__TMP0=" "
	movi	r3, string10
;__TMP0=__TMP0#
	rd	r3, r3, 0
;__TMP1=__TMP1!=__TMP0
	cmp	r1, r1, r3
	andi	r1, r1, 1
;__TMP0=input#
	rd	r3, r2, 0
;__TMP2=(char)__ZERO
	andi	r4, r0, 255
;__TMP0=__TMP0!=__TMP2
	cmp	r3, r3, r4
	andi	r3, r3, 1
;__TMP1=__TMP1&__TMP0
	and	r1, r1, r3
	beq	r1, r0, endwhile39
;START IF
;__TMP1=12
	movil	r1, 12
;counter==__TMP1
	rd	r3, fp, 4
	bne	r3, r1, if41
;__TMP1="Program name overflow\n\r"
	movi	r1, string11
;alloc: __TMP1
	call	prints
;__TMP1=(func1)__ZERO
	mov	r1, r0
;alloc: __TMP1
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if41:
;END IF
;__TMP1=programName+counter
	rd	r1, fp, 1
	add	r4, r1, r3
;__TMP0=input#
	rd	r5, r2, 0
;__TMP1<-__TMP0
	wr	r5, r4, 0
;input++
	addi	r2, r2, 1
	wr	r2, fp, 0
;counter++
	addi	r3, r3, 1
	wr	r3, fp, 4
	br	while39
endwhile39:
;END WHILE
;START IF
;counter==__ZERO
	rd	r1, fp, 4
	bne	r1, r0, if46
;__TMP0=(func1)__ZERO
	mov	r1, r0
;alloc: __TMP0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if46:
;END IF
;alloc: programName
	rd	r1, fp, 1
	call	openfile
;alloc: tapePos
;START IF
;tapePos==__ZERO
	bne	r1, r0, if51
	wr	r1, fp, 6
;__TMP0="No such program:\n\r"
	movi	r1, string12
;alloc: __TMP0
	call	prints
;alloc: programName
	rd	r1, fp, 1
	call	prints
;__TMP0="\n\r"
	movi	r1, string13
;alloc: __TMP0
	call	prints
;__TMP0=(func1)__ZERO
	mov	r1, r0
;alloc: __TMP0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if51:
	wr	r1, fp, 6
;END IF
;__TMP0=2
	movil	r1, 2
;alloc: __TMP0
	call	salloc
	wr	r1, fp, 7
;alloc: farea
;__TMP0=2
	movil	r2, 2
;alloc: farea __TMP0 tapePos
	rd	r3, fp, 6
	call	os_tape_readfile
;alloc: status tapePos
;__TMP0=farea+1
;progLen=(farea+1)*
	rd	r4, fp, 7
	rd	r3, r4, 1
;__TMP0=farea#
	rd	r4, r4, 0
;__TMP0=(int#)__TMP0
;farea=__TMP0
;START IF
;__TMP0=6000
	movil	r5, 6000
;farea<__TMP0
	ble	r5, r4, if58
	wr	r2, fp, 6
	wr	r4, fp, 7
;__TMP0="Cannot open program:\n\rstart index too low\n\r"
	movi	r1, string14
;alloc: __TMP0
	call	prints
;__TMP0=(func1)__ZERO
	mov	r1, r0
;alloc: __TMP0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if58:
	wr	r2, fp, 6
	mov	r2, r3
	mov	r1, r4
	wr	r1, fp, 7
;END IF
;alloc: farea progLen tapePos
	rd	r3, fp, 6
	call	os_tape_readfile
;alloc: status tapePos
;__TMP0=(func1)farea
	rd	r1, fp, 7
;alloc: __TMP0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
#global format
format:
	push	ra
	push	fp
	mov	fp, sp
	addi	sp, sp, 10
;__TMP0=tmp@
	addi	r1, fp, 0
;__TMP1=1
	movil	r2, 1
;__TMP2=7
	movil	r3, 7
;alloc: __TMP0 __TMP1 __TMP2
	call	os_tape_read
;alloc: __TMP2
;START IF
	rd	r1, fp, 0
	beq	r1, r0, if65
;__TMP2="Tape already formatted\n\r"
	movi	r1, string15
;alloc: __TMP2
	call	prints
;__TMP2="Continue?y/n\n\r"
	movi	r1, string16
;alloc: __TMP2
	call	prints
	call	readc
;alloc: c
;START IF
;__TMP2="y"
	movi	r2, string17
;__TMP2=__TMP2#
	rd	r2, r2, 0
;c!=__TMP2
	beq	r1, r2, if68
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if68:
;END IF
	rd	r1, fp, 0
if65:
;END IF
;__TMP2="Formatting...\n\r"
	movi	r1, string18
;alloc: __TMP2
	call	prints
;tmp=8
	movil	r1, 8
	wr	r1, fp, 0
;__TMP2=tmp@
	addi	r1, fp, 0
;__TMP1=1
	movil	r2, 1
;__TMP0=7
	movil	r3, 7
;alloc: __TMP2 __TMP1 __TMP0
	call	os_tape_write
;alloc: __TMP0
;tmp=__ZERO
	mov	r1, r0
;i=8
	movil	r2, 8
;START WHILE
	wr	r1, fp, 0
	wr	r2, fp, 5
while74:
;__TMP0=1024
	movil	r1, 1024
;i<__TMP0
	rd	r2, fp, 5
	ble	r1, r2, endwhile74
;__TMP0=tmp@
	addi	r1, fp, 0
;__TMP1=1
	movil	r2, 1
;alloc: __TMP0 __TMP1 i
	rd	r3, fp, 5
	call	os_tape_write
;alloc: __TMP1
;i++
	rd	r1, fp, 5
	addi	r1, r1, 1
	wr	r1, fp, 5
	br	while74
endwhile74:
;END WHILE
;tmp=-1
	subi	r1, r0, 1
	wr	r1, fp, 0
;__TMP1=tmp@
	addi	r1, fp, 0
;__TMP0=1
	movil	r2, 1
;__TMP2=512
	movil	r3, 512
;alloc: __TMP1 __TMP0 __TMP2
	call	os_tape_write
;alloc: __TMP2
;__TMP0=tmp@
	addi	r1, fp, 0
;__TMP1=1
	movil	r2, 1
;__TMP3=513
	movil	r3, 513
;alloc: __TMP0 __TMP1 __TMP3
	call	os_tape_write
;alloc: __TMP3
;__TMP1=tmp@
	addi	r1, fp, 0
;__TMP0=1
	movil	r2, 1
;__TMP4=3
	movil	r3, 3
;alloc: __TMP1 __TMP0 __TMP4
	call	os_tape_read
;alloc: __TMP4
;START IF
	rd	r1, fp, 0
	beq	r1, r0, if77
;__TMP2="Bootable program present\n\r"
	movi	r1, string19
;alloc: __TMP2
	call	prints
;__TMP2="Do you want to keep it?y/n\n\r"
	movi	r1, string20
;alloc: __TMP2
	call	prints
	call	readc
;alloc: c
;START IF
;__TMP2="n"
	movi	r2, string21
;__TMP2=__TMP2#
	rd	r2, r2, 0
;c==__TMP2
	bne	r1, r2, if79
;tmp=__ZERO
	mov	r2, r0
	wr	r2, fp, 0
;__TMP2=tmp@
	addi	r1, fp, 0
;__TMP3=1
	movil	r2, 1
;__TMP4=3
	movil	r3, 3
;alloc: __TMP2 __TMP3 __TMP4
	call	os_tape_write
;alloc: __TMP4
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if79:
;END IF
;__TMP4="Creating boot file...\n\r"
	movi	r1, string22
;alloc: __TMP4
	call	prints
;location=tmp
	rd	r2, fp, 0
	wr	r2, fp, 8
;__TMP4=len@
	addi	r1, fp, 9
;__TMP3=1
	movil	r4, 1
;__TMP2=location+1
	addi	r3, r2, 1
	mov	r2, r4
;alloc: __TMP4 __TMP3 __TMP2
	call	os_tape_read
;alloc: __TMP2
;__TMP2=location@
	addi	r1, fp, 8
;__TMP3=1
	movil	r2, 1
;__TMP4=11
	movil	r3, 11
;alloc: __TMP2 __TMP3 __TMP4
	call	os_tape_write
;alloc: __TMP4
;location/=512
	rd	r1, fp, 8
	divi	r1, r1, 512
;location+=512
	addi	r1, r1, 512
;__TMP4=len+2
	rd	r2, fp, 9
	addi	r3, r2, 2
;len=__TMP4/512
	divi	r2, r3, 512
;len++
	addi	r2, r2, 1
;tmp=-1
	subi	r3, r0, 1
;START WHILE
	wr	r1, fp, 8
	wr	r2, fp, 9
	wr	r3, fp, 0
while85:
	rd	r1, fp, 9
	beq	r1, r0, endwhile85
;__TMP4=tmp@
	addi	r1, fp, 0
;__TMP3=1
	movil	r2, 1
;alloc: __TMP4 __TMP3 location
	rd	r3, fp, 8
	call	os_tape_write
;alloc: __TMP3
;len--
	rd	r1, fp, 9
	subi	r1, r1, 1
	wr	r1, fp, 9
;location++
	rd	r2, fp, 8
	addi	r2, r2, 1
	wr	r2, fp, 8
	br	while85
endwhile85:
;END WHILE
;__TMP3="b"
	movi	r2, string23
;__TMP3=__TMP3#
	rd	r1, r2, 0
;__TMP4="o"
	movi	r3, string24
;__TMP4=__TMP4#
	rd	r2, r3, 0
;__TMP2="o"
	movi	r4, string24
;__TMP2=__TMP2#
	rd	r3, r4, 0
;__TMP0="t"
	movi	r5, string25
;__TMP0=__TMP0#
	rd	r4, r5, 0
;alloc: __TMP3 __TMP4 __TMP2 __TMP0
	call	char_pack
	wr	r1, fp, 0
;alloc: tmp
;__TMP0=tmp@
	addi	r1, fp, 0
;__TMP2=1
	movil	r2, 1
;__TMP4=8
	movil	r3, 8
;alloc: __TMP0 __TMP2 __TMP4
	call	os_tape_write
;alloc: __TMP4
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if77:
;END IF
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
#global list
list:
	push	ra
	push	fp
	mov	fp, sp
	addi	sp, sp, 7
;__TMP0=13
	movil	r1, 13
;alloc: __TMP0
	call	salloc
	mov	r1, r1
	wr	r1, fp, 0
;alloc: __TMP0
;__TMP0=(char#)__TMP0
;filename=__TMP0
;__TMP0=(int#)filename
;__TMP1=13
	movil	r2, 13
;alloc: __TMP0 __TMP1 __ZERO
	mov	r3, r0
	call	arrset
;__TMP1=4
	movil	r1, 4
;alloc: __TMP1
	call	salloc
;alloc: fsname
;index=8
	movil	r2, 8
;lineCounter=__ZERO
	mov	r3, r0
;START WHILE
	wr	r1, fp, 3
	wr	r2, fp, 4
	wr	r3, fp, 5
while96:
;__TMP1=512
	movil	r1, 512
;index<__TMP1
	rd	r2, fp, 4
	ble	r1, r2, endwhile96
;__TMP1=4
	movil	r2, 4
;alloc: fsname __TMP1 index
	rd	r1, fp, 3
	rd	r3, fp, 4
	call	os_tape_read
;alloc: index
;START IF
;__TMP1=fsname#
	rd	r3, fp, 3
	rd	r2, r3, 0
;__TMP1=(char)__TMP1
	andi	r2, r2, 255
	beq	r2, r0, if98
;i=__ZERO
	mov	r2, r0
;START WHILE
	wr	r1, fp, 4
	wr	r2, fp, 6
while101:
;__TMP1=3
	movil	r1, 3
;i<__TMP1
	rd	r2, fp, 6
	ble	r1, r2, endwhile101
;__TMP1=fsname+i
;tmp=(fsname+i)*
	rd	r3, fp, 3
	rdr	r1, r3, r2
;__TMP1=i<<2
	sli	r4, r2, 2
;__TMP1=filename+__TMP1
	rd	r5, fp, 0
	add	r4, r5, r4
;__TMP0=(char)tmp
	andi	r6, r1, 255
;__TMP1<-__TMP0
	wr	r6, r4, 0
;__TMP0=i<<2
	sli	r4, r2, 2
;__TMP0=filename+__TMP0
	add	r4, r5, r4
;__TMP0=__TMP0+1
	addi	r4, r4, 1
;__TMP1=tmp>>8
	sri	r6, r1, 8
;__TMP1=(char)__TMP1
	andi	r6, r6, 255
;__TMP0<-__TMP1
	wr	r6, r4, 0
;__TMP1=i<<2
	sli	r4, r2, 2
;__TMP1=filename+__TMP1
	add	r4, r5, r4
;__TMP1=__TMP1+2
	addi	r4, r4, 2
;__TMP0=tmp>>16
	sri	r6, r1, 16
;__TMP0=(char)__TMP0
	andi	r6, r6, 255
;__TMP1<-__TMP0
	wr	r6, r4, 0
;__TMP0=i<<2
	sli	r4, r2, 2
;__TMP0=filename+__TMP0
	add	r4, r5, r4
;__TMP0=__TMP0+3
	addi	r4, r4, 3
;__TMP1=tmp>>24
	sri	r6, r1, 24
;__TMP1=(char)__TMP1
	andi	r6, r6, 255
;__TMP0<-__TMP1
	wr	r6, r4, 0
;i++
	addi	r2, r2, 1
	wr	r2, fp, 6
	br	while101
endwhile101:
;END WHILE
;alloc: filename
	rd	r1, fp, 0
	call	prints
;__TMP1="\n\r"
	movi	r1, string13
;alloc: __TMP1
	call	prints
;lineCounter++
	rd	r1, fp, 5
	addi	r1, r1, 1
;START IF
;__TMP1=30
	movil	r2, 30
;lineCounter==__TMP1
	bne	r1, r2, if105
	wr	r1, fp, 5
;__TMP1="Continue?y/n\n\r"
	movi	r1, string16
;alloc: __TMP1
	call	prints
	call	readc
;alloc: c
;START IF
;__TMP1="n"
	movi	r2, string21
;__TMP1=__TMP1#
	rd	r2, r2, 0
;c==__TMP1
	bne	r1, r2, if108
;break
	rd	r2, fp, 6
	br	endwhile96
if108:
;END IF
;lineCounter=__ZERO
	mov	r2, r0
	mov	r1, r2
if105:
;END IF
	wr	r1, fp, 5
	rd	r1, fp, 4
	rd	r3, fp, 3
if98:
	wr	r1, fp, 4
;END IF
	br	while96
endwhile96:
;END WHILE
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
#global fileCopy
fileCopy:
	push	ra
	push	fp
	mov	fp, sp
	addi	sp, sp, 7
;__TMP0=13
	movil	r1, 13
;alloc: __TMP0
	call	salloc
	mov	r2, r1
	wr	r2, fp, 0
;alloc: __TMP0
;__TMP0=(char#)__TMP0
;fname=__TMP0
;__TMP0=20
	movil	r1, 20
;alloc: __TMP0
	call	salloc
;alloc: __TMP0
;datlen=__ZERO
	mov	r1, r0
;__TMP0="Enter filename:\n\r"
	movi	r1, string26
;alloc: __TMP0
	call	prints
;__TMP0=13
	movil	r2, 13
;alloc: fname __TMP0
	rd	r1, fp, 0
	call	reads
;alloc: fname
	rd	r1, fp, 0
	call	openFile
;alloc: index
;START IF
;index==__ZERO
	bne	r1, r0, if120
	wr	r1, fp, 1
;__TMP0="File does not exist\n\r"
	movi	r1, string27
;alloc: __TMP0
	call	prints
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if120:
	wr	r1, fp, 1
;END IF
;status=__ZERO
	mov	r2, r0
	wr	r2, fp, 4
;fileLen=__ZERO
	mov	r3, r0
	wr	r3, fp, 5
;alloc: __ZERO
	mov	r1, r0
	call	salloc
;alloc: farea
;START IF
;__TMP0=index>>24
	rd	r2, fp, 1
	sri	r3, r2, 24
;__TMP0=__TMP0&1
	andi	r3, r3, 1
;__TMP0==__ZERO
	bne	r3, r0, if127
	mov	r3, r2
	wr	r1, fp, 6
;__TMP0=fileLen@
	addi	r1, fp, 5
;__TMP1=1
	movil	r2, 1
;alloc: __TMP0 __TMP1 index
	call	readFile
	mov	r3, r2
;alloc: status index
;__TMP1=fileLen@
	addi	r1, fp, 5
;__TMP0=1
	movil	r2, 1
;alloc: __TMP1 __TMP0 index
	call	readFile
;alloc: status index
;__TMP0=fileLen+2
	rd	r3, fp, 5
	wr	r3, fp, 5
	addi	r4, r3, 2
;__TMP1=index-2
	subi	r3, r2, 2
	mov	r2, r4
;alloc: farea __TMP0 __TMP1
	rd	r1, fp, 6
	call	readFile
	wr	r1, fp, 4
	wr	r2, fp, 1
;alloc: status index
	rd	r1, fp, 6
	br	endif127
if127:
	wr	r1, fp, 6
;prevPointer=farea
	mov	r3, r1
	mov	r3, r2
;__TMP1=14
	movil	r2, 14
;alloc: farea __TMP1 index
	call	readFile
;alloc: status index
;START WHILE
	wr	r1, fp, 4
	wr	r2, fp, 1
while132:
;status==__ZERO
	rd	r1, fp, 4
	bne	r1, r0, endwhile132
;farea+=14
	rd	r2, fp, 6
	addi	r1, r2, 14
	wr	r1, fp, 6
;fileLen+=14
	rd	r3, fp, 5
	addi	r3, r3, 14
	wr	r3, fp, 5
;__TMP1=14
	movil	r2, 14
;alloc: farea __TMP1 index
	rd	r3, fp, 1
	call	readFile
	wr	r2, fp, 1
	wr	r1, fp, 4
;alloc: status index
	br	while132
endwhile132:
;END WHILE
;__TMP1=index&15
	rd	r2, fp, 1
	andi	r3, r2, 15
;__TMP1=__TMP1-1
	subi	r3, r3, 1
;fileLen+=__TMP1
	rd	r4, fp, 5
	add	r4, r4, r3
	wr	r4, fp, 5
	rd	r1, fp, 6
endif127:
	wr	r1, fp, 6
;END IF
;__TMP1="Swap tape, then press any key"
	movi	r1, string28
;alloc: __TMP1
	call	prints
	call	readc
;alloc: __TMP1
;__TMP1="Enter filename:\n\r"
	movi	r1, string26
;alloc: __TMP1
	call	prints
;__TMP1=13
	movil	r2, 13
;alloc: fname __TMP1
	rd	r1, fp, 0
	call	reads
;alloc: fname
	rd	r1, fp, 0
	call	openFile
;alloc: index
;START IF
;index==__ZERO
	bne	r1, r0, if136
;alloc: fname
	rd	r1, fp, 0
	call	makeFile
;alloc: index
	br	endif136
if136:
	wr	r1, fp, 1
;__TMP1="File already exists\n\r"
	movi	r1, string29
;alloc: __TMP1
	call	prints
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
endif136:
	mov	r3, r1
;END IF
;alloc: farea fileLen index
	rd	r1, fp, 6
	rd	r2, fp, 5
	call	writeFile
;alloc: status index
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
#global get_segment_length
get_segment_length:
	push	ra
	push	fp
	mov	fp, sp
	addi	sp, sp, 4
;alloc: index
;__TMP0=index&15
	andi	r2, r1, 15
;index-=__TMP0
	sub	r1, r1, r2
;__TMP0=tmp2@
	addi	r2, fp, 1
;__TMP1=1
	movil	r3, 1
	wr	r3, fp, 3
	mov	r3, r1
	mov	r1, r2
;alloc: __TMP0 __TMP1 index
	rd	r2, fp, 3
	call	os_tape_read
;alloc: __TMP1
;tmp2=tmp2>>24
	rd	r1, fp, 1
	sri	r1, r1, 24
;alloc: tmp2
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
#global get_next_segment
get_next_segment:
	push	ra
	push	fp
	mov	fp, sp
	addi	sp, sp, 2
;alloc: index
;__TMP0=tmp@
	addi	r2, fp, 1
;__TMP1=1
	movil	r3, 1
;__TMP2=index|15
	ori	r4, r1, 15
	mov	r1, r2
;alloc: __TMP0 __TMP1 __TMP2
	mov	r2, r3
	mov	r3, r4
	call	os_tape_read
;alloc: __TMP2
;alloc: tmp
	rd	r1, fp, 1
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
#global get_prev_segment
get_prev_segment:
	push	ra
	push	fp
	mov	fp, sp
	addi	sp, sp, 2
;alloc: index
;__TMP0=tmp@
	addi	r2, fp, 1
;__TMP1=1
	movil	r3, 1
;__TMP2=index&15
	andi	r4, r1, 15
;__TMP2=index-__TMP2
	sub	r4, r1, r4
	mov	r1, r2
;alloc: __TMP0 __TMP1 __TMP2
	mov	r2, r3
	mov	r3, r4
	call	os_tape_read
;alloc: __TMP2
;__TMP2=tmp<<8
	rd	r1, fp, 1
	sli	r2, r1, 8
;tmp=__TMP2>>8
	sri	r1, r2, 8
;alloc: tmp
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
#global set_segment_length
set_segment_length:
	push	ra
	push	fp
	mov	fp, sp
	addi	sp, sp, 3
	wr	r2, fp, 1
;alloc: index length
;__TMP0=index&15
	andi	r3, r1, 15
;index-=__TMP0
	sub	r1, r1, r3
	wr	r1, fp, 0
;__TMP0=tmp2@
	addi	r1, fp, 2
;__TMP1=1
	movil	r2, 1
;alloc: __TMP0 __TMP1 index
	rd	r3, fp, 0
	call	os_tape_read
;alloc: __TMP1
;__TMP1=tmp2<<8
	rd	r1, fp, 2
	sli	r2, r1, 8
;tmp2=__TMP1>>8
	sri	r1, r2, 8
;__TMP1=length<<24
	rd	r2, fp, 1
	sli	r3, r2, 24
;tmp2|=__TMP1
	or	r1, r1, r3
	wr	r1, fp, 2
;__TMP1=tmp2@
	addi	r1, fp, 2
;__TMP0=1
	movil	r2, 1
;alloc: __TMP1 __TMP0 index
	rd	r3, fp, 0
	call	os_tape_write
;alloc: __TMP0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
#global read_segment_data
read_segment_data:
	push	ra
	push	fp
	mov	fp, sp
	addi	sp, sp, 3
	wr	r2, fp, 1
	wr	r3, fp, 2
	wr	r1, fp, 0
;alloc: index length buffer
;alloc: index
	call	get_segment_length
;alloc: tmp
;__TMP0=14
	movil	r2, 14
;__TMP1=index&15
	rd	r3, fp, 0
	andi	r4, r3, 15
;__TMP1=__TMP1-1
	subi	r4, r4, 1
;tmp2=__TMP0-__TMP1
	sub	r5, r2, r4
;START IF
;__TMP1=tmp2-1
	subi	r2, r5, 1
;tmp<__TMP1
	ble	r2, r1, if157
if157:
;END IF
;START IF
;length>tmp2
	rd	r2, fp, 1
	ble	r2, r5, if160
if160:
;END IF
;alloc: buffer length index
	rd	r1, fp, 2
	call	os_tape_read
	mov	r2, r1
;alloc: index
;alloc: __ZERO index
	mov	r1, r0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
#global write_segment_data
write_segment_data:
	push	ra
	push	fp
	mov	fp, sp
	addi	sp, sp, 3
;alloc: index length buffer
;tmp=index&15
	andi	r4, r1, 15
;START IF
;tmp==__ZERO
	bne	r4, r0, if166
;index++
	addi	r1, r1, 1
;tmp=1
	movil	r4, 1
if166:
	wr	r2, fp, 1
	wr	r3, fp, 2
	wr	r1, fp, 0
;END IF
;alloc: index
	call	remaining_segment_data
;alloc: tmp
;START IF
;length>tmp
	rd	r2, fp, 1
	ble	r2, r1, if169
;__TMP0=1
	movil	r1, 1
;alloc: __TMP0 index
	rd	r2, fp, 0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if169:
;END IF
;alloc: buffer length index
	rd	r1, fp, 2
	rd	r3, fp, 0
	call	os_tape_write
	mov	r2, r1
;alloc: index
;alloc: __ZERO index
	mov	r1, r0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
#global remaining_segment_data
remaining_segment_data:
	push	fp
	mov	fp, sp
	addi	sp, sp, 2
;alloc: index
;index=index&15
	andi	r1, r1, 15
;START IF
;index==__ZERO
	bne	r1, r0, if176
;result=14
	movil	r2, 14
	wr	r2, fp, 1
	br	endif176
if176:
;index-=1
	subi	r1, r1, 1
;START IF
;__TMP0=14
	movil	r2, 14
;index==__TMP0
	bne	r1, r2, if180
;result=__ZERO
	mov	r2, r0
	wr	r2, fp, 1
	br	endif180
if180:
;__TMP0=14
	movil	r2, 14
;result=__TMP0-index
	sub	r3, r2, r1
	wr	r3, fp, 1
endif180:
;END IF
endif176:
;END IF
;alloc: result
	rd	r1, fp, 1
;return
	mov	sp, fp
	pop	fp
	jmpr	ra
#global openfile
openfile:
	push	ra
	push	fp
	mov	fp, sp
	addi	sp, sp, 5
	wr	r1, fp, 0
;alloc: filename
;tapePos=8
	movil	r2, 8
	wr	r2, fp, 1
;nameOffset=__ZERO
	mov	r3, r0
	wr	r3, fp, 2
;fileOffset=__ZERO
	mov	r4, r0
	wr	r4, fp, 3
;__TMP0=4
	movil	r1, 4
;alloc: __TMP0
	call	salloc
;alloc: farea
;START WHILE
	wr	r1, fp, 4
while192:
;__TMP0=512
	movil	r1, 512
;tapePos<__TMP0
	rd	r2, fp, 1
	ble	r1, r2, endwhile192
;__TMP0=4
	movil	r2, 4
;alloc: farea __TMP0 tapePos
	rd	r1, fp, 4
	rd	r3, fp, 1
	call	os_tape_read
;alloc: tapePos
;START WHILE
while194:
;__TMP0=3
	movil	r2, 3
;fileOffset<__TMP0
	rd	r3, fp, 3
	ble	r2, r3, endwhile194
;__TMP0=farea+fileOffset
;fileInt=(farea+fileOffset)*
	rd	r4, fp, 4
	rdr	r2, r4, r3
;__TMP0=(char)fileInt
	andi	r6, r2, 255
;fchar=__TMP0
;__TMP0=filename+nameOffset
;nchar=(filename+nameOffset)*
	rd	r7, fp, 0
	rd	r8, fp, 2
	rdr	r5, r7, r8
;START IF
;fchar!=nchar
	beq	r6, r5, if199
;break
	br	endwhile194
if199:
;START IF
;nchar==__ZERO
	bne	r5, r0, if203
;__TMP0=farea+3
;__TMP0=(farea+3)*
	rd	r1, r4, 3
;alloc: __TMP0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if203:
;END IF
endif199:
;END IF
;nameOffset++
	addi	r8, r8, 1
;__TMP0=fileInt>>8
	sri	r9, r2, 8
;__TMP0=(char)__TMP0
	andi	r9, r9, 255
;fchar=__TMP0
	mov	r6, r9
;__TMP0=filename+nameOffset
;nchar=(filename+nameOffset)*
	rdr	r5, r7, r8
;START IF
;fchar!=nchar
	beq	r6, r5, if207
	wr	r8, fp, 2
;break
	br	endwhile194
if207:
;START IF
;nchar==__ZERO
	bne	r5, r0, if211
;__TMP0=farea+3
;__TMP0=(farea+3)*
	rd	r1, r4, 3
;alloc: __TMP0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if211:
;END IF
endif207:
;END IF
;nameOffset++
	addi	r8, r8, 1
;__TMP0=fileInt>>16
	sri	r9, r2, 16
;__TMP0=(char)__TMP0
	andi	r9, r9, 255
;fchar=__TMP0
	mov	r6, r9
;__TMP0=filename+nameOffset
;nchar=(filename+nameOffset)*
	rdr	r5, r7, r8
;START IF
;fchar!=nchar
	beq	r6, r5, if215
	wr	r8, fp, 2
;break
	br	endwhile194
if215:
;START IF
;nchar==__ZERO
	bne	r5, r0, if219
;__TMP0=farea+3
;__TMP0=(farea+3)*
	rd	r1, r4, 3
;alloc: __TMP0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if219:
;END IF
endif215:
;END IF
;nameOffset++
	addi	r8, r8, 1
;__TMP0=fileInt>>24
	sri	r9, r2, 24
;__TMP0=(char)__TMP0
	andi	r9, r9, 255
;fchar=__TMP0
	mov	r6, r9
;__TMP0=filename+nameOffset
;nchar=(filename+nameOffset)*
	rdr	r5, r7, r8
;START IF
;fchar!=nchar
	beq	r6, r5, if223
	wr	r8, fp, 2
;break
	br	endwhile194
if223:
;START IF
;__TMP0=nchar==__ZERO
	cmp	r9, r5, r0
	not	r9
	andi	r9, r9, 1
;__TMP1=2
	movil	r10, 2
;__TMP1=fileOffset==__TMP1
	cmp	r10, r3, r10
	not	r10
	andi	r10, r10, 1
;__TMP0=__TMP0|__TMP1
	or	r9, r9, r10
	beq	r9, r0, if227
;__TMP0=farea+3
;__TMP0=(farea+3)*
	rd	r1, r4, 3
;alloc: __TMP0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if227:
;END IF
endif223:
;END IF
;fileOffset++
	addi	r3, r3, 1
	wr	r3, fp, 3
;nameOffset++
	addi	r8, r8, 1
	wr	r8, fp, 2
	br	while194
endwhile194:
	wr	r1, fp, 1
;END WHILE
;fileOffset=__ZERO
	mov	r3, r0
	wr	r3, fp, 3
;nameOffset=__ZERO
	mov	r2, r0
	wr	r2, fp, 2
	br	while192
endwhile192:
;END WHILE
;alloc: __ZERO
	mov	r1, r0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
#global os_tape_read
os_tape_read:
	push	ra
	push	fp
	mov	fp, sp
	addi	sp, sp, 5
	wr	r3, fp, 2
	wr	r1, fp, 0
	wr	r2, fp, 1
;alloc: buffer length index
;__TMP0=tapemutex@
	movi	r1, tapemutex
;alloc: __TMP0
	call	mutexAquire
;alloc: index
	rd	r1, fp, 2
	call	seek
;alloc: __TMP0
;i=__ZERO
	mov	r1, r0
;START WHILE
	wr	r1, fp, 4
while236:
;i<length
	rd	r1, fp, 1
	rd	r2, fp, 4
	ble	r1, r2, endwhile236
	call	os_tape_readword
;alloc: __TMP0
;buffer<-__TMP0
	rd	r2, fp, 0
	wr	r1, r2, 0
;i++
	rd	r1, fp, 4
	addi	r1, r1, 1
	wr	r1, fp, 4
;buffer++
	addi	r2, r2, 1
	wr	r2, fp, 0
	br	while236
endwhile236:
;END WHILE
;__TMP0=tapemutex@
	movi	r1, tapemutex
;alloc: __TMP0
	call	mutexRelease
;__TMP0=index+i
	rd	r1, fp, 2
	rd	r2, fp, 4
	add	r1, r1, r2
;alloc: __TMP0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
#global os_tape_write
os_tape_write:
	push	ra
	push	fp
	mov	fp, sp
	addi	sp, sp, 5
	wr	r3, fp, 2
	wr	r1, fp, 0
	wr	r2, fp, 1
;alloc: data length index
;__TMP0=tapemutex@
	movi	r1, tapemutex
;alloc: __TMP0
	call	mutexAquire
;alloc: index
	rd	r1, fp, 2
	call	seek
;alloc: __TMP0
;i=__ZERO
	mov	r1, r0
;START WHILE
	wr	r1, fp, 4
while242:
;i<length
	rd	r1, fp, 1
	rd	r2, fp, 4
	ble	r1, r2, endwhile242
;__TMP0=data#
	rd	r4, fp, 0
	rd	r1, r4, 0
;alloc: __TMP0
	call	os_tape_writeword
;i++
	rd	r1, fp, 4
	addi	r1, r1, 1
	wr	r1, fp, 4
;data++
	rd	r2, fp, 0
	addi	r2, r2, 1
	wr	r2, fp, 0
	br	while242
endwhile242:
;END WHILE
;__TMP0=tapemutex@
	movi	r1, tapemutex
;alloc: __TMP0
	call	mutexRelease
;__TMP0=index+i
	rd	r1, fp, 2
	rd	r2, fp, 4
	add	r1, r1, r2
;alloc: __TMP0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
#global os_tape_readfile
os_tape_readfile:
	push	ra
	push	fp
	mov	fp, sp
	addi	sp, sp, 13
;alloc: buffer length index
;errCode=FILE_NOERR
	rd	r4, r0, FILE_NOERR
	mov	r5, r4
;fileType=index>>24
	sri	r6, r3, 24
;START IF
;__TMP0=fileType&1
	andi	r7, r6, 1
;__TMP0==__ZERO
	bne	r7, r0, if249
	wr	r6, fp, 4
	wr	r1, fp, 0
	wr	r2, fp, 1
	wr	r3, fp, 2
	wr	r5, fp, 3
;alloc: buffer length index
	call	os_tape_read
	mov	r2, r1
;alloc: __TMP0
;alloc: FILE_NOERR __TMP0
	rd	r1, r0, FILE_NOERR
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if249:
;END IF
;__TMP0=16777215
;__TMP0=16777215
	movhi	r7, 255
	ori	r7, r7, 65535
;index&=__TMP0
	and	r3, r3, r7
;START IF
;index==__ZERO
	bne	r3, r0, if253
;__TMP0=fileType<<24
	sli	r7, r6, 24
;__TMP0=index|__TMP0
	or	r2, r3, r7
;alloc: FILE_IFP __TMP0
	rd	r1, r0, FILE_IFP
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if253:
	wr	r2, fp, 1
	wr	r3, fp, 2
	wr	r5, fp, 3
	wr	r6, fp, 4
	wr	r1, fp, 0
;END IF
;__TMP0=14
	movil	r1, 14
;alloc: __TMP0
	call	salloc
;alloc: tapeOut
;currentLoc=index&15
	rd	r2, fp, 2
	andi	r3, r2, 15
;START IF
;currentLoc==__ZERO
	bne	r3, r0, if261
	wr	r3, fp, 9
	wr	r1, fp, 6
	mov	r3, r2
;__TMP0=tmp@
	addi	r1, fp, 7
;__TMP1=1
	movil	r2, 1
;alloc: __TMP0 __TMP1 index
	call	os_tape_read
;alloc: index
;currentLoc=1
	movil	r2, 1
	wr	r2, fp, 9
	mov	r2, r1
	wr	r2, fp, 2
	rd	r1, fp, 6
	rd	r3, fp, 9
	br	endif261
if261:
;START IF
;__TMP1=15
	movil	r4, 15
;currentLoc==__TMP1
	bne	r3, r4, if264
	wr	r3, fp, 9
	wr	r1, fp, 6
	mov	r3, r2
;__TMP1=dummy@
	addi	r1, fp, 8
;__TMP0=1
	movil	r2, 1
;alloc: __TMP1 __TMP0 index
	call	os_tape_read
;alloc: __TMP0
;START IF
;dummy==__ZERO
	rd	r1, fp, 8
	bne	r1, r0, if266
;__TMP0=fileType<<24
	rd	r2, fp, 4
	sli	r3, r2, 24
;__TMP0=index|__TMP0
	rd	r4, fp, 2
	or	r2, r4, r3
;alloc: FILE_EOF __TMP0
	rd	r1, r0, FILE_EOF
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if266:
	mov	r3, r1
;END IF
;currentLoc=1
	movil	r2, 1
	wr	r2, fp, 9
;index=dummy
;__TMP0=tmp@
	addi	r1, fp, 7
;__TMP1=1
	movil	r2, 1
;alloc: __TMP0 __TMP1 index
	call	os_tape_read
	mov	r2, r1
;alloc: index
	wr	r2, fp, 2
	rd	r1, fp, 6
	rd	r3, fp, 9
	br	endif264
if264:
	wr	r1, fp, 6
	wr	r3, fp, 9
;__TMP1=tmp@
	addi	r1, fp, 7
;__TMP0=1
	movil	r5, 1
;__TMP2=index-currentLoc
	sub	r3, r2, r3
	mov	r2, r5
;alloc: __TMP1 __TMP0 __TMP2
	call	os_tape_read
;alloc: __TMP2
	rd	r1, fp, 6
	rd	r2, fp, 2
	rd	r3, fp, 9
endif264:
;END IF
endif261:
;END IF
;tmp=tmp>>24
	rd	r4, fp, 7
	sri	r4, r4, 24
;__TMP2=15
	movil	r5, 15
;toRead=__TMP2-currentLoc
	sub	r6, r5, r3
;START IF
;toRead>length
	rd	r5, fp, 1
	ble	r6, r5, if274
;toRead=length
	mov	r6, r5
if274:
;END IF
;START IF
;__TMP2=14
	movil	r7, 14
;__TMP2=tmp<__TMP2
	cmp	r7, r4, r7
	addi	r7, r7, 1
	cmp	r7, r7, r0
	xori	r7, r7, 1
;__TMP0=toRead>tmp
	movi	r8, 0
	ble	r6, r4, 1
	movi	r8, 1
;__TMP2=__TMP2&__TMP0
	and	r7, r7, r8
	beq	r7, r0, if277
;toRead=tmp
	mov	r6, r4
;length=toRead
	mov	r5, r6
;errCode=FILE_EOF
	rd	r7, r0, FILE_EOF
	mov	r8, r7
	wr	r5, fp, 1
	wr	r8, fp, 3
if277:
	wr	r3, fp, 9
	mov	r3, r2
	wr	r4, fp, 7
	wr	r1, fp, 6
	mov	r2, r6
	wr	r2, fp, 12
;END IF
;alloc: buffer toRead index
	rd	r1, fp, 0
	call	os_tape_read
;alloc: index
;buffer+=toRead
	rd	r2, fp, 0
	rd	r3, fp, 12
	add	r2, r2, r3
;length-=toRead
	rd	r4, fp, 1
	sub	r4, r4, r3
;START WHILE
	wr	r1, fp, 2
	wr	r2, fp, 0
	wr	r4, fp, 1
while280:
	rd	r1, fp, 1
	beq	r1, r0, endwhile280
;__TMP2=dummy@
	addi	r2, fp, 8
;__TMP0=1
	movil	r3, 1
;alloc: index
	rd	r1, fp, 2
	call	get_next_segment
;alloc: __TMP1
;START IF
;dummy==__ZERO
	rd	r1, fp, 8
	bne	r1, r0, if282
;errCode=FILE_EOF
	rd	r2, r0, FILE_EOF
	mov	r3, r2
	wr	r3, fp, 3
;break
	rd	r1, fp, 1
	br	endwhile280
if282:
;END IF
;index=dummy
	mov	r1, r1
	wr	r1, fp, 2
;alloc: index
	call	get_segment_length
;alloc: tmp
;toRead=14
	movil	r2, 14
;START IF
;toRead>length
	rd	r3, fp, 1
	ble	r2, r3, if286
;toRead=length
	mov	r2, r3
if286:
;END IF
;START IF
;__TMP2=14
	movil	r4, 14
;__TMP2=tmp<__TMP2
	cmp	r4, r1, r4
	addi	r4, r4, 1
	cmp	r4, r4, r0
	xori	r4, r4, 1
;__TMP0=toRead>tmp
	movi	r5, 0
	ble	r2, r1, 1
	movi	r5, 1
;__TMP2=__TMP2&__TMP0
	and	r4, r4, r5
	beq	r4, r0, if289
;toRead=tmp
	mov	r2, r1
;length=toRead
	mov	r3, r2
;errCode=FILE_EOF
	rd	r4, r0, FILE_EOF
	mov	r5, r4
	wr	r3, fp, 1
	wr	r5, fp, 3
if289:
	wr	r1, fp, 7
	wr	r2, fp, 12
;END IF
;alloc: buffer toRead index
	rd	r1, fp, 0
	rd	r3, fp, 2
	call	read_segment_data
	wr	r1, fp, 3
	wr	r2, fp, 2
;alloc: errCode index
;buffer+=toRead
	rd	r3, fp, 0
	rd	r4, fp, 12
	add	r3, r3, r4
	wr	r3, fp, 0
;length-=toRead
	rd	r5, fp, 1
	sub	r5, r5, r4
	wr	r5, fp, 1
	br	while280
endwhile280:
;END WHILE
;__TMP2=fileType<<24
	rd	r2, fp, 4
	sli	r3, r2, 24
;__TMP2=index|__TMP2
	rd	r4, fp, 2
	or	r2, r4, r3
;alloc: errCode __TMP2
	rd	r1, fp, 3
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
#global os_tape_writefile
os_tape_writefile:
	push	ra
	push	fp
	mov	fp, sp
	addi	sp, sp, 12
;alloc: data length index
;errors=FILE_NOERR
	rd	r4, r0, FILE_NOERR
	mov	r5, r4
;fileType=index>>24
	sri	r6, r3, 24
;START IF
;__TMP0=fileType&1
	andi	r7, r6, 1
;__TMP0==__ZERO
	bne	r7, r0, if297
	wr	r6, fp, 3
	wr	r1, fp, 0
	wr	r2, fp, 1
	wr	r3, fp, 2
	wr	r5, fp, 4
;alloc: data length index
	call	os_tape_write
	mov	r2, r1
;alloc: __TMP0
;alloc: errors __TMP0
	rd	r1, fp, 4
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if297:
;END IF
;__TMP0=16777215
;__TMP0=16777215
	movhi	r7, 255
	ori	r7, r7, 65535
;index&=__TMP0
	and	r3, r3, r7
;START IF
;index==__ZERO
	bne	r3, r0, if301
;__TMP0=fileType<<24
	sli	r7, r6, 24
;__TMP0=index|__TMP0
	or	r2, r3, r7
;alloc: FILE_IFP __TMP0
	rd	r1, r0, FILE_IFP
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if301:
;END IF
;__TMP0=15
	movil	r7, 15
;__TMP1=index&15
	andi	r8, r3, 15
;toWrite=__TMP0-__TMP1
	sub	r9, r7, r8
;START IF
;__TMP1=15
	movil	r7, 15
;toWrite==__TMP1
	bne	r9, r7, if306
;toWrite=14
	movil	r9, 14
;index++
	addi	r3, r3, 1
if306:
;END IF
;START IF
;toWrite>length
	ble	r9, r2, if309
;toWrite=length
	mov	r9, r2
if309:
	wr	r5, fp, 4
	wr	r6, fp, 3
	wr	r2, fp, 1
	mov	r2, r9
	wr	r2, fp, 6
	wr	r1, fp, 0
;END IF
;alloc: data toWrite index
	call	os_tape_write
	wr	r1, fp, 2
;alloc: index
;length-=toWrite
	rd	r2, fp, 1
	rd	r3, fp, 6
	wr	r3, fp, 6
	sub	r2, r2, r3
	wr	r2, fp, 1
;data+=toWrite
	rd	r4, fp, 0
	add	r4, r4, r3
	wr	r4, fp, 0
;__TMP1=index&15
	andi	r5, r1, 15
;tmp=index-__TMP1
	sub	r3, r1, r5
	wr	r3, fp, 9
;__TMP1=tmp2@
	addi	r1, fp, 8
;__TMP0=1
	movil	r2, 1
;alloc: __TMP1 __TMP0 tmp
	call	os_tape_read
;alloc: __TMP0
;fillsize=tmp2>>24
	rd	r1, fp, 8
	sri	r2, r1, 24
;fillsize+=toWrite
	rd	r3, fp, 6
	add	r2, r2, r3
;__TMP0=tmp2<<8
	sli	r4, r1, 8
;tmp2=__TMP0>>8
	sri	r1, r4, 8
;__TMP0=fillsize<<24
	sli	r4, r2, 24
;tmp2|=__TMP0
	or	r1, r1, r4
	wr	r1, fp, 8
;__TMP0=tmp2@
	addi	r1, fp, 8
;__TMP1=1
	movil	r2, 1
;alloc: __TMP0 __TMP1 tmp
	rd	r3, fp, 9
	call	os_tape_write
;alloc: __TMP1
;START WHILE
while313:
	rd	r1, fp, 1
	beq	r1, r0, endwhile313
;__TMP1=newIndex@
	addi	r1, fp, 11
;__TMP0=1
	movil	r2, 1
;alloc: __TMP1 __TMP0 index
	rd	r3, fp, 2
	call	os_tape_read
;alloc: __TMP0
;START IF
;newIndex==__ZERO
	rd	r1, fp, 11
	bne	r1, r0, if316
;__TMP0=1
	movil	r1, 1
;alloc: __TMP0
	call	os_tape_allocate
	wr	r1, fp, 11
;alloc: newIndex
;__TMP0=newIndex@
	addi	r1, fp, 11
;__TMP1=1
	movil	r2, 1
;alloc: __TMP0 __TMP1 index
	rd	r3, fp, 2
	call	os_tape_write
;alloc: __TMP1
;index-=15
	rd	r1, fp, 2
	subi	r1, r1, 15
	wr	r1, fp, 2
;__TMP1=index@
	addi	r1, fp, 2
;__TMP0=1
	movil	r2, 1
;alloc: __TMP1 __TMP0 newIndex
	rd	r3, fp, 11
	call	os_tape_write
;alloc: __TMP0
	rd	r1, fp, 11
if316:
	mov	r3, r1
	wr	r3, fp, 2
;END IF
;index=newIndex
;__TMP0=tmp2@
	addi	r1, fp, 8
;__TMP1=1
	movil	r2, 1
;alloc: __TMP0 __TMP1 index
	call	os_tape_read
;alloc: __TMP1
;tmp=tmp2>>24
	rd	r1, fp, 8
	sri	r2, r1, 24
;__TMP1=tmp2<<8
	sli	r3, r1, 8
;tmp2=__TMP1>>8
	sri	r1, r3, 8
;toWrite=14
	movil	r3, 14
;START IF
;toWrite>length
	rd	r4, fp, 1
	ble	r3, r4, if319
;toWrite=length
	mov	r3, r4
if319:
;END IF
;START IF
;toWrite>tmp
	ble	r3, r2, if322
;__TMP1=toWrite<<24
	sli	r5, r3, 24
;tmp2|=__TMP1
	or	r1, r1, r5
	br	endif322
if322:
;__TMP1=tmp<<24
	sli	r5, r2, 24
;tmp2|=__TMP1
	or	r1, r1, r5
endif322:
	wr	r1, fp, 8
	wr	r3, fp, 6
	wr	r2, fp, 9
;END IF
;__TMP1=tmp2@
	addi	r1, fp, 8
;__TMP0=1
	movil	r2, 1
;alloc: __TMP1 __TMP0 index
	rd	r3, fp, 2
	call	os_tape_write
	mov	r3, r1
	wr	r3, fp, 2
;alloc: index
;alloc: data toWrite index
	rd	r1, fp, 0
	rd	r2, fp, 6
	call	os_tape_write
	wr	r1, fp, 2
;alloc: index
;length-=toWrite
	rd	r2, fp, 1
	rd	r3, fp, 6
	sub	r2, r2, r3
	wr	r2, fp, 1
;data+=toWrite
	rd	r4, fp, 0
	add	r4, r4, r3
	wr	r4, fp, 0
	br	while313
endwhile313:
;END WHILE
;__TMP0=fileType<<24
	rd	r2, fp, 3
	sli	r3, r2, 24
;__TMP0=index|__TMP0
	rd	r4, fp, 2
	or	r2, r4, r3
;alloc: errors __TMP0
	rd	r1, fp, 4
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
#global os_tape_allocate
os_tape_allocate:
	push	ra
	push	fp
	mov	fp, sp
	addi	sp, sp, 10
;alloc: segments
;START IF
;segments==__ZERO
	bne	r1, r0, if332
;alloc: __ZERO
	mov	r1, r0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if332:
;END IF
;START IF
;__TMP0=1
	movil	r2, 1
;segments==__TMP0
	bne	r1, r2, if336
;i=512
	movil	r2, 512
;START WHILE
	wr	r1, fp, 0
	wr	r2, fp, 1
while338:
;__TMP0=1024
	movil	r1, 1024
;i<__TMP0
	rd	r2, fp, 1
	ble	r1, r2, endwhile338
;__TMP0=tmp@
	addi	r1, fp, 2
;__TMP1=1
	movil	r2, 1
;alloc: __TMP0 __TMP1 i
	rd	r3, fp, 1
	call	os_tape_read
;alloc: i
;START IF
;__TMP1=-1
	subi	r2, r0, 1
;tmp!=__TMP1
	rd	r3, fp, 2
	beq	r3, r2, if340
	wr	r1, fp, 1
;__TMP1=-1
	subi	r2, r0, 1
;tmp^=__TMP1
	xor	r3, r3, r2
;tmp2=tmp-1
	subi	r2, r3, 1
;tmp2&=tmp
	and	r2, r2, r3
;tmp2^=tmp
	xor	r1, r2, r3
;__TMP1=-1
	subi	r4, r0, 1
;tmp^=__TMP1
	xor	r3, r3, r4
;tmp|=tmp2
	or	r3, r3, r1
	wr	r3, fp, 2
;alloc: tmp2
	call	bitnum
;alloc: tmp2
;__TMP1=tmp2*16
	muli	r2, r1, 16
;__TMP0=i-512
	rd	r3, fp, 1
	subi	r4, r3, 512
;__TMP0=__TMP0*512
	muli	r4, r4, 512
;tmp2=__TMP1+__TMP0
	add	r1, r2, r4
	wr	r1, fp, 5
;__TMP0=tmp@
	addi	r1, fp, 2
;__TMP1=1
	movil	r2, 1
;__TMP2=i-1
	subi	r3, r3, 1
;alloc: __TMP0 __TMP1 __TMP2
	call	os_tape_write
;alloc: __TMP2
;alloc: tmp2
	rd	r1, fp, 5
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if340:
	wr	r1, fp, 1
;END IF
	br	while338
endwhile338:
;END WHILE
;alloc: __ZERO
	mov	r1, r0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if336:
;END IF
;segments/=32
	divi	r1, r1, 32
;segments++
	addi	r1, r1, 1
;i=512
	movil	r2, 512
;START WHILE
	wr	r1, fp, 0
	wr	r2, fp, 1
while349:
;__TMP2=1024
	movil	r1, 1024
;i<__TMP2
	rd	r2, fp, 1
	ble	r1, r2, endwhile349
;segCopy=segments
	rd	r1, fp, 0
	mov	r3, r1
;attemptLoc=i
	mov	r4, r2
;longEnough=1
	movil	r5, 1
;START WHILE
	wr	r3, fp, 7
	wr	r4, fp, 8
	wr	r5, fp, 9
while353:
	rd	r1, fp, 7
	beq	r1, r0, endwhile353
;__TMP2=tmp@
	addi	r1, fp, 2
;__TMP1=1
	movil	r2, 1
;alloc: __TMP2 __TMP1 i
	rd	r3, fp, 1
	call	os_tape_read
;alloc: i
;START IF
;__TMP1=tmp!=__ZERO
	rd	r2, fp, 2
	cmp	r3, r2, r0
	andi	r3, r3, 1
;__TMP2=1024
	movil	r4, 1024
;__TMP2=i==__TMP2
	cmp	r4, r1, r4
	not	r4
	andi	r4, r4, 1
;__TMP1=__TMP1|__TMP2
	or	r3, r3, r4
	beq	r3, r0, if355
	wr	r1, fp, 1
;longEnough=__ZERO
	mov	r3, r0
	wr	r3, fp, 9
;break
	rd	r1, fp, 7
	br	endwhile353
if355:
	wr	r1, fp, 1
;END IF
;segCopy--
	rd	r3, fp, 7
	subi	r3, r3, 1
	wr	r3, fp, 7
	br	while353
endwhile353:
;END WHILE
;START IF
	rd	r2, fp, 9
	beq	r2, r0, if360
;i=attemptLoc
	rd	r3, fp, 8
	mov	r4, r3
;tmp=-1
	subi	r5, r0, 1
;START WHILE
	wr	r4, fp, 1
	wr	r5, fp, 2
while362:
	rd	r1, fp, 0
	beq	r1, r0, endwhile362
;segments--
	subi	r1, r1, 1
	wr	r1, fp, 0
;__TMP1=tmp@
	addi	r1, fp, 2
;__TMP2=1
	movil	r2, 1
;alloc: __TMP1 __TMP2 i
	rd	r3, fp, 1
	call	os_tape_write
	wr	r1, fp, 1
;alloc: i
	br	while362
endwhile362:
;END WHILE
;break
	br	endwhile349
if360:
;END IF
	br	while349
endwhile349:
;END WHILE
;START IF
;__TMP2=1024
	movil	r1, 1024
;i==__TMP2
	bne	r2, r1, if368
;alloc: __ZERO
	mov	r1, r0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if368:
;END IF
;__TMP2=attemptLoc-512
	rd	r1, fp, 8
	subi	r3, r1, 512
;__TMP2=__TMP2*512
	muli	r1, r3, 512
;alloc: __TMP2
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
#global os_create_file
os_create_file:
	push	ra
	push	fp
	mov	fp, sp
	addi	sp, sp, 14
;alloc: fileName type length
;START IF
;type==__ZERO
	bne	r2, r0, if374
	wr	r1, fp, 0
	mov	r2, r3
;alloc: fileName length
	call	os_create_file_continuous
;alloc: __TMP0
;alloc: __TMP0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if374:
	wr	r1, fp, 0
;END IF
;__TMP0=4
	movil	r1, 4
;alloc: __TMP0
	call	salloc
	wr	r1, fp, 4
;alloc: fileInfo
;__TMP0=3
	movil	r2, 3
;alloc: fileInfo __TMP0 __ZERO
	mov	r3, r0
	call	arrset
;__TMP0=1
	movil	r1, 1
;alloc: __TMP0
	call	os_tape_allocate
;alloc: index
;__TMP0=1
	movil	r2, 1
;__TMP0=__TMP0<<24
	sli	r2, r2, 24
;index|=__TMP0
	or	r1, r1, r2
	wr	r1, fp, 5
;__TMP0=fileInfo+3
	rd	r2, fp, 4
	addi	r3, r2, 3
;__TMP0<-index
	wr	r1, r3, 0
;alloc: fileName
	rd	r1, fp, 0
	call	strlen
;alloc: strlength
;START IF
;__TMP0=12
	movil	r2, 12
;strlength>__TMP0
	ble	r1, r2, if381
;alloc: __ZERO
	mov	r1, r0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
if381:
;END IF
;nloops=__ZERO
	mov	r2, r0
;START WHILE
	wr	r1, fp, 6
	wr	r2, fp, 8
while387:
	rd	r1, fp, 6
	beq	r1, r0, endwhile387
;tmpChars=__ZERO
	mov	r2, r0
;__TMP0=fileName+++
	rd	r3, fp, 0
	mov	r4, r3
	addi	r3, r3, 1
;__TMP0=__TMP0#
	rd	r1, r4, 0
;__TMP1=fileName+++
	mov	r5, r3
	addi	r3, r3, 1
;__TMP1=__TMP1#
	rd	r2, r5, 0
;__TMP2=fileName+++
	mov	r6, r3
	addi	r3, r3, 1
;__TMP2=__TMP2#
	rd	r6, r6, 0
;__TMP3=fileName+++
	mov	r7, r3
	addi	r3, r3, 1
	wr	r3, fp, 0
	mov	r3, r6
;__TMP3=__TMP3#
	rd	r4, r7, 0
;alloc: __TMP0 __TMP1 __TMP2 __TMP3
	call	char_pack
;alloc: tmpChars
;strlength-=4
	rd	r2, fp, 6
	subi	r2, r2, 4
;START IF
;strlength<__ZERO
	ble	r0, r2, if389
;__TMP3=-8
	subi	r3, r0, 8
;strlength*=__TMP3
	mul	r2, r2, r3
;tmpChars=tmpChars<<strlength
	shl	r1, r1, r2
;tmpChars=tmpChars>>strlength
	shr	r1, r1, r2
;strlength=__ZERO
	mov	r2, r0
if389:
	wr	r2, fp, 6
;END IF
;__TMP3=fileInfo+nloops
	rd	r3, fp, 4
	rd	r4, fp, 8
	add	r5, r3, r4
;__TMP3<-tmpChars
	wr	r1, r5, 0
;nloops++
	addi	r4, r4, 1
	wr	r4, fp, 8
	br	while387
endwhile387:
;END WHILE
;i=8
	movil	r2, 8
;START WHILE
	wr	r2, fp, 12
while394:
;__TMP3=512
	movil	r1, 512
;i<__TMP3
	rd	r2, fp, 12
	ble	r1, r2, endwhile394
;__TMP3=tmp@
	addi	r1, fp, 13
;__TMP2=1
	movil	r2, 1
;alloc: __TMP3 __TMP2 i
	rd	r3, fp, 12
	call	os_tape_read
;alloc: __TMP2
;START IF
;tmp==__ZERO
	rd	r1, fp, 13
	bne	r1, r0, if397
;break
	rd	r2, fp, 12
	br	endwhile394
if397:
;END IF
;i+=4
	rd	r2, fp, 12
	addi	r2, r2, 4
	wr	r2, fp, 12
	br	while394
endwhile394:
;END WHILE
;__TMP2=4
	movil	r2, 4
;alloc: fileInfo __TMP2 i
	rd	r1, fp, 4
	rd	r3, fp, 12
	call	os_tape_write
;alloc: i
;alloc: index
	rd	r1, fp, 5
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
#global os_create_file_continuous
os_create_file_continuous:
;alloc: fileName length
;alloc: __ZERO
	mov	r1, r0
;return
	jmpr	ra
#global os_tape_writeword
os_tape_writeword:
	push	ra
	push	fp
	mov	fp, sp
	addi	sp, sp, 1
;alloc: word
;START WHILE
	wr	r1, fp, 0
while406:
;alloc: TAPE_BUSY
	rd	r1, r0, TAPE_BUSY
	call	tape_getStatus
;alloc: __TMP0
	beq	r1, r0, endwhile406
	br	while406
endwhile406:
;END WHILE
;START WHILE
while409:
;alloc: TAPE_OBE
	rd	r1, r0, TAPE_OBE
	call	tape_getStatus
;alloc: __TMP0
;__TMP0==__ZERO
	bne	r1, r0, endwhile409
	br	while409
endwhile409:
;END WHILE
;__TMP0=259
	movil	r1, 259
;tapeout<-__TMP0
	rd	r2, r0, tapeout
	wr	r1, r2, 0
;START WHILE
while412:
;alloc: TAPE_OBE
	rd	r1, r0, TAPE_OBE
	call	tape_getStatus
;alloc: __TMP0
;__TMP0==__ZERO
	bne	r1, r0, endwhile412
	br	while412
endwhile412:
;END WHILE
;tapeout<-word
	rd	r1, fp, 0
	rd	r2, r0, tapeout
	wr	r1, r2, 0
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
#global os_tape_readword
os_tape_readword:
	push	ra
;START WHILE
while417:
;alloc: TAPE_BUSY
	rd	r1, r0, TAPE_BUSY
	call	tape_getStatus
;alloc: __TMP0
	beq	r1, r0, endwhile417
	br	while417
endwhile417:
;END WHILE
;START WHILE
while420:
;alloc: TAPE_OBE
	rd	r1, r0, TAPE_OBE
	call	tape_getStatus
;alloc: __TMP0
;__TMP0==__ZERO
	bne	r1, r0, endwhile420
	br	while420
endwhile420:
;END WHILE
;__TMP0=257
	movil	r1, 257
;tapeout<-__TMP0
	rd	r2, r0, tapeout
	wr	r1, r2, 0
;START WHILE
while423:
;alloc: TAPE_IBF
	rd	r1, r0, TAPE_IBF
	call	tape_getStatus
;alloc: __TMP0
;__TMP0==__ZERO
	bne	r1, r0, endwhile423
	br	while423
endwhile423:
;END WHILE
;__TMP0=tapein#
	rd	r2, r0, tapein
	rd	r1, r2, 0
;alloc: __TMP0
;return
	pop	ra
	jmpr	ra
#global seek
seek:
	push	ra
	push	fp
	mov	fp, sp
	addi	sp, sp, 1
;alloc: pos
;START WHILE
	wr	r1, fp, 0
while428:
;alloc: TAPE_BUSY
	rd	r1, r0, TAPE_BUSY
	call	tape_getStatus
;alloc: __TMP0
	beq	r1, r0, endwhile428
	br	while428
endwhile428:
;END WHILE
;START WHILE
while431:
;alloc: TAPE_OBE
	rd	r1, r0, TAPE_OBE
	call	tape_getStatus
;alloc: __TMP0
;__TMP0==__ZERO
	bne	r1, r0, endwhile431
	br	while431
endwhile431:
;END WHILE
;__TMP0=pos<<8
	rd	r1, fp, 0
	sli	r2, r1, 8
;__TMP0=__TMP0|2
	ori	r2, r2, 2
;tapeout<-__TMP0
	rd	r3, r0, tapeout
	wr	r2, r3, 0
;alloc: pos
;return
	mov	sp, fp
	pop	fp
	pop	ra
	jmpr	ra
#global tape_getStatus
tape_getStatus:
;alloc: status
;__TMP0=tapestat#
	rd	r3, r0, tapestat
	rd	r2, r3, 0
;__TMP1=1
	movil	r4, 1
;__TMP1=__TMP1<<status
	shl	r4, r4, r1
;__TMP0=__TMP0&__TMP1
	and	r2, r2, r4
;__TMP0=__TMP0>>status
	shr	r1, r2, r1
;alloc: __TMP0
;return
	jmpr	ra
#global os_mutex_aq
os_mutex_aq:
;alloc: mutex
;i=mutex#
	rd	r2, r1, 0
;START IF
	beq	r2, r0, if439
;alloc: __ZERO
	mov	r1, r0
;return
	jmpr	ra
if439:
;END IF
;__TMP0=1
	movil	r3, 1
;mutex<-__TMP0
	wr	r3, r1, 0
;__TMP0=1
	movil	r1, 1
;alloc: __TMP0
;return
	jmpr	ra
#global os_mutex_drop
os_mutex_drop:
;alloc: mutex
;mutex<-__ZERO
	wr	r0, r1, 0
;return
	jmpr	ra
#global reboot
reboot:
	push	ra
	push	fp
	mov	fp, sp
	addi	sp, sp, 1
;__TMP0=44
	movil	r1, 44
;__TMP0=(int#)__TMP0
;__TMP0<-__ZERO
	wr	r0, r1, 0
;__TMP0=128
	movil	r1, 128
	wr	r1, fp, 0
;__TMP0=(func0)__TMP0
	rd	r1, fp, 0
	callr	r1
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
while486:
;i==__ZERO
	rd	r1, fp, 1
	bne	r1, r0, endwhile486
	call	sleep
;alloc: mutex
	rd	r1, fp, 0
;---BEGIN INLINE ASSEMBLER---
	inti	128
;---END INLINE ASSEMBLER---
	wr	r1, fp, 1
;alloc: i
	br	while486
endwhile486:
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
while538:
;__TMP0=str#
	rd	r3, r1, 0
	beq	r3, r0, endwhile538
;len++
	addi	r2, r2, 1
;str++
	addi	r1, r1, 1
	br	while538
endwhile538:
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
while552:
;len--
	subi	r2, r2, 1
;len>=__ZERO
	bl	r2, r0, endwhile552
;arr<-val
	wr	r3, r1, 0
;arr++
	addi	r1, r1, 1
	br	while552
endwhile552:
;END WHILE
;return
	jmpr	ra
#global strcmp
strcmp:
;alloc: str1 str2 maxlen
;i=__ZERO
	mov	r4, r0
;START WHILE
while558:
;i<maxlen
	ble	r3, r4, endwhile558
;__TMP0=str1+i
;c1=(str1+i)*
	rdr	r5, r1, r4
;__TMP0=str2+i
;c2=(str2+i)*
	rdr	r6, r2, r4
;START IF
;c1!=c2
	beq	r5, r6, if562
;alloc: __ZERO
	mov	r1, r0
;return
	jmpr	ra
if562:
;START IF
;c1==__ZERO
	bne	r5, r0, if566
;__TMP0=1
	movil	r1, 1
;alloc: __TMP0
;return
	jmpr	ra
if566:
;END IF
endif562:
;END IF
;i++
	addi	r4, r4, 1
;nosave c1
;nosave c2
	br	while558
endwhile558:
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
while576:
;__TMP0=i==__ZERO
	cmp	r5, r4, r0
	not	r5
	andi	r5, r5, 1
;__TMP0=x|__TMP0
	or	r5, r2, r5
	beq	r5, r0, endwhile576
;tmp=x%base
	div	r0, r2, r3
	mov	r5, ex
;x/=base
	div	r2, r2, r3
;__TMP0="0123456789abcdef"
	movi	r6, string30
;__TMP0=__TMP0+tmp
;__TMP0=(__TMP0+tmp)*
	rdr	r6, r6, r5
;outStr<-__TMP0
	wr	r6, r1, 0
;i++
	addi	r4, r4, 1
;outStr++
	addi	r1, r1, 1
	br	while576
endwhile576:
;END WHILE
;outStr<-__ZERO
	wr	r0, r1, 0
;outStr--
	subi	r1, r1, 1
;i--
	subi	r4, r4, 1
;START WHILE
while580:
;i>__ZERO
	ble	r4, r0, endwhile580
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
	br	while580
endwhile580:
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
while588:
	beq	r3, r0, endwhile588
;START IF
;__TMP0="0"
	movi	r4, string31
;__TMP0=__TMP0#
	rd	r4, r4, 0
;__TMP0=c<__TMP0
	cmp	r4, r3, r4
	addi	r4, r4, 1
	cmp	r4, r4, r0
	xori	r4, r4, 1
;__TMP1="9"
	movi	r5, string32
;__TMP1=c>__TMP1
	cmp	r5, r5, r3
	addi	r5, r5, 1
	cmp	r5, r5, r0
	xori	r5, r5, 1
;__TMP0=__TMP0|__TMP1
	or	r4, r4, r5
	beq	r4, r0, if590
;alloc: __ZERO __ZERO
	mov	r1, r0
	mov	r2, r0
;return
	jmpr	ra
if590:
;END IF
;__TMP0=result*10
	muli	r4, r2, 10
;__TMP1="0"
	movi	r5, string31
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
	br	while588
endwhile588:
;END WHILE
;__TMP1=1
	movil	r1, 1
;alloc: __TMP1 result
;return
	jmpr	ra
.data
string0:
#string "PC-OS version 0.1\n\r"
string1:
#string "Ready\n\r"
string2:
#string "halt"
string3:
#string "goodbye"
string4:
#string "clear"
string5:
#string "reboot"
string6:
#string "rebooting..."
string7:
#string "format"
string8:
#string "list"
string9:
#string "copy"
string10:
#string " "
string11:
#string "Program name overflow\n\r"
string12:
#string "No such program:\n\r"
string13:
#string "\n\r"
string14:
#string "Cannot open program:\n\rstart index too low\n\r"
string15:
#string "Tape already formatted\n\r"
string16:
#string "Continue?y/n\n\r"
string17:
#string "y"
string18:
#string "Formatting...\n\r"
string19:
#string "Bootable program present\n\r"
string20:
#string "Do you want to keep it?y/n\n\r"
string21:
#string "n"
string22:
#string "Creating boot file...\n\r"
string23:
#string "b"
string24:
#string "o"
string25:
#string "t"
string26:
#string "Enter filename:\n\r"
string27:
#string "File does not exist\n\r"
string28:
#string "Swap tape, then press any key"
string29:
#string "File already exists\n\r"
tapemutex:
0
tapein:
65
tapeout:
64
tapestat:
66
tapeint:
67
FILE_NOERR:
0
FILE_EOF:
1
FILE_IFP:
2
TAPE_BUSY:
3
TAPE_OBE:
1
TAPE_IBF:
0
string30:
#string "0123456789abcdef"
string31:
#string "0"
string32:
#string "9"
