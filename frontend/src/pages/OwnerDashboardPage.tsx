import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { DollarSign, Package, Plus, Pencil, Power, Inbox, CheckCircle, Clock, Trash2 } from 'lucide-react';
import { Button, Card, EmptyState } from '@/components/ui';
import { StatusBadge } from '@/components/ui/StatusBadge';
import { listingsApi, rentalsApi } from '@/lib/api';
import { formatCurrency, formatDate } from '@/lib/utils';

type Tab = 'listings' | 'requests' | 'rentals' | 'earnings';

export default function OwnerDashboardPage({ tab = 'listings' }: { tab?: Tab }) {
  const { data: dashboard } = useQuery({ queryKey: ['owner-dashboard'], queryFn: listingsApi.ownerDashboard });
  const qc = useQueryClient();
  const [rejectId, setRejectId] = useState<string | null>(null);
  const [rejectReason, setRejectReason] = useState('');

  const statusMutation = useMutation({
    mutationFn: ({ id, status, notes }: { id: string; status: string; notes?: string }) =>
      rentalsApi.updateStatus(id, status, notes),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['owner-dashboard'] });
      qc.invalidateQueries({ queryKey: ['owner-requests'] });
      setRejectId(null);
      setRejectReason('');
    },
  });

  const deactivateMutation = useMutation({
    mutationFn: (id: string) => listingsApi.delete(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['owner-dashboard'] }),
  });

  const listings = dashboard?.listings ?? [];
  const requests = dashboard?.requests ?? [];
  const activeRentals = dashboard?.activeRentalsList ?? [];
  const pending = requests.filter((r) => r.status === 'Requested');
  const activeListings = listings.filter((l) => l.status === 'Active');
  const inactiveListings = listings.filter((l) => l.status !== 'Active');

  const titles: Record<Tab, string> = {
    listings: 'My Listings',
    requests: 'Incoming Requests',
    rentals: 'Active Rentals',
    earnings: 'Earnings Summary',
  };

  return (
    <div className="space-y-8">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 bg-white p-6 rounded-3xl border border-slate-100 shadow-sm">
        <div>
          <h2 className="text-2xl font-bold text-slate-900 tracking-tight">{titles[tab]}</h2>
          <p className="text-sm font-medium text-slate-500 mt-1">Manage your owner account</p>
        </div>
        {tab === 'listings' && (
          <Link to="/owner/listings/new">
            <Button className="bg-primary-600 hover:bg-primary-700 shadow-md shadow-primary-600/20 text-white font-bold rounded-xl h-11 px-5">
              <Plus className="h-4 w-4 mr-2" /> New listing
            </Button>
          </Link>
        )}
      </div>

      {tab === 'listings' && (
        <>
          <div className="grid sm:grid-cols-3 gap-6">
            <Card className="rounded-3xl border-slate-100 p-6 shadow-sm hover:shadow-md transition-shadow">
              <div className="flex justify-between items-start mb-4">
                <div className="p-3 bg-primary-50 rounded-2xl">
                  <Package className="h-6 w-6 text-primary-600" />
                </div>
              </div>
              <div className="text-4xl font-bold text-slate-900 tracking-tight">{dashboard?.activeListings ?? 0}</div>
              <div className="text-sm font-medium text-slate-500 mt-2">Active listings</div>
            </Card>
            
            <Card className="rounded-3xl border-slate-100 p-6 shadow-sm hover:shadow-md transition-shadow">
              <div className="flex justify-between items-start mb-4">
                <div className="p-3 bg-slate-100 rounded-2xl">
                  <Power className="h-6 w-6 text-slate-600" />
                </div>
              </div>
              <div className="text-4xl font-bold text-slate-900 tracking-tight">{dashboard?.inactiveListings ?? 0}</div>
              <div className="text-sm font-medium text-slate-500 mt-2">Inactive listings</div>
            </Card>

            <Card className="rounded-3xl border-slate-100 p-6 shadow-sm hover:shadow-md transition-shadow">
              <div className="flex justify-between items-start mb-4">
                <div className="p-3 bg-amber-50 rounded-2xl">
                  <Inbox className="h-6 w-6 text-amber-600" />
                </div>
              </div>
              <div className="text-4xl font-bold text-slate-900 tracking-tight">{pending.length}</div>
              <div className="text-sm font-medium text-slate-500 mt-2">Pending requests</div>
            </Card>
          </div>

          <Card className="rounded-3xl border-slate-100 p-8 shadow-sm">
            <h3 className="text-xl font-bold text-slate-900 mb-6">Active listings</h3>
            {!activeListings.length ? (
              <EmptyState title="No active listings" description="Create your first listing to start earning." action={<Link to="/owner/listings/new" className="text-primary-600 font-bold hover:underline">Create listing →</Link>} />
            ) : (
              <div className="space-y-3">
                {activeListings.map((l) => (
                  <div key={l.id} className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 p-5 rounded-2xl border border-slate-100 bg-white hover:border-primary-100 hover:shadow-sm transition-all">
                    <Link to={`/listings/${l.id}`} className="font-bold text-slate-900 hover:text-primary-600 transition-colors text-base">{l.title}</Link>
                    <div className="flex items-center gap-4">
                      <StatusBadge status={l.status} />
                      <span className="text-base font-bold text-primary-700 bg-primary-50 px-3 py-1 rounded-lg">{formatCurrency(l.pricePerDay)}/day</span>
                      <Link to={`/owner/listings/${l.id}/edit`}>
                        <Button size="sm" variant="ghost" className="h-9 w-9 p-0 rounded-xl bg-slate-50 hover:bg-slate-100 text-slate-600"><Pencil className="h-4 w-4" /></Button>
                      </Link>
                      <Button size="sm" variant="secondary" className="rounded-xl font-bold text-rose-600 bg-rose-50 hover:bg-rose-100 border-0" loading={deactivateMutation.isPending} onClick={() => deactivateMutation.mutate(l.id)}>
                        <Power className="h-4 w-4 mr-1.5" /> Deactivate
                      </Button>
                      <button 
                        onClick={(e) => {
                          e.preventDefault();
                          if (window.confirm("Are you sure you want to delete this listing?")) {
                            listingsApi.delete(l.id).then(() => qc.invalidateQueries({ queryKey: ['owner-dashboard'] }));
                          }
                        }}
                        className="bg-red-50 hover:bg-red-100 text-red-600 p-2 rounded-xl border border-red-200 transition-colors shadow-sm flex items-center justify-center"
                        title="Delete Listing"
                      >
                        <Trash2 className="w-4 h-4" />
                      </button>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </Card>

          {inactiveListings.length > 0 && (
            <Card className="rounded-3xl border-slate-100 p-8 shadow-sm bg-slate-50/50">
              <h3 className="text-lg font-bold text-slate-900 mb-4">Inactive listings</h3>
              <div className="space-y-3">
                {inactiveListings.map((l) => (
                  <div key={l.id} className="flex justify-between items-center p-4 rounded-2xl bg-white border border-slate-100 text-sm">
                    <span className="font-medium text-slate-600">{l.title}</span>
                    <div className="flex items-center gap-4">
                      <StatusBadge status={l.status} />
                      <button 
                        onClick={(e) => {
                          e.preventDefault();
                          if (window.confirm("Are you sure you want to delete this listing?")) {
                            listingsApi.delete(l.id).then(() => qc.invalidateQueries({ queryKey: ['owner-dashboard'] }));
                          }
                        }}
                        className="bg-red-50 hover:bg-red-100 text-red-600 p-1.5 rounded-lg border border-red-200 transition-colors shadow-sm flex items-center justify-center"
                        title="Delete Listing"
                      >
                        <Trash2 className="w-4 h-4" />
                      </button>
                    </div>
                  </div>
                ))}
              </div>
            </Card>
          )}
        </>
      )}

      {tab === 'requests' && (
        <Card className="rounded-3xl border-slate-100 p-8 shadow-sm">
          {!requests.length ? (
            <EmptyState title="No requests" description="Rental requests from renters will appear here." />
          ) : (
            <div className="space-y-4">
              {requests.map((r) => (
                <div key={r.id} className="p-6 rounded-2xl border border-slate-100 bg-white shadow-sm space-y-4 transition-all hover:shadow-md">
                  <div className="flex flex-col sm:flex-row sm:items-start justify-between gap-4">
                    <div>
                      <div className="font-bold text-lg text-slate-900">{r.listingTitle}</div>
                      <div className="text-sm font-medium text-slate-500 mt-1 flex items-center gap-2">
                        <span className="text-slate-700 bg-slate-100 px-2 py-0.5 rounded-md">From {r.renterName}</span>
                        <span>{formatDate(r.startDate)} – {formatDate(r.endDate)}</span>
                        <span className="font-bold text-slate-900">{formatCurrency(r.totalPrice)}</span>
                      </div>
                      {r.message && <p className="text-sm text-slate-700 mt-3 p-3 bg-slate-50 rounded-xl italic border border-slate-100">"{r.message}"</p>}
                    </div>
                    <StatusBadge status={r.status} />
                  </div>
                  {r.status === 'Requested' && (
                    <div className="flex flex-wrap gap-3 pt-2">
                      <Button size="sm" className="bg-primary-600 hover:bg-primary-700 font-bold px-5 rounded-xl" loading={statusMutation.isPending} onClick={() => statusMutation.mutate({ id: r.id, status: 'Approved' })}>
                        Approve Request
                      </Button>
                      <Button size="sm" variant="danger" className="font-bold px-5 rounded-xl border-0" onClick={() => setRejectId(r.id)}>
                        Reject
                      </Button>
                    </div>
                  )}
                  {r.status === 'Approved' && (
                    <div className="pt-2">
                      <Button size="sm" className="bg-emerald-600 hover:bg-emerald-700 text-white font-bold px-5 rounded-xl" onClick={() => statusMutation.mutate({ id: r.id, status: 'HandedOver' })}>
                        <CheckCircle className="h-4 w-4 mr-2" /> Mark as Handed Over
                      </Button>
                    </div>
                  )}
                  {rejectId === r.id && (
                    <div className="flex flex-col sm:flex-row gap-3 pt-4 border-t border-slate-100">
                      <input
                        value={rejectReason}
                        onChange={(e) => setRejectReason(e.target.value)}
                        placeholder="Reason for rejection (optional)"
                        className="flex-1 rounded-xl border border-slate-200 px-4 py-2 text-sm bg-slate-50 focus:bg-white focus:ring-2 focus:ring-primary-100 outline-none transition-all"
                      />
                      <Button size="sm" variant="danger" className="font-bold rounded-xl" onClick={() => statusMutation.mutate({ id: r.id, status: 'Rejected', notes: rejectReason })}>Confirm Rejection</Button>
                      <Button size="sm" variant="ghost" className="font-bold rounded-xl" onClick={() => setRejectId(null)}>Cancel</Button>
                    </div>
                  )}
                </div>
              ))}
            </div>
          )}
        </Card>
      )}

      {tab === 'rentals' && (
        <Card className="rounded-3xl border-slate-100 p-8 shadow-sm">
          {!activeRentals.length ? (
            <EmptyState title="No active rentals" description="Approved and in-progress rentals appear here." />
          ) : (
            <div className="space-y-4">
              {activeRentals.map((r) => (
                <div key={r.id} className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 p-6 rounded-2xl border border-slate-100 bg-white hover:shadow-sm transition-all">
                  <div>
                    <div className="font-bold text-lg text-slate-900">{r.listingTitle}</div>
                    <div className="text-sm font-medium text-slate-500 mt-1 flex items-center gap-2">
                      <span className="text-slate-700 bg-slate-100 px-2 py-0.5 rounded-md">{r.renterName}</span>
                      <Clock className="h-3 w-3 text-slate-400" />
                      <span>{formatDate(r.startDate)} – {formatDate(r.endDate)}</span>
                    </div>
                  </div>
                  <div className="flex items-center gap-3">
                    <StatusBadge status={r.status} />
                    {r.status === 'Approved' && (
                      <Button size="sm" className="bg-emerald-600 hover:bg-emerald-700 font-bold rounded-xl px-4" onClick={() => statusMutation.mutate({ id: r.id, status: 'HandedOver' })}>
                        Hand over item
                      </Button>
                    )}
                  </div>
                </div>
              ))}
            </div>
          )}
        </Card>
      )}

      {tab === 'earnings' && (
        <div className="space-y-6">
          <Card className="bg-gradient-to-br from-emerald-600 to-emerald-500 text-white border-0 shadow-lg shadow-emerald-500/20 p-8 rounded-3xl">
            <div className="flex items-center justify-between">
              <div>
                <div className="text-emerald-100 mb-2 font-medium flex items-center gap-2">
                  <DollarSign className="h-5 w-5 opacity-80" /> Total Earnings
                </div>
                <div className="text-5xl font-bold tracking-tight">{formatCurrency(dashboard?.totalEarnings ?? 0)}</div>
              </div>
            </div>
          </Card>
          
          <Card className="rounded-3xl border-slate-100 p-8 shadow-sm">
            <h3 className="text-xl font-bold text-slate-900 mb-6">Completed rentals history</h3>
            {!requests.filter((r) => ['Completed', 'Reviewed'].includes(r.status)).length ? (
              <EmptyState title="No earnings yet" description="Completed rentals will show here." />
            ) : (
              <div className="space-y-3">
                {requests.filter((r) => ['Completed', 'Reviewed'].includes(r.status)).map((r) => (
                  <div key={r.id} className="flex justify-between items-center p-4 rounded-2xl border border-slate-100 bg-white">
                    <span className="font-bold text-slate-700">{r.listingTitle}</span>
                    <span className="font-bold text-emerald-600 bg-emerald-50 px-3 py-1.5 rounded-lg text-base">{formatCurrency(r.totalPrice)}</span>
                  </div>
                ))}
              </div>
            )}
          </Card>
        </div>
      )}
    </div>
  );
}
