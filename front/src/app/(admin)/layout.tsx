'use client';

import Link from 'next/link';
import { usePathname } from 'next/navigation';

import Header from '@/components/Header';
import ProtectedLayout from '@/components/ProtectedLayout';

export default function AdminLayout({
  children
}: {
  children: React.ReactNode;
}) {
  const pathname = usePathname();
  return (
    <ProtectedLayout allowedRoles={['admin']}>
      <div className="min-h-screen bg-slate-50">
        <Header />
        <nav
          aria-label="Administração"
          className="border-b bg-white px-5 md:px-10"
        >
          <div className="mx-auto flex max-w-7xl gap-2 py-2 text-sm font-medium">
            {[
              { href: '/curso', label: 'Cursos' },
              { href: '/usuarios', label: 'Pessoas e acesso' }
            ].map(({ href, label }) => (
              <Link
                key={href}
                href={href}
                aria-current={pathname.startsWith(href) ? 'page' : undefined}
                className={`rounded-lg px-4 py-2.5 transition-colors ${pathname.startsWith(href) ? 'bg-blue-700 text-white' : 'text-slate-700 hover:bg-blue-50 hover:text-blue-800'}`}
              >
                {label}
              </Link>
            ))}
          </div>
        </nav>
        <main className="mt-6 px-5 md:px-10 pb-5">{children}</main>
      </div>
    </ProtectedLayout>
  );
}
