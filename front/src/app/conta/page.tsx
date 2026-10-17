import Header from '@/components/Header';
import ProtectedLayout from '@/components/ProtectedLayout';

export default function ContaPage() {
  return (
    <ProtectedLayout allowedRoles={['usuario']}>
      <Header />
      <main className="mx-auto max-w-xl p-8">
        <h1 className="text-2xl font-semibold">Minha conta</h1>
        <p className="mt-4 text-gray-600">
          Sua conta está ativa, sem permissões administrativas. Procure o
          administrador responsável se precisar de acesso a outros recursos.
        </p>
      </main>
    </ProtectedLayout>
  );
}
