# HP6050A GUI
A simple GUI to run tests with a HP6050A Electronic load. 

## Install Dependancies

### NI VisaNS driver and .Net API
The NI VisaNS driver is used to communicate with the electronic load via GPIB (or via an adapter).

Install versions 19.0 of all the following:
------------------------------------------------------
Install the NI-VISA C# (.NET) API from [here](https://www.ni.com/visa/). Install `NI-VISA .NET Development Support` and `NI-VISA .NET Runtime`. Other components are not required. Note: components selection will probably be the second window shown. First window installs NI Package Manager, which will have no component selection. Afterwards another window will be opened to install NI-VISA.

(MAYBE - this may not be required) Also have to install https://www.ni.com/en-us/support/downloads/drivers/download.ivi-compliance-package.html
Make sure to select .NET component

Install the GPIB to USB adapter driver with the NI-488.2 package.
http://www.ni.com/en-us/support/downloads/drivers/download.ni-488-2.html
NOT SURE WHICH COMPONENTS...
