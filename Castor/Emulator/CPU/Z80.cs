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
        #endregion

        #region Utility Functions
        public void InternalDelay(int cycles = 1) => _cycles += cycles * 4;

        public ushort ReadWord(int addr)
        {
            InternalDelay(2);
            return (ushort)(_d.MMU[addr + 1] << 8 | _d.MMU[addr]);
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
            var opcode = _d.MMU[_registers.Bump()];

            Decode(_d, opcode);
        }

        #region Opcode Methods
        public override void Load(ref byte out8, byte in8)
        {
            throw new NotImplementedException();
        }

        public override void Load(IndirectAddress indr, byte in8)
        {
            throw new NotImplementedException();
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
            F = (byte)Cond.Z.Test(result == 0);
            A = in8;
        }

        public override void Inc(ref byte io8)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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

        public override void CbPrefix()
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

        public override void Load16IndrSP()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
