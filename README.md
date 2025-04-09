# 💉 Dose em Dia - Full Stack (Frontend + Backend + Banco de Dados)

Projeto universitário que integra:

- 🧠 **Backend**: ASP.NET (C#)
- 🎨 **Frontend**: Vue.js 3
- 🗄️ **Banco de dados**: SQL Server
- 🐳 **Orquestração**: Docker + Docker Compose

---

## 🚀 Requisitos

Antes de começar, você precisa ter:

- [Docker Desktop](https://www.docker.com/products/docker-desktop) instalado e em execução
- (Opcional) Git instalado para clonar o projeto

---

## 📦 Clonando o repositório

```bash
git clone https://github.com/PedroAFG/dose_em_dia.git
cd dose_em_dia
```

---

## 🖥️ Como rodar o projeto com Docker

1. Após clonar o repositório, abra o terminal na raiz do projeto. Exemplo no Windows:

```bash
cd "C:\Users\SeuUsuario\Documents\programação\dose_em_dia"
```

2. Em seguida, execute:

```bash
docker-compose up --build
```

> O processo pode levar alguns minutos na primeira vez.

---

## 🌐 Acessando os serviços

- 🔹 **Frontend (Vue)**: http://localhost:8080  
- 🔹 **Backend (API)**: http://localhost:5054/api/usuario  
- 🔹 **Banco de Dados (SQL Server)**: `localhost:1433`  
  - **Usuário**: `sa`  
  - **Senha**: `Strong!Pass123`

---

## 🧪 Testes com Postman

Você pode testar os endpoints da API com o [Postman](https://www.postman.com/).

- Acesse nossa coleção de testes:  
  [🔗 Coleção no Postman](https://postman.com/YOUR-COLLECTION-LINK)

---

## 🤝 Colaboradores

- 👨‍💻 Pedro Antonio – Frontend e integração
- 👩‍💻 Maria Alice – Backend e banco de dados
- 👩‍💻 Ana Paula – Produto, documentação e estratégia

---

## 🛑 Encerrando os containers

Quando quiser parar os serviços:

```bash
docker-compose down
```

---

## 🧼 Limpando imagens e volumes (opcional)

```bash
docker system prune -a
docker volume prune
```