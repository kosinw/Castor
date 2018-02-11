using Castor.Emulator.Utility;

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
            var ret = ReadWord(SP, 0);
            return ret;
        }
#endif        

        #endregion

        #region Internal Members
        private Registers _r;
        private Device _d;
        private InterruptMasterEnable _ime;

        private int _cycles;
        #endregion;

        #region Constructor
        public Z80(Device d)
        {
            _d = d;
            _cycles = 0;
            _r = new Registers();
            _ime = InterruptMasterEnable.Disabled;
        }
        #endregion

        #region Step Methods
        public int Step()
        {
            _cycles = 0;

            var opcode = DecodeInstruction();
            Decode(opcode);

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
        #endregion

        #region Instruction Implementations
        void Adc(int i)
        {
            A = AluAdd(this[R, i], true);
        }

        void Adc8()
        {
            A = AluAdd(N8, true);
        }

        void Add(int i)
        {
            A = AluAdd(this[R, i], false);
        }

        void Add8()
        {
            A = AluAdd(N8, false);
        }

        void AddHL(int i)
        {
            InternalDelay();
            HL = AluAddHL(this[RP, i]);
        }

        void AddSP()
        {
            InternalDelay(2);
            SP = AluAddSP(N8);
        }

        void And(int i)
        {
            A = AluAnd(this[R, i]);
        }

        void And8()
        {
            A = AluAnd(N8);
        }

        void Bit(int n, int i)
        {
            var operand = this[R, i];
            var result = (byte)((operand >> n) & 1);

            _r[Registers.Flags.Z] = result == 0;
            _r[Registers.Flags.N] = false;
            _r[Registers.Flags.H] = true;
        }

        void Call()
        {
            InternalDelay();

            var nw = N16;
            Push(PC);
            PC = nw;
        }

        public void Call(int i)
        {
            var nw = N16;

            if (_r.CanJump(CC[i]))
            {
                InternalDelay();
                Push(PC);
                PC = nw;
            }
        }

        public void Ccf()
        {
            _r[Registers.Flags.N] = false;
            _r[Registers.Flags.H] = false;
            _r[Registers.Flags.C] = !_r[Registers.Flags.C];
        }

        public void Cp(int i)
        {
            AluSub(this[R, i], false);
        }

        public void Cp8()
        {
            AluSub(N8, false);
        }

        public void Cpl()
        {
            A = (byte)(~A);

            _r[Registers.Flags.N] = true;
            _r[Registers.Flags.H] = true;
        }

        public void Daa()
        {
            var carry = false;

            if (_r[Registers.Flags.H] || (A & 0xF) > 0x9)
            {
                A += 0x6;
            }

            if (_r[Registers.Flags.C] || (A & 0xFF) > 0x90)
            {
                A += 0x60;
                carry = true;
            }

            _r[Registers.Flags.Z] = A == 0;
            _r[Registers.Flags.H] = false;
            _r[Registers.Flags.C] = carry;
        }

        public void Dec(int t, int i)
        {
            var operand = this[t, i];
            var result = (byte)(operand - 1);

            var h = ((operand & 0xF - 1) & 0x10) == 0x10;

            this[t, i] = result;

            if (t == R)
            {
                _r[Registers.Flags.Z] = result == 0;
                _r[Registers.Flags.N] = true;
                _r[Registers.Flags.H] = h;
            }
        }

        public void Di()
        {
            _ime = InterruptMasterEnable.Disabled;
        }

        public void Ei()
        {
            _ime = InterruptMasterEnable.Enabling;
        }

        public void Halt()
        {
            //_halted = true;
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
            sbyte r = E8;

            InternalDelay();
            PC = (ushort)(PC + r);
        }

        public void JR(int i)
        {
            sbyte r = E8;

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
            InternalDelay();

            HL = AluAddSP(E8);
        }

        public void LoadSP()
        {
            InternalDelay();

            SP = HL;
        }

        public void Or(int i)
        {
            A = AluOr(this[R, i]);
        }

        public void Or8()
        {
            A = AluOr(N8);
        }

        public void Pop(int i)
        {
            this[RP2, i] = Pop();
        }

        public void Push(int i)
        {
            InternalDelay();
            Push((ushort)this[RP2, i]);
        }

        public void Res(int n, int i)
        {
            var operand = (byte)this[R, i];

            this[R, i] = Utility.Bit.SetBit(operand, n);
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

        public void Rl(int i)
        {
            this[R, i] = AluRl(this[R, i], true);
        }

        public void Rla()
        {
            A = AluRl(A, true);
            _r[Registers.Flags.Z] = false;
        }

        public void Rlc(int i)
        {
            this[R, i] = AluRl(this[R, i], false);
        }

        public void Rlca()
        {
            A = AluRl(A, false);
            _r[Registers.Flags.Z] = false;
        }

        public void Rr(int i)
        {
            this[R, i] = AluRr(this[R, i], true);
        }

        public void Rrc(int i)
        {
            this[R, i] = AluRr(this[R, i], false);
        }

        public void Rra()
        {
            A = AluRr(A, true);
            _r[Registers.Flags.Z] = false;
        }

        public void Rrca()
        {
            A = AluRr(A, false);
            _r[Registers.Flags.Z] = false;
        }

        public void Rst(int addr)
        {
            ushort result = (ushort)(addr * 8);

            InternalDelay();
            Push(PC);
            PC = result;
        }

        public void Sbc(int i)
        {
            A = AluSub(this[R, i], true);
        }

        public void Sbc8()
        {
            A = AluSub(N8, true);
        }

        public void Scf()
        {
            _r[Registers.Flags.N] = false;
            _r[Registers.Flags.H] = false;
            _r[Registers.Flags.C] = true;
        }

        public void Set(int n, int i)
        {
            var operand = this[R, i];

            var result = Utility.Bit.SetBit((byte)operand, n);

            this[R, i] = result;
        }

        public void Sla(int i)
        {
            var operand = this[R, i];

            var shiftedBit = Utility.Bit.BitValue((byte)operand, 7);
            var result = (byte)(this[R, i] << 1);

            _r[Registers.Flags.Z] = result == 0;
            _r[Registers.Flags.N] = false;
            _r[Registers.Flags.H] = false;
            _r[Registers.Flags.C] = shiftedBit == 1;

            this[R, i] = result;
        }

        public void Sra(int i)
        {
            var operand = this[R, i];

            var bit7 = Utility.Bit.BitValue((byte)operand, 7);
            var shiftedBit = Utility.Bit.BitValue((byte)operand, 0);
            var result = (byte)((this[R, i] >> 1) | (bit7 << 7));

            _r[Registers.Flags.Z] = result == 0;
            _r[Registers.Flags.N] = false;
            _r[Registers.Flags.H] = false;
            _r[Registers.Flags.C] = shiftedBit == 1;

            this[R, i] = result;
        }

        public void Srl(int i)
        {
            var operand = this[R, i];

            var shiftedBit = Utility.Bit.BitValue((byte)operand, 0);
            var result = (byte)(this[R, i] >> 1);

            _r[Registers.Flags.Z] = result == 0;
            _r[Registers.Flags.N] = false;
            _r[Registers.Flags.H] = false;
            _r[Registers.Flags.C] = shiftedBit == 1;

            this[R, i] = result;
        }

        public void Stop()
        {
            // _halted = true;
        }

        public void Sub(int i)
        {
            A = AluSub(this[R, i], false);
        }

        public void Sub8()
        {
            A = AluSub(N8, false);
        }

        public void Swap(int i)
        {
            var operand = this[R, i];

            var hi = (byte)(operand >> 4) & 0xF;
            var lo = (byte)(operand >> 0) & 0xF;

            var result = (byte)(lo << 4 | hi);

            _r[Registers.Flags.Z] = result == 0;
            _r[Registers.Flags.N] = false;
            _r[Registers.Flags.H] = false;
            _r[Registers.Flags.C] = false;

            this[R, i] = result;
        }

        public void Xor(int i)
        {
            A = AluXor(this[R, i]);
        }

        public void Xor8()
        {
            A = AluXor(N8);
        }

        public void Nop()
        {
        }


        #endregion
    }
}