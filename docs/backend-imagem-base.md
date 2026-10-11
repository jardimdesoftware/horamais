# Tratamento dos achados da imagem-base do backend (#489)

O Trivy 0.70.0 encontrou 52 ocorrências HIGH e uma CRITICAL **por arquitetura**
na imagem anterior do backend, baseada em Debian 12.15. O relatório está nos
artefatos AMD64 e ARM64 da
[execução 38072243642](https://github.com/jardimdesoftware/horamais/actions/runs/38072243642).
Os 53 registros correspondem a 14 identificadores distintos, pois um mesmo
CVE pode afetar vários pacotes binários da mesma origem.

| Identificador | Pacotes Debian apontados | Tratamento |
| --- | --- | --- |
| CVE-2023-45853 (CRITICAL) | zlib1g | Substituição da base Debian |
| CVE-2025-69720 | ncurses-base, ncurses-bin, libtinfo6 | Substituição da base Debian |
| CVE-2026-16742 | libsystemd0, libudev1 | Substituição da base Debian |
| CVE-2026-41992 | gzip | Substituição da base Debian |
| CVE-2026-53613 | família util-linux | Substituição da base Debian |
| CVE-2026-54369 | libacl1 | Substituição da base Debian |
| CVE-2026-76642 | família util-linux | Substituição da base Debian |
| CVE-2026-78408 | família util-linux | Substituição da base Debian |
| CVE-2026-78409 | família util-linux | Substituição da base Debian |
| CVE-2026-78410 | família util-linux | Substituição da base Debian |
| CVE-2026-84782 | openssl, libssl3 | Substituição da base Debian |
| CVE-2026-88647 | libgnutls30 | Substituição da base Debian |
| CVE-2026-88648 | libgnutls30 | Substituição da base Debian |
| CVE-2026-9538 | perl-base | Substituição da base Debian |

Os relatórios Debian não indicavam uma versão corrigida para esses pacotes.
Isso não significa que os CVEs foram corrigidos no Debian: a nova imagem usa
outra distribuição, com inventário e advisories próprios. Os pacotes Debian
da tabela deixam de integrar a imagem final, e a **imagem construída** deve
passar pelo scan completo antes da publicação.

O runtime escolhido é a variante oficial
[`mcr.microsoft.com/dotnet/aspnet:8.0-alpine-extra`](https://github.com/dotnet/dotnet-docker/blob/main/README.aspnet.md),
que inclui ICU e dados de fuso horário. O estágio de build usa o SDK Alpine,
para manter o mesmo ambiente libc/musl. O usuário `app` e os diretórios
graváveis da #488 são mantidos; o caminho do volume de chaves não muda nesta
issue. A tag flutua para receber patches .NET 8 e Alpine; cada nova imagem
continua sujeita ao gate HIGH/CRITICAL, inclusive para achados sem correção.

## Comparação reproduzível

Em 10/10/2026, o scan remoto da base com Trivy 0.70.0 (`--scanners vuln
--severity HIGH,CRITICAL`, sem exclusões) retornou:

| Plataforma | Sistema detectado | HIGH | CRITICAL | Image ID |
| --- | --- | ---: | ---: | --- |
| AMD64 | Alpine 3.24.2 / .NET 8.0.31 | 0 | 0 | `sha256:c9703b1c513b859c8f7b7adf874765b73d3bd65809d0b53fd8ed786c0853e157` |
| ARM64 | Alpine 3.24.2 / .NET 8.0.31 | 0 | 0 | `sha256:b87a6c0cc16892a99b96a97f4d04192f916b2a4621a635482b509ce7dcc76dc9` |

O digest de referência multiarquitetura observado foi
`mcr.microsoft.com/dotnet/aspnet@sha256:2c3ffd0c71e7ae90c46f684dfa6a517d59382c521f84c8231b616235fac761d8`.
Esses números descrevem **a base no momento do scan**, não substituem o
relatório da imagem final. O workflow do backend preserva o JSON do Trivy
por arquitetura e falha se a imagem final contiver HIGH ou CRITICAL.
