const HOP_BY_HOP_HEADERS = new Set([
  'connection',
  'keep-alive',
  'proxy-authenticate',
  'proxy-authorization',
  'te',
  'trailer',
  'transfer-encoding',
  'upgrade',
  'host'
]);

type RouteContext = {
  params: Promise<{
    path: string[];
  }>;
};

function resolveApiBaseUrl() {
  return (
    process.env.INTERNAL_API_URL ||
    process.env.NEXT_PUBLIC_API_URL ||
    'http://localhost:5000/api'
  ).replace(/\/$/, '');
}

function buildTargetUrl(path: string[], request: Request) {
  const sourceUrl = new URL(request.url);
  const targetUrl = new URL(`${resolveApiBaseUrl()}/${path.join('/')}`);
  targetUrl.search = sourceUrl.search;
  return targetUrl;
}

function copyRequestHeaders(request: Request) {
  const headers = new Headers();
  request.headers.forEach((value, key) => {
    if (!HOP_BY_HOP_HEADERS.has(key.toLowerCase())) {
      headers.set(key, value);
    }
  });
  return headers;
}

function copyResponseHeaders(response: Response) {
  const headers = new Headers();
  response.headers.forEach((value, key) => {
    if (!HOP_BY_HOP_HEADERS.has(key.toLowerCase())) {
      headers.set(key, value);
    }
  });
  return headers;
}

async function proxy(request: Request, context: RouteContext) {
  const { path } = await context.params;
  const method = request.method.toUpperCase();
  const hasBody = method !== 'GET' && method !== 'HEAD';
  const requestInit: RequestInit = {
    method,
    headers: copyRequestHeaders(request),
    cache: 'no-store'
  };

  if (hasBody) {
    requestInit.body = await request.arrayBuffer();
  }

  const response = await fetch(buildTargetUrl(path, request), requestInit);

  return new Response(await response.arrayBuffer(), {
    status: response.status,
    statusText: response.statusText,
    headers: copyResponseHeaders(response)
  });
}

export const dynamic = 'force-dynamic';

export {
  proxy as DELETE,
  proxy as GET,
  proxy as PATCH,
  proxy as POST,
  proxy as PUT
};
