import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useParams, useNavigate } from 'react-router-dom';
import { MapPin } from 'lucide-react';
import { useState } from 'react';
import ListingMap from '@/components/ui/MapComponent';
import { PremiumGallery } from '@/components/listing/PremiumGallery';
import { OwnerProfileBlock } from '@/components/listing/OwnerProfileBlock';
import { BookingCard } from '@/components/listing/BookingCard';
import { ReviewsSection } from '@/components/listing/ReviewsSection';
import { ListingCardSkeleton } from '@/components/listings/ListingCard';
import { StarRating } from '@/components/ui';
import { listingsApi, rentalsApi, reviewsApi, usersApi } from '@/lib/api';
import api from '@/lib/api';
import { useAuthStore } from '@/stores';

export default function ListingDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { token, user } = useAuthStore();
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [message, setMessage] = useState('');
  
  const [translatedDescription, setTranslatedDescription] = useState<{ listingId: string; text: string } | null>(null);
  const [isTranslating, setIsTranslating] = useState(false);
  
  const qc = useQueryClient();

  const { data: listing, isLoading } = useQuery({
    queryKey: ['listing', id],
    queryFn: () => listingsApi.get(id!),
    enabled: !!id,
  });

  const { data: myRentals } = useQuery({
    queryKey: ['my-rentals'],
    queryFn: rentalsApi.myRentals,
    enabled: !!token,
  });

  const { data: reviews } = useQuery({
    queryKey: ['reviews', id],
    queryFn: () => reviewsApi.forListing(id!),
    enabled: !!id,
  });

  const { data: owner } = useQuery({
    queryKey: ['user', listing?.ownerId],
    queryFn: () => usersApi.get(listing!.ownerId),
    enabled: !!listing?.ownerId,
  });

  const rentalMutation = useMutation({
    mutationFn: () => rentalsApi.create({ listingId: id!, startDate, endDate, message }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['my-rentals'] });
      navigate('/renter/rentals');
    },
  });

  if (isLoading || !listing) {
    return (
      <div className="mx-auto max-w-7xl px-4 py-8 lg:px-8">
        <div className="grid gap-8 lg:grid-cols-5">
          <div className="lg:col-span-3">
            <div className="aspect-[4/3] w-full rounded-2xl bg-slate-100 animate-pulse"></div>
          </div>
          <div className="lg:col-span-2 space-y-6">
            <ListingCardSkeleton />
          </div>
        </div>
      </div>
    );
  }

  const handleTranslate = async (targetLang: 'en' | 'si' | 'ta' | string) => {
    if (!listing) return;
    
    if (targetLang === 'original') {
      setTranslatedDescription(null);
      return;
    }

    setIsTranslating(true);
    try {
      const { data } = await api.post<Array<{ translations: Array<{ text: string }> }>>('/translate', {
        text: listing.description,
        targetLanguage: targetLang,
      });
      setTranslatedDescription({ listingId: listing.id, text: data[0].translations[0].text });
    } catch (error) {
      console.error('Translation failed:', error);
    } finally {
      setIsTranslating(false);
    }
  };

  const days = startDate && endDate ? Math.max(1, (new Date(endDate).getTime() - new Date(startDate).getTime()) / 86400000 + 1) : 0;
  const displayedDescription = translatedDescription?.listingId === listing.id ? translatedDescription.text : listing.description;
  const total = days * listing.pricePerDay;
  const isOwner = user?.id === listing.ownerId;
  const hasPendingRequest = myRentals?.some(
    (r) => r.listingId === listing.id && ['Requested', 'Approved', 'HandedOver', 'Active'].includes(r.status),
  );
  const unavailable = listing.status !== 'Active';
  const canRequest = token && !isOwner && !hasPendingRequest && !unavailable;

  const buttonLabel = !token
    ? 'Sign in to book'
    : isOwner
      ? 'Your listing'
      : hasPendingRequest
        ? 'Already requested'
        : unavailable
          ? 'Unavailable'
          : 'Instant Book';

  return (
    <div className="mx-auto max-w-7xl px-4 py-8 lg:px-8 bg-surface min-h-screen">
      <div className="grid gap-8 lg:grid-cols-5">
        {/* 60% Left Side */}
        <div className="space-y-8 lg:col-span-3">
          <PremiumGallery images={listing.images} title={listing.title} />
          
          <header>
            <div className="flex flex-col justify-between gap-4 sm:flex-row sm:items-start">
              <div>
                <p className="text-sm font-bold text-primary-600 uppercase tracking-wider">{listing.category}</p>
                <h1 className="mt-1 text-3xl font-bold tracking-tight text-slate-900">{listing.title}</h1>
                <p className="mt-2 flex items-center gap-1.5 text-slate-500 font-medium">
                  <MapPin className="h-4 w-4 shrink-0" />
                  {listing.location}
                  {listing.distanceKm != null && (
                    <span className="text-primary-600"> · {listing.distanceKm.toFixed(1)} km away</span>
                  )}
                </p>
              </div>
              <StarRating rating={listing.averageRating} size="md" />
            </div>
          </header>

          {owner && <OwnerProfileBlock owner={owner} />}

          <article className="rounded-3xl border border-slate-100 bg-white p-8 shadow-sm">
            <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-4">
              <h2 className="text-2xl font-bold tracking-tight text-slate-900">Description</h2>
              
              <div className="flex items-center gap-2">
                <span className="text-sm font-medium text-slate-500">Translate:</span>
                <select 
                  className="bg-slate-50 border border-slate-200 text-slate-700 text-sm rounded-xl focus:ring-primary-500 focus:border-primary-500 block p-2 outline-none cursor-pointer transition-colors hover:bg-slate-100"
                  onChange={(e) => handleTranslate(e.target.value)}
                  disabled={isTranslating}
                  defaultValue="original"
                >
                  <option value="original">Original</option>
                  <option value="en">🇺🇸 English ('en')</option>
                  <option value="si">🇱🇰 සිංහල ('si')</option>
                  <option value="ta">🇱🇰 தமிழ் ('ta')</option>
                </select>
              </div>
            </div>

            <div className="prose prose-slate max-w-none text-base leading-relaxed text-slate-600">
              {isTranslating ? (
                <div className="space-y-3 animate-pulse py-2">
                  <div className="h-4 bg-slate-200 rounded-full w-full"></div>
                  <div className="h-4 bg-slate-200 rounded-full w-5/6"></div>
                  <div className="h-4 bg-slate-200 rounded-full w-4/6"></div>
                </div>
              ) : (
                displayedDescription.split('\n').map((paragraph, i) => (
                  <p key={i} className={i > 0 ? 'mt-4' : ''}>{paragraph}</p>
                ))
              )}
            </div>
          </article>

          {/* 🗺️ React-Leaflet Map component injected perfectly with dynamic location data */}
          <ListingMap
            latitude={listing.latitude}
            longitude={listing.longitude}
            location={listing.location}
            mapImageUrl={listing.mapImageUrl}
          />

          {reviews && <ReviewsSection reviews={reviews} />}
        </div>

        {/* 40% Right Side (Sticky Booking Panel) */}
        <div className="lg:col-span-2">
          <div className="sticky top-24">
            <BookingCard
              pricePerDay={listing.pricePerDay}
              deposit={listing.deposit}
              startDate={startDate}
              endDate={endDate}
              message={message}
              days={days}
              total={total}
              canBook={!!canRequest || !token}
              isLoading={rentalMutation.isPending}
              buttonLabel={buttonLabel}
              onStartDateChange={setStartDate}
              onEndDateChange={setEndDate}
              onMessageChange={setMessage}
              onBook={() => {
                if (!token) navigate('/login');
                else rentalMutation.mutate();
              }}
            />
          </div>
        </div>
      </div>
    </div>
  );
}
