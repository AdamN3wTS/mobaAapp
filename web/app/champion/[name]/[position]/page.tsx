import Link from 'next/link';
import { getChampion } from '@/lib/api';
import ChampionIcon from '@/components/ChampionIcon';
import WinRateTrend from '@/components/WinRateTrends';
import { getDdVersion, getDdData, runeIcon, itemIcon, spellIcon, SHARDS } from '@/lib/ddragon';

interface ChampionPageProps {
  params: Promise<{ name: string; position: string }>;
}
export const instant = false;

const POSITIONS = [
  { key: 'TOP', slug: 'top', label: 'Top' },
  { key: 'JUNGLE', slug: 'jungle', label: 'Jungle' },
  { key: 'MIDDLE', slug: 'mid', label: 'Mid' },
  { key: 'BOTTOM', slug: 'adc', label: 'ADC' },
  { key: 'UTILITY', slug: 'support', label: 'Support' },
];

const POSITION_ALIASES: Record<string, string> = {
  top: 'TOP', toplane: 'TOP',
  jungle: 'JUNGLE', jg: 'JUNGLE', jungler: 'JUNGLE',
  mid: 'MIDDLE', middle: 'MIDDLE', midlane: 'MIDDLE',
  adc: 'BOTTOM', bot: 'BOTTOM', bottom: 'BOTTOM',
  support: 'UTILITY', supp: 'UTILITY', sup: 'UTILITY', utility: 'UTILITY',
};

const normalizePosition = (p: string) => POSITION_ALIASES[p.toLowerCase().trim()] ?? p.toUpperCase();
const positionLabel = (key: string) => POSITIONS.find((p) => p.key === key)?.label ?? key;
const capitalize = (s: string) => (s ? s.charAt(0).toUpperCase() + s.slice(1) : s);

const TRINKETS = [3340, 3363, 3364];
const pct = (v: number) => `${v.toFixed(1)}%`;
const wrColor = (wr: number) => (wr >= 52 ? 'text-blue-400' : wr <= 48 ? 'text-red-400' : 'text-neutral-200');

const SPELL_SLOTS = 2;
const CORE_ITEM_SLOTS = 12;
const BOOT_SLOTS = 3;
const RUNE_BOX_H = 'min-h-[248px]';
const SPELL_CARD_H = 'h-[72px]';
const ITEM_CARD_H = 'h-[68px]';

const padSlots = <T,>(arr: T[], n: number): (T | null)[] =>
  [...arr.slice(0, n), ...Array(Math.max(0, n - arr.length)).fill(null)];

export default async function ChampionPage({ params }: ChampionPageProps) {
  const resolvedParams = await params;
  const rawName = resolvedParams.name;
  const name = decodeURIComponent(rawName);
  const position = normalizePosition(decodeURIComponent(resolvedParams.position));

  const [res, version]: [any, string] = await Promise.all([
    getChampion(name, position).catch(() => null),
    getDdVersion(),
  ]);
  const raw = res?.data ?? res;
  const hasData = Boolean(raw?.championId);
  const champion: any = hasData ? raw : {};

  const dd = await getDdData(version);

  const runeStats = new Map<number, any>((champion.runes ?? []).map((r: any) => [r.perkId, r]));
  const gamesOf = (id: number) => runeStats.get(id)?.games ?? 0;
  const topInRow = (row: any) =>
    row.runes.reduce((best: any, r: any) => (gamesOf(r.id) > gamesOf(best?.id) ? r : best), null);

  const page = champion.runePages?.[0];
  const primaryTree = dd.runes.find((t) => t.id === page?.primaryStyleId);
  const subTree = dd.runes.find((t) => t.id === page?.subStyleId);

  const selected = new Set<number>();
  if (page && primaryTree) {
    selected.add(page.keystoneId);
    primaryTree.slots.slice(1).forEach((row: any) => {
      const top = topInRow(row);
      if (top && gamesOf(top.id) > 0) selected.add(top.id);
    });
  }
  if (subTree) {
    subTree.slots
      .slice(1)
      .map((row: any) => topInRow(row))
      .filter((r: any) => r && gamesOf(r.id) > 0)
      .sort((a: any, b: any) => gamesOf(b.id) - gamesOf(a.id))
      .slice(0, 2)
      .forEach((r: any) => selected.add(r.id));
  }

  const shards = champion.shards?.[0];

  const spellById = (id: number) => dd.spells.find((s) => s.key === String(id));
  const spellSlots = padSlots(champion.spells ?? [], SPELL_SLOTS);

  const games = champion.games ?? 0;
  const minGames = 1
  const withInfo = (champion.items ?? [])
    .map((i: any) => ({ ...i, info: dd.items[String(i.itemId)] }))
    .filter(
      (i: any) =>
        i.info &&
        i.games >= minGames &&
        !TRINKETS.includes(i.itemId) &&
        !i.info.tags?.includes('Consumable') &&
        !i.info.tags?.includes('Trinket'),
    );

  const BASIC_BOOTS = 1001;

const bootSlots = padSlots(
  withInfo
    .filter((i: any) => i.info.tags?.includes('Boots') && i.itemId !== BASIC_BOOTS)
    .sort((a: any, b: any) => b.games - a.games),
  BOOT_SLOTS,
);
  const coreSlots = padSlots(
    withInfo.filter((i: any) => !i.info.into && !i.info.tags?.includes('Boots') && (i.info.gold?.total ?? 0) >= 2000),
    CORE_ITEM_SLOTS,
  );

  const val = (v: number | null | undefined, f: (x: number) => string) => (hasData && v != null ? f(v) : '-');

  return (
    <main className="min-h-screen bg-neutral-950 text-white p-4 sm:p-8">
      <div className="max-w-4xl mx-auto bg-neutral-900 rounded-2xl p-6 sm:p-8 shadow-2xl border border-neutral-800">
        <ChampionHeader
          championId={hasData ? champion.championId : null}
          displayName={hasData ? champion.championName : capitalize(name)}
          urlName={rawName}
          activePosition={position}
          note={hasData ? null : `Brak danych na roli ${positionLabel(position)}`}
        />

        <div className="mt-8 grid grid-cols-2 sm:grid-cols-4 gap-3">
          <StatCard
            label="Win rate"
            value={val(champion.winRate, (v) => pct(v * 100))}
            className={hasData && champion.winRate != null ? wrColor(champion.winRate * 100) : 'text-neutral-600'}
          />
          <StatCard label="Pick rate" value={val(champion.pickRate, (v) => pct(v * 100))} />
          <StatCard label="Ban rate" value={val(champion.banRate, (v) => pct(v * 100))} />
          <StatCard label="Gry" value={val(champion.games, (v) => v.toLocaleString('pl-PL'))} />
        </div>

        <Section title="Trend win rate">
          <WinRateTrend
            currentPatch={champion.trend?.currentPatch ?? champion.patch ?? ''}
            currentPatchHistory={champion.trend?.currentPatchHistory ?? []}
            previousPatches={champion.trend?.previousPatches ?? []}
          />
        </Section>

        <Section title="Runy">
          <div className="h-7 mb-5 flex items-center text-sm">
            {page && (
              <span className="bg-neutral-800 px-3 py-1 rounded-lg">
                <span className={`font-semibold ${wrColor(page.winRate)}`}>{pct(page.winRate)} WR</span>
                <span className="text-neutral-400"> · {pct(page.pickRate)} PR · {page.games} gier</span>
              </span>
            )}
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
            {primaryTree ? (
              <RuneTree tree={primaryTree} rows={primaryTree.slots} selected={selected} stats={runeStats} keystone />
            ) : (
              <EmptyBox className={RUNE_BOX_H} />
            )}
            {subTree ? (
              <RuneTree tree={subTree} rows={subTree.slots.slice(1)} selected={selected} stats={runeStats} />
            ) : (
              <EmptyBox className={RUNE_BOX_H} />
            )}
            {shards ? (
              <div className={`bg-neutral-800/50 rounded-xl p-4 ${RUNE_BOX_H}`}>
                <div className="text-sm font-semibold text-neutral-300 mb-3">Odłamki</div>
                <div className="space-y-2 text-sm">
                  {[shards.offensePerk, shards.flexPerk, shards.defensePerk].map((id: number, idx: number) => (
                    <div key={idx} className="bg-neutral-900 rounded-lg px-3 py-2 text-neutral-200">
                      {SHARDS[id] ?? id}
                    </div>
                  ))}
                </div>
                <div className="mt-3 text-xs text-neutral-500">
                  {pct(shards.winRate)} WR · {pct(shards.pickRate)} PR
                </div>
              </div>
            ) : (
              <EmptyBox className={RUNE_BOX_H} />
            )}
          </div>
        </Section>

        <Section title="Czary przywoływacza">
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
            {spellSlots.map((s: any, idx) => {
              if (!s) return <EmptyBox key={`empty-${idx}`} className={SPELL_CARD_H} />;
              const a = spellById(s.spell1Id);
              const b = spellById(s.spell2Id);
              return (
                <div
                  key={`${s.spell1Id}-${s.spell2Id}`}
                  className={`bg-neutral-800/50 px-4 rounded-xl flex items-center justify-between ${SPELL_CARD_H}`}
                >
                  <div className="flex gap-2">
                    {[a, b].map((sp, i) =>
                      sp ? (
                        <img
                          key={i}
                          src={spellIcon(version, sp.image.full)}
                          alt={sp.name}
                          title={sp.name}
                          width={40}
                          height={40}
                          className="rounded-lg w-10 h-10"
                        />
                      ) : (
                        <div key={i} className="w-10 h-10 rounded-lg bg-neutral-800" />
                      ),
                    )}
                  </div>
                  <div className="text-right">
                    <div className="font-bold">{pct(s.pickRate)}</div>
                    <div className="text-xs text-neutral-400">pick rate</div>
                  </div>
                </div>
              );
            })}
          </div>
        </Section>

        <Section title="Przedmioty">
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
            {coreSlots.map((i: any, idx) =>
              i ? <ItemCard key={i.itemId} item={i} version={version} /> : <EmptyBox key={`empty-${idx}`} className={ITEM_CARD_H} />,
            )}
          </div>

          <h3 className="text-lg font-semibold mt-6 mb-3 text-neutral-300">Buty</h3>
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
            {bootSlots.map((i: any, idx) =>
              i ? <ItemCard key={i.itemId} item={i} version={version} /> : <EmptyBox key={`empty-${idx}`} className={ITEM_CARD_H} />,
            )}
          </div>
        </Section>
      </div>
    </main>
  );
}

function ChampionHeader({
  championId,
  displayName,
  urlName,
  activePosition,
  note,
}: {
  championId: number | null;
  displayName: string;
  urlName: string;
  activePosition: string;
  note: string | null;
}) {
  return (
    <div className="flex flex-col sm:flex-row items-center gap-6 text-center sm:text-left">
      <div className="w-24 h-24 shrink-0">
        {championId ? (
          <ChampionIcon
            championId={championId}
            name={displayName}
            size={96}
            className="rounded-2xl border border-neutral-700 w-24 h-24"
          />
        ) : (
          <div className="w-24 h-24 rounded-2xl border border-neutral-700 bg-neutral-800 flex items-center justify-center text-3xl font-bold text-neutral-500">
            {displayName.charAt(0)}
          </div>
        )}
      </div>
      <div className="flex-1">
        <h1 className="text-4xl font-bold leading-tight">{displayName}</h1>
        <p className="h-5 text-sm text-neutral-500">{note ?? ''}</p>
        <div className="mt-2 flex flex-wrap gap-2 justify-center sm:justify-start text-sm">
          {POSITIONS.map((p) => (
            <Link
              key={p.key}
              href={`/champion/${urlName}/${p.slug}`}
              scroll={false}
              className={`px-3 py-1 rounded-lg border transition-colors ${
                p.key === activePosition
                  ? 'bg-blue-500/20 text-blue-400 border-blue-500/50'
                  : 'bg-neutral-800 text-neutral-400 border-transparent hover:bg-neutral-700'
              }`}
            >
              {p.label}
            </Link>
          ))}
        </div>
      </div>
    </div>
  );
}

function Section({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <div className="mt-10 border-t border-neutral-800 pt-8">
      <h2 className="text-2xl font-semibold mb-6">{title}</h2>
      {children}
    </div>
  );
}

function EmptyBox({ className = '' }: { className?: string }) {
  return <div className={`rounded-xl bg-neutral-800/25 border border-dashed border-neutral-800 ${className}`} />;
}

function StatCard({ label, value, className = '' }: { label: string; value: string; className?: string }) {
  return (
    <div className="bg-neutral-800/50 rounded-xl p-4 text-center border border-neutral-800">
      <div className={`text-2xl font-bold tabular-nums ${className}`}>{value}</div>
      <div className="text-xs text-neutral-400 mt-1 uppercase tracking-wide">{label}</div>
    </div>
  );
}

function RuneTree({
  tree,
  rows,
  selected,
  stats,
  keystone = false,
}: {
  tree: any;
  rows: any[];
  selected: Set<number>;
  stats: Map<number, any>;
  keystone?: boolean;
}) {
  return (
    <div className={`bg-neutral-800/50 rounded-xl p-4 ${RUNE_BOX_H}`}>
      <div className="flex items-center gap-2 mb-4 h-6">
        <img src={runeIcon(tree.icon)} alt={tree.name} width={24} height={24} className="w-6 h-6" />
        <span className="text-sm font-semibold text-neutral-300">{tree.name}</span>
      </div>
      <div className="space-y-3">
        {rows.map((row: any, rowIdx: number) => {
          const size = keystone && rowIdx === 0 ? 44 : 32;
          return (
            <div key={rowIdx} className="flex justify-around" style={{ height: size }}>
              {row.runes.map((r: any) => {
                const isSel = selected.has(r.id);
                const s = stats.get(r.id);
                const tip = s ? `${r.name} · ${s.winRate.toFixed(1)}% WR · ${s.pickRate.toFixed(1)}% PR` : r.name;
                return (
                  <img
                    key={r.id}
                    src={runeIcon(r.icon)}
                    alt={r.name}
                    title={tip}
                    width={size}
                    height={size}
                    style={{ width: size, height: size }}
                    className={`rounded-full transition ${
                      isSel ? 'ring-2 ring-blue-500 bg-neutral-900' : 'opacity-25 grayscale'
                    }`}
                  />
                );
              })}
            </div>
          );
        })}
      </div>
    </div>
  );
}

function ItemCard({ item, version }: { item: any; version: string }) {
  return (
    <div className={`bg-neutral-800/50 px-3 rounded-xl flex items-center gap-3 hover:bg-neutral-800 transition-colors ${ITEM_CARD_H}`}>
      <img
        src={itemIcon(version, item.itemId)}
        alt={item.info.name}
        title={item.info.name}
        width={44}
        height={44}
        className="rounded-lg w-11 h-11 shrink-0"
      />
      <div className="min-w-0 flex-1">
        <div className="text-sm font-medium truncate">{item.info.name}</div>
        <div className="text-xs text-neutral-400 truncate">
          <span className={wrColor(item.winRate)}>{pct(item.winRate)} WR</span> · {pct(item.pickRate)} PR · {item.games}
        </div>
      </div>
    </div>
  );
}