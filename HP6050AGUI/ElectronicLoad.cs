using Ivi.Visa;
using NationalInstruments.Visa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HP6050AGUI {

    public class ElectronicLoad {
        protected MessageBasedSession mbSession;
        protected int channel = 1;

        /// <summary>
        /// Open a sesion to a specified resource. If an existing session is open it will be closed first.
        /// </summary>
        /// <param name="resourceName"></param>
        public void open(string resourceName) {
            if (isOpen())
                close();
            using (var rmSession = new ResourceManager()) {
                mbSession = (MessageBasedSession)rmSession.Open(resourceName);
            }
        }

        /// <summary>
        /// Close the session (if opened)
        /// </summary>
        public void close() {
            lock (mbSession) {
                if (mbSession != null) {
                    mbSession.Dispose();
                    mbSession = null;
                }
            }
        }

        public void setCurrent(int channel, int currentMa) {
            switchChannel(channel);
            write("MODE:CURR");
            write("CURR " + currentMa + "MA");
        }

        public void inputOff(int channel) {
            switchChannel(channel);
            write("INP OFF");
        }

        public void inputOn(int channel) {
            switchChannel(channel);
            write("INP ON");
        }

        public double readVoltage(int channel) {
            switchChannel(channel);
            write("MEAS:VOLT?");
            return double.Parse(read());
        }

        public double readCurrent(int channel) {
            switchChannel(channel);
            write("MEAS:CURR?");
            return double.Parse(read());
        }

        /// <summary>
        /// Checks if the session is currently open.
        /// </summary>
        /// <returns>True if the session has been opened and not yet disposed.</returns>
        public bool isOpen() {
            return mbSession != null && !mbSession.IsDisposed;
        }

        /// <summary>
        /// Checks if the session has been opened.
        /// </summary>
        /// <returns>True if the session has been opened (even if it is now disposed).</returns>
        protected bool hasBeenOpened() {
            return mbSession != null;
        }

        protected void switchChannel(int channel) {
            if (this.channel == channel)
                return;
            write("CHAN " + channel);
            this.channel = channel;
        }

        /// <summary>
        /// Makes sure message based session has been and is still opened. Throws an exception if not.
        /// </summary>
        protected void preCheck() {
            if (!isOpen())
                throw new Exception("No session opened.");
            // If disposed and not set to null it was not closed here. Closed due to loss of communication
            if (mbSession.IsDisposed && isOpen()) {
                mbSession = null;
                throw new Exception("Communication with the device was lost.");
            }
        }

        /// <summary>
        /// Read a string from the device
        /// </summary>
        /// <returns>The string read</returns>
        protected string read() {
            preCheck();
            return mbSession.RawIO.ReadString();
        }

        /// <summary>
        /// Write a string to the device
        /// </summary>
        /// <param name="message">The string to write</param>
        protected void write(string message) {
            preCheck();
            mbSession.RawIO.Write(message);
        }

    }
}
