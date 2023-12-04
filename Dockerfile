FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env

WORKDIR /source

COPY . ./
RUN dotnet restore --disable-parallel
WORKDIR /source/MysterySantaBot
RUN dotnet publish -c release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:7.0
#RUN apt-get update \
#    && apt-get upgrade -y \
#    && apt-get install -y --allow-unauthenticated \
#        libc6-dev \
#        libgdiplus \
#        libx11-dev \
#        git \
#    && rm -rf /var/lib/apt/lists/* 
  
WORKDIR /app
COPY --from=build-env /app ./
#ENTRYPOINT ["dotnet", "MarathonBot.dll", "--environment=Development"]
ENTRYPOINT ["dotnet", "MysterySantaBot.dll"]