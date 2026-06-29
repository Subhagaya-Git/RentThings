import { clsx, type ClassValue } from 'clsx';
import { twMerge } from 'tailwind-merge';

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

export function formatCurrency(amount: number) {
  return new Intl.NumberFormat('en-LK', { style: 'currency', currency: 'LKR', maximumFractionDigits: 0 }).format(amount);
}

export function formatDate(date: string) {
  return new Intl.DateTimeFormat('en-US', { month: 'short', day: 'numeric', year: 'numeric' }).format(new Date(date));
}

export function getDashboardPath(role: string) {
  switch (role) {
    case 'Owner': return '/owner/dashboard';
    case 'Admin': return '/admin/dashboard';
    default: return '/renter/dashboard';
  }
}

export const trustColors: Record<string, string> = {
  Bronze: 'bg-amber-100 text-amber-800 border-amber-200',
  Silver: 'bg-slate-100 text-slate-700 border-slate-200',
  Gold: 'bg-yellow-100 text-yellow-800 border-yellow-200',
  Platinum: 'bg-brand-100 text-brand-800 border-brand-200',
};

export const categories = [
  { name: 'Cameras', icon: '📷' },
  { name: 'Power Tools', icon: '🔧' },
  { name: 'Camping Gear', icon: '⛺' },
  { name: 'Sports Equipment', icon: '⚽' },
  { name: 'Event Equipment', icon: '🎪' },
  { name: 'Speakers', icon: '🔊' },
  { name: 'Home Appliances', icon: '🏠' },
  { name: 'Electronics', icon: '💻' },
];

export const testimonials = [
  { name: 'Sarah M.', role: 'Photographer', text: 'RentThings saved me thousands on camera gear for a weekend shoot. Seamless experience!', rating: 5 },
  { name: 'James K.', role: 'DIY Enthusiast', text: 'Borrowed a power tool set for my kitchen remodel. Owner was responsive and the item was pristine.', rating: 5 },
  { name: 'Emily R.', role: 'Event Planner', text: 'Found projectors and speakers for our corporate event in minutes. Will use again!', rating: 5 },
];
