import React, { useState } from 'react';
import axios, { AxiosError } from 'axios';

export default function CreateListing() {
    const [title, setTitle] = useState('');
    const [price, setPrice] = useState('');
    const [file, setFile] = useState<File | null>(null);
    const [message, setMessage] = useState('');
    const [loading, setLoading] = useState(false);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!file) return alert("කරුණාකර පින්තූරයක් තෝරන්න!");

        setLoading(true);
        setMessage('');

        // 🖼️ FormData සාදා ගැනීම (පින්තූර ඇතුළත් API සඳහා අනිවාර්ය වේ)
        const formData = new FormData();
        formData.append("file", file);
        formData.append("title", title);
        formData.append("price", price);

        try {
const response = await axios.post(`${import.meta.env.VITE_API_BASE_URL}/api/listings/create`, formData, {
                headers: { 'Content-Type': 'multipart/form-data' }
            });
            setMessage(`✅ ${response.data.message}`);
        } catch (error) {
            const axiosError = error as AxiosError<{ message?: string }>;
            const errorMsg = axiosError.response?.data?.message || "යම් දෝෂයක් සිදු විය.";
            setMessage(`❌ ${errorMsg}`);
        } finally {
            setLoading(false);
        }
    };

    return (
        <div style={{ maxWidth: '400px', margin: '20px auto', padding: '20px', border: '1px solid #ccc', borderRadius: '8px' }}>
            <h3>➕ අලුත් භාණ්ඩයක් ඇතුළත් කරන්න</h3>
            <form onSubmit={handleSubmit}>
                <input type="text" placeholder="භාණ්ඩයේ නම" value={title} onChange={e => setTitle(e.target.value)} required style={{ width: '100%', marginBottom: '10px', padding: '8px' }} /><br />
                <input type="number" placeholder="දිනක කුලිය (Rs.)" value={price} onChange={e => setPrice(e.target.value)} required style={{ width: '100%', marginBottom: '10px', padding: '8px' }} /><br />
                <input type="file" accept="image/*" onChange={e => setFile(e.target.files ? e.target.files[0] : null)} required style={{ marginBottom: '15px' }} /><br />
                <button type="submit" disabled={loading} style={{ width: '100%', padding: '10px', backgroundColor: '#007bef', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer' }}>
                    {loading ? "AI පරීක්ෂා කරමින් පවතී..." : "භාණ්ඩය සුරකින්න"}
                </button>
            </form>
            {message && <p style={{ marginTop: '15px', fontWeight: 'bold' }}>{message}</p>}
        </div>
    );
}
