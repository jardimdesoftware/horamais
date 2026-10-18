# Changelog

Histórico das mudanças relevantes do HoraMais. As datas usam o fuso
America/Sao_Paulo. As tags são preservadas como publicadas no GitHub; frontend
e backend têm workflows próprios, portanto uma release não garante que ambos
os componentes tenham recebido uma nova imagem. Cada item identifica seu escopo.

Este histórico inicial resume as cinco releases mais recentes na data de sua
criação. Consulte as [releases anteriores](https://github.com/jardimdesoftware/horamais/releases)
para o histórico completo. O procedimento de atualização está em
[CONTRIBUTING.md](CONTRIBUTING.md#atualização-do-changelog).

## Não lançado

### Segurança

- **Backend:** `System.Security.Cryptography.Xml` atualizado para 10.0.13,
  corrigindo os achados altos do Trivy durante a validação da administração
  de usuários ([#504](https://github.com/jardimdesoftware/horamais/issues/504)).
- **CI/CD:** gates de SAST e DAST antes da publicação e bloqueio de
  vulnerabilidades HIGH/CRITICAL pelo Trivy em AMD64 e ARM64, incluindo achados
  sem correção disponível ([#481](https://github.com/jardimdesoftware/horamais/issues/481)).
- **Backend:** rota legada de cadastro de aluno por senha removida, eliminando
  o aviso de antiforgery sem exceção no SAST
  ([#491](https://github.com/jardimdesoftware/horamais/issues/491)).
- **Frontend:** dependências `sharp` e `source-map-js` atualizadas e npm removido
  da imagem de produção para tratar os seis achados altos do Trivy
  ([#490](https://github.com/jardimdesoftware/horamais/issues/490)).
- **Backend:** runtime migrado da imagem Debian para a variante oficial Alpine
  com ICU e dados de fuso horário para eliminar os achados altos/críticos da
  imagem-base ([#489](https://github.com/jardimdesoftware/horamais/issues/489)).
- **Backend e frontend:** processos das imagens finais executados sem root,
  com migração das permissões do volume de chaves do backend
  ([#488](https://github.com/jardimdesoftware/horamais/issues/488)).

### Adições

- **Backend e frontend:** administração de contas e atribuição direta de
  coordenadores, inclusive antes do primeiro acesso com Google
  ([#504](https://github.com/jardimdesoftware/horamais/issues/504)).
- **Deploy:** orquestração automática da stack na VM após aprovação de ambos
  os workflows de produção, com imagens fixadas por digest, verificação de
  saúde e rollback em falha; a VM consulta a `main` por um timer
  ([#483](https://github.com/jardimdesoftware/horamais/issues/483)).
- **Armazenamento:** Garage substitui MinIO nos ambientes de produção e
  desenvolvimento, com bucket persistente e credencial própria da aplicação.
  Os objetos antigos do MinIO são descartados no corte
  ([#479](https://github.com/jardimdesoftware/horamais/issues/479)).

- **Documentação:** histórico de mudanças e orientações para atualizá-lo a cada
  contribuição e release ([#482](https://github.com/jardimdesoftware/horamais/issues/482)).

### Alterações

- **Backend e frontend:** entrada somente com Google institucional; formulários
  e endpoints legados de senha e convite deixam de atender no ambiente normal
  ([#504](https://github.com/jardimdesoftware/horamais/issues/504)).

## [1.0.7](https://github.com/jardimdesoftware/horamais/releases/tag/1.0.7) — 2026-10-06

### Segurança

- **Aplicação e infraestrutura:** restrição de acesso a recursos e remoção de
  credenciais de seed ([#476](https://github.com/jardimdesoftware/horamais/pull/476)).

## [1.0.6](https://github.com/jardimdesoftware/horamais/releases/tag/1.0.6) — 2026-10-06

### Adições

- **Frontend:** primeiro acesso com Google sem senha local
  ([#473](https://github.com/jardimdesoftware/horamais/pull/473)).
- **Backend:** cadastro com Google sem senha local
  ([#472](https://github.com/jardimdesoftware/horamais/pull/472)).

## [1.0.5](https://github.com/jardimdesoftware/horamais/releases/tag/1.0.5) — 2026-09-30

### Alterações

- **Frontend:** atualização de listr2, react-is-19, @react-pdf/pdfkit e
  get-east-asian-width
  ([#467](https://github.com/jardimdesoftware/horamais/pull/467),
  [#466](https://github.com/jardimdesoftware/horamais/pull/466),
  [#465](https://github.com/jardimdesoftware/horamais/pull/465),
  [#464](https://github.com/jardimdesoftware/horamais/pull/464)).
- **Backend:** atualização do grupo de dependências da camada Application
  ([#461](https://github.com/jardimdesoftware/horamais/pull/461)).

## [1.0.4](https://github.com/jardimdesoftware/horamais/releases/tag/1.0.4) — 2026-09-29

### Adições

- **Infraestrutura (frontend e backend):** publicação de imagens Docker para
  AMD64 e ARM64 ([#460](https://github.com/jardimdesoftware/horamais/pull/460)).

## [1.0.3](https://github.com/jardimdesoftware/horamais/releases/tag/1.0.3) — 2026-09-29

### Segurança

- **Frontend e backend:** correção de vulnerabilidades nas dependências npm e
  NuGet ([#459](https://github.com/jardimdesoftware/horamais/pull/459)).
