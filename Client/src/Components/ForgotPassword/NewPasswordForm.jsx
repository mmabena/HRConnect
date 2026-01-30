import React from 'react';
import "../../Navy.css";

const NewPasswordForm = ({ 
  newPassword, 
  setNewPassword, 
  confirmPassword, 
  setConfirmPassword, 
  showPassword, 
  togglePasswordVisibility, 
  onSubmit, 
  error, 
  successMessage,
  isLoading // added this
}) => {
  return (
    <>
      <div className="new-password-title">New password</div>
      <div className="new-password-instruction">
        Enter your new password
      </div>
      <div className="password-input-group">
        <img src="/images/key2.svg" alt="password icon" className="input-icon-key" />
        <input
          type={showPassword ? 'text' : 'password'}
          placeholder="Enter your new password"
          className="input-field"
          value={newPassword}
          onChange={(e) => setNewPassword(e.target.value)}
        />
        <img
          src="/images/visibility_off.svg"
          alt="toggle visibility"
          className="visibility-icon"
          onClick={togglePasswordVisibility}
          style={{ cursor: 'pointer' }}
        />
      </div>
      <div className="password-input-group">
        <img src="/images/key2.svg" alt="password icon" className="input-icon-key" />
        <input
          type={showPassword ? 'text' : 'password'}
          placeholder="Confirm your new password"
          className="input-field"
          value={confirmPassword}
          onChange={(e) => setConfirmPassword(e.target.value)}
        />
      </div>

      {isLoading ? (
        <div className="loader">Resetting password...</div>
      ) : (
        <button className="request-button" onClick={onSubmit}>Save</button>
      )}

      {error && <div className="error-message">{error}</div>}
      {successMessage && <div className="success-message">{successMessage}</div>}
    </>
  );
};

export default NewPasswordForm;
