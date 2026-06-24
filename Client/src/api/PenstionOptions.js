import api from "./api.js";

export const fetchALLPensionOptions = async () => {
    const response = await api.get("/Pension/options");
    return response.data || [];
  };