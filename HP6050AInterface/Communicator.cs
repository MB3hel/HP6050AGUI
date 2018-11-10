using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NationalInstruments.Visa;

namespace HP6050AInterface {

    /// <summary>
    /// Communicate with a load via GPIB using NI's VisaNS driver.
    /// </summary>
    public class Communicator {

        MessageBasedSession mbSession;

        /// <summary>
        /// Start communication with the load
        /// </summary>
        /// <param name="gpibAdapter">Which gpib adapter to open communication on</param>
        /// <param name="insturmentAddress">The address of the load</param>
        public void open(string resourceName) {
            if (mbSession != null)
                close(); // Close first
            using (var rmSession = new ResourceManager()) {
                try {
                    mbSession = (MessageBasedSession)rmSession.Open(resourceName);
                } catch (InvalidCastException) {
                    throw new InvalidOperationException("Tried to open a message-based sesssion for a non message-based resource.");
                }
            }
        }

        /// <summary>
        /// Close an open session
        /// </summary>
        void close() {
            if (mbSession != null) {
                mbSession.Dispose();
                mbSession = null;
            }
        }

        bool isOpen() {
            return mbSession != null;
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
