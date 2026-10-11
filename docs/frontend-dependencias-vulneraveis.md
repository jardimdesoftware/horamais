# Tratamento dos achados da imagem do frontend (#490)

O Trivy 0.70.0 encontrou seis ocorrências HIGH **por arquitetura** na imagem
anterior. AMD64 e ARM64 repetiam os mesmos seis identificadores; os relatórios
originais estão nos artefatos da
[execução 38072243659](https://github.com/jardimdesoftware/horamais/actions/runs/38072243659).

| Identificador | Pacote e versão anterior | Origem na imagem | Tratamento |
| --- | --- | --- | --- |
| CVE-2026-102276 | brace-expansion 5.0.9 | npm global | Remover npm da imagem de produção após a instalação |
| CVE-2026-102278 | brace-expansion 5.0.9 | npm global | Remover npm da imagem de produção após a instalação |
| CVE-2026-19534 | undici 6.28.0 | npm global | Remover npm da imagem de produção após a instalação |
| CVE-2026-93748 | http-cache-semantics 4.2.0 | npm global | Remover npm da imagem de produção após a instalação |
| GHSA-wq5f-xc86-pv6w | sharp 0.35.4 | dependência do Next em `/app/node_modules` | Atualizar o lockfile para 0.35.5 |
| CVE-2026-93749 | source-map-js 1.2.1 | dependência de PostCSS/Tailwind em `/app/node_modules` | Atualizar o lockfile para 1.2.2 |

Os quatro primeiros estavam em
`/usr/local/lib/node_modules/npm/node_modules`, fora da árvore de dependências
da aplicação. O npm é necessário para instalar e compilar o frontend, mas não
para executar o Next.js. Por isso, a imagem de produção remove seu diretório e
seus executáveis após `npm ci` e inicia o Next diretamente com `node`. O
estágio de build conserva o npm atualizado, inclusive a correção anterior do
`node-tar`.

O relatório antigo não indicava `FixedVersion` para CVE-2026-93748. Isso não
o torna falso positivo: o pacote afetado existia na imagem final anterior. A
remoção do npm elimina esse pacote da imagem final; não depende da publicação
de uma versão corrigida. O npm da imagem-base pode mudar em rebuilds, mas o
gate Trivy continua obrigatório para a imagem construída. Nenhum achado é
suprimido.

Os PRs de Dependabot [#485](https://github.com/jardimdesoftware/horamais/pull/485)
e [#486](https://github.com/jardimdesoftware/horamais/pull/486) propõem,
respectivamente, as mesmas atualizações de `source-map-js` e `sharp` no
lockfile. Esta issue reúne ambas com a remoção do npm global e verifica o
resultado na imagem final, em AMD64 e ARM64.

O workflow do frontend usa Trivy 0.70.0 com `--scanners vuln`, severidade
HIGH/CRITICAL, sem ignorar achados sem correção. Ele falha antes da publicação
e preserva o relatório JSON por arquitetura. Também verifica que a imagem
inicia sem npm e responde por HTTP. O DAST completo será revalidado quando a
branch de gates de segurança da #481 for integrada.
