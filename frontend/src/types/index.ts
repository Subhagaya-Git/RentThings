export type UserRole = 'Renter' | 'Owner' | 'Admin';
export type TrustLevel = 'Bronze' | 'Silver' | 'Gold' | 'Platinum';

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  phone?: string;
  bio?: string;
  location?: string;
  avatarUrl?: string;
  role: UserRole;
  trustScore: number;
  trustLevel: TrustLevel;
  isVerified: boolean;
  isActive?: boolean;
  createdAt: string;
}

export interface ListingImage {
  id: string;
  url: string;
  thumbnailUrl?: string;
  isPrimary: boolean;
  passedValidation: boolean;
  validationNotes?: string;
}

export interface Listing {
  id: string;
  ownerId: string;
  ownerName: string;
  title: string;
  description: string;
  category: string;
  pricePerDay: number;
  deposit: number;
  location: string;
  city?: string;
  state?: string;
  status: string;
  averageRating: number;
  reviewCount: number;
  isFeatured: boolean;
  isAiVerified?: boolean;
  images: ListingImage[];
  createdAt: string;
  latitude?: number;
  longitude?: number;
  distanceKm?: number;
  mapImageUrl?: string;
}

export interface Rental {
  id: string;
  listingId: string;
  listingTitle: string;
  listingImage?: string;
  renterId: string;
  renterName: string;
  ownerId: string;
  ownerName: string;
  startDate: string;
  endDate: string;
  status: string;
  totalPrice: number;
  depositAmount: number;
  message?: string;
  rejectionReason?: string;
  createdAt: string;
}

export interface Review {
  id: string;
  rentalId: string;
  reviewerId: string;
  reviewerName: string;
  rating: number;
  comment: string;
  isOwnerReview: boolean;
  createdAt: string;
}

export interface Notification {
  id: string;
  type: string;
  title: string;
  message: string;
  actionUrl?: string;
  isRead: boolean;
  createdAt: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface AdminStats {
  totalUsers: number;
  totalListings: number;
  activeRentals: number;
  completedRentals: number;
  totalRevenue: number;
  flaggedListings: number;
  pendingReports: number;
  monthlyRentals: { month: string; count: number; revenue: number }[];
  rentalsByCategory?: { category: string; count: number }[];
  rentalsByStatus?: { status: string; count: number }[];
}

export interface OwnerDashboard {
  activeListings: number;
  inactiveListings: number;
  pendingRequests: number;
  activeRentals: number;
  totalEarnings: number;
  listings: Listing[];
  requests: Rental[];
  activeRentalsList: Rental[];
}

export interface Report {
  id: string;
  reporterId: string;
  reporterName: string;
  reportedUserId?: string;
  reportedUserName?: string;
  reportedListingId?: string;
  reportedListingTitle?: string;
  reason: string;
  description: string;
  isResolved: boolean;
  createdAt: string;
}

export interface PlatformStats {
  totalListings: number;
  totalRentals: number;
  happyRenters: number;
  averageRating: number;
}

export interface ImageValidation {
  isValid: boolean;
  hasInappropriateContent: boolean;
  isLowQuality: boolean;
  isBlurry: boolean;
  hasVisibleObject: boolean;
  qualityScore: number;
  issues: string[];
  recommendations: string[];
  category: string;
  subcategory: string;
  tags: string[];
  confidence: number;
  flagged: boolean;
}

export interface AiListingSuggestion {
  title: string;
  description: string;
  category: string;
  rentalTips: string[];
  suggestedCategories: string[];
}
