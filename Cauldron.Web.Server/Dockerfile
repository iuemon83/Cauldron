FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY ./release/. .
ENTRYPOINT ["dotnet", "Cauldron.Web.Server.dll"]