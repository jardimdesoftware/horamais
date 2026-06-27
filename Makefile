# Convenience targets for validating the frontend build and provisioning the stack

FRONT_DIR ?= front
DOCKER_COMPOSE ?= docker compose
DOCKER_COMPOSE_FILE ?= docker-compose.dev.yml
PROD_DOCKER_COMPOSE_FILE ?= docker-compose.yml
LOG_DIR ?= .logs
LOG_DIR_ABS := $(abspath $(LOG_DIR))
FRONT_BUILD_LOG ?= $(LOG_DIR)/frontend-build.log
ENV_FILE ?= .env

.PHONY: frontend-build check up docker-up docker-down prod-pull prod-up prod-down clean-logs db-up wait-db backend-seed

check: frontend-build

frontend-build: | $(LOG_DIR)
	powershell.exe -NoLogo -NoProfile -Command "Push-Location '$(FRONT_DIR)'; try { npm run build 2>&1 | Tee-Object -FilePath '$(LOG_DIR_ABS)/frontend-build.log'; $$exit = $$LASTEXITCODE; if ($$exit -eq 0 -and -not (Test-Path '.next/BUILD_ID')) { 'Error: frontend build did not produce .next/BUILD_ID.' | Tee-Object -FilePath '$(LOG_DIR_ABS)/frontend-build.log' -Append; $$exit = 1 } } finally { Pop-Location }; exit $$exit"

$(LOG_DIR):
	powershell.exe -NoLogo -NoProfile -Command "New-Item -ItemType Directory -Force -Path '$(LOG_DIR_ABS)' | Out-Null"

db-up:
	$(DOCKER_COMPOSE) -f $(DOCKER_COMPOSE_FILE) up -d db

wait-db:
	powershell.exe -NoLogo -NoProfile -Command "& { $$envFile = '$(ENV_FILE)'; $$vars = @{}; if (Test-Path $$envFile) { Get-Content $$envFile | Where-Object { $$_.Trim().StartsWith('POSTGRES_USER=') -or $$_.Trim().StartsWith('POSTGRES_DB=') } | ForEach-Object { $$parts = $$_.Split('=',2); if ($$parts.Count -eq 2) { $$vars[$$parts[0]] = $$parts[1] } } } $$user = $$vars['POSTGRES_USER']; if ([string]::IsNullOrWhiteSpace($$user)) { $$user = 'postgres' } $$db = $$vars['POSTGRES_DB']; if ([string]::IsNullOrWhiteSpace($$db)) { $$db = 'postgres' } Write-Host 'Waiting for database...'; do { try { $(DOCKER_COMPOSE) -f $(DOCKER_COMPOSE_FILE) exec -T db pg_isready -U $$user -d $$db | Out-Null; $$status = $$LASTEXITCODE } catch { $$status = 1 } if ($$status -ne 0) { Start-Sleep -Seconds 2 } } while ($$status -ne 0); Write-Host 'Database ready.' }"

backend-seed:
	$(DOCKER_COMPOSE) -f $(DOCKER_COMPOSE_FILE) run --rm backend dotnet Back.API.dll --seed

docker-up:
	$(DOCKER_COMPOSE) -f $(DOCKER_COMPOSE_FILE) up -d

prod-pull:
	$(DOCKER_COMPOSE) -f $(PROD_DOCKER_COMPOSE_FILE) pull

prod-up:
	$(DOCKER_COMPOSE) -f $(PROD_DOCKER_COMPOSE_FILE) up -d --pull always

up:
	$(MAKE) frontend-build
	$(MAKE) db-up
	$(MAKE) wait-db
	$(MAKE) backend-seed
	$(MAKE) docker-up

docker-down:
	$(DOCKER_COMPOSE) -f $(DOCKER_COMPOSE_FILE) down

prod-down:
	$(DOCKER_COMPOSE) -f $(PROD_DOCKER_COMPOSE_FILE) down

clean-logs:
	powershell.exe -NoLogo -NoProfile -Command "If (Test-Path '$(LOG_DIR_ABS)') { Remove-Item '$(LOG_DIR_ABS)' -Recurse -Force }"
