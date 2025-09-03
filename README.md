# 🍎 FoodKeep Backend API

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com/)
[![Architecture](https://img.shields.io/badge/Architecture-Clean-brightgreen)](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED)](https://www.docker.com/)
[![License](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

Service backend REST API pour FoodKeep - Solution intelligente de gestion d'inventaire alimentaire anti-gaspillage.

## 📋 Table des matières

-   [Vue d'ensemble](#-vue-densemble)
-   [Architecture](#-architecture)
-   [Prérequis](#-prérequis)
-   [Installation](#-installation)
-   [Développement](#-développement)
-   [Structure du projet](#-structure-du-projet)
-   [API Documentation](#-api-documentation)
-   [Tests](#-tests)
-   [Base de données](#-base-de-données)
-   [Déploiement](#-déploiement)
-   [Dépannage](#-dépannage)
-   [Contribution](#-contribution)

## 🎯 Vue d'ensemble

FoodKeep Backend est une API REST moderne construite avec .NET 9 suivant les principes de Clean Architecture. Elle gère l'ensemble de la logique métier pour la gestion d'inventaire alimentaire, les notifications d'expiration, et l'analyse des habitudes de consommation.

### Fonctionnalités principales

-   🔐 **Authentification JWT** : Sécurisation complète avec tokens
-   🍕 **Gestion d'inventaire** : CRUD complet sur les aliments
-   📊 **Analytics** : Statistiques de consommation et gaspillage
-   🔔 **Notifications** : Alertes avant expiration
-   👥 **Multi-utilisateurs** : Gestion de foyers partagés
-   📸 **Reconnaissance d'image** : Scan de produits (futur)

## 🏗️ Architecture

### Clean Architecture

```
┌───────────────────────────────────────────────┐
│             Presentation Layer                 │
│         (Controllers, Middleware)              │
├───────────────────────────────────────────────┤
│            Application Layer                   │
│      (Use Cases, DTOs, Interfaces)            │
├───────────────────────────────────────────────┤
│              Domain Layer                      │
│    (Entities, Value Objects, Events)          │
├───────────────────────────────────────────────┤
│           Infrastructure Layer                 │
│    (EF Core, External Services, Email)        │
└───────────────────────────────────────────────┘

Flux : UI → Controller → Use Case → Domain → Repository → Database
```

### Principes appliqués

-   **Domain-Driven Design (DDD)** : Le domaine métier au centre
-   **CQRS Pattern** : Séparation Command/Query avec MediatR
-   **Repository Pattern** : Abstraction de l'accès aux données
-   **SOLID Principles** : Code maintenable et extensible
-   **Dependency Injection** : Inversion de contrôle native .NET

## ✅ Prérequis

-   [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
-   [Docker Desktop](https://www.docker.com/products/docker-desktop)
-   [Git](https://git-scm.com/)
-   IDE : [Visual Studio 2022](https://visualstudio.microsoft.com/) / [VS Code](https://code.visualstudio.com/) / [Rider](https://www.jetbrains.com/rider/)

## 🚀 Installation

### 1. Cloner les repositories

```bash
# Infrastructure (obligatoire)
git clone https://github.com/[votre-org]/foodkeep-infra.git

# Backend
git clone https://github.com/[votre-org]/foodkeep-backend.git
```

### 2. Structure des projets

```
workspace/
├── foodkeep-backend/       # Ce projet
├── foodkeep-infra/        # Infrastructure Docker
├── foodkeep-mobile/       # App mobile (futur)
└── foodkeep-web/          # App web (futur)
```

### 3. Démarrer l'infrastructure

```bash
cd foodkeep-infra
make up     # Lance PostgreSQL, Redis, MailHog
```

### 4. Installer les dépendances

```bash
cd ../foodkeep-backend
dotnet restore
```

## 💻 Développement

### Workflow recommandé

#### Option 1 : Développement local avec hot-reload

```bash
# Terminal 1 : Infrastructure
cd foodkeep-infra
make up

# Terminal 2 : API avec hot-reload
cd ../foodkeep-backend
dotnet watch run --project FoodKeep.API
```

**URLs disponibles :**

-   🌐 API : http://localhost:5000
-   📚 Swagger : http://localhost:5000/swagger
-   💾 PostgreSQL : `localhost:5432`
-   🚀 Redis : `localhost:6379`
-   📧 MailHog : http://localhost:8025

#### Option 2 : Tout en Docker

```bash
cd foodkeep-infra
make stack              # Lance infrastructure + API
make api-logs          # Voir les logs de l'API
```

**API sur :** http://localhost:8080

### Variables d'environnement

| Variable                               | Description            | Valeur par défaut                                                                            |
| -------------------------------------- | ---------------------- | -------------------------------------------------------------------------------------------- |
| `ASPNETCORE_ENVIRONMENT`               | Environnement          | `Development`                                                                                |
| `UseInMemoryDatabase`                  | Utiliser DB en mémoire | `false`                                                                                      |
| `ConnectionStrings__DefaultConnection` | PostgreSQL             | `Host=localhost;Port=5432;Database=foodkeep;Username=foodkeep_user;Password=DevPassword123!` |
| `ConnectionStrings__Redis`             | Redis                  | `localhost:6379`                                                                             |
| `JWT__Secret`                          | Clé secrète JWT        | Générer en production                                                                        |
| `JWT__Issuer`                          | Émetteur JWT           | `FoodKeep.API`                                                                               |
| `JWT__Audience`                        | Audience JWT           | `FoodKeep.Client`                                                                            |

## 📁 Structure du projet

```
foodkeep-backend/
├── FoodKeep.Domain/              # 🏛️ Logique métier pure
│   ├── Common/                   # Classes de base
│   ├── Entities/                 # Entités métier
│   ├── ValueObjects/            # Objets valeur DDD
│   ├── Enums/                   # Énumérations
│   ├── Events/                  # Domain events
│   └── Exceptions/              # Exceptions métier
│
├── FoodKeep.Application/         # 💼 Logique applicative
│   ├── Common/
│   │   ├── Interfaces/          # Contrats
│   │   ├── Behaviours/          # Pipeline MediatR
│   │   └── Mappings/            # AutoMapper
│   └── Features/                # Use Cases CQRS
│       ├── Auth/
│       ├── Foods/
│       └── Notifications/
│
├── FoodKeep.Infrastructure/      # 🔧 Implémentations
│   ├── Persistence/             # EF Core, DbContext
│   ├── Identity/                # Authentication
│   └── Services/                # Services externes
│
├── FoodKeep.API/                 # 🎯 Présentation
│   ├── Controllers/             # Endpoints REST
│   ├── Middleware/              # Middleware custom
│   ├── Extensions/              # Extensions
│   └── Program.cs               # Point d'entrée
│
├── FoodKeep.Tests/              # 🧪 Tests
│   ├── Domain/
│   ├── Application/
│   ├── Infrastructure/
│   └── API/
│
├── Dockerfile                   # 🐳 Config Docker
└── FoodKeep.sln                # Solution .NET
```

## 📖 API Documentation

### Swagger/OpenAPI

Documentation interactive disponible :

-   **Local** : http://localhost:5000/swagger
-   **Docker** : http://localhost:8080/swagger

### Endpoints principaux

| Méthode  | Endpoint             | Description       | Auth |
| -------- | -------------------- | ----------------- | ---- |
| `GET`    | `/health`            | Health check      | ❌   |
| `POST`   | `/api/auth/register` | Inscription       | ❌   |
| `POST`   | `/api/auth/login`    | Connexion         | ❌   |
| `GET`    | `/api/foods`         | Liste aliments    | ✅   |
| `POST`   | `/api/foods`         | Ajouter aliment   | ✅   |
| `PUT`    | `/api/foods/{id}`    | Modifier aliment  | ✅   |
| `DELETE` | `/api/foods/{id}`    | Supprimer aliment | ✅   |
| `GET`    | `/api/notifications` | Notifications     | ✅   |
| `GET`    | `/api/analytics`     | Statistiques      | ✅   |

### Authentification

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

### Exemples de requêtes

#### Login

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"Password123!"}'
```

#### Ajouter un aliment

```bash
curl -X POST http://localhost:5000/api/foods \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{"name":"Lait","expirationDate":"2025-09-10","quantity":1}'
```

## 🧪 Tests

### Structure des tests

```
FoodKeep.Tests/
├── Domain/          # Tests unitaires domaine
├── Application/     # Tests use cases
├── Infrastructure/  # Tests intégration
└── API/            # Tests E2E
```

### Exécution

```bash
# Tous les tests
dotnet test

# Avec couverture
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=lcov

# Par catégorie
dotnet test --filter "Category=Unit"
dotnet test --filter "Category=Integration"
```

### Tests en Docker

```bash
cd foodkeep-infra
make api-test
```

## 💾 Base de données

### Migrations

```bash
# Créer une migration
dotnet ef migrations add <NomMigration> \
  -p FoodKeep.Infrastructure \
  -s FoodKeep.API

# Appliquer les migrations
dotnet ef database update \
  -p FoodKeep.Infrastructure \
  -s FoodKeep.API

# Via Docker
cd foodkeep-infra
make db-migrate
```

### Accès direct

```bash
# Via Adminer (interface web)
open http://localhost:8081
# Login : foodkeep_user / DevPassword123!

# Via CLI
docker exec -it foodkeep-postgres psql -U foodkeep_user -d foodkeep
```

## 🚢 Déploiement

### Build Docker

```bash
cd foodkeep-infra
make api-build
```

### Configuration production

```yaml
# docker-compose.prod.yml
services:
    api:
        image: foodkeep-api:latest
        environment:
            - ASPNETCORE_ENVIRONMENT=Production
            - UseInMemoryDatabase=false
            - ConnectionStrings__DefaultConnection=${PROD_DB_CONNECTION}
            - JWT__Secret=${JWT_SECRET}
```

### Health checks

-   **Liveness** : `/health/live`
-   **Readiness** : `/health/ready`
-   **Database** : `/health/db`

## 🔧 Dépannage

### Problèmes courants

#### L'API ne démarre pas

```bash
# Vérifier les logs
cd foodkeep-infra
make api-logs

# Vérifier la base de données
docker-compose ps
make db-reset  # Reset si nécessaire
```

#### Erreurs de connexion PostgreSQL

```bash
# Vérifier que PostgreSQL est healthy
docker-compose ps

# Tester la connexion
docker exec -it foodkeep-postgres pg_isready
```

#### Erreurs de build

```bash
# Nettoyer et reconstruire
dotnet clean
dotnet restore
dotnet build
```

## 🤝 Contribution

### Workflow Git

```
main
  └── develop
       ├── feature/MVP-XXX-description
       ├── bugfix/XXX-description
       └── hotfix/XXX-description
```

### Convention de commit

```bash
feat: nouvelle fonctionnalité
fix: correction de bug
docs: documentation
chore: maintenance
test: ajout de tests
refactor: refactoring
perf: amélioration performance
```

### Standards de code

-   Suivre les [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
-   Utiliser l'analyseur de code Roslyn
-   Maintenir une couverture de tests > 80%

### Pull Request

1. Fork le projet
2. Créer une branche (`git checkout -b feature/AmazingFeature`)
3. Commit (`git commit -m 'feat: add amazing feature'`)
4. Push (`git push origin feature/AmazingFeature`)
5. Ouvrir une Pull Request

## 📞 Support

-   **Documentation** : [Wiki](https://github.com/[votre-org]/foodkeep-backend/wiki)
-   **Issues** : [GitHub Issues](https://github.com/[votre-org]/foodkeep-backend/issues)
-   **Email** : support@foodkeep.app
-   **Discord** : [FoodKeep Community](https://discord.gg/foodkeep)

## 📄 Licence

MIT License - Voir [LICENSE](LICENSE) pour plus de détails.

## 🙏 Remerciements

-   [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html) - Robert C. Martin
-   [Domain-Driven Design](https://www.domainlanguage.com/) - Eric Evans
-   [MediatR](https://github.com/jbogard/MediatR) - Jimmy Bogard
-   [Entity Framework Core](https://docs.microsoft.com/ef/core/) - Microsoft

---

**FoodKeep Backend** - Développé avec ❤️ pour réduire le gaspillage alimentaire

_Version 1.0.0 - MVP_
