import { useEffect } from "react";
import {companyHubConnection} from "./signalrService.js";

const SignalRProvider = ({ children }) => {
  useEffect(() => {
    const startConnection = async () => {
      try {
        if (companyHubConnection.state === "Disconnected") {
          await companyHubConnection.start();
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
