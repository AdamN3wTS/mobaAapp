# **MobaApp – dokumentacja**

Aplikacja do statystyk z League of Legends w stylu op.gg / u.gg. Można wyszukać gracza po nicku i tagu, zobaczyć jego rangę i historię meczów, a do tego statystyki championów (winrate, pickrate, banrate, runy, itemy, spelle). Statystyki są liczone z meczów, które aplikacja sama zbiera w tle.

Przy każdej części w nawiasie jest podpisany autor.

**Stack:**
  - **Backend (`api/`):** ASP.NET Core, EF Core, PostgreSQL (Supabase)
  - **Frontend (`web/`):** Next.js, React, Tailwind
  - **Dane:** Riot API (konta, rangi, mecze) i Data Dragon (ikony, nazwy itemów, run i spelli)

---

# **Backend**

## **Klient Riot API + rate limiter (Adam J)**
`api/Client/`

`RiotClient` to jedno miejsce, przez które idą wszystkie zapytania do Riota. Klucz bierze z `RIOT_API_KEY` i dokleja go jako nagłówek `X-Riot-Token`. Wszystkie metody korzystają ze wspólnej `GetAsync<T>`, która:
  - przy błędzie 429 albo 5xx ponawia zapytanie, maksymalnie 3 razy;
  - przy 429 czeka tyle, ile Riot podał w nagłówku `Retry-After`;
  - zwraca `ResponseRiotDto` ze status code i danymi, więc serwisy same decydują, co zrobić z błędem.

**Rate limiter (`RiotRateLimiter`)** pilnuje dwóch limitów jednocześnie, osobno dla każdego regionu (`eun1`, `europe` itd.):
  - 18 zapytań na sekundę,
  - 90 zapytań na 2 minuty.

Klucz developerski Riota ma limit 20/s i 100/2min. Na początku łapałem przez to błędy 429. Liczby w limiterze dobrałem tak, żeby błąd nigdy nie wyskoczył i żeby został zapas: crawler może chodzić w tle, a użytkownik wciąż może normalnie wyszukiwać graczy.

**Flaga `IsBcg`** rozróżnia zapytania z serwisów działających w tle od zapytań od użytkownika:
  - **Serwis w tle** przechodzi przez limiter i w razie potrzeby cierpliwie czeka.
  - **Zapytanie użytkownika** omija limiter. Jeśli Riot każe czekać dłużej niż 2 sekundy, odpowiedź wraca od razu, zamiast wieszać stronę.

Chodziło o to, żeby dla człowieka odpowiedź była praktycznie natychmiastowa, a czekał crawler. *Do poprawy*: zapytania użytkownika nie są wliczane do limitera, więc przy dużym ruchu mogą zjadać limit crawlerowi.

## **Crawler (Adam J)**
`api/Crawler/Crawler.cs`, `api/Services/CrawlerService.cs`, `api/Helpers/RandomRankGenerator.cs`, `api/Helpers/MatchMapper.cs`

Crawler to `BackgroundService`, który w pętli zbiera mecze rankingowe do bazy. Jedna tura wygląda tak:
  - **1. Pula graczy.** Jeśli w bazie jest mniej niż 10 graczy „do przecrawlowania”, crawler losuje rangę (tier od Iron do Diamond i dywizję I–IV). Potem pobiera z League API listę graczy z tej rangi na EUNE i dodaje do tabeli `CrawlPlayers` tych, których jeszcze tam nie ma. Losowanie jest po to, żeby dane były uśrednione ze wszystkich rang, a nie np. tylko z platyny.
  - **2. ID meczów.** Crawler bierze do 30 graczy, których nigdy nie crawlowano albo crawlowano ponad 6 godzin temu. Dla każdego pobiera ID meczów: tylko ranked solo (`queue=420`), od ostatniego crawla albo z ostatnich 14 dni. Potem zapisuje graczowi `LastCrawledAt`.
  - **3. Pobranie meczów.** ID są deduplikowane, a te, które już są w bazie, odpadają. Resztę pobieram z Match API, mapuję przez `MatchMapper` na encje i zapisuję.
  - **4. Przerwa** na `CRAWL_TIME_MINUTES`. Jeśli w tej turze wystąpił wyjątek, przerwa wynosi 30 s i pętla leci dalej.

Skąd te liczby:
  - **6 godzin** to bezpieczna przerwa: ktoś gra o 15, a następne gry ma np. o 21. Możliwe, że zwiększę to nawet do 2 dni.
  - **30 graczy na turę** to bezpieczna liczba pod limity API.
  - **Tylko rankedy**, bo tylko one nas interesują.
  - **14 dni**, żeby dane były w miarę aktualne. *Do zrobienia*: brać mecze dokładnie od daty wyjścia ostatniego patcha.

`MatchMapper` z ogromnego JSON-a meczu wyciąga tylko to, co jest potrzebne:
  - patch, czas gry, bany,
  - dla każdego z 10 graczy: champion, pozycja, wynik, KDA, CS, spelle, itemy,
  - runy: drzewo główne i poboczne, keystone, wszystkie 6 run i odłamki.

`DiscoverNewPlayersAsync` dodaje do puli graczy znalezionych w pobranych meczach. Na razie jest wyłączone, bo wymaga dodatkowego zapytania o rangę każdego nowego gracza (około 10 zapytań na mecz). To prototyp do dokładnego przetestowania.

**Wyniki:** w około 3 godziny crawler zebrał ok. 15 tys. meczów. Problem w tym, że baza ma już ok. 150 MB (ok. 10 KB na mecz).

## **Statystyki (Adam J)**
`api/Stats/StatsBcg.cs`, `api/Services/StatsService.cs`, `api/Controllers/StatsController.cs`

`StatsBcg` to drugi `BackgroundService`. Co `STATS_TIME_MINUTES` wywołuje `GenerateStats`, które:
  1. Ładuje wszystkich uczestników meczów z przypisaną pozycją oraz liczbę banów każdego championa.
  2. Grupuje uczestników po championie i pozycji. Dla każdej grupy liczy:
      - **winrate** = wygrane / gry,
      - **pickrate** = gry / wszystkie mecze,
      - **banrate** = mecze z banem / wszystkie mecze,
      - **spelle**: pary spelli, kolejność nie ma znaczenia (Flash+Ignite = Ignite+Flash), z pickrate,
      - **itemy**: każdy item liczony raz na grę, z winrate i pickrate,
      - **strony run**: keystone + drzewo główne + poboczne, top 10,
      - **pojedyncze runy** z winrate i pickrate,
      - **odłamki**: top 5 zestawów.
  3. W jednej transakcji zapisuje nowy snapshot i kasuje wszystkie starsze.

Pierwszy pomysł był taki, żeby trzymać historię statystyk i pokazywać, jak zmieniały się w czasie. Zostało to do przemyślenia. Na razie trzymamy tylko najnowszy snapshot, żeby baza nie puchła.

`GET /api/stats/get` zwraca najnowszy snapshot dla strony głównej: banrate wszystkich championów oraz listy championów pogrupowane po pozycjach (TOP, JUNGLE, MIDDLE, BOTTOM, UTILITY). Na liście są tylko championi z **co najmniej 80 grami** na danej pozycji.

80 gier to i tak mało. Przy małej próbce łatwo trafić np. na kilku mainów Garena, którzy dużo wygrywają, i statystyki przestają być realne. Duże strony pokazują championa dopiero od ok. 1–2 tys. gier na linii. My przy obecnej ilości danych musieliśmy zejść niżej.

Runy doszły jako osobny etap (migracja `AddRuneStats`). Najtrudniejsze było dobranie się do tych danych w odpowiedzi Riota, a potem sensowne ich wyświetlenie.

## **Baza danych (Adam J)**
`api/Data/AppDbContext.cs`, `api/DbModels/`, `api/DbServices/`, `api/Migrations/`

PostgreSQL na Supabase, EF Core w podejściu code-first z migracjami. Postgres, bo jest powszechny. Supabase, bo pozwala bardzo szybko postawić bazę. Myślimy jednak o migracji na inny hosting, bo Supabase ma dość niskie limity (patrz 150 MB po 3 godzinach crawla).

Tabele:

| Tabela | Co trzyma |
|---|---|
| `CrawlPlayers` | Pula graczy dla crawlera: puuid, platforma, tier, `LastCrawledAt` |
| `Matches` | Mecz: id, patch, czas trwania, data |
| `MatchParticipants` | Jeden wiersz na gracza w meczu (10 na mecz): champion, pozycja, wynik, KDA, spelle, itemy, runy, odłamki |
| `ParticipantRunes` | 6 run każdego uczestnika |
| `MatchBans` | Bany w meczu |
| `ChampionStatsInfos` | Wyliczone statystyki championa na pozycji |
| `ChampionStats*` (Items, Spells, RunePage, Rune, Shard) | Szczegóły statystyk powiązane z `ChampionStatsInfos` |

`DbServices` (`CrawlPlayerDbService`, `MatchDbService`) opakowują zapytania do bazy, z których korzysta crawler: kto jest do crawlowania, które mecze już są w bazie, dodawanie graczy i meczów.

## **Program.cs (Adam J)**
Konfiguracja całej aplikacji:
  - ładowanie `.env` przez DotNetEnv,
  - podpięcie bazy,
  - rejestracja serwisów w DI: `RiotRateLimiter` jako singleton, żeby był jeden licznik na całą aplikację; `RiotClient` przez `AddHttpClient`,
  - CORS dla frontu na `localhost:3000`.

Crawler i StatsBcg odpalają się tylko przy `CRAWLER_ENABLED=true`, więc lokalnie można postawić samo API bez crawlowania.

Zmienne w `.env`:

| Zmienna | Opis |
|---|---|
| `CONNECTION_STRING` | Connection string do Postgresa |
| `RIOT_API_KEY` | Klucz Riot API |
| `CRAWLER_ENABLED` | `true` włącza crawler i liczenie statystyk |
| `CRAWL_TIME_MINUTES` | Przerwa między turami crawlera |
| `STATS_TIME_MINUTES` | Co ile przeliczać statystyki |

---

## **Integracja graczy i meczów z Riot API oraz setup środowiska (Aleksander K)**
`api/Services/RiotPlayerService.cs`, `api/Services/RiotMatchService.cs`, `api/Controllers/PlayerController.cs`, `api/Program.cs`, `.env`, `appsettings.Development.json`

Postawiłem moduły odpowiedzialne za bezpośrednie pobieranie danych graczy oraz ich historii meczów. Napisałem przeliczanie identyfikatorów (Riot ID / Tag -> PUUID -> SummonerId), dzięki czemu można wyszukać kogoś po zwykłym nicku i tagu (np. `Nick#EUNE`), a API zgarnia z Riota szczegóły konta oraz rangi w kolejce solo/duo i flex.

Zbudowałem też logikę do stronnicowanego (paginowanego) pobierania historii meczów gracza paczkami oraz algorytmy wyliczające metryki wydajności z ostatnich gier (średnie KDA, udział w zabójstwach, CS/min, winrate).

Dodatkowo zająłem się konfiguracją środowiska startowego – wdrożyłem bibliotekę DotNetEnv, żeby zautomatyzować ładowanie sekretów i kluczy API z plików `.env`, przygotowałem `.gitignore` oraz konfigurację w `appsettings.Development.json`. Całość przetestowałem i zdebugowałem w Postmanie, przygotowując kolekcję zapytań do weryfikacji endpointów i obsługi błędów sieciowych.

---

# **Frontend**

## **lib/api.ts (Adam J)**
Funkcje do komunikacji z backendem: `getStats`, `getPlayer`, `getMatches`, `getChampion`, `getChampionPositions`. Wszystkie pobierają dane z `cache: "no-store"`, żeby zawsze pokazywać świeże dane.

## **Top 5 na stronie głównej (Adam J)**
`components/StatsBoard.tsx`

Pod wyszukiwarką są zakładki z pozycjami (oraz „wszystkie”). Dla każdej pokazujemy top 5 championów według winrate, pickrate i banrate, z linkami do stron championów. Dane pochodzą z `/api/stats/get`.

## **Strona championa (Adam J)**
`app/champion/[name]/page.tsx`, `app/champion/[name]/[position]/page.tsx`

  - **`/champion/{name}`** sprawdza, na jakich pozycjach champion jest grany, i przekierowuje na główną, np. `/champion/ahri/mid`.
  - **`/champion/{name}/{position}`** pokazuje:
      - winrate, pickrate, banrate i liczbę gier; poniżej 100 gier wyświetla się ostrzeżenie o małej ilości danych,
      - przełącznik pozycji; obsługuje też aliasy, np. `adc`/`bot`, `jg`, `supp`,
      - **runy**: najczęstsza strona run, a w każdym rzędzie zaznaczona najczęściej grana runa; po najechaniu widać WR i PR,
      - **odłamki** i **spelle**: top 2 zestawy,
      - **itemy**: tylko kompletne przedmioty za co najmniej 2000 golda, buty osobno, bez trinketów i consumabli; pomijamy też itemy kupione w mniej niż 1% gier (minimum 10).

Ikony, nazwy run, itemów i spelli pochodzą z Data Dragon, z którego korzysta każda strona tego typu. Nie było tu dużo kombinowania.

Endpointy championów są na razie w `TestController` (`/api/test/champion/...`). To dość nowa funkcjonalność, jeszcze nie w 100% przetestowana. Po testach przejdą do normalnego kontrolera.

---

### Zakładka /champion/page.tsx (Dave N)

* Utworzono nową ścieżkę `/champion` (Server Component) wykorzystującą wbudowany w React `<Suspense>` do asynchronicznego pobierania danych.
* Przygotowano interfejs tabeli Tier Listy (wzorowany na u.gg) w komponencie klienckim `TierListClient`.
* Zintegrowano widok z API (`getStats()`). 
* **Przetwarzanie danych z API:** Ponieważ dane o statystykach były podzielone w obiekcie, spłaszczono tablicę `statsPositions` ze statystykami per rola do jednej wspólnej listy, a następnie połączono ją z tablicą `statsBans` na podstawie `championId`.
* Zaimplementowano interaktywne filtry ról (All Roles, Top, Jungle, Mid, ADC, Support), które mapują przyciski na poprawne wartości zwracane przez backend (TOP, JUNGLE, MIDDLE, BOTTOM, UTILITY).

---

# **Największe trudności**
  - **Ogarnięcie Riot API.** Endpoint z meczem zwraca ogromnego JSON-a: ponad 100 pól na każdego z 10 graczy, do tego obiekt `challenges` z kolejnymi dziesiątkami pól, drużyny, bany i runy zagnieżdżone w kilku poziomach. Trzeba było ustalić, które dane są w ogóle potrzebne. W DTO zostało ok. 25 pól na uczestnika. Dochodzi do tego podział na routing regionalny (`europe`) i platformowy (`eun1`), który na początku myli.
  - **Rate limity.** Błędy 429, dopóki nie powstał limiter z zapasem.
  - **Stabilny crawler.** Musiał chodzić godzinami bez wywalania się, np. nie paść po 2 godzinach przez jakiś wyjątek, nie dublować meczów i graczy oraz dobrze reagować na zatrzymanie aplikacji.
  - **Liczenie winrate i pickrate.** Ciężkie zapytania LINQ z grupowaniem po championie i pozycji, a w środku kolejne grupowania po itemach, runach i spellach.
  - **Rozmiar bazy.** 150 MB po 15 tys. meczów przy limitach darmowego Supabase.

# **Do zrobienia**
  - Liczyć zapytania użytkownika w limiterze albo dać im osobną pulę.
  - Przetestować i włączyć `DiscoverNewPlayersAsync`.
  - Brać mecze od daty ostatniego patcha zamiast z 14 dni.
  - Zastanowić się nad wydłużeniem przerwy między crawlami gracza (np. do 2 dni).
  - Historia statystyk w czasie.
  - Przeniesienie bazy z Supabase na inny hosting.
  - Przenieść endpointy championów z `TestController` do docelowego kontrolera.
  - Obsługa innych regionów niż EUNE (teraz `eun1` jest wpisane na sztywno).
  - Logowanie błędów w pustych `catch`. Teraz część wyjątków jest połykana bez śladu, np. w `Crawler` i `CrawlPlayerDbService`.