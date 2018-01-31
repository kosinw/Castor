# Castor

Castor is a Gameboy interpreter written using C# and Monogame. The primary purpose of this project is to learn about emulator systems and low-level operations.

# Implemented Features
* partial CPU decoding
* background rendering
* working ui using wpf
* oam dma support
* functional bootrom
* window rendering
* input
* interrupt support

# Unimplemented Features
* complete CPU decoding
* any type of testing
* timers
* cartridge types
* oam support (sprites)
* audio

# Resources used:
* [The Ultimate Game Boy Talk](https://www.youtube.com/watch?v=HyzD8pNlpwI&t=2247s) - A talk attempting to communicate "everything about the Game Boy".
* [Pan Docs](http://bgb.bircd.org/pandocs.htm) - Everything you wanted to know about the GAMEBOY, but were afraid to ask.
* [The Cycle Accurate Game Boy Document](https://github.com/AntonioND/giibiiadvance/blob/master/docs/TCAGBD.pdf) - A cycle-accurate documentation on the Gameboy's inner workings like
* [Gameboy CPU instruction set](http://pastraiser.com/cpu/gameboy/gameboy_opcodes.html) - A table of all of the legal gameboy opcodes.
* [Gameboy Bootstrap ROM](http://gbdev.gg8.se/wiki/articles/Gameboy_Bootstrap_ROM) - A dissassembly of the Gameboy's boot rom.
* [Mooneye GB](https://github.com/Gekkio/mooneye-gb) - A Game Boy research project and emulator written in Rust.
* [Emulation accuracy](https://github.com/Gekkio/mooneye-gb/blob/master/docs/accuracy.markdown) - A markdown talking about questions about certain specifics of the gameboy.
* [GameBoy CPU Manual](http://marc.rawer.de/Gameboy/Docs/GBCPUman.pdf) - Assembly Language Commands, Timings and Opcodes, and everything you always wanted to know about GB but were afraid to ask.
