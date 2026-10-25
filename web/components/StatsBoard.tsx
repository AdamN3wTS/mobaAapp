'use client';

import { useEffect, useState } from 'react';
import { getStats } from '@/lib/api';

const tabs = [['ALL', 'Ogólnie'], ['TOP', 'Top'], ['JUNGLE', 'Jungle'], ['MIDDLE', 'Mid'], ['BOTTOM', 'Bot'], ['UTILITY', 'Support']];
const lists = [['Win Rate', 'winRate'], ['Pick Rate', 'pickRate'], ['Ban Rate', 'banRate']];
const MIN_GAMES = 5;
const pct = (v: number) => (v * 100).toFixed(1) + '%';
const top5 = (arr: any[], k: string) => [...arr].sort((a, b) => b[k] - a[k]).slice(0, 5);

export default function StatsBoard() {
  const [s, setS] = useState<any>(null);
  const [tab, setTab] = useState('ALL');

  useEffect(() => { getStats().then(setS); }, []);
  if (!s) return <p className="text-neutral-500">Ładowanie statystyk...</p>;

  const bans = Object.fromEntries(s.statsBans.map((b: any) => [b.championId, b.banRate]));

  const all: any = {};
  s.statsPositions.forEach((p: any) => p.stats.forEach((c: any) => {
    const a = (all[c.championId] ??= { championId: c.championId, championName: c.championName, games: 0, wins: 0, pickRate: 0 });
    a.games += c.games;
    a.wins += c.winRate * c.games;
    a.pickRate += c.pickRate;
  }));

  const champs = (tab === 'ALL'
    ? Object.values(all).map((a: any) => ({ ...a, winRate: a.wins / a.games }))
    : s.statsPositions.find((p: any) => p.teamPosition === tab)?.stats ?? []
  ).map((c: any) => ({ ...c, banRate: bans[c.championId] ?? 0 }));

  const source = (k: string) =>
    k === 'winRate' ? champs.filter((c: any) => c.games >= MIN_GAMES)
    : k === 'banRate' && tab === 'ALL' ? s.statsBans
    : champs;

  return (
    <div className="w-full bg-neutral-900 p-4 rounded-2xl shadow-xl border border-neutral-800 text-left">
      <div className="flex flex-wrap gap-2 mb-4">
        {tabs.map(([k, l]) => (
          <button key={k} onClick={() => setTab(k)}
            className={`px-4 py-2 rounded-xl text-sm font-semibold transition-colors ${tab === k ? 'bg-blue-600' : 'bg-neutral-800 text-neutral-400 hover:bg-neutral-700'}`}>
            {l}
          </button>
        ))}
      </div>
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
        {lists.map(([title, k]) => (
          <div key={k} className="bg-neutral-800/50 rounded-xl p-4">
            <h3 className="font-semibold text-blue-400 mb-3">Top 5 {title}</h3>
            <ol className="space-y-2">
              {top5(source(k), k).map((c: any, i: number) => (
                <li key={c.championId} className="flex justify-between text-sm">
                  <span><span className="text-neutral-500 mr-2">{i + 1}.</span>{c.championName}</span>
                  <span className="font-bold">{pct(c[k])}</span>
                </li>
              ))}
            </ol>
          </div>
        ))}
      </div>
    </div>
  );
}