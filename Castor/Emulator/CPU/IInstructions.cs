using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.CPU
{
    public interface IInstructions
    {
        void Load(int t1, int i1, int t2, int i2);
        void Load8(int t, int i);
        void Load16(int t, int i);
        void LoadHL();
        void LoadSP();

        void JumpAbsolute();
        void JumpAbsolute(int i);
        void JumpRelative();
        void JumpRelative(int i);
        void JumpHL();

        void AddHL(int i);

        void Increment(int t, int i);
        void Decrement(int t, int i);

        void Rla();
        void Rlca();
        void Rra();
        void Rrac();
        void Daa();
        void Cpl();
        void Scf();
        void Ccf();

        void Stop();
        void Halt();

        void Add(int i);
        void Adc(int i);
        void Sub(int i);
        void Sbc(int i);
        void And(int i);
        void Xor(int i);
        void Or(int i);
        void Cp(int i);

        void Add();
        void Adc();
        void Sub();
        void Sbc();
        void And();
        void Xor();
        void Or();
        void Cp();

        void Call();
        void Call(int i);
        void Rst(ushort adr);
        void Ret();
        void Ret(int i);
        void Reti();

        void AddSP();

        void Push(int i);
        void Pop(int i);

        void Di();
        void Ei();

        void Nop();

        void Rlc(int i);
        void Rrc(int i);
        void Rl(int i);
        void Rr(int i);
        void Sla(int i);
        void Sra(int i);
        void Swap(int i);
        void Srl(int i);

        void Bit(int n, int i);
        void Res(int n, int i);
        void Set(int n, int i);
    }
}
