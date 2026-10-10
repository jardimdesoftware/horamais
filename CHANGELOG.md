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

- **Backend e frontend:** processos das imagens finais executados sem root,
  com migração das permissões do volume de chaves do backend
  ([#488](https://github.com/jardimdesoftware/horamais/issues/488)).

### Adições

- **Documentação:** histórico de mudanças e orientações para atualizá-lo a cada
  contribuição e release ([#482](https://github.com/jardimdesoftware/horamais/issues/482)).

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
