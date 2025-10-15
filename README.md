# Tournament Manager

## Настройка базы данных

1. Скопируйте `appsettings.example.json` в `appsettings.json`
2. Настройте строку подключения к вашей MySQL базе:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;database=taekwondo_tournaments;user=your_username;password=your_password;"
  }
}