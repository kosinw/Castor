using Castor.Emulator.Memory;
using Castor.Emulator.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Castor.Emulator.CPU
{
    public partial class Z80
    {
        #region Registers       
        public byte A;
        public byte F;
        public byte B;
        public byte C;
        public byte D;
        public byte E;
        public byte H;
        public byte L;

        public ushort AF
        {
            get => Convert.ToUInt16(A << 8 | F);
            set
            {
                A = value.MostSignificantByte();
                F = value.LeastSignificantByte();
            }
        }
        public ushort BC
        {
            get => Convert.ToUInt16(B << 8 | C);
            set
            {
                B = value.MostSignificantByte();
                C = value.LeastSignificantByte();
            }
        }
        public ushort DE
        {
            get => Convert.ToUInt16(D << 8 | E);
            set
            {
                D = value.MostSignificantByte();
                E = value.LeastSignificantByte();
            }
        }
        public ushort HL
        {
            get => Convert.ToUInt16(H << 8 | L);
            set
            {
                H = value.MostSignificantByte();
                L = value.LeastSignificantByte();
            }
        }

        #region Pointer Accessors
        public byte AddrHL
        {
            get
            {
                _waitcycles += 4;
                return _system.MMU[HL];
            }
            set
            {
                _waitcycles += 4;
                _system.MMU[HL] = value;
            }
        }
        public byte AddrHLI
        {
            get
            {
                _waitcycles += 4;
                return _system.MMU[HL++];
            }
            set
            {
                _waitcycles += 4;
                _system.MMU[HL++] = value;
            }
        }
        public byte AddrHLD
        {
            get
            {
                _waitcycles += 4;
                return _system.MMU[HL--];
            }
            set
            {
                _waitcycles += 4;
                _system.MMU[HL--] = value;
            }
        }
        public byte AddrC
        {
            get
            {
                _waitcycles += 4;
                return _system.MMU[0xFF00 + C];
            }

            set
            {
                _waitcycles += 4;
                _system.MMU[0xFF00 + C] = value;
            }
        }
        public byte AddrBC
        {
            get
            {
                _waitcycles += 4;
                return _system.MMU[BC];
            }
            set
            {
                _waitcycles += 4;
                _system.MMU[BC] = value;
            }
        }
        public byte AddrDE
        {
            get
            {
                _waitcycles += 4;
                return _system.MMU[DE];
            }
            set
            {
                _waitcycles += 4;
                _system.MMU[DE] = value;
            }
        }
        public byte Addr8
        {
            get
            {
                _waitcycles += 4;
                return _system.MMU[0xFF00 + ReadByte(PC)];
            }

            set
            {
                _waitcycles += 4;
                _system.MMU[0xFF00 + ReadByte(PC)] = value;
            }
        }
        public byte Addr16
        {
            get
            {
                _waitcycles += 4;
                return _system.MMU[ReadUshort(PC)];
            }

            set
            {
                _waitcycles += 4;
                _system.MMU[ReadUshort(PC)] = value;
            }
        }
        public ushort AccessorSP
        {
            get
            {
                _waitcycles += 4;
                return SP;
            }

            set
            {
                _waitcycles += 4;
                SP = value;
            }
        }
        #endregion

        public ushort SP;
        public ushort PC;
        #endregion

        #region Private Members
        /// <summary>
        /// This counts how many cycles need to be waited until the next instruction is executed.
        /// </summary>
        private int _waitcycles = 0;

        /// <summary>
        /// This is a reference to the overarching class of all components
        /// </summary>
        private GameboySystem _system { get; set; }

        /// <summary>
        /// This is the master interrupt enable. If this isn't enabled, then no interrupts will occur.
        /// </summary>
        private bool _ime = false;

        ///<summary>
        /// This holds the state of the next behavior of the IME flag.
        ///<summary>
        private InterruptToggle _setime;

        /// <summary>
        /// This holds a list of instructions and their addreses.
        /// </summary>
        private List<(int addr, byte inst)> _disasm;

        private enum InterruptToggle
        {
            None,
            DisableInterrupt,
            EnableInterrupt,
            EnableInterruptSoon
        }

        /// <summary>
        /// This is the halted flag. If this is enabled, all program activity will stop until interrupts.
        /// </summary>
        private bool _halted = false;
        #endregion

        public void AddWaitCycles(int cycles) => _waitcycles += cycles;

        public delegate void Instruction();
        private Instruction[] _op = new Instruction[256];
        private Instruction[] _cb = new Instruction[256];

        public Z80(GameboySystem system)
        {
            _system = system;

            #region Opcode Mappings
            _op = new Instruction[256];
            _disasm = new List<(int addr, byte inst)>();

#if DEBUG
            _op = Enumerable.Repeat<Instruction>(
                delegate
                {
                    throw new Exception($"Instruction (0x{_system.MMU[PC - 1]:X2}) " +
                        $"not implemented! " +
                        $"PC = (0x{PC - 1:X4})");

                }, 256).ToArray();

            _cb = Enumerable.Repeat<Instruction>(
                () =>
                {
                    throw new Exception($"Instruction (0xCB 0x{_system.MMU[PC - 1]:X2}) " +
                        $"not implemented! " +
                        $"PC = (0x{PC - 2:X4})");

                }, 256).ToArray();
#endif            
            PopulateLoadInstructions();             // LD, LDH, LDD, LDI, PUSH, POP
            PopulateControlFunctions();             // NOP, STOP, EI, DI, HALT, PREFIX CB
            PopulateJumpFunctions();                // JP, JR, RST, CALL, RET, RETI
            PopulateALUInstructions();              // INC, DEC, CPL, CCF, DAA, SCF, AND, XOR, OR, ADD, SUB, CP, ADC, SBC
            PopulateBitwiseInstructions();          // LOTS OK

            _op[0x05] = delegate            // DEC B
            {
                B--;

                SetFlag(B == 0, StatusFlags.Z);
                SetFlag(true, StatusFlags.N);
                SetFlag(B % 16 == 0, StatusFlags.H);
            };
            _op[0x0B] = delegate            // DEC BC
            {
                BC--;

                _waitcycles += 4;
            };
            _op[0x0D] = delegate            // DEC C
            {
                C--;

                SetFlag(C == 0, StatusFlags.Z);
                SetFlag(true, StatusFlags.N);
                SetFlag(C % 16 == 0, StatusFlags.H);
            };
            _op[0x15] = delegate             // DEC D
            {
                D--;

                SetFlag(D == 0, StatusFlags.Z);
                SetFlag(true, StatusFlags.N);
                SetFlag(D % 16 == 0, StatusFlags.H);
            };
            _op[0x17] = delegate             // RLA
            {
                Utility.Math.RotateLeft(ref A, ref F);

                SetFlag(false, StatusFlags.Z);
                SetFlag(false, StatusFlags.N);
                SetFlag(false, StatusFlags.H);
            };
            _op[0x1D] = delegate            // DEC E
            {
                E--;

                SetFlag(E == 0, StatusFlags.Z);
                SetFlag(true, StatusFlags.N);
                SetFlag(E % 16 == 0, StatusFlags.H);
            };
            _op[0x3D] = delegate            // DEC A
            {
                A--;

                SetFlag(A == 0, StatusFlags.Z);
                SetFlag(true, StatusFlags.N);
                SetFlag(A % 16 == 0, StatusFlags.H);
            };
            _op[0x90] = delegate            // SUB B
            {
                SetFlag(A == B, StatusFlags.Z);
                SetFlag(true, StatusFlags.N);
                SetFlag((B & 0xF) > (A & 0xF), StatusFlags.H);
                SetFlag(B > A, StatusFlags.C);

                A -= B;
            };
            _op[0xB0] = delegate            // OR B
            {
                A = (byte)(A | B);

                SetFlag(A == 0, StatusFlags.Z);
                SetFlag(false, StatusFlags.N);
                SetFlag(false, StatusFlags.H);
                SetFlag(false, StatusFlags.C);
            };
            _op[0xB1] = delegate            // OR C
            {
                A = (byte)(A | C);

                SetFlag(A == 0, StatusFlags.Z);
                SetFlag(false, StatusFlags.N);
                SetFlag(false, StatusFlags.H);
                SetFlag(false, StatusFlags.C);
            };
            _op[0xBE] = delegate            // CP (HL)
            {
                byte d8 = _system.MMU[HL];

                SetFlag(d8 == A, StatusFlags.Z);
                SetFlag(true, StatusFlags.N);
                SetFlag((d8 & 0xF) > (A & 0xF), StatusFlags.H);
                SetFlag(d8 > A, StatusFlags.C);

                _waitcycles += 4;
            };
            _op[0xFE] = delegate            // CP d8
            {
                byte d8 = ReadByte(PC);

                SetFlag(d8 == A, StatusFlags.Z);
                SetFlag(true, StatusFlags.N);
                SetFlag((d8 & 0xF) > (A & 0xF), StatusFlags.H);
                SetFlag(d8 > A, StatusFlags.C);
            };

            _cb[0x11] = delegate     // RL C
            {
                Utility.Math.RotateLeft(ref C, ref F);

                SetFlag(C == 0, StatusFlags.Z);
                SetFlag(false, StatusFlags.N);
                SetFlag(false, StatusFlags.H);
            };
            _cb[0x7C] = delegate     // BIT 7,H
            {
                int result = H.BitValue(7);

                SetFlag(result == 0, StatusFlags.Z);
                SetFlag(false, StatusFlags.N);
                SetFlag(true, StatusFlags.H);
            };
            #endregion
        }

        public int Step()
        { 
            _waitcycles = 0;

            if (_system.ISR.CanServiceInterrupts && _halted) // if stop or halt, unhalt if interrupt
            {
                _halted = false;
            }

            if (_ime == true) // check for interrupt stuff only if IME is enabled
            {
                if (_system.ISR.CanHandleInterrupt(InterruptFlags.VBlank))
                    ServiceInterurpt(InterruptFlags.VBlank, 0x40);
                else if (_system.ISR.CanHandleInterrupt(InterruptFlags.LCDStat))
                    ServiceInterurpt(InterruptFlags.LCDStat, 0x48);
                else if (_system.ISR.CanHandleInterrupt(InterruptFlags.Timer))
                    ServiceInterurpt(InterruptFlags.Timer, 0x50);
                else if (_system.ISR.CanHandleInterrupt(InterruptFlags.Serial))
                    ServiceInterurpt(InterruptFlags.Serial, 0x58);
                else if (_system.ISR.CanHandleInterrupt(InterruptFlags.Joypad))
                    ServiceInterurpt(InterruptFlags.Joypad, 0x60);

                if (_system.ISR.CanServiceInterrupts) // return before any opcode decoding
                    return _waitcycles;
            }

            if (!_halted)
            {
                _disasm.Add((PC, _system.MMU[PC]));

                Instruction operation = _op[DecodeOpcode()];               
                operation.Invoke();

                switch (_setime)
                {
                    case InterruptToggle.DisableInterrupt:
                        _ime = false;
                        _setime = InterruptToggle.None;
                        break;
                    case InterruptToggle.EnableInterruptSoon:
                        _setime = InterruptToggle.EnableInterrupt;
                        break;
                    case InterruptToggle.EnableInterrupt:
                        _ime = true;
                        _setime = InterruptToggle.None;
                        break;
                }
            }
            else
            {
                InternalDelay(1);
            }

            return _waitcycles;
        }

        private void ServiceInterurpt(InterruptFlags flag, ushort jumpVector)
        {
            _ime = false; // disable ime flag
            _system.ISR.DisableInterrupt(flag); // clear whatever interrupt

            InternalDelay(3); // have an internal delay of 3

            PushUshort(PC); // push current pc onto stack

            PC = jumpVector;
        }

        private byte DecodeOpcode() => _system.MMU.ReadByte(PC++, ref _waitcycles);

        private byte ReadByte(int addr)
        {
            return _system.MMU.ReadByte(PC++, ref _waitcycles);
        }

        private void WriteByte(int addr, byte value)
        {
            _system.MMU.WriteByte(addr, value, ref _waitcycles);
        }

        private ushort ReadUshort(int addr)
        {
            ushort ret = _system.MMU.ReadWord(addr, ref _waitcycles);
            PC += 2;
            return ret;
        }

        private void WriteUshort(int addr, ushort value)
        {
            _system.MMU.WriteWord(addr, value, ref _waitcycles);
        }

        private void InternalDelay(int count) => AddWaitCycles(4 * count);

        private void SetFlag(bool condition, StatusFlags flag)
        {
            if (condition)
                F |= (byte)flag;
            else
                F &= (byte)~flag;
        }

        private bool CheckFlag(StatusFlags flag)
        {
            return (F & (byte)flag) == (byte)flag;
        }
    }
}
