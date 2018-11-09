using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HP6050AInterface {
    /// <summary>
    /// Generif output parser. Base class for more specific output parsers
    /// </summary>
    public abstract class IOutputParser {
        private Communicator communicator;
        protected string buffer;

        protected IOutputParser(Communicator communicator) {
            this.communicator = communicator;
        }

        /// <summary>
        /// Get the output from a communicator, buffer it and process as needed
        /// </summary>
        /// <returns></returns>
        public bool process() {
            buffer += communicator.getOutput();
            bool res = handleOutput();
            if (res)
                buffer = "";
            return res;
        }

        /// <summary>
        /// Handle the output if needed
        /// </summary>
        /// <returns>True if there a complete set of new data from the output</returns>
        public abstract bool handleOutput();
    }

    public class BatteryTestOutputParser : IOutputParser{

        private double voltage = 0;
        private double current = 0;

        public BatteryTestOutputParser(Communicator comunicator) : base(comunicator) {

        }

        public override bool handleOutput() {
            bool isComplete = this.buffer.EndsWith("\n");
            if (isComplete) {
                //TODO: Set voltage and current values from read data
            }
            return isComplete;
        }

        public double getReadVoltage() {
            return voltage;
        }

        public double getReadCurrent() {
            return current;
        }

    }
}
