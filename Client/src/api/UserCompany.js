import api from "./api.js";

export const fetchMyCompanies = async () => {
    const response = await api.get("UserCompany/my-companies");
    return response.data || [];
  };

export const switchCompany = async (companyId) => {
    const response = await api.post(
      `UserCompany/switch-company?companyId=${companyId}`,
    );
    return response.data || {};
  }
;
