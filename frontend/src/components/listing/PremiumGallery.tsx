import { useState } from 'react';
import { Heart, ShieldCheck } from 'lucide-react';
import { cn } from '@/lib/utils';
import type { ListingImage } from '@/types';

const fallback = 'https://images.unsplash.com/photo-1560393464-5c69a73c5770?w=1200';

interface PremiumGalleryProps {
  images: ListingImage[];
  title: string;
}

export function PremiumGallery({ images, title }: PremiumGalleryProps) {
  const [active, setActive] = useState(0);
  const [saved, setSaved] = useState(false);
  const urls = images.length > 0 ? images.map((i) => i.url) : [fallback];
  const thumbnails = urls.slice(0, 4);

  return (
    <div className="space-y-4">
      <div className="group relative overflow-hidden rounded-3xl border border-slate-100 bg-slate-100 shadow-sm">
        <img
          src={urls[active]}
          alt={title}
          className="aspect-[4/3] sm:aspect-[16/10] w-full object-cover transition-transform duration-700 group-hover:scale-[1.02]"
          loading="eager"
          onError={(e) => {
            (e.target as HTMLImageElement).src = fallback;
          }}
        />
        
        <div className="absolute top-4 left-4 flex items-center gap-1.5 bg-emerald-500/90 backdrop-blur-sm text-white px-3 py-1.5 rounded-full text-sm font-bold shadow-sm">
          <ShieldCheck className="h-4 w-4" />
          AI Verified
        </div>

        <button
          type="button"
          onClick={() => setSaved(!saved)}
          className={cn(
            'absolute right-4 top-4 flex h-10 w-10 items-center justify-center rounded-full border border-slate-100 bg-white/90 shadow-md backdrop-blur-sm transition-all hover:scale-105',
            saved && 'border-rose-200 bg-rose-50',
          )}
          aria-label={saved ? 'Remove from favorites' : 'Save to favorites'}
        >
          <Heart className={cn('h-5 w-5 transition-colors', saved ? 'fill-rose-500 text-rose-500' : 'text-slate-600')} />
        </button>
      </div>

      {thumbnails.length > 1 && (
        <div className="grid grid-cols-4 gap-3">
          {thumbnails.map((url, i) => (
            <button
              key={url + i}
              type="button"
              onClick={() => setActive(i)}
              className={cn(
                'overflow-hidden rounded-2xl border-2 transition-all duration-200 hover:opacity-90',
                i === active ? 'border-primary-500 shadow-sm' : 'border-transparent opacity-60 hover:opacity-100',
              )}
            >
              <img 
                src={url} 
                alt="" 
                className="aspect-square sm:aspect-[4/3] w-full object-cover" 
                loading="lazy" 
                onError={(e) => {
                  (e.target as HTMLImageElement).src = fallback;
                }}
              />
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
