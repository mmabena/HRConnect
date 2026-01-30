import api from '../../api.js';

const AUTH_BASE = '/auth';

export const authService = {
  // Step 1: Login
  login: async (email, password) => {
    try {
      const response = await api.post(`${AUTH_BASE}/login`, { email, password });
      return response.data;
    } catch (error) {
      throw error.response?.data || { message: 'Login failed' };
    }
  },

  // Step 2: Request password reset (sends PIN to email)
  forgotPassword: async (email) => {
    try {
      const response = await api.post(`${AUTH_BASE}/forgot-password`, { email });
      return response.data;
    } catch (error) {
      throw error.response?.data || { message: 'Failed to send PIN' };
    }
  },

  // Step 3: Verify the PIN
  verifyPin: async (email, pin) => {
    try {
      const response = await api.post(`${AUTH_BASE}/verify-pin`, { email, pin });
      return response.data;
    } catch (error) {
      throw error.response?.data || { message: 'Invalid PIN' };
    }
  },

  // Step 4: Reset password with verified PIN
  resetPassword: async (email, pin, newPassword, confirmPassword) => {
    try {
      const response = await api.post(`${AUTH_BASE}/reset-password`, {
        email,
        pin,
        newPassword,
        confirmPassword,
      });
      return response.data;
    } catch (error) {
      throw error.response?.data || { message: 'Password reset failed' };
    }
  },

  // Logout
  logout: () => {
    localStorage.removeItem('currentUser');
    localStorage.removeItem('token');
  },

  // Get stored token
  getToken: () => {
    const user = localStorage.getItem('currentUser');
    return user ? JSON.parse(user).token : null;
  },
};
