# Anti-piracy in Pokémon Conquest (VPYP)

## Flow overlay 8

_Close to the start of the init / main function._

### To the overlay 8 AP jump function

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

### AP 0

TODO

## Flow overlay 9

_Everywhere in the game. Found calls before starting the main game loop, before
loading world map (overlay 0) or before starting a battle (overlay 2). Each time
the same flow repeats, looks like a macro or copy/pasted code._

### To the overlay 9 AP jump function

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

### AP 1

TODO

### AP 2

TODO

### AP 3

TODO

### AP 4

TODO

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
