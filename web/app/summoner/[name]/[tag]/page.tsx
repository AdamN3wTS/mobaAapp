import { FaUserCircle } from 'react-icons/fa';

interface SummonerPageProps {
  params: Promise<{
    nickname: string;
    tagline: string;
  }>;
}

export default async function SummonerPage({ params }: SummonerPageProps) {
  const resolvedParams = await params;
  const nickname = decodeURIComponent(resolvedParams.nickname);
  const tagline = decodeURIComponent(resolvedParams.tagline);

  return (
    <main className="min-h-screen bg-neutral-950 text-white p-4 sm:p-8">
      <div className="max-w-4xl mx-auto bg-neutral-900 rounded-2xl p-6 sm:p-8 shadow-2xl border border-neutral-800">
        <div className="flex flex-col sm:flex-row items-center gap-6 text-center sm:text-left">
          <div className="w-24 h-24 bg-neutral-800 rounded-2xl flex items-center justify-center text-5xl text-blue-500 border border-neutral-700">
            <FaUserCircle />
          </div>
          <div>
            <h1 className="text-4xl font-bold flex flex-wrap items-baseline justify-center sm:justify-start gap-2">
              {nickname} <span className="text-xl text-neutral-500 font-medium">#{tagline}</span>
            </h1>
          </div>
        </div>

        <div className="mt-10 border-t border-neutral-800 pt-8">
          <h2 className="text-2xl font-semibold mb-6">Historia Meczów</h2>
          <div className="space-y-3">
            {[1, 2, 3].map((match) => (
              <div key={match} className="bg-neutral-800/50 p-4 rounded-xl flex justify-between items-center border-l-4 border-blue-500 hover:bg-neutral-800 transition-colors">
                <div>
                  <div className="font-bold text-blue-400">Zwycięstwo</div>
                  <div className="text-sm text-neutral-400">Ranked Solo</div>
                </div>
                <div className="text-right">
                  <div className="font-bold text-lg">12 / 3 / 8</div>
                  <div className="text-sm text-neutral-400">KDA</div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </main>
  );
}