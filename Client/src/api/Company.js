import api from "./api.js";
import { toast } from "react-toastify";

const API_BASE = "http://localhost:5147/api";

export const fetchAllCompanies = async () => {
    try {
        const response = await api.get(`${API_BASE}/company`);
        return response.data || [];
    } catch (error) {
        console.error("fetch all companies error:", error);
        throw error;
    }
};

export const addCompany = async (companyData) => {
    try {
        const response = await api.post(`${API_BASE}/company`, companyData);
        return response.data || {};
    } catch (error) {
        console.error("Adding company error:", error);
        throw error;
    }
}

