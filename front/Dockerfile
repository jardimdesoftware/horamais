# npm atualizado inclui a correção de segurança do node-tar.
FROM node:22-alpine AS base
RUN npm install --global npm@11.19.1 --ignore-scripts --no-audit --no-fund \
    && npm cache clean --force

# Etapa de build
FROM base AS builder
WORKDIR /app

# Versionamento
ARG VERSION=dev
ARG REVISION=local
ARG BUILDTIME=dev

# Variáveis públicas (entram no bundle do cliente)
ARG NEXT_PUBLIC_API_URL
ARG NEXT_PUBLIC_VERSION
ARG NEXT_PUBLIC_REVISION
ARG NEXT_PUBLIC_BUILDTIME

# Exporta para o ambiente do build antes do build do Next
ENV NEXT_PUBLIC_API_URL=$NEXT_PUBLIC_API_URL
ENV NEXT_PUBLIC_VERSION=${NEXT_PUBLIC_VERSION:-$VERSION}
ENV NEXT_PUBLIC_REVISION=${NEXT_PUBLIC_REVISION:-$REVISION}
ENV NEXT_PUBLIC_BUILDTIME=${NEXT_PUBLIC_BUILDTIME:-$BUILDTIME}

# Dependências e build
COPY package*.json ./
RUN npm ci --legacy-peer-deps
COPY . .
RUN npm run build

# Etapa de produção
FROM base AS production
WORKDIR /app

# Atualiza pacotes do sistema para corrigir CVEs (ex: zlib CVE-2026-22184)
RUN apk upgrade --no-cache

ENV NODE_ENV=production

# Reexporta info de versionamento (opcional; bundle já contém os valores)
ARG VERSION=dev
ARG REVISION=local
ARG BUILDTIME=dev
ENV NEXT_PUBLIC_VERSION=$VERSION
ENV NEXT_PUBLIC_REVISION=$REVISION
ENV NEXT_PUBLIC_BUILDTIME=$BUILDTIME

# Copia artefatos
COPY --from=builder /app/.next ./.next
COPY --from=builder /app/public ./public
COPY --from=builder /app/package*.json ./
COPY --from=builder /app/node_modules ./node_modules


EXPOSE 3000
CMD ["npm", "start"]
