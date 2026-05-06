import api from "./api.js";
import {toast} from "react-toastify";

const API_BASE = "http://localhost:5147/api/BankingDetails";

api.interceptors.response.use(
    (response) => {
        if (response.data === "") {
            response.data = null;
        }

        return response;
    },
    (error) => {
        return Promise.reject(error);
    }
);

export const addBankingDetails = async (BankingDetails) => {
    try{
        const response = await api.post(`${API_BASE}`,BankingDetails, {
            headers: { "Content-Type": "application/json" },
        });

        return response.data || {};
    }catch(error){
        if (error.response){
          console.error("Add banking details error response data:", error.response.data);
      console.error("Add banking details error status:", error.response.status);
    } else {
      console.error("Add banking details error message:", error.message);
    }
    throw error;
    }

export const editBankingDetails = async (employeeId, employee) => {
    try {
        const response = await api.put(
      `${API_BASE}/${employeeId}`,
      employee,
      {
        headers: { "Content-Type": "application/json" },
      }
    );
    return response.data || {};
  } catch (error) {
    if (error.response) {
      console.error("Edit employee error response data:", error.response.data);
      console.error("Edit employee error status:", error.response.status);
    } else {
      console.error("Edit employee error message:", error.message);
    }
    throw error;
  }
}
};