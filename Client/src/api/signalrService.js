import * as signalR from "@microsoft/signalr";

export const companyHubConnection = new signalR.HubConnectionBuilder()
  .withUrl(process.env.REACT_APP_COMPANY_HUB_URL)
  .withAutomaticReconnect()
  .build();

export const userManagementHubConnection = new signalR.HubConnectionBuilder()
  .withUrl(process.env.REACT_APP_USER_MANAGEMENT_HUB_URL)
  .withAutomaticReconnect()
  .build();

export const connection = new signalR.HubConnectionBuilder()
  .withUrl(process.env.REACT_APP_SIGNALR_URL)
  .withAutomaticReconnect()
  .build();


