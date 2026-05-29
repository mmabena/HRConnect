import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
  .withUrl(process.env.REACT_APP_SIGNALR_URL)
  .withAutomaticReconnect()
  .build();

export default connection;
