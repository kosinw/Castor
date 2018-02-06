using Castor.Emulator.Utility;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace Castor.Emulator.CPU
{
    public partial class Z80 : IInstructions
    {
        #region References
        public ref byte A => ref _r.A;
        public ref byte F => ref _r.F;
        public ref byte B => ref _r.B;
        public ref byte C => ref _r.C;
        public ref byte D => ref _r.D;
        public ref byte E => ref _r.E;
        public ref byte H => ref _r.H;
        public ref byte L => ref _r.L;

        public ref ushort AF => ref _r.AF;
        public ref ushort BC => ref _r.BC;
        public ref ushort DE => ref _r.DE;
        public ref ushort HL => ref _r.HL;

        public ref ushort SP => ref _r.SP;
        public ref ushort PC => ref _r.PC;
        #endregion                

        #region Utility Functions
        private void InternalDelay(int cycles = 1) => _cycles += cycles * 4;

        private byte DecodeInstruction() { InternalDelay(); return _d.MMU[_r.Bump()]; }

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

        private void InterruptVec(byte vec)
        {
            _halted = false;

            if (_ime == IME.Enabled)
            {
                InternalDelay(3);
                Push(PC);
                PC = vec;
                _ime = IME.Disabled;
                _d.IRQ.IF = 0x00;
            }
        }
        #endregion

        #region Internal Members
        private Registers _r;
        private Device _d;
        private IME _ime;

        private int _cycles;
        private bool _halted;
        #endregion;

        public Z80(Device d)
        {
            _d = d;
            _cycles = 0;
            _r = new Registers();
            _halted = false;
        }

        public int Step()
        {
            _cycles = 0;

            if (!_halted)
            {
                DecodeStep();
            }

            else
            {
                HaltedStep();
            }

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

        #region Instruction Implementations
        public void Load(int t1, int i1, int t2, int i2)
        {
            this[t1, i1] = this[t2, i2];
        }

        public void Load8(int i)
        {
            this[R, i] = NB;
        }

        public void Load16(int i)
        {
            this[RP, i] = NW;
        }

        public void LoadHL()
        {
            throw new NotImplementedException();
        }

        public void LoadSP()
        {
            throw new NotImplementedException();
        }

        public void JumpAbsolute()
        {
            throw new NotImplementedException();
        }

        public void JumpAbsolute(int i)
        {
            throw new NotImplementedException();
        }

        public void JumpRelative()
        {
            sbyte r = (sbyte)NB;

            InternalDelay();
            PC = (ushort)(PC + r);
        }

        public void JumpRelative(int i)
        {
            sbyte r = (sbyte)NB;

            if (_r.CanJump(CC[i]))
            {
                InternalDelay();
                PC = (ushort)(PC + r);
            }
        }

        public void JumpHL()
        {
            throw new NotImplementedException();
        }

        public void AddHL(int i)
        {
            throw new NotImplementedException();
        }

        public void Increment(int t, int i)
        {
            var v = this[t, i];
            var r = v + 1;

            this[t, i] = r;

            if (t == R)
            {
                _r[Registers.Flags.Z] = r == 0;
                _r[Registers.Flags.N] = false;
                _r[Registers.Flags.H] = r % 16 == 0;
            }
        }

        public void Decrement(int t, int i)
        {
            var v = this[t, i];
            var r = v - 1;

            this[t, i] = r;

            if (t == R)
            {
                _r[Registers.Flags.Z] = r == 0;
                _r[Registers.Flags.N] = true;
                _r[Registers.Flags.H] = r % 16 == 15;
            }
        }

        public void Rla()
        {
            int r = A;
            var bit7 = Utility.Bit.BitValue(A, 7);
            var t = _r[Registers.Flags.C];

            _r[Registers.Flags.C] = bit7 == 1;

            r = r << 1;
            r |= Convert.ToInt32(t);

            A = (byte)r;

            _r[Registers.Flags.Z] = false;
            _r[Registers.Flags.N] = false;
            _r[Registers.Flags.H] = false;
        }

        public void Rlca()
        {
            throw new NotImplementedException();
        }

        public void Rra()
        {
            throw new NotImplementedException();
        }

        public void Rrca()
        {
            throw new NotImplementedException();
        }

        public void Daa()
        {
            throw new NotImplementedException();
        }

        public void Cpl()
        {
            throw new NotImplementedException();
        }

        public void Scf()
        {
            throw new NotImplementedException();
        }

        public void Ccf()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void Halt()
        {
            throw new NotImplementedException();
        }

        public void Add(int i)
        {
            throw new NotImplementedException();

            //var v = this[R, i];
            //var r = A + v;

            //var hc = ((A & 0xF) + (v & 0xF) & 0x10) == 0x10;
            //var c = (A + v) > byte.MaxValue;

            //_r[Registers.Flags.Z] = r == 0;
            //_r[Registers.Flags.N] = false;
            //_r[Registers.Flags.H] = hc;
            //_r[Registers.Flags.C] = c;

            //this[R, i] = r;
        }

        public void Adc(int i)
        {
            throw new NotImplementedException();
        }

        public void Sub(int i)
        {
            var v = this[R, i];
            var r = A - v;

            var hc = (A & 0xF) < (v & 0xF);
            var c = A < v;

            _r[Registers.Flags.Z] = r == 0;
            _r[Registers.Flags.N] = true;
            _r[Registers.Flags.H] = hc;
            _r[Registers.Flags.C] = c;

            this[R, i] = r;
        }

        public void Sbc(int i)
        {
            throw new NotImplementedException();
        }

        public void And(int i)
        {
            throw new NotImplementedException();
        }

        public void Xor(int i)
        {
            A = (byte)(A ^ this[R, i]);

            _r[Registers.Flags.Z] = (A == 0);
            _r[Registers.Flags.N] = false;
            _r[Registers.Flags.H] = false;
            _r[Registers.Flags.C] = false;
        }

        public void Or(int i)
        {
            throw new NotImplementedException();
        }

        public void Cp(int i)
        {
            var v = this[R, i];
            var r = A - v;
            var hc = (A & 0xF) < (v & 0xF);
            var c = A < v;

            _r[Registers.Flags.Z] = r == 0;
            _r[Registers.Flags.N] = true;
            _r[Registers.Flags.H] = hc;
            _r[Registers.Flags.C] = c;
        }

        public void Call()
        {
            var nw = NW;
            Push(PC);
            PC = nw;
        }

        public void Call(int i)
        {
            throw new NotImplementedException();
        }

        public void Ret()
        {
            InternalDelay();
            PC = Pop();
        }

        public void Ret(int i)
        {
            throw new NotImplementedException();
        }

        public void Reti()
        {
            throw new NotImplementedException();
        }

        public void AddSP()
        {
            throw new NotImplementedException();
        }

        public void Push(int i)
        {
            InternalDelay();
            Push((ushort)this[RP2, i]);
        }

        public void Pop(int i)
        {
            this[RP2, i] = Pop();
        }

        public void Di()
        {
            throw new NotImplementedException();
        }

        public void Ei()
        {
            throw new NotImplementedException();
        }

        public void Nop()
        {
        }

        public void Add8()
        {
            throw new NotImplementedException();
        }

        public void Adc8()
        {
            throw new NotImplementedException();
        }

        public void Sub8()
        {
            throw new NotImplementedException();
        }

        public void Sbc8()
        {
            throw new NotImplementedException();
        }

        public void And8()
        {
            throw new NotImplementedException();
        }

        public void Xor8()
        {
            throw new NotImplementedException();
        }

        public void Or8()
        {
            throw new NotImplementedException();
        }

        public void Cp8()
        {
            var v = NB;
            var r = A - v;

            var hc = (A & 0xF) < (v & 0xF);
            var c = A < v;

            _r[Registers.Flags.Z] = r == 0;
            _r[Registers.Flags.N] = true;
            _r[Registers.Flags.H] = hc;
            _r[Registers.Flags.C] = c;
        }

        public void Rst(ushort adr)
        {
            throw new NotImplementedException();
        }

        public void Rlc(int i)
        {
            throw new NotImplementedException();
        }

        public void Rrc(int i)
        {
            throw new NotImplementedException();
        }

        public void Rl(int i)
        {
            var r = this[R, i];
            var bit7 = Utility.Bit.BitValue((byte)r, 7);
            var t = _r[Registers.Flags.C];

            _r[Registers.Flags.C] = bit7 == 1;

            r <<= 1;

            r |= Convert.ToInt32(t);

            this[R, i] = r;

            _r[Registers.Flags.Z] = r == 0;
            _r[Registers.Flags.N] = false;
            _r[Registers.Flags.H] = false;
        }

        public void Rr(int i)
        {
            throw new NotImplementedException();
        }

        public void Sla(int i)
        {
            throw new NotImplementedException();
        }

        public void Sra(int i)
        {
            throw new NotImplementedException();
        }

        public void Swap(int i)
        {
            throw new NotImplementedException();
        }

        public void Srl(int i)
        {
            throw new NotImplementedException();
        }

        public void Bit(int n, int i)
        {
            var r = (this[R, i] >> n) & 1;

            _r[Registers.Flags.Z] = r == 0;
            _r[Registers.Flags.N] = false;
            _r[Registers.Flags.H] = true;
        }

        public void Res(int n, int i)
        {
            throw new NotImplementedException();
        }

        public void Set(int n, int i)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}