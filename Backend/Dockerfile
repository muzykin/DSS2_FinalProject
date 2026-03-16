# 1. Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 2. Copy the project file and restore dependencies
COPY ["Todo.Api.csproj", "./"]
RUN dotnet restore "Todo.Api.csproj"

# 3. Copy the rest of the code and build the application
COPY . .
RUN dotnet publish "Todo.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 4. Use the official .NET ASP.NET runtime image to run the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# 5. Start the application
ENTRYPOINT ["dotnet", "Todo.Api.dll"]