# Tournament Manager
Система управления спортивными турнирами с серверной частью на ASP.NET Core 8 и клиентским приложением. Проект состоит из API, общей бизнес-логики и настольного приложения, которые могут разворачиваться как локально, так и в Docker-контейнерах.

## Архитектура
Backend: TournamentManager.Api - ASP.NET Core 8 Web API с общей бизнес-логикой в TournamentManager.Core. Используется Entity Framework Core для работы с данными.

Client: TournamentManager.App - WPF/MAUI приложение для управления турнирами, использующее общие модели и сервисы из Core слоя.

База данных: MySQL 8.x (или совместимая).

## Требования
.NET 8 SDK

MySQL 8.x (локально) или совместимый сервер

Docker + Docker Compose (для контейнеризации)

Для WPF приложения: Windows или соответствующий фреймворк для MAUI

Локальный запуск (разработка)
База данных
Установите и запустите MySQL сервер

Создайте базу данных:

```sql
CREATE DATABASE sport_tournaments;
CREATE USER 'tournament_user'@'localhost' IDENTIFIED BY 'azsxdc10';
GRANT ALL PRIVILEGES ON sport_tournaments.* TO 'tournament_user'@'localhost';
FLUSH PRIVILEGES;
```
## Backend (API)
Перейдите в директорию API:
```
bash
cd TournamentManager.Api
```
Создайте файл appsettings.json на основе примера:
```
json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;database=sport_tournaments;user=tournament_user;password=azsxdc10;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```
Примените миграции Entity Framework Core:
```
bash
dotnet ef database update --project ../TournamentManager.Core --startup-project .
```
Запустите API:
```
bash
dotnet run
```
или для режима разработки с автоматической перезагрузкой:
```
bash
dotnet watch run
```
API будет доступен по умолчанию на https://localhost:7074.

Клиентское приложение (TournamentManager.App)
Откройте решение в Visual Studio или другом редакторе

Убедитесь, что TournamentManager.App настроен на использование правильного URL API (обычно через конфигурацию или appsettings)

Запустите приложение

Docker-сборка и запуск
Подготовка окружения
Создайте файл .env в корневой директории проекта с содержимым:
```
DB_ROOT_PASSWORD=120421
DB_NAME=sport_tournaments
DB_USER=tournament_user
DB_PASSWORD=azsxdc10
ASPNETCORE_ENVIRONMENT=Development
FRONTEND_API_URL=https://localhost:7074
DB_VOLUME_NAME=sport_tournaments_volume
```
Убедитесь, что Docker и Docker Compose установлены и запущены

Запуск контейнеров
```
bash
docker compose up -d --build
```
После запуска:

API будет доступен на https://localhost:7074

База данных будет инициализирована

Данные БД будут сохранены в Docker volume sport_tournaments_volume

Остановка контейнеров
bash
docker compose down
Для полной очистки с удалением volume:
```
bash
docker compose down -v
```
Основные настройки API хранятся в TournamentManager.Api/appsettings.json:
```
json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;database=sport_tournaments;user=tournament_user;password=azsxdc10;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```
# Структура проекта

TournamentManager/
├─ TournamentManager.Api/ 
│  ├─ Controllers/            
│  ├─ Program.cs          
│  ├─ appsettings.json           
│  ├─ Dockerfile                 
│  └─ TournamentManager.Api.csproj 
│
├─ TournamentManager.Core/       
│  ├─ Models/                   
│  ├─ Services/                    
│  ├─ Interfaces/                    
│  ├─ Data/                        
│  └─ TournamentManager.Core.csproj    
│
├─ TournamentManager.App/            
│  ├─ Views/                          
│  ├─ ViewModels/                    
│  ├─ App.xaml                         
│  ├─ App.xaml.cs                       
│  └─ TournamentManager.App.csproj     
│
├─ .env                                  
├─ docker-compose.yml                  
└─ README.md                           
##Зависимости проектов
TournamentManager.Api зависит от TournamentManager.Core

TournamentManager.App зависит от TournamentManager.Core

Оба проекта используют общие модели и сервисы из Core
