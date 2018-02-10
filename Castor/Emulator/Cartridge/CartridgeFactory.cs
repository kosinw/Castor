using System;

namespace Castor.Emulator.Cartridge
{
    public static class CartridgeFactory
    {
        public static ICartridge CreateCartridge(byte[] bytecode)
        {
            byte romTypeSelect = bytecode[0x147];

            switch (romTypeSelect)
            {
                case 0x00:
                    return new MBC0(bytecode);
                case 0x01:
                    return new MBC1(bytecode);
                default:
                    throw new Exception("Unsupported MBC Type was found!");
            }
        }
    }
}
