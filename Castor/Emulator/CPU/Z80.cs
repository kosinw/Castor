using Castor.Emulator.Memory;
using Castor.Emulator.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.CPU
{
    public partial class Z80
    {
        #region Registers       
        public byte A;
        public byte F;
        public byte B;
        public byte C;
        public byte D;
        public byte E;
        public byte H;
        public byte L;
        
        public ushort AF
        {
            get => Convert.ToUInt16(A << 8 | F);
            set
            {
                A = value.MostSignificantByte();
                F = value.LeastSignificantByte();
            }
        }
        public ushort BC
        {
            get => Convert.ToUInt16(B << 8 | C);
            set
            {
                B = value.MostSignificantByte();
                C = value.LeastSignificantByte();
            }
        }
        public ushort DE
        {
            get => Convert.ToUInt16(D << 8 | E);
            set
            {
                D = value.MostSignificantByte();
                E = value.LeastSignificantByte();
            }
        }
        public ushort HL
        {
            get => Convert.ToUInt16(H << 8 | L);
            set
            {
                B = value.MostSignificantByte();
                C = value.LeastSignificantByte();
            }
        }
        public ushort SP;
        public ushort PC;
        #endregion

        #region Private Members
        /// <summary>
        /// This counts how many cycles need to be waited until the next instruction is executed.
        /// </summary>
        private int _cyclesToWait = 0;

        /// <summary>
        /// This is a reference to the overarching class of all components
        /// </summary>
        private GameboySystem _system { get; set; }
        #endregion

        public Z80(GameboySystem system)
        {
            _system = system;
            PC = 0;
        }

        public void Step()
        {
            if (_cyclesToWait > 0)
                --_cyclesToWait;
            else
                _cyclesToWait = ExecuteNextInstruction();
        }

        private byte ReadByte()
        {            
            byte ret = _system.MMU[++PC];
            return ret;
        }
        
        // gameboy is little-endian (reverse order)
        private ushort ReadUshort()
        {            
            ushort ret = Convert.ToUInt16(_system.MMU[PC + 2] << 8 | _system.MMU[PC + 1]);
            PC += 2;
            return ret;
        }

        private int ExecuteNextInstruction()
        {
            int cyclesToWait = Execute(_system.MMU[PC]);
            ++PC;

            return cyclesToWait;
        }

        public int Execute(byte opcode)
        {
            switch (opcode)
            {
                case 0x05:
                    DecrementR8(ref B);
                    return 4;
                case 0x06:
                    LoadIntoBRegister(ReadByte());
                    return 8;
                case 0x0C:
                    IncrementR8(ref C);
                    return 4;
                case 0x0E:
                    LoadIntoCRegister(ReadByte());
                    return 8;
                case 0x11:
                    LoadIntoDERegister(ReadUshort());
                    return 12;
                case 0x13:
                    IncrementDE();
                    return 8;
                case 0x17:
                    BitRotateLeftCarryA();
                    return 4;
                case 0x1A:
                    LoadValueDEIntoA();
                    return 8;
                case 0x20:
                    JumpRelativeIfNotZero();
                    return 8;
                case 0x21:
                    LoadIntoHLRegister(ReadUshort());
                    return 12;
                case 0x22:
                    LoadAIntoHLPointerPostIncrement();
                    return 8;
                case 0x23:
                    IncrementHL();
                    return 8;
                case 0x31:
                    LoadIntoSPRegister(ReadUshort());
                    return 12;
                case 0x32:
                    LoadAIntoHLPointerPostDecrement();
                    return 8;
                case 0x3E:
                    LoadIntoARegister(ReadByte());
                    return 8;
                case 0x4F:
                    LoadIntoARegister(C);
                    return 4;
                case 0x77:
                    LoadAIntoHLPointer();
                    return 8;
                case 0x7B:
                    LoadIntoARegister(E);
                    return 4;
                case 0xAF:
                    XORAWithA();
                    return 4;
                case 0xC1:
                    PopBCOffStack();
                    return 12;
                case 0xC5:
                    PushPairOntoStack(BC);
                    return 16;
                case 0xC9:
                    ReturnSubroutine();
                    return 16;
                case 0xCB:
                    int nextOpcode = ReadByte();
                    switch (nextOpcode)
                    {
                        case 0x11:
                            BitRotateLeftCarryC();
                            return 8;
                        case 0x7C:
                            CheckBIT(7, H);
                            return 8;
                    }
                    return 4;
                case 0xCD:
                    CallSubroutine(ReadUshort());
                    return 12;
                case 0xE0:
                    LoadARegisterIntoAddress8(ReadByte());
                    return 12;
                case 0xE2:
                    LoadARegisterIntoCPointer();
                    return 8;
                case 0xEA:
                    LoadARegisterIntoAddress16(ReadUshort());
                    return 16;
                case 0xFE:
                    CompareWithA(ReadByte());
                    return 8;
            }
            throw new Exception("This opcode is not implemented!");
        }
    }
}
