import api from "./api.js";
import { toast } from "react-toastify";

const API_BASE = "http://localhost:5147/api/userCompany";


export const fetchMyCompanies = async () => {
    try {
        const response =  await api.get(`${API_BASE}/my-companies`);
        return response.data || [];
    } catch (error) {
        console.error("Fetch companies error:", error);
        throw error;
    }
};


export const switchCompany = async (companyId) => {
    try {
        const response = await api.post(`${API_BASE}/switch-company?companyId=${companyId}`);
        return response.data || {};
    } catch (error){
        console.error("Switch company error:", error);
        throw error;
    }
};