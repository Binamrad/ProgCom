;this file contains the interrupt handler and other speed critical things that are most suitable for assembler code

;TODO:
;add a wayt to tie keypresses to functions. When the possibility has been added:
;	have a way to pause screen scrolling
;	have a way to exit a program and resume OS (even if in infinite loop)
.text
#include libLabel.txt
#include libText.txt
#include os.asm

;*********************************************************
;OS input functionality
;*********************************************************
.data
os_input_queue:
#allocate 64
os_input_read_index:
0
os_input_write_index:
0
.text

;the reason for handling input like this is that we can intercept certain
;keypresses (F1-F12, scroll lock etc), and thus provide some easy functionality for certain programs
keyboard_interrupt:
	rd	r1, r0, GLOBAL_KEYBOARD
	movi	r2, 2
	wr	r2, r0, GLOBAL_KEYBOARD_STATUS
	rd	r2, r0, os_input_read_index
	rd	r3, r0, os_input_write_index
	addi	r4, r3, 1
	andi	r4, r4, 63
	beq	r4, r2, keyboard_interrupt_end
	wr	r1, r4, os_input_queue
	wr	r4, r0, os_input_write_index
keyboard_interrupt_end:
	movi	r1, 2;clear new input bit
	wr	r1, r0, GLOBAL_KEYBOARD_STATUS
	jmpr	ra

os_keyboard_input:
	rd	r3, r0, os_input_read_index
	addi	r3, r3, 1
	andi	r3, r3, 63
	rd	r4, r0, os_input_write_index
	bne	r3, r4, os_keyboard_input_success
	movi	r2, 0
	br	os_keyboard_input_end
os_keyboard_input_success:
	rd	r1, r3, os_input_queue
	wr	r3, r0, os_input_read_index
	movi	r2, 1
os_keyboard_input_end:
	jmpr	ra

os_keyboard_input_synchronus:
	push	ra
	inti	132
	pop	ra
	jmpr	ra

os_input_flush:
	rd	r1, r0, os_input_write_index
	wr	r1, r0, os_input_read_index
	jmpr	ra

;*********************************************************
;OS entry point
;*********************************************************

.data
bootinitmsg:
#string "booting..."
.text

;entry point for operating system
#global main
main:
	;rd	r1, r0, GLOBAL_NUMPAD_NEWIN
	;beq	r1, r0, main

	movi	r1, bootinitmsg
	call	printStrLn

	;initialise text reading utility
	movi	r1, sleep			;sleep is provided by os.asm
	wr	r1, r0, libt_input_wait_function
	movi	r1, os_keyboard_input_synchronus
	wr	r1, r0, libt_input_function
	wr	r0, r0, GLOBAL_SCREEN_SCROLL

	;initialise all handlers to ignore interrupts
	movi	r1, __dummyhandler
	movi	r2, 32
int_initloop:
	subi	r2, r2, 1
	wr	r1, r2, hwint
	wr	r1, r2, swint
	wr	r1, r2, sysint
	ble	r0, r2, int_initloop


	;register interrupt handlers
	movi	r1, 0
	movi	r2, printStr
	call	interrupthandler_register

	movi	r1, 1
	movi	r2, printChar
	call	interrupthandler_register

	movi	r1, 2
	movi	r2, readString
	call	interrupthandler_register
	
	movi	r1, 3
	movi	r2, readChar
	call	interrupthandler_register

	movi	r1, 4
	movi	r2, scr_scroll
	call	interrupthandler_register

	movi	r1, 5
	movi	r2, fillScreen
	call	interrupthandler_register

	movi	r1, 6
	movi	r2, setBgCol
	call	interrupthandler_register

	movi	r1, 7
	movi	r2, setFontCol
	call	interrupthandler_register

	movi	r1, 8
	movi	r2, editString
	call	interrupthandler_register

	movi	r1, 20
	movi	r2, interrupthandler_register
	call	interrupthandler_register

	movi	r1, 130
	movi	r2, taskEnd
	call	interrupthandler_register

	movi	r1, 131
	movi	r2, taskStart
	call	interrupthandler_register

	movi	r1, 132
	movi	r2, os_keyboard_input
	call	interrupthandler_register

	movi	r1, 133
	movi	r2, os_input_flush
	call	interrupthandler_register

	movi	r1, 256
	movi	r2, taskSwitch
	call	interrupthandler_register

	movi	r1, 258
	movi	r2, IllegalInstruction
	call	interrupthandler_register

	movi	r1, 262
	movi	r2, keyboard_interrupt
	call	interrupthandler_register

	movi	r1, 2
	wr	r1, r0, GLOBAL_KEYBOARD_STATUS

	;set main interrupt manager
	movi	r1, interrupt
	wr	r1, r0, GLOBAL_IADRESS

	;initialise interrupt sources
	;timer
	;each thread needs to run once per frame, as we start with one task, we have cycles_per_frame-300=12500 cycles
	;once more threads are spawned this number decreases
	movil	r1, 12500
	wr	r1, r0, GLOBAL_TIMER_MAX

	;enable interrupts
	movi	r1, 0x03
	wr	r1, r0, GLOBAL_IENABLE

	;jump to high level os loop
	call	osmain
	halt;if we get here we should probably not do anything else


;******************************************************************
;**************************Interrupt handler***********************
;******************************************************************


.data
__interruptsignored:
0
.text

__dummyhandler:
	rd	r1, r0, __interruptsignored
	addi	r1, r1, 1
	;wr	r1, r0, GLOBAL_NUMPAD_OUT
	wr	r1, r0, __interruptsignored
	jmpr	ra
	
;todo: the interrupt handler destroys the ex register, make sure to fix that
interrupt:

	;sanity check
	ble	r0, es, 2
	movi	r1, 1
	call	interr
	
	push	r1
	
	;check if we got a software exception
	movi	r1, 32
	ble	r1, es, 5
	rd	es, es, swint
	pop	r1
	mov	ra, ea
	mov	ea, es;have the software interrupt execute in the current thread
	eret
	
	push	ex
	;check if we got a system interrupt
	;update exception status for new interrupt space
	subi	es, es, 128
	;sanity check
	ble	r0, es, 3
	addi	es, es, 128
	movi	r1, 1
	call	interr

	;movi	r1, 32; not needed, r1 is already 32
	ble	r1, es, 5
	rd	es, es, sysint
	pop	ex
	pop	r1
	callr	es
	eret

	;check if we got a hardware interrupt
	;update exception status for new interrupt space
	subi	es, es, 128
	;sanity check
	bl	es, r0, 2
	movi	r1, 32
	bl	es, r1, 3

	addi	es, es, 256
	movi	r1, 1
	call	interr

	rd	es, es, hwint
	
	;calculate register store area
	rd	r1, r0, activeTask
	muli	r1, r1, 37

	;save all registers & etc
	wr	r2, r1, regarea+1
	wr	r3, r1, regarea+2
	wr	r4, r1, regarea+3
	wr	r5, r1, regarea+4
	wr	r6, r1, regarea+5
	wr	r7, r1, regarea+6
	wr	r8, r1, regarea+7
	wr	r9, r1, regarea+8
	wr	r10, r1, regarea+9
	wr	r11, r1, regarea+10
	wr	r12, r1, regarea+11
	wr	fp, r1, regarea+12
	wr	ra, r1, regarea+14
	wr	a0, r1, regarea+15
	wr	a1, r1, regarea+16
	wr	a2, r1, regarea+17
	wr	a3, r1, regarea+18
	wr	a4, r1, regarea+19
	wr	a5, r1, regarea+20
	wr	a6, r1, regarea+21
	wr	a7, r1, regarea+22
	wr	a8, r1, regarea+23
	wr	a9, r1, regarea+24
	wr	a10, r1, regarea+25
	wr	a11, r1, regarea+26
	wr	a12, r1, regarea+27
	pop	ex
	wr	ex, r1, regarea+28
	pop	r2;store r1
	wr	r2, r1, regarea+0
	wr	sp, r1, regarea+13
	;TODO: save floating point state

	;branch to interrupt handler
	callr	es

interrupt_restoreRegs:
	;restore all registers & etc
	;calculate register load area
	rd	r1, r0, activeTask
	muli	r1, r1, 37

	rd	r2, r1, regarea+1
	rd	r3, r1, regarea+2
	rd	r4, r1, regarea+3
	rd	r5, r1, regarea+4
	rd	r6, r1, regarea+5
	rd	r7, r1, regarea+6
	rd	r8, r1, regarea+7
	rd	r9, r1, regarea+8
	rd	r10, r1, regarea+9
	rd	r11, r1, regarea+10
	rd	r12, r1, regarea+11
	rd	fp, r1, regarea+12
	rd	sp, r1, regarea+13
	rd	ra, r1, regarea+14
	rd	a0, r1, regarea+15
	rd	a1, r1, regarea+16
	rd	a2, r1, regarea+17
	rd	a3, r1, regarea+18
	rd	a4, r1, regarea+19
	rd	a5, r1, regarea+20
	rd	a6, r1, regarea+21
	rd	a7, r1, regarea+22
	rd	a8, r1, regarea+23
	rd	a9, r1, regarea+24
	rd	a10, r1, regarea+25
	rd	a11, r1, regarea+26
	rd	a12, r1, regarea+27
	rd	ex, r1, regarea+28
	rd	r1, r1, regarea+0
	;return to interrupt point
	eret

;handler in r2
;interrupt ID in r1
interrupthandler_register:
	ble	r0, r1, 2
	movi	r1, 4
	call	interr
	
	movi	r3, 32
	ble	r3, r1, 2
	wr	r2, r1, swint
	jmpr	ra
	
	subi	r1, r1, 128
	ble	r0, r1, 2
	movi	r1, 4
	call	interr
	
	movi	r3, 32
	ble	r3, r1, 2
	wr	r2, r1, sysint
	jmpr	ra
	
	subi	r1, r1, 128
	ble	r0, r1, 2
	movi	r1, 4
	call	interr
	
	movi	r3, 32
	ble	r3, r1, 2
	wr	r2, r1, hwint
	jmpr	ra

	movi	r1, 4
	call	interr

.data
hwint:
#allocate 32
sysint:
#allocate 32
swint:
#allocate 32
.text

;****************************************************
;************Interrupt error handler*****************
;****************************************************
;Interrupt errors:
;0: Unspecific fault/wrong arguments to interr
;1: Invalid interrupt
;2: Major system fault
;3: Hardware error
;4: Invalid interrupt registered
;5: invalid instruction
;6: current task pointer corrupted
.data
intstr_errstrings:
intstr_errstr1
intstr_errstr2
intstr_errstr3
intstr_errstr4
intstr_errstr5
intstr_errstr6
intstr_errstr7
intstr_errstr1:
	#string "Interrupt error!"
intstr_errstr2:
	#string "Invalid interrupt!"
intstr_errstr3:
	#string "Major fault detected!"
intstr_errstr4:
	#string "Hardware error!"
intstr_errstr5:
	#string "Invalid int. ID registered!"
intstr_errstr6:
	#string "Invalid instruction!"
intstr_errstr7:
	#string "Invalid Task ID at switch!"
.text
;interrupt error
;prints some fancy error messages depending on some conditions
interr:
	wr	r0, r0, GLOBAL_IENABLE
	call	setFontCol
	ble	r0, r1, 2
	movi	r1, 0
	br	interr_proceed
	movi	r4, 7
	bl	r1, r4, interr_proceed
	movi	r1, 0
interr_proceed:
	wr	es, r0, GLOBAL_NUMPAD_OUT
	wr	ea, r0, GLOBAL_NUMPAD_OUT+1
	wr	ra, r0, GLOBAL_NUMPAD_OUT+2
	wr	sp, r0, GLOBAL_NUMPAD_OUT+3
	rd	r2, ea, -1
	wr	r2, r0, GLOBAL_NUMPAD_OUT+4
	rd	r1, r1, intstr_errstrings
	call	printStr

	halt;we cannot recover from this state

IllegalInstruction:
	movi	r1, 5
	br	interr

;*****************************************************
;**************Task management************************
;*****************************************************

;switches to the next active task
taskSwitch:
	rd	r1, r0, activeTask
	wr	ea, r1, taskExecPoint
	movi	r2, 8

	;perform a sanity check on task ID
	bl	r1, r0, 1;sanity check
	bl	r1, r2, taskLoop
	movi	r1, 6
	br	interr
taskLoop:
	addi	r1, r1, 1;get next task ID
	andi	r1, r1, 7;make sure r1 is in range 0-7
	
	rd	ea, r1, taskExecPoint
	beq	ea, r0, taskLoop;if there are no active tasks this is practically a 'halt' instruction.
	wr	r1, r0, activeTask
	;reset timer
	wr	r0, r0, GLOBAL_TIMER
	jmpr	ra

;ends the currently running task
taskEnd:
	rd	r1, r0, activeTask
	wr	r0, r1, taskExecPoint

	movi	ea, 0;so that this task does not get re-enabled in the task switch 
	call	taskSwitch

	;update the timer interrupt frequency
	rd	r1, r0, activeTasks
	subi	r1, r1, 1
	wr	r1, r0, activeTasks
	call	calcTimerLen
	wr	r1, r0, GLOBAL_TIMER_MAX

	br	interrupt_restoreRegs;restore the registers belonging to the new task


;task pointer in r1
taskStart:
	movi	r2, 0
	movi	r4, 8
taskStartLoop:	
	rd	r3, r2, taskExecPoint
	beq	r3, r0, taskStartLoopEnd
	addi	r2, r2, 1
	beq	r2, r4, taskStartError
	br	taskStartLoop
taskStartLoopEnd:
	wr	r1, r2, taskExecPoint
	rd	r1, r0, activeTasks
	addi	r1, r1, 1
	wr	r1, r0, activeTasks	
	
	push	ra
	call	calcTimerLen
	pop	ra
	wr	r1, r0, GLOBAL_TIMER_MAX

	movi	r1, 1
	jmpr	ra
taskStartError:
	movi	r1, 0
	jmpr	ra


;parameters:	number of active tasks in r1
;returns:	an appropriate number of cycles between each task switch in r1
calcTimerLen:
	movil	r2, CPU_CLOCKRATE
	beq	r1, r0, timerCalcDivBy0
	muli	r1, r1, 30;each task should run 30 times each second
	div	r2, r2, r1
	subi	r1, r2, 300;remove approximate time for task switch
	br	timerCalcEnd
timerCalcDivBy0:
	movil	r1, 12800
timerCalcEnd:
	jmpr	ra

.data
;allocate a memory location for each register that needs to be saved
;32 registers - 2 (es, r0) + 4*2 (fpu) * 8 (number of threads) = 296 (37 per thread)
regarea:
#allocate 296
;a value between 0 and 7
activeTask:
0
;execution point of all threads
taskExecPoint:
#allocate 8
activeTasks:
1




