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
        public ushort SP;
        public ushort PC;

        #region Pointer Accessors
        public byte AddrHL
        {
            get
            {
                InternalDelay(1);
                return _system.MMU[HL];
            }
            set
            {
                InternalDelay(1);
                _system.MMU[HL] = value;
            }
        }
        public byte AddrHLI
        {
            get
            {
                InternalDelay(1);
                return _system.MMU[HL++];
            }
            set
            {
                InternalDelay(1);
                _system.MMU[HL++] = value;
            }
        }
        public byte AddrHLD
        {
            get
            {
                InternalDelay(1);
                return _system.MMU[HL--];
            }
            set
            {
                InternalDelay(1);
                _system.MMU[HL--] = value;
            }
        }
        public byte AddrC
        {
            get
            {
                InternalDelay(1);
                return _system.MMU[0xFF00 + C];
            }

            set
            {
                InternalDelay(1);
                _system.MMU[0xFF00 + C] = value;
            }
        }
        public byte AddrBC
        {
            get
            {
                InternalDelay(1);
                return _system.MMU[BC];
            }
            set
            {
                InternalDelay(1);
                _system.MMU[BC] = value;
            }
        }
        public byte AddrDE
        {
            get
            {
                InternalDelay(1);
                return _system.MMU[DE];
            }
            set
            {
                InternalDelay(1);
                _system.MMU[DE] = value;
            }
        }
        public byte Addr8
        {
            get
            {
                InternalDelay(1);
                return _system.MMU[0xFF00 + ReadByte(PC)];
            }

            set
            {
                InternalDelay(1);
                _system.MMU[0xFF00 + ReadByte(PC)] = value;
            }

        }
        public byte Addr16
        {
            get
            {
                InternalDelay(1);
                return _system.MMU[ReadUshort(PC)];
            }

            set
            {
                InternalDelay(1);
                _system.MMU[ReadUshort(PC)] = value;
            }
        }
        public ushort AccessorSP
        {
            get
            {
                InternalDelay(1);
                return SP;
            }

            set
            {
                InternalDelay(1);
                SP = value;
            }
        }
        #endregion        
        #endregion

        #region Private Members
        /// <summary>
        /// This counts how many cycles need to be waited until the next instruction is executed.
        /// </summary>
        private int _cyclesToWait = 0;

        /// <summary>
        /// This is a reference to the overarching class of all components
        /// </summary>
        private Device _system { get; set; }

        /// <summary>
        /// This is the master interrupt enable. If this isn't enabled, then no interrupts will occur.
        /// </summary>
        private bool _ime = false;

        /// <summary>
        /// If this is set to 2, then decrement.
        /// After next instruction, if set to 1 then trigger ime, then decrement.
        /// If 0 just ignore it.
        /// </summary>
        private int _setei = 0;

        /// <summary>
        /// This is the halted flag. If this is enabled, all program activity will stop until interrupts.
        /// </summary>
        private bool _halted = false;
        #endregion

        public void AddWaitCycles(int cycles) => _cyclesToWait += cycles;
        public void InternalDelay(int cycles) => AddWaitCycles(cycles * 4);

        public delegate void Instruction();
        private Instruction[] _op = new Instruction[256];
        private Instruction[] _cb = new Instruction[256];

        public Z80(Device system)
        {
            _system = system;

            #region Opcode Mappings
            _op = new Instruction[256];

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

                _cyclesToWait += 4;
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

                _cyclesToWait += 4;
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
            _cyclesToWait = 0;

            if (!_halted)
            {
                if (PC == 0x100)
                {
                    ;
                }

                _op[ReadByte(PC)]();

                if (_setei == 1)
                    _ime = true;
                if (_setei > 0)
                    --_setei;
            }

            else
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

                    PushUshort(PC); // push current pc onto stack

                    _cyclesToWait += 8; // add an extra 8 cycles

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

            return _cyclesToWait;
        }

        private byte ReadByte(int addr)
        {
            byte ret = _system.MMU[addr];
            PC++;
            _cyclesToWait += 4;
            return ret;
        }        

        private void WriteByte(int addr, byte value)
        {
            _system.MMU[addr] = value;
            _cyclesToWait += 4;
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
    }
}
