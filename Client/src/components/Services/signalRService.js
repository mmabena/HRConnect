import * as signalR from "@microsoft/signalr";

let connection = null;

// Initializes and starts the SignalR connection to the UserPositionHub
export const startSignalRConnection = async () => {
  if (connection) {
    // Already initialized, just return it
    return connection;
  }

  connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5147/userPositionHub")
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
    await connection.stop();
    connection = null;
    console.log("SignalR Disconnected");
  }
};