```bash
dotnet ef migrations add AddModeratorLetterQueueEntity --context SantaBotDbContext -o Database/Migrations
dotnet ef database update --context SantaBotDbContext
```