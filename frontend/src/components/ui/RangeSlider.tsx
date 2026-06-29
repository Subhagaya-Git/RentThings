import { cn } from '@/lib/utils';

interface RangeSliderProps {
  min: number;
  max: number;
  value: [number, number];
  onChange: (value: [number, number]) => void;
  step?: number;
  formatLabel?: (value: number) => string;
  className?: string;
}

export function RangeSlider({ min, max, value, onChange, step = 100, formatLabel, className }: RangeSliderProps) {
  const [low, high] = value;
  const lowPct = ((low - min) / (max - min)) * 100;
  const highPct = ((high - min) / (max - min)) * 100;

  const handleLow = (v: number) => onChange([Math.min(v, high - step), high]);
  const handleHigh = (v: number) => onChange([low, Math.max(v, low + step)]);

  return (
    <div className={cn('space-y-3', className)}>
      <div className="flex items-center justify-between text-sm">
        <span className="font-medium text-slate-700">{formatLabel ? formatLabel(low) : low}</span>
        <span className="text-slate-400">—</span>
        <span className="font-medium text-slate-700">{formatLabel ? formatLabel(high) : high}</span>
      </div>
      <div className="relative h-2 rounded-full bg-slate-100">
        <div
          className="absolute h-full rounded-full bg-accent-indigo-200"
          style={{ left: `${lowPct}%`, right: `${100 - highPct}%` }}
        />
        <input
          type="range"
          min={min}
          max={max}
          step={step}
          value={low}
          onChange={(e) => handleLow(Number(e.target.value))}
          className="range-thumb absolute inset-0 w-full cursor-pointer appearance-none bg-transparent"
          aria-label="Minimum price"
        />
        <input
          type="range"
          min={min}
          max={max}
          step={step}
          value={high}
          onChange={(e) => handleHigh(Number(e.target.value))}
          className="range-thumb absolute inset-0 w-full cursor-pointer appearance-none bg-transparent"
          aria-label="Maximum price"
        />
      </div>
    </div>
  );
}
