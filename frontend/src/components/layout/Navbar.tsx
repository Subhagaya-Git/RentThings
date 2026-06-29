import { Link } from 'react-router-dom';
import { Bell, Menu, X } from 'lucide-react';
import { useState } from 'react';
import { useAuthStore, useUiStore } from '@/stores';
import { getDashboardPath } from '@/lib/utils';


export function Navbar() {
  const { user, logout } = useAuthStore();
  const { sidebarOpen, toggleSidebar } = useUiStore();

  const [notifications, setNotifications] = useState<string[]>([]);
  const [hasUnread, setHasUnread] = useState(false);
  const [isDropdownOpen, setIsDropdownOpen] = useState(false);
  const [isAvatarDropdownOpen, setIsAvatarDropdownOpen] = useState(false);

  return (
    <header className="sticky top-0 z-50 bg-white border-b border-slate-100">
      <nav className="mx-auto flex max-w-7xl items-center justify-between px-4 py-3 lg:px-8" aria-label="Main navigation">
        
        {/* Left: Logo */}
        <div className="flex items-center gap-4">
          <button className="lg:hidden p-2 text-slate-600 hover:bg-slate-50 rounded-lg" onClick={toggleSidebar} aria-label="Toggle menu">
            {sidebarOpen ? <X className="h-5 w-5" /> : <Menu className="h-5 w-5" />}
          </button>
          
          <Link to="/" className="flex items-center gap-2 shrink-0">
            <div className="flex h-9 w-9 items-center justify-center rounded-xl bg-primary-600 text-white font-bold text-sm">RT</div>
            <span className="text-xl font-bold text-slate-900 hidden sm:block tracking-tight">RentThings</span>
          </Link>
        </div>

        {/* Center: Desktop Links */}
        <div className="hidden md:flex items-center gap-8">
          <Link to="/search" className="text-sm font-medium text-slate-600 hover:text-primary-600 transition-colors">Browse</Link>
          <a href="/#how-it-works" className="text-sm font-medium text-slate-600 hover:text-primary-600 transition-colors">How it Works</a>
          <Link to="/owner/listings/new" className="text-sm font-medium text-slate-600 hover:text-primary-600 transition-colors">List Your Item</Link>
        </div>

        {/* Right: Actions */}
        <div className="flex items-center gap-2">
          {user ? (
            <div className="flex items-center gap-1 md:gap-3">
              {/* Notifications */}
              <div className="relative">
                <button 
                  onClick={() => { setIsDropdownOpen(!isDropdownOpen); setIsAvatarDropdownOpen(false); setHasUnread(false); }} 
                  className="p-2 rounded-full hover:bg-slate-50 transition-colors relative block" 
                  aria-label="Notifications"
                >
                  <Bell className="h-5 w-5 text-slate-600" />
                  {hasUnread && (
                    <span className="absolute top-1.5 right-1.5 h-2.5 w-2.5 bg-rose-500 rounded-full animate-pulse border-2 border-white" />
                  )}
                </button>

                {isDropdownOpen && (
                  <div className="absolute right-0 top-12 bg-white border border-slate-100 w-80 shadow-lg rounded-2xl p-4 z-50 animate-in fade-in slide-in-from-top-2 duration-200">
                    <h4 className="font-bold text-sm border-b border-slate-50 pb-3 mb-3 text-slate-900 flex justify-between items-center">
                      <span>Notifications</span>
                      {notifications.length > 0 && (
                        <button onClick={() => setNotifications([])} className="text-xs text-primary-600 font-medium hover:underline">Clear all</button>
                      )}
                    </h4>
                    {notifications.length === 0 ? (
                      <p className="text-sm text-slate-500 py-6 text-center">No new notifications.</p>
                    ) : (
                      <ul className="space-y-2 max-h-64 overflow-y-auto pr-1">
                        {notifications.map((note, index) => (
                          <li key={index} className="text-sm bg-slate-50 p-3 rounded-xl text-slate-700 leading-relaxed">
                            {note}
                          </li>
                        ))}
                      </ul>
                    )}
                  </div>
                )}
              </div>

              {/* Avatar Dropdown */}
              <div className="relative">
                <button 
                  onClick={() => { setIsAvatarDropdownOpen(!isAvatarDropdownOpen); setIsDropdownOpen(false); }}
                  className="flex items-center gap-2 rounded-full border border-slate-200 p-1 pr-2 hover:shadow-sm transition-all bg-white"
                >
                  <div className="bg-primary-100 text-primary-700 h-8 w-8 rounded-full flex items-center justify-center font-bold text-sm">
                    {user.firstName.charAt(0)}
                  </div>
                  <Menu className="h-4 w-4 text-slate-500" />
                </button>

                {isAvatarDropdownOpen && (
                  <div className="absolute right-0 top-12 w-48 bg-white rounded-2xl shadow-lg border border-slate-100 py-2 z-50 animate-in fade-in slide-in-from-top-2 duration-200">
                    <div className="px-4 py-2 border-b border-slate-50 mb-1">
                      <p className="font-bold text-sm text-slate-900">{user.firstName} {user.lastName}</p>
                      <p className="text-xs text-slate-500 truncate">{user.email}</p>
                    </div>
                    <Link to={getDashboardPath(user.role)} className="block px-4 py-2 text-sm text-slate-700 hover:bg-slate-50 hover:text-primary-600 transition-colors">My Dashboard</Link>
                    <Link to="/profile" className="block px-4 py-2 text-sm text-slate-700 hover:bg-slate-50 hover:text-primary-600 transition-colors">Profile</Link>
                    <div className="h-px bg-slate-50 my-1"></div>
                    <button onClick={logout} className="w-full text-left px-4 py-2 text-sm text-rose-600 hover:bg-rose-50 transition-colors">Log out</button>
                  </div>
                )}
              </div>
            </div>
          ) : (
            <div className="flex items-center gap-3">
              <Link to="/login" className="hidden sm:block text-sm font-medium text-slate-700 hover:text-slate-900 transition-colors px-4 py-2">
                Log in
              </Link>
              <Link to="/register">
                <button className="bg-primary-600 hover:bg-primary-700 text-white rounded-xl px-5 py-2.5 text-sm font-medium transition-colors shadow-sm">
                  Sign up
                </button>
              </Link>
            </div>
          )}
        </div>
      </nav>
    </header>
  );
}

export function Footer() {
  return (
    <footer className="border-t border-slate-100 bg-white mt-auto" role="contentinfo">
      <div className="mx-auto max-w-7xl px-4 py-16 lg:px-8">
        <div className="grid gap-8 md:grid-cols-4">
          <div className="md:col-span-1">
            <div className="flex items-center gap-2 mb-4">
              <div className="flex h-8 w-8 items-center justify-center rounded-xl bg-primary-600 text-white font-bold text-xs">RT</div>
              <span className="font-bold tracking-tight text-slate-900 text-lg">RentThings</span>
            </div>
            <p className="text-sm text-slate-500 leading-relaxed pr-4">The most trusted marketplace for peer-to-peer rentals in Sri Lanka. Rent anything, anytime.</p>
          </div>
          {[
            { title: 'Explore', links: [['Browse', '/search'], ['Categories', '/search'], ['How it works', '/#how-it-works']] },
            { title: 'Owners', links: [['List an item', '/owner/listings/new'], ['Owner dashboard', '/owner/dashboard'], ['Trust & safety', '/trust']] },
            { title: 'Support', links: [['Help center', '/help'], ['Terms of service', '/terms'], ['Contact', '/contact']] },
          ].map((col) => (
            <div key={col.title}>
              <h4 className="font-bold text-slate-900 mb-4">{col.title}</h4>
              <ul className="space-y-3">
                {col.links.map(([label, href]) => (
                  <li key={label}><Link to={href} className="text-sm text-slate-500 hover:text-primary-600 transition-colors">{label}</Link></li>
                ))}
              </ul>
            </div>
          ))}
        </div>
        <div className="mt-12 border-t border-slate-50 pt-8 flex flex-col md:flex-row items-center justify-between gap-4">
          <p className="text-sm text-slate-400">© {new Date().getFullYear()} RentThings. All rights reserved.</p>
          <div className="flex gap-4">
            <span className="text-sm font-medium px-2 py-1 bg-slate-50 text-slate-500 rounded-lg">Built on Azure</span>
          </div>
        </div>
      </div>
    </footer>
  );
}