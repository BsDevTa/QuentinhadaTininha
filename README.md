# Quentinhas da Tininha

> Um sistema que nasceu de uma necessidade simples e real: facilitar os pedidos de um pequeno restaurante sem tirar da proprietária o controle sobre o próprio negócio.

## Sobre o projeto

O **Quentinhas da Tininha** é um projeto Full Stack desenvolvido para atender uma necessidade real de um pequeno restaurante.

A ideia inicial parecia simples: criar um cardápio online onde os clientes pudessem consultar as refeições disponíveis.

Mas, quando comecei a entender melhor como o restaurante realmente funcionava no dia a dia, ficou claro que somente colocar um cardápio na internet não resolveria o problema.

O cardápio muda.

Alguns pratos podem acabar durante o dia.

Existem acompanhamentos diferentes para determinadas refeições.

O restaurante pode abrir ou fechar excepcionalmente.

Os valores podem variar de acordo com o tamanho e a forma de pagamento.

Existem bairros com valores de entrega diferentes.

E, principalmente, a proprietária precisava conseguir controlar tudo isso sem depender de alguém alterando o código da aplicação.

Foi a partir dessa necessidade que o **Quentinhas da Tininha** começou a se transformar de uma simples página de cardápio em um sistema completo de gerenciamento e pedidos.

---

# O problema

Antes de pensar nas tecnologias, tentei entender o funcionamento do negócio.

Em um restaurante desse tipo, o cardápio não é completamente estático.

Um prato que estava disponível pela manhã pode acabar algumas horas depois.

Em determinado dia pode existir feijoada, cozido ou comida baiana. Em outro, o cardápio já pode ser diferente.

Além disso, cada prato pode possuir regras próprias de acompanhamentos.

Por exemplo, um prato pode permitir:

* arroz;
* feijão de caldo;
* feijão tropeiro;
* macarrão;
* salada.

Enquanto outro pode trabalhar com:

* arroz;
* feijão fradinho;
* caruru;
* vatapá;
* farofa.

Criar apenas uma página bonita não seria suficiente.

Era necessário criar uma aplicação capaz de representar essas regras.

---

# A solução

O sistema foi dividido em duas experiências principais:

```text
CLIENTE
   │
   ▼
Cardápio público
   │
   ▼
Escolha do prato
   │
   ▼
Personalização
   │
   ▼
Tamanho / acompanhamentos
   │
   ▼
Dados para entrega
   │
   ▼
Pedido
   │
   ▼
Restaurante


ADMINISTRAÇÃO
   │
   ▼
Login
   │
   ▼
Painel administrativo
   │
   ├── Pratos
   ├── Cardápio
   ├── Acompanhamentos
   ├── Disponibilidade
   ├── Funcionamento
   ├── Fretes
   ├── Imagens
   └── Pedidos
```

Dessa forma, o cliente possui uma experiência simples para fazer o pedido, enquanto a proprietária possui uma área separada para administrar o restaurante.

---

# Área do cliente

A parte pública da aplicação foi construída pensando principalmente em simplicidade.

O objetivo não era fazer o cliente aprender a utilizar um sistema.

Ele deveria simplesmente abrir a página, visualizar a comida e fazer o pedido.

Na aplicação é possível trabalhar com:

* cardápio disponível no dia;
* categorias de pratos;
* fotos das refeições;
* tamanhos diferentes;
* preços;
* acompanhamentos;
* personalização do pedido;
* observações;
* endereço de entrega;
* cálculo de frete;
* disponibilidade do restaurante;
* realização do pedido.

Toda a experiência foi pensada para ser utilizada principalmente pelo celular, já que esse é o dispositivo mais comum para esse tipo de pedido.

---

# Personalização dos pratos

Um dos pontos mais interessantes do projeto foi perceber que um prato não poderia ser tratado simplesmente como:

```text
Nome
Preço
Foto
```

Na prática, existem regras.

Cada prato pode possuir grupos diferentes de acompanhamentos e opções disponíveis para o cliente.

Por isso, o sistema possui uma estrutura específica para relacionar:

```text
Prato
   │
   ├── Categoria
   ├── Preços
   ├── Imagem
   ├── Disponibilidade
   │
   └── Grupos de acompanhamentos
           │
           ├── Arroz
           ├── Feijão
           ├── Macarrão
           ├── Salada
           └── outras opções
```

Esse tipo de situação foi importante para mim porque mostrou, na prática, que desenvolver software não é apenas criar telas.

É necessário transformar regras que existem no mundo real em estruturas que o sistema consiga entender.

---

# Painel administrativo

Uma das decisões mais importantes do projeto foi criar um painel administrativo.

A proprietária não deveria precisar falar com o desenvolvedor toda vez que:

* um prato acabasse;
* uma imagem precisasse ser alterada;
* o restaurante não funcionasse naquele dia;
* um acompanhamento mudasse;
* um novo prato fosse disponibilizado;
* um valor fosse atualizado.

O sistema precisava dar autonomia para quem realmente administra o restaurante.

Por isso, foi criada uma área administrativa protegida por autenticação.

Através do painel é possível gerenciar diferentes partes da operação.

---

## Gerenciamento de pratos

Os pratos podem ser cadastrados e administrados individualmente.

Cada prato pode possuir informações como:

* nome;
* descrição;
* categoria;
* imagem;
* preço;
* disponibilidade;
* acompanhamentos relacionados.

---

## Disponibilidade

Nem sempre tudo que está cadastrado está disponível.

Por isso, o sistema diferencia **existência** de **disponibilidade**.

Um prato pode continuar cadastrado no sistema e simplesmente ser marcado como indisponível.

Isso evita apagar e recriar produtos constantemente.

---

## Cardápio por dia

O restaurante possui pratos que podem variar de acordo com o dia.

Por isso, o sistema trabalha com uma estrutura de cardápio diário.

A proposta é permitir que a administração defina o que será exibido para o cliente de acordo com o funcionamento real do restaurante.

---

## Acompanhamentos

Os acompanhamentos também são tratados como parte da regra de negócio.

O sistema permite organizar opções que posteriormente podem ser associadas aos pratos.

Isso torna possível representar cardápios diferentes sem precisar criar essas combinações diretamente no código.

---

## Funcionamento do restaurante

Outra regra importante é saber se o restaurante pode receber pedidos naquele momento.

Foram criadas estruturas para controlar:

* horários de funcionamento;
* disponibilidade;
* fechamentos excepcionais.

Assim, a aplicação consegue representar situações como um dia em que normalmente haveria atendimento, mas o restaurante precisou ficar fechado.

---

## Frete por bairro

A entrega também faz parte da regra de negócio.

O sistema possui estrutura para relacionar bairros e valores de frete.

Com isso, o valor da entrega pode ser determinado de acordo com a localização informada pelo cliente.

---

# Pedidos

O pedido é o ponto onde várias partes do sistema se encontram.

Um pedido pode envolver:

```text
Cliente
   +
Prato
   +
Tamanho
   +
Acompanhamentos
   +
Observações
   +
Endereço
   +
Frete
   =
PEDIDO
```

Essa parte do projeto exigiu pensar não somente no que aparece na tela, mas em como as informações deveriam ser armazenadas e relacionadas no banco de dados.

---

# Arquitetura

No backend, decidi não concentrar toda a aplicação em um único projeto.

A solução foi separada em camadas:

```text
src/
│
├── QuentinhasDaTininha.Api
│
├── QuentinhasDaTininha.Aplicacao
│
├── QuentinhasDaTininha.Dominio
│
└── QuentinhasDaTininha.Infraestrutura
```

Cada projeto possui uma responsabilidade diferente.

---

## API

```text
QuentinhasDaTininha.Api
```

É a porta de entrada do backend.

Aqui ficam principalmente:

* Controllers;
* configuração da aplicação;
* autenticação;
* Swagger;
* CORS;
* injeção de dependência;
* configuração do banco;
* configuração dos serviços.

É através dessa camada que o frontend se comunica com o backend.

---

## Aplicação

```text
QuentinhasDaTininha.Aplicacao
```

Essa camada concentra contratos, DTOs e definições relacionadas aos casos de uso da aplicação.

Ela organiza funcionalidades relacionadas a:

* autenticação;
* pratos;
* cardápios;
* pedidos;
* acompanhamentos;
* categorias;
* funcionamento;
* configurações;
* fretes;
* armazenamento;
* informações públicas.

---

## Domínio

```text
QuentinhasDaTininha.Dominio
```

O domínio representa os principais elementos do negócio.

Entre as entidades existentes estão:

```text
Acompanhamento
CardapioDia
CardapioDiaPrato
Categoria
ConfiguracaoRestaurante
FechamentoExcepcional
FreteBairro
GrupoAcompanhamento
GrupoAcompanhamentoItem
HistoricoAlteracao
HorarioFuncionamento
Pedido
PedidoItem
Prato
PratoAcompanhamento
PrecoPrato
UsuarioAdministrativo
```

Essa foi uma parte importante do desenvolvimento, porque foi necessário sair da visão de "telas" e começar a pensar em **entidades e relacionamentos de negócio**.

---

## Infraestrutura

```text
QuentinhasDaTininha.Infraestrutura
```

É onde estão implementados diversos serviços utilizados pela aplicação.

Entre eles:

* persistência dos dados;
* serviços de pratos;
* serviços de pedidos;
* serviços de cardápio;
* autenticação;
* funcionamento;
* disponibilidade;
* fretes;
* armazenamento de imagens.

Essa separação permite que a regra da aplicação não fique completamente acoplada à forma como os dados são armazenados ou acessados.

---

# Frontend

O frontend foi desenvolvido em **Angular**.

A aplicação também foi separada de acordo com as responsabilidades da interface.

```text
src/app/
│
├── admin/
├── administrativo/
├── compartilhado/
├── core/
├── nucleo/
├── publico/
└── shared/
```

A ideia é manter claramente separadas as funcionalidades utilizadas pelos clientes das funcionalidades administrativas.

---

# Backend

O backend foi desenvolvido utilizando **C# e ASP.NET Core**.

A API é responsável por centralizar as regras da aplicação e disponibilizar os dados utilizados pelo frontend.

Também foram utilizados:

* Entity Framework Core;
* autenticação JWT;
* Swagger/OpenAPI;
* injeção de dependência;
* migrations;
* serviços;
* DTOs;
* controllers REST.

---

# Banco de dados

O banco utilizado é **PostgreSQL**.

O acesso aos dados é realizado através do **Entity Framework Core**.

Uma decisão que tomei durante o desenvolvimento foi manter os principais nomes relacionados ao negócio em português.

Isso pode ser percebido tanto nas entidades quanto nos serviços:

```text
Prato
Pedido
Acompanhamento
Cardapio
FreteBairro
HorarioFuncionamento
ConfiguracaoRestaurante
```

Como o projeto foi desenvolvido especificamente para esse negócio, achei mais natural que o domínio também representasse a linguagem utilizada nele.

---

# Imagens

As imagens dos pratos não ficam simplesmente armazenadas dentro do frontend.

O backend possui integração com armazenamento externo através do **Supabase Storage**.

Isso permite que as imagens sejam alteradas pela administração sem ser necessário gerar uma nova versão do frontend para cada mudança de foto.

Esse era um requisito importante, porque o objetivo sempre foi dar autonomia para a proprietária.

---

# Autenticação

A área administrativa não pode ser acessada da mesma forma que o cardápio público.

Por isso, a API possui autenticação utilizando **JWT — JSON Web Token**.

O fluxo pode ser resumido como:

```text
Administrador
      │
      ▼
   Login
      │
      ▼
API valida credenciais
      │
      ▼
Token JWT
      │
      ▼
Acesso às rotas administrativas
```

As rotas públicas continuam acessíveis aos clientes sem necessidade de login.

---

# Tecnologias utilizadas

## Frontend

* Angular 22
* TypeScript
* HTML
* SCSS
* RxJS

## Backend

* C#
* ASP.NET Core / .NET
* Entity Framework Core
* JWT
* Swagger / OpenAPI

## Dados e armazenamento

* PostgreSQL
* Supabase Storage

## Infraestrutura e publicação

* Docker
* Firebase Hosting
* Git
* GitHub

---

# Estrutura geral

```text
QuentinhadaTininha/
│
├── frontend/
│   └── quentinhas-da-tininha-web/
│       │
│       ├── src/
│       │   └── app/
│       │       ├── admin/
│       │       ├── administrativo/
│       │       ├── compartilhado/
│       │       ├── core/
│       │       ├── nucleo/
│       │       └── publico/
│       │
│       ├── angular.json
│       ├── firebase.json
│       └── package.json
│
├── src/
│   │
│   ├── QuentinhasDaTininha.Api/
│   │
│   ├── QuentinhasDaTininha.Aplicacao/
│   │
│   ├── QuentinhasDaTininha.Dominio/
│   │
│   └── QuentinhasDaTininha.Infraestrutura/
│
├── Dockerfile
├── QuentinhasDaTininha.sln
└── README.md
```

---

# Comunicação entre as partes

De forma simplificada, a aplicação funciona assim:

```text
┌─────────────────────────────┐
│          CLIENTE            │
│     Angular / Navegador     │
└──────────────┬──────────────┘
               │
               │ HTTP
               ▼
┌─────────────────────────────┐
│      ASP.NET Core API       │
│                             │
│ Controllers + Serviços      │
│ Regras de negócio + JWT     │
└─────────┬───────────┬───────┘
          │           │
          │           │
          ▼           ▼
┌──────────────┐  ┌───────────────┐
│ PostgreSQL   │  │ Supabase      │
│              │  │ Storage       │
│ Dados        │  │ Imagens       │
└──────────────┘  └───────────────┘
```

---

# O que mais aprendi com esse projeto

Esse projeto foi muito importante para mim porque me fez lidar com situações que não aparecem quando fazemos apenas pequenos exercícios de programação.

Não era mais simplesmente:

> "Crie um CRUD de produtos."

Eu precisava entender perguntas como:

* O que acontece quando o restaurante fecha?
* E se um prato acabar?
* Um prato indisponível deve ser apagado?
* Como representar acompanhamentos diferentes?
* Como calcular o frete?
* Como permitir que a proprietária altere as informações?
* O que pode ser público?
* O que precisa de autenticação?
* Onde armazenar as imagens?
* Como organizar o backend para ele não virar apenas um grande `Program.cs`?
* Como fazer frontend e backend conversarem?
* Como preparar uma aplicação que funciona localmente para funcionar também publicada?

Foram essas perguntas que fizeram o projeto crescer.

---

# Mais do que código

Uma das maiores coisas que aprendi desenvolvendo o Quentinhas da Tininha foi que nem sempre o maior desafio de um sistema está no código.

Antes de criar uma classe, uma API ou uma tela, é necessário entender quem vai utilizar aquilo.

Nesse projeto existem basicamente dois usuários completamente diferentes.

O cliente quer rapidez.

Ele quer abrir o cardápio, escolher a comida e pedir.

Já quem administra o restaurante quer controle.

Precisa alterar pratos, disponibilidade, fotos, valores e funcionamento sem depender de conhecimento técnico.

Construir algo que atendesse essas duas necessidades foi uma das partes mais interessantes do projeto.

---

# Um sistema pensado para um negócio real

O **Quentinhas da Tininha não nasceu como um SaaS genérico para restaurantes**.

A primeira versão foi pensada especificamente para o funcionamento da Quentinhas da Tininha.

Isso foi proposital.

Antes de tentar criar uma plataforma que sirva para dezenas de estabelecimentos diferentes, preferi primeiro entender profundamente um único negócio e construir algo que realmente resolvesse seus problemas.

Essa abordagem também permitiu que as decisões fossem tomadas com base em necessidades reais, e não apenas em funcionalidades que "talvez alguém use".

---

# Desafios encontrados

Durante o desenvolvimento apareceram diversos desafios técnicos e de negócio.

Entre eles:

* modelagem do banco;
* relacionamentos entre pratos e acompanhamentos;
* cardápio por dia;
* autenticação administrativa;
* upload e exibição de imagens;
* integração entre Angular e API;
* configuração de CORS;
* persistência com PostgreSQL;
* publicação do frontend;
* configuração do ambiente do backend;
* tratamento de diferentes regras do restaurante;
* disponibilidade de pratos;
* regras de funcionamento;
* cálculo de entrega.

Cada uma dessas etapas trouxe algum aprendizado novo.

E muitas vezes foi necessário voltar em uma decisão anterior e refatorar a solução depois de entender melhor a regra de negócio.

---

# Executando o projeto localmente

## Backend

Na raiz do projeto:

```bash
dotnet restore
```

Depois:

```bash
dotnet build
```

Entre no projeto da API:

```bash
cd src/QuentinhasDaTininha.Api
```

Execute:

```bash
dotnet run
```

Antes da execução, é necessário configurar corretamente as variáveis e credenciais utilizadas pela aplicação, incluindo:

* conexão PostgreSQL;
* configurações JWT;
* configurações do Supabase Storage.

Nunca publique credenciais reais diretamente no repositório.

---

## Frontend

Acesse:

```bash
cd frontend/quentinhas-da-tininha-web
```

Instale as dependências:

```bash
npm install
```

Execute:

```bash
npm start
```

ou:

```bash
ng serve
```

Por padrão, o Angular poderá ser acessado em:

```text
http://localhost:4200
```

---

# Próximas evoluções

O projeto continua sendo uma oportunidade de evolução.

Algumas melhorias que podem ser trabalhadas futuramente incluem:

* acompanhamento de status do pedido em tempo real;
* impressão automática de pedidos no restaurante;
* integração com impressora térmica;
* notificações de novos pedidos;
* melhoria de observabilidade e logs;
* criação de testes automatizados;
* dashboards de vendas;
* relatórios;
* histórico de pedidos;
* métricas de pratos mais vendidos;
* controle financeiro;
* melhorias na experiência mobile;
* expansão da infraestrutura de produção.

Também existe a possibilidade de, depois da validação completa dessa solução, estudar uma arquitetura que permita transformar a ideia em um produto utilizável por outros restaurantes.

Mas essa não foi a prioridade da primeira versão.

Primeiro, o objetivo foi resolver bem o problema para o qual o sistema nasceu.

---

# Sobre este projeto

O **Quentinhas da Tininha** representa muito bem o tipo de desenvolvimento que gosto de estudar e praticar:

**pegar um problema que existe fora do computador, entender suas regras e transformá-lo em software.**

Aqui pude trabalhar desde a modelagem do banco até o frontend, passando pela API, autenticação, arquitetura, regras de negócio, armazenamento de imagens e publicação.

Ainda existem melhorias a serem feitas — e provavelmente sempre existirão.

Mas esse também é um dos motivos pelos quais mantenho esse projeto no GitHub.

Ele não mostra apenas um código pronto.

Ele registra parte da minha evolução como desenvolvedor e a experiência de transformar uma necessidade real de um pequeno negócio em uma aplicação Full Stack.

---

## Autor

Projeto desenvolvido por **BsDevTa**.

Desenvolvido com C#, .NET, Angular e muita tentativa, teste, erro, ajuste e aprendizado.

Porque, no final, essa foi a parte mais importante do projeto.
