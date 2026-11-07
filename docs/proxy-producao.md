# Proxy de produção do HoraMais

O registro A de `horamais.me` aponta para o IPv4 público da VM. O Nginx do host
recebe HTTP e HTTPS. O frontend encaminha `/api/` pela rede do Compose, e o
Nginx encaminha `/hubs/` diretamente ao backend para permitir WebSocket.

## Portas e configuração

O frontend usa `PORT_MACHINE_FRONTEND=127.0.0.1:3000`. O backend usa
`PORT_MACHINE_BACKEND=5000` com bind exclusivo em `127.0.0.1`. PostgreSQL e
Garage ficam na rede privada do Compose. No firewall da VM e na rede Oracle,
mantenha o ingresso web nas portas 80 e 443 e restrinja SSH às origens autorizadas.

Configure as URLs públicas no `.env` da VM e registre
`https://horamais.me/api/auth/callback/google` no cliente OAuth do Google.

```dotenv
NEXTAUTH_URL=https://horamais.me
NEXT_PUBLIC_API_URL=
CORS_ALLOWED_ORIGIN=https://horamais.me
```

Em uma instalação nova, coloque o `map` no contexto `http`, por exemplo em
`/etc/nginx/conf.d/horamais-websocket.conf`. Aplique as duas rotas no bloco do
site `horamais.me` em `/etc/nginx/sites-available/`. Ajuste as portas do exemplo
se os valores do `.env` forem diferentes.

```nginx
map $http_upgrade $horamais_connection_upgrade {
    default upgrade;
    ''      close;
}
```

```nginx
client_max_body_size 20m;

location /hubs/ {
    proxy_pass http://127.0.0.1:5000;
    proxy_http_version 1.1;
    proxy_set_header Host $host;
    proxy_set_header X-Forwarded-Proto $scheme;
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    proxy_set_header Upgrade $http_upgrade;
    proxy_set_header Connection $horamais_connection_upgrade;
    proxy_read_timeout 120s;
}

location / {
    proxy_pass http://127.0.0.1:3000;
    proxy_set_header Host $host;
    proxy_set_header X-Forwarded-Proto $scheme;
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
}
```

Os cabeçalhos `Upgrade` e `Connection` precisam ser encaminhados explicitamente
para o WebSocket. [Documentação do Nginx](https://nginx.org/en/docs/http/websocket.html)

## Certificado, verificação e recuperação

Para a primeira emissão, habilite o site HTTP com `server_name horamais.me`,
valide com `sudo nginx -t` e recarregue o Nginx. Use
`sudo certbot --nginx --redirect -d horamais.me` para emitir o certificado e
configurar o redirecionamento. Confira o agendamento do Certbot e use
`sudo certbot renew --dry-run` para testar a renovação. Preserve a configuração
do site e `/etc/letsencrypt` no backup da VM.
[Documentação do Certbot](https://eff-certbot.readthedocs.io/en/stable/using.html#renewing-certificates)

Após uma alteração, confira o redirecionamento HTTP, a sessão e o login Google
com uma conta de teste autorizada. Envie e baixe um certificado de teste e
confirme a atualização em tempo real em uma segunda sessão. Para isolar o
caminho dos hubs, compare os retornos local e público da negociação SignalR.

```sh
curl -fsS -X POST 'http://127.0.0.1:5000/hubs/turma/negotiate?negotiateVersion=1'
curl -fsS -X POST 'https://horamais.me/hubs/turma/negotiate?negotiateVersion=1'
curl -fsS -X POST 'https://horamais.me/hubs/certificado/negotiate?negotiateVersion=1'
```

Se o retorno local funcionar e o público falhar, confira o destino da rota e
os logs do Nginx. Para reverter uma alteração do proxy, restaure a cópia da
configuração anterior, execute `sudo nginx -t` e recarregue o serviço somente
após a validação. A atualização e o rollback dos containers seguem o
[procedimento de deploy na VM](deploy-automatico.md).
