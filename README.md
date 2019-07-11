# HP6050A GUI
A simple GUI to run tests with a HP6050A Electronic load. 

## Installation

### Installing the GPIB to USB Adapter Driver
- First connect the GPIB to USB adapter. The adapter must be connected before installing the driver.
- The driver for the adapter is installed with the NI 488.2 package which can be downloaded from [NI's Website.](https://www.ni.com/en-us/support/downloads/drivers/download.ni-488-2.html). Version 19.0 has been tested. Other versions may or may not work.

### Installing the Tester GUI
- Download the latest release form the releases tab of the github page.
- Extract the zip and run the .exe file.

## Build Dependencies
- The NI Visa NS library is used to communicate with the device via the GPIB adapter. Install version 19.0 from [NI's Website](http://www.ni.com/en-us/support/downloads/drivers/download.ni-visa.htm)
