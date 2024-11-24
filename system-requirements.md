# Documento de Requisitos - Sistema de E-commerce Microsserviços

## 1. Propósito do Sistema
O sistema tem como objetivo gerenciar operações de e-commerce através de uma arquitetura de microsserviços, permitindo o controle de usuários, inventário e pedidos. A arquitetura distribuída visa garantir escalabilidade, manutenibilidade e isolamento de responsabilidades.

## 2. Usuários do Sistema

### 2.1 Usuários Comuns (USER)
- Podem realizar login
- Podem visualizar produtos
- Podem fazer pedidos

### 2.2 Administradores (ADMIN)
- Possuem todas as permissões dos usuários comuns
- Podem gerenciar o inventário (CRUD de produtos)
- Podem cadastrar outros administradores

## 3. Requisitos Funcionais

### 3.1 Gestão de Usuários
- RF01: O sistema deve permitir o registro de novos usuários
- RF02: O sistema deve permitir a autenticação de usuários via login
- RF03: O sistema deve permitir que administradores registrem novos administradores
- RF04: O sistema deve gerar tokens JWT para autenticação

### 3.2 Gestão de Inventário
- RF05: O sistema deve permitir o cadastro de produtos (ADMIN)
- RF06: O sistema deve permitir a atualização de produtos (ADMIN)
- RF07: O sistema deve permitir a exclusão de produtos (ADMIN)
- RF08: O sistema deve permitir a listagem de produtos
- RF09: O sistema deve permitir a consulta de produtos individuais
- RF10: O sistema deve controlar o estoque dos produtos

### 3.3 Gestão de Pedidos
- RF11: O sistema deve permitir a criação de pedidos
- RF12: O sistema deve validar a disponibilidade de estoque
- RF13: O sistema deve calcular o valor total do pedido
- RF14: O sistema deve atualizar automaticamente o estoque após confirmação do pedido

## 4. Descrição Técnica dos Microsserviços

### 4.1 Microsserviço de Usuários (User Microservice)
**Função**: Gerenciar usuários e autenticação
- Endpoints:
  - POST /users/register: Registro de novos usuários
  - POST /users/login: Autenticação de usuários
  - POST /users/register/admin: Registro de administradores
  - GET /users/{id}: Consulta de usuário

**Tecnologias**:
- .NET 8
- Entity Framework Core
- SQL Server
- JWT Authentication

### 4.2 Microsserviço de Inventário (Inventory Microservice)
**Função**: Gerenciar produtos e estoque
- Endpoints:
  - GET /inventory/products: Listar produtos
  - GET /inventory/products/{id}: Consultar produto
  - POST /inventory/products: Criar produto
  - PUT /inventory/products/{id}: Atualizar produto
  - DELETE /inventory/products/{id}: Deletar produto
  - PUT /inventory/products/{id}/quantity: Atualizar quantidade

**Tecnologias**:
- .NET 8
- Entity Framework Core
- SQL Server
- JWT Authentication

### 4.3 Microsserviço de Pedidos (Order Microservice)
**Função**: Gerenciar pedidos e integração com outros serviços
- Endpoints:
  - POST /order-requests: Criar pedido

**Tecnologias**:
- .NET 8
- Entity Framework Core
- SQL Server
- HTTP Client para comunicação entre serviços
- JWT Authentication

## 5. Comunicação entre Microsserviços

### 5.1 Fluxo de Pedido
1. Cliente autenticado envia pedido ao Order Microservice
2. Order Microservice valida usuário com User Microservice
3. Order Microservice consulta produto no Inventory Microservice
4. Order Microservice solicita atualização de estoque ao Inventory Microservice
5. Order Microservice salva o pedido e retorna confirmação

### 5.2 Segurança
- Comunicação via HTTP/HTTPS
- Autenticação via JWT Tokens
- Validação de roles para operações administrativas
- Tratamento de erros e indisponibilidade de serviços

### 5.3 Persistência de Dados
- Cada microsserviço possui seu próprio banco de dados
- Uso de Entity Framework Core para ORM
- SQL Server como SGBD
- Migrations para versionamento de banco de dados

## 6. Considerações Técnicas Adicionais

### 6.1 Escalabilidade
- Serviços independentes podem ser escalados separadamente
- Stateless design permite múltiplas instâncias
- Conexões de banco isoladas por serviço

### 6.2 Manutenibilidade
- Separação clara de responsabilidades
- Código organizado por domínios
- Uso de padrões de projeto (Repository, DTO)
- Documentação via Swagger/OpenAPI

### 6.3 Resiliência
- Tratamento de falhas de comunicação entre serviços
- Circuit breakers para prevenir cascata de falhas
- Logs para rastreamento de problemas
