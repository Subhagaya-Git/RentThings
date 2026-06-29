import { MessageCircle, Shield } from 'lucide-react';
import { TrustBadge } from '@/components/ui';
import type { User } from '@/types';

interface OwnerProfileBlockProps {
  owner: User;
  responseRate?: number;
}

export function OwnerProfileBlock({ owner, responseRate = 98 }: OwnerProfileBlockProps) {
  const initials = `${owner.firstName[0]}${owner.lastName[0]}`;

  return (
    <div className="rounded-2xl border border-slate-100 bg-white p-6 shadow-sm">
      <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Hosted by</h2>
      <div className="mt-4 flex items-start gap-4">
        <div className="relative">
          {owner.avatarUrl ? (
            <img src={owner.avatarUrl} alt={owner.firstName} className="h-16 w-16 rounded-2xl object-cover ring-2 ring-white" />
          ) : (
            <div className="flex h-16 w-16 items-center justify-center rounded-2xl bg-gradient-to-br from-accent-indigo-100 to-accent-emerald-100 text-lg font-bold text-accent-indigo-700">
              {initials}
            </div>
          )}
          {owner.isVerified && (
            <div className="absolute -bottom-1 -right-1 flex h-6 w-6 items-center justify-center rounded-full bg-accent-emerald-500 text-white ring-2 ring-white">
              <Shield className="h-3 w-3" />
            </div>
          )}
        </div>
        <div className="flex-1 min-w-0">
          <h3 className="text-lg font-semibold text-slate-900">{owner.firstName} {owner.lastName}</h3>
          {owner.location && <p className="text-sm text-slate-500">{owner.location}</p>}
          <div className="mt-2 flex flex-wrap items-center gap-2">
            <TrustBadge level={owner.trustLevel} score={owner.trustScore} />
            <span className="inline-flex items-center gap-1 rounded-full border border-slate-100 bg-slate-50 px-2.5 py-0.5 text-xs font-medium text-slate-600">
              <MessageCircle className="h-3 w-3" />
              {responseRate}% response rate
            </span>
          </div>
        </div>
      </div>
      {owner.bio && <p className="mt-4 text-sm leading-relaxed text-slate-600">{owner.bio}</p>}
    </div>
  );
}
