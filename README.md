# APB Watcher

C# application for monitoring the state of APB Reloaded. Includes a library that can be used to interact with the lobby and world servers for the game.

## APB Client Library

The `APBClient` project within this repository can be used on its own to interact with the game servers.

### Example Usage

You'll need to create a YAML file with some hardware information which the library will send to the servers. Here's an example:

```
---
# Data returned from WMI queries
WmiSections:
  CPU:
    Select: "ProcessorId,Manufacturer,Name,Description,Revision,L2CacheSize,@AddressWidth"
    From: "FROM Win32_Processor"
    NumericFields:
      - Revision
      - L2CacheSize
      - AddressWidth
    Data:
      - ProcessorId: BFEBFBFF00040661
        Manufacturer: GenuineIntel
        Name: Intel(R) Core(TM) i7-4960HQ CPU @ 2.60GHz
        Description: Intel64 Family 6 Model 70 Stepping 1
        Revision: 17921
        L2CacheSize: 1024
        AddressWidth: 64

  MB:
    Select: "SerialNumber,Manufacturer,Product,Version"
    From: "FROM Win32_BaseBoard"
    Data: 
      - SerialNumber: QWERTYUIOPASDFGHJKL
        Manufacturer: QWERTYUIOPASDFGHJKL
        Product: QWERTYUIOPASDFGHJKL
        Version: QWERTYUIOPASDFGHJKL

  BIOS:
    Select: "Name,Caption,SerialNumber"
    From: "FROM Win32_BIOS"
    Data:
      - Name: QWERTYUIOPASDFGHJKL
        Caption: QWERTYUIOPASDFGHJKL
        SerialNumber: QWERTYUIOPASDFGHJKL

  IDE:
    Select: "Manufacturer,Name,Description"
    From: "FROM Win32_IDEController WHERE NOT Manufacturer LIKE '(%'"
    Data:
      - Manufacturer: Standard SATA AHCI Controller
        Name: Standard SATA AHCI Controller
        Description: Standard SATA AHCI Controller

  SCSI:
    Select: "Manufacturer"
    From: "FROM Win32_SCSIController WHERE NOT PNPDeviceId LIKE 'USB%'"
    Data:
      - Manufacturer: Microsoft

  VGA:
    Select: "AdapterRAM,Caption,VideoProcessor"
    From: "FROM Win32_VideoController WHERE Availability=3 AND PNPDeviceId LIKE 'PCI%'"
    NumericFields: 
      - AdapterRAM
    Data:
      - AdapterRAM: -2147483648
        Caption: QWERTYUIOPASDFGHJKL
        VideoProcessor: QWERTYUIOPASDFGHJKL

  AUD:
    Select: "Name,Manufacturer"
    From: "FROM Win32_SoundDevice WHERE Status='OK' AND (PNPDeviceId LIKE 'HDAUDIO%' OR PNPDeviceId LIKE 'PCI%')"
    Data:
      - Name: QWERTYUIOPASDFGHJKL
        Manufacturer: QWERTYUIOPASDFGHJKL
      - Name: QWERTYUIOPASDFGHJKL
        Manufacturer: QWERTYUIOPASDFGHJKL

  RAM:
    Select: "Capacity,PartNumber,@DeviceLocator,Manufacturer,SerialNumber,Speed"
    From: "FROM Win32_PhysicalMemory"
    NumericFields: 
      - Speed
    Data:
      - Capacity: 8589934592
        PartNumber: "0x000000000000000000000000000000000000"
        DeviceLocator: DIMM0
        Manufacturer: "0x0000"
        SerialNumber: "0x00000000"
        Speed: 1333
      - Capacity: 8589934592
        PartNumber: "0x000000000000000000000000000000000000"
        DeviceLocator: DIMM0
        Manufacturer: "0x0000"
        SerialNumber: "0x00000000"
        Speed: 1333

  HDD:
    Select: "Size,Model,Signature"
    From: "FROM Win32_DiskDrive WHERE InterfaceType<>'USB' AND InterfaceType<>'1394' AND (PNPDeviceID LIKE 'IDE%' OR PNPDeviceID LIKE 'SCSI%')"
    NumericFields: 
      - Signature
    Data:
      - Size: 500269754880
        Model: QWERTYUIOP
  SYS:
    Select: "IdentifyingNumber,Name,Vendor,UUID"
    From: "FROM Win32_ComputerSystemProduct"
    Data:
      - IdentifyingNumber: QWERTYUIOPASDFGHJKL
        Name: QWERTYUIOPASDFGHJKL
        Vendor: QWERTYUIOPASDFGHJKL
        UUID: 4216BDA4-5AB1-FED1-C913-832F9AB3627A

  OS:
    Select: "Name,SerialNumber,InstallDate,CountryCode,Version,ServicePackMajorVersion,ServicePackMinorVersion"
    From: "FROM Win32_OperatingSystem"
    Data:
      - Name: Microsoft Windows 10 Pro|C:\Windows|\Device\Harddisk0\Partition4
        SerialNumber: 12345-12345-12345-ABCDE
        InstallDate: 20000101010203.000000+500
        CountryCode: 1
        Version: 10.0.10586
        ServicePackMajorVersion: 0
        ServicePackMinorVersion: 0

# BFP Information
SmbiosVersion: "2.4"
# TODO: This seems to be hardcoded inside APB.exe and could change at any time, might be worth more digging. 
BfpVersion: 14071501
BfpSections:
  BIOS:
    Vendor: QWERTYUIOPASDFGHJKL
    RomSize: 127

  SYSINFO:
    Manufacture: QWERTYUIOPASDFGHJKL
    ProductName: QWERTYUIOPASDFGHJKL
    Serial: "QWERTYUIOPASDFGHJKL"
    UUID: "{4216BDA4-5AB1-FED1-C913-832F9AB3627A}"
    SKU: "System SKU#"

  BASEBOARD:
    Manufacture: QWERTYUIOPASDFGHJKL
    Product: QWERTYUIOPASDFGHJKL
    Version: QWERTYUIOPASDFGHJKL
    Serial: QWERTYUIOPASDFGHJKL

  CHASSIS:
    Manufacture: QWERTYUIOPASDFGHJKL
    Type: 10
    Version: QWERTYUIOPASDFGHJKL
    Serial: QWERTYUIOPASDFGHJKL

  PROCESSOR:
    Type: 3
    Family: 198
    Manufacture: "Intel(R) Corporation"
    RawId: "0x40661"

  SLOTS:
    Slot001: PCI Express
    Slot002: PCI
    Slot003: PCI

  MEMSLOTS:
    MaxCapacity: 8388608
    NumMemoryDevices: 2

# GUID from primary volume, obtained using mountvol command
HddGuid: "{4216bda4-5ab1-fed1-c913-832f9ab3627a}"

# Results returned from GetVersionEx
WindowsVersion:
  MajorVersion: 6
  MinorVersion: 3
  ProductType: 1
  BuildNumber: 9600

# Value from HKLM\Software\Microsoft\Windows NT\CurrentVersion\InstallDate
InstallDate: 0x12345678
```

You can then use the library as shown in the following example:

```
string username = "your_username";
string password = "your_password";

HardwareStore hw;
using (TextReader reader = File.OpenText("hw.yml"))
{
    hw = new HardwareStore(reader);
}

Task.Run(async () =>
{
    try
    {
        var client = new APBClient.APBClient(username, password, hw);
        await client.Login();
        Console.WriteLine("Logged In!");
        List<CharacterInfo> characters = client.GetCharacters();
        Console.WriteLine("Got characters!");
        List<WorldInfo> worlds = await client.GetWorlds();
        Console.WriteLine("Received worlds!");
        FinalWorldEnterData worldEnterData = await client.EnterWorld(characters[0].SlotNumber);
        Console.WriteLine("Connected to world!");
        Dictionary<int, DistrictInfo> districts = client.GetDistricts();
        Console.WriteLine("Got districts");
        List<InstanceInfo> instances = await client.GetInstances();
        Console.WriteLine("Recieved instances");
    }
    catch (Exception e)
    {
        Console.WriteLine("Error occurred");
        Console.WriteLine(e);
    }
}).Wait();
```
