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
        #endregion

        private delegate void Instruction();
        private Instruction[] _operations = new Instruction[256];
        private Instruction[] _bitwiseOperations = new Instruction[256];

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
                        $"PC = (0x{PC - 1:X2})");

                }, 256).ToArray();

            _bitwiseOperations = Enumerable.Repeat<Instruction>(
                () =>
                {
                    throw new Exception($"Instruction (0xCB 0x{_system.MMU[PC - 1]:X2}) " +
                        $"not implemented! " +
                        $"PC = (0x{PC - 2:X2})");

                }, 256).ToArray();
#endif

            _operations[0x00] = delegate { };       // NOP
            _operations[0x01] = delegate            // LD BC,d16
            {
                BC = ReadUshort(PC);
            };
            _operations[0x02] = delegate            // LD (BC),A
            {
                _system.MMU[BC] = A;
                _cyclesToWait += 4;
            };
            _operations[0x03] = delegate            // INC BC
            {
                BC++;
                _cyclesToWait += 4;
            };
            _operations[0x04] = delegate            // INC B
            {
                B++;

                SetFlag(B == 0, StatusFlags.ZeroFlag);
                SetFlag(false, StatusFlags.SubtractFlag);
                SetFlag(B % 16 == 0, StatusFlags.HalfCarryFlag);
            };
            _operations[0x05] = delegate            // DEC B
            {
                B--;

                SetFlag(B == 0, StatusFlags.ZeroFlag);
                SetFlag(true, StatusFlags.SubtractFlag);
                SetFlag(B % 16 == 0, StatusFlags.HalfCarryFlag);
            };
            _operations[0x06] = delegate            // LD B,d8
            {
                B = ReadByte(PC);
            };
            _operations[0x0C] = delegate            // INC C
            {
                C++;

                SetFlag(C == 0, StatusFlags.ZeroFlag);
                SetFlag(false, StatusFlags.SubtractFlag);
                SetFlag(C % 16 == 0, StatusFlags.HalfCarryFlag);
            };
            _operations[0x0D] = delegate            // DEC C
            {
                C--;

                SetFlag(C == 0, StatusFlags.ZeroFlag);
                SetFlag(true, StatusFlags.SubtractFlag);
                SetFlag(C % 16 == 0, StatusFlags.HalfCarryFlag);
            };
            _operations[0x0E] = delegate            // LD C,d8
            {
                C = ReadByte(PC);
            };
            _operations[0x11] = delegate            // LD DE,d16
            {
                DE = ReadUshort(PC);
            };
            _operations[0x13] = delegate             // INC DE
            {
                DE++;
                _cyclesToWait += 4;
            };
            _operations[0x15] = delegate             // DEC D
            {
                D--;

                SetFlag(D == 0, StatusFlags.ZeroFlag);
                SetFlag(true, StatusFlags.SubtractFlag);
                SetFlag(D % 16 == 0, StatusFlags.HalfCarryFlag);
            };
            _operations[0x16] = delegate             // LD D,d8
            {
                D = ReadByte(PC);
            };
            _operations[0x17] = delegate             // RLA
            {
                Bitwise.RotateLeft(ref A, ref F);

                SetFlag(false, StatusFlags.ZeroFlag);
                SetFlag(false, StatusFlags.SubtractFlag);
                SetFlag(false, StatusFlags.HalfCarryFlag);
            };
            _operations[0x1A] = delegate            // LD A,(DE)
            {
                A = _system.MMU[DE];
                _cyclesToWait += 4;
            };
            _operations[0x1D] = delegate            // DEC E
            {
                E--;

                SetFlag(E == 0, StatusFlags.ZeroFlag);
                SetFlag(true, StatusFlags.SubtractFlag);
                SetFlag(E % 16 == 0, StatusFlags.HalfCarryFlag);
            };
            _operations[0x18] = delegate            // JR r8
            {
                sbyte jumpValue = (sbyte)ReadByte(PC);
                JumpRelative(jumpValue);
            };
            _operations[0x1E] = delegate            // LD E,d8
            {
                E = ReadByte(PC);
            };
            _operations[0x20] = delegate            // JR NZ,r8
            {
                sbyte jumpValue = (sbyte)ReadByte(PC);
                if (!CheckFlag(StatusFlags.ZeroFlag))
                    JumpRelative(jumpValue);
            };
            _operations[0x21] = delegate            // LD HL,d16
            {
                HL = ReadUshort(PC);
            };
            _operations[0x22] = delegate            // LD (HL+),A
            {
                _system.MMU[HL] = A;
                HL++;
                _cyclesToWait += 4;
            };
            _operations[0x23] = delegate            // INC HL
            {
                HL++;
                _cyclesToWait += 4;
            };
            _operations[0x24] = delegate            // INC H
            {
                H++;

                SetFlag(H == 0, StatusFlags.ZeroFlag);
                SetFlag(false, StatusFlags.SubtractFlag);
                SetFlag(H % 16 == 0, StatusFlags.HalfCarryFlag);
            };
            _operations[0x28] = delegate            // JR Z,r8
            {
                sbyte jumpValue = (sbyte)ReadByte(PC);
                if (CheckFlag(StatusFlags.ZeroFlag))
                    JumpRelative(jumpValue);
            };
            _operations[0x2E] = delegate            // LD L,d8
            {
                L = ReadByte(PC);
            };
            _operations[0x31] = delegate            // LD SP,d16
            {
                SP = ReadUshort(PC);
            };
            _operations[0x32] = delegate            // LD (HL-),A
            {
                _system.MMU[HL] = A;
                HL--;
                _cyclesToWait += 4;
            };
            _operations[0x3D] = delegate            // DEC A
            {
                A--;

                SetFlag(A == 0, StatusFlags.ZeroFlag);
                SetFlag(true, StatusFlags.SubtractFlag);
                SetFlag(A % 16 == 0, StatusFlags.HalfCarryFlag);
            };
            _operations[0x4F] = delegate            // LD C,A
            {
                C = A;
            };
            _operations[0x57] = delegate            // LD D,A
            {
                D = A;
            };
            _operations[0x67] = delegate            // LD H,A
            {
                H = A;
            };
            _operations[0x77] = delegate            // LD (HL),A
            {
                _system.MMU[HL] = A;
                _cyclesToWait += 4;
            };
            _operations[0x78] = delegate            // LD A,B
            {
                A = B;
            };
            _operations[0x7B] = delegate            // LD A,E
            {
                A = E;
            };
            _operations[0x7C] = delegate            // LD A,H
            {
                A = H;
            };
            _operations[0x7D] = delegate            // LD A,L
            {
                A = L;
            };
            _operations[0x86] = delegate            // ADD A,(HL)
            {
                byte d8 = _system.MMU[HL];

                SetFlag((A + d8 - 1) == byte.MaxValue, StatusFlags.ZeroFlag);
                SetFlag(false, StatusFlags.SubtractFlag);
                SetFlag((A + d8) % 16 == 0, StatusFlags.HalfCarryFlag);
                SetFlag(A + d8 > byte.MaxValue, StatusFlags.CarryFlag);

                A += d8;

                _cyclesToWait += 4;
            };
            _operations[0x3E] = delegate            // LD A,d8
            {
                A = ReadByte(PC);
            };
            _operations[0x90] = delegate            // SUB B
            {               
                SetFlag(A == B, StatusFlags.ZeroFlag);
                SetFlag(true, StatusFlags.SubtractFlag);
                SetFlag((B & 0xF) > (A & 0xF), StatusFlags.HalfCarryFlag);
                SetFlag(B > A, StatusFlags.CarryFlag);

                A -= B;
            };
            _operations[0xAF] = delegate            // XOR A
            {
                A = (byte)(A ^ A);

                SetFlag(A == 0, StatusFlags.ZeroFlag);
                SetFlag(false, StatusFlags.SubtractFlag);
                SetFlag(false, StatusFlags.HalfCarryFlag);
                SetFlag(false, StatusFlags.CarryFlag);
            };
            _operations[0xBE] = delegate            // CP (HL)
            {
                byte d8 = _system.MMU[HL];

                SetFlag(d8 == A, StatusFlags.ZeroFlag);
                SetFlag(true, StatusFlags.SubtractFlag);
                SetFlag((d8 & 0xF) > (A & 0xF), StatusFlags.HalfCarryFlag);
                SetFlag(d8 > A, StatusFlags.CarryFlag);

                _cyclesToWait += 4;
            };
            _operations[0xC1] = delegate            // POP BC
            {
                BC = PopUshort();
            };
            _operations[0xC3] = delegate            // JP a16
            {
                JumpAbsolute(ReadUshort(PC));
            };
            _operations[0xC5] = delegate            // PUSH BC
            {
                PushUshort(BC);
            };
            _operations[0xC9] = delegate            // RET
            {
                PC = PopUshort();
                _cyclesToWait += 4;
            };
            _operations[0xCB] = delegate            // PREFIX CB
            {
                _bitwiseOperations[ReadByte(PC)]();
            };
            _operations[0xCD] = delegate            // CALL a16
            {
                ushort d16 = ReadUshort(PC);
                PushUshort(PC);
                PC = d16;   
            };
            _operations[0xE0] = delegate            // LDH (a8),A
            {
                _system.MMU[ReadByte(PC) + 0xFF00] = A;
                _cyclesToWait += 4;
            };
            _operations[0xE2] = delegate            // LD (C),A
            {
                _system.MMU[C + 0xFF00] = A;
                _cyclesToWait += 4;
            };
            _operations[0xEA] = delegate            // LD (a16),A
            {
                _system.MMU[ReadUshort(PC)] = A;
                _cyclesToWait += 4;
            };
            _operations[0xF0] = delegate            // LDH A,(a8)
            {
                A = _system.MMU[0xFF00 + ReadByte(PC)];
                _cyclesToWait += 4;
            };
            _operations[0xFE] = delegate            // CP d8
            {
                byte d8 = ReadByte(PC);

                SetFlag(d8 == A, StatusFlags.ZeroFlag);
                SetFlag(true, StatusFlags.SubtractFlag);
                SetFlag((d8 & 0xF) > (A & 0xF), StatusFlags.HalfCarryFlag);
                SetFlag(d8 > A, StatusFlags.CarryFlag);
            };

            _bitwiseOperations[0x11] = delegate     // RL C
            {
                Bitwise.RotateLeft(ref C, ref F);

                SetFlag(C == 0, StatusFlags.ZeroFlag);
                SetFlag(false, StatusFlags.SubtractFlag);
                SetFlag(false, StatusFlags.HalfCarryFlag);
            };
            _bitwiseOperations[0x7C] = delegate     // BIT 7,H
            {
                int result = H.BitValue(7);

                SetFlag(result == 0, StatusFlags.ZeroFlag);
                SetFlag(false, StatusFlags.SubtractFlag);
                SetFlag(true, StatusFlags.HalfCarryFlag);
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
                _operations[ReadByte(PC)]();
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
            if (ret == 0x0104)
            {
                ;
            }
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

        private void PushUshort(ushort value)
        {
            SP -= 2;
            WriteUshort(SP, value);
        }

        private ushort PopUshort()
        {
            ushort ret = ReadUshort(SP);
            SP += 2;
            PC -= 2; // only one parameter, read ushort
            return ret;
        }
    }
}
