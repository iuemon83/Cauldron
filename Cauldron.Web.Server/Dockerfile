FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY ./out/. .
ENTRYPOINT ["dotnet", "Cauldron.Web.Server.dll"]