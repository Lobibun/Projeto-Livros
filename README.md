# LivroCDF - Sistema de Gestão de Livraria e Estoque

Sistema completo para gerenciamento de estoque, vendas e controle financeiro de livrarias. O projeto foca em regras de negócio complexas, controle de acesso hierárquico e auditoria de ações.

## 🚀 Funcionalidades Principais

### 📦 Gestão de Estoque e Produtos
* **Cadastro Inteligente:** Busca automática da capa e dados do livro via **API do Google Books** utilizando o ISBN.
* **Gestão de Exemplares:** Controle individual de exemplares por livro (rastreabilidade do estoque).

### 💰 Vendas e Financeiro
* **Controle de "Fiado" (Contas a Receber):**
    * Status de venda "A Pagar": O sistema registra o débito no perfil do cliente.
    * Monitoramento de tempo da dívida.
* **Relatórios:** Geração de relatórios de vendas efetivadas.
* **Logística Reversa:** Sistema de devolução com validação de prazo limite (ex: 30 dias).

### Segurança e Acesso (RBAC)
Sistema hierárquico de permissões (Role-Based Access Control):
1.  **Básico:** Gerenciamento operacional de livros e vendas.
2.  **Gerente:** Gestão de funcionários (Nível Básico) e relatórios.
3.  **CEO:** Acesso total, incluindo promoção de funcionários e auditoria.

### Auditoria e Logs
* **Rastreabilidade Total:** O sistema grava um log imutável de todas as ações críticas (vendas, exclusões, promoções de cargo), identificando **quem** fez e **quando**.

## 🛠️ Tecnologias Utilizadas
* **Linguagem:** C# (.NET)
* **Framework:** ASP.NET Core MVC
* **Banco de Dados:** MySQL (Server=localhost)
* **ORM:** Entity Framework Core (com Pomelo ou driver oficial)
* **Integrações:** Google Books API
* **Front-end:** HTML, CSS, Bootstrap (Razor Views)

## 🔧 Como Rodar o Projeto

1. Clone o repositório:
```bash
git clone [https://github.com/Lobibun/Projeto-Livros.git](https://github.com/Lobibun/Projeto-Livros.git)
