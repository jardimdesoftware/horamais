import axios from 'axios';

const api = axios.create({
  // IMPORTANTE:
  // - Em runtime (browser), variáveis NEXT_PUBLIC_* são "baked" no build.
  //   Se o container mudar env sem rebuild, o bundle não muda.
  // - Para produção, preferimos URL relativa e deixamos o Nginx (ou rewrites) fazer proxy.
  baseURL: '/api'
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    return Promise.reject(error);
  }
);

export default api;
