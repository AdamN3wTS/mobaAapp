'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { FaSearch } from 'react-icons/fa';

export default function LandingPage() {
  const router = useRouter();
  const [nickname, setNickname] = useState('');
  const [tagline, setTagline] = useState('');

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    if (nickname && tagline) {
      const cleanTagline = tagline.replace('#', '');
      router.push(`/summoner/${encodeURIComponent(nickname)}/${encodeURIComponent(cleanTagline)}`);
    }
  };

  return (
    <main className="min-h-screen bg-neutral-950 text-white flex flex-col items-center justify-center p-4">
      <div className="max-w-2xl w-full text-center space-y-8">
        <h1 className="text-5xl font-extrabold text-blue-500 tracking-tight">MobaApp</h1>
        <p className="text-neutral-400 text-lg">Wyszukaj statystyki gracza i historię meczów.</p>
        
        <form onSubmit={handleSearch} className="flex flex-col sm:flex-row gap-3 justify-center bg-neutral-900 p-4 rounded-2xl shadow-xl border border-neutral-800">
          <input 
            type="text" 
            placeholder="Game Name (np. Faker)" 
            value={nickname}
            onChange={(e) => setNickname(e.target.value)}
            className="flex-1 px-4 py-3 bg-neutral-800 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500 text-white placeholder-neutral-500"
            required
          />
          <input 
            type="text" 
            placeholder="#Tag (np. KR1)" 
            value={tagline}
            onChange={(e) => setTagline(e.target.value)}
            className="w-full sm:w-32 px-4 py-3 bg-neutral-800 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500 text-white placeholder-neutral-500"
            required
          />
          <button 
            type="submit" 
            className="bg-blue-600 hover:bg-blue-500 transition-colors px-6 py-3 rounded-xl font-semibold flex items-center justify-center gap-2"
          >
            <FaSearch />
            .GG
          </button>
        </form>
      </div>
    </main>
  );
}