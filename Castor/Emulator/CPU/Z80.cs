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
                H = value.MostSignificantByte();
                L = value.LeastSignificantByte();
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

        private delegate void Instruction();
        private Instruction[] _operations;
        private Instruction[] _bitwiseOperations;

        public Z80(GameboySystem system)
        {
            _system = system;
            PC = 0;
        }

        public void Step()
        {
            if (_cyclesToWait > 0)
            {
                --_cyclesToWait;
            }
            else
            {
                int cyclesToAdd = 0;
                _operations[ReadByte(PC)]();
                _cyclesToWait += cyclesToAdd;
            }
        }

        private byte ReadByte(int addr)
        {
            byte ret = _system.MMU[addr];
            PC++;
            return ret;
        }

        private ushort ReadUshort(int addr)
        {
            ushort ret = Convert.ToUInt16(_system.MMU[addr + 1] << 8 | _system.MMU[addr]);
            PC += 2;
            return ret;
        }

        #region Instructions
        /*
         * Guide
         * R8  = 8-bit Register
         * R16 = 16-bit Register
         * D8  = Direct-value 8-bit
         * D16 = Direct-value 16-bit
         * S8  = Signed-value 8-bit
         * PR8 = Pointer 8-bit register
         * PR16 = Pointer 16-bit register
         * PD16 = Pointer Direct 16-bit register
         * PD8  = Pointer Direct 8-bit register
         */        
        private void OP_LD_R16_D16(ref byte rh, ref byte rl)
        {
            rl = ReadByte(PC);
            rh = ReadByte(PC);            
        }
        private void OP_PR16_R8(ref byte r16_h, ref byte r16_l, ref byte r8)
        {
            r8 = ReadByte(r16_h << 8 | r16_l);
        }
        private void OP_INC_R16(ref byte rh, ref byte rl)
        {
            ushort value = (ushort)(rh << 8 | rl);
            value++;
            rh = (byte)((value >> 8) & 0xFF);
            rl = (byte)((value >> 0) & 0xFF);
        }
        #endregion
    }
}
