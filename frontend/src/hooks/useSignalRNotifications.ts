import { useEffect } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { startNotificationHub, stopNotificationHub } from '@/lib/signalr';
import { useAuthStore } from '@/stores';
import type { Listing, PagedResult } from '@/types';

function invalidateListingQueries(qc: ReturnType<typeof useQueryClient>) {
  qc.invalidateQueries({ queryKey: ['search'] });
  qc.invalidateQueries({ queryKey: ['featured'] });
  qc.invalidateQueries({ queryKey: ['owner-dashboard'] });
  qc.invalidateQueries({ queryKey: ['admin-flagged'] });
  qc.invalidateQueries({ queryKey: ['listing'] });
  qc.invalidateQueries({ queryKey: ['admin-stats'] });
}

function updateListingInCache(qc: ReturnType<typeof useQueryClient>, listing: Listing) {
  // Update specific listing in cache
  qc.setQueryData(['listing', listing.id], listing);

  // Update search results cache
  qc.setQueriesData(
    { queryKey: ['search'] },
    (old: PagedResult<Listing> | undefined) => {
      if (!old || !old.items) return old;
      return {
        ...old,
        items: old.items.map((item: Listing) => 
          item.id === listing.id ? listing : item
        )
      };
    }
  );

  // Update featured listings cache
  qc.setQueriesData(
    { queryKey: ['featured'] },
    (old: Listing[] | undefined) => {
      if (!old) return old;
      return old.map((item: Listing) => 
        item.id === listing.id ? listing : item
      );
    }
  );

  // Invalidate other queries to ensure consistency
  invalidateListingQueries(qc);
}

export function useSignalRNotifications() {
  const { token } = useAuthStore();
  const qc = useQueryClient();

  useEffect(() => {
    let cancelled = false;

    startNotificationHub({
      onListingCreated: (listing: Listing) => {
        if (!cancelled) {
          console.log('SignalR: Listing created', listing.id);
          updateListingInCache(qc, listing);
        }
      },
      onListingUpdated: (listing: Listing) => {
        if (!cancelled) {
          console.log('SignalR: Listing updated', listing.id);
          updateListingInCache(qc, listing);
        }
      },
      ...(token
        ? {
            onNotification: () => {
              if (!cancelled) {
                console.log('SignalR: Notification received');
                qc.invalidateQueries({ queryKey: ['notifications'] });
              }
            },
          }
        : {}),
    }).catch((err) => {
      if (!cancelled) console.error('SignalR connection failed:', err);
    });

    return () => {
      cancelled = true;
      stopNotificationHub();
    };
  }, [token, qc]);
}
