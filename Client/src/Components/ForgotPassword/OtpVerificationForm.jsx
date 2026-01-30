import React from 'react';
import "../../Navy.css";

const OtpVerificationForm = ({ 
  otp, 
  handleOtpChange, 
  handleKeyDown, 
  onSubmit, 
  isLoading, 
  error,
  onBackToLogin 
}) => {
  return (
    <>
      <div className="otp-title">Check your mail</div>
      <div className="reset-instruction">
        We've sent a verification code to your email address. Please enter the code below to continue resetting your password.
      </div>
      <div className="otp-container">
        {otp.map((digit, index) => (
          <input
            key={index}
            id={`otp-input-${index}`}
            type="text"
            className="otp-input"
            maxLength={1}
            value={digit}
            onChange={(e) => handleOtpChange(index, e.target.value)}
            onKeyDown={(e) => handleKeyDown(index, e)}
          />
        ))}
      </div>
      {isLoading ? (
        <div className="loader">Verifying...</div>
      ) : (
        <button type="button" className="request-button" onClick={onSubmit}>
          Submit
        </button>
      )}
      {error && <div className="error-message">{error}</div>}
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

export default OtpVerificationForm;
