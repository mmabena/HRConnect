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

    companyHubConnection.on("CompanyCreated", () => {
        window.location.reload();
    });

    companyHubConnection.on("EmployeeCreated", () => {
        window.location.reload();
    });

    return () => {companyHubConnection.off("CompanyCreated")
                  companyHubConnection.off("EmployeeCreated");
    };
  }, []);
}

  export default useCompanyConnection;