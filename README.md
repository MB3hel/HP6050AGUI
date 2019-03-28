# HP6050A GUI
A simple GUI to run tests with a HP6050A Electronic load. 

## Install Dependancies

### NI VisaNS driver and .Net API
The NI VisaNS driver is used to communicate with the electronic load via GPIB (or via an adapter).

Install the C# API from [here](https://www.ni.com/visa/). Be sure to download the `Ni-VISA...Runtime` package.

Make sure to install the `.Net 4.0 - 4.5.1 Runtime Support (IVI)` component. Under `Development Support` make sure `.Net 4.0 - 4.5.1 Development Support (IVI)` is selected.

Install the GPIB to USB adapter driver with the NI-488.2 package.
http://www.ni.com/en-us/support/downloads/drivers/download.ni-488-2.html