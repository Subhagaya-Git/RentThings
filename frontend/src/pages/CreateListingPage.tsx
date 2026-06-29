import { useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { Sparkles, Upload, X } from 'lucide-react';
import { Button, Card, Input, Textarea } from '@/components/ui';
import { aiApi, listingsApi } from '@/lib/api';
import type { AiListingSuggestion, ImageValidation, ListingImage } from '@/types';

const DISTRICTS = [
  'Colombo', 'Gampaha', 'Kalutara', 'Kandy', 'Matale', 'Nuwara Eliya',
  'Galle', 'Matara', 'Hambantota', 'Jaffna', 'Kurunegala', 'Badulla',
];

const CATEGORIES = ['Cameras', 'Power Tools', 'Camping Gear', 'Sports Equipment', 'Event Equipment', 'Speakers', 'Home Appliances', 'Electronics'];

interface PhotoPreview {
  id?: string;
  file?: File;
  url: string;
  validation?: ImageValidation;
  uploading?: boolean;
}

function ListingForm({ editId }: { editId?: string }) {
  const navigate = useNavigate();
  const [form, setForm] = useState({
    title: '', description: '', category: 'Cameras', pricePerDay: 2500, deposit: 10000,
    location: '', city: '', state: 'Western Province',
    availableFrom: '', availableTo: '',
  });
  const [listingId, setListingId] = useState<string | null>(editId ?? null);
  const [photos, setPhotos] = useState<PhotoPreview[]>([]);
  const [aiSuggestion, setAiSuggestion] = useState<AiListingSuggestion | null>(null);
  const [generating, setGenerating] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const ensureListing = async () => {
    if (listingId) return listingId;
    const payload = {
      ...form,
      location: form.city ? `${form.city}, ${form.state}` : form.location,
      availableFrom: form.availableFrom || undefined,
      availableTo: form.availableTo || undefined,
    };
    const listing = await listingsApi.create(payload);
    setListingId(listing.id);
    return listing.id;
  };

  const handleFiles = async (files: FileList | null) => {
    if (!files?.length) return;
    setError('');
    for (const file of Array.from(files)) {
      const previewUrl = URL.createObjectURL(file);
      const tempId = crypto.randomUUID();
      setPhotos((p) => [...p, { id: tempId, file, url: previewUrl, uploading: true }]);

      try {
        const validation = await aiApi.validateImage(file);
        setPhotos((p) => p.map((ph) => ph.id === tempId ? { ...ph, validation, uploading: false } : ph));
        if (validation.isValid) {
          const id = await ensureListing();
          const uploaded = await listingsApi.uploadImage(id, file) as ListingImage;
          setPhotos((p) => p.map((ph) => ph.id === tempId ? { id: uploaded.id, file, url: uploaded.url, validation } : ph));
          if (validation.category) setForm((f) => ({ ...f, category: validation.category }));
        }
      } catch {
        setPhotos((p) => p.filter((ph) => ph.id !== tempId));
        setError('Failed to upload image.');
      }
    }
  };

  const removePhoto = async (photo: PhotoPreview) => {
    if (photo.id && listingId && !photo.file) {
      await listingsApi.deleteImage(listingId, photo.id);
    }
    setPhotos((p) => p.filter((x) => x !== photo));
  };

  const handleAiGenerate = async () => {
    setGenerating(true);
    setError('');
    try {
      const firstFile = photos.find((p) => p.file)?.file;
      const hint = form.title.trim() || undefined;
      const suggestion = await aiApi.generateListing(firstFile, hint);
      setAiSuggestion(suggestion);
      const category = CATEGORIES.includes(suggestion.category)
        ? suggestion.category
        : suggestion.suggestedCategories.find((c) => CATEGORIES.includes(c)) ?? suggestion.category;
      setForm((f) => ({
        ...f,
        title: suggestion.title || f.title,
        description: suggestion.description,
        category,
      }));
    } catch {
      setError('Failed to generate description with AI.');
    } finally {
      setGenerating(false);
    }
  };

  const handlePublish = async () => {
    setSaving(true);
    setError('');
    try {
      const payload = {
        ...form,
        location: form.city ? `${form.city}, ${form.state}` : form.location,
        availableFrom: form.availableFrom || undefined,
        availableTo: form.availableTo || undefined,
      };
      if (listingId) await listingsApi.update(listingId, payload);
      else await listingsApi.create(payload);
      navigate('/owner/dashboard');
    } catch {
      setError('Failed to save listing.');
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="max-w-2xl mx-auto space-y-6">
      <h1 className="text-2xl font-bold text-slate-900">{editId ? 'Edit listing' : 'Create listing'}</h1>

      <Card>
        <h2 className="font-semibold mb-4 flex items-center gap-2"><Upload className="h-5 w-5" /> Photos</h2>
        <label className="flex flex-col items-center justify-center border-2 border-dashed border-slate-200 rounded-2xl p-8 cursor-pointer hover:border-brand-300 transition-colors">
          <Upload className="h-8 w-8 text-slate-400 mb-2" />
          <span className="text-sm text-slate-500">Click to upload one or more photos</span>
          <input type="file" accept="image/*" multiple onChange={(e) => handleFiles(e.target.files)} className="hidden" aria-label="Upload listing photos" />
        </label>

        {photos.length > 0 && (
          <div className="mt-4 grid grid-cols-2 sm:grid-cols-3 gap-3">
            {photos.map((photo) => (
              <div key={photo.id ?? photo.url} className="relative group rounded-xl overflow-hidden border border-slate-200">
                <img src={photo.url} alt="" className="aspect-square w-full object-cover" />
                <button
                  type="button"
                  onClick={() => removePhoto(photo)}
                  className="absolute top-1 right-1 rounded-full bg-red-600 text-white p-1 opacity-90 hover:opacity-100"
                  aria-label="Remove photo"
                >
                  <X className="h-3 w-3" />
                </button>
                {photo.uploading && <div className="absolute inset-0 bg-black/40 flex items-center justify-center text-white text-xs">Uploading...</div>}
                {photo.validation && (
                  <div className={`absolute bottom-0 inset-x-0 px-2 py-1 text-xs ${photo.validation.isValid ? 'bg-green-600/90 text-white' : 'bg-red-600/90 text-white'}`}>
                    {photo.validation.isValid ? 'Validated' : 'Failed'}
                  </div>
                )}
              </div>
            ))}
          </div>
        )}

        <Button variant="secondary" className="mt-4" loading={generating} onClick={handleAiGenerate}>
          <Sparkles className="h-4 w-4" /> Generate description with AI
        </Button>
      </Card>

      {aiSuggestion && (
        <Card className="bg-brand-50 border-brand-100">
          <h3 className="font-semibold text-brand-800 mb-2">AI Suggestions</h3>
          {aiSuggestion.suggestedCategories.length > 0 && (
            <p className="text-sm text-brand-700 mb-2">
              Suggested tags: {aiSuggestion.suggestedCategories.join(', ')}
            </p>
          )}
          <p className="text-sm text-brand-700">{aiSuggestion.rentalTips.join(' · ')}</p>
        </Card>
      )}

      <Card>
        <div className="space-y-4">
          <div>
            <label className="text-sm font-medium text-slate-600">Title</label>
            <Input value={form.title} onChange={(e) => setForm({ ...form, title: e.target.value })} className="mt-1" />
          </div>
          <div>
            <label className="text-sm font-medium text-slate-600">Description</label>
            <Textarea value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} className="mt-1" />
          </div>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label className="text-sm font-medium text-slate-600">Category</label>
              <select value={form.category} onChange={(e) => setForm({ ...form, category: e.target.value })} className="mt-1 w-full rounded-xl border border-slate-200 px-3 py-2 text-sm">
                {CATEGORIES.map((c) => <option key={c}>{c}</option>)}
              </select>
            </div>
            <div>
              <label className="text-sm font-medium text-slate-600">District</label>
              <select value={form.city} onChange={(e) => setForm({ ...form, city: e.target.value })} className="mt-1 w-full rounded-xl border border-slate-200 px-3 py-2 text-sm">
                <option value="">Select district</option>
                {DISTRICTS.map((d) => <option key={d} value={d}>{d}</option>)}
              </select>
            </div>
          </div>
          <div>
            <label className="text-sm font-medium text-slate-600">Location details</label>
            <Input value={form.location} onChange={(e) => setForm({ ...form, location: e.target.value })} placeholder="e.g. Kandy town center" className="mt-1" />
          </div>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label className="text-sm font-medium text-slate-600">Price per day (LKR)</label>
              <Input type="number" value={form.pricePerDay} onChange={(e) => setForm({ ...form, pricePerDay: +e.target.value })} className="mt-1" />
            </div>
            <div>
              <label className="text-sm font-medium text-slate-600">Deposit (LKR)</label>
              <Input type="number" value={form.deposit} onChange={(e) => setForm({ ...form, deposit: +e.target.value })} className="mt-1" />
            </div>
          </div>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label className="text-sm font-medium text-slate-600">Available from</label>
              <Input type="date" value={form.availableFrom} onChange={(e) => setForm({ ...form, availableFrom: e.target.value })} className="mt-1" />
            </div>
            <div>
              <label className="text-sm font-medium text-slate-600">Available to</label>
              <Input type="date" value={form.availableTo} onChange={(e) => setForm({ ...form, availableTo: e.target.value })} min={form.availableFrom} className="mt-1" />
            </div>
          </div>
          {error && <p className="text-sm text-red-600">{error}</p>}
          <Button className="w-full" loading={saving} onClick={handlePublish}>
            {editId ? 'Save changes' : 'Publish listing'}
          </Button>
        </div>
      </Card>
    </div>
  );
}

export default function CreateListingPage() {
  return <ListingForm />;
}

export function EditListingPage() {
  const { id } = useParams<{ id: string }>();
  return <ListingForm editId={id} />;
}
