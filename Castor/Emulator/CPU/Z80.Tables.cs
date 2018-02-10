using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.CPU
{
    public partial class Z80
    {
        private ushort N16
        {
            get => ReadWord(_r.Bump(2));
        }

        private byte N8
        {
            get => ReadByte(_r.Bump());
        }

        private sbyte E8
        {
            get => (sbyte)ReadByte(_r.Bump());
        }

        private ushort HLI
        {
            get => _r.BumpHL(1);
        }

        private ushort HLD
        {
            get => _r.BumpHL(-1);
        }

        #region Register Dissassembly Tables
        const int R = 0;
        const int RP = 1;
        const int RP2 = 2;
        const int ADDR = 6;

        const int b = 0;
        const int c = 1;
        const int d = 2;
        const int e = 3;
        const int h = 4;
        const int l = 5;
        const int _hl = 6;
        const int a = 7;

        const int bc = 0;
        const int de = 1;
        const int hl = 2;
        const int sp = 3;
        const int af = 3;

        int this[int table, int index]
        {
            get
            {
                switch (table)
                {
                    case R:
                        {
                            switch (index)
                            {
                                case b:
                                    return B;
                                case c:
                                    return C;
                                case d:
                                    return D;
                                case e:
                                    return E;
                                case h:
                                    return H;
                                case l:
                                    return L;
                                case _hl:
                                    return ReadByte(HL);
                                case a:
                                    return A;
                                default:
                                    return 0;
                            }
                        }
                    case RP:
                        {
                            switch (index)
                            {
                                case bc:
                                    return BC;
                                case de:
                                    return DE;
                                case hl:
                                    return HL;
                                case sp:
                                    return SP;
                                default:
                                    return 0;
                            }
                        }
                    case RP2:
                        {
                            switch (index)
                            {
                                case bc:
                                    return BC;
                                case de:
                                    return DE;
                                case hl:
                                    return HL;
                                case af:
                                    return AF;
                                default:
                                    return 0;
                            }
                        }

                    case ADDR: return ReadByte(index);
                }

                return 0;
            }

            set
            {
                switch (table)
                {
                    case R:
                        {
                            switch (index)
                            {
                                case b:
                                    B = (byte)value;
                                    break;
                                case c:
                                    C = (byte)value;
                                    break;
                                case d:
                                    D = (byte)value;
                                    break;
                                case e:
                                    E = (byte)value;
                                    break;
                                case h:
                                    H = (byte)value;
                                    break;
                                case l:
                                    L = (byte)value;
                                    break;
                                case _hl:
                                    WriteByte(HL, (byte)value);
                                    break;
                                case a:
                                    A = (byte)value;
                                    break;
                            }

                            break;
                        }
                    case RP:
                        {
                            switch (index)
                            {
                                case bc:
                                    BC = (ushort)value;
                                    break;
                                case de:
                                    DE = (ushort)value;
                                    break;
                                case hl:
                                    HL = (ushort)value;
                                    break;
                                case sp:
                                    SP = (ushort)value;
                                    break;
                            }

                            break;
                        }
                    case RP2:
                        {
                            switch (index)
                            {
                                case bc:
                                    BC = (ushort)value;
                                    break;
                                case de:
                                    DE = (ushort)value;
                                    break;
                                case hl:
                                    HL = (ushort)value;
                                    break;
                                case af:
                                    AF = (ushort)value;
                                    break;

                            }

                            break;
                        }

                    case ADDR: WriteByte(index, (byte)value); break;
                }
            }
        }
        #endregion

        #region Condition Dissassembly Table
        Cond[] CC = new Cond[] { Cond.NZ, Cond.Z, Cond.NC, Cond.C };
        #endregion
    }
}
