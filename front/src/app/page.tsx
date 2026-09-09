import { Suspense } from 'react';

import { LoginCard } from '@/components/LoginCard';

export default function LoginPage() {
  return (
    <div className="min-h-screen flex w-full">
      <Suspense fallback={null}>
        <LoginCard />
      </Suspense>
    </div>
  );
}
