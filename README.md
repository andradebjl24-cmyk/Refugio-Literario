# 🌿 Refúgio Literário

### ✨ Um lugar onde cada livro esconde um mundo inteiro.

O **Refúgio Literário** é um sistema web de gerenciamento de biblioteca, desenvolvido para organizar acervos literários, conectar leitores às suas próximas histórias e facilitar a administração de livros, exemplares e empréstimos.

Construído com **ASP.NET Core MVC, .NET 10, Entity Framework Core e SQL Server**, o projeto reúne um catálogo literário com recursos para leitores e uma área administrativa protegida por autenticação e autorização.

<p align="center">
  📚 <strong>Explore histórias.</strong>
  &nbsp; · &nbsp;
  🌱 <strong>Descubra novos mundos.</strong>
  &nbsp; · &nbsp;
  🪄 <strong>Viva a literatura.</strong>
</p>

---

## 🧭 Índice

- [📖 Sobre o projeto](#-sobre-o-projeto)
- [✨ Funcionalidades](#-funcionalidades)
- [🛠️ Tecnologias utilizadas](#️-tecnologias-utilizadas)
- [🏗️ Arquitetura do projeto](#️-arquitetura-do-projeto)
- [⚙️ Como executar](#️-como-executar)
- [🗄️ Banco de dados](#️-banco-de-dados)
- [📚 Regras de empréstimo](#-regras-de-empréstimo)
- [🔐 Segurança](#-segurança)
- [🌱 Próximos passos](#-próximos-passos)
- [👩‍💻 Desenvolvido com](#-desenvolvido-com)

---

## 📖 Sobre o projeto

O **Refúgio Literário** nasceu da ideia de transformar a experiência de uma biblioteca em um ambiente organizado, acessível e acolhedor para quem ama ler.

A aplicação permite explorar o acervo, pesquisar títulos e autores, navegar por categorias e subcategorias, salvar livros favoritos, publicar comentários e acompanhar solicitações de empréstimo.

Para a administração, oferece ferramentas de gerenciamento do catálogo, dos usuários e da circulação dos exemplares.

O projeto utiliza o padrão **Model–View–Controller (MVC)** para organizar responsabilidades e facilitar a manutenção e a evolução do sistema.

## ✨ Funcionalidades

### 📚 Para leitores

- **Catálogo literário:** consulta e exploração dos livros disponíveis no sistema.
- **Pesquisa:** busca de livros e autores.
- **Categorias e subcategorias:** organização hierárquica do acervo para facilitar a descoberta de histórias.
- **Conta de usuário:** cadastro, autenticação e acesso ao perfil.
- **Favoritos:** coleção pessoal de livros preferidos.
- **Comentários:** interação dos leitores por meio de comentários sobre os livros.
- **Solicitação de empréstimos:** pedido de empréstimo de exemplares disponíveis.
- **Acompanhamento:** consulta aos empréstimos e às respectivas situações, incluindo devoluções.

### 🛡️ Para administradores

- **Painel administrativo:** visualização de indicadores do sistema.
- **Gerenciamento de livros:** cadastro e manutenção do catálogo.
- **Gerenciamento de autores e editoras:** organização das informações bibliográficas.
- **Gerenciamento de categorias:** criação e organização de categorias principais e subcategorias.
- **Controle de exemplares:** gerenciamento das unidades físicas e de sua disponibilidade.
- **Gerenciamento de usuários:** administração dos usuários cadastrados.
- **Controle de empréstimos:** análise, aprovação e recusa de solicitações, além do registro de devoluções.
- **Acesso protegido:** área administrativa restrita ao perfil autorizado de administrador.

## 🛠️ Tecnologias utilizadas

| Tecnologia | Finalidade |
|---|---|
| C# | Linguagem principal |
| .NET 10 | Plataforma de desenvolvimento |
| ASP.NET Core MVC | Estrutura da aplicação web |
| Entity Framework Core 10 | Mapeamento objeto-relacional e acesso a dados |
| SQL Server | Banco de dados relacional |
| Razor | Renderização das páginas dinâmicas |
| HTML5 | Estrutura das páginas |
| CSS3 | Estilização e responsividade |
| Bootstrap 5.3 | Componentes e recursos de interface |
| Git e GitHub | Versionamento e hospedagem do código |

## 🏗️ Arquitetura do projeto

A aplicação está organizada em diretórios que separam as principais responsabilidades do sistema.

```text
Biblioteca/
├── Areas/
│   └── Admin/
│       ├── Controllers/
│       └── Views/
├── Controllers/
├── Data/
│   └── BibliotecaContext.cs
├── Migrations/
├── Models/
├── ViewModels/
├── Views/
├── wwwroot/
├── appsettings.json
├── Biblioteca.csproj
├── Biblioteca.sln
├── Program.cs
└── README.md
```

**Responsabilidade de cada diretório:**

- `Areas/Admin/Controllers`: ações e operações da área administrativa.
- `Areas/Admin/Views`: páginas da administração.
- `Controllers`: controladores das funcionalidades públicas e dos usuários.
- `Data`: contexto e configurações de acesso ao banco de dados.
- `Migrations`: histórico de alterações do esquema do banco.
- `Models`: entidades e tipos utilizados pela aplicação.
- `ViewModels`: modelos específicos para páginas e formulários.
- `Views`: páginas Razor da aplicação.
- `wwwroot`: arquivos estáticos, como CSS, JavaScript e imagens.
- `Program.cs`: configuração e inicialização da aplicação.
- `appsettings.json`: configurações da aplicação, incluindo a conexão padrão com o banco.

## ⚙️ Como executar

### Pré-requisitos

Antes de iniciar, tenha instalado:

- [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0)
- [SQL Server](https://www.microsoft.com/sql-server)
- [Visual Studio](https://visualstudio.microsoft.com/) com suporte ao desenvolvimento ASP.NET e web, ou um editor de sua preferência.

### 1. Clone o repositório

```bash
git clone https://github.com/andradebjl24-cmyk/Refugio-Literario.git
```

### 2. Acesse a pasta do projeto

```bash
cd Refugio-Literario
```

Caso a solução esteja dentro de uma subpasta, entre nela antes de continuar.

### 3. Configure a conexão com o banco

Abra `appsettings.json` e confira a chave:

```text
ConnectionStrings:DefaultConnection
```

Ajuste o servidor e o nome do banco de dados conforme o ambiente em que a aplicação será executada.

O projeto foi configurado para utilizar um banco SQL Server local chamado `RefugioLiterario`. Essa configuração pode precisar de ajustes em outro computador.

### 4. Restaure as dependências

```bash
dotnet restore
```

### 5. Compile o projeto

```bash
dotnet build
```

### 6. Execute a aplicação

Entre na pasta que contém `Biblioteca.csproj` e execute:

```bash
dotnet run
```

### 7. Acesse no navegador

Utilize a URL HTTP ou HTTPS exibida no terminal após a inicialização.

> **Importante:** a execução depende de uma instância acessível do SQL Server, de uma conexão configurada corretamente e de um banco compatível com as migrations do projeto.

## 🗄️ Banco de dados

O acesso aos dados é realizado pelo **Entity Framework Core**, utilizando o contexto `BibliotecaContext` e o SQL Server.

O sistema trabalha com entidades relacionadas às principais operações de uma biblioteca, incluindo:

- Usuários
- Livros
- Autores
- Categorias
- Editoras
- Exemplares
- Empréstimos
- Comentários
- Favoritos
- Relacionamentos entre livros e autores

### Migrations

O projeto utiliza migrations para registrar alterações na estrutura do banco de dados.

Antes de executar a aplicação em outro ambiente:

1. Confira a conexão com o SQL Server.
2. Verifique se o banco está criado e se corresponde ao modelo atual.
3. Confira as