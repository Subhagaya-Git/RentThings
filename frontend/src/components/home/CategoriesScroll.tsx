import { Link } from 'react-router-dom';

const categories = [
  { name: 'Cameras', emoji: '📷' },
  { name: 'Power Tools', emoji: '🔧' },
  { name: 'Camping', emoji: '⛺' },
  { name: 'Event Equipment', emoji: '🎥' },
  { name: 'Vehicles', emoji: '🚲' },
  { name: 'Electronics', emoji: '💻' },
  { name: 'Construction', emoji: '🏗️' },
];

export function CategoriesScroll() {
  return (
    <section className="border-b border-slate-100 bg-white py-6" aria-label="Categories">
      <div className="mx-auto max-w-7xl px-4 lg:px-8">
        <div className="scrollbar-hide flex gap-3 overflow-x-auto pb-2">
          {categories.map((cat) => (
            <Link
              key={cat.name}
              to={`/search?category=${encodeURIComponent(cat.name)}`}
              className="flex shrink-0 items-center gap-2 rounded-full bg-primary-100 px-5 py-2.5 text-sm font-semibold text-primary-700 transition-colors hover:bg-primary-200"
            >
              <span className="text-lg">{cat.emoji}</span>
              <span>{cat.name}</span>
            </Link>
          ))}
        </div>
      </div>
    </section>
  );
}
