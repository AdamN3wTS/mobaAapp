'use client';

import { useState } from 'react';

const CDRAGON =
  'https://raw.communitydragon.org/latest/plugins/rcp-be-lol-game-data/global/default/v1/champion-icons';

interface ChampionIconProps {
  championId?: number;
  name?: string;
  size?: number;
  className?: string;
}

export default function ChampionIcon({ championId, name, size = 32, className = '' }: ChampionIconProps) {
  const [error, setError] = useState(false);

  return (
    <div
      style={{ width: size, height: size }}
      className={`rounded-lg overflow-hidden bg-neutral-700 shrink-0 ${className}`}
      title={name}
    >
      {championId && !error && (
        <img
          src={`${CDRAGON}/${championId}.png`}
          alt={name ?? ''}
          width={size}
          height={size}
          loading="lazy"
          onError={() => setError(true)}
          className="w-full h-full object-cover scale-110"
        />
      )}
    </div>
  );
}