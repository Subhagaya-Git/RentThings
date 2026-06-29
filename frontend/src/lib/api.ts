import axios from 'axios';
import type {
  AdminStats,
  AiListingSuggestion,
  ImageValidation,
  Listing,
  ListingImage,
  Notification,
  OwnerDashboard,
  PagedResult,
  PlatformStats,
  Rental,
  Report,
  Review,
  User,
} from '@/types';

// සජීවී Azure Web App API URL එක සෘජුවම ඇතුළත් කර ඇත
const api = axios.create({
  baseURL: 'https://rentthings-api-bcadewgbakfrbfdd.southeastasia-01.azurewebsites.net/api',
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('rentthings_token');
  if (token) config.headers.Authorization = `Bearer ${token}`;

  // FormData: omit Content-Type so the browser sets multipart boundary automatically
  if (config.data instanceof FormData) {
    delete config.headers['Content-Type'];
  } else if (!config.headers['Content-Type']) {
    config.headers['Content-Type'] = 'application/json';
  }

  return config;
});

export const authApi = {
  login: (email: string, password: string) =>
    api.post<{ token: string; user: User }>('/auth/login', { email, password }).then((r) => r.data),
  register: (data: { email: string; password: string; firstName: string; lastName: string; role?: string }) =>
    api.post<{ token: string; user: User }>('/auth/register', data).then((r) => r.data),
  passwordReset: (email: string) => api.post('/auth/password-reset', { email, password: '' }),
};

export const listingsApi = {
  search: (params: Record<string, string | number | undefined>) =>
    api.get<PagedResult<Listing>>('/listings', { params }).then((r) => r.data),
  featured: () => api.get<Listing[]>('/listings/featured').then((r) => r.data),
  categories: () => api.get<string[]>('/listings/categories').then((r) => r.data),
  get: (id: string) => api.get<Listing>(`/listings/${id}`).then((r) => r.data),
  create: (data: Partial<Listing> & { availableFrom?: string; availableTo?: string }) => api.post<Listing>('/listings', data).then((r) => r.data),
  update: (id: string, data: Partial<Listing> & { availableFrom?: string; availableTo?: string }) => api.put<Listing>(`/listings/${id}`, data).then((r) => r.data),
  delete: (id: string) => api.delete(`/listings/${id}`),
  myListings: () => api.get<Listing[]>('/listings/owner/mine').then((r) => r.data),
  ownerDashboard: () => api.get<OwnerDashboard>('/listings/owner/dashboard').then((r) => r.data),
  uploadImage: (id: string, file: File) => {
    const form = new FormData();
    form.append('file', file, file.name);
    return api.post<ListingImage>(`/listings/${id}/images`, form).then((r) => r.data);
  },
  deleteImage: (listingId: string, imageId: string) => api.delete(`/listings/${listingId}/images/${imageId}`),
};

export const rentalsApi = {
  create: (data: { listingId: string; startDate: string; endDate: string; message?: string }) =>
    api.post<Rental>('/rentals', data).then((r) => r.data),
  get: (id: string) => api.get<Rental>(`/rentals/${id}`).then((r) => r.data),
  updateStatus: (id: string, status: string, notes?: string) =>
    api.patch<Rental>(`/rentals/${id}/status`, { status, notes }).then((r) => r.data),
  myRentals: () => api.get<Rental[]>('/rentals/renter/mine').then((r) => r.data),
  ownerRequests: () => api.get<Rental[]>('/rentals/owner/requests').then((r) => r.data),
};

export const reviewsApi = {
  forListing: (listingId: string) => api.get<Review[]>(`/reviews/listing/${listingId}`).then((r) => r.data),
  create: (data: { rentalId: string; rating: number; comment: string }) =>
    api.post<Review>('/reviews', data).then((r) => r.data),
};

export const notificationsApi = {
  getAll: () => api.get<Notification[]>('/notifications').then((r) => r.data),
  markRead: (id: string) => api.patch(`/notifications/${id}/read`),
  markAllRead: () => api.post('/notifications/read-all'),
};

export const usersApi = {
  me: () => api.get<User>('/users/me').then((r) => r.data),
  update: (data: Partial<User>) => api.put<User>('/users/me', data).then((r) => r.data),
  get: (id: string) => api.get<User>(`/users/${id}`).then((r) => r.data),
};

export const adminApi = {
  stats: () => api.get<AdminStats>('/admin/stats').then((r) => r.data),
  users: (search?: string, role?: string) =>
    api.get<User[]>('/admin/users', { params: { search, role } }).then((r) => r.data),
  flaggedListings: () => api.get<Listing[]>('/admin/flagged-listings').then((r) => r.data),
  updateTrustScore: (id: string, trustScore: number) =>
    api.patch<User>(`/admin/users/${id}/trust-score`, { trustScore }).then((r) => r.data),
  suspendUser: (id: string, isActive: boolean) =>
    api.patch<User>(`/admin/users/${id}/suspend`, { isActive }).then((r) => r.data),
  updateListingStatus: (id: string, status: string) =>
    api.patch(`/admin/listings/${id}/status`, { status }),
  rentals: () => api.get<Rental[]>('/admin/rentals').then((r) => r.data),
  reports: () => api.get<Report[]>('/admin/reports').then((r) => r.data),
  resolveReport: (id: string) => api.patch(`/admin/reports/${id}/resolve`),
};

export const statsApi = {
  platform: () => api.get<PlatformStats>('/stats/platform').then((r) => r.data),
};

export const favoritesApi = {
  getAll: () => api.get<Listing[]>('/favorites').then((r) => r.data),
  add: (listingId: string) => api.post(`/favorites/${listingId}`),
  remove: (listingId: string) => api.delete(`/favorites/${listingId}`),
};

export const aiApi = {
  generateListing: (file?: File, hint?: string) => {
    const form = new FormData();
    if (file) form.append('image', file, file.name);
    if (hint) form.append('hint', hint);
    return api.post<AiListingSuggestion>('/ai/generate-listing', form).then((r) => r.data);
  },
  validateImage: (file: File) => {
    const form = new FormData();
    form.append('file', file, file.name);
    return api.post<ImageValidation>('/ai/validate-image', form).then((r) => r.data);
  },
  chat: (message: string, conversationId?: string) =>
    api.post<{ reply: string; conversationId: string }>('/ai/chat', { message, conversationId }).then((r) => r.data),
};

export const mapsApi = {
  geocode: (address: string) =>
    api.get<{ latitude: number; longitude: number; formattedAddress: string }>('/maps/geocode', { params: { address } }).then((r) => r.data),
  
  // Static Map URL එකටත් සජීවී Azure Domain එක සෘජුවම එකතු කර ඇත
  staticMapUrl: (lat: number, lon: number, zoom = 14) =>
    `https://rentthings-api-bcadewgbakfrbfdd.southeastasia-01.azurewebsites.net/api/maps/static?lat=${lat}&lon=${lon}&zoom=${zoom}`,
    
  distance: (lat1: number, lon1: number, lat2: number, lon2: number) =>
    api.get<number>('/maps/distance', { params: { lat1, lon1, lat2, lon2 } }).then((r) => r.data),
};

export default api;