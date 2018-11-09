using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HP6050AInterface {

    /// <summary>
    /// Communicate with a load via GPIB using NI's VisaNS driver.
    /// </summary>
    public class Communicator {

        /// <summary>
        /// Start communication with the load
        /// </summary>
        /// <param name="gpibAdapter">Which gpib adapter to open communication on</param>
        /// <param name="insturmentAddress">The address of the load</param>
        public Communicator(int gpibAdapter, int insturmentAddress) {

        }

        /// <summary>
        /// Start a certain test defined by an ITest object
        /// </summary>
        /// <param name="test">The test to start</param>
        public void startTest(ITest test) {

        }

        /// <summary>
        /// Stop the running test
        /// </summary>
        public void stopTest() {

        }

        /// <summary>
        /// Is a test currently running
        /// </summary>
        /// <returns></returns>
        public bool isRunningTest() {
            return false;
        }

        /// <summary>
        /// Get the output sent from the load
        /// </summary>
        public string getOutput() {
            return "";
        }

    }
}
