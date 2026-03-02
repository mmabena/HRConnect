import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import axios from "axios";
import "../Navy.css";

const ChangePassword = ({ onClose }) => {
  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState("");
  const [successMessage, setSuccessMessage] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const navigate = useNavigate();
  const togglePasswordVisibility = () => {
    setShowPassword(!showPassword);
  };

  const handleChangePassword = async () => {
    setError("");
    setSuccessMessage("");

    if (!currentPassword || !newPassword || !confirmPassword) {
      setError("All fields are required.");
      return;
    }

    if (newPassword !== confirmPassword) {
      setError("New passwords do not match.");
      return;
    }

    try {
      setIsLoading(true);

      const token = localStorage.getItem("token");

      const response = await axios.put(
        "http://localhost:5147/api/user/change-password",
        {
          currentPassword,
          newPassword,
        },
        {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        }
      );

      if (response.status === 200) {
        setSuccessMessage("Password changed successfully!");
        setCurrentPassword("");
        setNewPassword("");
        setConfirmPassword("");
      }

    } catch (error) {
      console.error("Change password error:", error);

      if (error.response?.data) {
        setError(error.response.data);
      } else {
        setError("Failed to change password. Please try again.");
      }
    } finally {
      setIsLoading(false);
    }
  };

    return (
        <div className="modal-overlay1">
    <div className="signin-container">
      <div className="logo-container">
        <span className="logo-bold">singular</span>
        <span className="logo-light">express</span>
      </div>

         <div className="auth-content"> 
        <div className="column img-left-column">
          <img
            src="/images/password_image.png"
            alt="Reset Password"
            className="password-image"
          />
        </div>
              
        <div className="column right-column">

          <div className="new-password-title">Change Password</div>
          <div className="new-password-instruction">
            Enter your current password and choose a new one
          </div>

          {/* Current Password */}
          <div className="password-input-group">
            <img src="/images/key2.svg"
                alt="key"
                className="input-icon-key" />
            <input
              type={showPassword ? "text" : "password"}
              placeholder="Current password"
              className="input-field"
              value={currentPassword}
              onChange={(e) => setCurrentPassword(e.target.value)}
            />
          </div>

          {/* New Password */}
          <div className="password-input-group">
            <img src="/images/key2.svg" alt="key" className="input-icon-key" />
            <input
              type={showPassword ? "text" : "password"}
              placeholder="New password"
              className="input-field"
              value={newPassword}
              onChange={(e) => setNewPassword(e.target.value)}
            />
            <img
              src="/images/visibility_off.svg"
              alt="toggle visibility"
              className="visibility-icon"
              onClick={togglePasswordVisibility}
            />
          </div>

          {/* Confirm Password */}
          <div className="password-input-group">
            <img src="/images/key2.svg" alt="key" className="input-icon-key" />
            <input
              type={showPassword ? "text" : "password"}
              placeholder="Confirm new password"
              className="input-field"
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
            />
          </div>

          {isLoading ? (
            <div className="loader">Updating...</div>
          ) : (
            <button className="request-button" onClick={handleChangePassword}>
              Save Changes
            </button>
          )}

          {error && <div className="error-message">{error}</div>}
          {successMessage && <div className="success-message">{successMessage}</div>}

          <div
            className="back-to-login"
            onClick={() => navigate(-1)}
          > Back
          </div>
        </div>
        </div>
      </div>
    </div>
  );
};

export default ChangePassword;