import React from "react";
import "./HomePage.css";
import { ArrowRight } from "lucide-react";
import { Navigate, useNavigate } from "react-router-dom";

const HomePage = () => {
  const navigate = useNavigate();

  const goToLogin = () => {
    navigate("/login");
  };

  return (
    <div className="homepage">
      <div className="circle-wrapper">
        <div className="circle1"></div>
        <div className="circle2"></div>
      </div>
      <div className="circle3"></div>
      <div className="circle4"></div>
      <div className="circle5"></div>
      <div className="circle6"></div>
      <div className="circle7"></div>
      <div className="singular-wrapper">
        <div className="singular-container">
          <div className="singular">Singular</div>
          <div className="express">Express</div>
        </div>
        <div className="slogan-wrapper">
          <div className="slogan">HR & PAYROLL MANAGEMENT PLATFORM</div>
        </div>
      </div>

      <button className="start-button" onClick={goToLogin}>
        Get Started
        <ArrowRight className="arrow-icon" size={24} />
      </button>
      <div className="footer-wrapper">
        <div className="footer">
          Copyright © 2026 Singular Systems. All rights reserved.
        </div>
      </div>
    </div>
  );
};

export default HomePage;
