import axios from "axios";

const API_BASE = "http://localhost:5147/api/Pension";

// OPTIONS
export const getPensionOptions = async () => {
  const token = localStorage.getItem("authToken");
  const response = await axios.get(`${API_BASE}/options`, {
    headers: { Authorization: `Bearer ${token}` }
  });
  return response.data;
};

export const createPensionOption = async (option) => {
  const token = localStorage.getItem("authToken");
  const response = await axios.post(
    `${API_BASE}/options`,
    {
      ContributionPercentage: option.contributionPercentage, // ✅ PascalCase
    },
    {
      headers: {
        Authorization: `Bearer ${token}`,
        "Content-Type": "application/json",
      },
    }
  );
  return response.data;
};




export const updatePensionOption = async (option) => {
  const token = localStorage.getItem("authToken");
  const response = await axios.put(
    `${API_BASE}/options`,   // no ID in URL
    {
      pensionOptionId: option.pensionOptionId,
      contributionPercentage: option.contributionPercentage,
    },
    {
      headers: {
        Authorization: `Bearer ${token}`,
        "Content-Type": "application/json",
      },
    }
  );
  return response.data;
};


// FUNDS
export const getPensionFunds = async () => {
  const token = localStorage.getItem("authToken");
  const response = await axios.get(`${API_BASE}/funds`, {
    headers: { Authorization: `Bearer ${token}` }
  });
  return response.data;
};

export const addPensionFund = async (fund) => {
  const token = localStorage.getItem("authToken");
  const response = await axios.post(`${API_BASE}/funds`, fund, {
    headers: {
      Authorization: `Bearer ${token}`,
      "Content-Type": "application/json"
    }
  });
  return response.data;
};

export const deletePensionFund = async (id) => {
  const token = localStorage.getItem("authToken");
  const response = await axios.delete(`${API_BASE}/funds/${id}`, {
    headers: { Authorization: `Bearer ${token}` }
  });
  return response.data;
};

export const deleteAllPensionOptions = async () => {
  const token = localStorage.getItem("authToken");
  const response = await axios.delete(`${API_BASE}/options/delete-all`, {
    headers: { Authorization: `Bearer ${token}` }
  });
  return response.data;
};



