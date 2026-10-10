# Contribuindo com o HoraMais

Descreva no pull request o problema resolvido, o comportamento resultante e como
a mudança foi validada. Use o template do repositório e selecione exatamente um
tipo de mudança, pois os workflows usam esse campo no cálculo da versão.

## Atualização do CHANGELOG

1. No mesmo PR da mudança, adicione uma entrada em **Não lançado** no
   [CHANGELOG.md](CHANGELOG.md). Descreva o efeito para usuários ou operadores,
   identifique o componente (Frontend, Backend, Infraestrutura ou Documentação)
   e inclua um link para a issue ou PR quando disponível.
2. Agrupe as entradas em **Adições**, **Alterações**, **Correções** ou
   **Segurança**. Crie somente as categorias necessárias. Não publique segredos
   nem detalhes de exploração de vulnerabilidades ainda não corrigidas.
3. Ajustes internos sem impacto relevante podem dispensar uma entrada; nesse
   caso, explique a dispensa no PR. Revise o checklist antes de solicitar revisão.
4. Após a publicação bem-sucedida de uma release, abra um PR de documentação
   movendo somente as entradas efetivamente entregues para uma seção com a tag
   exata, o link da release e sua data de publicação em America/Sao_Paulo.
   Preserve **Não lançado** para as mudanças pendentes; se ficar vazia, registre
   “Nenhuma mudança pendente registrada.” e remova essa frase na próxima entrada.
5. Confira as notas automáticas da release e os PRs incluídos. Mantenha o resumo
   do CHANGELOG coerente com essas fontes, sem atribuir mudanças a versões em
   que elas ainda não foram entregues. Ordene as releases por data de publicação,
   da mais recente para a mais antiga.

### Frontend e backend

Os workflows `production-front.yml` e `production-back.yml` publicam imagens
separadas e usam as releases do repositório para calcular tags. Preserve a
numeração existente, inclusive tags históricas com prefixo `v`. Não deduza o
componente ou uma versão conjunta apenas pelo número da tag.

Para cada release, verifique o workflow que a publicou e a imagem correspondente.
Se duas releases registrarem a mesma mudança, deixe explícita a relação e
identifique os componentes comprovados pelas execuções. Quando uma release
contiver mudanças de ambos os componentes, use uma seção com itens identificados
por componente. O link da seção deve apontar para
`https://github.com/jardimdesoftware/horamais/releases/tag/<tag>`.

As notas de release são geradas automaticamente pelos workflows atuais; o
CHANGELOG é revisado e atualizado por PR. Alterar apenas a documentação não
dispara os workflows de imagem com os filtros de caminhos atuais. Não execute
uma publicação de imagem apenas para atualizar o histórico.
