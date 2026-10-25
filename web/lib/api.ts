const url = "http://localhost:5074/api"

export async function getStats() {
  if (!url) return null;
  const req = await fetch(`${url}/stats/get`, { cache: "no-store" });
  const res = await req.json();
  return res.data ?? res;
}

export async function getPlayer(gameName: string, tagLine: string) {
  if (!url) return null;
  const req = await fetch(`${url}/player/get/${gameName}/${tagLine}`, { cache: "no-store" });
  const res = await req.json();
  return res.data ?? res;
}

export async function getMatches(puuid: string, page = 1, pageSize = 20) {
  if (!url) return null;
  const req = await fetch(`${url}/player/${puuid}/matches?page=${page}&pageSize=${pageSize}`, { cache: "no-store" });
  return req.json();
}