'use client';

import { useEffect, useMemo, useRef, useState } from 'react';

export interface TrendPoint {
  patch: string;
  createdAt: string;
  games: number;
  winRate: number; 
  pickRate: number;
  banRate: number;
}

interface WinRateTrendProps {
  currentPatch?: string;
  currentPatchHistory?: TrendPoint[];
  previousPatches?: TrendPoint[];
}

type View = 'patch' | 'patches';

interface ChartPoint {
  x: number; 
  wr: number; 
  axisLabel: string;
  tipTitle: string;
  games: number;
  isCurrent?: boolean;
}

const HEIGHT = 220;
const M = { top: 14, right: 18, bottom: 28, left: 44 };
const LINE = '#60a5fa'; 

const fmtDate = (iso: string) => new Date(iso).toLocaleDateString('pl-PL', { day: '2-digit', month: '2-digit' });
const fmtDateTime = (iso: string) =>
  new Date(iso).toLocaleString('pl-PL', { day: '2-digit', month: '2-digit', hour: '2-digit', minute: '2-digit' });

const niceStep = (range: number) => [0.5, 1, 2, 5, 10].find((s) => range / s <= 5) ?? 10;

export default function WinRateTrend({ currentPatch = '', currentPatchHistory = [], previousPatches = [] }: WinRateTrendProps) {
  const [view, setView] = useState<View>('patch');
  const [hover, setHover] = useState<number | null>(null);
  const wrapRef = useRef<HTMLDivElement>(null);
  const [width, setWidth] = useState(720);

  useEffect(() => {
    const el = wrapRef.current;
    if (!el) return;
    const ro = new ResizeObserver(([e]) => setWidth(Math.max(280, e.contentRect.width)));
    ro.observe(el);
    return () => ro.disconnect();
  }, []);

  const latestCurrent = currentPatchHistory.at(-1);

  const inPatchDelta =
    currentPatchHistory.length >= 2 ? (latestCurrent!.winRate - currentPatchHistory[0].winRate) * 100 : null;
  const prevPatch = previousPatches.at(-1);
  const vsPrevDelta = latestCurrent && prevPatch ? (latestCurrent.winRate - prevPatch.winRate) * 100 : null;

  const points: ChartPoint[] = useMemo(() => {
    if (view === 'patch') {
      return currentPatchHistory.map((p) => ({
        x: new Date(p.createdAt).getTime(),
        wr: p.winRate * 100,
        axisLabel: fmtDate(p.createdAt),
        tipTitle: fmtDateTime(p.createdAt),
        games: p.games,
      }));
    }
    const list = latestCurrent ? [...previousPatches, latestCurrent] : previousPatches;
    return list.map((p, i) => ({
      x: i,
      wr: p.winRate * 100,
      axisLabel: p.patch,
      tipTitle: `Patch ${p.patch}${i === list.length - 1 && latestCurrent ? ' (obecny)' : ''}`,
      games: p.games,
      isCurrent: i === list.length - 1 && Boolean(latestCurrent),
    }));
  }, [view, currentPatchHistory, previousPatches, latestCurrent]);

  const innerW = width - M.left - M.right;
  const innerH = HEIGHT - M.top - M.bottom;

  const wrs = points.map((p) => p.wr);
  const rawMin = Math.min(50, ...wrs);
  const rawMax = Math.max(50, ...wrs);
  const step = niceStep(Math.max(1, rawMax - rawMin));
  const yMin = Math.floor((rawMin - 0.25) / step) * step;
  const yMax = Math.ceil((rawMax + 0.25) / step) * step;
  const yTicks: number[] = [];
  for (let v = yMin; v <= yMax + 1e-9; v += step) yTicks.push(Number(v.toFixed(2)));

  const xs = points.map((p) => p.x);
  const xMin = Math.min(...xs);
  const xMax = Math.max(...xs);
  const sx = (x: number) => M.left + (xMax === xMin ? innerW / 2 : ((x - xMin) / (xMax - xMin)) * innerW);
  const sy = (wr: number) => M.top + (1 - (wr - yMin) / (yMax - yMin)) * innerH;

  const path = points.map((p, i) => `${i ? 'L' : 'M'}${sx(p.x).toFixed(1)},${sy(p.wr).toFixed(1)}`).join(' ');

  const maxXLabels = Math.max(2, Math.floor(innerW / 90));
  const labelEvery = Math.max(1, Math.ceil(points.length / maxXLabels));

  const onMove = (e: React.MouseEvent<SVGSVGElement>) => {
    if (!points.length) return;
    const rect = e.currentTarget.getBoundingClientRect();
    const mx = e.clientX - rect.left;
    let best = 0;
    points.forEach((p, i) => {
      if (Math.abs(sx(p.x) - mx) < Math.abs(sx(points[best].x) - mx)) best = i;
    });
    setHover(best);
  };

  const hp = hover != null ? points[hover] : null;
  const enough = points.length >= 2;

  return (
    <div>
      

      <div className="flex gap-2 mb-3 text-sm">
        {(
          [
            ['patch', 'Ten patch'],
            ['patches', 'Ostatnie patche'],
          ] as [View, string][]
        ).map(([v, label]) => (
          <button
            key={v}
            type="button"
            onClick={() => {
              setView(v);
              setHover(null);
            }}
            className={`px-3 py-1 rounded-lg border transition-colors ${
              view === v
                ? 'bg-blue-500/20 text-blue-400 border-blue-500/50'
                : 'bg-neutral-800 text-neutral-400 border-transparent hover:bg-neutral-700'
            }`}
          >
            {label}
          </button>
        ))}
      </div>

      <div ref={wrapRef} className="relative rounded-xl bg-neutral-800/30 border border-neutral-800" style={{ height: HEIGHT }}>
        {!enough ? (
          <div className="flex h-full items-center justify-center text-sm text-neutral-600">
            {view === 'patch' ? 'Za mało pomiarów w tym patchu' : 'Brak danych z poprzednich patchy'}
          </div>
        ) : (
          <>
            <svg
              width={width}
              height={HEIGHT}
              className="block"
              onMouseMove={onMove}
              onMouseLeave={() => setHover(null)}
              role="img"
              aria-label="Win rate w czasie"
            >
              {yTicks.map((t) => (
                <g key={t}>
                  <line
                    x1={M.left}
                    x2={width - M.right}
                    y1={sy(t)}
                    y2={sy(t)}
                    stroke={t === 50 ? '#525252' : '#262626'}
                    strokeDasharray={t === 50 ? '4 4' : undefined}
                    strokeWidth={1}
                  />
                  <text x={M.left - 8} y={sy(t)} dy="0.32em" textAnchor="end" fontSize={11} fill="#737373">
                    {t % 1 ? t.toFixed(1) : t}%
                  </text>
                </g>
              ))}

              {points.map((p, i) =>
                i % labelEvery === 0 || i === points.length - 1 ? (
                  <text
                    key={i}
                    x={sx(p.x)}
                    y={HEIGHT - 8}
                    textAnchor={i === 0 ? 'start' : i === points.length - 1 ? 'end' : 'middle'}
                    fontSize={11}
                    fill={p.isCurrent ? '#d4d4d4' : '#737373'}
                    fontWeight={p.isCurrent ? 600 : 400}
                  >
                    
                  </text>
                ) : null,
              )}

              <path d={path} fill="none" stroke={LINE} strokeWidth={2} strokeLinejoin="round" strokeLinecap="round" />

              {points.map((p, i) =>
                view === 'patches' || i === points.length - 1 ? (
                  <circle key={i} cx={sx(p.x)} cy={sy(p.wr)} r={4} fill={LINE} stroke="#171717" strokeWidth={2} />
                ) : null,
              )}

              {hp && (
                <g pointerEvents="none">
                  <line x1={sx(hp.x)} x2={sx(hp.x)} y1={M.top} y2={HEIGHT - M.bottom} stroke="#525252" strokeWidth={1} />
                  <circle cx={sx(hp.x)} cy={sy(hp.wr)} r={5} fill={LINE} stroke="#171717" strokeWidth={2} />
                </g>
              )}
            </svg>

            {hp && (
              <div
                className="pointer-events-none absolute z-10 rounded-lg border border-neutral-700 bg-neutral-900/95 px-3 py-2 text-xs shadow-xl"
                style={{
                  left: Math.min(Math.max(sx(hp.x), 80), width - 80),
                  top: Math.max(sy(hp.wr) - 64, 4),
                  transform: 'translateX(-50%)',
                }}
              >
                <div className="text-neutral-400">{hp.tipTitle}</div>
                <div className="text-sm font-semibold text-neutral-100 tabular-nums">{hp.wr.toFixed(2)}% WR</div>
                <div className="text-neutral-500 tabular-nums">{hp.games.toLocaleString('pl-PL')} gier</div>
              </div>
            )}
          </>
        )}
      </div>
    </div>
  );
}

function DeltaTile({ label, value }: { label: string; value: number | null }) {
  const flat = value == null || Math.abs(value) < 0.05;
  const up = !flat && value! > 0;
  const color = flat ? 'text-neutral-300' : up ? 'text-blue-400' : 'text-red-400';
  const arrow = flat ? '→' : up ? '▲' : '▼';

  return (
    <div className="bg-neutral-800/50 rounded-xl p-4 border border-neutral-800 h-[84px]">
      <div className={`text-2xl font-bold tabular-nums ${value == null ? 'text-neutral-600' : color}`}>
        {value == null ? '-' : `${arrow} ${value > 0 ? '+' : ''}${value.toFixed(2)} pp`}
      </div>
      <div className="text-xs text-neutral-400 mt-1 uppercase tracking-wide truncate">{label}</div>
    </div>
  );
}