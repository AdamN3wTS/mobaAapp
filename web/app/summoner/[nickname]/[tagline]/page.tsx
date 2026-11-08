import { FaUserCircle } from 'react-icons/fa';
import { getPlayer, getMatches } from '@/lib/api';
import MatchHistory from '@/components/MatchHistory';
import type { MatchHistory as MatchHistoryData, PlayerProfile } from '@/types/summoner';

interface SummonerPageProps {
  params: Promise<{ nickname: string; tagline: string }>;
}
export const instant = false;

export default async function SummonerPage({ params }: SummonerPageProps) {
  const resolvedParams = await params;
  const nickname = decodeURIComponent(resolvedParams.nickname);
  const tagline = decodeURIComponent(resolvedParams.tagline);

  const player: PlayerProfile | null = await getPlayer(nickname, tagline);
  const history: MatchHistoryData | null = player?.puuid ? await getMatches(player.puuid, 1, 10) : null;

  return (
    <main className="min-h-screen bg-neutral-950 text-white p-4 sm:p-8">
      <div className="max-w-4xl mx-auto bg-neutral-900 rounded-2xl p-6 sm:p-8 shadow-2xl border border-neutral-800">
        <div className="flex flex-col sm:flex-row items-center gap-6 text-center sm:text-left">
          <div className="w-24 h-24 bg-neutral-800 rounded-2xl flex items-center justify-center text-5xl text-blue-500 border border-neutral-700">
            <FaUserCircle />
          </div>
          <div>
            <h1 className="text-4xl font-bold flex flex-wrap items-baseline justify-center sm:justify-start gap-2">
              {nickname} <span className="text-xl text-neutral-500 font-medium">#{tagline}</span>
            </h1>
            {player?.puuid ? (
              <div className="mt-2 flex flex-wrap gap-2 justify-center sm:justify-start text-sm">
                <span className="bg-neutral-800 px-3 py-1 rounded-lg text-neutral-400">Poziom {player.summonerLevel}</span>
                {player.ranks.map((r) => (
                  <span key={r.queueType} className="bg-neutral-800 px-3 py-1 rounded-lg">
                    <span className="text-blue-400 font-semibold">{r.queueType === 'RANKED_SOLO_5x5' ? 'Solo' : 'Flex'}</span>{' '}
                    {r.tier} {r.rank} · {r.leaguePoints} LP · {r.wins}W / {r.losses}L
                  </span>
                ))}
              </div>
            ) : (
              <p className="mt-2 text-red-400">Nie znaleziono gracza.</p>
            )}
          </div>
        </div>

        <div className="mt-10 border-t border-neutral-800 pt-8">
          <h2 className="text-2xl font-semibold mb-6">Historia Meczów</h2>
          {history?.matches?.length
            ? <MatchHistory matches={history.matches} />
            : <p className="text-neutral-500">Brak meczów do wyświetlenia.</p>}
        </div>
      </div>
    </main>
  );
}