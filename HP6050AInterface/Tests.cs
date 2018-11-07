using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HP6050AInterface {

    /// <summary>
    /// Basic test interface. Implement to create a test
    /// </summary>
    public interface ITest {
        string getCommands();
    }

    public class BatteryTest : ITest {

        // Properties specific to a battery test
        double eodVoltage;
        double cellCount;
        double dischargeRate;

        /// <summary>
        /// Create a battery test with the given parameters
        /// </summary>
        /// <param name="eodVoltage">End of discharge voltage for each cell</param>
        /// <param name="cellCount">The number of cells</param>
        /// <param name="dischargeRate">Rate of discharge in amps</param>
        public BatteryTest(double eodVoltage, double cellCount, double dischargeRate) {
            this.eodVoltage = eodVoltage;
            this.cellCount = cellCount;
            this.dischargeRate = dischargeRate;
        }

        public string getCommands() {
            StringBuilder builder = new StringBuilder();

            // Set eodvoltage, cellcount, and dischargerate
            builder.Append(String.Format("Eodv={0}", eodVoltage)); //TODO: append newline

        }

    }

}
