# Introduction

This SDK is created to streamline the configuration and usage of hardware produced by Horiba. This includes ChargetCoupleDevices and Monochromators.
On top of the hardware devices, Horiba develops propriatery communication layer based on WebSocket connection. This layer is encapsulated in a process called ICL. ICL needs to be licensed and installed on a PC which has USB connection to the hardware. Once this is set up and ready, this SDK comes to play.

C# developers can use this SDK to offload the complexity related to establishing and maintaining connection to both the ICL and the hardware devices. This will allow them to focus on building the solution they need from the get go.
___

⬇️⬇️⬇️⬇️⬇️⬇️⬇️⬇️⬇️⬇️⬇️⬇️⬇️⬇️⬇️⬇️⬇️⬇️⬇️⬇️⬇️⬇️⬇️⬇️⬇️⬇️⬇️⬇️⬇️⬇️⬇️

> [!WARNING]  
> This SDK is under development and not yet released.

> [!IMPORTANT]  
> For this .NET code to work, the SDK from Horiba has to be purchased, installed and licensed.
> The code in this repo and the SDK are under development and not yet released for public use!

⬆️⬆️⬆️⬆️⬆️⬆️⬆️⬆️⬆️⬆️⬆️⬆️⬆️⬆️⬆️⬆️⬆️⬆️⬆️⬆️⬆️⬆️⬆️⬆️⬆️⬆️⬆️⬆️⬆️⬆️⬆️

___

**📦 Prerequisites**

* .NET Standard or .NET 6+
* ICL.exe installed as part of the Horiba SDK, licensed and activated

## API reference is available at

https://thatstheend.github.io/horiba-dotnet-sdk/docs/api/Horiba.Sdk.html

## ICL Commands Status available at

https://thatstheend.github.io/horiba-dotnet-sdk

# Getting started

This project is built and deployed to [Nuget.org](https://www.nuget.org/packages/Horiba.Sdk/). Installing it in a project can be done by the .NET CLI like so:

```dotnetcli
dotnet add package Horiba.Sdk
```

After having the package referenced in the project, you are ready to start using it.

#### DeviceManager

This is the entry point of the SDK. It is responsible for:

* starting up the ICL process  
* maintaining the WebSocket connection
* discoverty process for different device types

```csh
using var deviceManager = new DeviceManager();
await deviceManager.StartAsync();
```

Note the **using** declaration, this is needed to ensure proper disposal of all resources utilized by the **DeviceManager**. If the class is not properly disposed, multiple instances of the ICL.exe process might be left running. This can lead to inconsistent communication between available hardware and deviceManager.

After completion of the **StartAsync()** method, the collections of devices of the **DeviceManager** will be populated with concreat devices

#### Access specific device

```csh
var ccd = deviceManager.ChargedCoupledDevices.First();
var mono = deviceManager.Monochromators.First();
```

By keeping reference to the concreate device you can access all functionalities it supports. The first step should be to establish the USB connection to the device.
This is done by invoking the **OpenConnectionAsync()** method

```csh
await ccd.OpenConnectionAsync();
await mono.OpenConnectionAsync();
```

If no **CommunicationException** is thrown, a connection will be established.

After establishing connection, you can start invoking the rest of the available commands

#### Interacting with a device

```csh
var ccdConfig = await ccd.GetDeviceConfigurationAsync();
var monoConfig = await mono.GetDeviceConfigurationAsync();
```

Now you are ready to start implementing the functionality that best suites your case.

# How To?

## Send separate commands to supported devices

The [test project](https://github.com/ThatsTheEnd/horiba-dotnet-sdk/tree/main/Horiba.Sdk.Tests) demonstrates how commands can be sent to the devices.
You can look around to see more detailed examples.

    > NOTE: There are seemingly random delays in the tests. However, they are not random! These are the timeouts that the hardware needs to process the commands. They are set in empirical way so keep in mind that this is not exhausted list of all possible delays.

As a rule of thumb you can use the following pattern to send pairs of commands to the devices:

* send a SET command (e.g. setting a parameter on the device)
* wait for at least 300ms
* send a corresponding GET command

---

## Read actual data from CCD

* Create new ConsoleApplication

```dotnetcli
dotnet new console --framework net8.0
```

* Install the Nuget package

```dotnetcli
dotnet add package Horiba.Sdk
```

* Open the Program.cs file and update the implementation

```csh
using Horiba.Sdk.Devices;
using Horiba.Sdk.Enums;

using var deviceManager = new DeviceManager();
await deviceManager.StartAsync();
var ccd = deviceManager.ChargedCoupledDevices.First();
await ccd.OpenConnectionAsync();

await ccd.SetAcquisitionCountAsync(1);
await ccd.SetExposureTimeAsync(1500);
await ccd.SetRegionOfInterestAsync(RegionOfInterest.Default);
await ccd.SetXAxisConversionTypeAsync(ConversionType.None);

Dictionary<string, object> data = [];
if (await ccd.GetAcquisitionReadyAsync())
{
    await ccd.SetAcquisitionStartAsync(true);
    
    // This method will start a polling procedure after 1s initial delay
    // this initial delay is needed to allow the device to start the acquisition.
    // The interval of the polling procedure is set to be 300ms
    // every iteration of the polling procedure will check if the device is busy
    // by sending a request to the device.
    // The method will return when the device is not busy anymore
    await ccd.WaitForDeviceNotBusy(TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(300));
    
    data = await ccd.GetAcquisitionDataAsync();
}
        
var acquisitionRawData = data.GetValueOrDefault("acquisition");
var timestamp = data.GetValueOrDefault("timestamp");
```

The raw data will be in the shape of **Dictionary<string, object>** you will be able to extract the interesting data as per your needs.

However, if you need to use JSON deserialization functionality to work with typed objects, you can take a look at the [**Horiba.Sdk.Tests.AcquisitionDescription.cs**](https://github.com/ThatsTheEnd/horiba-dotnet-sdk/blob/main/Horiba.Sdk.Tests/AcquisitionDescription.cs) class and use it to deserialize the data into.

```csh
var parsedData = JsonConvert.DeserializeObject<List<AcquisitionDescription>>(acquisitionRawData.ToString());
```

---

## Use different installation path of the ICL

By default, the SDK will look for the ICL.exe to be present in the following path: 
    
>C:\Program Files\HORIBA Scientific\SDK\icl.exe

In cases where the local installation is in different folder, you can provide full path to the same executable in the constructor of the **DeviceManager** class.

```csh
using var deviceManager = new DeviceManager("C:\Path\To\ICL\icl.exe");
```
This path is used to start the ICL process prior connecting to it. The starting and stopping of this process is managed automatically by the DeviceManager. This is why it needs to be properly disposed. Otherwise, there might be multiple instances of the ICL process left running which will cause issues with the communication.

Fixing such issue boils down to manually stopping all locally running instances of the icl.exe and restarting the DeviceManager. 

---

## Use local network to connect to the ICL

The SDK supports connecting to the ICL process over the local network. This can be done by providing the IP address and port of the machine where the ICL process is running.

>NOTE: If you are using this approach, you need to make sure that the ICL process is running on the remote PC prior creating the DeviceManager instance.

```csh
using var deviceManager = new DeviceManager(ipAddress: IPAddress.Parse("192.168.123.123"), port: 1111);
```

---
