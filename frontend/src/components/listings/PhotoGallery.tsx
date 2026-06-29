import { useState } from 'react';
import { ChevronLeft, ChevronRight } from 'lucide-react';
import { cn } from '@/lib/utils';
import type { ListingImage } from '@/types';

const fallback = 'https://images.unsplash.com/photo-1560393464-5c69a73c5770?w=800';

export function PhotoGallery({ images, title }: { images: ListingImage[]; title: string }) {
  const [active, setActive] = useState(0);
  const urls = images.length > 0 ? images.map((i) => i.url) : [fallback];

  const prev = () => setActive((i) => (i === 0 ? urls.length - 1 : i - 1));
  const next = () => setActive((i) => (i === urls.length - 1 ? 0 : i + 1));

  return (
    <div className="space-y-2">
      <div className="relative rounded-2xl overflow-hidden bg-slate-100">
        <img src={urls[active]} alt={title} className="aspect-video w-full object-cover" loading="eager" />
        {urls.length > 1 && (
          <>
            <button type="button" onClick={prev} className="absolute left-2 top-1/2 -translate-y-1/2 rounded-full bg-white/90 p-2 shadow hover:bg-white" aria-label="Previous image">
              <ChevronLeft className="h-5 w-5" />
            </button>
            <button type="button" onClick={next} className="absolute right-2 top-1/2 -translate-y-1/2 rounded-full bg-white/90 p-2 shadow hover:bg-white" aria-label="Next image">
              <ChevronRight className="h-5 w-5" />
            </button>
            <div className="absolute bottom-3 left-1/2 -translate-x-1/2 flex gap-1.5">
              {urls.map((_, i) => (
                <button
                  key={i}
                  type="button"
                  onClick={() => setActive(i)}
                  className={cn('h-2 w-2 rounded-full transition-colors', i === active ? 'bg-white' : 'bg-white/50')}
                  aria-label={`Image ${i + 1}`}
                />
              ))}
            </div>
          </>
        )}
      </div>
      {urls.length > 1 && (
        <div className="flex gap-2 overflow-x-auto pb-1">
          {urls.map((url, i) => (
            <button
              key={url + i}
              type="button"
              onClick={() => setActive(i)}
              className={cn('shrink-0 rounded-lg overflow-hidden border-2 transition-colors', i === active ? 'border-brand-500' : 'border-transparent')}
            >
              <img src={url} alt="" className="h-16 w-24 object-cover" loading="lazy" />
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
