# Build stage
FROM microsoft/dotnet:sdk AS build
WORKDIR /

# Copy everything
COPY . ./

# Publish project
RUN dotnet publish Nanoka.Web -c Release -o ./bin

# Runtime stage
FROM microsoft/dotnet:aspnetcore-runtime
WORKDIR /

# Copy publish output
COPY --from=build /Nanoka.Web/bin .

# Expose ports
EXPOSE 80

# Entrypoint
ENTRYPOINT ["dotnet", "Nanoka.Web.dll"]
