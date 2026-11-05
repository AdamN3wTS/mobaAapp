import { getChampion } from '@/lib/api';
import ChampionIcon from '@/components/ChampionIcon';
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

const TRINKETS = [3340, 3363, 3364];
const pct = (v: number) => `${v.toFixed(1)}%`;
const wrColor = (wr: number) => (wr >= 52 ? 'text-blue-400' : wr <= 48 ? 'text-red-400' : 'text-neutral-200');

export default async function ChampionPage({ params }: ChampionPageProps) {
  const resolvedParams = await params;
  const name = decodeURIComponent(resolvedParams.name);
  const position = normalizePosition(decodeURIComponent(resolvedParams.position));

  const [res, version]: [any, string] = await Promise.all([getChampion(name, position), getDdVersion()]);
  const champion = res?.data ?? res; 
  if (!champion?.championId) {
    return (
      <main className="min-h-screen bg-neutral-950 text-white p-4 sm:p-8">
        <div className="max-w-4xl mx-auto bg-neutral-900 rounded-2xl p-8 border border-neutral-800 text-center">
<p className="text-red-400">Brak statystyk dla {name} ({positionLabel(position)}).</p>
        </div>
      </main>
    );
  }

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
  const topSpells = (champion.spells ?? []).slice(0, 2);

  const minGames = Math.max(10, Math.round(champion.games * 0.01));
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

  const boots = withInfo.filter((i: any) => i.info.tags?.includes('Boots') && !i.info.into).slice(0, 3);
  const coreItems = withInfo
    .filter((i: any) => !i.info.into && !i.info.tags?.includes('Boots') && (i.info.gold?.total ?? 0) >= 2000)
    .slice(0, 9);

  return (
    <main className="min-h-screen bg-neutral-950 text-white p-4 sm:p-8">
      <div className="max-w-4xl mx-auto bg-neutral-900 rounded-2xl p-6 sm:p-8 shadow-2xl border border-neutral-800">
        {/* ---------- Nagłówek ---------- */}
        <div className="flex flex-col sm:flex-row items-center gap-6 text-center sm:text-left">
          <ChampionIcon
            championId={champion.championId}
            name={champion.championName}
            size={96}
            className="rounded-2xl border border-neutral-700"
          />
          <div className="flex-1">
            <h1 className="text-4xl font-bold">{champion.championName}</h1>
            <div className="mt-3 flex flex-wrap gap-2 justify-center sm:justify-start text-sm">
              {POSITIONS.map((p) => (
                <a
                  key={p.key}
                  href={`./${p.slug}`}
                  className={`px-3 py-1 rounded-lg transition-colors ${
                    p.key === position
                      ? 'bg-blue-500/20 text-blue-400 border border-blue-500/50'
                      : 'bg-neutral-800 text-neutral-400 hover:bg-neutral-700'
                  }`}
                >
                  {p.label}
                </a>
              ))}
            </div>
          </div>
        </div>

        <div className="mt-8 grid grid-cols-2 sm:grid-cols-4 gap-3">
          <StatCard label="Win rate" value={pct(champion.winRate * 100)} className={wrColor(champion.winRate * 100)} />
          <StatCard label="Pick rate" value={pct(champion.pickRate * 100)} />
          <StatCard label="Ban rate" value={pct(champion.banRate * 100)} />
          <StatCard label="Gry" value={champion.games.toLocaleString('pl-PL')} />
        </div>

        {champion.games < 100 && (
          <p className="mt-4 text-sm text-yellow-400 bg-yellow-500/10 border border-yellow-500/30 rounded-lg px-4 py-2">
            Mało danych ({champion.games} gier). Statystyki mogą być niedokładne.
          </p>
        )}

        <Section title="Runy">
          {page && primaryTree && subTree ? (
            <>
              <div className="flex flex-wrap items-center gap-3 mb-5 text-sm">
                <span className="bg-neutral-800 px-3 py-1 rounded-lg">
                  <span className={`font-semibold ${wrColor(page.winRate)}`}>{pct(page.winRate)} WR</span>
                  <span className="text-neutral-400"> · {pct(page.pickRate)} PR · {page.games} gier</span>
                </span>
              </div>

              <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
                <RuneTree tree={primaryTree} rows={primaryTree.slots} selected={selected} stats={runeStats} keystone />
                <RuneTree tree={subTree} rows={subTree.slots.slice(1)} selected={selected} stats={runeStats} />

                {shards && (
                  <div className="bg-neutral-800/50 rounded-xl p-4">
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
                )}
              </div>
            </>
          ) : (
            <p className="text-neutral-500 text-sm">Brak danych o runach.</p>
          )}
        </Section>

        <Section title="Czary przywoływacza">
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
            {topSpells.map((s: any) => {
              const a = spellById(s.spell1Id);
              const b = spellById(s.spell2Id);
              return (
                <div key={`${s.spell1Id}-${s.spell2Id}`} className="bg-neutral-800/50 p-4 rounded-xl flex items-center justify-between">
                  <div className="flex gap-2">
                    {[a, b].map((sp, idx) =>
                      sp ? (
                        <img
                          key={idx}
                          src={spellIcon(version, sp.image.full)}
                          alt={sp.name}
                          title={sp.name}
                          width={40}
                          height={40}
                          className="rounded-lg"
                        />
                      ) : null,
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
            {coreItems.map((i: any) => (
              <ItemCard key={i.itemId} item={i} version={version} />
            ))}
          </div>

          {boots.length > 0 && (
            <>
              <h3 className="text-lg font-semibold mt-6 mb-3 text-neutral-300">Buty</h3>
              <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
                {boots.map((i: any) => (
                  <ItemCard key={i.itemId} item={i} version={version} />
                ))}
              </div>
            </>
          )}
        </Section>
      </div>
    </main>
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

function StatCard({ label, value, className = '' }: { label: string; value: string; className?: string }) {
  return (
    <div className="bg-neutral-800/50 rounded-xl p-4 text-center border border-neutral-800">
      <div className={`text-2xl font-bold ${className}`}>{value}</div>
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
    <div className="bg-neutral-800/50 rounded-xl p-4">
      <div className="flex items-center gap-2 mb-4">
        <img src={runeIcon(tree.icon)} alt={tree.name} width={24} height={24} />
        <span className="text-sm font-semibold text-neutral-300">{tree.name}</span>
      </div>
      <div className="space-y-3">
        {rows.map((row: any, rowIdx: number) => {
          const size = keystone && rowIdx === 0 ? 44 : 32;
          return (
            <div key={rowIdx} className="flex justify-around">
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
    <div className="bg-neutral-800/50 p-3 rounded-xl flex items-center gap-3 hover:bg-neutral-800 transition-colors">
      <img
        src={itemIcon(version, item.itemId)}
        alt={item.info.name}
        title={item.info.name}
        width={44}
        height={44}
        className="rounded-lg"
      />
      <div className="min-w-0 flex-1">
        <div className="text-sm font-medium truncate">{item.info.name}</div>
        <div className="text-xs text-neutral-400">
          <span className={wrColor(item.winRate)}>{pct(item.winRate)} WR</span> · {pct(item.pickRate)} PR · {item.games}
        </div>
      </div>
    </div>
  );
}