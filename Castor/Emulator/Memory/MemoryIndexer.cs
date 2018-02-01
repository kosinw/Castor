using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.Memory
{
    public class MemoryIndexer
    {
        Device _d;

        public enum Type
        {
            Address,
            Register
        }

        public MemoryIndexer(Device d)
        {
            _d = d;
        }

        public byte this[Type t, int idx]
        {
            get
            {
                if (t == Type.Register)
                {
                    switch (idx)
                    {
                        case 0:
                            return _d.CPU.B;
                        case 1:
                            return _d.CPU.C;
                        case 2:
                            return _d.CPU.D;
                        case 3:
                            return _d.CPU.E;
                        case 4:
                            return _d.CPU.H;
                        case 5:
                            return _d.CPU.L;
                        case 6:
                            return _d.MMU[_d.CPU.HL];
                        case 7:
                            return _d.CPU.A;
                    }
                }
                else
                {
                    return _d.MMU[idx];
                }

                return 0;
            }

            set
            {
                if (t == Type.Register)
                {
                    switch (idx)
                    {
                        case 0:
                            _d.CPU.B = value;
                            break;
                        case 1:
                            _d.CPU.C = value;
                            break;
                        case 2:
                            _d.CPU.D = value;
                            break;
                        case 3:
                            _d.CPU.E = value;
                            break;
                        case 4:
                            _d.CPU.H = value;
                            break;
                        case 5:
                            _d.CPU.L = value;
                            break;
                        case 6:
                            _d.MMU[_d.CPU.HL] = value;
                            break;
                        case 7:
                            _d.CPU.A = value;
                            break;
                    }
                }
                else
                {
                    _d.MMU[idx] = value;
                }
            }
        }
    }
}