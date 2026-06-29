import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { Search, CalendarDays, PackageCheck } from 'lucide-react';
import { HeroSection } from '@/components/home/HeroSection';
import { CategoriesScroll } from '@/components/home/CategoriesScroll';
import { TrendingRentals } from '@/components/home/TrendingRentals';
import { Button } from '@/components/ui';
import { listingsApi } from '@/lib/api';

export default function HomePage() {
  const { data: featured, isLoading } = useQuery({ queryKey: ['featured'], queryFn: listingsApi.featured });

  return (
    <div>
      <HeroSection />
      
      <CategoriesScroll />
      
      <TrendingRentals listings={featured} isLoading={isLoading} />

      <section className="bg-white py-24" id="how-it-works">
        <div className="mx-auto max-w-7xl px-4 lg:px-8">
          <div className="text-center mb-16">
            <h2 className="text-3xl font-bold tracking-tight text-slate-900 sm:text-4xl">How RentThings Works</h2>
            <p className="mt-4 text-lg text-slate-500">Rent what you need in three simple steps</p>
          </div>

          <div className="grid gap-12 sm:grid-cols-3">
            <div className="text-center">
              <div className="mx-auto flex h-16 w-16 items-center justify-center rounded-2xl bg-primary-50 text-primary-600 mb-6 shadow-sm">
                <Search className="h-8 w-8" />
              </div>
              <h3 className="text-xl font-bold text-slate-900 mb-3">1. Browse & Find</h3>
              <p className="text-slate-500 leading-relaxed">
                Search thousands of items listed by verified users in your local area.
              </p>
            </div>
            
            <div className="text-center relative">
              <div className="hidden sm:block absolute top-8 left-1/2 w-full h-[2px] bg-gradient-to-r from-primary-100 to-transparent -z-10" />
              <div className="mx-auto flex h-16 w-16 items-center justify-center rounded-2xl bg-primary-50 text-primary-600 mb-6 shadow-sm">
                <CalendarDays className="h-8 w-8" />
              </div>
              <h3 className="text-xl font-bold text-slate-900 mb-3">2. Book & Pay</h3>
              <p className="text-slate-500 leading-relaxed">
                Select your dates, pay securely online, and confirm the pickup details.
              </p>
            </div>

            <div className="text-center relative">
              <div className="hidden sm:block absolute top-8 left-1/2 w-full h-[2px] bg-gradient-to-r from-primary-100 to-transparent -z-10" />
              <div className="mx-auto flex h-16 w-16 items-center justify-center rounded-2xl bg-primary-50 text-primary-600 mb-6 shadow-sm">
                <PackageCheck className="h-8 w-8" />
              </div>
              <h3 className="text-xl font-bold text-slate-900 mb-3">3. Get & Return</h3>
              <p className="text-slate-500 leading-relaxed">
                Pick up the item, enjoy your rental, and return it safely to the owner.
              </p>
            </div>
          </div>

          <div className="mt-16 text-center">
            <Link to="/register">
              <Button className="rounded-xl bg-primary-600 px-8 py-6 text-base font-bold text-white shadow-md hover:bg-primary-700 transition-colors">
                Start Renting Today
              </Button>
            </Link>
          </div>
        </div>
      </section>
    </div>
  );
}
