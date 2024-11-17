FROM mcr.microsoft.com/dotnet/runtime:7.0.2-bullseye-slim-amd64

COPY bin/Release/net7.0/linux-x64/publish /App

WORKDIR /App
ENTRYPOINT ["dotnet", "EW_Link.dll"]