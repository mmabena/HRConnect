import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { authService } from '../Services/authService';
import '../../Navy.css';

const ForgotPassword = ({ onBackToLogin }) => {
  const [step, setStep] = useState(1); // 1: email, 2: PIN, 3: reset password
  const [email, setEmail] = useState('');
  const [pin, setPin] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [loading, setLoading] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const navigate = useNavigate();

  // Step 1: Request PIN
  const handleRequestPin = async () => {
    setError('');
    setSuccess('');

    if (!email.trim()) {
      setError('Email is required.');
      return;
    }

    if (!email.endsWith('@singular.co.za')) {
      setError('Email must be from @singular.co.za domain.');
      return;
    }

    setLoading(true);
    try {
      await authService.forgotPassword(email);
      setSuccess('PIN sent to your email. Check your inbox.');
      setStep(2);
    } catch (err) {
      setError(err.message || 'Failed to send PIN.');
    } finally {
      setLoading(false);
    }
  };

  // Step 2: Verify PIN
  const handleVerifyPin = async () => {
    setError('');
    setSuccess('');

    if (!pin.trim()) {
      setError('PIN is required.');
      return;
    }

    if (pin.length !== 4 || !/^\d+$/.test(pin)) {
      setError('PIN must be exactly 4 digits.');
      return;
    }

    setLoading(true);
    try {
      await authService.verifyPin(email, pin);
      setSuccess('PIN verified. Please enter your new password.');
      setStep(3);
    } catch (err) {
      setError(err.message || 'Invalid or expired PIN.');
    } finally {
      setLoading(false);
    }
  };

  // Step 3: Reset Password
  const handleResetPassword = async () => {
    setError('');
    setSuccess('');

    if (!newPassword.trim()) {
      setError('New password is required.');
      return;
    }

    if (newPassword.length < 8) {
      setError('Password must be at least 8 characters.');
      return;
    }

    if (!/[A-Z]/.test(newPassword)) {
      setError('Password must contain an uppercase letter.');
      return;
    }

    if (!/[a-z]/.test(newPassword)) {
      setError('Password must contain a lowercase letter.');
      return;
    }

    if (!/[0-9]/.test(newPassword)) {
      setError('Password must contain a digit.');
      return;
    }

    if (!/[!@#$%^&*(),.?"{}|<>]/.test(newPassword)) {
      setError('Password must contain a special character (!@#$%^&*...).');
      return;
    }

    if (newPassword !== confirmPassword) {
      setError('Passwords do not match.');
      return;
    }

    setLoading(true);
    try {
      await authService.resetPassword(email, pin, newPassword, confirmPassword);
      setSuccess('Password reset successfully! Redirecting to login...');
      setTimeout(() => {
        navigate('/');
      }, 2000);
    } catch (err) {
      setError(err.message || 'Failed to reset password.');
    } finally {
      setLoading(false);
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
          <div className="left-inner-column" style={{ marginLeft: '50px' }}>
            <div className="adjusted-content">
              <div className="welcome-text">Forgot Password?</div>
              <div className="log-details">
                {/* Step 1: Enter Email */}
                {step === 1 && (
                  <>
                    <p style={{ marginBottom: '15px', fontSize: '0.95rem' }}>
                      Enter your email to receive a password reset PIN.
                    </p>
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
                  </>
                )}

                {/* Step 2: Enter PIN */}
                {step === 2 && (
                  <>
                    <p style={{ marginBottom: '15px', fontSize: '0.95rem' }}>
                      Enter the 4-digit PIN sent to your email.
                    </p>
                    <div className="input-group">
                      <input
                        type="text"
                        placeholder="Enter 4-digit PIN"
                        className="input-field"
                        value={pin}
                        onChange={(e) => setPin(e.target.value.slice(0, 4))}
                        maxLength="4"
                        disabled={loading}
                      />
                    </div>
                  </>
                )}

                {/* Step 3: Reset Password */}
                {step === 3 && (
                  <>
                    <p style={{ marginBottom: '15px', fontSize: '0.95rem' }}>
                      Enter your new password.
                    </p>
                    <div className="input-group">
                      <img src="/images/key2.svg" alt="password icon" className="input-icon-key" />
                      <input
                        type={showPassword ? 'text' : 'password'}
                        placeholder="New password"
                        className="input-field"
                        value={newPassword}
                        onChange={(e) => setNewPassword(e.target.value)}
                        disabled={loading}
                      />
                      <button
                        className="toggle-visibility-btn"
                        onClick={() => setShowPassword(!showPassword)}
                        type="button"
                        style={{ background: 'none', border: 'none', cursor: 'pointer' }}
                      >
                        <img
                          src={showPassword ? '/images/eye-open.svg' : '/images/eye-closed.svg'}
                          alt="toggle"
                          style={{ height: '20px', width: '20px' }}
                        />
                      </button>
                    </div>

                    <div className="input-group">
                      <img src="/images/key2.svg" alt="password icon" className="input-icon-key" />
                      <input
                        type={showPassword ? 'text' : 'password'}
                        placeholder="Confirm password"
                        className="input-field"
                        value={confirmPassword}
                        onChange={(e) => setConfirmPassword(e.target.value)}
                        disabled={loading}
                      />
                    </div>

                    <div style={{ fontSize: '0.85rem', marginTop: '10px', color: '#666' }}>
                      <p style={{ margin: '3px 0' }}>✓ At least 8 characters</p>
                      <p style={{ margin: '3px 0' }}>✓ Contains uppercase (A-Z)</p>
                      <p style={{ margin: '3px 0' }}>✓ Contains lowercase (a-z)</p>
                      <p style={{ margin: '3px 0' }}>✓ Contains digit (0-9)</p>
                      <p style={{ margin: '3px 0' }}>✓ Contains special char (!@#$%...)</p>
                    </div>
                  </>
                )}

                {/* Error/Success Messages */}
                {error && <div className="error-message" style={{ marginTop: '10px' }}>{error}</div>}
                {success && <div style={{ marginTop: '10px', color: 'green', fontSize: '0.95rem' }}>{success}</div>}

                {/* Action Buttons */}
                <button
                  className="sign-in-button"
                  onClick={
                    step === 1 ? handleRequestPin :
                    step === 2 ? handleVerifyPin :
                    handleResetPassword
                  }
                  disabled={loading}
                  style={{ marginTop: '20px' }}
                >
                  {loading ? 'Processing...' : step === 1 ? 'Send PIN' : step === 2 ? 'Verify PIN' : 'Reset Password'}
                </button>

                <button
                  className="forgot-password"
                  onClick={onBackToLogin}
                  type="button"
                  disabled={loading}
                  style={{ marginTop: '10px', border: 'none', background: 'none', color: '#1976d2', cursor: 'pointer', fontSize: '0.95rem' }}
                >
                  Back to Login
                </button>
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

export default ForgotPassword;