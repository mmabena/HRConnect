import * as signalR from "@microsoft/signalr";

const HUB_URL = "http://localhost:5147/leaveHub";

let connection = null;

export const startLeaveHubConnection = async (employeeId) => {
  if (connection) {
    return connection;
  }

  connection = new signalR.HubConnectionBuilder()
    .withUrl(HUB_URL, {
      accessTokenFactory: () => localStorage.getItem("token"),
    })
    .withAutomaticReconnect()
    .build();

  try {
    await connection.start();

    console.log("SignalR Connected");

    await connection.invoke(
      "JoinEmployeeGroup",
      employeeId
    );

    console.log(
      `Joined employee group: ${employeeId}`
    );

    return connection;
  } catch (error) {
    console.error(
      "SignalR Connection Error:",
      error
    );

    return null;
  }
};

export const getLeaveHubConnection = () => {
  return connection;
};