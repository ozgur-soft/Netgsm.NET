# Netgsm.NET
An easy-to-use netgsm.com.tr API with .NET

# Installation
```bash
dotnet add package Netgsm --version 1.2.0
```

# Usage
```c#
namespace Netgsm {
    internal class Program {
        static void Main(string[] args) {
            var netgsm = new Netgsm();
            netgsm.SetUsercode("api usercode");
            netgsm.SetPassword("api password");
            netgsm.Sms("header", "phone", "message"); // Sending SMS message
            netgsm.Otp("header", "phone", "message"); // Sending OTP message
        }
    }
}
```
