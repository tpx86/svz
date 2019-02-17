FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /app


# copy csproj and restore as distinct layers
COPY *.sln .
COPY projectfiles.tar .
RUN tar -xvf projectfiles.tar
RUN dotnet restore


# copy everything else and build app
COPY ./ ./
RUN dotnet restore Svz.Tool/Svz.Tool.csproj
RUN dotnet publish Svz.Tool/Svz.Tool.csproj -c Release -o out

FROM microsoft/dotnet:2.2-aspnetcore-runtime AS runtime
WORKDIR /app
COPY --from=build app/Svz.Tool/out ./
COPY wait-for-it.sh .
COPY docker-entrypoint.sh .
ENTRYPOINT ["./docker-entrypoint.sh"]

