using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.CPU
{
    public partial class Z80
    {
        public void Decode(byte op)
        {
            int z = (op & 0b00_000_111) >> 0;
            int y = (op & 0b00_111_000) >> 3;
            int x = (op & 0b11_000_000) >> 6;
            int p = (op & 0b00_110_000) >> 4;
            int q = (op & 0b00_001_000) >> 3;

            if (x == 0)
            {
                switch (z)
                {
                    #region Relative Jumps and Assorted Operations
                    case 0:
                        {
                            switch (y)
                            {
                                case 0: Nop(); return;
                                case 1: Load(ADR, NW, RP, sp); return;
                                case 2: Stop(); return;
                                case 3: JumpRelative(); return;
                                case var r when r >= 4 && r <= 7: JumpRelative(y - 4); return;
                            }

                            break;
                        }
                    #endregion

                    #region 16-bit Load Immediate/Add
                    case 1:
                        {
                            switch (q)
                            {
                                case 0: Load16(RP, p); return;
                                case 1: AddHL(p); return;
                            }

                            break;
                        }
                    #endregion

                    #region Indirect Loading
                    case 2:
                        {
                            switch ((q << 2 | p))
                            {
                                case 0: Load(ADR, BC, R, a); return;
                                case 1: Load(ADR, DE, R, a); return;
                                case 2: Load(ADR, HLI, R, a); return;
                                case 3: Load(ADR, HLD, R, a); return;

                                case 4: Load(R, a, ADR, BC); return;
                                case 5: Load(R, a, ADR, DE); return;
                                case 6: Load(R, a, ADR, HLI); return;
                                case 7: Load(R, a, ADR, HLD); return;
                            }

                            break;
                        }
                    #endregion

                    #region 16-bit Increment/Decrement
                    case 3:
                        {
                            switch (q)
                            {
                                case 0: Increment(RP, p); return;
                                case 1: Decrement(RP, p); return;
                            }
                            break;
                        }
                    #endregion

                    #region 8-bit Increment
                    case 4: Increment(R, y); return;
                    #endregion

                    #region 8-bit Decrement
                    case 5: Decrement(R, y); return;
                    #endregion

                    #region 8-bit Load Immediate
                    case 6: Load8(R, y); return;
                    #endregion

                    #region Assorted Operations on Accumulator/Flags
                    case 7:
                        {
                            switch (y)
                            {
                                case 0: Rlca(); return;
                                case 1: Rrac(); return;
                                case 2: Rla(); return;
                                case 3: Rrac(); return;
                                case 4: Daa(); return;
                                case 5: Cpl(); return;
                                case 6: Scf(); return;
                                case 7: Ccf(); return;
                            }

                            break;
                        }
                        #endregion
                }
            }

            else if (x == 1)
            {
                #region Halt 
                if (z == 6)
                {
                    Halt();
                    return;
                }
                #endregion

                #region 8-bit Loading
                else
                {
                    Load(R, y, R, z);
                    return;
                }
                #endregion
            }

            else if (x == 2)
            {
                #region Operate on accumulator and register/memory location
                switch (y)
                {
                    case 0: Add(z); return;
                    case 1: Adc(z); return;
                    case 2: Sub(z); return;
                    case 3: Sbc(z); return;
                    case 4: And(z); return;
                    case 5: Xor(z); return;
                    case 6: Or(z); return;
                    case 7: Cp(z); return;
                }
                #endregion
            }

            else if (x == 3)
            {
                switch (z)
                {
                    #region Assorted Instructions
                    case 0:
                        {
                            switch (y)
                            {
                                case var r when r >= 0 && r <= 3: Ret(y); return;
                                case 4: Load(ADR, NB + 0xFF00, R, a); return;
                                case 5: AddSP(); return;
                                case 6: Load(R, a, ADR, NB + 0xFF00); return;
                                case 7: LoadHL(); return;
                            }

                            break;
                        }
                    #endregion

                    #region Pop and Various Instructions
                    case 1:
                        {
                            switch ((q << 2 | p))
                            {
                                case 0: Pop(y); return;
                                case 4: Ret(); return;
                                case 5: Reti(); return;
                                case 6: JumpHL(); return;
                                case 7: LoadSP(); return;
                            }

                            break;
                        }
                    #endregion

                    #region Conditional Jumps + Load A from Mem-mapped Region
                    case 2:
                        {
                            switch (y)
                            {
                                case var r when r >= 0 && r <= 3: JumpAbsolute(y); return;
                                case 4: Load(ADR, C + 0xFF00, R, a); return;
                                case 5: Load(ADR, NW, R, a); return;
                                case 6: Load(R, a, ADR, C + 0xFF00); return;
                                case 7: Load(R, a, ADR, NW); return;
                            }

                            break;
                        }
                    #endregion

                    #region More Assorted Instructions
                    case 3:
                        {
                            switch (y)
                            {
                                case 0: JumpAbsolute(); return;
                                case 1: DecodeCB(); return;
                                case 6: Di(); return;
                                case 7: Ei(); return;
                            }

                            break;
                        }
                    #endregion

                    #region Conditional Call
                    case 4:
                        {
                            if (y >= 0 && y <= 3)
                            {
                                Call(y);
                                return;
                            }

                            break;
                        }
                    #endregion

                    #region Push and Call
                    case 5:
                        {
                            switch ((q << 2 | p))
                            {
                                case 0: Push(y); return;
                                case 4: Call(); return;
                            }

                            break;
                        }
                    #endregion

                    #region Operate on Akk and Imm Operand
                    case 6:
                        {
                            switch (y)
                            {
                                case 0: Add(); return;
                                case 1: Adc(); return;
                                case 2: Sub(); return;
                                case 3: Sbc(); return;
                                case 4: And(); return;
                                case 5: Xor(); return;
                                case 6: Or(); return;
                                case 7: Cp(); return;
                            }

                            break;
                        }
                    #endregion

                    #region Restart
                    case 7: Rst((ushort)(y * 8)); return;
                        #endregion
                }
            }

            throw Unimplemented(op);
        }

        private void DecodeCB()
        {
            var op = DecodeInstruction();

            int z = (op & 0b00_000_111) >> 0;
            int y = (op & 0b00_111_000) >> 3;
            int x = (op & 0b11_000_000) >> 6;
            int p = (op & 0b00_110_000) >> 4;
            int q = (op & 0b00_001_000) >> 3;

            switch (x)
            {
                #region Roll/shift register or memory location
                case 0:
                    {
                        switch (y)
                        {
                            case 0: Rlc(z); return;
                            case 1: Rrc(z); return;
                            case 2: Rl(z); return;
                            case 3: Rr(z); return;
                            case 4: Sla(z); return;
                            case 5: Sra(z); return;
                            case 6: Swap(z); return;
                            case 7: Srl(z); return;
                        }


                        break;
                    }
                #endregion

                #region Test Bit
                case 1: Bit(y, z); return;
                #endregion

                #region Reset Bit
                case 2: Res(y, z); return;
                #endregion

                #region Set Bit
                case 3: Set(y, z); return;
                    #endregion
            }

            throw UnimplementedCB(op);
        }

        private Exception Unimplemented(byte op)
        {
            return new Exception($"Opcode not defined: 0x{op:X2} at PC: 0x{PC - 1:X4}.");
        }

        private Exception UnimplementedCB(byte op)
        {
            return new Exception($"Opcode not defined: 0xCB 0x{op:X2} at PC: 0x{PC - 2:X4}.");
        }
    }
}