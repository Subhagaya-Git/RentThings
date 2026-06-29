import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, LineChart, Line, PieChart, Pie, Cell } from 'recharts';
import { Card, EmptyState, Input, Skeleton, TrustBadge } from '@/components/ui';
import { StatusBadge } from '@/components/ui/StatusBadge';
import { Button } from '@/components/ui';
import { formatCurrency, formatDate } from '@/lib/utils';
import { adminApi, listingsApi } from '@/lib/api';
import { Users, LayoutDashboard, Flag, Activity, FileText, CheckCircle, Trash2 } from 'lucide-react';

type Tab = 'overview' | 'users' | 'listings' | 'transactions' | 'reports';

// Modern, vibrant color palette for charts
const PIE_COLORS = ['#4f46e5', '#06b6d4', '#10b981', '#f59e0b', '#f43f5e', '#8b5cf6'];
const CHART_PRIMARY = '#4f46e5'; // primary-600 equivalent

export default function AdminDashboardPage({ tab = 'overview' }: { tab?: Tab }) {
  const [userSearch, setUserSearch] = useState('');
  const [roleFilter, setRoleFilter] = useState('');
  const [editingTrust, setEditingTrust] = useState<{ id: string; score: number } | null>(null);
  const qc = useQueryClient();

  const { data: stats, isLoading } = useQuery({ queryKey: ['admin-stats'], queryFn: adminApi.stats });
  const { data: users } = useQuery({ queryKey: ['admin-users', userSearch, roleFilter], queryFn: () => adminApi.users(userSearch || undefined, roleFilter || undefined) });
  const { data: flagged } = useQuery({ queryKey: ['admin-flagged'], queryFn: adminApi.flaggedListings });
  const { data: rentals } = useQuery({ queryKey: ['admin-rentals'], queryFn: adminApi.rentals, enabled: tab === 'transactions' || tab === 'overview' });
  const { data: reports } = useQuery({ queryKey: ['admin-reports'], queryFn: adminApi.reports, enabled: tab === 'reports' || tab === 'overview' });

  const trustMutation = useMutation({
    mutationFn: ({ id, score }: { id: string; score: number }) => adminApi.updateTrustScore(id, score),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['admin-users'] }); setEditingTrust(null); },
  });

  const suspendMutation = useMutation({
    mutationFn: ({ id, isActive }: { id: string; isActive: boolean }) => adminApi.suspendUser(id, isActive),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['admin-users'] }),
  });

  const listingMutation = useMutation({
    mutationFn: ({ id, status }: { id: string; status: string }) => adminApi.updateListingStatus(id, status),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['admin-flagged'] }),
  });

  const resolveMutation = useMutation({
    mutationFn: (id: string) => adminApi.resolveReport(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['admin-reports'] }),
  });

  if (isLoading && tab === 'overview') return <div className="space-y-6">{Array.from({ length: 4 }).map((_, i) => <Skeleton key={i} className="h-32 w-full rounded-3xl" />)}</div>;

  if (tab === 'overview') {
    return (
      <div className="space-y-8">
        <div className="grid sm:grid-cols-2 lg:grid-cols-4 gap-6">
          {[
            { label: 'Total Users', value: stats?.totalUsers, icon: Users, color: 'text-indigo-600 bg-indigo-50' },
            { label: 'Active Listings', value: stats?.totalListings, icon: LayoutDashboard, color: 'text-emerald-600 bg-emerald-50' },
            { label: 'Active Rentals', value: stats?.activeRentals, icon: Activity, color: 'text-amber-600 bg-amber-50' },
            { label: 'Total Revenue', value: formatCurrency(stats?.totalRevenue ?? 0), icon: FileText, color: 'text-rose-600 bg-rose-50' },
          ].map((s) => (
            <Card key={s.label} className="rounded-3xl border-slate-100 p-6 shadow-sm hover:shadow-md transition-shadow">
              <div className="flex items-center justify-between mb-4">
                <div className={`p-3 rounded-2xl ${s.color}`}>
                  <s.icon className="h-6 w-6" />
                </div>
              </div>
              <div className="text-3xl font-bold text-slate-900 tracking-tight">{s.value}</div>
              <div className="text-sm font-medium text-slate-500 mt-2">{s.label}</div>
            </Card>
          ))}
        </div>

        <div className="grid lg:grid-cols-2 gap-8">
          <Card className="rounded-3xl border-slate-100 p-8 shadow-sm">
            <h3 className="text-xl font-bold text-slate-900 mb-6">Rentals by Category</h3>
            {(stats?.rentalsByCategory?.length ?? 0) > 0 ? (
              <ResponsiveContainer width="100%" height={300}>
                <BarChart data={stats?.rentalsByCategory} margin={{ top: 10, right: 10, left: -20, bottom: 0 }}>
                  <CartesianGrid strokeDasharray="3 3" stroke="#f1f5f9" vertical={false} />
                  <XAxis dataKey="category" tick={{ fontSize: 12, fill: '#64748b' }} axisLine={false} tickLine={false} />
                  <YAxis tick={{ fontSize: 12, fill: '#64748b' }} axisLine={false} tickLine={false} />
                  <Tooltip 
                    cursor={{ fill: '#f8fafc' }}
                    contentStyle={{ borderRadius: '16px', border: 'none', boxShadow: '0 10px 15px -3px rgb(0 0 0 / 0.1)' }}
                  />
                  <Bar dataKey="count" fill={CHART_PRIMARY} radius={[6, 6, 0, 0]} barSize={40} />
                </BarChart>
              </ResponsiveContainer>
            ) : <EmptyState title="No data" description="Rental category data will appear here." />}
          </Card>

          <Card className="rounded-3xl border-slate-100 p-8 shadow-sm">
            <h3 className="text-xl font-bold text-slate-900 mb-6">Revenue Trend</h3>
            <ResponsiveContainer width="100%" height={300}>
              <LineChart data={stats?.monthlyRentals ?? []} margin={{ top: 10, right: 10, left: -20, bottom: 0 }}>
                <CartesianGrid strokeDasharray="3 3" stroke="#f1f5f9" vertical={false} />
                <XAxis dataKey="month" tick={{ fontSize: 12, fill: '#64748b' }} axisLine={false} tickLine={false} />
                <YAxis tick={{ fontSize: 12, fill: '#64748b' }} axisLine={false} tickLine={false} />
                <Tooltip 
                  formatter={(v) => formatCurrency(Number(v))}
                  contentStyle={{ borderRadius: '16px', border: 'none', boxShadow: '0 10px 15px -3px rgb(0 0 0 / 0.1)' }}
                />
                <Line type="monotone" dataKey="revenue" stroke={CHART_PRIMARY} strokeWidth={3} dot={{ fill: CHART_PRIMARY, strokeWidth: 2, r: 4, stroke: '#fff' }} activeDot={{ r: 6 }} />
              </LineChart>
            </ResponsiveContainer>
          </Card>
        </div>

        <div className="grid lg:grid-cols-2 gap-8">
          <Card className="rounded-3xl border-slate-100 p-8 shadow-sm">
            <h3 className="text-xl font-bold text-slate-900 mb-6">Rentals by Status</h3>
            {(stats?.rentalsByStatus?.length ?? 0) > 0 ? (
              <ResponsiveContainer width="100%" height={300}>
                <PieChart>
                  <Pie 
                    data={stats?.rentalsByStatus} 
                    dataKey="count" 
                    nameKey="status" 
                    cx="50%" 
                    cy="50%" 
                    innerRadius={60}
                    outerRadius={100} 
                    paddingAngle={5}
                    label={({ name, percent }) => `${name} ${((percent ?? 0) * 100).toFixed(0)}%`}
                    labelLine={false}
                  >
                    {stats?.rentalsByStatus?.map((_, i) => <Cell key={i} fill={PIE_COLORS[i % PIE_COLORS.length]} stroke="transparent" />)}
                  </Pie>
                  <Tooltip contentStyle={{ borderRadius: '16px', border: 'none', boxShadow: '0 10px 15px -3px rgb(0 0 0 / 0.1)' }} />
                </PieChart>
              </ResponsiveContainer>
            ) : <EmptyState title="No data" description="Status breakdown will appear here." />}
          </Card>

          <Card className="rounded-3xl border-slate-100 p-8 shadow-sm">
            <div className="flex items-center justify-between mb-6">
              <h3 className="text-xl font-bold text-slate-900">Pending Reports</h3>
              <span className="bg-rose-100 text-rose-700 font-bold px-3 py-1 rounded-full text-sm">{stats?.pendingReports ?? 0}</span>
            </div>
            {!reports?.filter((r) => !r.isResolved).length ? (
              <EmptyState title="No pending reports" description="All reports resolved." />
            ) : (
              <div className="space-y-4 max-h-[300px] overflow-y-auto pr-2 custom-scrollbar">
                {reports?.filter((r) => !r.isResolved).slice(0, 5).map((r) => (
                  <div key={r.id} className="p-4 rounded-2xl bg-rose-50 border border-rose-100">
                    <div className="font-bold text-rose-900 flex items-center gap-2">
                      <Flag className="h-4 w-4" /> {r.reason}
                    </div>
                    <p className="text-rose-700/80 mt-2 text-sm font-medium">{r.description}</p>
                  </div>
                ))}
              </div>
            )}
          </Card>
        </div>
      </div>
    );
  }

  if (tab === 'users') {
    return (
      <div className="space-y-6">
        <div className="flex flex-col sm:flex-row gap-4 bg-white p-6 rounded-3xl border border-slate-100 shadow-sm">
          <Input placeholder="Search users..." value={userSearch} onChange={(e) => setUserSearch(e.target.value)} className="sm:max-w-md h-12 bg-slate-50 border-transparent focus:bg-white" />
          <select value={roleFilter} onChange={(e) => setRoleFilter(e.target.value)} className="h-12 rounded-xl border border-slate-200 px-4 text-sm font-medium text-slate-700 outline-none focus:ring-2 focus:ring-primary-100 transition-all bg-white hover:bg-slate-50 cursor-pointer min-w-[150px]">
            <option value="">All roles</option>
            <option value="Renter">Renter</option>
            <option value="Owner">Owner</option>
            <option value="Admin">Admin</option>
          </select>
        </div>
        <Card className="rounded-3xl border-slate-100 shadow-sm overflow-hidden p-0">
          {!users?.length ? (
            <div className="p-8">
              <EmptyState title="No users found" description="Try adjusting your search." />
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-slate-100 bg-slate-50/50">
                    <th className="px-6 py-4 text-left font-bold text-slate-500">User</th>
                    <th className="px-6 py-4 text-left font-bold text-slate-500">Role</th>
                    <th className="px-6 py-4 text-left font-bold text-slate-500">Trust Score</th>
                    <th className="px-6 py-4 text-left font-bold text-slate-500">Status</th>
                    <th className="px-6 py-4 text-left font-bold text-slate-500">Actions</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {users.map((u) => (
                    <tr key={u.id} className="hover:bg-slate-50/50 transition-colors">
                      <td className="px-6 py-4">
                        <div className="font-bold text-slate-900">{u.firstName} {u.lastName}</div>
                        <div className="text-slate-500 text-xs mt-1">{u.email}</div>
                      </td>
                      <td className="px-6 py-4 font-medium text-slate-700">{u.role}</td>
                      <td className="px-6 py-4">
                        {editingTrust?.id === u.id ? (
                          <div className="flex gap-2 items-center">
                            <Input type="number" min={0} max={100} value={editingTrust.score} onChange={(e) => setEditingTrust({ id: u.id, score: +e.target.value })} className="w-20 h-9 px-2" />
                            <Button size="sm" className="h-9 font-bold bg-primary-600 hover:bg-primary-700" onClick={() => trustMutation.mutate({ id: u.id, score: editingTrust.score })}>Save</Button>
                          </div>
                        ) : (
                          <button type="button" onClick={() => setEditingTrust({ id: u.id, score: u.trustScore })} className="hover:opacity-80 transition-opacity"><TrustBadge level={u.trustLevel} score={u.trustScore} /></button>
                        )}
                      </td>
                      <td className="px-6 py-4">
                        {u.isActive === false ? <span className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-bold bg-rose-100 text-rose-700"><span className="w-1.5 h-1.5 rounded-full bg-rose-500"></span>Suspended</span> : <span className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-bold bg-emerald-100 text-emerald-700"><span className="w-1.5 h-1.5 rounded-full bg-emerald-500"></span>Active</span>}
                      </td>
                      <td className="px-6 py-4">
                        <Button size="sm" className="font-bold rounded-lg" variant={u.isActive === false ? 'secondary' : 'danger'} onClick={() => suspendMutation.mutate({ id: u.id, isActive: u.isActive === false })}>
                          {u.isActive === false ? 'Reactivate' : 'Suspend'}
                        </Button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </Card>
      </div>
    );
  }

  if (tab === 'listings') {
    return (
      <Card className="rounded-3xl border-slate-100 p-8 shadow-sm">
        <h3 className="text-xl font-bold text-slate-900 mb-6">Flagged / Pending Listings</h3>
        {!flagged?.length ? (
          <EmptyState title="No flagged listings" description="All listings are in good standing." />
        ) : (
          <div className="space-y-4">
            {flagged.map((l) => (
              <div key={l.id} className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 p-6 rounded-2xl bg-amber-50 border border-amber-100 hover:shadow-md transition-shadow">
                <div>
                  <div className="font-bold text-lg text-slate-900">{l.title}</div>
                  <div className="text-sm font-medium text-slate-500 mt-1 mb-3">Owner: <span className="text-slate-700">{l.ownerName}</span></div>
                  <StatusBadge status={l.status} />
                </div>
                <div className="flex flex-wrap gap-3">
                  {l.status === 'PendingReview' && <Button size="sm" className="font-bold bg-emerald-600 hover:bg-emerald-700 px-5 rounded-xl" onClick={() => listingMutation.mutate({ id: l.id, status: 'Active' })}>Approve</Button>}
                  <Button size="sm" variant="secondary" className="font-bold px-5 rounded-xl border-0 bg-amber-200/50 hover:bg-amber-200 text-amber-900" onClick={() => listingMutation.mutate({ id: l.id, status: 'Flagged' })}>Flag</Button>
                  <Button size="sm" variant="danger" className="font-bold px-5 rounded-xl border-0" onClick={() => listingMutation.mutate({ id: l.id, status: 'Inactive' })}>Deactivate</Button>
                  <button 
                    onClick={(e) => {
                      e.preventDefault();
                      if (window.confirm("Are you sure you want to delete this listing permanently?")) {
                        listingsApi.delete(l.id).then(() => qc.invalidateQueries({ queryKey: ['admin-flagged'] }));
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
    );
  }

  if (tab === 'transactions') {
    return (
      <Card className="rounded-3xl border-slate-100 p-0 shadow-sm overflow-hidden">
        <div className="p-8 border-b border-slate-100">
          <h3 className="text-xl font-bold text-slate-900">Rental Transactions</h3>
        </div>
        {!rentals?.length ? (
          <div className="p-8">
            <EmptyState title="No transactions" description="Rental transactions will appear here." />
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="bg-slate-50/50 border-b border-slate-100">
                  <th className="px-8 py-4 text-left font-bold text-slate-500">Listing</th>
                  <th className="px-8 py-4 text-left font-bold text-slate-500">Renter</th>
                  <th className="px-8 py-4 text-left font-bold text-slate-500">Amount</th>
                  <th className="px-8 py-4 text-left font-bold text-slate-500">Status</th>
                  <th className="px-8 py-4 text-left font-bold text-slate-500">Date</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {rentals.map((r) => (
                  <tr key={r.id} className="hover:bg-slate-50/50 transition-colors">
                    <td className="px-8 py-4 font-bold text-slate-900">{r.listingTitle}</td>
                    <td className="px-8 py-4 font-medium text-slate-700">{r.renterName}</td>
                    <td className="px-8 py-4 font-bold text-slate-900">{formatCurrency(r.totalPrice)}</td>
                    <td className="px-8 py-4"><StatusBadge status={r.status} /></td>
                    <td className="px-8 py-4 font-medium text-slate-500">{formatDate(r.createdAt)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </Card>
    );
  }

  if (tab === 'reports') {
    return (
      <Card className="rounded-3xl border-slate-100 p-8 shadow-sm">
        <h3 className="text-xl font-bold text-slate-900 mb-6">Reported Items & Users</h3>
        {!reports?.length ? (
          <EmptyState title="No reports" description="User reports will appear here." />
        ) : (
          <div className="space-y-4">
            {reports.map((r) => (
              <div key={r.id} className={`p-6 rounded-2xl border transition-all ${r.isResolved ? 'bg-slate-50 border-slate-200' : 'bg-rose-50 border-rose-200 hover:shadow-md'}`}>
                <div className="flex flex-col sm:flex-row sm:items-start justify-between gap-4">
                  <div className="space-y-2">
                    <div className="font-bold text-lg text-slate-900 flex items-center gap-2">
                      {!r.isResolved && <Flag className="h-5 w-5 text-rose-500" />} {r.reason}
                    </div>
                    <p className="text-slate-700 font-medium">{r.description}</p>
                    <div className="flex flex-wrap gap-2 text-xs font-bold text-slate-500 bg-white inline-block px-3 py-2 rounded-xl border border-slate-100 mt-2">
                      <span>Reporter: {r.reporterName}</span>
                      {r.reportedListingTitle && <><span className="text-slate-300">•</span><span className="text-primary-600">Listing: {r.reportedListingTitle}</span></>}
                      {r.reportedUserName && <><span className="text-slate-300">•</span><span className="text-indigo-600">User: {r.reportedUserName}</span></>}
                    </div>
                  </div>
                  <div className="flex items-center">
                    {!r.isResolved ? (
                      <Button size="sm" className="bg-primary-600 hover:bg-primary-700 font-bold px-6 rounded-xl" onClick={() => resolveMutation.mutate(r.id)}>Mark Resolved</Button>
                    ) : (
                      <span className="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-full text-xs font-bold bg-emerald-100 text-emerald-700">
                        <CheckCircle className="h-3.5 w-3.5" /> Resolved
                      </span>
                    )}
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </Card>
    );
  }

  return null;
}
