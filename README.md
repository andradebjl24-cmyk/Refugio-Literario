# Biblioteca MVC — documentação atualizada

Sistema de gerenciamento de biblioteca desenvolvido em ASP.NET Core MVC com .NET 10, Entity Framework Core e SQL Server.

## Funcionalidades

- Catálogo, busca e filtragem de livros por categoria e subcategoria.
- Cadastro e autenticação de usuários.
- Favoritos e comentários.
- Área administrativa protegida por perfil de administrador.
- Gestão de livros, autores, categorias, editoras e exemplares.
- Solicitações de empréstimo, aprovação, recusa e devolução.

## Tecnologias

- ASP.NET Core MVC (.NET 10)
- Entity Framework Core 10
- SQL Server
- Razor, HTML, CSS e Bootstrap

## Configuração do banco

A conexão é configurada em `appsettings.json`, na chave `ConnectionStrings:DefaultConnection`. O arquivo entregue usa o banco `RefugioLiterario` no SQL Server local. Ajuste o servidor e o nome do banco conforme o ambiente antes de executar.

As migrations são aplicadas pela inicialização da aplicação. Antes de executar em um ambiente com dados importantes, faça backup do banco e confira a migration pendente.

## Executar localmente

1. Instale o .NET SDK 10 e o SQL Server.
2. Abra um terminal na pasta que contém `Biblioteca.csproj`.
3. Revise `appsettings.json` para confirmar a conexão correta.
4. Execute:

   ```bash
   dotnet restore
   dotnet build
   dotnet run
   ```

5. Abra a URL indicada no terminal.

## Administrador

A aplicação não cria credenciais de administrador padrão no código. Utilize o usuário administrador já provisionado no banco de dados ou o procedimento seguro de administração definido para o ambiente. Não coloque senhas reais no README, no repositório ou em arquivos versionados.

## Regras de empréstimo

- O usuário não deve manter solicitação pendente ou empréstimo ativo do mesmo livro, ainda que existam vários exemplares desse título.
- O administrador deve validar novamente a regra ao criar ou aprovar um empréstimo.
- Um exemplar precisa estar disponível para ser emprestado.
- Somente solicitações pendentes podem ser aprovadas ou recusadas.
- Somente empréstimos ativos podem ser devolvidos.

## Estrutura

- `Controllers`: páginas e ações públicas.
- `Areas/Admin/Controllers`: ações administrativas.
- `Areas/Admin/Views`: telas administrativas.
- `Models`: entidades e enums.
- `ViewModels`: modelos específicos de formulários e páginas.
- `Data/BibliotecaContext.cs`: contexto e configurações do Entity Framework.
- `Migrations`: histórico de alterações do banco.
- `wwwroot`: CSS, JavaScript e imagens públicas.

## Observações

- Nunca compartilhe senhas, strings de conexão com credenciais ou dados pessoais reais.
- Ao publicar o sistema, configure segredos e conexões por variáveis de ambiente ou mecanismo de secrets.
- O projeto precisa ser compilado e testado no ambiente de desenvolvimento após cada alteração.
