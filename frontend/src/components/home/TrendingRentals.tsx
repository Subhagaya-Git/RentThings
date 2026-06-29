import { Link } from 'react-router-dom';
import { ArrowRight } from 'lucide-react';
import { ListingCard, ListingCardSkeleton } from '@/components/listings/ListingCard';
import type { Listing } from '@/types';

interface TrendingRentalsProps {
  listings?: Listing[];
  isLoading: boolean;
}

export function TrendingRentals({ listings, isLoading }: TrendingRentalsProps) {
  return (
    <section className="bg-slate-50/50 py-16">
      <div className="mx-auto max-w-7xl px-4 lg:px-8">
        <div className="mb-8 flex flex-col sm:flex-row sm:items-end justify-between gap-4">
          <div>
            <h2 className="text-3xl font-bold tracking-tight text-slate-900">
              Featured Rentals
            </h2>
            <p className="mt-2 text-slate-500">Hand-picked gear and equipment near you</p>
          </div>
          <Link
            to="/search"
            className="group flex items-center gap-1 text-sm font-semibold text-primary-600 transition-colors hover:text-primary-700"
          >
            View all <ArrowRight className="h-4 w-4 transition-transform group-hover:translate-x-1" />
          </Link>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
          {isLoading
            ? Array.from({ length: 3 }).map((_, i) => <ListingCardSkeleton key={i} />)
            : listings?.slice(0, 6).map((l) => <ListingCard key={l.id} listing={l} />)}
        </div>
      </div>
    </section>
  );
}
