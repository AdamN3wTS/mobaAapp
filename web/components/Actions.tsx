'use server';

import { getMatches } from '@/lib/api';
import type { MatchSummary } from '@/types/summoner';

export interface MatchPage {
  matches: MatchSummary[];
  totalItems: number | null;
}

export async function loadMoreMatches(puuid: string, page: number, pageSize: number): Promise<MatchPage> {
  const res = await getMatches(puuid, page, pageSize);
  return {
    matches: res?.matches ?? [],
    totalItems: res?.totalItems ?? null,
  };
}