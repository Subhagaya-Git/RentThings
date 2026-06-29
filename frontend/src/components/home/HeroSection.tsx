import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { ArrowRight, Search, MapPin, Calendar } from 'lucide-react';

export function HeroSection() {
  const navigate = useNavigate();
  const [itemName, setItemName] = useState('');
  const [location, setLocation] = useState('');
  const [dateFrom, setDateFrom] = useState('');

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    const params = new URLSearchParams();
    if (itemName) params.set('q', itemName);
    if (location) params.set('location', location);
    if (dateFrom) params.set('availableFrom', dateFrom);
    navigate(`/search?${params.toString()}`);
  };

  return (
    <section className="relative overflow-hidden bg-white py-16 sm:py-24 lg:py-32">
      {/* Background Gradients */}
      <div className="absolute inset-0 bg-gradient-to-b from-primary-600 to-primary-400" aria-hidden="true" />
      <div className="absolute inset-0 bg-[url('https://images.unsplash.com/photo-1522202176988-66273c2fd55f?q=80&w=2071&auto=format&fit=crop')] bg-cover bg-center mix-blend-overlay opacity-10" aria-hidden="true" />

      <div className="relative mx-auto max-w-7xl px-4 text-center lg:px-8">
        <h1 className="mx-auto max-w-4xl text-4xl font-bold tracking-tight text-white sm:text-6xl lg:text-7xl">
          Rent Anything. Anytime. Anywhere in Sri Lanka.
        </h1>
        <p className="mx-auto mt-6 max-w-2xl text-lg leading-8 text-primary-50">
          From cameras to camping gear — find what you need from trusted owners near you.
        </p>

        {/* Search Bar */}
        <div className="mx-auto mt-10 max-w-4xl">
          <form
            onSubmit={handleSearch}
            className="flex flex-col sm:flex-row items-center gap-2 rounded-3xl bg-white p-2 shadow-xl sm:rounded-full"
            role="search"
          >
            <div className="flex w-full items-center pl-4 pr-2 sm:w-1/3 border-b sm:border-b-0 sm:border-r border-slate-100">
              <Search className="h-5 w-5 text-slate-400 shrink-0" />
              <input
                value={itemName}
                onChange={(e) => setItemName(e.target.value)}
                placeholder="What are you looking for?"
                className="w-full bg-transparent px-3 py-3 text-slate-900 placeholder:text-slate-500 focus:outline-none"
                aria-label="Item name"
              />
            </div>
            
            <div className="flex w-full items-center pl-4 pr-2 sm:w-1/4 border-b sm:border-b-0 sm:border-r border-slate-100">
              <MapPin className="h-5 w-5 text-slate-400 shrink-0" />
              <select
                value={location}
                onChange={(e) => setLocation(e.target.value)}
                className="w-full bg-transparent px-3 py-3 text-slate-900 focus:outline-none appearance-none"
                aria-label="Location"
              >
                <option value="">All Locations</option>
                <option value="Colombo">Colombo</option>
                <option value="Kandy">Kandy</option>
                <option value="Galle">Galle</option>
                <option value="Kurunegala">Kurunegala</option>
              </select>
            </div>

            <div className="flex w-full items-center pl-4 pr-2 sm:w-1/4">
              <Calendar className="h-5 w-5 text-slate-400 shrink-0" />
              <input
                type="date"
                value={dateFrom}
                onChange={(e) => setDateFrom(e.target.value)}
                className="w-full bg-transparent px-3 py-3 text-slate-900 focus:outline-none"
                aria-label="Start date"
              />
            </div>

            <button
              type="submit"
              className="mt-2 w-full sm:mt-0 sm:w-auto shrink-0 rounded-full bg-primary-600 px-8 py-3.5 text-sm font-bold text-white transition-all hover:bg-primary-700 active:scale-95 flex items-center justify-center gap-2"
            >
              Search
              <ArrowRight className="h-4 w-4" />
            </button>
          </form>
        </div>

        {/* Trust Stats */}
        <div className="mt-12 flex flex-col sm:flex-row items-center justify-center gap-4 sm:gap-8 text-sm font-medium text-white/90">
          <div className="flex items-center gap-2">
            <span className="flex h-2 w-2 rounded-full bg-emerald-400"></span>
            2,400+ Items Listed
          </div>
          <div className="hidden sm:block h-1 w-1 rounded-full bg-white/50"></div>
          <div className="flex items-center gap-2">
            <span className="flex h-2 w-2 rounded-full bg-emerald-400"></span>
            1,800+ Happy Renters
          </div>
          <div className="hidden sm:block h-1 w-1 rounded-full bg-white/50"></div>
          <div className="flex items-center gap-2">
            <span className="flex h-2 w-2 rounded-full bg-emerald-400"></span>
            98% On-Time Returns
          </div>
        </div>
      </div>
    </section>
  );
}
