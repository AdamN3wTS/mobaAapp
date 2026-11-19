'use client';

import { useEffect, useRef, useState, useTransition } from 'react';
import Link from 'next/link';
import ChampionIcon from '@/components/ChampionIcon';
import type { MatchParticipant, MatchSummary } from '@/types/summoner';

interface DataDragonEntry {
  key?: string;
  name: string;
  image: { full: string };
}

interface DataDragonCatalog {
  version: string;
  items: Record<string, DataDragonEntry>; 
  spells: Record<string, DataDragonEntry>;
}

interface MatchHistoryProps {
  matches: MatchSummary[];
  puuid?: string;
  pageSize?: number;
  totalItems?: number | null;
  loadMore?: (
    puuid: string,
    page: number,
    pageSize: number,
  ) => Promise<{ matches: MatchSummary[]; totalItems: number | null }>;
}

const DD = 'https://ddragon.leagueoflegends.com/cdn';
const ITEM_SLOTS = 7; 

const time = (seconds: number) => `${Math.floor(seconds / 60)}:${String(seconds % 60).padStart(2, '0')}`;

const SPELL_FALLBACK: Record<number, { file: string; name: string }> = {
  1: { file: 'SummonerBoost', name: 'Oczyszczenie' },
  3: { file: 'SummonerExhaust', name: 'Wyczerpanie' },
  4: { file: 'SummonerFlash', name: 'Błysk' },
  6: { file: 'SummonerHaste', name: 'Duch' },
  7: { file: 'SummonerHeal', name: 'Uzdrowienie' },
  11: { file: 'SummonerSmite', name: 'Porażenie' },
  12: { file: 'SummonerTeleport', name: 'Teleportacja' },
  13: { file: 'SummonerMana', name: 'Przejrzystość' },
  14: { file: 'SummonerDot', name: 'Podpalenie' },
  21: { file: 'SummonerBarrier', name: 'Bariera' },
  32: { file: 'SummonerSnowball', name: 'Oznaczenie' },
};


const ROLE_SLUG: Record<string, string> = {
  TOP: 'top',
  JUNGLE: 'jungle',
  MIDDLE: 'mid',
  BOTTOM: 'adc',
  UTILITY: 'support',
};
const DEFAULT_ROLE_SLUG = 'mid'; // np. ARAM nie ma pozycji

const championHref = (championName: string, teamPosition?: string) =>
  `/champion/${encodeURIComponent(championName.toLowerCase())}/${ROLE_SLUG[teamPosition ?? ''] ?? DEFAULT_ROLE_SLUG}`;

const summonerHref = (gameName: string, tagLine: string) =>
  `/summoner/${encodeURIComponent(gameName)}/${encodeURIComponent(tagLine)}`;

const majorMinor = (patch: string) => patch?.split('.').slice(0, 2).join('.') ?? '';

async function fetchCatalog(version: string): Promise<DataDragonCatalog> {
  const [itemsRes, spellsRes] = await Promise.all([
    fetch(`${DD}/${version}/data/pl_PL/item.json`),
    fetch(`${DD}/${version}/data/pl_PL/summoner.json`),
  ]);
  if (!itemsRes.ok || !spellsRes.ok) throw new Error(`Data Dragon ${version}`);

  const items = (await itemsRes.json()).data as Record<string, DataDragonEntry>;
  const spellsByName = (await spellsRes.json()).data as Record<string, DataDragonEntry>;

  const spells: Record<string, DataDragonEntry> = {};
  for (const s of Object.values(spellsByName)) {
    if (s.key) spells[s.key] = s;
  }

  return { version, items, spells };
}

async function getCatalogs(matches: MatchSummary[]) {
  const versions = (await (await fetch(`${DD}/versions.json`)).json()) as string[];
  const latest = versions[0];

  const patchToVersion = new Map<string, string>();
  for (const m of matches) {
    const mm = majorMinor(m.patch);
    patchToVersion.set(m.patch, versions.find((v) => v.startsWith(`${mm}.`)) ?? latest);
  }

  const unique = [...new Set(patchToVersion.values())];
  const loaded = await Promise.allSettled(unique.map(fetchCatalog));
  const byVersion = new Map<string, DataDragonCatalog>();
  loaded.forEach((r, i) => r.status === 'fulfilled' && byVersion.set(unique[i], r.value));

  const byPatch: Record<string, DataDragonCatalog> = {};
  for (const [patch, version] of patchToVersion) {
    const c = byVersion.get(version);
    if (c) byPatch[patch] = c;
  }
  return { latest, byPatch };
}


function ItemIcon({ itemId, version, catalog, size }: { itemId: number; version: string; catalog?: DataDragonCatalog; size: number }) {
  const style = { width: size, height: size };
  if (!itemId) return <div style={style} className="rounded bg-neutral-800/70" />;

  const name = catalog?.items[String(itemId)]?.name ?? `Przedmiot ${itemId}`;
  return (
    <img
      src={`${DD}/${version}/img/item/${itemId}.png`}
      alt={name}
      title={name}
      width={size}
      height={size}
      style={style}
      loading="lazy"
      className="rounded bg-neutral-800"
      onError={(e) => ((e.target as HTMLImageElement).style.visibility = 'hidden')}
    />
  );
}

function SpellIcon({ spellId, version, catalog, size }: { spellId: number; version: string; catalog?: DataDragonCatalog; size: number }) {
  const style = { width: size, height: size };
  const fromCatalog = catalog?.spells[String(spellId)];
  const fallback = SPELL_FALLBACK[spellId];
  const file = fromCatalog?.image.full ?? (fallback ? `${fallback.file}.png` : null);
  const name = fromCatalog?.name ?? fallback?.name ?? `Czar ${spellId}`;

  if (!file) return <div style={style} title={name} className="rounded bg-neutral-800" />;
  return (
    <img
      src={`${DD}/${version}/img/spell/${file}`}
      alt={name}
      title={name}
      width={size}
      height={size}
      style={style}
      loading="lazy"
      className="rounded bg-neutral-800"
    />
  );
}

function ItemRow({ items, version, catalog, size = 28 }: { items: { itemId: number }[]; version: string; catalog?: DataDragonCatalog; size?: number }) {
  const ids = Array.from({ length: ITEM_SLOTS }, (_, i) => items[i]?.itemId ?? 0);
  return (
    <div className="flex gap-1" aria-label="Przedmioty">
      {ids.map((id, i) => (
        <ItemIcon key={i} itemId={id} version={version} catalog={catalog} size={size} />
      ))}
    </div>
  );
}

function SpellPair({ p, version, catalog, size = 22 }: { p: MatchParticipant; version: string; catalog?: DataDragonCatalog; size?: number }) {
  return (
    <div className="flex flex-col gap-1" aria-label="Czary przywoływacza">
      <SpellIcon spellId={p.summoner1Id} version={version} catalog={catalog} size={size} />
      <SpellIcon spellId={p.summoner2Id} version={version} catalog={catalog} size={size} />
    </div>
  );
}


function ParticipantRow({ participant: p, version, catalog }: { participant: MatchParticipant; version: string; catalog?: DataDragonCatalog }) {
  const hasRiotId = Boolean(p.gameName && p.tagLine);

  return (
    <li className={`flex items-center gap-2 rounded-lg border p-2 ${p.isCurrentPlayer ? 'border-blue-700 bg-blue-950/30' : 'border-neutral-800 bg-neutral-900/60'}`}>
      <Link
        href={championHref(p.championName, p.teamPosition)}
        title={`${p.championName} – statystyki`}
        className="shrink-0 rounded transition hover:ring-2 hover:ring-blue-500"
      >
        <ChampionIcon championId={p.championId} name={p.championName} size={40} />
      </Link>
      <SpellPair p={p} version={version} catalog={catalog} size={19} />
      <div className="min-w-0 flex-1">
        <div className="truncate text-sm font-semibold">
          {hasRiotId ? (
            <Link href={summonerHref(p.gameName, p.tagLine)} className="hover:text-blue-400 hover:underline">
              {p.gameName}
              <span className="text-neutral-500">#{p.tagLine}</span>
            </Link>
          ) : (
            p.championName
          )}
          {p.isCurrentPlayer && <span className="ml-1 text-blue-400">(Ty)</span>}
        </div>
        <div className="text-xs text-neutral-400 tabular-nums">
          {p.kills}/{p.deaths}/{p.assists}
        </div>
      </div>
      <ItemRow items={p.items} version={version} catalog={catalog} size={24} />
    </li>
  );
}

export default function MatchHistory({
  matches: initialMatches,
  puuid,
  pageSize = 10,
  totalItems = null,
  loadMore,
}: MatchHistoryProps) {
  const [matches, setMatches] = useState<MatchSummary[]>(initialMatches);
  const [page, setPage] = useState(1);
  const [total, setTotal] = useState<number | null>(totalItems);

  const [hasMore, setHasMore] = useState(
    initialMatches.length >= pageSize || (totalItems != null && initialMatches.length < totalItems),
  );
  const [loadError, setLoadError] = useState(false);
  const [isLoading, startLoading] = useTransition();

  const [catalogs, setCatalogs] = useState<Record<string, DataDragonCatalog>>({});
  const [latest, setLatest] = useState<string | null>(null);
  const requestedPatches = useRef(new Set<string>());

  useEffect(() => {
    const missing = matches.filter((m) => !requestedPatches.current.has(m.patch));
    if (missing.length === 0) return;
    missing.forEach((m) => requestedPatches.current.add(m.patch));

    getCatalogs(missing)
      .then(({ latest, byPatch }) => {
        setLatest((prev) => prev ?? latest);
        setCatalogs((prev) => ({ ...prev, ...byPatch }));
      })
      .catch(() => {});
  }, [matches]);

  const handleLoadMore = () => {
    if (!loadMore || !puuid) return;
    setLoadError(false);
    startLoading(async () => {
      try {
        const nextPage = page + 1;
        const res = await loadMore(puuid, nextPage, pageSize);
        const newTotal = res.totalItems ?? total;

        const seen = new Set(matches.map((m) => m.matchId));
        const merged = [...matches, ...res.matches.filter((m) => !seen.has(m.matchId))];

        setMatches(merged);
        setPage(nextPage);
        setTotal(newTotal);
        const added = merged.length - matches.length;
        setHasMore(added > 0 && res.matches.length >= pageSize);
      } catch {
        setLoadError(true);
      }
    });
  };

  return (
    <div className="space-y-3">
      {matches.map((match) => {
        const player = match.participants.find((p) => p.isCurrentPlayer);
        const allies = match.participants.filter((p) => p.teamId === player?.teamId);
        const opponents = match.participants.filter((p) => p.teamId !== player?.teamId);
        const catalog = catalogs[match.patch];
        const version = catalog?.version ?? latest ?? `${majorMinor(match.patch)}.1`;

        return (
          <details
            key={match.matchId}
            className={`group rounded-xl border-l-4 ${match.isWin ? 'border-blue-500' : 'border-red-500'} bg-neutral-800/50`}
          >
            <summary className="flex cursor-pointer list-none flex-wrap items-center gap-4 p-4 hover:bg-neutral-800">
              <div className="flex items-center gap-2">
                <Link
                  href={championHref(match.championName, player?.teamPosition)}
                  title={`${match.championName} – statystyki`}
                  onClick={(e) => e.stopPropagation()} 
                  className="rounded-xl transition hover:ring-2 hover:ring-blue-500"
                >
                  <ChampionIcon championId={match.championId} name={match.championName} size={56} className="rounded-xl" />
                </Link>
                {player && <SpellPair p={player} version={version} catalog={catalog} size={26} />}
              </div>

              <div className="min-w-0 w-36">
                <div className={`font-bold ${match.isWin ? 'text-blue-400' : 'text-red-400'}`}>
                  {match.isWin ? 'Zwycięstwo' : 'Porażka'}
                </div>
                <div className="truncate text-sm text-neutral-400">
                  {match.queueType} · {time(match.gameDurationInSeconds)}
                </div>
              </div>

              <div className="text-center">
                <div className="text-lg font-bold tabular-nums">
                  {match.stats.kills} / <span className="text-red-400">{match.stats.deaths}</span> / {match.stats.assists}
                </div>
                <div className="text-xs text-neutral-400">{match.stats.totalMinionsKilled} CS</div>
              </div>

              {player && (
                <div className="ml-auto">
                  <ItemRow items={player.items} version={version} catalog={catalog} size={32} />
                </div>
              )}
            </summary>

            <div className="grid gap-4 border-t border-neutral-700 p-4 lg:grid-cols-2">
              <section>
                <h3 className="mb-2 font-semibold text-blue-300">Twoja drużyna</h3>
                <ul className="space-y-2">
                  {allies.map((p, i) => (
                    <ParticipantRow key={`${p.teamId}-${p.championId}-${i}`} participant={p} version={version} catalog={catalog} />
                  ))}
                </ul>
              </section>
              <section>
                <h3 className="mb-2 font-semibold text-red-300">Przeciwnicy</h3>
                <ul className="space-y-2">
                  {opponents.map((p, i) => (
                    <ParticipantRow key={`${p.teamId}-${p.championId}-${i}`} participant={p} version={version} catalog={catalog} />
                  ))}
                </ul>
              </section>
            </div>
          </details>
        );
      })}

      {loadMore && puuid && (hasMore || loadError) && (
        <div className="pt-2 text-center">
          {loadError && <p className="mb-2 text-sm text-red-400">Nie udało się pobrać meczów. Spróbuj ponownie.</p>}
          <button
            type="button"
            onClick={handleLoadMore}
            disabled={isLoading}
            className="inline-flex items-center gap-2 rounded-lg bg-neutral-800 px-6 py-2.5 text-sm font-medium text-neutral-200 transition-colors hover:bg-neutral-700 disabled:cursor-wait disabled:opacity-60"
          >
            {isLoading && (
              <span className="h-4 w-4 animate-spin rounded-full border-2 border-neutral-500 border-t-white" />
            )}
            {isLoading ? 'Ładowanie…' : 'Załaduj kolejne'}
          </button>
          {total != null && total > matches.length && (
            <div className="mt-2 text-xs text-neutral-500">
              {matches.length} z {total}
            </div>
          )}
        </div>
      )}
    </div>
  );
}