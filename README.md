# HP6050A GUI
A simple GUI to run tests with a HP6050A Electronic load. 

## Install Dependancies

### NI VisaNS driver and .Net API
The NI VisaNS driver is used to communicate with the electronic load via GPIB (or via an adapter).

Install the driver from [here](https://www.ni.com/visa/).

Make sure to install the `.Net 4.5 Runtime Support (NS)` and `.Net 4.0 Runtime Support (NS)`.

### Install NI-488.2 driver
http://sine.ni.com/psp/app/doc/p/id/psp-356/lang/en
Be sure to install .Net API support

## The project
The project is separated into two parts: an interface library, and the GUI. The interface library is used to communciate with the device and define tests/handle output from the device. The GUI is used to start different tests.

### GUI
The GUI allows starting of predefined tests for certain types of batteries (used by FRC teams).

## Interface Library
The interface libary handles communication with the load (using NI VisaNS), configuring of tests, and parsing output form the load.