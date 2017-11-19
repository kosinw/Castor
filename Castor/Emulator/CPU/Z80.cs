using Castor.Emulator.Memory;
using Castor.Emulator.Utility;
using System;
using System.Linq;

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
                _cyclesToWait += 4;
                return _system.MMU[HL];
            }
            set
            {
                _cyclesToWait += 4;
                _system.MMU[HL] = value;
            }
        }
        public byte AddrHLInc
        {
            get
            {
                _cyclesToWait += 4;
                return _system.MMU[HL++];
            }
            set
            {
                _cyclesToWait += 4;
                _system.MMU[HL++] = value;
            }
        }
        public byte AddrHLDec
        {
            get
            {
                _cyclesToWait += 4;
                return _system.MMU[HL--];
            }
            set
            {
                _cyclesToWait += 4;
                _system.MMU[HL--] = value;
            }
        }
        public byte AddrC
        {
            get
            {
                _cyclesToWait += 4;
                return _system.MMU[0xFF00 + C];
            }

            set
            {
                _cyclesToWait += 4;
                _system.MMU[0xFF00 + C] = value;
            }
        }
        public byte AddrBC
        {
            get
            {
                _cyclesToWait += 4;
                return _system.MMU[BC];
            }
            set
            {
                _cyclesToWait += 4;
                _system.MMU[BC] = value;
            }
        }
        public byte AddrDE
        {
            get
            {
                _cyclesToWait += 4;
                return _system.MMU[DE];
            }
            set
            {
                _cyclesToWait += 4;
                _system.MMU[DE] = value;
            }
        }
        public byte Addr8
        {
            get
            {
                _cyclesToWait += 4;
                return _system.MMU[0xFF00 + ReadByte(PC)];
            }

            set
            {
                _cyclesToWait += 4;
                _system.MMU[0xFF00 + ReadByte(PC)] = value;
            }
        }
        public byte Addr16
        {
            get
            {
                _cyclesToWait += 4;
                return _system.MMU[ReadUshort(PC)];
            }

            set
            {
                _cyclesToWait += 4;
                _system.MMU[ReadUshort(PC)] = value;
            }
        }
        public ushort AccessorSP
        {
            get
            {
                _cyclesToWait += 4;
                return AccessorSP;
            }

            set
            {
                _cyclesToWait += 4;
                AccessorSP = value;
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
        private int _cyclesToWait = 0;

        /// <summary>
        /// This is a reference to the overarching class of all components
        /// </summary>
        private GameboySystem _system { get; set; }

        /// <summary>
        /// This is the master interrupt enable. If this isn't enabled, then no interrupts will occur.
        /// </summary>
        private bool _ime = false;

        /// <summary>
        /// This is the halted flag. If this is enabled, all program activity will stop until interrupts.
        /// </summary>
        private bool _halted = false;
        #endregion

        public void AddWaitCycles(int cycles) => _cyclesToWait += cycles;

        private delegate void Instruction();
        private Instruction[] _operations = new Instruction[256];
        private Instruction[] _extendedOperations = new Instruction[256];

        public Z80(GameboySystem system)
        {
            _system = system;

            #region Opcode Mappings
            _operations = new Instruction[256];

#if DEBUG
            _operations = Enumerable.Repeat<Instruction>(
                delegate
                {
                    throw new Exception($"Instruction (0x{_system.MMU[PC - 1]:X2}) " +
                        $"not implemented! " +
                        $"PC = (0x{PC - 1:X4})");

                }, 256).ToArray();

            _extendedOperations = Enumerable.Repeat<Instruction>(
                () =>
                {
                    throw new Exception($"Instruction (0xCB 0x{_system.MMU[PC - 1]:X2}) " +
                        $"not implemented! " +
                        $"PC = (0x{PC - 2:X4})");

                }, 256).ToArray();
#endif            
            PopulateLoadInstructions();             // LD, LDH, LDD, LDI, PUSH, POP
            PopulateControlFunctions();             // NOP, STOP, EI, DI, HALT, PREFIX CB
            
            _operations[0x03] = delegate            // INC BC
            {
                BC++;
                _cyclesToWait += 4;
            };
            _operations[0x04] = delegate            // INC B
            {
                B++;

                SetFlag(B == 0, StatusFlags.Z);
                SetFlag(false, StatusFlags.N);
                SetFlag(B % 16 == 0, StatusFlags.H);
            };
            _operations[0x05] = delegate            // DEC B
            {
                B--;

                SetFlag(B == 0, StatusFlags.Z);
                SetFlag(true, StatusFlags.N);
                SetFlag(B % 16 == 0, StatusFlags.H);
            };
            _operations[0x0B] = delegate            // DEC BC
            {
                BC--;

                _cyclesToWait += 4;
            };
            _operations[0x0C] = delegate            // INC C
            {
                C++;

                SetFlag(C == 0, StatusFlags.Z);
                SetFlag(false, StatusFlags.N);
                SetFlag(C % 16 == 0, StatusFlags.H);
            };
            _operations[0x0D] = delegate            // DEC C
            {
                C--;

                SetFlag(C == 0, StatusFlags.Z);
                SetFlag(true, StatusFlags.N);
                SetFlag(C % 16 == 0, StatusFlags.H);
            };
            _operations[0x13] = delegate             // INC DE
            {
                DE++;
                _cyclesToWait += 4;
            };
            _operations[0x15] = delegate             // DEC D
            {
                D--;

                SetFlag(D == 0, StatusFlags.Z);
                SetFlag(true, StatusFlags.N);
                SetFlag(D % 16 == 0, StatusFlags.H);
            };
            _operations[0x17] = delegate             // RLA
            {
                Bitwise.RotateLeft(ref A, ref F);

                SetFlag(false, StatusFlags.Z);
                SetFlag(false, StatusFlags.N);
                SetFlag(false, StatusFlags.H);
            };
            _operations[0x1D] = delegate            // DEC E
            {
                E--;

                SetFlag(E == 0, StatusFlags.Z);
                SetFlag(true, StatusFlags.N);
                SetFlag(E % 16 == 0, StatusFlags.H);
            };
            _operations[0x18] = delegate            // JR r8
            {
                sbyte jumpValue = (sbyte)ReadByte(PC);
                JumpRelative(jumpValue);
            };
            _operations[0x20] = delegate            // JR NZ,r8
            {
                sbyte jumpValue = (sbyte)ReadByte(PC);
                if (!CheckFlag(StatusFlags.Z))
                    JumpRelative(jumpValue);
            };
            _operations[0x23] = delegate            // INC HL
            {
                HL++;
                _cyclesToWait += 4;
            };
            _operations[0x24] = delegate            // INC H
            {
                H++;

                SetFlag(H == 0, StatusFlags.Z);
                SetFlag(false, StatusFlags.N);
                SetFlag(H % 16 == 0, StatusFlags.H);
            };
            _operations[0x28] = delegate            // JR Z,r8
            {
                sbyte jumpValue = (sbyte)ReadByte(PC);
                if (CheckFlag(StatusFlags.Z))
                    JumpRelative(jumpValue);
            };
            _operations[0x3D] = delegate            // DEC A
            {
                A--;

                SetFlag(A == 0, StatusFlags.Z);
                SetFlag(true, StatusFlags.N);
                SetFlag(A % 16 == 0, StatusFlags.H);
            };
            _operations[0x86] = delegate            // ADD A,(HL)
            {
                byte d8 = _system.MMU[HL];

                SetFlag((A + d8 - 1) == byte.MaxValue, StatusFlags.Z);
                SetFlag(false, StatusFlags.N);
                SetFlag((A + d8) % 16 == 0, StatusFlags.H);
                SetFlag(A + d8 > byte.MaxValue, StatusFlags.C);

                A += d8;

                _cyclesToWait += 4;
            };
            _operations[0x90] = delegate            // SUB B
            {
                SetFlag(A == B, StatusFlags.Z);
                SetFlag(true, StatusFlags.N);
                SetFlag((B & 0xF) > (A & 0xF), StatusFlags.H);
                SetFlag(B > A, StatusFlags.C);

                A -= B;
            };
            _operations[0xAF] = delegate            // XOR A
            {
                A = (byte)(A ^ A);

                SetFlag(A == 0, StatusFlags.Z);
                SetFlag(false, StatusFlags.N);
                SetFlag(false, StatusFlags.H);
                SetFlag(false, StatusFlags.C);
            };
            _operations[0xB0] = delegate            // OR B
            {
                A = (byte)(A | B);

                SetFlag(A == 0, StatusFlags.Z);
                SetFlag(false, StatusFlags.N);
                SetFlag(false, StatusFlags.H);
                SetFlag(false, StatusFlags.C);
            };
            _operations[0xB1] = delegate            // OR C
            {
                A = (byte)(A | C);

                SetFlag(A == 0, StatusFlags.Z);
                SetFlag(false, StatusFlags.N);
                SetFlag(false, StatusFlags.H);
                SetFlag(false, StatusFlags.C);
            };
            _operations[0xBE] = delegate            // CP (HL)
            {
                byte d8 = _system.MMU[HL];

                SetFlag(d8 == A, StatusFlags.Z);
                SetFlag(true, StatusFlags.N);
                SetFlag((d8 & 0xF) > (A & 0xF), StatusFlags.H);
                SetFlag(d8 > A, StatusFlags.C);

                _cyclesToWait += 4;
            };
            _operations[0xC9] = delegate            // RET
            {
                PC = PopUshort();
                _cyclesToWait += 4;
            };
            _operations[0xCD] = delegate            // CALL a16
            {
                ushort d16 = ReadUshort(PC);
                PushUshort(PC);
                PC = d16;
            };
            _operations[0xFE] = delegate            // CP d8
            {
                byte d8 = ReadByte(PC);

                SetFlag(d8 == A, StatusFlags.Z);
                SetFlag(true, StatusFlags.N);
                SetFlag((d8 & 0xF) > (A & 0xF), StatusFlags.H);
                SetFlag(d8 > A, StatusFlags.C);
            };

            _extendedOperations[0x11] = delegate     // RL C
            {
                Bitwise.RotateLeft(ref C, ref F);

                SetFlag(C == 0, StatusFlags.Z);
                SetFlag(false, StatusFlags.N);
                SetFlag(false, StatusFlags.H);
            };
            _extendedOperations[0x7C] = delegate     // BIT 7,H
            {
                int result = H.BitValue(7);

                SetFlag(result == 0, StatusFlags.Z);
                SetFlag(false, StatusFlags.N);
                SetFlag(true, StatusFlags.H);
            };
            #endregion
        }

        public void Step()
        {
            if (_cyclesToWait > 0)
            {
                --_cyclesToWait;
            }
            else
            {
                if (!_halted)
                    _operations[ReadByte(PC)]();
                else // if halted keep adding 4 extra cycles to wait
                    _cyclesToWait += 4;

                if (_system.ISR.CanServiceInterrupts && _halted) // if stop or halt, unhalt if interrupt
                {
                    _halted = false;
                }

                if (_ime == true) // check for interrupt stuff only if IME is enabled
                {
                    if (_system.ISR.CanHandleInterrupt(InterruptFlags.VBlank))
                    {
                        _ime = false; // disable ime flag
                        _system.ISR.DisableInterrupt(InterruptFlags.VBlank); // clear IF bit 0

                        PushUshort(PC);

                        _cyclesToWait -= 12; // removing the push ushort
                        _cyclesToWait += 20; // 20 cycles for an interrupt

                        PC = 0x40; // interrupt vector is always 40h
                    }
                    else if (_system.ISR.CanHandleInterrupt(InterruptFlags.Timer))
                    {
                    }
                    else if (_system.ISR.CanHandleInterrupt(InterruptFlags.Serial))
                    {
                    }
                    else if (_system.ISR.CanHandleInterrupt(InterruptFlags.LCDStat))
                    {
                    }
                    else if (_system.ISR.CanHandleInterrupt(InterruptFlags.Joypad))
                    {
                    }
                }
            }
        }

        private byte ReadByte(int addr)
        {
            byte ret = _system.MMU[addr];
            PC++;
            _cyclesToWait += 4;
            return ret;
        }

        private ushort ReadUshort(int addr)
        {
            ushort ret = Convert.ToUInt16(_system.MMU[addr + 1] << 8 | _system.MMU[addr]);
            PC += 2;
            _cyclesToWait += 8;
            return ret;
        }

        private void WriteUshort(int addr, ushort value)
        {
            byte byte1 = value.MostSignificantByte();
            byte byte2 = value.LeastSignificantByte();

            _system.MMU[addr] = byte2;
            _system.MMU[addr + 1] = byte1;
            _cyclesToWait += 12;
        }

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

        private void JumpRelative(sbyte relativeValue)
        {
            PC = (ushort)((PC + relativeValue) & 0xFFFF);
            _cyclesToWait += 4;
        }

        private void JumpAbsolute(ushort addr)
        {
            PC = addr;
            _cyclesToWait += 4;
        }
    }
}
