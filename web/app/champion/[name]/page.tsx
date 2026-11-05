import { redirect } from 'next/navigation';
import { getChampionPositions } from '@/lib/api';

interface ChampionRedirectProps {
  params: Promise<{ name: string }>;
}

const TAB_SLUG: Record<string, string> = {
  TOP: 'top',
  JUNGLE: 'jungle',
  MIDDLE: 'mid',
  BOTTOM: 'adc',
  UTILITY: 'support',
};

export default async function ChampionRedirectPage({ params }: ChampionRedirectProps) {
  const resolvedParams = await params;
  const decoded = decodeURIComponent(resolvedParams.name);

  const res: any = await getChampionPositions(decoded);
  const positions = res?.data ?? res;
  const main: string | undefined = Array.isArray(positions) ? positions[0]?.teamPosition : undefined;

  if (!main) {
    return (
      <main className="min-h-screen bg-neutral-950 text-white p-4 sm:p-8">
        <div className="max-w-4xl mx-auto bg-neutral-900 rounded-2xl p-8 border border-neutral-800 text-center">
          <p className="text-red-400">Brak statystyk dla {decoded}.</p>
        </div>
      </main>
    );
  }

  redirect(`/champion/${encodeURIComponent(decoded.toLowerCase())}/${TAB_SLUG[main] ?? main.toLowerCase()}`);
}