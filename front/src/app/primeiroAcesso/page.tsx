import { Suspense } from 'react';

import { FirstAccess } from '@/components/FirstAccess';

export default function PrimeiroAcesso() {
  return (
    <div className="min-h-screen flex w-full">
      <Suspense fallback={null}>
        <FirstAccess />
      </Suspense>
    </div>
  );
}
