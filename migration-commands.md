# Entity Framework Migration Commands

## Creating a New Migration
```bash
dotnet ef migrations add [MigrationName] --project src/Infrastructure --startup-project src/Api
```

## Applying Migrations to Database
```bash
dotnet ef database update --project src/Infrastructure --startup-project src/Api
```

## Removing the Last Migration (if not applied)
```bash
dotnet ef migrations remove --project src/Infrastructure --startup-project src/Api
```

## Listing All Migrations
```bash
dotnet ef migrations list --project src/Infrastructure --startup-project src/Api
```

## Generating SQL Script (without applying)
```bash
dotnet ef migrations script --project src/Infrastructure --startup-project src/Api
```

## Updating Database to Specific Migration
```bash
dotnet ef database update [MigrationName] --project src/Infrastructure --startup-project src/Api
```

## Rolling Back to Previous Migration
```bash
dotnet ef database update [PreviousMigrationName] --project src/Infrastructure --startup-project src/Api
```

## Examples

### Add a new migration after entity changes:
```bash
dotnet ef migrations add AddUserProfileTable --project src/Infrastructure --startup-project src/Api
```

### Apply all pending migrations:
```bash
dotnet ef database update --project src/Infrastructure --startup-project src/Api
```

### Check migration status:
```bash
dotnet ef migrations list --project src/Infrastructure --startup-project src/Api
```

## Important Notes

1. **Always create migrations** when you make changes to your entity models
2. **Test migrations** in development before applying to production
3. **Backup your database** before applying migrations in production
4. **Review migration files** before applying them
5. **Use descriptive migration names** that explain what the migration does

## Current Database Schema

The initial migration creates the following tables:
- **Users**: User accounts with authentication info
- **Roles**: User roles (Admin, User, Moderator)
- **UserRoles**: Many-to-many relationship between users and roles
- **RefreshTokens**: JWT refresh tokens for authentication

## Seeded Data

The initial migration includes seed data for default roles:
- Admin (Administrator with full access)
- User (Standard user)
- Moderator (Content moderator) 