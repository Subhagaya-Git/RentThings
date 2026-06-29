import { ChevronDown, MapPin, SlidersHorizontal, X } from 'lucide-react';
import { Input, RangeSlider, Toggle } from '@/components/ui';
import { filterCategories } from '@/lib/categories';
import { formatCurrency, cn } from '@/lib/utils';

export interface FilterState {
  query: string;
  categories: string[];
  priceRange: [number, number];
  radiusKm?: number;
  location: string;
  availableOnly: boolean;
  availableFrom: string;
  availableTo: string;
}

interface FilterSidebarProps {
  filters: FilterState;
  onChange: (filters: FilterState) => void;
  onUseLocation: () => void;
  open: boolean;
  onClose: () => void;
  className?: string;
}

export function FilterSidebar({ filters, onChange, onUseLocation, open, onClose, className }: FilterSidebarProps) {
  const update = <K extends keyof FilterState>(key: K, value: FilterState[K]) => {
    onChange({ ...filters, [key]: value });
  };

  const toggleCategory = (cat: string) => {
    const next = filters.categories.includes(cat)
      ? filters.categories.filter((c) => c !== cat)
      : [...filters.categories, cat];
    update('categories', next);
  };

  const panel = (
    <div className="space-y-6">
      <div className="flex items-center justify-between lg:hidden">
        <h2 className="font-semibold text-slate-900 flex items-center gap-2">
          <SlidersHorizontal className="h-4 w-4 text-accent-indigo-600" /> Filters
        </h2>
        <button type="button" onClick={onClose} className="rounded-lg p-1.5 hover:bg-slate-100" aria-label="Close filters">
          <X className="h-5 w-5 text-slate-500" />
        </button>
      </div>

      <div className="hidden lg:block">
        <h2 className="font-semibold text-slate-900 flex items-center gap-2">
          <SlidersHorizontal className="h-4 w-4 text-accent-indigo-600" /> Filters
        </h2>
        <p className="mt-1 text-xs text-slate-500">Refine your search</p>
      </div>

      <div>
        <label className="text-sm font-medium text-slate-700">Keywords</label>
        <Input
          value={filters.query}
          onChange={(e) => update('query', e.target.value)}
          placeholder="Search items..."
          className="mt-1.5 border-slate-100"
        />
      </div>

      <div>
        <label className="text-sm font-medium text-slate-700">Price range</label>
        <div className="mt-3">
          <RangeSlider
            min={0}
            max={50000}
            step={500}
            value={filters.priceRange}
            onChange={(v) => update('priceRange', v)}
            formatLabel={formatCurrency}
          />
        </div>
      </div>

      <div>
        <p className="text-sm font-medium text-slate-700 mb-3">Category</p>
        <div className="space-y-2 max-h-48 overflow-y-auto pr-1">
          {filterCategories.map((cat) => (
            <label key={cat} className="flex cursor-pointer items-center gap-3 rounded-lg px-2 py-1.5 transition-colors hover:bg-slate-50">
              <input
                type="checkbox"
                checked={filters.categories.includes(cat)}
                onChange={() => toggleCategory(cat)}
                className="h-4 w-4 rounded border-slate-300 text-accent-indigo-600 focus:ring-accent-indigo-500"
              />
              <span className="text-sm text-slate-600">{cat}</span>
            </label>
          ))}
        </div>
      </div>

      <div>
        <label className="text-sm font-medium text-slate-700">Proximity radius</label>
        <select
          value={filters.radiusKm ?? ''}
          onChange={(e) => update('radiusKm', e.target.value ? Number(e.target.value) : undefined)}
          className="premium-input mt-1.5"
          aria-label="Distance radius"
        >
          <option value="">Any distance</option>
          <option value="5">Within 5 km</option>
          <option value="10">Within 10 km</option>
          <option value="25">Within 25 km</option>
          <option value="50">Within 50 km</option>
        </select>
        <button type="button" onClick={onUseLocation} className="mt-2 flex items-center gap-1 text-xs font-medium text-accent-indigo-600 hover:text-accent-indigo-700">
          <MapPin className="h-3 w-3" /> Use my location
        </button>
      </div>

      <div>
        <label className="text-sm font-medium text-slate-700">Location</label>
        <Input
          value={filters.location}
          onChange={(e) => update('location', e.target.value)}
          placeholder="Colombo, Kandy..."
          className="mt-1.5 border-slate-100"
        />
      </div>

      <Toggle
        checked={filters.availableOnly}
        onChange={(v) => update('availableOnly', v)}
        label="Available now"
        description="Show only items available for selected dates"
      />

      {filters.availableOnly && (
        <div className="grid grid-cols-2 gap-2">
          <div>
            <label className="text-xs font-medium text-slate-600">From</label>
            <Input type="date" value={filters.availableFrom} onChange={(e) => update('availableFrom', e.target.value)} className="mt-1 border-slate-100" />
          </div>
          <div>
            <label className="text-xs font-medium text-slate-600">To</label>
            <Input type="date" value={filters.availableTo} onChange={(e) => update('availableTo', e.target.value)} className="mt-1 border-slate-100" />
          </div>
        </div>
      )}
    </div>
  );

  return (
    <>
      {open && (
        <div className="fixed inset-0 z-40 bg-slate-900/20 backdrop-blur-sm lg:hidden" onClick={onClose} aria-hidden="true" />
      )}
      <aside
        className={cn(
          'fixed inset-y-0 left-0 z-50 w-80 transform overflow-y-auto border-r border-slate-100 bg-white p-6 shadow-xl transition-transform duration-300 lg:static lg:z-auto lg:w-72 lg:shrink-0 lg:transform-none lg:overflow-visible lg:border-0 lg:bg-transparent lg:p-0 lg:shadow-none',
          open ? 'translate-x-0' : '-translate-x-full lg:translate-x-0',
          className,
        )}
      >
        <div className="lg:sticky lg:top-24 rounded-2xl border border-slate-100 bg-white p-6 shadow-sm">
          {panel}
        </div>
      </aside>
    </>
  );
}

export function FilterToggleButton({ count, onClick }: { count?: number; onClick: () => void }) {
  return (
    <button
      type="button"
      onClick={onClick}
      className="inline-flex items-center gap-2 rounded-xl border border-slate-100 bg-white px-4 py-2 text-sm font-medium text-slate-700 shadow-sm transition-all hover:border-slate-200 hover:shadow-md lg:hidden"
    >
      <SlidersHorizontal className="h-4 w-4" />
      Filters
      {count != null && count > 0 && (
        <span className="rounded-full bg-accent-indigo-100 px-1.5 py-0.5 text-xs font-semibold text-accent-indigo-700">{count}</span>
      )}
      <ChevronDown className="h-4 w-4 text-slate-400" />
    </button>
  );
}
