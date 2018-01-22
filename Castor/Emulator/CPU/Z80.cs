using Castor.Emulator.Utility;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace Castor.Emulator.CPU
{
    public partial class Z80 : ICPU
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
        public byte AddrHLI
        {
            get
            {
                InternalDelay();
                return _d.MMU[HL++];
            }

            set
            {
                InternalDelay();
                _d.MMU[HL++] = value;
            }
        }
        public byte AddrHLD
        {
            get
            {
                InternalDelay();
                return _d.MMU[HL--];
            }

            set
            {
                InternalDelay();
                _d.MMU[HL--] = value;
            }
        }
        public byte ZeroPageC
        {
            get => ReadByte(C + 0xFF00);
            set => WriteByte(C + 0xFF00, value);
        }
        public byte ZeroPage
        {
            get => ReadByte(Imm8() + 0xFF00);
            set => WriteByte(Imm8() + 0xFF00, value);
        }
        public byte AddrBC
        {
            get => ReadByte(BC);
            set => WriteByte(BC, value);
        }
        public byte AddrDE
        {
            get => ReadByte(DE);
            set => WriteByte(DE, value);
        }
        #endregion

        #region Utility Functions
        private void InternalDelay(int cycles = 1) => _cycles += cycles * 4;
        private byte DecodeInstruction() { InternalDelay(); return _d.MMU[_registers.Bump()]; }
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
        private void WriteWord(int addr, ushort value, int delay = 2)
        {
            InternalDelay(delay);
            _d.MMU[addr] = value.LSB();
            _d.MMU[addr + 1] = value.MSB();
        }
        private void Jump(ushort value)
        {
            InternalDelay();
            PC = value;
        }
        private void Push(ushort value)
        {
            SP -= 2;
            WriteWord(SP, value);
        }
        private ushort Pop()
        {
            var ret = ReadWord(SP);
            SP += 2;
            return ret;
        }
        private byte Imm8()
        {
            return ReadByte(_registers.Bump());
        }
        private ushort Imm16()
        {
            return ReadWord(_registers.Bump(2));
        }

        private void InterruptVec(byte vec)
        {
            _halted = false;

            if (_ime == IME.Enabled)
            {
                InternalDelay(3);
                Push16(PC);
                PC = vec;
                _ime = IME.Disabled;
                _d.IRQ.IF = 0x00;
            }
        }
        #endregion

        #region Internal Members
        private Registers _registers;
        private Device _d;
        private IME _ime;


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

            for (int i = 0; i < _cycles / 4; ++i)
            {
                switch (_ime)
                {
                    case IME.Enabling:
                        _ime = IME.Enabled;
                        break;
                }
            }

            return _cycles;
        }

        private void HaltedStep()
        {
            HandleInterrupts();
            InternalDelay();
        }

        private void DecodeStep()
        {
            HandleInterrupts();
            var opcode = DecodeInstruction();
            Decode(opcode);
        }

        private void HandleInterrupts()
        {
            if (_d.IRQ.CanServiceInterrupts) // if any interrupts are available
            {
                if (_d.IRQ.CanHandleInterrupt(Memory.InterruptFlags.VBL))
                    InterruptVec(0x40);
                else if (_d.IRQ.CanHandleInterrupt(Memory.InterruptFlags.STAT))
                    InterruptVec(0x48);
                else if (_d.IRQ.CanHandleInterrupt(Memory.InterruptFlags.Timer))
                    InterruptVec(0x50);
                else if (_d.IRQ.CanHandleInterrupt(Memory.InterruptFlags.Serial))
                    InterruptVec(0x58);
                else if (_d.IRQ.CanHandleInterrupt(Memory.InterruptFlags.Joypad))
                    InterruptVec(0x60);
            }
        }

        #region Opcode Methods
        public void Load(ref byte out8, byte in8)
        {
            out8 = in8;
        }

        public void Load(int indr, byte in8, int hlAction = 0)
        {
            WriteByte(indr, in8);
            HL += (ushort)hlAction;
        }

        public void Add(byte in8)
        {
            A = Utility.Math.Add.Add8(in8, ref _registers);
        }

        public void Adc(byte in8)
        {
            byte cy = (F & (byte)Cond.C) != 0 ? (byte)1 : (byte)0;
            A = Utility.Math.Add.Add8((byte)(in8 + cy), ref _registers);
        }

        public void Sub(byte in8)
        {
            A = Utility.Math.Sub.Subt(in8, ref _registers);
        }

        public void Sbc(byte in8)
        {
            throw new NotImplementedException();
        }

        public void Cp(byte in8)
        {
            Utility.Math.Sub.Subt(in8, ref _registers);
        }

        public void And(byte in8)
        {
            A = Utility.Math.And(in8, ref _registers);
        }

        public void Or(byte in8)
        {
            A = Utility.Math.Or(in8, ref _registers);
        }

        public void Xor(byte in8)
        {
            var result = A ^ in8;
            F = Cond.Z.Test(result == 0);
            A = in8;
        }

        public void Inc(ref byte io8)
        {
            io8 = Utility.Math.Add.Inc(io8, ref _registers.F);
        }

        public void Inc(int indr)
        {
            var in8 = ReadByte(indr);
            WriteByte(indr, Utility.Math.Add.Inc(in8, ref _registers.F));
        }

        public void Dec(ref byte io8)
        {
            io8 = Utility.Math.Sub.Dec(io8, ref _registers.F);
        }

        public void Dec(int indr)
        {
            var in8 = ReadByte(indr);
            WriteByte(indr, Utility.Math.Sub.Dec(in8, ref _registers.F));
        }

        public void Rlca()
        {
            throw new NotImplementedException();
        }

        public void Rla()
        {
            A = Utility.Bit.Rl(A, ref F, true);
            F &= (byte)~Cond.Z; // unset the zero flag
        }

        public void Rrca()
        {
            throw new NotImplementedException();
        }

        public void Rra()
        {
            throw new NotImplementedException();
        }

        public void Rlc(ref byte io8)
        {
            throw new NotImplementedException();
        }

        public void Rl(ref byte io8)
        {
            io8 = Utility.Bit.Rl(io8, ref F, true);
        }

        public void Rrc(ref byte io8)
        {
            throw new NotImplementedException();
        }

        public void Rr(ref byte io8)
        {
            throw new NotImplementedException();
        }

        public void Sla(ref byte io8)
        {
            throw new NotImplementedException();
        }

        public void Sra(ref byte io8)
        {
            throw new NotImplementedException();
        }

        public void Srl(ref byte io8)
        {
            throw new NotImplementedException();
        }

        public void Swap(ref byte io8)
        {
            io8 = Utility.Bit.Swap(io8, ref _registers.F);
        }

        public void SwapHL()
        {
            AddrHL = Utility.Bit.Swap(AddrHL, ref _registers.F);
        }

        public void Bit(int num, byte in8)
        {
            Utility.Bit.Value(in8, num, ref _registers.F);
        }

        public void Set(int num, ref byte io8)
        {
            throw new NotImplementedException();
        }

        public void Res(int num, ref byte io8)
        {
            io8 = Utility.Bit.ClearBit(io8, num);
        }

        public void Jp()
        {
            ushort value = ReadWord(_registers.Bump(2));

            Jump(value);
        }

        public void JpHL()
        {
            PC = HL;
        }

        public void Jr()
        {
            var offset = (sbyte)ReadByte(_registers.Bump());
            ushort value = (ushort)(PC + offset);

            Jump(value);
        }

        public void Call()
        {

            ushort addr = ReadWord(_registers.Bump(2)); // memory access low + hi byte (2 M-cycles)
            InternalDelay(); // extra internal delay (1 M-cycle)
            Push(PC); // memory access pc hi + low byte (2 M-cycles);
            PC = addr;
        }

        public void Ret()
        {
            InternalDelay();
            PC = Pop();
        }

        public void Reti()
        {
            InternalDelay();
            PC = Pop();
            _ime = IME.Enabled;
        }

        public void Jp(Cond cond)
        {            
            ushort value = ReadWord(_registers.Bump(2));

            if (cond.FlagSet(F))
                Jump(value);
        }

        public void Jr(Cond cond)
        {
            var offset = (sbyte)ReadByte(_registers.Bump());
            ushort value = (ushort)(PC + offset);

            if (cond.FlagSet(F))
                Jump(value);
        }

        public void Call(Cond cond)
        {
            throw new NotImplementedException();
        }

        public void Ret(Cond cond)
        {
            InternalDelay();

            if (cond.FlagSet(F))
                PC = Pop();
        }

        public void Rst(byte vec)
        {
            InternalDelay();
            Push(PC);
            PC = vec;
        }

        public void Halt()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void Di()
        {
            _ime = IME.Disabled;
        }

        public void Ei()
        {
            _ime = IME.Enabling;
        }

        public void Ccf()
        {
            throw new NotImplementedException();
        }

        public void Scf()
        {
            throw new NotImplementedException();
        }

        public void Nop()
        {
            // do nothing
        }

        public void Daa()
        {
            var carry = false;
            F &= (byte)~Cond.H;

            if ((_registers.F & (byte)Cond.H) != 0 || (_registers.A & 0xF) > 0x09)
            {
                _registers.A += 0x6;
            }
            if ((_registers.F & (byte)Cond.C) != 0 || _registers.A > 0x90)
            {
                _registers.A += 0x60;

                carry = true;
            }

            if (A == 0)
            {
                F |= (byte)Cond.Z;
            }

            if (carry)
            {
                F |= (byte)Cond.C;
            }
            else
            {
                F &= (byte)~Cond.C;
            }

            return;
        }

        public void Cpl()
        {
            A = (byte)~A;

            Utility.Bit.AlterFlag(ref _registers.F, Cond.N, true);
            Utility.Bit.AlterFlag(ref _registers.F, Cond.H, true);
        }

        public void Load16(ref ushort io16)
        {
            io16 = ReadWord(_registers.Bump(2));
        }

        public void Load16SPHL()
        {
            throw new NotImplementedException();
        }

        public void Load16IndrSP()
        {
            throw new NotImplementedException();
        }

        public void Load16HLSPe()
        {
            throw new NotImplementedException();
        }

        public void Push16(ushort io16)
        {
            InternalDelay();
            Push(io16);
        }

        public void Pop16(ref ushort io16)
        {
            io16 = Pop();
        }

        public void Add16(ushort io16)
        {
            HL = Utility.Math.Add.Add16(io16, ref _registers);
        }

        public void Add16SPe()
        {
            throw new NotImplementedException();
        }

        public void Inc16(ref ushort io16)
        {
            InternalDelay();
            io16++;
        }

        public void Dec16(ref ushort io16)
        {
            InternalDelay();
            io16--;
        }

        public void Res(int num, int indr)
        {
            WriteByte(indr, Utility.Bit.ClearBit(ReadByte(indr), num));
        }
        #endregion
    }
}