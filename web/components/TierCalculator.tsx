

export type Tier = "S+" | "S" | "A" | "B" | "C" | "D";

export interface TierConfig {
  weights: { winRate: number; pickRate: number; banRate: number };
  priorGames: number;
  minGames: number;
  minPickRate: number; 

  thresholds: [Tier, number][];
}

export const DEFAULT_TIER_CONFIG: TierConfig = {
  weights: { winRate: 0.6, pickRate: 0.2, banRate: 0.2 },
  priorGames: 2000,
  minGames: 50,
  minPickRate: 0.005,
  thresholds: [
    ["S+", 1.6], 
    ["S", 0.9],  
    ["A", 0.3],  
    ["B", -0.3], 
    ["C", -0.9],
  ],
};

export interface TierResult {
  score: number | null;
  tier: Tier | null;
  lowSample: boolean;
  adjustedWinRate: number | null;
}

interface StatRow {
  teamPosition?: string;
  winRate?: number | null;
  pickRate?: number | null;
  banRate?: number | null;
  games?: number | null;
}

const mean = (xs: number[]) => (xs.length ? xs.reduce((a, b) => a + b, 0) / xs.length : 0);

const std = (xs: number[], m = mean(xs)) =>
  xs.length > 1 ? Math.sqrt(xs.reduce((a, x) => a + (x - m) ** 2, 0) / xs.length) : 0;

const zScorer = (xs: number[]) => {
  const m = mean(xs);
  const s = std(xs, m);
  return (x: number) => (s === 0 ? 0 : (x - m) / s);
};

const scoreToTier = (score: number, thresholds: TierConfig["thresholds"]): Tier => {
  for (const [tier, min] of thresholds) {
    if (score >= min) return tier;
  }
  return "D";
};

export function computeTiers<T extends StatRow>(
  rows: T[],
  config: TierConfig = DEFAULT_TIER_CONFIG
): (T & TierResult)[] {
  const byRole = new Map<string, number[]>();
  rows.forEach((r, i) => {
    const key = r.teamPosition ?? "UNKNOWN";
    if (!byRole.has(key)) byRole.set(key, []);
    byRole.get(key)!.push(i);
  });

  const results: TierResult[] = rows.map(() => ({
    score: null,
    tier: null,
    lowSample: true,
    adjustedWinRate: null,
  }));

  for (const indices of byRole.values()) {
    const valid = indices.filter((i) => {
      const r = rows[i];
      return (r.games ?? 0) > 0 && r.winRate != null;
    });
    if (valid.length === 0) continue;

    const isEligible = (i: number) =>
      (rows[i].games ?? 0) >= config.minGames && (rows[i].pickRate ?? 0) >= config.minPickRate;


    let reference = valid.filter(isEligible);
    if (reference.length < 5) reference = valid;

    const totalGames = reference.reduce((a, i) => a + (rows[i].games ?? 0), 0);
    const totalWins = reference.reduce((a, i) => a + (rows[i].winRate ?? 0) * (rows[i].games ?? 0), 0);
    const roleAvgWR = totalGames > 0 ? totalWins / totalGames : 0.5;

    const adjWR = (i: number) => {
      const g = rows[i].games ?? 0;
      const w = (rows[i].winRate ?? 0) * g;
      return (w + config.priorGames * roleAvgWR) / (g + config.priorGames);
    };
    const pickF = (i: number) => Math.sqrt(rows[i].pickRate ?? 0);
    const banF = (i: number) => Math.sqrt(rows[i].banRate ?? 0);

    const zWR = zScorer(reference.map(adjWR));
    const zPick = zScorer(reference.map(pickF));
    const zBan = zScorer(reference.map(banF));

    const { weights } = config;
    const raw = (i: number) =>
      weights.winRate * zWR(adjWR(i)) +
      weights.pickRate * zPick(pickF(i)) +
      weights.banRate * zBan(banF(i));

    const zFinal = zScorer(reference.map(raw));

    for (const i of valid) {
      const score = zFinal(raw(i));
      const lowSample = !isEligible(i);
      results[i] = {
        score: lowSample ? null : score,
        tier: lowSample ? null : scoreToTier(score, config.thresholds),
        lowSample,
        adjustedWinRate: adjWR(i),
      };
    }
  }

  return rows.map((r, i) => ({ ...r, ...results[i] }));
}