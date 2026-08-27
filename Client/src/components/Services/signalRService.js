import * as signalR from "@microsoft/signalr";

let connection = null;
let startPromise = null;


// Initializes and starts the SignalR connection to the UserPositionHub
export const startSignalRConnection = async () => {
  if (connection) {
    // Already initialized, just return it
    return connection;
  }

  connection = new signalR.HubConnectionBuilder()
    .withUrl(process.env.REACT_APP_USER_POSITION_HUB_URL)
    .withAutomaticReconnect()
    .build();

  try {
    await connection.start();
    console.log("SignalR Connected");

    // Example: add global event listener
    connection.on("UserPositionUpdated", (updatedEmployee) => {
      console.log("Employee updated:", updatedEmployee);
      // You can update a global state here or emit events
    });

  } catch (err) {
    console.error("SignalR Connection Error:", err);
    setTimeout(startSignalRConnection, 5000); // retry if failed
  }

  return connection;
};

// Getter for other components
export const getConnection = () => connection;

// Optional: stop connection (logout)
export const stopSignalRConnection = async () => {
  if (connection) {
    try {
      await connection.stop();
      console.log("SignalR Disconnected");
    } catch (err) {
      console.error("Error stopping SignalR connection:", err);
    } finally {
      connection = null;
      startPromise = null;
    }
  }
};