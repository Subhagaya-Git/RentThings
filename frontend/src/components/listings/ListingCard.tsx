import { Link } from 'react-router-dom';
import { formatCurrency } from '@/lib/utils';
import type { Listing } from '@/types';
import { Badge, Skeleton } from '../ui';
import { Star, MapPin, Trash2 } from 'lucide-react';
import { useAuthStore } from '@/stores';
import { listingsApi } from '@/lib/api';
export function ListingCard({ listing, allowDelete = false }: { listing: Listing, allowDelete?: boolean }) {
  const { user } = useAuthStore();
  const image = listing.images.find((i) => i.isPrimary)?.url || listing.images[0]?.url || 'https://images.unsplash.com/photo-1560393464-5c69a73c5770?w=400';

  const handleDelete = async (e: React.MouseEvent, id: string) => {
    e.preventDefault();
    e.stopPropagation();
    if (!window.confirm("Are you sure you want to delete this listing?")) return;
    try {
      await listingsApi.delete(id);
      window.location.reload();
    } catch (err) {
      console.error(err);
      alert('Failed to delete listing.');
    }
  };

  return (
    <Link
      to={`/listings/${listing.id}`}
      className="group block relative bg-white rounded-2xl shadow-sm hover:shadow-md transition-all duration-300 overflow-hidden border border-slate-100 h-full"
    >
      {/* Image area */}
      <div className="relative aspect-[4/3] w-full overflow-hidden bg-slate-100">
        <img
          src={image}
          alt={listing.title}
          loading="lazy"
          className="h-full w-full object-cover transition-transform duration-500 group-hover:scale-105"
          onError={(e) => {
            (e.target as HTMLImageElement).src =
              "https://images.unsplash.com/photo-1560518883-ce09059eeffa?w=600&q=80";
          }}
        />
        
        {/* Badges — bottom left of image, not overlapping title */}
        <div className="absolute bottom-3 left-3 flex flex-wrap gap-2">
          {listing.isFeatured && (
            <Badge className="bg-primary-600 hover:bg-primary-700 text-white border-none shadow-sm">Featured</Badge>
          )}
          {listing.isAiVerified && (
            <Badge className="bg-emerald-500 hover:bg-emerald-600 text-white border-none shadow-sm">AI Verified</Badge>
          )}
        </div>

        {/* Rating — top right */}
        <div className="absolute top-3 right-3">
          <div className="bg-white/90 backdrop-blur-sm px-2 py-1 rounded-lg shadow-sm flex items-center gap-1">
            <Star className="w-3.5 h-3.5 fill-amber-400 text-amber-400" />
            <span className="text-xs font-bold text-slate-700">{listing.averageRating.toFixed(1)}</span>
          </div>
        </div>
      </div>

      {/* Card body */}
      <div className="p-4">
        <div className="mb-1">
          <h3 className="font-bold tracking-tight text-slate-900 line-clamp-1 group-hover:text-primary-600 transition-colors">
            {listing.title}
          </h3>
        </div>

        <div className="flex items-center text-sm text-slate-500 mb-3">
          <MapPin className="w-3.5 h-3.5 mr-1" />
          <span className="truncate">{listing.location}</span>
        </div>

        <div className="flex items-end justify-between pt-2 border-t border-slate-50">
          <div className="flex items-baseline">
            <span className="font-bold text-slate-900 text-lg">{formatCurrency(listing.pricePerDay)}</span>
            <span className="text-sm text-slate-500 ml-1">/day</span>
          </div>
          <span className="text-xs font-medium px-2 py-1 bg-slate-100 text-slate-600 rounded-full">
            {listing.category}
          </span>
        </div>
      </div>
      {allowDelete && (user?.role === 'Admin' || user?.id === listing.ownerId) && (
        <button 
          onClick={(e) => handleDelete(e, listing.id)}
          className="absolute bottom-16 right-4 bg-red-50 hover:bg-red-100 text-red-600 p-2 rounded-xl border border-red-200 transition-colors shadow-sm z-10"
          title="Delete Listing"
        >
          <Trash2 className="w-4 h-4" />
        </button>
      )}
    </Link>
  );
}

export function ListingCardSkeleton() {
  return (
    <div className="bg-white rounded-2xl shadow-sm border border-slate-100 overflow-hidden">
      <Skeleton className="aspect-[4/3] w-full rounded-none" />
      <div className="p-4 space-y-3">
        <Skeleton className="h-5 w-3/4" />
        <Skeleton className="h-4 w-1/2" />
        <div className="pt-2 border-t border-slate-50 flex justify-between">
          <Skeleton className="h-6 w-1/3" />
          <Skeleton className="h-6 w-1/4 rounded-full" />
        </div>
      </div>
    </div>
  );
}

export function ListingListItem({ listing, allowDelete = false }: { listing: Listing, allowDelete?: boolean }) {
  const { user } = useAuthStore();
  const image = listing.images[0]?.url || 'https://images.unsplash.com/photo-1560393464-5c69a73c5770?w=400';

  const handleDelete = async (e: React.MouseEvent, id: string) => {
    e.preventDefault();
    e.stopPropagation();
    if (!window.confirm("Are you sure you want to delete this listing?")) return;
    try {
      await listingsApi.delete(id);
      window.location.reload();
    } catch (err) {
      console.error(err);
      alert('Failed to delete listing.');
    }
  };

  return (
    <Link to={`/listings/${listing.id}`} className="relative bg-white flex gap-4 rounded-2xl p-4 shadow-sm hover:shadow-md transition-shadow border border-slate-100 h-full">
      <div className="relative shrink-0">
        <img 
          src={image} 
          alt={listing.title} 
          loading="lazy" 
          className="h-24 w-32 md:h-32 md:w-48 rounded-xl object-cover" 
          onError={(e) => {
            (e.target as HTMLImageElement).src =
              "https://images.unsplash.com/photo-1560518883-ce09059eeffa?w=600&q=80";
          }}
        />
        {listing.isFeatured && (
          <Badge className="absolute top-2 left-2 bg-primary-600 hover:bg-primary-700 text-white border-none shadow-sm text-[10px] px-1.5 py-0">Featured</Badge>
        )}
      </div>
      <div className="flex-1 min-w-0 flex flex-col justify-between py-1">
        <div>
          <div className="flex justify-between items-start">
            <h3 className="font-bold text-slate-900 truncate hover:text-primary-600 transition-colors">{listing.title}</h3>
            <div className="flex items-center gap-1 shrink-0 bg-slate-50 px-1.5 py-0.5 rounded-md">
              <Star className="w-3 h-3 fill-amber-400 text-amber-400" />
              <span className="text-xs font-bold text-slate-700">{listing.averageRating.toFixed(1)}</span>
            </div>
          </div>
          <div className="flex items-center text-sm text-slate-500 mt-1">
            <MapPin className="w-3.5 h-3.5 mr-1 shrink-0" />
            <span className="truncate">{listing.location}</span>
          </div>
          <p className="text-sm text-slate-500 mt-2 line-clamp-2 hidden md:block">{listing.description}</p>
        </div>
        <div className="mt-2 flex items-center justify-between">
          <div className="flex items-baseline">
            <span className="font-bold text-slate-900 text-lg">{formatCurrency(listing.pricePerDay)}</span>
            <span className="text-sm text-slate-500 ml-1">/day</span>
          </div>
          <Badge className="bg-slate-100 text-slate-600 border-none font-medium">{listing.category}</Badge>
        </div>
      </div>
      {allowDelete && (user?.role === 'Admin' || user?.id === listing.ownerId) && (
        <button 
          onClick={(e) => handleDelete(e, listing.id)}
          className="absolute bottom-4 right-4 bg-red-50 hover:bg-red-100 text-red-600 p-2 rounded-xl border border-red-200 transition-colors shadow-sm z-10"
          title="Delete Listing"
        >
          <Trash2 className="w-4 h-4" />
        </button>
      )}
    </Link>
  );
}
