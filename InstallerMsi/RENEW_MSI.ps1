wix build .\InstallerMsi\Product.wxs `
>>   -arch x64 `
>>   -d ServiceBin=".\WinEcoSensor.Service\bin\Release" `
>>   -d TrayBin=".\WinEcoSensor.TrayApp\bin\Release" `
>>   -o .\InstallerMsi\WinEcoSensor.msi