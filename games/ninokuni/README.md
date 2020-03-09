# Ni no kuni

The anti-piracy patch was created by _Rudolph_ and it is the following:

```plain
5400 - Ninokuni Shikkoku no Madoushi [68B796B1] Rudolph
00004200: E2 26 89 33 28 01 4D 39 BD DA EC C9 7C CD AB 7F → 00 00 9F E5 1E FF 2F E1 CF B3 00 00 00 00 9F E5
00004210: DC 75 B3 D1 E8 0B 68 3F 9A F4 3C 02 0D D2 AB DE → 1E FF 2F E1 77 B1 00 00 07 40 2D E9 1C 00 9F E5
00004220: 41 DD 29 28 FE C0 B7 19 DC B6 C6 32 7C 20 CB 0F → 1C 10 9F E5 00 20 91 E5 02 00 50 E1 14 00 9F 05
00004230: 7C 6C FF 7D EB 06 F6 56 79 79 D6 27 44 EB CE 5D → 00 00 81 05 0C 00 80 02 3C 00 81 05 07 80 BD E8
00004240: CF 1E 36 1B C6 EF 89 63 BF FF 8E 8A → E0 D7 15 02 4C C0 15 02 00 23 00 02
000049F8: 1E FF 2F E1 → 06 FE FF EA
```

The patch only affect to secure area of the ARM9. The diff in assembly is:

```asm
; New code in garbage section
0x02000200      ldr     r0, [pc]   ; 0x02000208
0x02000204      bx      lr
0x02000208      .dword 0x0000b3cf

0x0200020c      ldr     r0, [pc]   ; 0x0200014
0x02000210      bx      lr
0x02000214      .dword 0x0000b177

0x02000218      push    {r0, r1, r2, lr}
0x0200021c      ldr     r0, [0x02000240]
0x02000220      ldr     r1, [0x02000244]
0x02000224      ldr     r2, [r1]
0x02000228      cmp     r0, r2
0x0200022c      ldreq   r0, [0x02000248]
0x02000230      streq   r0, [r1]
0x02000234      addeq   r0, r0, 0xc
0x02000238      streq   r0, [r1, 0x3c]
0x0200023c      pop     {r0, r1, r2, pc}
0x02000240      .dword 0x0215d7e0
0x02000244      .dword 0x0215c04c
0x02000248      .dword 0x02002300

; Change the return in function 0x950 to jump to our new defined function
- 0x020009f8      b       lr
+ 0x020009f8      b       0x02000218
```

The function at 0x950 that introduces the jump is the function that decompress
the code files (ARM9 + overlays). This is a good way to hook logic every time a
new code file is loaded.

What the new function is doing is to check if at specific address of the new
code file there is a constant, and if so, replace it. The address we replace in
the code file at 0x0215c04c was originally 0x0215d7e0 and we replace it to
0x02002300. By setting a read breakpoint we find that this constant is read
here:

```arm

```

This explain why we couldn't find any reference to the other first functions,
because they get the constant and substract 0x2100 to get the final function
pointer. Nice trick to make crazy the debuggers (although radare2 can detect it
thanks to some basic emulation capabilities).

However we can't find that code in any overlay. So I set a breakpoint to know
when this code is copied and from where. The answer is amazing: it's decrypted:

```arm
RAM:0215B468 ap_decrypt                              ; CODE XREF: sub_215A750+8j
RAM:0215B468                                         ; RAM:0215AC64j ...
RAM:0215B468 STMFD   SP!, {R4-R6,LR}
RAM:0215B46C MOV     R1, #0
RAM:0215B470 SUB     R2, R0, #0xC
RAM:0215B474 LDR     LR, =0x7FEC9DF1
RAM:0215B478 LDR     R12, =dword_215C1CC
RAM:0215B47C STR     R1, [R2,#8]
RAM:0215B480 STR     R1, [R2,#4]
RAM:0215B484 STR     R1, [R0,#-0xC]
RAM:0215B488 MOV     R2, R1
RAM:0215B48C
RAM:0215B48C loc_215B48C                             ; CODE XREF: ap_decrypt+74j
RAM:0215B48C LDR     R1, [R0,#4]
RAM:0215B490 LDR     R3, [R0]
RAM:0215B494 SUB     R1, R1, R12
RAM:0215B498 SUB     R1, R1, #0x2100
RAM:0215B49C SUB     R4, R3, #0x2100
RAM:0215B4A0 MOV     R1, R1,LSR#2
RAM:0215B4A4 MOV     R6, LR
RAM:0215B4A8 ADD     R5, R4, R1,LSL#2
RAM:0215B4AC B       loc_215B4C4
RAM:0215B4B0 ; ---------------------------------------------------------------------------
RAM:0215B4B0
RAM:0215B4B0 loc_215B4B0                             ; CODE XREF: ap_decrypt+60j
RAM:0215B4B0 LDR     R1, [R4]
RAM:0215B4B4 EOR     R3, R1, R6
RAM:0215B4B8 SUB     R1, R3, R3,LSR#8
RAM:0215B4BC STR     R3, [R4],#4
RAM:0215B4C0 EOR     R6, R6, R1
RAM:0215B4C4
RAM:0215B4C4 loc_215B4C4                             ; CODE XREF: ap_decrypt+44j
RAM:0215B4C4                                         ; DATA XREF: RAM:0215BDE4w
RAM:0215B4C4 CMP     R4, R5
RAM:0215B4C8 BCC     loc_215B4B0
RAM:0215B4CC STR     R2, [R0,#4]
RAM:0215B4D0 STR     R2, [R0]
RAM:0215B4D4 LDR     R1, [R0,#8]!
RAM:0215B4D8 CMP     R1, #0
RAM:0215B4DC BNE     loc_215B48C
RAM:0215B4E0 MOV     R12, #0
RAM:0215B4E4 MOV     R1, #0
RAM:0215B4E8
RAM:0215B4E8 loc_215B4E8                             ; CODE XREF: ap_decrypt+A4
RAM:0215B4E8 MOV     R0, #0
RAM:0215B4EC
RAM:0215B4EC loc_215B4EC                             ; CODE XREF: ap_decrypt+98
RAM:0215B4EC ORR     R2, R1, R0
RAM:0215B4F0 MCR     p15, 0, R12,c7,c10, 4
RAM:0215B4F4 MCR     p15, 0, R2,c7,c14, 2
RAM:0215B4F8 ADD     R0, R0, #0x20
RAM:0215B4FC CMP     R0, #0x400
RAM:0215B500 BLT     loc_215B4EC
RAM:0215B504 ADD     R1, R1, #0x40000000
RAM:0215B508 CMP     R1, #0
RAM:0215B50C BNE     loc_215B4E8
RAM:0215B510 MOV     R0, #0
RAM:0215B514 MCR     p15, 0, R0,c7,c5, 0
RAM:0215B518 MCR     p15, 0, R12,c7,c10, 4
RAM:0215B51C LDMFD   SP!, {R4-R6,PC}
RAM:0215B51C ; End of function ap_decrypt
RAM:0215B51C
RAM:0215B51C ; ---------------------------------------------------------------------------
RAM:0215B520 dword_215B520 DCD 0x7FEC9DF1            ; DATA XREF: ap_decrypt+C
RAM:0215B524 off_215B524 DCD dword_215C1CC           ; DATA XREF: ap_decrypt+10
```

This code is in the overlay9_19.

## ARM7 things

- 0x02FFFC3C: it writes a frame counter. It's a shared area (firmware) so ARM9
  can access.

## Function naming convention

```plain
<file><flag>_<name>
file: ov<num> (we assume always from arm9). For ap replace ov for ap
flag:
  - h: hidden. Code anti-debuggers
  - d: autodestroy, after running the code it's removed (zero'ed)
  - e: encrypted, repeat per layer
```

## ROM operations

```plain
Structure:
00 = next node in the linked list
04 =
08 = struct wtih start of linked list at +8 (0x0206E8A0)
0C = init to 0x2300. If bit0 set, throw exception. OR with arg1 (op) in bit8-bit15. Bit3 set if arg2 is not zero
10 = ptr to payload
14 = 6 when 08 is zero and return. 2 otherwise for now

Hard-coded ptr:
00 =
04 =
08 = if bit7 is set, skip things.
0C =
10 =
14 =
18 =
1C =

Operations:
7 | 1 = payload: 0xFFFF, input ptr, start + end ptr, 0
```
