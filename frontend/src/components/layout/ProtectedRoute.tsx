import { Link, Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuthStore } from '@/stores';
import { getDashboardPath } from '@/lib/utils';
import type { UserRole } from '@/types';

export function ProtectedRoute({ roles }: { roles?: UserRole[] }) {
  const { user, token } = useAuthStore();
  if (!token || !user) return <Navigate to="/login" replace />;
  if (roles && !roles.includes(user.role)) return <Navigate to={getDashboardPath(user.role)} replace />;
  return <Outlet />;
}

export function DashboardLayout({ title, nav }: { title: string; nav: { label: string; href: string }[] }) {
  const location = useLocation();

  return (
    <div className="mx-auto max-w-7xl px-4 py-8 lg:px-8">
      <h1 className="text-2xl font-bold text-slate-900 mb-6">{title}</h1>
      <div className="flex flex-col lg:flex-row gap-8">
        <aside className="lg:w-56 shrink-0">
          <nav className="glass rounded-2xl p-4 space-y-1" aria-label={`${title} navigation`}>
            {nav.map((item) => (
              <Link
                key={item.href}
                to={item.href}
                className={`block rounded-xl px-4 py-2.5 text-sm font-medium transition-colors ${
                  location.pathname === item.href || location.pathname.startsWith(item.href + '/')
                    ? 'bg-brand-50 text-brand-700'
                    : 'text-slate-600 hover:bg-brand-50 hover:text-brand-700'
                }`}
              >
                {item.label}
              </Link>
            ))}
          </nav>
        </aside>
        <main className="flex-1 min-w-0">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
