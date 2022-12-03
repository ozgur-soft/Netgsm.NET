[![license](https://img.shields.io/:license-mit-blue.svg)](https://github.com/ozgur-soft/Netgsm.NET/blob/main/LICENSE.md)

# Netgsm.NET
An easy-to-use netgsm.com.tr API with .NET

# Installation
```bash
dotnet add package Netgsm --version 1.1.0
```

# Usage
```c#
using Netgsm;

var netgsm = new Netgsm();
netgsm.SetUsercode("api usercode");
netgsm.SetPassword("api password");
netgsm.Sms("header", "phone", "message"); // Sending SMS message
netgsm.Otp("header", "phone", "message"); // Sending OTP message
```
