using Castor.Emulator.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Castor.Emulator.CPU.Types.ByteTypeEx;

namespace Castor.Emulator.CPU
{
    public class MicroOperation
    {
        /// <summary>
        /// This is a constant that states how much needs to be added to the C register when accessing memory.
        /// </summary>
        const int _C_OFFSET = 0xFF00;

        /// <summary>
        /// The amount to increment the program counter by.
        /// </summary>
        public int Parameters { get; set; }

        /// <summary>
        /// The amount of machine cycles this instruction took.
        /// </summary>
        public int MachineCycles { get; set; }

        /// <summary>
        /// The functor that will be invoked at runtime.
        /// The return type can be null if there is no return value.
        /// The first parameter represents the value of the last operation.
        /// The second parameter represents the current PC;
        /// The third parameter represents a reference to the Gameboy system.
        /// </summary>
        public Func<int?, int, GameboySystem, int?> Invoke { get; set; }
        
        /// <summary>
        /// A generated method that returns an eight-bit register.
        /// </summary>
        /// <returns></returns>
        public static MicroOperation Load8(ByteType type, GameboySystem system)
        {            
            int machineCycles = type.Delay();
            int parameters = type.Length();            

            return new MicroOperation
            {
                Parameters = parameters,
                MachineCycles = machineCycles,
                Invoke = (_, pc, sys) =>
                {
                    var cpu = sys.CPU;
                    var mmu = sys.MMU;

                    switch (type)
                    {
                        case ByteType.A:
                            return cpu.A;
                        case ByteType.B:
                            return cpu.B;
                        case ByteType.C:
                            return cpu.C;
                        case ByteType.D:
                            return cpu.D;
                        case ByteType.E:
                            return cpu.E;
                        case ByteType.F:
                            return cpu.F;
                        case ByteType.H:
                            return cpu.H;
                        case ByteType.L:
                            return cpu.L;
                        case ByteType.Imm8:
                            return mmu.ReadByte(pc);
                        case ByteType._HL:
                            return mmu.ReadByte(cpu.HL);                        
                        case ByteType._HLI:
                            return mmu.ReadByte(cpu.HL++);
                        case ByteType._HLD:
                            return mmu.ReadByte(cpu.HL--);
                        case ByteType._BC:
                            return mmu.ReadByte(cpu.BC);
                        case ByteType._DE:
                            return mmu.ReadByte(cpu.DE);
                        case ByteType._C:
                            return mmu.ReadByte(_C_OFFSET + cpu.C);
                        default:
                            throw new Exception("Invalid parameter.");
                    }
                }
            };
        }

        /// <summary>
        /// A generated method that loads the result of the previous operation in an eight-bit location.
        /// </summary>
        public static MicroOperation Store8(ByteType type, GameboySystem system)
        {
            int machineCycles = type.Delay();
            int parameters = type.Length();

            return new MicroOperation
            {
                Parameters = parameters,
                MachineCycles = machineCycles,
                Invoke = (byt, pc, sys) =>
                {
                    var cpu = sys.CPU;
                    var mmu = sys.MMU;

                    switch (type)
                    {
                        case ByteType.A:
                            cpu.A = (byte)byt;
                            break;
                        case ByteType.B:
                            cpu.B = (byte)byt;
                            break;
                        case ByteType.C:
                            cpu.C = (byte)byt;
                            break;
                        case ByteType.D:
                            cpu.D = (byte)byt;
                            break;
                        case ByteType.E:
                            cpu.E = (byte)byt;
                            break;
                        case ByteType.F:
                            cpu.F = (byte)byt;
                            break;
                        case ByteType.H:
                            cpu.H = (byte)byt;
                            break;
                        case ByteType.L:
                            cpu.L = (byte)byt;
                            break;
                        case ByteType._C:
                            mmu.WriteByte(_C_OFFSET + cpu.C, (byte)byt);
                            break;
                        case ByteType.Addr8:
                            mmu.WriteByte(mmu.ReadByte(pc), (byte)byt);
                            break;
                        case ByteType._HL:
                            mmu.WriteByte(cpu.HL, (byte)byt);
                            break;
                        case ByteType._BC:
                            return mmu.ReadByte(cpu.BC);
                        case ByteType._DE:
                            return mmu.ReadByte(cpu.DE);
                        case ByteType._HLD:
                            mmu.WriteByte(cpu.HL--, (byte)byt);
                            break;
                        case ByteType._HLI:
                            mmu.WriteByte(cpu.HL++, (byte)byt);
                            break;
                        case ByteType.Addr16:
                            mmu.WriteByte(mmu.ReadWord(pc), (byte)byt);
                            break;
                        default:
                            throw new Exception("Invalid Parameter");
                    }

                    return null;
                }
            };
        }
    }
}