'use client';

import { useSession } from 'next-auth/react';

export default function PerfilCoordenadorPage() {
  const { data: session } = useSession();
  return (
    <main className="mx-auto max-w-xl rounded-xl border bg-white p-6">
      <h1 className="text-2xl font-semibold">Minha conta</h1>
      <p className="mt-4 text-sm text-gray-600">
        Acesso pela conta Google institucional.
      </p>
      <p className="mt-4 font-medium">{session?.user.email}</p>
    </main>
  );
}
