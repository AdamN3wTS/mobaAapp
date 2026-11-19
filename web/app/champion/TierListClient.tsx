"use client";

import React, { useMemo, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { computeTiers } from "../../components/TierCalculator"

const getTierColor = (tier: string) => {
  switch (tier) {
    case "S+": return "text-orange-400";
    case "S": return "text-blue-400";
    case "A": return "text-green-400";
    case "B": return "text-yellow-400";
    case "C": return "text-gray-400";
    case "D": return "text-red-500";
    default: return "text-white";
  }
};

const ROLES = ["All Roles", "Top", "Jungle", "Mid", "ADC", "Support"];

const roleMapping: Record<string, string> = {
  "Top": "TOP",
  "Jungle": "JUNGLE",
  "Mid": "MIDDLE",
  "ADC": "BOTTOM",
  "Support": "UTILITY"
};

const roleToSlug: Record<string, string> = {
  TOP: "top",
  JUNGLE: "jungle",
  MIDDLE: "mid",
  BOTTOM: "adc",
  UTILITY: "support"
};

const getChampionHref = (champ: any) => {
  const name = encodeURIComponent(String(champ.championName).toLowerCase());
  const role = roleToSlug[champ.teamPosition] ?? String(champ.teamPosition || "").toLowerCase();
  return `/champion/${name}/${role}`;
};

type SortKey = "championName" | "score" | "winRate" | "pickRate" | "banRate" | "games";
type SortDir = "asc" | "desc";

const COLUMNS: { key: SortKey; label: string; align: "left" | "center" | "right" }[] = [
  { key: "championName", label: "Champion", align: "left" },
  { key: "score", label: "Tier", align: "center" },
  { key: "winRate", label: "Win Rate", align: "right" },
  { key: "pickRate", label: "Pick Rate", align: "right" },
  { key: "banRate", label: "Ban Rate", align: "right" },
  { key: "games", label: "Matches", align: "right" }
];

interface TierListClientProps {
  championsData: any;
  version: string;
}

export default function TierListClient({ championsData, version }: TierListClientProps) {
  const router = useRouter();
  const [activeRole, setActiveRole] = useState("All Roles");
  const [sortKey, setSortKey] = useState<SortKey>("score");
  const [sortDir, setSortDir] = useState<SortDir>("desc");

  const mergedData = useMemo(() => {
    const allStats = Array.isArray(championsData?.statsPositions)
      ? championsData.statsPositions.flatMap((pos: any) =>
          Array.isArray(pos.stats)
            ? pos.stats.map((stat: any) => ({ ...stat, teamPosition: pos.teamPosition }))
            : []
        )
      : [];

    const bans = Array.isArray(championsData?.statsBans) ? championsData.statsBans : [];
    const banMap = new Map<number | string, number>(
      bans.map((b: any) => [b.championId, b.banRate])
    );

    const merged = allStats.map((stat: any) => ({
      ...stat,
      banRate: banMap.has(stat.championId) ? banMap.get(stat.championId) : null
    }));

    return computeTiers(merged);
  }, [championsData]);

  const sortedData = useMemo(() => {
    const filtered =
      activeRole === "All Roles"
        ? mergedData
        : mergedData.filter((c: any) => c.teamPosition === roleMapping[activeRole]);

    const dir = sortDir === "asc" ? 1 : -1;

    return [...filtered].sort((a: any, b: any) => {
      const va = a[sortKey];
      const vb = b[sortKey];

      const aEmpty = va === null || va === undefined;
      const bEmpty = vb === null || vb === undefined;
      if (aEmpty && bEmpty) return 0;
      if (aEmpty) return 1;
      if (bEmpty) return -1;

      if (sortKey === "championName") {
        return String(va).localeCompare(String(vb)) * dir;
      }
      return (Number(va) - Number(vb)) * dir;
    });
  }, [mergedData, activeRole, sortKey, sortDir]);

  const handleSort = (key: SortKey) => {
    if (key === sortKey) {
      setSortDir((d) => (d === "asc" ? "desc" : "asc"));
    } else {
      setSortKey(key);
      setSortDir(key === "championName" ? "asc" : "desc");
    }
  };

  const SortIcon = ({ col }: { col: SortKey }) => {
    if (col !== sortKey) return <span className="text-gray-600 ml-1">↕</span>;
    return <span className="text-blue-400 ml-1">{sortDir === "asc" ? "▲" : "▼"}</span>;
  };

  const renderHeader = (key: SortKey) => {
    const col = COLUMNS.find((c) => c.key === key)!;
    return (
      <th
        key={key}
        onClick={() => handleSort(key)}
        className={`px-6 py-4 font-semibold cursor-pointer select-none hover:text-white transition-colors ${
          col.align === "right" ? "text-right" : col.align === "center" ? "text-center" : "text-left"
        } ${sortKey === key ? "text-white" : ""}`}
      >
        <span className="inline-flex items-center">
          {col.label}
          <SortIcon col={key} />
        </span>
      </th>
    );
  };

  return (
    <div className="min-h-screen bg-neutral-950 text-gray-200 p-6 font-sans">
      <div className="max-w-7xl mx-auto">

        <div className="mb-8">
          <h1 className="text-3xl font-bold text-white mb-6">LoL Tier List</h1>

          <div className="flex flex-wrap items-center gap-4 bg-[#151521] p-4 rounded-lg border border-[#2c2c3e]">
            <div className="flex space-x-2">
              {ROLES.map((role) => (
                <button
                  key={role}
                  onClick={() => setActiveRole(role)}
                  className={`px-4 py-2 rounded text-sm font-medium transition-colors ${
                    activeRole === role
                      ? "bg-blue-600 text-white"
                      : "bg-[#202030] text-gray-400 hover:bg-[#2c2c3e] hover:text-white"
                  }`}
                >
                  {role}
                </button>
              ))}
            </div>

            <div className="ml-auto flex gap-3 text-sm text-gray-400 items-center">
              <span>Patch: <strong className="text-white">{version}</strong></span>
            </div>
          </div>
        </div>

        <div className="bg-[#151521] rounded-lg border border-[#2c2c3e] overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full text-sm text-left">
              <thead className="text-xs text-gray-400 uppercase bg-[#0b0b13] border-b border-[#2c2c3e]">
                <tr>
                  <th className="px-6 py-4 font-semibold">#</th>
                  {renderHeader("championName")}
                  {renderHeader("score")}
                  {renderHeader("winRate")}
                  {renderHeader("pickRate")}
                  {renderHeader("banRate")}
                  {renderHeader("games")}
                </tr>
              </thead>
              <tbody>
                {sortedData.map((champ: any, index: number) => {
                  const avatarUrl = `https://ddragon.leagueoflegends.com/cdn/${version}/img/champion/${champ.championName}.png`;
                  const href = getChampionHref(champ);

                  const formattedPosition = champ.teamPosition
                    ? champ.teamPosition.charAt(0) + champ.teamPosition.slice(1).toLowerCase()
                    : "";

                  return (
                    <tr
                      key={`${champ.championId}-${champ.teamPosition}`}
                      onClick={() => router.push(href)}
                      className="border-b border-[#2c2c3e] hover:bg-[#202030] transition-colors cursor-pointer"
                    >
                      <td className="px-6 py-4 text-gray-400 font-medium">
                        {index + 1}
                      </td>
                      <td className="px-6 py-4">
                        <Link
                          href={href}
                          onClick={(e) => e.stopPropagation()}
                          className="flex items-center gap-3 group"
                        >
                          <img
                            src={avatarUrl}
                            alt={champ.championName}
                            className="w-10 h-10 rounded shadow-sm border border-gray-700"
                            onError={(e) => {
                              (e.target as HTMLImageElement).src = "https://raw.communitydragon.org/latest/plugins/rcp-be-lol-game-data/global/default/v1/profile-icons/0.jpg";
                            }}
                          />
                          <div>
                            <div className="font-bold text-base text-white group-hover:text-blue-400 transition-colors">
                              {champ.championName}
                            </div>
                            <div className="text-xs text-gray-500">{formattedPosition}</div>
                          </div>
                        </Link>
                      </td>
                      <td className="px-6 py-4 text-center">
                        {champ.tier ? (
                          <span
                            className={`font-black text-lg ${getTierColor(champ.tier)}`}
                            title={`Wynik: ${champ.score.toFixed(2)}`}
                          >
                            {champ.tier}
                          </span>
                        ) : (
                          <span
                            className="font-black text-lg text-gray-600"
                            title="Za mało gier, żeby wiarygodnie ocenić"
                          >
                            ?
                          </span>
                        )}
                      </td>
                      <td className="px-6 py-4 text-right font-medium text-blue-400">
                        {champ.winRate ? `${(champ.winRate * 100).toFixed(2)}%` : "-"}
                      </td>
                      <td className="px-6 py-4 text-right text-gray-300">
                        {champ.pickRate ? `${(champ.pickRate * 100).toFixed(2)}%` : "-"}
                      </td>
                      <td className="px-6 py-4 text-right text-gray-300">
                        {champ.banRate ? `${(champ.banRate * 100).toFixed(2)}%` : "-"}
                      </td>
                      <td className="px-6 py-4 text-right text-gray-500">
                        {champ.games ? champ.games.toLocaleString() : "-"}
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  );
}