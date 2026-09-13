# UCS — Projeto Integrador I-V (B)

Resumo técnico
-------------
Aplicação web ASP.NET Core (Razor Pages / controllers + views) implementando CRUD para Gestão de Estágios por Empresas.
Arquitetura simples em camadas:
- `Controllers` (endpoints)
- `Services` (lógica de negócio)
- `Entities/` (entidades)

Publish e Execução do Projeto
----------
PUBLISH
- Executar via cmd, power shwell ou algum terminal 
   - `dotnet publish -c Release -r win-x64 --self-contained false -o ./publish`

EXECUTAR O PROJETO
- Acessar a pasta publish recénm publicada

- Executar via cmd, power shwell ou algum terminal 
   - Setar a variável de ambiente
      - `$env:ASPNETCORE_ENVIRONMENT = 'Production'`
   - Executar o comando para rodar a aplicação
      - `dotnet .\UCS-ProjetoIntegrador-III-B.dll`
   - Acessar via browser o endereço retornado no terminal
      - Ex: "Now listening on: http://localhost:5000"
      - `http://localhost:5000`

Requisitos
----------
- .NET SDK 10 (instalar do site da Microsoft)
- SQL Server (instância acessível) — pode ser SQL Server LocalDB, Developer ou remota


Execução
--------
Abra terminal na pasta do projeto (`UCS-ProjetoIntegrador-III-B`) e execute:

```
dotnet restore
dotnet build
dotnet run
```

A aplicação inicia e fica disponível em `https://localhost:5001` (ou porta mostrada no console).

Estrutura de pastas relevante
----------------------------
- `Controllers/` — controllers MVC
- `Views/` — views Razor
- `Services/` — regras de negócio
- `Entities/` — classes que representam as tabelas
