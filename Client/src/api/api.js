import axios from "axios";

const api = axios.create({
  baseURL: process.env.REACT_APP_API_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

// Add JWT token to all requests
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem("token");

    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }

    return config;
  },
  (error) => Promise.reject(error),
);

api.interceptors.response.use(
  (response) => response,
  (error) => {
    const formattedMessage = {
      message: error.response?.data?.message || "An error occurred",

      status: error.response?.status,

      error: error.response?.data?.errors || null,
    };
    console.log("API URL:", process.env.REACT_APP_API_URL);
    console.log("Interceptor caught error:", error.response?.status);
    error.formattedMessage = formattedMessage;
    return Promise.reject(error);
  },
);
export default api;
