import React, { useState } from 'react';
import "../Navy.css";
// import MailIcon from '../images/mail2.svg';
// import KeyIcon from '../images/key2.svg';
// import VisibilityOffIcon from '../assets/icons/visibility_off.svg';
// import ImageGenForWeb from '../images/iMAGE gEN FOR WEB.svg';
import { useNavigate } from 'react-router-dom';

const SignIn = ({ onForgotPasswordClick, onLoginSuccess }) => {
  const [email, setEmail] = useState(''); 
  const [password, setPassword] = useState(''); 
  const [error, setError] = useState('');
  const [showPassword, setShowPassword] = useState(false); 
  const [attemptCount, setAttemptCount] = useState(0);
  const navigate = useNavigate(); 


  const togglePasswordVisibility = () => {
    
    setShowPassword(!showPassword); 
  };

    const handleLogin = async () => {
      setError(''); 

      // Validation
      if (!email.trim()) {
        setError('Email is required.');
        return;
      }

      const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
      if (!emailRegex.test(email)) {
        setError('Please enter a valid email address.');
        return;
      }

      if (!password) {
        setError('Password is required.');
        return;
      }

      try {
        console.log('Attempting login with email:', email);
        
        const response = await fetch('http://localhost:5147/api/auth/login', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({ email, password }),
        });

        console.log('Response status:', response.status);
        
        const responseText = await response.text(); 
        console.log('Response text:', responseText);

        if (response.ok) {
          console.log('Login successful');
          setError('');
          setAttemptCount(0);

          // Parse the responseText to get the actual user data (usually in JSON format)
          const responseData = JSON.parse(responseText);
          console.log('Parsed response data:', responseData);

          // Pass the user data to onLoginSuccess
          if (typeof onLoginSuccess === 'function') {
            onLoginSuccess(responseData);  // Pass the whole response data (user, token, etc.)
          }
        } else {
          // Try to parse error response
          try {
            const errorData = JSON.parse(responseText);
            const errorMessage = errorData.errors?.[0] || responseText || 'Login failed';
            console.error('Login error response:', errorData);
            setError(errorMessage);
          } catch {
            console.error('Error parsing response:', responseText);
            setError(responseText || 'Login failed. Please try again.');
          }
          
          if (response.status === 401) {
            setAttemptCount(prev => prev + 1);
          }
        }
      } catch (err) {
        console.error('Login network error:', err);
        setError('Network error. Please check your connection and ensure the server is running.');
      }
    };
      const handleForgotPassword = () => {
    navigate('/reset-password'); 
  };

  return (
    <div className="signin-container">
      <div className="logo-container">
        <span className="logo-bold">singular</span>
        <span className="logo-light">express</span>
      </div>

      <div className="auth-content">
        <div className="column left-column">
          <div className="left-inner-column" style={{ marginLeft: '50px' }}>
            <div className="adjusted-content">
              <div className="welcome-text">Welcome!</div>
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
                <div className="input-group">
                  <img src="/images/key2.svg" alt="password icon" className="input-icon-key" />
                  <input
                    type={showPassword ? 'text' : 'password'}
                    placeholder="Enter your password"
                    className="input-field"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)} 
                  />
                  <img
                    src="/images/visibility_off.svg"
                    alt="toggle visibility"
                    className="visibility-icon"
                    onClick={togglePasswordVisibility}
                    style={{ cursor: 'pointer' }}
                  />
                </div>
                {error && <div className="error-message">{error}</div>}
                <button className="sign-in-button" onClick={handleLogin}>
                  Sign in
                </button>
                <div
                  className="forgot-password"
                  onClick={onForgotPasswordClick} 
                  style={{ cursor: 'pointer' }}
                >
                  Forgot password
                </div>
                <div className="footer-text">
                  Privacy Policy | Terms & Conditions
                  <br />
                  Copyright © 2025 Singular Systems. All rights reserved.
                </div>
              </div>
            </div>
          </div>
        </div>

        <div className="column right-column">
          <img
            src="/images/iMAGEgENFORWEB.svg"
            alt="Image Gen For Web"
            className="right-column-image"
          />
        </div>
      </div>
    </div>
  );
};

export default SignIn;