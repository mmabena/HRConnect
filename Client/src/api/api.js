import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:7243/api',
  headers: {
    'Content-Type': 'application/json',
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
  (error) => Promise.reject(error)
);

api.interceptors.response.use(
  (response) => {
    // Return full response, avoiding instances of destructuring data on 204	  
    return response;
  },
  (error) => {
    console.log("Interceptor caught error:", (error || error.response?.status));
    return Promise.reject(error);
  }
);
export default api;
