import { lazy, Suspense } from 'react';
import { BrowserRouter, Navigate, Routes, Route } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { Navbar, Footer } from '@/components/layout/Navbar';
import { AiAssistant } from '@/components/ai/AiAssistant';
import { useSignalRNotifications } from '@/hooks/useSignalRNotifications';
import { ProtectedRoute, DashboardLayout } from '@/components/layout/ProtectedRoute';
import HomePage from '@/pages/HomePage';

const SearchPage = lazy(() => import('@/pages/SearchPage'));
const ListingDetailPage = lazy(() => import('@/pages/ListingDetailPage'));
const LoginPage = lazy(() => import('@/pages/AuthPages').then((m) => ({ default: m.default })));
const RegisterPage = lazy(() => import('@/pages/AuthPages').then((m) => ({ default: m.RegisterPage })));
const RenterDashboardPage = lazy(() => import('@/pages/DashboardPages').then((m) => ({ default: m.default })));
const RentalsPage = lazy(() => import('@/pages/DashboardPages').then((m) => ({ default: m.RentalsPage })));
const NotificationsPage = lazy(() => import('@/pages/DashboardPages').then((m) => ({ default: m.NotificationsPage })));
const FavoritesPage = lazy(() => import('@/pages/DashboardPages').then((m) => ({ default: m.FavoritesPage })));
const ProfilePage = lazy(() => import('@/pages/Profile'));
const OwnerDashboardPage = lazy(() => import('@/pages/OwnerDashboardPage'));
const CreateListingPage = lazy(() => import('@/pages/CreateListingPage'));
const EditListingPage = lazy(() => import('@/pages/CreateListingPage').then((m) => ({ default: m.EditListingPage })));
const AdminDashboardPage = lazy(() => import('@/pages/AdminDashboardPage'));

const queryClient = new QueryClient({
  defaultOptions: { queries: { staleTime: 30_000, retry: 1 } },
});

function Layout({ children }: { children: React.ReactNode }) {
  useSignalRNotifications();
  return (
    <div className="flex min-h-screen flex-col">
      <Navbar />
      <main className="flex-1">
        <Suspense fallback={<div className="flex items-center justify-center py-32 text-slate-400">Loading...</div>}>
          {children}
        </Suspense>
      </main>
      <Footer />
      <AiAssistant />
    </div>
  );
}

const renterNav = [
  { label: 'Browse', href: '/search' },
  { label: 'My Rentals', href: '/renter/dashboard' },
  { label: 'Messages', href: '/renter/notifications' },
  { label: 'Profile', href: '/profile' },
];

const ownerNav = [
  { label: 'My Listings', href: '/owner/dashboard' },
  { label: 'Requests', href: '/owner/requests' },
  { label: 'Rentals', href: '/owner/rentals' },
  { label: 'Earnings', href: '/owner/earnings' },
  { label: 'Profile', href: '/profile' },
];

const adminNav = [
  { label: 'Dashboard', href: '/admin/dashboard' },
  { label: 'Users', href: '/admin/users' },
  { label: 'Listings', href: '/admin/listings' },
  { label: 'Transactions', href: '/admin/transactions' },
  { label: 'Reports', href: '/admin/reports' },
];

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Layout>
          <Routes>
            <Route path="/" element={<HomePage />} />
            <Route path="/search" element={<SearchPage />} />
            <Route path="/listings/:id" element={<ListingDetailPage />} />
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />

            {/* Legacy redirects */}
            <Route path="/dashboard/*" element={<Navigate to="/renter/dashboard" replace />} />
            <Route path="/owner" element={<Navigate to="/owner/dashboard" replace />} />
            <Route path="/admin" element={<Navigate to="/admin/dashboard" replace />} />

            <Route element={<ProtectedRoute roles={['Renter', 'Owner', 'Admin']} />}>
              <Route path="/profile" element={<ProfilePage />} />
            </Route>

            <Route element={<ProtectedRoute roles={['Renter', 'Owner', 'Admin']} />}>
              <Route element={<DashboardLayout title="Renter" nav={renterNav} />}>
                <Route path="/renter/dashboard" element={<RenterDashboardPage />} />
                <Route path="/renter/rentals" element={<RentalsPage />} />
                <Route path="/renter/notifications" element={<NotificationsPage />} />
                <Route path="/renter/favorites" element={<FavoritesPage />} />
                <Route path="/renter/profile" element={<ProfilePage />} />
              </Route>
            </Route>

            <Route element={<ProtectedRoute roles={['Owner', 'Admin']} />}>
              <Route element={<DashboardLayout title="Owner" nav={ownerNav} />}>
                <Route path="/owner/dashboard" element={<OwnerDashboardPage tab="listings" />} />
                <Route path="/owner/requests" element={<OwnerDashboardPage tab="requests" />} />
                <Route path="/owner/rentals" element={<OwnerDashboardPage tab="rentals" />} />
                <Route path="/owner/earnings" element={<OwnerDashboardPage tab="earnings" />} />
                <Route path="/owner/listings/new" element={<CreateListingPage />} />
                <Route path="/owner/listings/:id/edit" element={<EditListingPage />} />
              </Route>
            </Route>

            <Route element={<ProtectedRoute roles={['Admin']} />}>
              <Route element={<DashboardLayout title="Admin" nav={adminNav} />}>
                <Route path="/admin/dashboard" element={<AdminDashboardPage tab="overview" />} />
                <Route path="/admin/users" element={<AdminDashboardPage tab="users" />} />
                <Route path="/admin/listings" element={<AdminDashboardPage tab="listings" />} />
                <Route path="/admin/transactions" element={<AdminDashboardPage tab="transactions" />} />
                <Route path="/admin/reports" element={<AdminDashboardPage tab="reports" />} />
              </Route>
            </Route>
          </Routes>
        </Layout>
      </BrowserRouter>
    </QueryClientProvider>
  );
}
