import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useMsal } from '@azure/msal-react';
import { Sparkles, ShieldCheck } from 'lucide-react';
import { Button, Input } from '@/components/ui';
import { getDashboardPath } from '@/lib/utils';
import { useAuthStore } from '@/stores';
import { isMicrosoftLoginConfigured, loginRequest } from '@/authConfig';

function MicrosoftLogo() {
  return (
    <svg className="h-5 w-5" viewBox="0 0 21 21" aria-hidden="true">
      <rect x="1" y="1" width="9" height="9" fill="#f25022" />
      <rect x="11" y="1" width="9" height="9" fill="#7fba00" />
      <rect x="1" y="11" width="9" height="9" fill="#00a4ef" />
      <rect x="11" y="11" width="9" height="9" fill="#ffb900" />
    </svg>
  );
}

function AuthLayout({ children, title, subtitle }: { children: React.ReactNode; title: string; subtitle: string }) {
  return (
    <div className="flex min-h-screen bg-white">
      {/* Left Panel: Gradient & Testimonial (Hidden on mobile) */}
      <div className="hidden lg:flex lg:w-1/2 relative bg-gradient-to-br from-primary-600 to-primary-400 p-12 text-white flex-col justify-between overflow-hidden">
        {/* Decorative elements */}
        <div className="absolute top-0 left-0 w-full h-full bg-[url('https://images.unsplash.com/photo-1522202176988-66273c2fd55f?q=80&w=2071&auto=format&fit=crop')] bg-cover bg-center mix-blend-overlay opacity-20"></div>
        <div className="absolute -bottom-32 -left-32 w-96 h-96 rounded-full bg-white opacity-10 blur-3xl"></div>
        <div className="absolute -top-32 -right-32 w-96 h-96 rounded-full bg-primary-300 opacity-20 blur-3xl"></div>

        <div className="relative z-10">
          <Link to="/" className="flex items-center gap-2 text-2xl font-bold tracking-tight">
            <ShieldCheck className="h-8 w-8 text-white" />
            RentThings
          </Link>
        </div>

        <div className="relative z-10 max-w-md">
          <div className="mb-6 flex gap-1 text-amber-300">
            {[1, 2, 3, 4, 5].map((i) => (
              <Sparkles key={i} className="h-5 w-5 fill-current" />
            ))}
          </div>
          <blockquote className="text-3xl font-medium leading-tight text-white mb-6">
            "RentThings changed how I do business. I rented a RED camera for my shoot seamlessly. The trust system is incredible."
          </blockquote>
          <div className="flex items-center gap-4">
            <img 
              src="https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150&q=80" 
              alt="Avatar" 
              className="h-12 w-12 rounded-full border-2 border-white/20 object-cover"
            />
            <div>
              <div className="font-bold">Ayesha Fernando</div>
              <div className="text-primary-100 text-sm">Filmmaker & Platinum Renter</div>
            </div>
          </div>
        </div>
      </div>

      {/* Right Panel: Form */}
      <div className="flex w-full lg:w-1/2 flex-col justify-center px-8 py-12 sm:px-16 lg:px-24 xl:px-32 relative">
        <Link to="/" className="absolute top-8 left-8 lg:hidden flex items-center gap-2 text-xl font-bold text-primary-600">
          <ShieldCheck className="h-6 w-6" />
          RentThings
        </Link>
        <div className="w-full max-w-sm mx-auto">
          <h1 className="text-3xl font-bold tracking-tight text-slate-900">{title}</h1>
          <p className="mt-2 text-slate-500">{subtitle}</p>
          <div className="mt-8">{children}</div>
        </div>
      </div>
    </div>
  );
}

export default function LoginPage() {
  const [email, setEmail] = useState('renter@rentthings.com');
  const [password, setPassword] = useState('password');
  const [error, setError] = useState('');
  const [msalLoading, setMsalLoading] = useState(false);
  const { login, loginWithMicrosoft, isLoading } = useAuthStore();
  const { instance } = useMsal();
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    try {
      await login(email, password);
      const role = useAuthStore.getState().user?.role ?? 'Renter';
      navigate(getDashboardPath(role));
    } catch {
      setError('Invalid email or password. Try renter@rentthings.com or owner@rentthings.com');
    }
  };

  const handleMicrosoftLogin = async () => {
    if (!isMicrosoftLoginConfigured) {
      setError('Microsoft login is not configured. Use the email/password form for local dev.');
      return;
    }

    setError('');
    setMsalLoading(true);
    try {
      const loginResponse = await instance.loginPopup(loginRequest);
      if (!loginResponse.account) {
        throw new Error('No account returned from Microsoft login');
      }
      loginWithMicrosoft(loginResponse.account, loginResponse.accessToken);
      navigate(getDashboardPath('Renter'));
    } catch {
      setError('Microsoft login failed. Please try again.');
    } finally {
      setMsalLoading(false);
    }
  };

  return (
    <AuthLayout title="Welcome back" subtitle="Sign in to your RentThings account">
      <form onSubmit={handleSubmit} className="space-y-5">
        <div>
          <label htmlFor="email" className="text-sm font-bold text-slate-700">Email</label>
          <Input id="email" type="email" value={email} onChange={(e) => setEmail(e.target.value)} required className="mt-2 h-12" autoComplete="email" />
        </div>
        <div>
          <label htmlFor="password" className="text-sm font-bold text-slate-700">Password</label>
          <Input id="password" type="password" value={password} onChange={(e) => setPassword(e.target.value)} required className="mt-2 h-12" autoComplete="current-password" />
        </div>
        {error && <p className="text-sm text-rose-600 font-medium" role="alert">{error}</p>}
        <Button type="submit" className="w-full h-12 text-base font-bold bg-primary-600 hover:bg-primary-700" loading={isLoading}>Sign in</Button>
      </form>

      <div className="mt-8">
        <div className="relative">
          <div className="absolute inset-0 flex items-center">
            <div className="w-full border-t border-slate-200" />
          </div>
          <div className="relative flex justify-center text-sm">
            <span className="bg-white px-4 text-slate-400 font-medium">or continue with</span>
          </div>
        </div>
        <Button
          type="button"
          variant="secondary"
          className="mt-6 w-full h-12 text-slate-700 border-slate-200 hover:bg-slate-50 font-bold"
          loading={msalLoading}
          onClick={handleMicrosoftLogin}
        >
          <MicrosoftLogo />
          Microsoft
        </Button>
      </div>

      <p className="mt-8 text-center text-sm text-slate-600">
        Don't have an account? <Link to="/register" className="font-bold text-primary-600 hover:text-primary-700">Sign up</Link>
      </p>
      <p className="mt-4 text-center text-xs text-slate-400">Demo: renter@rentthings.com · owner@rentthings.com</p>
    </AuthLayout>
  );
}

export function RegisterPage() {
  const [form, setForm] = useState({ email: '', password: '', firstName: '', lastName: '', role: 'Renter' });
  const [error, setError] = useState('');
  const { register, isLoading } = useAuthStore();
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    try {
      await register(form);
      navigate(getDashboardPath(form.role));
    } catch {
      setError('Registration failed. Email may already be in use.');
    }
  };

  return (
    <AuthLayout title="Create an account" subtitle="Join the RentThings community today">
      <form onSubmit={handleSubmit} className="space-y-5">
        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="text-sm font-bold text-slate-700">First name</label>
            <Input value={form.firstName} onChange={(e) => setForm({ ...form, firstName: e.target.value })} required className="mt-2 h-12" />
          </div>
          <div>
            <label className="text-sm font-bold text-slate-700">Last name</label>
            <Input value={form.lastName} onChange={(e) => setForm({ ...form, lastName: e.target.value })} required className="mt-2 h-12" />
          </div>
        </div>
        <div>
          <label className="text-sm font-bold text-slate-700">Email</label>
          <Input type="email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} required className="mt-2 h-12" />
        </div>
        <div>
          <label className="text-sm font-bold text-slate-700">Password</label>
          <Input type="password" value={form.password} onChange={(e) => setForm({ ...form, password: e.target.value })} required className="mt-2 h-12" />
        </div>
        <div>
          <label className="text-sm font-bold text-slate-700">I want to</label>
          <select value={form.role} onChange={(e) => setForm({ ...form, role: e.target.value })} className="mt-2 h-12 w-full rounded-xl border border-slate-200 bg-white px-4 text-sm text-slate-900 focus:border-primary-400 focus:ring-2 focus:ring-primary-100 outline-none transition-all">
            <option value="Renter">Rent items</option>
            <option value="Owner">List my items</option>
          </select>
        </div>
        {error && <p className="text-sm text-rose-600 font-medium" role="alert">{error}</p>}
        <Button type="submit" className="w-full h-12 text-base font-bold bg-primary-600 hover:bg-primary-700" loading={isLoading}>Create account</Button>
      </form>

      <p className="mt-8 text-center text-sm text-slate-600">
        Already have an account? <Link to="/login" className="font-bold text-primary-600 hover:text-primary-700">Sign in</Link>
      </p>
    </AuthLayout>
  );
}
