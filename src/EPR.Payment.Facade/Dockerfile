FROM defradigital/dotnetcore-development:dotnet8.0 AS base

USER root
ARG PORT=8080
ENV ASPNETCORE_URLS=http://*:${PORT}
EXPOSE ${PORT}

RUN apk update && apk --no-cache add icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=0

FROM defradigital/dotnetcore-development:dotnet8.0 AS build
USER root

# Install dotnet-ef tool
RUN dotnet tool install dotnet-ef --version 6.* --tool-path "/root/.dotnet/tools"

# Add the tools folder to PATH
ENV PATH="${PATH}:/root/.dotnet/tools"

WORKDIR /src
COPY ["EPR.Payment.Facade/EPR.Payment.Facade.csproj", "EPR.Payment.Facade/"]
COPY ["EPR.Payment.Facade.Common/EPR.Payment.Facade.Common.csproj", "EPR.Payment.Facade.Common/"]
RUN dotnet restore "EPR.Payment.Facade/EPR.Payment.Facade.csproj"

COPY --chown=dotnet:dotnet EPR.Payment.Facade/. ./EPR.Payment.Facade/.
COPY --chown=dotnet:dotnet EPR.Payment.Facade.Common/. ./EPR.Payment.Facade.Common/.

WORKDIR "/src/EPR.Payment.Facade"
RUN dotnet build "EPR.Payment.Facade.csproj" -c Release /p:AzureBuild=true -o /app/build

FROM build AS publish
RUN dotnet publish "EPR.Payment.Facade.csproj" -c Release /p:AzureBuild=false -o /app/publish

FROM base AS final

# Switch to the non-root user
USER dotnet

WORKDIR /app
COPY --from=publish /app/publish .
USER dotnet
ENTRYPOINT ["dotnet", "EPR.Payment.Facade.dll"]