using Castor.Emulator.Utility;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Castor.Emulator.Memory;

namespace Castor.Emulator.CPU
{
    public partial class Z80
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

#if DEBUG
        private ushort Peek()
        {
            InternalDelay(-1);
            var ret = ReadWord(SP);
            return ret;
        }
#endif

        private void InterruptVec(byte vec, InterruptFlags flag)
        {
            _halted = false;

            if (_ime == InterruptMasterEnable.Enabled)
            {
                InternalDelay(3);
                Push(PC);
                PC = vec;
                _ime = InterruptMasterEnable.Disabled;
                _d.IRQ.DisableInterrupt(flag);
            }
        }
        #endregion

        #region Internal Members
        private Registers _r;
        private Device _d;
        private InterruptMasterEnable _ime;

        private int _cycles;
        private bool _halted;
        #endregion;

        #region Constructor
        public Z80(Device d)
        {
            _d = d;
            _cycles = 0;
            _r = new Registers();
            _halted = false;
            _ime = InterruptMasterEnable.Disabled;
        }
        #endregion

        #region Step Methods
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
                    case InterruptMasterEnable.Enabling:
                        _ime = InterruptMasterEnable.Enabled;
                        break;
                }
            }

            return _cycles;
        }

        private void HaltedStep()
        {
            //HandleInterrupts();
            InternalDelay();
        }

        private void DecodeStep()
        {
            //HandleInterrupts();
            var opcode = DecodeInstruction();
            Decode(opcode);
        }

        private void HandleInterrupts()
        {
            if (_d.IRQ.CanServiceInterrupts) // if any interrupts are available
            {
                if (_d.IRQ.CanHandleInterrupt(InterruptFlags.VBL))
                    InterruptVec(0x40, InterruptFlags.VBL);
                else if (_d.IRQ.CanHandleInterrupt(InterruptFlags.STAT))
                    InterruptVec(0x48, InterruptFlags.STAT);
                else if (_d.IRQ.CanHandleInterrupt(InterruptFlags.Timer))
                    InterruptVec(0x50, InterruptFlags.Timer);
                else if (_d.IRQ.CanHandleInterrupt(InterruptFlags.Serial))
                    InterruptVec(0x58, InterruptFlags.Serial);
                else if (_d.IRQ.CanHandleInterrupt(InterruptFlags.Joypad))
                    InterruptVec(0x60, InterruptFlags.Joypad);
            }
        }
        #endregion

        #region Instruction Implementations
        public void Adc(int i)
        {
            AluAdd(this[R, i], true);
        }

        public void Adc8()
        {
            AluAdd(N8, true);
        }

        public void Add(int i)
        {
            AluAdd(this[R, i], false);
        }

        public void Add8()
        {
            AluAdd(N8, false);
        }

        public void AddHL(int i)
        {
            InternalDelay();

            AluAddHL(this[RP, i]);
        }

        public void AddSP()
        {
            InternalDelay(2);

            AluAddSP(N8);
        }



        public void Load(int t1, int i1, int t2, int i2)
        {
            this[t1, i1] = this[t2, i2];
        }

        public void Load8(int i)
        {
            this[R, i] = N8;
        }

        public void Load16(int i)
        {
            this[RP, i] = N16;
        }

        public void LoadHL()
        {
            throw new NotImplementedException();
        }

        public void LoadSP()
        {
            throw new NotImplementedException();
        }

        public void JP()
        {
            ushort a = N16;

            InternalDelay();
            PC = a;
        }

        public void JP(int i)
        {
            ushort a = N16;

            if (_r.CanJump(CC[i]))
            {
                InternalDelay();
                PC = a;
            }
        }

        public void JR()
        {
            sbyte r = (sbyte)N8;

            InternalDelay();
            PC = (ushort)(PC + r);
        }

        public void JR(int i)
        {
            sbyte r = (sbyte)N8;

            if (_r.CanJump(CC[i]))
            {
                InternalDelay();
                PC = (ushort)(PC + r);
            }
        }

        public void JPHL()
        {
            PC = HL;
        }

        

        public void Inc(int t, int i)
        {
            var v = this[t, i];
            var r = v + 1;

            this[t, i] = r;

            if (t == R)
            {
                _r[Registers.Flags.Z] = (byte)r == 0;
                _r[Registers.Flags.N] = false;
                _r[Registers.Flags.H] = (byte)r % 16 == 0;
            }
        }

        public void Dec(int t, int i)
        {
            var v = this[t, i];
            var r = v - 1;

            this[t, i] = r;

            if (t == R)
            {
                _r[Registers.Flags.Z] = (byte)r == 0;
                _r[Registers.Flags.N] = true;
                _r[Registers.Flags.H] = (byte)r % 16 == 15;
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
            int r = A;
            var bit0 = Utility.Bit.BitValue(A, 0);

            _r[Registers.Flags.C] = bit0 == 1;

            r = r << 1;
            r |= bit0;

            A = (byte)r;

            _r[Registers.Flags.Z] = false;
            _r[Registers.Flags.N] = false;
            _r[Registers.Flags.H] = false;
        }

        public void Daa()
        {
            throw new NotImplementedException();
        }

        public void Cpl()
        {
            A = (byte)(A ^ 0xFF);

            _r[Registers.Flags.N] = true;
            _r[Registers.Flags.H] = true;
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
            //_halted = true;
        }

        

        

        public void Sub(int i)
        {
            var v = this[R, i];
            var r = A - v;

            var hc = (A & 0xF) < (v & 0xF);
            var c = A < v;

            _r[Registers.Flags.Z] = (byte)r == 0;
            _r[Registers.Flags.N] = true;
            _r[Registers.Flags.H] = hc;
            _r[Registers.Flags.C] = c;

            A = (byte)r;
        }

        public void Sbc(int i)
        {
            throw new NotImplementedException();
        }

        public void And(int i)
        {
            A = (byte)(A & this[R, i]);

            _r[Registers.Flags.Z] = A == 0;
            _r[Registers.Flags.N] = false;
            _r[Registers.Flags.H] = true;
            _r[Registers.Flags.C] = false;
        }

        public void Xor(int i)
        {
            A = (byte)(A ^ this[R, i]);

            _r[Registers.Flags.Z] = A == 0;
            _r[Registers.Flags.N] = false;
            _r[Registers.Flags.H] = false;
            _r[Registers.Flags.C] = false;
        }

        public void Or(int i)
        {
            A = (byte)(A | this[R, i]);

            _r[Registers.Flags.Z] = A == 0;
            _r[Registers.Flags.N] = false;
            _r[Registers.Flags.H] = false;
            _r[Registers.Flags.C] = false;
        }

        public void Cp(int i)
        {
            var v = this[R, i];
            var r = A - v;
            var hc = (A & 0xF) < (v & 0xF);
            var c = A < v;

            _r[Registers.Flags.Z] = (byte)r == 0;
            _r[Registers.Flags.N] = true;
            _r[Registers.Flags.H] = hc;
            _r[Registers.Flags.C] = c;
        }

        public void Call()
        {
            var nw = N16;
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
            InternalDelay();

            if (_r.CanJump(CC[i]))
            {
                InternalDelay();
                PC = Pop();
            }
        }

        public void Reti()
        {
            _ime = InterruptMasterEnable.Enabling;
            Ret();
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
            _ime = InterruptMasterEnable.Disabled;
        }

        public void Ei()
        {
            _ime = InterruptMasterEnable.Enabling;
        }

        public void Nop()
        {
        }

        

        

        public void Sub8()
        {
            var v = N8;
            var r = A - v;

            var hc = (A & 0xF) < (v & 0xF);
            var c = A < v;

            _r[Registers.Flags.Z] = (byte)r == 0;
            _r[Registers.Flags.N] = true;
            _r[Registers.Flags.H] = hc;
            _r[Registers.Flags.C] = c;

            A = (byte)r;
        }

        public void Sbc8()
        {
            throw new NotImplementedException();
        }

        public void And8()
        {
            A = (byte)(A & N8);

            _r[Registers.Flags.Z] = A == 0;
            _r[Registers.Flags.N] = false;
            _r[Registers.Flags.H] = true;
            _r[Registers.Flags.C] = false;
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
            var v = N8;
            var r = A - v;

            var hc = (A & 0xF) < (v & 0xF);
            var c = A < v;

            _r[Registers.Flags.Z] = (byte)r == 0;
            _r[Registers.Flags.N] = true;
            _r[Registers.Flags.H] = hc;
            _r[Registers.Flags.C] = c;
        }

        public void Rst(ushort adr)
        {
            InternalDelay();
            Push(PC);
            PC = adr;
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

            _r[Registers.Flags.Z] = (byte)r == 0;
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
            var v = this[R, i];

            var hi = (byte)(v >> 4) & 0xF;
            var lo = (byte)(v >> 0) & 0xF;

            var r = (lo << 4 | hi);

            this[R, i] = r;

            _r[Registers.Flags.Z] = (byte)r == 0;
            _r[Registers.Flags.N] = false;
            _r[Registers.Flags.H] = false;
            _r[Registers.Flags.C] = false;
        }

        public void Srl(int i)
        {
            throw new NotImplementedException();
        }

        public void Bit(int n, int i)
        {
            var r = (this[R, i] >> n) & 1;

            _r[Registers.Flags.Z] = (byte)r == 0;
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