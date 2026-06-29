import { cn } from '@/lib/utils';

const statusStyles: Record<string, string> = {
  Requested: 'bg-amber-100 text-amber-800 border-amber-200',
  Pending: 'bg-amber-100 text-amber-800 border-amber-200',
  Approved: 'bg-blue-100 text-blue-800 border-blue-200',
  HandedOver: 'bg-green-100 text-green-800 border-green-200',
  Active: 'bg-green-100 text-green-800 border-green-200',
  Returned: 'bg-teal-100 text-teal-800 border-teal-200',
  Reviewed: 'bg-slate-100 text-slate-600 border-slate-200',
  Completed: 'bg-slate-100 text-slate-600 border-slate-200',
  Rejected: 'bg-red-100 text-red-800 border-red-200',
  Cancelled: 'bg-red-100 text-red-800 border-red-200',
  PaymentPending: 'bg-purple-100 text-purple-800 border-purple-200',
  Draft: 'bg-slate-100 text-slate-600 border-slate-200',
  PendingReview: 'bg-amber-100 text-amber-800 border-amber-200',
  Inactive: 'bg-slate-100 text-slate-500 border-slate-200',
  Flagged: 'bg-red-100 text-red-800 border-red-200',
};

const statusLabels: Record<string, string> = {
  Requested: 'Pending',
  HandedOver: 'Active',
  Reviewed: 'Completed',
};

export function StatusBadge({ status, className }: { status: string; className?: string }) {
  const label = statusLabels[status] ?? status.replace(/([A-Z])/g, ' $1').trim();
  return (
    <span className={cn('inline-flex items-center rounded-full border px-2.5 py-0.5 text-xs font-medium', statusStyles[status] ?? 'bg-slate-100 text-slate-600 border-slate-200', className)}>
      {label}
    </span>
  );
}
