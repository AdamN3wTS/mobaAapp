"use client";

import React, { useState } from "react";

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

interface TierListClientProps {
  championsData: any; 
  version: string;
}

export default function TierListClient({ championsData, version }: TierListClientProps) {
  const [activeRole, setActiveRole] = useState("All Roles");


  const allStats = Array.isArray(championsData?.statsPositions)
    ? championsData.statsPositions.flatMap((pos: any) => 
        Array.isArray(pos.stats) ? pos.stats.map((stat: any) => ({
          ...stat,
          teamPosition: pos.teamPosition
        })) : []
      )
    : [];


  const bans = Array.isArray(championsData?.statsBans) ? championsData.statsBans : [];


  const mergedData = allStats.map(stat => {
    const banInfo = bans.find((b: any) => b.championId === stat.championId);
    return {
      ...stat,
      banRate: banInfo ? banInfo.banRate : null
    };
  });


  const filteredData = activeRole === "All Roles" 
    ? mergedData 
    : mergedData.filter(champ => champ.teamPosition === roleMapping[activeRole]);


  const sortedData = [...filteredData].sort((a, b) => (b.winRate || 0) - (a.winRate || 0));

  return (
    <div className="min-h-screen bg-[#0b0b13] text-gray-200 p-6 font-sans">
      <div className="max-w-7xl mx-auto">
        
        {}
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

        {}
        <div className="bg-[#151521] rounded-lg border border-[#2c2c3e] overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full text-sm text-left">
              <thead className="text-xs text-gray-400 uppercase bg-[#0b0b13] border-b border-[#2c2c3e]">
                <tr>
                  <th className="px-6 py-4 font-semibold">#</th>
                  <th className="px-6 py-4 font-semibold">Champion</th>
                  <th className="px-6 py-4 font-semibold text-center">Tier</th>
                  <th className="px-6 py-4 font-semibold text-right">Win Rate</th>
                  <th className="px-6 py-4 font-semibold text-right">Pick Rate</th>
                  <th className="px-6 py-4 font-semibold text-right">Ban Rate</th>
                  <th className="px-6 py-4 font-semibold text-right">Matches</th>
                </tr>
              </thead>
              <tbody>
                {sortedData.map((champ: any, index: number) => {
                  const avatarUrl = `https://ddragon.leagueoflegends.com/cdn/${version}/img/champion/${champ.championName}.png`;
                  

                  const formattedPosition = champ.teamPosition 
                    ? champ.teamPosition.charAt(0) + champ.teamPosition.slice(1).toLowerCase() 
                    : "";

                  return (
                    <tr key={`${champ.championId}-${champ.teamPosition}-${index}`} className="border-b border-[#2c2c3e] hover:bg-[#202030] transition-colors">
                      <td className="px-6 py-4 text-gray-400 font-medium">
                        {index + 1}
                      </td>
                      <td className="px-6 py-4">
                        <div className="flex items-center gap-3">
                          <img 
                            src={avatarUrl} 
                            alt={champ.championName} 
                            className="w-10 h-10 rounded shadow-sm border border-gray-700"
                            onError={(e) => {
                              (e.target as HTMLImageElement).src = "https://raw.communitydragon.org/latest/plugins/rcp-be-lol-game-data/global/default/v1/profile-icons/0.jpg";
                            }}
                          />
                          <div>
                            <div className="font-bold text-base text-white">{champ.championName}</div>
                            {}
                            <div className="text-xs text-gray-500">{formattedPosition}</div>
                          </div>
                        </div>
                      </td>
                      <td className="px-6 py-4 text-center">
                        {}
                        <span className="font-black text-lg text-gray-500">-</span>
                      </td>
                      <td className="px-6 py-4 text-right font-medium text-blue-400">
                        {champ.winRate ? `${(champ.winRate * 100).toFixed(2)}%` : "-"}
                      </td>
                      <td className="px-6 py-4 text-right text-gray-300">
                        {champ.pickRate ? `${(champ.pickRate * 100).toFixed(2)}%` : "-"}
                      </td>
                      <td className="px-6 py-4 text-right text-gray-300">
                        {}
                        {champ.banRate ? `${(champ.banRate * 100).toFixed(2)}%` : "-"}
                      </td>
                      <td className="px-6 py-4 text-right text-gray-500">
                        {champ.games || "-"}
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