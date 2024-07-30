FROM defradigital/dotnetcore-development:dotnet8.0 AS build-env

ARG PORT=3000
ENV ASPNETCORE_URLS=http://*:${PORT}
EXPOSE ${PORT}

COPY --chown=dotnet ["EPR.Payment.Facade/EPR.Payment.Facade.csproj", "EPR.Payment.Facade/"]
COPY --chown=dotnet  ["EPR.Payment.Facade.Common/EPR.Payment.Facade.Common.csproj", "EPR.Payment.Facade.Common/"]

RUN dotnet restore "EPR.Payment.Facade/EPR.Payment.Facade.csproj"

USER root
RUN apk update && apk add --no-cache npm
USER dotnet
ENV NODE_EXTRA_CA_CERTS=/usr/local/share/ca-certificates/internal-ca.crt

COPY --chown=dotnet:dotnet . .
WORKDIR /home/dotnet/EPR.Payment.Facade
RUN dotnet publish -c Release -o out

FROM defradigital/dotnetcore:dotnet6.0
COPY --from=build-env --chown=dotnet /home/dotnet/EPR.Payment.Facade/out .

USER root
RUN apk add icu-libs libintl
USER dotnet

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=0

ARG PORT=3000
ENV ASPNETCORE_URLS=http://*:${PORT}
EXPOSE ${PORT}

CMD dotnet EPR.Payment.Facade.dll

