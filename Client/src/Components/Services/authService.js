import api from "./../../api/api";

const AUTH_BASE = "/auth";

export const authService = {

  // ✅ Login
  login: async (email, password) => {
      const response = await api.post(`${AUTH_BASE}/login`, { email, password });

      const { token, user } = response.data;

      localStorage.setItem("token", token);
      localStorage.setItem("user", JSON.stringify(user));

      if (!token || !user) {
        throw new Error("Invalid login response from server");
      }
      console.log("Backend login response:", response.data);
      return { token, user };
    },

  // ✅ Forgot Password
  forgotPassword: async (email) => {
    try {
      const response = await api.post(`${AUTH_BASE}/forgot-password`, { email });
      return response.data;
    } catch (error) {
      throw error.response?.data || { message: "Failed to send PIN" };
    }
  },

  // ✅ Verify PIN
  verifyPin: async (email, pin) => {
    try {
      const response = await api.post(`${AUTH_BASE}/verify-pin`, { email, pin });
      return response.data;
    } catch (error) {
      throw error.response?.data || { message: "Invalid PIN" };
    }
  },

  // ✅ Reset Password
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
      throw error.response?.data || { message: "Password reset failed" };
    }
  },

  // ✅ Logout (frontend only)
  logout: () => {
    localStorage.removeItem("currentUser");
  },

  // ✅ Get stored user
  getCurrentUser: () => {
    const stored = localStorage.getItem("currentUser");
    return stored ? JSON.parse(stored) : null;
  },

  // ✅ Get token safely
  getToken: () => {
    const user = authService.getCurrentUser();
    return user?.token || null;
  },
};