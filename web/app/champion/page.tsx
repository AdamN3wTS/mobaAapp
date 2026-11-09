import { Suspense } from "react";
import { getStats } from "@/lib/api";
import { getDdVersion } from "@/lib/ddragon";
import TierListClient from "./TierListClient";

async function TierListWrapper() {
  const statsData = await getStats();
  const patchVersion = await getDdVersion();

  return <TierListClient championsData={statsData} version={patchVersion} />;
}

export default function ChampionPage() {
  return (
    <Suspense 
      fallback={
        <div className="min-h-screen bg-[#0b0b13] flex items-center justify-center text-white">
          Ładowanie statystyk...
        </div>
      }
    >
      <TierListWrapper />
    </Suspense>
  );
}