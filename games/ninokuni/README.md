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

## Onion obfuscation

1. ap_start_boot1
2. - ap19he_run1_call_lv1
3. ap19hee_run1_lv1
4. - ap19he_run1_call_lv2_1
5. ap19hee_run1_lv2_1

## Encrypted entrypoints

- [0x0215C014, 0x0215C038) - 0x0024
- [0x0215C050, 0x0215C074) - 0x0024
- [0x0215C08C, 0x0215C0B0) - 0x0024
- [0x0215C0C8, 0x0215C0EC) - 0x0024
- [0x0215B920, 0x0215B9B0) - 0x0090
- [0x0215B5F8, 0x0215B6D4) - 0x00DC
- [0x0215B528, 0x0215B5F0) - 0x00C8
- [0x0215B6E0, 0x0215B748) - 0x0068
- [0x0215A660, 0x0215A684) - 0x0024
- [0x0215A69C, 0x0215A6C0) - 0x0024
- [0x0215A6D8, 0x0215A6FC) - 0x0024
- [0x0215A714, 0x0215A738) - 0x0024
- [0x0215A7C0, 0x0215A844) - 0x0084
- [0x0215A8A4, 0x0215A9C4) - 0x0120
- [0x0215A9C8, 0x0215AB64) - 0x019C
- [0x0215AB6C, 0x0215ABE0) - 0x0074
- [0x0215ABE4, 0x0215AC58) - 0x0074
- [0x0215A84C, 0x0215A8A4) - 0x0058
- [0x0215B310, 0x0215B334) - 0x0024
- [0x0215B34C, 0x0215B370) - 0x0024
- [0x0215B388, 0x0215B3AC) - 0x0024
- [0x0215B3C4, 0x0215B3E8) - 0x0024

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
