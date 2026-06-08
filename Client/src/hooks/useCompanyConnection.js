import { useEffect} from "react";
import connection from "../api/signalrService.js";


const useCompanyConnection = () => {
  useEffect(() => {
    const startConnection = async () => {
        try {
            await connection.start();
            console.log("SignalR Connected.");
        } catch (error) {
            console.error("SignalR Connection Error:", error);
        }
    };
    startConnection();

    connection.on("CompanyCreated", (data) => {
      console.log("Company Switched:", data);
      
      window.location.reload();
    });

    return () => {connection.off("CompanyCreated")};
  }, []);
}

  export default useCompanyConnection;