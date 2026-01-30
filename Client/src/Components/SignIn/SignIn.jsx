import React, { useState } from 'react';
import "../../Navy.css";
import { useNavigate } from 'react-router-dom';
import { authService } from '../Services/authService';

const SignIn = ({ onForgotPasswordClick, onLoginSuccess }) => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  const togglePasswordVisibility = () => {
    setShowPassword(!showPassword);
  };

  const handleLogin = async () => {
    setError('');

    if (!email.trim()) {
      setError('Email is required.');
      return;
    }

    if (!email.endsWith('@singular.co.za')) {
      setError('Email must be from @singular.co.za domain.');
      return;
    }

    if (!password) {
      setError('Password is required.');
      return;
    }

    setLoading(true);
    try {
      const responseData = await authService.login(email, password);

      // Store auth data in localStorage
      localStorage.setItem('currentUser', JSON.stringify({
        token: responseData.token,
        user: responseData.user,
      }));

      setError('');

      // Notify parent component of successful login
      if (typeof onLoginSuccess === 'function') {
        onLoginSuccess(responseData.user);
      }
    } catch (err) {
      setError(err.message || 'Login failed. Please try again.');
      console.error('Login error:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleForgotPassword = () => {
    navigate('/forgot-password');
  };  return (
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
                    disabled={loading}
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
                    disabled={loading}
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
                <button 
                  className="sign-in-button" 
                  onClick={handleLogin}
                  disabled={loading}
                >
                  {loading ? 'Logging in...' : 'Sign in'}
                </button>
                <div
                  className="forgot-password"
                  onClick={handleForgotPassword}
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