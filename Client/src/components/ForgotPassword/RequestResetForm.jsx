import React from 'react';
import "../../Navy.css";

const RequestResetForm = ({ 
  email, 
  setEmail, 
  onSubmit, 
  isLoading, 
  error,
  onBackToLogin 
}) => {
  return (
    <>
      <div className="forgot-password-title">Forgot your password</div>
      <div className="reset-instruction">
        Enter the email address and we will send you instructions to reset your password
      </div>
      <div className="log-details">
        <div className="input-group">
          <img src="/images/mail2.svg" alt="email icon" className="input-icon-mail" />
          <input
            type="email"
            placeholder="Enter your email"
            className="input-field"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
          />
        </div>
        {isLoading ? (
          <div className="loader">Sending reset link...</div>
        ) : (
          <button className="request-button" onClick={onSubmit}>
            Request reset link
          </button>
        )}
        {error && <div className="error-message">{error}</div>}
      </div>
      <div
        className="back-to-login"
        onClick={onBackToLogin}
        style={{ cursor: 'pointer' }}
      >
        Back to log in
      </div>
    </>
  );
};

export default RequestResetForm;