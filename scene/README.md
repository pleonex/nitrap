# Scene

Third-parties have created different mechanisms to by-pass the original
anti-piracy mechanisms: flaschards. However, they introduce their own
_anti-piracy_ mechanisms to prevent cheap-copies of their flashcards.

## Flashcard timebombs

Some flashcards contain timebombs to force users to buy a new one.

- [ ] Investigate timebomb in M3 Real
- [ ] Document missing updates in AP DB

## Flashcard anti-updates protection

Some flashcards encrypt and obfuscate files to avoid modifications.

- [ ] Investigate software entry-points in YSMenu
- [ ] Investigate encryption of AP database

## Flashcard anti-piracy patches

Most flashcards needs to do live-patching of games to remove the anti-piracy
code. However, these database are encrypted.

- [ ] Investigate format of anti-piracy database
- [ ] Investigate format of cheat database
- [ ] Investigate live-patching auto-detection of DSTT
- [ ] Investigate anti-piracy detection of _infolib custom kit_
  - [ ] Compare current DB with patches auto-generated with these tools

## Scene flashcard updater anti-updates protection

There is a free tool to download the latest game patches. However, it is unknown
from where it gets the patch database (I cannot find information of the people
making the patches) and this file is encrypted.

- [x] Document DS-Scene ROM Tool program
- [x] Find URL to download files
- [x] Find encryption / decryption algorithms
- [ ] Write programs to parse the DB
- [x] Document other files of interest
- [ ] Authors and community behind anti-piracy patches
