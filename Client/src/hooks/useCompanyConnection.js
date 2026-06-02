import { useEffect} from "react";
import {companyHubConnection} from "../api/signalrService.js";


const useCompanyConnection = () => {
  useEffect(() => {
    const startConnection = async () => {
        try {
            await companyHubConnection.start();
            console.log("SignalR Connected.");
        } catch (error) {
            console.error("SignalR Connection Error:", error);
        }
    };
    startConnection();

    companyHubConnection.on("CompanyCreated", (data) => {
      console.log("Company Switched:", data);
      
      window.location.reload();
    });

    return () => {companyHubConnection.off("CompanyCreated")};
  }, []);
}

  export default useCompanyConnection;