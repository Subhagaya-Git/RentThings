import { Calendar, Shield, Zap } from 'lucide-react';
import { Button } from '@/components/ui';
import { formatCurrency } from '@/lib/utils';

interface BookingCardProps {
  pricePerDay: number;
  deposit: number;
  startDate: string;
  endDate: string;
  message: string;
  days: number;
  total: number;
  canBook: boolean;
  isLoading: boolean;
  buttonLabel: string;
  onStartDateChange: (v: string) => void;
  onEndDateChange: (v: string) => void;
  onMessageChange: (v: string) => void;
  onBook: () => void;
}

export function BookingCard({
  pricePerDay,
  deposit,
  startDate,
  endDate,
  message,
  days,
  total,
  canBook,
  isLoading,
  buttonLabel,
  onStartDateChange,
  onEndDateChange,
  onMessageChange,
  onBook,
}: BookingCardProps) {
  return (
    <div className="rounded-3xl border border-slate-100 bg-white p-8 shadow-xl shadow-slate-200/50">
      <div className="flex items-baseline gap-1">
        <span className="text-4xl font-bold tracking-tight text-slate-900">{formatCurrency(pricePerDay)}</span>
        <span className="text-lg text-slate-500">/day</span>
      </div>

      <div className="mt-8 space-y-4">
        <div>
          <label className="flex items-center gap-2 text-sm font-bold text-slate-700 mb-2">
            <Calendar className="h-4 w-4 text-slate-400" /> Rental dates
          </label>
          <div className="grid grid-cols-2 gap-3">
            <input
              type="date"
              value={startDate}
              onChange={(e) => onStartDateChange(e.target.value)}
              className="w-full rounded-xl border border-slate-200 bg-slate-50 px-4 py-3 text-sm text-slate-900 focus:border-primary-400 focus:ring-2 focus:ring-primary-100 outline-none transition-all"
              aria-label="Start date"
            />
            <input
              type="date"
              value={endDate}
              onChange={(e) => onEndDateChange(e.target.value)}
              min={startDate}
              className="w-full rounded-xl border border-slate-200 bg-slate-50 px-4 py-3 text-sm text-slate-900 focus:border-primary-400 focus:ring-2 focus:ring-primary-100 outline-none transition-all"
              aria-label="End date"
            />
          </div>
        </div>

        <textarea
          value={message}
          onChange={(e) => onMessageChange(e.target.value)}
          placeholder="Message to owner (optional)"
          className="w-full rounded-xl border border-slate-200 bg-slate-50 px-4 py-3 text-sm text-slate-900 focus:border-primary-400 focus:ring-2 focus:ring-primary-100 outline-none transition-all min-h-[100px] resize-none"
        />
      </div>

      <div className="mt-6 space-y-3 rounded-2xl border border-slate-100 bg-slate-50/80 p-5 text-sm">
        <div className="flex justify-between text-slate-600">
          <span>{formatCurrency(pricePerDay)} × {days || '—'} days</span>
          <span className="font-semibold text-slate-900">{days ? formatCurrency(total) : '—'}</span>
        </div>
        <div className="flex justify-between text-slate-600">
          <span>Refundable deposit</span>
          <span className="font-semibold text-slate-900">{formatCurrency(deposit)}</span>
        </div>
        <div className="flex justify-between border-t border-slate-200 pt-3 text-base font-bold text-slate-900">
          <span>Total due</span>
          <span>{days ? formatCurrency(total + deposit) : '—'}</span>
        </div>
      </div>

      <Button
        className="mt-8 w-full rounded-xl bg-primary-600 py-4 text-base font-bold shadow-lg shadow-primary-600/25 hover:bg-primary-700 transition-all active:scale-95 text-white"
        loading={isLoading}
        disabled={!canBook || !startDate || !endDate}
        onClick={onBook}
      >
        <Zap className="h-4 w-4 mr-2" />
        {buttonLabel}
      </Button>

      <p className="mt-4 flex items-center justify-center gap-1.5 text-xs font-medium text-slate-400">
        <Shield className="h-4 w-4" /> Protected by RentThings Trust Score
      </p>
    </div>
  );
}
