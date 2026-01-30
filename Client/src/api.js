import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:5147/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add JWT token to all requests
api.interceptors.request.use(
  (config) => {
    const currentUser = localStorage.getItem('currentUser');
    if (currentUser) {
      try {
        const { token } = JSON.parse(currentUser);
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }
      } catch (error) {
        console.error('Failed to parse stored user:', error);
      }
    }
    return config;
  },
  (error) => Promise.reject(error)
);

api.interceptors.response.use(
  (response) => response,
  (error) => {
    // Handle 401 Unauthorized (expired token, etc.)
    if (error.response?.status === 401) {
      localStorage.removeItem('currentUser');
      window.location.href = '/';
    }
    console.error('API Error:', error.response?.data || error.message);
    return Promise.reject(error);
  }
);

export default api;