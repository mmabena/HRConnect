import api from "./api.js";

export const fetchAllCompanies = async () => {
    const response = await api.get("/company");
    return response.data || [];
  };

export const addCompany = async (companyData) => {
    const response = await api.post("/company", companyData);
    return response.data || {};
  };
