import { Link, useNavigate } from 'react-router-dom';
import { MapPin, Sparkles, Star } from 'lucide-react';
import { formatCurrency, cn } from '@/lib/utils';
import type { Listing } from '@/types';
import { Badge, Skeleton } from '@/components/ui';

const fallbackImage = 'https://images.unsplash.com/photo-1560393464-5c69a73c5770?w=600';

function getListingBadges(listing: Listing) {
  const badges: { label: string; className: string }[] = [];
  if (listing.images.some((img) => img.passedValidation)) {
    badges.push({ label: 'AI Verified Image', className: 'bg-accent-indigo-50 text-accent-indigo-700 border-accent-indigo-100' });
  }
  if (listing.averageRating >= 4.5) {
    badges.push({ label: 'Top Rated', className: 'bg-amber-50 text-amber-700 border-amber-100' });
  }
  if (listing.isFeatured) {
    badges.push({ label: 'Featured', className: 'bg-accent-emerald-50 text-accent-emerald-700 border-accent-emerald-100' });
  }
  return badges;
}

interface PremiumListingCardProps {
  listing: Listing;
  variant?: 'grid' | 'trending';
}

export function PremiumListingCard({ listing, variant = 'grid' }: PremiumListingCardProps) {
  const navigate = useNavigate();
  const image = listing.images.find((i) => i.isPrimary)?.url || listing.images[0]?.url || fallbackImage;
  const badges = getListingBadges(listing);

  if (variant === 'trending') {
    return (
      <article className="premium-card group flex w-[300px] shrink-0 flex-col overflow-hidden sm:w-[320px]">
        <Link to={`/listings/${listing.id}`} className="relative block aspect-[4/3] overflow-hidden bg-slate-100">
          <img
            src={image}
            alt={listing.title}
            loading="lazy"
            className="h-full w-full object-cover transition-transform duration-500 group-hover:scale-105"
          />
          <div className="absolute inset-0 bg-gradient-to-t from-slate-900/30 via-transparent to-transparent opacity-0 transition-opacity duration-300 group-hover:opacity-100" />
          <div className="absolute right-3 top-3 flex items-center gap-1 rounded-full bg-white/95 px-2.5 py-1 text-xs font-semibold text-slate-800 shadow-sm backdrop-blur-sm">
            <Star className="h-3.5 w-3.5 fill-amber-400 text-amber-400" />
            {listing.averageRating.toFixed(1)}
          </div>
        </Link>
        <div className="flex flex-1 flex-col p-5">
          <div className="flex flex-wrap gap-1.5">
            {badges.slice(0, 2).map((b) => (
              <Badge key={b.label} className={cn('border text-[10px]', b.className)}>{b.label}</Badge>
            ))}
          </div>
          <Link to={`/listings/${listing.id}`}>
            <h3 className="mt-2 line-clamp-1 text-base font-semibold text-slate-900 transition-colors group-hover:text-accent-indigo-600">
              {listing.title}
            </h3>
          </Link>
          <p className="mt-1 flex items-center gap-1 text-sm text-slate-500">
            <MapPin className="h-3.5 w-3.5 shrink-0" />
            <span className="line-clamp-1">{listing.location}</span>
          </p>
          <div className="mt-auto flex items-end justify-between pt-4">
            <div>
              <span className="text-xl font-bold text-slate-900">{formatCurrency(listing.pricePerDay)}</span>
              <span className="text-sm text-slate-500">/day</span>
            </div>
            <button
              type="button"
              onClick={() => navigate(`/listings/${listing.id}`)}
              className="rounded-xl bg-accent-emerald-600 px-4 py-2 text-sm font-semibold text-white shadow-sm shadow-accent-emerald-600/25 transition-all hover:bg-accent-emerald-700 hover:shadow-md active:scale-[0.98]"
            >
              Rent Now
            </button>
          </div>
        </div>
      </article>
    );
  }

  return (
    <Link to={`/listings/${listing.id}`} className="premium-card group block overflow-hidden">
      <div className="relative aspect-[4/3] overflow-hidden bg-slate-100">
        <img
          src={image}
          alt={listing.title}
          loading="lazy"
          className="h-full w-full object-cover transition-transform duration-500 group-hover:scale-105"
        />
        <div className="absolute left-3 top-3 flex flex-col gap-1.5">
          {badges.map((b) => (
            <Badge key={b.label} className={cn('border text-[10px] shadow-sm backdrop-blur-sm', b.className)}>
              {b.label === 'AI Verified Image' && <Sparkles className="mr-0.5 h-3 w-3" />}
              {b.label}
            </Badge>
          ))}
        </div>
        <div className="absolute right-3 top-3 flex items-center gap-1 rounded-full bg-white/95 px-2 py-0.5 text-xs font-semibold text-slate-800 shadow-sm">
          <Star className="h-3 w-3 fill-amber-400 text-amber-400" />
          {listing.averageRating.toFixed(1)}
        </div>
      </div>
      <div className="p-4">
        <h3 className="line-clamp-1 font-semibold text-slate-900 group-hover:text-accent-indigo-600">{listing.title}</h3>
        <p className="mt-1 flex items-center gap-1 text-sm text-slate-500">
          <MapPin className="h-3.5 w-3.5" />{listing.location}
        </p>
        <div className="mt-3 flex items-center justify-between">
          <span className="text-lg font-bold text-slate-900">
            {formatCurrency(listing.pricePerDay)}
            <span className="text-sm font-normal text-slate-500">/day</span>
          </span>
          <Badge className="border-slate-100 bg-slate-50 text-slate-600">{listing.category}</Badge>
        </div>
      </div>
    </Link>
  );
}

export function PremiumListingCardSkeleton({ variant = 'grid' }: { variant?: 'grid' | 'trending' }) {
  if (variant === 'trending') {
    return (
      <div className="premium-card w-[300px] shrink-0 overflow-hidden sm:w-[320px]">
        <Skeleton className="aspect-[4/3] w-full rounded-none" />
        <div className="space-y-3 p-5">
          <Skeleton className="h-4 w-1/3" />
          <Skeleton className="h-5 w-3/4" />
          <Skeleton className="h-4 w-1/2" />
          <div className="flex justify-between pt-2">
            <Skeleton className="h-7 w-20" />
            <Skeleton className="h-9 w-24 rounded-xl" />
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="premium-card overflow-hidden">
      <Skeleton className="aspect-[4/3] w-full rounded-none" />
      <div className="space-y-3 p-4">
        <Skeleton className="h-5 w-3/4" />
        <Skeleton className="h-4 w-1/2" />
        <Skeleton className="h-6 w-1/3" />
      </div>
    </div>
  );
}
