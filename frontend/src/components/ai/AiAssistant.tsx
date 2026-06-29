import { useState } from 'react';
import { MessageCircle, Send, X } from 'lucide-react';
import { aiApi } from '@/lib/api';
import { Button } from '../ui';

export function AiAssistant() {
  const [open, setOpen] = useState(false);
  const [messages, setMessages] = useState<{ role: 'user' | 'assistant'; text: string }[]>([
    { role: 'assistant', text: "Hi! I'm RentThings AI. Ask me about bookings, listings, trust scores, or returns." },
  ]);
  const [input, setInput] = useState('');
  const [loading, setLoading] = useState(false);
  const [conversationId, setConversationId] = useState<string>();

  const send = async () => {
    if (!input.trim() || loading) return;
    const msg = input.trim();
    setInput('');
    setMessages((m) => [...m, { role: 'user', text: msg }]);
    setLoading(true);
    try {
      const res = await aiApi.chat(msg, conversationId);
      setConversationId(res.conversationId);
      setMessages((m) => [...m, { role: 'assistant', text: res.reply }]);
    } catch {
      setMessages((m) => [...m, { role: 'assistant', text: 'Sorry, I had trouble responding. Please try again.' }]);
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      <button
        onClick={() => setOpen(true)}
        className="fixed bottom-6 right-6 z-50 flex h-14 w-14 items-center justify-center rounded-full bg-brand-600 text-white shadow-lg shadow-brand-600/30 hover:bg-brand-700 transition-all hover:scale-105"
        aria-label="Open AI assistant"
      >
        <MessageCircle className="h-6 w-6" />
      </button>

      {open && (
        <div className="fixed bottom-24 right-6 z-50 w-[min(400px,calc(100vw-2rem))] glass rounded-2xl shadow-2xl flex flex-col max-h-[500px]" role="dialog" aria-label="AI Assistant">
          <div className="flex items-center justify-between border-b border-slate-100 px-4 py-3">
            <div>
              <h3 className="font-semibold text-slate-800">RentThings AI</h3>
              <p className="text-xs text-slate-500">Powered by Azure AI Services</p>
            </div>
            <button onClick={() => setOpen(false)} aria-label="Close assistant" className="p-1 rounded-lg hover:bg-slate-100">
              <X className="h-5 w-5" />
            </button>
          </div>
          <div className="flex-1 overflow-y-auto p-4 space-y-3" aria-live="polite">
            {messages.map((m, i) => (
              <div key={i} className={`flex ${m.role === 'user' ? 'justify-end' : 'justify-start'}`}>
                <div className={`max-w-[85%] rounded-2xl px-4 py-2 text-sm ${m.role === 'user' ? 'bg-brand-600 text-white' : 'bg-slate-100 text-slate-700'}`}>
                  {m.text}
                </div>
              </div>
            ))}
            {loading && <div className="text-sm text-slate-400 animate-pulse">Thinking...</div>}
          </div>
          <div className="border-t border-slate-100 p-3 flex gap-2">
            <input
              value={input}
              onChange={(e) => setInput(e.target.value)}
              onKeyDown={(e) => e.key === 'Enter' && send()}
              placeholder="Ask a question..."
              className="flex-1 rounded-xl border border-slate-200 px-3 py-2 text-sm"
              aria-label="Chat message"
            />
            <Button size="sm" onClick={send} loading={loading} aria-label="Send message">
              <Send className="h-4 w-4" />
            </Button>
          </div>
        </div>
      )}
    </>
  );
}
