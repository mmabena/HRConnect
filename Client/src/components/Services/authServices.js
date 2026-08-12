import axios from 'axios';

const API_URL = 'http://localhost:5037/api/User';

export const requestResetLink = async (email) => {
  const response = await axios.get(API_URL);
  const userExists = response.data.some(user =>
    user.email && user.email.trim().toLowerCase() === email.trim().toLowerCase()
  );

  if (!userExists) {
    throw new Error('Please enter a valid email address.');
  }

   return await axios.post(`${API_URL}/forgot-password`, { Email: email }); //Changed here
};

export const verifyOtp = async (email, otp) => {
  try {
    const response = await axios.post(`${API_URL}/verify-otp`, {
      Email: email,
      Otp: otp
    });

    // Defensive: ensure data is present
    if (!response.data || typeof response.data.valid === 'undefined') {
      throw new Error('Unexpected response from server.');
    }

    if (!response.data.valid) {
      throw new Error('Invalid OTP. Please try again.');
    }

    return response.data;
  } catch (error) {
    if (error.response && error.response.data) {
      throw new Error(
        error.response.data.message || 'Invalid OTP. Please try again.'
      );
    } else {
      throw new Error('Failed to verify OTP. Please try again later.');
    }
  }
};

export const updatePassword = async (email, newPassword) => {
  const response = await axios.put(`${API_URL}/update-password`, {
    email,
    newPassword
  });

  if (response.status !== 200) {
    throw new Error(
      'Failed to update password. Please ensure your new passwords match and try again.'
    );
  }

  return response.data;
};
