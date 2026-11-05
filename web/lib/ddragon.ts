const DD = 'https://ddragon.leagueoflegends.com';
const CACHE = { next: { revalidate: 60 * 60 * 24 } }; 

export async function getDdVersion(): Promise<string> {
  const res = await fetch(`${DD}/api/versions.json`, CACHE);
  const versions: string[] = await res.json();
  return versions[0];
}

export async function getDdData(version: string, locale = 'pl_PL') {
  const [runes, items, spells] = await Promise.all([
    fetch(`${DD}/cdn/${version}/data/${locale}/runesReforged.json`, CACHE).then((r) => r.json()),
    fetch(`${DD}/cdn/${version}/data/${locale}/item.json`, CACHE).then((r) => r.json()),
    fetch(`${DD}/cdn/${version}/data/${locale}/summoner.json`, CACHE).then((r) => r.json()),
  ]);

  return {
    runes: runes as any[],
    items: items.data as Record<string, any>,
    spells: Object.values(spells.data) as any[],
  };
}

export const runeIcon = (icon: string) => `${DD}/cdn/img/${icon}`;
export const itemIcon = (version: string, id: number) => `${DD}/cdn/${version}/img/item/${id}.png`;
export const spellIcon = (version: string, file: string) => `${DD}/cdn/${version}/img/spell/${file}`;

export const SHARDS: Record<number, string> = {
  5008: 'Siła adaptacyjna',
  5005: 'Prędkość ataku',
  5007: 'Przyspieszenie umiejętności',
  5010: 'Prędkość ruchu',
  5001: 'Zdrowie (skalowane)',
  5011: 'Zdrowie',
  5013: 'Nieustępliwość i odp. na spowolnienia',
  5002: 'Pancerz',
  5003: 'Odporność na magię',
};