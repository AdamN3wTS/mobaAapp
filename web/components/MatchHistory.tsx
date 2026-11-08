'use client';

import { useEffect, useState } from 'react';
import ChampionIcon from '@/components/ChampionIcon';
import type { MatchParticipant, MatchSummary } from '@/types/summoner';

interface DataDragonImage {
  full: string;
}

interface DataDragonEntry {
  name: string;
  image: DataDragonImage;
}

interface DataDragonCatalog {
  items: Record<string, DataDragonEntry>;
  spells: Record<string, DataDragonEntry>;
  version: string;
}

interface MatchHistoryProps {
  matches: MatchSummary[];
}

const dataDragon = 'https://ddragon.leagueoflegends.com/cdn';
const time = (seconds: number) => `${Math.floor(seconds / 60)}:${String(seconds % 60).padStart(2, '0')}`;

async function getDataDragonCatalogs(matches: MatchSummary[]): Promise<Record<string, DataDragonCatalog>> {
  const patches = [...new Set(matches.map((match) => match.patch))];
  const versionsResponse = await fetch(`${dataDragon}/versions.json`);
  if (!versionsResponse.ok) throw new Error('Nie udało się pobrać wersji Data Dragon.');
  const versions = (await versionsResponse.json()) as string[];

  const catalogs = await Promise.all(patches.map(async (patch) => {
    const version = versions.find((candidate) => candidate.startsWith(`${patch}.`));
    if (!version) throw new Error(`Brak danych Data Dragon dla patcha ${patch}.`);

    const [itemsResponse, spellsResponse] = await Promise.all([
      fetch(`${dataDragon}/${version}/data/pl_PL/item.json`),
      fetch(`${dataDragon}/${version}/data/pl_PL/summoner.json`),
    ]);
    if (!itemsResponse.ok || !spellsResponse.ok) {
      throw new Error(`Nie udało się pobrać danych Data Dragon dla patcha ${patch}.`);
    }

    const [itemsData, spellsData] = await Promise.all([
      itemsResponse.json() as Promise<{ data: Record<string, DataDragonEntry> }>,
      spellsResponse.json() as Promise<{ data: Record<string, DataDragonEntry> }>,
    ]);
    return [patch, { items: itemsData.data, spells: spellsData.data, version }] as const;
  }));

  return Object.fromEntries(catalogs);
}

function ItemBuild({ participant, catalog }: { participant: MatchParticipant; catalog?: DataDragonCatalog }) {
  return (
    <div className="flex flex-wrap gap-1" aria-label="Zakupione przedmioty">
      {participant.items.length ? participant.items.map(({ itemId }, index) => {
        const item = catalog?.items[String(itemId)];
        return (
          <span
            key={`${itemId}-${index}`}
            className="flex items-center gap-1 rounded bg-neutral-800 px-1.5 py-1 text-xs text-neutral-300"
            title={item?.name ?? `Przedmiot ${itemId}`}
          >
            {item && (
              <img
                src={`${dataDragon}/${catalog.version}/img/item/${item.image.full}`}
                alt=""
                width={24}
                height={24}
                loading="lazy"
                className="rounded"
              />
            )}
            {item?.name ?? `#${itemId}`}
          </span>
        );
      }) : <span className="text-xs text-neutral-500">Brak przedmiotów</span>}
    </div>
  );
}

function SummonerSpells({ participant, catalog }: { participant: MatchParticipant; catalog?: DataDragonCatalog }) {
  return (
    <div className="flex gap-1" aria-label="Czary przywoływacza">
      {[participant.summoner1Id, participant.summoner2Id].map((spellId) => {
        const spell = catalog?.spells[String(spellId)];
        return (
          <span
            key={spellId}
            className="flex items-center gap-1 rounded bg-neutral-800 px-1.5 py-1 text-xs text-neutral-300"
            title={spell?.name ?? `Czar ${spellId}`}
          >
            {spell && (
              <img
                src={`${dataDragon}/${catalog.version}/img/spell/${spell.image.full}`}
                alt=""
                width={24}
                height={24}
                loading="lazy"
                className="rounded"
              />
            )}
            {spell?.name ?? `#${spellId}`}
          </span>
        );
      })}
    </div>
  );
}

function ParticipantRow({ participant, catalog }: { participant: MatchParticipant; catalog?: DataDragonCatalog }) {
  const name = participant.gameName
    ? `${participant.gameName}#${participant.tagLine}`
    : participant.championName;

  return (
    <li className={`rounded-lg border p-3 ${participant.isCurrentPlayer ? 'border-blue-700 bg-blue-950/30' : 'border-neutral-800 bg-neutral-900/60'}`}>
      <div className="flex flex-wrap items-center gap-3">
        <ChampionIcon championId={participant.championId} name={participant.championName} size={40} />
        <div className="min-w-0 flex-1">
          <div className="truncate text-sm font-semibold">
            {name}
            {participant.isCurrentPlayer && <span className="ml-2 text-blue-400">(Ty)</span>}
          </div>
          <div className="text-xs text-neutral-400">
            {participant.championName}{participant.teamPosition ? ` · ${participant.teamPosition}` : ''}
            {' · '}{participant.kills}/{participant.deaths}/{participant.assists}
          </div>
        </div>
        <SummonerSpells participant={participant} catalog={catalog} />
      </div>
      <div className="mt-2 pl-[52px]">
        <ItemBuild participant={participant} catalog={catalog} />
      </div>
    </li>
  );
}

export default function MatchHistory({ matches }: MatchHistoryProps) {
  const [catalogs, setCatalogs] = useState<Record<string, DataDragonCatalog>>({});
  const [catalogError, setCatalogError] = useState(false);

  useEffect(() => {
    let active = true;
    getDataDragonCatalogs(matches)
      .then((result) => {
        if (active) setCatalogs(result);
      })
      .catch(() => {
        if (active) setCatalogError(true);
      });

    return () => {
      active = false;
    };
  }, [matches]);

  return (
    <div className="space-y-3">
      {catalogError && (
        <p className="text-sm text-amber-400">
          Nie udało się pobrać nazw i ikon z Data Dragon. Nadal wyświetlam identyfikatory Riot.
        </p>
      )}
      {matches.map((match) => {
        const player = match.participants.find((participant) => participant.isCurrentPlayer);
        const playerTeam = player?.teamId;
        const allies = match.participants.filter((participant) => participant.teamId === playerTeam);
        const opponents = match.participants.filter((participant) => participant.teamId !== playerTeam);
        const catalog = catalogs[match.patch];

        return (
          <details
            key={match.matchId}
            className={`group rounded-xl border-l-4 ${match.isWin ? 'border-blue-500' : 'border-red-500'} bg-neutral-800/50`}
          >
            <summary className="flex cursor-pointer list-none items-center justify-between gap-4 p-4 hover:bg-neutral-800">
              <div className="flex min-w-0 items-center gap-4">
                <ChampionIcon championId={match.championId} name={match.championName} size={56} className="rounded-xl" />
                <div className="min-w-0">
                  <div className={`font-bold ${match.isWin ? 'text-blue-400' : 'text-red-400'}`}>
                    {match.isWin ? 'Zwycięstwo' : 'Porażka'}
                  </div>
                  <div className="truncate text-sm text-neutral-400">
                    {match.queueType} · {match.championName} · {time(match.gameDurationInSeconds)}
                  </div>
                  <div className="mt-1 text-xs text-blue-400">Pokaż przedmioty, czary i drużyny</div>
                </div>
              </div>
              <div className="shrink-0 text-right">
                <div className="text-lg font-bold">{match.stats.kills} / {match.stats.deaths} / {match.stats.assists}</div>
                <div className="text-sm text-neutral-400">KDA · {match.stats.totalMinionsKilled} CS</div>
              </div>
            </summary>
            <div className="grid gap-4 border-t border-neutral-700 p-4 lg:grid-cols-2">
              <section>
                <h3 className="mb-2 font-semibold text-blue-300">Twoja drużyna</h3>
                <ul className="space-y-2">
                  {allies.map((participant, index) => (
                    <ParticipantRow key={`${participant.teamId}-${participant.championId}-${index}`} participant={participant} catalog={catalog} />
                  ))}
                </ul>
              </section>
              <section>
                <h3 className="mb-2 font-semibold text-red-300">Przeciwnicy</h3>
                <ul className="space-y-2">
                  {opponents.map((participant, index) => (
                    <ParticipantRow key={`${participant.teamId}-${participant.championId}-${index}`} participant={participant} catalog={catalog} />
                  ))}
                </ul>
              </section>
            </div>
          </details>
        );
      })}
    </div>
  );
}
