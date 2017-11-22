# Castor

Castor is a Gameboy project written using .NET Framework, C#, and WPF. The primary purpose of this project is to learn about emulator systems and low-level operations.

# Implemented Features
* all load/move instructions
* all jump instructions
* all control instructions
* background rendering
* working ui using wpf
* oam dma support
* enough instructions for boot rom to complete
* interrupts

# Unimplemented Features
* refactoring of dma, interrupts, and opcodes
* all alu instructions
* all bitwise instructions
* input
* timers
* other mbcs besides mbc0
* oam support (sprites)
* window rendering support
* audio

# Resources used:
* [The Ultimate Game Boy Talk](https://www.youtube.com/watch?v=HyzD8pNlpwI&t=2247s) - A talk attempting to communicate "everything about the Game Boy".
* [Pan Docs](http://bgb.bircd.org/pandocs.htm) - Everything you wanted to know about the GAMEBOY, but were afraid to ask.
* [The Cycle Accurate Game Boy Document](https://github.com/AntonioND/giibiiadvance/blob/master/docs/TCAGBD.pdf) - A cycle-accurate documentation on the Gameboy's inner workings like
* [Gameboy CPU instruction set](http://pastraiser.com/cpu/gameboy/gameboy_opcodes.html) - A table of all of the legal gameboy opcodes.
* [Gameboy Bootstrap ROM](http://gbdev.gg8.se/wiki/articles/Gameboy_Bootstrap_ROM) - A dissassembly of the Gameboy's boot rom.
* [Mooneye GB](https://github.com/Gekkio/mooneye-gb) - A Game Boy research project and emulator written in Rust.
* [Emulation accuracy](https://github.com/Gekkio/mooneye-gb/blob/master/docs/accuracy.markdown) - A markdown talking about questions about certain specifics of the gameboy.
