![License](https://img.shields.io/github/license/ifpebj-ti/horas-discentes)
![Last Commit](https://img.shields.io/github/last-commit/ifpebj-ti/horas-discentes)
![Top Languages](https://img.shields.io/github/languages/top/ifpebj-ti/horas-discentes)
![Repo Size](https://img.shields.io/github/repo-size/ifpebj-ti/horas-discentes)
![Contributors](https://img.shields.io/github/contributors/ifpebj-ti/horas-discentes)
![Open Issues](https://img.shields.io/github/issues/ifpebj-ti/horas-discentes)
[![Open PRs](https://img.shields.io/github/issues-pr/ifpebj-ti/horas-discentes)](https://github.com/ifpebj-ti/horas-discentes/pulls)
![Forks](https://img.shields.io/github/forks/ifpebj-ti/horas-discentes)
![Stars](https://img.shields.io/github/stars/ifpebj-ti/horas-discentes)
[![Vulnerabilidades](./badges/vulnerabilidades.svg)](https://github.com/ifpebj-ti/horas-discentes/security/dependabot)
[![CI/CD Back-End](https://img.shields.io/github/actions/workflow/status/ifpebj-ti/horas-discentes/production-back.yml?branch=main&label=CI%2FCD%20Back-End)](https://github.com/ifpebj-ti/horas-discentes/actions/workflows/production-back.yml)
[![CI/CD Front-End](https://img.shields.io/github/actions/workflow/status/ifpebj-ti/horas-discentes/production-front.yml?branch=main&label=CI%2FCD%20Front-End)](https://github.com/ifpebj-ti/horas-discentes/actions/workflows/production-front.yml)


# 📘 Horas Discentes

---

Sistema de controle e registro de **horas complementares** para estudantes do curso de Engenharia de Software do IFPE - Campus Belo Jardim.

Esta aplicação permitirá que os estudantes cadastrem suas atividades extracurriculares, façam upload de certificados e acompanhem a contabilização de suas horas por meio de uma interface simples, acessível e responsiva (mobile e desktop).

---

## 🚀 Funcionalidades Previstas

- 📥 Envio de certificados e comprovantes  
- 👨‍🎓 Cadastro e login de estudantes  
- 🧮 Cálculo automático das horas acumuladas  
- 🗂️ Validação de documentos  
- 📊 Painel com progresso individual  
- 📱 Interface responsiva (mobile e desktop)  

---

## 🛠️ Tecnologias Utilizadas

### Frontend  
![Next.js](https://img.shields.io/badge/Next.js-000?style=for-the-badge&logo=next.js&logoColor=white)
![TypeScript](https://img.shields.io/badge/TypeScript-3178C6?style=for-the-badge&logo=typescript&logoColor=white)

### Backend  
![.NET 8](https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)

### Banco de Dados  
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-316192?style=for-the-badge&logo=postgresql&logoColor=white)

### Outros  
![Git](https://img.shields.io/badge/Git-F05032?style=for-the-badge&logo=git&logoColor=white)
![GitHub](https://img.shields.io/badge/GitHub-181717?style=for-the-badge&logo=github&logoColor=white)
![GitHub Actions](https://img.shields.io/badge/GitHub_Actions-2088FF?style=for-the-badge&logo=github-actions&logoColor=white)
![Figma](https://img.shields.io/badge/Figma-F24E1E?style=for-the-badge&logo=figma&logoColor=white)
![Canva](https://img.shields.io/badge/Canva-00C4CC?style=for-the-badge&logo=canva&logoColor=white)

---

## 👥 Equipe do Projeto

| Nome              | Função             | GitHub |
|-------------------|--------------------|--------|
| Ingrid Santos     | Infra / DevSecOps  | [@ingriidssantoss](https://github.com/ingriidssantoss) |
| Erimilson Silva   | Dev Front-End      | [@Erysilva98](https://github.com/Erysilva98) |
| Erison Cavalcante | Dev Back-End       | [@erison7596](https://github.com/erison7596) |
| Victoria Tiburcio | UX Designer        | [@Mavi-Tiburcio](https://github.com/mavitiburcio) |

---

## 📚 Documentação

Consulte o [CHANGELOG](CHANGELOG.md) para o histórico de mudanças e o
[guia de contribuição](CONTRIBUTING.md) para atualizar o histórico a cada release.

### Provisionamento e remediação de contas

Defina `ADMIN_EMAIL` e uma `ADMIN_PASSWORD` única e forte antes do primeiro boot.
O administrador usa o endpoint de convite para cadastrar coordenadores; o
convite só vale para o e-mail destinatário, expira em duas horas e é consumido
uma vez. Alunos entram com uma conta Google institucional e não precisam de
senha local. O seed de desenvolvimento cria curso e turmas, mas não cria mais
usuários com senha pública.

Ao atualizar uma instalação antiga, o boot remove automaticamente a senha
conhecida das seis contas de demonstração que ainda a utilizem. Os dados
dessas contas permanecem; cadastre coordenadores reais por convite e remova
os usuários de demonstração que não forem necessários. Revise também as
credenciais do administrador e de qualquer coordenador provisionado por
variáveis de ambiente: mudar a variável após o primeiro boot não altera a
senha já gravada no Identity. Faça a troca pelo fluxo de recuperação ou por
um procedimento administrativo controlado. A atualização invalida JWTs
anteriores, exigindo novo login.

Em produção, o Compose publica apenas o frontend. Backend, PostgreSQL e
MinIO ficam na rede interna. No Compose de desenvolvimento, as portas são
vinculadas a `127.0.0.1` para permitir acesso local sem exposição na rede.

### Imagens Docker AMD64 e ARM64

Os workflows de frontend e backend geram imagens para `linux/amd64` e
`linux/arm64` em runners nativos. Cada imagem é verificada pelo Trivy antes de
ser enviada ao GitHub Container Registry (GHCR). Após as duas arquiteturas
passarem, o pipeline publica um manifest com a tag da versão e atualiza `latest`.
O Docker seleciona automaticamente a arquitetura da máquina ao baixar a imagem.

- Frontend: `ghcr.io/jardimdesoftware/horas-discentes-frontend:<versão>`
- Backend: `ghcr.io/jardimdesoftware/horas-discentes-backend:<versão>`

Para conferir as plataformas de uma versão publicada:

```sh
docker buildx imagetools inspect ghcr.io/jardimdesoftware/horas-discentes-frontend:latest
docker buildx imagetools inspect ghcr.io/jardimdesoftware/horas-discentes-backend:latest
```

As tags `build-<run_id>-<tentativa>-amd64` e `build-<run_id>-<tentativa>-arm64`
identificam as imagens intermediárias verificadas de cada execução. Use as tags
de versão ou `latest` no deploy. ARM de 32 bits não está incluído.

PRs executam os builds e scans das duas arquiteturas sem publicar imagens ou
releases. A publicação acontece nos eventos de produção já configurados
(`push` na `main`, PR integrado ou execução manual).

A documentação do projeto está disponível na nossa [📖 Wiki](https://github.com/ifpebj-ti/horas-discentes/wiki), contendo:

- 📌 Visão Geral do Projeto  
- 🧠 Relatórios de Sprint  
- 🔐 Contexto de Segurança  
- 🛠️ Estrutura de Infra  
- 🧱 Requisitos e Arquitetura  
- 🎨 UX / Protótipos  

---

## 📄 Licença

Este é um projeto acadêmico, desenvolvido sem fins lucrativos para a disciplina de Engenharia de Software no IFPE - Campus Belo Jardim.
