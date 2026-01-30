import React, { useState, useEffect } from 'react';
import "../Navy.css";
import axios from 'axios';

const ForgotPassword = ({ onBackToLogin }) => {
  const [isOtpSent, setIsOtpSent] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [currentStep, setCurrentStep] = useState('request'); 
  const [email, setEmail] = useState('');
  const [otp, setOtp] = useState(Array(4).fill(''));
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError] = useState('');
  const [successMessage, setSuccessMessage] = useState('');   
  const [isLoading, setIsLoading] = useState(false); 
  
  useEffect(() => {
    const queryParams = new URLSearchParams(window.location.search);
    const emailParam = queryParams.get('email');
    const stepParam = queryParams.get('step');

    if (emailParam) setEmail(emailParam); 
    if (stepParam === 'otp') {
      setIsOtpSent(true);
      setCurrentStep('otp'); 
    }
  }, []);

const handleRequestResetLink = async () => {
  setIsLoading(true); 
  try {
    const response = await axios.get('http://localhost:5037/api/User');
    
    console.log('Response data:', response.data); 

    const userExists = response.data.some(user => 
      user.email && user.email.trim().toLowerCase() === email.trim().toLowerCase()
    );

    if (userExists) {
      
      const otpResponse = await axios.post('http://localhost:5037/api/User/forgot-password', {
        email: email
      });
      setIsOtpSent(true);
      setError('');
      console.log(otpResponse.data);
    } else {
      setError('Please enter a valid email address.');
    }
  } catch (error) {
    console.error('Error fetching users:', error);
    setError('An error occurred. Please try again.');
  }
  finally {
      setIsLoading(false); 
    }
};

  const handleVerifyOtp = async () => {
    setIsLoading(true);
    try {
      const response = await axios.post('http://localhost:5037/api/User/verify-otp', {
        Email: email,
        Otp: otp.join('')
      });

      if (response.data.valid) {
        setCurrentStep('newPassword');
        setError('');
      } else {
        setError('Invalid OTP. Please try again.');
      }
    } catch (error) {
      console.error('Error verifying OTP:', error);
      setError('Invalid OTP. Please enter the code sent to your email and try again.');
    }
     finally {
      setIsLoading(false); 
    }
  };

  const handleUpdatePassword = async () => {
    if (newPassword !== confirmPassword) {
      setError("Passwords do not match.");
      return;
    }

    try {
      const response = await axios.put('http://localhost:5037/api/User/update-password', {
        email: email,
        newPassword: newPassword
      });

      if (response.status === 200) {
          setSuccessMessage('Password updated successfully!');
          setError(''); 
      }
    } catch (error) {
    console.error('Error updating password:', error);
    if (error.response && error.response.data) {
      setError(error.response.data);  
    } else {
      setError('Failed to update password. Please ensure your new passwords match and try again.');
    }
  }
};
  const handleSubmit = () => {
    if (currentStep === 'otp') {
      handleVerifyOtp();
    } else if (currentStep === 'newPassword') {
      handleUpdatePassword(); 
    }
  };

  const togglePasswordVisibility = () => {
    setShowPassword(!showPassword);
  };

  const handleOtpChange = (index, value) => {
    const newOtp = [...otp];
    newOtp[index] = value;
    setOtp(newOtp);

    if (value && index < 3) {
      document.getElementById(`otp-input-${index + 1}`).focus();
    }
  };

  const handleKeyDown = (index, event) => {
    if (event.key === 'Backspace' && !otp[index] && index > 0) {
      document.getElementById(`otp-input-${index - 1}`).focus();
    }
  };

  return (
    <div className="signin-container">
      <div className="logo-container">
        <span className="logo-bold">singular</span>
        <span className="logo-light">express</span>
      </div>

      <div className="auth-content"> 
        <div className="column left-column">
          <img
            src="/images/password_image.png"
            alt="Reset Password"
            className="password-image"
          />
        </div>

        <div className="column right-column">
          {currentStep === 'newPassword' ? (
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
              <button className="request-button" onClick={handleSubmit}>Save</button>
              {error && <div className="error-message">{error}</div>}
              {successMessage && <div className="success-message">{successMessage}</div>}
            </>
          ) : isOtpSent ? (
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
                <button className="request-button" onClick={handleVerifyOtp}>Submit</button>
              )}
              {error && <div className="error-message">{error}</div>}
            </>
          ) : (
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
                  <button className="request-button" onClick={handleRequestResetLink}>
                    Request reset link
                  </button>
                )}
                {error && <div className="error-message">{error}</div>}
              </div>
            </>
          )}
          {currentStep !== 'newPassword' && (
          <div
            className="back-to-login"
            onClick={onBackToLogin}
            style={{ cursor: 'pointer' }}
          >
            Back to log in
          </div>
          )}
          <div className="footer-text">
            Privacy Policy | Terms & Conditions
            <br />
            Copyright © 2025 Singular Systems. All rights reserved.
          </div>
        </div>
      </div>
    </div>
  );
};

export default ForgotPassword;