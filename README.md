# PousadaApp

Sistema SaaS multi-tenant de gestao integrada para pousadas, desenvolvido como TCC do MBA em Engenharia de Software da USP/Esalq.

Autor: Igor Brandao Fagundes do Nascimento — Orientador: Prof. Victor Inacio de Oliveira

Integra **Hospedagem** (quartos, hospedes, reservas), **Restaurante / POS** (cardapio, comandas) e **Financeiro** (consolidacao de receitas) em uma unica plataforma.

## Stack

| Camada | Tecnologia |
| --- | --- |
| Frontend | Angular 19 + PrimeNG + Material Icons (signals, standalone components, novo control flow) |
| Backend | .NET 8 Clean Architecture + EF Core |
| Auth | JWT Bearer + BCrypt |
| Banco | SQL Server 2022 |
| Tests | xUnit (77 testes) |
| DevOps | Docker Compose, GitHub Actions, Azure App Service |

## Rodando com Docker (recomendado)

Pre-requisitos: Docker Desktop instalado.

```bash
# 1. Copiar variaveis de ambiente
cp .env.example .env
# (edite .env e ajuste DB_PASSWORD e JWT_KEY se quiser)

# 2. Subir toda a stack (DB + API + Frontend)
docker compose up --build
```

Servicos:

| URL | O que e |
| --- | --- |
| http://localhost:4200 | Frontend Angular (nginx) |
| http://localhost:5000/api | API .NET 8 |
| localhost:1433 | SQL Server (sa / valor de DB_PASSWORD) |

Para parar: `docker compose down` (mantem dados). Para apagar volume do banco: `docker compose down -v`.

### Login padrao

```
Email:  admin@pousadaapp.com
Senha:  admin123
```

(Definidos no seed do banco em `api/src/PousadaApp.Infrastructure/Data/SeedData.cs`.)

## Desenvolvimento local (sem Docker)

### Frontend

```bash
npm install
npx ng serve
# http://localhost:4200
```

### Backend

```bash
cd api
dotnet restore
dotnet ef database update --project src/PousadaApp.Infrastructure --startup-project src/PousadaApp.API
dotnet run --project src/PousadaApp.API
# http://localhost:5000
```

### Testes

```bash
cd api
dotnet test
```

## Estrutura

```
pousada-app/
  src/                           # Frontend Angular
    app/
      core/                      # services, models, guards, interceptors, layout
      pages/                     # dashboard, agenda, hospedes, quartos, reservas, restaurante, cardapio, financeiro, login
  api/                           # Backend .NET 8
    src/
      PousadaApp.Domain/         # entidades, value objects
      PousadaApp.Infrastructure/ # EF Core, repos, migrations, seed
      PousadaApp.API/            # controllers, DTOs, JWT, Program.cs
    tests/PousadaApp.Tests/      # xUnit
  docker-compose.yml
  Dockerfile.frontend            # nginx + build Angular
  api/Dockerfile                 # .NET 8 runtime
  .github/workflows/             # CI + deploys Azure
```

## Citacao

Se voce utilizar este software academicamente, por favor cite conforme `CITATION.cff`.

## Licenca

MIT — veja `LICENSE`.

## TCC

Trabalho de conclusao do MBA em Engenharia de Software da USP/Esalq (2026). Documento, diagramas e prints permanecem com o autor; este repositorio contem o artefato de software desenvolvido.
