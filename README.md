# Sistema de Gestão Eletrônica de Fichas de EPI

Sistema corporativo para controle de entrega e assinatura eletrônica de Equipamentos de Proteção Individual (EPI) com autenticação biométrica via DigitalPersona.

## Tecnologias

| Camada | Tecnologia |
|---|---|
| Backend | C# .NET 8 / ASP.NET Core Web API |
| Frontend | React 18 + TypeScript + Vite |
| Banco de Dados | PostgreSQL 16 |
| ORM | Entity Framework Core 8 |
| Autenticação | JWT Bearer |
| Biometria | DigitalPersona One Touch SDK |
| UI | Material UI v5 |
| Estado | React Query + Zustand |
| PDF | QuestPDF |

## Estrutura do Projeto

```
digital-rh/
├── backend/
│   └── src/
│       ├── EpiManagement.Domain/         # Entidades, interfaces, enums
│       ├── EpiManagement.Application/    # Services, DTOs, casos de uso
│       ├── EpiManagement.Infrastructure/ # EF Core, repositórios, DigitalPersona
│       └── EpiManagement.API/            # Controllers, middlewares, Swagger
├── frontend/
│   └── src/
│       ├── api/          # Clientes HTTP (axios)
│       ├── components/   # Layout, componentes compartilhados
│       ├── pages/        # Páginas da aplicação
│       └── store/        # Estado global (Zustand)
└── docker-compose.yml
```

## Execução Rápida com Docker

```bash
docker-compose up -d
```

Acesse: http://localhost:3000

**Credenciais padrão:** `admin@epi.com` / `Admin@123`

## Execução Local (Desenvolvimento)

### Pré-requisitos
- .NET 8 SDK
- Node.js 20+
- PostgreSQL 16

### Backend

```bash
cd backend
# Criar banco e executar migrations
dotnet ef database update --project src/EpiManagement.Infrastructure --startup-project src/EpiManagement.API

# Executar API
dotnet run --project src/EpiManagement.API
# API disponível em: http://localhost:5000
# Swagger: http://localhost:5000/swagger
```

### Frontend

```bash
cd frontend
npm install
npm run dev
# Disponível em: http://localhost:3000
```

## Módulos do Sistema

### 1. Dashboard
- Entregas do dia
- Funcionários atendidos
- EPIs vencendo nos próximos 30 dias
- Entregas recentes

### 2. Módulo RH
- **Funcionários:** cadastro, edição, ativação/desativação, histórico de EPIs
- **Setores:** gerenciamento de setores
- **EPIs:** catálogo com código, tipo, validade

### 3. Módulo Almoxarifado — Entrega de EPI (4 passos)
1. **Identificação biométrica** — leitura da digital no leitor DigitalPersona
2. **Seleção de EPIs** — operador seleciona itens e quantidades
3. **Assinatura biométrica** — funcionário assina com a digital
4. **Confirmação** — registra entrega com hash biométrico

### 4. Ficha Eletrônica
- Histórico completo por funcionário
- Exportação em PDF com dados e assinaturas

### 5. Relatórios
- Funcionários por setor (ativos/inativos)
- Catálogo de EPIs
- Entregas recentes

### 6. Usuários
- Perfis: **Administrador**, **RH**, **Almoxarifado**
- Controle de acesso por role

## Integração DigitalPersona SDK

O arquivo de integração está em:
`backend/src/EpiManagement.Infrastructure/Biometric/DigitalPersonaService.cs`

### Para integrar o SDK real:

1. Instale o **DigitalPersona One Touch for Windows SDK** (Setup.exe na pasta `Digital-Persona-SDK-master/SDK/`)
2. Referencie o assembly `DpOTDotNET.dll` no projeto Infrastructure
3. Substitua os métodos em `DigitalPersonaService.cs`:

```csharp
// Captura de template (enrollment)
var capture = new DPFP.Capture.Capture();
var enrollment = new DPFP.Processing.Enrollment();
// Capturar 4+ amostras, depois: enrollment.Template.Serialize()

// Identificação 1:N
var verifier = new DPFP.Verification.Verification();
var result = new DPFP.Verification.Verification.Result();
verifier.Verify(featureSet, storedTemplate, ref result);
if (result.Verified) { /* funcionário identificado */ }
```

4. No frontend (`DeliveryPage.tsx`), substitua o `setTimeout` mock por chamada ao bridge local que acessa o SDK Windows via WebSocket ou named pipe.

## Segurança

- Senhas com SHA-256 + salt
- JWT com expiração de 8 horas
- Roles: Administrator > HR > Warehouse
- CORS restrito a localhost em desenvolvimento

## Perfis de Acesso

| Tela | Administrator | RH | Almoxarifado |
|---|:---:|:---:|:---:|
| Dashboard | ✓ | ✓ | ✓ |
| Funcionários | ✓ | ✓ | - |
| EPIs | ✓ | ✓ | - |
| Setores | ✓ | ✓ | - |
| Entrega de EPI | ✓ | - | ✓ |
| Relatórios | ✓ | ✓ | - |
| Usuários | ✓ | - | - |
