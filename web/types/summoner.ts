export interface RankedInfo {
  queueType: string;
  tier: string;
  rank: string;
  leaguePoints: number;
  wins: number;
  losses: number;
}

export interface PlayerProfile {
  puuid: string;
  summonerLevel: number;
  ranks: RankedInfo[];
}

export interface MatchItem {
  itemId: number;
}

export interface MatchParticipant {
  isCurrentPlayer: boolean;
  gameName: string;
  tagLine: string;
  championId: number;
  championName: string;
  teamPosition: string;
  teamId: number;
  isWin: boolean;
  kills: number;
  deaths: number;
  assists: number;
  summoner1Id: number;
  summoner2Id: number;
  items: MatchItem[];
}

export interface MatchSummary {
  matchId: string;
  isWin: boolean;
  gameDurationInSeconds: number;
  queueType: string;
  patch: string;
  gameCreationTimestamp: string;
  championName: string;
  championId: number;
  stats: {
    kills: number;
    deaths: number;
    assists: number;
    totalMinionsKilled: number;
  };
  participants: MatchParticipant[];
}

export interface MatchHistory {
  page: number;
  pageSize: number;
  totalItems: number;
  matches: MatchSummary[];
}