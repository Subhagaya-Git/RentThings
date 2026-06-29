import { useMemo, useState } from 'react';
import { Navigate, Link } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  BarChart3,
  Edit3,
  FileText,
  LayoutDashboard,
  Lock,
  Package,
  ShieldCheck,
  UserRound,
  Users,
} from 'lucide-react';
import { Button, Card, EmptyState, Input, Skeleton, TrustBadge } from '@/components/ui';
import { StatusBadge } from '@/components/ui/StatusBadge';
import { adminApi, listingsApi, rentalsApi, usersApi } from '@/lib/api';
import api from '@/lib/api';
import { formatCurrency, formatDate } from '@/lib/utils';
import { useAuthStore } from '@/stores';
import type { UserRole } from '@/types';

const roleStyles: Record<UserRole, string> = {
  Renter: 'bg-primary-50 text-primary-700 border-primary-100',
  Owner: 'bg-emerald-50 text-emerald-700 border-emerald-100',
  Admin: 'bg-indigo-50 text-indigo-700 border-indigo-100',
};

function initials(firstName?: string, lastName?: string, email?: string) {
  const letters = `${firstName?.[0] ?? ''}${lastName?.[0] ?? ''}`.trim();
  return (letters || email?.slice(0, 2) || 'RT').toUpperCase();
}

function roleDescription(role: UserRole) {
  if (role === 'Admin') return 'Platform administrator';
  if (role === 'Owner') return 'Item owner';
  return 'Renter account';
}

export default function ProfilePage() {
  const { user, token, setUser } = useAuthStore();
  const qc = useQueryClient();
  const [editing, setEditing] = useState(false);
  const [profileForm, setProfileForm] = useState({
    firstName: user?.firstName ?? '',
    lastName: user?.lastName ?? '',
    phone: user?.phone ?? '',
    location: user?.location ?? '',
  });
  const [passwordForm, setPasswordForm] = useState({ currentPassword: '', newPassword: '', confirmPassword: '' });
  const [passwordMessage, setPasswordMessage] = useState('');

  const rentalsQuery = useQuery({
    queryKey: ['my-rentals'],
    queryFn: rentalsApi.myRentals,
    enabled: !!token && user?.role === 'Renter',
  });

  const ownerDashboardQuery = useQuery({
    queryKey: ['owner-dashboard'],
    queryFn: listingsApi.ownerDashboard,
    enabled: !!token && user?.role === 'Owner',
  });

  const adminStatsQuery = useQuery({
    queryKey: ['admin-stats'],
    queryFn: adminApi.stats,
    enabled: !!token && user?.role === 'Admin',
  });

  const profileMutation = useMutation({
    mutationFn: () => usersApi.update(profileForm),
    onSuccess: (updated) => {
      setUser(updated);
      setEditing(false);
      qc.invalidateQueries({ queryKey: ['me'] });
    },
  });

  const passwordMutation = useMutation({
    mutationFn: () =>
      api.post('/auth/change-password', {
        currentPassword: passwordForm.currentPassword,
        newPassword: passwordForm.newPassword,
      }),
    onSuccess: () => {
      setPasswordMessage('Password changed successfully.');
      setPasswordForm({ currentPassword: '', newPassword: '', confirmPassword: '' });
    },
    onError: () => setPasswordMessage('Could not change password. Please check your current password and try again.'),
  });

  const rentalGroups = useMemo(() => {
    const rentals = rentalsQuery.data ?? [];
    return {
      active: rentals.filter((r) => ['Requested', 'Approved', 'HandedOver', 'Active'].includes(r.status)),
      past: rentals.filter((r) => ['Completed', 'Reviewed', 'Rejected', 'Cancelled', 'Returned'].includes(r.status)),
    };
  }, [rentalsQuery.data]);

  if (!token || !user) return <Navigate to="/login" replace />;

  const fullName = `${user.firstName} ${user.lastName}`.trim() || user.email;
  const roleClass = roleStyles[user.role] ?? roleStyles.Renter;
  const ownerDashboard = ownerDashboardQuery.data;

  const submitPassword = (event: React.FormEvent) => {
    event.preventDefault();
    setPasswordMessage('');
    if (passwordForm.newPassword.length < 8) {
      setPasswordMessage('New password must be at least 8 characters.');
      return;
    }
    if (passwordForm.newPassword !== passwordForm.confirmPassword) {
      setPasswordMessage('New password and confirmation do not match.');
      return;
    }
    passwordMutation.mutate();
  };

  return (
    <div className="mx-auto max-w-7xl px-4 py-8 lg:px-8 bg-surface min-h-screen">
      <div className="grid gap-8 lg:grid-cols-[minmax(0,1fr)_380px]">
        <div className="space-y-8">
          <Card className="rounded-3xl border-slate-100 bg-white p-6 shadow-sm sm:p-8">
            <div className="flex flex-col gap-6 sm:flex-row sm:items-center sm:justify-between">
              <div className="flex items-center gap-5">
                <div className="flex h-20 w-20 shrink-0 items-center justify-center rounded-full bg-primary-600 text-2xl font-bold text-white shadow-lg shadow-primary-600/20">
                  {initials(user.firstName, user.lastName, user.email)}
                </div>
                <div>
                  <div className="flex flex-wrap items-center gap-3">
                    <h1 className="text-3xl font-bold tracking-tight text-slate-900">{fullName}</h1>
                    <span className={`inline-flex items-center rounded-full border px-3 py-1 text-xs font-bold ${roleClass}`}>
                      {user.role}
                    </span>
                  </div>
                  <p className="mt-1 text-sm font-medium text-slate-500">{user.email}</p>
                  <p className="mt-2 text-sm text-slate-500">{roleDescription(user.role)}</p>
                </div>
              </div>
              <Button
                type="button"
                onClick={() => {
                  setProfileForm({
                    firstName: user.firstName,
                    lastName: user.lastName,
                    phone: user.phone ?? '',
                    location: user.location ?? '',
                  });
                  setEditing((value) => !value);
                }}
                className="bg-primary-600 hover:bg-primary-700"
              >
                <Edit3 className="h-4 w-4" />
                {editing ? 'Cancel edit' : 'Edit Profile'}
              </Button>
            </div>
          </Card>

          {editing && (
            <Card className="rounded-3xl border-slate-100 bg-white p-6 shadow-sm sm:p-8">
              <h2 className="text-xl font-bold text-slate-900">Edit profile</h2>
              <form
                className="mt-6 grid gap-5 sm:grid-cols-2"
                onSubmit={(event) => {
                  event.preventDefault();
                  profileMutation.mutate();
                }}
              >
                <div>
                  <label className="text-sm font-bold text-slate-700">First name</label>
                  <Input value={profileForm.firstName} onChange={(e) => setProfileForm({ ...profileForm, firstName: e.target.value })} className="mt-2" required />
                </div>
                <div>
                  <label className="text-sm font-bold text-slate-700">Last name</label>
                  <Input value={profileForm.lastName} onChange={(e) => setProfileForm({ ...profileForm, lastName: e.target.value })} className="mt-2" required />
                </div>
                <div>
                  <label className="text-sm font-bold text-slate-700">Phone</label>
                  <Input value={profileForm.phone} onChange={(e) => setProfileForm({ ...profileForm, phone: e.target.value })} className="mt-2" placeholder="+94..." />
                </div>
                <div>
                  <label className="text-sm font-bold text-slate-700">Address</label>
                  <Input value={profileForm.location} onChange={(e) => setProfileForm({ ...profileForm, location: e.target.value })} className="mt-2" placeholder="City or address" />
                </div>
                {profileMutation.isError && <p className="sm:col-span-2 text-sm font-medium text-rose-600">Could not save profile changes. Please try again.</p>}
                <div className="sm:col-span-2 flex flex-wrap gap-3">
                  <Button type="submit" loading={profileMutation.isPending} className="bg-primary-600 hover:bg-primary-700">Save changes</Button>
                  <Button type="button" variant="ghost" onClick={() => setEditing(false)}>Cancel</Button>
                </div>
              </form>
            </Card>
          )}

          {user.role === 'Renter' && (
            <Card className="rounded-3xl border-slate-100 bg-white p-6 shadow-sm sm:p-8">
              <div className="mb-6 flex items-center justify-between gap-4">
                <div>
                  <h2 className="text-xl font-bold text-slate-900">My Rentals</h2>
                  <p className="mt-1 text-sm text-slate-500">Active and past rental activity.</p>
                </div>
                <Link to="/renter/rentals" className="text-sm font-bold text-primary-600 hover:text-primary-700">View all</Link>
              </div>
              {rentalsQuery.isLoading ? (
                <div className="space-y-3">
                  <Skeleton className="h-20 w-full rounded-2xl" />
                  <Skeleton className="h-20 w-full rounded-2xl" />
                </div>
              ) : rentalsQuery.isError ? (
                <p className="rounded-2xl bg-rose-50 p-4 text-sm font-medium text-rose-700">Could not load rentals.</p>
              ) : !rentalsQuery.data?.length ? (
                <EmptyState title="No rentals yet" description="Your bookings and completed rentals will appear here." action={<Link to="/search" className="font-bold text-primary-600 hover:underline">Browse rentals</Link>} />
              ) : (
                <div className="space-y-6">
                  {[
                    ['Active rentals', rentalGroups.active],
                    ['Past rentals', rentalGroups.past],
                  ].map(([title, rentals]) => (
                    <section key={title as string}>
                      <h3 className="mb-3 text-sm font-bold uppercase tracking-wide text-slate-500">{title as string}</h3>
                      {(rentals as typeof rentalGroups.active).length ? (
                        <div className="space-y-3">
                          {(rentals as typeof rentalGroups.active).slice(0, 4).map((r) => (
                            <div key={r.id} className="flex flex-col justify-between gap-4 rounded-2xl border border-slate-100 bg-slate-50 p-4 sm:flex-row sm:items-center">
                              <div>
                                <div className="font-bold text-slate-900">{r.listingTitle}</div>
                                <div className="mt-1 text-sm font-medium text-slate-500">{formatDate(r.startDate)} to {formatDate(r.endDate)}</div>
                              </div>
                              <StatusBadge status={r.status} />
                            </div>
                          ))}
                        </div>
                      ) : (
                        <p className="rounded-2xl border border-slate-100 bg-slate-50 p-4 text-sm text-slate-500">Nothing here yet.</p>
                      )}
                    </section>
                  ))}
                </div>
              )}
            </Card>
          )}

          {user.role === 'Owner' && (
            <Card className="rounded-3xl border-slate-100 bg-white p-6 shadow-sm sm:p-8">
              <div className="mb-6 flex items-center justify-between gap-4">
                <div>
                  <h2 className="text-xl font-bold text-slate-900">My Listings</h2>
                  <p className="mt-1 text-sm text-slate-500">Manage items you have listed for rent.</p>
                </div>
                <Link to="/owner/listings/new" className="text-sm font-bold text-primary-600 hover:text-primary-700">New listing</Link>
              </div>
              {ownerDashboardQuery.isLoading ? (
                <div className="space-y-3">
                  <Skeleton className="h-20 w-full rounded-2xl" />
                  <Skeleton className="h-20 w-full rounded-2xl" />
                </div>
              ) : ownerDashboardQuery.isError ? (
                <p className="rounded-2xl bg-rose-50 p-4 text-sm font-medium text-rose-700">Could not load owner data.</p>
              ) : !ownerDashboard?.listings.length ? (
                <EmptyState title="No listings yet" description="Create your first listing to start receiving rental requests." action={<Link to="/owner/listings/new" className="font-bold text-primary-600 hover:underline">Create listing</Link>} />
              ) : (
                <div className="space-y-3">
                  {ownerDashboard.listings.slice(0, 6).map((listing) => (
                    <div key={listing.id} className="flex flex-col justify-between gap-4 rounded-2xl border border-slate-100 bg-slate-50 p-4 sm:flex-row sm:items-center">
                      <div>
                        <Link to={`/listings/${listing.id}`} className="font-bold text-slate-900 hover:text-primary-600">{listing.title}</Link>
                        <div className="mt-1 text-sm font-medium text-slate-500">{formatCurrency(listing.pricePerDay)}/day</div>
                      </div>
                      <div className="flex items-center gap-3">
                        <StatusBadge status={listing.status} />
                        <Link to={`/owner/listings/${listing.id}/edit`} className="rounded-xl border border-primary-100 bg-white px-3 py-2 text-sm font-bold text-primary-600 hover:bg-primary-50">
                          Quick edit
                        </Link>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </Card>
          )}

          {user.role === 'Admin' && (
            <Card className="rounded-3xl border-indigo-100 bg-white p-6 shadow-sm sm:p-8">
              <div className="mb-6 flex items-center gap-3">
                <div className="rounded-2xl bg-indigo-50 p-3 text-indigo-600">
                  <ShieldCheck className="h-6 w-6" />
                </div>
                <div>
                  <h2 className="text-xl font-bold text-slate-900">Admin Access</h2>
                  <p className="text-sm text-slate-500">Quick links to platform management areas.</p>
                </div>
              </div>
              <div className="grid gap-3 sm:grid-cols-2">
                {[
                  { label: 'Dashboard', href: '/admin/dashboard', icon: LayoutDashboard },
                  { label: 'Users', href: '/admin/users', icon: Users },
                  { label: 'Listings', href: '/admin/listings', icon: Package },
                  { label: 'Reports', href: '/admin/reports', icon: FileText },
                ].map((item) => (
                  <Link key={item.href} to={item.href} className="flex items-center gap-3 rounded-2xl border border-slate-100 bg-slate-50 p-4 font-bold text-slate-700 hover:border-primary-100 hover:bg-primary-50 hover:text-primary-700">
                    <item.icon className="h-5 w-5" />
                    {item.label}
                  </Link>
                ))}
              </div>
            </Card>
          )}
        </div>

        <aside className="space-y-8">
          <Card className="rounded-3xl border-slate-100 bg-white p-6 shadow-sm">
            <h2 className="text-lg font-bold text-slate-900">Account details</h2>
            <dl className="mt-5 space-y-4 text-sm">
              <div className="flex items-start gap-3">
                <UserRound className="mt-0.5 h-4 w-4 text-primary-600" />
                <div>
                  <dt className="font-bold text-slate-700">Trust level</dt>
                  <dd className="mt-1"><TrustBadge level={user.trustLevel} score={user.trustScore} /></dd>
                </div>
              </div>
              <div>
                <dt className="font-bold text-slate-700">Phone</dt>
                <dd className="mt-1 text-slate-500">{user.phone || 'Not set'}</dd>
              </div>
              <div>
                <dt className="font-bold text-slate-700">Address</dt>
                <dd className="mt-1 text-slate-500">{user.location || 'Not set'}</dd>
              </div>
              <div>
                <dt className="font-bold text-slate-700">Account created</dt>
                <dd className="mt-1 text-slate-500">{user.createdAt ? formatDate(user.createdAt) : 'Not available'}</dd>
              </div>
            </dl>
          </Card>

          {user.role === 'Owner' && (
            <Card className="rounded-3xl border-slate-100 bg-white p-6 shadow-sm">
              <h2 className="text-lg font-bold text-slate-900">Earnings summary</h2>
              {ownerDashboardQuery.isLoading ? (
                <Skeleton className="mt-5 h-24 w-full rounded-2xl" />
              ) : (
                <div className="mt-5 space-y-4">
                  <div className="rounded-2xl bg-emerald-50 p-4">
                    <div className="text-sm font-bold text-emerald-700">Total earnings</div>
                    <div className="mt-1 text-3xl font-bold text-emerald-900">{formatCurrency(ownerDashboard?.totalEarnings ?? 0)}</div>
                  </div>
                  <div className="grid grid-cols-2 gap-3 text-sm">
                    <div className="rounded-2xl bg-slate-50 p-4">
                      <div className="font-bold text-slate-900">{ownerDashboard?.activeListings ?? 0}</div>
                      <div className="text-slate-500">Active listings</div>
                    </div>
                    <div className="rounded-2xl bg-slate-50 p-4">
                      <div className="font-bold text-slate-900">{ownerDashboard?.pendingRequests ?? 0}</div>
                      <div className="text-slate-500">Pending requests</div>
                    </div>
                  </div>
                </div>
              )}
            </Card>
          )}

          {user.role === 'Admin' && (
            <Card className="rounded-3xl border-slate-100 bg-white p-6 shadow-sm">
              <h2 className="flex items-center gap-2 text-lg font-bold text-slate-900">
                <BarChart3 className="h-5 w-5 text-indigo-600" />
                Platform snapshot
              </h2>
              {adminStatsQuery.isLoading ? (
                <Skeleton className="mt-5 h-28 w-full rounded-2xl" />
              ) : (
                <div className="mt-5 grid grid-cols-2 gap-3 text-sm">
                  <div className="rounded-2xl bg-slate-50 p-4">
                    <div className="font-bold text-slate-900">{adminStatsQuery.data?.totalUsers ?? 0}</div>
                    <div className="text-slate-500">Users</div>
                  </div>
                  <div className="rounded-2xl bg-slate-50 p-4">
                    <div className="font-bold text-slate-900">{adminStatsQuery.data?.totalListings ?? 0}</div>
                    <div className="text-slate-500">Listings</div>
                  </div>
                  <div className="rounded-2xl bg-slate-50 p-4">
                    <div className="font-bold text-slate-900">{adminStatsQuery.data?.activeRentals ?? 0}</div>
                    <div className="text-slate-500">Active rentals</div>
                  </div>
                  <div className="rounded-2xl bg-slate-50 p-4">
                    <div className="font-bold text-slate-900">{adminStatsQuery.data?.pendingReports ?? 0}</div>
                    <div className="text-slate-500">Reports</div>
                  </div>
                </div>
              )}
            </Card>
          )}

          <Card className="rounded-3xl border-slate-100 bg-white p-6 shadow-sm">
            <h2 className="flex items-center gap-2 text-lg font-bold text-slate-900">
              <Lock className="h-5 w-5 text-primary-600" />
              Change Password
            </h2>
            <form className="mt-5 space-y-4" onSubmit={submitPassword}>
              <div>
                <label className="text-sm font-bold text-slate-700">Current password</label>
                <Input type="password" value={passwordForm.currentPassword} onChange={(e) => setPasswordForm({ ...passwordForm, currentPassword: e.target.value })} className="mt-2" required />
              </div>
              <div>
                <label className="text-sm font-bold text-slate-700">New password</label>
                <Input type="password" value={passwordForm.newPassword} onChange={(e) => setPasswordForm({ ...passwordForm, newPassword: e.target.value })} className="mt-2" required />
              </div>
              <div>
                <label className="text-sm font-bold text-slate-700">Confirm password</label>
                <Input type="password" value={passwordForm.confirmPassword} onChange={(e) => setPasswordForm({ ...passwordForm, confirmPassword: e.target.value })} className="mt-2" required />
              </div>
              {passwordMessage && (
                <p className={`text-sm font-medium ${passwordMessage.includes('success') ? 'text-emerald-600' : 'text-rose-600'}`}>{passwordMessage}</p>
              )}
              <Button type="submit" loading={passwordMutation.isPending} className="w-full bg-primary-600 hover:bg-primary-700">
                Update password
              </Button>
            </form>
          </Card>
        </aside>
      </div>
    </div>
  );
}
