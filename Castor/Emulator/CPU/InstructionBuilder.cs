using Castor.Emulator.CPU.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Castor.Emulator.CPU
{
    public class InstructionBuilder
    {
        List<MicroOperation> _microOperations;
        GameboySystem _system;

        public static InstructionBuilder DescibedAs(GameboySystem system)
        {
            return new InstructionBuilder
            {
                _system = system,
                _microOperations = new List<MicroOperation>()
            };
        }

        public InstructionBuilder Load8(ByteTypeEx.ByteType type)
        {
            _microOperations.Add(MicroOperation.Load8(type, _system));
            return this;
        }

        public InstructionBuilder Store8(ByteTypeEx.ByteType type)
        {
            _microOperations.Add(MicroOperation.Store8(type, _system));
            return this;
        }

        public Z80.Instruction Build() => () =>
        {
            int current_pc = _system.CPU.PC;
            int wait_time = 0;
            int? last_result = null;

            foreach (var op in _microOperations)
            {
                last_result = op.Invoke(last_result, current_pc, _system);

                current_pc += op.Parameters;
                wait_time += op.MachineCycles;
            }

            _system.CPU.AddWaitCycles(wait_time*4);
        };
    }
}
