import { useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useSearchParams } from 'react-router-dom';
import { Grid, List } from 'lucide-react';
import { FilterSidebar, FilterToggleButton, type FilterState } from '@/components/browse/FilterSidebar';
import { ListingCard, ListingCardSkeleton, ListingListItem } from '@/components/listings/ListingCard';
import { EmptyState } from '@/components/ui';
import { listingsApi } from '@/lib/api';
import { useUiStore } from '@/stores';

function parseFilters(params: URLSearchParams): FilterState {
  const minPrice = Number(params.get('minPrice') || 0);
  const maxPrice = Number(params.get('maxPrice') || 50000);
  return {
    query: params.get('q') || '',
    categories: params.get('category') ? [params.get('category')!] : [],
    priceRange: [minPrice, maxPrice],
    radiusKm: params.get('radius') ? Number(params.get('radius')) : undefined,
    location: params.get('location') || '',
    availableOnly: Boolean(params.get('availableFrom') || params.get('availableTo')),
    availableFrom: params.get('availableFrom') || '',
    availableTo: params.get('availableTo') || '',
  };
}

function filtersToParams(filters: FilterState): URLSearchParams {
  const next = new URLSearchParams();
  if (filters.query) next.set('q', filters.query);
  if (filters.categories[0]) next.set('category', filters.categories[0]);
  if (filters.priceRange[0] > 0) next.set('minPrice', String(filters.priceRange[0]));
  if (filters.priceRange[1] < 50000) next.set('maxPrice', String(filters.priceRange[1]));
  if (filters.radiusKm) next.set('radius', String(filters.radiusKm));
  if (filters.location) next.set('location', filters.location);
  if (filters.availableFrom) next.set('availableFrom', filters.availableFrom);
  if (filters.availableTo) next.set('availableTo', filters.availableTo);
  return next;
}

export default function SearchPage() {
  const [params, setParams] = useSearchParams();
  const { viewMode, setViewMode } = useUiStore();
  const [showFilters, setShowFilters] = useState(false);
  const [userLat, setUserLat] = useState<number | undefined>();
  const [userLon, setUserLon] = useState<number | undefined>();

  const sortBy = params.get('sort') || 'featured';
  const filters = useMemo(() => parseFilters(params), [params]);

  const activeFilterCount = useMemo(() => {
    let count = 0;
    if (filters.query) count++;
    if (filters.categories.length) count++;
    if (filters.priceRange[0] > 0 || filters.priceRange[1] < 50000) count++;
    if (filters.radiusKm) count++;
    if (filters.location) count++;
    if (filters.availableFrom || filters.availableTo) count++;
    return count;
  }, [filters]);

  const applyFilters = (next: FilterState) => {
    const urlParams = filtersToParams(next);
    if (sortBy !== 'featured') urlParams.set('sort', sortBy);
    setParams(urlParams);
  };

  const { data, isLoading } = useQuery({
    queryKey: ['search', params.toString(), userLat, userLon],
    queryFn: () =>
      listingsApi.search({
        query: params.get('q') || undefined,
        category: params.get('category') || undefined,
        location: params.get('location') || undefined,
        sortBy: params.get('radius') ? 'distance' : sortBy,
        minPrice: params.get('minPrice') || undefined,
        maxPrice: params.get('maxPrice') || undefined,
        availableFrom: params.get('availableFrom') || undefined,
        availableTo: params.get('availableTo') || undefined,
        latitude: userLat,
        longitude: userLon,
        radiusKm: params.get('radius') ? Number(params.get('radius')) : undefined,
        page: 1,
        pageSize: 24,
      }),
  });

  const useMyLocation = () => {
    navigator.geolocation?.getCurrentPosition((pos) => {
      setUserLat(pos.coords.latitude);
      setUserLon(pos.coords.longitude);
    });
  };

  const updateSort = (value: string) => {
    const next = new URLSearchParams(params);
    if (value) next.set('sort', value);
    else next.delete('sort');
    setParams(next);
  };

  return (
    <div className="mx-auto max-w-7xl px-4 py-8 lg:px-8 bg-surface min-h-screen">
      <div className="mb-8">
        <h1 className="text-3xl font-bold tracking-tight text-slate-900">Browse rentals</h1>
        <p className="mt-2 text-slate-500">Discover premium gear from trusted owners near you</p>
      </div>

      <div className="flex flex-col lg:flex-row gap-8">
        <FilterSidebar
          filters={filters}
          onChange={applyFilters}
          onUseLocation={useMyLocation}
          open={showFilters}
          onClose={() => setShowFilters(false)}
        />

        <div className="min-w-0 flex-1">
          <div className="mb-6 flex flex-wrap items-center justify-between gap-3">
            <div className="flex items-center gap-3">
              <FilterToggleButton count={activeFilterCount} onClick={() => setShowFilters(true)} />
              <p className="text-sm text-slate-500">
                <span className="font-bold text-slate-900">{data?.totalCount ?? 0}</span> listings found
              </p>
            </div>
            <div className="flex items-center gap-3">
              <select
                value={sortBy}
                onChange={(e) => updateSort(e.target.value)}
                className="rounded-xl border border-slate-200 bg-white px-4 py-2 text-sm text-slate-700 focus:border-primary-400 focus:ring-2 focus:ring-primary-100 outline-none transition-shadow"
                aria-label="Sort by"
              >
                <option value="featured">Featured</option>
                <option value="price_asc">Price: Low to High</option>
                <option value="price_desc">Price: High to Low</option>
                <option value="rating">Top Rated</option>
                <option value="newest">Newest</option>
              </select>
              <div className="flex overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
                <button
                  type="button"
                  onClick={() => setViewMode('grid')}
                  className={`p-2 transition-colors ${viewMode === 'grid' ? 'bg-primary-50 text-primary-600' : 'text-slate-400 hover:bg-slate-50 hover:text-slate-600'}`}
                  aria-label="Grid view"
                >
                  <Grid className="h-4 w-4" />
                </button>
                <button
                  type="button"
                  onClick={() => setViewMode('list')}
                  className={`p-2 transition-colors ${viewMode === 'list' ? 'bg-primary-50 text-primary-600' : 'text-slate-400 hover:bg-slate-50 hover:text-slate-600'}`}
                  aria-label="List view"
                >
                  <List className="h-4 w-4" />
                </button>
              </div>
            </div>
          </div>

          {isLoading ? (
            <div className="grid gap-6 grid-cols-1 sm:grid-cols-2 lg:grid-cols-3">
              {Array.from({ length: 6 }).map((_, i) => (
                <ListingCardSkeleton key={i} />
              ))}
            </div>
          ) : !data?.items.length ? (
            <div className="mt-12">
              <EmptyState title="No listings found" description="Try adjusting your filters or search terms to find what you're looking for." />
            </div>
          ) : viewMode === 'grid' ? (
            <div className="grid gap-6 grid-cols-1 sm:grid-cols-2 lg:grid-cols-3">
              {data.items.map((l) => (
                <ListingCard key={l.id} listing={l} />
              ))}
            </div>
          ) : (
            <div className="space-y-4">
              {data.items.map((l) => (
                <ListingListItem key={l.id} listing={l} />
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
