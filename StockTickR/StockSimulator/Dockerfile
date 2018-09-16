FROM microsoft/dotnet@sha256:d340d97ed80dc3cecedabbb0aca77f5691c19b810a91bf8e9973e4db342d44da AS build
WORKDIR /src

COPY *.csproj ./
COPY NuGet.config ./
RUN dotnet restore -nowarn:NU1701
COPY . .
RUN dotnet build -c Release -nowarn:NU1701 -o /app -r alpine.3.7-x64

FROM build AS publish
RUN dotnet publish -c Release -nowarn:NU1701 -o /app -r alpine.3.7-x64

FROM lohrer/aspnetcore-alpine-kafka@sha256:93ce7bc4540f3d9b73c9fd2954da9b42e21a3a645f58b2bc073198857c544168 as base

# Define the port that is exposed by the application
ENV PORT=8083

ENV ASPNETCORE_URLS=http://+:${PORT} \
    # Enable detection of running in a container
    DOTNET_RUNNING_IN_CONTAINER=true \
    # Since icu_libs (International Components for Unicode library (libraries)) are included
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

EXPOSE ${PORT}

WORKDIR /app
COPY --from=publish /app .
RUN chmod +x /app/StockSimulator \
    && find $sysdirs -xdev \( \
       -name chmod -o \
        \) -delete

USER appuser

ENTRYPOINT ["./StockSimulator"]
