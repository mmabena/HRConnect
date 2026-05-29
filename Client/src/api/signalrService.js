import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
  .withUrl(process.env.REACT_APP_SIGNALR_URL||"http://localhost:5147/companyHub")
  .withAutomaticReconnect()
  .build();

export default connection;