# UHLNoCS v3.0
CAD for high-level modeling of networks-on-chip

## Components of CAD solution
- server application
- client application
- auxiliary libraries
- tests

### Server application
gRpc ASP.NET Core service which acts as server in CAD.  
The most important parts of UhlnocsServer project:
- app settings (UhlnocsServer/Program.cs, UhlnocsServer/appsettings.json and UhlnocsServer/Properties)
- proto files with description of gRpc contracts (UhlnocsServer/Protos)
- services which implement those contracts (UhlnocsServer/Services)
- classes to communicate with database (UhlnocsServer/Database)
- classes for User Service (UhlnocsServer/Users)
- classes for Model Service (UhlnocsServer/Models)
- classes for Calculation Service (UhlnocsServer/Calculations)
- classes for Decision Support System (DSS) module (UhlnocsServer/DSS)
- classes for Modeling Process Optimization module (UhlnocsServer/Optimizations)
- classes with common methods (UhlnocsServer/Utils)

### Client application
WPF application which is a client app with GUI.  
MVVM pattern was used for development of this app.  
The most important parts of UhlnocsClient project:
- app settings (UhlnocsClient/App.xaml and UhlnocsClient/appsettings.json)
- main window (UhlnocsClient/MainWindow.*)
- views (UhlnocsClient/Views)
- view models (UhlnocsClient/ViewModels)
- models (UhlnocsClient/Models)

### Libraries
C# classes library for client and server projects (UhlnocsLibrary/*)

### Tests
Unit tests for server project (UhlnocsTests/*)

## CAD and models files
Archives with latest versions of server app can be found [here](https://drive.google.com/drive/folders/15iQbJkoshGzZmrkByFuh0WKycfGDQmh1?usp=share_link)  
Archives with latest versions of client app can be found [here](https://drive.google.com/drive/folders/1684iMrC6DiIUosMoYM1TSXVx5Q0oMDha?usp=share_link)  
Archives with latest versions of some integrated models can be found [here](https://drive.google.com/drive/folders/1zu0hRHXKSjRA2MSB81W-mKO-FuR59vVW?usp=share_link)  

## Documentation, papers and other materials
Diplomas:
- about CAD architecture TODO: add link
- about DSS module TODO: add link
- about Optimization module TODO: add link

[User manual](https://docs.google.com/document/d/1TrPLn3PegrUGO-2gakrWGtYrEsQyDj8W-Q40dktWOlc/edit?usp=share_link)  
[Developer manual](https://docs.google.com/document/d/102qEiaBLqWzryhyl_K7A4h_qTBtQD5un-EVjZ4CNDOI/edit?usp=share_link)  

TODO: add bibliographies of armntk (not published yet though) and mwent and patent(if request will be made)

## Contacts
asponomarev@edu.hse.ru (architecture developer)  
tvtarzhanov@edu.hse.ru (optimization module developer)  
nyborodin@edu.hse.ru (DSS module developer)  
a.romanov@hse.ru (project manager)  
aamerikanov@hse.ru (project advisor)  
