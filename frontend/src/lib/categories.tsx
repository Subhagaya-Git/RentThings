import {
  Camera,
  Dumbbell,
  Home,
  Laptop,
  PartyPopper,
  Plane,
  Speaker,
  Tent,
  Wrench,
  type LucideIcon,
} from 'lucide-react';

export interface CategoryItem {
  name: string;
  icon: LucideIcon;
  color: string;
}

export const categoryItems: CategoryItem[] = [
  { name: 'Cameras', icon: Camera, color: 'bg-indigo-50 text-indigo-600 group-hover:bg-indigo-100' },
  { name: 'Power Tools', icon: Wrench, color: 'bg-amber-50 text-amber-600 group-hover:bg-amber-100' },
  { name: 'Camping Gear', icon: Tent, color: 'bg-emerald-50 text-emerald-600 group-hover:bg-emerald-100' },
  { name: 'Drones', icon: Plane, color: 'bg-sky-50 text-sky-600 group-hover:bg-sky-100' },
  { name: 'Sports Equipment', icon: Dumbbell, color: 'bg-rose-50 text-rose-600 group-hover:bg-rose-100' },
  { name: 'Event Equipment', icon: PartyPopper, color: 'bg-violet-50 text-violet-600 group-hover:bg-violet-100' },
  { name: 'Speakers', icon: Speaker, color: 'bg-orange-50 text-orange-600 group-hover:bg-orange-100' },
  { name: 'Electronics', icon: Laptop, color: 'bg-cyan-50 text-cyan-600 group-hover:bg-cyan-100' },
  { name: 'Home Appliances', icon: Home, color: 'bg-slate-100 text-slate-600 group-hover:bg-slate-200' },
];

export const filterCategories = categoryItems.map((c) => c.name);
