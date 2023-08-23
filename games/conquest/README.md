# Anti-piracy in Pokémon Conquest (VPYP)

It looks like overlay 8 contains the AP for DS checks, adapted for DSi (with the
new jump functions). Overlay 9 has new AP code for DSi.

## Flow summary

Before any call, run `DecryptGroups` to run the XOR decryption over all the AP
infrastructure code: jumper functions and RC4N algorithm.

To call any of the end AP functions the flow is:

1. Jumper function:
   1. In its data pool there is the AP address, size.
   2. The key seed is generated from the checksum fo `DecryptRunEncrypt`
   3. Call `DecryptRunEncrypt`
      1. Decrypt AP code with RC4N
      2. Run AP code -- it may call other AP repeating the flow
      3. Encrypt AP code with RC4N

## DS AP

_Close to the start of the init / main function._

### DS - AP initialization

1. Call dedicated function `0x02071be4` from arm9.
   1. Load overlay 8
      1. Load overlay into memory
      2. Decompress overlay
      3. Static constructors call the 5 functions `DecryptGroupX`
         1. Each of them call `DecryptCodeXOR` that decrypts a set of functions
   2. Validate checksum of (_XOR encrypted_) jump function
      1. Call the AP jumper function
      2. If fails, call `0x02215c38`
   3. Free the overlay
   4. Do initialization stuff with a structure passed into this function

### DS - AP flow

1. Call AP jumper method
   1. De-obfuscate address of next call `DecryptRunEncrypt`
   2. Do checksum of next function for the seed of the encrypted AP code.
   3. Save return address in data pool as well
   4. Call `DecryptRunEncrypt`
      1. Decrypt target AP with RC4N
      2. Run target AP with arguments
         1. Call AP check 0 via jumper (XORed)
            1. Call `DecryptRunEncrypt` for AP check 0
         2. Call AP check 1 via jumper (XORed)
            1. Call `DecryptRunEncrypt` for AP check 0
         3. Call AP check 2 via jumper (XORed)
            1. Call `DecryptRunEncrypt` for AP check 0
         4. Call AP check 3 via jumper (XORed)
            1. Call `DecryptRunEncrypt` for AP check 0
      3. Encrypt target AP again
      4. Return value from target AP

### DS - AP main

It does a challenge by calling 4 AP checks. Each check returns a value and it
sums all of them, plus the initial value `0x30EB87`. At the end (`0x338F75`),
multiply by `0x10FEF011` (division?), take the upper part and divide by 16. Then
multiply by `0xF1` and subtract the result with the end value. It must be 0.

It also verifies the checksum of each jump function. If it fails call same
function as the main function (`02215C38`).

#### DS - AP 0

Check byte by byte jump function for AP 1. If bytes are different, return
`0xA99F`, if not `0xA2DD`.

#### DS - AP 1

It looks like the MAC and firmware check of _no$gba_ emulator. Returns
`0xF1 * 0xBF` on success, `0xFB * 0xBF` if fails.

#### DS - AP 2

TODO: same as Ninokuni AP2.

#### DS - AP 3

Check byte by byte jump function for AP 2. If bytes are different, return
`0xA99F`, if not `0xA2DD`.

## DSi AP

_Everywhere in the game. Found calls before starting the main game loop, before
loading world map (overlay 0) or before starting a battle (overlay 2). Each time
the same flow repeats, looks like a macro or copy/pasted code._

### DSi - AP initialization

1. Free overlay 7
2. Load overlay 9
   1. This overlay has no static initializers.
3. Call main function to decrypt all AP code `DecryptGroupsAP`
   1. It calls 4 functions `DecryptGroupX` and itself is one of them at the end.
      1. Each of them call `DecryptCodeXOR` (from overlay 9).
4. Run 4 different AP functions by:
   1. Verify checksum of (_XOR encrypted_) jump function
      1. Call the AP jumper function.
      2. If fails:
         1. Load `0x02218960`
         2. Flip bit0 (XOR)
         3. Call the function at `0x02218964 + (bit0 * 4)` with arg `0`. The
            first function is `0x020A90DC`, second is `0x020A909C`.

### DSi - AP flow

### AP 1

TODO

### AP 2

TODO

### AP 3

TODO

### AP 4

TODO

## Infrastructure functions

The following functions are involved in the anti-piracy checks in order or call:

- Main group XOR decrypt
- Group XOR decrypt
- XOR decrypt
- [XORed] Jump function
- [XORed] RC4N - Decrypt-Run-Encrypt
- [XORed] RC4N - DecryptWrapper
- [XORed] RC4N - Decrypt
- [XORed] RC4N - KSA
- [XORed] RC4N - DoDecrypt
- [XORed] RC4N - GetMode
- [XORed] RC4N - PRGA NextRandom
- [XORed] RC4N - EncryptWrapper
- [XORed] RC4N - Encrypt
- [XORed] RC4N - DoEncrypt
- [XORed] Compute CRC32

### AP jump checksum

This one is different from the one found in
[Ninokuni](../ninokuni/README.md#jump-function).

The area to check is `0x25 * 4`. The initial value is `0`. For each 32-bits
value:

1. `a = (0x20 - remaining) & 0xFF`
2. `b = input << a`
3. `c = input >> remaining`
4. `d = b | c`
5. `checksum ^= d`

> [!NOTE]  
> In C# shifting operations with a value bigger or equal to 32 is not working as
> on an ARM CPU. C# will do a mask to the first 5 bits (`& 0x1f`), while on the
> CPU it will make it 0.

## How to find AP

- Finding the XOR decryption function is easy (same between DS / DSi)
- Breakpoint to the XOR decryption to get started
- Jump calls to XOR are also similar but some overlays don't have the main
- Checkout the static initializers from the overlay with XOR decryption
- Find function `LoadOverlay` and static analyze all calls that loads overlays
  with the XOR decryption.
- Pokémon Conquest:
  - It passes to the AP of overlay 9 a random number, tracking the calls to
    `GetRandom` also works.

### Update decrypted once overlay AP is found

1. Get key seed, address offset and size offset from XOR decryption.
2. Get the 5 entrypoint functions
   - From overlay static initializer
   - Finding main entrypoint (tracking from breakpoint)
3. Get the jump functions and their expected checksum
   - Verify checksum
   - Get key seed and address for RC4N encrypted code
