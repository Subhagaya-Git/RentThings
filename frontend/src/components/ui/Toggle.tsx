import { cn } from '@/lib/utils';

interface ToggleProps {
  checked: boolean;
  onChange: (checked: boolean) => void;
  label: string;
  description?: string;
  className?: string;
}

export function Toggle({ checked, onChange, label, description, className }: ToggleProps) {
  return (
    <label className={cn('flex cursor-pointer items-start justify-between gap-3', className)}>
      <div>
        <span className="text-sm font-medium text-slate-700">{label}</span>
        {description && <p className="mt-0.5 text-xs text-slate-500">{description}</p>}
      </div>
      <button
        type="button"
        role="switch"
        aria-checked={checked}
        onClick={() => onChange(!checked)}
        className={cn(
          'relative inline-flex h-6 w-11 shrink-0 rounded-full transition-colors duration-200',
          checked ? 'bg-accent-emerald-500' : 'bg-slate-200',
        )}
      >
        <span
          className={cn(
            'inline-block h-5 w-5 translate-y-0.5 rounded-full bg-white shadow-sm transition-transform duration-200',
            checked ? 'translate-x-[22px]' : 'translate-x-0.5',
          )}
        />
      </button>
    </label>
  );
}
