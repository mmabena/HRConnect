import { useEffect } from "react";
import connection from "./signalrService.js";

const SignalRProvider = ({ children }) => {
  useEffect(() => {
    const startConnection = async () => {
      try {
        if (connection.state === "Disconnected") {
          await connection.start();
          console.log("SignalR connected");
        }
      } catch (err) {
        console.error("SignalR error:", err);
      }
    };
    startConnection();

    return () => {

    };
  }, []);

  return children;
};

export default SignalRProvider;
