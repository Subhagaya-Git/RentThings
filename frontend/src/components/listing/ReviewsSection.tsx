import { StarRating } from '@/components/ui';
import { formatDate } from '@/lib/utils';
import type { Review } from '@/types';

interface ReviewsSectionProps {
  reviews: Review[];
}

export function ReviewsSection({ reviews }: ReviewsSectionProps) {
  if (!reviews.length) return null;

  const avgRating = reviews.reduce((sum, r) => sum + r.rating, 0) / reviews.length;

  return (
    <div className="rounded-2xl border border-slate-100 bg-white p-6 shadow-sm">
      <div className="flex items-center justify-between border-b border-slate-100 pb-4">
        <h2 className="text-lg font-semibold text-slate-900">Reviews</h2>
        <div className="flex items-center gap-2">
          <StarRating rating={avgRating} size="md" />
          <span className="text-sm text-slate-500">({reviews.length})</span>
        </div>
      </div>

      <ul className="mt-4 divide-y divide-slate-100">
        {reviews.map((review) => (
          <li key={review.id} className="py-4 first:pt-0 last:pb-0">
            <div className="flex items-start justify-between gap-4">
              <div className="flex items-center gap-3">
                <div className="flex h-9 w-9 items-center justify-center rounded-full bg-slate-100 text-sm font-semibold text-slate-600">
                  {review.reviewerName[0]}
                </div>
                <div>
                  <p className="text-sm font-medium text-slate-900">{review.reviewerName}</p>
                  <p className="text-xs text-slate-400">{formatDate(review.createdAt)}</p>
                </div>
              </div>
              <StarRating rating={review.rating} />
            </div>
            <p className="mt-3 text-sm leading-relaxed text-slate-600">{review.comment}</p>
          </li>
        ))}
      </ul>
    </div>
  );
}
