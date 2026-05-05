import axios from "axios";

const API_BASE = "http://localhost:5147/api";

const api = axios.create({
  baseURL: API_BASE,
});

api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem("token");

    if (!token) {
      throw new Error("No authentication token found. Please login again.");
    }

    config.headers.Authorization = `Bearer ${token}`;
    return config;
  },
  (error) => Promise.reject(error)
);

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      console.error("Unauthorized - token invalid or expired");
    }
    return Promise.reject(error);
  }
);

export const getLeaveHistory = async (employeeId) => {
  const response = await api.get(
    `/LeaveApplications/by-employee-id/${employeeId}`
  );
  return response.data;
};
export const applyLeave = async (formData) => {
  const response = await api.post(
    `/LeaveApplications`,
    formData,
    {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    }
  );
  return response.data;
};

export const getEmployeeLeave = async (employeeId) => {
  const response = await api.get(
    `/leave-types/employees/${employeeId}`
  );
  return response.data;
};