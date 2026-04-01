# ===========================
# Stage 1: Build
# ===========================
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["SLT.sln", "."]
COPY ["SLT.API/SLT.API.csproj", "SLT.API/"]
COPY ["SLT.Application/SLT.Application.csproj", "SLT.Application/"]
COPY ["SLT.Core/SLT.Core.csproj", "SLT.Core/"]
COPY ["SLT.Infrastructure/SLT.Infrastructure.csproj", "SLT.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "SLT.sln"

# Copy all source code
COPY . .

# Build and publish the API
WORKDIR "/src/SLT.API"
RUN dotnet publish -c Release -o /app --no-restore

# ===========================
# Stage 2: Runtime
# ===========================
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# Environment variable for CORS origin (your frontend URL)
ENV FRONTEND_URL="https://smart-learning-tracker-frontend-tbv.vercel.app"

# Start the API
ENTRYPOINT ["dotnet", "SLT.API.dll"]