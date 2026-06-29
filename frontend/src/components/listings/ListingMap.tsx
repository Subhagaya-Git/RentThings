import { MapContainer, TileLayer, Marker, Popup } from 'react-leaflet';
import 'leaflet/dist/leaflet.css';
import L from 'leaflet';

// Import marker images directly to fix Leaflet's default icon loading issue in build tools
import markerIcon2x from 'leaflet/dist/images/marker-icon-2x.png';
import markerIcon from 'leaflet/dist/images/marker-icon.png';
import markerShadow from 'leaflet/dist/images/marker-shadow.png';

// Fix for Leaflet default icon URL issues with React/TypeScript packaging
delete (L.Icon.Default.prototype as L.Icon.Default & { _getIconUrl?: unknown })._getIconUrl;

L.Icon.Default.mergeOptions({
  iconRetinaUrl: markerIcon2x,
  iconUrl: markerIcon,
  shadowUrl: markerShadow,
});

interface ListingMapProps {
  latitude?: number;
  longitude?: number;
}

const ListingMap: React.FC<ListingMapProps> = ({ latitude = 7.2906, longitude = 80.6337 }) => {
  const position: [number, number] = [latitude, longitude];

  return (
    <div className="w-full h-[300px] rounded-lg overflow-hidden border border-slate-100 shadow-sm">
      <MapContainer 
        center={position} 
        zoom={13} 
        scrollWheelZoom={false} 
        style={{ height: '100%', width: '100%' }}
      >
        <TileLayer
          attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
          url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
        />
        <Marker position={position}>
          <Popup>
            Item Location <br /> Central Province, Sri Lanka.
          </Popup>
        </Marker>
      </MapContainer>
    </div>
  );
};

export default ListingMap;
