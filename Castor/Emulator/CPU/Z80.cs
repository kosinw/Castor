using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.CPU
{
    public class Z80 : OpcodeBuilder
    {
        #region References
        public ref byte A => ref _registers.A;
        public ref byte F => ref _registers.F;
        public ref byte B => ref _registers.B;
        public ref byte C => ref _registers.C;
        public ref byte D => ref _registers.D;
        public ref byte E => ref _registers.E;
        public ref byte H => ref _registers.H;
        public ref byte L => ref _registers.L;

        public ref ushort AF => ref _registers.AF;
        public ref ushort BC => ref _registers.BC;
        public ref ushort DE => ref _registers.DE;
        public ref ushort HL => ref _registers.HL;

        public ref ushort SP => ref _registers.SP;
        public ref ushort PC => ref _registers.PC;

        public ushort HLI
        {
            get => HL++;
            set { HL = value; HL++; }
        }
        public ushort HLD
        {
            get => HL--;
            set { HL = value; HL--; }
        }
        #endregion

        #region Memory Accessors
        public byte AddrHL
        {
            get
            {
                InternalDelay();
                return _d.MMU[HL];
            }

            set
            {
                InternalDelay();
                _d.MMU[HL] = value;
            }
        }
        public byte AddrHLI
        {
            get
            {
                InternalDelay();
                return _d.MMU[HLI];
            }

            set
            {
                InternalDelay();
                _d.MMU[HLI] = value;
            }
        }
        public byte AddrHLD
        {
            get
            {
                InternalDelay();
                return _d.MMU[HLD];
            }

            set
            {
                InternalDelay();
                _d.MMU[HLD] = value;
            }
        }
        public byte Imm8
        {
            get => ReadByte(_registers.Bump());
            set => WriteByte(_registers.Bump(), value);
        }        
        public byte ZeroPageC
        {
            get => ReadByte(C + 0xFF00);
            set => WriteByte(C + 0xFF00, value);
        }
        #endregion

        #region Utility Functions
        private void InternalDelay(int cycles = 1) => _cycles += cycles * 4;
        public byte DecodeInstruction() { InternalDelay(); return _d.MMU[_registers.Bump()]; }
        private byte ReadByte(int addr, int delay = 1)
        {
            InternalDelay(delay);
            return _d.MMU[addr];
        }
        private void WriteByte(int addr, byte value, int delay = 1)
        {
            InternalDelay(delay);
            _d.MMU[addr] = value;
        }
        private ushort ReadWord(int addr, int delay = 2)
        {
            InternalDelay(delay);
            return (ushort)(_d.MMU[addr + 1] << 8 | _d.MMU[addr]);
        }
        private void Jump(ushort value)
        {
            InternalDelay();
            PC = value;
        }
        #endregion

        #region Internal Members
        private Registers _registers;        
        private Device _d;

        private int _cycles;
        private bool _halted;
        #endregion;

        public Z80(Device d)
        {
            _d = d;
            _cycles = 0;
            _registers = new Registers();
            _halted = false;
        }

        public int Step()
        {
            _cycles = 0;

            if (!_halted)
                DecodeStep();
            else
                HaltedStep();

            return _cycles;
        }

        private void HaltedStep()
        {
            InternalDelay();
        }

        public void DecodeStep()
        {
            var opcode = DecodeInstruction();

            Decode(_d, opcode);
        }

        #region Opcode Methods
        public override void Load(ref byte out8, byte in8)
        {
            out8 = in8;
        }

        public override void Load(int indr, byte in8, int hlAction = 0)
        {
            WriteByte(indr, in8);
            HL += (ushort)hlAction;
        }

        public override void Add(byte in8)
        {
            throw new NotImplementedException();
        }

        public override void Adc(byte in8)
        {
            throw new NotImplementedException();
        }

        public override void Sub(byte in8)
        {
            throw new NotImplementedException();
        }

        public override void Sbc(byte in8)
        {
            throw new NotImplementedException();
        }

        public override void Cp(byte in8)
        {
            throw new NotImplementedException();
        }

        public override void And(byte in8)
        {
            throw new NotImplementedException();
        }

        public override void Or(byte in8)
        {
            throw new NotImplementedException();
        }

        public override void Xor(byte in8)
        {
            var result = A ^ in8;
            F = Cond.Z.Test(result == 0);
            A = in8;
        }

        public override void Inc(ref byte io8)
        {
            io8 = Utility.Math.Add.Inc(io8, ref _registers.F);
        }

        public override void Inc(int indr)
        {
            var in8 = ReadByte(indr);
            WriteByte(indr, Utility.Math.Add.Inc(in8,, ref _registers.F));
        }

        public override void Dec(ref byte io8)
        {
            throw new NotImplementedException();
        }

        public override void Rlca()
        {
            throw new NotImplementedException();
        }

        public override void Rla()
        {
            throw new NotImplementedException();
        }

        public override void Rrca()
        {
            throw new NotImplementedException();
        }

        public override void Rra()
        {
            throw new NotImplementedException();
        }

        public override void Rlc(ref byte io8)
        {
            throw new NotImplementedException();
        }

        public override void Rl(ref byte io8)
        {
            throw new NotImplementedException();
        }

        public override void Rrc(ref byte io8)
        {
            throw new NotImplementedException();
        }

        public override void Rr(ref byte io8)
        {
            throw new NotImplementedException();
        }

        public override void Sla(ref byte io8)
        {
            throw new NotImplementedException();
        }

        public override void Sra(ref byte io8)
        {
            throw new NotImplementedException();
        }

        public override void Srl(ref byte io8)
        {
            throw new NotImplementedException();
        }

        public override void Swap(ref byte io8)
        {
            throw new NotImplementedException();
        }

        public override void Bit(uint num, byte in8)
        {
            Utility.Bit.Value(in8, num, ref _registers.F);
        }

        public override void Set(uint num, ref byte io8)
        {
            throw new NotImplementedException();
        }

        public override void Res(uint num, ref byte io8)
        {
            throw new NotImplementedException();
        }

        public override void Jp()
        {
            throw new NotImplementedException();
        }

        public override void JpHL()
        {
            throw new NotImplementedException();
        }

        public override void Jr()
        {
            throw new NotImplementedException();
        }

        public override void Call()
        {
            throw new NotImplementedException();
        }

        public override void Ret()
        {
            throw new NotImplementedException();
        }

        public override void Reti()
        {
            throw new NotImplementedException();
        }

        public override void Jp(Cond cond)
        {
            throw new NotImplementedException();
        }

        public override void Jr(Cond cond)
        {
            ushort value = (ushort)((sbyte)ReadByte(_registers.Bump()) + PC);

            if (cond == Cond.NC || cond == Cond.NZ)
            {
                if (!((Cond)_registers.F).HasFlag(cond))
                    Jump(value);
            }
            else if (((Cond)_registers.F).HasFlag(cond))
            {
                Jump(value);
            }
        }

        public override void Call(Cond cond)
        {
            throw new NotImplementedException();
        }

        public override void Ret(Cond cond)
        {
            throw new NotImplementedException();
        }

        public override void Rst(byte vec)
        {
            throw new NotImplementedException();
        }

        public override void Halt()
        {
            throw new NotImplementedException();
        }

        public override void Stop()
        {
            throw new NotImplementedException();
        }

        public override void Di()
        {
            throw new NotImplementedException();
        }

        public override void Ei()
        {
            throw new NotImplementedException();
        }

        public override void Ccf()
        {
            throw new NotImplementedException();
        }

        public override void Scf()
        {
            throw new NotImplementedException();
        }

        public override void Nop()
        {
            throw new NotImplementedException();
        }

        public override void Daa()
        {
            throw new NotImplementedException();
        }

        public override void Cpl()
        {
            throw new NotImplementedException();
        }

        public override void Load16(ref ushort io16)
        {
            io16 = ReadWord(_registers.Bump(2));
        }

        public override void Load16SPHL()
        {
            throw new NotImplementedException();
        }

        public override void Load16IndrSP()
        {
            throw new NotImplementedException();
        }

        public override void Load16HLSPe()
        {
            throw new NotImplementedException();
        }

        public override void Push16(ref ushort io16)
        {
            throw new NotImplementedException();
        }

        public override void Pop16(ref ushort io16)
        {
            throw new NotImplementedException();
        }

        public override void Add16(ref ushort io16)
        {
            throw new NotImplementedException();
        }

        public override void Add16SPe()
        {
            throw new NotImplementedException();
        }

        public override void Inc16(ref ushort io16)
        {
            throw new NotImplementedException();
        }

        public override void Dec16(ref ushort io16)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
