import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { Bell, Calendar, Heart, Star, ChevronRight } from 'lucide-react';
import { Button, Card, EmptyState, Input, Textarea, TrustBadge } from '@/components/ui';
import { StatusBadge } from '@/components/ui/StatusBadge';
import { notificationsApi, rentalsApi, favoritesApi, reviewsApi } from '@/lib/api';
import { formatCurrency, formatDate } from '@/lib/utils';
import { useAuthStore } from '@/stores';

const ACTIVE = ['Active', 'Approved', 'HandedOver'];
const PENDING = ['Requested', 'PaymentPending'];
const PAST = ['Completed', 'Reviewed', 'Rejected', 'Cancelled', 'Returned'];

export default function DashboardPage() {
  const { user } = useAuthStore();
  const { data: rentals } = useQuery({ queryKey: ['my-rentals'], queryFn: rentalsApi.myRentals });
  const { data: notifications } = useQuery({ queryKey: ['notifications'], queryFn: notificationsApi.getAll });
  const { data: favorites } = useQuery({ queryKey: ['favorites'], queryFn: favoritesApi.getAll });

  const activeRentals = rentals?.filter((r) => ACTIVE.includes(r.status)) ?? [];

  return (
    <div className="space-y-8">
      {user && (
        <Card className="bg-gradient-to-r from-primary-600 to-primary-500 text-white border-0 shadow-lg shadow-primary-500/20 p-8 rounded-3xl">
          <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-6">
            <div>
              <h2 className="text-3xl font-bold tracking-tight">Hello, {user.firstName}!</h2>
              <p className="text-primary-100 mt-2 text-lg">Manage your rentals and account</p>
            </div>
            <div className="bg-white/10 p-4 rounded-2xl backdrop-blur-sm border border-white/20">
              <div className="text-sm text-primary-100 mb-1 font-medium">Your Trust Score</div>
              <TrustBadge level={user.trustLevel} score={user.trustScore} />
            </div>
          </div>
        </Card>
      )}

      <div className="grid sm:grid-cols-2 lg:grid-cols-4 gap-6">
        {[
          { icon: Calendar, label: 'Active rentals', value: activeRentals.length, href: '/renter/rentals', color: 'bg-indigo-50 text-indigo-600' },
          { icon: Bell, label: 'Notifications', value: notifications?.filter((n) => !n.isRead).length ?? 0, href: '/renter/notifications', color: 'bg-amber-50 text-amber-600' },
          { icon: Heart, label: 'Favorites', value: favorites?.length ?? 0, href: '/renter/favorites', color: 'bg-rose-50 text-rose-600' },
          { icon: Star, label: 'Trust score', value: user?.trustScore ?? 0, href: '/renter/profile', color: 'bg-emerald-50 text-emerald-600' },
        ].map((stat) => (
          <Link key={stat.label} to={stat.href} className="group">
            <Card className="hover:shadow-xl transition-all duration-300 hover:-translate-y-1 rounded-3xl p-6 border-slate-100">
              <div className="flex items-center justify-between mb-4">
                <div className={`p-3 rounded-2xl ${stat.color}`}>
                  <stat.icon className="h-6 w-6" aria-hidden="true" />
                </div>
                <ChevronRight className="h-5 w-5 text-slate-300 group-hover:text-primary-500 transition-colors" />
              </div>
              <div className="text-3xl font-bold text-slate-900 tracking-tight">{stat.value}</div>
              <div className="text-sm font-medium text-slate-500 mt-1">{stat.label}</div>
            </Card>
          </Link>
        ))}
      </div>

      <Card className="rounded-3xl border-slate-100 p-8 shadow-sm">
        <h3 className="text-xl font-bold text-slate-900 tracking-tight mb-6">Recent rentals</h3>
        {!rentals?.length ? (
          <EmptyState title="No rentals yet" description="Browse listings and request your first rental." action={<Link to="/search" className="text-primary-600 font-bold hover:underline">Browse listings →</Link>} />
        ) : (
          <div className="space-y-4">
            {rentals.slice(0, 5).map((r) => (
              <div key={r.id} className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 p-5 rounded-2xl bg-slate-50 border border-slate-100 hover:border-slate-200 transition-colors">
                <div>
                  <div className="font-bold text-slate-900 text-base">{r.listingTitle}</div>
                  <div className="text-sm font-medium text-slate-500 mt-1">{formatDate(r.startDate)} – {formatDate(r.endDate)}</div>
                </div>
                <div className="flex items-center gap-4">
                  <span className="text-base font-bold text-slate-900">{formatCurrency(r.totalPrice)}</span>
                  <StatusBadge status={r.status} />
                </div>
              </div>
            ))}
          </div>
        )}
      </Card>
    </div>
  );
}

export function RentalsPage() {
  const [tab, setTab] = useState<'active' | 'pending' | 'past' | 'reviews'>('active');
  const [reviewRental, setReviewRental] = useState<string | null>(null);
  const [rating, setRating] = useState(5);
  const [comment, setComment] = useState('');
  const qc = useQueryClient();

  const { data: rentals, isLoading } = useQuery({ queryKey: ['my-rentals'], queryFn: rentalsApi.myRentals });

  const statusMutation = useMutation({
    mutationFn: ({ id, status }: { id: string; status: string }) => rentalsApi.updateStatus(id, status),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['my-rentals'] }),
  });

  const reviewMutation = useMutation({
    mutationFn: () => reviewsApi.create({ rentalId: reviewRental!, rating, comment }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['my-rentals'] });
      setReviewRental(null);
      setComment('');
    },
  });

  if (isLoading) return <div className="animate-pulse text-slate-400">Loading...</div>;

  const filtered = {
    active: rentals?.filter((r) => ACTIVE.includes(r.status)) ?? [],
    pending: rentals?.filter((r) => PENDING.includes(r.status)) ?? [],
    past: rentals?.filter((r) => PAST.includes(r.status)) ?? [],
    reviews: rentals?.filter((r) => ['Reviewed', 'Completed'].includes(r.status)) ?? [],
  };

  const tabs = [
    { id: 'active' as const, label: 'Active Rentals', count: filtered.active.length },
    { id: 'pending' as const, label: 'Pending Requests', count: filtered.pending.length },
    { id: 'past' as const, label: 'Past Rentals', count: filtered.past.length },
    { id: 'reviews' as const, label: 'Reviews Given', count: filtered.reviews.length },
  ];

  const current = filtered[tab];

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap gap-3">
        {tabs.map((t) => (
          <button
            key={t.id}
            type="button"
            onClick={() => setTab(t.id)}
            className={`rounded-full px-5 py-2.5 text-sm font-bold transition-all ${tab === t.id ? 'bg-primary-600 text-white shadow-md shadow-primary-600/20' : 'bg-white text-slate-600 border border-slate-200 hover:border-slate-300 hover:bg-slate-50'}`}
          >
            {t.label} <span className={`ml-1 px-2 py-0.5 rounded-full text-xs ${tab === t.id ? 'bg-white/20' : 'bg-slate-100'}`}>{t.count}</span>
          </button>
        ))}
      </div>

      <Card className="rounded-3xl border-slate-100 p-8 shadow-sm">
        {!current.length ? (
          <EmptyState
            title={`No ${tabs.find((t) => t.id === tab)?.label.toLowerCase()}`}
            description={tab === 'active' ? 'Your active rentals will appear here.' : 'Nothing to show in this tab yet.'}
            action={tab === 'pending' ? <Link to="/search" className="text-primary-600 font-bold hover:underline">Browse listings →</Link> : undefined}
          />
        ) : (
          <div className="space-y-4">
            {current.map((r) => (
              <div key={r.id} className="p-6 rounded-2xl bg-white border border-slate-100 shadow-sm space-y-4">
                <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
                  <div>
                    <div className="font-bold text-lg text-slate-900">{r.listingTitle}</div>
                    <div className="text-sm font-medium text-slate-500 mt-1">{formatDate(r.startDate)} – {formatDate(r.endDate)} <span className="mx-2">•</span> {formatCurrency(r.totalPrice)}</div>
                  </div>
                  <StatusBadge status={r.status} />
                </div>
                {r.rejectionReason && (
                  <p className="text-sm text-rose-700 bg-rose-50 border border-rose-100 rounded-xl px-4 py-3 font-medium">Rejection reason: {r.rejectionReason}</p>
                )}
                <div className="flex flex-wrap gap-3 pt-2">
                  {['HandedOver', 'Active'].includes(r.status) && (
                    <Button size="sm" className="bg-primary-600 hover:bg-primary-700 font-bold" loading={statusMutation.isPending} onClick={() => statusMutation.mutate({ id: r.id, status: 'Returned' })}>
                      Mark returned
                    </Button>
                  )}
                  {r.status === 'Returned' && (
                    <Button size="sm" className="bg-primary-600 hover:bg-primary-700 font-bold" onClick={() => setReviewRental(r.id)}>Leave review</Button>
                  )}
                </div>
              </div>
            ))}
          </div>
        )}
      </Card>

      {reviewRental && (
        <Card className="rounded-3xl border-slate-100 p-8 shadow-xl border-t-4 border-t-primary-500">
          <h3 className="text-xl font-bold text-slate-900 mb-6">Leave a review</h3>
          <div className="space-y-5">
            <div>
              <label className="text-sm font-bold text-slate-700">Rating (1-5)</label>
              <Input type="number" min={1} max={5} value={rating} onChange={(e) => setRating(+e.target.value)} className="mt-2 w-32 h-12" />
            </div>
            <Textarea value={comment} onChange={(e) => setComment(e.target.value)} placeholder="Share your experience..." className="min-h-[120px]" />
            <div className="flex gap-3 pt-2">
              <Button loading={reviewMutation.isPending} onClick={() => reviewMutation.mutate()} className="bg-primary-600 hover:bg-primary-700 font-bold px-6">Submit review</Button>
              <Button variant="ghost" onClick={() => setReviewRental(null)} className="font-bold">Cancel</Button>
            </div>
          </div>
        </Card>
      )}
    </div>
  );
}

export function NotificationsPage() {
  const qc = useQueryClient();
  const { data: notifications } = useQuery({ queryKey: ['notifications'], queryFn: notificationsApi.getAll });

  return (
    <Card className="rounded-3xl border-slate-100 p-8 shadow-sm">
      <div className="flex justify-between items-center mb-8">
        <h2 className="text-xl font-bold text-slate-900">Messages & Alerts</h2>
        <Button size="sm" variant="secondary" className="font-bold border-slate-200" onClick={() => notificationsApi.markAllRead().then(() => qc.invalidateQueries({ queryKey: ['notifications'] }))}>
          Mark all read
        </Button>
      </div>
      {!notifications?.length ? (
        <EmptyState title="All caught up" description="No notifications yet." />
      ) : (
        <div className="space-y-3">
          {notifications.map((n) => (
            <div key={n.id} className={`p-5 rounded-2xl border transition-colors ${n.isRead ? 'bg-white border-slate-100' : 'bg-primary-50 border-primary-100'}`}>
              <div className="flex items-start justify-between gap-4">
                <div>
                  <div className={`font-bold text-base ${n.isRead ? 'text-slate-700' : 'text-slate-900'}`}>{n.title}</div>
                  <p className={`text-sm mt-1 leading-relaxed ${n.isRead ? 'text-slate-500' : 'text-slate-700 font-medium'}`}>{n.message}</p>
                </div>
                <div className="text-xs font-medium text-slate-400 whitespace-nowrap">{formatDate(n.createdAt)}</div>
              </div>
            </div>
          ))}
        </div>
      )}
    </Card>
  );
}

export function FavoritesPage() {
  const { data: favorites } = useQuery({ queryKey: ['favorites'], queryFn: favoritesApi.getAll });

  return (
    <div className="space-y-6">
      <h2 className="text-2xl font-bold tracking-tight text-slate-900">Saved Favorites</h2>
      {!favorites?.length ? (
        <EmptyState title="No favorites" description="Save listings you love for later." action={<Link to="/search" className="text-primary-600 font-bold hover:underline">Browse listings →</Link>} />
      ) : (
        <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-6">
          {favorites.map((l) => (
            <Link key={l.id} to={`/listings/${l.id}`} className="group block">
              <Card className="rounded-2xl p-5 border-slate-100 hover:border-primary-200 hover:shadow-lg transition-all duration-300">
                <div className="font-bold text-slate-900 group-hover:text-primary-600 transition-colors">{l.title}</div>
                <div className="text-sm font-medium text-slate-500 mt-2">{formatCurrency(l.pricePerDay)}/day</div>
              </Card>
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}

export function ProfilePage() {
  const { user } = useAuthStore();
  if (!user) return null;

  return (
    <Card className="rounded-3xl border-slate-100 p-8 shadow-sm">
      <h2 className="text-2xl font-bold tracking-tight text-slate-900 mb-8">Your Profile</h2>
      <dl className="grid sm:grid-cols-2 gap-x-8 gap-y-6 text-sm">
        <div className="bg-slate-50 p-4 rounded-xl border border-slate-100"><dt className="text-slate-500 font-medium">Name</dt><dd className="font-bold text-base text-slate-900 mt-1">{user.firstName} {user.lastName}</dd></div>
        <div className="bg-slate-50 p-4 rounded-xl border border-slate-100"><dt className="text-slate-500 font-medium">Email</dt><dd className="font-bold text-base text-slate-900 mt-1">{user.email}</dd></div>
        <div className="bg-slate-50 p-4 rounded-xl border border-slate-100"><dt className="text-slate-500 font-medium">Role</dt><dd className="font-bold text-base text-slate-900 mt-1">{user.role}</dd></div>
        <div className="bg-slate-50 p-4 rounded-xl border border-slate-100"><dt className="text-slate-500 font-medium mb-2">Trust Level</dt><dd><TrustBadge level={user.trustLevel} score={user.trustScore} /></dd></div>
        <div className="bg-slate-50 p-4 rounded-xl border border-slate-100"><dt className="text-slate-500 font-medium">Location</dt><dd className="font-bold text-base text-slate-900 mt-1">{user.location || 'Not set'}</dd></div>
        <div className="bg-slate-50 p-4 rounded-xl border border-slate-100"><dt className="text-slate-500 font-medium">Verified Status</dt><dd className="font-bold text-base text-emerald-600 mt-1">{user.isVerified ? 'Verified ✓' : 'Unverified'}</dd></div>
      </dl>
    </Card>
  );
}
