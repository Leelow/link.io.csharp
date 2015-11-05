![alt text](http://img15.hostingpics.net/pics/329504linkio.png "Link.IO C# API")

# Client-side

This is the client-side Link.IO C# API.

There is an ugly proof of concept using the first version of the API  [here](https://github.com/Leelow/link.io.csharp.poc).

#### Installation

After adding sources to your project on visual studio, you have to add a NuGet package. You can find some information about NuGet packages [here](https://www.nuget.org/).

The Nuget package to install is [SocketIoClientDotNet](https://github.com/Quobject/SocketIoClientDotNet/) (tested with v0.9.12). It's the only way to remove dependencies errors. In the future, we will propose our own NuGet package.

To finish, you juste have to add the following import :

> using LinkIOcsharp;

# Server-side

You can find more information about the server-side Link.IO API [here](https://github.com/Chaniro/link.io.server/). There is the possibility to monitoring the server thanks to [link.io.server.monitoring](https://github.com/Leelow/link.io.server.monitoring).
