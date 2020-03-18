# Game anti-piracy protection

The game software implements several mechanims to detect if it is running on
non-authorized hardware (i.e.: flashcards) or it has been modified.

## Game anti-piracy

Many games implement anti-piracy detection mechanims. This is the kind of
patches that flashcard try to apply to make it work.

- [ ] WIP: Research anti-piracy in Ninokuni
- [ ] Research anti-piracy in Metal Max 3
- [ ] Reserach anti-piracy in Pokémon

## Save editing prevention

Some games encrypt and sign the save files to prevent modifications. This is an
important security risk since many exploit have been found in save files.

- [ ] Document save exploits
      [(ref)](https://cturt.github.io/DS-exploit-finding.html)
- [ ] Document Ninokuni saves (see
      [ninokuni repo](https://github.com/GradienWords/Ninokuni))
- [ ] Document Pokemon Mystery Dungeon (see
      [twitter thread](https://twitter.com/pleonex/status/1015933593904992256?s=20))

## Modding protection

Some games encrypt or ofuscate files to make it harder to modify. This is
usually the case for files with text, image and font content.

- [ ] Document general encryption and obfuscation (see
      [airorom](https://github.com/pleonex/AiroRom))
- [ ] Document Pokémon (see [airorom](https://github.com/pleonex/AiroRom))
- [ ] Document Ninokuni (see [airorom](https://github.com/pleonex/AiroRom))

## Code signature

The developers may be signing the files with code to prevent modifications.

- [ ] Sending games over WiFi (download-play) (see
      [airorom](https://github.com/pleonex/AiroRom))
- [ ] Signing ARMx and overlays (see
      [airorom](https://github.com/pleonex/AiroRom))
