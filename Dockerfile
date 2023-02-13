#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
#EXPOSE 80
EXPOSE 443

COPY src/DSF.AspNetCore.Web.Template/cert/ariadni-test.crt /usr/local/share/ca-certificates/ariadni-test.crt
RUN chmod 644 /usr/local/share/ca-certificates/ariadni-test.crt && update-ca-certificates

# Create a group and user so we are not running our container and application as root and thus user 0 which is a security issue.
RUN addgroup --system --gid 1000 appgroup && adduser --system --uid 1000 --ingroup appgroup --shell /bin/sh appuser
  
# Serve on port 8080, we cannot serve on port 80 with a custom user that is not root.
#ENV ASPNETCORE_URLS=http://+:8080
#EXPOSE 8080
  
# Tell docker that all future commands should run as the appuser user, must use the user number
USER 1000

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY src/DSF.AspNetCore.Web.Template/DSF.AspNetCore.Web.Template.csproj .
COPY src/DSF.Localization/DSF.Localization.csproj .
COPY src/DSF.Authentication/DSF.Authentication.csproj .
RUN dotnet restore "./DSF.AspNetCore.Web.Template.csproj"


# See https://stackoverflow.com/questions/51533448/why-copy-package-json-precedes-copy
# for why we copy just the csproj then the whole directory
COPY src/DSF.AspNetCore.Web.Template .
RUN dotnet build DSF.AspNetCore.Web.Template.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DSF.AspNetCore.Web.Template.csproj" -c Release -o /app/publish

# Serve on port 8080, we cannot serve on port 80 with a custom user that is not root.
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DSF.AspNetCore.Web.Template.dll"]