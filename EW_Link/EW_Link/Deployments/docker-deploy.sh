#!/bin/bash

dotnet publish -c Release -r linux-x64 --self-contained ../EW_Link.csproj

docker build --platform linux/x86_64 -t ew-link1.0 ../
docker tag ew-link1.0 creativ360/development:ew-link1.0
docker push creativ360/development:ew-link1.0