# In-device protection

The Nintendo DS console has some mechanisms to protect against anti-piracy and
hacking.

## Game cardtrigde encryption

The game (save file too?) is encrypted. Trying to do a raw dump is useless if
the encryption protocol and keys are not known. There is a full protocol that
the Nintendo DS and the chip in the cardtrigde follow to decrypt the game
content.

- [ ] Document encryption protocol (see gbatek)
- [ ] Create rom dumper
- [ ] Create save dumper

## ROM format protection

The file format of for ROM contains different strategy to prevent modification
and distribution.

- [ ] Document checksums
- [ ] Document Nintendo logo
      [(ref)](http://pleonet.blogspot.com/2013/08/logo-de-nintendo-en-gba-y-nds.html)

## Firmware checks

The Nintendo DS does not have an OS but it has a firmware that works to
configure some settings (like the clock) and to launch the games. This firmware
runs some checks before lunching a game.

- [ ] Research how to dump firware
- [ ] Dump firmware (check DSLinux)
- [ ] Analyze firmware
- [ ] Check if something in ARM9 and ARM7 BIOS
  - Check registry
    [BIOSPROT](http://problemkaputt.de/gbatek.htm#dsmemorycontrolbios)
  - [biosdumper](http://www.cryptosystem.org/archives/2007/02/libfatdldi-enabled-ds-bios-dumper/)
