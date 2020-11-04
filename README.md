# BeKind readme

BeKind application backend

Для запуска необходимо запустить в докере:
```
winpty docker run --name postgres -e POSTGRES_PASSWORD=admin -e POSTGRES_USER=postgres -e POSTGRES_DB=kinddb -p 5432:5432 -d postgres
```

Затем выполнить команды в bash:

```
dotnet ef database update
```

и

```
dotnet watch run
```