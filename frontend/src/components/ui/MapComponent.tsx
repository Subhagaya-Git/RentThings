import React, { useEffect } from 'react';
import { MapContainer, TileLayer, Marker, useMap } from 'react-leaflet';
import 'leaflet/dist/leaflet.css';
import L from 'leaflet';

import markerIcon2x from 'leaflet/dist/images/marker-icon-2x.png';
import markerIcon from 'leaflet/dist/images/marker-icon.png';
import markerShadow from 'leaflet/dist/images/marker-shadow.png';

delete (L.Icon.Default.prototype as L.Icon.Default & { _getIconUrl?: unknown })._getIconUrl;
L.Icon.Default.mergeOptions({
  iconRetinaUrl: markerIcon2x,
  iconUrl: markerIcon,
  shadowUrl: markerShadow,
});

interface MapComponentProps {
  latitude?: number;
  longitude?: number;
  location?: string;
  mapImageUrl?: string;
}

// 🔄 ඛණ්ඩාංක වෙනස් වන විට සිතියම මැදට (Center) ගන්නා අනු-component එකක්
const ChangeMapCenter = ({ center }: { center: [number, number] }) => {
  const map = useMap();
  useEffect(() => {
    map.setView(center, 13); // අලුත් තැන ලැබුණු ගමන් සිතියම එතනට රීසෙට් කරයි
    map.invalidateSize();    // සිතියම කැඩී පෙනීම වළක්වයි
  }, [center, map]);
  return null;
};

const MapComponent: React.FC<MapComponentProps> = ({
  latitude = 7.2906,
  longitude = 80.6337,
}) => {
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
        <Marker position={position} />
        {/* 👈 සිතියම හැමවෙලේම dynamic ලෙස update කරන්න මෙය උදව් වේ */}
        <ChangeMapCenter center={position} />
      </MapContainer>
    </div>
  );
};

export default MapComponent;
