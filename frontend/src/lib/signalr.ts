import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import type { Listing, Notification } from '@/types';

let connection: HubConnection | null = null;

export interface SignalRHandlers {
  onNotification?: (notification: Notification) => void;
  onListingCreated?: (listing: Listing) => void;
  onListingUpdated?: (listing: Listing) => void;
}

export async function startNotificationHub(handlers: SignalRHandlers): Promise<void> {
  await stopNotificationHub();

  const token = localStorage.getItem('rentthings_token');
  
  // සජීවී Azure Web App එකේ SignalR Hub ලිපිනය මෙතනට සෘජුවම එකතු කර ඇත
  const baseUrl = 'https://rentthings-api-bcadewgbakfrbfdd.southeastasia-01.azurewebsites.net/hubs/notifications';
  
  // SignalR .withUrl() එක ඇතුළේ access_token එක සාමාන්‍යයෙන් දෙන්නේ options object එකක් විදිහටයි
  const hub = new HubConnectionBuilder()
    .withUrl(baseUrl, {
      accessTokenFactory: () => token || ''
    })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Information)
    .build();

  if (handlers.onNotification) {
    hub.on('ReceiveNotification', handlers.onNotification);
  }
  if (handlers.onListingCreated) {
    hub.on('ListingCreated', handlers.onListingCreated);
  }
  if (handlers.onListingUpdated) {
    hub.on('ListingUpdated', handlers.onListingUpdated);
  }

  try {
    await hub.start();
    connection = hub;
  } catch (err) {
    await hub.stop().catch(() => undefined);
    throw err;
  }
}

export async function stopNotificationHub(): Promise<void> {
  if (connection) {
    await connection.stop();
    connection = null;
  }
}

export function isNotificationHubConnected(): boolean {
  return connection?.state === 'Connected';
}