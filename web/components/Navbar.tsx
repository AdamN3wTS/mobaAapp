'use client';

import { useEffect, useRef, useState } from 'react';
import Link from 'next/link';
import { usePathname, useRouter } from 'next/navigation';
import { FaSearch } from 'react-icons/fa';
import ChampionIcon from '@/components/ChampionIcon';
import { searchChampions } from '@/lib/api';

interface ChampionHit {
  championId: number;
  championName: string;
  mainPosition: string;
}

const NAV_LINKS = [
  { href: '/', label: 'Home' },
  { href: '/champion', label: 'Champions' },
];

export default function Navbar() {
  const router = useRouter();
  const pathname = usePathname();
  const [query, setQuery] = useState('');
  const [fetchedHits, setHits] = useState<ChampionHit[]>([]);
  const [open, setOpen] = useState(false);
  const boxRef = useRef<HTMLFormElement>(null);

  const trimmed = query.trim();
  const isSummonerSearch = trimmed.includes('#');
  const hits = trimmed && !isSummonerSearch ? fetchedHits : [];

  useEffect(() => {
    if (!trimmed || isSummonerSearch) return;
    const controller = new AbortController();
    const timer = setTimeout(async () => {
      try {
        const data = await searchChampions(trimmed, controller.signal);
        setHits(data ?? []);
      } catch {
      }
    }, 200);
    return () => {
      clearTimeout(timer);
      controller.abort();
    };
  }, [trimmed, isSummonerSearch]);

  useEffect(() => {
    const onClick = (e: MouseEvent) => {
      if (boxRef.current && !boxRef.current.contains(e.target as Node)) setOpen(false);
    };
    document.addEventListener('mousedown', onClick);
    return () => document.removeEventListener('mousedown', onClick);
  }, []);

  const goToChampion = (name: string) => {
    setOpen(false);
    setQuery('');
    router.push(`/champion/${encodeURIComponent(name.toLowerCase())}`);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!trimmed) return;

    if (isSummonerSearch) {
      const [name, ...rest] = trimmed.split('#');
      const tag = rest.join('#').trim();
      if (!name.trim() || !tag) return;
      setOpen(false);
      setQuery('');
      router.push(`/summoner/${encodeURIComponent(name.trim())}/${encodeURIComponent(tag)}`);
      return;
    }

    if (hits.length > 0) {
      goToChampion(hits[0].championName);
      return;
    }

    try {
      const data = await searchChampions(trimmed);
      if (data && data.length > 0) goToChampion(data[0].championName);
    } catch {
      // brak API
    }
  };

  return (
    <header className="sticky top-0 z-50 border-b border-neutral-800 bg-neutral-950/90 backdrop-blur">
      <nav className="mx-auto flex h-14 max-w-7xl items-center gap-4 px-4 sm:px-6">
        <Link href="/" className="text-xl font-extrabold tracking-tight text-blue-500">
          MobaApp
        </Link>

        <ul className="flex items-center gap-1">
          {NAV_LINKS.map((link) => {
            const active = link.href === '/' ? pathname === '/' : pathname.startsWith(link.href);
            return (
              <li key={link.href}>
                <Link
                  href={link.href}
                  className={`rounded-lg px-3 py-1.5 text-sm font-medium transition-colors ${
                    active ? 'bg-neutral-800 text-white' : 'text-neutral-400 hover:text-white'
                  }`}
                >
                  {link.label}
                </Link>
              </li>
            );
          })}
        </ul>

        <form ref={boxRef} onSubmit={handleSubmit} className="relative ml-auto w-full max-w-sm">
          <div className="flex items-center gap-2 rounded-xl border border-neutral-800 bg-neutral-900 px-3 focus-within:ring-2 focus-within:ring-blue-500">
            <FaSearch className="shrink-0 text-sm text-neutral-500" />
            <input
              type="text"
              value={query}
              onChange={(e) => {
                setQuery(e.target.value);
                setOpen(true);
              }}
              onFocus={() => setOpen(true)}
              placeholder="Champion lub Nick#TAG"
              aria-label="Wyszukaj championa lub gracza"
              className="w-full bg-transparent py-2 text-sm text-white placeholder-neutral-500 focus:outline-none"
            />
          </div>

          {open && trimmed && (
            <div className="absolute left-0 right-0 top-full mt-2 overflow-hidden rounded-xl border border-neutral-800 bg-neutral-900 shadow-xl">
              {isSummonerSearch ? (
                <p className="px-4 py-3 text-sm text-neutral-400">
                  Enter – szukaj gracza <span className="text-white">{trimmed}</span>
                </p>
              ) : hits.length > 0 ? (
                <ul>
                  {hits.map((hit) => (
                    <li key={hit.championId}>
                      <button
                        type="button"
                        onClick={() => goToChampion(hit.championName)}
                        className="flex w-full items-center gap-3 px-4 py-2 text-left text-sm text-white hover:bg-neutral-800"
                      >
                        <ChampionIcon championId={hit.championId} name={hit.championName} size={28} />
                        {hit.championName}
                      </button>
                    </li>
                  ))}
                </ul>
              ) : (
                <p className="px-4 py-3 text-sm text-neutral-500">Brak championów. Dla gracza wpisz Nick#TAG.</p>
              )}
            </div>
          )}
        </form>
      </nav>
    </header>
  );
}
