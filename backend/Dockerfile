FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY backend/src/EmployeeManager.Api/EmployeeManager.Api.csproj backend/src/EmployeeManager.Api/
COPY backend/src/EmployeeManager.Application/EmployeeManager.Application.csproj backend/src/EmployeeManager.Application/
COPY backend/src/EmployeeManager.Domain/EmployeeManager.Domain.csproj backend/src/EmployeeManager.Domain/
COPY backend/src/EmployeeManager.Infrastructure/EmployeeManager.Infrastructure.csproj backend/src/EmployeeManager.Infrastructure/

RUN dotnet restore backend/src/EmployeeManager.Api/EmployeeManager.Api.csproj

COPY backend/src/ backend/src/
RUN dotnet publish backend/src/EmployeeManager.Api/EmployeeManager.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EmployeeManager.Api.dll"]
